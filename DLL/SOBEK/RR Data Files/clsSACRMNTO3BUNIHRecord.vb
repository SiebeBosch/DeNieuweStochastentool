Imports STOCHLIB.General

Public Class clsSACRMNTO3BUNIHRecord

    Friend ID As String
    Friend Name As String

    'werkelijke .3B-content
    Friend dt As Integer
    Friend uh As New List(Of Double)

    Friend InUse As Boolean
    Friend record As String   'het gehele record

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub


    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, mydt As Integer, myuh As List(Of Double))
        ID = myID
        Name = myName
        dt = mydt
        uh = myuh
        InUse = True
    End Sub

    Friend Sub Build()
        'deze routine bouwt het unpaved.3b-record op, op basis van de parameterwaarden
        record = "UNIH id '" & ID & "' nm '" & Name & "' dt " & dt & " uh"
        For Each item In uh
            record &= " " & item
        Next
        record &= " unih"
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub

    Friend Function Read(ByVal myRecord As String) As Boolean
        Dim Done As Boolean, myStr As String, tmpstr As String
        Done = False
        Try
            'initialize tokens that might be missing

            While Not myRecord = ""
                myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Select Case LCase(myStr)
                    Case "id"
                        ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "nm"
                        Name = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "dt"
                        dt = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "uh"
                        While Not myRecord = ""
                            tmpstr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                            If IsNumeric(tmpstr) Then uh.Add(tmpstr)
                        End While
                End Select
            End While

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error while reading sacrmnto3b UNIH record " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
End Class
