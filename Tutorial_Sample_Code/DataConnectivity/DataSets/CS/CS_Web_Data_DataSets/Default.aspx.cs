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
    private ReportDocument customerReport;


    private void ConfigureCrystalReports()
    {
        customerReport = new ReportDocument();
        string reportPath = Server.MapPath("Customer.rpt");
        customerReport.Load(reportPath);
        DataSet dataSet = DataSetConfiguration.CustomerDataSet;
        customerReport.SetDataSource(dataSet);
        crystalReportViewer.ReportSource = customerReport;
    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }
}
