using Microsoft.AspNetCore.Mvc;
using OtManager.Web.Models;
using OtManager.Web.Models.ViewModels;
using OtManager.Web.Services;

namespace OtManager.Web.Controllers;

public sealed class WorkOrdersController : Controller
{
    private readonly WorkOrderService _service;
    private readonly ILogger<WorkOrdersController> _logger;

    public WorkOrdersController(WorkOrderService service, ILogger<WorkOrdersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index([FromQuery] WorkOrderFiltersViewModel? filters)
    {
        if (!EnsureAuthenticated(out var redirect))
        {
            return redirect!;
        }

        filters ??= new WorkOrderFiltersViewModel();

        var orders = _service.ApplyFilters(filters);
        var viewModel = new WorkOrdersListViewModel
        {
            Filters = filters,
            Orders = orders,
            Clientes = _service.GetClientes(),
            Sistemas = _service.GetSistemas(),
            Estados = _service.GetEstados(),
            CurrentUser = _service.CurrentUser,
        };

        ViewData["ActiveView"] = "list";
        TransferToastFromTempData();
        PopulateLayout();
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Details(int? id, string? searchNumber, bool edit = false)
    {
        if (!EnsureAuthenticated(out var redirect))
        {
            return redirect!;
        }

        OrdenTrabajo? order = null;
        string? search = searchNumber;

        if (!string.IsNullOrWhiteSpace(searchNumber))
        {
            if (int.TryParse(searchNumber, out var number))
            {
                order = _service.GetWorkOrder(number);
            }
            else
            {
                TempData["ToastMessage"] = "Ingrese un número de orden válido";
                TempData["ToastVariant"] = "error";
            }
        }

        if (order is null && id.HasValue)
        {
            order = _service.GetWorkOrder(id.Value);
        }

        if (order is not null && string.IsNullOrWhiteSpace(search))
        {
            search = order.Numero.ToString();
        }

        var viewModel = new WorkOrderDetailViewModel
        {
            Order = order,
            Clientes = _service.GetClientes(),
            Sistemas = _service.GetSistemas(),
            Estados = _service.GetEstados(),
            Usuarios = _service.GetUsuarios(),
            Avances = order is not null ? _service.GetAvances(order.Numero) : Array.Empty<AvanceTrabajo>(),
            Historial = order is not null ? _service.GetHistorial(order.Numero) : Array.Empty<HistorialEstado>(),
            Archivos = order is not null ? _service.GetArchivos(order.Numero) : Array.Empty<ArchivoAdjunto>(),
            Puestas = order is not null ? _service.GetPuestas(order.Numero) : Array.Empty<PuestaProduccion>(),
            SearchNumber = search,
            IsEditing = edit,
            CurrentUser = _service.CurrentUser,
        };

        if (TempData.ContainsKey("ToastMessage"))
        {
            ViewData["ToastMessage"] = TempData["ToastMessage"];
            ViewData["ToastVariant"] = TempData["ToastVariant"] ?? "info";
        }

        ViewData["ActiveView"] = "detail";
        PopulateLayout();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(WorkOrderUpdateInputModel model)
    {
        if (!EnsureAuthenticated(out var redirect))
        {
            return redirect!;
        }

        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "Revise los datos ingresados";
            TempData["ToastVariant"] = "error";
            return RedirectToAction(nameof(Details), new { id = model.Numero, edit = true });
        }

        TempData["ToastMessage"] = "Orden de trabajo actualizada (solo demostración)";
        TempData["ToastVariant"] = "success";
        _logger.LogInformation("Orden {Numero} actualizada por {Usuario} (demo)", model.Numero, HttpContext.Session.GetString(SessionKeys.Username));

        return RedirectToAction(nameof(Details), new { id = model.Numero });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TriggerDemoToast(int id, string type)
    {
        if (!EnsureAuthenticated(out var redirect))
        {
            return redirect!;
        }

        TempData["ToastMessage"] = type switch
        {
            "avance" => "Avance registrado (solo demostración)",
            "delete" => "Elemento eliminado (solo demostración)",
            _ => "Acción ejecutada"
        };
        TempData["ToastVariant"] = type == "delete" ? "warning" : "success";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult History()
    {
        if (!EnsureAuthenticated(out var redirect))
        {
            return redirect!;
        }

        var model = new RecentHistoryViewModel
        {
            RecentOrders = _service.GetRecentWorkOrders(),
            CurrentUser = _service.CurrentUser,
        };

        ViewData["ActiveView"] = "history";
        TransferToastFromTempData();
        PopulateLayout();
        return View(model);
    }

    private bool EnsureAuthenticated(out IActionResult? redirect)
    {
        if (HttpContext.Session.GetString(SessionKeys.IsLoggedIn) == "true")
        {
            redirect = null;
            return true;
        }

        var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
        redirect = RedirectToAction(nameof(AccountController.Login), "Account", new { returnUrl });
        return false;
    }

    private void PopulateLayout()
    {
        ViewBag.CurrentUser = _service.CurrentUser;
    }

    private void TransferToastFromTempData()
    {
        if (TempData.ContainsKey("ToastMessage"))
        {
            ViewData["ToastMessage"] = TempData["ToastMessage"];
            ViewData["ToastVariant"] = TempData["ToastVariant"] ?? "info";
        }
    }
}
