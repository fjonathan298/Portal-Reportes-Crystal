Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared


Public Class Form1

    Private functionTestReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        functionTestReport = New ReportDocument()
        functionTestReport.Load((IO.Directory.GetParent(IO.Directory.GetParent(Application.StartupPath).ToString).ToString) & "\FunctionTest.rpt")
        CrystalReportViewer1.ReportSource = functionTestReport
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

End Class
