using System;
using System.Data;
using BBGFinance.Core;
using BBGFinance.Data;

namespace BBGFinance.Modules.Kullanicilar
{
    /// <summary>Admin'in yeni bir kullanıcı (Admin veya Musteri) oluşturduğu ekran. Musteri
    /// rolü seçildiğinde kayıtlı bir CustomerGroup (JP_Customer) seçimi zorunludur; şifre admin
    /// tarafından elle girilebilir veya "Suggest Password" ile önerilen rastgele bir şifre
    /// kullanılabilir - iki durumda da hash/salt sunucuda SessionManager ile üretilir.</summary>
    public partial class Ekle : AdminBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MusteriGruplariniYukle();
            }
        }

        private void MusteriGruplariniYukle()
        {
            ddlCustomerGroup.Items.Clear();
            ddlCustomerGroup.Items.Add(new System.Web.UI.WebControls.ListItem("Select...", ""));

            DataTable dt = RezervasyonRepository.MusteriGruplari();
            foreach (DataRow row in dt.Rows)
            {
                string id = row["CustomerGroupId"].ToString();
                string ad = row["CustomerGroup"].ToString();
                ddlCustomerGroup.Items.Add(new System.Web.UI.WebControls.ListItem(ad, id));
            }
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!IsValid) return;

            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string adSoyad      = txtAdSoyad.Text.Trim();
            string email        = txtEmail.Text.Trim();
            string rol          = ddlRol.SelectedValue;
            string sifre        = txtSifre.Text;

            if (sifre.Length < 8)
            {
                GosterHata("Password must be at least 8 characters long.");
                return;
            }

            int? customerGroupId = null;
            if (rol == AppConstants.Roller.Musteri)
            {
                if (string.IsNullOrEmpty(ddlCustomerGroup.SelectedValue))
                {
                    GosterHata("Please select a customer group for a customer user.");
                    return;
                }
                customerGroupId = Convert.ToInt32(ddlCustomerGroup.SelectedValue);
            }

            try
            {
                var mevcut = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM dbo.Kullanici WHERE KullaniciAdi = @KullaniciAdi",
                    DbHelper.Param("@KullaniciAdi", kullaniciAdi));

                if (Convert.ToInt32(mevcut) > 0)
                {
                    GosterHata("This username is already in use.");
                    return;
                }

                string tuz = SessionManager.SifreTuzUret();
                string hash = SessionManager.SifreHashle(sifre, tuz);

                DbHelper.ExecuteNonQuery(@"
                    INSERT INTO dbo.Kullanici
                        (KullaniciAdi, SifreHash, SifreTuz, AdSoyad, Email, Rol, CustomerGroupId, AktifMi, OlusturmaTarihi)
                    VALUES
                        (@KullaniciAdi, @SifreHash, @SifreTuz, @AdSoyad, @Email, @Rol, @CustomerGroupId, @AktifMi, GETDATE())",
                    DbHelper.Param("@KullaniciAdi", kullaniciAdi),
                    DbHelper.Param("@SifreHash", hash),
                    DbHelper.Param("@SifreTuz", tuz),
                    DbHelper.Param("@AdSoyad", adSoyad),
                    DbHelper.Param("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    DbHelper.Param("@Rol", rol),
                    DbHelper.Param("@CustomerGroupId", customerGroupId.HasValue ? (object)customerGroupId.Value : DBNull.Value),
                    DbHelper.Param("@AktifMi", chkAktif.Checked));

                GosterBasari("User \"" + kullaniciAdi + "\" was created successfully.");
                FormuTemizle();
            }
            catch (Exception ex)
            {
                GosterHata("System error: " + ex.Message);
            }
        }

        private void FormuTemizle()
        {
            txtKullaniciAdi.Text = "";
            txtAdSoyad.Text = "";
            txtEmail.Text = "";
            txtSifre.Text = "";
            ddlRol.SelectedIndex = 0;
            MusteriGruplariniYukle();
            chkAktif.Checked = true;
        }

        private void GosterHata(string mesaj)
        {
            pnlBasari.Visible = false;
            pnlHata.Visible = true;
            lblHata.Text = System.Web.HttpUtility.HtmlEncode(mesaj);
        }

        private void GosterBasari(string mesaj)
        {
            pnlHata.Visible = false;
            pnlBasari.Visible = true;
            lblBasari.Text = System.Web.HttpUtility.HtmlEncode(mesaj);
        }
    }
}
