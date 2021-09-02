Option Explicit On

Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Friend Class clsNetworkCPRecord
    Friend ID As String
    Friend cp As Integer
    Friend Table As clsSobekTable       'de oorspronkelijke tabel zoals sobek intern gebruikt: afstand - hoek telkens halverwege het lijnelement
    Friend CPTable As clsSobekCPTable   'een aanvullende tabel door onszelf vervaardigd: x, y, afstand, hoek voor ELK curvingpoint

    Private Reach As clsSbkReach
    Private Setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myReach As clsSbkReach)
        Me.Setup = mySetup
        Me.Reach = myReach

        'Init classes:
        Table = New clsSobekTable(Me.Setup)       'de oorspronkelijke tabel zoals sobek intern gebruikt: afstand - hoek telkens halverwege het lijnelement
        CPTable = New clsSobekCPTable(Me.Setup)   'een aanvullende tabel door onszelf vervaardigd: x, y, afstand, hoek voor ELK curvingpoint
    End Sub

    Friend Function Duplicate(ByVal newReachID As String) As clsNetworkCPRecord
        Dim newRecord As New clsNetworkCPRecord(Me.Setup, Me.Reach)
        newRecord.ID = ID
        newRecord.cp = cp
        newRecord.Table = Table.Clone
        newRecord.CPTable = CPTable.Duplicate(newReachID)
        Return newRecord
    End Function

    Public Function CompleteCPTableFromCoordinates() As Boolean
        Try
            'this function completes the CPTable table (Dist, Angle) based on the coordinates it already knows (X and Y)
            CPTable.CP(0).Dist = 0
            For i = 1 To CPTable.CP.Count - 1
                CPTable.CP(i).Dist = CPTable.CP(i - 1).Dist + Setup.GeneralFunctions.Pythagoras(CPTable.CP(i - 1).X, CPTable.CP(i - 1).Y, CPTable.CP(i).X, CPTable.CP(i).Y)
            Next
            For i = 0 To CPTable.CP.Count - 2
                CPTable.CP(i).Angle = Setup.GeneralFunctions.LineAngleDegrees(CPTable.CP(i).X, CPTable.CP(i).Y, CPTable.CP(i + 1).X, CPTable.CP(i + 1).Y)
            Next
            CPTable.CP(CPTable.CP.Count - 1).Angle = 0
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    'let op: CPTable bevat dus de curving points in een wat handzamer formaat dan 
    'aangemaakt door SOBEK, namelijk X, Y, Angle(startpunt), Dist(startpunt)

    'let op: de tabel bevat x-values (=afstand tot halverwege taksegment
    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, tmpStr As String
        Done = False

        While Not Done
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "cp"
                    cp = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ct"
                    tmpStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmpStr = "bc" Then
                        Table = Me.Setup.GeneralFunctions.ParseSobekTable(myRecord)
                    End If
                Case ""
                    Done = True
            End Select
        End While

    End Sub

    Friend Function getAngle(ByVal Dist As Double) As Double
        '--------------------------------------------------------------------------------------------------------------
        'Author: Siebe Bosch
        'Date: 28-4-2013
        'Description: gets the angle of the current reach given a distance on the reach
        '--------------------------------------------------------------------------------------------------------------
        Dim myAngle As Double = CPTable.CP.Item(0).Angle
        For Each myCP As clsSbkVectorPoint In CPTable.CP
            If myCP.Dist > Dist Then
                Return myAngle
            Else
                myAngle = myCP.Angle
            End If
        Next
        Return myAngle

    End Function

    Friend Function CalcXY(ByVal Dist As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 23-05-2012
        'Auteur: Siebe Bosch
        'deze functie berekent, gegeven de afstand op de tak, het bijbehorende X- en Y-coordinaat
        'voorwaarde is wel dat de CPTable gevuld is, dus we werken niet op basis van de interne sobektabel van vector points
        'maar op de omgewerkte tabel. Die rekent namelijk eenvoudiger
        '--------------------------------------------------------------------------------------------------------------
        Dim i As Integer, myCP As clsSbkVectorPoint, nextCP As clsSbkVectorPoint
        Try

            If CPTable Is Nothing Then
                Throw New Exception("Error in function calcXY of class clsNetworkCPRecord. Function could not be called since collection of curving points is null. Please contact Hydroconsult.")
            ElseIf CPTable.CP.Count = 0 Then
                Throw New Exception("Error in function calcXY of class clsNetworkCPRecord. Function could not be called since collection of curving points has zero points. Please contact Hydroconsult.")
            Else
                For i = 0 To CPTable.CP.Count - 2
                    myCP = CPTable.CP(i)
                    nextCP = CPTable.CP(i + 1)
                    If Dist >= myCP.Dist And Dist <= nextCP.Dist Then
                        'we hebben het segment gevonden. Bereken x en y dmv interpolatie van de omringende punten
                        X = Setup.GeneralFunctions.Interpolate(myCP.Dist, myCP.X, nextCP.Dist, nextCP.X, Dist)
                        Y = Setup.GeneralFunctions.Interpolate(myCP.Dist, myCP.Y, nextCP.Dist, nextCP.Y, Dist)
                        Return True
                    End If
                Next

                'niet gevonden, maar accepteer nog een afrondfout bij lc > lengte tak
                nextCP = CPTable.CP(CPTable.CP.Count - 1)

                If Math.Abs(Dist - nextCP.Dist) < 0.01 Then
                    X = nextCP.X
                    Y = nextCP.Y
                    Return True
                End If

            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function CalcSnapLocationInsidePolygon(X As Double, Y As Double, ByVal SearchRadius As Double, ByRef SnapChainage As Double, ByRef SnapDistance As Double, ByVal AllowSnappingToVectorPoints As Boolean, Polygon As MapWinGIS.Shape) As Boolean
        Try
            'siebe: new in v1.77
            'this function calculates the snapping location of a point to the current reach. 
            'returns true if a snapping point was found within the given search radius
            Dim sumDist As Double = 0               'this variable is used to keep track of the distance run on the reach so far
            Dim snapChainageSegment As Double = 0   'the snapping chainage on the current segment
            Dim snapDistanceSegment As Double = 0   'the snapping distance to the current segment
            Dim snapChainageReach As Double = 0     'the snapping chainage on the current reach
            Dim minSnapDist As Double = 9.9E+100    'the smallest snapping distance to the current reach found so far
            Dim i As Long
            Dim Found As Boolean = False
            snapChainageReach = 0                        'initialize the snap chainage to be 0
            With CPTable

                If CPTable.CP.Count = 0 Then Throw New Exception("Error: no curving points available for Network.CP record " & ID & ".")

                For i = 0 To CPTable.CP.Count - 2
                    Dim pt As New MapWinGIS.Point
                    If Setup.GeneralFunctions.PointToLineSnapping(.CP(i).X, .CP(i).Y, .CP(i + 1).X, .CP(i + 1).Y, X, Y, SearchRadius, snapChainageSegment, snapDistanceSegment) Then
                        'we found a valid snapping point within the search radius! Now check if it's inside our polygon
                        If snapDistanceSegment < minSnapDist Then

                            Dim SnappingPoint As New clsSbkReachObject(Me.Setup, Me.Setup.SOBEKData.ActiveProject.ActiveCase)
                            SnappingPoint.ci = Me.Reach.Id
                            SnappingPoint.lc = SnapChainage
                            SnappingPoint.calcXY()
                            pt.x = SnappingPoint.X
                            pt.y = SnappingPoint.Y
                            If Polygon.PointInThisPoly(pt) Then
                                snapChainageReach = sumDist + snapChainageSegment
                                minSnapDist = snapDistanceSegment
                                Found = True
                            End If
                        End If
                    End If

                    'also look for snapping to the vector points, if allowed
                    If AllowSnappingToVectorPoints Then
                        'snapping to a boundary node is not allowed so skip those. Siebe: added for v1.71 on 12-5-2020
                        If Not (i = 0 AndAlso Reach.bn.isBoundary) Then
                            If Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, X, Y) < minSnapDist Then
                                snapChainageReach = sumDist
                                minSnapDist = Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, X, Y)
                                Found = True
                            End If
                        End If
                    End If

                    'we searched segment k so add its length to the variable curDist
                    sumDist += Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, .CP(i + 1).X, .CP(i + 1).Y)
                Next

                'test the last vector point as well
                If AllowSnappingToVectorPoints Then
                    'snapping to a boundary node is not allowed so skip those. Siebe: added for v1.71 on 12-5-2020
                    If Not Reach.en.isBoundary Then
                        If Setup.GeneralFunctions.Pythagoras(.CP(.CP.Count - 1).X, .CP(.CP.Count - 1).Y, X, Y) < minSnapDist Then
                            snapChainageReach = sumDist
                            minSnapDist = Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, X, Y)
                            Found = True
                        End If
                    End If
                End If

            End With


            If Found Then
                SnapChainage = snapChainageReach
                SnapDistance = minSnapDist
            End If
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalcSnapLocation.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function CalcSnapLocation(X As Double, Y As Double, ByVal SearchRadius As Double, ByRef SnapChainage As Double, ByRef SnapDistance As Double, ByVal AllowSnappingToVectorPoints As Boolean) As Boolean
        Try
            'this function calculates the snapping location of a point to the current reach. 
            'returns true if a snapping point was found within the given search radius
            Dim sumDist As Double = 0               'this variable is used to keep track of the distance run on the reach so far
            Dim snapChainageSegment As Double = 0   'the snapping chainage on the current segment
            Dim snapDistanceSegment As Double = 0   'the snapping distance to the current segment
            Dim snapChainageReach As Double = 0     'the snapping chainage on the current reach
            Dim minSnapDist As Double = 9.9E+100    'the smallest snapping distance to the current reach found so far
            Dim i As Long
            Dim Found As Boolean = False
            snapChainageReach = 0                        'initialize the snap chainage to be 0
            With CPTable

                If CPTable.CP.Count = 0 Then Throw New Exception("Error: no curving points available for Network.CP record " & ID & ".")

                For i = 0 To CPTable.CP.Count - 2

                    If Setup.GeneralFunctions.PointToLineSnapping(.CP(i).X, .CP(i).Y, .CP(i + 1).X, .CP(i + 1).Y, X, Y, SearchRadius, snapChainageSegment, snapDistanceSegment) Then
                        'we found a valid snapping point within the search radius! 
                        If snapDistanceSegment < minSnapDist Then
                            snapChainageReach = sumDist + snapChainageSegment
                            minSnapDist = snapDistanceSegment
                            Found = True
                        End If
                    End If

                    'also look for snapping to the vector points, if allowed
                    If AllowSnappingToVectorPoints Then
                        'snapping to a boundary node is not allowed so skip those. Siebe: added for v1.71 on 12-5-2020
                        If Not (i = 0 AndAlso Reach.bn.isBoundary) Then
                            If Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, X, Y) < minSnapDist Then
                                snapChainageReach = sumDist
                                minSnapDist = Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, X, Y)
                                Found = True
                            End If
                        End If
                    End If

                    'we searched segment k so add its length to the variable curDist
                    sumDist += Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, .CP(i + 1).X, .CP(i + 1).Y)
                Next

                'test the last vector point as well
                If AllowSnappingToVectorPoints Then
                    'snapping to a boundary node is not allowed so skip those. Siebe: added for v1.71 on 12-5-2020
                    If Not Reach.en.isBoundary Then
                        If Setup.GeneralFunctions.Pythagoras(.CP(.CP.Count - 1).X, .CP(.CP.Count - 1).Y, X, Y) < minSnapDist Then
                            snapChainageReach = sumDist
                            minSnapDist = Setup.GeneralFunctions.Pythagoras(.CP(i).X, .CP(i).Y, X, Y)
                            Found = True
                        End If
                    End If
                End If
            End With


            If Found Then
                SnapChainage = snapChainageReach
                SnapDistance = minSnapDist
            End If
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalcSnapLocation.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function CalcAll(ReachID As String, ByRef bn As clsSbkReachNode, ByRef en As clsSbkReachNode) As Boolean
        'this function calculates all vector point records we need
        'strangely if we want the property Dist to be set we need to iterate. We need the angle+distance pairs in order to calculate the vector points but we also need the vector points before we can caclculate angle + distance
        Call CalcCPTable(ReachID, bn, en)
        Call CalcTable()
        Call CalcCPTable(ReachID, bn, en)
    End Function

    Friend Function CalcTable() As Boolean
        Try
            Dim i As Long, myDist As Double, lastDist As Double = 0, myAngle As Double, myCP As clsSbkVectorPoint
            Dim segLen As Double
            Table = New clsSobekTable(Me.Setup)
            For i = 1 To CPTable.CP.Count - 1
                myCP = CPTable.CP(i)
                segLen = Setup.GeneralFunctions.Pythagoras(CPTable.CP(i - 1).X, CPTable.CP(i - 1).Y, CPTable.CP(i).X, CPTable.CP(i).Y)
                myDist = lastDist + segLen / 2
                myAngle = Setup.GeneralFunctions.LineAngleDegrees(CPTable.CP(i - 1).X, CPTable.CP(i - 1).Y, CPTable.CP(i).X, CPTable.CP(i).Y)
                lastDist += segLen 'set the last distance equal to the last vector point
                If myDist > 0 Then Table.AddDataPair(2, myDist, myAngle) 'note: we have encountered some invalid coordinates in the past, with dist=0
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
    Friend Sub CalcCPTable(ByVal ReachID As String, ByVal bn As clsSbkReachNode, ByVal en As clsSbkReachNode)
        Try
            'calculates the X and Y coordinates + angle and distance for each vector point and stores it in the CPTable
            'note: this it NOT the internal table as used by SOBEK topo. That one is simply called Table
            Dim segLength As Double
            Dim prevCP As clsSbkVectorPoint
            Dim Idx As Long

            'Me.setup.Log.AddDebugMessage("In CalcCPTable")
            'first clear the existing values since we're recalculating all points
            Me.CPTable.CP.Clear()

            'voeg deze begincoordinaat ook toe als curving point
            Dim myCP As clsSbkVectorPoint = New clsSbkVectorPoint(Me.Setup)
            myCP.X = bn.X
            myCP.Y = bn.Y
            myCP.Angle = Me.Setup.GeneralFunctions.NormalizeAngle(Me.Table.Values1.Values(0))
            myCP.Dist = 0
            myCP.Idx = 0
            myCP.ID = ReachID & "_" & myCP.Idx
            Me.CPTable.CP.Add(myCP)

            If Me.Table.XValues.Count > 1 Then
                For Idx = 1 To Me.Table.XValues.Count - 1                         'doorloop alle Dist/Angle-paren in de tabel

                    'bereken op basis van de VORIGE angle, de VORIGE dist + Segmentlengte de ligging van het volgende buigpunt
                    ' TODO: Door het hergebruiken van variablen is het onduidelijk wat nu wat is
                    ' Je maakt hier twee nieuwe variabelen, geef ze dan ook eigen namen!
                    myCP = New clsSbkVectorPoint(Me.Setup)                                  'huidige buigpunt
                    prevCP = Me.CPTable.CP(CPTable.CP.Count - 1)              'vorige buigpunt

                    segLength = (Me.Table.XValues.Values(Idx - 1) - prevCP.Dist) * 2   'de lengte van het onderhavige taksegment TOT het huidige buigpunt
                    myCP.Dist = prevCP.Dist + segLength                    'de totale lengte tot en met het huidige buigpunt
                    myCP.Angle = Me.Setup.GeneralFunctions.NormalizeAngle(Me.Table.Values1.Values(Idx))          'de hoek van het VOLGENDE taksegment
                    myCP.Idx = Idx
                    myCP.ID = ReachID & "_" & myCP.Idx

                    Select Case prevCP.Angle                               'let op: de ligging van het huidige buigpunt is nog altijd een functie van de vorige hoek
                        Case Is = 0 'recht omhoog
                            myCP.X = prevCP.X
                            myCP.Y = prevCP.Y + segLength
                        Case Is < 90 'eerste kwadrant
                            myCP.X = prevCP.X + Math.Sin(Me.Setup.GeneralFunctions.D2R(prevCP.Angle)) * segLength
                            myCP.Y = prevCP.Y + Math.Cos(Me.Setup.GeneralFunctions.D2R(prevCP.Angle)) * segLength
                        Case Is = 90 'horizontaal naar rechts
                            myCP.X = prevCP.X + segLength
                            myCP.Y = prevCP.Y
                        Case Is < 180 'tweede kwadrant
                            myCP.X = prevCP.X + Math.Cos(Me.Setup.GeneralFunctions.D2R(prevCP.Angle - 90)) * segLength
                            myCP.Y = prevCP.Y - Math.Sin(Me.Setup.GeneralFunctions.D2R(prevCP.Angle - 90)) * segLength
                        Case Is = 180 'recht omlaag
                            myCP.X = prevCP.X
                            myCP.Y = prevCP.Y - segLength
                        Case Is < 270 'derde kwadrant
                            myCP.X = prevCP.X - Math.Sin(Me.Setup.GeneralFunctions.D2R(prevCP.Angle - 180)) * segLength
                            myCP.Y = prevCP.Y - Math.Cos(Me.Setup.GeneralFunctions.D2R(prevCP.Angle - 180)) * segLength
                        Case Is = 270 'naar links
                            myCP.X = prevCP.X - segLength
                            myCP.Y = prevCP.Y
                        Case Is < 360 'vierde kwadrant
                            myCP.X = prevCP.X - Math.Cos(Me.Setup.GeneralFunctions.D2R(prevCP.Angle - 270)) * segLength
                            myCP.Y = prevCP.Y + Math.Sin(Me.Setup.GeneralFunctions.D2R(prevCP.Angle - 270)) * segLength
                        Case Is = 360 'recht omhoog
                            myCP.X = prevCP.X
                            myCP.Y = prevCP.Y + segLength
                    End Select

                    'voeg het buigpunt toe aan de collection
                    Me.CPTable.CP.Add(myCP)
                Next Idx
            End If

            'en tot slot van het laatste buigpunt tot de eindknoop
            prevCP = Me.CPTable.CP(CPTable.CP.Count - 1)                               'vorige buigpunt
            myCP = New clsSbkVectorPoint(Me.Setup)                                          'maak nieuw buigpunt aan
            myCP.X = en.X
            myCP.Y = en.Y
            segLength = (Me.Table.XValues.Values(Me.Table.XValues.Count - 1) - prevCP.Dist) * 2   'de lengte van het onderhavige taksegment TOT het huidige buigpunt
            myCP.Dist = prevCP.Dist + segLength                                  'de totale lengte tot en met het huidige buigpunt
            myCP.Angle = 0                                                       'vanaf hier natuurlijk geen hoek meer nodig

            Me.CPTable.CP.Add(myCP)
        Catch ex As Exception
            Dim log As String = "Error in CalcCPTable"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try
    End Sub

    Friend Function GetPartFromReach(ByRef bn As clsSbkReachNode, ByRef en As clsSbkReachNode, ByVal myReach As clsSbkReach,
                                ByVal fromDist As Double, ByVal toDist As Double) As Boolean
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 17-05-2012
        'Auteur: Siebe Bosch
        'Deze routine extraheert een gedeelte van de curving points uit een andere tak en kent die toe aan de onderhavige
        'wordt gebruikt bij het opsplitsen van een grotere tak in twee kleinere
        '--------------------------------------------------------------------------------------------------------------
        Dim i As Integer
        'Dim VectorLengthNew As Double, VectorLengthOld As Double
        Dim curCP As clsSbkVectorPoint, nextCP As clsSbkVectorPoint
        Dim Started As Boolean = False
        Dim vectorPoint As clsSbkVectorPoint = Nothing
        Dim lastvp As clsSbkVectorPoint = Nothing

        Try
            If fromDist < 0 Then Throw New Exception("Error in function GetPartFromReach of class clsNetworkCPRecord: starting distance for truncating cannot be smaller than zero: " & myReach.Id)
            If toDist < 0 Then Throw New Exception("Error in function GetPartFromReach of class clsNetworkCPRecord: ending distance for truncating cannot be smaller than zero: " & myReach.Id)
            If fromDist > myReach.getReachLength Then Throw New Exception("Error in function GetPartFromReach of class clsNetworkCPRecord: starting distance for truncating cannot be larger than reach length: " & myReach.Id)
            If toDist > myReach.getReachLength Then toDist = myReach.getReachLength

            'to make life easier, we'll first convert the curving points table which only specifies the CENTER of each vector
            'to a CPTable, containing the exact xy-locations and angles of each vector point
            myReach.NetworkcpRecord.CalcCPTable(myReach.Id, myReach.bn, myReach.en)

            'clear the existing table with curving points
            CPTable.CP.Clear()

            For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 2
                curCP = myReach.NetworkcpRecord.CPTable.CP(i)
                nextCP = myReach.NetworkcpRecord.CPTable.CP(i + 1)

                If curCP.Dist <= fromDist Then
                    If nextCP.Dist >= fromDist Then
                        vectorPoint = New clsSbkVectorPoint(Setup)
                        vectorPoint.X = Setup.GeneralFunctions.Interpolate(curCP.Dist, curCP.X, nextCP.Dist, nextCP.X, fromDist)
                        vectorPoint.Y = Setup.GeneralFunctions.Interpolate(curCP.Dist, curCP.Y, nextCP.Dist, nextCP.Y, fromDist)
                        vectorPoint.Dist = 0
                        vectorPoint.Angle = Setup.GeneralFunctions.LineAngleDegrees(vectorPoint.X, vectorPoint.Y, nextCP.X, nextCP.Y)
                        CPTable.CP.Add(vectorPoint)
                        lastvp = vectorPoint
                    End If
                    If nextCP.Dist >= toDist OrElse i = myReach.NetworkcpRecord.CPTable.CP.Count - 2 Then
                        vectorPoint = New clsSbkVectorPoint(Setup)
                        vectorPoint.X = Setup.GeneralFunctions.Interpolate(curCP.Dist, curCP.X, nextCP.Dist, nextCP.X, toDist)
                        vectorPoint.Y = Setup.GeneralFunctions.Interpolate(curCP.Dist, curCP.Y, nextCP.Dist, nextCP.Y, toDist)
                        vectorPoint.Dist = lastvp.Dist + Setup.GeneralFunctions.Pythagoras(lastvp.X, lastvp.Y, vectorPoint.X, vectorPoint.Y)
                        vectorPoint.Angle = 0 'not relevant for the last point
                        CPTable.CP.Add(vectorPoint)
                    End If


                ElseIf curCP.Dist < toDist Then
                    vectorPoint = New clsSbkVectorPoint(Setup)
                    vectorPoint.X = curCP.X
                    vectorPoint.Y = curCP.Y
                    If Not lastvp Is Nothing Then
                        vectorPoint.Dist = lastvp.Dist + Setup.GeneralFunctions.Pythagoras(lastvp.X, lastvp.Y, curCP.X, curCP.Y)
                    Else
                        vectorPoint.Dist = 0
                    End If
                    vectorPoint.Angle = Setup.GeneralFunctions.LineAngleDegrees(curCP.X, curCP.Y, nextCP.X, nextCP.Y)
                    CPTable.CP.Add(vectorPoint)
                    lastvp = vectorPoint

                    If nextCP.Dist >= toDist OrElse i = myReach.NetworkcpRecord.CPTable.CP.Count - 2 Then
                        vectorPoint = New clsSbkVectorPoint(Setup)
                        vectorPoint.X = Setup.GeneralFunctions.Interpolate(curCP.Dist, curCP.X, nextCP.Dist, nextCP.X, toDist)
                        vectorPoint.Y = Setup.GeneralFunctions.Interpolate(curCP.Dist, curCP.Y, nextCP.Dist, nextCP.Y, toDist)
                        vectorPoint.Dist = lastvp.Dist + Setup.GeneralFunctions.Pythagoras(curCP.X, curCP.Y, vectorPoint.X, vectorPoint.Y)
                        vectorPoint.Angle = 0
                        CPTable.CP.Add(vectorPoint)
                        Exit For
                    End If
                End If

            Next

            ''If no curving points were found, this is caused by the required distance lying entirely within the last segment
            'If Table.XValues.Count = 0 Then
            '    Table.AddDataPair(2, Setup.GeneralFunctions.Pythagoras(bn.X, bn.Y, en.X, en.Y) / 2, Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, en.X, en.Y))
            'End If



            'translate this table with vector points to the actual internal table SOBEK needs
            Call CalcTable()

            'For i = 1 To myReach.NetworkcpRecord.CPTable.CP.Count - 2

            '    lastCP = myReach.NetworkcpRecord.CPTable.CP(i - 1)
            '    curCP = myReach.NetworkcpRecord.CPTable.CP(i)
            '    nextCP = myReach.NetworkcpRecord.CPTable.CP(i + 1)

            '    If curCP.Dist >= fromDist AndAlso curCP.Dist < toDist Then
            '        If Not Started Then          'this will be the first record!
            '            Table.AddDataPair(2, (curCP.Dist - fromDist) / 2, lastCP.Angle)
            '            VectorLengthNew = curCP.Dist - fromDist
            '            VectorLengthOld = curCP.Dist
            '            Started = True
            '        Else
            '            Table.AddDataPair(2, VectorLengthNew + (curCP.Dist - VectorLengthOld) / 2, lastCP.Angle)
            '            VectorLengthNew = curCP.Dist - fromDist
            '            VectorLengthOld = curCP.Dist
            '        End If
            '    ElseIf curCP.Dist >= toDist Then
            '        'add the last curving point
            '        Table.AddDataPair(2, VectorLengthNew + (toDist - VectorLengthOld) / 2, lastCP.Angle)
            '        Exit For
            '    End If
            'Next


            'Call CalcCPTable(myReach.Id, bn, en)
            Return True


            'For i = 0 To myReach.NetworkcpRecord.Table.XValues.Count - 1
            '  myDist = myReach.NetworkcpRecord.Table.XValues.Values(i)
            '  myAngle = myReach.NetworkcpRecord.Table.Values1.Values(i)
            '  If myDist >= fromDist And myDist <= toDist Then
            '    Call Table.AddDataPair(2, myDist - fromDist, myAngle)
            '  End If
            'Next

            ''converteer hem nu naar de cptable
            'Call CalcCPTable(myReach.Id, bn, en)
            'Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function
End Class
