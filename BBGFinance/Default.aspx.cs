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
    public partial class Default : AdminBase
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
                // 10 rapor sorgusu ThreadPool worker thread'lerini I/O boyunca MEŞGUL ETMEDEN
                // paralel çalışır (eski Task.Run + senkron sorgu yaklaşımının aksine).
                RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    DashboardJson = await DashboardVerisiniHazirlaAsync(bas, bit).ConfigureAwait(false);
                }));
            }
        }

        private void CozFiltreTarihleri(out DateTime bas, out DateTime bit)
        {
            // bit (üst sınır) her zaman DIŞLANAN gün olarak tutulur: BookingDate < bit
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
            var hatalar = new ConcurrentBag<string>();

            // Her rapor sorgusu KENDİ SqlConnection'ını açtığından (bkz. ReportDbHelper) birbirinden
            // bağımsızdır - sırayla değil, paralel olarak çalıştırılır. Ayrıca her sorgu KENDİ
            // try/catch'i içinde çalışır - biri hata verirse sadece o widget boş kalır.
            var ozetTask = Guvenli(async () =>
            {
                var dtOzet = await RezervasyonRepository.GenelOzet(bas, bit).ConfigureAwait(false);
                int toplamRezervasyon = 0, iptalSayisi = 0, toplamGece = 0, toplamPax = 0;
                if (dtOzet.Rows.Count > 0)
                {
                    var r = dtOzet.Rows[0];
                    toplamRezervasyon = ToInt(r["ToplamRezervasyon"]);
                    iptalSayisi       = ToInt(r["IptalSayisi"]);
                    toplamGece        = ToInt(r["ToplamGece"]);
                    toplamPax         = ToInt(r["ToplamPax"]);
                }
                decimal iptalOrani = toplamRezervasyon > 0
                    ? Math.Round((decimal)iptalSayisi / toplamRezervasyon * 100, 1)
                    : 0;

                return (object)new
                {
                    ToplamRezervasyon = toplamRezervasyon,
                    IptalSayisi       = iptalSayisi,
                    IptalOrani        = iptalOrani,
                    ToplamGece        = toplamGece,
                    ToplamPax         = toplamPax
                };
            }, new { ToplamRezervasyon = 0, IptalSayisi = 0, IptalOrani = (decimal)0, ToplamGece = 0, ToplamPax = 0 }, "ozet", hatalar);

            var finansalTask            = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.ParaBirimiBazliTutarlar(bas, bit).ConfigureAwait(false)), new object[0], "finansal", hatalar);
            var karTask                 = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.ParaBirimiBazliKar(bas, bit).ConfigureAwait(false)), new object[0], "kar", hatalar);
            var aylikTrendTask          = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.AylikTrend(bas, bit).ConfigureAwait(false)), new object[0], "aylikTrend", hatalar);
            var kanalDagilimTask        = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.KanalDagilimi(bas, bit).ConfigureAwait(false)), new object[0], "kanalDagilim", hatalar);
            var urunGrubuTask           = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.UrunGrubuDagilimi(bas, bit).ConfigureAwait(false)), new object[0], "urunGrubu", hatalar);
            var pazarDagilimTask        = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.PazarDagilimi(bas, bit).ConfigureAwait(false)), new object[0], "pazarDagilim", hatalar);
            var tedarikciTask           = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.TedarikciDagilimi(bas, bit).ConfigureAwait(false)), new object[0], "tedarikci", hatalar);
            var sonRezervasyonlarTask   = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.SonRezervasyonlar(bas, bit, 10).ConfigureAwait(false)), new object[0], "sonRezervasyonlar", hatalar);
            var yaklasanKonaklamalarTask = Guvenli(async () => (object)TabloyaÇevir(await RezervasyonRepository.YaklasanKonaklamalar(bas, bit, 10).ConfigureAwait(false)), new object[0], "yaklasanKonaklamalar", hatalar);

            await Task.WhenAll(ozetTask, finansalTask, karTask, aylikTrendTask, kanalDagilimTask,
                urunGrubuTask, pazarDagilimTask, tedarikciTask, sonRezervasyonlarTask, yaklasanKonaklamalarTask).ConfigureAwait(false);

            data["ozet"]                 = ozetTask.Result; 
            data["finansal"]             = finansalTask.Result;
            data["kar"]                  = karTask.Result;
            data["aylikTrend"]           = aylikTrendTask.Result;
            data["kanalDagilim"]         = kanalDagilimTask.Result;
            data["urunGrubu"]            = urunGrubuTask.Result;
            data["pazarDagilim"]         = pazarDagilimTask.Result;
            data["tedarikci"]            = tedarikciTask.Result;
            data["sonRezervasyonlar"]    = sonRezervasyonlarTask.Result;
            data["yaklasanKonaklamalar"] = yaklasanKonaklamalarTask.Result;

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
