Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1

    Private Sub ConfigureCrystalReports()
        selectOperatorList.DataSource = System.Enum.GetValues(GetType(CeComparisonOperator))

        Dim mySelectFormula As String = "{Customer.Last Year's Sales} > 11000.00 " _
        & "AND Mid({Customer.Customer Name}, 1, 1) = ""A"" "
        myCrystalReportViewer.SelectionFormula = mySelectFormula

        Dim reportPath As String = Application.StartupPath & "\" & "CustomersBySalesName.rpt"
        myCrystalReportViewer.ReportSource = reportPath


    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Private Sub redisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles redisplay.Click
        Dim mySelectedOperator As String = GetSelectedCompareOperator()

        Dim mySelectFormula As String = "{Customer.Last Year's Sales} > " & lastYearsSales.Text _
& " AND Mid({Customer.Customer Name}, 1, 1) " & mySelectedOperator & " """ & customerName.Text & """"
        myCrystalReportViewer.SelectionFormula = mySelectFormula
        myCrystalReportViewer.ReportSource = Application.StartupPath & "\" & "CustomersBySalesName.rpt"
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
