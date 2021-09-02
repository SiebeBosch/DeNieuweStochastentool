Imports STOCHLIB.General

Public Class clsOpenwate3BRecord

    Public InUse As Boolean

    Public ID As String        'openwater node id
    Friend X As Double
    Friend Y As Double
    Friend FromNode As String
    Friend ToNode As String

    Public na As Integer       'number of area-relations
    Public ar As New List(Of Double)    'areas
    Public lv As New List(Of Double)    'levels
    Public rl As Double       'reference level
    Public al As Integer       'area level relation (1=constant, 2 = interpolation, 3 = llinear
    Public ml As Double       'maximum allowed level
    Public bl As Double     'bed level
    Public aaf As Double        'area reduction factor
    Public sp As String         'seepage definition
    Public ms As String         'meteo station
    Public isalt As Double    'initial salt
    Public tlopt As Integer    'target level option (0=constant, 1=table)
    Public tltable As String   'target level table id


    Friend record As String   'het gehele openwate.3b-record!

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub


    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String, i As Integer
        '------------------------------------------------------
        'hier parsen we een record uit openwate.3b
        'en vullen een instantie uit de klasse openwate3brecord
        '------------------------------------------------------
        aaf = 1

        While Not myRecord = ""
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    id = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "na"
                    na = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ar"
                    For i = 1 To na
                        ar.Add(Me.Setup.GeneralFunctions.ParseString(myRecord, " "))
                    Next
                Case "lv"
                    For i = 1 To na
                        lv.Add(Me.Setup.GeneralFunctions.ParseString(myRecord, " "))
                    Next
                Case "rl"
                    rl = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "al"
                    al = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ml"
                    ml = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "bl"
                    bl = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "tl"
                    tlopt = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tlopt = 1 Then
                        tltable = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case "sp"
                    sp = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ms"
                    ms = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "aaf"
                    aaf = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "is"
                    isalt = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            End Select
        End While

    End Sub

    Friend Sub Build()
        'deze routine bouwt het openwate.3b-record op, op basis van de parameterwaarden
        Dim i As Integer
        Dim arString As String = ""
        Dim lvString As String = ""
        For i = 0 To ar.Count - 1
            arString = arString & ar.Item(i) & " "
            lvString = lvString & lv.Item(i) & " "
        Next

        'foutafhandeling
        If aaf = 0 Then
            aaf = 1
            Me.Setup.Log.AddWarning("Area reduction factor of zero detected at greenhouse node " & id & ". Was replaced by 1")
        End If

        record = "OPWA id '" & id & "' na 6 ar " & arString & " lv " & lvString & " rl " & rl & " al " & al & " ml " & ml & " bl " & bl & " tl" & tlopt & "'" & tltable & "'" & " sp " & "'" & sp & "'" & " ms " & "'" & ms & "'" & " aaf " & aaf & " is " & isalt & " opwa"
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub

    Friend Function Check() As Boolean
        Return True
    End Function

    Friend Sub GetTopology(ByRef SbkCase As ClsSobekCase)
        For Each myLink As clsRRBrchTPRecord In SbkCase.RRTopo.Links.Values
            If myLink.bn = id Then
                Me.ToNode = myLink.en
                Exit For
            End If
            If myLink.en = ID Then
                Me.FromNode = myLink.bn
            End If
        Next myLink

        For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values
            If myNode.ID = id Then
                Me.X = myNode.X
                Me.Y = myNode.Y
            End If
        Next myNode
    End Sub

End Class
