using System.ComponentModel.DataAnnotations;

namespace OtManager.Web.Models.ViewModels;

public sealed class WorkOrderUpdateInputModel
{
    [Required]
    public int Numero { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    public int SistemaId { get; set; }

    [Required]
    public int EstadoId { get; set; }

    [Required]
    [Display(Name = "Proyecto")]
    public string Proyecto { get; set; } = string.Empty;

    public string Modulo { get; set; } = string.Empty;

    public string SolicitadoPor { get; set; } = string.Empty;

    public string Asunto { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Observaciones { get; set; } = string.Empty;

    public int PorcentajeAvance { get; set; }

    public string UsuarioResponsableId { get; set; } = string.Empty;

    public string UsuarioSolicitanteId { get; set; } = string.Empty;
}
