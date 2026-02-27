namespace UAssetAPP.Mobile;

public static class AppPalette
{
    public static ResourceDictionary Premium => new()
    {
        ["PageBg"] = Color.FromArgb("#F8F6FF"),
        ["CardBg"] = Color.FromArgb("#FFFFFF"),
        ["PrimaryText"] = Color.FromArgb("#2F1E52"),
        ["SecondaryText"] = Color.FromArgb("#564A7A"),
        ["AccentPurple"] = Color.FromArgb("#7B2EFF"),
        ["AccentBlue"] = Color.FromArgb("#1C9BFF"),
        ["AccentBurgundy"] = Color.FromArgb("#8E1A44"),
        ["BorderColor"] = Color.FromArgb("#E4DAFF"),
        ["BorderBrush"] = new SolidColorBrush(Color.FromArgb("#E4DAFF"))
    };

    public static ResourceDictionary Dark => new()
    {
        ["PageBg"] = Color.FromArgb("#0F1117"),
        ["CardBg"] = Color.FromArgb("#181C27"),
        ["PrimaryText"] = Colors.White,
        ["SecondaryText"] = Color.FromArgb("#B7C0D8"),
        ["AccentPurple"] = Color.FromArgb("#8C5CFF"),
        ["AccentBlue"] = Color.FromArgb("#4BAEFF"),
        ["AccentBurgundy"] = Color.FromArgb("#C43A67"),
        ["BorderColor"] = Color.FromArgb("#2D3550"),
        ["BorderBrush"] = new SolidColorBrush(Color.FromArgb("#2D3550"))
    };

    public static ResourceDictionary Light => new()
    {
        ["PageBg"] = Colors.White,
        ["CardBg"] = Color.FromArgb("#F8F8F8"),
        ["PrimaryText"] = Colors.Black,
        ["SecondaryText"] = Color.FromArgb("#4D4D4D"),
        ["AccentPurple"] = Color.FromArgb("#6E3BFF"),
        ["AccentBlue"] = Color.FromArgb("#287BFF"),
        ["AccentBurgundy"] = Color.FromArgb("#A3264C"),
        ["BorderColor"] = Color.FromArgb("#DADADA"),
        ["BorderBrush"] = new SolidColorBrush(Color.FromArgb("#DADADA"))
    };
}
