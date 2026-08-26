using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Drawing;
using CrystalDecisions.Web;

public partial class _Default : System.Web.UI.Page 
{

    private void ConfigureCrystalReports()
    {
        if (!IsPostBack)
        {
            listCRVReport.DataSource = System.Enum.GetValues(typeof(CeWebCRVReportOptions));
            listCRVReport.DataBind();
            listCRVToolbar.DataSource = System.Enum.GetValues(typeof(CeWebCRVToolbarOptions));
            listCRVToolbar.DataBind();

            selectBackColor.DataSource = System.Enum.GetValues(typeof(KnownColor));
            selectBackColor.DataBind();

            selectBorderStyle.DataSource = System.Enum.GetValues(typeof(BorderStyle));
            selectBorderStyle.DataBind();
            selectBorderColor.DataSource = System.Enum.GetValues(typeof(KnownColor));
            selectBorderColor.DataBind();
        }
        crystalReportViewer.ReportSource = "C:\\WorldSalesReport.rpt";

        if (Session["backColor"] != null)
        {
            crystalReportViewer.BackColor = Color.FromName((string)Session["backColor"]);
        }
        if (Session["borderColor"] != null)
        {
            crystalReportViewer.BorderColor = Color.FromName((string)Session["borderColor"]);
        }
        if (Session["borderStyle"] != null)
        {
            crystalReportViewer.BorderStyle = (BorderStyle)Session["borderStyle"];
        }
        if (Session["borderStyle"] != null)
        {
            crystalReportViewer.BorderWidth = Convert.ToInt32(Session["borderStyle"]);
        }

    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }
    protected void redisplay_Click(object sender, EventArgs e)
    {
        crystalReportViewer.HasToggleGroupTreeButton = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Group_Tree_Button)].Selected;
        crystalReportViewer.HasExportButton = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Export_Button)].Selected;
        crystalReportViewer.HasPrintButton = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Print_Button)].Selected;
        crystalReportViewer.HasViewList = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.View_List_Button)].Selected;
        crystalReportViewer.HasDrillUpButton = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Drill_Up_Button)].Selected;
        crystalReportViewer.HasPageNavigationButtons = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Page_Navigation_Button)].Selected;
        crystalReportViewer.HasGotoPageButton = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Go_to_Page_Button)].Selected;
        crystalReportViewer.HasSearchButton = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Search_Button)].Selected;
        crystalReportViewer.HasZoomFactorList = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Zoom_Button)].Selected;
        crystalReportViewer.HasCrystalLogo = listCRVToolbar.Items[Convert.ToInt32(CeWebCRVToolbarOptions.Crystal_Logo)].Selected;

        crystalReportViewer.DisplayToolbar = listCRVReport.Items[Convert.ToInt32(CeWebCRVReportOptions.Toolbar)].Selected;
        if (listCRVReport.Items[Convert.ToInt32(CeWebCRVReportOptions.Group_Tree)].Selected)
        {
            crystalReportViewer.ToolPanelView = ToolPanelViewType.GroupTree;
        }
        else
        {
            crystalReportViewer.ToolPanelView = ToolPanelViewType.None;
        }

        crystalReportViewer.DisplayPage = listCRVReport.Items[Convert.ToInt32(CeWebCRVReportOptions.Main_Page)].Selected;
        crystalReportViewer.SeparatePages = listCRVReport.Items[Convert.ToInt32(CeWebCRVReportOptions.Enable_Separate_Pages)].Selected;

        crystalReportViewer.BackColor = Color.FromName(selectBackColor.SelectedItem.Text);

        Session["backColor"] = selectBackColor.SelectedItem.Text;


    }
    protected void goToPage_Click(object sender, EventArgs e)
    {
        crystalReportViewer.ShowNthPage(Convert.ToInt32(pageNumber.Text));
    }
    protected void updateZoomFactor_Click(object sender, EventArgs e)
    {
        crystalReportViewer.Zoom(Convert.ToInt32(zoomFactor.Text));
    }
    protected void search_Click(object sender, EventArgs e)
    {
        bool searchResult = crystalReportViewer.SearchAndHighlightText(searchText.Text, SearchDirection.Forward);
        if (!searchResult)
        {
            message.Text = MessageConstants.SUCCESS;
        }
        else
        {
            message.Text = MessageConstants.FAILURE;
        }

    }
    protected void drawBorder_Click(object sender, EventArgs e)
    {
        crystalReportViewer.BorderWidth = Convert.ToInt32(borderWidth.Text);
        crystalReportViewer.BorderStyle = (BorderStyle)selectBorderStyle.SelectedIndex;
        crystalReportViewer.BorderColor = Color.FromName(selectBorderColor.SelectedItem.Text);

        Session["borderColor"] = crystalReportViewer.BorderColor.ToString();
        Session["borderStyle"] = crystalReportViewer.BorderStyle;
        Session["borderWidth"] = crystalReportViewer.BorderWidth;

        Session["borderWidth"] = borderWidth.Text;
        Session["borderStyle"] = selectBorderStyle.SelectedIndex;
        Session["borderColor"] = selectBorderColor.SelectedItem.Text;

    }
}
