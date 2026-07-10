using System;
using System.Web.UI;
using BBGFinance.Core;

namespace BBGFinance
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionManager.OturumAcikMi)
            {
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                lblAdSoyad.Text = SessionManager.AdSoyad;
                lblRol.Text     = RolEtiket(SessionManager.Rol);
                lblAvatar.Text  = AvatarHarf(SessionManager.AdSoyad);

                // Reservations menu (contains cost/profit/supplier) is shown to Admin only.
                // The customer menu is shown to everyone who is NOT Admin (matches AdminBase /
                // CustomerGroupId elsewhere in the app) rather than requiring an exact "Musteri"
                // role match - a role value that doesn't match either constant exactly (typo,
                // different casing, etc.) must still fall on the customer side, not disappear
                // from the sidebar entirely.
                bool adminMi = SessionManager.Rol == AppConstants.Roller.Admin;
                pnlAdminMenu.Visible   = adminMi;
                pnlMusteriMenu.Visible = !adminMi;
            }
        }

        protected void lnkCikis_Click(object sender, EventArgs e)
        {
            SessionManager.OturumKapat();
            Response.Redirect("~/Login.aspx?cikis=1", true);
        }

        protected string ActivePage(string pageName)
        {
            string path = Request.AppRelativeCurrentExecutionFilePath;
            return path != null && path.IndexOf(pageName, StringComparison.OrdinalIgnoreCase) >= 0
                ? "active" : "";
        }

        /// <summary>Admin -> iç Dashboard; Musteri -> kendi (sadeleştirilmiş) Dashboard.</summary>
        protected string AnaSayfaUrl()
        {
            return SessionManager.Rol == AppConstants.Roller.Admin
                ? "~/Default.aspx"
                : "~/MusteriDashboard.aspx";
        }

        private static string RolEtiket(string rol)
        {
            // Aynı "Admin değilse müşteridir" mantığı burada da geçerli - rol değeri "Musteri"
            // sabitiyle birebir eşleşmese bile (ve Admin de değilse) rozet "Customer" göstermeli.
            return rol == AppConstants.Roller.Admin ? "Admin" : "Customer";
        }

        private static string AvatarHarf(string adSoyad)
        {
            if (string.IsNullOrWhiteSpace(adSoyad)) return "?";
            var parcalar = adSoyad.Trim().Split(' ');
            if (parcalar.Length >= 2)
                return (parcalar[0][0].ToString() + parcalar[1][0]).ToUpper();
            return adSoyad[0].ToString().ToUpper();
        }
    }
}
