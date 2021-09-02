Imports STOCHLIB.General

Public Class clsHydroEvents
    Public Events As Dictionary(Of Integer, clsHydroEvent) 'event number as key
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        Events = New Dictionary(Of Integer, clsHydroEvent)
    End Sub

    Public Function GetEventSumList() As List(Of Double)
        Dim myList As New List(Of Double)
        For Each myEvent As clsHydroEvent In Events.Values
            myList.Add(myEvent.EventSum)
        Next
        Return myList
    End Function

    Public Function SortByStartDate() As Dictionary(Of Integer, clsHydroEvent)
        'this function sorts the events by startdate and returns it as a dictionary
        Dim Sorted As New Dictionary(Of Integer, clsHydroEvent)
        Sorted = (From entry In Events Order By entry.Value.GetStartDate Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        Return Sorted
    End Function

    Public Function WriteToDatabase(LocationID As String, Parameter As String, Duration As Integer, tsseconds As Integer) As Boolean
        Try
            Dim query As String
            Dim ts As Integer
            Dim iEvent As Integer = 0, nEvents As Integer = Events.Values.Count

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing events to database...", 0, 10, True)
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Using transaction = Me.Setup.SqliteCon.BeginTransaction

                    For Each myEvent As clsHydroEvent In Events.Values
                        iEvent += 1
                        Me.Setup.GeneralFunctions.UpdateProgressBar("", iEvent, nEvents)

                        'we will write the metadata for this event to the EVENTS table
                        query = "INSERT INTO EVENTS (EVENTNUM, EVENTSUM, EVENTMIN, EVENTMAX, DURATION, LOCATIONID, PARAMETER, STARTDATE) VALUES (" & myEvent.GetEventNum & "," & myEvent.EventSum & "," & myEvent.EventMin & "," & myEvent.EventMax & "," & Duration & ",'" & LocationID & "','" & Parameter & "','" & Me.Setup.GeneralFunctions.FormatDateAsISO8601(myEvent.GetStartDate) & "');"
                        cmd.CommandText = query
                        cmd.ExecuteNonQuery()

                        For ts = 0 To Duration - 1
                            query = "INSERT INTO EVENTSERIES (EVENTNUM, DURATION, LOCATIONID, PARAMETER, TIMESTEP, DATAVALUE) VALUES (" & myEvent.GetEventNum & "," & Duration & ",'" & LocationID & "','" & Parameter & "'," & (ts + 1) & "," & myEvent.Values(ts) & ");"
                            cmd.CommandText = query
                            cmd.ExecuteNonQuery()
                        Next
                    Next
                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing new events to database...", 0, nEvents, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try
    End Function


    Public Function WriteAsBoundarySeriesToDatabase(BoundaryID As String, Duration As Integer, tsseconds As Integer) As Boolean
        Try
            Dim ts As Integer
            Dim iEvent As Integer = 0, nEvents As Integer = Events.Values.Count

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing boundary series to database...", 0, 10, True)
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Dim cmd As SQLite.SQLiteCommand

            'begin transaction
            cmd = New SQLite.SQLiteCommand("begin", Me.Setup.SqliteCon)
            cmd.ExecuteNonQuery()

            'insert our records
            For Each myEvent As clsHydroEvent In Events.Values
                iEvent += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iEvent, nEvents)

                For ts = 0 To Duration - 1
                    cmd = New SQLite.SQLiteCommand("INSERT INTO BOUNDARYSERIES (EVENTNUM, DURATION, BOUNDARYID, TIMESTEP, DATAVALUE) VALUES (" & myEvent.GetEventNum & "," & Duration & ",'" & BoundaryID & "'," & (ts + 1) & "," & myEvent.Values(ts) & ");", Me.Setup.SqliteCon)
                    cmd.ExecuteNonQuery()
                Next
            Next

            'end transaction
            cmd = New SQLite.SQLiteCommand("end", Me.Setup.SqliteCon)
            cmd.ExecuteNonQuery()

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing new boundary series to database...", 0, nEvents, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try
    End Function
    Public Function WriteInitialValueToDatabase(LocationID As String, Duration As Integer) As Boolean
        Try
            Dim query As String
            Dim iEvent As Integer = 0, nEvents As Integer = Events.Values.Count

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing new events to database...", 0, nEvents, True)
            For Each myEvent As clsHydroEvent In Events.Values
                iEvent += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iEvent, nEvents)
                query = "INSERT INTO INITIALS (LOCATIONID, DURATION, EVENTNUM, DATEANDTIME, DATAVALUE) VALUES ('" & LocationID & "'," & Duration & "," & myEvent.GetEventNum & ",'" & myEvent.GetStartDate & "'," & myEvent.Values(0) & ");"
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try
    End Function
    Public Function ReadFromDatabase(LocationID As String, Parameter As String, Duration As Integer, IncludeEventSeries As Boolean) As Boolean
        Try
            Dim myQuery As String = "SELECT EVENTNUM, EVENTSUM, STARTDATE, VOLUMECLASS, BOUNDARYCLASS, INITIALCLASS, PATTERNCLASS FROM EVENTS WHERE LOCATIONID='" & LocationID & "' AND PARAMETER='" & Parameter & "' AND DURATION=" & Duration & " ORDER BY EVENTNUM;"
            Dim evt As New DataTable
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, myQuery, evt)
            Dim RecordIdx As Integer
            Dim newEvent As clsHydroEvent

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading events from database...", 0, 10, True)
            For RecordIdx = 0 To evt.Rows.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", RecordIdx + 1, evt.Rows.Count)
                newEvent = New clsHydroEvent
                newEvent.SetParameter(Parameter)
                newEvent.SetLocationID(LocationID)
                newEvent.SetEventNum(evt.Rows(RecordIdx)(0))
                newEvent.EventSum = evt.Rows(RecordIdx)(1)
                newEvent.SetStartDate(evt.Rows(RecordIdx)(2))
                newEvent.SetEndDate(newEvent.GetStartDate.AddHours(Duration))
                If Not IsDBNull(evt.Rows(RecordIdx)(3)) Then newEvent.VolumeClass = evt.Rows(RecordIdx)(3)
                If Not IsDBNull(evt.Rows(RecordIdx)(4)) Then newEvent.BoundaryClass = evt.Rows(RecordIdx)(4)
                If Not IsDBNull(evt.Rows(RecordIdx)(5)) Then newEvent.InitialClass = evt.Rows(RecordIdx)(5)
                If Not IsDBNull(evt.Rows(RecordIdx)(6)) Then newEvent.PatternClass = evt.Rows(RecordIdx)(6)
                If IncludeEventSeries Then
                    myQuery = "SELECT DATAVALUE FROM EVENTSERIES WHERE LOCATIONID='" & LocationID & "' AND PARAMETER='" & Parameter & "' AND EVENTNUM=" & newEvent.GetEventNum & " AND DURATION=" & Duration & " ORDER BY TIMESTEP;"
                    Dim tt As New DataTable
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, myQuery, tt, False)
                    For i = 0 To tt.Rows.Count - 1
                        newEvent.Values.Add(tt.Rows(i)(0))
                    Next
                End If
                Events.Add(newEvent.GetEventNum, newEvent)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Events successfully read.", 0, 10, True)

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class
