using System;
using System.Web.UI;

namespace BBGFinance
{
    public partial class ErrorPage : Page
    {
        protected string HataKodu   = "500";
        protected string HataMesaji = "Beklenmeyen bir hata oluştu.";

        protected void Page_Load(object sender, EventArgs e)
        {
            string kod = Request.QueryString["code"];
            if (kod == "404")
            {
                HataKodu   = "404";
                HataMesaji = "Aradığınız sayfa bulunamadı.";
            }
        }
    }
}
