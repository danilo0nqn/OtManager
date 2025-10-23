using System.IO;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OtManager.Web.Models;

namespace OtManager.Web.Services;

/// <summary>
/// Clase base para servicios que consumen la API externa del gestor de OT.
/// Encapsula la creación de clientes HTTP autenticados y operaciones comunes de sesión.
/// </summary>
public abstract class ApiClientBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApiClientBase"/>.
    /// </summary>
    /// <param name="clientFactory">Fábrica de clientes HTTP configurada en la aplicación.</param>
    /// <param name="httpContextAccessor">Accessor para recuperar el contexto HTTP actual.</param>
    /// <param name="logger">Instancia de logger para registrar diagnósticos.</param>
    protected ApiClientBase(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor, ILogger logger)
    {
        _clientFactory = clientFactory;
        _httpContextAccessor = httpContextAccessor;
        Logger = logger;
    }

    /// <summary>
    /// Obtiene el logger asociado al servicio concreto.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Opciones de serialización JSON comunes para toda la capa de servicios.
    /// </summary>
    protected JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Devuelve el contexto HTTP actual, si existe.
    /// </summary>
    protected HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    /// <summary>
    /// Recupera la sesión del usuario activo, si se encuentra disponible.
    /// </summary>
    protected ISession? Session => HttpContext?.Session;

    /// <summary>
    /// Crea un cliente HTTP preparado para consumir la API, adjuntando el token JWT almacenado en sesión cuando corresponde.
    /// </summary>
    /// <param name="includeToken">Indica si debe agregarse el encabezado Authorization con el token actual.</param>
    /// <returns>Instancia de <see cref="HttpClient"/> con la configuración estándar.</returns>
    protected HttpClient CreateClient(bool includeToken = true)
    {
        var client = _clientFactory.CreateClient("OTApi");

        if (includeToken && Session is { } session)
        {
            var token = session.GetString(SessionKeys.AuthToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return client;
    }

    /// <summary>
    /// Intenta leer un mensaje de error de la respuesta HTTP en formato JSON.
    /// </summary>
    /// <param name="response">Respuesta recibida desde la API.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la operación actual.</param>
    /// <returns>Mensaje de error si está disponible; en caso contrario, <see langword="null"/>.</returns>
    protected static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return null;
        }

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            if (stream == Stream.Null)
            {
                return null;
            }

            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (document.RootElement.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString();
                }

                if (document.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                {
                    return error.GetString();
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
