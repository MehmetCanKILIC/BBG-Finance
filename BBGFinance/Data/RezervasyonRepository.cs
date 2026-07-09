using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using BBGFinance.Core;

namespace BBGFinance.Data
{
    /// <summary>
    /// JP_ROIBEDS üzerindeki JP_Booking / JP_BookingDetail / JP_BookingDetailLine tablolarına
    /// SADECE OKUMA amaçlı erişen katman. Bu sınıf hiçbir yazma işlemi yapmaz.
    ///
    /// JP_BookingDetail = rezervasyon başlığı (bir BookingCode = bir rezervasyon).
    /// JP_BookingDetailLine = rezervasyon kalemleri (otel/hizmet satırları, fiyat/kar/tedarikçi bilgisi).
    ///
    /// Status kolonunun değer kümesi (kod anlamları) bilinmediğinden iptal tespiti bu katmanda
    /// Status'a değil, kendini açıklayan CancelDate (başlık) ve LineCancelled (kalem) alanlarına
    /// göre yapılır.
    /// </summary>
    public static class RezervasyonRepository
    {
        // ---------------------------------------------------------------
        // GENEL ÖZET (dashboard KPI kartları)
        // ---------------------------------------------------------------

        public static DataTable GenelOzet(DateTime bas, DateTime bit)
        {
            const string sql = @"
                SELECT
                    COUNT(DISTINCT bd.BookingCode) AS ToplamRezervasyon,
                    SUM(CASE WHEN bd.CancelDate IS NOT NULL THEN 1 ELSE 0 END) AS IptalSayisi,
                    ISNULL(SUM(satirlar.ToplamGece), 0) AS ToplamGece,
                    ISNULL(SUM(satirlar.ToplamPax), 0)  AS ToplamPax
                FROM dbo.JP_BookingDetail bd
                OUTER APPLY (
                    SELECT SUM(ISNULL(l.NightsNumber, 0)) AS ToplamGece,
                           SUM(ISNULL(l.PaxNumber, 0))    AS ToplamPax
                    FROM dbo.JP_BookingDetailLine l
                    WHERE l.BookingCode = bd.BookingCode
                ) satirlar
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        /// <summary>
        /// Rezervasyon başlığındaki tutarlar (satış/komisyon/bekleyen tahsilat) para birimi
        /// bazında gruplanır; başlıkta doğrudan para birimi kolonu olmadığından her rezervasyonun
        /// ilk kalem satırındaki SellCurrency'si o rezervasyonun para birimi kabul edilir.
        /// </summary>
        public static DataTable ParaBirimiBazliTutarlar(DateTime bas, DateTime bit)
        {
            const string sql = @"
                ;WITH BookingParaBirimi AS (
                    SELECT
                        bd.BookingCode,
                        bd.SellingPrice,
                        bd.Commission,
                        bd.OutStandingAmount,
                        bd.Invoiced,
                        (SELECT TOP 1 l.SellCurrency
                         FROM dbo.JP_BookingDetailLine l
                         WHERE l.BookingCode = bd.BookingCode AND l.SellCurrency IS NOT NULL) AS ParaBirimi
                    FROM dbo.JP_BookingDetail bd
                    WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                )
                SELECT
                    ISNULL(ParaBirimi, 'Belirtilmemiş') AS ParaBirimi,
                    SUM(ISNULL(SellingPrice, 0))        AS ToplamSatis,
                    SUM(ISNULL(Commission, 0))          AS ToplamKomisyon,
                    SUM(CASE WHEN ISNULL(Invoiced, 0) = 0 THEN ISNULL(OutStandingAmount, 0) ELSE 0 END) AS BekleyenTahsilat
                FROM BookingParaBirimi
                GROUP BY ISNULL(ParaBirimi, 'Belirtilmemiş')
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable ParaBirimiBazliKar(DateTime bas, DateTime bit)
        {
            const string sql = @"
                SELECT
                    ISNULL(l.SellCurrency, 'Belirtilmemiş') AS ParaBirimi,
                    SUM(ISNULL(l.Profit, 0)) AS ToplamKar
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(l.SellCurrency, 'Belirtilmemiş')
                ORDER BY ToplamKar DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        // ---------------------------------------------------------------
        // TRENDLER VE DAĞILIMLAR
        // ---------------------------------------------------------------

        public static DataTable AylikTrend(DateTime bas, DateTime bit)
        {
            const string sql = @"
                SELECT
                    FORMAT(bd.BookingDate, 'yyyy-MM') AS Ay,
                    COUNT(DISTINCT bd.BookingCode) AS RezervasyonSayisi,
                    SUM(CASE WHEN bd.CancelDate IS NOT NULL THEN 1 ELSE 0 END) AS IptalSayisi
                FROM dbo.JP_BookingDetail bd
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY FORMAT(bd.BookingDate, 'yyyy-MM')
                ORDER BY 1";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable KanalDagilimi(DateTime bas, DateTime bit)
        {
            const string sql = @"
                SELECT
                    ISNULL(bd.Channel, 'Belirtilmemiş') AS Kanal,
                    COUNT(DISTINCT bd.BookingCode) AS Adet
                FROM dbo.JP_BookingDetail bd
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(bd.Channel, 'Belirtilmemiş')
                ORDER BY Adet DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable UrunGrubuDagilimi(DateTime bas, DateTime bit, int topN = 10)
        {
            const string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(l.ProductGroupName, ISNULL(l.ProductGroup, 'Belirtilmemiş')) AS UrunGrubu,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(l.SellingPrice, 0)) AS ToplamSatis
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(l.ProductGroupName, ISNULL(l.ProductGroup, 'Belirtilmemiş'))
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable PazarDagilimi(DateTime bas, DateTime bit, int topN = 10)
        {
            const string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(l.Market, 'Belirtilmemiş') AS Pazar,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(l.SellingPrice, 0)) AS ToplamSatis
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(l.Market, 'Belirtilmemiş')
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable TedarikciDagilimi(DateTime bas, DateTime bit, int topN = 10)
        {
            const string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(l.SupplierName, 'Belirtilmemiş') AS Tedarikci,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(l.SellingPrice, 0)) AS ToplamSatis,
                    SUM(ISNULL(l.Profit, 0)) AS ToplamKar
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                WHERE bd.BookingDate >= @Bas AND bd.BookingDate < @Bit
                GROUP BY ISNULL(l.SupplierName, 'Belirtilmemiş')
                ORDER BY ToplamSatis DESC";

            return ReportDbHelper.ExecuteQuery(sql,
                ReportDbHelper.Param("@TopN", topN),
                ReportDbHelper.Param("@Bas", bas),
                ReportDbHelper.Param("@Bit", bit));
        }

        public static DataTable SonRezervasyonlar(int topN = 10)
        {
            const string sql = @"
                SELECT TOP (@TopN)
                    bd.BookingCode,
                    bd.BookingDate,
                    ISNULL(bd.CustomerName, '') AS CustomerName,
                    ISNULL(bd.Channel, '') AS Channel,
                    bd.SellingPrice,
                    (SELECT TOP 1 l.SellCurrency FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode AND l.SellCurrency IS NOT NULL) AS ParaBirimi,
                    CASE WHEN bd.CancelDate IS NOT NULL THEN 1 ELSE 0 END AS IptalMi
                FROM dbo.JP_BookingDetail bd
                ORDER BY bd.BookingDate DESC";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@TopN", topN));
        }

        public static DataTable YaklasanKonaklamalar(int topN = 10)
        {
            const string sql = @"
                SELECT TOP (@TopN)
                    l.BookingCode,
                    ISNULL(bd.CustomerName, '') AS CustomerName,
                    ISNULL(l.ServiceName, '') AS ServiceName,
                    l.BeginTravelDate,
                    l.EndTravelDate,
                    l.NightsNumber,
                    l.PaxNumber
                FROM dbo.JP_BookingDetailLine l
                INNER JOIN dbo.JP_BookingDetail bd ON bd.BookingCode = l.BookingCode
                WHERE l.BeginTravelDate >= CAST(GETDATE() AS DATE)
                  AND ISNULL(l.LineCancelled, 0) = 0
                  AND bd.CancelDate IS NULL
                ORDER BY l.BeginTravelDate ASC";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@TopN", topN));
        }

        // ---------------------------------------------------------------
        // REZERVASYON LİSTESİ (filtrelenebilir grid)
        // ---------------------------------------------------------------

        public static DataTable RezervasyonListesi(
            DateTime? bas, DateTime? bit, string durum, string kanal, string arama, int maxSatir = 5000)
        {
            var parametreler = new List<SqlParameter>();
            var where = new StringBuilder(" WHERE 1=1 ");

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
                where.Append(" AND bd.CancelDate IS NULL ");
            else if (durum == "Iptal")
                where.Append(" AND bd.CancelDate IS NOT NULL ");

            if (!string.IsNullOrEmpty(kanal))
            {
                where.Append(" AND bd.Channel = @Kanal ");
                parametreler.Add(ReportDbHelper.Param("@Kanal", kanal));
            }

            if (!string.IsNullOrEmpty(arama))
            {
                where.Append(@" AND (bd.BookingCode LIKE @Arama OR bd.CustomerName LIKE @Arama
                                     OR bd.AgentName LIKE @Arama OR bd.CustomerEmail LIKE @Arama) ");
                parametreler.Add(ReportDbHelper.Param("@Arama", "%" + arama + "%"));
            }

            string sql = @"
                SELECT TOP (" + maxSatir + @")
                    bd.Id,
                    bd.BookingCode,
                    bd.BookingDate,
                    CASE WHEN bd.CancelDate IS NOT NULL THEN 'Iptal' ELSE 'Aktif' END AS Durum,
                    ISNULL(bd.CustomerName, '') AS CustomerName,
                    ISNULL(bd.AgentName, '')    AS AgentName,
                    ISNULL(bd.Channel, '')      AS Channel,
                    bd.SellingPrice,
                    bd.Commission,
                    bd.OutStandingAmount,
                    ISNULL(bd.Invoiced, 0) AS Invoiced,
                    (SELECT TOP 1 l.SellCurrency FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode AND l.SellCurrency IS NOT NULL) AS ParaBirimi,
                    (SELECT SUM(ISNULL(l.NightsNumber,0)) FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode) AS ToplamGece,
                    (SELECT SUM(ISNULL(l.PaxNumber,0)) FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode) AS ToplamPax
                FROM dbo.JP_BookingDetail bd
                " + where + @"
                ORDER BY bd.BookingDate DESC";

            return ReportDbHelper.ExecuteQuery(sql, parametreler.ToArray());
        }

        public static DataTable RezervasyonKanallari()
        {
            const string sql = @"
                SELECT DISTINCT ISNULL(Channel,'Belirtilmemiş') AS Kanal
                FROM dbo.JP_BookingDetail
                WHERE Channel IS NOT NULL
                ORDER BY Kanal";

            return ReportDbHelper.ExecuteQuery(sql);
        }

        // ---------------------------------------------------------------
        // REZERVASYON DETAYI
        // ---------------------------------------------------------------

        public static DataTable RezervasyonBasligi(string bookingCode)
        {
            const string sql = @"
                SELECT bd.*
                FROM dbo.JP_BookingDetail bd
                WHERE bd.BookingCode = @BookingCode";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@BookingCode", bookingCode));
        }

        public static DataTable RezervasyonKalemleri(string bookingCode)
        {
            const string sql = @"
                SELECT l.*
                FROM dbo.JP_BookingDetailLine l
                WHERE l.BookingCode = @BookingCode
                ORDER BY l.IdBookLine";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@BookingCode", bookingCode));
        }
    }
}
