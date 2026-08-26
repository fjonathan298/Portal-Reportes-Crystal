Imports System.Data
Imports System.Data.OleDb
Public Class DataCenter
    Private Const OLEDB_CONNECTION_STRING As String = "Provider=sqloledb;Server=localhost;Database=Northwind;Uid=limitedPermissionAccount;Pwd=1234;"
    Private Const CUSTOMER_TABLE_QUERY As String = "SELECT CustomerID, CompanyName, ContactName, Address, City FROM Customers"

    Public Shared Function GetCustomersUsingOleDb() As IDataReader
        Dim myOleDbConnection As OleDbConnection = New OleDbConnection(OLEDB_CONNECTION_STRING)
        myOleDbConnection.Open()
        Dim myOleDbCommand As OleDbCommand = New OleDbCommand(CUSTOMER_TABLE_QUERY, myOleDbConnection)
        Dim myIDataReader As IDataReader = myOleDbCommand.ExecuteReader()
        Return myIDataReader
    End Function
End Class
