using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace UAssetAPP.Mobile;

public static class AuthService
{
    private const string AllowedUser = "paneladmin";
    private const string AllowedPasswordHash = "0f9b17748cdb243f6830a269b389a700551ca725aabc6f5caf7feef67dd76c25";
    private const string AllowedPanelKeyHash = "690a90e0ba3eacc8f904cc47e2b8967a2a5333858106e7e6209ef459373f9889";

    public static async Task<AuthResult> TryLoginAsync(string username, string password, string panelKey, string panelLink, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(panelKey))
            return AuthResult.Fail("Kullanıcı adı, şifre ve panel key zorunludur.");

        var user = username.Trim();
        var pass = password.Trim();
        var key = panelKey.Trim();

        var normalizedLink = NormalizePanelLink(panelLink);
        if (!string.IsNullOrWhiteSpace(normalizedLink))
        {
            var remote = await TryRemoteLoginAsync(user, pass, key, normalizedLink, ct);
            if (remote.IsSuccess)
                return remote;

            return AuthResult.Fail($"Panel doğrulaması başarısız: {remote.Message}");
        }

        // Panel link henüz girilmediyse geçici demo fallback.
        if (!user.Equals(AllowedUser, StringComparison.OrdinalIgnoreCase))
            return AuthResult.Fail("Demo kullanıcı adı hatalı.");

        var passwordHash = Sha256(pass);
        var panelKeyHash = Sha256(key);

        var ok = string.Equals(passwordHash, AllowedPasswordHash, StringComparison.OrdinalIgnoreCase)
                 && string.Equals(panelKeyHash, AllowedPanelKeyHash, StringComparison.OrdinalIgnoreCase);

        return ok
            ? AuthResult.Success(user, "Demo mod giriş başarılı.")
            : AuthResult.Fail("Demo mod kullanıcı adı/şifre/key hatalı.");
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

    public static string BuildRegisterLink(string panelLink)
    {
        var normalized = NormalizePanelLink(panelLink);
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized + "/register";
    }

    private static async Task<AuthResult> TryRemoteLoginAsync(string username, string password, string panelKey, string panelLink, CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var endpoint = panelLink + "/api/mobile/auth/validate";
            var payload = new RemoteAuthRequest(username, password, panelKey, "android");
            var response = await http.PostAsJsonAsync(endpoint, payload, ct);
            if (!response.IsSuccessStatusCode)
                return AuthResult.Fail($"HTTP {(int)response.StatusCode}");

            var data = await response.Content.ReadFromJsonAsync<RemoteAuthResponse>(cancellationToken: ct);
            if (data is null)
                return AuthResult.Fail("Panel yanıtı boş.");

            return data.Success
                ? AuthResult.Success(data.UserName ?? username, data.Message ?? "Panel doğrulandı.")
                : AuthResult.Fail(data.Message ?? "Panel kimlik bilgilerini kabul etmedi.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail(ex.Message);
        }
    }

    public static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record RemoteAuthRequest(string UserName, string Password, string PanelKey, string Client);
    private sealed record RemoteAuthResponse(bool Success, string? Message, string? UserName);
}

public sealed record AuthResult(bool IsSuccess, string Message, string? EffectiveUser)
{
    public static AuthResult Success(string user, string message) => new(true, message, user);
    public static AuthResult Fail(string message) => new(false, message, null);
}
