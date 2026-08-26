using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Collections;

namespace CS_Win_CRVObjMod_ParametersSubrpt
{
    public partial class Form1 : Form
    {
        private const string PARAMETER_FIELD_NAME = "City";
        private const string SUBREPORT_PARAMETER_FIELD_NAME = "OrderDateRange";
        private const string SUBREPORT_NAME = "CustomerOrders";


        public Form1()
        {
            InitializeComponent();
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


        private void ConfigureCrystalReports()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add("Paris");
            arrayList.Add("Tokyo");

            string startDate = "8/1/1997";
            string endDate = "8/31/1997";

            string reportPath = Application.StartupPath + "\\" + "CustomersByCity.rpt";
            crystalReportViewer.ReportSource = reportPath;
            ParameterFields parameterFields = crystalReportViewer.ParameterFieldInfo;
            SetCurrentValuesForParameterField(parameterFields, arrayList);
            SetDateRangeForOrders(parameterFields, startDate, endDate);
            defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(parameterFields);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();
        }

        private void crystalReportViewer_Load(object sender, EventArgs e)
        {

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

        private void redisplay_Click(object sender, EventArgs e)
        {
            ArrayList arrayList = new ArrayList();
            foreach (string item in defaultParameterValuesList.SelectedItems)
            {
                arrayList.Add(item);
            }

            string startDate = orderStartDate.Text;
            string endDate = orderEndDate.Text;

            crystalReportViewer.ReportSource = Application.StartupPath + "\\" + "CustomersByCity.rpt";
            ParameterFields parameterFields = crystalReportViewer.ParameterFieldInfo;
            SetCurrentValuesForParameterField(parameterFields, arrayList);

            SetDateRangeForOrders(parameterFields, startDate, endDate);

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
}
