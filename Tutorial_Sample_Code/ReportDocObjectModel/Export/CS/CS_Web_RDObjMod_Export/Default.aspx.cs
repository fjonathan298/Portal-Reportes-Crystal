using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;


public partial class _Default : System.Web.UI.Page 
{
    private ReportDocument hierarchicalGroupingReport;
    private string exportPath;
    private DiskFileDestinationOptions diskFileDestinationOptions;
    private ExportOptions exportOptions;
    private bool selectedNoFormat = true;

    private void ConfigureCrystalReports()
    {
        hierarchicalGroupingReport = new ReportDocument();
        string reportPath = Server.MapPath("Hierarchical Grouping.rpt");
        hierarchicalGroupingReport.Load(reportPath);
        crystalReportViewer.ReportSource = hierarchicalGroupingReport;

        if (!IsPostBack)
        {
            exportTypesList.DataSource = System.Enum.GetValues(typeof(ExportFormatType));
            exportTypesList.DataBind();
        }
    }

    private void Page_Init(object sender, EventArgs e)
    {
        ConfigureCrystalReports();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    private void ExportSetup()
    {
        exportPath = "C:\\Exported\\";

        if (!System.IO.Directory.Exists(exportPath))
        {
            System.IO.Directory.CreateDirectory(exportPath);
        }

        diskFileDestinationOptions = new DiskFileDestinationOptions();
        exportOptions = hierarchicalGroupingReport.ExportOptions;
        exportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
        exportOptions.FormatOptions = null;
    }

    private void ExportSelection()
    {
        switch ((ExportFormatType)exportTypesList.SelectedIndex)
        {
            case ExportFormatType.NoFormat:
                selectedNoFormat = true;
                break;
            case ExportFormatType.CrystalReport:
                ConfigureExportToRpt();
                break;
            case ExportFormatType.RichText:
                ConfigureExportToRtf();
                break;
            case ExportFormatType.WordForWindows:
                ConfigureExportToDoc();
                break;
            case ExportFormatType.Excel:
                ConfigureExportToXls();
                break;
            case ExportFormatType.PortableDocFormat:
                ConfigureExportToPdf();
                break;
            case ExportFormatType.HTML32:
                ConfigureExportToHtml32();
                break;
            case ExportFormatType.HTML40:
                ConfigureExportToHtml40();
                break;
        }
    }

    private void ExportCompletion()
    {
        try
        {
            if (selectedNoFormat)
            {
                message.Text = MessageConstants.FORMAT_NOT_SUPPORTED;
            }
            else
            {
                hierarchicalGroupingReport.Export();
                message.Text = MessageConstants.SUCCESS;
            }
        }
        catch (Exception ex)
        {
            message.Text = MessageConstants.FAILURE + ex.Message;
        }

        message.Visible = true;
    }

    private void ConfigureExportToRpt()
    {
        exportOptions.ExportFormatType = ExportFormatType.CrystalReport;
        diskFileDestinationOptions.DiskFileName = exportPath + "Report.rpt";
        exportOptions.DestinationOptions = diskFileDestinationOptions;
    }

    private void ConfigureExportToRtf()
    {
        exportOptions.ExportFormatType = ExportFormatType.RichText;
        diskFileDestinationOptions.DiskFileName = exportPath + "RichTextFormat.rtf";
        exportOptions.DestinationOptions = diskFileDestinationOptions;
    }

    private void ConfigureExportToDoc()
    {
        exportOptions.ExportFormatType = ExportFormatType.WordForWindows;
        diskFileDestinationOptions.DiskFileName = exportPath + "Word.doc";
        exportOptions.DestinationOptions = diskFileDestinationOptions;
    }

    private void ConfigureExportToXls()
    {
        exportOptions.ExportFormatType = ExportFormatType.Excel;
        diskFileDestinationOptions.DiskFileName = exportPath + "Excel.xls";
        exportOptions.DestinationOptions = diskFileDestinationOptions;
    }

    private void ConfigureExportToPdf()
    {
        exportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
        diskFileDestinationOptions.DiskFileName = exportPath + "PortableDoc.pdf";
        exportOptions.DestinationOptions = diskFileDestinationOptions;
    }

    private void ConfigureExportToHtml32()
    {
        exportOptions.ExportFormatType = ExportFormatType.HTML32;
        HTMLFormatOptions html32FormatOptions = new HTMLFormatOptions();
        html32FormatOptions.HTMLBaseFolderName = exportPath + "Html32Folder";
        html32FormatOptions.HTMLFileName = "html32.html";
        html32FormatOptions.HTMLEnableSeparatedPages = false;
        html32FormatOptions.HTMLHasPageNavigator = false;
        exportOptions.FormatOptions = html32FormatOptions;
    }

    private void ConfigureExportToHtml40()
    {
        exportOptions.ExportFormatType = ExportFormatType.HTML40;
        HTMLFormatOptions html40FormatOptions = new HTMLFormatOptions();
        html40FormatOptions.HTMLBaseFolderName = exportPath + "Html40Folder";
        html40FormatOptions.HTMLEnableSeparatedPages = true;
        html40FormatOptions.HTMLHasPageNavigator = true;
        html40FormatOptions.FirstPageNumber = 1;
        html40FormatOptions.LastPageNumber = 3;
        exportOptions.FormatOptions = html40FormatOptions;
    }

    private void ConfigureExportToXlsRec()
    {
        exportOptions.ExportFormatType = ExportFormatType.ExcelRecord;
        diskFileDestinationOptions.DiskFileName = exportPath + "ExcelRecord.xls";
        exportOptions.DestinationOptions = diskFileDestinationOptions;
    }

    protected void exportByType_Click(object sender, EventArgs e)
    {
        ExportSetup();
        ExportSelection();
        ExportCompletion();
    }
}
