using Microsoft.AspNetCore.Mvc;
using OtManager.Web.Models;
using OtManager.Web.Models.ViewModels;

namespace OtManager.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (IsLoggedIn())
        {
            return RedirectToAction(nameof(WorkOrdersController.Index), "WorkOrders");
        }

        ViewData["ReturnUrl"] = returnUrl;
        var model = new LoginViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        HttpContext.Session.SetString(SessionKeys.IsLoggedIn, "true");
        HttpContext.Session.SetString(SessionKeys.Username, model.Username);

        TempData["ToastMessage"] = $"Bienvenido {model.Username}!";
        TempData["ToastVariant"] = "success";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(WorkOrdersController.Index), "WorkOrders");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _logger.LogInformation("Usuario {User} cerró sesión", HttpContext.Session.GetString(SessionKeys.Username));
        HttpContext.Session.Clear();
        TempData["ToastMessage"] = "Sesión finalizada";
        TempData["ToastVariant"] = "info";
        return RedirectToAction(nameof(Login));
    }

    private bool IsLoggedIn() => HttpContext.Session.GetString(SessionKeys.IsLoggedIn) == "true";
}
