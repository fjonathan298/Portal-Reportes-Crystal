using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace CS_Win_Data_DataSets
{
    public class DataSetConfiguration
    {
        private const string CONNECTION_STRING = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Xtreme.mdb";
        private const string QUERY_STRING = "SELECT * FROM CUSTOMER";
        private const string DATATABLE_NAME = "Customer";
        private const string DIRECTORY_FILE_PATH = "";

        public static DataSet CustomerDataSet
        {
            get
            {
                CustomerDataSetSchema dataSet = new CustomerDataSetSchema();
                OleDbConnection oleDbConnection = new OleDbConnection(CONNECTION_STRING);
                OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(QUERY_STRING, oleDbConnection);
                oleDbDataAdapter.Fill(dataSet, DATATABLE_NAME);
                return dataSet;
            }
        }
    }
}
