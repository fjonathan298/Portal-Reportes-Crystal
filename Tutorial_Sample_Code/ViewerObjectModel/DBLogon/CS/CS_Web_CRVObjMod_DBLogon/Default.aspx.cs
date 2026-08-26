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

    private void ConfigureCrystalReports()
    {
        string reportPath = Server.MapPath("NorthwindCustomers.rpt");

        ConnectionInfo connectionInfo = new ConnectionInfo();
        
        connectionInfo.ServerName = "localhost";
        connectionInfo.DatabaseName = "Northwind";
        connectionInfo.UserID = "i822460";
        connectionInfo.Password = "5";
        crystalReportViewer.ReportSource = reportPath;
        SetDBLogonForReport(connectionInfo);
    }

    public void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }
    private void SetDBLogonForReport(ConnectionInfo connectionInfo)
    {
        TableLogOnInfos tableLogOnInfos = crystalReportViewer.LogOnInfo;
        foreach (TableLogOnInfo tableLogOnInfo in tableLogOnInfos)
        {
            tableLogOnInfo.ConnectionInfo = connectionInfo;
        }
    }
    

}
