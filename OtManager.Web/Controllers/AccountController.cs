using Microsoft.AspNetCore.Mvc;
using OtManager.Web.Models.ViewModels;
using OtManager.Web.Services;

namespace OtManager.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly AuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(AuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Renderiza el formulario de autenticación; si ya existe sesión activa se redirige a la bandeja principal.
    /// </summary>
    /// <param name="returnUrl">Ruta a la que se debe volver tras iniciar sesión.</param>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_authService.HasValidSession())
        {
            return RedirectToAction(nameof(WorkOrdersController.Index), "WorkOrders");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    /// <summary>
    /// Procesa las credenciales enviadas por el usuario autenticando contra la API externa.
    /// </summary>
    /// <param name="model">Modelo con usuario y contraseña.</param>
    /// <param name="returnUrl">Ruta original solicitada.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model.Username, model.Password, HttpContext.RequestAborted).ConfigureAwait(false);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "No fue posible iniciar sesión.");
            return View(model);
        }

        TempData["ToastMessage"] = $"Bienvenido {result.DisplayName ?? model.Username}!";
        TempData["ToastVariant"] = "success";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(WorkOrdersController.Index), "WorkOrders");
    }

    /// <summary>
    /// Cierra la sesión actual eliminando el token almacenado en servidor.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        var user = _authService.GetCurrentUser();
        _logger.LogInformation("Usuario {User} cerró sesión", user?.Id ?? "desconocido");
        _authService.Logout();
        TempData["ToastMessage"] = "Sesión finalizada";
        TempData["ToastVariant"] = "info";
        return RedirectToAction(nameof(Login));
    }
}
