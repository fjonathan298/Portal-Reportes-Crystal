Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Collections

Partial Class _Default
    Inherits System.Web.UI.Page
    Private stockValues As ArrayList
    Private stockObjectsReport As ReportDocument
    Private Sub ConfigureCrystalReports()
        PopulateStockValuesArrayList()
        Dim reportPath As String = Server.MapPath("StockObjects.rpt")

        stockObjectsReport = New ReportDocument()
        stockObjectsReport.Load(reportPath)
        stockObjectsReport.SetDataSource(stockValues)
        myCrystalReportViewer.ReportSource = stockObjectsReport

    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

    End Sub
    Public Sub PopulateStockValuesArrayList()
        If (Session("stockValues") Is Nothing) Then
            stockValues = New ArrayList
            Dim c1 As Company = New Company("AWRK", "Arthur, Wren, Roberts and King")
            Dim c2 As Company = New Company("CTSO", "Control Systems and Operations")
            Dim c3 As Company = New Company("LTWR", "Letter Works")
            Dim s1 As Stock = New Stock(c1, 1200, 28.47)
            Dim s2 As Stock = New Stock(c2, 800, 128.69)
            Dim s3 As Stock = New Stock(c3, 1800, 12.95)
            stockValues.Add(s1)
            stockValues.Add(s2)
            stockValues.Add(s3)
            Session("stockValues") = stockValues

        Else
            stockValues = CType(Session("stockValues"), ArrayList)

        End If

    End Sub

    Protected Sub addStockInformation_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles addStockInformation.Click
        Dim temp As Stock = New Stock()
        Try
            temp.Company = New Company(symbol.Text, name.Text)
            temp.Price = CType(price.Text, Double)
            temp.Volume = CType(volume.Text, Integer)

        Catch ex As Exception

        End Try
        stockValues.Add(temp)
        Session("stockValues") = stockValues
        ConfigureCrystalReports()

    End Sub
End Class
