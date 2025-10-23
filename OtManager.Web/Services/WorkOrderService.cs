
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OtManager.Web.Models;
using OtManager.Web.Models.ViewModels;

namespace OtManager.Web.Services;

/// <summary>
/// Servicio que encapsula el consumo de la API externa para gestionar órdenes de trabajo reales.
/// </summary>
public sealed class WorkOrderService : ApiClientBase
{
    private readonly ILogger<WorkOrderService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="WorkOrderService"/>.
    /// </summary>
    /// <param name="clientFactory">Fábrica de clientes HTTP configurada con la API OT Manager.</param>
    /// <param name="httpContextAccessor">Accessor para recuperar la sesión actual.</param>
    /// <param name="logger">Logger para diagnósticos y trazas.</param>
    public WorkOrderService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor, ILogger<WorkOrderService> logger)
        : base(clientFactory, httpContextAccessor, logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el listado de órdenes aplicando los filtros proporcionados junto con la data de apoyo para los combos.
    /// </summary>
    /// <param name="filters">Filtros ingresados por el usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la petición HTTP.</param>
    /// <returns>Datos de órdenes y catálogos asociados.</returns>
    public async Task<WorkOrdersData> GetAllAsync(WorkOrderFiltersViewModel filters, CancellationToken cancellationToken = default)
    {
        var ordersTask = FetchWorkOrdersAsync(filters, cancellationToken);
        var referenceTask = GetReferenceDataAsync(cancellationToken);

        await Task.WhenAll(ordersTask, referenceTask).ConfigureAwait(false);

        var orders = await ordersTask.ConfigureAwait(false);
        var reference = await referenceTask.ConfigureAwait(false);

        var clientes = reference.Clientes.Count > 0
            ? reference.Clientes
            : orders.Select(o => o.Cliente).GroupBy(c => c.Id).Select(g => g.First()).ToArray();

        var sistemas = reference.Sistemas.Count > 0
            ? reference.Sistemas
            : orders.Select(o => o.Sistema).GroupBy(s => s.Id).Select(g => g.First()).ToArray();

        var estados = reference.Estados.Count > 0
            ? reference.Estados
            : orders.Select(o => o.Estado).GroupBy(e => e.Id).Select(g => g.First()).ToArray();

        return new WorkOrdersData(orders, clientes, sistemas, estados);
    }

    /// <summary>
    /// Recupera el detalle completo de una orden individual, incluyendo colecciones relacionadas.
    /// </summary>
    /// <param name="id">Identificador (número) de la orden a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación para la llamada.</param>
    /// <returns>Detalle consolidado de la orden o <see langword="null"/> si no existe.</returns>
    public async Task<WorkOrderDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync($"workorders/{id}", cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar el detalle de la orden {WorkOrderId}.", id);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("La API devolvió {Status} al obtener la orden {WorkOrderId}: {Error}", response.StatusCode, id, error);
            return null;
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            var element = document.RootElement;

            if (element.ValueKind == JsonValueKind.Array)
            {
                element = element.EnumerateArray().FirstOrDefault();
            }

            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            var order = WorkOrderMapper.MapWorkOrder(element);
            var avances = WorkOrderMapper.MapAvances(element);
            var historial = WorkOrderMapper.MapHistorial(element);
            var archivos = WorkOrderMapper.MapArchivos(element);
            var puestas = WorkOrderMapper.MapPuestas(element);

            return new WorkOrderDetailResponse(order, avances, historial, archivos, puestas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible deserializar el detalle de la orden {WorkOrderId}.", id);
            return null;
        }
    }

    /// <summary>
    /// Recupera el historial reciente de órdenes asociadas a un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario autenticado.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación.</param>
    /// <returns>Colección de órdenes recientes.</returns>
    public async Task<IReadOnlyCollection<OrdenTrabajo>> GetRecentHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query["userId"] = userId;
        }

        var requestUri = query.Count == 0
            ? "workorders/history"
            : QueryHelpers.AddQueryString("workorders/history", query!);

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible recuperar el historial reciente para el usuario {UserId}.", userId);
            return Array.Empty<OrdenTrabajo>();
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("El historial devolvió {Status} para el usuario {UserId}: {Error}", response.StatusCode, userId, error);
            return Array.Empty<OrdenTrabajo>();
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return document.RootElement.EnumerateArray()
                    .Select(WorkOrderMapper.MapWorkOrder)
                    .Where(order => order is not null)
                    .Select(order => order!)
                    .ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible interpretar el historial de órdenes para {UserId}.", userId);
        }

        return Array.Empty<OrdenTrabajo>();
    }

    /// <summary>
    /// Crea una nueva orden en la API externa.
    /// </summary>
    /// <param name="model">Datos capturados desde el formulario.</param>
    /// <param name="cancellationToken">Token de cancelación para la solicitud.</param>
    /// <returns><see langword="true"/> si la creación fue exitosa.</returns>
    public async Task<bool> CreateAsync(WorkOrderUpdateInputModel model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        try
        {
            var response = await client.PostAsJsonAsync("workorders", BuildPayload(model), cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("No se pudo crear la orden {Numero}. Código {Status} - {Error}", model.Numero, response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear la orden {Numero}.", model.Numero);
            return false;
        }
    }

    /// <summary>
    /// Actualiza una orden existente enviando los datos a la API.
    /// </summary>
    /// <param name="model">Modelo con la información actualizada.</param>
    /// <param name="cancellationToken">Token de cancelación para la petición.</param>
    /// <returns><see langword="true"/> si la API confirmó la actualización.</returns>
    public async Task<bool> UpdateAsync(WorkOrderUpdateInputModel model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        try
        {
            var response = await client.PutAsJsonAsync($"workorders/{model.Numero}", BuildPayload(model), cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("No se pudo actualizar la orden {Numero}. Código {Status} - {Error}", model.Numero, response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al actualizar la orden {Numero}.", model.Numero);
            return false;
        }
    }

    /// <summary>
    /// Carga un archivo adjunto asociado a una orden de trabajo.
    /// </summary>
    /// <param name="file">Archivo a subir.</param>
    /// <param name="workOrderId">Identificador de la orden asociada.</param>
    /// <param name="cancellationToken">Token de cancelación de la subida.</param>
    /// <returns><see langword="true"/> si la carga fue aceptada.</returns>
    public async Task<bool> UploadAttachmentAsync(IFormFile file, int workOrderId, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return false;
        }

        using var client = CreateClient();
        await using var stream = file.OpenReadStream();
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(stream);
        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        }

        content.Add(streamContent, "file", file.FileName);

        try
        {
            var response = await client.PostAsync($"workorders/{workOrderId}/attachments", content, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Fallo al adjuntar archivo a la orden {Numero}. Código {Status} - {Error}", workOrderId, response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al subir un archivo para la orden {Numero}.", workOrderId);
            return false;
        }
    }

    /// <summary>
    /// Obtiene los catálogos básicos requeridos por el módulo de órdenes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Estructura con clientes, sistemas, estados y usuarios.</returns>
    public async Task<WorkOrderReferenceData> GetReferenceDataAsync(CancellationToken cancellationToken = default)
    {
        var clientesTask = FetchClientesAsync(cancellationToken);
        var sistemasTask = FetchSistemasAsync(cancellationToken);
        var estadosTask = FetchEstadosAsync(cancellationToken);
        var usuariosTask = FetchUsuariosAsync(cancellationToken);

        await Task.WhenAll(clientesTask, sistemasTask, estadosTask, usuariosTask).ConfigureAwait(false);

        return new WorkOrderReferenceData(
            await clientesTask.ConfigureAwait(false),
            await sistemasTask.ConfigureAwait(false),
            await estadosTask.ConfigureAwait(false),
            await usuariosTask.ConfigureAwait(false));
    }

    private async Task<IReadOnlyCollection<OrdenTrabajo>> FetchWorkOrdersAsync(WorkOrderFiltersViewModel filters, CancellationToken cancellationToken)
    {
        using var client = CreateClient();

        var requestUri = BuildWorkOrdersUri(filters);

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible obtener el listado de órdenes de trabajo.");
            return Array.Empty<OrdenTrabajo>();
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("El endpoint de órdenes devolvió {Status}: {Error}", response.StatusCode, error);
            return Array.Empty<OrdenTrabajo>();
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return document.RootElement.EnumerateArray()
                    .Select(WorkOrderMapper.MapWorkOrder)
                    .Where(order => order is not null)
                    .Select(order => order!)
                    .ToArray();
            }

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                var single = WorkOrderMapper.MapWorkOrder(document.RootElement);
                return single is null ? Array.Empty<OrdenTrabajo>() : new[] { single };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible interpretar el listado de órdenes retornado por la API.");
        }

        return Array.Empty<OrdenTrabajo>();
    }

    private Task<IReadOnlyCollection<Cliente>> FetchClientesAsync(CancellationToken cancellationToken)
        => FetchCollectionAsync(WorkOrderMapper.MapCliente, cancellationToken, "clients", "clientes");

    private Task<IReadOnlyCollection<Sistema>> FetchSistemasAsync(CancellationToken cancellationToken)
        => FetchCollectionAsync(WorkOrderMapper.MapSistema, cancellationToken, "systems", "sistemas");

    private Task<IReadOnlyCollection<Estado>> FetchEstadosAsync(CancellationToken cancellationToken)
        => FetchCollectionAsync(WorkOrderMapper.MapEstado, cancellationToken, "statuses", "estados");

    private Task<IReadOnlyCollection<Usuario>> FetchUsuariosAsync(CancellationToken cancellationToken)
        => FetchCollectionAsync(WorkOrderMapper.MapUsuario, cancellationToken, "users", "usuarios");

    private async Task<IReadOnlyCollection<T>> FetchCollectionAsync<T>(Func<JsonElement, T> mapper, CancellationToken cancellationToken, params string[] resources)
    {
        foreach (var resource in resources)
        {
            var result = await FetchCollectionForResourceAsync(resource, mapper, cancellationToken).ConfigureAwait(false);
            if (result.Count > 0)
            {
                return result;
            }
        }

        return Array.Empty<T>();
    }

    private async Task<IReadOnlyCollection<T>> FetchCollectionForResourceAsync<T>(string resource, Func<JsonElement, T> mapper, CancellationToken cancellationToken)
    {
        using var client = CreateClient();

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(resource, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "No fue posible contactar el recurso {Resource}.", resource);
            return Array.Empty<T>();
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("El recurso {Resource} devolvió {Status}: {Error}", resource, response.StatusCode, error);
            return Array.Empty<T>();
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                return document.RootElement.EnumerateArray().Select(mapper).ToArray();
            }

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                return new[] { mapper(document.RootElement) };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible deserializar la colección {Resource} devuelta por la API.", resource);
        }

        return Array.Empty<T>();
    }

    private static string BuildWorkOrdersUri(WorkOrderFiltersViewModel filters)
    {
        var query = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(filters.NumeroOrden))
        {
            query["numero"] = filters.NumeroOrden;
        }

        if (!string.IsNullOrWhiteSpace(filters.ClienteId))
        {
            query["clienteId"] = filters.ClienteId;
        }

        if (!string.IsNullOrWhiteSpace(filters.SistemaId))
        {
            query["sistemaId"] = filters.SistemaId;
        }

        if (!string.IsNullOrWhiteSpace(filters.EstadoId))
        {
            query["estadoId"] = filters.EstadoId;
        }

        if (!string.IsNullOrWhiteSpace(filters.Asunto))
        {
            query["asunto"] = filters.Asunto;
        }

        if (filters.FechaDesde.HasValue)
        {
            query["fechaDesde"] = filters.FechaDesde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (filters.FechaHasta.HasValue)
        {
            query["fechaHasta"] = filters.FechaHasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return query.Count == 0 ? "workorders" : QueryHelpers.AddQueryString("workorders", query!);
    }

    private static object BuildPayload(WorkOrderUpdateInputModel model)
    {
        return new
        {
            nroOrdenTrabajo = model.Numero,
            clienteId = model.ClienteId,
            sistemaId = model.SistemaId,
            estadoId = model.EstadoId,
            proyecto = model.Proyecto,
            modulo = model.Modulo,
            solicitadoPor = model.SolicitadoPor,
            asunto = model.Asunto,
            descripcion = model.Descripcion,
            observaciones = model.Observaciones,
            porcentajeAvance = model.PorcentajeAvance,
            usuarioResponsableId = model.UsuarioResponsableId,
            usuarioSolicitanteId = model.UsuarioSolicitanteId
        };
    }
}

/// <summary>
/// Datos base requeridos para poblar el listado de órdenes.
/// </summary>
public sealed record WorkOrdersData(
    IReadOnlyCollection<OrdenTrabajo> Orders,
    IReadOnlyCollection<Cliente> Clientes,
    IReadOnlyCollection<Sistema> Sistemas,
    IReadOnlyCollection<Estado> Estados);

/// <summary>
/// Resultado estructurado del detalle de una orden de trabajo.
/// </summary>
public sealed record WorkOrderDetailResponse(
    OrdenTrabajo? Order,
    IReadOnlyCollection<AvanceTrabajo> Avances,
    IReadOnlyCollection<HistorialEstado> Historial,
    IReadOnlyCollection<ArchivoAdjunto> Archivos,
    IReadOnlyCollection<PuestaProduccion> Puestas);

/// <summary>
/// Agrupa los catálogos recuperados desde la API externa.
/// </summary>
public sealed record WorkOrderReferenceData(
    IReadOnlyCollection<Cliente> Clientes,
    IReadOnlyCollection<Sistema> Sistemas,
    IReadOnlyCollection<Estado> Estados,
    IReadOnlyCollection<Usuario> Usuarios);

internal static class WorkOrderMapper
{
    public static OrdenTrabajo? MapWorkOrder(JsonElement element)
    {
        var numero = GetInt(element, "nroOrdenTrabajo", "numero", "id", "workOrderId");
        var cliente = MapCliente(GetChild(element, "cliente", "client"));
        var sistema = MapSistema(GetChild(element, "sistema", "system"));
        var modulo = GetString(element, "modulo", "module") ?? string.Empty;
        var asunto = GetString(element, "asunto", "subject", "titulo") ?? string.Empty;
        var fechaSolicitud = GetDateTime(element, "fechaSolicitud", "fechaAlta", "createdAt") ?? DateTime.MinValue;
        var fechaFinalizacion = GetDateTime(element, "fechaFinalizacion", "fechaCierre", "closedAt");
        var horasEstimadas = GetDouble(element, "cantidadHorasEstimadas", "horasEstimadas", "estimatedHours");
        var horasConsumidas = GetDouble(element, "cantidadHorasConsumidas", "horasConsumidas", "elapsedHours", "horasInvertidas");
        var estado = MapEstado(GetChild(element, "estado", "status"));
        var porcentajeAvance = GetInt(element, "porcentajeAvance", "avance", "progress");
        var usuarioSolicitante = MapUsuario(GetChild(element, "usuarioSolicitante", "solicitante", "createdBy"));
        var usuarioResponsable = MapUsuario(GetChild(element, "usuarioResponsable", "responsable", "assignedTo"));
        var descripcion = GetString(element, "descripcion", "description", "detalle") ?? string.Empty;
        var observaciones = GetString(element, "observaciones", "notes", "observacion") ?? string.Empty;
        var prioridad = GetInt(element, "prioridad", "priority");
        var proyecto = GetString(element, "proyecto", "project", "codigoProyecto") ?? string.Empty;
        var dependeDe = GetNullableInt(element, "dependeDe", "parentId", "ordenPadre");
        var fechaVencimiento = GetDateTime(element, "fechaVencimiento", "dueDate", "fechaCompromiso");
        var solicitadoPor = GetString(element, "solicitadoPor", "requestedBy", "solicitanteNombre") ?? usuarioSolicitante.Nombre;

        return new OrdenTrabajo(
            numero,
            cliente,
            sistema,
            modulo,
            asunto,
            fechaSolicitud,
            fechaFinalizacion,
            horasEstimadas,
            horasConsumidas,
            estado,
            porcentajeAvance,
            usuarioSolicitante,
            usuarioResponsable,
            descripcion,
            observaciones,
            prioridad,
            proyecto,
            dependeDe,
            fechaVencimiento,
            solicitadoPor);
    }

    public static IReadOnlyCollection<AvanceTrabajo> MapAvances(JsonElement element)
    {
        if (!TryGetProperty(element, out var avances, "avances", "progress", "detallesAvance") || avances.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<AvanceTrabajo>();
        }

        var list = new List<AvanceTrabajo>();
        foreach (var item in avances.EnumerateArray())
        {
            var id = GetInt(item, "id", "avanceId");
            var fecha = GetDateTime(item, "fecha", "date", "fechaRegistro") ?? DateTime.MinValue;
            var usuario = MapUsuario(GetChild(item, "usuario", "user"));
            var horas = GetDouble(item, "horasAvance", "horas", "cantidadHoras");
            var descripcion = GetString(item, "descripcion", "detalle", "comentarios") ?? string.Empty;
            list.Add(new AvanceTrabajo(id, fecha, usuario, horas, descripcion));
        }

        return list;
    }

    public static IReadOnlyCollection<HistorialEstado> MapHistorial(JsonElement element)
    {
        if (!TryGetProperty(element, out var historial, "historialEstados", "historial") || historial.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<HistorialEstado>();
        }

        var list = new List<HistorialEstado>();
        foreach (var item in historial.EnumerateArray())
        {
            var id = GetInt(item, "id", "historialId");
            var secuencia = GetInt(item, "secuencia", "orden");
            var estado = MapEstado(GetChild(item, "estado", "status"));
            var fechaAlta = GetDateTime(item, "fechaAlta", "fecha", "date") ?? DateTime.MinValue;
            var usuario = MapUsuario(GetChild(item, "usuario", "user"));
            list.Add(new HistorialEstado(id, secuencia, estado, fechaAlta, usuario));
        }

        return list;
    }

    public static IReadOnlyCollection<ArchivoAdjunto> MapArchivos(JsonElement element)
    {
        if (!TryGetProperty(element, out var archivos, "archivosAdjuntos", "attachments") || archivos.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ArchivoAdjunto>();
        }

        var list = new List<ArchivoAdjunto>();
        foreach (var item in archivos.EnumerateArray())
        {
            var id = GetInt(item, "id", "archivoId");
            var nombre = GetString(item, "nombreArchivo", "nombre", "fileName") ?? $"Archivo_{id}";
            var fecha = GetDateTime(item, "fechaSubida", "fecha", "uploadedAt") ?? DateTime.MinValue;
            var usuario = MapUsuario(GetChild(item, "usuario", "user"));
            var tamanio = GetLong(item, "tamanioBytes", "size", "tamanio");
            list.Add(new ArchivoAdjunto(id, nombre, fecha, usuario, tamanio));
        }

        return list;
    }

    public static IReadOnlyCollection<PuestaProduccion> MapPuestas(JsonElement element)
    {
        if (!TryGetProperty(element, out var puestas, "puestasProduccion", "deployments") || puestas.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<PuestaProduccion>();
        }

        var list = new List<PuestaProduccion>();
        foreach (var item in puestas.EnumerateArray())
        {
            var id = GetInt(item, "id", "puestaId");
            var fecha = GetDateTime(item, "fecha", "date", "fechaPuesta") ?? DateTime.MinValue;
            var usuario = MapUsuario(GetChild(item, "usuario", "user"));
            var formularios = MapFormularios(GetChild(item, "formulariosModificados", "forms"));
            var modificaciones = MapModificaciones(GetChild(item, "modificacionesBaseDatos", "scripts"));
            list.Add(new PuestaProduccion(id, fecha, usuario, formularios, modificaciones));
        }

        return list;
    }

    public static IReadOnlyCollection<FormularioModificado> MapFormularios(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<FormularioModificado>();
        }

        var list = new List<FormularioModificado>();
        foreach (var item in element.EnumerateArray())
        {
            var tipo = GetString(item, "tipo", "type") ?? "Formulario";
            var ruta = GetString(item, "ruta", "path") ?? string.Empty;
            var descripcion = GetString(item, "descripcion", "description");
            list.Add(new FormularioModificado(tipo, ruta, descripcion));
        }

        return list;
    }

    public static IReadOnlyCollection<ModificacionBaseDatos> MapModificaciones(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ModificacionBaseDatos>();
        }

        var list = new List<ModificacionBaseDatos>();
        foreach (var item in element.EnumerateArray())
        {
            var tipo = GetString(item, "tipo", "type") ?? "SCRIPT";
            var nombre = GetString(item, "nombre", "name") ?? string.Empty;
            var script = GetString(item, "script", "contenido") ?? string.Empty;
            var descripcion = GetString(item, "descripcion", "description");
            list.Add(new ModificacionBaseDatos(tipo, nombre, script, descripcion));
        }

        return list;
    }

    public static Cliente MapCliente(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var id = GetInt(element, "id", "clienteId");
            var descripcion = GetString(element, "descripcion", "nombre", "razonSocial") ?? $"Cliente {id}";
            return new Cliente(id, descripcion);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var descripcion = element.GetString() ?? string.Empty;
            return new Cliente(0, descripcion);
        }

        return new Cliente(0, string.Empty);
    }

    public static Sistema MapSistema(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var id = GetInt(element, "id", "sistemaId", "systemId");
            var descripcion = GetString(element, "descripcion", "nombre", "name") ?? $"Sistema {id}";
            return new Sistema(id, descripcion);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var descripcion = element.GetString() ?? string.Empty;
            return new Sistema(0, descripcion);
        }

        return new Sistema(0, string.Empty);
    }

    public static Estado MapEstado(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var id = GetInt(element, "id", "estadoId", "statusId");
            var descripcion = GetString(element, "descripcion", "nombre", "name", "descripcionEstado") ?? $"Estado {id}";
            return new Estado(id, descripcion);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var descripcion = element.GetString() ?? string.Empty;
            return new Estado(0, descripcion);
        }

        return new Estado(0, string.Empty);
    }

    public static Usuario MapUsuario(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var id = GetString(element, "id", "usuario", "userId", "username", "userName") ?? "";
            var nombre = GetString(element, "nombre", "name", "firstName") ?? id;
            var apellidos = GetString(element, "apellidos", "lastName", "apellido") ?? string.Empty;
            var iniciales = GetString(element, "iniciales", "initials") ?? BuildInitials(nombre, apellidos, id);
            return new Usuario(id, nombre, apellidos, iniciales);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var id = element.GetString() ?? string.Empty;
            return new Usuario(id, id, string.Empty, BuildInitials(id, string.Empty, id));
        }

        return new Usuario(string.Empty, string.Empty, string.Empty, string.Empty);
    }

    private static string BuildInitials(string? nombre, string? apellidos, string fallback)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            builder.Append(char.ToUpperInvariant(nombre.Trim()[0]));
        }

        if (!string.IsNullOrWhiteSpace(apellidos))
        {
            builder.Append(char.ToUpperInvariant(apellidos.Trim()[0]));
        }

        if (builder.Length == 0 && !string.IsNullOrWhiteSpace(fallback))
        {
            builder.Append(char.ToUpperInvariant(fallback[0]));
            if (fallback.Length > 1)
            {
                builder.Append(char.ToUpperInvariant(fallback[^1]));
            }
        }

        return builder.ToString();
    }

    private static bool TryGetProperty(JsonElement element, out JsonElement property, params string[] names)
    {
        foreach (var name in names)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out property))
            {
                return true;
            }
        }

        property = default;
        return false;
    }

    private static JsonElement GetChild(JsonElement element, params string[] names)
        => TryGetProperty(element, out var property, names) ? property : default;

    private static string? GetString(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null,
        };
    }

    private static int GetInt(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names))
        {
            return 0;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt32(out var value) => value,
            JsonValueKind.Number => (int)property.GetDouble(),
            JsonValueKind.String when int.TryParse(property.GetString(), out var value) => value,
            _ => 0,
        };
    }

    private static int? GetNullableInt(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Number when property.TryGetInt32(out var value) => value,
            JsonValueKind.Number => (int)property.GetDouble(),
            JsonValueKind.String when int.TryParse(property.GetString(), out var value) => value,
            _ => null,
        };
    }

    private static long GetLong(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names))
        {
            return 0;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetInt64(out var value) => value,
            JsonValueKind.String when long.TryParse(property.GetString(), out var value) => value,
            _ => 0,
        };
    }

    private static double GetDouble(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names))
        {
            return 0d;
        }

        return property.ValueKind switch
        {
            JsonValueKind.Number when property.TryGetDouble(out var value) => value,
            JsonValueKind.String when double.TryParse(property.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) => value,
            _ => 0d,
        };
    }

    private static DateTime? GetDateTime(JsonElement element, params string[] names)
    {
        if (!TryGetProperty(element, out var property, names))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var raw = property.GetString();
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result))
            {
                return result;
            }
        }
        else if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var ticks))
        {
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(ticks).DateTime;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
