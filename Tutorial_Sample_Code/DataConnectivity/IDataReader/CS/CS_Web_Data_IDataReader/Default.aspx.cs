using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;

public partial class _Default : System.Web.UI.Page 
{
    private ReportDocument customersViaIdrReport;

    public void configureCrystalReports() {
        customersViaIdrReport = new ReportDocument();
        string reportPath = Server.MapPath("CustomersViaIDR.rpt");
        customersViaIdrReport.Load(reportPath);
        crystalReportViewer.ReportSource = customersViaIdrReport;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        configureCrystalReports();
    }

}
