Imports STOCHLIB.General
Public Class cls1DBranch
    Private Setup As clsSetup
    Private FlowFM As clsFlowFMComponent
    Public ID As String
    Friend RouteNumber As Integer = -1
    Friend VectorPoints As Dictionary(Of Double, clsXY) 'key = chainage
    Friend bn As cls1DNode
    Friend en As cls1DNode

    Friend MeshNodes As New Dictionary(Of Double, cls1DMeshNode)
    Friend BoundingBox As clsBoundingBox

    Public Sub New(ByRef mySetup As clsSetup, ByRef myFlowFM As clsFlowFMComponent, myID As String)
        Setup = mySetup
        FlowFM = myFlowFM
        ID = myID
        VectorPoints = New Dictionary(Of Double, clsXY)
    End Sub

    Public Function calculateLength() As Double
        Dim TotalLength As Double = 0
        For i = 1 To VectorPoints.Count - 1
            TotalLength += Setup.GeneralFunctions.Pythagoras(VectorPoints.Values(i).X, VectorPoints.Values(i).Y, VectorPoints.Values(i - 1).X, VectorPoints.Values(i - 1).Y)
        Next
        Return TotalLength
    End Function

    Public Function getFirstUpstreamObservationPoint(startChainage As Double, ByRef Observationpoint As cls1DBranchObject) As Boolean
        Try
            'this routine searches the first observationpoint on the given branch with a given start chainage, searching in upstream direction
            Dim MinDist As Double = 9.0E+99
            Dim Found As Boolean = False

            'walk through all observation points in the collection and check if they're located on this branch
            'if so, check if they're upstream of the startchainage
            'if so, check if they're closer to the startchainage than any previously found upstream observation point
            For Each obs As cls1DBranchObject In FlowFM.Observationpoints1D.Values
                If obs.Branch.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                    If obs.Chainage <= startChainage Then
                        If (startChainage - obs.Chainage) < MinDist Then
                            Found = True
                            MinDist = startChainage - obs.Chainage
                            Observationpoint = obs
                        End If
                    End If
                End If
            Next
            Return Found
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function AddMeshNode(ByRef myMeshNode As cls1DMeshNode) As Boolean
        Try
            If MeshNodes.ContainsKey(myMeshNode.Chainage) Then Throw New Exception("meshnode with chainage of " & myMeshNode.Chainage & " already present in the collection.")
            MeshNodes.Add(myMeshNode.Chainage, myMeshNode)
        Catch ex As Exception

        End Try
    End Function

    Public Function AddVectorPoint(X As Double, Y As Double, Chainage As Double) As Boolean
        Try
            If VectorPoints.ContainsKey(Chainage) Then Throw New Exception("Possibly multiple vector points at chainage " & Chainage & " of branch " & ID & ". Point was skipped.")
            VectorPoints.Add(Chainage, New clsXY(X, Y))
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddWarning("Error in function AddVectorPoint of class cls1DBranch: " & ex.Message)
            Return False
        End Try
    End Function
    Public Function calcBoundingBox() As Boolean
        Try
            BoundingBox = New clsBoundingBox
            Dim i As Integer
            For i = 0 To VectorPoints.Values.Count - 1
                If VectorPoints.Values(i).X < BoundingBox.XLL Then BoundingBox.XLL = VectorPoints.Values(i).X
                If VectorPoints.Values(i).X > BoundingBox.XUR Then BoundingBox.XUR = VectorPoints.Values(i).X
                If VectorPoints.Values(i).Y < BoundingBox.YLL Then BoundingBox.YLL = VectorPoints.Values(i).Y
                If VectorPoints.Values(i).Y > BoundingBox.YUR Then BoundingBox.YUR = VectorPoints.Values(i).Y
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function CalcSnapLocation(X As Double, Y As Double, ByVal SearchRadius As Double, ByRef SnapChainage As Double, ByRef SnapDistance As Double) As Boolean
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

            If VectorPoints.Count = 0 Then Throw New Exception("Error: no curving points available for Network.CP record " & ID & ".")

            For i = 0 To VectorPoints.Count - 2

                If Setup.GeneralFunctions.PointToLineSnapping(VectorPoints.Values(i).X, VectorPoints.Values(i).Y, VectorPoints.Values(i + 1).X, VectorPoints.Values(i + 1).Y, X, Y, SearchRadius, snapChainageSegment, snapDistanceSegment) Then
                    'we found a valid snapping point within the search radius! 
                    If snapDistanceSegment < minSnapDist Then
                        snapChainageReach = sumDist + snapChainageSegment
                        minSnapDist = snapDistanceSegment
                        Found = True
                    End If
                End If

                'also look for snapping to the vector points, if allowed
                If Setup.GeneralFunctions.Pythagoras(VectorPoints.Values(i).X, VectorPoints.Values(i).Y, X, Y) < minSnapDist Then
                    snapChainageReach = sumDist
                    minSnapDist = Setup.GeneralFunctions.Pythagoras(VectorPoints.Values(i).X, VectorPoints.Values(i).Y, X, Y)
                    Found = True
                End If

                'we searched segment k so add its length to the variable curDist
                sumDist += Setup.GeneralFunctions.Pythagoras(VectorPoints.Values(i).X, VectorPoints.Values(i).Y, VectorPoints.Values(i + 1).X, VectorPoints.Values(i + 1).Y)
            Next

            'test the last vector point as well
            If Setup.GeneralFunctions.Pythagoras(VectorPoints.Values(VectorPoints.Count - 1).X, VectorPoints.Values(VectorPoints.Count - 1).Y, X, Y) < minSnapDist Then
                snapChainageReach = sumDist
                minSnapDist = Setup.GeneralFunctions.Pythagoras(VectorPoints.Values(i).X, VectorPoints.Values(i).Y, X, Y)
                Found = True
            End If

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


    Public Function GetCoordsFromChainage(Chainage As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        Try
            Dim i As Integer
            For i = 1 To VectorPoints.Count - 1
                If VectorPoints.Keys(i) >= Chainage Then
                    X = Me.Setup.GeneralFunctions.Interpolate(VectorPoints.Keys(i - 1), VectorPoints.Values(i - 1).X, VectorPoints.Keys(i), VectorPoints.Values(i).X, Chainage)
                    Y = Me.Setup.GeneralFunctions.Interpolate(VectorPoints.Keys(i - 1), VectorPoints.Values(i - 1).Y, VectorPoints.Keys(i), VectorPoints.Values(i).Y, Chainage)
                    Return True
                End If
            Next
            Throw New Exception("")
        Catch ex As Exception
            Me.Setup.Log.AddError("Unable to get coordinates from branch " & ID & " for chainage " & Chainage & ": " & ex.Message)
            Return False
        End Try
    End Function

End Class
