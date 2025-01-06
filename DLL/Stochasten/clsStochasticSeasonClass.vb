Imports STOCHLIB.General

Public Class clsStochasticSeasonClass
    Friend Use As Boolean
    Friend Name As String
    Friend P As Double
    Friend EventStart As Date
    Friend IDColIdx As Integer  'index number of the ID column in the runs grid
    Friend PColIdx As Integer   'index number of the P column in the runs grid

    'keeps track of whether a stochast is in use
    Friend VolumeUse As Boolean
    Friend PatternUse As Boolean
    Friend GroundwaterUse As Boolean
    Friend WaterLevelsUse As Boolean
    Friend WindUse As Boolean
    Friend Extra1Use As Boolean
    Friend Extra2Use As Boolean
    Friend Extra3Use As Boolean
    Friend Extra4Use As Boolean

    Friend MileageCounter As clsMileageCounter      'within this season, the mileagecounter keeps track of all possible combinations of the stochasts stated below

    'all supported stochasts
    Public Volumes As Dictionary(Of Double, clsStochasticVolumeClass)
    Public Patterns As Dictionary(Of String, clsStochasticPatternClass)
    Public Groundwater As Dictionary(Of String, clsStochasticGroundwaterClass)
    Public WaterLevels As Dictionary(Of String, clsStochasticWaterLevelClass)
    Public Wind As Dictionary(Of String, clsStochasticWindClass)
    Public Extra1 As Dictionary(Of String, clsStochasticExtraClass)
    Public Extra2 As Dictionary(Of String, clsStochasticExtraClass)
    Public Extra3 As Dictionary(Of String, clsStochasticExtraClass)
    Public Extra4 As Dictionary(Of String, clsStochasticExtraClass)

    Friend Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, mySeason As STOCHLIB.GeneralFunctions.enmSeason, ClassName As String, ClassEventStart As Date, ClassP As Double)

        Setup = mySetup
        Use = True
        Name = ClassName
        EventStart = ClassEventStart
        P = ClassP

        Volumes = New Dictionary(Of Double, clsStochasticVolumeClass)
        Patterns = New Dictionary(Of String, clsStochasticPatternClass)
        Groundwater = New Dictionary(Of String, clsStochasticGroundwaterClass)
        WaterLevels = New Dictionary(Of String, clsStochasticWaterLevelClass)
        Wind = New Dictionary(Of String, clsStochasticWindClass)
        Extra1 = New Dictionary(Of String, clsStochasticExtraClass)
        Extra2 = New Dictionary(Of String, clsStochasticExtraClass)
        Extra3 = New Dictionary(Of String, clsStochasticExtraClass)
        Extra4 = New Dictionary(Of String, clsStochasticExtraClass)

        MileageCounter = New clsMileageCounter(Me.Setup)

        VolumeUse = False
        PatternUse = False
        GroundwaterUse = False
        WaterLevelsUse = False
        WindUse = False
        Extra1Use = False
        Extra2Use = False
        Extra3Use = False
        Extra4Use = False

    End Sub

    Public Sub New(ByRef mySetup As clsSetup)

        Setup = mySetup

        Volumes = New Dictionary(Of Double, clsStochasticVolumeClass)
        Patterns = New Dictionary(Of String, clsStochasticPatternClass)
        Groundwater = New Dictionary(Of String, clsStochasticGroundwaterClass)
        WaterLevels = New Dictionary(Of String, clsStochasticWaterLevelClass)
        Wind = New Dictionary(Of String, clsStochasticWindClass)
        Extra1 = New Dictionary(Of String, clsStochasticExtraClass)
        Extra2 = New Dictionary(Of String, clsStochasticExtraClass)
        Extra3 = New Dictionary(Of String, clsStochasticExtraClass)
        Extra4 = New Dictionary(Of String, clsStochasticExtraClass)

        MileageCounter = New clsMileageCounter(Me.Setup)

        VolumeUse = False
        PatternUse = False
        GroundwaterUse = False
        WaterLevelsUse = False
        WindUse = False
        Extra1Use = False
        Extra2Use = False
        Extra3Use = False
        Extra4Use = False
    End Sub
    Public Function GetEventStart() As Date
        Return EventStart
    End Function

    Public Function GetAddVolumeClass(Volume As Double, Volume_P As Double) As clsStochasticVolumeClass
        If Volumes.ContainsKey(Volume) Then
            Return Volumes.Item(Volume)
        Else
            'Dim myVolume As New clsStochasticVolumeClass(Volume, Volume_P)
            'Volumes.Add(Volume, myVolume)
            'Return myVolume
        End If
        Return Nothing
    End Function

    Public Function GetAddPatternClass(Patroon As String, Patroon_P As Double) As clsStochasticPatternClass
        If Patterns.ContainsKey(Patroon.Trim.ToUpper) Then
            Return Patterns.Item(Patroon.Trim.ToUpper)
        Else
            Dim myPattern As New clsStochasticPatternClass(Patroon, Patroon_P)
            Patterns.Add(Patroon.Trim.ToUpper, myPattern)
            Return myPattern
        End If
    End Function

    Public Function GetAddGWClass(ID As String, GW_P As Double) As clsStochasticGroundwaterClass
        If Groundwater.ContainsKey(ID.Trim.ToUpper) Then
            Return Groundwater.Item(ID.Trim.ToUpper)
        Else
            Dim myGW As New clsStochasticGroundwaterClass(ID, GW_P)
            Groundwater.Add(ID.Trim.ToUpper, myGW)
            Return myGW
        End If
    End Function

    Public Function GetAddWLClass(ID As String, WL_P As Double) As clsStochasticWaterLevelClass
        If WaterLevels.ContainsKey(ID.Trim.ToUpper) Then
            Return WaterLevels.Item(ID.Trim.ToUpper)
        Else
            Dim myWL As New clsStochasticWaterLevelClass(ID, WL_P)
            WaterLevels.Add(ID.Trim.ToUpper, myWL)
            Return myWL
        End If
    End Function

    Public Function GetAddWindClass(ID As String, Wind_P As Double) As clsStochasticWindClass
        If Wind.ContainsKey(ID.Trim.ToUpper) Then
            Return Wind.Item(ID.Trim.ToUpper)
        Else
            Dim myWind As New clsStochasticWindClass(ID, Wind_P)
            Wind.Add(ID.Trim.ToUpper, myWind)
            Return myWind
        End If
    End Function

    Public Function GetAddExtraClass(ExtraNum As Integer, ID As String, Extra_P As Double) As clsStochasticExtraClass
        Dim myExtra As clsStochasticExtraClass
        Dim MyDict As Dictionary(Of String, clsStochasticExtraClass) = Nothing
        Select Case ExtraNum
            Case Is = 1
                MyDict = Extra1
            Case Is = 2
                MyDict = Extra2
            Case Is = 3
                MyDict = Extra3
            Case Is = 4
                MyDict = Extra4
        End Select

        If MyDict.ContainsKey(ID.Trim.ToUpper) Then
            Return MyDict.Item(ID.Trim.ToUpper)
        Else
            myExtra = New clsStochasticExtraClass(ID, Extra_P)
            MyDict.Add(ID.Trim.ToUpper, myExtra)
            Return myExtra
        End If
    End Function

End Class
