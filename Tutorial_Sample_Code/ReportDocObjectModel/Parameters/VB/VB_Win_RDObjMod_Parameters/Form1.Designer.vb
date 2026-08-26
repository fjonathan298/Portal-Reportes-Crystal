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
        Me.defaultParameterValuesList = New System.Windows.Forms.ListBox
        Me.redisplay = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'myCrystalReportViewer
        '
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(0, 120)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.SelectionFormula = ""
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(590, 352)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ViewTimeSelectionFormula = ""
        '
        'defaultParameterValuesList
        '
        Me.defaultParameterValuesList.FormattingEnabled = True
        Me.defaultParameterValuesList.Location = New System.Drawing.Point(12, 12)
        Me.defaultParameterValuesList.Name = "defaultParameterValuesList"
        Me.defaultParameterValuesList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.defaultParameterValuesList.Size = New System.Drawing.Size(210, 95)
        Me.defaultParameterValuesList.TabIndex = 1
        '
        'redisplay
        '
        Me.redisplay.Location = New System.Drawing.Point(252, 46)
        Me.redisplay.Name = "redisplay"
        Me.redisplay.Size = New System.Drawing.Size(120, 23)
        Me.redisplay.TabIndex = 2
        Me.redisplay.Text = "Redisplay Report"
        Me.redisplay.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(592, 473)
        Me.Controls.Add(Me.redisplay)
        Me.Controls.Add(Me.defaultParameterValuesList)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
    Friend WithEvents defaultParameterValuesList As System.Windows.Forms.ListBox
    Friend WithEvents redisplay As System.Windows.Forms.Button

End Class
