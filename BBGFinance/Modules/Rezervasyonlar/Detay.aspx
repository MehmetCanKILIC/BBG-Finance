<%@ Page Title="Reservation Details" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Detay.aspx.cs"
         Inherits="BBGFinance.Modules.Rezervasyonlar.Detay"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">Reservation Details</asp:Content>

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
    <a href="<%= ResolveUrl("~/Modules/Rezervasyonlar/Liste.aspx") %>">Reservations</a>
    <span> / </span>
    <span><asp:Literal ID="litBookingCode" runat="server" /></span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server">
    Reservation <asp:Literal ID="litBaslik" runat="server" />
    <asp:Label ID="litDurum" runat="server" />
</asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    Reservation header and line item details (read-only).
</asp:Content>

<asp:Content ID="cPageActions" ContentPlaceHolderID="cphPageActions" runat="server">
    <a href="<%= ResolveUrl("~/Modules/Rezervasyonlar/Liste.aspx") %>" class="btn-geri">&#8592; Back to List</a>
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <!-- General Info -->
    <div class="info-card">
        <h3>General Info</h3>
        <div class="info-grid">
            <div class="info-item"><label>Booking Date</label><span><asp:Literal ID="litBookingDate" runat="server" /></span></div>
            <div class="info-item"><label>Last Modified</label><span><asp:Literal ID="litLastModified" runat="server" /></span></div>
            <div class="info-item"><label>Cancel Date</label><span><asp:Literal ID="litCancelDate" runat="server" /></span></div>
            <div class="info-item"><label>Channel</label><span><asp:Literal ID="litChannel" runat="server" /></span></div>
            <div class="info-item"><label>Agency Reference</label><span><asp:Literal ID="litAgencyRef" runat="server" /></span></div>
            <div class="info-item"><label>Booking Label</label><span><asp:Literal ID="litBookingLabel" runat="server" /></span></div>
        </div>
    </div>

    <!-- Customer Info -->
    <div class="info-card">
        <h3>Customer Info</h3>
        <div class="info-grid">
            <div class="info-item"><label>Full Name</label><span><asp:Literal ID="litCustomerName" runat="server" /></span></div>
            <div class="info-item"><label>Email</label><span><asp:Literal ID="litCustomerEmail" runat="server" /></span></div>
            <div class="info-item"><label>Phone</label><span><asp:Literal ID="litCustomerPhone" runat="server" /></span></div>
            <div class="info-item"><label>Country / City</label><span><asp:Literal ID="litCustomerCountry" runat="server" /> / <asp:Literal ID="litCustomerCity" runat="server" /></span></div>
        </div>
    </div>

    <!-- Agent / Manager -->
    <div class="info-card">
        <h3>Agent / Manager</h3>
        <div class="info-grid">
            <div class="info-item"><label>Agency / Agent</label><span><asp:Literal ID="litAgentName" runat="server" /></span></div>
            <div class="info-item"><label>Agent Email</label><span><asp:Literal ID="litAgentEmail" runat="server" /></span></div>
            <div class="info-item"><label>Booking Admin</label><span><asp:Literal ID="litBookingAdmin" runat="server" /></span></div>
            <div class="info-item"><label>Account Manager</label><span><asp:Literal ID="litAccountManager" runat="server" /></span></div>
        </div>
    </div>

    <!-- Financial Summary -->
    <div class="info-card">
        <h3>Financial Summary</h3>
        <div class="info-grid">
            <div class="info-item"><label>Selling Price</label><span><asp:Literal ID="litSellingPrice" runat="server" /></span></div>
            <div class="info-item"><label>Cost</label><span><asp:Literal ID="litCost" runat="server" /></span></div>
            <div class="info-item"><label>Commission</label><span><asp:Literal ID="litCommission" runat="server" /></span></div>
            <div class="info-item"><label>Outstanding Amount</label><span><asp:Literal ID="litOutstanding" runat="server" /></span></div>
            <div class="info-item"><label>Invoiced</label><span><asp:Literal ID="litInvoiced" runat="server" /></span></div>
        </div>
    </div>

    <!-- Notes -->
    <div class="info-card">
        <h3>Notes</h3>
        <div class="info-grid" style="grid-template-columns:1fr;">
            <div class="info-item"><label>Description</label><span><asp:Literal ID="litDescription" runat="server" /></span></div>
            <div class="info-item"><label>Remarks</label><span><asp:Literal ID="litRemarks" runat="server" /></span></div>
            <div class="info-item"><label>Financial Notes</label><span><asp:Literal ID="litFinancialNotes" runat="server" /></span></div>
        </div>
    </div>

    <!-- Line Items -->
    <div class="info-card">
        <h3>Reservation Line Items</h3>
        <div id="gridKalemler"></div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var kalemler = <%=KalemlerJson%>;

    if (typeof DevExpress === 'undefined' || !DevExpress.ui) {
        console.error('DevExtreme failed to load - check access to cdn3.devexpress.com / ajax.googleapis.com / cdn.jsdelivr.net.');
        document.getElementById('gridKalemler').innerHTML =
            '<div style="padding:16px;color:#C0392B;font-size:13px;">The line item table could not load (no DevExtreme CDN access).</div>';
    } else {
        dxOlustur(DevExpress.ui.dxDataGrid, {
            dataSource: kalemler,
            showBorders: true,
            rowAlternationEnabled: true,
            columnAutoWidth: true,
            allowColumnResizing: true,
            paging: { pageSize: 10 },
            columns: [
                { dataField: 'ServiceName',      caption: 'Service' },
                { dataField: 'ProductGroupName', caption: 'Product Group' },
                { dataField: 'SupplierName',     caption: 'Supplier' },
                { dataField: 'Market',           caption: 'Market', width: 100 },
                { dataField: 'BeginTravelDate',  caption: 'Check-in', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
                { dataField: 'EndTravelDate',    caption: 'Check-out', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
                { dataField: 'NightsNumber',     caption: 'Nights', width: 70, dataType: 'number' },
                { dataField: 'PaxNumber',        caption: 'Pax', width: 70, dataType: 'number' },
                { dataField: 'SellingPrice',     caption: 'Selling Price', width: 110, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'SellCurrency',     caption: 'Cur.', width: 60 },
                { dataField: 'Cost',             caption: 'Cost', width: 110, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'Commission',       caption: 'Commission', width: 100, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'Profit',           caption: 'Profit', width: 100, dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'LineIptalMi',      caption: 'Status', width: 80,
                  cellTemplate: function (c, o) {
                      $('<span>').addClass('badge ' + (o.value ? 'badge-red' : 'badge-green')).text(o.value ? 'Cancelled' : 'Active').appendTo(c);
                  }
                }
            ],
            summary: {
                totalItems: [
                    { column: 'SellingPrice', summaryType: 'sum', valueFormat: { type: 'fixedPoint', precision: 2 }, displayFormat: 'Total: {0}' },
                    { column: 'Profit', summaryType: 'sum', valueFormat: { type: 'fixedPoint', precision: 2 }, displayFormat: 'Total: {0}' }
                ]
            }
        }, document.getElementById('gridKalemler'));
    }
</script>
</asp:Content>
