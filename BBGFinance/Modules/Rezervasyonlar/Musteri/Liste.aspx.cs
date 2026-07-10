using System;
using BBGFinance.Core;
using BBGFinance.Data;

namespace BBGFinance.Modules.Rezervasyonlar.Musteri
{
    /// <summary>
    /// "My Reservations" - customer-facing reservation list. Tenant-scoped via CustomerGroupId
    /// and backed by MusteriRaporRepository, which NEVER selects Cost/Profit/Commission/Supplier
    /// columns. AuthBase (not AdminBase) - both Admin and Musteri roles may open this page, but
    /// Admin sees an empty grid since Admin has no CustomerGroupId.
    /// </summary>
    public partial class Liste : AuthBase
    {
        protected string GridJson = "[]";
        protected string FilterBaslangic = "";
        protected string FilterBitis     = "";
        protected string FilterDurum     = "";
        protected string FilterArama     = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FilterBaslangic = Request.QueryString["bas"] ?? "";
                FilterBitis     = Request.QueryString["bit"] ?? "";
                FilterDurum     = Request.QueryString["durum"] ?? "";
                FilterArama     = Request.QueryString["ara"] ?? "";

                DateTime? bas = null, bit = null;
                DateTime tmp;
                if (DateTime.TryParse(FilterBaslangic, out tmp)) bas = tmp;
                if (DateTime.TryParse(FilterBitis, out tmp)) bit = tmp;

                int? grupId = CustomerGroupId;
                if (!grupId.HasValue)
                {
                    GridJson = "[]";
                    return;
                }

                try
                {
                    var dt = MusteriRaporRepository.RezervasyonListesi(grupId.Value, bas, bit, FilterDurum, FilterArama);
                    GridJson = JsonHelper.DataTableToJson(dt);
                }
                catch
                {
                    GridJson = "[]";
                }
            }
        }
    }
}
