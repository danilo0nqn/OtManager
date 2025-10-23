
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OtManager.Web.Models;

namespace OtManager.Web.Services;

/// <summary>
/// Servicio encargado de gestionar la autenticación contra la API remota y persistir el token JWT en sesión.
/// </summary>
public sealed class AuthService : ApiClientBase
{
    private static readonly string[] TokenPropertyNames = ["token", "accessToken", "jwt", "jwtToken"];
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AuthService"/>.
    /// </summary>
    /// <param name="clientFactory">Fábrica de clientes HTTP configurada para la API externa.</param>
    /// <param name="httpContextAccessor">Accessor de contexto HTTP para manipular la sesión.</param>
    /// <param name="logger">Instancia de logger para diagnósticos.</param>
    public AuthService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
        : base(clientFactory, httpContextAccessor, logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Realiza el flujo de login contra la API, guarda el token JWT en sesión y retorna el resultado de la operación.
    /// </summary>
    /// <param name="username">Nombre de usuario proporcionado por el cliente.</param>
    /// <param name="password">Contraseña asociada al usuario.</param>
    /// <param name="cancellationToken">Token de cancelación para abortar la solicitud.</param>
    /// <returns>Resultado de la autenticación, incluyendo token y nombre a mostrar cuando corresponde.</returns>
    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient(includeToken: false);

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("auth/login", new { username, password }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contactar el endpoint de login para el usuario {User}.", username);
            return LoginResult.CreateFailure("No se pudo contactar con el servicio de autenticación.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Autenticación fallida para {User}. Código: {StatusCode}", username, response.StatusCode);
            return LoginResult.CreateFailure(errorMessage ?? "Credenciales inválidas.");
        }

        string token;
        Usuario? userFromLogin;

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            token = ExtractToken(document.RootElement) ?? string.Empty;
            userFromLogin = ExtractUsuario(document.RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No fue posible interpretar la respuesta de autenticación para {User}.", username);
            return LoginResult.CreateFailure("No fue posible interpretar la respuesta de autenticación.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("La API no devolvió un token JWT válido para el usuario {User}.", username);
            return LoginResult.CreateFailure("La API no devolvió un token JWT válido.");
        }

        PersistToken(token);
        PersistUser(userFromLogin ?? BuildFallbackUser(username));

        var profile = await GetUserProfileAsync(token, cancellationToken).ConfigureAwait(false);
        if (profile is not null)
        {
            PersistUser(profile);
        }

        var displayName = Session?.GetString(SessionKeys.DisplayName) ?? profile?.Nombre ?? username;
        return LoginResult.CreateSuccess(token, displayName);
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado utilizando el token proporcionado.
    /// </summary>
    /// <param name="token">Token JWT que se enviará como Bearer al endpoint de perfil.</param>
    /// <param name="cancellationToken">Token de cancelación asociado a la llamada.</param>
    /// <returns>Instancia de <see cref="Usuario"/> si la API devuelve datos válidos; de lo contrario <see langword="null"/>.</returns>
    public async Task<Usuario?> GetUserProfileAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        PersistToken(token);

        using var client = CreateClient(includeToken: false);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        try
        {
            var response = await client.GetAsync("auth/profile", cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("El endpoint de perfil devolvió {StatusCode}; se continuará con los datos básicos en sesión.", response.StatusCode);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            var profile = ExtractUsuario(document.RootElement);

            if (profile is not null)
            {
                PersistUser(profile);
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible recuperar el perfil del usuario autenticado.");
            return null;
        }
    }

    /// <summary>
    /// Limpia la sesión actual y elimina cualquier token almacenado.
    /// </summary>
    public void Logout()
    {
        Session?.Clear();
    }

    /// <summary>
    /// Indica si existe un token JWT vigente en sesión.
    /// </summary>
    /// <returns><see langword="true"/> si hay un token almacenado; en caso contrario <see langword="false"/>.</returns>
    public bool HasValidSession()
    {
        return !string.IsNullOrWhiteSpace(Session?.GetString(SessionKeys.AuthToken));
    }

    /// <summary>
    /// Obtiene el usuario actualmente almacenado en sesión, cuando está disponible.
    /// </summary>
    /// <returns>Datos del usuario autenticado o <see langword="null"/> si no se pudieron recuperar.</returns>
    public Usuario? GetCurrentUser()
    {
        var session = Session;
        if (session is null)
        {
            return null;
        }

        try
        {
            var serialized = session.GetString(SessionKeys.UserData);
            if (!string.IsNullOrWhiteSpace(serialized))
            {
                var user = JsonSerializer.Deserialize<Usuario>(serialized, SerializerOptions);
                if (user is not null)
                {
                    return user;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No fue posible deserializar la información de usuario desde la sesión.");
        }

        var username = session.GetString(SessionKeys.Username);
        return string.IsNullOrWhiteSpace(username) ? null : BuildFallbackUser(username);
    }

    /// <summary>
    /// Recupera el token JWT almacenado en sesión.
    /// </summary>
    /// <returns>Token actual o <see langword="null"/> si no existe.</returns>
    public string? GetToken()
    {
        return Session?.GetString(SessionKeys.AuthToken);
    }

    private void PersistToken(string token)
    {
        Session?.SetString(SessionKeys.AuthToken, token);
        Session?.SetString(SessionKeys.IsLoggedIn, bool.TrueString);
    }

    private void PersistUser(Usuario user)
    {
        var serialized = JsonSerializer.Serialize(user, SerializerOptions);
        Session?.SetString(SessionKeys.UserData, serialized);
        Session?.SetString(SessionKeys.Username, user.Id);
        Session?.SetString(SessionKeys.DisplayName, $"{user.Nombre} {user.Apellidos}".Trim());
    }

    private Usuario BuildFallbackUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new Usuario("", "Usuario", string.Empty, "US");
        }

        var initialsBuilder = new StringBuilder();
        foreach (var part in username.Split([' ', '.', '_'], StringSplitOptions.RemoveEmptyEntries))
        {
            initialsBuilder.Append(char.ToUpperInvariant(part[0]));
        }

        if (initialsBuilder.Length == 0)
        {
            initialsBuilder.Append(username[..Math.Min(2, username.Length)].ToUpperInvariant());
        }

        return new Usuario(username, username, string.Empty, initialsBuilder.ToString());
    }

    private static string? ExtractToken(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in TokenPropertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString();
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var nested = ExtractToken(property.Value);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = ExtractToken(item);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static Usuario? ExtractUsuario(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = ExtractUsuario(item);
                    if (nested is not null)
                    {
                        return nested;
                    }
                }
            }

            return null;
        }

        if (TryCreateUsuario(element, out var user))
        {
            return user;
        }

        foreach (var property in element.EnumerateObject())
        {
            var nested = ExtractUsuario(property.Value);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static bool TryCreateUsuario(JsonElement element, out Usuario? user)
    {
        var id = GetString(element, "id", "usuario", "userId", "username", "userName", "correo", "email");
        if (string.IsNullOrWhiteSpace(id))
        {
            user = null;
            return false;
        }

        var nombre = GetString(element, "nombre", "name", "firstName") ?? id;
        var apellidos = GetString(element, "apellidos", "lastName", "apellido") ?? string.Empty;
        var iniciales = GetString(element, "iniciales", "initials") ?? BuildInitials(nombre, apellidos, id);

        user = new Usuario(id, nombre, apellidos, iniciales);
        return true;
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

    private static string? GetString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
            {
                var value = property.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }
}

/// <summary>
/// Representa el resultado de una operación de autenticación contra la API externa.
/// </summary>
public sealed record LoginResult(bool Success, string? Token, string? DisplayName, string? ErrorMessage)
{
    /// <summary>
    /// Crea una instancia exitosa del resultado de login.
    /// </summary>
    /// <param name="token">Token JWT emitido por la API.</param>
    /// <param name="displayName">Nombre legible del usuario autenticado.</param>
    /// <returns>Instancia de <see cref="LoginResult"/> con estado satisfactorio.</returns>
    public static LoginResult CreateSuccess(string token, string? displayName) => new(true, token, displayName, null);

    /// <summary>
    /// Crea una instancia fallida del resultado de login.
    /// </summary>
    /// <param name="errorMessage">Mensaje de error para mostrar en la interfaz.</param>
    /// <returns>Instancia de <see cref="LoginResult"/> indicando fallo.</returns>
    public static LoginResult CreateFailure(string errorMessage) => new(false, null, null, errorMessage);
}
