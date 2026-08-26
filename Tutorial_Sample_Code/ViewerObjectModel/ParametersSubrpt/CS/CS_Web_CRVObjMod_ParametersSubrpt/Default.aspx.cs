using System;
using System.Data;
using System.Configuration;
using System.Collections;
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
	private const string PARAMETER_FIELD_NAME = "City";
    private const string SUBREPORT_PARAMETER_FIELD_NAME = "OrderDateRange";
    private const string SUBREPORT_NAME = "CustomerOrders";
	
	protected void Page_Load(object sender, EventArgs e)
    {

    }
	
    private void Page_Init(object sender, EventArgs e)
	{
		ConfigureCrystalReports();
	}
    
	private void ConfigureCrystalReports()
	{
		ArrayList arrayList = new ArrayList();
        string startDate;
        string endDate;
		string reportPath = Server.MapPath("CustomersByCity.rpt");
		crystalReportViewer.ReportSource = reportPath;
		ParameterFields parameterFields = crystalReportViewer.ParameterFieldInfo;

		if (!IsPostBack)
		{
			defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(parameterFields);
			defaultParameterValuesList.DataBind();
			arrayList.Add("Paris");
			arrayList.Add("Tokyo");
            startDate = "8/1/1997";
            endDate = "8/31/1997";
			Session["arrayList"] = arrayList;
            Session["startDate"] = startDate;
            Session["endDate"] = endDate;

		}
		else
		{
			arrayList = (ArrayList)Session["arrayList"];
            startDate = Session["startDate"].ToString();
            endDate = Session["endDate"].ToString();

		}

       	SetCurrentValuesForParameterField(parameterFields, arrayList);
        SetDateRangeForOrders(parameterFields, startDate, endDate);
	}
	
	private void SetCurrentValuesForParameterField(ParameterFields parameterFields, ArrayList arrayList)
	{
		ParameterValues currentParameterValues = new ParameterValues();
		foreach (object submittedValue in arrayList)
		{
			ParameterDiscreteValue parameterDiscreteValue = new ParameterDiscreteValue();
			parameterDiscreteValue.Value = submittedValue.ToString();
			currentParameterValues.Add(parameterDiscreteValue);
		}

		ParameterField parameterField = parameterFields[PARAMETER_FIELD_NAME];
		parameterField.CurrentValues = currentParameterValues;

	}

	private ArrayList GetDefaultValuesFromParameterField(ParameterFields parameterFields)
	{
		ParameterField parameterField = parameterFields[PARAMETER_FIELD_NAME];
		ParameterValues defaultParameterValues = parameterField.DefaultValues;
		ArrayList arrayList = new ArrayList();
		foreach (ParameterValue parameterValue in defaultParameterValues)
		{
			if (!parameterValue.IsRange)
			{
				ParameterDiscreteValue parameterDiscreteValue = (ParameterDiscreteValue)parameterValue;
				arrayList.Add(parameterDiscreteValue.Value.ToString());
			}
		}

		return arrayList;
	}

	protected void redisplay_Click(object sender, EventArgs e)
	{
		ArrayList arrayList = new ArrayList();
		foreach (ListItem item in defaultParameterValuesList.Items)
		{
			if (item.Selected)
			{
				arrayList.Add(item.Value);
			}
		}
        Session["arrayList"] = arrayList;
        Session["startDate"] = orderStartDate.Text;
        Session["endDate"] = orderEndDate.Text;

		ConfigureCrystalReports();
	}

    private void SetDateRangeForOrders(ParameterFields parameterFields, string startDate, string endDate)
    {
        ParameterRangeValue parameterRangeValue = new ParameterRangeValue();
        parameterRangeValue.StartValue = startDate;
        parameterRangeValue.EndValue = endDate;
        parameterRangeValue.LowerBoundType = RangeBoundType.BoundInclusive;
        parameterRangeValue.UpperBoundType = RangeBoundType.BoundInclusive;
        ParameterField parameterField = parameterFields[SUBREPORT_PARAMETER_FIELD_NAME, SUBREPORT_NAME];
        parameterField.CurrentValues.Clear();
        parameterField.CurrentValues.Add(parameterRangeValue);
    }
}
