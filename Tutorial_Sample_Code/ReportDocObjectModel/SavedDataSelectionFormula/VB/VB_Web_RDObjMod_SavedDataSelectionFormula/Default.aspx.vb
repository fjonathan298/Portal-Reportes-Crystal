Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Partial Class _Default
    Inherits System.Web.UI.Page
    Private customerBySalesNameReport As ReportDocument
    Private salesAmount As String
    Private operatorValue As String
    Private customerName As String

    Private Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
        customerBySalesNameReport.Close()
        myCrystalReportViewer.Dispose()
    End Sub

    Private Sub ConfigureCrystalReports()
        salesAmount = "4000"
        operatorValue = "<"
        customerName = "K"

        Dim selectionFormula As String = "{Customer.Last Year's Sales} > " & salesAmount _
        & " AND Mid({Customer.Customer Name}, 1, 1) " & operatorValue & "'" & customerName & "'"
        operatorValueList.DataSource = System.Enum.GetValues(GetType(CeComparisonOperator))
        operatorValueList.DataBind()
        customerBySalesNameReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("CustomerBySalesName.rpt")
        customerBySalesNameReport.Load(reportPath)
        customerBySalesNameReport.DataDefinition.RecordSelectionFormula = selectionFormula
        myCrystalReportViewer.ReportSource = customerBySalesNameReport
        formula.Text = selectionFormula
    End Sub

    Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()
    End Sub

    Protected Sub redisplay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles redisplay.Click
        salesAmount = lastYearsSales.Text
        operatorValue = GetSelectedOperator()
        customerName = letterOfName.Text

        Dim selectionFormula As String = "{Customer.Last Year's Sales} > " & salesAmount _
        & " AND Mid({Customer.Customer Name}, 1, 1) " & operatorValue & "'" & customerName & "'"
        customerBySalesNameReport.DataDefinition.SavedDataSelectionFormula = selectionFormula
        myCrystalReportViewer.ReportSource = customerBySalesNameReport
        formula.Text = customerBySalesNameReport.DataDefinition.SavedDataSelectionFormula
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
