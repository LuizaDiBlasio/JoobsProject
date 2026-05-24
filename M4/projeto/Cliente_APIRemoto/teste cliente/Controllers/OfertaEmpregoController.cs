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

namespace teste_cliente.Controllers
{
    public class OfertaEmpregoController : Controller
    {
        public async Task<IActionResult> Index(JornadaEnum? jornada, ConcelhoEnum? concelho, RegimeTrabalhoEnum? regimeTrabalho, string? search, int page = 1)
        {
            
            int pageSize = 10;
            List<OfertaEmpregoDTO> ofertaList = new List<OfertaEmpregoDTO>();

            //AQUI NÃO É PRECISO O TOKEN
            using (var httpClient = new HttpClient())
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (concelho.HasValue) queryParams.Add($"concelho={(int)concelho}");
                if (jornada.HasValue) queryParams.Add($"jornada={(int)jornada}");
                if (regimeTrabalho.HasValue) queryParams.Add($"regimeTrabalho={(int)regimeTrabalho}");

                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                string apiUrl = $"https://localhost:7211/api/oferta/TodasOfertas{queryString}";


                    // Buscar a lista de ofertas
                    using (var response = await httpClient.GetAsync(apiUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                            ofertaList = JsonConvert.DeserializeObject<List<OfertaEmpregoDTO>>(apiResponse) ?? new List<OfertaEmpregoDTO>(); 
                        }
                    }

                    // Obter IDs únicos de empresas para buscar as reviews
                    var empresaIds = ofertaList.Select(o => o.IdEmpresa).Distinct().ToList();
                    var reviewsByEmpresa = new Dictionary<int, List<Review>>();

                    // Buscar as reviews para cada empresa
                    foreach (var idEmpresa in empresaIds)
                    {
                        using (var reviewResponse = await httpClient.GetAsync($"https://localhost:7211/api/Review/empresa/{idEmpresa}"))
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

                    // Buscar o logo e associar as reviews a cada oferta
                    foreach (var oferta in ofertaList)
                    {
                        var logoEmpresaBase64 = await GetLogoByEmpresaId(oferta.IdEmpresa);
                        oferta.LogoEmpresaBase64 = logoEmpresaBase64;

                        // Associar as reviews da empresa à oferta via ViewData ou uma propriedade temporária
                        ViewData[$"Reviews_{oferta.IdOferta}"] = reviewsByEmpresa.ContainsKey(oferta.IdEmpresa) ? reviewsByEmpresa[oferta.IdEmpresa] : new List<Review>();
                    }
                
            }

            int totalItems = ofertaList.Count;

            ofertaList = ofertaList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 🔽 FAVORITOS via Cookie
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

            //Buscar listas 
            var listas = LoadLists();

            var model = new IndexOfertaViewModel
            {
                OfertaEmpregosList = ofertaList,
                Concelho = concelho,
                Jornada = jornada,
                RegimeTrabalho = regimeTrabalho
            };


            // 🔽 Enviar info para  a View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;
            ViewBag.Favoritos = favoritos;
            ViewBag.Concelho = concelho;
            ViewBag.Jornada = jornada;
            ViewBag.RegimeTrabalho = regimeTrabalho;

            if (listas != null)
            {
                ViewBag.ConcelhosList = listas.SelectListConcelhos;
                ViewBag.JornadasList = listas.SelectListJornada;
                ViewBag.RegimeTrabalhoList = listas.SelectListRegimeTrabalho;
            }

            model.OfertaEmpregosList = ofertaList;

            return View(model);
        }


        private async Task<string> GetLogoByEmpresaId(int idEmpresa)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;


            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                using (var response = await httpClient.GetAsync($"https://localhost:7211/api/Logo/{idEmpresa}"))
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
                
                string apiUrl = $"https://localhost:7211/api/Oferta/historicoEmpresa?idEmpresa={idEmpresa}";

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

            //AQUI NÃO PRECISA DO TOKEN
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://localhost:7211/api/oferta/BuscarPorId/" + id))
                {   
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);
                }
            }
            return View(oferta);
        }

        //___________MODIFICAÇÃO DE CODIGO_________(Get com lista de comboboxes)
        //[HttpGet]
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // ______________ADIÇÃO DE CÓDIGO____________
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new OfertaEmprego();

            var lists = LoadLists();

            model.SelectListConcelhos = lists.SelectListConcelhos;
            model.SelectListTiposContratos = lists.SelectListContratos;
            model.SelectListRegimeTrabalho = lists.SelectListRegimeTrabalho;
            model.SelectListJornada = lists.SelectListJornada;

            return View(model);
               
        }


        [HttpPost]
        public async Task<IActionResult> Create(Models.OfertaEmprego oferta)
        {
            if (ModelState.IsValid)
            {
                var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    StringContent content = new StringContent(JsonConvert.SerializeObject(oferta), Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync("https://localhost:7211/api/Oferta/CriarOferta/", content))
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
                        oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);
                    }
                }

                return RedirectToAction("Index");
            }


            oferta.SelectListConcelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();

            oferta.SelectListTiposContratos = EnumHelper.ObterSelectListDoEnum<TipoContratoEnum>();

            return View(oferta);
           
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            OfertaEmprego oferta = new OfertaEmprego();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                using (var response = await httpClient.GetAsync("https://localhost:7211/api/Oferta/EditarOferta/" + id))
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
                    oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);

                }
                return View(oferta);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.OfertaEmprego oferta)
        {
            Candidato e = new Candidato();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                StringContent content = new StringContent(JsonConvert.SerializeObject(oferta), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PutAsync("https://localhost:7211/api/Oferta/EditarOferta/" + oferta.IdOferta, content))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                        return Forbid();
                    }
                    

                    string apiResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.Result = "Success";
                    e = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                }
                return RedirectToAction("Historico");

            }

            return View(e);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            OfertaEmprego oferta = new OfertaEmprego();

            using (var httpClient = new HttpClient())
            {
                // Incrementar a contagem apenas se o usuário for um Candidato
                if (User.IsInRole("Candidato"))
                {
                    using (var response = await httpClient.PatchAsync($"https://localhost:7211/api/oferta/{id}/incrementarContagem", null))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            // Logar erro, se necessário, mas não bloquear a exibição dos detalhes
                            Console.WriteLine($"Erro ao incrementar contagem: {response.StatusCode}");
                        }
                    }
                }

                // Buscar os detalhes da oferta
                using (var response = await httpClient.GetAsync("https://localhost:7211/api/Oferta/BuscarPorId/" + id))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);
                }

                // Buscar o logo da empresa
                var logoEmpresaBase64 = await GetLogoByEmpresaId(oferta.IdEmpresa);
                oferta.LogoEmpresaBase64 = logoEmpresaBase64;

                // Buscar as reviews da empresa associada à oferta
                List<Review> reviews = new List<Review>();
                using (var reviewResponse = await httpClient.GetAsync($"https://localhost:7211/api/Review/empresa/{oferta.IdEmpresa}"))
                {
                    if (reviewResponse.IsSuccessStatusCode)
                    {
                        string reviewApiResponse = await reviewResponse.Content.ReadAsStringAsync();
                        reviews = JsonConvert.DeserializeObject<List<Review>>(reviewApiResponse);
                    }
                }

                // Passar as reviews para a view via ViewBag
                ViewBag.Reviews = reviews ?? new List<Review>();

                return View(oferta);
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Details(int id)
        //{
        //    OfertaEmprego oferta = new OfertaEmprego();
        //    using (var httpClient = new HttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync("https://localhost:7211/api/Oferta/" + id))
        //        {
        //            string apiResponse = await response.Content.ReadAsStringAsync();
        //            oferta = JsonConvert.DeserializeObject<OfertaEmprego>(apiResponse);

        //        }

        //        var logoEmpresaBase64 = await GetLogoByEmpresaId(oferta.IdEmpresa);
        //        oferta.LogoEmpresaBase64 = logoEmpresaBase64;

        //        return View(oferta);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                using (var response = await httpClient.DeleteAsync("https://localhost:7211/api/Oferta/" + id))
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
            // 1. Obter os IDs dos favoritos do cookie
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

            // 2. Buscar todas as ofertas da API
            List<OfertaEmprego> todasOfertas = new List<OfertaEmprego>();
            using (var httpClient = new HttpClient())
            {
                string apiUrl = "https://localhost:7211/api/Oferta/TodasOfertas";
                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        todasOfertas = JsonConvert.DeserializeObject<List<OfertaEmprego>>(apiResponse) ?? new List<OfertaEmprego>();
                    }
                }
            }

            // 3. Filtrar apenas as favoritas
            var ofertasFavoritas = todasOfertas.Where(o => favoritosIds.Contains(o.IdOferta)).ToList();

            return Json(ofertasFavoritas);
        }

        //ADIÇÃO DE CÓDIGO (métodos auxiliares)

        //____________ADIÇÂO DE CÓDIGO___________(carregamento de listas para index)
        private ListsDTO LoadLists()
            {
                var listsDTO = new ListsDTO();

                //converter para selectlist

                listsDTO.SelectListConcelhos = EnumHelper.ObterSelectListDoEnum<ConcelhoEnum>();

                listsDTO.SelectListContratos = EnumHelper.ObterSelectListDoEnum<TipoContratoEnum>();

                listsDTO.SelectListJornada = EnumHelper.ObterSelectListDoEnum<JornadaEnum>();

                listsDTO.SelectListRegimeTrabalho = EnumHelper.ObterSelectListDoEnum<RegimeTrabalhoEnum>(); 
 
                return listsDTO;   
            }

    }
}
