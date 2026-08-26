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
        Me.Label1 = New System.Windows.Forms.Label
        Me.lastYearsSales = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.selectOperatorList = New System.Windows.Forms.ComboBox
        Me.customerName = New System.Windows.Forms.TextBox
        Me.redisplay = New System.Windows.Forms.Button
        Me.SuspendLayout()
        
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(0, 73)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.SelectionFormula = ""
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(396, 200)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ViewTimeSelectionFormula = ""
        
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 5)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(225, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Enter the minimum value for last year's sales: $"
        
        Me.lastYearsSales.Location = New System.Drawing.Point(234, 2)
        Me.lastYearsSales.Name = "lastYearsSales"
        Me.lastYearsSales.Size = New System.Drawing.Size(100, 20)
        Me.lastYearsSales.TabIndex = 2
        Me.lastYearsSales.Text = "11000.00"
        
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(15, 25)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(139, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Display the customer names"
        
        Me.selectOperatorList.FormattingEnabled = True
        Me.selectOperatorList.Location = New System.Drawing.Point(161, 25)
        Me.selectOperatorList.Name = "selectOperatorList"
        Me.selectOperatorList.Size = New System.Drawing.Size(121, 21)
        Me.selectOperatorList.TabIndex = 4
        
        Me.customerName.Location = New System.Drawing.Point(288, 26)
        Me.customerName.Name = "customerName"
        Me.customerName.Size = New System.Drawing.Size(100, 20)
        Me.customerName.TabIndex = 5
        Me.customerName.Text = "A"
        
        Me.redisplay.Location = New System.Drawing.Point(18, 44)
        Me.redisplay.Name = "redisplay"
        Me.redisplay.Size = New System.Drawing.Size(108, 23)
        Me.redisplay.TabIndex = 6
        Me.redisplay.Text = "Redisplay Report"
        Me.redisplay.UseVisualStyleBackColor = True
        
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(396, 273)
        Me.Controls.Add(Me.redisplay)
        Me.Controls.Add(Me.customerName)
        Me.Controls.Add(Me.selectOperatorList)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lastYearsSales)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lastYearsSales As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents selectOperatorList As System.Windows.Forms.ComboBox
    Friend WithEvents customerName As System.Windows.Forms.TextBox
    Friend WithEvents redisplay As System.Windows.Forms.Button

End Class
