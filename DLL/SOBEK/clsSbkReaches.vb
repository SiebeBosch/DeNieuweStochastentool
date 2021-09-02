Option Explicit On

Imports STOCHLIB.General
Imports System.IO

Public Class clsSbkReaches
    Friend Reaches As New Dictionary(Of String, clsSbkReach)
    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Public Function CountInuse() As Integer
        Dim n As Integer = 0
        For Each myReach As clsSbkReach In Reaches.Values
            If myReach.InUse Then n += 1
        Next
        Return n
    End Function
    Friend Sub SetInuse(ByVal Setting As Boolean)
        For Each myReach As clsSbkReach In Reaches.Values
            myReach.InUse = Setting
        Next
    End Sub

    Friend Function GetTotalLength() As Double
        Dim TotalLength As Double
        For Each myReach As clsSbkReach In Reaches.Values
            If myReach.InUse Then TotalLength += myReach.getReachLength
        Next
        Return TotalLength
    End Function

    Friend Function CalculateRouteNumbers() As Boolean
        Try
            'this function assignes a unique route numbers to each chain of reaches that are connected via interpolation
            Dim Done As Boolean = False
            Dim RouteNumber As Integer = 0
            Dim ReachesProcessed As New List(Of String)
            Dim ReachChain As New Dictionary(Of String, clsSbkReach)

            For Each myReach As clsSbkReach In Reaches.Values
                If myReach.InUse AndAlso myReach.RouteNumber = 0 Then
                    ReachChain = myReach.getInterpolatedReachesChain()
                    If ReachChain.Count <= 1 Then
                        For Each part As clsSbkReach In ReachChain.Values
                            part.RouteNumber = 0
                        Next
                    Else
                        RouteNumber += 1
                        For Each part As clsSbkReach In ReachChain.Values
                            part.RouteNumber = RouteNumber
                        Next
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SetRouteNumbers of class clsSbkReaches.")
            Return False
        End Try
    End Function

    Friend Function BuildReachStraight(ByVal NameBase As String, ByRef bn As clsSbkReachNode, ByRef en As clsSbkReachNode) As clsSbkReach
        'creates a new reach with a 90 degrees angle between two given nodes
        Dim newReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim Length As Double
        Dim Angle As Double = Me.Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, en.X, en.Y)
        Length = Me.Setup.GeneralFunctions.Pythagoras(bn.X, bn.Y, en.X, en.Y)

        'set the begin- and endnode for this reach and set the reach to InUse = true
        newReach.Id = CreateID(NameBase)
        newReach.bn = bn
        newReach.en = en
        newReach.InUse = True

        newReach.NetworkcpRecord.Table.AddDataPair(2, Length / 2, 180 + Angle)
        newReach.NetworkcpRecord.CalcCPTable(newReach.Id, newReach.bn, newReach.en)              'calculates the exact location of the vector points. For internal use

        'add the reach to the topography
        Reaches.Add(newReach.Id.Trim.ToUpper, newReach)

        Return newReach

    End Function

    Friend Function setSnapLocationForReachNode(ByRef myNode As clsSbkReachNode, ByVal SearchRadius As Double, AllowSnappingToVectorPoints As Boolean) As Boolean
        Try
            'this function finds a snapping location on the nearest reach for an existing reach node
            'note: in order to do so it skips all reaches that it's already part of (either beginning node or ending node)
            Dim nearestDist As Double = 9.9E+100
            Dim reachSnapDist As Double, reachSnapChainage As Double
            Dim Found As Boolean = False
            For Each myReach As clsSbkReach In Reaches.Values
                'make sure we only look at reaches that do NOT have the current node as their start- or endpoint
                If myReach.InUse = True AndAlso Not myReach.bn.ID = myNode.ID AndAlso Not myReach.en.ID = myNode.ID Then
                    If myReach.calcSnapLocation(myNode.X, myNode.Y, SearchRadius, reachSnapChainage, reachSnapDist, AllowSnappingToVectorPoints) AndAlso reachSnapDist < nearestDist Then
                        Found = True
                        nearestDist = reachSnapDist
                        myNode.SnapReachID = myReach.Id
                        myNode.SnapChainage = reachSnapChainage
                        myNode.SnapDistance = reachSnapDist
                    End If
                End If
            Next
            Return Found
        Catch ex As Exception
            Return False
        End Try
    End Function


    Friend Function FindSnapLocationInsidePolygon(ByVal X As Double, ByVal Y As Double, ByRef Polygon As MapWinGIS.Shape, ByRef SearchRadius As Double, ByRef snapReach As clsSbkReach, ByRef snapChainage As Double, ByRef snapDistance As Double, AllowSnappingToVectorPoints As Boolean, Optional ByVal IncludeInactiveReaches As Boolean = False, Optional ByVal ExcludeReachesWithPrefix As List(Of String) = Nothing, Optional ByVal ReachIDShortList As List(Of String) = Nothing) As Boolean
        'v1.798: introduced a shortlist of ID's that need checking.
        'this will prevent the algorithm to process all reaches in the network
        'if no shortlist is given, all reaches are processed
        Try
            Dim nearestDist As Double = 9.9E+100
            Dim reachSnapDist As Double = 9.9E+100
            Dim Found As Boolean = False
            Dim SkipReach As Boolean

            If Polygon Is Nothing Then Throw New Exception("Error finding snapping location inside polygon since polygon is void.")

            For Each myReach As clsSbkReach In Reaches.Values

                'check whether this reach should be skipped.

                'if there is a shortlist of reach ID's, skip reaches that are not present in that list
                If Not ReachIDShortList Is Nothing AndAlso Not ReachIDShortList.Contains(myReach.Id.Trim.ToUpper) Then Continue For

                'skip inactive reaches if required
                If (myReach.InUse = False AndAlso Not IncludeInactiveReaches) Then Continue For

                'skip reaches with a prefix from the prefixes shortlist
                If Not ExcludeReachesWithPrefix Is Nothing Then
                    For Each myPrefix As String In ExcludeReachesWithPrefix
                        If Me.Setup.GeneralFunctions.HasPrefix(myReach.Id, myPrefix, False) Then SkipReach = True
                    Next
                End If
                If SkipReach Then Continue For

                'if an intersection with a polygon is required, we will first attempt that
                'this is new in v1.77
                'we must find a snapping point inside a given polygon
                Dim curChainage As Double
                'v1.798: changed the initial snapping distance from 9E99 to the the maximum extent withinin the polygon's extent
                Dim curDistance As Double = Me.Setup.GeneralFunctions.Pythagoras(Polygon.Extents.xMin, Polygon.Extents.yMin, Polygon.Extents.xMax, Polygon.Extents.yMax)
                If myReach.calcSnapLocationInsidePolygon(X, Y, SearchRadius, curChainage, curDistance, Polygon) Then
                    If curDistance < snapDistance Then
                        snapDistance = curDistance
                        snapChainage = curChainage
                        snapReach = myReach
                        Found = True
                    End If
                End If

            Next
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error snapping location to reaches.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function
    Friend Function FindSnapLocation(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef snapReach As clsSbkReach, ByRef snapChainage As Double, ByRef snapDistance As Double, AllowSnappingToVectorPoints As Boolean, Optional ByVal IncludeInactiveReaches As Boolean = False, Optional ByVal ExcludeReachesWithPrefix As List(Of String) = Nothing) As Boolean
        Try
            Dim nearestDist As Double = 9.9E+100
            Dim reachSnapDist As Double = 9.9E+100, reachSnapChainage As Double
            Dim DistToBoundingBox As Double
            Dim Found As Boolean = False
            Dim SkipReach As Boolean

            For Each myReach As clsSbkReach In Reaches.Values

                'check whether this reach should be skipped
                SkipReach = False

                If (myReach.InUse = False AndAlso Not IncludeInactiveReaches) Then SkipReach = True
                If Not ExcludeReachesWithPrefix Is Nothing Then
                    For Each myPrefix As String In ExcludeReachesWithPrefix
                        If Me.Setup.GeneralFunctions.HasPrefix(myReach.Id, myPrefix, False) Then SkipReach = True
                    Next
                End If
                If SkipReach Then Continue For

                'calculate the bounding box for our current reach
                If myReach.BoundingBox Is Nothing Then myReach.calcBoundingBox()

                If X >= myReach.BoundingBox.XLL - SearchRadius AndAlso X <= myReach.BoundingBox.XUR + SearchRadius AndAlso Y >= myReach.BoundingBox.YLL - SearchRadius AndAlso Y <= myReach.BoundingBox.YUR + SearchRadius Then
                    'ok, we've found a reach that matches the criteria, but is it worth to actually find the snapping point? 
                    'only if the distance to the extent is smaller than the nearest distance found so far!
                    DistToBoundingBox = myReach.BoundingBox.calcDistance(X, Y)
                    If DistToBoundingBox <= nearestDist Then 'important: this MUST be <=, not < because any point INSIDE the bounding box gets value 0 and must be tried anyhow
                        If myReach.calcSnapLocation(X, Y, SearchRadius, reachSnapChainage, reachSnapDist, AllowSnappingToVectorPoints) AndAlso reachSnapDist <= SearchRadius AndAlso reachSnapDist < nearestDist Then
                            Found = True
                            nearestDist = reachSnapDist
                            snapDistance = reachSnapDist
                            snapChainage = reachSnapChainage
                            snapReach = myReach
                        End If
                    End If
                End If
            Next
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error snapping location to reaches.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function FindSnapLocations(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef SnapReaches As List(Of clsSbkReach), ByRef SnapChainages As List(Of Double), ByRef snapDistances As List(Of Double), AllowSnappingToVectorPoints As Boolean, Optional ByVal IncludeInactiveReaches As Boolean = False, Optional ByVal ExcludeReachesWithPrefix As List(Of String) = Nothing) As Boolean
        Try
            Dim nearestDist As Double = 9.9E+100
            Dim reachSnapDist As Double = 9.9E+100, reachSnapChainage As Double
            Dim Found As Boolean = False
            Dim SkipReach As Boolean
            For Each myReach As clsSbkReach In Reaches.Values

                'check whether this reach should be skipped
                SkipReach = False
                If (myReach.InUse = False AndAlso Not IncludeInactiveReaches) Then SkipReach = True
                If Not ExcludeReachesWithPrefix Is Nothing Then
                    For Each myPrefix As String In ExcludeReachesWithPrefix
                        If Me.Setup.GeneralFunctions.HasPrefix(myReach.Id, myPrefix, False) Then SkipReach = True
                    Next
                End If
                If SkipReach Then Continue For

                'calculate the bounding box for our current reach
                If myReach.BoundingBox Is Nothing Then myReach.calcBoundingBox()

                'first we will check if the original location potentially lies within the snapping distance in the first place
                If X >= myReach.BoundingBox.XLL - SearchRadius AndAlso X <= myReach.BoundingBox.XUR + SearchRadius AndAlso Y >= myReach.BoundingBox.YLL - SearchRadius AndAlso Y <= myReach.BoundingBox.YUR + SearchRadius Then
                    'ok, we've found a reach that matches the criteria, but is it worth to actually find the snapping point? 
                    If myReach.calcSnapLocation(X, Y, SearchRadius, reachSnapChainage, reachSnapDist, AllowSnappingToVectorPoints) AndAlso reachSnapDist <= SearchRadius AndAlso reachSnapDist < nearestDist Then
                        Found = True
                        If reachSnapDist <= SearchRadius Then
                            snapDistances.Add(reachSnapDist)
                            SnapChainages.Add(reachSnapChainage)
                            SnapReaches.Add(myReach)
                        End If
                    End If
                End If
            Next
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error snapping location to reaches.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function FindSnapLocationForLineElement(ID As String, ByRef Shape As MapWinGIS.Shape, MaxAngleDifferenceDegrees As Double, ByVal SearchRadius As Double, ByRef snapReach As clsSbkReach, ByRef snapChainage As Double, ByRef snapDistance As Double, AllowSnappingToVectorPoints As Boolean, Optional ByVal IncludeInactiveReaches As Boolean = False, Optional ByVal ExcludeReachesWithPrefix As List(Of String) = Nothing) As Boolean
        'siebe: this function is new in v1.71
        'it aims to find a snapping location for an object that is a line segment
        'this might be e.g. culverts, siphons etc.
        'snapping takes place at the start and end point of the object. The sum of squares of the snapping distance determines which reach is eligible for snapping
        Try
            Dim nearestDist As Double = 9.9E+100
            Dim reachSnapDist As Double = 9.9E+100
            Dim Xstart As Double = Shape.Point(0).x
            Dim Ystart As Double = Shape.Point(0).y
            Dim Xend As Double = Shape.Point(Shape.numPoints - 1).x
            Dim Yend As Double = Shape.Point(Shape.numPoints - 1).y
            Dim ObjectAngle As Double = Me.Setup.GeneralFunctions.NormalizeAngle(Me.Setup.GeneralFunctions.LineAngleDegrees(Xstart, Ystart, Xend, Yend))
            Dim Xcenter As Double = Shape.Center.x
            Dim Ycenter As Double = Shape.Center.y
            Dim SnapReaches As New List(Of clsSbkReach) 'a list of reaches where the object can snap to
            Dim SnapChainages As New List(Of Double)
            Dim SnapDistances As New List(Of Double)

            If FindSnapLocations(Xcenter, Ycenter, SearchRadius, SnapReaches, SnapChainages, SnapDistances, True) Then

                'we have found one or more reaches for snapping. Now let's analyze the angles and compare them with our object that needs snapping
                Dim minAngleDiff As Double = 9.0E+99
                Dim CurAngle As Double, curAngleDiff As Double
                Dim BestReachIDx As Integer = -1

                For i = 0 To SnapReaches.Count - 1
                    SnapReaches(i).getVectorAngle(SnapChainages(i), CurAngle)
                    curAngleDiff = Me.Setup.GeneralFunctions.AngleDifferenceDeg(CurAngle, ObjectAngle, True)

                    'only if the angle difference is smaller than the assigned maximum will we implement this object
                    If curAngleDiff <= MaxAngleDifferenceDegrees Then
                        If Math.Min(curAngleDiff, 360 - curAngleDiff) < minAngleDiff Then       'since the angle difference can be anything between 0 and 360 we also have to take into account the fact that e.g. 359 degrees is essentially 1 degree. 
                            minAngleDiff = Math.Min(curAngleDiff, 360 - curAngleDiff)
                            BestReachIDx = i
                        End If
                    End If
                Next

                If BestReachIDx < 0 Then
                    Return False
                Else
                    snapReach = SnapReaches(BestReachIDx)
                    snapChainage = SnapChainages(BestReachIDx)
                    snapDistance = SnapDistances(BestReachIDx)
                    Return True
                End If
            Else
                Return False
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error snapping location " & ID & " to reaches.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function BuildReachByAngle(ByVal NameBase As String, ByRef bn As clsSbkReachNode, ByRef en As clsSbkReachNode, ByVal BendRight As Boolean, ByVal BendAngleDeg As Double) As clsSbkReach
        'creates a new reach with a given angle between two given nodes
        Dim newReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim Length As Double
        Dim Angle As Double = Me.Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, en.X, en.Y)
        Dim BendAngleRad As Double = Me.Setup.GeneralFunctions.D2R(BendAngleDeg)

        'first calculate the length in a straight line. Then convert it to the actual length of each vector
        Length = Me.Setup.GeneralFunctions.Pythagoras(bn.X, bn.Y, en.X, en.Y)
        Length = (Length / 2) / Math.Cos(BendAngleRad)

        'set the begin- and endnode for this reach and set the reach to InUse = true
        newReach.Id = CreateID(NameBase)
        newReach.bn = bn
        newReach.en = en
        newReach.InUse = True

        'decide which way to bend the reach
        If BendRight Then
            newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 1 / 2, Angle + BendAngleDeg)
            newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 3 / 2, Angle - BendAngleDeg)
        Else
            newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 1 / 2, Angle - BendAngleDeg)
            newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 3 / 2, Angle + BendAngleDeg)
        End If

        newReach.NetworkcpRecord.CalcCPTable(newReach.Id, newReach.bn, newReach.en)              'calculates the exact location of the vector points. For internal use

        'add the reach to the topography
        Reaches.Add(newReach.Id.Trim.ToUpper, newReach)
        Return newReach

    End Function

    'Friend Function BuildReach90DegAngle(ByVal NameBase As String, ByRef bn As clsSbkReachNode, ByRef en As clsSbkReachNode, ByVal BendRight As Boolean) As clsSbkReach
    '  'creates a new reach with a 90 degrees angle between two given nodes
    '  Dim newReach As New clsSbkReach(Me.Setup, Me.SbkCase)
    '  Dim Length As Double
    '  Dim Angle As Double = Me.Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, en.X, en.Y)

    '  'first calculate the length in a straight line. Then convert it to the actual reach length
    '  'a^2 + b^2 = 10^2 en a = b dus 2a^2=100 dus a = SQR(50)
    '  Length = Me.Setup.GeneralFunctions.Pythagoras(bn.X, bn.Y, en.X, en.Y)
    '  Length = 2 * Math.Sqrt(Length ^ 2 / 2)

    '  'set the begin- and endnode for this reach and set the reach to InUse = true
    '  newReach.Id = CreateID(NameBase)
    '  newReach.bn = bn
    '  newReach.en = en
    '  newReach.InUse = True

    '  'decide which way to bend the reach
    '  If BendRight Then
    '    newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 1 / 4, Angle + 45)
    '    newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 3 / 4, Angle - 45)
    '  Else
    '    newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 1 / 4, Angle - 45)
    '    newReach.NetworkcpRecord.Table.AddDataPair(2, Length * 3 / 4, Angle + 45)
    '  End If

    '  newReach.NetworkcpRecord.CalcCPTable(newReach.Id, newReach.bn, newReach.en)              'calculates the exact location of the vector points. For internal use

    '  'add the reach to the topography
    '  Reaches.Add(newReach.Id.Trim.ToUpper, newReach)
    '  Return newReach

    'End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        'add the prefix to the reach id's and the id's of all reach objects!
        'note: do NOT do this for the reach nodes. They will be treated separately in clsSbkReachNodes
        For Each myReach As clsSbkReach In Reaches.Values
            myReach.Id = Prefix & myReach.Id                  'add the prefix to the reach id
            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                myObj.ID = Prefix & myObj.ID                    'add the prefix to each reach object
                myObj.ci = Prefix & myObj.ci                    'also the reach id
            Next
        Next
    End Sub

    ''' <summary>
    ''' Creëer een ID voor een nieuw te maken sobektak
    ''' </summary>
    ''' <param name="NameBase"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function CreateID(ByVal NameBase As String) As String
        'in deze functie gaan we een ID voor een nieuwe tak creëren, gebaseerd op een oorspronkelijke tak
        'deze functie wordt aangeroepen nadat een tak is opgesplitst in twee delen
        Dim myID As String = ""
        Dim i As Integer = 0
        Dim Done As Boolean = False

        'first remove all postfixes such as _1 etc. from the namebase.
        'this way we avoid id's to grow immensely to e.g. REACHABC_1_2_1_3_3 etc.
        NameBase = Me.Setup.GeneralFunctions.RemoveNumericPostfixes(NameBase, "_")

        If Not Reaches.ContainsKey(NameBase.Trim.ToUpper) AndAlso Not NameBase.Trim = "" Then
            Return NameBase.Trim
        Else
            While Not Done
                i = i + 1
                myID = Trim(NameBase.Trim & "_" & Str(i).Trim)
                If Not Reaches.ContainsKey(myID.Trim.ToUpper) Then
                    Done = True
                    Return myID
                End If
            End While
            Return Nothing
        End If

    End Function

    Friend Function GetUpstreamInterpolatedReachChain(ByRef StartReachID As String) As Dictionary(Of String, clsSbkReach)
        'siebe bosch, 12-4-2017
        'this function collects all upstream reaches that are interconnected to a given reach via interpolation
        Dim newList As New Dictionary(Of String, clsSbkReach)
        Dim Done As Boolean = False
        Dim NodeDat As clsNodesDatNODERecord
        Dim myReach As clsSbkReach = GetReach(StartReachID)
        Dim upReach As clsSbkReach
        Dim upReaches As New List(Of clsSbkReach)


        Try
            While Not Done
                Done = True
                If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.bn.ID.Trim.ToUpper) Then
                    NodeDat = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.bn.ID.Trim.ToUpper)
                    If NodeDat.ni = 1 AndAlso (myReach.Id.Trim.ToUpper = NodeDat.r1.Trim.ToUpper OrElse myReach.Id.Trim.ToUpper = NodeDat.r2.Trim.ToUpper) Then  'interpolation takes place over this node
                        upReaches = myReach.GetUpstreamReachesSameDirection     'collects all upstream reaches that are directly connected to the current reach
                        For Each upReach In upReaches
                            If upReach.Id.Trim.ToUpper = NodeDat.r1.Trim.ToUpper OrElse upReach.Id.Trim.ToUpper = NodeDat.r2.Trim.ToUpper Then
                                If Not newList.ContainsKey(upReach.Id.Trim.ToUpper) Then
                                    newList.Add(upReach.Id.Trim.ToUpper, upReach)    'add this upstream reach to the list of upstream & interpolated reaches. 
                                    myReach = upReach       'now set the upstream reach we found to be 'myreach'
                                    Done = False            'we found an upstream reach, so reset done to false in order to allow another cycle
                                End If
                            End If
                        Next
                    End If
                End If
            End While
            Return newList
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetUpstreamInterpolatedReaches")
            Return newList
        End Try
    End Function


    Friend Function GetDownstreamInterpolatedReachChain(ByRef StartReachID As String) As Dictionary(Of String, clsSbkReach)
        'siebe bosch, 12-4-2017
        'this function collects all downstream reaches that are interconnected to a given reach via interpolation
        Dim newList As New Dictionary(Of String, clsSbkReach)
        Dim Done As Boolean = False
        Dim NodeDat As clsNodesDatNODERecord
        Dim myReach As clsSbkReach = GetReach(StartReachID)
        Dim dnReach As clsSbkReach
        Dim dnReaches As New List(Of clsSbkReach)

        Try
            While Not Done
                Done = True
                If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.en.ID.Trim.ToUpper) Then
                    NodeDat = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.en.ID.Trim.ToUpper)
                    If NodeDat.ni = 1 AndAlso (myReach.Id.Trim.ToUpper = NodeDat.r1.Trim.ToUpper OrElse myReach.Id.Trim.ToUpper = NodeDat.r2.Trim.ToUpper) Then                                      'interpolation takes place over this node
                        dnReaches = myReach.GetDownstreamReachesSameDirection     'collects all downstream reaches that are directly connected to the current reach
                        For Each dnReach In dnReaches
                            If dnReach.Id.Trim.ToUpper = NodeDat.r1.Trim.ToUpper OrElse dnReach.Id.Trim.ToUpper = NodeDat.r2.Trim.ToUpper Then
                                If Not newList.ContainsKey(dnReach.Id.Trim.ToUpper) Then
                                    newList.Add(dnReach.Id.Trim.ToUpper, dnReach)       'add this downstream reach to the list of downstream & interpolated reaches. 
                                    myReach = dnReach                                   'now set the downstream reach we found to be 'myreach'
                                    Done = False                                        'we found an downstream reach, so reset done to false in order to allow another cycle
                                End If
                            End If
                        Next
                    End If
                End If
            End While
            Return newList
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetDownstreamInterpolatedReaches")
            Return newList
        End Try
    End Function


    Friend Function GetInterpolatedReaches(ByVal StartReachIdx As Long, ByRef NodesDat As clsCFNodesData) As List(Of clsSbkReach)
        'this function searches the network and returns all connected & interpolated reaches that are connected to the start reach
        Dim Done As Boolean = False
        Dim curNode As clsSbkReachNode, curReach As clsSbkReach
        Dim NodeRecord As clsNodesDatNODERecord
        Dim ReachList As New List(Of clsSbkReach)

        'first move upstream
        curReach = Reaches.Values(StartReachIdx)
        ReachList.Add(curReach)
        curNode = curReach.bn

        'search upstream
        While Not Done
            If NodesDat.NodesDatNodeRecords.records.ContainsKey(curNode.ID.Trim.ToUpper) Then
                NodeRecord = NodesDat.NodesDatNodeRecords.records(curNode.ID.Trim.ToUpper)
                If NodeRecord.ni = 1 Then
                    If NodeRecord.r2.Trim.ToUpper = curReach.Id.Trim.ToUpper Then
                        curReach = Reaches(NodeRecord.r1.Trim.ToUpper)
                        ReachList.Add(curReach)
                        If curReach.bn.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.bn
                        ElseIf curReach.en.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.en
                        End If
                    ElseIf NodeRecord.r1.Trim.ToUpper = curReach.Id.Trim.ToUpper Then
                        curReach = Reaches(NodeRecord.r2.Trim.ToUpper)
                        ReachList.Add(curReach)
                        If curReach.bn.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.bn
                        ElseIf curReach.en.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.en
                        End If
                    Else
                        'the current reach is not the one being interpolated at this node. So move on
                        Done = True
                    End If
                Else
                    Done = True 'no interpolation of this node
                End If
            Else
                Done = True 'no nodes.dat record for this node, so no interpolation either
            End If
        End While

        'then move downstream
        curReach = Reaches.Values(StartReachIdx)
        curNode = curReach.en
        Done = False

        'search downstream
        While Not Done
            If NodesDat.NodesDatNodeRecords.records.ContainsKey(curNode.ID.Trim.ToUpper) Then
                NodeRecord = NodesDat.NodesDatNodeRecords.records(curNode.ID.Trim.ToUpper)
                If NodeRecord.ni = 1 Then
                    If NodeRecord.r2.Trim.ToUpper = curReach.Id.Trim.ToUpper Then
                        curReach = Reaches(NodeRecord.r1.Trim.ToUpper)
                        ReachList.Add(curReach)
                        If curReach.bn.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.bn
                        ElseIf curReach.en.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.en
                        End If
                    ElseIf NodeRecord.r1.Trim.ToUpper = curReach.Id.Trim.ToUpper Then
                        curReach = Reaches(NodeRecord.r2.Trim.ToUpper)
                        ReachList.Add(curReach)
                        If curReach.bn.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.bn
                        ElseIf curReach.en.ID.Trim.ToUpper <> curNode.ID.Trim.ToUpper Then
                            curNode = curReach.en
                        End If
                    Else
                        'the current reach is not the one being interpolated at this node. So move on
                        Done = True
                    End If
                Else
                    Done = True
                End If
            Else
                Done = True
            End If
        End While

        Return ReachList

    End Function

    Friend Function GetDownReaches(ByVal upReach As clsSbkReach) As Dictionary(Of String, clsSbkReach)
        'returns a dictionary containing all downstream connected reaches for a given reach
        Dim myDict As New Dictionary(Of String, clsSbkReach)
        For Each myReach As clsSbkReach In Reaches.Values
            If myReach.bn.ID = upReach.en.ID Then
                myDict.Add(myReach.Id.Trim.ToUpper, myReach)
            ElseIf myReach.en.ID = upReach.en.ID AndAlso myReach.Id <> upReach.Id Then
                myDict.Add(myReach.Id.Trim.ToUpper, myReach)
            End If
        Next
        Return myDict
    End Function

    Friend Function CollectByIDBase(ByVal ID As String) As List(Of clsSbkReach)
        Dim myList As New List(Of clsSbkReach)
        Dim i As Long, myReach As clsSbkReach

        'this function makes a list of reaches that have the same ID-base
        'eg. rInGPG20 and rOutGPG20 will end up in the collection if ID = GPG20
        For i = 0 To Reaches.Count - 1
            myReach = Reaches.Values(i)
            If Right(myReach.Id.Trim.ToUpper, ID.Trim.Length) = ID.Trim.ToUpper Then myList.Add(myReach)
        Next
        Return myList

    End Function

    Public Function GetnextReach(ByRef curReach As clsSbkReach, ByRef ReachNode As clsSbkReachNode, ByVal IgnoreDisabledReaches As Boolean) As clsSbkReach

        'finds the next reach connected to a given reachnode with a given first reach
        For Each myReach As clsSbkReach In Reaches.Values
            If myReach.bn.ID = ReachNode.ID OrElse myReach.en.ID = ReachNode.ID Then
                If Not myReach.Id = curReach.Id AndAlso (IgnoreDisabledReaches = False OrElse myReach.InUse = True) Then
                    Return myReach
                End If
            End If
        Next
        Return Nothing

    End Function

    Friend Function CollectByConnectionNode(ByVal ID As String, ByVal SkipInactiveReaches As Boolean) As clsSbkReaches
        Dim myCollection = New clsSbkReaches(Me.Setup, Me.SbkCase)

        For Each myReach As clsSbkReach In Reaches.Values
            If SkipInactiveReaches = False OrElse myReach.InUse Then
                If myReach.bn.ID = ID OrElse myReach.en.ID = ID Then
                    myCollection.Reaches.Add(myReach.Id.Trim.ToUpper, myReach)
                End If
            End If
        Next
        Return myCollection
    End Function

    Friend Sub BuildFromShapeFile()
        'vervaardigt sobektakken uit de aangeleverde shapefile met reaches
        Dim i As Integer, j As Integer
        Dim myShape As MapWinGIS.Shape
        Dim myReach As clsSbkReach
        'Dim bn As clsSbkReachNode, en As clsSbkReachNode
        Dim myCP As clsSbkVectorPoint
        Dim nextDist As Double = 0

        Me.Setup.GISData.ChannelDataSource.Shapefile.sf.ExplodeShapes(False)
        For i = 0 To Me.Setup.GISData.ChannelDataSource.Shapefile.sf.NumShapes - 1
            myShape = Me.Setup.GISData.ChannelDataSource.Shapefile.sf.Shape(i)
            myReach = New clsSbkReach(Me.Setup, Me.SbkCase)

            'zoek de beginknoop
            myReach.Id = Me.Setup.GISData.ChannelDataSource.GetTextValue(i, GeneralFunctions.enmInternalVariable.ID, GeneralFunctions.enmMessageType.ErrorMessage)
            myReach.InUse = True
            For j = 0 To myShape.numPoints - 2
                myCP = New clsSbkVectorPoint(Me.Setup)
                myCP.X = myShape.Point(j).x
                myCP.Y = myShape.Point(j).y
                myCP.Dist = nextDist
                myCP.Angle = Setup.GeneralFunctions.LineAngleDegrees(myShape.Point(j).x, myShape.Point(j).y, myShape.Point(j + 1).x, myShape.Point(j + 1).y)
                nextDist += Setup.GeneralFunctions.Pythagoras(myShape.Point(j).x, myShape.Point(j).y, myShape.Point(j + 1).x, myShape.Point(j + 1).y)
                myReach.NetworkcpRecord.CPTable.CP.Add(myCP)
            Next
        Next
    End Sub

    Friend Function BuildCalculationGrid(ApplyAttributeLength As Boolean, ByVal OverAll As Double, ByVal MergeDist As Double, ByVal Culverts As Double, ByVal Pumps As Double, ByVal Weirs As Double, ByVal Orifices As Double, ByVal Bridges As Double, ByVal OtherStructures As Double, ByVal QBounds As Double, ByVal HBounds As Double, Optional ByVal ReplaceCPs As Boolean = True, Optional ByVal ReplaceFixedCPs As Boolean = False, Optional ByVal Prefix As String = "fxcp", Optional ByVal ReachID As Boolean = False, Optional ByVal AddIndexNumber As Boolean = True, Optional ByVal AddChainage As Boolean = False, Optional ByVal SkipReachesWithPrefix As String = "") As Boolean
        Try
            Dim i As Integer = 0, n As Integer = Reaches.Values.Count
            Dim IDBase As String
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building calculation grid...", 0, 10)
            For Each myReach As clsSbkReach In Reaches.Values
                If myReach.InUse AndAlso Not myReach.IsUrban Then
                    i += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                    IDBase = Prefix & myReach.Id
                    myReach.BuildCalculationGrid(ApplyAttributeLength, OverAll, MergeDist, IDBase, Culverts, Pumps, Weirs, Orifices, Bridges, OtherStructures, QBounds, HBounds, ReplaceCPs, ReplaceFixedCPs, AddIndexNumber, AddChainage)
                    myReach.OptimizeCalculationGrid(0.05)
                End If
            Next
            Me.Setup.Log.AddMessage("Calculation grid was successfully created.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function BuildCalculationGrid of class clsSbkReaches.")
            Return False
        End Try
    End Function

    Friend Function ExportCalculationGrid() As Boolean
        Try
            Using bnaWriter As New System.IO.StreamWriter(Setup.Settings.ExportDirSOBEK & "\calculationpoints.bna")
                For Each myReach As clsSbkReach In Reaches.Values
                    If Not myReach.IsUrban Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.InUse AndAlso (myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint OrElse myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed) Then
                                bnaWriter.WriteLine(Setup.GeneralFunctions.BNAString(myObj.ID, myObj.ID, myObj.X, myObj.Y))
                            End If
                        Next
                    End If
                Next
                bnaWriter.Close()
            End Using
            Me.Setup.Log.AddMessage("Calculation grid successfully written.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ExportCalculationGrid of class clsSbkReaches.")
        End Try
    End Function

    Friend Sub SetReachObjectTypeFromOBI(ByVal myRecord As clsNetworkOBIOBIDRecord)
        For Each myReach As clsSbkReach In Reaches.Values
            If myReach.ReachObjects.ReachObjects.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                Call myReach.ReachObjects.AddNetworkOBIRecord(myRecord) 'tak gevonden. Voeg OBI-record toe aan dit takobject
                Exit For
            End If
        Next
    End Sub

    Friend Function GetReach(ByVal ID As String) As clsSbkReach
        If Reaches.ContainsKey(ID.Trim.ToUpper) Then
            Return Reaches.Item(ID.Trim.ToUpper)
        End If
        Return Nothing
    End Function

    Friend Function GetByOriginalID(ByVal ID As String) As List(Of clsSbkReach)
        'this function returns a list of reaches that all originate from the same reach that has later been split
        Dim myList As New List(Of clsSbkReach)
        For Each myReach As clsSbkReach In Reaches.Values
            If myReach.OriginalID = ID Then
                myList.Add(myReach)
            End If
        Next
        Return myList
    End Function

    Friend Sub AddNetworkTPBRCHRecord(ByVal myRecord As clsNetworkTPBrchRecord, ByVal StructureReachPrefix As String)
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If Reaches.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            myReach = Reaches(myRecord.ID.Trim.ToUpper)
            myReach.NetworkTPBrchRecord = myRecord
            If StructureReachPrefix.Trim <> "" Then
                If Left(myReach.Id.Trim.ToUpper, StructureReachPrefix.Trim.Length) = StructureReachPrefix.Trim.ToUpper Then myReach.ChannelUsage = GeneralFunctions.enmChannelUsage.LINESTRUCTURE
            End If
        Else
            myReach = New clsSbkReach(Me.Setup, Me.SbkCase)
            myReach.InUse = True
            myReach.Id = myRecord.ID
            myReach.NetworkTPBrchRecord = myRecord
            If StructureReachPrefix.Trim <> "" Then
                If Left(myReach.Id.Trim.ToUpper, StructureReachPrefix.Trim.Length) = StructureReachPrefix.Trim.ToUpper Then myReach.ChannelUsage = GeneralFunctions.enmChannelUsage.LINESTRUCTURE
            End If
            Call Reaches.Add(myReach.Id.Trim.ToUpper, myReach)
        End If
    End Sub

    Friend Sub AddNetworkCPRecord(ByVal myRecord As clsNetworkCPRecord)

        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        If Reaches.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            myReach = Reaches(myRecord.ID.Trim.ToUpper)
            myReach.NetworkcpRecord = myRecord
        Else
            myReach = New clsSbkReach(Me.Setup, Me.SbkCase)
            myReach.Id = myRecord.ID.Trim.ToUpper
            myReach.NetworkcpRecord = myRecord
            Call Reaches.Add(myReach.Id.Trim.ToUpper, myReach)
        End If

    End Sub
    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub AddCrossSectionsIfNotPresent()
        Dim Done As Boolean = False
        Dim i As Long = 0

        If SbkCase.CFDataRead Then
            Try
                While Not Done
                    i += 1
                    Done = True
                    Dim ProfileID As String = ""
                    'maakt een dwarsprofiel aan voor elke tak waar die ontbreekt (=kopie van aangrenzende)
                    For Each myReach As clsSbkReach In Reaches.Values
                        'als er alsnog een dwarsprofiel wordt toegevoegd, zetten we done weer op false
                        If Not myReach.ContainsProfile(ProfileID) Then
                            Done = False
                            myReach.AddCrossSectionFromNeighbors()
                        End If
                    Next
                    If i > 10 Then Done = True 'veiligheidsklep
                End While
            Catch ex As Exception
                Dim log As String = "Error in AddCrossSectionsIfNotPresent"
                Me.Setup.Log.AddError(log + ": " + ex.Message)
                Throw New Exception(log, ex)
            End Try
        End If
    End Sub

    Public Function BuildInitialDatRecordsFromTargetLevels(TargetLevel As GeneralFunctions.enmTargetLevelType, minDepth As Double, addDepth As Double) As Boolean
        'creëert een initial.dat-record voor iedere tak, aan de hand van de in ReachNodes ingevulde streefpeilen
        Dim upProfile As clsSbkReachObject
        Dim downProfile As clsSbkReachObject
        Dim i As Integer, n As Integer = Reaches.Count
        Dim InitialDatFLINRecord As clsInitialDatFLINRecord

        Setup.GeneralFunctions.UpdateProgressBar("Building initial.dat records...", 0, 10, True)
        Try
            For Each myReach As clsSbkReach In Reaches.Values
                If myReach.InUse Then
                    i += 1
                    InitialDatFLINRecord = Nothing
                    Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                    upProfile = myReach.GetFirstProfile(myReach)
                    downProfile = myReach.GetLastProfile(myReach)
                    If myReach.buildInitialDatRecordFromTargetLevels(TargetLevel, upProfile, downProfile, minDepth, addDepth, InitialDatFLINRecord) Then
                        If Not SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.ContainsKey(InitialDatFLINRecord.ID.Trim.ToUpper) Then
                            SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.Add(InitialDatFLINRecord.ID.Trim.ToUpper, InitialDatFLINRecord)
                        End If
                    Else
                        Me.Setup.Log.AddWarning("No initial.dat record written for reach " & myReach.Id)
                    End If
                End If
            Next
            Me.Setup.Log.AddMessage("Initial.dat records were successfully built.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function BuildInitialDatRecordsFromTargetLevels.")
            Return False
        End Try
    End Function

End Class
