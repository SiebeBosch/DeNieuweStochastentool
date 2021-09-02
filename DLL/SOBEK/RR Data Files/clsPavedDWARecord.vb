Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsPavedDWARecord
    Friend ID As String
    Friend nm As String
    Friend do_ As Double
    Friend wc As Double
    Friend wd As Double
    Friend wh(23) As Double
    Friend sc As Double

    Friend record As String
    Private setup As clsSetup

    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, mydo_ As Double, mywc As Double, mywd As Double, mysc As Double)
        ID = myID
        nm = myName
        do_ = mydo_
        wc = mywc
        wd = mywd
        sc = mysc
    End Sub

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub BuildRecord()
        Dim i As Integer
        record = "DWA id '" & ID & "' nm '" & nm & "' do " & do_ & " wc " & wc & " wd " & wd & " wh"
        For i = 0 To 23
            record = record & " " & wh(i)
        Next
        record = record & " sc " & sc & " dwa"
    End Sub
    Friend Sub Write(ByVal myWriter As StreamWriter)
        Call BuildRecord()
        myWriter.WriteLine(record)
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, i As Integer
        Done = False

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "do"
                    do_ = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "wc"
                    wc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "wd"
                    wd = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "wh"
                    For i = 0 To 23
                        wh(i) = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Next
                Case "sc"
                    sc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            End Select
        End While
    End Sub
End Class
