using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace BBGFinance.Core
{
    public static class SessionManager
    {
        public static int KullaniciID
        {
            get
            {
                var v = HttpContext.Current.Session[AppConstants.SessionKeys.KullaniciID];
                return v != null ? (int)v : 0;
            }
            set { HttpContext.Current.Session[AppConstants.SessionKeys.KullaniciID] = value; }
        }

        public static string KullaniciAdi
        {
            get
            {
                var v = HttpContext.Current.Session[AppConstants.SessionKeys.KullaniciAdi];
                return v != null ? v.ToString() : string.Empty;
            }
            set { HttpContext.Current.Session[AppConstants.SessionKeys.KullaniciAdi] = value; }
        }

        public static string AdSoyad
        {
            get
            {
                var v = HttpContext.Current.Session[AppConstants.SessionKeys.AdSoyad];
                return v != null ? v.ToString() : string.Empty;
            }
            set { HttpContext.Current.Session[AppConstants.SessionKeys.AdSoyad] = value; }
        }

        public static string Rol
        {
            get
            {
                var v = HttpContext.Current.Session[AppConstants.SessionKeys.Rol];
                return v != null ? v.ToString() : string.Empty;
            }
            set { HttpContext.Current.Session[AppConstants.SessionKeys.Rol] = value; }
        }

        /// <summary>
        /// Musteri rolündeki kullanıcının hangi JP_Customer.customerGroupId'ye ait olduğu.
        /// Admin için NULL'dur (filtre uygulanmaz, tüm veriyi görür).
        /// </summary>
        public static int? CustomerGroupId
        {
            get
            {
                var v = HttpContext.Current.Session[AppConstants.SessionKeys.CustomerGroupId];
                return v != null ? (int?)v : null;
            }
            set { HttpContext.Current.Session[AppConstants.SessionKeys.CustomerGroupId] = value; }
        }

        public static bool OturumAcikMi
        {
            get { return KullaniciID > 0; }
        }

        public static void OturumKapat()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
            FormsAuthentication.SignOut();
        }

        public static string SifreHashle(string sifre, string tuz)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(sifre + tuz);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static string SifreTuzUret()
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static bool SifreDogrula(string girilen, string hash, string tuz)
        {
            return SifreHashle(girilen, tuz) == hash;
        }
    }
}
