Imports STOCHLIB.General

Public Class clsUnpavedTBLSC_TRecord

    Friend ID As String
    Friend nm As String
    Friend Table As clsSobekTable
    Friend record As String

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        Table = New clsSobekTable(Me.setup)
    End Sub

    Public Function InitializeTable(BlockInterpolation As Boolean) As Boolean
        Try
            Table = New clsSobekTable(Me.setup)
            If BlockInterpolation Then Table.pdin1 = 1 Else Table.pdin1 = 0
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function InitializeTable of class clsUnpavedTBLRecord.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function AddTableValue(xVal As Double, yVal As Double) As Boolean
        Try
            Table.AddDataPair(2, xVal, yVal)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function AddTableValue of class clsUnpavedTBLRecord.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub PopulateFromDatatable(ByRef dt As DataTable)
        Dim i As Long
        Table = New clsSobekTable(Me.setup)
        Table.pdin1 = 1 'block interpolation
        Table.pdin2 = 0 'no return period
        For i = 0 To dt.Rows.Count - 1
            Table.AddDataPair(2, dt.Rows(i)(0), dt.Rows(i)(1))
        Next
    End Sub

    Friend Sub Build()
        Dim i As Integer
        'deze routine bouwt het unpaved.tbl-record op, op basis van de parameterwaarden
        record = "SC_T id '" & ID & "' nm '" & nm & "' PDIN " & Table.pdin1 & " " & Table.pdin2 & " pdin TBLE" & vbCrLf
        For i = 0 To Table.Dates.Count - 1
            record = record & "'" & Table.XValues.Values(i) & " " & Table.Values1.Values(i) & " <" & vbCrLf
        Next
        record = record & " tble sc_t"
    End Sub

    Public Sub Write(ByRef myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub


    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, myTable As String
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
                    table.pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    table.pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "TBLE"
                    Pos = InStr(1, myRecord, "tble")
                    myTable = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, myRecord.Length - Pos - 3)
                    Call Table.Read(myTable)
            End Select
        End While
    End Sub

End Class

