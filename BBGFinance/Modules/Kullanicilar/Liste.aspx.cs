using System;
using System.Collections.Generic;
using System.Data;
using BBGFinance.Core;
using BBGFinance.Data;

namespace BBGFinance.Modules.Kullanicilar
{
    /// <summary>Tüm kullanıcıları (Admin/Musteri) listeler. Kullanici tablosu AuthDB'de,
    /// CustomerGroup etiketleri JP_ROIBEDS'teki JP_Customer'da olduğundan (ayrı veritabanları,
    /// çapraz JOIN mümkün değil) iki ayrı sorgu sonucu burada koda dahil edilir.</summary>
    public partial class Liste : AdminBase
    {
        protected string GridJson = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    var dtKullanicilar = DbHelper.ExecuteQuery(@"
                        SELECT KullaniciID, KullaniciAdi, AdSoyad, Email, Rol, CustomerGroupId,
                               AktifMi, HesapKilitliMi, SonGirisTarihi
                        FROM   dbo.Kullanici
                        ORDER BY AdSoyad");

                    var gruplar = new Dictionary<string, string>();
                    foreach (DataRow r in RezervasyonRepository.MusteriGruplari().Rows)
                        gruplar[r["CustomerGroupId"].ToString()] = r["CustomerGroup"].ToString();

                    dtKullanicilar.Columns.Add("CustomerGroupAdi", typeof(string));
                    dtKullanicilar.Columns.Add("Durum", typeof(string));

                    foreach (DataRow r in dtKullanicilar.Rows)
                    {
                        string grupId = r["CustomerGroupId"] == DBNull.Value ? null : r["CustomerGroupId"].ToString();
                        r["CustomerGroupAdi"] = grupId != null && gruplar.ContainsKey(grupId) ? gruplar[grupId] : "";

                        bool aktif = Convert.ToBoolean(r["AktifMi"]);
                        bool kilitli = Convert.ToBoolean(r["HesapKilitliMi"]);
                        r["Durum"] = kilitli ? "Locked" : (aktif ? "Active" : "Inactive");
                    }

                    GridJson = JsonHelper.DataTableToJson(dtKullanicilar);
                }
                catch
                {
                    GridJson = "[]";
                }
            }
        }
    }
}
