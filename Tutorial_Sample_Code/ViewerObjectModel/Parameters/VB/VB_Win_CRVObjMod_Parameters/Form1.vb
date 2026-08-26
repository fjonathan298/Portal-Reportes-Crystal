Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Collections


Public Class Form1

    Private Const PARAMETER_FIELD_NAME As String = "City"

    Private Sub ConfigureCrystalReports()
        Dim myArrayList As ArrayList = New ArrayList()
        myArrayList.Add("Paris")
        myArrayList.Add("Tokyo")
        Dim reportPath As String = Application.StartupPath & "\" & "CustomersByCity.rpt"
        myCrystalReportViewer.ReportSource = reportPath
        Dim myParameterFields As ParameterFields = myCrystalReportViewer.ParameterFieldInfo
        SetCurrentValuesForParameterField(myParameterFields, myArrayList)
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
        myCrystalReportViewer.ReportSource = Application.StartupPath & "\" & "CustomersByCity.rpt"
        Dim myParameterFields As ParameterFields = myCrystalReportViewer.ParameterFieldInfo
        SetCurrentValuesForParameterField(myParameterFields, myArrayList)
    End Sub
End Class
