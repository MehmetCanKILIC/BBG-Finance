using System;
using System.Web.UI;

namespace BBGFinance
{
    public partial class ErrorPage : Page
    {
        protected string HataKodu   = "500";
        protected string HataMesaji = "An unexpected error occurred.";

        protected void Page_Load(object sender, EventArgs e)
        {
            string kod = Request.QueryString["code"];
            if (kod == "404")
            {
                HataKodu   = "404";
                HataMesaji = "The page you are looking for could not be found.";
            }
        }
    }
}
