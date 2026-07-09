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
    }
}
