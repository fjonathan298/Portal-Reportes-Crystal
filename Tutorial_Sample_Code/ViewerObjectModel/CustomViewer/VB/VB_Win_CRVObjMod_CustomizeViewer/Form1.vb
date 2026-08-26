Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports CrystalDecisions.Windows.Forms

Public Class Form1

    Private Sub ConfigureCrystalReports()
        listCRVReport.DataSource = System.Enum.GetValues(GetType(CeWinCRVReportOptions))
        listCRVToolbar.DataSource = System.Enum.GetValues(GetType(CeWinCRVToolbarOptions))
        myCrystalReportViewer.ReportSource = "C:\WorldSalesReport.rpt"

        selectBackColor.DataSource = System.Enum.GetValues(GetType(KnownColor))
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Private Sub redisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles redisplay.Click
        myCrystalReportViewer.ShowPageNavigateButtons = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Page_Navigation_Button)
        myCrystalReportViewer.ShowGotoPageButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Go_to_Page_Button)
        myCrystalReportViewer.ShowCloseButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Close_View_Button)
        myCrystalReportViewer.ShowPrintButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Print_Button)
        myCrystalReportViewer.ShowRefreshButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Refresh_Button)
        myCrystalReportViewer.ShowExportButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Export_Button)
        myCrystalReportViewer.ShowGroupTreeButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Group_Tree_Button)
        myCrystalReportViewer.ShowZoomButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Zoom_Button)
        myCrystalReportViewer.ShowTextSearchButton = listCRVToolbar.GetSelected(CeWinCRVToolbarOptions.Search_Button)

        myCrystalReportViewer.DisplayToolbar = listCRVReport.GetSelected(CeWinCRVReportOptions.Toolbar)

        If (listCRVReport.GetSelected(Convert.ToInt32(CeWinCRVReportOptions.Group_Tree))) Then
            myCrystalReportViewer.ToolPanelView = ToolPanelViewType.GroupTree
        Else
            myCrystalReportViewer.ToolPanelView = ToolPanelViewType.None
        End If

        myCrystalReportViewer.DisplayStatusBar = listCRVReport.GetSelected(CeWinCRVReportOptions.Status_Bar)

        Dim mySelectedKnownColor As KnownColor = CType(selectBackColor.SelectedItem, KnownColor)
        If Not mySelectedKnownColor = KnownColor.Transparent Then
            myCrystalReportViewer.BackColor = System.Drawing.Color.FromKnownColor(mySelectedKnownColor)
        End If

    End Sub

    Private Sub goToPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles goToPage.Click
        myCrystalReportViewer.ShowNthPage(Convert.ToInt32(pageNumber.Text))

    End Sub

    Private Sub updateZoomFactor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles updateZoomFactor.Click
        myCrystalReportViewer.Zoom(Convert.ToInt32(zoomFactor.Text))
    End Sub

    Private Sub search_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles search.Click
        Dim mySearchResult As Boolean = myCrystalReportViewer.SearchForText(searchText.Text)
        If mySearchResult Then
            message.Text = MessageConstants.SUCCESS
        Else
            message.Text = MessageConstants.FAILURE
        End If
    End Sub
End Class
