<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Label ID="drillLabel" runat="server" Text="Label"></asp:Label><br />
        <br />
        <CR:CrystalReportViewer ID="myCrystalReportViewer" runat="server" AutoDataBind="True"
            Height="991px" ReportSourceID="CrystalReportSource1" Width="845px" />
        <CR:CrystalReportSource ID="CrystalReportSource1" runat="server">
            <Report FileName="\WorldSalesReport.rpt">
            </Report>
        </CR:CrystalReportSource>
    
    </div>
    </form>
</body>
</html>
