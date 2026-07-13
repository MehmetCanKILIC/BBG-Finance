<%@ Page Title="Users" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Liste.aspx.cs"
         Inherits="BBGFinance.Modules.Kullanicilar.Liste"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">Users</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="cphHead" runat="server">
<style>
    .action-btn { background:none; border:none; cursor:pointer; padding:3px 6px; border-radius:3px; font-size:13px; }
    .action-btn:hover { background:#f5f5f5; }
    .badge-green { background:#e8f5e9; color:#2e7d32; }
    .badge-red   { background:#ffebee; color:#c62828; }
    .badge-grey  { background:#f5f5f5; color:#616161; }
    .btn-add-user { background:#C2185B; color:#fff; border:none; border-radius:4px; padding:8px 16px; cursor:pointer; font-size:13px; text-decoration:none; display:inline-block; }
</style>
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="cphBreadcrumb" runat="server">
    <a href="<%= ResolveUrl("~/Default.aspx") %>">Dashboard</a>
    <span> / </span>
    <span>Users</span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server">
    Users
</asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    All logins (Admin and Customer). Edit a row to update its details, role, customer group or password.
</asp:Content>

<asp:Content ID="cPageActions" ContentPlaceHolderID="cphPageActions" runat="server">
    <a class="btn-add-user" href="<%= ResolveUrl("~/Modules/Kullanicilar/Ekle.aspx") %>">+ Add User</a>
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">
    <div id="gridListesi"></div>
</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    var gridData = <%=GridJson%>;

    if (typeof DevExpress === 'undefined' || !DevExpress.ui) {
        console.error('DevExtreme failed to load - check access to cdn3.devexpress.com / ajax.googleapis.com / cdn.jsdelivr.net.');
        document.getElementById('gridListesi').innerHTML =
            '<div style="padding:16px;color:#C0392B;font-size:13px;">The list widget could not load (no DevExtreme CDN access).</div>';
    } else {
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
            columns: [
                { dataField: 'KullaniciAdi', caption: 'Username', width: 150, fixed: true },
                { dataField: 'AdSoyad',      caption: 'Full Name', width: 180 },
                { dataField: 'Email',        caption: 'Email', width: 180 },
                { dataField: 'Rol',          caption: 'Role', width: 100,
                  cellTemplate: function (c, o) {
                      $('<span>').text(o.value === 'Admin' ? 'Admin' : 'Customer').appendTo(c);
                  }
                },
                { dataField: 'CustomerGroupAdi', caption: 'Customer Group', width: 160 },
                { dataField: 'Durum', caption: 'Status', width: 100,
                  cellTemplate: function (c, o) {
                      var cls = o.value === 'Locked' ? 'badge-red' : (o.value === 'Inactive' ? 'badge-grey' : 'badge-green');
                      $('<span>').addClass('badge ' + cls).text(o.value).appendTo(c);
                  }
                },
                { dataField: 'SonGirisTarihi', caption: 'Last Login', width: 120, dataType: 'date', format: 'dd.MM.yyyy' },
                {
                    caption: 'Actions', width: 90, fixed: true, fixedPosition: 'right',
                    allowFiltering: false, allowSorting: false,
                    cellTemplate: function (container, options) {
                        var id = options.data.KullaniciID;
                        $('<a>').addClass('action-btn').attr({ href: 'Duzenle.aspx?id=' + encodeURIComponent(id), title: 'Edit' })
                            .html('<span style="color:#C2185B;font-size:15px;">&#9998;</span>')
                            .appendTo(container);
                    }
                }
            ]
        }, document.getElementById('gridListesi'));
    }
</script>
</asp:Content>
