Imports STOCHLIB.General
Public Class cls1DBranch
    Private Setup As clsSetup
    Friend ID As String
    Friend RouteNumber As Integer = -1
    Friend VectorPoints As Dictionary(Of Double, clsXY) 'key = chainage
    Friend bn As cls1DNode
    Friend en As cls1DNode

    Public Sub New(ByRef mySetup As clsSetup, myID As String)
        Setup = mySetup
        ID = myID
        VectorPoints = New Dictionary(Of Double, clsXY)
    End Sub

    Public Sub AddVectorPoint(X As Double, Y As Double, Chainage As Double)
        VectorPoints.Add(Chainage, New clsXY(X, Y))
    End Sub

    Public Function GetCoordsFromChainage(Chainage As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        Try
            Dim i As Integer
            For i = 1 To VectorPoints.Count - 1
                If VectorPoints.Keys(i) >= Chainage Then
                    X = Me.Setup.GeneralFunctions.Interpolate(VectorPoints.Keys(i - 1), VectorPoints.Values(i - 1).X, VectorPoints.Keys(i), VectorPoints.Values(i).X, Chainage)
                    Y = Me.Setup.GeneralFunctions.Interpolate(VectorPoints.Keys(i - 1), VectorPoints.Values(i - 1).Y, VectorPoints.Keys(i), VectorPoints.Values(i).Y, Chainage)
                    Return True
                End If
            Next
            Throw New Exception("")
        Catch ex As Exception
            Me.Setup.Log.AddError("Unable to get coordinates from branch " & ID & " for chainage " & Chainage & ": " & ex.Message)
            Return False
        End Try
    End Function

End Class
