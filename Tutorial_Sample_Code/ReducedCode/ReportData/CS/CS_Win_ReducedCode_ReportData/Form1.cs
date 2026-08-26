using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CS_Win_ReducedCode_ReportData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void crystalReportViewer_Drill(object source, CrystalDecisions.Windows.Forms.DrillEventArgs e)
        {
            drillLabel.Text = "You drilled down on: " + e.NewGroupName;
        }
    }
}