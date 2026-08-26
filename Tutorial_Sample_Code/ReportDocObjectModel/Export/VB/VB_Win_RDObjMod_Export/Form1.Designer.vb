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
        Me.exportTypesList = New System.Windows.Forms.ComboBox
        Me.exportByType = New System.Windows.Forms.Button
        Me.message = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'myCrystalReportViewer
        '
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(12, 40)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.SelectionFormula = ""
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(776, 214)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ViewTimeSelectionFormula = ""
        '
        'exportTypesList
        '
        Me.exportTypesList.FormattingEnabled = True
        Me.exportTypesList.Location = New System.Drawing.Point(12, 13)
        Me.exportTypesList.Name = "exportTypesList"
        Me.exportTypesList.Size = New System.Drawing.Size(121, 21)
        Me.exportTypesList.TabIndex = 1
        '
        'exportByType
        '
        Me.exportByType.Location = New System.Drawing.Point(150, 13)
        Me.exportByType.Name = "exportByType"
        Me.exportByType.Size = New System.Drawing.Size(178, 23)
        Me.exportByType.TabIndex = 2
        Me.exportByType.Text = "Export As Selected Type."
        Me.exportByType.UseVisualStyleBackColor = True
        '
        'message
        '
        Me.message.AutoSize = True
        Me.message.Location = New System.Drawing.Point(232, 22)
        Me.message.Name = "message"
        Me.message.Size = New System.Drawing.Size(0, 13)
        Me.message.TabIndex = 3
        Me.message.Visible = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(352, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(49, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "message"
        Me.Label1.Visible = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 266)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.message)
        Me.Controls.Add(Me.exportByType)
        Me.Controls.Add(Me.exportTypesList)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
    Friend WithEvents exportTypesList As System.Windows.Forms.ComboBox
    Friend WithEvents exportByType As System.Windows.Forms.Button
    Friend WithEvents message As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label

End Class
