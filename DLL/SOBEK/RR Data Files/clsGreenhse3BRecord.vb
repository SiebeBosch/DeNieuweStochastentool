Option Explicit On
Imports STOCHLIB.General

Public Class clsGreenhse3BRecord

    Friend ID As String
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend ToNode As String

    Friend na As Integer = 10   'number of areas
    Friend ar As New List(Of Double) 'na number of areas, each associated with a certain storage volume
    Friend as_ As Double        'connected areas storage in m2
    Friend sl As Double    'surface level
    Friend asArea As Double 'area associated with underground (SILO) storage
    Friend si As String     'silo definition
    Friend sd As String     'storage definition on roofs
    Friend ms As String     'meteo station
    Friend aaf As Integer = 1 'area reduction factor
    Friend isConc As Double = 0 'initial salt concentration

    Friend InUse As Boolean
    Friend record As String   'het gehele greenhse.3b-record!

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Sub setMS(myMS As String)
        ms = myMS
    End Sub


    Friend Function getTotalArea() As Double
        Dim Area As Double
        For Each MyArea As Double In ar
            Area += MyArea
        Next
        Return Area
    End Function



    Friend Function Check() As Boolean
        Return True
    End Function

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
        'deze routine bouwt het greenhse.3b-record op, op basis van de parameterwaarden
        Dim i As Integer
        Dim arString As String = ""
        For i = 0 To ar.Count - 1
            arString = arString & ar.Item(i) & " "
        Next

        'foutafhandeling
        If aaf = 0 Then
            aaf = 1
            Me.setup.Log.AddWarning("Area reduction factor of zero detected at greenhouse node " & ID & ". Was replaced by 1")
        End If

        record = "GRHS id '" & ID & "' na 10 ar " & arString & " sl " & sl & " as " & asArea & " si '" & si & "' sd '" & sd & "' ms '" & ms & "' aaf " & aaf & " is " & isConc & " grhs"
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, i As Integer
        Done = False

        'initialize tokens that might be missing
        aaf = 1

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "na"
                    na = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ar"
                    For i = 1 To na
                        ar.Add(Me.setup.GeneralFunctions.ParseString(myRecord, " "))
                    Next
                Case "sl"
                    sl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "as"
                    asArea = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "si"
                    si = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "sd"
                    sd = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ms"
                    ms = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "aaf"
                    aaf = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "is"
                    isConc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            End Select
        End While

    End Sub

End Class
