
Imports STOCHLIB.General

Public Class clsUnpavedTBLIG_TRecord
    Friend ID As String
    Friend nm As String
    Friend TimeTable As clsSobekTable
    Friend record As String

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        TimeTable = New clsSobekTable(Me.setup)
    End Sub

    Public Function InitializeTable(BlockInterpolation As Boolean, ReturnPeriodInUse As Boolean, ReturnPeriod As String) As Boolean
        Try
            TimeTable = New clsSobekTable(Me.setup)
            If BlockInterpolation Then TimeTable.pdin1 = 1 Else TimeTable.pdin1 = 0
            If ReturnPeriodInUse Then
                TimeTable.pdin2 = 1
                TimeTable.PDINPeriod = "'" & ReturnPeriod & "'"
            Else
                TimeTable.pdin2 = 0
                TimeTable.PDINPeriod = "''"
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function InitializeTable of class clsUnpavedTBLRecord.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function AddTableValue(myDate As DateTime, myVal As Double) As Boolean
        Try
            TimeTable.AddDatevalPair(myDate, myVal)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function AddTableValue of class clsUnpavedTBLRecord.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub PopulateFromDatatable(ByRef dt As DataTable)
        Dim i As Long
        TimeTable = New clsSobekTable(Me.setup)
        TimeTable.pdin1 = 1 'block interpolation
        TimeTable.pdin2 = 0 'no return period
        TimeTable.PDINPeriod = "''"
        For i = 0 To dt.Rows.Count - 1
            TimeTable.AddDatevalPair(dt.Rows(i)(0), dt.Rows(i)(1))
        Next
    End Sub

    Friend Sub Build()
        Dim i As Integer
        'deze routine bouwt het unpaved.tbl-record op, op basis van de parameterwaarden
        record = "IG_T id '" & ID & "' nm '" & nm & "' PDIN " & TimeTable.pdin1 & " " & TimeTable.pdin2 & " " & TimeTable.PDINPeriod & " pdin TBLE" & vbCrLf
        For i = 0 To TimeTable.Dates.Count - 1
            record = record & "'" & TimeTable.Dates.Values(i).Year & "/" & Format(TimeTable.Dates.Values(i).Month, "00") & "/" & Format(TimeTable.Dates.Values(i).Day, "00") & ";00:00:00' " & TimeTable.Values1.Values(i) & " <" & vbCrLf
        Next
        record = record & " tble ig_t"
    End Sub

    Public Sub Write(ByRef myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub


    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, table As String
        Dim PDINDone As Boolean = False
        Dim Pos As Long
        Done = False

        myRecord = Replace(myRecord, vbCrLf, " ")
        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "PDIN"
                    TimeTable.pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    TimeTable.pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    TimeTable.PDINPeriod = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "TBLE"
                    Pos = InStr(1, myRecord, "tble")
                    table = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, myRecord.Length - Pos - 3)
                    Call TimeTable.Read(table)
            End Select
        End While
    End Sub

End Class

