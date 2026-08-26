Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1

    Private Sub ConfigureCrystalReports()
        Dim reportPath As String = Application.StartupPath & "\" & "NorthwindCustomers.rpt"
        myCrystalReportViewer.ReportSource = reportPath
        Dim myConnectionInfo As ConnectionInfo = New ConnectionInfo()
        SetDBLogonForReport(myConnectionInfo)
        myConnectionInfo.ServerName = "localhost"
        myConnectionInfo.DatabaseName = "Northwind"
        myConnectionInfo.UserID = "i822460"
        myConnectionInfo.Password = ""
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Private Sub SetDBLogonForReport(ByVal myConnectionInfo As ConnectionInfo)
        Dim myTableLogOnInfos As TableLogOnInfos = myCrystalReportViewer.LogOnInfo
        For Each myTableLogOnInfo As TableLogOnInfo In myTableLogOnInfos
            myTableLogOnInfo.ConnectionInfo = myConnectionInfo
        Next
    End Sub

End Class
