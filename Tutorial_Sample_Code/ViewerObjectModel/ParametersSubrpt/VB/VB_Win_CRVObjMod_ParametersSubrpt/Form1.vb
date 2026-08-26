Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Collections


Public Class Form1

    Private Const PARAMETER_FIELD_NAME As String = "City"
    Private Const SUBREPORT_PARAMETER_FIELD_NAME As String = "OrderDateRange"
    Private Const SUBREPORT_NAME As String = "CustomerOrders"


    Private Sub ConfigureCrystalReports()
        Dim myArrayList As ArrayList = New ArrayList()
        myArrayList.Add("Paris")
        myArrayList.Add("Tokyo")

        Dim startDate As String = "8/1/1997"
        Dim endDate As String = "8/31/1997"

        Dim reportPath As String = Application.StartupPath & "\" & "CustomersByCity.rpt"
        myCrystalReportViewer.ReportSource = reportPath
        Dim myParameterFields As ParameterFields = myCrystalReportViewer.ParameterFieldInfo
        SetCurrentValuesForParameterField(myParameterFields, myArrayList)
        SetDateRangeForOrders(myParameterFields, startDate, endDate)
        defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(myParameterFields)

    End Sub

    Private Function GetDefaultValuesFromParameterField(ByVal myParameterFields As ParameterFields) As ArrayList
        Dim myParameterField As ParameterField = myParameterFields(PARAMETER_FIELD_NAME)
        Dim defaultParameterValues As ParameterValues = myParameterField.DefaultValues
        Dim myArrayList As ArrayList = New ArrayList()
        For Each myParameterValue As ParameterValue In defaultParameterValues
            If (Not myParameterValue.IsRange) Then
                Dim myParameterDiscreteValue As ParameterDiscreteValue = CType(myParameterValue, ParameterDiscreteValue)
                myArrayList.Add(myParameterDiscreteValue.Value.ToString())
            End If
        Next
        Return myArrayList
    End Function

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Private Sub SetCurrentValuesForParameterField(ByVal myParameterFields As ParameterFields, ByVal myArrayList As ArrayList)
        Dim currentParameterValues As ParameterValues = New ParameterValues()
        For Each submittedValue As Object In myArrayList
            Dim myParameterDiscreteValue As ParameterDiscreteValue = New ParameterDiscreteValue()
            myParameterDiscreteValue.Value = submittedValue.ToString()
            currentParameterValues.Add(myParameterDiscreteValue)
        Next
        Dim myParameterField As ParameterField = myParameterFields(PARAMETER_FIELD_NAME)
        myParameterField.CurrentValues = currentParameterValues
    End Sub

    Private Sub redisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles redisplay.Click
        Dim myArrayList As ArrayList = New ArrayList()
        For Each item As String In defaultParameterValuesList.SelectedItems
            myArrayList.Add(item)
        Next

        Dim startDate As String = orderStartDate.Text
        Dim endDate As String = orderEndDate.Text

        myCrystalReportViewer.ReportSource = Application.StartupPath & "\" & "CustomersByCity.rpt"
        Dim myParameterFields As ParameterFields = myCrystalReportViewer.ParameterFieldInfo
        SetCurrentValuesForParameterField(myParameterFields, myArrayList)
        SetDateRangeForOrders(myParameterFields, startDate, endDate)
    End Sub

    Private Sub SetDateRangeForOrders(ByVal myParameterFields As ParameterFields, ByVal startDate As String, ByVal endDate As String)
        Dim myParameterRangeValue As ParameterRangeValue = New ParameterRangeValue()
        myParameterRangeValue.StartValue = startDate
        myParameterRangeValue.EndValue = endDate
        myParameterRangeValue.LowerBoundType = RangeBoundType.BoundInclusive
        myParameterRangeValue.UpperBoundType = RangeBoundType.BoundInclusive
        Dim myParameterField As ParameterField = myParameterFields(SUBREPORT_PARAMETER_FIELD_NAME, SUBREPORT_NAME)
        myParameterField.CurrentValues.Clear()
        myParameterField.CurrentValues.Add(myParameterRangeValue)
    End Sub

End Class
