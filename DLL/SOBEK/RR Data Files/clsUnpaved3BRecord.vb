Option Explicit On
Imports STOCHLIB.General

Public Class clsUnpaved3BRecord

    Friend ID As String
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend ToNode As String

    'werkelijke .3B-content
    Friend na As Integer
    Friend aaf As Double
    Friend ar As New Dictionary(Of Integer, Double)
    Friend ga As Double
    Friend lv As Double
    Friend co As enmCo
    Friend rc As Double       'reservoir coef krayenhof
    Friend su As Integer      '0 = no scurve, 1 = scurve
    Friend SCurve As String   'id'
    Friend sd As String       'storage id
    Friend ad As String       'alpha id
    Friend ed As String       'ernst def ID
    Friend SP As String       'seep def
    Friend ic As String       'infiltration def
    Friend bt As Integer      'soil type
    Friend ig As Integer      '0 const, 1 table
    Friend igconst As Double
    Friend igtable As String
    Friend mg As Double       'max groundwater level
    Friend gl As Double       'depth groundwater layer
    Friend ms As String       'meteo station

    Friend InUse As Boolean
    Friend record As String   'het gehele unpaved.3b-record!

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        Dim i As Integer
        For i = 1 To 16
            If Not ar.ContainsKey(i) Then ar.Add(i, 0)
        Next
    End Sub

    Public Function getlv() As Double
        Return lv
    End Function

    Public Sub setMS(myMS As String)
        ms = myMS
    End Sub

    Public Sub setigConst(myIG As Double)
        igconst = myIG
    End Sub

    Public Sub setigAsVariable(TableID As String)
        ig = 1
        igtable = TableID
    End Sub

    Public Function getigTableID() As String
        Return igtable
    End Function

    Public Function getTBLID() As String
        Return igtable
    End Function

    Friend Enum enmCo
        Hellinga = 1
        Krayenhoff = 2
        ernst = 3
    End Enum
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

    Friend Function getSurfaceLevel() As Double
        If su = 0 Then
            Return lv
        ElseIf su = 1 Then
            Me.setup.Log.AddWarning("Functionality variable surface level unpaved not (yet) supported")
            Return -999
        End If
    End Function

    Friend Function getTotalLandUseArea() As Double
        'returns the sum of all land use for this node (NOT the groundwater area!)
        Dim Area As Double
        For Each myArea As Double In ar.Values
            Area += myArea
        Next
        Return Area
    End Function

    Friend Sub Build()
        'deze routine bouwt het unpaved.3b-record op, op basis van de parameterwaarden
        Dim i As Integer
        Dim arString As String = ""
        For i = 1 To 16
            arString = arString & ar(i) & " "
        Next

        'foutafhandeling
        If aaf = 0 Then
            aaf = 1
            Me.setup.Log.AddWarning("Area reduction factor of zero detected. Was replaced by 1")
        End If

        record = "UNPV id '" & ID & "' na " & na & " ar " & arString & "ga " & ga & " lv " & Format(lv, "0.###") & " co " & co
        If su = 1 Then
            record &= " su " & su & " '" & SCurve & "'"
        Else
            record &= " su " & su & " ''"
        End If

        If ig = 0 Then
            record &= " sd '" & sd & "' ad '" & ad & "' ed '" & ed & "' sp '" & SP & "' ic '" & ic & "' bt " & bt & " ig " & ig & " " & igconst
        Else
            record &= " sd '" & sd & "' ad '" & ad & "' ed '" & ed & "' sp '" & SP & "' ic '" & ic & "' bt " & bt & " ig " & ig & " '" & igtable & "'"
        End If
        record &= " mg " & mg & " gl " & gl & " ms '" & ms & "' is 0 aaf " & aaf & " unpv"

    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub

    Friend Function Read(ByVal myRecord As String) As Boolean
        Dim Done As Boolean, myStr As String, i As Integer
        Done = False
        Try
            'initialize tokens that might be missing
            mg = -999
            aaf = 1

            While Not myRecord = ""
                myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Select Case LCase(myStr)
                    Case "id"
                        ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "na"
                        na = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ar"
                        For i = 1 To 16
                            Dim myar As Double
                            myar = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                            If Not ar.ContainsKey(i) Then
                                ar.Add(i, myar)
                            Else
                                ar.Item(i) += myar
                            End If
                        Next
                    Case "ga"
                        ga = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lv"
                        lv = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "co"
                        i = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        If i = 1 Then
                            co = enmCo.Hellinga
                        ElseIf i = 2 Then
                            co = enmCo.Krayenhoff
                        ElseIf i = 3 Then
                            co = enmCo.ernst
                        End If
                    Case "rc"
                        rc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "su"
                        su = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        If su = 1 Then SCurve = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "sd"
                        sd = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ad"
                        ad = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ed"
                        ed = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "sp"
                        SP = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ic"
                        ic = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "bt"
                        bt = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ig"
                        ig = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        If ig = 0 Then
                            igconst = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        ElseIf ig = 1 Then
                            igtable = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        End If
                    Case "mg"
                        mg = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "aaf"
                        aaf = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "gl"
                        gl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ms"
                        ms = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                End Select
            End While

            'fill in for missing tokens
            If mg = -999 Then mg = lv
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error while reading unpaved.3b record " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try



    End Function

    Friend Function Check() As Boolean
        Dim uPar As Double
        Dim j As Long

        Try
            If ar.Count < 16 Then
                Me.setup.Log.AddWarning("Unpaved node " & ID & ". has no landuse area values and will be skipped during import.")
                Return True
            End If

            'basischeck
            uPar = 0
            For j = 1 To 16
                uPar = uPar + ar.Item(j)
            Next

            If uPar <= 0 Then
                Me.setup.Log.AddWarning("Unpaved node " & ID & " has an area equal to or smaller than zero and will be skipped during import.")
                Return True
            End If

            If X = 0 And Y = 0 Then
                Me.setup.Log.AddWarning("Unpaved node " & ID & " has no coordinates and will therefore be skipped during import.")
                Return True
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function
End Class
