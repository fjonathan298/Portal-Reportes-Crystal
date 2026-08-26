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
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.orderStartDate = New System.Windows.Forms.TextBox
        Me.orderEndDate = New System.Windows.Forms.TextBox
        Me.SuspendLayout()
        '
        'myCrystalReportViewer
        '
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(12, 125)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.SelectionFormula = ""
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(568, 326)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ViewTimeSelectionFormula = ""
        '
        'defaultParameterValuesList
        '
        Me.defaultParameterValuesList.FormattingEnabled = True
        Me.defaultParameterValuesList.Location = New System.Drawing.Point(12, 12)
        Me.defaultParameterValuesList.Name = "defaultParameterValuesList"
        Me.defaultParameterValuesList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.defaultParameterValuesList.Size = New System.Drawing.Size(120, 95)
        Me.defaultParameterValuesList.TabIndex = 1
        '
        'redisplay
        '
        Me.redisplay.Location = New System.Drawing.Point(161, 84)
        Me.redisplay.Name = "redisplay"
        Me.redisplay.Size = New System.Drawing.Size(120, 23)
        Me.redisplay.TabIndex = 2
        Me.redisplay.Text = "Redisplay Report"
        Me.redisplay.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(158, 23)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(84, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Order Start Date"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(158, 54)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(81, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Order End Date"
        '
        'orderStartDate
        '
        Me.orderStartDate.Location = New System.Drawing.Point(259, 20)
        Me.orderStartDate.Name = "orderStartDate"
        Me.orderStartDate.Size = New System.Drawing.Size(100, 20)
        Me.orderStartDate.TabIndex = 5
        '
        'orderEndDate
        '
        Me.orderEndDate.Location = New System.Drawing.Point(259, 54)
        Me.orderEndDate.Name = "orderEndDate"
        Me.orderEndDate.Size = New System.Drawing.Size(100, 20)
        Me.orderEndDate.TabIndex = 6
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(592, 473)
        Me.Controls.Add(Me.orderEndDate)
        Me.Controls.Add(Me.orderStartDate)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.redisplay)
        Me.Controls.Add(Me.defaultParameterValuesList)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
    Friend WithEvents defaultParameterValuesList As System.Windows.Forms.ListBox
    Friend WithEvents redisplay As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents orderStartDate As System.Windows.Forms.TextBox
    Friend WithEvents orderEndDate As System.Windows.Forms.TextBox

End Class
