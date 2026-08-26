using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CS_Win_Data_DataSets;

namespace CS_Base
{
    public partial class Form1 : Form
    {
        private ReportDocument customerReport;

        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureCrystalReports()
        {
            customerReport = new ReportDocument();
            string reportPath = Application.StartupPath + "\\" + "Customer.rpt";
            customerReport.Load(reportPath);
            DataSet dataSet = DataSetConfiguration.CustomerDataSet;
            customerReport.SetDataSource(dataSet);
            crystalReportViewer.ReportSource = customerReport;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();

        }

        private void crystalReportViewer_Load(object sender, EventArgs e)
        {

        }
    }
}