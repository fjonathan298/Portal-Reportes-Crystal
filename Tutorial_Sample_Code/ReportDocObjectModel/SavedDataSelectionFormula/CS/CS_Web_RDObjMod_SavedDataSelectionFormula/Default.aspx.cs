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
    private string salesAmount;
    private string operatorValue;
    private string customerName;
    private ReportDocument customerBySalesNameReport;

    protected void Page_Unload(object sender, EventArgs e)
    {
        customerBySalesNameReport.Close();
        crystalReportViewer.Dispose();
    }

    private void ConfigureCrystalReports()
    {
        salesAmount = "4000";
        operatorValue = "<";
        customerName = "K";
        
        string selectionFormula = "{Customer.Last Year's Sales} > " + salesAmount
        + " AND Mid({Customer.Customer Name}, 1, 1) " + operatorValue + "'" + customerName + "'";
        operatorValueList.DataSource = System.Enum.GetValues(typeof(CeComparisonOperator));
        operatorValueList.DataBind();
        customerBySalesNameReport = new ReportDocument();
        string reportPath = Server.MapPath("CustomerBySalesName.rpt");
        customerBySalesNameReport.Load(reportPath);
        customerBySalesNameReport.DataDefinition.RecordSelectionFormula = selectionFormula;
        crystalReportViewer.ReportSource = customerBySalesNameReport;
        formula.Text = selectionFormula;
    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }

    protected void redisplay_Click(object sender, EventArgs e)
    {
        salesAmount = lastYearsSales.Text;
        operatorValue = GetSelectedOperator();
        customerName = letterOfName.Text;
        
        string selectionFormula = "{Customer.Last Year's Sales} > " + salesAmount
        + " AND Mid({Customer.Customer Name}, 1, 1) " + operatorValue + "'" + customerName + "'";
        customerBySalesNameReport.DataDefinition.SavedDataSelectionFormula = selectionFormula;
        crystalReportViewer.ReportSource = customerBySalesNameReport;
        formula.Text = customerBySalesNameReport.DataDefinition.SavedDataSelectionFormula;
    }
    
    private string GetSelectedOperator()
    {
        string selectedOperator = "";

        switch ((CeComparisonOperator)operatorValueList.SelectedIndex)
        {
            case CeComparisonOperator.EqualTo:
                selectedOperator = "=";
                break;
            case CeComparisonOperator.GreaterThan:
                selectedOperator = ">";
                break;
            case CeComparisonOperator.GreaterThanOrEqualTo:
                selectedOperator = ">=";
                break;
            case CeComparisonOperator.LessThan:
                selectedOperator = "<";
                break;
            case CeComparisonOperator.LessThanOrEqualTo:
                selectedOperator = "<=";
                break;
            case CeComparisonOperator.NotEqualTo:
                selectedOperator = "<>";
                break;
        }

        return selectedOperator;
    }
   
}
