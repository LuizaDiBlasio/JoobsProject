using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net;
using System.Net.Http.Headers;
using teste_cliente.Models;
using Microsoft.AspNetCore.Authorization;

namespace teste_cliente.Controllers
{
    public class LogoController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _baseUrl;

        public LogoController(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
        }

        public async Task<IActionResult> Index()
        {
            List<LogoEmpresa> logoList = new List<LogoEmpresa>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // CORREÇÃO: Adicionado prefixo "api/" e corrigido para minúsculas conforme o Swagger
                using (var response = await httpClient.GetAsync(_baseUrl + "api/logo"))
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
                    logoList = JsonConvert.DeserializeObject<List<LogoEmpresa>>(apiResponse);
                }
            }

            return View(logoList);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(LogoEmpresa logo)
        {
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file != null && file.Length > 0)
                {
                    using (var dataStream = new MemoryStream())
                    {
                        await file.CopyToAsync(dataStream);
                        logo.Logo = dataStream.ToArray();
                    }
                }
            }

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                StringContent content = new StringContent(JsonConvert.SerializeObject(logo), Encoding.UTF8, "application/json");

                // CORREÇÃO: Alinhado para o POST "api/logo" sem barras duplas residuais no final
                using (var response = await httpClient.PostAsync(_baseUrl + "api/logo", content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        logo = JsonConvert.DeserializeObject<LogoEmpresa>(apiResponse);
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            return Forbid();
                        }
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"API Error: {errorResponse}");
                        return View(logo);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateLogo()
        {
            var idEmpresaStr = Request.Form["IdEmpresaFoto"].ToString();
            if (string.IsNullOrEmpty(idEmpresaStr) || !int.TryParse(idEmpresaStr, out int idEmpresa))
            {
                ModelState.AddModelError("", "Id da Empresa inválido.");
                return RedirectToAction("Index", "Empresa");
            }

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Nenhum ficheiro enviado.");
                return RedirectToAction("Details", "Empresa", new { id = idEmpresa });
            }

            byte[] logoBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                logoBytes = memoryStream.ToArray();
            }

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var formContent = new MultipartFormDataContent())
                {
                    formContent.Add(new StringContent(idEmpresa.ToString()), "IdEmpresaFoto");
                    var fileContent = new StreamContent(new MemoryStream(logoBytes));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    formContent.Add(fileContent, "file", file.FileName);

                    // CORREÇÃO: Atualizado com "api/" e ajustado o "Update" com "U" maiúsculo conforme exige o Swagger
                    var apiResponse = await httpClient.PostAsync(_baseUrl + "api/logo/Update", formContent);
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        if (apiResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            return Forbid();
                        }

                        var errorMessage = await apiResponse.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Erro ao atualizar o logo: {errorMessage}";
                        return RedirectToAction("Details", "Empresa", new { id = idEmpresa });
                    }
                }
            }
            TempData["SuccessMessage"] = "Logo updated successfully.";
            return RedirectToAction("Details", "Empresa", new { id = idEmpresa });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetLogoImage(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return Forbid();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // CORREÇÃO: Adicionado o prefixo "api/" para bater com o GET do Swagger (/api/logo/empresa/{idEmpresa})
            var apiResponse = await httpClient.GetAsync(_baseUrl + $"api/logo/empresa/{id}");

            if (apiResponse.StatusCode == HttpStatusCode.Forbidden)
                return Forbid();

            if (apiResponse.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true)
            {
                var imageBytes = await apiResponse.Content.ReadAsByteArrayAsync();
                return File(imageBytes, apiResponse.Content.Headers.ContentType.MediaType);
            }

            var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "icone_perfil.png");
            var defaultBytes = System.IO.File.ReadAllBytes(defaultPath);
            return File(defaultBytes, "image/png");
        }
    }
}