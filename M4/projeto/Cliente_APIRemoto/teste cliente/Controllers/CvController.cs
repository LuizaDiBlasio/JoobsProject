using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;
using Rotativa.AspNetCore;
using teste_cliente.Models;
using System.Net.Http;

namespace teste_cliente.Controllers
{
    [Authorize(Roles = "Candidato")]
    public class CvController : Controller
    {
        private readonly string _baseUrl;
        private readonly IConfiguration _config;

        public CvController(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var idCandidato = int.Parse(User.FindFirst("IdCandidato").Value);

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

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
            }
            else
            {
                resp = await client.PostAsync(_baseUrl + "cv", content);
                if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }
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
        [HttpGet]
        public async Task<IActionResult> DownloadPdf()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var idCandidato = int.Parse(User.FindFirst("IdCandidato").Value);

            // buscar CV atual
            CV cv;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await client.GetAsync(_baseUrl + $"cv/idCandidato?idCandidato={idCandidato}");
                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                        return Forbid();
                    }

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
                    _baseUrl + $"foto/BuscarFotoPorIdCandidato/{idCandidato}");

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

            // upload do PDF ao API FileCV: tenta PUT e, se não existir, faz POST
            using var upload = new HttpClient();
            upload.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var form = new MultipartFormDataContent();
            var byteContent = new ByteArrayContent(pdfBytes);
            byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(byteContent, "file", $"CV_{cv.Nome}.pdf");
            form.Add(new StringContent(idCandidato.ToString()), "idCandidatoFile");


            // 1) PUT para atualizar
            var putResponse = await upload.PutAsync(_baseUrl + $"filecv/{idCandidato}", form);
            // 2) se não existir, cria
            if (putResponse.StatusCode == HttpStatusCode.NotFound)
                await upload.PostAsync(_baseUrl + "filecv", form);

            // devolver PDF ao browser
            return File(pdfBytes, "application/pdf", $"CV_{cv.Nome}.pdf");
        }
    }
}
