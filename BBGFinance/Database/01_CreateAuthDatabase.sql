-- ============================================================
-- BBG Finance - Yetkilendirme Veritabanı Kurulum Scripti
-- SQL Server 2016+
--
-- Bu veritabanı SADECE bu portalın kendi kullanıcı/oturum
-- bilgilerini tutar. Rezervasyon verisi JP_ROIBEDS üzerinde
-- ayrı ve salt-okunur olarak kalır (bkz. 02_JPRoibeds_TestSchema.sql).
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BBGFinanceAuth')
BEGIN
    CREATE DATABASE BBGFinanceAuth
    COLLATE Turkish_CI_AS;
END
GO

USE BBGFinanceAuth;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.Kullanici') AND type = 'U')
CREATE TABLE dbo.Kullanici (
    KullaniciID     INT IDENTITY(1,1) PRIMARY KEY,
    KullaniciAdi    NVARCHAR(50)  NOT NULL UNIQUE,
    SifreHash       NVARCHAR(256) NOT NULL,
    SifreTuz        NVARCHAR(64)  NOT NULL,
    AdSoyad         NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(100) NULL,
    Rol             NVARCHAR(50)  NOT NULL DEFAULT 'Kullanici', -- Admin, Kullanici, Musteri
    -- Rol='Musteri' olan kullanıcılar için JP_ROIBEDS..JP_Customer.customerGroupId eşleşmesi.
    -- Rol='Admin' için NULL bırakılır (tüm müşteri gruplarını görür, filtre uygulanmaz).
    CustomerGroupId INT           NULL,
    AktifMi         BIT           NOT NULL DEFAULT 1,
    SonGirisTarihi  DATETIME      NULL,
    BasarisizGiris  INT           NOT NULL DEFAULT 0,
    HesapKilitliMi  BIT           NOT NULL DEFAULT 0,
    OlusturmaTarihi DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('dbo.OturumGecmisi') AND type = 'U')
CREATE TABLE dbo.OturumGecmisi (
    OturumID        INT IDENTITY(1,1) PRIMARY KEY,
    KullaniciID     INT           NULL REFERENCES dbo.Kullanici(KullaniciID),
    GirisTarihi     DATETIME      NOT NULL DEFAULT GETDATE(),
    IPAdresi        NVARCHAR(50)  NULL,
    BasariMi        BIT           NOT NULL DEFAULT 1,
    HataMesaji      NVARCHAR(500) NULL
);
GO

-- ============================================================
-- İlk yönetici kullanıcı
--   Kullanıcı adı : admin
--   Geçici şifre  : BBGFinance2026!
--
-- Hash, SessionManager.SifreHashle(sifre, tuz) = SHA256(sifre + tuz) ile
-- üretildi. İLK GİRİŞTEN SONRA bu şifreyi değiştirin: yeni bir tuz üretip
-- (SessionManager.SifreTuzUret) yeni şifreyi aynı yöntemle hashleyip
-- aşağıdaki UPDATE'e benzer bir sorguyla Kullanici tablosuna yazmanız
-- gerekir (uygulamada ayrı bir "şifre değiştir" ekranı yoktur).
-- ============================================================
IF NOT EXISTS (SELECT * FROM dbo.Kullanici WHERE KullaniciAdi = 'admin')
INSERT INTO dbo.Kullanici (KullaniciAdi, SifreHash, SifreTuz, AdSoyad, Rol, AktifMi)
VALUES (
    'admin',
    '97eb79eac874471b094ffe0b426434c4b8adb0bed1a146192817b6e1f640d183',
    'BBGSaltDegistir123==',
    'Sistem Yöneticisi',
    'Admin',
    1
);
GO
