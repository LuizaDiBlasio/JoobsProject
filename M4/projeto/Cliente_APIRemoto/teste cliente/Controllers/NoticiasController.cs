using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using teste_cliente.Models;
using System.Web;

namespace teste_cliente.Controllers
{
    public class NoticiasController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NoticiasController(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = "15e3d873f4664f4aa3516d0fc170797d"; // Sua API Key
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "teste_cliente");
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Define a consulta para obter notícias relacionadas a empregos em português
                // Usando HttpUtility.UrlEncode para garantir que os caracteres especiais são codificados corretamente
                string query = HttpUtility.UrlEncode("emprego OR trabalho OR carreira");
                string sortBy = "publishedAt";

                // Simplificando os parâmetros da consulta para reduzir chances de erro
                //string apiUrl = $"https://newsapi.org/v2/everything?q={query}&sortBy={sortBy}&language=pt&pageSize=10";
                // Filtrar notícias apenas de Portugal
                // Usando o endpoint everything com domínios específicos de Portugal
                string apiUrl = $"https://newsapi.org/v2/everything?q={query} portugal&language=pt&domains=publico.pt,dn.pt,jn.pt,expresso.pt,observador.pt,rtp.pt&sortBy={sortBy}&pageSize=10";

                // Adicionar o header de autenticação em vez de passar na URL
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Faz a requisição para a API
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // Para fins de diagnóstico
                ViewBag.StatusCode = (int)response.StatusCode;
                ViewBag.ReasonPhrase = response.ReasonPhrase;

                if (response.IsSuccessStatusCode)
                {
                    // Lê o conteúdo da resposta
                    string content = await response.Content.ReadAsStringAsync();

                    // Para fins de diagnóstico, armazenamos os primeiros caracteres da resposta
                    ViewBag.ResponsePreview = content.Length > 100 ? content.Substring(0, 100) : content;

                    // Tentamos deserializar a resposta
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var newsApiResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, options);

                    if (newsApiResponse?.Articles != null)
                    {
                        return View(newsApiResponse.Articles);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Não foi possível processar os dados da API.";
                        return View(new List<Noticia>());
                    }
                }
                else
                {
                    // Caso a requisição falhe - adicionado detalhes do erro
                    string errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.ErrorMessage = $"Erro ao buscar notícias: {response.StatusCode} - {response.ReasonPhrase}";
                    ViewBag.ErrorDetails = errorContent;

                    // Plano B: Exibir notícias fictícias para fins de demonstração
                    var noticiasDemo = CriarNoticiasDemonstracao();
                    ViewBag.UsingDemoData = true;
                    return View(noticiasDemo);
                }
            }
            catch (Exception ex)
            {
                // Em caso de exceção
                ViewBag.ErrorMessage = $"Ocorreu um erro: {ex.Message}";
                ViewBag.StackTrace = ex.StackTrace;

                // Plano B: Exibir notícias fictícias para fins de demonstração
                var noticiasDemo = CriarNoticiasDemonstracao();
                ViewBag.UsingDemoData = true;
                return View(noticiasDemo);
            }
        }

        // Método para criar notícias de demonstração quando a API falhar
        public List<Noticia> CriarNoticiasDemonstracao()
        {
            return new List<Noticia>
            {
                new Noticia
                {
                    Source = new Source { Id = "demo-1", Name = "JoQbs News" },
                    Author = "Equipe Editorial",
                    Title = "Mercado de TI em alta: empresas buscam profissionais qualificados",
                    Description = "O mercado de tecnologia está em constante crescimento com novas vagas surgindo diariamente em áreas como desenvolvimento web, cloud computing e inteligência artificial.",
                    Url = "#",
                    UrlToImage = "/img/ti-jobs.jpg", // Caminho ajustado para a pasta wwwroot/img
                    PublishedAt = DateTime.Now.AddDays(-1),
                    Content = "O setor de TI continua sendo um dos que mais contrata em Portugal, com salários acima da média do mercado."
                },
                new Noticia
                {
                    Source = new Source { Id = "demo-2", Name = "Portugal Empregos" },
                    Author = "Maria Silva",
                    Title = "Novas oportunidades no setor de hotelaria para o verão",
                    Description = "Com a aproximação da temporada turística, hotéis e restaurantes estão recrutando pessoal para diversas posições.",
                    Url = "#",
                    UrlToImage = "/img/hotel-jobs.jpg", // Caminho ajustado para a pasta wwwroot/img
                    PublishedAt = DateTime.Now.AddDays(-2),
                    Content = "O turismo está recuperando rapidamente e a demanda por profissionais qualificados está maior que nunca."
                },
                new Noticia
                {
                    Source = new Source { Id = "demo-3", Name = "Carreira & Negócios" },
                    Author = "João Almeida",
                    Title = "Governo lança programa de incentivo ao primeiro emprego",
                    Description = "Novo programa disponibiliza subsídios para empresas que contratarem jovens em busca do primeiro emprego.",
                    Url = "#",
                    UrlToImage = "/img/first-job.jpg", // Caminho ajustado para a pasta wwwroot/img
                    PublishedAt = DateTime.Now.AddDays(-3),
                    Content = "A iniciativa visa reduzir o desemprego entre jovens recém-formados e estimular a economia."
                },
                new Noticia
                {
                    Source = new Source { Id = "demo-4", Name = "Tech Hoje" },
                    Author = "Carlos Mendes",
                    Title = "Startups portuguesas abrem 500 novas vagas em tecnologia",
                    Description = "Empresas emergentes do ecossistema tecnológico português estão em fase de expansão e buscam talentos em diversas áreas.",
                    Url = "#",
                    UrlToImage = "/img/startup-jobs.jpg", // Caminho ajustado para a pasta wwwroot/img
                    PublishedAt = DateTime.Now.AddDays(-4),
                    Content = "As vagas são para desenvolvedores, especialistas em marketing digital, designers e profissionais de produto."
                },
                new Noticia
                {
                    Source = new Source { Id = "demo-5", Name = "Economia Digital" },
                    Author = "Ana Costa",
                    Title = "Trabalho remoto: empresas adaptam-se ao novo modelo de trabalho",
                    Description = "Grandes empresas anunciam modelos híbridos permanentes e novas políticas de contratação que permitem trabalho 100% remoto.",
                    Url = "#",
                    UrlToImage = "/img/remote-work.jpg", // Caminho ajustado para a pasta wwwroot/img
                    PublishedAt = DateTime.Now.AddDays(-5),
                    Content = "A tendência do trabalho remoto veio para ficar, com benefícios tanto para empresas quanto para colaboradores."
                }
            };
        }


        // método para adicionar ultimas noticias na home através de injeção.
        public async Task<List<Noticia>> GetNoticiasAsync(int count)
        {
            try
            {
                string query = HttpUtility.UrlEncode("emprego OR trabalho OR carreira OR \"mercado de trabalho\" OR contratação OR vagas OR oportunidades OR recrutamento OR profissões OR salário OR desemprego");
                string domains = "publico.pt,dn.pt,jn.pt,expresso.pt,observador.pt,rtp.pt,sapo.pt,tsf.pt,cmjornal.pt,iol.pt";
                string fromDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                string sortBy = "publishedAt";

                string apiUrl = $"https://newsapi.org/v2/everything?q={query} portugal&language=pt&domains={domains}&from={fromDate}&sortBy={sortBy}&pageSize={count}";                

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var newsApiResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, options);

                    if (newsApiResponse?.Articles != null)
                    {
                        return newsApiResponse.Articles.Take(count).ToList();
                    }
                }

                // Se não conseguir obter notícias, retorne dados de demonstração
                return CriarNoticiasDemonstracao().Take(count).ToList();
            }
            catch
            {
                return CriarNoticiasDemonstracao().Take(count).ToList();
            }
        }
    }
}
