using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;

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

                using (var response = await httpClient.GetAsync(_baseUrl + "Logo"))
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
                using (var response = await httpClient.PostAsync(_baseUrl + "Logo/", content))
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
                            // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                            return Forbid();
                        }
                        // Log ou tratamento de erro
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError("", $"API Error: {errorResponse}");
                        return View(logo);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        // Ação proxy para criar/atualizar o logo
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateLogo()
        {
            // Obtém o valor do campo oculto "IdEmpresaFoto" do formulário
            var idEmpresaStr = Request.Form["IdEmpresaFoto"].ToString();
            if (string.IsNullOrEmpty(idEmpresaStr) || !int.TryParse(idEmpresaStr, out int idEmpresa))
            {
                ModelState.AddModelError("", "Id da Empresa inválido.");
                // Como não temos um ID válido, redireciona para o Index da Empresa (ou outra página de erro)
                return RedirectToAction("Index", "Empresa");
            }

            // Verifica se foi enviado um ficheiro
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

            // Prepara o conteúdo multipart/form-data para enviar para a API
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var formContent = new MultipartFormDataContent())
                {
                    formContent.Add(new StringContent(idEmpresa.ToString()), "IdEmpresaFoto");
                    var fileContent = new StreamContent(new MemoryStream(logoBytes));
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    formContent.Add(fileContent, "file", file.FileName);

                    var apiResponse = await httpClient.PostAsync(_baseUrl + "logo/update", formContent);
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        if (apiResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                            return Forbid();
                        }
                        
                        var errorMessage = await apiResponse.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Erro ao atualizar o logo: {errorMessage}";
                        return RedirectToAction("Details", "Empresa", new { id = idEmpresa });
                    }
                }
            }
            TempData["SuccessMessage"] = "Logo atualizado com sucesso.";
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
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var apiResponse = await httpClient.GetAsync(
                _baseUrl + $"logo/empresa/{id}");

            if (apiResponse.StatusCode == HttpStatusCode.Forbidden)
                return Forbid();

            // Se a API devolveu diretamente o arquivo binário (image/jpeg)
            if (apiResponse.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true)
            {
                var imageBytes = await apiResponse.Content.ReadAsByteArrayAsync();
                return File(imageBytes, apiResponse.Content.Headers.ContentType.MediaType);
            }

            // Se a API devolveu JSON (caso não exista logo), vamos retornar a imagem padrão
            // (ou poderíamos detectar 404 acima e saltar direto para o default)
            var defaultPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "img", "icone_perfil.png");
            var defaultBytes = System.IO.File.ReadAllBytes(defaultPath);
            return File(defaultBytes, "image/png");
        }

    }
}
