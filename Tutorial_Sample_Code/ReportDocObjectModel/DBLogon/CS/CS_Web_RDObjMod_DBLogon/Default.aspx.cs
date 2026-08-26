using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class _Default : System.Web.UI.Page 
{
    private ReportDocument northwindCustomersReport;

    private void ConfigureCrystalReports()
    {
        ConnectionInfo connectionInfo = new ConnectionInfo();
        connectionInfo.DatabaseName = "Northwind";
        connectionInfo.UserID = "limitedPermissionAccount";
        connectionInfo.Password = "1234";
        connectionInfo.ServerName = "localhost";

        northwindCustomersReport = new ReportDocument();
        string reportPath = Server.MapPath("NorthwindCustomers.rpt");
        northwindCustomersReport.Load(reportPath);
        crystalReportViewer.ReportSource = northwindCustomersReport;

        SetDBLogonForReport(connectionInfo, northwindCustomersReport);
    }

    private void SetDBLogonForReport(ConnectionInfo connectionInfo, ReportDocument reportDocument)
    {
        Tables tables = reportDocument.Database.Tables;
        foreach (CrystalDecisions.CrystalReports.Engine.Table table in tables)
        {
            TableLogOnInfo tableLogonInfo = table.LogOnInfo;
            tableLogonInfo.ConnectionInfo = connectionInfo;
            table.ApplyLogOnInfo(tableLogonInfo);

        }
    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }
}
