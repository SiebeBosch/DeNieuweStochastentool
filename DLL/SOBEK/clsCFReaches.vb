Option Explicit On

Imports STOCHLIB.General

Friend Class clsCFReaches

  'Friend Reaches As Dictionary(Of String, clsCFReach)
  'Private setup As clsSetup

  'Friend Sub New(ByRef mySetup As clsSetup)
  '  Me.setup = mySetup
  'End Sub
  'Friend Sub AddNetworkTPBRCHRecord(ByVal myRecord As clsNetworkTPBrchRecord)
  '  On Error Resume Next

  '  Dim myReach As clsCFReach ' Geen New, wordt later gedaan
  '  If Reaches(myRecord.ID.ToUpper.Trim) Is Nothing Then
  '    myReach = New clsCFReach(Me.setup)
  '    myReach.ID = myRecord.ID
  '    myReach.Name = myRecord.Name
  '    myReach.bn = myRecord.bn
  '    myReach.en = myRecord.en
  '    myReach.NetworkTPBrchRecord = myRecord
  '    Call Reaches.Add(myReach.ID.ToUpper.Trim, myReach)
  '  Else
  '    myReach = Reaches(myRecord.ID.ToUpper.Trim)
  '    myReach.ID = myRecord.ID
  '    myReach.Name = myRecord.Name
  '    myReach.bn = myRecord.bn
  '    myReach.en = myRecord.en
  '    myReach.NetworkTPBrchRecord = myRecord
  '  End If
  'End Sub

  'Friend Sub AddFrictionDatBDFRRecord(ByVal myRecord As clsFrictionDatBDFRRecord)
  '  On Error Resume Next

  '  Dim myReach As clsCFReach ' Geen New, wordt later gedaan
  '  If Reaches(myRecord.ci.ToUpper.Trim) Is Nothing Then
  '    myReach = New clsCFReach(Me.setup)
  '    myReach.ID = myRecord.ci
  '    myReach.FrictionType = myRecord.mf
  '    myReach.Friction = myRecord.mtcpConstant
  '    myReach.GLFrictionType = myRecord.sf
  '    myReach.GLFriction = myRecord.stcpConstant
  '    If myReach.GLFriction <= 0 Then
  '      myReach.GLFriction = myReach.Friction
  '      myReach.GLFrictionType = myReach.FrictionType
  '    End If
  '    Call Reaches.Add(myReach.ID.ToUpper.Trim, myReach)
  '  Else
  '    myReach = Reaches(myRecord.ci.ToUpper.Trim)
  '    myReach.ID = myRecord.ci
  '    myReach.FrictionType = myRecord.mf
  '    myReach.Friction = myRecord.mtcpConstant
  '    myReach.GLFrictionType = myRecord.sf
  '    myReach.GLFriction = myRecord.stcpConstant
  '    If myReach.GLFriction <= 0 Then
  '      myReach.GLFriction = myReach.Friction
  '      myReach.GLFrictionType = myReach.FrictionType
  '    End If
  '  End If

  'End Sub

  'Friend Sub AddNetworkCPRecord(ByVal myRecord As clsNetworkCPRecord)
  '  On Error Resume Next

  '  Dim myReach As New clsCFReach(Me.setup)
  '  If Reaches(myRecord.ID.ToUpper.Trim) Is Nothing Then
  '    myReach = New clsCFReach(Me.setup)
  '    myReach.ID = myRecord.ID
  '    myReach.NetworkCpRecord = myRecord
  '    Call Reaches.Add(myReach.ID.ToUpper.Trim, myReach)
  '  Else
  '    myReach = Reaches(myRecord.ID.ToUpper.Trim)
  '    myReach.ID = myRecord.ID
  '    myReach.NetworkCpRecord = myRecord
  '  End If

  'End Sub

  'Friend Function getFrictionStr(ByVal FrictionType As Integer) As String
  '  Select Case FrictionType
  '    Case Is = 0
  '      Return "Chezy"
  '    Case Is = 1
  '      Return "Manning"
  '    Case Is = 2
  '      Return "Strickler Kn"
  '    Case Is = 3
  '      Return "Strickler Ks"
  '    Case Is = 4
  '      Return "White-Colebrook"
  '    Case Is = 7
  '      Return "Bos and Bijkerk"
  '    Case Is = 99
  '      Return "Global Value"
  '  End Select
  '  Return Nothing
  'End Function

  'friend Sub Export(ByVal fricdatfn As Integer, ByVal initdatfn As Integer, ByVal lateraldatfn As Integer, ByVal CFData As clsCFData)
  '  Dim i As Integer, myFrictionType As Integer
  '  Dim myReach As clsCFReach, myTable As clsSobekTable

  '  On Error Resume Next
  '  'globale initiële waarden
  'Print #initdatfn, "GLIN fi 0 fr '(null)' FLIN nm '' ss 0 id '-1' ci '-1' lc 9.9999e+009 q_ lq 0 " & CFData.Settings.InitialdatGLINRecord.q_lqConstant & " 9.9999e+009 ty " & CFData.Settings.InitialdatGLINRecord.ty & " lv ll 0 " & CFData.Settings.InitialdatGLINRecord.lvllConstant & " 9.9999e+009 flin glin"
  '  For Each myReach In CFData.Network.Reaches.Reaches
  '    With myReach
  '    Print #fricdatfn, "BDFR id '" & .ID & "' ci '" & .ID & "' mf " & .FrictionType & " mt cp 0 " & VBA.Replace(.Friction & " 0 mr cp 0 " & .Friction & " 0 s1 6 s2 6 sf " & .GLFrictionType & " st cp 0 " & Round(.GLFriction, 5) & " 0 sr cp 0 " & Round(.GLFriction, 5) & " bdfr", ",", ".")
  '      If myReach.InitialDatRecord.ci <> "" Then
  '      Print #initdatfn, "FLIN nm 'initial' ss 0 id '" & .ID & "' ci '" & .ID & VBA.Replace("' lc 9.9999e+009 q_ lq 0 " & .InitialDatRecord.q_lqConstant & " 9.9999e+009 ty " & .InitialDatRecord.ty & " lv ll 0  " & .InitialDatRecord.lvllConstant & " 9.9999e+009 flin", ",", ".")
  '      End If
  '      If myReach.hasLateral Then
  '        If myReach.DischargeTableID <> "" Then
  '          For Each myTable In CFData.TimeTables
  '            If myTable.ID = myReach.DischargeTableID Then

  '            Print #lateraldatfn, "FLDI id '" & .ID & "' ci '" & .ID & "' sc 0 lt -1 dc lt 1 0 0 PDIN " & myTable.pdin1 & " " & myTable.pdin2 & " " & myTable.PDINPeriod & " pdin"
  '            Print #lateraldatfn, "TBLE"
  '              If myTable.DateValStrings.Count > 0 Then 'mooi zo, ze zijn al beschikbaar als strings voor sobek
  '                For i = 1 To myTable.DateValStrings.Count
  '                Print #lateraldatfn, myTable.DateValStrings(i)
  '                Next
  '              Else
  '                For i = 1 To myTable.XValues.Count
  '                Print #lateraldatfn, "'" & myTable.XValues(i) & " " & myTable.Values1(i) & " <"
  '                Next
  '              End If
  '            Print #lateraldatfn, "tble fldi"
  '              Exit For
  '            End If
  '          Next
  '        Else
  '        Print #lateraldatfn, "FLDI id '" & .ID & "' ci '" & .ID & VBA.Replace("' sc 0 lt -1 dc lt 0 " & .q & " 0 fldi", ",", ".")
  '        End If
  '      End If

  '    End With
  '  Next
  'End Sub

End Class
