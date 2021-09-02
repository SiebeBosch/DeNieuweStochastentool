Public Class clsFrictionTable

    Public LanduseSource As GeneralFunctions.enmLanduseSource
    Public FrictionList As New Dictionary(Of Integer, clsFrictionRange)
    Public FrictionType As GeneralFunctions.enmFrictionType

    Public Sub AddRecord(LanduseCode As Integer, Description As String, LowerValue As Double, CenterValue As Double, UpperValue As Double, Literature As String)
        Dim myRange = New clsFrictionRange
        myRange.LanduseCode = LanduseCode
        myRange.LanduseDescription = Description
        myRange.Lower = LowerValue
        myRange.Center = CenterValue
        myRange.Upper = UpperValue
        myRange.Literature = Literature
        If Not FrictionList.ContainsKey(LanduseCode) Then FrictionList.Add(myRange.LanduseCode, myRange)
    End Sub

    Public Sub New(myLanduseSource As GeneralFunctions.enmLanduseSource, myFrictionType As GeneralFunctions.enmFrictionType)
        LanduseSource = myLanduseSource
        FrictionType = myFrictionType
    End Sub

End Class
