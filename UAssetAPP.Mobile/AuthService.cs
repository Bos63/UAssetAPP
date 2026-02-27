using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace UAssetAPP.Mobile;

public static class AuthService
{
    public static async Task<AuthResult> TryKeyLoginAsync(string username, string panelKey, string panelLink, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(panelKey))
            return AuthResult.Fail("Kullanıcı adı ve key zorunludur.");

        var normalizedLink = NormalizePanelLink(panelLink);
        if (string.IsNullOrWhiteSpace(normalizedLink))
            return AuthResult.Fail("Admin panel HTTPS linki zorunludur.");

        if (!normalizedLink.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return AuthResult.Fail("Admin panel sadece HTTPS üzerinden kullanılabilir.");

        return await TryRemoteKeyLoginAsync(username.Trim(), panelKey.Trim(), normalizedLink, ct);
    }

    public static string NormalizePanelLink(string input)
    {
        var link = (input ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(link))
            return string.Empty;

        if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            link = "https://" + link;

        return link.TrimEnd('/');
    }

    public static string BuildAdminLink(string panelLink) => NormalizePanelLink(panelLink);

    public static string BuildKeyManagementLink(string panelLink)
    {
        var normalized = NormalizePanelLink(panelLink);
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized + "/keys";
    }

    private static async Task<AuthResult> TryRemoteKeyLoginAsync(string username, string panelKey, string panelLink, CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var endpoint = panelLink + "/api/mobile/keys/consume";
            var payload = new RemoteKeyLoginRequest(username, panelKey, "android");
            var response = await http.PostAsJsonAsync(endpoint, payload, ct);
            if (!response.IsSuccessStatusCode)
                return AuthResult.Fail($"HTTP {(int)response.StatusCode}");

            var data = await response.Content.ReadFromJsonAsync<RemoteKeyLoginResponse>(cancellationToken: ct);
            if (data is null)
                return AuthResult.Fail("Panel yanıtı boş.");

            if (!data.Success)
                return AuthResult.Fail(data.Message ?? "Panel key doğrulaması başarısız.");

            if (data.IsExpired)
                return AuthResult.Fail("Bu key süresi dolduğu için kullanılamaz.");

            if (data.ExpiresAtUtc is null || data.RemainingSeconds is null || string.IsNullOrWhiteSpace(data.KeyType))
                return AuthResult.Fail("Panel yanıtında key meta bilgileri eksik.");

            var expires = DateTimeOffset.Parse(data.ExpiresAtUtc);
            var session = new KeySessionInfo(
                data.UserName ?? username,
                data.KeyType,
                expires,
                data.RemainingSeconds.Value,
                panelLink);

            return AuthResult.Success(data.Message ?? "Key doğrulandı.", session);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }

    public static bool IsLikelyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return Regex.IsMatch(key.Trim(), "^[A-Za-z0-9\\-]{8,128}$");
    }

    private sealed record RemoteKeyLoginRequest(string UserName, string PanelKey, string Client);

    private sealed record RemoteKeyLoginResponse(
        bool Success,
        string? Message,
        string? UserName,
        string? KeyType,
        string? ExpiresAtUtc,
        long? RemainingSeconds,
        bool IsExpired);
}

public sealed record AuthResult(bool IsSuccess, string Message, KeySessionInfo? Session)
{
    public static AuthResult Success(string message, KeySessionInfo session) => new(true, message, session);
    public static AuthResult Fail(string message) => new(false, message, null);
}
