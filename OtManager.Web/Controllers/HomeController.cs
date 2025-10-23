using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OtManager.Web.Models;

namespace OtManager.Web.Controllers;

public sealed class HomeController : Controller
{
    /// <summary>
    /// Renderiza la vista de error estándar.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
