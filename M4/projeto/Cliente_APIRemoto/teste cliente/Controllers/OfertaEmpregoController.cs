using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using teste_cliente.Helpers;
using teste_cliente.Models;
using teste_cliente.Models.Dto;
using teste_cliente.Models.Enums;
using teste_cliente.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace teste_cliente.Controllers
{
    public class OfertaEmpregoController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _baseUrl;

        public OfertaEmpregoController(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
        }

        public async Task<IActionResult> Index(JornadaEnum? jornada, ConcelhoEnum? concelho, RegimeTrabalhoEnum? regimeTrabalho, string? search, string? faixaSalarial, int page = 1)
        {
            int pageSize = 10;
            List<OfertaEmpregoDTO> ofertaList = new List<OfertaEmpregoDTO>();

            using (var httpClient = new HttpClient())
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (concelho.HasValue) queryParams.Add($"concelho={(int)concelho}");
                if (jornada.HasValue) queryParams.Add($"jornada={(int)jornada}");
                if (regimeTrabalho.HasValue) queryParams.Add($"regimeTrabalho={(int)regimeTrabalho}");
                if (!string.IsNullOrEmpty(faixaSalarial)) queryParams.Add($"faixaSalarial={Uri.EscapeDataString(faixaSalarial)}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

                // CORREÇÃO: Adicionado o prefixo "api/" e corrigido para minúsculas "oferta"
                string apiUrl = _baseUrl + $"api/oferta/TodasOfertas{queryString}";

                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        ofertaList = JsonConvert.DeserializeObject<List<OfertaEmpregoDTO>>(apiResponse) ?? new List<OfertaEmpregoDTO>();
                    }
                }

                var empresaIds = ofertaList.Select(o => o.IdEmpresa).Distinct().ToList();
                var reviewsByEmpresa = new Dictionary<int, List<Review>>();

                foreach (var idEmpresa in empresaIds)
                {
                    // CORREÇÃO: Adicionado "api/" e corrigido "review" para minúsculas
                    using (var reviewResponse = await httpClient.GetAsync(_baseUrl + $"api/review/empresa/{idEmpresa}"))
                    {
                        if (reviewResponse.IsSuccessStatusCode)
                        {
                            string reviewApiResponse = await reviewResponse.Content.ReadAsStringAsync();
                            var reviews = JsonConvert.DeserializeObject<List<Review>>(reviewApiResponse);
                            reviewsByEmpresa[idEmpresa] = reviews ?? new List<Review>();
                        }
                        else
                        {
                            reviewsByEmpresa[idEmpresa] = new List<Review>();
                        }
                    }
                }

                foreach (var oferta in ofertaList)
                {
                    var logoEmpresaBase64 = await GetLogoByEmpresaId(oferta.IdEmpresa);
                    oferta.LogoEmpresaBase64 = logoEmpresaBase64;

                    ViewData[$"Reviews_{oferta.IdOferta}"] = reviewsByEmpresa.ContainsKey(oferta.IdEmpresa) ? reviewsByEmpresa[oferta.IdEmpresa] : new List<Review>();
                }
            }

            int totalItems = ofertaList.Count;

            ofertaList = ofertaList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idClaim = identity?.FindFirst("IdCandidato");
            List<int> favoritos = new();

            if (idClaim != null)
            {
                var cookieKey = $"Favoritos_{idClaim.Value}";
                if (Request.Cookies.TryGetValue(cookieKey, out string cookieValue))
                {
                    favoritos = JsonConvert.DeserializeObject<List<int>>(cookieValue) ?? new List<int>();
                }
            }

            var model = new IndexOfertaViewModel
            {
                OfertaEmpregosList = ofertaList,
                Concelho = concelho,
                Jornada = jornada,
                RegimeTrabalho = regimeTrabalho
            };

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;
            ViewBag.Favoritos = favoritos;
            ViewBag.Concelho = concelho;
            ViewBag.Jornada = jornada;
            ViewBag.RegimeTrabalho = regimeTrabalho;
            ViewBag.ConcelhosList = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            ViewBag.JornadasList = EnumHelper.ObterSelectListDoEnum<JornadaEnum>();
            ViewBag.RegimeTrabalhoList = EnumHelper.ObterSelectListDoEnum<RegimeTrabalhoEnum>();

            model.OfertaEmpregosList = ofertaList;

            return View(model);
        }

        private async Task<string> GetLogoByEmpresaId(int idEmpresa)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Rota corrigida com o padrão real do Swagger "api/logo/empresa/{id}"
                using (var response = await httpClient.GetAsync(_baseUrl + $"api/logo/empresa/{idEmpresa}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var logoData = JsonConvert.DeserializeObject<LogoEmpresa>(jsonResponse);

                        if (logoData?.Logo != null)
                        {
                            return Convert.ToBase64String(logoData.Logo);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<IActionResult> Historico()
        {
            List<OfertaEmprego> ofertasEmpresa = new List<OfertaEmprego>();

            var identity = HttpContext.User.Identity as System.Security.Claims.ClaimsIdentity;
            var idClaim = identity?.FindFirst("IdEmpresa");

            if (idClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            int idEmpresa = int.Parse(idClaim.Value);

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado o prefixo "api/" e corrigido "oferta" para minúsculas
                string apiUrl = _baseUrl + $"api/oferta/historicoEmpresa?idEmpresa={idEmpresa}";

                using (var response = await httpClient.GetAsync(apiUrl))
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
                    ofertasEmpresa = JsonConvert.DeserializeObject<List<OfertaEmprego>>(apiResponse);
                }
            }

            return View("Historico", ofertasEmpresa);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Get(int id)
        {
            OfertaEmprego oferta = new OfertaEmprego();

            using (var httpClient = new HttpClient())
            {
                // CORREÇÃO: Adicionado "api/" e corrigido "oferta" para minúsculas
                using (var response = await httpClient.GetAsync(_baseUrl + "api/oferta/BuscarPorId/" + id))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);
                }
            }
            return View(oferta);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new OfertaEmpregoViewModel();
            LoadLists(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OfertaEmpregoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var ofertaDTO = new OfertaEmpregoDTO
                    {
                        Concelho = model.Concelho,
                        TipoContrato = model.TipoContrato,
                        RegimeTrabalho = model.RegimeTrabalho,
                        Jornada = model.Jornada,
                        Titulo = model.Titulo,
                        Requisitos = model.Requisitos,
                        VagaDisponivel = true,
                        Descricao = model.Descricao,
                        Salario = model.Salario
                    };

                    StringContent content = new StringContent(JsonConvert.SerializeObject(ofertaDTO), Encoding.UTF8, "application/json");

                    // CORREÇÃO: Adicionado "api/", corrigido para minúsculas e limpo caractere "/" extra do final da rota
                    using (var response = await httpClient.PostAsync(_baseUrl + "api/oferta/CriarOferta", content))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            return Forbid();
                        }
                        if (!response.IsSuccessStatusCode)
                        {
                            string erroDetalhado = await response.Content.ReadAsStringAsync();
                            return NotFound();
                        }
                    }
                }

                return RedirectToAction("Index");
            }

            LoadLists(model);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var model = new OfertaEmpregoViewModel();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado "api/" e corrigido "oferta" para minúsculas
                using (var response = await httpClient.GetAsync(_baseUrl + "api/oferta/EditarOferta/" + id))
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
                    var ofertaDTO = JsonConvert.DeserializeObject<OfertaEmpregoDTO>(apiResponse);

                    model.Salario = ofertaDTO.Salario;
                    model.Titulo = ofertaDTO.Titulo;
                    model.TipoContrato = ofertaDTO.TipoContrato;
                    model.Concelho = ofertaDTO.Concelho;
                    model.Descricao = ofertaDTO.Descricao;
                    model.Jornada = ofertaDTO.Jornada;
                    model.Requisitos = ofertaDTO.Requisitos;
                    model.RegimeTrabalho = ofertaDTO.RegimeTrabalho;
                    model.IdOferta = id;
                    model.VagaDisponivel = ofertaDTO.VagaDisponivel;
                    model.Contagem = ofertaDTO.Contagem;

                    LoadLists(model);
                }
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OfertaEmpregoViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                StringContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                // CORREÇÃO: Adicionado "api/" e corrigido para minúsculas "oferta"
                using (var response = await httpClient.PutAsync(_baseUrl + "api/oferta/EditarOferta/" + model.IdOferta, content))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.Result = "Success";
                }
                return RedirectToAction("Historico");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            OfertaEmprego oferta = new OfertaEmprego();

            using (var httpClient = new HttpClient())
            {
                if (User.IsInRole("Candidato"))
                {
                    // CORREÇÃO: Adicionado o prefixo "api/" e corrigido para minúsculas
                    using (var response = await httpClient.PatchAsync(_baseUrl + $"api/oferta/{id}/incrementarContagem", null))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Erro ao incrementar contagem: {response.StatusCode}");
                        }
                    }
                }

                // CORREÇÃO: Adicionado "api/" e corrigido "oferta" para minúsculas
                using (var response = await httpClient.GetAsync(_baseUrl + "api/oferta/BuscarPorId/" + id))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);
                }

                var logoEmpresaBase64 = await GetLogoByEmpresaId(oferta.IdEmpresa);
                oferta.LogoEmpresaBase64 = logoEmpresaBase64;

                List<Review> reviews = new List<Review>();

                // CORREÇÃO: Adicionado "api/" e corrigido "review" para minúsculas
                using (var reviewResponse = await httpClient.GetAsync(_baseUrl + $"api/review/empresa/{oferta.IdEmpresa}"))
                {
                    if (reviewResponse.IsSuccessStatusCode)
                    {
                        string reviewApiResponse = await reviewResponse.Content.ReadAsStringAsync();
                        reviews = JsonConvert.DeserializeObject<List<Review>>(reviewApiResponse);
                    }
                }

                ViewBag.Reviews = reviews ?? new List<Review>();

                return View(oferta);
            }
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

                // CORREÇÃO: Adicionado "api/" e corrigido para minúsculas "oferta"
                using (var response = await httpClient.DeleteAsync(_baseUrl + "api/oferta/" + id))
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFavorito(int idOferta)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idClaim = identity?.FindFirst("IdCandidato");

            if (idClaim == null)
            {
                return Json(new { success = false, message = "Não autenticado." });
            }

            var idCandidato = idClaim.Value;
            var cookieKey = $"Favoritos_{idCandidato}";
            var favoritos = new List<int>();

            if (Request.Cookies.TryGetValue(cookieKey, out string cookieValue))
            {
                favoritos = JsonConvert.DeserializeObject<List<int>>(cookieValue) ?? new List<int>();
            }

            bool isFavorito = favoritos.Remove(idOferta);
            if (!isFavorito)
            {
                favoritos.Add(idOferta);
                isFavorito = true;
            }

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = false,
                IsEssential = true
            };

            Response.Cookies.Append(cookieKey, JsonConvert.SerializeObject(favoritos), cookieOptions);

            return Json(new { success = true, isFavorito = favoritos.Contains(idOferta) });
        }

        [HttpGet]
        public IActionResult GetFavoritosIds()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idClaim = identity?.FindFirst("IdCandidato");

            if (idClaim == null)
            {
                return Unauthorized();
            }

            var idCandidato = idClaim.Value;
            var cookieKey = $"Favoritos_{idCandidato}";
            List<int> favoritos = new();

            if (Request.Cookies.TryGetValue(cookieKey, out string cookieValue))
            {
                favoritos = JsonConvert.DeserializeObject<List<int>>(cookieValue) ?? new();
            }

            return Json(favoritos);
        }

        [HttpGet]
        public async Task<IActionResult> GetOfertasFavoritas()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idClaim = identity?.FindFirst("IdCandidato");
            if (idClaim == null)
            {
                return Unauthorized();
            }

            var idCandidato = idClaim.Value;
            var cookieKey = $"Favoritos_{idCandidato}";
            List<int> favoritosIds = new List<int>();

            if (Request.Cookies.TryGetValue(cookieKey, out string cookieValue))
            {
                favoritosIds = JsonConvert.DeserializeObject<List<int>>(cookieValue) ?? new List<int>();
            }

            List<OfertaEmprego> todasOfertas = new List<OfertaEmprego>();
            using (var httpClient = new HttpClient())
            {
                // CORREÇÃO: Adicionado o prefixo "api/" e corrigido para minúsculas "oferta"
                string apiUrl = _baseUrl + "api/oferta/TodasOfertas";
                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        todasOfertas = JsonConvert.DeserializeObject<List<OfertaEmprego>>(apiResponse) ?? new List<OfertaEmprego>();
                    }
                }
            }

            var ofertasFavoritas = todasOfertas.Where(o => favoritosIds.Contains(o.IdOferta)).ToList();

            return Json(ofertasFavoritas);
        }

        private void LoadLists(OfertaEmpregoViewModel model)
        {
            model.SelectListConcelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();
            model.SelectListTiposContratos = EnumHelper.ObterSelectListDoEnum<TipoContratoEnum>();
            model.SelectListJornada = EnumHelper.ObterSelectListDoEnum<JornadaEnum>();
            model.SelectListRegimeTrabalho = EnumHelper.ObterSelectListDoEnum<RegimeTrabalhoEnum>();
        }
    }
}