using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace CS_Base
{
    public partial class Form1 : Form
    {
        private ReportDocument customersViaIdrReport;
        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureCrystalReports()
        {
            customersViaIdrReport = new ReportDocument();
            string reportPath = Application.StartupPath + "\\" + "CustomersViaIDR.rpt";
            customersViaIdrReport.Load(reportPath);
            crystalReportViewer.ReportSource = customersViaIdrReport;
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