Imports STOCHLIB.General

Public Class clsValveTabVLVERecord

    Public ID As String 'ID
    Public nm As String
    Public ltlcTable As clsSobekTable
    Public InUse As Boolean

    Private record As String
    Private Setup As clsSetup

    Public Sub New(ByVal mySetup As clsSetup)
        Me.Setup = mySetup
        ltlcTable = New clsSobekTable(Me.Setup)
    End Sub

    Friend Sub build()
        record = "VLVE id '" & ID & "' nm '" & ID & "' lt lc" & vbCrLf
        record &= "TBLE" & vbCrLf
        For i = 0 To ltlcTable.XValues.Count - 1
            record &= ltlcTable.XValues.Values(i) & " " & ltlcTable.Values1.Values(i) & " <" & vbCrLf
        Next
        record &= "tble" & vbCrLf
        record &= " vlve"
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String, tmpStr As String
        Dim Pos As Integer
        record = myRecord

        While Not myRecord = ""
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "nm"
                    nm = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "TBLE"
                    Pos = InStr(1, myRecord, "tble")
                    tmpStr = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, Len(myRecord) - Pos - 4 + 1)
                    Call ltlcTable.Read(tmpStr)
            End Select
        End While

    End Sub

    Friend Sub write(ByRef datWriter As System.IO.StreamWriter)
        Call build()
        datWriter.WriteLine(record)
    End Sub
End Class
