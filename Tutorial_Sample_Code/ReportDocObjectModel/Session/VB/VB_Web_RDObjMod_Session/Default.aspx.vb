Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page
    Private hierarchicalGroupingReport As ReportDocument


    Private Sub ConfigureCrystalReports()
        If (Session("hierarchicalGroupingReport") Is Nothing) Then
            hierarchicalGroupingReport = New ReportDocument()
            hierarchicalGroupingReport.Load(Server.MapPath("Hierarchical Grouping.rpt"))
            Session("hierarchicalGroupingReport") = hierarchicalGroupingReport
        Else
            hierarchicalGroupingReport = CType(Session("hierarchicalGroupingReport"), ReportDocument)
        End If
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport

    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub

    Protected Sub sortOrderDescending_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles sortOrderDescending.Click
        Dim mySortFields As SortFields = hierarchicalGroupingReport.DataDefinition.SortFields
        Dim firstSortField As SortField = mySortFields(0)
        firstSortField.SortDirection = SortDirection.DescendingOrder
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport
        Session("hierarchicalGroupingReport") = hierarchicalGroupingReport
        ConfigureCrystalReports()
    End Sub

    Protected Sub sortOrderAscending_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles sortOrderAscending.Click
        Dim mySortFields As SortFields = hierarchicalGroupingReport.DataDefinition.SortFields
        Dim firstSortField As SortField = mySortFields(0)
        firstSortField.SortDirection = SortDirection.AscendingOrder
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport
        Session("hierarchicalGroupingReport") = hierarchicalGroupingReport
        ConfigureCrystalReports()
    End Sub
End Class
