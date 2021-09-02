Imports STOCHLIB.General
Public Class clsTemporalPattern
    Public ID As String
    Public Fractions As New List(Of Double)        'for every timestep the fraction of the total eventsum
    Public Events As New List(Of clsHydroEvent)          'a list of all events (event numbers) that have been classified as belonging to this pattern
    Public InUse As Boolean
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByVal myID As String)
        ID = myID
        Setup = mySetup
        InUse = True
    End Sub

    Public Function GetID() As String
        Return ID
    End Function

    Public Sub AddValue(Value As Double)
        Fractions.Add(Value)
    End Sub

    Public Function getSum() As Double
        Dim mySum As Double, i As Integer
        For i = 0 To Fractions.Count - 1
            mySum += Fractions(i)
        Next
        Return mySum
    End Function

    Public Function AddEvent(ByRef myEvent As clsHydroEvent) As Boolean
        Try
            Events.Add(myEvent)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function calcRMSE(Values As List(Of Double), EventSum As Double, ByRef RMSE As Double) As Boolean
        'this function computes the RMSE for a given event by comparing its values to the current pattern.
        Try
            RMSE = 0
            If Values.Count <> Fractions.Count Then Throw New Exception("Error: number of values in event (" & Values.Count & ") does not match number of values in pattern (" & Fractions.Count & ")")
            For i = 0 To Fractions.Count - 1
                RMSE += (Fractions(i) - Values(i) / EventSum) ^ 2
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function readFromDatabase(TableName As String, Duration As Integer) As Boolean
        Try
            Dim dt As New DataTable, i As Integer
            Dim query As String = "SELECT FRACTION FROM " & TableName & " WHERE DURATION=" & Duration & " AND PATTERNID='" & ID & "' ORDER BY TIMESTEP;"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, False)
            For i = 0 To dt.Rows.Count - 1
                Fractions.Add(dt.Rows(i)(0))
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function WriteToDatabase(TableName As String, LocationID As String, Parameter As String) As Boolean
        Try
            Dim queryStart As String = "INSERT INTO " & TableName & " (PATTERNID,LOCATIONID,PARAMETER,DURATION,TIMESTEP,FRACTION) VALUES "
            Dim query As String = ""
            Dim i As Integer
            For i = 0 To Fractions.Count - 1
                query = queryStart & "('" & ID & "','" & LocationID & "','" & Parameter & "'," & Fractions.Count & "," & i & "," & Fractions(i) & ");"
                If Not Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqLiteCon, query, False) Then Throw New Exception("Error executing query " & query)
            Next
            'Me.Setup.SqLiteCon.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function WriteToDatabase(TableName As String) As Boolean
        Try
            Dim queryStart As String = "INSERT INTO " & TableName & " (PATTERNID,DURATION,TIMESTEP,FRACTION) VALUES "
            Dim query As String = ""
            Dim i As Integer

            Me.Setup.SqliteCon.Open()
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Using transaction = Me.Setup.SqliteCon.BeginTransaction
                    For i = 0 To Fractions.Count - 1
                        query = queryStart & "('" & ID & "'," & Fractions.Count & "," & i + 1 & "," & Fractions(i) & ");"
                        cmd.CommandText = query
                        'If Not Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False) Then Throw New Exception("Error executing query " & query)
                        cmd.ExecuteNonQuery()
                    Next
                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using
            Me.Setup.SqliteCon.Close()

            '@@@
            'For i = 0 To Fractions.Count - 1
            '    query = queryStart & "('" & ID & "'," & Fractions.Count & "," & i + 1 & "," & Fractions(i) & ");"
            '    If Not Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False) Then Throw New Exception("Error executing query " & query)
            'Next
            'Me.Setup.SqLiteCon.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function WriteClassToDatabase(TableName As String) As Boolean
        Try
            Dim queryStart As String = "INSERT INTO " & TableName & " (PATTERNID,DURATION,TIMESTEP,FRACTION) VALUES "
            Dim query As String = ""
            Dim i As Integer
            For i = 0 To Fractions.Count - 1
                query = queryStart & "('" & ID & "','" & "'," & Fractions.Count & "," & i & "," & Fractions(i) & ");"
                If Not Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False) Then Throw New Exception("Error executing query " & query)
            Next
            'Me.Setup.SqLiteCon.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function MakeUniform() As Boolean
        'this function makes sure the sum of all values inside the pattern equals 1
        'ergo: each timestep represents the fraction of the total volume of the event
        Try
            Dim mySum As Double
            For i = 0 To Fractions.Count - 1
                mySum += Fractions(i)
            Next
            If mySum = 0 Then
                Throw New Exception("Error making pattern uniform: sum was 0 for pattern " & ID)
            ElseIf mySum <> 1 Then
                For i = 0 To Fractions.Count - 1
                    Fractions(i) *= 1 / mySum
                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function MakeUniform of class clsTemporalPattern.")
            Return False
        End Try
    End Function


End Class
