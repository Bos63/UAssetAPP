namespace UAssetAPP.Mobile;

public class LoginPage : ContentPage
{
    private const string PanelLinkPrefKey = "panel_link";
    private readonly App _app;

    public LoginPage(App app)
    {
        _app = app;
        Title = "Özel Panel Giriş";
        SetDynamicResource(BackgroundColorProperty, "PageBg");

        var userEntry = new Entry { Placeholder = "Kullanıcı adı", ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
        var keyEntry = new Entry { Placeholder = "Süreli Key", ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
        var panelLinkEntry = new Entry
        {
            Placeholder = "Admin Panel Link (https://admin-panel-site.com)",
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
            Text = Preferences.Default.Get(PanelLinkPrefKey, "https://admin-panel-site.com")
        };

        var info = new Label
        {
            Text = "Admin panel HTTPS linki zorunlu. Key üretimi panelde yapılır ve mobilde key ile giriş yapılır.",
            FontSize = 13
        };
        info.SetDynamicResource(Label.TextColorProperty, "SecondaryText");

        var loginButton = MakeButton("Key ile Giriş Yap", "AccentPurple");
        var openPanelButton = MakeButton("Admin Paneli Aç", "AccentBlue");
        var keyPanelButton = MakeButton("Key Yönetimi", "AccentBurgundy");

        openPanelButton.Clicked += async (_, _) => await OpenPanelLinkAsync(panelLinkEntry.Text, false);
        keyPanelButton.Clicked += async (_, _) => await OpenPanelLinkAsync(panelLinkEntry.Text, true);

        loginButton.Clicked += async (_, _) =>
        {
            var panelLink = panelLinkEntry.Text ?? string.Empty;
            var normalized = AuthService.NormalizePanelLink(panelLink);
            Preferences.Default.Set(PanelLinkPrefKey, normalized);

            if (!AuthService.IsLikelyKey(keyEntry.Text ?? string.Empty))
            {
                await DisplayAlert("Geçersiz key", "Key formatı geçersiz görünüyor.", "Tamam");
                return;
            }

            loginButton.IsEnabled = false;
            try
            {
                var result = await AuthService.TryKeyLoginAsync(
                    userEntry.Text ?? string.Empty,
                    keyEntry.Text ?? string.Empty,
                    normalized);

                if (!result.IsSuccess || result.Session is null)
                {
                    await DisplayAlert("Erişim reddedildi", result.Message, "Tamam");
                    return;
                }

                _app.MainPage = new NavigationPage(new MainPage(_app, result.Session));
            }
            finally
            {
                loginButton.IsEnabled = true;
            }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "UAssetGUİ",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    }.AssignDynamic(Label.TextColorProperty, "PrimaryText"),
                    new Label
                    {
                        Text = "Admin Panel Key Girişi",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center
                    }.AssignDynamic(Label.TextColorProperty, "SecondaryText"),
                    info,
                    panelLinkEntry,
                    new HorizontalStackLayout { Spacing = 8, Children = { openPanelButton, keyPanelButton } },
                    userEntry,
                    keyEntry,
                    loginButton
                }
            }
        };
    }

    private static Button MakeButton(string text, string colorKey)
    {
        var button = new Button
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            CornerRadius = 14,
            Padding = new Thickness(14, 12)
        };
        button.SetDynamicResource(Button.BackgroundColorProperty, colorKey);
        return button;
    }

    private async Task OpenPanelLinkAsync(string? link, bool keys)
    {
        var normalized = AuthService.NormalizePanelLink(link ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            await DisplayAlert("Panel link gerekli", "Önce panel link girin.", "Tamam");
            return;
        }

        Preferences.Default.Set(PanelLinkPrefKey, normalized);
        var uri = keys ? AuthService.BuildKeyManagementLink(normalized) : AuthService.BuildAdminLink(normalized);

        try
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch
        {
            await DisplayAlert("Açılamadı", "Panel link açılamadı. Link formatını kontrol edin.", "Tamam");
        }
    }
}

file static class ViewExt
{
    public static T AssignDynamic<T>(this T view, BindableProperty property, string key) where T : BindableObject
    {
        view.SetDynamicResource(property, key);
        return view;
    }
}
