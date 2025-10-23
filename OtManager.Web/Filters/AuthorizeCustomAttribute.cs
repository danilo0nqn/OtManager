using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OtManager.Web.Models;

namespace OtManager.Web.Filters;

/// <summary>
/// Filtro sencillo que valida la existencia de un token JWT en sesión y redirige al login si no existe.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AuthorizeCustomAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Verifica la sesión del usuario antes de ejecutar la acción; si no hay token se redirige al formulario de login.
    /// </summary>
    /// <param name="context">Contexto de ejecución de la acción.</param>
    /// <param name="next">Delegado que representa la acción siguiente en la canalización.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        var httpContext = context.HttpContext;
        var token = httpContext.Session.GetString(SessionKeys.AuthToken);

        if (!string.IsNullOrWhiteSpace(token))
        {
            await next().ConfigureAwait(false);
            return;
        }

        var returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;
        context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
    }
}
