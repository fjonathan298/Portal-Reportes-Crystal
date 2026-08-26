Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1

    Private northwindCustomersReport As ReportDocument

    Private Sub ConfigureCrystalReports()
        northwindCustomersReport = New ReportDocument()
        Dim reportPath As String = Application.StartupPath & "\" & "NorthwindCustomers.rpt"
        northwindCustomersReport.Load(reportPath)

        Dim myConnectionInfo As ConnectionInfo = New ConnectionInfo()
        myConnectionInfo.ServerName = "localhost"
        myConnectionInfo.DatabaseName = "Northwind"
        myConnectionInfo.IntegratedSecurity = True
        SetDBLogonForReport(myConnectionInfo, northwindCustomersReport)

        myCrystalReportViewer.ReportSource = reportPath
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
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
