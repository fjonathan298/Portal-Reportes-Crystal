<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

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
        <br />
        <asp:TextBox ID="symbol" runat="server">Symbol</asp:TextBox>
        <asp:TextBox ID="name" runat="server">Company Name</asp:TextBox>
        <asp:TextBox ID="price" runat="server">Price</asp:TextBox>
        <asp:TextBox ID="volume" runat="server">Volume</asp:TextBox><br />
        <br />
        &nbsp;<asp:Button ID="addStockInformation" runat="server" OnClick="addStockInformation_Click"
            Text="Add Stock Information" /><br />
        <br />
        <CR:CrystalReportViewer id="crystalReportViewer" runat="server" AutoDataBind="true" />
    
    </div>
    </form>
</body>
</html>
