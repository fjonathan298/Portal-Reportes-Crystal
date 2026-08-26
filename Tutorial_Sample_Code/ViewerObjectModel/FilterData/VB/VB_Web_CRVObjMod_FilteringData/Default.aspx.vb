Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page

    Private Sub ConfigureCrystalReports()
        If Not IsPostBack Then
            selectOperatorList.DataSource = System.Enum.GetValues(GetType(CeComparisonOperator))

            Dim mySelectFormula As String = "{Customer.Last Year's Sales} > 11000.00 " _
            & "AND Mid({Customer.Customer Name}, 1, 1) = ""A"" "
            myCrystalReportViewer.SelectionFormula = mySelectFormula
            selectOperatorList.DataBind()
        End If

        Dim reportPath As String = Server.MapPath("CustomersBySalesName.rpt")
        myCrystalReportViewer.ReportSource = reportPath

    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()
    End Sub

    Protected Sub redisplay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles redisplay.Click
        Dim mySelectedOperator As String = GetSelectedCompareOperator()

        Dim mySelectFormula As String = "{Customer.Last Year's Sales} > " & lastYearsSales.Text _
        & " AND Mid({Customer.Customer Name}, 1, 1) " & mySelectedOperator & " """ & customerName.Text & """"
        myCrystalReportViewer.SelectionFormula = mySelectFormula
    End Sub

    Private Function GetSelectedCompareOperator() As String
        Select Case selectOperatorList.SelectedIndex
            Case CeComparisonOperator.EqualTo
                Return "="
            Case CeComparisonOperator.LessThan
                Return "<"
            Case CeComparisonOperator.GreaterThan
                Return ">"
            Case CeComparisonOperator.LessThan_or_EqualTo
                Return "<="
            Case CeComparisonOperator.GreaterThan_or_EqualTo
                Return ">="
            Case CeComparisonOperator.Not_EqualTo
                Return "<>"
            Case Else
                Return "="
        End Select
    End Function

End Class
