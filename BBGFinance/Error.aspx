<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="BBGFinance.ErrorPage"
    ContentType="text/html" ResponseEncoding="UTF-8" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Error - BBG Finance</title>
    <link rel="stylesheet" href="Content/css/site.css?v=3" />
</head>
<body style="display:flex;align-items:center;justify-content:center;min-height:100vh;background:#F7F8FA;">
    <div style="text-align:center;">
        <h1 style="font-size:64px;color:#C2185B;margin-bottom:8px;"><%= HataKodu %></h1>
        <p style="color:#7F8C8D;font-size:15px;margin-bottom:20px;"><%= HataMesaji %></p>
        <a href="<%= ResolveUrl("~/Default.aspx") %>" style="color:#C2185B;font-weight:600;">Back to Dashboard</a>
    </div>
</body>
</html>
