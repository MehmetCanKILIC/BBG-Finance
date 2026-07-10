using System;

namespace BBGFinance.Core
{
    public static class AppConstants
    {
        public const string AppName = "BBG Finance";

        public static class Roller
        {
            /// <summary>Tüm müşteri gruplarının verisini görür; maliyet/kâr/tedarikçi dahil.</summary>
            public const string Admin     = "Admin";
            /// <summary>Genel (müşteri ayrımına tabi olmayan) iç kullanıcı - şu an kullanılmıyor, ileride gerekirse.</summary>
            public const string Kullanici = "Kullanici";
            /// <summary>Müşteri (acente) girişi. Sadece kendi CustomerGroupId'sine ait veriyi görür;
            /// maliyet/kâr/tedarikçi alanları bu rol için hiçbir sorguya/JSON'a dahil edilmez.</summary>
            public const string Musteri   = "Musteri";
        }

        public static class SessionKeys
        {
            public const string KullaniciID      = "KullaniciID";
            public const string KullaniciAdi     = "KullaniciAdi";
            public const string AdSoyad          = "AdSoyad";
            public const string Rol              = "Rol";
            public const string CustomerGroupId  = "CustomerGroupId";
        }
    }
}
