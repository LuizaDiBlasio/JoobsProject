using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Text;
using teste_cliente.Models;
using Microsoft.AspNetCore.Authorization;

namespace teste_cliente.Controllers
{
    public class FotoController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _baseUrl;

        public FotoController(IConfiguration config)
        {
            _config = config;
            _baseUrl = _config["ApiSettings:BaseUrl"];
        }
        public async Task<IActionResult> Index()
        {
            List<Foto> fotoList = new List<Foto>();

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using (var response = await httpClient.GetAsync(_baseUrl + "foto/TodasFotos"))
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
                    fotoList = JsonConvert.DeserializeObject<List<Foto>>(apiResponse);
                }
            }

            return View(fotoList);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate(Foto foto, IFormFile file)
        {
            try
            {
                // Verificar se há um arquivo enviado
                if (Request.Form.Files.Count > 0)
                {
                    file = Request.Form.Files.FirstOrDefault();
                    using var dataStream = new MemoryStream();
                    await file.CopyToAsync(dataStream);
                    foto.FotoPerfil = dataStream.ToArray();
                }
                else
                {
                    Console.WriteLine("Nenhum arquivo enviado para upload.");
                    TempData["ErrorMessage"] = "Nenhum arquivo selecionado.";
                    return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
                }

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Auth");

                using var httpClient = new HttpClient();

                Console.WriteLine($"Buscando foto para IdCandidatoFoto: {foto.IdCandidatoFoto}");

                // Tenta buscar a foto atual do candidato

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                // Tenta buscar a foto atual do candidato
                var checkResponse = await httpClient.GetAsync(_baseUrl + $"foto/ByCandidato/{foto.IdCandidatoFoto}");

                if (checkResponse.IsSuccessStatusCode)
                {
                    var existingPhotoJson = await checkResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Resposta da API para foto existente: {existingPhotoJson}");

                    try
                    {
                        var existingPhoto = JsonConvert.DeserializeObject<Foto>(existingPhotoJson);
                        if (existingPhoto == null)
                        {
                            Console.WriteLine("Foto existente retornada como nula após deserialização.");
                            TempData["ErrorMessage"] = "Erro ao processar a foto existente.";
                            return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
                        }

                        // Atualiza a foto
                        foto.Id = existingPhoto.Id;
                        var contentPut = new StringContent(JsonConvert.SerializeObject(foto), Encoding.UTF8, "application/json");
                        var putResponse = await httpClient.PutAsync(_baseUrl + $"Foto/{foto.Id}", contentPut);

                        if (!putResponse.IsSuccessStatusCode)
                        {
                            var putError = await putResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"Erro ao atualizar foto: {putResponse.StatusCode}, {putError}");
                            TempData["ErrorMessage"] = "Erro ao atualizar a foto.";
                            return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
                        }

                        Console.WriteLine($"Foto atualizada com sucesso para Id: {foto.Id}");
                    }
                    catch (JsonReaderException ex)
                    {
                        Console.WriteLine($"Erro ao parsear JSON da foto existente: {ex.Message}");
                        TempData["ErrorMessage"] = "Erro ao processar a resposta da API.";
                        return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
                    }
                }
                else
                {
                    var errorResponse = await checkResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Foto não encontrada para IdCandidatoFoto: {foto.IdCandidatoFoto}, Status: {checkResponse.StatusCode}, Resposta: {errorResponse}");

                    // Criar nova foto com POST
                    var contentPost = new StringContent(JsonConvert.SerializeObject(foto), Encoding.UTF8, "application/json");
                    var postResponse = await httpClient.PostAsync(_baseUrl + "Foto/CriarFoto", contentPost);

                    if (!postResponse.IsSuccessStatusCode)
                    {
                        var postError = await postResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"Erro ao criar nova foto: {postResponse.StatusCode}, {postError}");
                        TempData["ErrorMessage"] = "Erro ao criar a nova foto.";
                        return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
                    }

                    Console.WriteLine($"Nova foto criada com sucesso para IdCandidatoFoto: {foto.IdCandidatoFoto}");
                }

                TempData["SuccessMessage"] = "Foto atualizada com sucesso!";
                return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado em CreateOrUpdate: {ex.Message}");
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar a foto.";
                return RedirectToAction("Details", "Candidato", new { id = foto.IdCandidatoFoto });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Bearer", token);

            var apiResponse = await httpClient.GetAsync(
                _baseUrl + $"foto/BuscarFotoPorIdCandidato/{id}");
            if (!apiResponse.IsSuccessStatusCode)
            {
                // devolve um ficheiro estático default  
                var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/icone_perfil.png");
                var bytes_alternativo = await System.IO.File.ReadAllBytesAsync(defaultPath);
                return File(bytes_alternativo, "image/png");
            }
            Console.WriteLine($"[GetImage] Foto encontrada para id={id}. Retornando imagem!");
            var bytes = await apiResponse.Content.ReadAsByteArrayAsync();
            return File(bytes, "image/jpeg");
        }


    }
}
