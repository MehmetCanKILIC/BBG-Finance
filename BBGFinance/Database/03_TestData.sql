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

-- ============================================================
-- JP_Customer (çoklu müşteri / tenant test verisi)
-- CustomerGroupId=100 -> Acente Madrid + Acente Roma (aynı grup, iki ayrı hesap)
-- CustomerGroupId=200 -> Acente Paris (SeesCommission=0 -> komisyonu göremez)
-- CustomerGroupId=300 -> Acente Londra
-- ============================================================
IF NOT EXISTS (SELECT * FROM dbo.JP_Customer WHERE Name = 'Acente Madrid')
BEGIN
    SET IDENTITY_INSERT dbo.JP_Customer ON;

    INSERT INTO dbo.JP_Customer (Id, Name, CustomerGroup, CustomerGroupId, Market, SeesCommission, Active)
    VALUES
        (1, 'Acente Madrid', 'BBG Group A', 100, 'España',         1, 1),
        (2, 'Acente Paris',  'BBG Group B', 200, 'France',         0, 1),
        (3, 'Acente Roma',   'BBG Group A', 100, 'Italia',         1, 1),
        (4, 'Acente Londra', 'BBG Group C', 300, 'United Kingdom', 1, 1);

    SET IDENTITY_INSERT dbo.JP_Customer OFF;
END
GO

-- Rezervasyonları müşterilere bağla (CustomerId, JP_Customer.Id'yi metin olarak tutar -
-- gerçek JP_ROIBEDS'te de kolonlar arası tip farkı olabileceğinden SqlSafe.JoinEq metin
-- bazlı karşılaştırma yapar).
UPDATE dbo.JP_BookingDetail SET CustomerId = '1' WHERE BookingCode IN ('BK-0001','BK-0003','BK-0005','BK-0007');
UPDATE dbo.JP_BookingDetail SET CustomerId = '2' WHERE BookingCode IN ('BK-0004','BK-0008');
UPDATE dbo.JP_BookingDetail SET CustomerId = '3' WHERE BookingCode IN ('BK-0006');
UPDATE dbo.JP_BookingDetail SET CustomerId = '4' WHERE BookingCode IN ('BK-0002');
GO

-- Bölge (Zone) bilgisi ve IdBookLine (RoomList/Paxes ile eşleşme için) doldur.
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Costa del Sol',      Zonestate = 'Andalucía',   Zonecountry = 'España'         WHERE BookingCode = 'BK-0001';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Londra Merkez',       Zonestate = 'Greater London', Zonecountry = 'United Kingdom' WHERE BookingCode = 'BK-0002';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Barselona',            Zonestate = 'Katalonya',   Zonecountry = 'España'         WHERE BookingCode = 'BK-0003';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Fransız Rivierası',    Zonestate = 'PACA',        Zonecountry = 'France'         WHERE BookingCode = 'BK-0004';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Berlin',               Zonestate = 'Berlin',      Zonecountry = 'Deutschland'    WHERE BookingCode = 'BK-0005';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Roma',                 Zonestate = 'Lazio',       Zonecountry = 'Italia'         WHERE BookingCode = 'BK-0006';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Sevilla',               Zonestate = 'Andalucía',   Zonecountry = 'España'         WHERE BookingCode = 'BK-0007';
UPDATE dbo.JP_BookingDetailLine SET IdBookLine = 1, Zonedescription = 'Münih',                 Zonestate = 'Bavyera',     Zonecountry = 'Deutschland'    WHERE BookingCode = 'BK-0008';
GO

-- ============================================================
-- JP_BookingDetailLineRoomList (oda tipi - costroom ASLA müşteriye gösterilmez)
-- ============================================================
IF NOT EXISTS (SELECT * FROM dbo.JP_BookingDetailLineRoomList WHERE BookingCode = 'BK-0001')
INSERT INTO dbo.JP_BookingDetailLineRoomList
    (BookingCode, IdBookLine, hotelcode, namehotel, addressline, typeroom, typeroomname, roomnumber, priceroom, costroom, boardtype, name, lastname)
VALUES
    ('BK-0001', 1, 'HTL-001', 'Hotel Playa Sol',      'Costa del Sol, España',      'DBL', 'Deluxe Deniz Manzaralı', '204', 1450.00, 1020.00, 'AI', 'Elena',  'Torres'),
    ('BK-0002', 1, 'HTL-002', 'Hotel Central London', 'Londra Merkez, İngiltere',   'FAM', 'Aile Odası',             '512', 2100.00, 1500.00, 'BB', 'John',   'Smith'),
    ('BK-0003', 1, 'HTL-003', 'Hotel Ramblas',        'Barselona, İspanya',         'STD', 'Standart Oda',           '108', 890.00,  620.00,  'RO', 'Marta',  'Fernandez'),
    ('BK-0004', 1, 'HTL-004', 'Resort Cote Azur',     'Fransız Rivierası, Fransa',  'RES', 'Resort Suiti',           '301', 3200.00, 2350.00, 'AI', 'Pierre', 'Dubois'),
    ('BK-0005', 1, 'HTL-005', 'Hotel Romantico',      'Berlin, Almanya',            'HON', 'Balayı Suiti',           '702', 1750.00, 1210.00, 'HB', 'Anna',   'Keller'),
    ('BK-0006', 1, 'HTL-006', 'City Hotel Roma',      'Roma, İtalya',               'STD', 'Şehir Manzaralı Oda',    '215', 990.00,  700.00,  'BB', 'Marco',  'Rossi'),
    ('BK-0007', 1, 'HTL-007', 'Hotel Sevilla Palace', 'Sevilla, İspanya',           'HIS', 'Tarihi Suit',            '410', 2450.00, 1740.00, 'AI', 'Sara',   'Lopez'),
    ('BK-0008', 1, 'HTL-008', 'Hotel Munich Business','Münih, Almanya',             'BUS', 'İş Odası',               '618', 1320.00, 940.00,  'BB', 'Thomas', 'Weber');
GO

-- ============================================================
-- JP_BookingDetailLinePaxes (yolcu/misafir - TipPax: 0=Yetişkin, 1=Çocuk, 2=Bebek)
-- ============================================================
IF NOT EXISTS (SELECT * FROM dbo.JP_BookingDetailLinePaxes WHERE BookingCode = 'BK-0001')
INSERT INTO dbo.JP_BookingDetailLinePaxes
    (BookingCode, IdBookLine, Name, LastName, TipPax, Age, Country)
VALUES
    ('BK-0001', 1, 'Elena',  'Torres',   0, 34, 'España'),
    ('BK-0001', 1, 'Miguel', 'Torres',   0, 36, 'España'),
    ('BK-0002', 1, 'John',   'Smith',    0, 41, 'United Kingdom'),
    ('BK-0002', 1, 'Kate',   'Smith',    0, 39, 'United Kingdom'),
    ('BK-0002', 1, 'Oliver', 'Smith',    1, 8,  'United Kingdom'),
    ('BK-0002', 1, 'Emily',  'Smith',    1, 5,  'United Kingdom'),
    ('BK-0003', 1, 'Marta',  'Fernandez',0, 29, 'España'),
    ('BK-0004', 1, 'Pierre', 'Dubois',   0, 45, 'France'),
    ('BK-0004', 1, 'Claire', 'Dubois',   0, 43, 'France'),
    ('BK-0004', 1, 'Lucas',  'Dubois',   1, 11, 'France'),
    ('BK-0004', 1, 'Noah',   'Dubois',   2, 1,  'France'),
    ('BK-0005', 1, 'Anna',   'Keller',   0, 31, 'Deutschland'),
    ('BK-0005', 1, 'Jonas',  'Keller',   0, 33, 'Deutschland'),
    ('BK-0006', 1, 'Marco',  'Rossi',    0, 27, 'Italia'),
    ('BK-0006', 1, 'Giulia', 'Rossi',    0, 26, 'Italia'),
    ('BK-0007', 1, 'Sara',   'Lopez',    0, 38, 'España'),
    ('BK-0007', 1, 'Diego',  'Lopez',    0, 40, 'España'),
    ('BK-0008', 1, 'Thomas', 'Weber',    0, 50, 'Deutschland');
GO
