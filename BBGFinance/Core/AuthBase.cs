using System;
using System.Web;
using System.Web.UI;

namespace BBGFinance.Core
{
    public abstract class AuthBase : Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!SessionManager.OturumAcikMi)
            {
                Response.Redirect("~/Login.aspx?ReturnUrl=" +
                    HttpUtility.UrlEncode(Request.RawUrl), true);
            }
        }

        protected bool AdminMi
        {
            get { return SessionManager.Rol == AppConstants.Roller.Admin; }
        }

        /// <summary>Müşteri (acente) girişi mi? True ise veri katmanı CustomerGroupId ile
        /// filtrelenmeli ve maliyet/kâr/tedarikçi alanları hiçbir sorguya/JSON'a dahil edilmemeli.</summary>
        protected bool MusteriMi
        {
            get { return SessionManager.Rol == AppConstants.Roller.Musteri; }
        }

        /// <summary>Admin için NULL (filtresiz); Musteri için oturumdaki CustomerGroupId.</summary>
        protected int? CustomerGroupId
        {
            get { return AdminMi ? null : SessionManager.CustomerGroupId; }
        }
    }

    /// <summary>
    /// SADECE Admin rolünün erişebildiği sayfalar için (ciro/kâr/tedarikçi/komisyon gibi
    /// müşterilerin asla görmemesi gereken verileri içeren tüm sayfalar bundan türemelidir).
    /// Musteri rolü buraya URL ile doğrudan gelmeye çalışırsa MusteriDashboard.aspx'e atılır.
    /// </summary>
    public abstract class AdminBase : AuthBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!AdminMi)
            {
                Response.Redirect("~/MusteriDashboard.aspx", true);
            }
        }
    }
}
