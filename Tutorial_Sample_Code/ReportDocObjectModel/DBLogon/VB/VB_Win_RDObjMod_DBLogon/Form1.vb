Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1

    Private northwindCustomersReport As ReportDocument


    Private Sub ConfigureCrystalReports()
        Dim myConnectionInfo As ConnectionInfo = New ConnectionInfo()
        myConnectionInfo.DatabaseName = "Northwind"
        myConnectionInfo.UserID = "limitedPermissionAccount"
        myConnectionInfo.Password = "1234"
        myConnectionInfo.ServerName = "localhost"

        northwindCustomersReport = New ReportDocument()
        Dim reportPath As String = Application.StartupPath & "\" & "NorthwindCustomers.rpt"
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


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub
End Class
