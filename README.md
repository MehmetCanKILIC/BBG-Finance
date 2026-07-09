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
  Core/                 DbHelper (Auth+Report), SessionManager, AuthBase, AppConstants, JsonHelper
  Data/                 RezervasyonRepository.cs  (JP_ROIBEDS okuma sorguları)
  Modules/Rezervasyonlar/ Liste.aspx (filtrelenebilir liste), Detay.aspx (rezervasyon + kalemler)
  Default.aspx          Dashboard: KPI kartları, trend/kanal/pazar/tedarikçi grafikleri
  Login.aspx            Giriş ekranı
  Database/              Kurulum ve yerel test scriptleri
```

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
  üretimde lisanslı bir DevExtreme sürümü kullanılması gerekebilir.

## Sayfalar

- **Login.aspx** — Giriş ekranı (AuthDB `Kullanici` tablosu, SHA-256 + tuz).
- **Default.aspx** — Dashboard: toplam rezervasyon, iptal oranı, toplam
  gece/pax, para birimi bazlı satış/komisyon/bekleyen tahsilat/kâr, aylık
  trend, kanal/pazar/ürün grubu/tedarikçi dağılımları, son rezervasyonlar ve
  yaklaşan konaklamalar.
- **Modules/Rezervasyonlar/Liste.aspx** — Tarih aralığı, durum (aktif/iptal),
  kanal ve serbest metin aramasıyla filtrelenebilen, sıralanabilir/dışa
  aktarılabilir rezervasyon listesi.
- **Modules/Rezervasyonlar/Detay.aspx** — Bir rezervasyonun başlık bilgileri
  (müşteri, acente, finansal özet) ve kalem (hizmet) satırları.
