<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class Form1
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.myCrystalReportViewer = New CrystalDecisions.Windows.Forms.CrystalReportViewer
        Me.SuspendLayout()
        
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(0, 0)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.SelectionFormula = ""
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(292, 273)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ViewTimeSelectionFormula = ""
        
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer

End Class
