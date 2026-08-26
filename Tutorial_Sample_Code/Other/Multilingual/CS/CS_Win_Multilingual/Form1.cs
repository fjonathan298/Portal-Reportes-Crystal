using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;


namespace CS_Win_Multilingual
{
	public partial class Form1 : Form
	{
		private Hierarchical_Grouping hierarchicalGroupingReport;

		
		public Form1()
		{
			InitializeComponent();
		}

		private void ConfigureCrystalReports()
		{
			hierarchicalGroupingReport = new Hierarchical_Grouping();
			crystalReportViewer.ReportSource = hierarchicalGroupingReport;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			ConfigureCrystalReports();
		}

	}
}