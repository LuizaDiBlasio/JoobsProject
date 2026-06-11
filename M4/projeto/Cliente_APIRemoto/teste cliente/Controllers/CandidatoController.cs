using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;
using teste_cliente.Models.Dto;
using Vereyon.Web;

namespace teste_cliente.Controllers
{
    public class CandidatoController : Controller
    {
        private readonly string _baseUrl;
        private readonly IConfiguration _config;
        private readonly IFlashMessage _flashMessage;

        public CandidatoController(IConfiguration config, IFlashMessage flashMessage)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
            _flashMessage = flashMessage;
        }

        [Authorize(Roles = "Admin, Candidato")]
        public async Task<IActionResult> Index()
        {
            List<Candidato> candidatoList = new List<Candidato>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Confirmado com o Swagger: /api/candidato/BuscarTodos
                using (var response = await httpClient.GetAsync(_baseUrl + "api/candidato/BuscarTodos"))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    candidatoList = JsonConvert.DeserializeObject<List<Candidato>>(apiResponse);
                }
            }

            return View(candidatoList);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Get(int id)
        {
            Candidato candidato = new Candidato();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Confirmado com o Swagger: /api/candidato/BuscarPorId/{id}
                using (var response = await httpClient.GetAsync(_baseUrl + "api/candidato/BuscarPorId/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    candidato = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                }
            }
            return View(candidato);
        }

        [HttpGet]
        public async Task<IActionResult> Perfil(int id)
        {
            Candidato candidato = new Candidato();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await httpClient.GetAsync(_baseUrl + "api/candidato/BuscarPorId/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    candidato = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                }
            }
            return View(candidato);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Candidato candidato = new Candidato();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Confirmado com o Swagger: /api/candidato/EditarCandidato/{id}
                using (var response = await httpClient.GetAsync(_baseUrl + "api/candidato/EditarCandidato/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    candidato = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                }
                return View(candidato);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.Candidato candidato)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(candidato), Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await httpClient.PutAsync(_baseUrl + "api/candidato/EditarCandidato/" + candidato.IdCandidato, content))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                }

                return RedirectToAction("Details", new { id = candidato.IdCandidato });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Candidato candidato = null;

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await httpClient.GetAsync(_baseUrl + "api/candidato/BuscarPorId/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    candidato = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                }

                if (candidato == null)
                {
                    TempData["ErrorMessage"] = "Candidato não encontrado ou a conta foi apagada.";
                    return RedirectToAction("Index", "Home");
                }

                // Chame de foto unificada
                using (var fotoResponse = await httpClient.GetAsync(_baseUrl + $"api/foto/BuscarFotoPorIdCandidato/{id}"))
                {
                    if (fotoResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }

                    ViewBag.FotoExiste = fotoResponse.IsSuccessStatusCode;
                }
            }

            return View(candidato);
        }

        [Authorize(Roles = "Candidato,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var cookieHeader = HttpContext.Request.Headers["Cookie"].ToString();
                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                }

                // Confirmado com o Swagger: /api/candidato/DeletarCandidato/{id}
                var response = await httpClient.DeleteAsync(_baseUrl + "api/candidato/DeletarCandidato/" + id);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    TempData["ErrorMessage"] = "Não foi possível apagar a conta.";
                    return RedirectToAction("Details", new { id });
                }
            }

            if (User.IsInRole("Admin"))
            {
                TempData["SuccessMessage"] = "Candidato apagado com sucesso.";
                return RedirectToAction("Index", "Candidato");
            }
            else if (User.IsInRole("Candidato"))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                TempData["SuccessMessage"] = "Conta apagada com sucesso.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> HistoricoCandidaturas()
        {
            var idCandidato = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IdCandidato")?.Value;

            if (idCandidato == null)
            {
                return Forbid();
            }
            List<HistoricoCandidaturaDTO> historicoList = new List<HistoricoCandidaturaDTO>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Mapeamento mantido para a rota de histórico
                using (var response = await httpClient.GetAsync(_baseUrl + $"api/aplicacao/historico-candidato?idCandidato={idCandidato}"))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    historicoList = JsonConvert.DeserializeObject<List<HistoricoCandidaturaDTO>>(apiResponse);
                }
            }

            return View("HistoricoCandidaturas", historicoList);
        }

        [Authorize(Roles = "Candidato,Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string currentPassword, string newPassword)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var dto = new
            {
                IdCandidato = id,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            // Confirmado com o Swagger: /api/candidato/ChangePassword/{id}
            var response = await httpClient.PostAsync(_baseUrl + "api/candidato/ChangePassword/" + id, content);

            if (response.IsSuccessStatusCode)
            {
                _flashMessage.Confirmation("Password alterada com sucesso.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _flashMessage.Danger("Erro: " + error);
            }

            return RedirectToAction("Details", new { id });
        }
    }
}