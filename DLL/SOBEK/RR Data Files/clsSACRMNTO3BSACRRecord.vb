Imports STOCHLIB.General

Public Class clsSACRMNTO3BSACRRecord

    Friend ID As String
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend ToNode As String

    'werkelijke .3B-content
    Friend ar As Double
    Friend ca As String
    Friend uh As String
    Friend op As String
    Friend ms As String
    Friend aaf As Double

    Friend InUse As Boolean
    Friend record As String   'het gehele unpaved.3b-record!

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub GetTopology(ByRef SbkCase As clsSobekCase)
        For Each myLink As clsRRBrchTPRecord In SbkCase.RRTopo.Links.Values
            If myLink.bn = ID Then
                Me.ToNode = myLink.en
                Exit For
            End If
        Next myLink

        For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values
            If myNode.ID = ID Then
                Me.X = myNode.X
                Me.Y = myNode.Y
            End If
        Next myNode
    End Sub

    Friend Sub Build()
        'deze routine bouwt het unpaved.3b-record op, op basis van de parameterwaarden

        'foutafhandeling
        If aaf = 0 Then
            aaf = 1
            Me.setup.Log.AddWarning("Area reduction factor of zero detected. Was replaced by 1")
        End If

        'build
        record = "SACR id '" & ID & "' nm '" & Name & "' ar " & ar & " ca '" & ca & "' uh '" & uh & "' op '" & op & "' ms '" & ms & "' aaf " & aaf & " sacr"

    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub

    Friend Function Read(ByVal myRecord As String) As Boolean
        Dim Done As Boolean, myStr As String
        Done = False
        Try
            'initialize tokens that might be missing
            aaf = 1

            While Not myRecord = ""
                myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Select Case LCase(myStr)
                    Case "id"
                        ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "nm"
                        Name = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ar"
                        ar = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ca"
                        ca = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "uh"
                        uh = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "op"
                        op = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ms"
                        ms = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "aaf"
                        aaf = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                End Select
            End While

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error while reading sacrmnto.3b SACR record " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

End Class
