using System;
using System.Web.Security;
using System.Web.UI;
using BBGFinance.Core;

namespace BBGFinance
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (SessionManager.OturumAcikMi)
                    Response.Redirect(VarsayilanSayfa(), true);

                if (Request.QueryString["timeout"] == "1")
                {
                    pnlBilgi.Visible = true;
                    lblBilgi.Text = "Your session has expired. Please log in again.";
                }

                if (Request.QueryString["cikis"] == "1")
                {
                    pnlBilgi.Visible = true;
                    lblBilgi.Text = "You have been logged out successfully.";
                }
            }
        }

        protected void btnGiris_Click(object sender, EventArgs e)
        {
            if (!IsValid) return;

            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre        = txtSifre.Text;

            try
            {
                var dt = DbHelper.ExecuteQuery(@"
                    SELECT KullaniciID, SifreHash, SifreTuz, AdSoyad, Rol, CustomerGroupId,
                           AktifMi, HesapKilitliMi, BasarisizGiris
                    FROM   dbo.Kullanici
                    WHERE  KullaniciAdi = @KullaniciAdi",
                    DbHelper.Param("@KullaniciAdi", kullaniciAdi));

                if (dt.Rows.Count == 0)
                {
                    GirisLogYaz(0, false, "Unknown user: " + kullaniciAdi);
                    GosterHata("Invalid username or password.");
                    return;
                }

                var row = dt.Rows[0];
                int kullaniciID = Convert.ToInt32(row["KullaniciID"]);

                if (!Convert.ToBoolean(row["AktifMi"]))
                {
                    GosterHata("Your account is not active. Please contact your system administrator.");
                    return;
                }

                if (Convert.ToBoolean(row["HesapKilitliMi"]))
                {
                    GosterHata("Your account has been locked. Please contact your system administrator.");
                    return;
                }

                string hash = row["SifreHash"].ToString();
                string tuz  = row["SifreTuz"].ToString();

                if (!SessionManager.SifreDogrula(sifre, hash, tuz))
                {
                    int basarisiz = Convert.ToInt32(row["BasarisizGiris"]) + 1;
                    GirisBasarisiz(kullaniciAdi, "Invalid username or password.", basarisiz);
                    return;
                }

                // Successful login
                SessionManager.KullaniciID     = kullaniciID;
                SessionManager.KullaniciAdi    = kullaniciAdi;
                SessionManager.AdSoyad         = row["AdSoyad"].ToString();
                SessionManager.Rol             = row["Rol"].ToString();
                SessionManager.CustomerGroupId = row["CustomerGroupId"] != DBNull.Value
                    ? (int?)Convert.ToInt32(row["CustomerGroupId"])
                    : null;

                DbHelper.ExecuteNonQuery(@"
                    UPDATE dbo.Kullanici
                    SET    SonGirisTarihi = GETDATE(), BasarisizGiris = 0
                    WHERE  KullaniciID = @ID",
                    DbHelper.Param("@ID", kullaniciID));

                GirisLogYaz(kullaniciID, true, null);

                bool hatirla = chkBeniHatirla.Checked;
                FormsAuthentication.SetAuthCookie(kullaniciAdi, hatirla);

                string returnUrl = Request.QueryString["ReturnUrl"];
                if (!string.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
                    Response.Redirect(returnUrl, true);
                else
                    Response.Redirect(VarsayilanSayfa(), true);
            }
            catch (Exception ex)
            {
                GosterHata("System error: " + ex.Message);
            }
        }

        private void GirisBasarisiz(string kullaniciAdi, string mesaj, int basarisizSayac)
        {
            if (basarisizSayac >= 5)
            {
                DbHelper.ExecuteNonQuery(@"
                    UPDATE dbo.Kullanici
                    SET    HesapKilitliMi = 1, BasarisizGiris = @Sayac
                    WHERE  KullaniciAdi = @KullaniciAdi",
                    DbHelper.Param("@Sayac", basarisizSayac),
                    DbHelper.Param("@KullaniciAdi", kullaniciAdi));
                mesaj = "Your account has been locked due to 5 failed login attempts.";
            }
            else
            {
                DbHelper.ExecuteNonQuery(@"
                    UPDATE dbo.Kullanici
                    SET    BasarisizGiris = @Sayac
                    WHERE  KullaniciAdi = @KullaniciAdi",
                    DbHelper.Param("@Sayac", basarisizSayac),
                    DbHelper.Param("@KullaniciAdi", kullaniciAdi));
            }

            GirisLogYaz(0, false, mesaj);
            GosterHata(mesaj);
        }

        private void GosterHata(string mesaj)
        {
            pnlHata.Visible = true;
            lblHata.Text     = System.Web.HttpUtility.HtmlEncode(mesaj);
            txtSifre.Text    = string.Empty;
        }

        private void GirisLogYaz(int kullaniciID, bool basari, string hata)
        {
            try
            {
                DbHelper.ExecuteNonQuery(@"
                    INSERT INTO dbo.OturumGecmisi (KullaniciID, IPAdresi, BasariMi, HataMesaji)
                    VALUES (@ID, @IP, @Basari, @Hata)",
                    DbHelper.Param("@ID",     kullaniciID > 0 ? (object)kullaniciID : DBNull.Value),
                    DbHelper.Param("@IP",     Request.UserHostAddress),
                    DbHelper.Param("@Basari", basari),
                    DbHelper.Param("@Hata",   hata ?? (object)DBNull.Value));
            }
            catch { /* a logging failure must not block login */ }
        }

        private bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && (url.StartsWith("/") || url.StartsWith("~/"));
        }

        /// <summary>Admin -> internal Dashboard; Musteri -> their own (simplified) Dashboard.</summary>
        private string VarsayilanSayfa()
        {
            return SessionManager.Rol == AppConstants.Roller.Admin
                ? "~/Default.aspx"
                : "~/MusteriDashboard.aspx";
        }
    }
}
