using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<KeyStore>();
builder.Services.AddSingleton<SessionStore>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "UAssetAPP.ControlAPI", status = "ok" }));

// Admin: Süreli key üretir
app.MapPost("/api/admin/keys/create", (CreateKeyRequest req, KeyStore store) =>
{
    if (req.AdminSecret != "CHANGE_ME_ADMIN_SECRET")
        return Results.Unauthorized();

    if (!DurationMap.TryGetValue(req.KeyType, out var span))
        return Results.BadRequest(new { message = "Geçersiz keyType" });

    var now = DateTimeOffset.UtcNow;
    var key = $"UA-{Convert.ToHexString(RandomNumberGenerator.GetBytes(8))}-{req.KeyType.ToUpperInvariant()}";
    var item = new KeyItem(
        key,
        req.UserName.Trim(),
        req.KeyType,
        now,
        now.Add(span),
        false,
        null);

    store.Upsert(item);

    return Results.Ok(new
    {
        success = true,
        key,
        keyType = req.KeyType,
        expiresAtUtc = item.ExpiresAtUtc,
        remainingSeconds = (long)span.TotalSeconds
    });
});

// Mobil login: key tüketim/doğrulama
app.MapPost("/api/mobile/keys/consume", (ConsumeKeyRequest req, KeyStore store) =>
{
    var item = store.Get(req.PanelKey);
    if (item is null)
        return Results.Ok(new MobileConsumeResponse(false, "Key bulunamadı", req.UserName, null, null, 0, true));

    if (!item.UserName.Equals(req.UserName, StringComparison.OrdinalIgnoreCase))
        return Results.Ok(new MobileConsumeResponse(false, "Key kullanıcı ile eşleşmiyor", req.UserName, item.KeyType, item.ExpiresAtUtc, 0, true));

    if (item.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        return Results.Ok(new MobileConsumeResponse(false, "Key süresi dolmuş", item.UserName, item.KeyType, item.ExpiresAtUtc, 0, true));

    if (item.Consumed)
        return Results.Ok(new MobileConsumeResponse(false, "Key daha önce tüketildi", item.UserName, item.KeyType, item.ExpiresAtUtc, 0, true));

    var consumed = item with { Consumed = true, ConsumedAtUtc = DateTimeOffset.UtcNow };
    store.Upsert(consumed);

    var remain = Math.Max(0, (long)(consumed.ExpiresAtUtc - DateTimeOffset.UtcNow).TotalSeconds);
    return Results.Ok(new MobileConsumeResponse(true, "Key geçerli", consumed.UserName, consumed.KeyType, consumed.ExpiresAtUtc, remain, false));
});

// Mobil session açar (key sonrası)
app.MapPost("/api/mobile/sessions/open", (OpenSessionRequest req, SessionStore sessions) =>
{
    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    var expires = DateTimeOffset.UtcNow.AddHours(8);
    sessions.Upsert(new SessionItem(token, req.UserName, expires));
    return Results.Ok(new { success = true, token, expiresAtUtc = expires });
});

// UAsset+UExp dosyalarını okuyup analiz eder
app.MapPost("/api/mobile/uasset/analyze", async (HttpRequest request, SessionStore sessions) =>
{
    var auth = request.Headers.Authorization.ToString();
    var token = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? auth[7..].Trim()
        : string.Empty;

    if (string.IsNullOrWhiteSpace(token) || !sessions.IsValid(token, out var session))
        return Results.Unauthorized();

    if (!request.HasFormContentType)
        return Results.BadRequest(new { message = "multipart/form-data bekleniyor" });

    var form = await request.ReadFormAsync();
    var uasset = form.Files.GetFile("uasset");
    var uexp = form.Files.GetFile("uexp");

    if (uasset is null || uexp is null)
        return Results.BadRequest(new { message = "uasset ve uexp dosyaları zorunlu" });

    if (!uasset.FileName.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase) ||
        !uexp.FileName.EndsWith(".uexp", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Dosya uzantıları .uasset / .uexp olmalı" });

    var aBase = Path.GetFileNameWithoutExtension(uasset.FileName);
    var eBase = Path.GetFileNameWithoutExtension(uexp.FileName);
    if (!aBase.Equals(eBase, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Dosya çifti aynı ada sahip olmalı" });

    var ua = await AnalyzeAsync(uasset.OpenReadStream(), uasset.FileName);
    var ue = await AnalyzeAsync(uexp.OpenReadStream(), uexp.FileName);

    return Results.Ok(new
    {
        success = true,
        user = session!.UserName,
        pairName = aBase,
        uasset = ua,
        uexp = ue
    });
});

app.Run();

static async Task<FileAnalysis> AnalyzeAsync(Stream stream, string fileName)
{
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms);
    var data = ms.ToArray();

    var sha = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
    var header = BitConverter.ToString(data.Take(32).ToArray()).Replace("-", " ");
    var preview = BitConverter.ToString(data.Take(128).ToArray()).Replace("-", " ");

    return new FileAnalysis(fileName, data.Length, sha, header, preview);
}

static readonly Dictionary<string, TimeSpan> DurationMap = new(StringComparer.OrdinalIgnoreCase)
{
    ["1h"] = TimeSpan.FromHours(1),
    ["5h"] = TimeSpan.FromHours(5),
    ["1d"] = TimeSpan.FromDays(1),
    ["1w"] = TimeSpan.FromDays(7),
    ["1m"] = TimeSpan.FromDays(30),
    ["1season"] = TimeSpan.FromDays(90)
};

record CreateKeyRequest(string AdminSecret, string UserName, string KeyType);
record ConsumeKeyRequest(string UserName, string PanelKey, string Client);
record OpenSessionRequest(string UserName);
record MobileConsumeResponse(bool Success, string Message, string UserName, string? KeyType, DateTimeOffset? ExpiresAtUtc, long RemainingSeconds, bool IsExpired);
record FileAnalysis(string FileName, long Size, string Sha256, string Header32Hex, string Preview128Hex);
record KeyItem(string Key, string UserName, string KeyType, DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc, bool Consumed, DateTimeOffset? ConsumedAtUtc);
record SessionItem(string Token, string UserName, DateTimeOffset ExpiresAtUtc);

sealed class KeyStore
{
    private readonly ConcurrentDictionary<string, KeyItem> _map = new(StringComparer.OrdinalIgnoreCase);
    public KeyItem? Get(string key) => _map.TryGetValue(key, out var v) ? v : null;
    public void Upsert(KeyItem item) => _map[item.Key] = item;
}

sealed class SessionStore
{
    private readonly ConcurrentDictionary<string, SessionItem> _map = new(StringComparer.OrdinalIgnoreCase);
    public void Upsert(SessionItem item) => _map[item.Token] = item;
    public bool IsValid(string token, out SessionItem? session)
    {
        if (_map.TryGetValue(token, out var s) && s.ExpiresAtUtc > DateTimeOffset.UtcNow)
        {
            session = s;
            return true;
        }

        session = null;
        return false;
    }
}
