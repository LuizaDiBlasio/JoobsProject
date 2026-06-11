using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using Rotativa.AspNetCore;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

            ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            ViewBag.Escolaridades = EnumHelper.ObterSelectListDoEnum<EscolaridadeEnum>();

            CV model = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado o prefixo "api/" conforme o print do teu Swagger
                var respCv = await client.GetAsync(_baseUrl + $"api/cv/idCandidato?idCandidato={idCandidato}");
                if (respCv.IsSuccessStatusCode)
                {
                    model = JsonConvert.DeserializeObject<CV>(await respCv.Content.ReadAsStringAsync());
                }
                else
                {
                    if (respCv.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                }
            }
            if (model == null)
                model = new CV { IdCandidatoCv = idCandidato };

            return View(model);
        }

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
                // CORREÇÃO: Ajustado o nome do parâmetro de rota para "api/cv/{id}" batendo com o PUT do Swagger
                resp = await client.PutAsync(_baseUrl + $"api/cv/{cv.IdCV}", content);
                if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                _flashMessage.Confirmation("Seu currículo foi editado com sucesso!");
            }
            else
            {
                // CORREÇÃO: Adicionado o prefixo "api/" para bater com o POST do teu Swagger
                resp = await client.PostAsync(_baseUrl + "api/cv", content);
                if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }
                _flashMessage.Confirmation("Seu currículo foi publicado com sucesso!");
            }

            var statusCode = (int)resp.StatusCode;
            var responseBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", $"Erro ao guardar (HTTP {statusCode}): {responseBody}");
                return View(cv);
            }

            return RedirectToAction(nameof(Create));
        }

        [Authorize(Roles = "Candidato, Admin, Empresa")]
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int? idCandidato)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            int idAlvo;

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

            CV cv;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado o prefixo "api/" à rota
                var resp = await client.GetAsync(_baseUrl + $"api/cv/idCandidato?idCandidato={idAlvo}");
                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }

                    if (User.IsInRole("Empresa"))
                    {
                        var corpoErro = await resp.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"API deu {resp.StatusCode} para o ID Alvo {idAlvo}. Resposta: {corpoErro}";
                        return RedirectToAction("PesquisaCandidatos", "Empresa");
                    }

                    return RedirectToAction(nameof(Create));
                }

                cv = JsonConvert.DeserializeObject<CV>(await resp.Content.ReadAsStringAsync());
            }

            using (var httpFoto = new HttpClient())
            {
                httpFoto.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado o prefixo obrigatório "api/" para carregar os bytes da foto
                var respFoto = await httpFoto.GetAsync(_baseUrl + $"api/foto/BuscarFotoPorIdCandidato/{idAlvo}");

                if (respFoto.IsSuccessStatusCode)
                {
                    cv.FotoPerfil = await respFoto.Content.ReadAsByteArrayAsync();
                }
                else
                {
                    cv.FotoPerfil = null;
                }
            }

            var pdfResult = new ViewAsPdf("CreatePdf", cv)
            {
                FileName = $"CV_{cv.Nome}.pdf",
                PageMargins = new Rotativa.AspNetCore.Options.Margins(20, 10, 20, 10)
            };
            var pdfBytes = await pdfResult.BuildFile(ControllerContext);

            if (User.IsInRole("Candidato"))
            {
                using var upload = new HttpClient();
                upload.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var form = new MultipartFormDataContent();
                var byteContent = new ByteArrayContent(pdfBytes);
                byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                form.Add(byteContent, "file", $"CV_{cv.Nome}.pdf");
                form.Add(new StringContent(idAlvo.ToString()), "idCandidatoFile");

                // CORREÇÃO: Adicionado o prefixo "api/" tanto no PUT como no POST do FileCV
                var putResponse = await upload.PutAsync(_baseUrl + $"api/filecv/{idAlvo}", form);
                if (putResponse.StatusCode == HttpStatusCode.NotFound)
                    await upload.PostAsync(_baseUrl + "api/filecv", form);
            }

            Response.Headers.Append("Content-Disposition", $"attachment; filename=CV_{cv.Nome}.pdf");

            return File(pdfBytes, "application/pdf");
        }
    }
}