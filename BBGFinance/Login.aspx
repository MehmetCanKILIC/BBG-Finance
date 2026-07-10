<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="BBGFinance.Login"
    ContentType="text/html" ResponseEncoding="UTF-8" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Log In - BBG Finance</title>

    <!-- DevExtreme CSS -->
    <link rel="stylesheet" href="https://cdn3.devexpress.com/jslib/21.1.5/css/dx.material.purple.light.css" />
    <!-- Site CSS -->
    <link rel="stylesheet" href="Content/css/site.css?v=3" />
    <link rel="stylesheet" href="Content/css/login.css?v=3" />
</head>
<body class="login-body">

<form id="frmLogin" runat="server">
<div class="login-wrapper">

    <!-- Left panel - Brand info -->
    <div class="login-brand-panel">
        <div class="brand-content">
            <div class="brand-logo">
                <img src="https://static.wixstatic.com/media/3faa7d_2d926c566d544c968bdb716cbba8530d~mv2.png/v1/fill/w_330,h_59,al_c,q_85,usm_0.66_1.00_0.01,enc_avif,quality_auto/bbg%20logo.png" width="160" height="60" />
               
            </div>
            <h1 class="brand-title">BedBankGlobal<br />Reporting System</h1>
        </div>
    </div>

    <!-- Right panel - Login form -->
    <div class="login-form-panel">
        <div class="login-card">

            <div class="login-header">
                <h2>Welcome</h2>
                <p>Log in to your account</p>
            </div>

            <asp:Panel ID="pnlHata" runat="server" CssClass="alert-error" Visible="false">
                <asp:Label ID="lblHata" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlBilgi" runat="server" CssClass="alert-info" Visible="false">
                <asp:Label ID="lblBilgi" runat="server" />
            </asp:Panel>

            <div class="form-group" id="txtKullaniciAdiContainer">
                <label for="txtKullaniciAdi">Username</label>
                <div class="input-wrapper">
                    <span class="input-icon">&#128100;</span>
                    <asp:TextBox ID="txtKullaniciAdi" runat="server"
                        CssClass="form-control"
                        placeholder="Enter your username"
                        MaxLength="50"
                        autocomplete="username" />
                </div>
                <asp:RequiredFieldValidator ID="rfvKullaniciAdi" runat="server"
                    ControlToValidate="txtKullaniciAdi"
                    Display="Dynamic"
                    CssClass="field-error"
                    ErrorMessage="Username is required." />
            </div>

            <div class="form-group">
                <label for="txtSifre">Password</label>
                <div class="input-wrapper">
                    <span class="input-icon">&#128274;</span>
                    <asp:TextBox ID="txtSifre" runat="server"
                        TextMode="Password"
                        CssClass="form-control"
                        placeholder="Enter your password"
                        MaxLength="100"
                        autocomplete="current-password" />
                    <button type="button" class="btn-toggle-pass" onclick="togglePassword(this)" title="Show password">&#128065;</button>
                </div>
                <asp:RequiredFieldValidator ID="rfvSifre" runat="server"
                    ControlToValidate="txtSifre"
                    Display="Dynamic"
                    CssClass="field-error"
                    ErrorMessage="Password is required." />
            </div>

            <div class="form-row">
                <label class="checkbox-label">
                    <asp:CheckBox ID="chkBeniHatirla" runat="server" />
                    <span>Remember me</span>
                </label>
            </div>

            <asp:Button ID="btnGiris" runat="server"
                Text="Log In"
                CssClass="btn-login"
                OnClick="btnGiris_Click"
                UseSubmitBehavior="true" />

            <div class="login-footer-note">
                <small>If you don't have an account, please contact your system administrator.</small>
            </div>

        </div>

        <div class="login-copyright">
            &copy; <%= DateTime.Now.Year %> BBG Finance &mdash; All rights reserved.
        </div>
    </div>

</div>
</form>

<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
<script src="https://cdn3.devexpress.com/jslib/21.1.5/js/dx.all.js"></script>
<script>
    function togglePassword(btn) {
        var input = btn.previousElementSibling;
        if (input.type === 'password') {
            input.type = 'text';
            btn.innerHTML = '&#128064;';
        } else {
            input.type = 'password';
            btn.innerHTML = '&#128065;';
        }
    }

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            document.getElementById('<%= btnGiris.ClientID %>').click();
        }
    });
</script>
</body>
</html>
