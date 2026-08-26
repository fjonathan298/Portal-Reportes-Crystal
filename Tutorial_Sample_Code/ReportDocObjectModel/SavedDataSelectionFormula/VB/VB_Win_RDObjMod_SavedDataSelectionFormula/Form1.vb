Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Public Class Form1
    Private customerBySalesNameReport As CustomerBySalesName
    Private salesAmount As String
    Private operatorValue As String
    Private customerName As String
    Private useDefaultValues As Boolean = True

    Private Sub Form1_Unload(ByVal sender As System.Object, ByVal e As System.EventArgs)
        customerBySalesNameReport.Close()
        myCrystalReportViewer.Dispose()
    End Sub

    Private Sub ConfigureCrystalReports()
        If useDefaultValues Then
            salesAmount = "4000"
            operatorValue = "<"
            customerName = "K"
            operatorValueList.DataSource = System.Enum.GetValues(GetType(CeComparisonOperator))
        End If

        Dim selectionFormula As String = "{Customer.Last Year's Sales} > " & salesAmount _
        & " AND Mid({Customer.Customer Name}, 1, 1) " & operatorValue & "'" & customerName & "'"
        customerBySalesNameReport = New CustomerBySalesName()
        customerBySalesNameReport.DataDefinition.RecordSelectionFormula = selectionFormula
        myCrystalReportViewer.ReportSource = customerBySalesNameReport
        formula.Text = selectionFormula
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Private Sub redisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles redisplay.Click
        salesAmount = lastYearsSales.Text
        operatorValue = GetSelectedOperator()
        customerName = letterOfName.Text
        useDefaultValues = False

        Dim selectionFormula As String = "{Customer.Last Year's Sales} > " & salesAmount _
        & " AND Mid({Customer.Customer Name}, 1, 1) " & operatorValue & "'" & customerName & "'"
        customerBySalesNameReport.DataDefinition.SavedDataSelectionFormula = selectionFormula
        myCrystalReportViewer.ReportSource = customerBySalesNameReport
        formula.Text = selectionFormula
    End Sub

    Private Function GetSelectedOperator() As String
        Dim selectedOperator As String = ""

        Select Case operatorValueList.SelectedIndex
            Case CeComparisonOperator.EqualTo
                selectedOperator = "="
            Case CeComparisonOperator.GreaterThan
                selectedOperator = ">"
            Case CeComparisonOperator.GreaterThanOrEqualTo
                selectedOperator = ">="
            Case CeComparisonOperator.LessThan
                selectedOperator = "<"
            Case CeComparisonOperator.LessThanOrEqualTo
                selectedOperator = "<="
            Case CeComparisonOperator.NotEqualTo
                selectedOperator = "<>"
        End Select

        Return selectedOperator
    End Function
End Class
