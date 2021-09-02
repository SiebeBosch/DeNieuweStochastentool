Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsStructDefRecord

    Friend ID As String
    Friend nm As String
    Friend ty As Integer
    Friend tc As Integer     'culvert type
    Friend tb As GeneralFunctions.enmCFBridgeType     'bridge type 2=pillar, 3=abutment, 4=fixed bed, 5=soil bed
    Friend pw As Double   'pillar width
    Friend dn As Integer     'control direction
    Friend pu As Double
    Friend rt As GeneralFunctions.enmFlowDirection
    Friend ll As Double   'invert level left
    Friend rl As Double   'invert level right
    Friend dl As Double   'length
    Friend li As Double     'inlet loss
    Friend lo As Double     'outlet loss
    Friend lb As Double     'bend loss siphons
    Friend hs As Double     'on level inverted siphons
    Friend he As Double     'off level inverted siphons
    Friend ov As Double     'initial opening of valve
    Friend cl As Double     'crest level
    Friend cw As Double     'crest width
    Friend gh As Double     'gate height
    Friend ce As Double     'discharge coef
    Friend Mu As Double     'contraction coef
    Friend tv As Integer
    Friend vf As Double     'bridge shape factor
    Friend sv As Double     'for universal weirs
    Friend si As String     'cross section definition ID for universal weir
    Friend sc As Double     'lateral contraction coef
    Friend mpUsed As Integer   'use max flow positive direction 0=no 1=yes
    Friend mnUsed As Integer   'use max flow negative direction 0=no 1=yes
    Friend mp As Double     'max flow positive
    Friend mn As Double     'max flow negative

    Friend valvetable As String
    Friend ctlt As Integer
    Friend rtcr1 As Integer, rtcr2 As Integer, rtcr3 As Integer  'reduction factor. rt cr 0 = constant

    Friend ctltTable As clsSobekTable 'pumptable
    Friend rtcrTable As clsSobekTable 'reduction table for pumps

    Friend InUse As Boolean

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        'Init classes:
        ctltTable = New clsSobekTable(Me.setup) 'pumptable
        rtcrTable = New clsSobekTable(Me.setup) 'reduction table for pumps
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim activeToken As String = ""
        Dim Pos As Integer, Table As String
        Dim myStr As String, tmp As String

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "ct"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "lt" Then
                        activeToken = "ctlt"
                    End If
                Case Is = "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ty"
                    ty = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "tc"
                    tc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "tb"
                    tb = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "dn"
                    dn = Me.setup.GeneralFunctions.ParseString(myRecord, " ") 'control direction
                Case Is = "pu"
                    pu = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "pw"
                    pw = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "sv"
                    sv = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "si"
                    si = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "rt"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If IsNumeric(tmp) Then
                        rt = Val(tmp)
                    ElseIf tmp = "cr" Then
                        activeToken = "rtcr"
                    End If
                Case Is = "ll"
                    ll = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "rl"
                    rl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "dl"
                    dl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "si"
                    si = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "li"
                    li = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "lo"
                    lo = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "lb"
                    lb = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "hs"
                    hs = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "he"
                    he = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ov"
                    ov = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "tv"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If IsNumeric(tmp) Then
                        tv = Val(tmp)
                        If tv = 1 Then 'valve
                            valvetable = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        ElseIf tv = 0 Then
                            'no valve; do nothing
                        End If
                    End If
                Case Is = "cl"
                    cl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "gh"
                    gh = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "cw"
                    cw = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ce"
                    ce = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mu"
                    Mu = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mp"
                    mpUsed = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    mp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mn"
                    mnUsed = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    mn = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "vf"
                    vf = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "sc"
                    sc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "TBLE"
                    Pos = InStr(1, myRecord, "tble")
                    Table = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, Len(myRecord) - Pos - 4 + 1)

                    If activeToken = "ctlt" Then
                        Call ctltTable.Read(Table)
                        ctltTable.ID = ID
                    ElseIf activeToken = "rtcr" Then
                        Call rtcrTable.Read(Table)
                        rtcrTable.ID = ID
                    End If

                Case Is = "0"
                    If activeToken = "rtcr" Then
                        rtcr1 = 0
                        rtcr2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        rtcr3 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    ElseIf activeToken = "ctlt" Then
                        ctlt = 0
                    End If
                Case Is = "1"
                    If activeToken = "ctlt" Then
                        ctlt = 1 'hierna volgt een tabel
                    End If
                Case Is = "2"
                    If activeToken = "rtcr" Then 'pompreductietabel'
                        rtcr1 = 2
                    End If
                Case "stds"
                Case "STDS"
                Case ""
                Case Else
                    'doe niks
            End Select
        End While

        InUse = True

    End Sub

    Friend Sub Write(ByVal myWriter As StreamWriter)
        Dim i As Integer

        Try
            Select Case ty
                Case Is = 6 'weir
                    myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " cl " & Format(cl, "0.###") & " cw " & cw & " ce " & ce & " sc " & sc & " rt " & rt & " stds")
                Case Is = 7 'orifice
                    myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " cl " & Format(cl, "0.###") & " cw " & cw & " gh " & gh & " mu " & Mu & " sc " & sc & " rt " & rt & " mp " & mpUsed & " " & Format(mp, "0.###") & " mn " & mnUsed & " " & Format(mn, "0.###") & " stds")
                Case Is = 9 'pump
                    If rtcr1 = 2 And ctlt = 1 Then
                        myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " dn " & dn & " pu " & pu & " rt cr " & rtcr1)
                        myWriter.WriteLine("TBLE")
                        For i = 0 To rtcrTable.XValues.Count - 1
                            myWriter.WriteLine(rtcrTable.XValues.Values(i) & " " & rtcrTable.Values1.Values(i) & " <")
                        Next
                        myWriter.WriteLine("tble ct lt " & ctlt)
                        myWriter.WriteLine("TBLE")
                        For i = 0 To ctltTable.XValues.Count - 1
                            myWriter.WriteLine(ctltTable.XValues.Values(i) & " " & ctltTable.Values1.Values(i) & " " & ctltTable.Values2.Values(i) & " " & ctltTable.Values3.Values(i) & " " & ctltTable.Values4.Values(i) & " <")
                        Next
                        myWriter.WriteLine("tble stds")
                    ElseIf rtcr1 = 2 Then
                        myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " dn " & dn & " pu " & pu & " rt cr")
                        myWriter.WriteLine("TBLE")
                        For i = 0 To rtcrTable.XValues.Count - 1
                            myWriter.WriteLine(rtcrTable.XValues.Values(i) & " " & rtcrTable.Values1.Values(i) & " <")
                        Next
                        myWriter.WriteLine("tble ct lt " & ctlt & " stds")
                    ElseIf ctlt = 1 Then
                        myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " dn " & dn & " pu " & pu & " rt cr " & rtcr1 & " " & rtcr2 & " " & rtcr3 & " ct lt 1")
                        myWriter.WriteLine("TBLE")
                        For i = 0 To ctltTable.XValues.Count - 1
                            myWriter.WriteLine(ctltTable.XValues.Values(i) & " " & ctltTable.Values1.Values(i) & " " & ctltTable.Values2.Values(i) & " " & ctltTable.Values3.Values(i) & " " & ctltTable.Values4.Values(i) & " <")
                        Next
                        myWriter.WriteLine("tble stds")
                    Else
                        myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " dn " & dn & " pu " & pu & " rt cr")
                    End If
                Case Is = 10 'culvert
                    If tv = 1 Then
                        myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " tc " & tc & " dl " & dl & " ll " & Format(ll, "0.###") & " rl " & Format(rl, "0.###") & " si '" & si & "' li " & li & " lo " & lo & " lb " & lb & " tv " & tv & " '" & valvetable & "' ov " & ov & " rt " & rt & " hs " & hs & " he " & he & " stds")
                    Else
                        myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " tc " & tc & " dl " & dl & " ll " & Format(ll, "0.###") & " rl " & Format(rl, "0.###") & " si '" & si & "' li " & li & " lo " & lo & " lb " & lb & " tv " & tv & " ov " & ov & " rt " & rt & " hs " & hs & " he " & he & " stds")
                    End If
                Case Is = 11 'universal weir
                    myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " cl " & Format(cl, "0.###") & " si '" & si & "' ce " & ce & " sv " & sv & " rt " & rt & " stds")
                Case Is = 12 'bridge
                    myWriter.WriteLine("STDS id '" & ID & "' nm '" & nm & "' ty " & ty & " tb " & tb & " si '" & si & "' pw " & pw & " vf " & vf & " li " & li & " lo " & lo & " dl " & dl & " rl " & Format(rl, "0.###") & " stds")
            End Select
        Catch ex As Exception
            Me.setup.Log.AddError("Error writing struct.def record " & ID)
        End Try


    End Sub

End Class
