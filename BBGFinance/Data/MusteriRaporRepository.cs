using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using BBGFinance.Core;

namespace BBGFinance.Data
{
    /// <summary>
    /// Musteri (acente) rolü için JP_ROIBEDS'e SADECE OKUMA amaçlı erişen katman.
    ///
    /// ÇOK ÖNEMLİ GÜVENLİK KURALI: Bu sınıftaki hiçbir sorgu Cost, Profit, Commission veya
    /// tedarikçi (SupplierName/SupplierId/SupplierCodExport) kolonlarını SEÇMEZ. Bu bilerek
    /// yapılan bir tasarım kararıdır - müşteriler bizim alış fiyatımızı, kârımızı, komisyonumuzu
    /// veya hangi tedarikçiyle çalıştığımızı hiçbir koşulda görmemelidir. Bu sınıfa yeni bir
    /// kolon eklerken bu kuralı ihlal etmediğinizden emin olun.
    ///
    /// Çoklu müşteri (tenant) izolasyonu: her sorgu JP_BookingDetail.CustomerId -> JP_Customer.Id
    /// üzerinden JP_Customer'a bağlanır ve JP_Customer.CustomerGroupId = @CustomerGroupId ile
    /// filtrelenir. customerGroupId parametresi olmadan (NULL) bu sınıftan hiçbir sorgu
    /// çalıştırılmamalıdır - çağıran kod (MusteriDashboard.aspx.cs) bunu her zaman
    /// AuthBase.CustomerGroupId'den (oturumdaki Musteri kullanıcısına ait) almalıdır.
    /// </summary>
    public static class MusteriRaporRepository
    {
        private static string BuildJoin()
        {
            return @"
                FROM dbo.JP_BookingDetail bd
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)";
        }

        public static DataTable GenelOzet(int customerGroupId, DateTime bas, DateTime bit)
        {
            // NOT: OUTER APPLY bir tablo kaynağıdır (FROM/JOIN ile aynı grupta), WHERE'den
            // ÖNCE gelmelidir - bu yüzden burada BuildJoin() (WHERE'i baştan ekler) yerine
            // JOIN'ü elle yazıp WHERE'i en sona koyuyoruz.
            string sql = @"
                SELECT
                    COUNT(DISTINCT bd.BookingCode) AS ToplamRezervasyon,
                    SUM(CASE WHEN ISNULL(satirlar.AktifSatirSayisi, 0) = 0 THEN 1 ELSE 0 END) AS IptalSayisi,
                    ISNULL(SUM(satirlar.ToplamGece), 0) AS ToplamGece,
                    ISNULL(SUM(satirlar.ToplamPax), 0)  AS ToplamPax
                FROM dbo.JP_BookingDetail bd
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                OUTER APPLY (
                    SELECT SUM(ISNULL(" + SqlSafe.Num("l.NightsNumber") + @", 0)) AS ToplamGece,
                           SUM(ISNULL(" + SqlSafe.Num("l.PaxNumber") + @", 0))    AS ToplamPax,
                           SUM(CASE WHEN " + SqlSafe.SatirAktifMi("l.LineCancelledDate") + @" THEN 1 ELSE 0 END) AS AktifSatirSayisi
                    FROM dbo.JP_BookingDetailLine l
                    WHERE l.BookingCode = bd.BookingCode
                ) satirlar
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>Para birimi bazında sadece SATIŞ ve BEKLEYEN TAHSİLAT (kendi ödeme
        /// yükümlülüğü) - komisyon/maliyet/kâr burada YOKTUR.</summary>
        public static DataTable ParaBirimiBazliSatis(int customerGroupId, DateTime bas, DateTime bit)
        {
            string sql = @"
                ;WITH Rez AS (
                    SELECT
                        bd.BookingCode,
                        " + SqlSafe.Num("bd.SellingPrice") + @"      AS SellingPrice,
                        " + SqlSafe.Num("bd.OutStandingAmount") + @" AS OutStandingAmount,
                        (SELECT TOP 1 l.SellCurrency
                         FROM dbo.JP_BookingDetailLine l
                         WHERE l.BookingCode = bd.BookingCode AND l.SellCurrency IS NOT NULL) AS ParaBirimi
                    " + BuildJoin() + @"
                      AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                )
                SELECT
                    ISNULL(ParaBirimi, 'Belirtilmemiş') AS ParaBirimi,
                    SUM(ISNULL(SellingPrice, 0))        AS ToplamSatis,
                    SUM(ISNULL(OutStandingAmount, 0))   AS BekleyenTahsilat
                FROM Rez
                GROUP BY ISNULL(ParaBirimi, 'Belirtilmemiş')
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable AylikTrend(int customerGroupId, DateTime bas, DateTime bit)
        {
            // NOT: BookingDate bazı ortamlarda VARCHAR olarak saklanabildiğinden FORMAT()
            // doğrudan çağrılırsa tip hatası fırlatabilir - önce TRY_CONVERT ile DATETIME'a
            // çevriliyor (geçersiz değerler NULL olur, FORMAT hataya düşmez).
            string sql = @"
                SELECT
                    ISNULL(FORMAT(TRY_CONVERT(DATETIME, bd.BookingDate), 'yyyy-MM'), 'Belirtilmemiş') AS Ay,
                    COUNT(DISTINCT bd.BookingCode) AS RezervasyonSayisi
                " + BuildJoin() + @"
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(FORMAT(TRY_CONVERT(DATETIME, bd.BookingDate), 'yyyy-MM'), 'Belirtilmemiş')
                ORDER BY 1";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>Bölge dağılımı: JP_BookingDetailLine'daki Zone alanları esas alınır.</summary>
        public static DataTable BolgeDagilimi(int customerGroupId, DateTime bas, DateTime bit, int topN = 10)
        {
            string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(NULLIF(LTRIM(RTRIM(l.Zonedescription)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(l.Zonestate)), ''),
                            ISNULL(NULLIF(LTRIM(RTRIM(l.Zonecountry)), ''), 'Belirtilmemiş'))) AS Bolge,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(" + SqlSafe.Num("l.SellingPrice") + @", 0)) AS ToplamSatis
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(l.Zonedescription)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(l.Zonestate)), ''),
                            ISNULL(NULLIF(LTRIM(RTRIM(l.Zonecountry)), ''), 'Belirtilmemiş')))
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>
        /// Oda tipi dağılımı - sadece satış fiyatı, maliyet YOK. Ana kaynak
        /// JP_BookingDetailLine.ProductTypeName'dir (her zaman dolu); JP_BookingDetailLineRoomList
        /// bazı rezervasyonlarda eşleşmediğinden (BookingCode/IdBookLine uyuşmazlığı) sadece
        /// yedek (fallback) olarak LEFT JOIN ile kullanılır.
        /// </summary>
        public static DataTable OdaTipiDagilimi(int customerGroupId, DateTime bas, DateTime bit, int topN = 10)
        {
            string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(NULLIF(LTRIM(RTRIM(l.ProductTypeName)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(rl.typeroomname)), ''), 'Belirtilmemiş')) AS OdaTipi,
                    COUNT(*) AS OdaSayisi,
                    SUM(ISNULL(" + SqlSafe.Num("l.SellingPrice") + @", ISNULL(" + SqlSafe.Num("rl.priceroom") + @", 0))) AS ToplamSatis
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                LEFT JOIN dbo.JP_BookingDetailLineRoomList rl
                    ON " + SqlSafe.JoinEq("rl.BookingCode", "l.BookingCode") + @"
                   AND " + SqlSafe.JoinEq("rl.IdBookLine", "l.IdBookLine") + @"
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(l.ProductTypeName)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(rl.typeroomname)), ''), 'Belirtilmemiş'))
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>Yetişkin/Çocuk/Bebek dağılımı. TipPax: 0=Adult, 1=Child, 2=Baby.</summary>
        public static DataTable YasGrubuDagilimi(int customerGroupId, DateTime bas, DateTime bit)
        {
            string sql = @"
                SELECT
                    CASE LTRIM(RTRIM(CONVERT(NVARCHAR(10), px.TipPax)))
                        WHEN '0' THEN 'Yetişkin'
                        WHEN '1' THEN 'Çocuk'
                        WHEN '2' THEN 'Bebek'
                        ELSE 'Belirtilmemiş'
                    END AS YasGrubu,
                    COUNT(*) AS Adet
                FROM dbo.JP_BookingDetailLinePaxes px
                INNER JOIN dbo.JP_BookingDetail bd ON " + SqlSafe.JoinEq("bd.BookingCode", "px.BookingCode") + @"
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY CASE LTRIM(RTRIM(CONVERT(NVARCHAR(10), px.TipPax)))
                        WHEN '0' THEN 'Yetişkin'
                        WHEN '1' THEN 'Çocuk'
                        WHEN '2' THEN 'Bebek'
                        ELSE 'Belirtilmemiş'
                    END
                ORDER BY Adet DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>
        /// Milliyet dağılımı - JP_BookingDetail.HolderNacionalidad'a göre (rezervasyon/tutan kişi
        /// bazında). JP_BookingDetailLinePaxes.Country pratikte boş çıktığından kullanılmıyor.
        /// </summary>
        public static DataTable MilliyetDagilimi(int customerGroupId, DateTime bas, DateTime bit, int topN = 10)
        {
            string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(NULLIF(LTRIM(RTRIM(bd.HolderNacionalidad)), ''), 'Belirtilmemiş') AS Milliyet,
                    COUNT(DISTINCT bd.BookingCode) AS Adet
                FROM dbo.JP_BookingDetail bd
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(NULLIF(LTRIM(RTRIM(bd.HolderNacionalidad)), ''), 'Belirtilmemiş')
                ORDER BY Adet DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>
        /// Girişi henüz gelmemiş (bekleyen) odalar - otel/oda tipi/giriş tarihi ile. Otel adı ve
        /// oda tipi öncelikle JP_BookingDetailLine.ServiceName/ProductTypeName'den alınır (her
        /// zaman dolu); JP_BookingDetailLineRoomList bazı rezervasyonlarda eşleşmediğinden
        /// (BookingCode/IdBookLine uyuşmazlığı) sadece yedek (fallback) ve misafir adı için kullanılır.
        /// Dashboard'daki tarih filtresine göre bd.BookingDate (satış tarihi) aralığıyla da
        /// sınırlanır - bu sayede tarih filtresi değiştirildiğinde bu tablo da güncellenir.
        /// </summary>
        public static DataTable BekleyenGirisler(int customerGroupId, DateTime bas, DateTime bit, int topN = 20)
        {
            string sql = @"
                SELECT TOP (@TopN)
                    bd.BookingCode,
                    ISNULL(NULLIF(LTRIM(RTRIM(l.ServiceName)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(rl.namehotel)), ''), '')) AS OtelAdi,
                    ISNULL(NULLIF(LTRIM(RTRIM(l.ProductTypeName)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(rl.typeroomname)), ''), '')) AS OdaTipi,
                    LTRIM(RTRIM(ISNULL(rl.name, '') + ' ' + ISNULL(rl.lastname, ''))) AS MisafirAdi,
                    l.BeginTravelDate,
                    l.EndTravelDate,
                    " + SqlSafe.Num("l.NightsNumber") + @" AS NightsNumber,
                    " + SqlSafe.Num("l.PaxNumber") + @"    AS PaxNumber
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                LEFT JOIN dbo.JP_BookingDetailLineRoomList rl
                    ON " + SqlSafe.JoinEq("rl.BookingCode", "l.BookingCode") + @"
                   AND " + SqlSafe.JoinEq("rl.IdBookLine", "l.IdBookLine") + @"
                WHERE " + SqlSafe.Txt("c.CustomerGroupId") + @" = CONVERT(NVARCHAR(50), @CustomerGroupId)
                  AND bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                  AND l.BeginTravelDate >= CAST(GETDATE() AS DATE)
                  AND " + SqlSafe.SatirAktifMi("l.LineCancelledDate") + @"
                ORDER BY l.BeginTravelDate ASC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit),
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@CustomerGroupId", customerGroupId));
        }

        /// <summary>
        /// "My Reservations" - müşteriye özel, satır (otel/kalem) bazlı liste. Cost/Profit/
        /// Commission/Tedarikçi burada da YOKTUR - sadece satış tutarı/para birimi vardır.
        /// Otel adı öncelikle JP_BookingDetailLine.ServiceName'den alınır (bkz. OdaTipiDagilimi/
        /// BekleyenGirisler'deki aynı gerekçe). Adults sayısı booking bazında JP_BookingDetailLinePaxes
        /// üzerinden hesaplanır (Paxes tablosu satır bazında değil, booking bazında ilişkilidir).
        /// </summary>
        public static DataTable RezervasyonListesi(
            int customerGroupId, DateTime? bas, DateTime? bit, string durum, string arama, int maxSatir = 2000)
        {
            var parametreler = new List<SqlParameter>();
            var where = new StringBuilder(" WHERE " + SqlSafe.Txt("c.CustomerGroupId") + " = @CustomerGroupId ");
            parametreler.Add(ReportDbHelper.Param("@CustomerGroupId", customerGroupId));

            if (bas.HasValue)
            {
                where.Append(" AND bd.BookingDate >= @Bas ");
                parametreler.Add(ReportDbHelper.Param("@Bas", bas.Value));
            }
            if (bit.HasValue)
            {
                where.Append(" AND bd.BookingDate < @Bit ");
                parametreler.Add(ReportDbHelper.Param("@Bit", bit.Value.AddDays(1)));
            }
            if (durum == "Aktif")
                where.Append(" AND " + SqlSafe.SatirAktifMi("l.LineCancelledDate") + " ");
            else if (durum == "Iptal")
                where.Append(" AND NOT " + SqlSafe.SatirAktifMi("l.LineCancelledDate") + " ");

            if (!string.IsNullOrEmpty(arama))
            {
                where.Append(@" AND (bd.BookingCode LIKE @Arama OR l.ServiceName LIKE @Arama) ");
                parametreler.Add(ReportDbHelper.Param("@Arama", "%" + arama + "%"));
            }

            string sql = @"
                SELECT TOP (" + maxSatir + @")
                    bd.BookingCode,
                    bd.BookingDate AS SaleDate,
                    CASE WHEN " + SqlSafe.SatirAktifMi("l.LineCancelledDate") + @" THEN 'Aktif' ELSE 'Iptal' END AS Status,
                    ISNULL(NULLIF(LTRIM(RTRIM(l.ServiceName)), ''), 'Belirtilmemiş') AS HotelName,
                    ISNULL(NULLIF(LTRIM(RTRIM(l.Zonedescription)), ''),
                        ISNULL(NULLIF(LTRIM(RTRIM(l.Zonestate)), ''),
                            ISNULL(NULLIF(LTRIM(RTRIM(l.Zonecountry)), ''), 'Belirtilmemiş'))) AS Region,
                    l.BeginTravelDate AS CheckIn,
                    l.EndTravelDate AS CheckOut,
                    " + SqlSafe.Num("l.NightsNumber") + @" AS Nights,
                    " + SqlSafe.Num("l.PaxNumber") + @"    AS Pax,
                    ISNULL((SELECT COUNT(*) FROM dbo.JP_BookingDetailLinePaxes px
                            WHERE px.BookingCode = l.BookingCode
                              AND LTRIM(RTRIM(CONVERT(NVARCHAR(10), px.TipPax))) = '0'), 0) AS Adults,
                    ISNULL(NULLIF(LTRIM(RTRIM(bd.HolderNacionalidad)), ''), 'Belirtilmemiş') AS Nationality,
                    " + SqlSafe.Num("l.SellingPrice") + @" AS SellingPrice,
                    ISNULL(l.SellCurrency, '') AS Currency
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                INNER JOIN dbo.JP_Customer c ON " + SqlSafe.JoinEq("bd.CustomerId", "c.Id") + @"
                " + where + @"
                ORDER BY bd.BookingDate DESC, l.BeginTravelDate DESC";

            return ReportDbHelper.ExecuteQuery(sql, parametreler.ToArray());
        }
    }
}
