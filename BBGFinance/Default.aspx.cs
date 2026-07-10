using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

                DashboardJson = DashboardVerisiniHazirla(bas, bit);
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

            bas = basVar ? basParsed.Date : DateTime.Today.AddMonths(-6).Date;
            bit = bitVar ? bitParsed.Date.AddDays(1) : DateTime.Today.AddDays(1);
        }

        private string DashboardVerisiniHazirla(DateTime bas, DateTime bit)
        {
            var data = new Dictionary<string, object>();
            var hatalar = new ConcurrentBag<string>();

            // Her rapor sorgusu KENDİ SqlConnection'ını açtığından (bkz. ReportDbHelper) birbirinden
            // bağımsızdır - sırayla değil, paralel olarak çalıştırılır. İlk açılışta sayfanın yavaş
            // gelmesinin ana nedeni bu sorguların art arda (toplamları kadar) beklenmesiydi; paralel
            // çalıştırıldığında toplam süre en yavaş tek sorgu kadar olur. Ayrıca her sorgu KENDİ
            // try/catch'i içinde çalışır - biri hata verirse sadece o widget boş kalır.
            var ozetTask = GuvenliAsync(() =>
            {
                var dtOzet = RezervasyonRepository.GenelOzet(bas, bit);
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

            var finansalTask            = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.ParaBirimiBazliTutarlar(bas, bit)), new object[0], "finansal", hatalar);
            var karTask                 = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.ParaBirimiBazliKar(bas, bit)), new object[0], "kar", hatalar);
            var aylikTrendTask          = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.AylikTrend(bas, bit)), new object[0], "aylikTrend", hatalar);
            var kanalDagilimTask        = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.KanalDagilimi(bas, bit)), new object[0], "kanalDagilim", hatalar);
            var urunGrubuTask           = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.UrunGrubuDagilimi(bas, bit)), new object[0], "urunGrubu", hatalar);
            var pazarDagilimTask        = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.PazarDagilimi(bas, bit)), new object[0], "pazarDagilim", hatalar);
            var tedarikciTask           = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.TedarikciDagilimi(bas, bit)), new object[0], "tedarikci", hatalar);
            var sonRezervasyonlarTask   = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.SonRezervasyonlar(bas, bit, 10)), new object[0], "sonRezervasyonlar", hatalar);
            var yaklasanKonaklamalarTask = GuvenliAsync(() => (object)TabloyaÇevir(RezervasyonRepository.YaklasanKonaklamalar(bas, bit, 10)), new object[0], "yaklasanKonaklamalar", hatalar);

            Task.WaitAll(ozetTask, finansalTask, karTask, aylikTrendTask, kanalDagilimTask,
                urunGrubuTask, pazarDagilimTask, tedarikciTask, sonRezervasyonlarTask, yaklasanKonaklamalarTask);

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

        private static Task<object> GuvenliAsync(Func<object> sorgu, object varsayilan, string alanAdi, ConcurrentBag<string> hatalar)
        {
            return Task.Run(() =>
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
            });
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
