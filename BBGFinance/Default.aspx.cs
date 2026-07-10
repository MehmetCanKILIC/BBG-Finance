using System;
using System.Collections.Generic;
using System.Data;
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

            try
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

                data["ozet"] = new
                {
                    ToplamRezervasyon = toplamRezervasyon,
                    IptalSayisi       = iptalSayisi,
                    IptalOrani        = iptalOrani,
                    ToplamGece        = toplamGece,
                    ToplamPax         = toplamPax
                };

                data["finansal"]      = TabloyaÇevir(RezervasyonRepository.ParaBirimiBazliTutarlar(bas, bit));
                data["kar"]           = TabloyaÇevir(RezervasyonRepository.ParaBirimiBazliKar(bas, bit));
                data["aylikTrend"]    = TabloyaÇevir(RezervasyonRepository.AylikTrend(bas, bit));
                data["kanalDagilim"]  = TabloyaÇevir(RezervasyonRepository.KanalDagilimi(bas, bit));
                data["urunGrubu"]     = TabloyaÇevir(RezervasyonRepository.UrunGrubuDagilimi(bas, bit));
                data["pazarDagilim"]  = TabloyaÇevir(RezervasyonRepository.PazarDagilimi(bas, bit));
                data["tedarikci"]     = TabloyaÇevir(RezervasyonRepository.TedarikciDagilimi(bas, bit));
                data["sonRezervasyonlar"]    = TabloyaÇevir(RezervasyonRepository.SonRezervasyonlar(10));
                data["yaklasanKonaklamalar"] = TabloyaÇevir(RezervasyonRepository.YaklasanKonaklamalar(10));
            }
            catch (Exception ex)
            {
                data["hata"] = ex.Message;
                data["ozet"] = new { ToplamRezervasyon = 0, IptalSayisi = 0, IptalOrani = 0, ToplamGece = 0, ToplamPax = 0 };
                data["finansal"] = new object[0];
                data["kar"] = new object[0];
                data["aylikTrend"] = new object[0];
                data["kanalDagilim"] = new object[0];
                data["urunGrubu"] = new object[0];
                data["pazarDagilim"] = new object[0];
                data["tedarikci"] = new object[0];
                data["sonRezervasyonlar"] = new object[0];
                data["yaklasanKonaklamalar"] = new object[0];
            }

            return JsonConvert.SerializeObject(data);
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
