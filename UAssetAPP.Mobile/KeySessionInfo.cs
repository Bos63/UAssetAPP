namespace UAssetAPP.Mobile;

public sealed record KeySessionInfo(
    string UserName,
    string KeyType,
    DateTimeOffset ExpiresAtUtc,
    long RemainingSeconds,
    string PanelBaseUrl)
{
    public string RemainingText
    {
        get
        {
            if (RemainingSeconds <= 0)
                return "Süresi dolmuş";

            var ts = TimeSpan.FromSeconds(RemainingSeconds);
            if (ts.TotalDays >= 1)
                return $"{(int)ts.TotalDays} gün {ts.Hours} saat";
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours} saat {ts.Minutes} dk";
            return $"{ts.Minutes} dk {ts.Seconds} sn";
        }
    }
}
