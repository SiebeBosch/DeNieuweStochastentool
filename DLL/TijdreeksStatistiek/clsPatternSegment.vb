Public Class clsPatternSegment
    Dim LBoundPercentage As Integer
    Dim UBoundPercentage As Integer
    Dim ID As String

    Public Function getID() As String
        Return "[" & LBoundPercentage & "-" & UBoundPercentage & "]"
    End Function

    Public Function GetLowerBound() As Integer
        Return LBoundPercentage
    End Function

    Public Function GetUpperBound() As Integer
        Return UBoundPercentage
    End Function

    Public Sub New(LowerPerc As Integer, UpperPerc As Integer)
        LBoundPercentage = LowerPerc
        UBoundPercentage = UpperPerc
        Call getID()
    End Sub

End Class
