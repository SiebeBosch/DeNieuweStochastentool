Public Class clsStochasticVolumeClass

    Public Volumes As Dictionary(Of String, Double)     'one volume for each location ID; probably built in in favor of the CM model (Aa en Maas), however abandoned
    Public Volume As Double
    Public P As Double

    Public Sub New(myP As Double)
        Volumes = New Dictionary(Of String, Double)
        P = myP
    End Sub

    Public Sub New(ByVal LocationID As String, ByVal myVol As Double, ByVal myP As Double)
        Volumes.Add(LocationID.Trim.ToUpper, myVol)
        P = myP
    End Sub

    Public Sub AddLocation(ByVal LocationID As String, ByVal myVol As Double)
        Volumes.Add(LocationID.Trim.ToUpper, myVol)
    End Sub
End Class
