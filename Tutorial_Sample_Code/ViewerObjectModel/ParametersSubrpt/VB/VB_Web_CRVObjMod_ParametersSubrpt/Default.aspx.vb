Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports System.Collections
Imports System.Web.UI.WebControls



Partial Class _Default
    Inherits System.Web.UI.Page
    Private Const PARAMETER_FIELD_NAME As String = "City"
    Private Const SUBREPORT_PARAMETER_FIELD_NAME As String = "OrderDateRange"
    Private Const SUBREPORT_NAME As String = "CustomerOrders"

    Private Sub ConfigureCrystalReports()
        Dim myArrayList As ArrayList = New ArrayList()
        Dim startDate As String
        Dim endDate As String

        Dim reportPath As String = Server.MapPath("CustomersByCity.rpt")
        myCrystalReportViewer.ReportSource = reportPath
        Dim myParameterFields As ParameterFields = myCrystalReportViewer.ParameterFieldInfo
        If Not IsPostBack Then
            defaultParameterValuesList.DataSource = GetDefaultValuesFromParameterField(myParameterFields)
            defaultParameterValuesList.DataBind()
            myArrayList.Add("Paris")
            myArrayList.Add("Tokyo")
            Session("myArrayList") = myArrayList

            startDate = "8/1/1997"
            endDate = "8/31/1997"
            Session("startDate") = startDate
            Session("endDate") = endDate

        Else
            myArrayList = CType(Session("myArrayList"), ArrayList)
            startDate = Session("startDate").ToString()
            endDate = Session("endDate").ToString()
        End If
        SetCurrentValuesForParameterField(myParameterFields, myArrayList)
        SetDateRangeForOrders(myParameterFields, startDate, endDate)
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


    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
        ConfigureCrystalReports()

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


    Protected Sub redisplay_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles redisplay.Click
        Dim myArrayList As ArrayList = New ArrayList()
        For Each item As ListItem In defaultParameterValuesList.Items
            If item.Selected Then
                myArrayList.Add(item.Value)
            End If
        Next
        myCrystalReportViewer.ReportSource = Server.MapPath("CustomersByCity.rpt")
        Dim myParameterFields As ParameterFields = myCrystalReportViewer.ParameterFieldInfo
        SetCurrentValuesForParameterField(myParameterFields, myArrayList)
        Session("myArrayList") = myArrayList
        Session("startDate") = orderStartDate.Text
        Session("endDate") = orderEndDate.Text
        ConfigureCrystalReports()
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
