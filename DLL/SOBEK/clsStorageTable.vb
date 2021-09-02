Imports STOCHLIB.General
Public Class clsStorageTable
    Private Setup As clsSetup

    Public AreasTable As New Dictionary(Of Double, Double)
    Public VolumesTable As New Dictionary(Of Double, Double)
    Dim X As Double, Y As Double

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub AddValue(Elevation As Double, Area As Double)
        If AreasTable.ContainsKey(Elevation) Then
            AreasTable.Item(Elevation) += Area
        Else
            AreasTable.Add(Elevation, Area)
        End If
    End Sub

    Public Function getAreaByKey(Elevation As Double) As Double
        If AreasTable.ContainsKey(Elevation) Then
            Return AreasTable.Item(Elevation)
        Else
            Return 0
        End If
    End Function

    Public Function getVolumeByKey(Elevation As Double) As Double
        If VolumesTable.ContainsKey(Elevation) Then
            Return VolumesTable.Item(Elevation)
        Else
            Return 0
        End If
    End Function

    Public Function getAreasTable() As Dictionary(Of Double, Double)
        Return AreasTable
    End Function

    Public Sub Complete()
        'sorts by elevation value and adds up the areas
        Dim i As Integer, lastVal As Double = 0
        Dim sortedDict = (From entry In AreasTable Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        Dim cumDict As New Dictionary(Of Double, Double)
        For i = 0 To sortedDict.Values.Count - 1
            cumDict.Add(sortedDict.Keys(i), sortedDict.Values(i) + lastVal)
            lastVal = cumDict.Values(i)
        Next
        AreasTable = cumDict

        'this is also the moment where we can populate the volumes table
        Dim myVolume As Double = 0
        VolumesTable = New Dictionary(Of Double, Double)
        VolumesTable.Add(AreasTable.Keys(0), myVolume)   'first value
        For i = 1 To AreasTable.Count - 1
            myVolume += (AreasTable.Keys(i) - AreasTable.Keys(i - 1)) * AreasTable.Values(i - 1)
            VolumesTable.Add(AreasTable.Keys(i), myVolume)
        Next
    End Sub

    Public Function GetAreaByElevation(elevation As Double) As Double
        Dim lastValue As Double = 0

        If elevation < AreasTable.Keys(0) Then Return 0
        If elevation >= AreasTable.Keys(AreasTable.Count - 1) Then Return AreasTable.Values(AreasTable.Count - 1)

        For i = 0 To AreasTable.Values.Count - 1
            If AreasTable.Keys(i) > elevation Then
                Return lastValue
            Else
                lastValue = AreasTable.Values(i)
            End If
        Next
        Return lastValue
    End Function

    Public Function GetVolumeByElevation(elevation As Double) As Double
        Dim lastValue As Double = VolumesTable.Values(0)

        If elevation < VolumesTable.Keys(0) Then
            Return 0
        ElseIf elevation = VolumesTable.Keys(0) Then
            Return VolumesTable.Values(0)
        End If
        If elevation >= VolumesTable.Keys(VolumesTable.Count - 1) Then Return VolumesTable.Values(VolumesTable.Count - 1)

        For i = 1 To VolumesTable.Values.Count - 1
            If VolumesTable.Keys(i) > elevation Then
                Return lastValue
            Else
                lastValue = VolumesTable.Values(i)
            End If
        Next
    End Function

    'Public Sub Aggregate()
    '    'aggregates the elevation values to an increment of 1 cm
    '    Dim Elevation As Double, NewElevation As Double, SurfaceArea As Double, Volume As Double
    '    Dim NewVolumesTable As New Dictionary(Of Double, Double)
    '    Dim NewAreasTable As New Dictionary(Of Double, Double)
    '    Dim lastArea As Double = 0
    '    Dim lastVolume As Double = 0
    '    For i = 0 To AreasTable.Values.Count - 1
    '        Elevation = AreasTable.Keys(i)
    '        If AreasTable.Item(Elevation) > 0 Then SurfaceArea = AreasTable.Item(Elevation) Else SurfaceArea = lastArea
    '        If VolumesTable.Item(Elevation) > 0 Then Volume = VolumesTable.Item(Elevation) Else Volume = lastVolume
    '        NewElevation = Math.Round(Elevation, 2)
    '        If Not NewAreasTable.ContainsKey(NewElevation) Then NewAreasTable.Add(NewElevation, SurfaceArea)
    '        If Not NewVolumesTable.ContainsKey(NewElevation) Then NewVolumesTable.Add(NewElevation, Volume)
    '        lastArea = SurfaceArea
    '        lastVolume = Volume
    '    Next

    '    AreasTable = NewAreasTable
    '    VolumesTable = NewVolumesTable
    'End Sub


End Class
