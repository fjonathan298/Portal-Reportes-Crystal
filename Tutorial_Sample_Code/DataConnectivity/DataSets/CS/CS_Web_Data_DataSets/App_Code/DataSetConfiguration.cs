using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.OleDb;

public class DataSetConfiguration
{
    private const string CONNECTION_STRING = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Xtreme.mdb";
    private const string QUERY_STRING = "SELECT * FROM CUSTOMER";
    private const string DATATABLE_NAME = "Customer";
    private const string DIRECTORY_FILE_PATH = "";

    public static DataSet CustomerDataSet
    {
        get
        {
            DataSet dataSet = new DataSet();
            dataSet.ReadXmlSchema(HttpRuntime.AppDomainAppPath + "\\XMLSchema.xsd");
            OleDbConnection oleDbConnection = new OleDbConnection(CONNECTION_STRING);
            OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(QUERY_STRING, oleDbConnection);
            oleDbDataAdapter.Fill(dataSet, DATATABLE_NAME);
            return dataSet;
        }
    }


}
