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

        if (!IsPostBack)
        {
            selectOperatorList.DataSource = System.Enum.GetValues(typeof(CeComparisonOperator));

            string selectFormula = "{Customer.Last Year's Sales} > 11000.00 " + "AND Mid({Customer.Customer Name}, 1, 1) = \"A\"";
            crystalReportViewer.SelectionFormula = selectFormula;
            selectOperatorList.DataBind();
        }

        string reportPath = Server.MapPath("CustomersBySalesName.rpt");
        crystalReportViewer.ReportSource = reportPath;
    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }
    protected void redisplay_Click(object sender, EventArgs e)
    {
        string selectedOperator = GetSelectedCompareOperator();

        string selectFormula = "{Customer.Last Year's Sales} > " + lastYearsSales.Text
        + " AND Mid({Customer.Customer Name}, 1, 1) " + selectedOperator + " \"" + customerName.Text + "\""; 
        crystalReportViewer.ReportSource = Server.MapPath("CustomersBySalesName.rpt");
        crystalReportViewer.SelectionFormula = selectFormula;
    }

    private string GetSelectedCompareOperator()
    {
        switch ((CeComparisonOperator)selectOperatorList.SelectedIndex)
        {
            case CeComparisonOperator.EqualTo:
                return "=";
            case CeComparisonOperator.LessThan:
                return "<";
            case CeComparisonOperator.GreaterThan:
                return ">";
            case CeComparisonOperator.LessThan_or_EqualTo:
                return "<=";
            case CeComparisonOperator.GreaterThan_or_EqualTo:
                return ">=";
            case CeComparisonOperator.Not_EqualTo:
                return "<>";
            default:
                return "=";
        }
    }
}