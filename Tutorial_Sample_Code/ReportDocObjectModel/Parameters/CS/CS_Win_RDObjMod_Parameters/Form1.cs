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


namespace CS_Win_RDObjMod_Parameters
{
	public partial class Form1 : Form
	{
		private ReportDocument customersByCityReport;
		private const string PARAMETER_FIELD_NAME = "City";

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

		private void redisplay_Click(object sender, EventArgs e)
		{
			ArrayList arrayList = new ArrayList();

			foreach (string item in defaultParameterValuesList.SelectedItems)
			{
				arrayList.Add(item);
			}

			SetCurrentValuesForParameterField(customersByCityReport, arrayList);
			crystalReportViewer.ReportSource = customersByCityReport;
		}

	}
}