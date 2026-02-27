using System.Security.Cryptography;
using System.Text;

namespace UAssetAPP.Mobile;

public static class AuthService
{
    // TODO: User'ın vereceği özel link/API geldiğinde burası uzak doğrulamaya bağlanabilir.
    // Şu an yalnızca özel panel kullanıcı + şifre + panel key ile giriş verilir.
    private const string AllowedUser = "paneladmin";
    private const string AllowedPasswordHash = "0f9b17748cdb243f6830a269b389a700551ca725aabc6f5caf7feef67dd76c25";
    private const string AllowedPanelKeyHash = "690a90e0ba3eacc8f904cc47e2b8967a2a5333858106e7e6209ef459373f9889";

    public static bool TryLogin(string username, string password, string panelKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(panelKey))
            return false;

        if (!username.Trim().Equals(AllowedUser, StringComparison.OrdinalIgnoreCase))
            return false;

        var passwordHash = Sha256(password.Trim());
        var panelKeyHash = Sha256(panelKey.Trim());

        return string.Equals(passwordHash, AllowedPasswordHash, StringComparison.OrdinalIgnoreCase)
               && string.Equals(panelKeyHash, AllowedPanelKeyHash, StringComparison.OrdinalIgnoreCase);
    }

    public static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
