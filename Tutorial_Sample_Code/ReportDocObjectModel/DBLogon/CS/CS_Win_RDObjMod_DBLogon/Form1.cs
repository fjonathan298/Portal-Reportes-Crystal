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
        private ReportDocument northwindCustomersReport;

        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureCrystalReports()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.DatabaseName = "Northwind";
            connectionInfo.UserID = "limitedPermissionAccount";
            connectionInfo.Password = "1234";
            connectionInfo.ServerName = "localhost";

            northwindCustomersReport = new ReportDocument();
            string reportPath = Application.StartupPath + "\\" + "NorthwindCustomers.rpt";
            northwindCustomersReport.Load(reportPath);
            crystalReportViewer.ReportSource = northwindCustomersReport;

            SetDBLogonForReport(connectionInfo, northwindCustomersReport);

        }
        private void SetDBLogonForReport(ConnectionInfo connectionInfo, ReportDocument reportDocument)
        {
            Tables tables = reportDocument.Database.Tables;
            foreach (CrystalDecisions.CrystalReports.Engine.Table table in tables)
            {
                TableLogOnInfo tableLogonInfo = table.LogOnInfo;
                tableLogonInfo.ConnectionInfo = connectionInfo;
                table.ApplyLogOnInfo(tableLogonInfo);

            }
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