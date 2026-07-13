<%@ Page Title="Add User" Language="C#" MasterPageFile="~/Site.Master"
         AutoEventWireup="true" CodeBehind="Ekle.aspx.cs"
         Inherits="BBGFinance.Modules.Kullanicilar.Ekle"
         ContentType="text/html" ResponseEncoding="UTF-8" %>

<asp:Content ID="cTitle" ContentPlaceHolderID="cphTitle" runat="server">Add User</asp:Content>

<asp:Content ID="cHead" ContentPlaceHolderID="cphHead" runat="server">
<style>
    .form-card { background:#fff; border:1px solid #e0e0e0; border-radius:6px; padding:24px; max-width:640px; }
    .form-row { margin-bottom:16px; }
    .form-row label { display:block; font-size:12px; font-weight:600; color:#555; margin-bottom:4px; }
    .form-row .form-control { width:100%; max-width:360px; padding:8px 10px; border:1px solid #ccc; border-radius:4px; font-size:14px; box-sizing:border-box; }
    .form-row select.form-control { background:#fff; }
    .password-row { display:flex; gap:8px; align-items:center; }
    .btn-suggest { background:#757575; color:#fff; border:none; border-radius:4px; padding:8px 14px; cursor:pointer; font-size:13px; white-space:nowrap; }
    .btn-save { background:#C2185B; color:#fff; border:none; border-radius:4px; padding:9px 22px; cursor:pointer; font-size:14px; margin-top:8px; }
    .field-error { color:#c62828; font-size:12px; display:block; margin-top:4px; }
    .alert-error { background:#ffebee; color:#c62828; border:1px solid #ffcdd2; border-radius:4px; padding:10px 14px; margin-bottom:16px; font-size:13px; }
    .alert-success { background:#e8f5e9; color:#2e7d32; border:1px solid #c8e6c9; border-radius:4px; padding:10px 14px; margin-bottom:16px; font-size:13px; }
    .checkbox-row label { display:flex; align-items:center; gap:6px; font-size:13px; color:#333; font-weight:normal; }
</style>
</asp:Content>

<asp:Content ID="cBreadcrumb" ContentPlaceHolderID="cphBreadcrumb" runat="server">
    <a href="<%= ResolveUrl("~/Default.aspx") %>">Dashboard</a>
    <span> / </span>
    <span>Add User</span>
</asp:Content>

<asp:Content ID="cPageTitle" ContentPlaceHolderID="cphPageTitle" runat="server">
    Add User
</asp:Content>

<asp:Content ID="cPageSubtitle" ContentPlaceHolderID="cphPageSubtitle" runat="server">
    Create a new login. Customers must be linked to a customer group.
</asp:Content>

<asp:Content ID="cContent" ContentPlaceHolderID="cphContent" runat="server">

    <div class="form-card">

        <asp:Panel ID="pnlHata" runat="server" CssClass="alert-error" Visible="false">
            <asp:Label ID="lblHata" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlBasari" runat="server" CssClass="alert-success" Visible="false">
            <asp:Label ID="lblBasari" runat="server" />
        </asp:Panel>

        <div class="form-row">
            <label for="<%= txtKullaniciAdi.ClientID %>">Username</label>
            <asp:TextBox ID="txtKullaniciAdi" runat="server" CssClass="form-control" MaxLength="50" autocomplete="off" />
            <asp:RequiredFieldValidator ID="rfvKullaniciAdi" runat="server"
                ControlToValidate="txtKullaniciAdi" Display="Dynamic" CssClass="field-error"
                ErrorMessage="Username is required." ValidationGroup="Kaydet" />
        </div>

        <div class="form-row">
            <label for="<%= txtAdSoyad.ClientID %>">Full Name</label>
            <asp:TextBox ID="txtAdSoyad" runat="server" CssClass="form-control" MaxLength="100" />
            <asp:RequiredFieldValidator ID="rfvAdSoyad" runat="server"
                ControlToValidate="txtAdSoyad" Display="Dynamic" CssClass="field-error"
                ErrorMessage="Full name is required." ValidationGroup="Kaydet" />
        </div>

        <div class="form-row">
            <label for="<%= txtEmail.ClientID %>">Email (optional)</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="100" TextMode="Email" />
        </div>

        <div class="form-row">
            <label for="<%= ddlRol.ClientID %>">Role</label>
            <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-control" onchange="rolDegisti()">
                <asp:ListItem Text="Customer" Value="Musteri" />
                <asp:ListItem Text="Admin" Value="Admin" />
            </asp:DropDownList>
        </div>

        <div class="form-row" id="rowCustomerGroup" runat="server">
            <label for="<%= ddlCustomerGroup.ClientID %>">Customer Group</label>
            <asp:DropDownList ID="ddlCustomerGroup" runat="server" CssClass="form-control" />
        </div>

        <div class="form-row">
            <label for="<%= txtSifre.ClientID %>">Password</label>
            <div class="password-row">
                <asp:TextBox ID="txtSifre" runat="server" CssClass="form-control" MaxLength="100" autocomplete="new-password" />
                <button type="button" class="btn-suggest" onclick="sifreOner()">Suggest Password</button>
            </div>
            <asp:RequiredFieldValidator ID="rfvSifre" runat="server"
                ControlToValidate="txtSifre" Display="Dynamic" CssClass="field-error"
                ErrorMessage="A password is required (type one or click Suggest Password)." ValidationGroup="Kaydet" />
            <small style="color:#777;">At least 8 characters. The admin can type a password or generate a suggested one.</small>
        </div>

        <div class="form-row checkbox-row">
            <label>
                <asp:CheckBox ID="chkAktif" runat="server" Checked="true" />
                <span>Account active</span>
            </label>
        </div>

        <asp:Button ID="btnKaydet" runat="server" Text="Create User" CssClass="btn-save"
            OnClick="btnKaydet_Click" ValidationGroup="Kaydet" />
    </div>

</asp:Content>

<asp:Content ID="cScripts" ContentPlaceHolderID="cphScripts" runat="server">
<script>
    function rolDegisti() {
        var rol = document.getElementById('<%= ddlRol.ClientID %>').value;
        document.getElementById('<%= rowCustomerGroup.ClientID %>').style.display = (rol === 'Musteri') ? '' : 'none';
    }

    function sifreOner() {
        var alfabe = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%';
        var sifre = '';
        for (var i = 0; i < 12; i++) {
            sifre += alfabe.charAt(Math.floor(Math.random() * alfabe.length));
        }
        document.getElementById('<%= txtSifre.ClientID %>').value = sifre;
    }

    document.addEventListener('DOMContentLoaded', rolDegisti);
</script>
</asp:Content>
