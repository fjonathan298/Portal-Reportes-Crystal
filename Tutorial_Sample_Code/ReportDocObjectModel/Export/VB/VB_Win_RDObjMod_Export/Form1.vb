Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class Form1

    Private hierarchicalGroupingReport As Hierarchical_Grouping
    Private exportPath As String
    Private myDiskFileDestinationOptions As DiskFileDestinationOptions
    Private myExportOptions As ExportOptions
    Private selectedNoFormat As Boolean = True

    Private Sub ConfigureCrystalReports()
        hierarchicalGroupingReport = New Hierarchical_Grouping()
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport

        exportTypesList.DataSource = System.Enum.GetValues(GetType(ExportFormatType))
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ConfigureCrystalReports()
    End Sub

    Public Sub ExportSetup()
        exportPath = "C:\Exported\"

        If Not System.IO.Directory.Exists(exportPath) Then
            System.IO.Directory.CreateDirectory(exportPath)
        End If

        myDiskFileDestinationOptions = New DiskFileDestinationOptions()
        myExportOptions = hierarchicalGroupingReport.ExportOptions
        myExportOptions.ExportDestinationType = ExportDestinationType.DiskFile
        myExportOptions.FormatOptions = Nothing
    End Sub

    Public Sub ExportSelection()
        Select Case exportTypesList.SelectedIndex

            Case ExportFormatType.NoFormat
                selectedNoFormat = True
            Case ExportFormatType.CrystalReport
                ConfigureExportToRpt()
            Case ExportFormatType.RichText
                ConfigureExportToRtf()
            Case ExportFormatType.WordForWindows
                selectedNoFormat = True
            Case ExportFormatType.Excel
                ConfigureExportToXls()
            Case ExportFormatType.PortableDocFormat
                ConfigureExportToPdf()
            Case ExportFormatType.HTML32
                ConfigureExportToHtml32()
            Case ExportFormatType.HTML40
                ConfigureExportToHtml40()
        End Select
    End Sub

    Public Sub ExportCompletion()
        Try
            If selectedNoFormat Then
                message.Text = MessageConstants.FORMAT_NOT_SUPPORTED
            Else
                hierarchicalGroupingReport.Export()
                message.Text = MessageConstants.SUCCESS
            End If
        Catch ex As Exception
            message.Text = MessageConstants.FAILURE & ex.Message
        End Try

        message.Visible = True
        selectedNoFormat = False
    End Sub

    Public Sub ConfigureExportToRpt()
        myExportOptions.ExportFormatType = ExportFormatType.CrystalReport
        myDiskFileDestinationOptions.DiskFileName = exportPath & "Report.rpt"
        myExportOptions.DestinationOptions = myDiskFileDestinationOptions
    End Sub

    Public Sub ConfigureExportToRtf()
        myExportOptions.ExportFormatType = ExportFormatType.RichText
        myDiskFileDestinationOptions.DiskFileName = exportPath & "RichTextFormat.rtf"
        myExportOptions.DestinationOptions = myDiskFileDestinationOptions
    End Sub

    Public Sub ConfigureExportToDoc()
        myExportOptions.ExportFormatType = ExportFormatType.WordForWindows
        myDiskFileDestinationOptions.DiskFileName = exportPath & "Word.doc"
        myExportOptions.DestinationOptions = myDiskFileDestinationOptions
    End Sub

    Public Sub ConfigureExportToXls()
        myExportOptions.ExportFormatType = ExportFormatType.Excel
        myDiskFileDestinationOptions.DiskFileName = exportPath & "Excel.xls"
        myExportOptions.DestinationOptions = myDiskFileDestinationOptions
    End Sub

    Public Sub ConfigureExportToPdf()
        myExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat
        myDiskFileDestinationOptions.DiskFileName = exportPath & "PortableDoc.pdf"
        myExportOptions.DestinationOptions = myDiskFileDestinationOptions
    End Sub

    Public Sub ConfigureExportToHtml32()
        myExportOptions.ExportFormatType = ExportFormatType.HTML32
        Dim html32FormatOptions As HTMLFormatOptions = New HTMLFormatOptions()
        html32FormatOptions.HTMLBaseFolderName = exportPath & "Html32Folder"
        html32FormatOptions.HTMLFileName = "html32.html"
        html32FormatOptions.HTMLEnableSeparatedPages = False
        html32FormatOptions.HTMLHasPageNavigator = False
        myExportOptions.FormatOptions = html32FormatOptions
    End Sub

    Public Sub ConfigureExportToHtml40()
        myExportOptions.ExportFormatType = ExportFormatType.HTML40
        Dim html40FormatOptions As HTMLFormatOptions = New HTMLFormatOptions()
        html40FormatOptions.HTMLBaseFolderName = exportPath & "Html40Folder"
        html40FormatOptions.HTMLFileName = "html40.html"
        html40FormatOptions.HTMLEnableSeparatedPages = True
        html40FormatOptions.HTMLHasPageNavigator = True
        html40FormatOptions.FirstPageNumber = 1
        html40FormatOptions.LastPageNumber = 3
        myExportOptions.FormatOptions = html40FormatOptions
    End Sub

    Private Sub exportByType_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles exportByType.Click
        ExportSetup()
        ExportSelection()
        ExportCompletion()
    End Sub

End Class
