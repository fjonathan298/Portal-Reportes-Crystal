<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <link href="/aspnet_client/System_Web/2_0_50701/CrystalReportWebFormViewer3/css/default.css"
        rel="stylesheet" type="text/css" />
    <link href="/aspnet_client/System_Web/2_0_50701/CrystalReportWebFormViewer3/css/default.css"
        rel="stylesheet" type="text/css" />
    <link href="/aspnet_client/System_Web/2_0_50701/CrystalReportWebFormViewer3/css/default.css"
        rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:DropDownList ID="cityList" runat="server">
            <asp:ListItem>Paris</asp:ListItem>
            <asp:ListItem>Tokyo</asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="redisplay" runat="server" Text="Redisplay Report" /><br />
        <br />
        <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="True"
            Height="1039px" ReportSourceID="CrystalReportSource1" Width="901px" />
        <CR:CrystalReportSource ID="CrystalReportSource1" runat="server">
            <Report FileName="XtremeCustomers.rpt">
                <Parameters>
                    <CR:ControlParameter ControlID="cityList" ConvertEmptyStringToNull="False" DefaultValue=""
                        Name="City" PropertyName="SelectedValue" ReportName="" />
                </Parameters>

            </Report>
        </CR:CrystalReportSource>

    </div>
    </form>
</body>
</html>
