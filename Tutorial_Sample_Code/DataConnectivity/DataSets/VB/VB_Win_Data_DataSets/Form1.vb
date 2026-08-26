Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Data


Public Class Form1

    Private customerReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        customerReport = New ReportDocument()
        Dim reportPath As String = Application.StartupPath & "\" & "Customer.rpt"
        customerReport.Load(reportPath)
        Dim myDataSet As DataSet = DataSetConfiguration.CustomerDataSet
        customerReport.SetDataSource(myDataSet)
        myCrystalReportViewer.ReportSource = customerReport


    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub
End Class
