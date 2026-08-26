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
	private ReportDocument customersReport;

	private void Page_Init(object sender, EventArgs e)
	{
		ConfigureCrystalReports();
	}

	private void ConfigureCrystalReports()
	{
		customersReport = new ReportDocument();
		string reportPath = Server.MapPath("Customers.rpt");
		customersReport.Load(reportPath);
		crystalReportPartsViewer.ReportSource = customersReport;
	}

}
