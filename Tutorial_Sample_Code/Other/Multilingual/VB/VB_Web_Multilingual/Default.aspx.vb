Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page

    Private hierarchicalGroupingReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        hierarchicalGroupingReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("Hierarchical Grouping.rpt")
        hierarchicalGroupingReport.Load(reportPath)
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport
    End Sub



    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()
    End Sub
End Class
