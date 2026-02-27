namespace UAssetAPP.Mobile;

public class LoginPage : ContentPage
{
    private readonly App _app;

    public LoginPage(App app)
    {
        _app = app;
        Title = "Özel Panel Giriş";
        SetDynamicResource(BackgroundColorProperty, "PageBg");

        var userEntry = new Entry { Placeholder = "Kullanıcı adı", ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
        var passEntry = new Entry { Placeholder = "Şifre", IsPassword = true, ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
        var keyEntry = new Entry { Placeholder = "Panel Key", IsPassword = true, ClearButtonVisibility = ClearButtonVisibility.WhileEditing };

        var info = new Label
        {
            Text = "Sadece özel panel kullanıcı bilgileri ile giriş yapılabilir.",
            FontSize = 13
        };
        info.SetDynamicResource(Label.TextColorProperty, "SecondaryText");

        var loginButton = new Button
        {
            Text = "Giriş Yap",
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            CornerRadius = 14,
            Padding = new Thickness(14, 12)
        };
        loginButton.SetDynamicResource(Button.BackgroundColorProperty, "AccentPurple");

        loginButton.Clicked += async (_, _) =>
        {
            var ok = AuthService.TryLogin(userEntry.Text ?? string.Empty, passEntry.Text ?? string.Empty, keyEntry.Text ?? string.Empty);
            if (!ok)
            {
                await DisplayAlert("Erişim reddedildi", "Kullanıcı adı, şifre veya panel key hatalı.", "Tamam");
                return;
            }

            _app.MainPage = new NavigationPage(new MainPage(_app, userEntry.Text?.Trim() ?? "paneladmin"));
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
}

file static class ViewExt
{
    public static T AssignDynamic<T>(this T view, BindableProperty property, string key) where T : BindableObject
    {
        view.SetDynamicResource(property, key);
        return view;
    }
}
