<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <link href="/aspnet_client/System_Web/2_0_50701/CrystalReportWebFormViewer3/css/default.css"
        rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ListBox ID="defaultParameterValuesList" runat="server" SelectionMode="Multiple">
        </asp:ListBox><br />
        Order State Date&nbsp;
        <asp:TextBox ID="orderStartDate" runat="server"></asp:TextBox><br />
        Order End Date &nbsp;
        <asp:TextBox ID="orderEndDate" runat="server"></asp:TextBox><br />
        <asp:Button ID="redisplay" runat="server" Text="Redisplay Report" /><br />
        <br />
        <CR:CrystalReportViewer ID="myCrystalReportViewer" runat="server" AutoDataBind="true" />
    
    </div>
    </form>
</body>
</html>
