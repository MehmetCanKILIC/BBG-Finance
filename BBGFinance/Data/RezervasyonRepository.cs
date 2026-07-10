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
    ///
    /// JP_ROIBEDS'te sayısal görünümlü alanlar (tutar/adet/gece vb.) VARCHAR olarak tutulur;
    /// bu yüzden toplama (SUM/AVG) veya sayısal karşılaştırma öncesinde <see cref="Num"/> ile
    /// FLOAT'a çevrilirler. TRY_CONVERT kullanılır ki boş/boşluk/sayı olmayan bir değer hata
    /// fırlatmak yerine NULL'a düşsün.
    /// </summary>
    public static class RezervasyonRepository
    {
        /// <summary>VARCHAR sayısal alanları güvenli biçimde FLOAT'a çeviren SQL ifadesi üretir.</summary>
        private static string Num(string kolonIfadesi)
        {
            return "TRY_CONVERT(FLOAT, NULLIF(LTRIM(RTRIM(" + kolonIfadesi + ")), ''))";
        }

        // ---------------------------------------------------------------
        // GENEL ÖZET (dashboard KPI kartları)
        // ---------------------------------------------------------------

        public static DataTable GenelOzet(DateTime bas, DateTime bit)
        {
            string sql = @"
                SELECT
                    COUNT(DISTINCT bd.BookingCode) AS ToplamRezervasyon,
                    SUM(CASE WHEN bd.CancelDate IS NOT NULL THEN 1 ELSE 0 END) AS IptalSayisi,
                    ISNULL(SUM(satirlar.ToplamGece), 0) AS ToplamGece,
                    ISNULL(SUM(satirlar.ToplamPax), 0)  AS ToplamPax
                FROM dbo.JP_BookingDetail bd
                OUTER APPLY (
                    SELECT SUM(ISNULL(" + Num("l.NightsNumber") + @", 0)) AS ToplamGece,
                           SUM(ISNULL(" + Num("l.PaxNumber") + @", 0))    AS ToplamPax
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
            string sql = @"
                ;WITH BookingParaBirimi AS (
                    SELECT
                        bd.BookingCode,
                        " + Num("bd.SellingPrice") + @"      AS SellingPrice,
                        " + Num("bd.Commission") + @"        AS Commission,
                        " + Num("bd.OutStandingAmount") + @" AS OutStandingAmount,
                        " + Num("bd.Invoiced") + @"          AS Invoiced,
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
            string sql = @"
                SELECT
                    ISNULL(l.SellCurrency, 'Belirtilmemiş') AS ParaBirimi,
                    SUM(ISNULL(" + Num("l.Profit") + @", 0)) AS ToplamKar
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
            string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(l.ProductGroupName, ISNULL(l.ProductGroup, 'Belirtilmemiş')) AS UrunGrubu,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(" + Num("l.SellingPrice") + @", 0)) AS ToplamSatis
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
            string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(l.Market, 'Belirtilmemiş') AS Pazar,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(" + Num("l.SellingPrice") + @", 0)) AS ToplamSatis
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
            string sql = @"
                SELECT TOP (@TopN)
                    ISNULL(l.SupplierName, 'Belirtilmemiş') AS Tedarikci,
                    COUNT(*) AS KalemSayisi,
                    SUM(ISNULL(" + Num("l.SellingPrice") + @", 0)) AS ToplamSatis,
                    SUM(ISNULL(" + Num("l.Profit") + @", 0)) AS ToplamKar
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
            string sql = @"
                SELECT TOP (@TopN)
                    bd.BookingCode,
                    bd.BookingDate,
                    ISNULL(bd.CustomerName, '') AS CustomerName,
                    ISNULL(bd.Channel, '') AS Channel,
                    " + Num("bd.SellingPrice") + @" AS SellingPrice,
                    (SELECT TOP 1 l.SellCurrency FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode AND l.SellCurrency IS NOT NULL) AS ParaBirimi,
                    CASE WHEN bd.CancelDate IS NOT NULL THEN 1 ELSE 0 END AS IptalMi
                FROM dbo.JP_BookingDetail bd
                ORDER BY bd.BookingDate DESC";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@TopN", topN));
        }

        public static DataTable YaklasanKonaklamalar(int topN = 10)
        {
            string sql = @"
                SELECT TOP (@TopN)
                    l.BookingCode,
                    ISNULL(bd.CustomerName, '') AS CustomerName,
                    ISNULL(l.ServiceName, '') AS ServiceName,
                    l.BeginTravelDate,
                    l.EndTravelDate,
                    " + Num("l.NightsNumber") + @" AS NightsNumber,
                    " + Num("l.PaxNumber") + @"    AS PaxNumber
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
                    " + Num("bd.SellingPrice") + @"      AS SellingPrice,
                    " + Num("bd.Commission") + @"        AS Commission,
                    " + Num("bd.OutStandingAmount") + @" AS OutStandingAmount,
                    ISNULL(" + Num("bd.Invoiced") + @", 0) AS Invoiced,
                    (SELECT TOP 1 l.SellCurrency FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode AND l.SellCurrency IS NOT NULL) AS ParaBirimi,
                    (SELECT SUM(ISNULL(" + Num("l.NightsNumber") + @",0)) FROM dbo.JP_BookingDetailLine l
                     WHERE l.BookingCode = bd.BookingCode) AS ToplamGece,
                    (SELECT SUM(ISNULL(" + Num("l.PaxNumber") + @",0)) FROM dbo.JP_BookingDetailLine l
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
            // SELECT * yerine açık kolon listesi: sayısal alanlar FLOAT'a çevrilir,
            // geri kalan metin/tarih alanları olduğu gibi döner.
            string sql = @"
                SELECT
                    bd.Logicalref, bd.Id, bd.Status, bd.CancelDate,
                    bd.tcNumber, bd.tcAccountNumber,
                    " + Num("bd.tcPointsAmount") + @" AS tcPointsAmount,
                    bd.BookingLabel, bd.InvoiceFinalCustomer,
                    bd.BookingDate, bd.TimeLimit, bd.BookingCode, bd.Channel, bd.LastModifiedDate,
                    bd.AgencyRef, bd.FinalCustomerId, bd.timeZone,
                    " + Num("bd.SellingPrice") + @" AS SellingPrice,
                    bd.Description,
                    " + Num("bd.Cost") + @"       AS Cost,
                    " + Num("bd.Commission") + @" AS Commission,
                    " + Num("bd.OutStandingAmount") + @" AS OutStandingAmount,
                    bd.Invoiced, bd.Remarks, bd.InRemarks, bd.FinancialNotes,
                    bd.BookingAdmin, bd.AccountManager,
                    bd.CustomerId, bd.Customercodcli, bd.CustomerName,
                    bd.CustomerPhone1, bd.CustomerPhone2, bd.CustomerMobile, bd.CustomerFax, bd.CustomerEmail,
                    bd.CustomerAddress, bd.CustomerAddressNumber, bd.CustomerAddressBuilding,
                    bd.CustomerBranchOffice, bd.CustomerCountry, bd.CustomerCIF, bd.CustomerCity, bd.CustomerClientAccount,
                    bd.AgentId, bd.AgentName, bd.AgentEmail, bd.AgentEmailAgent, bd.AgentTaxID,
                    bd.IdTransaction, bd.PaymentTypeCreditCardType, bd.PaymentType,
                    bd.NameHolder, bd.LastName, bd.HolderCity, bd.HolderCountry, bd.HolderAddress,
                    bd.HolderPhone1, bd.HolderPhone2, bd.HolderPhone3, bd.HolderFax, bd.HolderEmail,
                    bd.HolderIdioma, bd.HolderTipoDocumento, bd.HolderDni, bd.HolderNacionalidadISO2, bd.HolderNacionalidad
                FROM dbo.JP_BookingDetail bd
                WHERE bd.BookingCode = @BookingCode";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@BookingCode", bookingCode));
        }

        public static DataTable RezervasyonKalemleri(string bookingCode)
        {
            // SELECT * yerine açık kolon listesi: sayısal alanlar FLOAT'a çevrilir,
            // geri kalan metin/tarih alanları olduğu gibi döner.
            string sql = @"
                SELECT
                    l.Logicalref, l.IdBookLine, l.BookingCode, l.Status, l.LineDate,
                    l.LineCancelled, l.LineCancelledDate,
                    " + Num("l.LineMarkup") + @" AS LineMarkup,
                    l.Externalreference, l.ExternalClientBookingNo, l.DirectPayment, l.PaymentAtDestination,
                    l.NumPackage, l.NonRefundable, l.LineCancellationChargesDate, l.SupplierLocator, l.Blocked,
                    l.RelatedBookingLine, l.ServiceName, l.ProductType, l.ProductTypeName, l.ProductGroup,
                    l.ProductCodExport, l.ProductGroupAnalyticCode, l.DepartmentGroup, l.DepartmentGroupAnalyticCode,
                    l.Market, l.AgencyGroupID, l.AgencyGroupName, l.ProductGroupName, l.Productid,
                    l.SaleCompanyId, l.SaleCompanyCodExt, l.SaleCompanyName, l.SaleCompanyCountry,
                    l.PuchasingCompanyId, l.PuchasingCompanyCodExt, l.PuchasingCompanyName, l.PuchasingCompanyCountry,
                    l.SupplierId, l.SupplierName, l.SupplierCodExport,
                    " + Num("l.SellingPrice") + @"          AS SellingPrice,
                    " + Num("l.Commission") + @"            AS Commission,
                    " + Num("l.PerCommission") + @"         AS PerCommission,
                    l.CostCurrency, l.SellCurrency,
                    " + Num("l.CostBaseLine") + @"          AS CostBaseLine,
                    " + Num("l.TaxCostNotIncluded") + @"    AS TaxCostNotIncluded,
                    " + Num("l.CostCancellationFees") + @"  AS CostCancellationFees,
                    " + Num("l.NetCostLine") + @"           AS NetCostLine,
                    " + Num("l.ComissionAmount") + @"       AS ComissionAmount,
                    " + Num("l.ComissionTaxPercent") + @"   AS ComissionTaxPercent,
                    " + Num("l.ComissionPercent") + @"      AS ComissionPercent,
                    " + Num("l.CommisionTaxAmount") + @"    AS CommisionTaxAmount,
                    " + Num("l.IndirectCommissionPercent") + @" AS IndirectCommissionPercent,
                    " + Num("l.IndirectCommissionFix") + @"     AS IndirectCommissionFix,
                    " + Num("l.IndirectCommissionAmount") + @"  AS IndirectCommissionAmount,
                    l.IndirectCommissionSettled,
                    " + Num("l.Profit") + @"                AS Profit,
                    " + Num("l.ProfitTaxtNotIncluded") + @"  AS ProfitTaxtNotIncluded,
                    l.SerialERP, l.Remarks, l.PromotionCode,
                    " + Num("l.BasePriceCommission") + @"   AS BasePriceCommission,
                    " + Num("l.CustomerCommission") + @"    AS CustomerCommission,
                    " + Num("l.BasePriceWithOutTax") + @"   AS BasePriceWithOutTax,
                    " + Num("l.BasePrice") + @"             AS BasePrice,
                    " + Num("l.TaxPriceNotIncluded") + @"   AS TaxPriceNotIncluded,
                    " + Num("l.CancellationFees") + @"      AS CancellationFees,
                    " + Num("l.BaseChangeFactor") + @"      AS BaseChangeFactor,
                    " + Num("l.CostChangeFactor") + @"      AS CostChangeFactor,
                    l.ZoneId, l.Zonedescription, l.Zonestate, l.Zonecountry,
                    l.BeginTravelDate, l.EndTravelDate, l.ExternalSupplierConfirmationNumber, l.ProviderAccount,
                    " + Num("l.PaxNumber") + @"     AS PaxNumber,
                    " + Num("l.NightsNumber") + @"  AS NightsNumber,
                    l.FlightDetails, l.Category, l.isExtranet, l.VirtualCreditCardPayment, l.HotelRemarks
                FROM dbo.JP_BookingDetailLine l
                WHERE l.BookingCode = @BookingCode
                ORDER BY l.IdBookLine";

            return ReportDbHelper.ExecuteQuery(sql, ReportDbHelper.Param("@BookingCode", bookingCode));
        }
    }
}
