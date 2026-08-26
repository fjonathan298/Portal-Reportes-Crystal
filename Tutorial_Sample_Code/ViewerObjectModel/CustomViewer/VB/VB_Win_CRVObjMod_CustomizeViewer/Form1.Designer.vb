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
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel
        Me.Label1 = New System.Windows.Forms.Label
        Me.listCRVReport = New System.Windows.Forms.ListBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.listCRVToolbar = New System.Windows.Forms.ListBox
        Me.redisplay = New System.Windows.Forms.Button
        Me.Label3 = New System.Windows.Forms.Label
        Me.selectBackColor = New System.Windows.Forms.ComboBox
        Me.pageNumber = New System.Windows.Forms.TextBox
        Me.goToPage = New System.Windows.Forms.Button
        Me.searchText = New System.Windows.Forms.TextBox
        Me.search = New System.Windows.Forms.Button
        Me.message = New System.Windows.Forms.Label
        Me.zoomFactor = New System.Windows.Forms.TextBox
        Me.updateZoomFactor = New System.Windows.Forms.Button
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'myCrystalReportViewer
        '
        Me.myCrystalReportViewer.ActiveViewIndex = -1
        Me.myCrystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.myCrystalReportViewer.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.myCrystalReportViewer.Location = New System.Drawing.Point(0, 174)
        Me.myCrystalReportViewer.Name = "myCrystalReportViewer"
        Me.myCrystalReportViewer.SelectionFormula = ""
        Me.myCrystalReportViewer.Size = New System.Drawing.Size(671, 414)
        Me.myCrystalReportViewer.TabIndex = 0
        Me.myCrystalReportViewer.ViewTimeSelectionFormula = ""
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 4
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.Label1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.listCRVReport, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Label2, 2, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.listCRVToolbar, 3, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.redisplay, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.Label3, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.selectBackColor, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.pageNumber, 0, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.goToPage, 1, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.searchText, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.search, 1, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.message, 2, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.zoomFactor, 2, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.updateZoomFactor, 3, 3)
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 5
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(671, 176)
        Me.TableLayoutPanel1.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(3, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(159, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Select report elements to display"
        '
        'listCRVReport
        '
        Me.listCRVReport.FormattingEnabled = True
        Me.listCRVReport.Location = New System.Drawing.Point(170, 3)
        Me.listCRVReport.Name = "listCRVReport"
        Me.listCRVReport.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listCRVReport.Size = New System.Drawing.Size(120, 56)
        Me.listCRVReport.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(337, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(132, 26)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Select toolbar elements to display"
        '
        'listCRVToolbar
        '
        Me.listCRVToolbar.FormattingEnabled = True
        Me.listCRVToolbar.Location = New System.Drawing.Point(504, 3)
        Me.listCRVToolbar.Name = "listCRVToolbar"
        Me.listCRVToolbar.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
        Me.listCRVToolbar.Size = New System.Drawing.Size(120, 56)
        Me.listCRVToolbar.TabIndex = 3
        '
        'redisplay
        '
        Me.redisplay.Location = New System.Drawing.Point(3, 99)
        Me.redisplay.Name = "redisplay"
        Me.redisplay.Size = New System.Drawing.Size(75, 20)
        Me.redisplay.TabIndex = 4
        Me.redisplay.Text = "Redisplay Report"
        Me.redisplay.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(3, 70)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(123, 13)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Select background color"
        '
        'selectBackColor
        '
        Me.selectBackColor.FormattingEnabled = True
        Me.selectBackColor.Location = New System.Drawing.Point(170, 73)
        Me.selectBackColor.Name = "selectBackColor"
        Me.selectBackColor.Size = New System.Drawing.Size(121, 21)
        Me.selectBackColor.TabIndex = 6
        '
        'pageNumber
        '
        Me.pageNumber.Location = New System.Drawing.Point(3, 125)
        Me.pageNumber.Name = "pageNumber"
        Me.pageNumber.Size = New System.Drawing.Size(100, 20)
        Me.pageNumber.TabIndex = 7
        '
        'goToPage
        '
        Me.goToPage.Location = New System.Drawing.Point(170, 125)
        Me.goToPage.Name = "goToPage"
        Me.goToPage.Size = New System.Drawing.Size(75, 20)
        Me.goToPage.TabIndex = 8
        Me.goToPage.Text = "Go to Page"
        Me.goToPage.UseVisualStyleBackColor = True
        '
        'searchText
        '
        Me.searchText.Location = New System.Drawing.Point(3, 151)
        Me.searchText.Name = "searchText"
        Me.searchText.Size = New System.Drawing.Size(100, 20)
        Me.searchText.TabIndex = 9
        '
        'search
        '
        Me.search.Location = New System.Drawing.Point(170, 151)
        Me.search.Name = "search"
        Me.search.Size = New System.Drawing.Size(107, 22)
        Me.search.TabIndex = 10
        Me.search.Text = "Search For Text"
        Me.search.UseVisualStyleBackColor = True
        '
        'message
        '
        Me.message.AutoSize = True
        Me.message.ForeColor = System.Drawing.Color.Red
        Me.message.Location = New System.Drawing.Point(337, 148)
        Me.message.Name = "message"
        Me.message.Size = New System.Drawing.Size(0, 13)
        Me.message.TabIndex = 11
        '
        'zoomFactor
        '
        Me.zoomFactor.Location = New System.Drawing.Point(337, 125)
        Me.zoomFactor.Name = "zoomFactor"
        Me.zoomFactor.Size = New System.Drawing.Size(100, 20)
        Me.zoomFactor.TabIndex = 12
        '
        'updateZoomFactor
        '
        Me.updateZoomFactor.Location = New System.Drawing.Point(504, 125)
        Me.updateZoomFactor.Name = "updateZoomFactor"
        Me.updateZoomFactor.Size = New System.Drawing.Size(75, 20)
        Me.updateZoomFactor.TabIndex = 13
        Me.updateZoomFactor.Text = "% Zoom"
        Me.updateZoomFactor.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(671, 588)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.myCrystalReportViewer)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents myCrystalReportViewer As CrystalDecisions.Windows.Forms.CrystalReportViewer
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents listCRVReport As System.Windows.Forms.ListBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents listCRVToolbar As System.Windows.Forms.ListBox
    Friend WithEvents redisplay As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents selectBackColor As System.Windows.Forms.ComboBox
    Friend WithEvents pageNumber As System.Windows.Forms.TextBox
    Friend WithEvents goToPage As System.Windows.Forms.Button
    Friend WithEvents searchText As System.Windows.Forms.TextBox
    Friend WithEvents search As System.Windows.Forms.Button
    Friend WithEvents message As System.Windows.Forms.Label
    Friend WithEvents zoomFactor As System.Windows.Forms.TextBox
    Friend WithEvents updateZoomFactor As System.Windows.Forms.Button

End Class
