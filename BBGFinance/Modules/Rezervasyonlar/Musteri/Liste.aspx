<%@ Page Title="My Reservations" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Liste.aspx.cs"
         Inherits="BBGFinance.Modules.Rezervasyonlar.Musteri.Liste"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">My Reservations</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="cphHead" runat="server">
<style>
    .filter-bar { background:#fff; border:1px solid #e0e0e0; border-radius:6px; padding:16px 20px; margin-bottom:18px; }
    .filter-row  { display:flex; flex-wrap:wrap; gap:12px; align-items:flex-end; }
    .filter-item { display:flex; flex-direction:column; min-width:160px; }
    .filter-item label { font-size:12px; font-weight:600; color:#555; margin-bottom:4px; }
    .filter-actions { display:flex; gap:8px; margin-top:20px; }
    .btn-search { background:#C2185B; color:#fff; border:none; border-radius:4px; padding:7px 18px; cursor:pointer; font-size:13px; }
    .btn-clear  { background:#757575; color:#fff; border:none; border-radius:4px; padding:7px 14px; cursor:pointer; font-size:13px; }
    .badge-green  { background:#e8f5e9; color:#2e7d32; }
    .badge-red    { background:#ffebee; color:#c62828; }
</style>
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="cphBreadcrumb" runat="server">
    <a href="<%= ResolveUrl("~/MusteriDashboard.aspx") %>">Dashboard</a>
    <span> / </span>
    <span>My Reservations</span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server"></asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    Full list of your reservations - hotel, dates, guests and pricing (read-only).
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <div class="filter-bar">
        <div class="filter-row">
            <div class="filter-item">
                <label>Sale Date From</label>
                <div id="dxBaslangicTarihi"></div>
            </div>
            <div class="filter-item">
                <label>Sale Date To</label>
                <div id="dxBitisTarihi"></div>
            </div>
            <div class="filter-item">
                <label>Status</label>
                <div id="dxDurum"></div>
            </div>
            <div class="filter-item" style="min-width:220px;">
                <label>Search (Booking No / Hotel)</label>
                <div id="dxArama"></div>
            </div>
            <div class="filter-actions">
                <button type="button" class="btn-search" onclick="aramayiUygula()">Search</button>
                <button type="button" class="btn-clear"  onclick="filtreleriTemizle()">Clear</button>
            </div>
        </div>
    </div>

    <div id="gridListesi"></div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var gridData = <%=GridJson%>;

    var initBaslangic = '<%=FilterBaslangic%>';
    var initBitis     = '<%=FilterBitis%>';
    var initDurum     = '<%=FilterDurum%>';
    var initArama     = '<%=FilterArama%>';

    var dxBaslangic, dxBitis, dxDurum, dxArama;

    if (typeof DevExpress === 'undefined' || !DevExpress.ui) {
        console.error('DevExtreme failed to load - check access to cdn3.devexpress.com / ajax.googleapis.com / cdn.jsdelivr.net.');
        document.getElementById('gridListesi').innerHTML =
            '<div style="padding:16px;color:#C0392B;font-size:13px;">The list widget could not load (no DevExtreme CDN access).</div>';
    } else {
        dxBaslangic = dxOlustur(DevExpress.ui.dxDateBox, {
            displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
            placeholder: 'dd.mm.yyyy', value: initBaslangic || null
        }, document.getElementById('dxBaslangicTarihi'));

        dxBitis = dxOlustur(DevExpress.ui.dxDateBox, {
            displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
            placeholder: 'dd.mm.yyyy', value: initBitis || null
        }, document.getElementById('dxBitisTarihi'));

        dxDurum = dxOlustur(DevExpress.ui.dxSelectBox, {
            items: [
                { text: 'All', value: '' },
                { text: 'Active', value: 'Aktif' },
                { text: 'Cancelled', value: 'Iptal' }
            ],
            displayExpr: 'text', valueExpr: 'value',
            value: initDurum || ''
        }, document.getElementById('dxDurum'));

        dxArama = dxOlustur(DevExpress.ui.dxTextBox, {
            placeholder: 'Search...', value: initArama || ''
        }, document.getElementById('dxArama'));

        // ---- Grid ----
        dxOlustur(DevExpress.ui.dxDataGrid, {
            dataSource: gridData,
            showBorders: true,
            rowAlternationEnabled: true,
            columnAutoWidth: false,
            allowColumnResizing: true,
            allowColumnReordering: true,
            paging: { pageSize: 20 },
            pager: { showPageSizeSelector: true, allowedPageSizes: [10, 20, 50, 100], showInfo: true },
            filterRow: { visible: true },
            headerFilter: { visible: true },
            searchPanel: { visible: true, placeholder: 'Search...' },
            export: { enabled: true, fileName: 'MyReservations' },
            columns: [
                { dataField: 'BookingCode', caption: 'Booking No', width: 130, fixed: true },
                { dataField: 'SaleDate',    caption: 'Sale Date', width: 100, dataType: 'date', format: 'dd.MM.yyyy' },
                { dataField: 'Status',      caption: 'Status', width: 90,
                  cellTemplate: function (c, o) {
                      $('<span>').addClass('badge ' + (o.value === 'Iptal' ? 'badge-red' : 'badge-green'))
                          .text(o.value === 'Iptal' ? 'Cancelled' : 'Active').appendTo(c);
                  }
                },
                { dataField: 'HotelName',   caption: 'Hotel', width: 180 },
                { dataField: 'Region',      caption: 'Region', width: 140 },
                { dataField: 'CheckIn',     caption: 'Check-in', width: 100, dataType: 'date', format: 'dd.MM.yyyy' },
                { dataField: 'CheckOut',    caption: 'Check-out', width: 100, dataType: 'date', format: 'dd.MM.yyyy' },
                { dataField: 'Nights',      caption: 'Nights', width: 70, dataType: 'number' },
                { dataField: 'Adults',      caption: 'Adults', width: 70, dataType: 'number' },
                { dataField: 'Pax',         caption: 'Pax', width: 70, dataType: 'number' },
                { dataField: 'Nationality', caption: 'Nationality', width: 120 },
                { dataField: 'SellingPrice', caption: 'Selling Price', width: 120, dataType: 'number',
                  format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'Currency',   caption: 'Cur.', width: 60 }
            ],
            summary: {
                totalItems: [
                    { column: 'SellingPrice', summaryType: 'sum', valueFormat: { type: 'fixedPoint', precision: 2 }, displayFormat: 'Total: {0}' }
                ]
            }
        }, document.getElementById('gridListesi'));
    }

    function aramayiUygula() {
        if (!dxBaslangic) { window.location.href = 'Liste.aspx'; return; }
        var bas = dxBaslangic.option('value');
        var bit = dxBitis.option('value');
        var durum = dxDurum.option('value');
        var ara = dxArama.option('value');

        var params = [];
        if (bas) params.push('bas=' + formatDateISO(bas));
        if (bit) params.push('bit=' + formatDateISO(bit));
        if (durum) params.push('durum=' + encodeURIComponent(durum));
        if (ara) params.push('ara=' + encodeURIComponent(ara));
        window.location.href = 'Liste.aspx' + (params.length ? '?' + params.join('&') : '');
    }

    function filtreleriTemizle() {
        window.location.href = 'Liste.aspx';
    }

    function formatDateISO(d) {
        var dt = new Date(d);
        var y = dt.getFullYear(), m = dt.getMonth() + 1, day = dt.getDate();
        return y + '-' + (m < 10 ? '0' + m : m) + '-' + (day < 10 ? '0' + day : day);
    }
</script>
</asp:Content>
