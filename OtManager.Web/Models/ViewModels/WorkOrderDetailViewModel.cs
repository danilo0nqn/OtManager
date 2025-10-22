using OtManager.Web.Models;

namespace OtManager.Web.Models.ViewModels;

public sealed class WorkOrderDetailViewModel
{
    public OrdenTrabajo? Order { get; init; }
    public IReadOnlyCollection<Cliente> Clientes { get; init; } = Array.Empty<Cliente>();
    public IReadOnlyCollection<Sistema> Sistemas { get; init; } = Array.Empty<Sistema>();
    public IReadOnlyCollection<Estado> Estados { get; init; } = Array.Empty<Estado>();
    public IReadOnlyCollection<Usuario> Usuarios { get; init; } = Array.Empty<Usuario>();
    public IReadOnlyCollection<AvanceTrabajo> Avances { get; init; } = Array.Empty<AvanceTrabajo>();
    public IReadOnlyCollection<HistorialEstado> Historial { get; init; } = Array.Empty<HistorialEstado>();
    public IReadOnlyCollection<ArchivoAdjunto> Archivos { get; init; } = Array.Empty<ArchivoAdjunto>();
    public IReadOnlyCollection<PuestaProduccion> Puestas { get; init; } = Array.Empty<PuestaProduccion>();
    public string? SearchNumber { get; init; }
    public bool IsEditing { get; init; }
    public Usuario CurrentUser { get; init; } = new("", "", "", "");
}
