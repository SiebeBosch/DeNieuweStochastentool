Option Explicit On

Imports STOCHLIB.General

Public Class clsSbkReachObject

    Public ID As String
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend Lat As Double
    Friend Lon As Double
    Friend ci As String                 'reachID
    Friend lc As Double                 'afstand op de tak
    Friend nt As STOCHLIB.GeneralFunctions.enmNodetype
    Friend SubType As STOCHLIB.GeneralFunctions.enmNodeSubType
    Friend ProfileType As STOCHLIB.GeneralFunctions.enmProfileType  'what type of cross section are we talking about here?
    Friend Angle As Double              'the angle of the vector this reach object is located on
    Friend ReachObjectOrderIdx As Long  'indexnummer van het object op de tak (oplopende volgorde startend bij 1 want startknoop tak =0)
    Friend NeedsCalculationPoints As Boolean
    Friend dxUp As Double                 'spacing for computational grid upstream side
    Friend dxDn As Double                 'spacing for computational grid downstream side
    Friend calcSegMin As Double           'the minimum chainage value that is allowed for our calculation segment
    Friend calcSegMax As Double           'the absolute maximum chainage value that is allowed for our calculation segment
    Friend QualityIdx As Integer  'a quality indicator for the source used to create this object. Lower = better

    Friend InUse As Boolean
    Friend Shift As Double                      'keeps track of how this object needs to be shifted along its reach in the end (positive = in reach direction, negative = against reach direction)
    Friend IsGridPoint As Boolean               'for objects that can ALSO be calculation points
    Friend SnapDistance As Double               'metadata for later reference: represents the distance between the original location and the final snapping location
    Friend snapChainage As Double               'metadata for later reference: represents the original snapping chainage on its reach

    'afgeleide gegevens
    Friend HStats As clsHisFileStats
    Friend DStats As clsHisFileStats
    Friend Embankment As clsEmbankment
    Friend ElevationProfile As clsSobekTable

    Friend NetworkObiRecord As clsNetworkOBIOBIDRecord       'bevat beschrijving objecttypes
    Friend NetworkCRRecord As clsNetworkCRRecord         'bevat ligging cross sections
    Friend NetworkSTRecord As clsNetworkSTRecord         'bevat ligging structures
    Friend NetworkMERecord As clsNetworkMErecord         'bevat ligging measurement stations
    Friend NetworkCNFLBXRecord As clsNetworkCNFLBXRecord 'bevat ligging Laterals
    Friend NetworkCNFLBRRecord As clsNetworkCNFLBRRecord 'bevat ligging RRCF-connections
    Friend NetworkTPNDLKRecord As clsNetworkTPNdlkRecord 'bevat ligging Linkage Nodes
    Friend NodeTPRecord As clsRRNodeTPRecord             'bevat de topologie vanuit het perspectief van RR (uitsluitend voor RR-on-flow-connections)


    Friend Waterlevel As Double                         'a temporary storage location for water level
    Friend WettedPerimeter As Double                    'a temporary storage location for wetted perimeter

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase


    Friend Sub New(ByRef mySetup As clsSetup, ByRef mySbkCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = mySbkCase

        'Init classes:
        NetworkObiRecord = New clsNetworkOBIOBIDRecord(Me.Setup)       'bevat beschrijving objecttypes
        NetworkCRRecord = New clsNetworkCRRecord(Me.Setup)         'bevat ligging cross sections
        NetworkSTRecord = New clsNetworkSTRecord(Me.Setup)         'bevat ligging structures
        NetworkMERecord = New clsNetworkMErecord(Me.Setup)         'bevat ligging measurement stations
        NetworkCNFLBXRecord = New clsNetworkCNFLBXRecord(Me.Setup) 'bevat ligging Laterals
        NetworkCNFLBRRecord = New clsNetworkCNFLBRRecord(Me.Setup) 'bevat ligging RRCF-connections

    End Sub


    Friend Sub InitializeEmbankment()
        Embankment = New clsEmbankment
    End Sub

    Public Function PlaceOnReach(InitialReachID As String, Chainage As Double) As Boolean
        'new function in v1.880: places the current object on an existing reach for a given reachid and chainage
        'IMPORTANT: in case of overshoot or undershoot of the current reach, it will search for the most appropriate neighboring reach
        Dim initReach As clsSbkReach = SbkCase.CFTopo.GetReachByID(InitialReachID)
        Dim newReach As New clsSbkReach(Me.Setup, Me.SbkCase), newChainage As Double
        Dim UpstreamReachesSearched As New List(Of String)
        Dim DownstreamReachesSearched As New List(Of String)
        UpstreamReachesSearched.Add(Me.ci.Trim.ToUpper)
        DownstreamReachesSearched.Add(Me.ci.Trim.ToUpper)
        If Chainage < 0 Then
            'we're dealing with an undershoot. Find the most suitable upstream reach + chainage
            initReach.getUpstreamLocation(Chainage, 0, newReach, newChainage, UpstreamReachesSearched)
            ci = newReach.Id
            lc = newChainage
        ElseIf Chainage > initReach.getReachLength Then
            'we're dealing with an overshoot. Find the most suitable downstream reach + chainage
            initReach.getDownstreamLocation(Chainage, 0, newReach, newChainage, DownstreamReachesSearched)
            ci = newReach.Id
            lc = newChainage
        Else
            'this is simple. There is sufficient space on the initial reach to place this object
            ci = InitialReachID
            lc = Chainage
            Return True
        End If
    End Function
    Public Function ExportToShape() As MapWinGIS.Shape
        Try
            'exports the current reachobject as an ESRI shape
            Dim objectShape As New MapWinGIS.Shape
            objectShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
            If X = 0 OrElse Y = 0 Then calcXY() 'introduced in v1.798 to prevent reach objects without computed coordinates to be written to shapefiles
            objectShape.AddPoint(X, Y)
            Return objectShape
        Catch ex As Exception
            Me.Setup.Log.AddError("Error converting reach object to shape: " & ID)
            Return Nothing
        End Try
    End Function

    Public Function IsCalculationPoint()
        If nt = GeneralFunctions.enmNodetype.ConnNodeLatStor Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeSFManhole Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeSFManholeWithDischargeAndRunoff Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeSFManholeWithLateralFlow Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeSFManholeWithMeasurement Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeSFManholeWithRunoff Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeCFBoundary Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeCFConnectionNode Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeCFGridpoint Then
            Return True
        ElseIf nt = GeneralFunctions.enmNodetype.NodeCFGridpointFixed Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Sub getHStats(ByRef CalcPnt As clsHisFileBinaryReader)
        HStats = New clsHisFileStats(Me.Setup)
        'HStats.calculate(CalcPnt, Me.ID, 1)
    End Sub

    Public Function getX() As Double
        If X = 0 Then calcXY()
        Return X
    End Function

    Public Function getY() As Double
        If Y = 0 Then calcXY()
        Return Y
    End Function

    Friend Function BNAString() As String
        Return Me.Setup.GeneralFunctions.BNAString(ID, Name, X, Y)
    End Function

    Friend Function makeMapWinGisPoint() As MapWinGIS.Point
        calcXY()
        Dim myPoint As New MapWinGIS.Point
        myPoint.x = X
        myPoint.y = Y
        Return myPoint
    End Function

    Public Function GetID() As String
        Return ID
    End Function

    Public Function getReachID() As String
        Return ci
    End Function

    Friend Sub ElevationProfileFromGrid(ByVal SearchRadius As Integer)
        'Author: Siebe Bosch
        'Date: 28-4-2013
        'Description: this routine builds a table that contains distance-elevation data for the current reach object
        'It derives the elevation data from the elevation grid
        'consider the object coordinate as the point of origin when calculating polar coordinates
        ElevationProfile = New clsSobekTable(Me.Setup)

        Dim myDist As Double, dX As Double, dY As Double, myElevation As Double
        Dim perpAngleDeg As Double, perpAngleRad As Double

        perpAngleDeg = Me.Setup.GeneralFunctions.NormalizeAngle(Angle + 90)
        For myDist = -SearchRadius To SearchRadius Step 1
            If myDist < 0 Then
                perpAngleRad = Me.Setup.GeneralFunctions.D2R(Me.Setup.GeneralFunctions.NormalizeAngle(perpAngleDeg + 180))        'draai de hoek tijdelijk om omdat we eerst naar de linker embankment kijken
            Else
                perpAngleRad = Me.Setup.GeneralFunctions.D2R(perpAngleDeg)                                               'hoek hoeft niet te worden gedraaid
            End If
            dX = Math.Abs(myDist) * Math.Cos(perpAngleRad)
            dY = Math.Abs(myDist) * Math.Sin(perpAngleRad)

            Me.Setup.GISData.ElevationGrid.GetCellValueFromXY(X + dX, Y + dY, myElevation)                    'get the elevation value from the grid
            ElevationProfile.AddDataPair(1, myDist, myElevation)                                              'add this coordinate to the table
        Next

    End Sub


    Friend Sub calcAngle()
        'Author: Siebe Bosch
        'Date: 28-4-2013
        'Description: this routine retrieves the angle of the vector the current reach object is located on
        Dim myReach As clsSbkReach
        myReach = SbkCase.CFTopo.Reaches.GetReach(ci)
        Angle = myReach.NetworkcpRecord.getAngle(lc)
    End Sub

    Friend Function isQBound() As Boolean
        If SubType = STOCHLIB.GeneralFunctions.enmNodeSubType.QBoundary Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isStructure() As Boolean
        Select Case nt
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFExtraResistance
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir
                Return True
            Case Else
                Return False
        End Select
    End Function


    Friend Function IsReachNode() As Boolean
        Select Case nt
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
                Return True
            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode
                Return True
            Case Else
                Return False
        End Select
    End Function

    Friend Function isBoundary() As Boolean
        If nt = GeneralFunctions.enmNodetype.NodeCFBoundary Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isLateral() As Boolean
        If nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral OrElse
         nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isMeas() As Boolean
        If nt = STOCHLIB.GeneralFunctions.enmNodetype.MeasurementStation Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function IsUrbanType() As Boolean
        If nt = GeneralFunctions.enmNodetype.NodeSFExternalPump OrElse
                nt = GeneralFunctions.enmNodetype.NodeSFManhole OrElse
                nt = GeneralFunctions.enmNodetype.NodeSFManholeWithDischargeAndRunoff OrElse
                nt = GeneralFunctions.enmNodetype.NodeSFManholeWithLateralFlow OrElse
                nt = GeneralFunctions.enmNodetype.NodeSFManholeWithMeasurement OrElse
                nt = GeneralFunctions.enmNodetype.NodeSFManholeWithRunoff Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isWaterLevelObject() As Boolean
        If nt = GeneralFunctions.enmNodetype.ConnNodeLatStor OrElse
        nt = GeneralFunctions.enmNodetype.NodeCFBoundary OrElse
        nt = GeneralFunctions.enmNodetype.NodeCFConnectionNode OrElse
        nt = GeneralFunctions.enmNodetype.NodeCFGridpoint OrElse
        nt = GeneralFunctions.enmNodetype.NodeCFGridpointFixed OrElse
        nt = GeneralFunctions.enmNodetype.NodeCFLinkage OrElse
        nt = GeneralFunctions.enmNodetype.NodeSFManhole OrElse
        nt = GeneralFunctions.enmNodetype.NodeSFManholeWithLateralFlow OrElse
        nt = GeneralFunctions.enmNodetype.NodeSFManholeWithDischargeAndRunoff OrElse
        nt = GeneralFunctions.enmNodetype.NodeSFManholeWithMeasurement OrElse
        nt = GeneralFunctions.enmNodetype.NodeSFManholeWithRunoff OrElse
        nt = GeneralFunctions.enmNodetype.NodeRRCFConnectionNode Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isLinkage() As Boolean
        If nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function calcXY(Optional ByVal IncludeWGS84 As Boolean = False) As Boolean
        Try
            If ci Is Nothing Then Throw New Exception("Reach Object is not a member of a Reach: " & ID)
            Dim myReach As clsSbkReach = SbkCase.CFTopo.Reaches.GetReach(ci.Trim.ToUpper)
            If Not myReach Is Nothing Then
                myReach.NetworkcpRecord.CalcXY(lc, X, Y)
                If IncludeWGS84 Then Call Me.Setup.GeneralFunctions.RD2WGS84(X, Y, Lat, Lon)
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function calcXY while processing reachobject " & ID)
            Return False
        End Try
    End Function


    Friend Sub SetObjectType()
        nt = NetworkObiRecord.GetNodeType
    End Sub

End Class
