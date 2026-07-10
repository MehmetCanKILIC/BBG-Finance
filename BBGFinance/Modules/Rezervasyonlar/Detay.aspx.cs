using System;
using System.Data;
using BBGFinance.Core;
using BBGFinance.Data;

namespace BBGFinance.Modules.Rezervasyonlar
{
    public partial class Detay : AdminBase
    {
        protected string KalemlerJson = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string bookingCode = Request.QueryString["kod"];
                if (string.IsNullOrEmpty(bookingCode))
                {
                    Response.Redirect("Liste.aspx");
                    return;
                }

                YukleBaslik(bookingCode);
                YukleKalemler(bookingCode);
            }
        }

        private void YukleBaslik(string bookingCode)
        {
            var dt = RezervasyonRepository.RezervasyonBasligi(bookingCode);
            if (dt.Rows.Count == 0)
            {
                Response.Redirect("Liste.aspx");
                return;
            }

            DataRow r = dt.Rows[0];

            litBookingCode.Text = Metin(r, "BookingCode");
            litBaslik.Text      = Metin(r, "BookingCode");

            bool iptalMi = BitMetin(r, "TumSatirlarIptal");
            litDurum.Text        = iptalMi ? "Cancelled" : "Active";
            litDurum.CssClass    = "badge " + (iptalMi ? "badge-red" : "badge-green");

            litBookingDate.Text     = TarihMetin(r, "BookingDate");
            litLastModified.Text    = TarihMetin(r, "LastModifiedDate");
            litCancelDate.Text      = TarihMetin(r, "CancelDate");
            litChannel.Text         = Metin(r, "Channel");
            litAgencyRef.Text       = Metin(r, "AgencyRef");
            litBookingLabel.Text    = Metin(r, "BookingLabel");

            litCustomerName.Text    = Metin(r, "CustomerName");
            litCustomerEmail.Text   = Metin(r, "CustomerEmail");
            litCustomerPhone.Text   = Metin(r, "CustomerPhone1", "CustomerMobile");
            litCustomerCountry.Text = Metin(r, "CustomerCountry");
            litCustomerCity.Text    = Metin(r, "CustomerCity");

            litAgentName.Text       = Metin(r, "AgentName");
            litAgentEmail.Text      = Metin(r, "AgentEmail");
            litBookingAdmin.Text    = Metin(r, "BookingAdmin");
            litAccountManager.Text  = Metin(r, "AccountManager");

            litSellingPrice.Text    = TutarMetin(r, "SellingPrice");
            litCost.Text            = TutarMetin(r, "Cost");
            litCommission.Text      = TutarMetin(r, "Commission");
            litOutstanding.Text     = TutarMetin(r, "OutStandingAmount");
            litInvoiced.Text        = BitMetin(r, "Invoiced") ? "Yes" : "No";

            litDescription.Text     = Metin(r, "Description");
            litRemarks.Text         = Metin(r, "Remarks");
            litFinancialNotes.Text  = Metin(r, "FinancialNotes");
        }

        private void YukleKalemler(string bookingCode)
        {
            try
            {
                var dt = RezervasyonRepository.RezervasyonKalemleri(bookingCode);
                KalemlerJson = JsonHelper.DataTableToJson(dt);
            }
            catch
            {
                KalemlerJson = "[]";
            }
        }

        private static string Metin(DataRow r, string kolon)
        {
            return r.Table.Columns.Contains(kolon) && r[kolon] != DBNull.Value
                ? r[kolon].ToString()
                : "-";
        }

        private static string Metin(DataRow r, string kolon, string yedekKolon)
        {
            string v = Metin(r, kolon);
            return v != "-" ? v : Metin(r, yedekKolon);
        }

        private static string TarihMetin(DataRow r, string kolon)
        {
            if (!r.Table.Columns.Contains(kolon) || r[kolon] == DBNull.Value) return "-";
            return Convert.ToDateTime(r[kolon]).ToString("dd.MM.yyyy HH:mm");
        }

        private static string TutarMetin(DataRow r, string kolon)
        {
            if (!r.Table.Columns.Contains(kolon) || r[kolon] == DBNull.Value) return "-";
            return Convert.ToDecimal(r[kolon]).ToString("N2");
        }

        /// <summary>
        /// JP_ROIBEDS'te bit alanları bazen '0'/'1' metni olarak tutulduğundan
        /// Convert.ToBoolean (sadece "True"/"False" kabul eder) burada kullanılmaz.
        /// </summary>
        private static bool BitMetin(DataRow r, string kolon)
        {
            if (!r.Table.Columns.Contains(kolon) || r[kolon] == DBNull.Value) return false;
            object v = r[kolon];
            if (v is bool) return (bool)v;
            string s = v.ToString().Trim();
            return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("Evet", StringComparison.OrdinalIgnoreCase);
        }
    }
}
