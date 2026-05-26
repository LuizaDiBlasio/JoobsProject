using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal_API.Controllers
{
        [ApiController]
        [ApiExplorerSettings(IgnoreApi = true)] // Esconde do Swagger
        public class ErrorController : ControllerBase
        {
            [Route("/error")]
            public IActionResult HandleError([FromServices] IHostEnvironment hostEnvironment, [FromServices] ILogger<ErrorController> logger)
            {
                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
                var exception = context?.Error;

                if (exception != null)
                {
                    logger.LogError(exception, "Erro global apanhado na WebAPI.");
                }

                if (hostEnvironment.IsDevelopment())
                {
                    return Problem(
                        detail: exception?.StackTrace,
                        title: exception?.Message);
                }
                return Problem(
                    title: "Ocorreu um erro interno no servidor. A nossa equipa já foi notificada."
                );
            }
        }
}