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
- Admin panel ayrı web sitesi: `https://admin-panel-site.com`
- Mobil giriş: **kullanıcı adı + süreli key**
- Uygulama, key doğrulamasını panel endpoint'i ile yapar ve key meta bilgisini ekranda gösterir.

### Süreli key türleri
- `1h`, `5h`, `1d`, `1w`, `1m`, `1season`

### Panel link ile doğrulama
- Login ekranına panel linkini gir (`https://...`).
- Uygulama girişte endpoint'e POST atar: `/api/mobile/keys/consume`
- Beklenen JSON istek alanları: `userName`, `panelKey`, `client`
- Beklenen JSON cevap alanları: `success`, `message`, `userName`, `keyType`, `expiresAtUtc`, `remainingSeconds`, `isExpired`
- Süresi dolmuş key için `isExpired=true` döner; uygulama girişi reddeder.
- Panel detay sözleşmesi: `AdminPanel/KEY_SYSTEM_SPEC.md`

## Android Studio (Native) proje
Eğer hedefin doğrudan Android Studio ile hatasız açıp APK almaksa `UAssetAPP.AndroidStudio/` klasörünü kullan.

### Android Studio ile açma
1. Android Studio > **Open** > `UAssetAPP.AndroidStudio`
2. Gradle senkronizasyonunu bekle
3. **Build > Build APK(s)**

Bu proje:
- Paket adı: `com.urzuasset.pbg`
- Uygulama adı: `UAssetGUİ`
- Min SDK: 21 (Android 5)
- Target SDK: 35 (Android 15)
- Giriş + `.uasset` / `.uexp` çift kontrolü + temel HEX/SHA analiz içerir.
