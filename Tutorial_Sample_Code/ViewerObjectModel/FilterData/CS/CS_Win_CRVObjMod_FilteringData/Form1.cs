using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace CS_Win_CRVObjMod_FilteringData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureCrystalReports()
        {
            selectOperatorList.DataSource = System.Enum.GetValues(typeof(CeComparisonOperator));

            string selectFormula = "{Customer.Last Year's Sales} > 11000.00 "
 + "AND Mid({Customer.Customer Name}, 1, 1) = \"A\"";
            crystalReportViewer.SelectionFormula = selectFormula;

            string reportPath = Application.StartupPath + "\\" + "CustomersBySalesName.rpt";
            crystalReportViewer.ReportSource = reportPath;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();
        }

        private void crystalReportViewer_Load(object sender, EventArgs e)
        {

        }

        private void redisplay_Click(object sender, EventArgs e)
        {
            string selectedOperator = GetSelectedCompareOperator();

            string selectFormula = "{Customer.Last Year's Sales} > " + lastYearsSales.Text
+ " AND Mid({Customer.Customer Name}, 1, 1) " + selectedOperator + " \"" + customerName.Text + "\"";

            crystalReportViewer.SelectionFormula = selectFormula;
            crystalReportViewer.ReportSource = Application.StartupPath + "\\" + "CustomersBySalesName.rpt";
        }

        private string GetSelectedCompareOperator()
        {
            switch ((CeComparisonOperator)selectOperatorList.SelectedIndex)
            {
                case CeComparisonOperator.EqualTo:
                    return "=";
                case CeComparisonOperator.LessThan:
                    return "<";
                case CeComparisonOperator.GreaterThan:
                    return ">";
                case CeComparisonOperator.LessThan_or_EqualTo:
                    return "<=";
                case CeComparisonOperator.GreaterThan_or_EqualTo:
                    return ">=";
                case CeComparisonOperator.Not_EqualTo:
                    return "<>";
                default:
                    return "=";
            }
        }

    }
}