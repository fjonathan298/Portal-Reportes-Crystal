<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:TextBox ID="symbol" runat="server">Enter Stock Symbol</asp:TextBox>
        <asp:TextBox ID="name" runat="server">Enter Company Name</asp:TextBox>
        <asp:TextBox ID="price" runat="server">Enter Stock Price</asp:TextBox>
        <asp:TextBox ID="volume" runat="server">Enter Volume</asp:TextBox>&nbsp;
        <br />
        <asp:Button ID="addStockInformation" runat="server" Text="Add Stock Information" />
        <br />
        <CR:CrystalReportViewer ID="myCrystalReportViewer" runat="server" AutoDataBind="true" />
    
    </div>
    </form>
</body>
</html>
