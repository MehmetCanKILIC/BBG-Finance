using System;
using BBGFinance.Core;
using BBGFinance.Data;

namespace BBGFinance.Modules.Rezervasyonlar
{
    public partial class Liste : AuthBase
    {
        protected string GridJson    = "[]";
        protected string KanallarJson = "[]";
        protected string FilterBaslangic = "";
        protected string FilterBitis     = "";
        protected string FilterDurum     = "";
        protected string FilterKanal     = "";
        protected string FilterArama     = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FilterBaslangic = Request.QueryString["bas"] ?? "";
                FilterBitis     = Request.QueryString["bit"] ?? "";
                FilterDurum     = Request.QueryString["durum"] ?? "";
                FilterKanal     = Request.QueryString["kanal"] ?? "";
                FilterArama     = Request.QueryString["ara"] ?? "";

                DateTime? bas = null, bit = null;
                DateTime tmp;
                if (DateTime.TryParse(FilterBaslangic, out tmp)) bas = tmp;
                if (DateTime.TryParse(FilterBitis, out tmp)) bit = tmp;

                try
                {
                    var dt = RezervasyonRepository.RezervasyonListesi(bas, bit, FilterDurum, FilterKanal, FilterArama);
                    GridJson = JsonHelper.DataTableToJson(dt);

                    var dtKanal = RezervasyonRepository.RezervasyonKanallari();
                    KanallarJson = JsonHelper.DataTableToJson(dtKanal);
                }
                catch
                {
                    GridJson = "[]";
                    KanallarJson = "[]";
                }
            }
        }
    }
}
