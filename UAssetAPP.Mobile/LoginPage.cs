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
        var passEntry = new Entry { Placeholder = "Şifre", IsPassword = true, ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
        var keyEntry = new Entry { Placeholder = "Panel Key", IsPassword = true, ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
        var panelLinkEntry = new Entry
        {
            Placeholder = "Panel Link (örn: panel.site.com)",
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
            Text = Preferences.Default.Get(PanelLinkPrefKey, string.Empty)
        };

        var info = new Label
        {
            Text = "Panel link girersen doğrulama panel API üzerinden yapılır. Link yoksa demo fallback kullanılır.",
            FontSize = 13
        };
        info.SetDynamicResource(Label.TextColorProperty, "SecondaryText");

        var loginButton = MakeButton("Giriş Yap", "AccentPurple");
        var openPanelButton = MakeButton("Panel Linkini Aç", "AccentBlue");
        var registerButton = MakeButton("Panelden Şifre/Key Oluştur", "AccentBurgundy");

        openPanelButton.Clicked += async (_, _) => await OpenPanelLinkAsync(panelLinkEntry.Text, false);
        registerButton.Clicked += async (_, _) => await OpenPanelLinkAsync(panelLinkEntry.Text, true);

        loginButton.Clicked += async (_, _) =>
        {
            var panelLink = panelLinkEntry.Text ?? string.Empty;
            var normalized = AuthService.NormalizePanelLink(panelLink);
            Preferences.Default.Set(PanelLinkPrefKey, normalized);

            loginButton.IsEnabled = false;
            try
            {
                var result = await AuthService.TryLoginAsync(
                    userEntry.Text ?? string.Empty,
                    passEntry.Text ?? string.Empty,
                    keyEntry.Text ?? string.Empty,
                    normalized);

                if (!result.IsSuccess)
                {
                    await DisplayAlert("Erişim reddedildi", result.Message, "Tamam");
                    return;
                }

                _app.MainPage = new NavigationPage(new MainPage(_app, result.EffectiveUser ?? "panel"));
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
                        Text = "Özel Panel Girişi",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center
                    }.AssignDynamic(Label.TextColorProperty, "SecondaryText"),
                    info,
                    panelLinkEntry,
                    new HorizontalStackLayout
                    {
                        Spacing = 8,
                        Children = { openPanelButton, registerButton }
                    },
                    userEntry,
                    passEntry,
                    keyEntry,
                    loginButton,
                    new Label
                    {
                        Text = "Demo kullanıcı: paneladmin",
                        FontSize = 12
                    }.AssignDynamic(Label.TextColorProperty, "SecondaryText")
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

    private async Task OpenPanelLinkAsync(string? link, bool register)
    {
        var normalized = AuthService.NormalizePanelLink(link ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            await DisplayAlert("Panel link gerekli", "Önce panel link girin.", "Tamam");
            return;
        }

        Preferences.Default.Set(PanelLinkPrefKey, normalized);
        var uri = register ? AuthService.BuildRegisterLink(normalized) : normalized;

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
