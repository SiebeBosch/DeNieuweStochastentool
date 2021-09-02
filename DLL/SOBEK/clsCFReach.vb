Option Explicit On

Imports STOCHLIB.General

Friend Class clsCFReach

  'Friend ID As String                     'ID van de tak
  'Friend Name As String                   'naam van de tak
  'Friend bn As String
  'Friend en As String

  'Friend fNode As clsSbkReachNode          'from node
  'Friend tNode As clsSbkReachNode          'to-node
  'Friend Length As Double                 'Lengte van de tak
  'Friend InUse As Boolean

  'Friend Enum enmFrictionType
  '  Chezy = 0
  '  Manning = 1
  '  Stricklerkn = 2
  '  Stricklerks = 3
  '  WhiteColebrook = 4
  '  BosBijkerk = 7
  '  GlobalFriction = 99
  'End Enum

  'Friend FrictionType As enmFrictionType
  'Friend Friction As Double
  'Friend GLFrictionType As enmFrictionType
  'Friend GLFriction As Double
  'Friend hasLateral As Boolean            'of het een tak van het type reach with lateral flow is
  'Friend q As Double                     'afvoer op een tak van het type lateral
  'Friend DischargeTableID As String       'verwijzing naar een tabel met specifieke afvoer (m2/s)

  'Friend NetworkTPBrchRecord As clsNetworkTPBrchRecord
  'Friend NetworkCpRecord As clsNetworkCPRecord
  'Friend InitialDatRecord As clsInitialDatRecord
  'Private setup As clsSetup

  'Friend Sub New(ByRef mySetup As clsSetup)
  '  Me.setup = mySetup

  '  'Init classes:
  '  NetworkTPBrchRecord = New clsNetworkTPBrchRecord(Me.setup)
  '  NetworkCpRecord = New clsNetworkCPRecord(Me.setup, Me)
  '  InitialDatRecord = New clsInitialDatRecord(Me.setup)
  'End Sub

  'Friend Sub SetReachNodes(ByRef myTopo As clsCFTopology)
  '  Dim bnStr As String, enStr As String

  '  bnStr = NetworkTPBrchRecord.bn.ToUpper.Trim
  '  enStr = NetworkTPBrchRecord.en.ToUpper.Trim

  '  If myTopo.ReachNodes.ReachNodes(NetworkTPBrchRecord.bn) Is Nothing Then
  '    Dim log As String = "Error in sub setReachNodes of class clsReach. Begin node for reach " & ID & " not found."
  '    Me.setup.Log.AddError(log)
  '    Throw New Exception(log)
  '  End If

  '  fNode = myTopo.ReachNodes.ReachNodes(bnStr.Trim.ToUpper)

  '  If myTopo.ReachNodes.ReachNodes(NetworkTPBrchRecord.en) Is Nothing Then
  '    Dim log As String = "Error in sub setReachNodes of class clsReach. End node for reach " & ID & " not found"
  '    Me.setup.Log.AddError(log)
  '    Throw New Exception(log)
  '  End If

  '  tNode = myTopo.ReachNodes.ReachNodes(enStr)

  'End Sub

  'Friend Function IsDeadEnd(ByRef myReaches As clsCFReaches, ByVal IgnoreDisabledReaches As Boolean, ByVal KeepBoundaries As Boolean) As Boolean
  '  'checkt of een tak doodlopend is
  '  'de optie SkipDisabledReaches is bedoeld om rekening te houden met takken die in een eerdere loop al op inactief waren gezet
  '  'als de waarde true is, worden dergelijke takken niet meegenomen als een verbinding

  '  If KeepBoundaries = True And (fNode.nt = clsCFReachNode.enmNodetype.NodeCFBoundary Or tNode.nt = clsCFReachNode.enmNodetype.NodeCFBoundary) Then
  '    IsDeadEnd = False
  '  ElseIf CountConnectedReaches(myReaches, True) > 1 Then
  '    'deze tak kunnen we (nog) niet weggooien omdat hij nog op ten minste twee plaatsen aan andere, actieve, takken zit
  '    IsDeadEnd = False
  '  Else
  '    IsDeadEnd = True
  '  End If

  'End Function

  'Friend Function CountConnectedReaches(ByRef myReaches As clsCFReaches, ByVal IgnoreDisabledReaches As Boolean) As Integer
  '  'bekijkt of er andere takken verbonden zijn aan een gegeven reach
  '  Dim nConnected As Integer = 0
  '  Dim bnDone As Boolean = False
  '  Dim enDone As Boolean = False

  '  'Me.setup.Log.AddDebugMessage("In CountConnectedReaches of clsCFReach")

  '  'zoek eerst uit of aan de beginknoop van onze tak een andere tak gekoppeld is (op normale wijze)
  '  For Each myReach As clsCFReach In myReaches.Reaches.Values
  '    If myReach.ID <> ID Then 'sluit zichzelf als kandidaat uit
  '      'check of deze tak op enigerlei wijze aan de BEGINknoop van de onderhavige tak gekoppeld is
  '      If myReach.fNode.ID = fNode.ID Or myReach.tNode.ID = tNode.ID Then
  '        'we hebben een verbinding gevonden. Nu nog checken of de gevonden tak niet al is uitgeschakeld
  '        If IgnoreDisabledReaches = True And myReach.InUse = False Then
  '          'geen officiele verbinding want deze mogen we overslaan
  '          Me.setup.Log.AddDebugMessage("Geen officiele verbinding")
  '        Else
  '          nConnected = nConnected + 1
  '          bnDone = True
  '          Exit For
  '        End If
  '      End If
  '    End If
  '  Next

  '  'zoek dan uit of aan de eindknoop van onze tak een andere tak gekoppeld is (op normale wijze)
  '  For Each myReach As clsCFReach In myReaches.Reaches.Values
  '    If myReach.ID <> ID Then 'sluit zichzelf als kandidaat uit
  '      'check of deze tak op enigerlei wijze aan de EINDknoop van de onderhavige tak gekoppeld is
  '      If myReach.fNode.ID = tNode.ID Or myReach.tNode.ID = tNode.ID Then
  '        'we hebben een verbinding gevonden. Nu nog checken of de gevonden tak niet al is uitgeschakeld
  '        If IgnoreDisabledReaches = True And myReach.InUse = False Then
  '          'geen officiele verbinding want deze mogen we overslaan
  '          Me.setup.Log.AddDebugMessage("Geen officiele verbinding")
  '        Else
  '          nConnected = nConnected + 1
  '          enDone = True
  '          Exit For
  '        End If
  '      End If
  '    End If
  '  Next

  '  'als de beginknoop van het type linkage is, zoek dan uit of de tak waar hij aan vast zit actief is
  '  If Not bnDone Then 'het kan voorkomen dat de beginknoop zowel een linkage is als gekoppeld aan andere takken
  '    If fNode.nt = clsCFReachNode.enmNodetype.NodeCFLinkageNode Then
  '      Dim myReach As clsCFReach = myReaches.Reaches(fNode.ci)
  '      If IgnoreDisabledReaches = True And myReach.InUse = False Then
  '        'geen officiele verbinding want deze mogen we overslaan
  '        Me.setup.Log.AddDebugMessage("Geen officiele verbinding")
  '      Else
  '        nConnected = nConnected + 1
  '      End If
  '    End If
  '  End If

  '  'als de eindknoop van het type linkage is, zoek dan uit of de tak waar hij aan vast zit actief is
  '  If Not enDone Then 'het kan voorkomen dat de eindknoop zowel een linkage is als gekoppeld aan andere takken
  '    If tNode.nt = clsCFReachNode.enmNodetype.NodeCFLinkageNode Then
  '      Dim myReach As clsCFReach = myReaches.Reaches(tNode.ci)
  '      If IgnoreDisabledReaches = True And myReach.InUse = False Then
  '        'geen officiele verbinding want deze mogen we overslaan
  '        Me.setup.Log.AddDebugMessage("Geen officiele verbinding")
  '      Else
  '        nConnected = nConnected + 1
  '      End If
  '    End If
  '  End If

  '  'controleer nu of de tak evt via linkage nodes aan de onderhavige tak zit vastgeklampt
  '  For Each myReach As clsCFReach In myReaches.Reaches.Values
  '    If myReach.fNode.ci = ID Or myReach.tNode.ci = ID Then
  '      If IgnoreDisabledReaches = True And myReach.InUse = False Then
  '        'geen officiele verbinding want deze mogen we overslaan
  '        Me.setup.Log.AddDebugMessage("Geen officiele verbinding")
  '      Else
  '        nConnected = nConnected + 1
  '      End If
  '    End If
  '  Next

  '  Return nConnected

  'End Function
End Class
