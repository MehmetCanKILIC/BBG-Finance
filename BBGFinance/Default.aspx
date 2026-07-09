<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BBGFinance.Default"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">Dashboard</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="cphHead" runat="server">
<style>
    .filter-bar { background:#fff; border:1px solid #e0e0e0; border-radius:6px; padding:16px 20px; margin-bottom:18px; }
    .filter-row  { display:flex; flex-wrap:wrap; gap:12px; align-items:flex-end; }
    .filter-item { display:flex; flex-direction:column; min-width:160px; }
    .filter-item label { font-size:12px; font-weight:600; color:#555; margin-bottom:4px; }
    .filter-actions { display:flex; gap:8px; margin-top:20px; }
    .btn-search { background:#00695C; color:#fff; border:none; border-radius:4px; padding:7px 18px; cursor:pointer; font-size:13px; }
    .btn-clear  { background:#757575; color:#fff; border:none; border-radius:4px; padding:7px 14px; cursor:pointer; font-size:13px; }
    .finans-tablo { width:100%; font-size:13px; border-collapse:collapse; }
    .finans-tablo td { padding:4px 0; }
    .finans-tablo td.tutar { text-align:right; font-weight:600; }
    .finans-tablo tr + tr td { border-top:1px solid #f0f0f0; }
</style>
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="cphBreadcrumb" runat="server">
    <span>Dashboard</span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server">
    Dashboard
</asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    Hoş geldiniz, <asp:Label ID="lblAdSoyad" runat="server" /> &mdash;
    <asp:Label ID="lblTarih" runat="server" />
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <!-- ======================================================
         TARİH FİLTRESİ
    ====================================================== -->
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
            <div class="filter-actions">
                <button type="button" class="btn-search" onclick="filtreyiUygula()">Uygula</button>
                <button type="button" class="btn-clear"  onclick="filtreyiTemizle()">Son 6 Ay</button>
            </div>
        </div>
    </div>

    <!-- ======================================================
         ÖZET KARTLARI
    ====================================================== -->
    <div class="summary-cards">
        <div class="summary-card card-green">
            <div class="card-body">
                <div class="card-value" id="valToplamRezervasyon">0</div>
                <div class="card-label">Toplam Rezervasyon</div>
            </div>
            <a href="Modules/Rezervasyonlar/Liste.aspx" class="card-link">Görüntüle &rarr;</a>
        </div>
        <div class="summary-card card-red">
            <div class="card-body">
                <div class="card-value" id="valIptalOrani">%0</div>
                <div class="card-label">İptal Oranı</div>
            </div>
        </div>
        <div class="summary-card card-orange">
            <div class="card-body">
                <div class="card-value" id="valToplamGece">0</div>
                <div class="card-label">Toplam Gece</div>
            </div>
        </div>
        <div class="summary-card card-maroon">
            <div class="card-body">
                <div class="card-value" id="valToplamPax">0</div>
                <div class="card-label">Toplam Pax</div>
            </div>
        </div>
    </div>

    <!-- ======================================================
         FİNANSAL ÖZET (para birimi bazlı)
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Satış / Komisyon / Bekleyen Tahsilat</h3></div>
            <div style="padding:16px 18px;" id="finansalTablo"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Kâr</h3></div>
            <div style="padding:16px 18px;" id="karTablo"></div>
        </div>
    </div>

    <!-- ======================================================
         TREND + KANAL
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header">
                <h3>Aylık Rezervasyon Trendi</h3>
                <span class="panel-subtitle">Rezervasyon &amp; İptal</span>
            </div>
            <div id="chartTrend" style="height:280px;"></div>
        </div>
        <div class="dashboard-panel panel-narrow">
            <div class="panel-header"><h3>Kanal Dağılımı</h3></div>
            <div id="chartKanal" style="height:280px;"></div>
        </div>
    </div>

    <!-- ======================================================
         ÜRÜN GRUBU / PAZAR / TEDARİKÇİ
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Ürün Grubu Dağılımı (Satış)</h3></div>
            <div id="chartUrunGrubu" style="height:280px;"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Pazar Dağılımı (Satış)</h3></div>
            <div id="chartPazar" style="height:280px;"></div>
        </div>
    </div>

    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header"><h3>Tedarikçi Dağılımı (Top 10)</h3></div>
            <div id="gridTedarikci"></div>
        </div>
    </div>

    <!-- ======================================================
         SON REZERVASYONLAR + YAKLAŞAN KONAKLAMALAR
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header">
                <h3>Son Rezervasyonlar</h3>
                <a href="Modules/Rezervasyonlar/Liste.aspx" class="panel-link">Tümü &rarr;</a>
            </div>
            <div id="gridSonRezervasyonlar"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Yaklaşan Konaklamalar</h3></div>
            <div id="gridYaklasanKonaklamalar"></div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var dashboardData = <%=DashboardJson%>;
    var initBaslangic  = '<%=FilterBaslangic%>';
    var initBitis      = '<%=FilterBitis%>';

    // ---- Tarih filtreleri ----
    var dxBaslangic = DevExpress.ui.dxDateBox({
        displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
        placeholder: 'gg.aa.yyyy', value: initBaslangic || null
    }, document.getElementById('dxBaslangicTarihi'));

    var dxBitis = DevExpress.ui.dxDateBox({
        displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
        placeholder: 'gg.aa.yyyy', value: initBitis || null
    }, document.getElementById('dxBitisTarihi'));

    function filtreyiUygula() {
        var bas = dxBaslangic.option('value');
        var bit = dxBitis.option('value');
        var params = [];
        if (bas) params.push('bas=' + formatDateISO(bas));
        if (bit) params.push('bit=' + formatDateISO(bit));
        window.location.href = 'Default.aspx' + (params.length ? '?' + params.join('&') : '');
    }

    function filtreyiTemizle() {
        window.location.href = 'Default.aspx';
    }

    function formatDateISO(d) {
        var dt = new Date(d);
        var y = dt.getFullYear(), m = dt.getMonth() + 1, day = dt.getDate();
        return y + '-' + (m < 10 ? '0' + m : m) + '-' + (day < 10 ? '0' + day : day);
    }

    function formatSayi(n) {
        return (n || 0).toLocaleString('tr-TR');
    }

    function formatTutar(n, doviz) {
        return (n || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (doviz || '');
    }

    // ---- Özet kartları ----
    var ozet = dashboardData.ozet || {};
    document.getElementById('valToplamRezervasyon').textContent = formatSayi(ozet.ToplamRezervasyon);
    document.getElementById('valIptalOrani').textContent        = '%' + (ozet.IptalOrani || 0).toString().replace('.', ',');
    document.getElementById('valToplamGece').textContent        = formatSayi(ozet.ToplamGece);
    document.getElementById('valToplamPax').textContent          = formatSayi(ozet.ToplamPax);

    // ---- Finansal tablo (para birimi bazlı) ----
    function finansalTabloOlustur(elId, satirlar, alanlar) {
        var el = document.getElementById(elId);
        if (!satirlar || satirlar.length === 0) {
            el.innerHTML = '<span style="color:#999;font-size:13px;">Kayıt yok</span>';
            return;
        }
        var html = '<table class="finans-tablo">';
        satirlar.forEach(function (s) {
            html += '<tr><td colspan="2" style="font-weight:700;padding-top:8px;">' + s.ParaBirimi + '</td></tr>';
            alanlar.forEach(function (a) {
                html += '<tr><td>' + a.label + '</td><td class="tutar">' + formatTutar(s[a.field], s.ParaBirimi) + '</td></tr>';
            });
        });
        html += '</table>';
        el.innerHTML = html;
    }

    finansalTabloOlustur('finansalTablo', dashboardData.finansal, [
        { field: 'ToplamSatis', label: 'Toplam Satış' },
        { field: 'ToplamKomisyon', label: 'Toplam Komisyon' },
        { field: 'BekleyenTahsilat', label: 'Bekleyen Tahsilat' }
    ]);

    finansalTabloOlustur('karTablo', dashboardData.kar, [
        { field: 'ToplamKar', label: 'Toplam Kâr' }
    ]);

    // ---- Aylık trend grafiği ----
    DevExpress.viz.dxChart({
        dataSource: dashboardData.aylikTrend,
        series: [
            { valueField: 'RezervasyonSayisi', name: 'Rezervasyon', type: 'bar', color: '#00695C' },
            { valueField: 'IptalSayisi', name: 'İptal', type: 'bar', color: '#C0392B' }
        ],
        argumentField: 'Ay',
        argumentAxis: { argumentType: 'string' },
        legend: { visible: true, verticalAlignment: 'bottom', horizontalAlignment: 'center' },
        tooltip: { enabled: true }
    }, document.getElementById('chartTrend'));

    // ---- Kanal dağılımı ----
    DevExpress.viz.dxPieChart({
        dataSource: dashboardData.kanalDagilim,
        series: [{ argumentField: 'Kanal', valueField: 'Adet', label: { visible: true, connector: { visible: true } } }],
        palette: ['#00695C', '#F9A825', '#26A69A', '#8E44AD', '#546E7A'],
        legend: { visible: true },
        tooltip: { enabled: true }
    }, document.getElementById('chartKanal'));

    // ---- Ürün grubu dağılımı ----
    DevExpress.viz.dxBarChart({
        dataSource: dashboardData.urunGrubu,
        series: [{ argumentField: 'UrunGrubu', valueField: 'ToplamSatis', name: 'Satış', color: '#00695C' }],
        rotated: true,
        tooltip: { enabled: true }
    }, document.getElementById('chartUrunGrubu'));

    // ---- Pazar dağılımı ----
    DevExpress.viz.dxBarChart({
        dataSource: dashboardData.pazarDagilim,
        series: [{ argumentField: 'Pazar', valueField: 'ToplamSatis', name: 'Satış', color: '#F9A825' }],
        rotated: true,
        tooltip: { enabled: true }
    }, document.getElementById('chartPazar'));

    // ---- Tedarikçi tablosu ----
    DevExpress.ui.dxDataGrid({
        dataSource: dashboardData.tedarikci,
        showBorders: true, rowAlternationEnabled: true,
        paging: { pageSize: 10 },
        columns: [
            { dataField: 'Tedarikci', caption: 'Tedarikçi' },
            { dataField: 'KalemSayisi', caption: 'Kalem Sayısı', width: 120, dataType: 'number' },
            { dataField: 'ToplamSatis', caption: 'Toplam Satış', dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
            { dataField: 'ToplamKar', caption: 'Toplam Kâr', dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' }
        ]
    }, document.getElementById('gridTedarikci'));

    // ---- Son rezervasyonlar ----
    DevExpress.ui.dxDataGrid({
        dataSource: dashboardData.sonRezervasyonlar,
        showBorders: true, rowAlternationEnabled: true,
        paging: { pageSize: 5 },
        columns: [
            { dataField: 'BookingCode', caption: 'Rezervasyon No', width: 130 },
            { dataField: 'CustomerName', caption: 'Müşteri' },
            { dataField: 'SellingPrice', caption: 'Tutar', dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right', width: 110 },
            { dataField: 'BookingDate', caption: 'Tarih', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
            { dataField: 'IptalMi', caption: 'Durum', width: 80,
              cellTemplate: function (c, o) {
                  $('<span>').addClass('badge ' + (o.value ? 'badge-red' : 'badge-green')).text(o.value ? 'İptal' : 'Aktif').appendTo(c);
              }
            }
        ]
    }, document.getElementById('gridSonRezervasyonlar'));

    // ---- Yaklaşan konaklamalar ----
    DevExpress.ui.dxDataGrid({
        dataSource: dashboardData.yaklasanKonaklamalar,
        showBorders: true, rowAlternationEnabled: true,
        paging: { pageSize: 5 },
        columns: [
            { dataField: 'BookingCode', caption: 'Rezervasyon No', width: 130 },
            { dataField: 'CustomerName', caption: 'Müşteri' },
            { dataField: 'BeginTravelDate', caption: 'Giriş', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
            { dataField: 'NightsNumber', caption: 'Gece', width: 70, dataType: 'number' },
            { dataField: 'PaxNumber', caption: 'Pax', width: 70, dataType: 'number' }
        ]
    }, document.getElementById('gridYaklasanKonaklamalar'));
</script>
</asp:Content>
