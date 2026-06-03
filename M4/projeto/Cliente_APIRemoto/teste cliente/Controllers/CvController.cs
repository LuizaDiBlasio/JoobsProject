using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using Rotativa.AspNetCore;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;
using System.Text;
using teste_cliente.Helpers;
using teste_cliente.Models;
using teste_cliente.Models.Enums;
using Vereyon.Web;

namespace teste_cliente.Controllers
{
    public class CvController : Controller
    {
        private readonly string _baseUrl;
        private readonly IConfiguration _config;
        private readonly IFlashMessage _flashMessage;

        public CvController(IConfiguration config, IFlashMessage flash)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
            _flashMessage = flash;
        }

        [Authorize(Roles = "Candidato")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var idCandidato = int.Parse(User.FindFirst("IdCandidato").Value);

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            // ALTERAÇÃO 26/05: Carrega as listas dos Enums para enviar para a View colocar nas Comboboxes
            ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            ViewBag.Escolaridades = EnumHelper.ObterSelectListDoEnum<EscolaridadeEnum>();


            // Carregar o CV
            CV model = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respCv = await client.GetAsync(_baseUrl + $"cv/idCandidato?idCandidato={idCandidato}");
                if (respCv.IsSuccessStatusCode)
                {
                    model = JsonConvert.DeserializeObject<CV>(await respCv.Content.ReadAsStringAsync());
                }
                else
                {
                    if (respCv.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                        return Forbid();
                    }
                }
            }
            if (model == null)
                model = new CV { IdCandidatoCv = idCandidato };

            return View(model);
        }

        // POST: Cv/Create  (criar ou atualizar)
        [Authorize(Roles = "Candidato")]
        [HttpPost]
        public async Task<IActionResult> Create(CV cv)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(cv);

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonConvert.SerializeObject(cv),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage resp;
            if (cv.IdCV > 0)
            {
                resp = await client.PutAsync(_baseUrl + $"cv/{cv.IdCV}", content);
                if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {                
                    return Forbid();
                }

                _flashMessage.Confirmation("Seu currículo foi editado com sucesso!");

            }
            else
            {
                resp = await client.PostAsync(_baseUrl + "cv", content);
                if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }
                _flashMessage.Confirmation("Seu currículo foi publicado com sucesso!");
            }
                

            // ler status e corpo da resposta
            var statusCode = (int)resp.StatusCode;
            var responseBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                // mostra o erro recebido da API no validation-summary
                ModelState.AddModelError("",
                    $"Erro ao guardar (HTTP {statusCode}): {responseBody}");
                return View(cv);
            }

            // no sucesso, volta ao GET para recarregar o modelo atualizado
            return RedirectToAction(nameof(Create));
        }


        // GET: Cv/DownloadPdf  —> gerar, guardar em FileCV e enviar ao browser
        // ALTERAÇÃO: Permitir que Empresas e Admins também chamem este método
        [Authorize(Roles = "Candidato, Admin, Empresa")]
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int? idCandidato)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");


            //-------------------------------------
            int idAlvo;

            // Se veio um idCandidato nos parâmetros (Empresa a clicar), usamos esse ID.
            // Se NÃO veio (Candidato a clicar no seu próprio menu), usamos o ID do utilizador logado.
            if (idCandidato.HasValue)
            {
                idAlvo = idCandidato.Value;
            }
            else
            {
                var idClaim = User.FindFirst("IdCandidato")?.Value;
                if (string.IsNullOrEmpty(idClaim)) return Unauthorized();
                idAlvo = int.Parse(idClaim);
            }
            //-------------------------------------

            //var idCandidato = int.Parse(User.FindFirst("IdCandidato").Value);

            // buscar CV atual
            CV cv;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync(_baseUrl + $"cv/idCandidato?idCandidato={idAlvo}");
                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                        return Forbid();
                    }

                    // -------------------------------------------

                    // Se for a empresa e o CV não existir, volta para a pesquisa com erro detalhado
                    if (User.IsInRole("Empresa"))
                    {
                        var corpoErro = await resp.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"API deu {resp.StatusCode} para o ID Alvo {idAlvo}. Resposta: {corpoErro}";
                        return RedirectToAction("PesquisaCandidatos", "Empresa");
                    }

                    // -------------------------------------------

                    return RedirectToAction(nameof(Create));
                }                   

                cv = JsonConvert.DeserializeObject<CV>(await resp.Content.ReadAsStringAsync());
            }

            // NOVO: buscar bytes da foto
            using (var httpFoto = new HttpClient())
            {
                httpFoto.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var respFoto = await httpFoto.GetAsync(
                    _baseUrl + $"foto/BuscarFotoPorIdCandidato/{idAlvo}");

                if (respFoto.IsSuccessStatusCode)
                {
                    cv.FotoPerfil = await respFoto.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    cv.FotoPerfil = null; // ou deixe default
                }
            }

            // gerar PDF
            var pdfResult = new ViewAsPdf("CreatePdf", cv)
            {
                FileName = $"CV_{cv.Nome}.pdf",
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 10, 20, 10)
            };
            var pdfBytes = await pdfResult.BuildFile(ControllerContext);


            // Apenas faz o upload para a API se quem estiver a descarregar for o próprio Candidato
            if (User.IsInRole("Candidato"))  // ----------------------------------------- LINHA
            {  // ----------------------------------------- LINHA
                // upload do PDF ao API FileCV: tenta PUT e, se não existir, faz POST
                using var upload = new HttpClient();
                upload.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var form = new MultipartFormDataContent();
                var byteContent = new ByteArrayContent(pdfBytes);
                byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                form.Add(byteContent, "file", $"CV_{cv.Nome}.pdf");
                form.Add(new StringContent(idAlvo.ToString()), "idCandidatoFile");


                // 1) PUT para atualizar
                var putResponse = await upload.PutAsync(_baseUrl + $"filecv/{idAlvo}", form);
                // 2) se não existir, cria
                if (putResponse.StatusCode == HttpStatusCode.NotFound)
                    await upload.PostAsync(_baseUrl + "filecv", form);
            } // ----------------------------------------- LINHA

            // devolver PDF ao browser
            //return File(pdfBytes, "application/pdf", $"CV_{cv.Nome}.pdf");

            // Força o browser a abrir a janela de guardar ficheiro/fazer download imediato
            Response.Headers.Append("Content-Disposition", $"attachment; filename=CV_{cv.Nome}.pdf");

            // devolver PDF ao browser
            return File(pdfBytes, "application/pdf");
        }
    }
}
