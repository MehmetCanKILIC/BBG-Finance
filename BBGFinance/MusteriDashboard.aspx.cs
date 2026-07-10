using System;
using System.Collections.Generic;
using System.Data;
using BBGFinance.Core;
using BBGFinance.Data;
using Newtonsoft.Json;

namespace BBGFinance
{
    /// <summary>
    /// Simplified dashboard for the Customer (agency) role. Cost/Profit/Commission/Supplier
    /// information is NEVER present on this page or in the MusteriRaporRepository queries behind it.
    /// Admin can open this page too (data comes back empty since Admin has no CustomerGroupId);
    /// the Customer role, in turn, is blocked from Default.aspx / Modules/Rezervasyonlar by
    /// AdminBase and only ever sees this page.
    /// </summary>
    public partial class MusteriDashboard : AuthBase
    {
        protected string DashboardJson = "{}";
        protected string FilterBaslangic = "";
        protected string FilterBitis     = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblAdSoyad.Text = SessionManager.AdSoyad;
                lblTarih.Text   = DateTime.Now.ToString("dd MMMM yyyy, dddd",
                                    new System.Globalization.CultureInfo("en-US"));

                DateTime bas, bit;
                CozFiltreTarihleri(out bas, out bit);
                FilterBaslangic = bas.ToString("yyyy-MM-dd");
                FilterBitis     = bit.AddDays(-1).ToString("yyyy-MM-dd");

                DashboardJson = DashboardVerisiniHazirla(bas, bit);
            }
        }

        private void CozFiltreTarihleri(out DateTime bas, out DateTime bit)
        {
            string basStr = Request.QueryString["bas"];
            string bitStr = Request.QueryString["bit"];

            DateTime basParsed, bitParsed;
            bool basVar = DateTime.TryParse(basStr, out basParsed);
            bool bitVar = DateTime.TryParse(bitStr, out bitParsed);

            bas = basVar ? basParsed.Date : DateTime.Today.AddMonths(-6).Date;
            bit = bitVar ? bitParsed.Date.AddDays(1) : DateTime.Today.AddDays(1);
        }

        private string DashboardVerisiniHazirla(DateTime bas, DateTime bit)
        {
            var data = new Dictionary<string, object>();

            // A user with no CustomerGroupId (e.g. Admin, or a misconfigured Customer account)
            // never runs any JP_ROIBEDS query for this page - it simply gets an empty result.
            int? grupId = CustomerGroupId;
            if (!grupId.HasValue)
            {
                data["yapilandirmaEksik"] = true;
                data["ozet"] = new { ToplamRezervasyon = 0, ToplamGece = 0, ToplamPax = 0 };
                data["satis"] = new object[0];
                data["aylikTrend"] = new object[0];
                data["bolgeDagilim"] = new object[0];
                data["odaTipiDagilim"] = new object[0];
                data["yasGrubuDagilim"] = new object[0];
                data["milliyetDagilim"] = new object[0];
                data["bekleyenGirisler"] = new object[0];
                return JsonConvert.SerializeObject(data);
            }

            int customerGroupId = grupId.Value;
            var hatalar = new List<string>();

            // Each query runs inside its OWN try/catch - if one of them fails (e.g. a legacy
            // column type mismatch), only that widget stays empty; every other successfully
            // loaded section (summary, region, nationality, etc.) is preserved.
            data["ozet"] = Guvenli(() =>
            {
                var dtOzet = MusteriRaporRepository.GenelOzet(customerGroupId, bas, bit);
                int toplamRezervasyon = 0, toplamGece = 0, toplamPax = 0;
                if (dtOzet.Rows.Count > 0)
                {
                    var r = dtOzet.Rows[0];
                    toplamRezervasyon = ToInt(r["ToplamRezervasyon"]);
                    toplamGece        = ToInt(r["ToplamGece"]);
                    toplamPax         = ToInt(r["ToplamPax"]);
                }
                return (object)new { ToplamRezervasyon = toplamRezervasyon, ToplamGece = toplamGece, ToplamPax = toplamPax };
            }, new { ToplamRezervasyon = 0, ToplamGece = 0, ToplamPax = 0 }, "ozet", hatalar);

            data["satis"]            = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.ParaBirimiBazliSatis(customerGroupId, bas, bit)), new object[0], "satis", hatalar);
            data["aylikTrend"]       = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.AylikTrend(customerGroupId, bas, bit)), new object[0], "aylikTrend", hatalar);
            data["bolgeDagilim"]     = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.BolgeDagilimi(customerGroupId, bas, bit)), new object[0], "bolgeDagilim", hatalar);
            data["odaTipiDagilim"]   = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.OdaTipiDagilimi(customerGroupId, bas, bit)), new object[0], "odaTipiDagilim", hatalar);
            data["yasGrubuDagilim"]  = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.YasGrubuDagilimi(customerGroupId, bas, bit)), new object[0], "yasGrubuDagilim", hatalar);
            data["milliyetDagilim"]  = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.MilliyetDagilimi(customerGroupId, bas, bit)), new object[0], "milliyetDagilim", hatalar);
            data["bekleyenGirisler"] = Guvenli(() => (object)TabloyaÇevir(MusteriRaporRepository.BekleyenGirisler(customerGroupId, 20)), new object[0], "bekleyenGirisler", hatalar);

            if (hatalar.Count > 0)
            {
                data["hata"] = string.Join(" | ", hatalar);
            }

            return JsonConvert.SerializeObject(data);
        }

        private static object Guvenli(Func<object> sorgu, object varsayilan, string alanAdi, List<string> hatalar)
        {
            try
            {
                return sorgu();
            }
            catch (Exception ex)
            {
                hatalar.Add(alanAdi + ": " + ex.Message);
                return varsayilan;
            }
        }

        private static List<Dictionary<string, object>> TabloyaÇevir(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    object val = row[col];
                    if (val == DBNull.Value) dict[col.ColumnName] = null;
                    else if (val is DateTime) dict[col.ColumnName] = ((DateTime)val).ToString("yyyy-MM-dd");
                    else dict[col.ColumnName] = val;
                }
                list.Add(dict);
            }
            return list;
        }

        private static int ToInt(object val)
        {
            return val == null || val == DBNull.Value ? 0 : Convert.ToInt32(val);
        }
    }
}
