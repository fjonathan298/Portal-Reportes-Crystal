Option Strict On

Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Partial Class _Default
    Inherits System.Web.UI.Page

    Private hierarchicalGroupingReport As ReportDocument
    Private exportPath As String
    Private myDiskFileDestinationOptions As DiskFileDestinationOptions
    Private myExportOptions As ExportOptions
    Private selectedNoFormat As Boolean = True

    Private Sub ConfigureCrystalReports()
        hierarchicalGroupingReport = New ReportDocument()
        Dim reportPath As String = Server.MapPath("Hierarchical Grouping.rpt")
        hierarchicalGroupingReport.Load(reportPath)
        myCrystalReportViewer.ReportSource = hierarchicalGroupingReport

        If Not IsPostBack Then
            exportTypesList.DataSource = System.Enum.GetValues(GetType(ExportFormatType))
            exportTypesList.DataBind()
        End If

        exportTypesList.DataSource = System.Enum.GetValues(GetType(ExportFormatType))
    End Sub

    Protected Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
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
                ConfigureExportToDoc()
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
        html40FormatOptions.FirstPageNumber = 1
        html40FormatOptions.LastPageNumber = 3
        myExportOptions.FormatOptions = html40FormatOptions
    End Sub

    Protected Sub exportByType_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles exportByType.Click
        ExportSetup()
        ExportSelection()
        ExportCompletion()
    End Sub
End Class