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
    .btn-search { background:#C2185B; color:#fff; border:none; border-radius:4px; padding:7px 18px; cursor:pointer; font-size:13px; }
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

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server"></asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    <asp:Label ID="lblTarih" runat="server" />
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <!-- ======================================================
         DATE FILTER
    ====================================================== -->
    <div class="filter-bar">
        <div class="filter-row">
            <div class="filter-item">
                <label>Start Date</label>
                <div id="dxBaslangicTarihi"></div>
            </div>
            <div class="filter-item">
                <label>End Date</label>
                <div id="dxBitisTarihi"></div>
            </div>
            <div class="filter-actions">
                <button type="button" class="btn-search" onclick="filtreyiUygula()">Apply</button>
                <button type="button" class="btn-clear"  onclick="filtreyiTemizle()">Last 6 Months</button>
            </div>
        </div>
    </div>

    <!-- ======================================================
         SUMMARY CARDS
    ====================================================== -->
    <div class="summary-cards">
        <div class="summary-card card-green">
            <div class="card-icon"><i data-feather="calendar"></i></div>
            <div class="card-body">
                <div class="card-value" id="valToplamRezervasyon">0</div>
                <div class="card-label">Total Reservations</div>
            </div>
            <a href="Modules/Rezervasyonlar/Liste.aspx" class="card-link">View &rarr;</a>
        </div>
        <div class="summary-card card-red">
            <div class="card-icon"><i data-feather="x-circle"></i></div>
            <div class="card-body">
                <div class="card-value" id="valIptalOrani">%0</div>
                <div class="card-label">Cancellation Rate</div>
            </div>
        </div>
        <div class="summary-card card-orange">
            <div class="card-icon"><i data-feather="moon"></i></div>
            <div class="card-body">
                <div class="card-value" id="valToplamGece">0</div>
                <div class="card-label">Total Nights</div>
            </div>
        </div>
        <div class="summary-card card-maroon">
            <div class="card-icon"><i data-feather="users"></i></div>
            <div class="card-body">
                <div class="card-value" id="valToplamPax">0</div>
                <div class="card-label">Total Pax</div>
            </div>
        </div>
    </div>

    <!-- ======================================================
         FINANCIAL SUMMARY (by currency)
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Sales / Commission / Outstanding Amount</h3></div>
            <div style="padding:16px 18px;" id="finansalTablo"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Profit</h3></div>
            <div style="padding:16px 18px;" id="karTablo"></div>
        </div>
    </div>

    <!-- ======================================================
         TREND + CHANNEL
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header">
                <h3>Monthly Reservation Trend</h3>
                <span class="panel-subtitle">Reservations &amp; Cancellations</span>
            </div>
            <div id="chartTrend" style="height:280px;"></div>
        </div>
        <div class="dashboard-panel panel-narrow">
            <div class="panel-header"><h3>Channel Breakdown</h3></div>
            <div id="chartKanal" style="height:280px;"></div>
        </div>
    </div>

    <!-- ======================================================
         PRODUCT GROUP / MARKET / SUPPLIER
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Product Group Breakdown (Sales)</h3></div>
            <div id="chartUrunGrubu" style="height:280px;"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Market Breakdown (Sales)</h3></div>
            <div id="chartPazar" style="height:280px;"></div>
        </div>
    </div>

    <div class="dashboard-row">
        <div class="dashboard-panel panel-wide">
            <div class="panel-header"><h3>Supplier Breakdown (Top 10)</h3></div>
            <div id="gridTedarikci"></div>
        </div>
    </div>

    <!-- ======================================================
         RECENT RESERVATIONS + UPCOMING STAYS
    ====================================================== -->
    <div class="dashboard-row">
        <div class="dashboard-panel panel-half">
            <div class="panel-header">
                <h3>Recent Reservations</h3>
                <a href="Modules/Rezervasyonlar/Liste.aspx" class="panel-link">All &rarr;</a>
            </div>
            <div id="gridSonRezervasyonlar"></div>
        </div>
        <div class="dashboard-panel panel-half">
            <div class="panel-header"><h3>Upcoming Stays</h3></div>
            <div id="gridYaklasanKonaklamalar"></div>
        </div>
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var dashboardData = <%=DashboardJson%>;
    var initBaslangic  = '<%=FilterBaslangic%>';
    var initBitis      = '<%=FilterBitis%>';

    function formatDateISO(d) {
        var dt = new Date(d);
        var y = dt.getFullYear(), m = dt.getMonth() + 1, day = dt.getDate();
        return y + '-' + (m < 10 ? '0' + m : m) + '-' + (day < 10 ? '0' + day : day);
    }

    function formatSayi(n) {
        return (n || 0).toLocaleString('en-US');
    }

    function formatTutar(n, doviz) {
        return (n || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (doviz || '');
    }

    // ---- Summary cards: plain DOM, no DevExtreme needed ----
    // (These cards keep working even if the DevExtreme CDN fails to load.)
    var ozet = dashboardData.ozet || {};
    document.getElementById('valToplamRezervasyon').textContent = formatSayi(ozet.ToplamRezervasyon);
    document.getElementById('valIptalOrani').textContent        = '%' + (ozet.IptalOrani || 0).toString();
    document.getElementById('valToplamGece').textContent        = formatSayi(ozet.ToplamGece);
    document.getElementById('valToplamPax').textContent          = formatSayi(ozet.ToplamPax);

    // ---- Financial table (by currency): plain DOM ----
    function finansalTabloOlustur(elId, satirlar, alanlar) {
        var el = document.getElementById(elId);
        if (!satirlar || satirlar.length === 0) {
            el.innerHTML = '<span style="color:#999;font-size:13px;">No records</span>';
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
        { field: 'ToplamSatis', label: 'Total Sales' },
        { field: 'ToplamKomisyon', label: 'Total Commission' },
        { field: 'BekleyenTahsilat', label: 'Outstanding Amount' }
    ]);

    finansalTabloOlustur('karTablo', dashboardData.kar, [
        { field: 'ToplamKar', label: 'Total Profit' }
    ]);

    // ---- DevExtreme-dependent widgets (date filter, charts, grids) ----
    // If DevExtreme fails to load from the CDN (cdn3.devexpress.com / ajax.googleapis.com /
    // cdn.jsdelivr.net), rather than blowing up here and halting the whole script, the KPI
    // cards and financial tables above stay populated; this section only shows a warning
    // in its own areas.
    var dxBaslangic, dxBitis;

    if (typeof DevExpress === 'undefined' || !DevExpress.ui || !DevExpress.viz) {
        console.error('DevExtreme failed to load - check access to cdn3.devexpress.com / ajax.googleapis.com / cdn.jsdelivr.net.');
        var uyariHtml = '<div style="padding:16px;color:#C0392B;font-size:13px;">Widget could not load (no DevExtreme CDN access).</div>';
        ['dxBaslangicTarihi', 'dxBitisTarihi', 'chartTrend', 'chartKanal', 'chartUrunGrubu', 'chartPazar',
         'gridTedarikci', 'gridSonRezervasyonlar', 'gridYaklasanKonaklamalar'].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.innerHTML = uyariHtml;
        });
    } else {
        // ---- Date filters ----
        try {
            dxBaslangic = dxOlustur(DevExpress.ui.dxDateBox, {
                displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
                placeholder: 'dd.mm.yyyy', value: initBaslangic || null
            }, document.getElementById('dxBaslangicTarihi'));

            dxBitis = dxOlustur(DevExpress.ui.dxDateBox, {
                displayFormat: 'dd.MM.yyyy', type: 'date', showClearButton: true,
                placeholder: 'dd.mm.yyyy', value: initBitis || null
            }, document.getElementById('dxBitisTarihi'));
        } catch (ex) {
            console.error('Date filter could not be rendered:', ex);
        }
    }

    // Each widget is set up inside its own try/catch: if one fails due to a data or
    // configuration error, the others (and the KPI cards) still render.
    function guvenliKur(elementId, fn) {
        if (typeof DevExpress === 'undefined' || !DevExpress.ui || !DevExpress.viz) return;
        try {
            fn();
        } catch (ex) {
            console.error(elementId + ' could not be rendered:', ex);
            var el = document.getElementById(elementId);
            if (el) el.innerHTML = '<div style="padding:16px;color:#C0392B;font-size:13px;">This widget could not be rendered (see console).</div>';
        }
    }

    guvenliKur('chartTrend', function () {
        dxOlustur(DevExpress.viz.dxChart, {
            dataSource: dashboardData.aylikTrend,
            series: [
                { argumentField: 'Ay', valueField: 'RezervasyonSayisi', name: 'Reservations', type: 'bar', color: '#C2185B' },
                { argumentField: 'Ay', valueField: 'IptalSayisi', name: 'Cancellations', type: 'bar', color: '#C0392B' }
            ],
            argumentAxis: { argumentType: 'string' },
            legend: { visible: true, verticalAlignment: 'bottom', horizontalAlignment: 'center' },
            tooltip: { enabled: true }
        }, document.getElementById('chartTrend'));
    });

    guvenliKur('chartKanal', function () {
        // With many small slices the connector-line labels used to overlap; labels are
        // disabled here and the info moved to the legend + a custom tooltip on hover.
        dxOlustur(DevExpress.viz.dxPieChart, {
            dataSource: dashboardData.kanalDagilim,
            series: [{ argumentField: 'Kanal', valueField: 'Adet', label: { visible: false } }],
            palette: ['#C2185B', '#F9A825', '#F06292', '#8E44AD', '#546E7A', '#1976D2', '#D35400', '#7F8C8D', '#AD1457', '#16A085'],
            legend: { visible: true, horizontalAlignment: 'center', verticalAlignment: 'bottom', margin: { top: 10 } },
            tooltip: {
                enabled: true,
                customizeTooltip: function (arg) {
                    return { text: arg.argumentText + ': ' + arg.valueText + ' (' + arg.percentText + ')' };
                }
            }
        }, document.getElementById('chartKanal'));
    });

    guvenliKur('chartUrunGrubu', function () {
        dxOlustur(DevExpress.viz.dxChart, {
            dataSource: dashboardData.urunGrubu,
            series: [{ type: 'bar', argumentField: 'UrunGrubu', valueField: 'ToplamSatis', name: 'Sales', color: '#C2185B' }],
            rotated: true,
            legend: { visible: false },
            tooltip: { enabled: true }
        }, document.getElementById('chartUrunGrubu'));
    });

    guvenliKur('chartPazar', function () {
        dxOlustur(DevExpress.viz.dxChart, {
            dataSource: dashboardData.pazarDagilim,
            series: [{ type: 'bar', argumentField: 'Pazar', valueField: 'ToplamSatis', name: 'Sales', color: '#F9A825' }],
            rotated: true,
            legend: { visible: false },
            tooltip: { enabled: true }
        }, document.getElementById('chartPazar'));
    });

    guvenliKur('gridTedarikci', function () {
        dxOlustur(DevExpress.ui.dxDataGrid, {
            dataSource: dashboardData.tedarikci,
            showBorders: true, rowAlternationEnabled: true,
            paging: { pageSize: 10 },
            columns: [
                { dataField: 'Tedarikci', caption: 'Supplier' },
                { dataField: 'KalemSayisi', caption: 'Line Count', width: 120, dataType: 'number' },
                { dataField: 'ToplamSatis', caption: 'Total Sales', dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' },
                { dataField: 'ToplamKar', caption: 'Total Profit', dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right' }
            ]
        }, document.getElementById('gridTedarikci'));
    });

    guvenliKur('gridSonRezervasyonlar', function () {
        dxOlustur(DevExpress.ui.dxDataGrid, {
            dataSource: dashboardData.sonRezervasyonlar,
            showBorders: true, rowAlternationEnabled: true,
            paging: { pageSize: 5 },
            columns: [
                { dataField: 'BookingCode', caption: 'Booking No', width: 130 },
                { dataField: 'CustomerName', caption: 'Customer' },
                { dataField: 'SellingPrice', caption: 'Amount', dataType: 'number', format: { type: 'fixedPoint', precision: 2 }, alignment: 'right', width: 110 },
                { dataField: 'BookingDate', caption: 'Date', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
                { dataField: 'IptalMi', caption: 'Status', width: 80,
                  cellTemplate: function (c, o) {
                      $('<span>').addClass('badge ' + (o.value ? 'badge-red' : 'badge-green')).text(o.value ? 'Cancelled' : 'Active').appendTo(c);
                  }
                }
            ]
        }, document.getElementById('gridSonRezervasyonlar'));
    });

    guvenliKur('gridYaklasanKonaklamalar', function () {
        dxOlustur(DevExpress.ui.dxDataGrid, {
            dataSource: dashboardData.yaklasanKonaklamalar,
            showBorders: true, rowAlternationEnabled: true,
            paging: { pageSize: 5 },
            columns: [
                { dataField: 'BookingCode', caption: 'Booking No', width: 130 },
                { dataField: 'CustomerName', caption: 'Customer' },
                { dataField: 'BeginTravelDate', caption: 'Check-in', dataType: 'date', format: 'dd.MM.yyyy', width: 100 },
                { dataField: 'NightsNumber', caption: 'Nights', width: 70, dataType: 'number' },
                { dataField: 'PaxNumber', caption: 'Pax', width: 70, dataType: 'number' }
            ]
        }, document.getElementById('gridYaklasanKonaklamalar'));
    });

    function filtreyiUygula() {
        if (!dxBaslangic || !dxBitis) { window.location.href = 'Default.aspx'; return; }
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
</script>
</asp:Content>
