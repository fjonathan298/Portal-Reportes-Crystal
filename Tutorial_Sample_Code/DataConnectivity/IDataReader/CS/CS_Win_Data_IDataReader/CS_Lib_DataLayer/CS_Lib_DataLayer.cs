using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace CS_Lib_DataLayer
{
    class CS_Lib_DataLayer
    {
        private const string OLEDB_CONNECTION_STRING = "Provider=sqloledb;Server=localhost;Database=Northwind;Uid=limitedPermissionAccount;Pwd=1234;";
        private const string CUSTOMER_TABLE_QUERY = "SELECT CustomerID, CompanyName, ContactName, Address, City FROM Customers";

        public static IDataReader GetCustomersUsingOleDb()
        {
            OleDbConnection oleDbConnection = new OleDbConnection(OLEDB_CONNECTION_STRING);
            oleDbConnection.Open();
            OleDbCommand oleDbCommand = new OleDbCommand(CUSTOMER_TABLE_QUERY, oleDbConnection);
            IDataReader iDataReader = oleDbCommand.ExecuteReader();
            return iDataReader;
        }
    }
}
