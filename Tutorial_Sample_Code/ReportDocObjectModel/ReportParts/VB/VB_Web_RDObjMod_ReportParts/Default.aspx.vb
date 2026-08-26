Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page
    Private customersReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        customersReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("Customers.rpt")
        customersReport.Load(reportPath)
        myCrystalReportPartsViewer.ReportSource = customersReport
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()
    End Sub
End Class
