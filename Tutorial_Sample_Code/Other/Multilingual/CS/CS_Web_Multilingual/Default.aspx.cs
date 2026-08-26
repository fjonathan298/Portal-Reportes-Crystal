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
	private ReportDocument hierarchicalGroupingReport;
	
	protected void Page_Load(object sender, EventArgs e)
    {

    }

	private void Page_Init(object sender, EventArgs e)
	{
		ConfigureCrystalReports();
	}

	private void ConfigureCrystalReports()
	{
		hierarchicalGroupingReport = new ReportDocument();
        string reportPath = Server.MapPath("Hierarchical Grouping.rpt");
        hierarchicalGroupingReport.Load(reportPath);
		crystalReportViewer.ReportSource = hierarchicalGroupingReport;
	}

}
