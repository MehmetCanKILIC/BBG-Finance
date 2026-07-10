using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.UI;
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
                lblTarih.Text = DateTime.Now.ToString("dd MMMM yyyy, dddd",
                                    new System.Globalization.CultureInfo("en-US"));

                DateTime bas, bit;
                CozFiltreTarihleri(out bas, out bit);
                FilterBaslangic = bas.ToString("yyyy-MM-dd");
                FilterBitis     = bit.AddDays(-1).ToString("yyyy-MM-dd");

                // @Page Async="true" ile birlikte kayıtlı bu görev bitene kadar ASP.NET
                // sayfayı render etmez - DashboardJson dolu olarak render'a girer. Gerçek
                // async ADO.NET (bkz. ReportDbHelper.ExecuteQueryAsync) kullanıldığından bu
                // 7 rapor sorgusu ThreadPool worker thread'lerini I/O boyunca MEŞGUL ETMEDEN
                // paralel çalışır (eski Task.Run + senkron sorgu yaklaşımının aksine).
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    DashboardJson = await DashboardVerisiniHazirlaAsync(bas, bit).ConfigureAwait(false);
                }));
            }
        }

        private void CozFiltreTarihleri(out DateTime bas, out DateTime bit)
        {
            string basStr = Request.QueryString["bas"];
            string bitStr = Request.QueryString["bit"];

            DateTime basParsed, bitParsed;
            bool basVar = DateTime.TryParse(basStr, out basParsed);
            bool bitVar = DateTime.TryParse(bitStr, out bitParsed);

            bas = basVar ? basParsed.Date : DateTime.Today.AddMonths(-2).Date;
            bit = bitVar ? bitParsed.Date.AddDays(1) : DateTime.Today.AddDays(1);
        }

        private async Task<string> DashboardVerisiniHazirlaAsync(DateTime bas, DateTime bit)
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
            var hatalar = new ConcurrentBag<string>();

            // Her rapor sorgusu KENDİ SqlConnection'ını açtığından (bkz. ReportDbHelper) birbirinden
            // bağımsızdır - sırayla değil, paralel olarak çalıştırılır. Ayrıca her sorgu KENDİ
            // try/catch'i içinde çalışır - biri hata verirse sadece o widget boş kalır.
            var ozetTask = Guvenli(async () =>
            {
                var dtOzet = await MusteriRaporRepository.GenelOzet(customerGroupId, bas, bit).ConfigureAwait(false);
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

            var satisTask           = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.ParaBirimiBazliSatis(customerGroupId, bas, bit).ConfigureAwait(false)), new object[0], "satis", hatalar);
            var aylikTrendTask      = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.AylikTrend(customerGroupId, bas, bit).ConfigureAwait(false)), new object[0], "aylikTrend", hatalar);
            var bolgeDagilimTask    = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.BolgeDagilimi(customerGroupId, bas, bit).ConfigureAwait(false)), new object[0], "bolgeDagilim", hatalar);
            var odaTipiDagilimTask  = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.OdaTipiDagilimi(customerGroupId, bas, bit).ConfigureAwait(false)), new object[0], "odaTipiDagilim", hatalar);
            var yasGrubuDagilimTask = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.YasGrubuDagilimi(customerGroupId, bas, bit).ConfigureAwait(false)), new object[0], "yasGrubuDagilim", hatalar);
            var milliyetDagilimTask = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.MilliyetDagilimi(customerGroupId, bas, bit).ConfigureAwait(false)), new object[0], "milliyetDagilim", hatalar);
            var bekleyenGirislerTask = Guvenli(async () => (object)TabloyaÇevir(await MusteriRaporRepository.BekleyenGirisler(customerGroupId, bas, bit, 20).ConfigureAwait(false)), new object[0], "bekleyenGirisler", hatalar);

            await Task.WhenAll(ozetTask, satisTask, aylikTrendTask, bolgeDagilimTask, odaTipiDagilimTask,
                yasGrubuDagilimTask, milliyetDagilimTask, bekleyenGirislerTask).ConfigureAwait(false);

            data["ozet"]             = ozetTask.Result;
            data["satis"]            = satisTask.Result;
            data["aylikTrend"]       = aylikTrendTask.Result;
            data["bolgeDagilim"]     = bolgeDagilimTask.Result;
            data["odaTipiDagilim"]   = odaTipiDagilimTask.Result;
            data["yasGrubuDagilim"]  = yasGrubuDagilimTask.Result;
            data["milliyetDagilim"]  = milliyetDagilimTask.Result;
            data["bekleyenGirisler"] = bekleyenGirislerTask.Result;

            if (hatalar.Count > 0)
            {
                data["hata"] = string.Join(" | ", hatalar);
            }

            return JsonConvert.SerializeObject(data);
        }

        private static async Task<object> Guvenli(Func<Task<object>> sorgu, object varsayilan, string alanAdi, ConcurrentBag<string> hatalar)
        {
            try
            {
                return await sorgu().ConfigureAwait(false);
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
