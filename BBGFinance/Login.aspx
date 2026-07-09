<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="BBGFinance.Login"
    ContentType="text/html" ResponseEncoding="UTF-8" %>
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Giriş Yap - BBG Finance</title>

    <!-- DevExtreme CSS -->
    <link rel="stylesheet" href="https://cdn3.devexpress.com/jslib/21.1.5/css/dx.material.teal.light.css" />
    <!-- Site CSS -->
    <link rel="stylesheet" href="Content/css/site.css" />
    <link rel="stylesheet" href="Content/css/login.css" />
</head>
<body class="login-body">

<form id="frmLogin" runat="server">
<div class="login-wrapper">

    <!-- Sol panel - Marka bilgisi -->
    <div class="login-brand-panel">
        <div class="brand-content">
            <div class="brand-logo">
                <svg width="60" height="60" viewBox="0 0 60 60" fill="none">
                    <rect width="60" height="60" rx="14" fill="white" fill-opacity="0.15"/>
                    <path d="M14 40 L24 22 L30 32 L36 20 L46 40" stroke="white" stroke-width="4" fill="none" stroke-linecap="round" stroke-linejoin="round"/>
                    <circle cx="46" cy="20" r="4" fill="white" fill-opacity="0.9"/>
                </svg>
                <span class="brand-name">BBG Finance</span>
            </div>
            <h1 class="brand-title">Rezervasyon Raporlama Portalı</h1>
            <p class="brand-desc">JP_ROIBEDS rezervasyon verilerini KPI'lar ve tablolarla tek ekranda izleyin.</p>
            <ul class="brand-features">
                <li><span class="feature-icon">&#10003;</span> Rezervasyon &amp; İptal KPI'ları</li>
                <li><span class="feature-icon">&#10003;</span> Ciro, Komisyon ve Kâr Özetleri</li>
                <li><span class="feature-icon">&#10003;</span> Kanal / Pazar / Tedarikçi Dağılımları</li>
                <li><span class="feature-icon">&#10003;</span> Detaylı Rezervasyon Listesi ve Kalemleri</li>
            </ul>
        </div>
    </div>

    <!-- Sağ panel - Login formu -->
    <div class="login-form-panel">
        <div class="login-card">

            <div class="login-header">
                <h2>Hoş Geldiniz</h2>
                <p>Hesabınıza giriş yapın</p>
            </div>

            <asp:Panel ID="pnlHata" runat="server" CssClass="alert-error" Visible="false">
                <asp:Label ID="lblHata" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlBilgi" runat="server" CssClass="alert-info" Visible="false">
                <asp:Label ID="lblBilgi" runat="server" />
            </asp:Panel>

            <div class="form-group" id="txtKullaniciAdiContainer">
                <label for="txtKullaniciAdi">Kullanıcı Adı</label>
                <div class="input-wrapper">
                    <span class="input-icon">&#128100;</span>
                    <asp:TextBox ID="txtKullaniciAdi" runat="server"
                        CssClass="form-control"
                        placeholder="Kullanıcı adınızı girin"
                        MaxLength="50"
                        autocomplete="username" />
                </div>
                <asp:RequiredFieldValidator ID="rfvKullaniciAdi" runat="server"
                    ControlToValidate="txtKullaniciAdi"
                    Display="Dynamic"
                    CssClass="field-error"
                    ErrorMessage="Kullanıcı adı zorunludur." />
            </div>

            <div class="form-group">
                <label for="txtSifre">Şifre</label>
                <div class="input-wrapper">
                    <span class="input-icon">&#128274;</span>
                    <asp:TextBox ID="txtSifre" runat="server"
                        TextMode="Password"
                        CssClass="form-control"
                        placeholder="Şifrenizi girin"
                        MaxLength="100"
                        autocomplete="current-password" />
                    <button type="button" class="btn-toggle-pass" onclick="togglePassword(this)" title="Şifreyi göster">&#128065;</button>
                </div>
                <asp:RequiredFieldValidator ID="rfvSifre" runat="server"
                    ControlToValidate="txtSifre"
                    Display="Dynamic"
                    CssClass="field-error"
                    ErrorMessage="Şifre zorunludur." />
            </div>

            <div class="form-row">
                <label class="checkbox-label">
                    <asp:CheckBox ID="chkBeniHatirla" runat="server" />
                    <span>Beni hatırla</span>
                </label>
            </div>

            <asp:Button ID="btnGiris" runat="server"
                Text="Giriş Yap"
                CssClass="btn-login"
                OnClick="btnGiris_Click"
                UseSubmitBehavior="true" />

            <div class="login-footer-note">
                <small>Hesabınız yoksa sistem yöneticinizle iletişime geçin.</small>
            </div>

        </div>

        <div class="login-copyright">
            &copy; <%= DateTime.Now.Year %> BBG Finance &mdash; Tüm hakları saklıdır.
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
