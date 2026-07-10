/* BBG Finance - Site Global Scripts */

(function () {
    'use strict';

    // DevExtreme Türkçe locale
    if (typeof DevExpress !== 'undefined') {
        DevExpress.localization.locale('tr');
    }

    // DevExtreme ortak ayarlar
    if (typeof DevExpress !== 'undefined' && DevExpress.ui) {
        DevExpress.ui.dxDataGrid.defaultOptions({
            options: {
                showBorders: true,   
                rowAlternationEnabled: true,
                hoverStateEnabled: true,
                filterRow: { visible: true },
                headerFilter: { visible: false },
                searchPanel: { visible: true, width: 200 },
                paging: { pageSize: 20 },
                pager: { showPageSizeSelector: true, allowedPageSizes: [10, 20, 50], showInfo: true },
                noDataText: 'Gösterilecek kayıt yok',
                loadingIndicator: { enabled: true }
            }
        });
    }

    // DevExtreme'in çerçevesiz (framework'süz) API'sinde widget constructor imzası
    // (element, options)'tır - (options, element) DEĞİL. Bu yardımcı, çağıran kodda
    // options bloğunu ve element'i yazma sırasını değiştirmeden doğru sırayla
    // "new Ctor(element, options)" çağrısı yapar.
    window.dxOlustur = function (Ctor, options, element) {
        return new Ctor(element, options);
    };

    // Para formatı
    window.formatTutar = function (tutar, doviz) {
        doviz = doviz || '';
        return new Intl.NumberFormat('tr-TR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(tutar || 0) + (doviz ? ' ' + doviz : '');
    };

    // Tarih formatı
    window.formatTarih = function (tarih) {
        if (!tarih) return '';
        var d = new Date(tarih);
        return d.toLocaleDateString('tr-TR');
    };

    // Bildirim (DevExtreme notify)
    window.notify = function (mesaj, tip, sure) {
        if (typeof DevExpress !== 'undefined') {
            DevExpress.ui.notify(mesaj, tip || 'info', sure || 3000);
        }
    };

    window.notifyBasari = function (m) { window.notify(m, 'success', 3000); };
    window.notifyHata   = function (m) { window.notify(m, 'error',   5000); };
    window.notifyUyari  = function (m) { window.notify(m, 'warning', 4000); };

})();
