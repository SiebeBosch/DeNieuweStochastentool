Public Class clsUnitConversion

    Dim Units As New clsUnits ' Dictionary(Of String, GeneralFunctions.enmDataUnit)

    Public Function GetAddUnit(UnitAlias As String, myUnitType As GeneralFunctions.enmDataUnit) As clsUnit
        Return Units.GetAddUnit(UnitAlias, myUnitType)
    End Function

    Public Function m3ps2mmph(ByVal Value As Double, ByVal AreaM2 As Double) As Double
    If AreaM2 > 0 Then
      Return Value / AreaM2 * 1000 * 3600
    Else
      Return 0
    End If
  End Function

  Public Function mmph2m3ps(ByVal Value As Double, ByVal AreaM2 As Double) As Double
    If AreaM2 > 0 Then
      Return Value / 1000 * AreaM2 / 3600
    Else
      Return 0
    End If
  End Function


End Class
