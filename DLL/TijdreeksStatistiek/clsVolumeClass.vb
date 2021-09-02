Imports STOCHLIB.General

Public Class clsVolumeClass
    Dim ID As String
    Dim LBound As Double
    Dim UBound As Double
    Dim Representative As Double

    Public Sub New(myID As String, myLBound As Double, myUBound As Double, myRepresentative As Double)
        ID = myID
        LBound = myLBound
        UBound = myUBound
        Representative = myRepresentative
    End Sub

    Public Function getID() As String
        Return ID
    End Function

    Public Function GetLBound() As Double
        Return LBound
    End Function

    Public Function GetUBound() As Double
        Return UBound
    End Function

    Public Function GetRepresentativeValue() As Double
        Return Representative
    End Function
End Class
