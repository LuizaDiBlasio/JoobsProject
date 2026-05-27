using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using System.Diagnostics;
using teste_cliente.Models;
using Vereyon.Web;

namespace teste_cliente.Controllers
{
    public class HomeController : Controller
    {
        private readonly NoticiasController _noticiasController;
        private readonly ILogger<HomeController> _logger;
        private readonly IFlashMessage _flashMessage; // Opcional, mas recomendado

        public HomeController(NoticiasController noticiasController, ILogger<HomeController> logger, IFlashMessage flashMessage)
        {
            _noticiasController = noticiasController;
            _logger = logger;
            _flashMessage = flashMessage;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obter notícias com segurança
                var noticiasResult = await _noticiasController.GetNoticiasAsync(3);

                var noticiasFinais = new List<Noticia>(noticiasResult ?? new List<Noticia>());

                if (noticiasFinais.Count < 3)
                {
                    var noticiasDemo = _noticiasController.CriarNoticiasDemonstracao();
                    int noticiasFaltando = 3 - noticiasFinais.Count;
                    noticiasFinais.AddRange(noticiasDemo.Take(noticiasFaltando));
                }

                ViewBag.Noticias = noticiasFinais.Take(3).ToList();
            }
            catch (Exception ex)
            {
                // Registamos o erro no Log, mas não paramos o site
                _logger.LogError(ex, "Falha ao carregar notícias na Home. Usando dados de demonstração.");

                // Avisamos o utilizador (se quiseres)
                _flashMessage.Warning("Não foi possível carregar as notícias mais recentes. A apresentar dados de exemplo.");

                // Fallback: Mostra apenas notícias de demonstração se a API falhar
                ViewBag.Noticias = _noticiasController.CriarNoticiasDemonstracao().Take(3).ToList();
            }

            return View();
        }

        // --- MÉTODOS DE TRATAMENTO DE ERROS / CATCHALL ---
        [Route("Home/Error/{statusCode?}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode)
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error,
                    "Erro global capturado em: {Path}", exceptionHandlerPathFeature.Path);
            }
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Faq()
        {
            return View();
        }
    }
}