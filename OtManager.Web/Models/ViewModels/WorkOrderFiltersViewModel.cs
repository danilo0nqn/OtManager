namespace OtManager.Web.Models.ViewModels;

public sealed class WorkOrderFiltersViewModel
{
    public string? NumeroOrden { get; set; }
    public string? ClienteId { get; set; }
    public string? SistemaId { get; set; }
    public string? EstadoId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? Asunto { get; set; }

    public bool HasFilters => !string.IsNullOrWhiteSpace(NumeroOrden)
        || !string.IsNullOrWhiteSpace(ClienteId)
        || !string.IsNullOrWhiteSpace(SistemaId)
        || !string.IsNullOrWhiteSpace(EstadoId)
        || FechaDesde.HasValue
        || FechaHasta.HasValue
        || !string.IsNullOrWhiteSpace(Asunto);
}
