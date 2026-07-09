-- ============================================================
-- JP_ROIBEDS_DEV - Örnek Test Verisi
-- Sadece yerel geliştirme/test amaçlıdır (02_JPRoibeds_TestSchema.sql
-- ile oluşturulan veritabanına karşı çalıştırılır).
-- ============================================================

USE JP_ROIBEDS_DEV;
GO

IF NOT EXISTS (SELECT * FROM dbo.JP_BookingDetail WHERE BookingCode = 'BK-0001')
BEGIN
    INSERT INTO dbo.JP_BookingDetail
        (BookingCode, BookingDate, CancelDate, Channel, AgencyRef, CustomerName, CustomerEmail,
         CustomerCountry, CustomerCity, AgentName, AgentEmail, BookingAdmin, AccountManager,
         SellingPrice, Cost, Commission, OutStandingAmount, Invoiced, Description, Remarks, FinancialNotes)
    VALUES
        ('BK-0001', DATEADD(MONTH,-5,GETDATE()), NULL, 'Web', 'AG-100', 'Elena Torres', 'elena.torres@example.com',
         'España', 'Madrid', 'Acente Madrid', 'madrid@agency.example', 'Ana Perez', 'Carlos Ruiz',
         1450.00, 1020.00, 145.00, 0.00, 1, 'Yaz tatili paketi', NULL, NULL),
        ('BK-0002', DATEADD(MONTH,-4,GETDATE()), NULL, 'B2B', 'AG-101', 'John Smith', 'john.smith@example.com',
         'United Kingdom', 'London', 'Acente Londra', 'london@agency.example', 'Ana Perez', 'Carlos Ruiz',
         2100.00, 1500.00, 210.00, 350.00, 0, 'Aile konaklaması', NULL, NULL),
        ('BK-0003', DATEADD(MONTH,-4,GETDATE()), DATEADD(MONTH,-3,GETDATE()), 'Web', 'AG-100', 'Marta Fernandez', 'marta.f@example.com',
         'España', 'Barcelona', 'Acente Madrid', 'madrid@agency.example', 'Luis Gomez', 'Carlos Ruiz',
         890.00, 620.00, 89.00, 0.00, 0, 'Hafta sonu kaçamağı', 'Müşteri isteğiyle iptal edildi', NULL),
        ('BK-0004', DATEADD(MONTH,-3,GETDATE()), NULL, 'Call Center', 'AG-102', 'Pierre Dubois', 'pierre.d@example.com',
         'France', 'Paris', 'Acente Paris', 'paris@agency.example', 'Luis Gomez', 'Sofia Diaz',
         3200.00, 2350.00, 320.00, 1200.00, 0, 'Grup rezervasyonu', NULL, NULL),
        ('BK-0005', DATEADD(MONTH,-2,GETDATE()), NULL, 'Web', 'AG-100', 'Anna Keller', 'anna.k@example.com',
         'Deutschland', 'Berlin', 'Acente Madrid', 'madrid@agency.example', 'Ana Perez', 'Sofia Diaz',
         1750.00, 1210.00, 175.00, 0.00, 1, 'Balayı paketi', NULL, NULL),
        ('BK-0006', DATEADD(MONTH,-1,GETDATE()), NULL, 'B2B', 'AG-103', 'Marco Rossi', 'marco.r@example.com',
         'Italia', 'Roma', 'Acente Roma', 'roma@agency.example', 'Luis Gomez', 'Carlos Ruiz',
         990.00, 700.00, 99.00, 990.00, 0, 'Kısa süreli konaklama', NULL, NULL),
        ('BK-0007', DATEADD(DAY,-10,GETDATE()), NULL, 'Web', 'AG-100', 'Sara Lopez', 'sara.lopez@example.com',
         'España', 'Sevilla', 'Acente Madrid', 'madrid@agency.example', 'Ana Perez', 'Sofia Diaz',
         2450.00, 1740.00, 245.00, 2450.00, 0, 'Yıl dönümü seyahati', NULL, 'Ödeme takibi bekleniyor'),
        ('BK-0008', DATEADD(DAY,-2,GETDATE()), NULL, 'Call Center', 'AG-102', 'Thomas Weber', 'thomas.w@example.com',
         'Deutschland', 'Munich', 'Acente Paris', 'paris@agency.example', 'Luis Gomez', 'Carlos Ruiz',
         1320.00, 940.00, 132.00, 1320.00, 0, 'Konferans konaklaması', NULL, NULL);
END
GO

IF NOT EXISTS (SELECT * FROM dbo.JP_BookingDetailLine WHERE BookingCode = 'BK-0001')
BEGIN
    INSERT INTO dbo.JP_BookingDetailLine
        (BookingCode, ServiceName, ProductType, ProductGroup, ProductGroupName, Market, SupplierName,
         SellingPrice, Commission, Profit, SellCurrency, CostCurrency, Cost, PaxNumber, NightsNumber,
         BeginTravelDate, EndTravelDate, LineCancelled, Category, HotelRemarks)
    VALUES
        ('BK-0001','Hotel Playa Sol','Accommodation','HOTEL','Otel','España','Hoteles del Sur SA',
         1450.00, 145.00, 430.00, 'EUR','EUR', 1020.00, 2, 7,
         DATEADD(MONTH,-5,GETDATE()), DATEADD(DAY,2,DATEADD(MONTH,-5,GETDATE())), 0, 'Beach', NULL),

        ('BK-0002','Hotel Central London','Accommodation','HOTEL','Otel','United Kingdom','London Hotels Ltd',
         2100.00, 210.00, 600.00, 'GBP','GBP', 1500.00, 4, 5,
         DATEADD(MONTH,-4,GETDATE()), DATEADD(DAY,5,DATEADD(MONTH,-4,GETDATE())), 0, 'City', NULL),

        ('BK-0003','Hotel Ramblas','Accommodation','HOTEL','Otel','España','Hoteles del Sur SA',
         890.00, 89.00, 270.00, 'EUR','EUR', 620.00, 2, 2, DATEADD(MONTH,-4,GETDATE()), DATEADD(DAY,2,DATEADD(MONTH,-4,GETDATE())), 1, 'City', NULL),

        ('BK-0004','Resort Cote Azur','Accommodation','HOTEL','Otel','France','Cote Azur Resorts',
         3200.00, 320.00, 850.00, 'EUR','EUR', 2350.00, 8, 10, DATEADD(MONTH,-3,GETDATE()), DATEADD(DAY,10,DATEADD(MONTH,-3,GETDATE())), 0, 'Resort', NULL),

        ('BK-0005','Hotel Romantico','Accommodation','HOTEL','Otel','Deutschland','Berlin Boutique Hotels',
         1750.00, 175.00, 540.00, 'EUR','EUR', 1210.00, 2, 4, DATEADD(MONTH,-2,GETDATE()), DATEADD(DAY,4,DATEADD(MONTH,-2,GETDATE())), 0, 'Boutique', NULL),

        ('BK-0006','City Hotel Roma','Accommodation','HOTEL','Otel','Italia','Roma Hospitality Group',
         990.00, 99.00, 290.00, 'EUR','EUR', 700.00, 2, 3, DATEADD(MONTH,-1,GETDATE()), DATEADD(DAY,3,DATEADD(MONTH,-1,GETDATE())), 0, 'City', NULL),

        ('BK-0007','Hotel Sevilla Palace','Accommodation','HOTEL','Otel','España','Hoteles del Sur SA',
         2450.00, 245.00, 710.00, 'EUR','EUR', 1740.00, 2, 6, DATEADD(DAY,20,GETDATE()), DATEADD(DAY,26,GETDATE()), 0, 'Historic', NULL),

        ('BK-0008','Hotel Munich Business','Accommodation','HOTEL','Otel','Deutschland','Munich Hotels AG',
         1320.00, 132.00, 380.00, 'EUR','EUR', 940.00, 1, 3, DATEADD(DAY,5,GETDATE()), DATEADD(DAY,8,GETDATE()), 0, 'Business', NULL);
END
GO
