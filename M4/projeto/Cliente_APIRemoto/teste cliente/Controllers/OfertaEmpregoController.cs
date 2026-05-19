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
using teste_cliente.Models;
using teste_cliente.Models.Dto;

namespace teste_cliente.Controllers
{
    public class OfertaEmpregoController : Controller
    {
        public async Task<IActionResult> Index(string? search, string? concelho, string? regimeTrabalho, int page = 1)
        {
            int pageSize = 10;
            List<OfertaEmprego> ofertaList = new List<OfertaEmprego>();
            List<Concelho> concelhosList = new List<Concelho>();

            //AQUI NÃO É PRECISO O TOKEN
            using (var httpClient = new HttpClient())
            {
                string apiUrl = "https://localhost:7211/api/oferta/TodasOfertas";

                List<string> queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={search}");
                if (!string.IsNullOrEmpty(concelho))
                    queryParams.Add($"concelho={concelho}");
                if (!string.IsNullOrEmpty(regimeTrabalho))
                    queryParams.Add($"regimeTrabalho={regimeTrabalho}");

                if (queryParams.Count > 0)
                    apiUrl += "?" + string.Join("&", queryParams);

                // Buscar a lista de ofertas
                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    ofertaList = JsonConvert.DeserializeObject<List<OfertaEmprego>>(apiResponse);
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

            foreach (var oferta in ofertaList)
            {
                var logoEmpresaBase64 = await GetLogoByEmpresaId(oferta.IdEmpresa);
                oferta.LogoEmpresaBase64 = logoEmpresaBase64;
            }

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
            var listas = await LoadListsIndex();
            

            // 🔽 Enviar info para a View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;
            ViewBag.Concelho = concelho;
            ViewBag.RegimeTrabalho = regimeTrabalho;
            ViewBag.Favoritos = favoritos;
            ViewBag.ConcelhosList = listas.SelectListConcelhos;
            ViewBag.JornadasList = listas.SelectListJornada;
            ViewBag.RegimeTrabalhoList = listas.SelectListRegimeTrabalho;


            return View(ofertaList);
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

            LoadListsOfertaEmprego(model);

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

            LoadListsOfertaEmprego(oferta);
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

        private async void LoadListsOfertaEmprego(OfertaEmprego model)
        {
            using (var httpClient = new HttpClient())
            {
                //Buscar concelhos na API
                var apiCallConcelhos = await httpClient.GetAsync("https://localhost:7211/api/LookUps/Concelhos");

                if (!apiCallConcelhos.IsSuccessStatusCode)
                    return;

                string responseConcelhos = await apiCallConcelhos.Content.ReadAsStringAsync();
                var listaConcelhos = JsonConvert.DeserializeObject<List<Concelho>>(responseConcelhos);


                //Criar lista para preencher o modelo da View
                if (listaConcelhos != null && listaConcelhos.Any())
                {
                    model.SelectListConcelhos = listaConcelhos.Select(c => new SelectListItem
                    {
                        Value = c.IdConcelho.ToString(),
                        Text = c.NomeConcelho.ToString()
                    }).ToList();
                }

                //Buscar contratos na API
                var apiCallContratos = await httpClient.GetAsync("https://localhost:7211/api/LookUps/TiposContratos");

                if (!apiCallContratos.IsSuccessStatusCode)
                    return;

                string responseContratos = await apiCallContratos.Content.ReadAsStringAsync();
                var listaContratos = JsonConvert.DeserializeObject<List<TipoContrato>>(responseContratos);


                //Criar lista para preencher o modelo da View
                if (listaContratos != null && listaContratos.Any())
                {
                    model.SelectListTiposContratos = listaContratos.Select(c => new SelectListItem
                    {
                        Value = c.IdTipoContrato.ToString(),
                        Text = c.Tipo.ToString()
                    }).ToList();
                }
            }
        }

            private async Task<IndexListsDTO> LoadListsIndex()
            {
                var indexListsDTO = new IndexListsDTO();

                using (var httpClient = new HttpClient())
                {
                    //Buscar concelhos na API
                    var apiCallConcelhos = await httpClient.GetAsync("https://localhost:7211/api/LookUps/Concelhos");

                if (!apiCallConcelhos.IsSuccessStatusCode)
                    return indexListsDTO;

                    string responseConcelhos = await apiCallConcelhos.Content.ReadAsStringAsync();
                    var listaConcelhos = JsonConvert.DeserializeObject<List<Concelho>>(responseConcelhos);


                    var selectConcelhosList = new List<SelectListItem>();

                    //Criar lista para preencher o modelo da View
                    if (listaConcelhos != null && listaConcelhos.Any())
                    {
                        selectConcelhosList = listaConcelhos.Select(c => new SelectListItem
                        {
                            Value = c.IdConcelho.ToString(),
                            Text = c.NomeConcelho.ToString()
                        }).ToList();
                    }

                    indexListsDTO.SelectListConcelhos = selectConcelhosList;
;
                    var selectRegimeList = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "True", Text = "Presencial" },
                        new SelectListItem { Value = "False", Text = "Remoto" },
                        new SelectListItem { Value = "", Text = "Híbrido" }
                    };

                    indexListsDTO.SelectListRegimeTrabalho = selectRegimeList;

                    var selectJornadaList = new List<SelectListItem>
                        {
                            new SelectListItem { Value = "True", Text = "Full time" },
                            new SelectListItem { Value = "False", Text = "Part time" },
                            new SelectListItem { Value = "", Text = "Flexível" }
                        };

                    indexListsDTO.SelectListJornada = selectJornadaList;
                }

                return indexListsDTO;   
            }

    }
}
