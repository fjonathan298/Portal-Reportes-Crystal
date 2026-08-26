<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.myCrystalReportViewer = New CrystalDecisions.Windows.Forms.CrystalReportViewer()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lastYearsSales = New System.Windows.Forms.TextBox()
        Me.operatorValueList = New System.Windows.Forms.ComboBox()
        Me.letterOfName = New System.Windows.Forms.TextBox()
        Me.redisplay = New System.Windows.Forms.Button()
        Me.formula = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'myCrystalReportViewer
        '
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Cursor = System.Windows.Forms.Cursors.Default
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(0, 91)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(551, 231)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(-3, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(157, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Display the following customers:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(-3, 20)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(104, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "- last year's sales > $"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(-5, 40)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(106, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "- first letter of name is"
        '
        'lastYearsSales
        '
        Me.lastYearsSales.Location = New System.Drawing.Point(108, 13)
        Me.lastYearsSales.Name = "lastYearsSales"
        Me.lastYearsSales.Size = New System.Drawing.Size(100, 20)
        Me.lastYearsSales.TabIndex = 4
        '
        'operatorValueList
        '
        Me.operatorValueList.FormattingEnabled = True
        Me.operatorValueList.Location = New System.Drawing.Point(107, 37)
        Me.operatorValueList.Name = "operatorValueList"
        Me.operatorValueList.Size = New System.Drawing.Size(121, 21)
        Me.operatorValueList.TabIndex = 5
        '
        'letterOfName
        '
        Me.letterOfName.Location = New System.Drawing.Point(226, 37)
        Me.letterOfName.Name = "letterOfName"
        Me.letterOfName.Size = New System.Drawing.Size(100, 20)
        Me.letterOfName.TabIndex = 6
        '
        'redisplay
        '
        Me.redisplay.Location = New System.Drawing.Point(214, 10)
        Me.redisplay.Name = "redisplay"
        Me.redisplay.Size = New System.Drawing.Size(123, 23)
        Me.redisplay.TabIndex = 7
        Me.redisplay.Text = "Redisplay Report"
        Me.redisplay.UseVisualStyleBackColor = True
        '
        'formula
        '
        Me.formula.AutoSize = True
        Me.formula.Location = New System.Drawing.Point(12, 64)
        Me.formula.Name = "formula"
        Me.formula.Size = New System.Drawing.Size(0, 13)
        Me.formula.TabIndex = 8
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(551, 344)
        Me.Controls.Add(Me.formula)
        Me.Controls.Add(Me.redisplay)
        Me.Controls.Add(Me.letterOfName)
        Me.Controls.Add(Me.operatorValueList)
        Me.Controls.Add(Me.lastYearsSales)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lastYearsSales As System.Windows.Forms.TextBox
    Friend WithEvents operatorValueList As System.Windows.Forms.ComboBox
    Friend WithEvents letterOfName As System.Windows.Forms.TextBox
    Friend WithEvents redisplay As System.Windows.Forms.Button
    Friend WithEvents formula As System.Windows.Forms.Label

End Class
