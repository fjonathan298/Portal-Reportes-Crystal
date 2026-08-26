using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;


namespace CS_Win_UseExchangeRate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ReportDocument functionTestReport;

        private void ConfigureCrystalReports()
        {
            functionTestReport = new ReportDocument();
            string reportPath = new DirectoryInfo(Application.StartupPath).Parent.Parent.FullName;
            functionTestReport.Load(reportPath + "\\FunctionTest.rpt");
            crystalReportViewer1.ReportSource = functionTestReport;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();
        }
    }
}
