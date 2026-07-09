-- ============================================================
-- JP_ROIBEDS - YEREL TEST/GELİŞTİRME ŞEMASI
--
-- ÖNEMLİ: JP_ROIBEDS gerçek (üretim) veritabanı zaten mevcuttur ve bu
-- portal ona SADECE OKUMA amaçlı bağlanır. Bu script üretim JP_ROIBEDS
-- üzerinde ÇALIŞTIRILMAMALIDIR.
--
-- Bu script sadece; gerçek JP_ROIBEDS'e erişimi olmayan bir geliştirme
-- makinesinde, portalı yerel bir SQL Server'da test edebilmek için
-- JP_Booking / JP_BookingDetail / JP_BookingDetailLine tablolarının
-- sağlanan kolon listesine göre bir kopyasını oluşturur. Kolon tipleri
-- gerçek üretim şemasından alınmadı (paylaşılmadı); mantıklı varsayımlarla
-- belirlendi - gerçek tiplerle farklılık gösterebilir.
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'JP_ROIBEDS_DEV')
BEGIN
    CREATE DATABASE JP_ROIBEDS_DEV;
END
GO

USE JP_ROIBEDS_DEV;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.JP_Booking') AND type = 'U')
CREATE TABLE dbo.JP_Booking (
    Logicalref          INT IDENTITY(1,1) PRIMARY KEY,
    Id                   INT              NULL,
    Status               NVARCHAR(50)     NULL,
    BookingCode          NVARCHAR(50)     NOT NULL,
    BookingDate          DATETIME         NOT NULL,
    LastModifiedDate     DATETIME         NULL,
    AgencyRef            NVARCHAR(50)     NULL,
    TimeZone             NVARCHAR(50)     NULL
);
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.JP_BookingDetail') AND type = 'U')
CREATE TABLE dbo.JP_BookingDetail (
    Logicalref                  INT IDENTITY(1,1) PRIMARY KEY,
    Id                          INT             NULL,
    Status                      NVARCHAR(50)    NULL,
    CancelDate                  DATETIME        NULL,
    tcNumber                    NVARCHAR(50)    NULL,
    tcAccountNumber             NVARCHAR(50)    NULL,
    tcPointsAmount               DECIMAL(18,2)   NULL,
    BookingLabel                NVARCHAR(100)   NULL,
    InvoiceFinalCustomer        BIT             NULL,
    BookingDate                 DATETIME        NOT NULL,
    TimeLimit                   DATETIME        NULL,
    BookingCode                 NVARCHAR(50)    NOT NULL UNIQUE,
    Channel                     NVARCHAR(50)    NULL,
    LastModifiedDate             DATETIME        NULL,
    AgencyRef                   NVARCHAR(50)    NULL,
    FinalCustomerId              NVARCHAR(50)    NULL,
    timeZone                    NVARCHAR(50)    NULL,
    SellingPrice                DECIMAL(18,2)   NULL,
    Description                 NVARCHAR(1000)  NULL,
    Cost                        DECIMAL(18,2)   NULL,
    Commission                  DECIMAL(18,2)   NULL,
    OutStandingAmount            DECIMAL(18,2)   NULL,
    Invoiced                    BIT             NULL,
    Remarks                      NVARCHAR(1000)  NULL,
    InRemarks                   NVARCHAR(1000)  NULL,
    FinancialNotes               NVARCHAR(1000)  NULL,
    BookingAdmin                 NVARCHAR(100)   NULL,
    AccountManager               NVARCHAR(100)   NULL,
    CustomerId                   NVARCHAR(50)    NULL,
    Customercodcli               NVARCHAR(50)    NULL,
    CustomerName                 NVARCHAR(200)   NULL,
    CustomerPhone1                NVARCHAR(50)    NULL,
    CustomerPhone2                NVARCHAR(50)    NULL,
    CustomerMobile                NVARCHAR(50)    NULL,
    CustomerFax                   NVARCHAR(50)    NULL,
    CustomerEmail                 NVARCHAR(150)   NULL,
    CustomerAddress               NVARCHAR(300)   NULL,
    CustomerAddressNumber          NVARCHAR(20)    NULL,
    CustomerAddressBuilding        NVARCHAR(100)   NULL,
    CustomerBranchOffice           NVARCHAR(100)   NULL,
    CustomerCountry                NVARCHAR(100)   NULL,
    CustomerCIF                    NVARCHAR(50)    NULL,
    CustomerCity                   NVARCHAR(100)   NULL,
    CustomerClientAccount           NVARCHAR(50)    NULL,
    AgentId                        NVARCHAR(50)    NULL,
    AgentName                      NVARCHAR(200)   NULL,
    AgentEmail                     NVARCHAR(150)   NULL,
    AgentEmailAgent                NVARCHAR(150)   NULL,
    AgentTaxID                     NVARCHAR(50)    NULL,
    IdTransaction                  NVARCHAR(100)   NULL,
    PaymentTypeCreditCardType       NVARCHAR(50)    NULL,
    PaymentType                     NVARCHAR(50)    NULL,
    NameHolder                      NVARCHAR(100)   NULL,
    LastName                        NVARCHAR(100)   NULL,
    HolderCity                      NVARCHAR(100)   NULL,
    HolderCountry                   NVARCHAR(100)   NULL,
    HolderAddress                   NVARCHAR(300)   NULL,
    HolderPhone1                    NVARCHAR(50)    NULL,
    HolderPhone2                    NVARCHAR(50)    NULL,
    HolderPhone3                    NVARCHAR(50)    NULL,
    HolderFax                       NVARCHAR(50)    NULL,
    HolderEmail                     NVARCHAR(150)   NULL,
    HolderIdioma                    NVARCHAR(10)    NULL,
    HolderTipoDocumento              NVARCHAR(50)    NULL,
    HolderDni                       NVARCHAR(50)    NULL,
    HolderNacionalidadISO2            NVARCHAR(5)     NULL,
    HolderNacionalidad               NVARCHAR(100)   NULL
);
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.JP_BookingDetailLine') AND type = 'U')
CREATE TABLE dbo.JP_BookingDetailLine (
    Logicalref                          INT IDENTITY(1,1) PRIMARY KEY,
    IdBookLine                          INT             NULL,
    BookingCode                         NVARCHAR(50)    NOT NULL,
    Status                              NVARCHAR(50)    NULL,
    LineDate                            DATETIME        NULL,
    LineCancelled                       BIT             NULL,
    LineCancelledDate                   DATETIME        NULL,
    LineMarkup                          DECIMAL(18,2)   NULL,
    Externalreference                   NVARCHAR(100)   NULL,
    ExternalClientBookingNo             NVARCHAR(100)   NULL,
    DirectPayment                       BIT             NULL,
    PaymentAtDestination                BIT             NULL,
    NumPackage                          NVARCHAR(50)    NULL,
    NonRefundable                       BIT             NULL,
    LineCancellationChargesDate         DATETIME        NULL,
    SupplierLocator                     NVARCHAR(100)   NULL,
    Blocked                             BIT             NULL,
    RelatedBookingLine                  NVARCHAR(50)    NULL,
    ServiceName                         NVARCHAR(200)   NULL,
    ProductType                         NVARCHAR(50)    NULL,
    ProductTypeName                     NVARCHAR(100)   NULL,
    ProductGroup                        NVARCHAR(50)    NULL,
    ProductCodExport                    NVARCHAR(50)    NULL,
    ProductGroupAnalyticCode            NVARCHAR(50)    NULL,
    DepartmentGroup                     NVARCHAR(50)    NULL,
    DepartmentGroupAnalyticCode         NVARCHAR(50)    NULL,
    Market                              NVARCHAR(100)   NULL,
    AgencyGroupID                       NVARCHAR(50)    NULL,
    AgencyGroupName                     NVARCHAR(200)   NULL,
    ProductGroupName                    NVARCHAR(200)   NULL,
    Productid                           NVARCHAR(50)    NULL,
    SaleCompanyId                       NVARCHAR(50)    NULL,
    SaleCompanyCodExt                   NVARCHAR(50)    NULL,
    SaleCompanyName                     NVARCHAR(200)   NULL,
    SaleCompanyCountry                  NVARCHAR(100)   NULL,
    PuchasingCompanyId                  NVARCHAR(50)    NULL,
    PuchasingCompanyCodExt              NVARCHAR(50)    NULL,
    PuchasingCompanyName                NVARCHAR(200)   NULL,
    PuchasingCompanyCountry             NVARCHAR(100)   NULL,
    SupplierId                          NVARCHAR(50)    NULL,
    SupplierName                        NVARCHAR(200)   NULL,
    SupplierCodExport                   NVARCHAR(50)    NULL,
    SellingPrice                        DECIMAL(18,2)   NULL,
    Commission                          DECIMAL(18,2)   NULL,
    PerCommission                       DECIMAL(9,4)    NULL,
    CostCurrency                        NVARCHAR(10)    NULL,
    SellCurrency                        NVARCHAR(10)    NULL,
    CostBaseLine                        DECIMAL(18,2)   NULL,
    TaxCostNotIncluded                  DECIMAL(18,2)   NULL,
    CostCancellationFees                DECIMAL(18,2)   NULL,
    NetCostLine                         DECIMAL(18,2)   NULL,
    ComissionAmount                     DECIMAL(18,2)   NULL,
    ComissionTaxPercent                 DECIMAL(9,4)    NULL,
    ComissionPercent                    DECIMAL(9,4)    NULL,
    CommisionTaxAmount                  DECIMAL(18,2)   NULL,
    IndirectCommissionPercent           DECIMAL(9,4)    NULL,
    IndirectCommissionFix               DECIMAL(18,2)   NULL,
    IndirectCommissionAmount            DECIMAL(18,2)   NULL,
    IndirectCommissionSettled           BIT             NULL,
    Profit                               DECIMAL(18,2)   NULL,
    ProfitTaxtNotIncluded                DECIMAL(18,2)   NULL,
    SerialERP                           NVARCHAR(50)    NULL,
    Remarks                              NVARCHAR(1000)  NULL,
    PromotionCode                       NVARCHAR(50)    NULL,
    BasePriceCommission                  DECIMAL(18,2)   NULL,
    CustomerCommission                   DECIMAL(18,2)   NULL,
    BasePriceWithOutTax                  DECIMAL(18,2)   NULL,
    BasePrice                           DECIMAL(18,2)   NULL,
    TaxPriceNotIncluded                  DECIMAL(18,2)   NULL,
    CancellationFees                    DECIMAL(18,2)   NULL,
    BaseChangeFactor                    DECIMAL(18,6)   NULL,
    CostChangeFactor                    DECIMAL(18,6)   NULL,
    ZoneId                              NVARCHAR(50)    NULL,
    Zonedescription                     NVARCHAR(200)   NULL,
    Zonestate                           NVARCHAR(100)   NULL,
    Zonecountry                         NVARCHAR(100)   NULL,
    BeginTravelDate                     DATETIME        NULL,
    EndTravelDate                       DATETIME        NULL,
    ExternalSupplierConfirmationNumber   NVARCHAR(100)   NULL,
    ProviderAccount                     NVARCHAR(100)   NULL,
    PaxNumber                           INT             NULL,
    NightsNumber                        INT             NULL,
    FlightDetails                       NVARCHAR(500)   NULL,
    Category                            NVARCHAR(100)   NULL,
    isExtranet                          BIT             NULL,
    VirtualCreditCardPayment             BIT             NULL,
    HotelRemarks                        NVARCHAR(1000)  NULL
);
GO

CREATE INDEX IX_JP_BookingDetailLine_BookingCode ON dbo.JP_BookingDetailLine(BookingCode);
GO
