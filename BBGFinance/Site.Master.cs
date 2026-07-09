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

        private static string RolEtiket(string rol)
        {
            switch (rol)
            {
                case AppConstants.Roller.Admin: return "Yönetici";
                default:                        return "Kullanıcı";
            }
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
