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

namespace CS_Win_CRVObjMod_DBLogon
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureCrystalReports()
        {
            string reportPath = Application.StartupPath + "\\" + "NorthwindCustomers.rpt";
            crystalReportViewer1.ReportSource = reportPath;
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.DatabaseName = "Northwind";
            connectionInfo.UserID = "limitedPermissionAccount";
            connectionInfo.Password = "1234";
            connectionInfo.ServerName = "localhost";
            SetDBLogonForReport(connectionInfo);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();
        }

        private void SetDBLogonForReport(ConnectionInfo connectionInfo)
        {
            TableLogOnInfos tableLogOnInfos = crystalReportViewer1.LogOnInfo;
            foreach (TableLogOnInfo tableLogOnInfo in tableLogOnInfos)
            {
                tableLogOnInfo.ConnectionInfo = connectionInfo;
            }
        }
    }
}
