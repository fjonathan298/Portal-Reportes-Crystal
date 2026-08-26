Public Class Form1

    Private Sub myCrystalReportViewer_Drill(ByVal source As System.Object, ByVal e As CrystalDecisions.Windows.Forms.DrillEventArgs) Handles myCrystalReportViewer.Drill
        drillLabel.Text = e.NewGroupName
    End Sub
End Class
