namespace OtManager.Web.Models;

public sealed record Cliente(int Id, string Descripcion);

public sealed record Sistema(int Id, string Descripcion);

public sealed record Usuario(string Id, string Nombre, string Apellidos, string Iniciales);

public sealed record Estado(int Id, string Descripcion);

public sealed record OrdenTrabajo(
    int Numero,
    Cliente Cliente,
    Sistema Sistema,
    string Modulo,
    string Asunto,
    DateTime FechaSolicitud,
    DateTime? FechaFinalizacion,
    double HorasEstimadas,
    double HorasConsumidas,
    Estado Estado,
    int PorcentajeAvance,
    Usuario UsuarioSolicitante,
    Usuario UsuarioResponsable,
    string Descripcion,
    string Observaciones,
    int Prioridad,
    string Proyecto,
    int? DependeDe,
    DateTime? FechaVencimiento,
    string SolicitadoPor);

public sealed record AvanceTrabajo(
    int Id,
    DateTime Fecha,
    Usuario Usuario,
    double HorasAvance,
    string Descripcion);

public sealed record HistorialEstado(
    int Id,
    int Secuencia,
    Estado Estado,
    DateTime FechaAlta,
    Usuario Usuario);

public sealed record ArchivoAdjunto(
    int Id,
    string NombreArchivo,
    DateTime FechaSubida,
    Usuario Usuario,
    long TamanioBytes);

public sealed record FormularioModificado(
    string Tipo,
    string Ruta,
    string? Descripcion = null);

public sealed record ModificacionBaseDatos(
    string Tipo,
    string Nombre,
    string Script,
    string? Descripcion = null);

public sealed record PuestaProduccion(
    int Id,
    DateTime Fecha,
    Usuario Usuario,
    IReadOnlyCollection<FormularioModificado> FormulariosModificados,
    IReadOnlyCollection<ModificacionBaseDatos> ModificacionesBaseDatos);
