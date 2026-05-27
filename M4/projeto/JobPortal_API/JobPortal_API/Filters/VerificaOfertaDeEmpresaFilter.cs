using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using JobPortal_API.Data;
using System.Threading.Tasks;

namespace JobPortal_API.Filters
{
    public class VerificaOfertaDeEmpresaFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _context;
        public VerificaOfertaDeEmpresaFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Se for Admin, já libera
            if (user.IsInRole("Admin"))
                return;

            // Pega o IdEmpresa do token
            var idEmpresaDoToken = user.FindFirst("IdEmpresa")?.Value;
            if (string.IsNullOrEmpty(idEmpresaDoToken))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Pega o IdOferta da rota
            if (!context.RouteData.Values.TryGetValue("id", out var rawId)
                || !int.TryParse(rawId.ToString(), out var idOferta))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Busca a oferta e compara o proprietário
            var oferta = await _context.OfertaEmprego
                                       .FindAsync(idOferta);
            if (oferta == null || oferta.IdEmpresa.ToString() != idEmpresaDoToken)
            {
                context.Result = new ForbidResult();
                return;
            }
            // passa pelo filtro
        }
    }
}

