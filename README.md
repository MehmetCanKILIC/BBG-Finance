# BBG Finance — Rezervasyon Raporlama Portalı

JP_ROIBEDS veritabanındaki rezervasyon verilerini KPI kartları ve tablolar
halinde raporlayan, salt-okunur bir web portalı. Alt yapı [Finaliza](https://github.com/mehmetcankilic/finaliza)
projesiyle aynıdır: ASP.NET Web Forms (.NET Framework 4.7.2), DevExtreme arayüz
bileşenleri, SQL Server ve Forms Authentication tabanlı oturum yönetimi.

## Mimari

- **AuthDB** (`BBGFinanceAuth`): Bu portala özel `Kullanici` / `OturumGecmisi`
  tabloları. Portalın giriş/oturum bilgileri buradadır, JP_ROIBEDS'e hiç
  dokunulmaz.
- **ReportDB** (`JP_ROIBEDS`): Rezervasyon verisinin gerçek kaynağı.
  **Bu portal buraya SADECE OKUMA (SELECT) sorguları gönderir.** `Core/DbHelper.cs`
  içindeki `ReportDbHelper` sınıfının kasıtlı olarak `ExecuteNonQuery` metodu
  yoktur. SQL Server tarafında da bu bağlantı için salt-okunur (`db_datareader`)
  ayrı bir login kullanılması önerilir; `Web.config`'teki connection string
  `ApplicationIntent=ReadOnly` ile işaretlenmiştir.

```
BBGFinance/
  Core/                 DbHelper (Auth+Report), SessionManager, AuthBase/AdminBase, AppConstants, JsonHelper
  Data/                 RezervasyonRepository.cs (Admin), MusteriRaporRepository.cs (Musteri), SqlSafe.cs
  Modules/Rezervasyonlar/ Liste.aspx / Detay.aspx  — SADECE Admin (AdminBase)
  Default.aspx           Admin Dashboard: ciro/kâr/komisyon/tedarikçi dahil tam görünüm
  MusteriDashboard.aspx  Musteri Dashboard: kendi verisi, cost/profit/commission/tedarikçi YOK
  Login.aspx             Giriş ekranı (role göre Default.aspx / MusteriDashboard.aspx'e yönlendirir)
  Database/              Kurulum ve yerel test scriptleri
```

## Rol modeli ve veri izolasyonu (ÖNEMLİ)

Bu portalda iki rol vardır:

- **Admin** — BBG Finance içindeki iç kullanıcılar (yönetim/finans ekibi). Tüm
  müşteri gruplarının verisini, maliyet/kâr/komisyon/tedarikçi bilgisi dahil
  tam olarak görür. `Default.aspx` ve `Modules/Rezervasyonlar/*` sadece Admin'e
  açıktır (`Core/AuthBase.cs` içindeki `AdminBase` bunu zorunlu kılar — Musteri
  rolü bu sayfalara URL ile doğrudan gitmeye çalışsa bile `MusteriDashboard.aspx`'e
  yönlendirilir).
- **Musteri** — Portalı kullanan acente/müşteri hesapları. Sadece
  `MusteriDashboard.aspx`'i görür ve orada **yalnızca kendi müşteri grubuna**
  ait veriyi görür.

**Çoklu müşteri (tenant) izolasyonu**: Her `Kullanici` kaydının (bizim kendi
`AuthDB`'mizdeki) bir `CustomerGroupId` kolonu vardır. Musteri rolündeki bir
kullanıcı giriş yaptığında bu değer oturuma yüklenir (`SessionManager.CustomerGroupId`)
ve `MusteriRaporRepository` içindeki HER sorgu
`JP_BookingDetail.CustomerId -> JP_Customer.Id -> JP_Customer.CustomerGroupId`
üzerinden bu değere filtrelenir. Admin için bu alan `NULL`'dur ve hiçbir filtre
uygulanmaz.

**Asla göstermeme garantisi**: `Data/MusteriRaporRepository.cs` — Musteri
rolüne sunulan TEK sorgu katmanı — hiçbir zaman `Cost`, `Profit`,
`Commission`/`ComissionAmount` veya tedarikçi (`SupplierName`/`SupplierId`)
kolonlarını SEÇMEZ. Bu sadece arayüzde gizleme değil, sorgunun kendisinde bu
kolonların hiç bulunmamasıdır — yani tarayıcının ağ sekmesinden bakan teknik
bir kullanıcı bile bu verilere ulaşamaz. Bu sınıfa yeni bir rapor eklerken bu
kuralı bozmadığınızdan emin olun. Komisyon görünürlüğü ileride müşteri bazında
esnetilmek istenirse `JP_Customer.SeesCommission` alanı zaten mevcuttur, ancak
şu an için bu portal komisyonu Musteri rolünden koşulsuz gizler.

Yerel test için: `Database/03_TestData.sql` üç örnek acente
(`Acente Madrid`/`Acente Roma` → `CustomerGroupId=100`, `Acente Paris` → `200`,
`Acente Londra` → `300`) ve `Database/01_CreateAuthDatabase.sql` bu gruplardan
birine bağlı örnek bir Musteri kullanıcısı (`musteri.madrid` / `Musteri2026!`)
oluşturur.

## Kurulum

1. **Newtonsoft.Json**: Proje NuGet paket referansı değil `bin\Newtonsoft.Json.dll`
   HintPath'i kullanır (Finaliza ile aynı yaklaşım). Visual Studio'da NuGet ile
   `Newtonsoft.Json` kurup çıkan dll'i `bin\` altına koyun ya da NuGet paket
   referansına çevirin.
2. **AuthDB**: `Database/01_CreateAuthDatabase.sql` scriptini çalıştırın. Bu,
   `BBGFinanceAuth` veritabanını ve içine `admin` kullanıcı adıyla
   (**geçici şifre: `BBGFinance2026!`**) bir yönetici hesabı oluşturur.
   İlk girişten sonra bu şifreyi değiştirin — ayrı bir "şifre değiştir" ekranı
   henüz yoktur; `Core/SessionManager.SifreTuzUret()` ile yeni bir tuz,
   `SessionManager.SifreHashle(yeniSifre, yeniTuz)` ile yeni hash üretip
   `dbo.Kullanici` tablosunda `SifreHash`/`SifreTuz` kolonlarını güncelleyin.
3. **ReportDB**: `Web.config` içindeki `ReportDB` connection string'ini gerçek
   JP_ROIBEDS sunucunuzu gösterecek şekilde düzenleyin. Prodüksiyonda mümkünse
   salt-okunur bir SQL login kullanın.
   - Gerçek JP_ROIBEDS'e erişiminiz yoksa (yerel geliştirme), `Database/02_JPRoibeds_TestSchema.sql`
     ve `Database/03_TestData.sql` scriptleriyle `JP_ROIBEDS_DEV` adında yerel bir
     test veritabanı oluşturup connection string'i ona yönlendirebilirsiniz.
     **Bu iki script gerçek/prodüksiyon JP_ROIBEDS'e karşı çalıştırılmamalıdır.**
4. `Web.config` → `AuthDB` connection string'ini de kendi SQL Server'ınıza göre
   düzenleyin.
5. IIS / IIS Express üzerinden `BBGFinance.sln`'i açıp çalıştırın.

## Varsayımlar ve bilinen sınırlamalar

- `JP_Booking` / `JP_BookingDetail` / `JP_BookingDetailLine` tablolarındaki
  `Status` kolonunun değer kümesi (kod anlamları) paylaşılmadığından, iptal
  tespiti `Status`'a değil kendini açıklayan `CancelDate` (başlık) ve
  `LineCancelled` (kalem) alanlarına göre yapılır.
- Rezervasyon başlığında (`JP_BookingDetail`) doğrudan bir para birimi kolonu
  yoktur; dashboard ve liste ekranlarında bir rezervasyonun para birimi, o
  rezervasyonun ilk kalem satırındaki `SellCurrency` değerinden türetilir.
  Farklı para birimlerindeki tutarlar birbirine çevrilmeden, para birimi
  bazında ayrı ayrı gösterilir (toplamlar asla farklı para birimleri
  arasında karıştırılmaz).
- `Database/02_JPRoibeds_TestSchema.sql` içindeki kolon tipleri (uzunluk/hassasiyet)
  gerçek üretim şemasından alınmadı; sadece kolon adları verildiği için mantıklı
  varsayımlarla belirlendi. Gerçek JP_ROIBEDS'e bağlanıldığında bu script
  kullanılmaz, sadece yerel test içindir.
- DevExtreme bileşenleri CDN üzerinden (deneme/topluluk sürümü) yüklenir;
  üretimde lisanslı bir DevExtreme sürümü kullanılması gerekebilir. Bu CDN'lere
  erişim yoksa (örn. kapalı kurumsal ağ), sayfalar KPI kartları/tablolar gibi
  düz-DOM içerikleri yine gösterir; grafik/grid alanlarında görünür bir uyarı
  çıkar ve konsola hata basılır (bkz. `Default.aspx`/`MusteriDashboard.aspx`
  script'lerindeki `guvenliKur` sarmalayıcısı).
- `JP_BookingDetailLinePaxes.TipPax`: 0 = Yetişkin, 1 = Çocuk, 2 = Bebek.
- "Bölge" raporu `JP_BookingDetailLine.Zonedescription/Zonestate/Zonecountry`
  alanlarına dayanır (otel adres bilgisi değil).
- `JP_Customer.Id`/`CustomerGroupId` gibi ID alanlarının gerçek üretimde INT mi
  yoksa VARCHAR mı olduğu bilinmediğinden, `MusteriRaporRepository`'deki tüm
  JOIN'ler `Data/SqlSafe.JoinEq` ile metin bazlı (tip farkına duyarsız) yapılır.

## Sayfalar

- **Login.aspx** — Giriş ekranı (AuthDB `Kullanici` tablosu, SHA-256 + tuz).
  Admin → `Default.aspx`'e, Musteri → `MusteriDashboard.aspx`'e yönlendirir.
- **Default.aspx** _(Admin)_ — Dashboard: toplam rezervasyon, iptal oranı,
  toplam gece/pax, para birimi bazlı satış/komisyon/bekleyen tahsilat/kâr,
  aylık trend, kanal/pazar/ürün grubu/tedarikçi dağılımları, son rezervasyonlar
  ve yaklaşan konaklamalar.
- **Modules/Rezervasyonlar/Liste.aspx / Detay.aspx** _(Admin)_ — Tüm
  rezervasyonların filtrelenebilir listesi ve maliyet/kâr/tedarikçi dahil tam
  detayı.
- **MusteriDashboard.aspx** _(Musteri)_ — Kendi müşteri grubuna ait: toplam
  rezervasyon/gece/pax, para birimi bazlı satış ve bekleyen tahsilat (komisyon/
  maliyet/kâr YOK), aylık trend, bölge dağılımı, oda tipi dağılımı,
  yetişkin/çocuk/bebek dağılımı, milliyet dağılımı ve girişi henüz gelmemiş
  odalar listesi.
