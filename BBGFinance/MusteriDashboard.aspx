<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="MusteriDashboard.aspx.cs" Inherits="BBGFinance.MusteriDashboard"
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

    <div id="pnlYapilandirmaUyarisi" style="display:none;padding:14px 18px;background:#FFF3E0;border:1px solid #FFCC80;border-radius:6px;color:#E65100;font-size:13px;margin-bottom:18px;">
        Hesabınıza bir müşteri grubu atanmamış. Lütfen sistem yöneticinizle iletişime geçin.
    </div>

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
         SATIŞ ÖZETİ (para birimi bazlı)
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header"><h3>Satış / Bekleyen Tahsilat</h3></div>
            <div style="padding:16px 18px;" id="satisTablo"></div>
        </div>
    </div>

    <!-- ======================================================
         TREND + BÖLGE
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header">
                <h3>Aylık Rezervasyon Trendi</h3>
            </div>
            <div id="chartTrend" style="height:280px;"></div>
        </div>
        <div class="dashboard-panel panel-narrow">
            <div class="panel-header"><h3>Bölge Dağılımı (Satış)</h3></div>
            <div id="chartBolge" style="height:280px;"></div>
        </div>
    </div>

    <!-- ======================================================
         ODA TİPİ / YAŞ GRUBU / MİLLİYET
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Oda Tipi Dağılımı</h3></div>
            <div id="chartOdaTipi" style="height:280px;"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Yetişkin / Çocuk / Bebek</h3></div>
            <div id="chartYasGrubu" style="height:280px;"></div>
        </div>
    </div>

    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header"><h3>Milliyet Dağılımı (Top 10)</h3></div>
            <div id="chartMilliyet" style="height:280px;"></div>
        </div>
    </div>

    <!-- ======================================================
         BEKLEYEN GİRİŞLER
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header"><h3>Girişi Henüz Gelmemiş Odalar</h3></div>
            <div id="gridBekleyenGirisler"></div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var dashboardData = <%=DashboardJson%>;
    var initBaslangic  = '<%=FilterBaslangic%>';
    var initBitis      = '<%=FilterBitis%>';

    if (dashboardData.yapilandirmaEksik) {
        document.getElementById('pnlYapilandirmaUyarisi').style.display = 'block';
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

    // ---- Özet kartları: düz DOM, DevExtreme'e ihtiyaç duymaz ----
    var ozet = dashboardData.ozet || {};
    document.getElementById('valToplamRezervasyon').textContent = formatSayi(ozet.ToplamRezervasyon);
    document.getElementById('valToplamGece').textContent        = formatSayi(ozet.ToplamGece);
    document.getElementById('valToplamPax').textContent          = formatSayi(ozet.ToplamPax);

    // ---- Satış tablosu (para birimi bazlı): düz DOM ----
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

    finansalTabloOlustur('satisTablo', dashboardData.satis, [
        { field: 'ToplamSatis', label: 'Toplam Satış' },
        { field: 'BekleyenTahsilat', label: 'Bekleyen Tahsilat' }
    ]);

    // ---- DevExtreme'e bağlı bileşenler ----
    var dxBaslangic, dxBitis;

    function guvenliKur(elementId, fn) {
        if (typeof DevExpress === 'undefined' || !DevExpress.ui || !DevExpress.viz) return;
        try {
            fn();
        } catch (ex) {
            console.error(elementId + ' render edilemedi:', ex);
            var el = document.getElementById(elementId);
            if (el) el.innerHTML = '<div style="padding:16px;color:#C0392B;font-size:13px;">Bu bileşen render edilirken hata oluştu (konsola bakın).</div>';
        }
    }

    if (typeof DevExpress === 'undefined' || !DevExpress.ui || !DevExpress.viz) {
        console.error('DevExtreme yüklenemedi - cdn3.devexpress.com / ajax.googleapis.com / cdn.jsdelivr.net erişimini kontrol edin.');
        var uyariHtml = '<div style="padding:16px;color:#C0392B;font-size:13px;">Bileşen yüklenemedi (DevExtreme CDN erişimi yok).</div>';
        ['dxBaslangicTarihi', 'dxBitisTarihi', 'chartTrend', 'chartBolge', 'chartOdaTipi',
         'chartYasGrubu', 'chartMilliyet', 'gridBekleyenGirisler'].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.innerHTML = uyariHtml;
        });
    } else {
        try {
            dxBaslangic = DevExpress.ui.dxDateBox({
                displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
                placeholder: 'gg.aa.yyyy', value: initBaslangic || null
            }, document.getElementById('dxBaslangicTarihi'));

            dxBitis = DevExpress.ui.dxDateBox({
                displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
                placeholder: 'gg.aa.yyyy', value: initBitis || null
            }, document.getElementById('dxBitisTarihi'));
        } catch (ex) {
            console.error('Tarih filtresi render edilemedi:', ex);
        }
    }

    guvenliKur('chartTrend', function () {
        DevExpress.viz.dxChart({
            dataSource: dashboardData.aylikTrend,
            series: [{ valueField: 'RezervasyonSayisi', name: 'Rezervasyon', type: 'bar', color: '#00695C' }],
            argumentField: 'Ay',
            argumentAxis: { argumentType: 'string' },
            legend: { visible: false },
            tooltip: { enabled: true }
        }, document.getElementById('chartTrend'));
    });

    guvenliKur('chartBolge', function () {
        DevExpress.viz.dxPieChart({
            dataSource: dashboardData.bolgeDagilim,
            series: [{ argumentField: 'Bolge', valueField: 'ToplamSatis', label: { visible: true, connector: { visible: true } } }],
            palette: ['#00695C', '#F9A825', '#26A69A', '#8E44AD', '#546E7A'],
            legend: { visible: true },
            tooltip: { enabled: true }
        }, document.getElementById('chartBolge'));
    });

    guvenliKur('chartOdaTipi', function () {
        DevExpress.viz.dxBarChart({
            dataSource: dashboardData.odaTipiDagilim,
            series: [{ argumentField: 'OdaTipi', valueField: 'OdaSayisi', name: 'Oda Sayısı', color: '#00695C' }],
            rotated: true,
            tooltip: { enabled: true }
        }, document.getElementById('chartOdaTipi'));
    });

    guvenliKur('chartYasGrubu', function () {
        DevExpress.viz.dxPieChart({
            dataSource: dashboardData.yasGrubuDagilim,
            series: [{ argumentField: 'YasGrubu', valueField: 'Adet', label: { visible: true, connector: { visible: true } } }],
            palette: ['#00695C', '#F9A825', '#8E44AD'],
            legend: { visible: true },
            tooltip: { enabled: true }
        }, document.getElementById('chartYasGrubu'));
    });

    guvenliKur('chartMilliyet', function () {
        DevExpress.viz.dxBarChart({
            dataSource: dashboardData.milliyetDagilim,
            series: [{ argumentField: 'Milliyet', valueField: 'Adet', name: 'Yolcu Sayısı', color: '#F9A825' }],
            rotated: true,
            tooltip: { enabled: true }
        }, document.getElementById('chartMilliyet'));
    });

    guvenliKur('gridBekleyenGirisler', function () {
        DevExpress.ui.dxDataGrid({
            dataSource: dashboardData.bekleyenGirisler,
            showBorders: true, rowAlternationEnabled: true,
            paging: { pageSize: 10 },
            columns: [
                { dataField: 'BookingCode', caption: 'Rezervasyon No', width: 130 },
                { dataField: 'OtelAdi', caption: 'Otel' },
                { dataField: 'OdaTipi', caption: 'Oda Tipi', width: 150 },
                { dataField: 'MisafirAdi', caption: 'Misafir', width: 160 },
                { dataField: 'BeginTravelDate', caption: 'Giriş', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
                { dataField: 'EndTravelDate', caption: 'Çıkış', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
                { dataField: 'NightsNumber', caption: 'Gece', width: 70, dataType: 'number' },
                { dataField: 'PaxNumber', caption: 'Pax', width: 70, dataType: 'number' }
            ]
        }, document.getElementById('gridBekleyenGirisler'));
    });

    function filtreyiUygula() {
        if (!dxBaslangic || !dxBitis) { window.location.href = 'MusteriDashboard.aspx'; return; }
        var bas = dxBaslangic.option('value');
        var bit = dxBitis.option('value');
        var params = [];
        if (bas) params.push('bas=' + formatDateISO(bas));
        if (bit) params.push('bit=' + formatDateISO(bit));
        window.location.href = 'MusteriDashboard.aspx' + (params.length ? '?' + params.join('&') : '');
    }

    function filtreyiTemizle() {
        window.location.href = 'MusteriDashboard.aspx';
    }
</script>
</asp:Content>
