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
                
                using (var response = await httpClient.GetAsync(_baseUrl + "candidato/BuscarTodos"))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
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

                using (var response = await httpClient.GetAsync(_baseUrl + "candidato/BuscarPorId/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
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
                using (var response = await httpClient.GetAsync(_baseUrl + "candidato/EditarCandidato/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
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

                using (var response = await httpClient.PutAsync(_baseUrl + "candidato/EditarCandidato/" + candidato.IdCandidato, content))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
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

                // Tenta buscar o candidato pela API
                using (var response = await httpClient.GetAsync(_baseUrl + "candidato/BuscarPorId/" + id))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        //vai cair aqui quando o filtro da API devolver 403
                        return Forbid();
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        // 404, 500…
                        return NotFound();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    candidato = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                }

                // Se não encontrar o candidato, redireciona para a Home ou para outra página
                if (candidato == null)
                {
                    TempData["ErrorMessage"] = "Candidato não encontrado ou a conta foi apagada.";
                    return RedirectToAction("Index", "Home");
                }

                // Verifica se a foto existe para este candidato
                using (var fotoResponse = await httpClient.GetAsync(_baseUrl + $"foto/BuscarFotoPorIdCandidato/{id}"))
                {
                    if (fotoResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
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
            Console.WriteLine($"MVC.Delete chamado com id = {id}");

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var cookieHeader = HttpContext.Request.Headers["Cookie"].ToString();
                System.Diagnostics.Debug.WriteLine("Cookie Header: " + cookieHeader);
                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                }

                var response = await httpClient.DeleteAsync(_baseUrl + "candidato/DeletarCandidato/" + id);
                string apiResponse = await response.Content.ReadAsStringAsync();

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

            // Verifica a role do usuário
            if (User.IsInRole("Admin"))
            {
                // Admin apenas redireciona
                TempData["SuccessMessage"] = "Candidato apagado com sucesso.";
                return RedirectToAction("Index", "Candidato"); // Exemplo: lista de candidatos
            }
            else if (User.IsInRole("Candidato"))
            {
                // 1) Limpa o cookie de autenticação
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                // 2) Limpa a sessão
                HttpContext.Session.Clear();
                // 3) Mensagem
                TempData["SuccessMessage"] = "Conta apagada com sucesso.";
                // 4) Redireciona para Home
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // fallback seguro
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
                using (var response = await httpClient.GetAsync(_baseUrl + $"aplicacao/historico-candidato?idCandidato={idCandidato}"))
                {   
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
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
            httpClient.DefaultRequestHeaders.Authorization =
                 new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                _baseUrl + "candidato/ChangePassword/" + id, content);

            if (response.IsSuccessStatusCode)
            {
                _flashMessage.Confirmation("Password alterada com sucesso.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _flashMessage.Confirmation("Erro: " + error) ;
            }

            return RedirectToAction("Details", new { id });
        }

    }
}
