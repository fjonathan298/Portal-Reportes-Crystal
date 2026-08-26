Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page

    Private northwindCustomersReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        northwindCustomersReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("NorthwindCustomers.rpt")
        northwindCustomersReport.Load(reportPath)

        Dim myConnectionInfo As ConnectionInfo = New ConnectionInfo()
        myConnectionInfo.ServerName = "localhost"
        myConnectionInfo.DatabaseName = "Northwind"
        myConnectionInfo.IntegratedSecurity = True
        SetDBLogonForReport(myConnectionInfo, northwindCustomersReport)

        myCrystalReportViewer.ReportSource = northwindCustomersReport
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub

    Private Sub SetDBLogonForReport(ByVal myConnectionInfo As ConnectionInfo, ByVal myReportDocument As ReportDocument)
        Dim myTables As Tables = myReportDocument.Database.Tables
        For Each myTable As CrystalDecisions.CrystalReports.Engine.Table In myTables
            Dim myTableLogonInfo As TableLogOnInfo = myTable.LogOnInfo
            myTableLogonInfo.ConnectionInfo = myConnectionInfo
            myTable.ApplyLogOnInfo(myTableLogonInfo)
        Next

    End Sub
End Class
