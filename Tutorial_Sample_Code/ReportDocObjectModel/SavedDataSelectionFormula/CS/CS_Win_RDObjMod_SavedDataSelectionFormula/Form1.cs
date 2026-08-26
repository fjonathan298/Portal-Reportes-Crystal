using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CS_Win_RDObjMod_SavedDataSelectionFormula
{
    
    public partial class Form1 : Form
    {
        private CustomerBySalesName customerBySalesNameReport;
        private string salesAmount;
        private string operatorValue;
        private string customerName;
        private bool useDefaultValues = true;
        
        public Form1()
        {
            InitializeComponent();
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            customerBySalesNameReport.Close();
            crystalReportViewer.Dispose();
        }

        private void ConfigureCrystalReports()
        {
            if (useDefaultValues)
            {
                salesAmount = "4000";
                operatorValue = "<";
                customerName = "K";
                operatorValueList.DataSource = System.Enum.GetValues(typeof(CeComparisonOperator));
            }

            string selectionFormula = "{Customer.Last Year's Sales} > " + salesAmount
            + " AND Mid({Customer.Customer Name}, 1, 1) " + operatorValue + "'" + customerName + "'";
            customerBySalesNameReport = new CustomerBySalesName();
            customerBySalesNameReport.DataDefinition.RecordSelectionFormula = selectionFormula;
            crystalReportViewer.ReportSource = customerBySalesNameReport;
            formula.Text = selectionFormula;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureCrystalReports();
        }

        private void redisplay_Click(object sender, EventArgs e)
        {
            salesAmount = lastYearsSales.Text;
            operatorValue = GetSelectedOperator();
            customerName = letterOfName.Text;
            useDefaultValues = false;

            string selectionFormula = "{Customer.Last Year's Sales} > " + salesAmount
            + " AND Mid({Customer.Customer Name}, 1, 1) " + operatorValue + "'" + customerName + "'";
            customerBySalesNameReport.DataDefinition.SavedDataSelectionFormula = selectionFormula;
            crystalReportViewer.ReportSource = customerBySalesNameReport;
            formula.Text = selectionFormula;
        } 

        private string GetSelectedOperator()
        {
            string selectedOperator = "";

            switch ((CeComparisonOperator)operatorValueList.SelectedIndex)
            {
                case CeComparisonOperator.EqualTo:
                    selectedOperator = "=";
                    break;
                case CeComparisonOperator.GreaterThan:
                    selectedOperator = ">";
                    break;
                case CeComparisonOperator.GreaterThanOrEqualTo:
                    selectedOperator = ">=";
                    break;
                case CeComparisonOperator.LessThan:
                    selectedOperator = "<";
                    break;
                case CeComparisonOperator.LessThanOrEqualTo:
                    selectedOperator = "<=";
                    break;
                case CeComparisonOperator.NotEqualTo:
                    selectedOperator = "<>";
                    break;
            }

            return selectedOperator;
        }
    }
}