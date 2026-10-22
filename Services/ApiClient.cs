using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CampusCart.Services;

public sealed class ApiClient(HttpClient http, SecureTokenStore tokens)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T> GetAsync<T>(string path, bool authenticated = false, CancellationToken ct = default) =>
        await SendAsync<T>(HttpMethod.Get, path, null, authenticated, ct);
    public async Task<T> PostAsync<T>(string path, object? body, bool authenticated = false, CancellationToken ct = default) =>
        await SendAsync<T>(HttpMethod.Post, path, body, authenticated, ct);
    public async Task<T> PutAsync<T>(string path, object body, bool authenticated = true, CancellationToken ct = default) =>
        await SendAsync<T>(HttpMethod.Put, path, body, authenticated, ct);
    public async Task PatchAsync(string path, object body, bool authenticated = true, CancellationToken ct = default) =>
        _ = await SendAsync<JsonElement>(HttpMethod.Patch, path, body, authenticated, ct, allowNoContent: true);
    public async Task DeleteAsync(string path, bool authenticated = true, CancellationToken ct = default) =>
        _ = await SendAsync<JsonElement>(HttpMethod.Delete, path, null, authenticated, ct, allowNoContent: true);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, bool authenticated,
        CancellationToken ct, bool allowNoContent = false, bool allowRefresh = true)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonOptions);
        if (authenticated)
        {
            var token = await tokens.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                AppSession.SignOut();
                throw new ApiException(HttpStatusCode.Unauthorized, "Session expirée. Connecte-toi à nouveau.");
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        HttpResponseMessage response;
        try { response = await http.SendAsync(request, ct); }
        catch (HttpRequestException ex) { throw new ApiException(0, $"API inaccessible : {ex.Message}"); }
        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && authenticated && allowRefresh)
            {
                if (await TryRefreshAsync(ct))
                    return await SendAsync<T>(method, path, body, authenticated, ct, allowNoContent, allowRefresh: false);
                { await tokens.ClearAsync(); AppSession.SignOut(); }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized && authenticated) { await tokens.ClearAsync(); AppSession.SignOut(); }
            if (!response.IsSuccessStatusCode)
                throw await ToApiExceptionAsync(response, ct);
            if (allowNoContent && response.StatusCode == HttpStatusCode.NoContent) return default!;
            try
            {
                var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
                return result is null ? default! : result;
            }
            catch (JsonException ex) { throw new ApiException(response.StatusCode, $"Réponse API illisible : {ex.Message}"); }
        }
    }

    private async Task<bool> TryRefreshAsync(CancellationToken ct)
    {
        var refreshToken = await tokens.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken)) return false;
        try
        {
            using var response = await http.PostAsJsonAsync("api/v1/sessions/refresh", new RefreshTokenRequestDto(refreshToken), JsonOptions, ct);
            if (!response.IsSuccessStatusCode) return false;
            var refreshed = await response.Content.ReadFromJsonAsync<TokenResponseDto>(JsonOptions, ct);
            if (refreshed is null || string.IsNullOrWhiteSpace(refreshed.AccessToken)) return false;
            await tokens.SaveAsync(refreshed);
            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException) { return false; }
    }

    private static async Task<ApiException> ToApiExceptionAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var raw = await response.Content.ReadAsStringAsync(ct);
        var messages = new List<string>();
        string? title = null;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.TryGetProperty("title", out var t)) title = t.GetString();
            if (root.TryGetProperty("detail", out var d) && !string.IsNullOrWhiteSpace(d.GetString())) messages.Add(d.GetString()!);
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                foreach (var field in errors.EnumerateObject())
                    foreach (var item in field.Value.EnumerateArray()) if (!string.IsNullOrWhiteSpace(item.GetString())) messages.Add(item.GetString()!);
            if (root.TryGetProperty("message", out var m) && !string.IsNullOrWhiteSpace(m.GetString())) messages.Add(m.GetString()!);
        }
        catch (JsonException) { }
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Identifiants invalides ou adresse e-mail non confirmée.",
            HttpStatusCode.Forbidden => "Cette action n’est pas autorisée pour ce compte.",
            HttpStatusCode.NotFound => "La ressource demandée est introuvable.",
            HttpStatusCode.Conflict => "Cette action n’est pas possible dans l’état actuel.",
            (HttpStatusCode)429 => "Trop de tentatives. Réessaie plus tard.",
            _ => $"Erreur API ({(int)response.StatusCode})."
        };
        if (messages.Count == 0 && !string.IsNullOrWhiteSpace(title)) messages.Add(title);
        return new ApiException(response.StatusCode, messages.FirstOrDefault() ?? fallback, messages);
    }
}

public sealed class SecureTokenStore
{
    private const string AccessKey = "campuscart.access-token";
    private const string RefreshKey = "campuscart.refresh-token";
    public Task<string?> GetAccessTokenAsync() => SecureStorage.Default.GetAsync(AccessKey);
    public Task<string?> GetRefreshTokenAsync() => SecureStorage.Default.GetAsync(RefreshKey);
    public async Task SaveAsync(TokenResponseDto tokens)
    {
        await SecureStorage.Default.SetAsync(AccessKey, tokens.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshKey, tokens.RefreshToken);
    }
    public Task ClearAsync() { SecureStorage.Default.Remove(AccessKey); SecureStorage.Default.Remove(RefreshKey); return Task.CompletedTask; }
}
