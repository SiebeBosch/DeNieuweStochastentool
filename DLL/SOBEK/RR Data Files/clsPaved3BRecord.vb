Option Explicit On

Imports STOCHLIB.General

Public Class clsPaved3BRecord

    Friend ID As String
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend Tonode1 As String
    Friend ToNode2 As String

    Friend InUse As Boolean

    Friend GPGID As String
    Friend GFEID As String

    'werkelijke 3B-records
    Friend ar As Double
    Friend lv As Double
    Friend sd As String
    Friend ss As enmSystemType
    Friend qc As Integer
    Friend MixedCap As Double
    Friend DWFCap As Double
    Friend PumpCapTable As String
    Friend qo1 As Integer   'dwf pump to
    Friend qo2 As Integer   'mixed pump to
    Friend ms As String
    Friend is0 As Double
    Friend np As Integer
    Friend dw As String
    Friend ro As Integer
    Friend ru As Integer
    Friend qh As String

    Friend record As String
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub
    Friend Enum enmSystemType
        Mixed = 0
        Separated = 1
        ImprovedSeparated = 2
    End Enum

    Public Sub setMS(myMS As String)
        ms = myMS
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String
        Done = False

        While Not Done
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr.ToLower
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ar"
                    ar = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "lv"
                    lv = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "sd"
                    sd = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ss"
                    ss = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "qc"
                    qc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If qc = 0 Then
                        MixedCap = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        DWFCap = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Else
                        PumpCapTable = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case "qo"
                    qo1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    qo2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ms"
                    ms = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "is"
                    is0 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "np"
                    np = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "dw"
                    dw = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ro"
                    ro = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ru"
                    ru = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "qh"
                    qh = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case ""
                    Done = True
            End Select
        End While
    End Sub

    Friend Sub GetTopology(ByRef SbkCase As clsSobekCase)
        For Each myLink As clsRRBrchTPRecord In SbkCase.RRTopo.Links.Values
            If myLink.bn = ID Then
                'ga op zoek naar de xy-coordinaten en de eindknopen hiervan
                For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values
                    If myNode.ID = ID Then
                        X = myNode.X
                        Y = myNode.Y
                    ElseIf myNode.ID = myLink.en Then 'we hebben een knoop gevonden waar deze paved op uitslaat of overstort
                        If Tonode1 = "" Then
                            Tonode1 = myNode.ID
                        Else
                            ToNode2 = myNode.ID
                        End If
                    End If
                Next myNode
            End If
        Next myLink
    End Sub

    Friend Sub Build()
        'deze routine bouwt het paved.3b-record op, op basis van de parameterwaarden
        record = "PAVE id '" & ID & "' ar " & ar & " lv " & Format(lv, "0.###") & " sd '" & sd & "' ss " & ss & " qc " & qc & " " & MixedCap & " " & DWFCap & " qo " & qo1 & " " & qo2 & " ms '" & ms & "' aaf 1 is " & is0 & " np " & np & " dw '" & dw & "' ro " & ro & " ru " & ru & " qh '" & qh & "' pave"
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub
    Friend Function Check() As Boolean

        If ar <= 0 Then
            Me.setup.Log.AddWarning("Paved node " & ID & " has an area equal to or smaller than zero and will be skipped during import.")
            Return True
        End If

        If X = 0 And Y = 0 Then
            Me.setup.Log.AddWarning("Paved node " & ID & " has no coordinates and will therefore be skipped during import.")
            Return True
        End If

        Return True
    End Function
End Class
