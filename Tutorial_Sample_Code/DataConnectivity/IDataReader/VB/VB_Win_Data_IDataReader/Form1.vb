Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1
    Private customersViaIdrReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        customersViaIdrReport = New ReportDocument()
        Dim reportPath As String = Application.StartupPath & "\" & "CustomersViaIDR.rpt"
        customersViaIdrReport.Load(reportPath)
        myCrystalReportViewer.ReportSource = customersViaIdrReport
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub
End Class
