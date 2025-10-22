using OtManager.Web.Models;

namespace OtManager.Web.Models.ViewModels;

public sealed class WorkOrdersListViewModel
{
    public required WorkOrderFiltersViewModel Filters { get; init; }
    public required IReadOnlyCollection<OrdenTrabajo> Orders { get; init; }
    public required IReadOnlyCollection<Cliente> Clientes { get; init; }
    public required IReadOnlyCollection<Sistema> Sistemas { get; init; }
    public required IReadOnlyCollection<Estado> Estados { get; init; }
    public required Usuario CurrentUser { get; init; }

    public double TotalHorasEstimadas => Orders.Sum(o => o.HorasEstimadas);
    public double TotalHorasConsumidas => Orders.Sum(o => o.HorasConsumidas);
}
