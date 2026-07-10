<%@ Page Title="Rezervasyonlar" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Liste.aspx.cs"
         Inherits="BBGFinance.Modules.Rezervasyonlar.Liste"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">Rezervasyonlar</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="cphHead" runat="server">
<style>
    .filter-bar { background:#fff; border:1px solid #e0e0e0; border-radius:6px; padding:16px 20px; margin-bottom:18px; }
    .filter-row  { display:flex; flex-wrap:wrap; gap:12px; align-items:flex-end; }
    .filter-item { display:flex; flex-direction:column; min-width:160px; }
    .filter-item label { font-size:12px; font-weight:600; color:#555; margin-bottom:4px; }
    .filter-actions { display:flex; gap:8px; margin-top:20px; }
    .btn-search { background:#00695C; color:#fff; border:none; border-radius:4px; padding:7px 18px; cursor:pointer; font-size:13px; }
    .btn-clear  { background:#757575; color:#fff; border:none; border-radius:4px; padding:7px 14px; cursor:pointer; font-size:13px; }
    .action-btn { background:none; border:none; cursor:pointer; padding:3px 6px; border-radius:3px; font-size:13px; }
    .action-btn:hover { background:#f5f5f5; }
    .badge-green  { background:#e8f5e9; color:#2e7d32; }
    .badge-red    { background:#ffebee; color:#c62828; }
</style>
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="cphBreadcrumb" runat="server">
    <a href="<%= ResolveUrl("~/Default.aspx") %>">Dashboard</a>
    <span> / </span>
    <span>Rezervasyonlar</span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server">
    Rezervasyonlar
</asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    JP_ROIBEDS üzerindeki tüm rezervasyonları listeler ve filtreler.
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <div class="filter-bar">
        <div class="filter-row">
            <div class="filter-item">
                <label>Başlangıç Tarihi</label>
                <div id="dxBaslangicTarihi"></div>
            </div>
            <div class="filter-item">
                <label>Bitiş Tarihi</label>
                <div id="dxBitisTarihi"></div>
            </div>
            <div class="filter-item">
                <label>Durum</label>
                <div id="dxDurum"></div>
            </div>
            <div class="filter-item" style="min-width:200px;">
                <label>Kanal</label>
                <div id="dxKanal"></div>
            </div>
            <div class="filter-item" style="min-width:220px;">
                <label>Ara (Rez. No / Müşteri / Acente / E-posta)</label>
                <div id="dxArama"></div>
            </div>
            <div class="filter-actions">
                <button type="button" class="btn-search" onclick="aramayiUygula()">Ara</button>
                <button type="button" class="btn-clear"  onclick="filtreleriTemizle()">Temizle</button>
            </div>
        </div>
    </div>

    <div id="gridListesi"></div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var gridData    = <%=GridJson%>;
    var kanalListesi = <%=KanallarJson%>;

    var initBaslangic = '<%=FilterBaslangic%>';
    var initBitis     = '<%=FilterBitis%>';
    var initDurum     = '<%=FilterDurum%>';
    var initKanal     = '<%=FilterKanal%>';
    var initArama     = '<%=FilterArama%>';

    var dxBaslangic, dxBitis, dxDurum, dxKanal, dxArama;

    if (typeof DevExpress === 'undefined' || !DevExpress.ui) {
        console.error('DevExtreme yüklenemedi - cdn3.devexpress.com / ajax.googleapis.com / cdn.jsdelivr.net erişimini kontrol edin.');
        document.getElementById('gridListesi').innerHTML =
            '<div style="padding:16px;color:#C0392B;font-size:13px;">Liste bileşeni yüklenemedi (DevExtreme CDN erişimi yok).</div>';
    } else {
        dxBaslangic = DevExpress.ui.dxDateBox({
            displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
            placeholder: 'gg.aa.yyyy', value: initBaslangic || null
        }, document.getElementById('dxBaslangicTarihi'));

        dxBitis = DevExpress.ui.dxDateBox({
            displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
            placeholder: 'gg.aa.yyyy', value: initBitis || null
        }, document.getElementById('dxBitisTarihi'));

        dxDurum = DevExpress.ui.dxSelectBox({
            items: [
                { text: 'Tümü', value: '' },
                { text: 'Aktif', value: 'Aktif' },
                { text: 'İptal', value: 'Iptal' }
            ],
            displayExpr: 'text', valueExpr: 'value',
            value: initDurum || ''
        }, document.getElementById('dxDurum'));

        var kanalItems = [{ text: 'Tümü', value: '' }].concat(
            (kanalListesi || []).map(function (k) { return { text: k.Kanal, value: k.Kanal }; })
        );
        dxKanal = DevExpress.ui.dxSelectBox({
            items: kanalItems,
            displayExpr: 'text', valueExpr: 'value',
            value: initKanal || ''
        }, document.getElementById('dxKanal'));

        dxArama = DevExpress.ui.dxTextBox({
            placeholder: 'Ara...', value: initArama || ''
        }, document.getElementById('dxArama'));

        // ---- Grid ----
        DevExpress.ui.dxDataGrid({
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
            searchPanel: { visible: true, placeholder: 'Ara...' },
            export: { enabled: true, fileName: 'Rezervasyonlar' },
            columns: [
                { dataField: 'Id', visible: false, allowSearch: false, showInColumnChooser: false },
                { dataField: 'BookingCode',  caption: 'Rezervasyon No', width: 130, fixed: true },
                { dataField: 'BookingDate',  caption: 'Tarih', width: 110, dataType: 'date', format: 'dd.MM.yyyy' },
                { dataField: 'Durum',        caption: 'Durum', width: 90,
                  cellTemplate: function (c, o) {
                      $('<span>').addClass('badge ' + (o.value === 'Iptal' ? 'badge-red' : 'badge-green'))
                          .text(o.value === 'Iptal' ? 'İptal' : 'Aktif').appendTo(c);
                  }
                },
                { dataField: 'CustomerName', caption: 'Müşteri', width: 180 },
                { dataField: 'AgentName',    caption: 'Acente', width: 150 },
                { dataField: 'Channel',      caption: 'Kanal', width: 110 },
                { dataField: 'SellingPrice', caption: 'Satış Tutarı', width: 120, dataType: 'number',
                  format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'ParaBirimi',   caption: 'P.B.', width: 60 },
                { dataField: 'Commission',   caption: 'Komisyon', width: 110, dataType: 'number',
                  format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'OutStandingAmount', caption: 'Bekleyen Tahsilat', width: 130, dataType: 'number',
                  format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'ToplamGece',  caption: 'Gece', width: 70, dataType: 'number' },
                { dataField: 'ToplamPax',   caption: 'Pax', width: 70, dataType: 'number' },
                {
                    caption: 'İşlemler', width: 90, fixed: true, fixedPosition: 'right',
                    allowFiltering: false, allowSorting: false,
                    cellTemplate: function (container, options) {
                        var kod = options.data.BookingCode;
                        $('<a>').addClass('action-btn').attr({ href: 'Detay.aspx?kod=' + encodeURIComponent(kod), title: 'Detay' })
                            .html('<span style="color:#00695C;font-size:15px;">&#128065;</span>')
                            .appendTo(container);
                    }
                }
            ]
        }, document.getElementById('gridListesi'));
    }

    function aramayiUygula() {
        if (!dxBaslangic) { window.location.href = 'Liste.aspx'; return; }
        var bas = dxBaslangic.option('value');
        var bit = dxBitis.option('value');
        var durum = dxDurum.option('value');
        var kanal = dxKanal.option('value');
        var ara = dxArama.option('value');

        var params = [];
        if (bas) params.push('bas=' + formatDateISO(bas));
        if (bit) params.push('bit=' + formatDateISO(bit));
        if (durum) params.push('durum=' + encodeURIComponent(durum));
        if (kanal) params.push('kanal=' + encodeURIComponent(kanal));
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
