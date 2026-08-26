<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=14.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304"
    Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <link href="/aspnet_client/System_Web/2_0_50701/CrystalReportWebFormViewer3/css/default.css"
        rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="True"
            Height="1039px" ReportSourceID="CrystalReportSource1" Width="901px" />
        <CR:CrystalReportSource ID="CrystalReportSource1" runat="server">
            <Report FileName="NorthwindCustomers.rpt">
                <DataSources>
                    <CR:DataSourceRef DataSourceID="SqlDataSource1" TableName="Customers" />
                </DataSources>
            </Report>
        </CR:CrystalReportSource>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:NorthwindConnectionString %>"
            ProviderName="<%$ ConnectionStrings:NorthwindConnectionString.ProviderName %>"
            SelectCommand="SELECT [CompanyName], [ContactName], [City] FROM [Customers]"></asp:SqlDataSource>
    
    </div>
    </form>
</body>
</html>
