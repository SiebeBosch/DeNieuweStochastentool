Option Explicit On

Imports STOCHLIB.General

Public Class clsWWTP3BRecord
    Friend ID As String
    Friend tb As Integer
    Friend TableID As String
    Friend record As String
    Friend InUse As Boolean

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub Build(Optional ByVal setInuse As Boolean = True)
        'deze routine bouwt het wwtp.3b-record op, op basis van de parameterwaarden
        If tb = 0 Then
            record = "WWTP id '" & ID & "' tb " & tb & " wwtp"
        Else
            record = "BOUN id '" & ID & "' tb " & tb & " '" & TableID & "' wwtp"
        End If
        InUse = setInuse
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub


    Friend Sub Read(ByVal myRecord As String, Optional ByVal SetInuse As Boolean = True)
        Dim Done As Boolean, myStr As String
        Done = False
        InUse = SetInuse

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "tb"
                    tb = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tb = 1 Then
                        TableID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
            End Select
        End While
    End Sub

End Class
