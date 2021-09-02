Public Class clsUnits
    Friend Units As New Dictionary(Of String, clsUnit)

    Public Function GetAddUnit(UnitAlias As String, UnitType As GeneralFunctions.enmDataUnit) As clsUnit
        If Not Units.ContainsKey(UnitAlias.Trim.ToUpper) Then
            Units.Add(UnitAlias.Trim.ToUpper, New clsUnit(UnitAlias, UnitType))
            Return Units.Item(UnitAlias.Trim.ToUpper)
        Else
            Return Units.Item(UnitAlias.Trim.ToUpper)
        End If
    End Function

End Class
