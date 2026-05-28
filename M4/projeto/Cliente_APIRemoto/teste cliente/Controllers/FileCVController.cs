using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using teste_cliente.Models;

namespace teste_cliente.Controllers
{
    public class FileCVController : Controller
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        public FileCVController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
        }

        [HttpGet]
        public async Task<IActionResult> Download(int idCandidato)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            try
            {
                Console.WriteLine($"Tentando baixar currículo para idCandidato: {idCandidato}");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(_baseUrl + $"filecv/por-candidato/{idCandidato}");

                Console.WriteLine($"Resposta da API: Status {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"JSON retornado: {jsonResponse}");

                    var fileCV = JsonConvert.DeserializeObject<FileCV>(jsonResponse);
                    if (fileCV?.File == null || fileCV.File.Length == 0)
                    {
                        Console.WriteLine("Currículo retornado é nulo ou vazio.");
                        return Json(new { success = false, message = "Nenhum currículo encontrado para este candidato." });
                    }

                    Console.WriteLine($"Currículo encontrado para idCandidato {idCandidato}, tamanho: {fileCV.File.Length} bytes");
                    return File(fileCV.File, "application/pdf", $"Curriculo_Candidato_{idCandidato}.pdf");
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                        return Forbid();
                    }
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Erro na API: {response.StatusCode}, Resposta: {errorResponse}");
                    return Json(new { success = false, message = "Nenhum currículo encontrado para este candidato." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar currículo para idCandidato {idCandidato}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Ocorreu um erro ao tentar baixar o currículo." });
            }
        }        
    }
}
