using System.Text;

namespace UAssetAPP.Mobile;

public class MainPage : ContentPage
{
    private readonly App _app;
    private readonly KeySessionInfo _session;
    private readonly Label _statusLabel;
    private readonly Label _uassetLabel;
    private readonly Label _uexpLabel;
    private readonly Label _analysisLabel;
    private readonly Button _analyzeButton;
    private readonly Picker _viewModePicker;
    private readonly Stepper _previewBytesStepper;
    private readonly Label _previewBytesLabel;
    private readonly Entry _hexSearchEntry;

    private FileResult? _uassetFile;
    private FileResult? _uexpFile;

    public MainPage(App app, KeySessionInfo session)
    {
        _app = app;
        _session = session;
        Title = "UAssetGUİ";
        SetDynamicResource(BackgroundColorProperty, "PageBg");

        ToolbarItems.Add(new ToolbarItem("Tema", null, async () => await SelectThemeAsync()));
        ToolbarItems.Add(new ToolbarItem("Çıkış", null, Logout));

        _statusLabel = CreateLabel("Durum: UAsset ve UExp dosyalarını seçin.", "SecondaryText", 13);
        _uassetLabel = CreateLabel("UAsset: seçilmedi", "SecondaryText", 13);
        _uexpLabel = CreateLabel("UExp: seçilmedi", "SecondaryText", 13);
        _analysisLabel = CreateLabel("Analiz çıktısı burada görünecek.", "PrimaryText", 14);

        _viewModePicker = new Picker { Title = "Görünüm seçeneği" };
        _viewModePicker.ItemsSource = new[] { "Özet", "HEX (Hx)", "Metin", "HEX + Metin" };
        _viewModePicker.SelectedIndex = 0;

        _previewBytesStepper = new Stepper { Minimum = 64, Maximum = 4096, Increment = 64, Value = 512 };
        _previewBytesLabel = CreateLabel("Önizleme byte: 512", "SecondaryText", 12);
        _previewBytesStepper.ValueChanged += (_, e) => _previewBytesLabel.Text = $"Önizleme byte: {(int)e.NewValue}";

        _hexSearchEntry = new Entry { Placeholder = "HEX ara (örn: 55 45 34 C1)", ClearButtonVisibility = ClearButtonVisibility.WhileEditing };

        var root = new VerticalStackLayout
        {
            Padding = new Thickness(18, 14),
            Spacing = 12,
            Children =
            {
                CreateHeaderCard(),
                CreateMenuCard(),
                CreateFilesCard(),
                CreateOptionsCard(),
                CreateAnalysisCard()
            }
        };

        _analyzeButton = CreateActionButton("Dosyayı Aç ve Oku", "AccentBurgundy", async () => await AnalyzeAsync());
        _analyzeButton.IsEnabled = false;
        root.Children.Insert(4, _analyzeButton);

        Content = new ScrollView { Content = root };
    }

    private View CreateHeaderCard() => CreateCard(new VerticalStackLayout
    {
        Spacing = 6,
        Children =
        {
            CreateLabel("Premium UAsset Explorer", "PrimaryText", 26, FontAttributes.Bold),
            CreateLabel("Mor • Beyaz • Karmen Mavisi • Bordo temalı modern görünüm.", "SecondaryText", 13),
            CreateLabel($"Giriş: {_session.UserName}", "SecondaryText", 12),
            CreateLabel($"Key Türü: {_session.KeyType}", "SecondaryText", 12),
            CreateLabel($"Kalan Süre: {_session.RemainingText}", "SecondaryText", 12),
            CreateLabel($"Geçerlilik: {_session.ExpiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC", "SecondaryText", 12),
            _statusLabel
        }
    });

    private View CreateMenuCard() => CreateCard(new HorizontalStackLayout
    {
        Spacing = 10,
        Children =
        {
            CreateActionButton("Premium", "AccentPurple", () => SetTheme(ThemeMode.Premium)),
            CreateActionButton("Siyah", "AccentBlue", () => SetTheme(ThemeMode.Dark)),
            CreateActionButton("Beyaz", "AccentBurgundy", () => SetTheme(ThemeMode.Light))
        }
    });

    private View CreateFilesCard() => CreateCard(new VerticalStackLayout
    {
        Spacing = 8,
        Children =
        {
            CreateLabel("Dosya Ekle", "PrimaryText", 18, FontAttributes.Bold),
            CreateActionButton("UAsset Seç (.uasset)", "AccentPurple", async () => await PickUassetAsync()),
            _uassetLabel,
            CreateActionButton("UExp Seç (.uexp)", "AccentBlue", async () => await PickUexpAsync()),
            _uexpLabel,
            CreateLabel("Sadece aynı isim köküne sahip .uasset + .uexp dosya çifti ile işlem yapılır.", "SecondaryText", 12)
        }
    });

    private View CreateOptionsCard() => CreateCard(new VerticalStackLayout
    {
        Spacing = 8,
        Children =
        {
            CreateLabel("Seçenekler", "PrimaryText", 18, FontAttributes.Bold),
            _viewModePicker,
            _previewBytesLabel,
            _previewBytesStepper,
            _hexSearchEntry,
            CreateLabel("HEX (Hx) özel görünüm seçeneği ile ham veriyi açabilirsiniz.", "SecondaryText", 12)
        }
    });

    private View CreateAnalysisCard() => CreateCard(new VerticalStackLayout
    {
        Spacing = 8,
        Children =
        {
            CreateLabel("Dosya İçeriği Analizi", "PrimaryText", 18, FontAttributes.Bold),
            _analysisLabel
        }
    });

    private async Task PickUassetAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "UAsset dosyası seçin" });
        if (file is null) return;
        if (!file.FileName.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert("Hatalı dosya", "Lütfen .uasset uzantılı dosya seçin.", "Tamam");
            return;
        }

        _uassetFile = file;
        _uassetLabel.Text = $"UAsset: {file.FileName}";
        ValidatePair();
    }

    private async Task PickUexpAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "UExp dosyası seçin" });
        if (file is null) return;
        if (!file.FileName.EndsWith(".uexp", StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert("Hatalı dosya", "Lütfen .uexp uzantılı dosya seçin.", "Tamam");
            return;
        }

        _uexpFile = file;
        _uexpLabel.Text = $"UExp: {file.FileName}";
        ValidatePair();
    }

    private void ValidatePair()
    {
        var valid = _uassetFile is not null && _uexpFile is not null &&
                    Path.GetFileNameWithoutExtension(_uassetFile.FileName)
                        .Equals(Path.GetFileNameWithoutExtension(_uexpFile.FileName), StringComparison.OrdinalIgnoreCase);

        _analyzeButton.IsEnabled = valid;

        _statusLabel.Text = valid
            ? "Durum: Eşleşen dosya çifti bulundu. Analiz edebilirsiniz."
            : "Durum: Çalıştırmak için aynı ada sahip .uasset ve .uexp dosyalarının ikisi de gerekli.";
    }

    private async Task AnalyzeAsync()
    {
        if (_uassetFile is null || _uexpFile is null)
            return;

        var mode = _viewModePicker.SelectedItem?.ToString() ?? "Özet";
        var previewBytes = (int)_previewBytesStepper.Value;
        var hexSearch = (_hexSearchEntry.Text ?? string.Empty).Trim();

        var uassetInfo = await InspectAsync(_uassetFile, mode, previewBytes, hexSearch);
        var uexpInfo = await InspectAsync(_uexpFile, mode, previewBytes, hexSearch);

        _analysisLabel.Text =
            $"Pair: {Path.GetFileNameWithoutExtension(_uassetFile.FileName)}\nMode: {mode}\n\n" +
            $"[UASSET]\n{uassetInfo}\n\n[UEXP]\n{uexpInfo}";
    }

    private static async Task<string> InspectAsync(FileResult file, string mode, int previewBytes, string hexSearch)
    {
        await using var stream = await file.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        var bytes = ms.ToArray();
        var viewBytes = bytes.Take(previewBytes).ToArray();

        var summary = $"Dosya: {file.FileName}\nBoyut: {bytes.Length:N0} byte\nSHA256: {AuthService.Sha256(Convert.ToHexString(bytes.Take(Math.Min(bytes.Length, 4096)).ToArray()))}";
        var hex = FormatHex(viewBytes, 16);
        var text = ToSafeText(viewBytes);

        var searchResult = string.Empty;
        if (!string.IsNullOrWhiteSpace(hexSearch))
        {
            var target = NormalizeHex(hexSearch);
            var source = BitConverter.ToString(bytes).Replace("-", string.Empty);
            var found = source.Contains(target, StringComparison.OrdinalIgnoreCase);
            searchResult = $"\nHEX arama ({hexSearch}): {(found ? "Bulundu" : "Bulunamadı")}";
        }

        return mode switch
        {
            "HEX (Hx)" => summary + "\n\n[HEX]\n" + hex + searchResult,
            "Metin" => summary + "\n\n[TEXT]\n" + text + searchResult,
            "HEX + Metin" => summary + "\n\n[HEX]\n" + hex + "\n\n[TEXT]\n" + text + searchResult,
            _ => summary + $"\nHeader(32): {BitConverter.ToString(bytes.Take(32).ToArray()).Replace("-", " ")}" + searchResult
        };
    }

    private static string NormalizeHex(string input) => input.Replace(" ", string.Empty).Replace("-", string.Empty);

    private static string ToSafeText(byte[] data)
    {
        var textPreview = Encoding.UTF8.GetString(data);
        return new string(textPreview.Select(c => char.IsControl(c) ? '·' : c).ToArray());
    }

    private static string FormatHex(byte[] data, int bytesPerLine)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < data.Length; i += bytesPerLine)
        {
            var line = data.Skip(i).Take(bytesPerLine).ToArray();
            sb.Append(i.ToString("X8")).Append("  ");
            sb.Append(string.Join(' ', line.Select(b => b.ToString("X2"))));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private Button CreateActionButton(string title, string colorKey, Action action)
    {
        var button = new Button
        {
            Text = title,
            CornerRadius = 14,
            Padding = new Thickness(14, 12),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };

        button.SetDynamicResource(Button.BackgroundColorProperty, colorKey);
        button.Clicked += (_, _) => action();
        return button;
    }

    private Button CreateActionButton(string title, string colorKey, Func<Task> action)
    {
        var button = new Button
        {
            Text = title,
            CornerRadius = 14,
            Padding = new Thickness(14, 12),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };

        button.SetDynamicResource(Button.BackgroundColorProperty, colorKey);
        button.Clicked += async (_, _) => await action();
        return button;
    }

    private Label CreateLabel(string text, string colorKey, double size, FontAttributes font = FontAttributes.None)
    {
        var label = new Label
        {
            Text = text,
            FontSize = size,
            FontAttributes = font
        };
        label.SetDynamicResource(Label.TextColorProperty, colorKey);
        return label;
    }

    private Border CreateCard(View content)
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            StrokeThickness = 1,
            Padding = new Thickness(14),
            Content = content
        };

        border.SetDynamicResource(Border.BackgroundColorProperty, "CardBg");
        border.SetDynamicResource(Border.StrokeProperty, "BorderBrush");
        return border;
    }

    private async Task SelectThemeAsync()
    {
        var result = await DisplayActionSheet("Tema seç", "İptal", null, "Premium", "Siyah", "Beyaz");
        if (result == "Premium") SetTheme(ThemeMode.Premium);
        else if (result == "Siyah") SetTheme(ThemeMode.Dark);
        else if (result == "Beyaz") SetTheme(ThemeMode.Light);
    }

    private void SetTheme(ThemeMode mode) => _app.ApplyTheme(mode);

    private void Logout() => _app.MainPage = new NavigationPage(new LoginPage(_app));
}
