Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Drawing
Imports CrystalDecisions.Web


Partial Class _Default
    Inherits System.Web.UI.Page

    Private Sub ConfigureCrystalReports()
        If Not IsPostBack Then
            listCRVReport.DataSource = System.Enum.GetValues(GetType(CeWebCRVReportOptions))
            listCRVReport.DataBind()
            listCRVToolbar.DataSource = System.Enum.GetValues(GetType(CeWebCRVToolbarOptions))
            listCRVToolbar.DataBind()

            selectBackColor.DataSource = System.Enum.GetValues(GetType(KnownColor))
            selectBackColor.DataBind()

            selectBorderStyle.DataSource = System.Enum.GetValues(GetType(BorderStyle))
            selectBorderStyle.DataBind()
            selectBorderColor.DataSource = System.Enum.GetValues(GetType(KnownColor))
            selectBorderColor.DataBind()

        End If
        myCrystalReportViewer.ReportSource = "C:\WorldSalesReport.rpt"

        If Not IsNothing(Session("myBackColor")) Then
            myCrystalReportViewer.BackColor = Color.FromName(CType(Session("myBackColor"), String))
        End If
        If Not IsNothing(Session("myBorderColor")) Then
            myCrystalReportViewer.BorderColor = Color.FromName(CType(Session("myBorderColor"), String))
        End If
        myCrystalReportViewer.BorderStyle = CType(Session("myBorderStyle"), BorderStyle)
        myCrystalReportViewer.BorderWidth = Convert.ToInt32(Session("myBorderWidth"))
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub

    Protected Sub redisplay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles redisplay.Click
        myCrystalReportViewer.HasToggleGroupTreeButton = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Group_Tree_Button)).Selected
        myCrystalReportViewer.HasExportButton = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Export_Button)).Selected
        myCrystalReportViewer.HasPrintButton = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Print_Button)).Selected
        myCrystalReportViewer.HasViewList = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.View_List_Button)).Selected
        myCrystalReportViewer.HasDrillUpButton = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Drill_Up_Button)).Selected
        myCrystalReportViewer.HasPageNavigationButtons = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Page_Navigation_Button)).Selected
        myCrystalReportViewer.HasGotoPageButton = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Go_to_Page_Button)).Selected
        myCrystalReportViewer.HasSearchButton = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Search_Button)).Selected
        myCrystalReportViewer.HasZoomFactorList = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Zoom_Button)).Selected
        myCrystalReportViewer.HasCrystalLogo = listCRVToolbar.Items(Convert.ToInt32(CeWebCRVToolbarOptions.Crystal_Logo)).Selected

        myCrystalReportViewer.DisplayToolbar = listCRVReport.Items(Convert.ToInt32(CeWebCRVReportOptions.Toolbar)).Selected

        If (listCRVReport.Items(Convert.ToInt32(CeWebCRVReportOptions.Group_Tree)).Selected) Then
            myCrystalReportViewer.ToolPanelView = ToolPanelViewType.GroupTree
        Else
            myCrystalReportViewer.ToolPanelView = ToolPanelViewType.None
        End If
        myCrystalReportViewer.DisplayPage = listCRVReport.Items(Convert.ToInt32(CeWebCRVReportOptions.Main_Page)).Selected
        myCrystalReportViewer.SeparatePages = listCRVReport.Items(Convert.ToInt32(CeWebCRVReportOptions.Enable_Separate_Pages)).Selected

        myCrystalReportViewer.BackColor = Color.FromName(selectBackColor.SelectedItem.Text)

        Session("myBackColor") = selectBackColor.SelectedItem.Text
    End Sub

    Protected Sub goToPage_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles goToPage.Click
        myCrystalReportViewer.ShowNthPage(Convert.ToInt32(pageNumber.Text))

    End Sub

    Protected Sub updateZoomFactor_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles updateZoomFactor.Click
        myCrystalReportViewer.Zoom(Convert.ToInt32(zoomFactor.Text))
    End Sub

    Protected Sub search_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles search.Click
        Dim mySearchResult As Boolean = myCrystalReportViewer.SearchAndHighlightText(searchText.Text, SearchDirection.Forward)
        If Not mySearchResult Then
            message.Text = MessageConstants.SUCCESS
        Else
            message.Text = MessageConstants.FAILURE
        End If

    End Sub

    Protected Sub drawBorder_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles drawBorder.Click
        myCrystalReportViewer.BorderWidth = Unit.Parse(borderWidth.Text.ToString())
        myCrystalReportViewer.BorderStyle = CType(selectBorderStyle.SelectedIndex, BorderStyle)
        myCrystalReportViewer.BorderColor = Color.FromName(selectBorderColor.SelectedItem.Text)

        Session("myBorderColor") = myCrystalReportViewer.BorderColor.ToString()
        Session("myBorderStyle") = myCrystalReportViewer.BorderStyle
        Session("myBorderWidth") = myCrystalReportViewer.BorderWidth

        Session("myBorderWidth") = borderWidth.Text
        Session("myBorderStyle") = selectBorderStyle.SelectedIndex
        Session("myBorderColor") = selectBorderColor.SelectedItem.Text

    End Sub
End Class
