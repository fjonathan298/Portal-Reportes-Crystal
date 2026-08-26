
Partial Class _Default
    Inherits System.Web.UI.Page

    Protected Sub myCrystalReportViewer_Drill(ByVal source As Object, ByVal e As CrystalDecisions.Web.DrillEventArgs) Handles myCrystalReportViewer.Drill
        drillLabel.Text = e.NewGroupName
    End Sub
End Class
