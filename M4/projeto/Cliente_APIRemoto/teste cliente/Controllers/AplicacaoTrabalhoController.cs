using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;

namespace teste_cliente.Controllers
{
    public class AplicacaoTrabalhoController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _baseUrl;

        public AplicacaoTrabalhoController(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];

        }
        public async Task<IActionResult> Index()
        {
            List<AplicacaoTrabalho> aplicacaoList = new List<AplicacaoTrabalho>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var response = await httpClient.GetAsync( _baseUrl + "aplicacao/BuscarTodas"))
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
                    aplicacaoList = JsonConvert.DeserializeObject<List<AplicacaoTrabalho>>(apiResponse);
                }
            }

            return View(aplicacaoList);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Get(int id)
        {
            AplicacaoTrabalho aplicacao = new AplicacaoTrabalho();
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var response = await httpClient.GetAsync(_baseUrl + "aplicacao/BuscarPorID/" + id))
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
                    aplicacao = JsonConvert.DeserializeObject<AplicacaoTrabalho>(apiResponse);
                }
            }
            return View(aplicacao);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Models.AplicacaoTrabalho aplicacao)
        {
           
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");
            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(aplicacao), Encoding.UTF8, "application/json");
                    
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                using (var response = await httpClient.PostAsync(_baseUrl + "aplicacao/CriarAplicacao/", content))
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
                    aplicacao = JsonConvert.DeserializeObject<AplicacaoTrabalho>(apiResponse);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            AplicacaoTrabalho aplicacao = new AplicacaoTrabalho();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                using (var response = await httpClient.GetAsync(_baseUrl + "aplicacao/EditarAplicacao/" + id))
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
                    aplicacao = JsonConvert.DeserializeObject<AplicacaoTrabalho>(apiResponse);

                }
                return View(aplicacao);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.AplicacaoTrabalho aplicacao)
        {
            AplicacaoTrabalho e = new AplicacaoTrabalho();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(aplicacao), Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                using (var response = await httpClient.PutAsync(_baseUrl + "aplicacao/EditarAplicacao/" + aplicacao.IdAplicacao, content))
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
                    ViewBag.Result = "Success";
                    e = JsonConvert.DeserializeObject<AplicacaoTrabalho>(apiResponse);
                }
                return RedirectToAction("Index");

            }

            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            AplicacaoTrabalho aplicacao = new AplicacaoTrabalho();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                using (var response = await httpClient.GetAsync(_baseUrl + "aplicacao/BuscarPorID/" + id))
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
                    aplicacao = JsonConvert.DeserializeObject<AplicacaoTrabalho>(apiResponse);

                }
                return View(aplicacao);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await httpClient.DeleteAsync(_baseUrl + "aplicacao/DeletarAplicacao/" + id))
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
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Candidato")]
        [HttpPost]
        public async Task<IActionResult> Candidatar(int id) // id da oferta
        {
            var idCandidato = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IdCandidato")?.Value;
            if (idCandidato == null)
                return Forbid();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            // Verifica se já existe candidatura
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                httpClient.BaseAddress = new Uri(_baseUrl);
                
                var response = await httpClient.GetAsync($"aplicacao/verificar?idOferta={id}&idCandidato={idCandidato}");

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                    return Forbid();
                }
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                if (response.IsSuccessStatusCode)
                {
                    bool jaExiste = bool.Parse(await response.Content.ReadAsStringAsync());
                    if (jaExiste)
                    {
                        TempData["Mensagem"] = "Já submeteste uma candidatura para esta oferta.";
                        return RedirectToAction("Details", "OfertaEmprego", new { id = id });
                    }
                }

                // Criar nova candidatura
                var novaAplicacao = new AplicacaoTrabalho
                {
                    IdOferta = id,
                    IdCandidato = int.Parse(idCandidato),
                };

                var json = JsonConvert.SerializeObject(novaAplicacao);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var postResponse = await httpClient.PostAsync("aplicacao/CriarAplicacao", content);
                if (!postResponse.IsSuccessStatusCode)
                {
                    TempData["Mensagem"] = "Ocorreu um erro ao candidatar-te.";
                    return RedirectToAction("Details", "OfertaEmprego", new { id = id });
                }
            }

            TempData["Mensagem"] = "Candidatura submetida com sucesso!";
            return RedirectToAction("Details", "OfertaEmprego", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> CarregarCandidaturas(int idOferta)
        {
            List<AplicacaoTrabalho> candidaturas = new List<AplicacaoTrabalho>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                string apiUrl = _baseUrl + $"aplicacao/idOferta?idOferta={idOferta}";

                using (var response = await httpClient.GetAsync(apiUrl))
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
                    candidaturas = JsonConvert.DeserializeObject<List<AplicacaoTrabalho>>(apiResponse);
                }
            }

            return PartialView("_Candidaturas", candidaturas);
        }
    }
}
