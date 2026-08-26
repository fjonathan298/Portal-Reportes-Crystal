Imports System.Runtime.InteropServices

<ComVisible(True), ClassInterface(ClassInterfaceType.None), GuidAttribute("CCF45F61-46D6-425A-AADF-2382774AE721")> _
Public Class ExchangeUfl : Implements IExchangeUfl
    Public Function ConvertUSDollarsToCDN1(ByVal usd As Double) As Double Implements IExchangeUfl.ConvertUSDollarsToCDN
        If usd > Double.MaxValue Then
            Throw New Exception("Value submitted is larger than the maximum value allowed for a double.")
        End If
        Return (usd * 1.45)
    End Function
End Class
