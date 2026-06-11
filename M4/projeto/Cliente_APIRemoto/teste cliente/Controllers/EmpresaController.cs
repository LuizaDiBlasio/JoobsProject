using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using teste_cliente.Models.Enums;
using teste_cliente.Helpers;
using System.Linq;

namespace teste_cliente.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _baseUrl;

        public EmpresaController(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
        }

        [Authorize(Roles = "Admin, Empresa")]
        public async Task<IActionResult> Index()
        {
            List<Empresa> empresaList = new List<Empresa>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado o prefixo "api/" conforme o Swagger
                using (var response = await httpClient.GetAsync(_baseUrl + "api/empresa/BuscarTodas"))
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
                    empresaList = JsonConvert.DeserializeObject<List<Empresa>>(apiResponse);
                }
            }

            return View(empresaList);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Get(int id)
        {
            Empresa empresa = new Empresa();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Rota alterada para o padrão correto do Swagger "api/empresa/BuscarPorId/{id}"
                using (var response = await httpClient.GetAsync(_baseUrl + "api/empresa/BuscarPorId/" + id))
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
                    empresa = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                }
            }
            return View(empresa);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Models.Empresa empresa)
        {
            if (ModelState.IsValid)
            {
                var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    StringContent content = new StringContent(JsonConvert.SerializeObject(empresa), Encoding.UTF8, "application/json");

                    // CORREÇÃO: Higienização de barras e inclusão do prefixo padrão da API
                    using (var response = await httpClient.PostAsync(_baseUrl + "api/empresa", content))
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
                        empresa = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                    }
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Empresa empresa = new Empresa();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Rota mapeada com base no padrão "api/empresa/BuscarPorId/{id}"
                using (var response = await httpClient.GetAsync(_baseUrl + "api/empresa/BuscarPorId/" + id))
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
                    empresa = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                }

                ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
                return View(empresa);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.Empresa empresa)
        {
            if (!ModelState.IsValid)
            {
                List<Review> reviews = new List<Review>();
                using (var httpClient = new HttpClient())
                {
                    // CORREÇÃO: Rota de reviews higienizada com o prefixo "api/"
                    var respRev = await httpClient.GetAsync(_baseUrl + $"api/review/empresa/{empresa.IdEmpresa}");
                    if (respRev.IsSuccessStatusCode)
                    {
                        var jsonRev = await respRev.Content.ReadAsStringAsync();
                        reviews = JsonConvert.DeserializeObject<List<Review>>(jsonRev);
                    }
                }

                ViewBag.Reviews = reviews;
                return View("Details", empresa);
            }

            Empresa e = new Empresa();
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(empresa), Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Rota do PUT corrigida com o prefixo "api/" conforme o Swagger
                using (var response = await httpClient.PutAsync(_baseUrl + "api/empresa/EditarEmpresa/" + empresa.IdEmpresa, content))
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
                    ViewBag.Result = "Success";
                    e = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                }
                return RedirectToAction("Details", new { id = empresa.IdEmpresa });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            Empresa empresa = null;
            List<Review> reviews = new List<Review>();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Rota unificada com o padrão "api/empresa/BuscarPorId/{id}" do Swagger
                var respEmp = await httpClient.GetAsync(_baseUrl + $"api/empresa/BuscarPorId/{id}");
                if (respEmp.IsSuccessStatusCode)
                {
                    var jsonEmp = await respEmp.Content.ReadAsStringAsync();
                    empresa = JsonConvert.DeserializeObject<Empresa>(jsonEmp);
                }
                else
                {
                    if (respEmp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    if (!respEmp.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }
                }

                if (empresa == null)
                    return RedirectToAction("Index", "Home");

                // CORREÇÃO: Rota de reviews atualizada com "api/"
                var respRev = await httpClient.GetAsync(_baseUrl + $"api/review/empresa/{id}");
                if (respRev.IsSuccessStatusCode)
                {
                    var jsonRev = await respRev.Content.ReadAsStringAsync();
                    reviews = JsonConvert.DeserializeObject<List<Review>>(jsonRev);
                }
            }

            ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            ViewBag.Reviews = reviews;
            return View(empresa);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Rota do DELETE atualizada com o prefixo "api/"
                using (var response = await httpClient.DeleteAsync(_baseUrl + "api/empresa/DeletarEmpresa/" + id))
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
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Empresa");
            }
            else if (User.IsInRole("Empresa"))
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        [Authorize(Roles = "Empresa,Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangePasswordEmpresa(int id, string currentPassword, string newPassword)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var dto = new
            {
                IdEmpresa = id,
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            // CORREÇÃO: Rota do POST de alteração de senha corrigida com o prefixo "api/"
            var response = await httpClient.PostAsync(_baseUrl + "api/empresa/ChangePassword/" + id, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Password alterada com sucesso.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Erro: " + error;
            }

            return RedirectToAction("Details", new { id });
        }

        // ==================================================================================================== //
        // MÉTODOS DE PESQUISA DE CANDIDATOS

        [Authorize(Roles = "Admin, Empresa")]
        [HttpGet]
        public IActionResult PesquisaCandidatos()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            List<Candidato> candidatos = new List<Candidato>();

            ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            ViewBag.Escolaridades = EnumHelper.ObterSelectListDoEnum<EscolaridadeEnum>();
            ViewBag.FoiPesquisado = false;

            return View(candidatos);
        }

        [Authorize(Roles = "Admin, Empresa")]
        [HttpPost]
        public async Task<IActionResult> PesquisaCandidatos(ConcelhoEnum? concelho, EscolaridadeEnum? escolaridade)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            List<Candidato> candidatos = new List<Candidato>();

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var queryParams = new List<string>();
                    if (concelho.HasValue) queryParams.Add($"concelho={(int)concelho.Value}");
                    if (escolaridade.HasValue) queryParams.Add($"escolaridade={(int)escolaridade.Value}");

                    // CORREÇÃO: Rota de pesquisa ajustada com o prefixo obrigatório "api/" conforme o Swagger
                    string url = _baseUrl + "api/candidato/Pesquisar";
                    if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

                    using (var response = await httpClient.GetAsync(url))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) return Forbid();

                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            var dtoResult = JsonConvert.DeserializeObject<teste_cliente.Models.Dto.CandidatoPesquisaDTO>(apiResponse);

                            if (dtoResult?.Candidatos != null)
                            {
                                foreach (var item in dtoResult.Candidatos)
                                {
                                    var candidatoReal = new Candidato
                                    {
                                        IdCandidato = item.IdCandidato,
                                        Nome = item.Nome,
                                        Email = item.Email,
                                        Telefone = item.Telefone,
                                        CV = new List<CV>()
                                    };

                                    if (item.CV != null)
                                    {
                                        candidatoReal.CV.Add(new CV
                                        {
                                            Concelho = (ConcelhoEnum)item.CV.Concelho,
                                            Escolaridade = (EscolaridadeEnum)item.CV.Escolaridade
                                        });
                                    }

                                    candidatos.Add(candidatoReal);
                                }
                            }
                        }
                        else
                        {
                            string erroApi = await response.Content.ReadAsStringAsync();
                            ModelState.AddModelError("", $"Erro da API: {erroApi}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro de comunicação: " + ex.Message);
            }

            ViewBag.Concelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            ViewBag.Escolaridades = EnumHelper.ObterSelectListDoEnum<EscolaridadeEnum>();
            ViewBag.ConcelhoSelecionado = (int?)concelho;
            ViewBag.EscolaridadeSelecionado = (int?)escolaridade;
            ViewBag.FoiPesquisado = true;

            return View(candidatos);
        }
    }
}