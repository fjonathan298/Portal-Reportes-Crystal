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
using System.Collections;

public partial class _Default : System.Web.UI.Page 
{
	private ArrayList stockValues;
	private ReportDocument stockObjectsReport;

	private void ConfigureCrystalReports()
	{
		PopulateStockValuesArrayList();

		string reportPath = Server.MapPath("StockObjects.rpt");

		stockObjectsReport = new ReportDocument();
		stockObjectsReport.Load(reportPath);
		stockObjectsReport.SetDataSource(stockValues);
		crystalReportViewer.ReportSource = stockObjectsReport;
	}

	private void Page_Init(Object sender, EventArgs e)
	{
		ConfigureCrystalReports();
	}

	public void PopulateStockValuesArrayList()
	{
		if (Session["stockValues"] == null)
		{
			stockValues = new ArrayList();
            Company c1 = new Company("AWRK","Arthur, Wren, Roberts and King");
            Company c2 = new Company("CTSO","Control Systems and Operations");
            Company c3 = new Company("LTWR","Letter Works");
			Stock s1 = new Stock(c1, 1200, 28.47);
			Stock s2 = new Stock(c2, 800, 128.69);
			Stock s3 = new Stock(c3, 1800, 12.95);
			stockValues.Add(s1);
			stockValues.Add(s2);
			stockValues.Add(s3);
			Session["stockValues"] = stockValues; 
		}
		else
		{
			stockValues = (ArrayList)Session["stockValues"];
		}
	}
	protected void addStockInformation_Click(object sender, EventArgs e)
	{
		Stock temp = new Stock();
		try
		{
            temp.Company = new Company(symbol.Text, name.Text);
			temp.Price = Convert.ToDouble(price.Text);
			temp.Volume = Convert.ToInt32(volume.Text);
		}
		catch
		{
		}
		stockValues.Add(temp);
		Session["stockValues"] = stockValues;
		ConfigureCrystalReports();
	}
}