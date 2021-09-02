Option Explicit On

Imports STOCHLIB.General

Friend Class clsNetworkGRrecord
    Friend ID As String             'id van het network.gr-record
    Friend ci As String             'takID aangaande deze record
    Friend re As Integer            'type of reach identifiers (0 = default, 2 = based on name and distance)
    Friend dc As Integer            'number of decimals
    Friend record As String         'het hele record zoals  uit de file gehaald

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub Read(ByVal myRecord As String, IncludeReachNodes As Boolean)

        Dim Done As Boolean, myStr As String
        Dim ID As String, i As Long
        Dim myTable As New clsSobekTableVariant(Me.Setup)
        Dim myObj As clsSbkReachObject
        Dim myReach As clsSbkReach = Nothing
        record = myRecord

        Done = False

        While Not Done
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr.Trim.ToLower
                Case "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")   'id of the record
                Case "ci"
                    ci = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")   'id of the reach
                    myReach = SbkCase.CFTopo.Reaches.GetReach(ci)
                Case "re"
                    re = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")   'type of reach identifiers (0 = default, 2 = based on name and distance)
                Case "dc"
                    dc = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")   'number of decimals
                Case "gridpoint table"
                    myTable = Me.Setup.GeneralFunctions.ParseSobekTableVariant(myRecord)
                    For i = 0 To myTable.XValues.Count - 1

                        'read the reach objects for this reach
                        myObj = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                        myObj.ID = myTable.Values3.Values(i)
                        myObj.ci = ci
                        myReach = SbkCase.CFTopo.Reaches.GetReach(ci.Trim.ToUpper)
                        myObj.lc = myTable.XValues.Values(i)
                        myObj.calcXY()
                        myObj.InUse = True
                        myObj.nt = SbkCase.CFTopo.getNodeType(myObj.ID)

                        'read the reach segments for this reach
                        Dim mySeg = New clsSbkReachSegment(Me.Setup, Me.SbkCase)
                        mySeg.id = myTable.Values4.Values(i)
                        mySeg.ci = ci
                        If Not myReach.ReachSegments.ContainsKey(mySeg.ID.Trim.ToUpper) Then
                            myReach.ReachSegments.Add(mySeg.ID.Trim.ToUpper, mySeg)
                        End If

                        'if the object found is a reachnode and including reachnodes is allowed, add it as if it were a reach object
                        If myObj.IsReachNode AndAlso IncludeReachNodes Then
                            If myObj.isWaterLevelObject Then myObj.IsGridPoint = True 'even though structures can be toggled as calculation points, their results won't give water levels!
                            If Me.SbkCase.CFTopo.ReachNodes.ReachNodes.ContainsKey(myObj.ID.Trim.ToUpper) Then
                                If Not myReach.ReachObjects.ReachObjects.ContainsKey(myObj.ID.Trim.ToUpper) Then
                                    myReach.ReachObjects.ReachObjects.Add(myObj.ID.Trim.ToUpper, myObj)
                                End If
                            End If
                        ElseIf myReach.ReachObjects.ReachObjects.ContainsKey(myObj.ID.Trim.ToUpper) Then
                            myObj = myReach.ReachObjects.ReachObjects.Item(myObj.ID.Trim.ToUpper)
                            If myObj.isWaterLevelObject Then myObj.IsGridPoint = True 'even though structures can be toggled as calculation points, their results won't give water levels!
                        Else
                            If myObj.isWaterLevelObject Then myObj.IsGridPoint = True
                            Call myReach.ReachObjects.ReachObjects.Add(myObj.ID.Trim.ToUpper, myObj)
                        End If
                    Next
                    'in case the reachnodes have been added, we'll need to sort the list of reachobjects again by chainage
                    myReach.ReachObjects.Sort()

            End Select
            If myRecord = "" Then Done = True
        End While

    End Sub





End Class

