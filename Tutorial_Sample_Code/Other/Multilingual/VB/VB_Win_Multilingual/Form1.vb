
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1
    Private hierarchicalGroupingReport As Hierarchical_Grouping


    Private Sub ConfigureCrystalReports()
        hierarchicalGroupingReport = New Hierarchical_Grouping()
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub
End Class
