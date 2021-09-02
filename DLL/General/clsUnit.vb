Public Class clsUnit
    'deze klasse behandelt eenheidsconversies. Het gaat uit van een eenvoudige opset van teller/noemer
    'teller en noemer mogen op hun beurt ook weer bestaan uit teller*teller of teller/noemer

    'Public UnitComposite As New clsUnitComposite



    Public UnitType As GeneralFunctions.enmDataUnit
    Public UnitAlias As String
    Public Sub New(myType As GeneralFunctions.enmDataUnit, myAlias As String)
        UnitType = myType
        UnitAlias = myAlias
    End Sub
End Class
