Imports System.Data
Imports System.Data.OleDb

Public Class DataSetConfiguration

    Private Const CONNECTION_STRING As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\xtreme.mdb"
    Private Const QUERY_STRING As String = "SELECT * FROM CUSTOMER"
    Private Const DATATABLE_NAME As String = "Customer"
    Private Const DIRECTORY_FILE_PATH As String = "C:\WebSites\VB_Web_Data_DataSets\"

    Public Shared ReadOnly Property CustomerDataSet() As DataSet
        Get
            Dim myDataSet As CustomerDataSetSchema = New CustomerDataSetSchema()
            Dim myOleDbConnection As OleDbConnection = New OleDbConnection(CONNECTION_STRING)
            Dim myOleDbDataAdapter As OleDbDataAdapter = New OleDbDataAdapter(QUERY_STRING, myOleDbConnection)
            myOleDbDataAdapter.Fill(myDataSet, DATATABLE_NAME)
            Return myDataSet
        End Get
    End Property



End Class
