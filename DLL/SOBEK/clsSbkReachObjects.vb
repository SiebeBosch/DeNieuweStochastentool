Option Explicit On

Imports STOCHLIB.General

Friend Class clsSbkReachObjects

    Friend ReachObjects As New Dictionary(Of String, clsSbkReachObject)       'all reachobjects
    Friend NetworkGRRecord As clsNetworkGRrecord

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub Sort()
        'we'll have to sort the objects by chainage again since we've just added two new reachobjects!
        Dim sorted = From pair In ReachObjects Order By pair.Value.lc
        ReachObjects = sorted.ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
    End Sub

    Friend Function Add(ByRef myObject As clsSbkReachObject) As Boolean
        Try
            If Not ReachObjects.ContainsKey(myObject.ID.Trim.ToUpper) Then
                ReachObjects.Add(myObject.ID.Trim.ToUpper, myObject)
                Return True
            Else
                Throw New Exception("Error creating new reach object " & myObject.ID & " since there was already an object with the same ID.")
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Sub addNetworkTPNDLKRecord(ByVal myRecord As clsNetworkTPNdlkRecord)
        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)

        If ReachObjects.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myRecord.ID.Trim.ToUpper)
            myReachObject.Name = myRecord.Name
            myReachObject.NetworkTPNDLKRecord = myRecord
            myReachObject.ci = myRecord.ci
            myReachObject.lc = myRecord.lc
            myReachObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myRecord.ID
            myReachObject.Name = myRecord.Name
            myReachObject.NetworkTPNDLKRecord = myRecord
            myReachObject.ci = myRecord.ci
            myReachObject.lc = myRecord.lc
            myReachObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
        End If

        Dim myReach As clsSbkReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
        If Not Me.SbkCase.CFTopo.ReachObjectExists(myReachObject.ID.Trim.ToUpper) Then
            'leg het object ook meteen vast op de betreffende reach
            myReach.ReachObjects.ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
        End If

    End Sub

    Friend Sub AddNetworkSTrecord(ByVal myRecord As clsNetworkSTRecord)
        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If ReachObjects.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myRecord.ID.Trim.ToUpper)
            myReachObject.NetworkSTRecord = myRecord
            myReachObject.Name = myRecord.Name
            myReachObject.ci = myRecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myRecord.lc, myReach.getReachLength)
            myReachObject.InUse = True
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myRecord.ID
            myReachObject.NetworkSTRecord = myRecord
            myReachObject.Name = myRecord.Name
            myReachObject.ci = myRecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myRecord.lc, myReach.getReachLength)
            myReachObject.InUse = True
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            If Not Me.SbkCase.CFTopo.ReachObjectExists(myReachObject.ID.Trim.ToUpper) Then
                'leg het object ook meteen vast op de betreffende reach
                myReach.ReachObjects.ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            End If
        End If
    End Sub

    Public Function AutoCorrectReachObjectLocation(lc As Double, ByRef myReachLength As Double) As Double
        'this function makes sure any reachobject is located at least 1 mm from the start or end of its reach
        If lc = 0 Then
            Return (lc + 0.001)
        ElseIf lc >= myReachLength Then
            Return (myReachLength - 0.001)
        Else
            Return lc
        End If
    End Function

    Friend Sub AddNetworkCNFLBRRecord(ByVal myrecord As clsNetworkCNFLBRRecord)

        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If ReachObjects.ContainsKey(myrecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myrecord.ID.Trim.ToUpper)
            myReachObject.NetworkCNFLBRRecord = myrecord
            myReachObject.ci = myrecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myrecord.lc, myReach.getReachLength)
            myReachObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
            myReachObject.InUse = True
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myrecord.ID
            myReachObject.NetworkCNFLBRRecord = myrecord
            myReachObject.ci = myrecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myrecord.lc, myReach.getReachLength)
            myReachObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
            myReachObject.InUse = True
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            If Not Me.SbkCase.CFTopo.ReachObjectExists(myReachObject.ID.Trim.ToUpper) Then
                'leg het object ook meteen vast op de betreffende reach
                myReach.ReachObjects.ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            End If
        End If

    End Sub

    Friend Sub AddNetworkCNFLBXRecord(ByVal myrecord As clsNetworkCNFLBXRecord)

        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If ReachObjects.ContainsKey(myrecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myrecord.ID.Trim.ToUpper)
            myReachObject.NetworkCNFLBXRecord = myrecord
            myReachObject.ci = myrecord.ci
            myReachObject.lc = myrecord.lc
            myReachObject.InUse = True
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myrecord.ID
            myReachObject.NetworkCNFLBXRecord = myrecord
            myReachObject.ci = myrecord.ci
            myReachObject.lc = myrecord.lc
            myReachObject.InUse = True
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            If Not Me.SbkCase.CFTopo.ReachObjectExists(myReachObject.ID.Trim.ToUpper) Then myReach.ReachObjects.ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject) 'leg het object ook meteen vast op de betreffende reach
        End If


    End Sub

    Friend Sub AddNetworkMERecord(ByVal myrecord As clsNetworkMErecord)

        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If ReachObjects.ContainsKey(myrecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myrecord.ID.Trim.ToUpper)
            myReachObject.NetworkMERecord = myrecord
            myReachObject.Name = myrecord.Name
            myReachObject.ci = myrecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myrecord.lc, myReach.getReachLength)
            myReachObject.InUse = True
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myrecord.ID
            myReachObject.NetworkMERecord = myrecord
            myReachObject.Name = myrecord.Name
            myReachObject.ci = myrecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myrecord.lc, myReach.getReachLength)
            myReachObject.InUse = True
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            If Not Me.SbkCase.CFTopo.ReachObjectExists(myReachObject.ID.Trim.ToUpper) Then
                'leg het object ook meteen vast op de betreffende reach
                myReach.ReachObjects.ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            End If
        End If
    End Sub

    Friend Sub AddNetworkCRrecord(ByVal myrecord As clsNetworkCRRecord, ByRef Setup As clsSetup)
        'als het een kunstwerk is
        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If ReachObjects.ContainsKey(myrecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myrecord.ID.Trim.ToUpper)
            myReachObject.NetworkCRRecord = myrecord
            myReachObject.Name = myrecord.Name
            myReachObject.ci = myrecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myrecord.lc, myReach.getReachLength)
            myReachObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE
            myReachObject.InUse = True
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myrecord.ID
            myReachObject.NetworkCRRecord = myrecord
            myReachObject.Name = myrecord.Name
            myReachObject.ci = myrecord.ci
            myReach = Me.SbkCase.CFTopo.Reaches.GetReach(myReachObject.ci.Trim.ToUpper)
            myReachObject.lc = AutoCorrectReachObjectLocation(myrecord.lc, myReach.getReachLength)
            myReachObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE
            myReachObject.InUse = True
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            If Not Me.SbkCase.CFTopo.ReachObjectExists(myReachObject.ID.Trim.ToUpper) Then
                'leg het object ook meteen vast op de betreffende reach
                myReach.ReachObjects.ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
            End If
        End If
    End Sub

    Friend Sub AddNetworkOBIRecord(ByVal myrecord As clsNetworkOBIOBIDRecord)

        'als het een dwarsprofiel is
        Dim myReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        If ReachObjects.ContainsKey(myrecord.ID.Trim.ToUpper) Then
            myReachObject = ReachObjects(myrecord.ID.Trim.ToUpper)
            myReachObject.NetworkObiRecord = myrecord
            myReachObject.SetObjectType()
            myReachObject.InUse = True
        Else
            myReachObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myReachObject.ID = myrecord.ID
            myReachObject.NetworkObiRecord = myrecord
            myReachObject.SetObjectType()
            myReachObject.InUse = True
            Call ReachObjects.Add(myReachObject.ID.Trim.ToUpper, myReachObject)
        End If

    End Sub

    Friend Function GetUpstreamCalculationPoint(ByVal myDist As Double) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-05-2012
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde bovenstrooms gelegen rekenpunt
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        Dim IncludeObject As Boolean = False
        For Each myObject As clsSbkReachObject In ReachObjects.Values
            If myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed Then
                If Math.Round(myObject.lc) <= Math.Round(myDist) Then
                    If myDist - myObject.lc < minDist Then
                        minDist = myDist - myObject.lc
                        nearestObject = myObject
                    End If
                End If
            End If
        Next myObject
        Return nearestObject

    End Function

    Friend Function GetDownstreamCalculationPoint(ByVal myDist As Double) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-05-2012
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde benedenstrooms gelegen rekenpunt
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        Dim IncludeObject As Boolean = False
        For Each myObject As clsSbkReachObject In ReachObjects.Values
            If myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed Then
                If Math.Round(myObject.lc) >= Math.Round(myDist) Then
                    If myObject.lc - myDist < minDist Then
                        minDist = myObject.lc - myDist
                        nearestObject = myObject
                    End If
                End If
            End If
        Next myObject
        Return nearestObject

    End Function

    Friend Function FindNearestUpstreamProfile(ByVal myObj As clsSbkReachObject) As clsSbkReachObject
        Dim minDist As Double = 99999999
        Dim Nearest As New clsSbkReachObject(Me.Setup, Me.SbkCase)

        Dim myReach As clsSbkReach = Me.SbkCase.CFTopo.Reaches.GetReach(myObj.ci)
        For Each onReachObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
            If onReachObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso onReachObj.lc < myObj.lc Then
                If (myObj.lc - onReachObj.lc) < minDist Then
                    minDist = myObj.lc - onReachObj.lc
                    Nearest = onReachObj
                End If
            End If
        Next onReachObj

        Return Nearest
    End Function



    Friend Function FindNearestDownstreamProfile(ByVal myObj As clsSbkReachObject) As clsSbkReachObject
        Dim minDist As Double = 99999999
        Dim Nearest As New clsSbkReachObject(Me.Setup, Me.SbkCase)

        Dim myReach As clsSbkReach = Me.SbkCase.CFTopo.Reaches.GetReach(myObj.ci)
        For Each onReachObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
            If onReachObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso onReachObj.lc > myObj.lc Then
                If (onReachObj.lc - myObj.lc) < minDist Then
                    minDist = onReachObj.lc - myObj.lc
                    Nearest = onReachObj
                End If
            End If
        Next onReachObj

        Return Nearest
    End Function

    Friend Function CreateID(ByVal Prefix As String) As String
        Dim Done As Boolean = False
        Dim i As Integer, myID As String
        While Not Done
            i += 1
            myID = Prefix.Trim & Str(i).Trim
            myID = myID.Trim
            If Not ReachObjects.ContainsKey(myID.Trim.ToUpper) AndAlso Not Me.SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.ContainsKey(myID.Trim.ToUpper) Then
                Done = True
                Return myID
            End If
        End While
        Return Nothing
    End Function
End Class
