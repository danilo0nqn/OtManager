
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using OtManager.Web.Filters;
using OtManager.Web.Models;
using OtManager.Web.Models.ViewModels;
using OtManager.Web.Services;

namespace OtManager.Web.Controllers;

/// <summary>
/// Controlador principal para la gestión de órdenes de trabajo en la interfaz MVC.
/// </summary>
[AuthorizeCustom]
public sealed class WorkOrdersController : Controller
{
    private readonly WorkOrderService _service;
    private readonly AuthService _authService;
    private readonly ILogger<WorkOrdersController> _logger;

    public WorkOrdersController(WorkOrderService service, AuthService authService, ILogger<WorkOrdersController> logger)
    {
        _service = service;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Renderiza el listado principal de órdenes aplicando los filtros ingresados por el usuario.
    /// </summary>
    /// <param name="filters">Filtros enviados en la query string.</param>
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] WorkOrderFiltersViewModel? filters)
    {
        filters ??= new WorkOrderFiltersViewModel();
        var data = await _service.GetAllAsync(filters, HttpContext.RequestAborted).ConfigureAwait(false);
        var currentUser = GetCurrentUser();

        var viewModel = new WorkOrdersListViewModel
        {
            Filters = filters,
            Orders = data.Orders,
            Clientes = data.Clientes,
            Sistemas = data.Sistemas,
            Estados = data.Estados,
            CurrentUser = currentUser,
        };

        ViewData["ActiveView"] = "list";
        TransferToastFromTempData();
        PopulateLayout(currentUser);
        return View(viewModel);
    }

    /// <summary>
    /// Muestra el detalle de una orden concreta, permitiendo búsqueda por número y edición.
    /// </summary>
    /// <param name="id">Número interno de la orden.</param>
    /// <param name="searchNumber">Valor ingresado en el buscador.</param>
    /// <param name="edit">Indica si la vista debe renderizar el modo de edición.</param>
    [HttpGet]
    public async Task<IActionResult> Details(int? id, string? searchNumber, bool edit = false)
    {
        var currentUser = GetCurrentUser();
        var reference = await _service.GetReferenceDataAsync(HttpContext.RequestAborted).ConfigureAwait(false);

        int? orderNumber = id;
        string? search = searchNumber;

        if (!string.IsNullOrWhiteSpace(searchNumber))
        {
            if (int.TryParse(searchNumber, out var parsed))
            {
                orderNumber = parsed;
            }
            else
            {
                TempData["ToastMessage"] = "Ingrese un número de orden válido";
                TempData["ToastVariant"] = "error";
            }
        }

        WorkOrderDetailResponse? detail = null;
        if (orderNumber.HasValue)
        {
            detail = await _service.GetByIdAsync(orderNumber.Value, HttpContext.RequestAborted).ConfigureAwait(false);
            if (detail is null)
            {
                TempData["ToastMessage"] = $"No se encontró la orden {orderNumber.Value}.";
                TempData["ToastVariant"] = "warning";
            }
        }

        var order = detail?.Order;
        if (order is not null && string.IsNullOrWhiteSpace(search))
        {
            search = order.Numero.ToString(CultureInfo.InvariantCulture);
        }

        var viewModel = new WorkOrderDetailViewModel
        {
            Order = order,
            Clientes = reference.Clientes,
            Sistemas = reference.Sistemas,
            Estados = reference.Estados,
            Usuarios = reference.Usuarios,
            Avances = detail?.Avances ?? Array.Empty<AvanceTrabajo>(),
            Historial = detail?.Historial ?? Array.Empty<HistorialEstado>(),
            Archivos = detail?.Archivos ?? Array.Empty<ArchivoAdjunto>(),
            Puestas = detail?.Puestas ?? Array.Empty<PuestaProduccion>(),
            SearchNumber = search,
            IsEditing = edit,
            CurrentUser = currentUser,
        };

        TransferToastFromTempData();
        ViewData["ActiveView"] = "detail";
        PopulateLayout(currentUser);
        return View(viewModel);
    }

    /// <summary>
    /// Envía a la API las modificaciones realizadas sobre una orden existente.
    /// </summary>
    /// <param name="model">Datos modificados desde el formulario.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(WorkOrderUpdateInputModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastMessage"] = "Revise los datos ingresados";
            TempData["ToastVariant"] = "error";
            return RedirectToAction(nameof(Details), new { id = model.Numero, edit = true });
        }

        var success = await _service.UpdateAsync(model, HttpContext.RequestAborted).ConfigureAwait(false);
        if (success)
        {
            TempData["ToastMessage"] = "Orden de trabajo actualizada correctamente";
            TempData["ToastVariant"] = "success";
            _logger.LogInformation("Orden {Numero} actualizada por {Usuario}", model.Numero, GetCurrentUser().Id);
        }
        else
        {
            TempData["ToastMessage"] = "No se pudo actualizar la orden. Intente nuevamente más tarde.";
            TempData["ToastVariant"] = "error";
        }

        return RedirectToAction(nameof(Details), new { id = model.Numero });
    }

    /// <summary>
    /// Muestra acciones simuladas (toasts) para mantener las interacciones del diseño original.
    /// </summary>
    /// <param name="id">Número de la orden afectada.</param>
    /// <param name="type">Tipo de toast a disparar.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TriggerDemoToast(int id, string type)
    {
        TempData["ToastMessage"] = type switch
        {
            "avance" => "Avance registrado correctamente",
            "delete" => "Elemento eliminado",
            _ => "Acción ejecutada",
        };
        TempData["ToastVariant"] = type == "delete" ? "warning" : "success";

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Presenta el historial reciente de órdenes asignadas al usuario autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var currentUser = GetCurrentUser();
        var orders = await _service.GetRecentHistoryAsync(currentUser.Id, HttpContext.RequestAborted).ConfigureAwait(false);

        var model = new RecentHistoryViewModel
        {
            RecentOrders = orders,
            CurrentUser = currentUser,
        };

        ViewData["ActiveView"] = "history";
        TransferToastFromTempData();
        PopulateLayout(currentUser);
        return View(model);
    }

    private Usuario GetCurrentUser()
    {
        return _authService.GetCurrentUser() ?? new Usuario("USUARIO", "Usuario", string.Empty, "US");
    }

    private void PopulateLayout(Usuario currentUser)
    {
        ViewBag.CurrentUser = currentUser;
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
