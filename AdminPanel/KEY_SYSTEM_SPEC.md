# Admin Panel & Süreli Key Sistemi (HTTPS)

Admin panel ayrı web sitesi olarak çalışır:
- Örnek URL: `https://admin-panel-site.com`
- Sadece HTTPS kabul edilir.

## Key türleri
Panel şu süreli key tiplerini üretir:
- `1h`  (1 saat)
- `5h`  (5 saat)
- `1d`  (1 gün)
- `1w`  (1 hafta)
- `1m`  (1 ay)
- `1season` (1 sezon)

## Mobil doğrulama endpoint
`POST /api/mobile/keys/consume`

### Request JSON
```json
{
  "userName": "player1",
  "panelKey": "ABCDEF-123456-KEY",
  "client": "android"
}
```

### Response JSON (başarılı)
```json
{
  "success": true,
  "message": "Key geçerli",
  "userName": "player1",
  "keyType": "1w",
  "expiresAtUtc": "2026-03-10T15:30:00Z",
  "remainingSeconds": 36000,
  "isExpired": false
}
```

### Response JSON (süresi dolmuş)
```json
{
  "success": false,
  "message": "Key süresi dolmuş",
  "userName": "player1",
  "keyType": "1d",
  "expiresAtUtc": "2026-03-01T12:00:00Z",
  "remainingSeconds": 0,
  "isExpired": true
}
```

## Kurallar
- Süresi dolan key tekrar kullanılamaz.
- Key başarılı kullanımlarda panel tarafında loglanır.
- Mobil uygulama key türü + kalan süre + geçerlilik tarihini gösterir.
