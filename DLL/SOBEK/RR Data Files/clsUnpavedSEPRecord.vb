Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpavedSEPRecord
    Friend ID As String
    Friend nm As String
    Friend co As Double
    Friend sp As Double
    Friend ss As Double
    Friend cv As Double

    Friend TimeTable As clsSobekTable

    Friend record As String
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        TimeTable = New clsSobekTable(Me.setup)
    End Sub

    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, myco As Double, mycv As Double, mysp As Double, myss As Double)
        ID = myID
        nm = myName
        co = myco
        sp = mysp
        ss = myss
        cv = mycv
    End Sub

    Friend Function BuildRecord() As Boolean
        Dim i As Integer

        Try
            If co = 0 Then co = 1 'foutcorrectie want 0 bestaat niet
            If co = 1 Then
                record = "SEEP id '" & ID & "' nm '" & nm & "' co " & co & " sp " & Replace(Math.Round(sp, 3), ",", ".") & " ss " & ss & " cv " & cv & " seep"
            ElseIf co = 4 Then
                record = "SEEP id '" & ID & "' nm '" & nm & "' co " & co & " PDIN " & TimeTable.pdin1 & " " & TimeTable.pdin2 & " " & TimeTable.PDINPeriod & " pdin ss " & ss & vbCrLf
                record = record & "TBLE" & vbCrLf
                For i = 0 To TimeTable.Dates.Count - 1
                    record = record & "'" & Format(TimeTable.Dates.Values(i), "yyyy/MM/dd;hh:mm:ss") & "' " & Format(TimeTable.Values1.Values(i), "0.00") & " <" & vbCrLf
                Next
                record &= "tble seep"
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function BuildRecord of class clsUnpavedSEPRecord.")
            Return False
        End Try
    End Function
    Friend Sub Write(ByVal myWriter As StreamWriter)
        Call BuildRecord()
        myWriter.WriteLine(record)
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String
        Dim Pos As Long, table As String
        Done = False

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr.Trim
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "co"
                    co = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "sp"
                    sp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ss"
                    ss = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "cv"
                    cv = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "PDIN"
                    TimeTable.pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    TimeTable.pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If TimeTable.pdin2 = 1 Then
                        TimeTable.PDINPeriod = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case "TBLE"
                    Pos = InStr(1, myRecord, "tble")
                    table = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, myRecord.Length - Pos - 3)
                    Call TimeTable.ReadFast(table)
            End Select
        End While
    End Sub
End Class
