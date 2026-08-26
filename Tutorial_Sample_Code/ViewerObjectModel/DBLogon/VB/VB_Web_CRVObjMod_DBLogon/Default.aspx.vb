Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page

    Private Sub ConfigureCrystalReports()
        Dim reportPath As String = Server.MapPath("NorthwindCustomers.rpt")
        myCrystalReportViewer.ReportSource = reportPath
        Dim myConnectionInfo As ConnectionInfo = New ConnectionInfo()
        SetDBLogonForReport(myConnectionInfo)
        myConnectionInfo.ServerName = "localhost"
        myConnectionInfo.DatabaseName = "Northwind"
        myConnectionInfo.UserID = "i822460"
        myConnectionInfo.Password = "5"
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()
    End Sub

    Private Sub SetDBLogonForReport(ByVal myConnectionInfo As ConnectionInfo)
        Dim myTableLogOnInfos As TableLogOnInfos = myCrystalReportViewer.LogOnInfo
        For Each myTableLogOnInfo As TableLogOnInfo In myTableLogOnInfos
            myTableLogOnInfo.ConnectionInfo = myConnectionInfo
        Next
    End Sub
End Class
