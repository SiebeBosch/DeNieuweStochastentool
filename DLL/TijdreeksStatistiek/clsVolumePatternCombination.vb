Imports STOCHLIB.General
Public Class clsVolumePatternCombination
    Dim Volume As clsVolumeClass
    Dim Pattern As clsTemporalPattern
    Dim Duration As Integer
    Dim Events As Dictionary(Of Integer, clsHydroEvent)
    Public SecondaryLocationParameterCombinations As clsLocationParameterCombinations
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByRef myVolume As clsVolumeClass, ByRef myPattern As clsTemporalPattern, ByRef myDuration As Integer)
        Setup = mySetup
        Events = New Dictionary(Of Integer, clsHydroEvent)
        Volume = myVolume
        Pattern = myPattern
        Duration = myDuration
        SecondaryLocationParameterCombinations = New clsLocationParameterCombinations(Me.Setup)
    End Sub

    Public Function GetID() As String
        Return Volume.getID.Trim.ToUpper & "_" & Pattern.GetID.Trim.ToUpper
    End Function

    Public Function GetPattern() As clsTemporalPattern
        Return Pattern
    End Function

    Public Function getVolumeClass() As clsVolumeClass
        Return Volume
    End Function

    Public Function GetPatternID() As String
        Return Pattern.GetID
    End Function

    Public Function GetVolumeClassID() As String
        Return Volume.getID
    End Function

    Public Function GetDuration() As Integer
        Return Duration
    End Function

    Public Sub AddEvent(ByRef myEvent As clsHydroEvent)
        If Not Events.ContainsKey(myEvent.GetEventNum) Then Events.Add(myEvent.GetEventNum, myEvent)
    End Sub

    Public Function CountEvents() As Integer
        Return Events.Count
    End Function

    Public Function GetEvent(Idx As Integer) As clsHydroEvent
        If Idx < Events.Count Then
            Return Events.Values(Idx)
        Else
            Return Nothing
        End If
    End Function

    Public Sub RemoveEvent(EventNum As Integer)
        If Events.ContainsKey(EventNum) Then Events.Remove(EventNum)
    End Sub

    Public Function AddSecondaryLocationParameterCombinationsFromDatabase()
        Try
            'this function reads all secondary locationID/parameter combinations from the database and calculates their temporal pattern
            'with secondary we mean all location/parameter combinations that have NOT been used to classify volume and pattern, but that are requested alongside the officially classified combination
            Dim query As String
            Dim i As Integer, j As Integer, k As Integer
            Dim lt As New DataTable
            Dim pt As New DataTable
            Dim et As New DataTable
            Dim myAvg As Double
            query = "SELECT DISTINCT LOCATIONID FROM EVENTS WHERE DURATION=" & Duration & ";"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, lt)
            query = "SELECT DISTINCT PARAMETER FROM EVENTS WHERE DURATION=" & Duration & ";"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, pt)

            'walk through all combinations of locationID and parameter
            For i = 0 To lt.Rows.Count - 1
                For j = 0 To pt.Rows.Count - 1

                    'the current combination of locationID and parameter. See if it contains timeseries in the EVENTS table of the database
                    query = "SELECT DISTINCT EVENTNUM FROM EVENTS WHERE LOCATIONID='" & lt.Rows(i)(0) & "' AND PARAMETER='" & pt.Rows(j)(0) & "' AND DURATION=" & Duration & ";"
                    et = New DataTable
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, et)
                    If et.Rows.Count > 0 Then
                        Dim EventSeries As New List(Of DataTable)   'this will contain all events within this volume/pattern combination for a given location+parameter
                        For k = 0 To Events.Count - 1
                            et = New DataTable
                            query = "SELECT DATAVALUE FROM EVENTS WHERE LOCATIONID='" & lt.Rows(i)(0) & "' AND PARAMETER='" & pt.Rows(j)(0) & "' AND DURATION=" & Duration & " AND EVENTNUM=" & Events.Values(k).GetEventNum & " ORDER BY DATEANDTIME;"
                            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, et)
                            If et.Rows.Count > 0 Then
                                EventSeries.Add(et)
                            End If
                        Next

                        'create a new LocationParameterCombination and add it to the current VOlumePatternCombination
                        'for every timestep compute the average value over all underlying events
                        Dim myCombi As clsLocationParameterCombination = SecondaryLocationParameterCombinations.GetAdd(lt.Rows(i)(0), pt.Rows(j)(0))
                        For ts = 1 To Duration
                            myAvg = 0
                            For k = 0 To EventSeries.Count - 1
                                myAvg += EventSeries.Item(k).Rows(ts - 1)(0)
                            Next
                            myAvg = myAvg / EventSeries.Count
                            myCombi.AddValue(myAvg)
                        Next
                    End If
                Next
            Next
            Return True
        Catch ex As Exception
            Return False
        Finally
            Me.Setup.SqliteCon.Close()
        End Try
    End Function

End Class
