Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsTimeSeries

    Public ID As String             'the ID for this timeseries
    Public LocationID As String     'the ID of the location this timeseries represents
    Public Par As String            'the parameter this timeseries represents
    Public Records As New List(Of clsTimeSeriesRecord)

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByVal iSeriesID As String, ByVal iLocID As String, ByVal iPar As String, ByVal iTimeSeries As List(Of clsTimeSeriesRecord))
        Setup = mySetup
        ID = iSeriesID
        LocationID = iLocID
        Par = iPar
        Records = iTimeSeries
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal iID As String, ByVal iLoc As String, ByVal iPar As String)
        ID = iID
        LocationID = iLoc
        Par = iPar
        Records = New List(Of clsTimeSeriesRecord)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup)
        Records = New List(Of clsTimeSeriesRecord)
        Setup = mySetup
    End Sub


    Public Sub addRecord(ByVal iDateTime As DateTime, ByVal iValue As Decimal)
        Records.Add(New clsTimeSeriesRecord(iDateTime, iValue))
    End Sub

    Public Function WriteToDatabase(TableName As String, SeriesIDName As String, IDColName As String, ParColName As String, DateColName As String, ValColName As String)
        Try
            Dim i As Long, queryBase As String = "INSERT INTO " & TableName & " (" & SeriesIDName & "," & IDColName & "," & ParColName & "," & DateColName & "," & ValColName & ") VALUES ('" & ID & "','" & LocationID & "','" & Par & "','"

            'then insert the new table. do this via a bulk insert
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing to database...", 0, 10, True)

            'v1.797: made the opening of the database dependent of the current state
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Using transaction = Me.Setup.SqliteCon.BeginTransaction
                    For i = 0 To Records.Count - 1
                        Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                        cmd.CommandText = queryBase & Format(Records(i).iDateTime, "yyyy/MM/dd HH:mm:ss") & "'," & Records(i).Value & ");"
                        cmd.ExecuteNonQuery()
                    Next
                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
                Me.Setup.SqliteCon.Close()
            End Using

            '@@@@

            'Me.Setup.GeneralFunctions.UpdateProgressBar("Writing to database...", 0, 10, True)
            'For i = 0 To Records.Count - 1
            '    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
            '    query = queryBase & Records(i).iDateTime & "'," & Records(i).Value & ");"
            '    Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
            'Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function WriteToDatabase in class clsTimeSeries.")
            Return False
        End Try
    End Function

    Public Function findRecord(ByVal FindDateTime As DateTime, Optional ByVal InterpolateLinear As Boolean = False) As clsTimeSeriesRecord
        'tijdelijke records om later de intepolatierecords aan te kunnen vergelijken
        Dim LRecord As New clsTimeSeriesRecord("1-1-1900", 0)
        Dim URecord As New clsTimeSeriesRecord("1-1-3000", 0)

        If InterpolateLinear = False Then
            'zoek de waarde die op het het dichtbijzijnste tijdstip net vóór het gezochte tijdstip zit
            For Each record As clsTimeSeriesRecord In Records
                If record.iDateTime > LRecord.iDateTime And record.iDateTime <= FindDateTime Then
                    LRecord = record
                End If
            Next
            Return LRecord
        Else
            'zoek de waarde die de dichtstbijzijnde tijdstip net voor en na het gezochte tijdstip
            For Each record As clsTimeSeriesRecord In Records
                If record.iDateTime > LRecord.iDateTime And record.iDateTime <= FindDateTime Then
                    LRecord = record
                ElseIf record.iDateTime < URecord.iDateTime And record.iDateTime >= FindDateTime Then
                    URecord = record
                End If
            Next
            Return New clsTimeSeriesRecord(FindDateTime, Setup.GeneralFunctions.Interpolate(Convert.ToDecimal(LRecord.iDateTime), Convert.ToDecimal(URecord.iDateTime), LRecord.Value, URecord.Value, Convert.ToDecimal(FindDateTime)))
        End If
    End Function

End Class
