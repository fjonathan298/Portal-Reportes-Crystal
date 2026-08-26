using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace CS_Win_RDObjMod_DBLogonSubrpt
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
            northwindCustomersReport = new ReportDocument();
            string reportPath = Application.StartupPath + "\\" + "NorthwindCustomers.rpt";
            northwindCustomersReport.Load(reportPath);

            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.ServerName = "localhost";
            connectionInfo.DatabaseName = "Northwind";
            connectionInfo.IntegratedSecurity = true;
            
            crystalReportViewer.ReportSource = northwindCustomersReport;
            SetDBLogonForReport(connectionInfo, northwindCustomersReport);

            SetDBLogonForSubreports(connectionInfo, northwindCustomersReport);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();
        }

        private void crystalReportViewer_Load(object sender, EventArgs e)
        {

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

        private void SetDBLogonForSubreports(ConnectionInfo connectionInfo, ReportDocument reportDocument)
        {
            Sections sections = reportDocument.ReportDefinition.Sections;
            foreach (Section section in sections)
            {
                ReportObjects reportObjects = section.ReportObjects;
                foreach (ReportObject reportObject in reportObjects)
                {
                    if (reportObject.Kind == ReportObjectKind.SubreportObject)
                    {
                        SubreportObject subreportObject = (SubreportObject)reportObject;
                        ReportDocument subReportDocument = subreportObject.OpenSubreport(subreportObject.SubreportName);
                        SetDBLogonForReport(connectionInfo, subReportDocument);

                    }
                }

            }

        }


    }
}