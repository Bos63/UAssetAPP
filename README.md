# UAssetAPP
API/GUI made to easily edit specific uasset files using [UAssetAPI](https://github.com/atenfyr/UAssetAPI).\
Below is a list of supported files.

## Octopath Traveler 2
- AbilityData.uasset

## Android APK (Android 5+ to Android 15)
`UAssetAPP.Mobile` is a .NET MAUI Android project configured for:
- **Minimum Android version:** Android 5.0 (API 21)
- **Target SDK:** Build ortamındaki en güncel Android SDK (android-35 yüklüyse Android 15 / API 35)

### Build APK
```bash
dotnet restore UAssetAPP.Mobile/UAssetAPP.Mobile.csproj
dotnet build UAssetAPP.Mobile/UAssetAPP.Mobile.csproj -c Release -f net8.0-android
```

### Publish signed/unsigned APK
```bash
dotnet publish UAssetAPP.Mobile/UAssetAPP.Mobile.csproj -c Release -f net8.0-android
```


> Not: Mobil proje şu anda APK üretimi için iskelet yapıdadır; mevcut masaüstü/OT2 kütüphaneleri Android ile doğrudan bağlanmamıştır.


### Build APK (Cloud / GitHub Actions)
Localde `dotnet` yoksa GitHub Actions ile APK üretebilirsin:
1. GitHub'da **Actions** sekmesine gir.
2. **Build Android APK** workflow'unu seç.
3. **Run workflow** ile çalıştır.
4. İş bitince **Artifacts** içinden `UAssetAPP-android-apk` dosyasını indir.


### Mobil özellikler
- Telefon dosya yöneticisinden `.uasset` ve `.uexp` dosyası seçme
- Aynı isim köküne sahip çift doğrulama (ikisi yoksa analiz kapalı)
- Dosya içeriği ön analizi: boyut, header hex, metin önizleme
- Tema seçenekleri: Premium (mor/beyaz/karmen mavi/bordo), Siyah, Beyaz

- Giriş paneli (kullanıcı + şifre + panel key zorunlu)
- HEX (Hx) görüntüleme modları: Özet, HEX, Metin, HEX+Metin
- HEX arama ve byte önizleme ayarı (64-4096)

### Özel panel giriş
Varsayılan demo bilgileri:
- Kullanıcı: `paneladmin`
- Şifre: `UAssetAdmin!2026`
- Panel Key: `PbgPanel#Key-2026`

> Not: Bunlar geçici demo değerlerdir. Vereceğiniz özel link/API geldiğinde uzak doğrulama ile değiştirilebilir.
