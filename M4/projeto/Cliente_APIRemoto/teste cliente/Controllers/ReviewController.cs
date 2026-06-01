using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using teste_cliente.Models;
using Vereyon.Web;

namespace teste_cliente.Controllers
{
    public class ReviewController : Controller
    {
        private readonly string apiBaseUrl;
        private readonly IConfiguration _config;
        private readonly IFlashMessage _flashMessage;

        public ReviewController(IConfiguration config)
        {
            _config = config;
            apiBaseUrl = _config["ApiSettings:BaseUrl"];

        }

        public async Task<IActionResult> Index(int? empresaId)
        {
            List<Review> reviewList = new List<Review>();
            Empresa empresa = null;
            string logoBase64 = null;

            if (empresaId.HasValue)
            {
                using (var httpClient = new HttpClient())
                {
                    // Obter reviews da empresa
                    using (var response = await httpClient.GetAsync($"{apiBaseUrl}review/empresa/{empresaId}"))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            ModelState.AddModelError(string.Empty, "Empresa não encontrada ou não possui reviews.");
                        }
                        else
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            reviewList = JsonConvert.DeserializeObject<List<Review>>(apiResponse);
                        }
                    }

                    // Obter dados da empresa
                    using (var response = await httpClient.GetAsync($"{apiBaseUrl}empresa/public/BuscarPorId/{empresaId}"))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            ModelState.AddModelError(string.Empty, "Erro ao buscar os dados da empresa.");
                        }
                        else
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            empresa = JsonConvert.DeserializeObject<Empresa>(apiResponse);
                        }
                    }

                    // Obter a logo da empresa
                    if (empresaId.HasValue)
                    {
                        var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
                        if (string.IsNullOrEmpty(token))
                            return RedirectToAction("Login", "Auth");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        using (var response = await httpClient.GetAsync($"{apiBaseUrl}logo/{empresaId}"))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                string apiResponse = await response.Content.ReadAsStringAsync();
                                var logoData = JsonConvert.DeserializeObject<LogoEmpresa>(apiResponse);
                                if (logoData?.Logo != null)
                                {
                                    logoBase64 = Convert.ToBase64String(logoData.Logo);
                                }
                            }
                            else
                            {
                                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                                {
                                    // retorna 403 ao browser ou redireciona para uma página de AccessDenied
                                    return Forbid();
                                }
                            }
                        }
                    }
                }
            }

            ViewBag.Empresa = empresa;
            ViewBag.LogoBase64 = logoBase64;
            return View(reviewList);
        }        

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([Bind("IdReview,IdEmpresa,Titulo,Descricao,Rating,DataCriacao,NomeUsuario")] Review review)
        {
            var idCandidato = User.Claims.First(c => c.Type == "IdCandidato").Value;
            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");
            // Log dos valores recebidos
            Console.WriteLine("Valores recebidos: IdEmpresa={0}, Rating={1}, Titulo={2}, Descricao={3}, NomeUsuario={4}, DataCriacao={5}",
                review.IdEmpresa, review.Rating, review.Titulo, review.Descricao, review.NomeUsuario, review.DataCriacao);

            if (ModelState.IsValid)
            {
                review.DataCriacao = DateTime.Now;

                // Buscar o nome do candidato com base no email
                string email = User.Identity.Name ?? "Usuário Anônimo";
                string nomeCandidato = email;

                

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    try
                    {
                        var response = await httpClient.GetAsync($"{apiBaseUrl}candidato/BuscarPorId/{idCandidato}");
                        if (response.IsSuccessStatusCode)
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            var candidato = JsonConvert.DeserializeObject<Candidato>(apiResponse);
                            nomeCandidato = candidato?.Nome;
                        }
                        else
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
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro ao buscar candidato: " + ex.Message);
                    }
                }

                review.NomeUsuario = nomeCandidato;

                Console.WriteLine("Valores enviados para a API: IdEmpresa={0}, Rating={1}, Titulo={2}, Descricao={3}, NomeUsuario={4}",
                    review.IdEmpresa, review.Rating, review.Titulo, review.Descricao, review.NomeUsuario);

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    StringContent content = new StringContent(
                        JsonConvert.SerializeObject(review),
                        Encoding.UTF8,
                        "application/json");

                    using (var response = await httpClient.PostAsync($"{apiBaseUrl}review", content))
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Resposta da API: Status = " + response.StatusCode + ", Corpo = " + responseContent);

                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("Index", new { empresaId = review.IdEmpresa });
                        }
                        else
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
                            
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("ModelState inválido. Erros: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Erro desconhecido ao criar o review.";
            return RedirectToAction("Index", "OfertaEmprego");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int idReview, int idEmpresa)
        {

            var token = User.Claims.FirstOrDefault(c => c.Type == "JWToken")?.Value;
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var cookieHeader = HttpContext.Request.Headers["Cookie"].ToString();
                System.Diagnostics.Debug.WriteLine("Cookie Header: " + cookieHeader);
                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                }

                var response = await httpClient.DeleteAsync(apiBaseUrl + "review/" + idReview);
                string apiResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Forbid();
                    }
                    _flashMessage.Danger("Não foi possível apagar review.");
                    return RedirectToAction("Details","Empresa", new { id = idEmpresa });
                }
            }
 
             return RedirectToAction("Details", "Empresa", new { id = idEmpresa });
        }
    }
}

