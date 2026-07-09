<%@ Page Title="Rezervasyon Detay" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Detay.aspx.cs"
         Inherits="BBGFinance.Modules.Rezervasyonlar.Detay"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">Rezervasyon Detay</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="cphHead" runat="server">
<style>
    .info-card   { background:#fff; border:1px solid #e0e0e0; border-radius:6px; padding:24px; margin-bottom:20px; }
    .info-card h3{ font-size:15px; font-weight:700; color:#333; margin:0 0 16px; padding-bottom:8px; border-bottom:2px solid #00695C; }
    .info-grid   { display:grid; grid-template-columns:repeat(4,1fr); gap:16px; }
    .info-item label { display:block; font-size:12px; font-weight:600; color:#555; margin-bottom:3px; }
    .info-item span  { font-size:13px; color:#222; }
    .btn-geri    { background:#757575; color:#fff; border:none; border-radius:4px; padding:10px 20px; font-size:14px; cursor:pointer; text-decoration:none; display:inline-block; }
    .btn-geri:hover { background:#616161; color:#fff; text-decoration:none; }
    .badge-green  { background:#e8f5e9; color:#2e7d32; }
    .badge-red    { background:#ffebee; color:#c62828; }
    @media(max-width:900px) { .info-grid { grid-template-columns:repeat(2,1fr); } }
    @media(max-width:600px) { .info-grid { grid-template-columns:1fr; } }
</style>
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="cphBreadcrumb" runat="server">
    <a href="<%= ResolveUrl("~/Default.aspx") %>">Dashboard</a>
    <span> / </span>
    <a href="<%= ResolveUrl("~/Modules/Rezervasyonlar/Liste.aspx") %>">Rezervasyonlar</a>
    <span> / </span>
    <span><asp:Literal ID="litBookingCode" runat="server" /></span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server">
    Rezervasyon <asp:Literal ID="litBaslik" runat="server" />
    <asp:Label ID="litDurum" runat="server" />
</asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    Rezervasyon başlığı ve kalem detayları (salt okunur).
</asp:Content>

<asp:Content ID="cPageActions" ContentPlaceHolderID="cphPageActions" runat="server">
    <a href="<%= ResolveUrl("~/Modules/Rezervasyonlar/Liste.aspx") %>" class="btn-geri">&#8592; Listeye Dön</a>
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <!-- Genel Bilgiler -->
    <div class="info-card">
        <h3>Genel Bilgiler</h3>
        <div class="info-grid">
            <div class="info-item"><label>Rezervasyon Tarihi</label><span><asp:Literal ID="litBookingDate" runat="server" /></span></div>
            <div class="info-item"><label>Son Güncelleme</label><span><asp:Literal ID="litLastModified" runat="server" /></span></div>
            <div class="info-item"><label>İptal Tarihi</label><span><asp:Literal ID="litCancelDate" runat="server" /></span></div>
            <div class="info-item"><label>Kanal</label><span><asp:Literal ID="litChannel" runat="server" /></span></div>
            <div class="info-item"><label>Acente Referansı</label><span><asp:Literal ID="litAgencyRef" runat="server" /></span></div>
            <div class="info-item"><label>Rezervasyon Etiketi</label><span><asp:Literal ID="litBookingLabel" runat="server" /></span></div>
        </div>
    </div>

    <!-- Müşteri Bilgileri -->
    <div class="info-card">
        <h3>Müşteri Bilgileri</h3>
        <div class="info-grid">
            <div class="info-item"><label>Ad Soyad</label><span><asp:Literal ID="litCustomerName" runat="server" /></span></div>
            <div class="info-item"><label>E-posta</label><span><asp:Literal ID="litCustomerEmail" runat="server" /></span></div>
            <div class="info-item"><label>Telefon</label><span><asp:Literal ID="litCustomerPhone" runat="server" /></span></div>
            <div class="info-item"><label>Ülke / Şehir</label><span><asp:Literal ID="litCustomerCountry" runat="server" /> / <asp:Literal ID="litCustomerCity" runat="server" /></span></div>
        </div>
    </div>

    <!-- Acente / Yönetici -->
    <div class="info-card">
        <h3>Acente / Yönetici</h3>
        <div class="info-grid">
            <div class="info-item"><label>Acente / Ajan</label><span><asp:Literal ID="litAgentName" runat="server" /></span></div>
            <div class="info-item"><label>Ajan E-posta</label><span><asp:Literal ID="litAgentEmail" runat="server" /></span></div>
            <div class="info-item"><label>Rezervasyon Yetkilisi</label><span><asp:Literal ID="litBookingAdmin" runat="server" /></span></div>
            <div class="info-item"><label>Hesap Yöneticisi</label><span><asp:Literal ID="litAccountManager" runat="server" /></span></div>
        </div>
    </div>

    <!-- Finansal Özet -->
    <div class="info-card">
        <h3>Finansal Özet</h3>
        <div class="info-grid">
            <div class="info-item"><label>Satış Tutarı</label><span><asp:Literal ID="litSellingPrice" runat="server" /></span></div>
            <div class="info-item"><label>Maliyet</label><span><asp:Literal ID="litCost" runat="server" /></span></div>
            <div class="info-item"><label>Komisyon</label><span><asp:Literal ID="litCommission" runat="server" /></span></div>
            <div class="info-item"><label>Bekleyen Tahsilat</label><span><asp:Literal ID="litOutstanding" runat="server" /></span></div>
            <div class="info-item"><label>Faturalandı mı</label><span><asp:Literal ID="litInvoiced" runat="server" /></span></div>
        </div>
    </div>

    <!-- Notlar -->
    <div class="info-card">
        <h3>Notlar</h3>
        <div class="info-grid" style="grid-template-columns:1fr;">
            <div class="info-item"><label>Açıklama</label><span><asp:Literal ID="litDescription" runat="server" /></span></div>
            <div class="info-item"><label>Notlar</label><span><asp:Literal ID="litRemarks" runat="server" /></span></div>
            <div class="info-item"><label>Finansal Notlar</label><span><asp:Literal ID="litFinancialNotes" runat="server" /></span></div>
        </div>
    </div>

    <!-- Kalemler -->
    <div class="info-card">
        <h3>Rezervasyon Kalemleri</h3>
        <div id="gridKalemler"></div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var kalemler = <%=KalemlerJson%>;

    DevExpress.ui.dxDataGrid({
        dataSource: kalemler,
        showBorders: true,
        rowAlternationEnabled: true,
        columnAutoWidth: true,
        allowColumnResizing: true,
        paging: { pageSize: 10 },
        columns: [
            { dataField: 'ServiceName',      caption: 'Hizmet' },
            { dataField: 'ProductGroupName', caption: 'Ürün Grubu' },
            { dataField: 'SupplierName',     caption: 'Tedarikçi' },
            { dataField: 'Market',           caption: 'Pazar', width: 100 },
            { dataField: 'BeginTravelDate',  caption: 'Başlangıç', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
            { dataField: 'EndTravelDate',    caption: 'Bitiş', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
            { dataField: 'NightsNumber',     caption: 'Gece', width: 70, dataType: 'number' },
            { dataField: 'PaxNumber',        caption: 'Pax', width: 70, dataType: 'number' },
            { dataField: 'SellingPrice',     caption: 'Satış', width: 110, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'SellCurrency',     caption: 'P.B.', width: 60 },
            { dataField: 'Cost',             caption: 'Maliyet', width: 110, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'Commission',       caption: 'Komisyon', width: 100, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'Profit',           caption: 'Kâr', width: 100, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'LineCancelled',    caption: 'Durum', width: 80,
              cellTemplate: function (c, o) {
                  $('<span>').addClass('badge ' + (o.value ? 'badge-red' : 'badge-green')).text(o.value ? 'İptal' : 'Aktif').appendTo(c);
              }
            }
        ],
        summary: {
            totalItems: [
                { column: 'SellingPrice', summaryType: 'sum', valueFormat: { type: 'fixedPoint', precision: 2 }, displayFormat: 'Toplam: {0}' },
                { column: 'Profit', summaryType: 'sum', valueFormat: { type: 'fixedPoint', precision: 2 }, displayFormat: 'Toplam: {0}' }
            ]
        }
    }, document.getElementById('gridKalemler'));
</script>
</asp:Content>
