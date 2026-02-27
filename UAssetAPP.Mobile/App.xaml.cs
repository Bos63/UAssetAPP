namespace UAssetAPP.Mobile;

public class App : Application
{
    public App()
    {
        ApplyTheme(ThemeMode.Premium);
        MainPage = new NavigationPage(new LoginPage(this));
    }

    public void ApplyTheme(ThemeMode mode)
    {
        var palette = mode switch
        {
            ThemeMode.Dark => AppPalette.Dark,
            ThemeMode.Light => AppPalette.Light,
            _ => AppPalette.Premium
        };

        Resources = palette;
    }
}
