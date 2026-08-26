Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page
    Private customersViaIdrReport As ReportDocument


    Private Sub ConfigureCrystalReports()
        customersViaIdrReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("CustomersViaIDR.rpt")
        customersViaIdrReport.Load(reportPath)
        myCrystalReportViewer.ReportSource = customersViaIdrReport
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub
End Class
