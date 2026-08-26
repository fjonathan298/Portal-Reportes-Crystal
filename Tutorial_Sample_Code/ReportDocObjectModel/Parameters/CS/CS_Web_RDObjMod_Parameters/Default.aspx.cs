using System;
using System.Data;
using System.Collections;
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
	private ReportDocument customersByCityReport;
	private const string PARAMETER_FIELD_NAME = "City";

	protected void Page_Load(object sender, EventArgs e)
    {

    }

	private void Page_Init(object sender, EventArgs e)
	{
		ConfigureCrystalReports();
	}

	private void ConfigureCrystalReports()
	{
		customersByCityReport = new ReportDocument();
		string reportPath = Server.MapPath("CustomersByCity.rpt");
		customersByCityReport.Load(reportPath);

		ArrayList arrayList = new ArrayList();

		if (!IsPostBack)
		{
			defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(customersByCityReport);
			defaultParameterValuesList.DataBind();
			arrayList.Add("Paris");
			arrayList.Add("Tokyo");
			Session["arrayList"] = arrayList;
		}

		else 
		{
			arrayList = (ArrayList)Session["arrayList"];
		}


		SetCurrentValuesForParameterField(customersByCityReport, arrayList);

		crystalReportViewer.ReportSource = customersByCityReport;
	}

	private void SetCurrentValuesForParameterField(ReportDocument reportDocument, ArrayList arrayList)
	{
		ParameterValues currentParameterValues = new ParameterValues();
		foreach (object submittedValue in arrayList)
		{
			ParameterDiscreteValue parameterDiscreteValue = new ParameterDiscreteValue();
			parameterDiscreteValue.Value = submittedValue.ToString();
			currentParameterValues.Add(parameterDiscreteValue);
		}

		ParameterFieldDefinitions parameterFieldDefinitions = reportDocument.DataDefinition.ParameterFields;
		ParameterFieldDefinition parameterFieldDefinition = parameterFieldDefinitions[PARAMETER_FIELD_NAME];
		parameterFieldDefinition.ApplyCurrentValues(currentParameterValues);

	}

	private ArrayList GetDefaultValuesFromParameterField(ReportDocument reportDocument)
	{
		ParameterFieldDefinitions parameterFieldDefinitions = reportDocument.DataDefinition.ParameterFields;
		ParameterFieldDefinition parameterFieldDefinition = parameterFieldDefinitions[PARAMETER_FIELD_NAME];
		ParameterValues defaultParameterValues = parameterFieldDefinition.DefaultValues;
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
		ConfigureCrystalReports();
	}
}
