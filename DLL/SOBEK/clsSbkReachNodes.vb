Option Explicit On

Imports STOCHLIB.General

Friend Class clsSbkReachNodes
    Friend ReachNodes As New Dictionary(Of String, clsSbkReachNode)
    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Function GetWithinSnappingDistance(X As Double, Y As Double, SnappingDistance As Double) As clsSbkReachNode
        'returns the nearest existing reachnode within snapping distance of a given coordinate
        'returns nothing if no node found
        Dim ResultNode As clsSbkReachNode = Nothing
        Dim minDist As Double = 9.0E+99
        Dim myDist As Double
        For Each myNode As clsSbkReachNode In ReachNodes.Values
            If myNode.InUse Then
                myDist = Me.Setup.GeneralFunctions.Pythagoras(X, Y, myNode.X, myNode.Y)
                If myDist <= SnappingDistance AndAlso myDist < minDist Then
                    minDist = myDist
                    ResultNode = myNode
                End If
            End If
        Next
        Return ResultNode
    End Function

    Friend Sub addPrefix(ByVal Prefix As String)
        For Each myReachNode As clsSbkReachNode In ReachNodes.Values
            myReachNode.ID = Prefix & myReachNode.ID
        Next
    End Sub

    Friend Sub AddNetworkTPNodeRecord(ByVal myRecord As clsNetworkTPNodeRecord)

        Dim myReachNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)
        If ReachNodes.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            myReachNode = ReachNodes(myRecord.ID.Trim.ToUpper)
            myReachNode.NetworkTPNodeRecord = myRecord
        Else
            myReachNode = New clsSbkReachNode(Me.Setup, Me.SbkCase)
            myReachNode.ID = myRecord.ID
            myReachNode.InUse = True
            myReachNode.Name = myRecord.Name
            myReachNode.X = myRecord.X
            myReachNode.Y = myRecord.Y
            myReachNode.NetworkTPNodeRecord = myRecord
            myReachNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
            Call ReachNodes.Add(myReachNode.ID.Trim.ToUpper, myReachNode)
        End If

    End Sub
    Friend Sub AddNetworkTPNdlkRecord(ByVal myRecord As clsNetworkTPNdlkRecord)

        Dim myReachNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)
        If ReachNodes.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            myReachNode = ReachNodes(myRecord.ID.Trim.ToUpper)
            myReachNode.NetworkTPNdlkRecord = myRecord
        Else
            myReachNode = New clsSbkReachNode(Me.Setup, Me.SbkCase)
            myReachNode.ID = myRecord.ID
            myReachNode.Name = myRecord.Name
            myReachNode.X = myRecord.X
            myReachNode.Y = myRecord.Y
            myReachNode.ci = myRecord.ci
            myReachNode.lc = myRecord.lc
            myReachNode.NetworkTPNdlkRecord = myRecord
            myReachNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage
            Call ReachNodes.Add(myReachNode.ID.Trim.ToUpper, myReachNode)
        End If

    End Sub

    Friend Sub SetNodeTypeFromOBI(ByVal myrecord As clsNetworkOBIOBIDRecord)
        Dim myReachNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)

        If ReachNodes.ContainsKey(myrecord.ID.Trim.ToUpper) Then
            myReachNode = ReachNodes(myrecord.ID.Trim.ToUpper)
            myReachNode.NetworkObiRecord = myrecord
            myReachNode.SetNodeType()
        Else
            myReachNode = New clsSbkReachNode(Me.Setup, Me.SbkCase)
            myReachNode.ID = myrecord.ID
            myReachNode.NetworkObiRecord = myrecord
            myReachNode.SetNodeType()
            Call ReachNodes.Add(myReachNode.ID.Trim.ToUpper, myReachNode)
        End If
    End Sub

    Public Function CountLinkageNodes() As Long
        Dim myReachNode As clsSbkReachNode, n As Long
        For Each myReachNode In ReachNodes.Values
            If myReachNode.nt = GeneralFunctions.enmNodetype.NodeCFLinkage Then n += 1
        Next
        Return n
    End Function

    Public Function getExtent(ByRef xMin As Double, ByRef yMin As Double, ByRef xMax As Double, ByRef yMax As Double) As Boolean
        Try
            xMin = 9.0E+99
            yMin = 9.0E+99
            xMax = -9.0E+99
            yMax = -9.0E+99

            For Each myNode As clsSbkReachNode In ReachNodes.Values
                If myNode.InUse Then
                    If myNode.X < xMin Then xMin = myNode.X
                    If myNode.X > xMax Then xMax = myNode.X
                    If myNode.Y < yMin Then yMin = myNode.Y
                    If myNode.Y > yMax Then yMax = myNode.Y
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getExtent of class clsSbkReachNodes.")
            Return False
        End Try

    End Function

    Public Function AssignTargetLevelsFromGeodataSource(ByRef myGeoDataSource As clsGeoDatasource) As Boolean

        'old argument, ByRef TargetLevelsFieldsCollection As List(Of clsTargetLevelFields)

        '--------------------------------------------------------------------------------------------------------
        '  LET OP: MET DEZE ROUTINE ZEER VEEL PROBLEMEN MET OUTOFMEMORYEXCEPTION GEHAD
        '  VANDAAR DE WAT VREEMDE METHODIEK MET VEEL GARBAGE COLLECTION. DIT IS VOORALSNOG DE ENIGE WORKAROUND
        '  PER 26-11-2017
        '--------------------------------------------------------------------------------------------------------

        Try
            'vult de collectie met reachnodes met het onderliggende streefpeil uit een shapefile
            Dim myNode As clsSbkReachNode
            Dim polyIdx As Integer

            myGeoDataSource.Open()
            For Each myNode In ReachNodes.Values

                Select Case myGeoDataSource.PrimaryDataSource
                    Case GeneralFunctions.enmGeoDataSource.Shapefile
                        myGeoDataSource.GetRecordIdxByCoord(myNode.X, myNode.Y, polyIdx)
                        'v1.900: fixed a bug that returned true for (polyIdx = nothing) if PolyIdx in fact yielded 0 (first polygon) 
                        'the IsNothing function seems to work better
                        If Not IsNothing(polyIdx) AndAlso polyIdx >= 0 Then
                            myNode.TargetLevels = New clsTargetLevels
                            myGeoDataSource.getTargetLevels(polyIdx, myNode.TargetLevels)
                        Else
                            Me.Setup.Log.AddWarning("Could not find target levels for node " & myNode.ID)
                        End If
                End Select

            Next
            myGeoDataSource.Close()
            Me.Setup.Log.AddMessage("Target levels were successfully read.")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AssignTargetLevelsFromShapefile of class clsSbkReachNodes.")
            Return False
        End Try

    End Function


End Class
