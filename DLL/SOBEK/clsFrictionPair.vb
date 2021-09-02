Public Class clsFrictionPair
    Dim FrictionType As GeneralFunctions.enmFrictionType
    Dim FrictionValue As Double
    Public Sub setFrictionType(myType As String)
        FrictionType = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmFrictionType), myType), GeneralFunctions.enmFrictionType)
    End Sub
    Public Sub setFrictionValue(myVal As Double)
        FrictionValue = myVal
    End Sub
    Public Function getFrictionTypeNum() As Integer
        Return Format("0", FrictionType)
    End Function
    Public Function getFrictionValue() As Double
        Return FrictionValue
    End Function

End Class
