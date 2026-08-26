Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Data


Partial Class _Default
    Inherits System.Web.UI.Page
    Private customerReport As ReportDocument


    Private Sub ConfigureCrystalReports()
        customerReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("Customer.rpt")
        customerReport.Load(reportPath)
        Dim myDataSet As DataSet = DataSetConfiguration.CustomerDataSet
        customerReport.SetDataSource(myDataSet)
        myCrystalReportViewer.ReportSource = customerReport


    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub
End Class
