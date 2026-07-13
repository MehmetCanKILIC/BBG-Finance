using System;
using System.Data;
using BBGFinance.Core;
using BBGFinance.Data;

namespace BBGFinance.Modules.Kullanicilar
{
    /// <summary>Admin'in mevcut bir kullanıcının (Admin veya Musteri) bilgilerini, rolünü,
    /// customer group'unu, aktif/kilitli durumunu ve isteğe bağlı olarak şifresini
    /// güncelleyebildiği ekran. Kullanıcı adı değiştirilemez (Login.aspx'te kimlik anahtarı).</summary>
    public partial class Duzenle : AdminBase
    {
        private int _kullaniciID;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["id"], out _kullaniciID) || _kullaniciID <= 0)
            {
                pnlForm.Visible = false;
                pnlBulunamadi.Visible = true;
                return;
            }

            if (!IsPostBack)
            {
                MusteriGruplariniYukle();
                if (!KullaniciyiYukle(_kullaniciID))
                {
                    pnlForm.Visible = false;
                    pnlBulunamadi.Visible = true;
                }
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

        private bool KullaniciyiYukle(int kullaniciID)
        {
            var dt = DbHelper.ExecuteQuery(@"
                SELECT KullaniciID, KullaniciAdi, AdSoyad, Email, Rol, CustomerGroupId, AktifMi, HesapKilitliMi
                FROM   dbo.Kullanici
                WHERE  KullaniciID = @ID",
                DbHelper.Param("@ID", kullaniciID));

            if (dt.Rows.Count == 0) return false;

            var row = dt.Rows[0];
            litKullaniciAdi.Text = System.Web.HttpUtility.HtmlEncode(row["KullaniciAdi"].ToString());
            txtAdSoyad.Text = row["AdSoyad"].ToString();
            txtEmail.Text = row["Email"] == DBNull.Value ? "" : row["Email"].ToString();
            ddlRol.SelectedValue = row["Rol"].ToString();
            chkAktif.Checked = Convert.ToBoolean(row["AktifMi"]);
            chkKilitliyiAc.Checked = false;

            if (row["CustomerGroupId"] != DBNull.Value)
            {
                var item = ddlCustomerGroup.Items.FindByValue(row["CustomerGroupId"].ToString());
                if (item != null) ddlCustomerGroup.SelectedValue = row["CustomerGroupId"].ToString();
            }

            return true;
        }

        protected void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!IsValid) return;

            string adSoyad = txtAdSoyad.Text.Trim();
            string email = txtEmail.Text.Trim();
            string rol = ddlRol.SelectedValue;

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

            string tuz = null, hash = null;
            if (chkSifreDegistir.Checked)
            {
                string yeniSifre = txtSifre.Text;
                if (yeniSifre.Length < 8)
                {
                    GosterHata("The new password must be at least 8 characters long.");
                    return;
                }
                tuz = SessionManager.SifreTuzUret();
                hash = SessionManager.SifreHashle(yeniSifre, tuz);
            }

            // Kilit sadece "Unlock account" işaretlenirse açılır; işaretlenmezse mevcut
            // kilitli durumu (BasarisizGiris sayacıyla birlikte) olduğu gibi kalır.
            bool kilitliKalsin = !chkKilitliyiAc.Checked && KullaniciKilitliMi(_kullaniciID);

            string sifreSql = chkSifreDegistir.Checked ? ", SifreHash = @SifreHash, SifreTuz = @SifreTuz" : "";

            try
            {
                DbHelper.ExecuteNonQuery(@"
                    UPDATE dbo.Kullanici
                    SET    AdSoyad = @AdSoyad, Email = @Email, Rol = @Rol, CustomerGroupId = @CustomerGroupId,
                           AktifMi = @AktifMi, HesapKilitliMi = @HesapKilitliMi,
                           BasarisizGiris = CASE WHEN @KilitAcildi = 1 THEN 0 ELSE BasarisizGiris END
                           " + sifreSql + @"
                    WHERE  KullaniciID = @ID",
                    DbHelper.Param("@AdSoyad", adSoyad),
                    DbHelper.Param("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    DbHelper.Param("@Rol", rol),
                    DbHelper.Param("@CustomerGroupId", customerGroupId.HasValue ? (object)customerGroupId.Value : DBNull.Value),
                    DbHelper.Param("@AktifMi", chkAktif.Checked),
                    DbHelper.Param("@HesapKilitliMi", kilitliKalsin),
                    DbHelper.Param("@KilitAcildi", chkKilitliyiAc.Checked),
                    DbHelper.Param("@SifreHash", (object)hash ?? DBNull.Value),
                    DbHelper.Param("@SifreTuz", (object)tuz ?? DBNull.Value),
                    DbHelper.Param("@ID", _kullaniciID));

                GosterBasari("User updated successfully.");
                chkSifreDegistir.Checked = false;
                txtSifre.Text = "";
                chkKilitliyiAc.Checked = false;
            }
            catch (Exception ex)
            {
                GosterHata("System error: " + ex.Message);
            }
        }

        private bool KullaniciKilitliMi(int kullaniciID)
        {
            var v = DbHelper.ExecuteScalar(
                "SELECT HesapKilitliMi FROM dbo.Kullanici WHERE KullaniciID = @ID",
                DbHelper.Param("@ID", kullaniciID));
            return v != null && v != DBNull.Value && Convert.ToBoolean(v);
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
