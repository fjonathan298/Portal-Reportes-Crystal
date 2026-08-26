Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page
    Private northwindCustomersReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        Dim myConnectionInfo As ConnectionInfo = New ConnectionInfo()
        myConnectionInfo.DatabaseName = "Northwind"
        myConnectionInfo.UserID = "limitedPermissionAccount"
        myConnectionInfo.Password = "1234"
        myConnectionInfo.ServerName = "localhost"

        northwindCustomersReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("NorthwindCustomers.rpt")
        northwindCustomersReport.Load(reportPath)
        myCrystalReportViewer.ReportSource = northwindCustomersReport
        SetDBLogonForReport(myConnectionInfo, northwindCustomersReport)
    End Sub

    Private Sub SetDBLogonForReport(ByVal myConnectionInfo As ConnectionInfo, ByVal myReportDocument As ReportDocument)
        Dim myTables As Tables = myReportDocument.Database.Tables
        For Each myTable As CrystalDecisions.CrystalReports.Engine.Table In myTables
            Dim myTableLogonInfo As TableLogOnInfo = myTable.LogOnInfo
            myTableLogonInfo.ConnectionInfo = myConnectionInfo
            myTable.ApplyLogOnInfo(myTableLogonInfo)
        Next
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub
End Class
