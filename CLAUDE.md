# PC Builder — Bilgisayar Toplama ve Uyumluluk Asistanı

Bu proje Kaan'ın CV/portfolyo projesidir. Kaan .NET/C# ve SQL Server geçmişine sahip, JavaScript/React/React Native'i yeni öğreniyor.

## Çalışma kuralları (Claude için)

- Kaan ile **Türkçe** konuş. Kod, değişken ve dosya adları İngilizce kalsın.
- Kaan bu projeyi mülakatta anlatacak. Her adımdan sonra oluşturduğun dosyaların ne işe yaradığını ve neden öyle tasarlandığını kısa ve sade bir dille açıkla.
- Bir seferde tek bir görev yap, sonra dur ve Kaan'ın onayını bekle. Bütün haftayı tek seferde yapma.
- Yeni bir NuGet/npm paketi eklemeden önce ne işe yaradığını söyle.
- Kod yazdıktan sonra derle (`dotnet build`) ve varsa testleri çalıştır (`dotnet test`). Hata varsa düzelt.
- Gizli bilgileri (JWT anahtarı, bulut bağlantı dizesi) koda yazma. Yerelde User Secrets, sunucuda ortam değişkeni kullan.
- Her mantıklı adımın sonunda commit mesajı öner.

## Geliştirme ortamı

- Windows, VS Code, PowerShell terminali
- .NET 10 SDK (10.0.404) kurulu
- Yerel veritabanı: SQL Server Express, örnek adı `SQLEXPRESS`, Windows kimlik doğrulaması
- Yerel bağlantı dizesi (`appsettings.Development.json`):
  `Server=localhost\SQLEXPRESS;Database=PcBuilder;Trusted_Connection=True;TrustServerCertificate=True`

## Mimari

- **Monorepo:** `backend/` (.NET çözümü) ve `mobile/` (Expo uygulaması) aynı repoda.
- **Backend:** ASP.NET Core Web API, .NET 10, controller tabanlı. EF Core (code-first, migration'lar), SQL Server.
- **Mobil:** React Native + Expo, Expo Router, TypeScript. Durum yönetimi React Context + AsyncStorage. Token `expo-secure-store` ile saklanır.
- **Test:** xUnit. CI: GitHub Actions.
- **Yayın:** API Azure App Service (Free F1), veritabanı Azure SQL (ücretsiz teklif), mobil EAS Build ile Android APK.

### Backend klasör yapısı

```text
backend/
├── PcBuilder.sln
├── src/
│   ├── PcBuilder.Core/              # EF Core'a bağımlı DEĞİL
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Compatibility/
│   │       ├── BuildSnapshot.cs
│   │       ├── CompatibilityEngine.cs
│   │       ├── PowerCalculator.cs
│   │       └── Rules/
│   └── PcBuilder.Api/
│       ├── Controllers/
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── Migrations/
│       │   └── Seed/parts.json
│       ├── Dtos/
│       ├── Services/
│       └── Program.cs
└── tests/
    └── PcBuilder.Core.Tests/        # xUnit
```

- Entity'ler API'den doğrudan dönmez, `Dtos` içindeki modellere dönüştürülür.
- Uyumluluk kuralları `PcBuilder.Core` içinde durur. Testler veritabanı olmadan çalışır.

## Veri modeli

Parça hiyerarşisi EF Core **TPT (Table-per-Type)** ile eşlenir: `Part` soyut temel sınıf, kategoriler ondan türer.
`modelBuilder.Entity<Part>().UseTptMappingStrategy();`

| Tablo | Alanlar |
| --- | --- |
| Parts | Id, Name, Brand, Category, Price (decimal), Tdp (W), ImageUrl, IsActive, CreatedAt |
| Cpus | Socket (string: AM4, AM5, LGA1700, LGA1851), Cores, Threads, BaseClockGhz, BoostClockGhz, HasIntegratedGraphics, IncludesCooler |
| Motherboards | Socket, Chipset, FormFactor, MemoryType, RamSlots, MaxRamGb, M2Slots, SataPorts |
| RamKits | MemoryType, ModuleCount, ModuleSizeGb, SpeedMhz |
| Gpus | GpuChip, VramGb, LengthMm, RecommendedPsuW |
| Psus | Wattage, EfficiencyRating, IsModular |
| Cases | SupportedFormFactors ([Flags] enum), MaxGpuLengthMm, MaxCoolerHeightMm |
| Storages | StorageType, Interface (M2/Sata), CapacityGb |
| Users | Id, Email (unique), PasswordHash, DisplayName, CreatedAt |
| Builds | Id, UserId, Name, CreatedAt, UpdatedAt |
| BuildItems | Id, BuildId, PartId, Quantity |

Enum'lar:

```csharp
public enum PartCategory { Cpu, Motherboard, Ram, Gpu, Psu, Case, Storage }
public enum MemoryType { DDR4, DDR5 }
public enum FormFactor { ATX, MicroATX, MiniITX }
[Flags] public enum FormFactorSupport { None = 0, ATX = 1, MicroATX = 2, MiniITX = 4 }
```

- Kullanıcı silinirse sistemleri de silinir (cascade). Parçalar silinmez, `IsActive = false` yapılır.
- Başlangıç verisi `Data/Seed/parts.json` içinde durur. Veritabanı boşsa uygulama açılışta bu dosyadan doldurur. Her kategoriden 10–15 gerçek ürün, gerçek teknik özelliklerle girilir. Fiyatlar yaklaşık değerdir.

## Uyumluluk kuralları

Her kural ayrı bir sınıftır ve `ICompatibilityRule` arayüzünü uygular. `CompatibilityEngine` DI'dan gelen tüm kuralları çalıştırır ve sonuçları önem sırasına göre döner. Bir kuralın ihtiyaç duyduğu parça seçilmemişse kural atlanır.

| Kod | Kural | Seviye |
| --- | --- | --- |
| R01 | CPU soketi = anakart soketi | Error |
| R02 | RAM tipi = anakart bellek tipi | Error |
| R03 | Toplam RAM modülü ≤ anakart slot sayısı | Error |
| R04 | Toplam RAM GB ≤ anakart MaxRamGb | Error |
| R05 | Kasa anakartın form faktörünü destekliyor | Error |
| R06 | GPU uzunluğu ≤ kasa MaxGpuLengthMm | Error |
| R07 | PSU < tahmini tüketim | Error |
| R08 | PSU < tahmini tüketim × 1.2 | Warning |
| R09 | PSU < GPU RecommendedPsuW | Warning |
| R10 | M.2 depolama adedi ≤ anakart M2Slots | Error |
| R11 | SATA depolama adedi ≤ anakart SataPorts | Error |
| R12 | GPU yok ve CPU'da dahili grafik yok | Warning |
| R13 | CPU kutusundan soğutucu çıkmıyor | Info |
| R14 | Zorunlu kategori eksik | Info |

Güç hesabı: tahmini tüketim = CPU TDP + GPU TDP + 100 W. Önerilen PSU = tahmini × 1.2, en yakın 50 W'a yukarı yuvarlanır. CPU ve GPU'nun ikisi de seçilmemişse ikisi de 0 döner.

```csharp
public enum Severity { Info, Warning, Error }
public record CompatibilityIssue(string RuleCode, Severity Severity, string Message);
public interface ICompatibilityRule { IEnumerable<CompatibilityIssue> Check(BuildSnapshot build); }
```

Hata mesajları Türkçe, kullanıcıya gösterilecek şekilde yazılır.

## API uçları

| Metot | Yol | Giriş |
| --- | --- | --- |
| POST | /api/auth/register | Hayır |
| POST | /api/auth/login | Hayır (JWT döner) |
| GET | /api/parts?category=&search=&sort= | Hayır |
| GET | /api/parts/{id} | Hayır |
| POST | /api/parts/compatible?category= | Hayır (gövdede mevcut seçim, sadece Error çıkarmayan adaylar döner) |
| POST | /api/compatibility/check | Hayır (issues, totalPrice, estimatedWattage, recommendedPsuWattage, isCompatible) |
| GET/POST/PUT/DELETE | /api/builds, /api/builds/{id} | Evet (sadece sahibi erişir, başkasınınki için 404) |

Hatalar `ProblemDetails` formatında döner. Şifreler `PasswordHasher<T>` ile hash'lenir. API dokümantasyonu için OpenAPI + Swagger UI veya Scalar kullanılır.

## Yol haritası

1. **Hafta 1:** Çözüm ve projeler, entity'ler, TPT, ilk migration, seed verisi, GET /api/parts uçları
2. **Hafta 2:** Uyumluluk motoru, R01–R14, PowerCalculator, 30+ xUnit testi, check ve compatible uçları, GitHub Actions
3. **Hafta 3:** Expo projesi, Expo Router ile 3 sekme (Topla, Sistemlerim, Profil), parça listesi ve detay ekranları
4. **Hafta 4:** BuildContext, sistem oluşturma ekranı, canlı uyumluluk kontrolü, "Sadece uyumlular" filtresi
5. **Hafta 5:** JWT ile giriş/kayıt, builds CRUD, Sistemlerim ekranı
6. **Hafta 6:** Azure'a yayın, EAS Build ile APK, README, ekran görüntüleri

## İlerleme

- [x] Hafta 1
- [ ] Hafta 2
- [ ] Hafta 3
- [ ] Hafta 4
- [ ] Hafta 5
- [ ] Hafta 6
