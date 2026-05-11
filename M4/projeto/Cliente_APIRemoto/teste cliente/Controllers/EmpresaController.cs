using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;

namespace teste_cliente.Controllers
{
    public class EmpresaController : Controller
    {
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

                using (var response = await httpClient.GetAsync("https://localhost:7211/api/empresa/BuscarTodas"))
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

                using (var response = await httpClient.GetAsync("https://localhost:7211/api/empresa/" + id))
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
                    empresa = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                }
            }
            return View(empresa);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Create()
        {
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
                    using (var response = await httpClient.PostAsync("https://localhost:7211/api/empresa/", content))
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

                using (var response = await httpClient.GetAsync("https://localhost:7211/api/empresa/" + id))
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
                    empresa = JsonConvert.DeserializeObject<Empresa>(apiResponse);

                }
                return View(empresa);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Models.Empresa empresa)
        {
            Empresa e = new Empresa();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(empresa), Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = await httpClient.PutAsync("https://localhost:7211/api/empresa/EditarEmpresa/" + empresa.IdEmpresa, content))
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
                    e = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                }
                return RedirectToAction("Details", new { id = empresa.IdEmpresa });


            }

            return View(e);
        }

 
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {


            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            // 1. Carregar a empresa
            Empresa empresa = null;
            List<Review> reviews = new List<Review>();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respEmp = await httpClient.GetAsync($"https://localhost:7211/api/empresa/BuscarPorId/{id}");
                if (respEmp.IsSuccessStatusCode)
                {
                    var jsonEmp = await respEmp.Content.ReadAsStringAsync();
                    empresa = JsonConvert.DeserializeObject<Empresa>(jsonEmp);
                }
                else
                {
                    if (respEmp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                        return Forbid();
                    }
                    if (!respEmp.IsSuccessStatusCode)
                    {
                        return NotFound();
                    }
                }

                if (empresa == null)
                    return RedirectToAction("Index", "Home");

                // 2. Carregar as reviews da API
                var respRev = await httpClient.GetAsync($"https://localhost:7211/api/review/empresa/{id}");
                if (respRev.IsSuccessStatusCode)
                {
                    var jsonRev = await respRev.Content.ReadAsStringAsync();
                    reviews = JsonConvert.DeserializeObject<List<Review>>(jsonRev);
                }
            }

            // 3. Passar para a View
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
                using (var response = await httpClient.DeleteAsync("https://localhost:7211/api/empresa/DeletarEmpresa/" + id))
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
            
            // Verifica a role do usuário logado
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
                return RedirectToAction("Login", "Auth"); // fallback seguro
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
            httpClient.DefaultRequestHeaders.Authorization =
                 new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                "https://localhost:7211/api/empresa/ChangePassword/" + id, content);

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
    }
}
