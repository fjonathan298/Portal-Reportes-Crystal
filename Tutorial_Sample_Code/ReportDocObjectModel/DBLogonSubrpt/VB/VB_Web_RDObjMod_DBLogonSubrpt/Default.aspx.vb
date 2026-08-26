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

        SetDBLogonForSubreports(myConnectionInfo, northwindCustomersReport)

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

    Private Sub SetDBLogonForSubreports(ByVal myConnectionInfo As ConnectionInfo, ByVal myReportDocument As ReportDocument)
        Dim mySections As Sections = myReportDocument.ReportDefinition.Sections
        For Each mySection As Section In mySections
            Dim myReportObjects As ReportObjects = mySection.ReportObjects
            For Each myReportObject As ReportObject In myReportObjects
                If myReportObject.Kind = ReportObjectKind.SubreportObject Then
                    Dim mySubreportObject As SubreportObject = CType(myReportObject, SubreportObject)
                    Dim subReportDocument As ReportDocument = mySubreportObject.OpenSubreport(mySubreportObject.SubreportName)
                    SetDBLogonForReport(myConnectionInfo, subReportDocument)
                End If
            Next
        Next

    End Sub

End Class
