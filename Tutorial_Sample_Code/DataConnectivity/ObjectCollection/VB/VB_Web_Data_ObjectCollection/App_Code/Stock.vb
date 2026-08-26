Imports Microsoft.VisualBasic
Imports CrystalDecisions.CrystalReports.Engine

Public Class Stock
    Private _company As Company
    Private _volume As Integer
    Private _price As Double

    Sub New()

    End Sub
    Public Property Price() As Double
        Get
            Return _price
        End Get
        Set(ByVal value As Double)
            _price = value
        End Set
    End Property
    Public Property Volume() As Integer
        Get
            Return _volume
        End Get
        Set(ByVal value As Integer)
            _volume = value
        End Set
    End Property

    <CrystalComplexTypeExpansionLevels(3)> _
    Public Property Company() As Company
        Get
            Return _company
        End Get
        Set(ByVal value As Company)
            _company = value
        End Set
    End Property

    Public Sub New(ByVal company As Company, ByVal volume As Integer, ByVal price As Double)
        _company = company
        _volume = volume
        _price = price
    End Sub

End Class


Public Class Company
    Private _symbol As String
    Private _name As String

    Sub New(ByVal symbol As String, ByVal name As String)
        _symbol = symbol
        _name = name
    End Sub

    Public Property Symbol() As String
        Get
            Return _symbol
        End Get
        Set(ByVal value As String)
            _symbol = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return _name
        End Get
        Set(ByVal value As String)
            _name = value
        End Set
    End Property
End Class