Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsGreenHseRFRecord
    Friend ID As String
    Friend nm As String
    Friend mk As Double
    Friend ik As Double

    Friend record As String
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, mymk As Double, myik As Double)
        ID = myID
        nm = myName
        mk = mymk
        ik = myik
    End Sub

    Friend Sub BuildRecord()
        record = "STDF id '" & ID & "' nm '" & nm & "' mk " & mk & " ik " & ik & " stdf"
    End Sub
    Friend Sub Write(ByVal myWriter As StreamWriter)
        Call BuildRecord()
        myWriter.WriteLine(record)
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String
        Done = False

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "mk"
                    mk = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ik"
                    ik = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            End Select
        End While
    End Sub

End Class
