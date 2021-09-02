Public Class clsLocationParameterCombination
    Dim LocationID As String
    Dim Parameter As String
    Dim TimeSeries As List(Of Double)

    Public Sub New(myLocation As String, myParameter As String)
        LocationID = myLocation
        Parameter = myParameter
        TimeSeries = New List(Of Double)
    End Sub

    Public Sub AddValue(myVal As Double)
        TimeSeries.Add(myVal)
    End Sub

    Public Function GetLocationID() As String
        Return LocationID
    End Function
    Public Function GetParameter() As String
        Return Parameter
    End Function
    Public Function GetValue(Idx As Integer) As Double
        Return TimeSeries(Idx)
    End Function
End Class
