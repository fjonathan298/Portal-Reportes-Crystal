using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;


namespace CS_Win_RDObjMod_ParametersSubrpt
{
	public partial class Form1 : Form
	{
		private ReportDocument customersByCityReport;
		private const string PARAMETER_FIELD_NAME = "City";
		private const string SUBREPORT_PARAMETER_FIELD_NAME = "OrderDateRange";
		private const string SUBREPORT_NAME = "CustomerOrders";

		public Form1()
		{
			InitializeComponent();
		}

		private void ConfigureCrystalReports()
		{
			customersByCityReport = new ReportDocument();
			string reportPath = Application.StartupPath + "\\" + "CustomersByCity.rpt";
			customersByCityReport.Load(reportPath);

			ArrayList arrayList = new ArrayList();
			arrayList.Add("Paris");
			arrayList.Add("Tokyo");

			string startDate = "8/1/1997";
			string endDate = "8/31/1997";
			SetDateRangeForOrders(customersByCityReport, startDate, endDate);

			defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(customersByCityReport);

			crystalReportViewer.ReportSource = customersByCityReport;
			SetCurrentValuesForParameterField(customersByCityReport, arrayList);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ConfigureCrystalReports();
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

		private void SetDateRangeForOrders(ReportDocument reportDocument, string startDate, string endDate)
		{
			ParameterRangeValue parameterRangeValue = new ParameterRangeValue();
			parameterRangeValue.StartValue = startDate;
			parameterRangeValue.EndValue = endDate;
			parameterRangeValue.LowerBoundType = RangeBoundType.BoundInclusive;
			parameterRangeValue.UpperBoundType = RangeBoundType.BoundInclusive;
			ParameterFieldDefinitions parameterFieldDefinitions = reportDocument.DataDefinition.ParameterFields;
			ParameterFieldDefinition parameterFieldDefinition = parameterFieldDefinitions[SUBREPORT_PARAMETER_FIELD_NAME, SUBREPORT_NAME];
			parameterFieldDefinition.CurrentValues.Clear();
			parameterFieldDefinition.CurrentValues.Add(parameterRangeValue);
			parameterFieldDefinition.ApplyCurrentValues(parameterFieldDefinition.CurrentValues);
		}


		private void redisplay_Click(object sender, EventArgs e)
		{
			ArrayList arrayList = new ArrayList();

			foreach (string item in defaultParameterValuesList.SelectedItems)
			{
				arrayList.Add(item);
			}

			SetCurrentValuesForParameterField(customersByCityReport, arrayList);

			string startDate = orderStartDate.Text;
			string endDate = orderEndDate.Text;
			SetDateRangeForOrders(customersByCityReport, startDate, endDate);

			crystalReportViewer.ReportSource = customersByCityReport;
		}

	}
}