using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class _Default : System.Web.UI.Page 
{
    private ReportDocument hierarchicalGroupingReport;

    private void ConfigureCrystalReports()
    {
        if(Session["hierarchicalGroupingReport"] == null)
        {
           hierarchicalGroupingReport = new ReportDocument();
           hierarchicalGroupingReport.Load(Server.MapPath("Hierarchical Grouping.rpt"));
           Session["hierarchicalGroupingReport"] = hierarchicalGroupingReport;
        }
        else
        {
            hierarchicalGroupingReport = (ReportDocument)Session["hierarchicalGroupingReport"];
        }
        crystalReportViewer.ReportSource = hierarchicalGroupingReport;

    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }

    protected void sortOrderAscending_Click(object sender, EventArgs e)
    {
        SortFields sortFields = hierarchicalGroupingReport.DataDefinition.SortFields;
        SortField firstSortField = sortFields[0];
        firstSortField.SortDirection = SortDirection.AscendingOrder;
        crystalReportViewer.ReportSource = hierarchicalGroupingReport;
        Session["hierarchicalGroupingReport"] = hierarchicalGroupingReport;
        ConfigureCrystalReports();
    }

    protected void sortOrderDescending_Click(object sender, EventArgs e)
    {
        SortFields sortFields = hierarchicalGroupingReport.DataDefinition.SortFields;
        SortField firstSortField = sortFields[0];
        firstSortField.SortDirection = SortDirection.DescendingOrder;
        crystalReportViewer.ReportSource = hierarchicalGroupingReport;
        Session["hierarchicalGroupingReport"] = hierarchicalGroupingReport;
        ConfigureCrystalReports();
    }
}
