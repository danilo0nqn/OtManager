using OtManager.Web.Models;

namespace OtManager.Web.Models.ViewModels;

public sealed class RecentHistoryViewModel
{
    public required IReadOnlyCollection<OrdenTrabajo> RecentOrders { get; init; }
    public required Usuario CurrentUser { get; init; }
}
