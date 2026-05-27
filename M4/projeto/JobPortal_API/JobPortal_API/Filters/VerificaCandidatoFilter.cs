using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JobPortal_API.Filters
{
    public class VerificaCandidatoFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var idDoToken = user.FindFirst("IdCandidato")?.Value;
            var idDaUrl = context.RouteData.Values["id"]?.ToString();

            var temPermissaoAdmin = user.IsInRole("Admin");

            if (temPermissaoAdmin)
                return;

            if (idDoToken == null || idDaUrl == null || idDoToken != idDaUrl)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
