Imports STOCHLIB.General
Public Class clsModelParameterClass
    'this class is meant to describe classifications of events by modelparameter
    'e.g. primary parameter is the HBV parameter lz (lower zone)
    'secondary parameter is uz (upper zone) with a side parameter sm (soil moisture)

    Private Setup As clsSetup

    Public PrimaryParameter As GeneralFunctions.enmModelParameter
    Public PrimarySideParameters As List(Of GeneralFunctions.enmModelParameter)
    Public SecondaryParameter As GeneralFunctions.enmModelParameter
    Public SecondarySideParameters As List(Of GeneralFunctions.enmModelParameter)


    Public Sub New(ByRef mySetup As clsSetup, myPrimaryParameter As GeneralFunctions.enmModelParameter, myPrimarySideParameters As List(Of GeneralFunctions.enmModelParameter), mySecondaryParameter As GeneralFunctions.enmModelParameter, mySecondarySideParameters As List(Of GeneralFunctions.enmModelParameter))
        Me.Setup = mySetup
        PrimaryParameter = myPrimaryParameter
        PrimarySideParameters = myPrimarySideParameters
        SecondaryParameter = mySecondaryParameter
        SecondarySideParameters = mySecondarySideParameters
    End Sub

End Class
