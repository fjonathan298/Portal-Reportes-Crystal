using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.Windows.Forms;

namespace CS_Win_CRVObjMod_CustomizeViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ConfigureCrystalReports()
        {
            listCRVReport.DataSource = System.Enum.GetValues(typeof(CeWinCRVReportOptions));
            listCRVToolbar.DataSource = System.Enum.GetValues(typeof(CeWinCRVToolbarOptions));
            crystalReportViewer.ReportSource = "C:\\WorldSalesReport.rpt";

            selectBackColor.DataSource = System.Enum.GetValues(typeof(KnownColor));

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
            crystalReportViewer.ShowPageNavigateButtons = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Page_Navigation_Button));
            crystalReportViewer.ShowGotoPageButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Go_to_Page_Button));
            crystalReportViewer.ShowCloseButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Close_View_Button));
            crystalReportViewer.ShowPrintButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Print_Button));
            crystalReportViewer.ShowRefreshButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Refresh_Button));
            crystalReportViewer.ShowExportButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Export_Button));
            crystalReportViewer.ShowGroupTreeButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Group_Tree_Button));
            crystalReportViewer.ShowZoomButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Zoom_Button));
            crystalReportViewer.ShowTextSearchButton = listCRVToolbar.GetSelected(Convert.ToInt32(CeWinCRVToolbarOptions.Search_Button));

            crystalReportViewer.DisplayToolbar = listCRVReport.GetSelected(Convert.ToInt32(CeWinCRVReportOptions.Toolbar));
            if (listCRVReport.GetSelected(Convert.ToInt32(CeWinCRVReportOptions.Group_Tree)))
            {
                crystalReportViewer.ToolPanelView = ToolPanelViewType.GroupTree;
            }
            else
            {
                crystalReportViewer.ToolPanelView = ToolPanelViewType.None;
            }
            crystalReportViewer.DisplayStatusBar = listCRVReport.GetSelected(Convert.ToInt32(CeWinCRVReportOptions.Status_Bar));

            KnownColor selectedKnownColor = (KnownColor)selectBackColor.SelectedItem;
            if (selectedKnownColor != KnownColor.Transparent)
            {
                crystalReportViewer.BackColor = System.Drawing.Color.FromKnownColor(selectedKnownColor);
            }
        }

        private void goToPage_Click(object sender, EventArgs e)
        {
            crystalReportViewer.ShowNthPage(Convert.ToInt32(pageNumber.Text));
        }

        private void updateZoomFactor_Click(object sender, EventArgs e)
        {
            crystalReportViewer.Zoom(Convert.ToInt32(zoomFactor.Text));
        }

        private void search_Click(object sender, EventArgs e)
        {
            bool searchResult = crystalReportViewer.SearchForText(searchText.Text);
            if (searchResult)
            {
                message.Text = MessageConstants.SUCCESS;
            }
            else
            {
                message.Text = MessageConstants.FAILURE;
            }

        }
    }
}