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

namespace CS_Base
{
    public partial class Form1 : Form
    {
        private const string PARAMETER_FIELD_NAME = "City";

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
            try
            {
                ParameterField parameterField = parameterFields[PARAMETER_FIELD_NAME];
                parameterField.CurrentValues = currentParameterValues;
            }
            catch
            {
                Console.WriteLine("Exception caught");
            }

        }


        private void ConfigureCrystalReports()
        {
            ArrayList arrayList = new ArrayList();
            arrayList.Add("Paris");
            arrayList.Add("Tokyo");
            string reportPath = Application.StartupPath + "\\" + "CustomersByCity.rpt";
            crystalReportViewer.ReportSource = reportPath;
            ParameterFields parameterFields = crystalReportViewer.ParameterFieldInfo;
            SetCurrentValuesForParameterField(parameterFields, arrayList);
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
            ArrayList arrayList = new ArrayList();
            try
            {
                ParameterField parameterField = parameterFields[PARAMETER_FIELD_NAME];
                ParameterValues defaultParameterValues = parameterField.DefaultValues;
                foreach (ParameterValue parameterValue in defaultParameterValues)
                {
                    if (!parameterValue.IsRange)
                    {
                        ParameterDiscreteValue parameterDiscreteValue = (ParameterDiscreteValue)parameterValue;
                        arrayList.Add(parameterDiscreteValue.Value.ToString());
                    }
                }
            }
            catch
            {
                Console.WriteLine("exception is caught");
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
            crystalReportViewer.ReportSource = Application.StartupPath + "\\" + "CustomersByCity.rpt";
            ParameterFields parameterFields = crystalReportViewer.ParameterFieldInfo;
            SetCurrentValuesForParameterField(parameterFields, arrayList);

        }
    }
}