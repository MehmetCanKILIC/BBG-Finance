using System;
using System.Collections.Generic;
using System.Data;
using BBGFinance.Core;
using BBGFinance.Data;
using Newtonsoft.Json;

namespace BBGFinance
{
    /// <summary>
    /// Musteri (acente) rolü için sadeleştirilmiş dashboard. Cost/Profit/Commission/Tedarikçi
    /// bilgisi bu sayfada ve arkasındaki MusteriRaporRepository sorgularında KESİNLİKLE yoktur.
    /// Admin bu sayfaya erişebilir (kendi CustomerGroupId'si olmadığından veri boş döner);
    /// Musteri rolü ise Default.aspx / Modules/Rezervasyonlar sayfalarına AdminBase tarafından
    /// sokulmaz, sadece burayı görür.
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
                                    new System.Globalization.CultureInfo("tr-TR"));

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

            // CustomerGroupId'si olmayan bir kullanıcı (örn. Admin ya da yanlış yapılandırılmış
            // bir Musteri hesabı) için hiçbir JP_ROIBEDS sorgusu ÇALIŞTIRILMAZ - boş sonuç döner.
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

            try
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

                data["ozet"] = new
                {
                    ToplamRezervasyon = toplamRezervasyon,
                    ToplamGece        = toplamGece,
                    ToplamPax         = toplamPax
                };

                data["satis"]            = TabloyaÇevir(MusteriRaporRepository.ParaBirimiBazliSatis(customerGroupId, bas, bit));
                data["aylikTrend"]       = TabloyaÇevir(MusteriRaporRepository.AylikTrend(customerGroupId, bas, bit));
                data["bolgeDagilim"]     = TabloyaÇevir(MusteriRaporRepository.BolgeDagilimi(customerGroupId, bas, bit));
                data["odaTipiDagilim"]   = TabloyaÇevir(MusteriRaporRepository.OdaTipiDagilimi(customerGroupId, bas, bit));
                data["yasGrubuDagilim"]  = TabloyaÇevir(MusteriRaporRepository.YasGrubuDagilimi(customerGroupId, bas, bit));
                data["milliyetDagilim"]  = TabloyaÇevir(MusteriRaporRepository.MilliyetDagilimi(customerGroupId, bas, bit));
                data["bekleyenGirisler"] = TabloyaÇevir(MusteriRaporRepository.BekleyenGirisler(customerGroupId, 20));
            }
            catch (Exception ex)
            {
                data["hata"] = ex.Message;
                data["ozet"] = new { ToplamRezervasyon = 0, ToplamGece = 0, ToplamPax = 0 };
                data["satis"] = new object[0];
                data["aylikTrend"] = new object[0];
                data["bolgeDagilim"] = new object[0];
                data["odaTipiDagilim"] = new object[0];
                data["yasGrubuDagilim"] = new object[0];
                data["milliyetDagilim"] = new object[0];
                data["bekleyenGirisler"] = new object[0];
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
