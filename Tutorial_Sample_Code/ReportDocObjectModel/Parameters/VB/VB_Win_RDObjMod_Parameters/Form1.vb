Imports System.Collections
Imports System.Web.UI.WebControls
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1
    Private customersByCityReport As ReportDocument
    Private Const PARAMETER_FIELD_NAME As String = "City"


    Private Sub ConfigureCrystalReports()
        customersByCityReport = New ReportDocument()
        Dim reportPath As String = Application.StartupPath & "\" & "CustomersByCity.rpt"
        customersByCityReport.Load(reportPath)

        Dim myArrayList As ArrayList = New ArrayList()
        myArrayList.Add("Paris")
        myArrayList.Add("Tokyo")

        defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(customersByCityReport)

        SetCurrentValuesForParameterField(customersByCityReport, myArrayList)
        myCrystalReportViewer.ReportSource = customersByCityReport
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Private Sub SetCurrentValuesForParameterField(ByVal myReportDocument As ReportDocument, ByVal myArrayList As ArrayList)
        Dim currentParameterValues As ParameterValues = New ParameterValues()
        For Each submittedValue As Object In myArrayList
            Dim myParameterDiscreteValue As ParameterDiscreteValue = New ParameterDiscreteValue()
            myParameterDiscreteValue.Value = submittedValue.ToString()
            currentParameterValues.Add(myParameterDiscreteValue)
        Next

        Dim myParameterFieldDefinitions As ParameterFieldDefinitions = myReportDocument.DataDefinition.ParameterFields
        Dim myParameterFieldDefinition As ParameterFieldDefinition = myParameterFieldDefinitions(PARAMETER_FIELD_NAME)
        myParameterFieldDefinition.ApplyCurrentValues(currentParameterValues)
    End Sub

    Private Function GetDefaultValuesFromParameterField(ByVal myReportDocument As ReportDocument) As ArrayList
        Dim myParameterFieldDefinitions As ParameterFieldDefinitions = myReportDocument.DataDefinition.ParameterFields
        Dim myParameterFieldDefinition As ParameterFieldDefinition = myParameterFieldDefinitions(PARAMETER_FIELD_NAME)
        Dim defaultParameterValues As ParameterValues = myParameterFieldDefinition.DefaultValues
        Dim myArrayList As ArrayList = New ArrayList()

        For Each myParameterValue As ParameterValue In defaultParameterValues
            If (Not myParameterValue.IsRange) Then
                Dim myParameterDiscreteValue As ParameterDiscreteValue = CType(myParameterValue, ParameterDiscreteValue)
                myArrayList.Add(myParameterDiscreteValue.Value.ToString())
            End If
        Next

        Return myArrayList
    End Function


    Private Sub redisplay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles redisplay.Click
        Dim myArrayList As ArrayList = New ArrayList()
        For Each item As String In defaultParameterValuesList.SelectedItems
            myArrayList.Add(item)
        Next

        SetCurrentValuesForParameterField(customersByCityReport, myArrayList)
        myCrystalReportViewer.ReportSource = customersByCityReport
    End Sub
End Class
