Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.Windows.Forms

Public Class ClsSobekCase

    Friend NetworkNTW As clsNetworkNTW          'import and export of network via the network.ntw file format
    Friend NetworkNVL As clsNetworkNVL          'import and export of vector points via the network.nvl file format

    Friend RRDataRead As Boolean
    Friend RRTopoRead As Boolean
    Friend CFDataRead As Boolean
    Friend SFDataRead As Boolean
    Friend CFTopoRead As Boolean

    Public CaseName As String
    Friend CaseDir As String
    Friend Prefixes As New clsSBKPrefixes

    Public RRData As clsRRData
    Public CFData As clsCFData
    Public SFData As clsSFData
    Public CFTopo As clsCFTopology
    Public RRTopo As clsRRTopology

    Public NodeTypes As clsSobekNodeTypes                   'nodetypes uit ntrpluv.ini
    Public BranchTypes As clsSobekBranchTypes               'branchtypes uit ntrpluv.ini

    Public RRResults As clsRRResults
    Public CFResults As clsCFResults

    Friend Areas As clsAreas                                'optioneel: bouw alleen areas zonder dummytakjes

    Friend BuiFile As clsBuiFile

    Public InUse As Boolean                                 'is deze case uberhaupt in gebruik bij ons of slaan we hem over
    Public IsActiveCase As Boolean                          'is de huidige case de actieve case

    Public ReferenceCase As ClsSobekCase                    'reference case to compare the current case's results to

    Private Setup As clsSetup
    Private SobekProject As clsSobekProject


    Public Sub New(ByRef mySetup As clsSetup, ByRef myProject As clsSobekProject, ByRef CaseListItem As clsSobekCaseListItem)
        Me.Setup = mySetup
        Me.SobekProject = myProject
        Me.CaseDir = CaseListItem.dir
        Me.CaseName = CaseListItem.name

        'Init classes:
        RRData = New clsRRData(Me.Setup, Me)
        RRTopo = New clsRRTopology(Me.Setup, Me)
        CFData = New clsCFData(Me.Setup, Me)
        SFData = New clsSFData(Me.Setup, Me)
        CFTopo = New clsCFTopology(Me.Setup, Me)
        'ModelCatchments = New clsModelCatchments(Me.Setup, Me)

        '02-01-2019 we have started to read the network.ntw file. This is because it supports urban pipes too, whereas the .sob does not
        NetworkNTW = New clsNetworkNTW(Me.Setup, Me)

        Areas = New clsAreas(Me.Setup)
        BuiFile = New clsBuiFile(Me.Setup)

        NodeTypes = New clsSobekNodeTypes()
        BranchTypes = New clsSobekBranchTypes()

        RRResults = New clsRRResults(Me.Setup, Me)
        CFResults = New clsCFResults(Me.Setup, Me)

    End Sub
    Public Function Get2DResultsFileNames(Parameter As STOCHLIB.GeneralFunctions.enm2DParameter, DomainName As String, Maximum As Boolean, ByRef FileNames As List(Of String)) As Boolean
        Dim FileName As String
        Dim Done As Boolean = False
        Dim ts As Integer = -1
        'v2.2040: added the possibility to also return the grids for all timesteps
        'also added the possibility to return the u-velocity and v-velocity grids
        Try
            While Not Done
                FileName = DomainName
                Select Case Parameter
                    Case enm2DParameter.depth
                        If Maximum Then
                            FileName &= "maxd0.asc"
                            FileNames.Add(FileName)
                            Done = True
                        Else
                            ts += 1
                            FileName = FileName & "d" & Format(ts, "0000") & ".asc"
                            If System.IO.File.Exists(CaseDir & "\" & FileName) Then
                                FileNames.Add(FileName)
                            Else
                                Done = True
                            End If
                        End If
                    Case enm2DParameter.waterlevel
                        If Maximum Then
                            FileName &= "maxh0.asc"
                            FileNames.Add(FileName)
                            Done = True
                        Else
                            ts += 1
                            FileName = FileName & "h" & Format(ts, "0000") & ".asc"
                            If System.IO.File.Exists(CaseDir & "\" & FileName) Then
                                FileNames.Add(FileName)
                            Else
                                Done = True
                            End If
                        End If
                    Case enm2DParameter.velocity
                        If Maximum Then
                            FileName &= "maxc0.asc"
                            FileNames.Add(FileName)
                            Done = True
                        Else
                            ts += 1
                            FileName = FileName & "c" & Format(ts, "0000") & ".asc"
                            If System.IO.File.Exists(CaseDir & "\" & FileName) Then
                                FileNames.Add(FileName)
                            Else
                                Done = True
                            End If
                        End If
                    Case enm2DParameter.u_velocity
                        If Maximum Then
                            FileName &= "maxu0.asc"
                            FileNames.Add(FileName)
                            Done = True
                        Else
                            ts += 1
                            FileName = FileName & "u" & Format(ts, "0000") & ".asc"
                            If System.IO.File.Exists(CaseDir & "\" & FileName) Then
                                FileNames.Add(FileName)
                            Else
                                Done = True
                            End If
                        End If
                    Case enm2DParameter.v_velocity
                        If Maximum Then
                            FileName &= "maxv0.asc"
                            FileNames.Add(FileName)
                            Done = True
                        Else
                            ts += 1
                            FileName = FileName & "v" & Format(ts, "0000") & ".asc"
                            If System.IO.File.Exists(CaseDir & "\" & FileName) Then
                                FileNames.Add(FileName)
                            Else
                                Done = True
                            End If
                        End If
                End Select
            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function
    Public Function Get2DDomainsList() As List(Of String)
        Try
            Dim domainsList As New List(Of String)
            Dim files() As String = IO.Directory.GetFiles(Me.CaseDir)

            Dim Found As Boolean = True
            Dim i As Integer = 0
            While Found
                i += 1
                Found = False
                Dim Domain As String = "dm" & i
                For Each file In files
                    If InStr(file, Domain, CompareMethod.Text) > 0 Then
                        domainsList.Add(Domain)
                        Found = True
                        Exit For
                    End If
                Next
            End While
            Return domainsList
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Function GetNearestObjectFromNeighborReach(ByRef myReach As clsSbkReach, Chainage As Double, NodeType As GeneralFunctions.enmNodetype) As clsSbkReachObject
        Try
            'create a new profile object
            Dim DownReachesReverse As New List(Of clsSbkReach)
            Dim DownReachesSame As New List(Of clsSbkReach)
            Dim UpReachesReverse As New List(Of clsSbkReach)
            Dim UpReachesSame As New List(Of clsSbkReach)

            'make collections of all directly connected reaches
            DownReachesReverse = myReach.GetDownstreamReachesReverse()
            DownReachesSame = myReach.GetDownstreamReachesSameDirection()
            UpReachesReverse = myReach.GetUpstreamReachesReverse()
            UpReachesSame = myReach.GetUpstreamReachesSameDirection()

            Dim curDist As Double
            Dim minDist As Double = 9.0E+99
            Dim ObjectFound As clsSbkReachObject = Nothing
            Dim DistToEnd As Double = myReach.getReachLength - Chainage

            For Each tmpReach As clsSbkReach In DownReachesReverse
                For Each myObj As clsSbkReachObject In tmpReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = NodeType Then
                        curDist = DistToEnd + (tmpReach.getReachLength - myObj.lc)
                        If curDist < minDist Then
                            minDist = curDist
                            ObjectFound = myObj
                        End If
                    End If
                Next
            Next

            For Each tmpReach As clsSbkReach In DownReachesSame
                For Each myObj As clsSbkReachObject In tmpReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = NodeType Then
                        curDist = DistToEnd + myObj.lc
                        If curDist < minDist Then
                            minDist = curDist
                            ObjectFound = myObj
                        End If
                    End If
                Next
            Next

            For Each tmpReach As clsSbkReach In UpReachesReverse
                For Each myObj As clsSbkReachObject In tmpReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = NodeType Then
                        curDist = Chainage + myObj.lc
                        If curDist < minDist Then
                            minDist = curDist
                            ObjectFound = myObj
                        End If
                    End If
                Next
            Next

            For Each tmpReach As clsSbkReach In UpReachesSame
                For Each myObj As clsSbkReachObject In tmpReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = NodeType Then
                        curDist = Chainage + (tmpReach.getReachLength - myObj.lc)
                        If curDist < minDist Then
                            minDist = curDist
                            ObjectFound = myObj
                        End If
                    End If
                Next
            Next

            Return ObjectFound
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in Function GetNearestObjectFromNeighborReach of class clsSobekCase while processing reach " & myReach.Id)
            Return Nothing
        End Try


    End Function


    Public Function AssignMeteoStationByStringMatch(ByRef BuiFile As clsBuiFile, PrefixList As List(Of String), PostfixList As List(Of String), SkipList As List(Of String)) As Boolean
        'this function assigns a meteo station to each node from a shapefile
        Dim iNode As Integer = 0, iReach As Integer = 0
        Dim up3B As clsUnpaved3BRecord, pv3b As clsPaved3BRecord, gr3b As clsGreenhse3BRecord
        Dim BareID As String, BestMS As String

        Me.Setup.GeneralFunctions.UpdateProgressBar("Assigning meteo stations to RR nodes...", 0, 10, True)
        For Each myNode As STOCHLIB.clsRRNodeTPRecord In RRTopo.Nodes.Values

            If Not Me.Setup.GeneralFunctions.RegExListContains(SkipList, myNode.ID, True) Then
                iNode += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iNode, RRTopo.Nodes.Values.Count)
                Select Case myNode.nt.ParentID
                    Case Is = "3B_UNPAVED"
                        up3B = Me.Setup.SOBEKData.ActiveProject.ActiveCase.RRData.GetUnpaved3BRecord(myNode.ID)
                        BareID = Me.Setup.GeneralFunctions.StripPrefixes(myNode.ID, PrefixList)
                        BareID = Me.Setup.GeneralFunctions.StripPostfixes(BareID, PostfixList)
                        BestMS = BuiFile.GetStationByClosestStringMatch(BareID)
                        up3B.setMS(BestMS)
                    Case Is = "3B_PAVED"
                        pv3b = Me.Setup.SOBEKData.ActiveProject.ActiveCase.RRData.GetPaved3BRecord(myNode.ID)
                        BareID = Me.Setup.GeneralFunctions.StripPrefixes(myNode.ID, PrefixList)
                        BareID = Me.Setup.GeneralFunctions.StripPostfixes(BareID, PostfixList)
                        BestMS = BuiFile.GetStationByClosestStringMatch(BareID)
                        pv3b.setMS(BestMS)
                    Case Is = "3B_GREENHOUSE"
                        gr3b = Me.Setup.SOBEKData.ActiveProject.ActiveCase.RRData.GetGreenhouse3BRecord(myNode.ID)
                        BareID = Me.Setup.GeneralFunctions.StripPrefixes(myNode.ID, PrefixList)
                        BareID = Me.Setup.GeneralFunctions.StripPostfixes(BareID, PostfixList)
                        BestMS = BuiFile.GetStationByClosestStringMatch(BareID)
                        gr3b.setMS(BestMS)
                End Select
            End If
        Next

        Me.Setup.GeneralFunctions.UpdateProgressBar("Assigning meteo stations to lateral nodes...", 0, 10, True)
        Dim latFlbr As clsLateralDatFLBRRecord
        For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
            iReach += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, CFTopo.Reaches.Reaches.Values.Count)
            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = enmNodetype.NodeCFLateral AndAlso Not Me.Setup.GeneralFunctions.RegExListContains(SkipList, myObj.ID, True) Then
                    latFlbr = CFData.getLateralDatFLBRRecord(myObj.ID)
                    If latFlbr.dclt1 = 7 Then
                        BareID = Me.Setup.GeneralFunctions.StripPrefixes(myObj.ID, PrefixList)
                        BareID = Me.Setup.GeneralFunctions.StripPostfixes(BareID, PostfixList)
                        BestMS = BuiFile.GetStationByClosestStringMatch(BareID)
                        latFlbr.ms = BestMS
                    End If
                End If
            Next
        Next
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

    End Function

    Public Function AssignMeteoStationByShapefile(ByRef sf As MapWinGIS.Shapefile, FieldIdx As Integer, ExcludeStationsList As List(Of String)) As Boolean
        'this function assigns a meteo station to each node from a shapefile
        Dim shpIdx As Integer, iNode As Integer = 0, iReach As Integer = 0
        Dim up3B As clsUnpaved3BRecord, pv3b As clsPaved3BRecord, gr3b As clsGreenhse3BRecord
        sf.BeginPointInShapefile()

        Me.Setup.GeneralFunctions.UpdateProgressBar("Assigning meteo stations to RR nodes...", 0, 10, True)
        For Each myNode As STOCHLIB.clsRRNodeTPRecord In RRTopo.Nodes.Values
            iNode += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", iNode, RRTopo.Nodes.Values.Count)
            shpIdx = sf.PointInShapefile(myNode.X, myNode.Y)
            If shpIdx >= 0 Then
                Select Case myNode.nt.ParentID
                    Case Is = "3B_UNPAVED"
                        up3B = Me.Setup.SOBEKData.ActiveProject.ActiveCase.RRData.GetUnpaved3BRecord(myNode.ID)
                        If Not Me.Setup.GeneralFunctions.RegExListContains(ExcludeStationsList, up3B.ms, True) Then
                            up3B.setMS(sf.CellValue(FieldIdx, shpIdx))
                        End If
                    Case Is = "3B_PAVED"
                        pv3b = Me.Setup.SOBEKData.ActiveProject.ActiveCase.RRData.GetPaved3BRecord(myNode.ID)
                        If Not Me.Setup.GeneralFunctions.RegExListContains(ExcludeStationsList, pv3b.ms, True) Then
                            pv3b.setMS(sf.CellValue(FieldIdx, shpIdx))
                        End If
                    Case Is = "3B_GREENHOUSE"
                        gr3b = Me.Setup.SOBEKData.ActiveProject.ActiveCase.RRData.GetGreenhouse3BRecord(myNode.ID)
                        If Not Me.Setup.GeneralFunctions.RegExListContains(ExcludeStationsList, gr3b.ms, True) Then
                            gr3b.setMS(sf.CellValue(FieldIdx, shpIdx))
                        End If
                End Select
            End If
        Next

        Me.Setup.GeneralFunctions.UpdateProgressBar("Assigning meteo stations to lateral nodes...", 0, 10, True)
        Dim latFlbr As clsLateralDatFLBRRecord
        For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
            iReach += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, CFTopo.Reaches.Reaches.Values.Count)
            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = enmNodetype.NodeCFLateral Then
                    latFlbr = CFData.getLateralDatFLBRRecord(myObj.ID)
                    If latFlbr.dclt1 = 7 Then
                        myObj.calcXY()
                        shpIdx = sf.PointInShapefile(myObj.X, myObj.Y)
                        If shpIdx >= 0 Then
                            If Not Me.Setup.GeneralFunctions.RegExListContains(ExcludeStationsList, latFlbr.ms, True) Then
                                latFlbr.ms = sf.CellValue(FieldIdx, shpIdx)
                            End If
                        End If
                    End If
                End If
            Next
        Next
        sf.EndPointInShapefile()
        sf.Close()
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

    End Function

    Public Function GetIDListByResultsType(ResultsType As STOCHLIB.GeneralFunctions.enmSobekResultsType, ByRef ResultsList As List(Of String), Optional ByVal IDFilter As String = "*", Optional ByVal UseRegEx As Boolean = True) As Boolean
        'this function produces a list of ID's for a given results type and an (optional) RegEx selectionlist. 
        Try
            Dim IncludeList As New List(Of String), ExcludeList As New List(Of String)
            IncludeList = Me.Setup.GeneralFunctions.ParseStringToList(IDFilter, ";")

            Select Case ResultsType
                Case Is = GeneralFunctions.enmSobekResultsType.CalcpntD, enmSobekResultsType.CalcpntH
                    'results at calculation points
                    If Not CFTopo.calculationpointsRead Then CFTopo.ReadCalculationPointsByReach()  'in case the calculation points have not yet been read
                    For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                        If myReach.InUse Then
                            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                                If myObj.InUse AndAlso myObj.isWaterLevelObject Then
                                    If UseRegEx Then
                                        If Me.Setup.GeneralFunctions.IncludebyRegEx(myObj.ID, True, True, IncludeList, False, ExcludeList) Then
                                            If Not ResultsList.Contains(myObj.ID) Then ResultsList.Add(myObj.ID)
                                        End If
                                    Else
                                        'v1.890: added the option for exact ID match
                                        If IncludeList.Contains(myObj.ID) Then
                                            If Not ResultsList.Contains(myObj.ID) Then ResultsList.Add(myObj.ID)
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next
                Case Is = GeneralFunctions.enmSobekResultsType.ReachsegQ, enmSobekResultsType.ReachsegV
                    'results at reach segments
                    If Not CFTopo.reachsegmentsread Then CFTopo.ReadCalculationPointsByReach()  'in case the calculation points and reach segments have not yet been read
                    For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                        If myReach.InUse Then
                            For Each mySeg As clsSbkReachSegment In myReach.ReachSegments.Values
                                If UseRegEx Then
                                    If Me.Setup.GeneralFunctions.IncludebyRegEx(mySeg.ID, True, True, IncludeList, False, ExcludeList) Then
                                        ResultsList.Add(mySeg.ID)
                                    End If
                                Else
                                    If IncludeList.Contains(mySeg.ID) Then
                                        If Not ResultsList.Contains(mySeg.ID) Then ResultsList.Add(mySeg.ID)
                                    End If
                                End If
                            Next
                        End If
                    Next
                    Throw New Exception("Error: results at reach segments not (yet) supported.")
                Case Is = GeneralFunctions.enmSobekResultsType.StrucCrest, enmSobekResultsType.StrucGateHeight, enmSobekResultsType.StrucH1, enmSobekResultsType.StrucH2, enmSobekResultsType.StrucQ
                    'results at structures
                    For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                        If myReach.InUse Then
                            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                                If myObj.InUse AndAlso myObj.isStructure Then
                                    If UseRegEx Then
                                        If Me.Setup.GeneralFunctions.IncludebyRegEx(myObj.ID, True, True, IncludeList, False, ExcludeList) Then
                                            ResultsList.Add(myObj.ID)
                                        End If
                                    Else
                                        If IncludeList.Contains(myObj.ID) Then
                                            If Not ResultsList.Contains(myObj.ID) Then ResultsList.Add(myObj.ID)
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next
                Case enmSobekResultsType.UPGroundwaterLevel
                    'results at unpaved nodes
                    For Each myObj As clsRRNodeTPRecord In RRTopo.Nodes.Values
                        If myObj.nt.ParentID = "3B_UNPAVED" Then
                            ResultsList.Add(myObj.ID)
                        End If
                    Next
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetObjectsByResultsType in class clsSobekCase.")
            Return False
        End Try

    End Function
    Public Sub ReadNetworkNTW()
        NetworkNTW = New clsNetworkNTW(Me.Setup, Me)
        NetworkNTW.Read()
    End Sub

    Public Function WriteNetworkNTW(path As String) As Boolean
        Try
            NetworkNTW.BuildFromCase(Me)
            NetworkNTW.Write(path)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReachesToShapefile(path As String, Optional ByVal UseSubcatchmentDataSource As Boolean = False) As Boolean

        Try
            Dim mySf As New clsPolyLineShapeFile(Me.Setup)
            mySf.CreateNew(Setup, path, True)
            Dim iReach As Integer
            Dim IdFieldIdx As Integer
            Dim RouteFieldIdx As Integer
            Dim BLUPFieldIdx As Integer, BWUPFieldIdx As Integer, MWUPFieldIdx As Integer, TLUPFIeldIdx As Integer, FWUPFieldIdx As Integer
            Dim BLDNFieldIdx As Integer, BWDNFieldIdx As Integer, MWDNFieldIdx As Integer, TLDNFieldIdx As Integer, FWDnFieldIdx As Integer
            Dim BLUP As Double, BLDN As Double, TLUP As Double, TLDN As Double
            Dim BWUP As Double, BWDN As Double
            Dim MWUP As Double, MWDN As Double
            Dim FWUP As Double, FWDN As Double
            Dim TLShpIdx As Integer
            Dim ShapeIdx As Integer
            Dim RouteNum As Integer
            Dim Chain As New Dictionary(Of String, clsSbkReach)

            If UseSubcatchmentDataSource Then
                Me.Setup.GISData.SubcatchmentDataSource.Open()
                Me.Setup.GISData.SubcatchmentDataSource.SetBeginPointInShapefile()
            End If

            'add the required shapefields
            mySf.StartEditing(True)
            mySf.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 80, 0, IdFieldIdx)
            mySf.AddField("ROUTENUM", MapWinGIS.FieldType.STRING_FIELD, 10, 2, RouteFieldIdx)
            mySf.AddField("BEDLEVELUP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, BLUPFieldIdx)
            mySf.AddField("BEDLEVELDN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, BLDNFieldIdx)
            mySf.AddField("BEDWIDTHUP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, BWUPFieldIdx)
            mySf.AddField("BEDWIDTHDN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, BWDNFieldIdx)
            mySf.AddField("MAXWIDTHUP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, MWUPFieldIdx)
            mySf.AddField("MAXWIDTHDN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, MWDNFieldIdx)
            mySf.AddField("TLUP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, TLUPFIeldIdx)   'target level
            mySf.AddField("TLDN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, TLDNFieldIdx)   'target level
            mySf.AddField("TLWIDTHUP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, FWUPFieldIdx)   'width at target level
            mySf.AddField("TLWIDTHDN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 2, FWDnFieldIdx)   'width at target level
            mySf.SF.sf.SaveAs(path)
            mySf.Close()

            'calculate the route number for each chain of interpolated reaches
            iReach = 0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting reaches to shapefile...", 0, 10, True)
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, CFTopo.Reaches.Reaches.Count)
                If myReach.InUse AndAlso myReach.RouteNumber = 0 Then
                    Chain = myReach.getInterpolatedReachesChain()
                    RouteNum += 1
                    For Each part As clsSbkReach In Chain.Values
                        part.RouteNumber = RouteNum
                    Next
                End If
            Next

            'open the shapefile and start editing
            mySf.Open()
            mySf.StartEditing(True)
            iReach = 0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting reaches to shapefile...", 0, 10, True)
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                iReach += 1

                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, CFTopo.Reaches.Reaches.Count)
                If myReach.InUse Then

                    Dim myShape As New MapWinGIS.Shape
                    myShape = myReach.exportToShape()
                    ShapeIdx = mySf.SF.sf.EditAddShape(myShape)
                    mySf.SF.sf.EditCellValue(IdFieldIdx, ShapeIdx, myReach.Id)
                    mySf.SF.sf.EditCellValue(RouteFieldIdx, ShapeIdx, myReach.RouteNumber)

                    'if the subcatchments shapefile is in use, we retrieve the upstrean and downstream target levels here and retrieve the flow width at target level
                    If UseSubcatchmentDataSource Then
                        TLShpIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.PointInShapefile(myReach.bn.X, myReach.bn.Y)
                        TLUP = Me.Setup.GISData.SubcatchmentDataSource.GetNumericalValue(TLShpIdx, enmInternalVariable.WPOutlet)
                        TLShpIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.PointInShapefile(myReach.en.X, myReach.en.Y)
                        TLDN = Me.Setup.GISData.SubcatchmentDataSource.GetNumericalValue(TLShpIdx, enmInternalVariable.WPOutlet)
                    End If

                    'initialize our variables to NaN
                    BLUP = Double.NaN
                    BWUP = Double.NaN
                    MWUP = Double.NaN
                    FWUP = Double.NaN
                    BLDN = Double.NaN
                    BWDN = Double.NaN
                    MWDN = Double.NaN
                    FWDN = Double.NaN

                    myReach.getProfileAttributes(0, BLUP, BWUP, MWUP, FWUP, TLUP)
                    If Not Double.IsNaN(BLUP) Then mySf.SF.sf.EditCellValue(BLUPFieldIdx, ShapeIdx, BLUP)
                    If Not Double.IsNaN(BWUP) Then mySf.SF.sf.EditCellValue(BWUPFieldIdx, ShapeIdx, BWUP)
                    If Not Double.IsNaN(MWUP) Then mySf.SF.sf.EditCellValue(MWUPFieldIdx, ShapeIdx, MWUP)
                    If Not Double.IsNaN(TLUP) Then mySf.SF.sf.EditCellValue(TLUPFIeldIdx, ShapeIdx, TLUP)
                    If Not Double.IsNaN(FWUP) Then mySf.SF.sf.EditCellValue(FWUPFieldIdx, ShapeIdx, FWUP)

                    myReach.getProfileAttributes(myReach.getReachLength, BLDN, BWDN, MWDN, FWDN, TLDN)
                    If Not Double.IsNaN(BLDN) Then mySf.SF.sf.EditCellValue(BLDNFieldIdx, ShapeIdx, BLDN)
                    If Not Double.IsNaN(BWDN) Then mySf.SF.sf.EditCellValue(BWDNFieldIdx, ShapeIdx, BWDN)
                    If Not Double.IsNaN(MWDN) Then mySf.SF.sf.EditCellValue(MWDNFieldIdx, ShapeIdx, MWDN)
                    If Not Double.IsNaN(TLDN) Then mySf.SF.sf.EditCellValue(TLDNFieldIdx, ShapeIdx, TLDN)
                    If Not Double.IsNaN(FWDN) Then mySf.SF.sf.EditCellValue(FWDnFieldIdx, ShapeIdx, FWDN)

                End If
            Next

            mySf.StopEditing(True, True)
            mySf.SF.sf.Close()

            If UseSubcatchmentDataSource Then
                Me.Setup.GISData.SubcatchmentDataSource.setEndPointInShapefile()
                Me.Setup.GISData.SubcatchmentDataSource.Close()
            End If

            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReachesToShapefile of class clsSobekCase.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Sub ImportByNetworkNTW()
        Me.Setup.Log.AddDebugMessage("In ImportByNetworkNTW Of Class clsCFTopology")
        Dim Records As New Collection
        Dim myReach As New clsSbkReach(Me.Setup, Me)

        'this routine reads the Network.NTW and Network.NVL files to import the entire schematization!
        'siebe bosch, august 14, 2016

        Try
            'leest nodes in uit network.ntw
            NetworkNTW = New clsNetworkNTW(Me.Setup, Me)
            Call NetworkNTW.ReadReaches()

            'leest vectorpunten in uit network.nvl
            NetworkNVL = New clsNetworkNVL(Me.Setup, Me)
            Call NetworkNVL.ReadAll()

        Catch ex As Exception
            Me.Setup.Log.AddError("Error In Sub Import Of Class clsCFTopology: " & ex.Message)
        End Try

    End Sub


    Public Function GetFrictionValue(ByRef myReach As clsSbkReach, ByVal StartDist As Double, ByVal EndDist As Double, ByRef FrictionType As GeneralFunctions.enmFrictionType, ByRef FrictionVal As Double) As Boolean
        Try
            'if there is a cross section within the section, use that for the friction
            Dim upDist As Double, dnDist As Double
            Dim NearestUpstream As clsSbkReachObject, NearestDownstream As clsSbkReachObject
            Dim CRFRRecord As clsFrictionDatCRFRRecord = Nothing
            Dim MidPoint As Double = (StartDist + EndDist) / 2

            Dim BDFRRecord As clsFrictionDatBDFRRecord = Nothing
            If CFData.Data.FrictionData.FrictionDatBDFRRecords.records.ContainsKey(myReach.Id.Trim.ToUpper) Then BDFRRecord = CFData.Data.FrictionData.FrictionDatBDFRRecords.records.Item(myReach.Id.Trim.ToUpper)
            If Not BDFRRecord Is Nothing Then
                FrictionType = BDFRRecord.mf
                FrictionVal = BDFRRecord.mtcpConstant
            End If

            NearestUpstream = myReach.GetUpstreamObjectOfType(MidPoint, upDist, GeneralFunctions.enmNodetype.SBK_PROFILE, True, False)
            If Not NearestUpstream Is Nothing Then
                If CFData.Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(NearestUpstream.ID.Trim.ToUpper) Then
                    CRFRRecord = CFData.Data.FrictionData.FrictionDatCRFRRecords.records.Item(NearestUpstream.ID.Trim.ToUpper)
                End If
            End If

            NearestDownstream = myReach.GetDownstreamObjectOfType(MidPoint, dnDist, GeneralFunctions.enmNodetype.SBK_PROFILE, True, False)
            If Not NearestDownstream Is Nothing AndAlso (NearestUpstream Is Nothing OrElse dnDist < upDist) Then
                If CFData.Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(NearestDownstream.ID.Trim.ToUpper) Then
                    CRFRRecord = CFData.Data.FrictionData.FrictionDatCRFRRecords.records.Item(NearestDownstream.ID.Trim.ToUpper)
                End If
            End If

            If Not CRFRRecord Is Nothing Then
                'find the middle section for this profile
                Dim Idx As Integer = CRFRRecord.ftysTable.XValues.Count / 2
                FrictionType = CRFRRecord.ftysTable.XValues.Values(Idx)
                FrictionVal = CRFRRecord.ftysTable.Values1.Values(Idx)
            ElseIf Not BDFRRecord Is Nothing Then
                FrictionType = BDFRRecord.mf
                FrictionVal = BDFRRecord.mtcpConstant
            Else
                'return global value
                FrictionType = CFData.Data.FrictionData.FrictionDatBDFRRecords.GLFRRecord.mf
                FrictionVal = CFData.Data.FrictionData.FrictionDatBDFRRecords.GLFRRecord.mtcpConstant
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function CalculatePolygonIntersections(ByRef PolygonShapefilePath As String, IDField As String, Optional ByVal ShapeIdx As Integer = -1) As Dictionary(Of String, clsSbkPolygonIntersection)
        Try
            Dim myReachShape As MapWinGIS.Shape
            Dim Results As Object = Nothing
            Dim myObj As clsSbkPolygonIntersection
            Dim SnapDist As Double, SnapChainage As Double
            Dim iReach As Integer, nReach As Integer = CFTopo.Reaches.Reaches.Count
            Dim IDFieldIdx As Integer
            Dim iShape As Integer, iResult As Integer
            Dim myDict As New Dictionary(Of String, clsSbkPolygonIntersection)

            'read the polygon shapefile and get the id field index number
            Dim PolygonShapefile As New clsShapeFile(Me.Setup)
            PolygonShapefile.Path = PolygonShapefilePath
            PolygonShapefile.Open()
            IDFieldIdx = PolygonShapefile.GetFieldIdx(IDField)
            PolygonShapefile.sf.BeginPointInShapefile()         'necessary to investigate in which shape a point lies

            'this function returns a collection of locations where the network enters or leaves polygons from a given shapefile
            'start by processing every reach and converting it into a shape
            Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating Reach-to-polygon intersections...", 0, nReach)

            'then walk through all polygons and look for intersections with our reach
            For iShape = 0 To PolygonShapefile.sf.NumShapes - 1

                If ShapeIdx < 0 OrElse ShapeIdx = iShape Then

                    Me.Setup.GeneralFunctions.UpdateProgressBar("Processing polygon " & iShape & " of " & PolygonShapefile.sf.NumShapes, iShape, PolygonShapefile.sf.NumShapes)
                    Dim myShape As MapWinGIS.Shape = PolygonShapefile.sf.Shape(iShape)

                    'explode each polygon to make sure we handle every part separately
                    Dim myPolygons As Object = Nothing
                    Call myShape.Explode(myPolygons)

                    'now convert each polygon into a polyline
                    For i = 0 To myPolygons.Length - 1
                        Dim myPolygon As MapWinGIS.Shape = DirectCast(myPolygons(i), MapWinGIS.Shape)
                        Dim myPolyLine As MapWinGIS.Shape = Me.Setup.GeneralFunctions.PolygonToPolyline(myPolygon)
                        iReach = 0
                        For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                            iReach += 1
                            Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, CFTopo.Reaches.Reaches.Count)
                            myReachShape = myReach.exportToShape
                            If myPolyLine.GetIntersection(myReachShape, Results) Then
                                'Stop
                                For iResult = 0 To Results.length - 1
                                    Dim myIntersection As MapWinGIS.Shape
                                    myIntersection = DirectCast(Results(iResult), MapWinGIS.Shape)

                                    'as it appears, every point in the intersection shape, is a separate intersection point, so process each separately
                                    For iPoint = 0 To myIntersection.numPoints - 1
                                        If myReach.calcSnapLocation(myIntersection.Point(iPoint).x, myIntersection.Point(iPoint).y, 0.000001, SnapChainage, SnapDist, True) Then
                                            myObj = New clsSbkPolygonIntersection(Me.Setup, Me) With {
                                            .ci = myReach.Id,
                                            .lc = SnapChainage
                                            }
                                            myObj.calcXY()

                                            'dit is een tricky gedeelte. We zitten op de overgang tussen twee polygonen, maar door afrondfouten e.d. komt dit niet altijd naar voren
                                            'daarom beginnen met een heel klein verschil, prikkend in de polygonenshapefile. Verschil steeds groter maken totdat we twee verschillende van- en na-shapes hebben
                                            Dim Done As Boolean = False
                                            Dim Diff As Double = 0.000001
                                            While Not Done
                                                myObj.FromPolyIdx = myReach.GetPolygonShapeIdx(PolygonShapefile, myObj.lc - Diff)
                                                myObj.ToPolyIdx = myReach.GetPolygonShapeIdx(PolygonShapefile, myObj.lc + Diff)
                                                If myObj.FromPolyIdx <> myObj.ToPolyIdx Then Done = True
                                                Diff *= 10
                                                If Diff > 1 Then Done = True
                                            End While

                                            'add this location if the polygon index
                                            If myObj.FromPolyIdx <> myObj.ToPolyIdx Then
                                                Dim myKey As String = myObj.ci & "_" & myObj.lc
                                                If Not myDict.ContainsKey(myKey) Then myDict.Add(myKey, myObj)
                                                Me.Setup.Log.AddMessage("Successfully added intersection at: " & myObj.X & "," & myObj.Y & ".")
                                            Else
                                                Me.Setup.Log.AddError("Identical polygon index number on both sides of intersection: " & myObj.X & "," & myObj.Y & ". Intersection not used.")
                                            End If

                                        End If
                                    Next
                                Next
                                'Stop
                            End If
                        Next
                    Next

                End If

            Next

            PolygonShapefile.sf.EndPointInShapefile()
            PolygonShapefile.Close()

            Return myDict
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("")
            Return Nothing
        End Try
    End Function

    Public Function ExportToCSV(FolderPath As String) As Boolean
        If Not CFTopo.ExportCSV(FolderPath) Then Throw New Exception("Error exporting SOBEK Reaches to CSV format.")
        If Not CFData.ExportCSV(FolderPath) Then Throw New Exception("Error exporting SOBEK Flow Data to CSV format.")
    End Function

    Public Sub Initialize()

        Try
            If Me.SobekProject.ProjectDir Is Nothing Then Throw New Exception("Variable ProjectDir not defined in clsSobekCase.Initialize")
            If Not Me.Setup.GeneralFunctions.AutoCorrectPath(Me.SobekProject.ProjectDir) Then
                Dim log As String = "Error: pad naar SOBEK-directory " & Me.SobekProject.ProjectDir & " bestaat niet."
                Me.Setup.Log.AddError(log)
                Throw New Exception(log)
            End If
            Me.setCaseDir()

            'Init classes:
            RRData = New clsRRData(Me.Setup, Me)
            RRTopo = New clsRRTopology(Me.Setup, Me)
            CFData = New clsCFData(Me.Setup, Me)
            CFTopo = New clsCFTopology(Me.Setup, Me)
            'ModelCatchments = New clsModelCatchments(Me.Setup)
            Areas = New clsAreas(Me.Setup)
            BuiFile = New clsBuiFile(Me.Setup)

        Catch ex As Exception

        End Try

    End Sub

    Public Function GetCaseDir() As String
        Return CaseDir
    End Function

    Public Function GetReachCount() As Integer
        Return CFTopo.Reaches.Reaches.Count
    End Function


    Public Function PopulateComboboxWithTimestepsFromHisfile(FileName As String, ByRef cmb As ComboBox) As Boolean
        Try
            'populate the combobox with all timesteps
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\" & FileName, Me.Setup)
            myReader.OpenFile()
            myReader.ReadHisHeader()
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = myReader.GetHisFileHeader
            For i = 0 To myHeader.TimeSteps.Count - 1
                cmb.Items.Add(myHeader.TimeSteps(i))
            Next
            myReader.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function PopulateCheckedListBoxWithHisfileLocations(Filename As String, ByRef cmb As CheckedListBox) As Boolean
        Try
            'populate the listbox with all timesteps
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\" & Filename, Me.Setup)
            cmb.DataSource = myReader.ReadAllLocations(True)
            myReader.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function PopulateCheckedListBoxWithHisfileTimesteps(FileName As String, ByRef cmb As CheckedListBox, formatting As String) As Boolean
        Try
            'populate the listbox with all timesteps
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading his file header...", 0, 10)
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\" & FileName, Me.Setup)
            Me.Setup.GeneralFunctions.UpdateProgressBar("", 2, 10)
            myReader.OpenFile(True, True)
            'Me.Setup.GeneralFunctions.UpdateProgressBar("", 3, 10)
            'myReader.ReadHisHeader()
            Me.Setup.GeneralFunctions.UpdateProgressBar("", 4, 10)
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = myReader.GetHisFileHeader
            Me.Setup.GeneralFunctions.UpdateProgressBar("", 5, 10)
            For i = 0 To myHeader.TimeSteps.Count - 1
                cmb.Items.Add(Format(myHeader.TimeSteps(i), formatting))
            Next
            myReader.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Complete.", 0, 10)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function WaterLevelsToShapefile(filename As String, ByRef tsBox As CheckedListBox, Optional ByVal CalcDifferenceWithReferenceCase As Boolean = False, Optional ByVal RefHisfile As String = "") As Boolean
        Try
            Dim dtCollection As New Dictionary(Of String, DataTable)
            Dim refCollection As New Dictionary(Of String, DataTable)
            Dim FieldIdx As Integer, IDFieldIdx As Integer
            Dim newField As MapWinGIS.Field, i As Long, j As Long, n As Long
            Dim dt As DataTable, dtref As DataTable = Nothing
            Dim NodesDone As New Dictionary(Of String, clsSbkReachObject) 'keeps track of which nodes have already been processed
            Dim PointSF As New clsPointShapeFile(Me.Setup)
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader
            Dim refHeader As clsHisFileBinaryReader.stHisFileHeader
            Dim myReader As clsHisFileBinaryReader, refReader As clsHisFileBinaryReader

            'remove exising shapefile and create new
            If System.IO.File.Exists(filename) Then Setup.GeneralFunctions.deleteShapeFile(filename)
            PointSF.SF.sf.CreateNew(filename, MapWinGIS.ShpfileType.SHP_POINT)
            PointSF.StartEditing(True)

            'read all hisfile results for the parameter waterlevel to a collection of datatables (one per location)
            myReader = New clsHisFileBinaryReader(CaseDir & "\calcpnt.his", Me.Setup)
            myReader.OpenFile()
            dtCollection = myReader.ReadAllDataOneParameterToDataTableCollection("Waterlevel")
            myHeader = myReader.GetHisFileHeader

            If CalcDifferenceWithReferenceCase Then
                refReader = New clsHisFileBinaryReader(RefHisfile, Me.Setup)
                refReader.OpenFile()
                refCollection = refReader.ReadAllDataOneParameterToDataTableCollection("Waterlevel")
                refHeader = myReader.GetHisFileHeader

                If refHeader.TimeSteps.Count <> myHeader.TimeSteps.Count Then
                    Throw New Exception("Number of timesteps in reference case must be equal to that in comparison case.")
                End If
            End If

            PointSF.AddField("SOBEK_ID", MapWinGIS.FieldType.STRING_FIELD, 20, 10, IDFieldIdx)
            For i = 0 To tsBox.CheckedIndices.Count - 1
                Debug.Print(tsBox.Items(i).ToString)
                newField = New MapWinGIS.Field
                PointSF.AddField(Format(myHeader.TimeSteps(i), "yyMMddhhmm"), MapWinGIS.FieldType.DOUBLE_FIELD, 20, 10, FieldIdx)
            Next

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading water level statistics...", 0, 10)
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values

                'first fake the start and endpoint of this reach as if they are reach objects
                Dim bn As New clsSbkReachObject(Me.Setup, Me) With {
                .nt = enmNodetype.NodeCFGridpointFixed,
                .ID = myReach.bn.ID,
                .ci = myReach.Id,
                .lc = 0,
                .InUse = True
                    }

                Dim en As New clsSbkReachObject(Me.Setup, Me) With {
                .nt = enmNodetype.NodeCFGridpointFixed,
                .ID = myReach.en.ID,
                .ci = myReach.Id,
                .lc = myReach.getReachLength,
                .InUse = True
                    }

                If Not myReach.ReachObjects.ReachObjects.ContainsKey(bn.ID.Trim.ToUpper) Then myReach.ReachObjects.Add(bn)
                If Not myReach.ReachObjects.ReachObjects.ContainsKey(en.ID.Trim.ToUpper) Then myReach.ReachObjects.Add(en)

                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.isWaterLevelObject AndAlso dtCollection.ContainsKey(myObj.ID.Trim.ToUpper) AndAlso Not NodesDone.ContainsKey(myObj.ID.Trim.ToUpper) Then

                        dt = dtCollection.Item(myObj.ID.Trim.ToUpper)
                        NodesDone.Add(myObj.ID.Trim.ToUpper, myObj)

                        If CalcDifferenceWithReferenceCase Then
                            If refCollection.ContainsKey(myObj.ID.Trim.ToUpper) Then
                                dtref = refCollection.Item(myObj.ID.Trim.ToUpper)
                            Else
                                dtref = Nothing
                            End If
                        End If

                        If (CalcDifferenceWithReferenceCase AndAlso Not dtref Is Nothing) OrElse Not CalcDifferenceWithReferenceCase Then

                            'create a new shape in the point shapefile and write the node ID
                            Dim newShape As New MapWinGIS.Shape
                            If Not newShape.Create(MapWinGIS.ShpfileType.SHP_POINT) Then MsgBox("Could not create shape.")
                            Call newShape.AddPoint(myObj.X, myObj.Y)
                            Dim ShapeIdx As Long = PointSF.SF.sf.EditAddShape(newShape)
                            If ShapeIdx < 0 Then Throw New Exception("Error writing points to shapefile.")
                            If Not PointSF.SF.sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID) Then Me.Setup.Log.AddWarning("Could not write ID to shapefile.")

                            If CalcDifferenceWithReferenceCase Then
                                'write the selected timesteps to their corresponding fields
                                For j = 0 To tsBox.CheckedIndices.Count - 1
                                    If Not PointSF.SF.sf.EditCellValue(j + 1, ShapeIdx, dt.Rows(tsBox.CheckedIndices.Item(j))(1) - dtref.Rows(tsBox.CheckedIndices.Item(j))(1)) Then Me.Setup.Log.AddWarning("Could not write value to shapefile.")
                                Next
                            Else
                                'write the selected timesteps to their corresponding fields
                                For j = 0 To tsBox.CheckedIndices.Count - 1
                                    If Not PointSF.SF.sf.EditCellValue(j + 1, ShapeIdx, dt.Rows(tsBox.CheckedIndices.Item(j))(1)) Then Me.Setup.Log.AddWarning("Could not write value to shapefile.")
                                Next
                            End If

                        End If

                    End If
                Next
            Next

            PointSF.SF.sf.StopEditingShapes(True, True)
            PointSF.SF.sf.SaveAs(filename)

            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
    End Function
    Public Function CalculateWaterlevelTrackingError(IDFilterList As List(Of String), SkipPercentage As Double, ByRef LocationsInUse As List(Of clsSbkVectorPoint), WinSumMonth As Integer, WinSumDay As Integer, SumWinMonth As Integer, SumWinDay As Integer) As Dictionary(Of Date, Double)
        Try
            'this function computes the tracking error for a given simulation
            'the error is computed as (waterlevel - targetlevel)^2
            'for each timestep the error is cumulated for all selected output locatons
            Me.Setup.GISData.SubcatchmentDataSource.Open()

            'read all hisfile results for the parameter waterlevel to a collection of datatables (one per location)
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\calcpnt.his", Me.Setup)
            myReader.OpenFile()

            'let's see if we can store the results in a dictionary of timeseries (less memory than a collection of datatables?)
            Dim tsCollection As New Dictionary(Of String, List(Of Single))
            tsCollection = myReader.ReadAllDataOneParameterToTimeSeriesDictionary("Waterlevel")


            'now we need to assign our target levels to each of the reach objects. Start by collecting all waterlevel points from the model
            Dim HPoints As New Dictionary(Of String, clsSbkVectorPoint)
            HPoints = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.CollectAllWaterLevelPoints()

            Dim nPoints As Integer = 0
            For Each HPoint As clsSbkVectorPoint In HPoints.Values

                Dim Match As Boolean = False
                For i = 0 To IDFilterList.Count - 1
                    If Setup.GeneralFunctions.RegExMatch(IDFilterList(i), HPoint.ID, True) Then Match = True
                Next

                If Match Then
                    HPoint.ZP = Me.Setup.GISData.SubcatchmentDataSource.GetNumericalValueByCoord(HPoint.X, HPoint.Y, enmInternalVariable.ZPUpstreamOutlet)
                    HPoint.WP = Me.Setup.GISData.SubcatchmentDataSource.GetNumericalValueByCoord(HPoint.X, HPoint.Y, enmInternalVariable.WPUpstreamOutlet)
                    HPoint.InUse = True
                    LocationsInUse.Add(HPoint)
                    nPoints += 1
                End If

            Next

            'now that we have our target levels, walk through all timesteps and compute the Tracking Error
            'first figure out how many timesteps we actually have
            Dim ts As List(Of Single)
            ts = tsCollection.Values(0)

            'decide the start and end timestep index for our analysis
            Dim StartRowIdx As Long = SkipPercentage / 100 * (ts.Count - 1)
            Dim EndRowIdx As Long = ts.Count - 1
            Dim ErrorSeries As New Dictionary(Of Date, Double)

            'walk through all timesteps. for each timestep compute the root mean square error of (waterlevel - target level) for all selected locations
            For i = StartRowIdx To EndRowIdx
                Dim mySeason As enmSeason = Me.Setup.GeneralFunctions.HydrologischHalfJaar(myReader.GetHisFileHeader.TimeSteps(i), WinSumMonth, WinSumDay, SumWinMonth, SumWinDay)
                Dim tsErr As Double = 0
                For Each HPoint As clsSbkVectorPoint In LocationsInUse
                    If HPoint.InUse AndAlso Not Double.IsNaN(HPoint.WP) Then
                        Select Case mySeason
                            Case enmSeason.hydrosummerhalfyear
                                tsErr += (tsCollection.Item(HPoint.ID.Trim.ToUpper)(i) - HPoint.ZP) ^ 2
                            Case enmSeason.hydrowinterhalfyear
                                tsErr += (tsCollection.Item(HPoint.ID.Trim.ToUpper)(i) - HPoint.WP) ^ 2
                        End Select
                    End If
                Next
                tsErr = Math.Sqrt(tsErr / LocationsInUse.Count)
                ErrorSeries.Add(myReader.GetHisFileHeader.TimeSteps(i), tsErr)
            Next

            Me.Setup.GISData.SubcatchmentDataSource.Close()
            Me.Setup.Log.AddMessage("Tracking Error has successfully been computed, based on " & nPoints & " waterlevel locations.")

            Return ErrorSeries
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalculateWaterlevelTrackingError of class clsSobekCase.")
            Return Nothing
        End Try

    End Function

    Public Function ExportReachesToShapefile(ExportDir As String) As Boolean
        'Date: 17-10-2013
        'Author: Siebe Bosch
        'Description: converts all SOBEK Reaches in the active case to geodatasource
        Dim myReach As clsSbkReach
        Dim myShape As MapWinGIS.Shape
        Dim myPoint As clsSbkVectorPoint, newPoint As MapWinGIS.Point
        Dim PointIdx As Long, ReachIDIdx As Long, ShapeIdx As Long
        Dim iReach As Long, nReach As Long
        Dim Path As String
        Dim sf As MapWinGIS.Shapefile

        Try

            Path = ExportDir & "\sbk_reach.shp"
            If System.IO.File.Exists(Path) Then Me.Setup.GeneralFunctions.deleteShapeFile(Path) 'delete existing
            sf = New MapWinGIS.Shapefile
            If Not sf.CreateNew(Path, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Throw New Exception("Could not create shapefile from SOBEK Reaches.")
            If Not sf.StartEditingShapes(True) Then Throw New Exception("Could not start editing newly created shapefile for reaches.")
            ReachIDIdx = sf.EditAddField("ReachID", MapWinGIS.FieldType.STRING_FIELD, 20, 20)

            iReach = 1
            nReach = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Converting SOBEK reaches to shapefile.", iReach, nReach)

            'walk through all sobek reaches
            With Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches
                For Each myReach In .Reaches.Values
                    iReach += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)

                    'create a new shape for this reach
                    myShape = New MapWinGIS.Shape
                    If Not myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE) Then Throw New Exception("Could not create shape for sobek reach " & myReach.Id)

                    'add all vector points as points to the shape
                    PointIdx = -1
                    For Each myPoint In myReach.NetworkcpRecord.CPTable.CP
                        PointIdx += 1
                        newPoint = New MapWinGIS.Point
                        newPoint.x = myPoint.X
                        newPoint.y = myPoint.Y
                        myShape.InsertPoint(newPoint, PointIdx)
                    Next

                    'add the shape to the shapefile
                    ShapeIdx = sf.EditAddShape(myShape)
                    If Not sf.EditCellValue(ReachIDIdx, ShapeIdx, myReach.Id) Then Throw New Exception("Could not set cell value in newly created shapefile containing sobek reaches.")
                Next
            End With

            'save the shapefile
            If Not sf.StopEditingShapes(True, True) Then Throw New Exception("Could not stop editing newly created shapefile with sobek reaches.")
            sf.Save()
            If Not sf.Close() Then Throw New Exception("Could not close newly created shapefile containing SOBEK Reaches.")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Public Function WaterlevelStatisticsToShapefile(Filename As String, SkipPercentage As Double, IncludeTargetLevelsFromAreaShapefile As Boolean, searchRadius As Double, ResultsTimestepIdx As Integer, ResultsDiffType As String, ResultsCompareTimestep As Boolean, ResultsCompareTimestepIdx As Integer, ResultsCompareWP As Boolean, ResultsCompareZP As Boolean, ResultsCompareMax As Boolean, ResultsCompareMin As Boolean, WritePercentiles As Boolean, PercentileSize As Integer, SkipPointsOutsideShapefile As Boolean, ByRef JSONContent As String, Optional IDFilterList As List(Of String) = Nothing) As Boolean
        Try
            Dim ShapeIdx As Integer
            Dim myStats As Dictionary(Of Integer, Double) 'percentile vs value
            Dim i As Long = 0
            Dim j As Long, p As Long, k As Integer
            Dim NewIdx As Integer
            Dim n As Long = CFTopo.Reaches.Reaches.Count
            Dim FirstVal As Double, LastVal As Double, MinVal As Double, MaxVal As Double, TSVal As Double 'the values that can be written
            Dim refVal As Double, resultsVal As Double                                                     'for the difference computation
            Dim useLevels As clsTargetLevels = Nothing

            'initialize our JSON data block
            JSONContent = "let locations = {" & vbCrLf
            JSONContent &= "    " & Chr(34) & "locations" & Chr(34) & ": [" & vbCrLf

            'Dim dt As DataTable
            Dim ts As List(Of Single)
            Dim upLevels As New clsTargetLevels, dnLevels As New clsTargetLevels, lcLevels As New clsTargetLevels
            Dim NodesDone As New Dictionary(Of String, clsSbkReachObject) 'keeps track of which nodes have already been processed

            Dim IDFieldIdx As Integer, LastValIdx As Integer, FirstValIdx As Integer, ZPFieldIdx As Integer, WPFieldIdx As Integer
            Dim MaxValIdx As Integer, MinValIdx As Integer
            Dim TSValIdx As Integer, RefValIdx As Integer, DifValIdx As Integer, AbsDifValIdx As Integer
            Dim PercentileFields As New List(Of Integer)
            'this function writes the statistics for the water levels in the current case to a shapefile.

            Setup.GISData.SubcatchmentDataSource.Open()
            Setup.GISData.SubcatchmentDataSource.SetBeginPointInShapefile() 'make sure MapWinGIS can perform the point-in-polygon routine

            'read all hisfile results for the parameter waterlevel to a collection of datatables (one per location)
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\calcpnt.his", Me.Setup)
            myReader.OpenFile()

            'only when storing the results for all locations in a datatable collection
            'Dim dtCollection As New Dictionary(Of String, DataTable)
            'dtCollection = myReader.ReadAllDataOneParameterToDataTableCollection("Waterlevel")

            'let's see if we can store the results in a dictionary of timeseries (less memory than a collection of datatables?)
            Dim tsCollection As New Dictionary(Of String, List(Of Single))
            tsCollection = myReader.ReadAllDataOneParameterToTimeSeriesDictionary("Waterlevel")

            'create the shapefields
            Setup.GISData.PointShapeFile = New clsPointShapeFile(Me.Setup)
            Setup.GISData.PointShapeFile.SF.sf.CreateNew(Filename, MapWinGIS.ShpfileType.SHP_POINT)
            If Not Setup.GISData.PointShapeFile.SF.sf.StartEditingShapes(True) Then MsgBox("Could not start editing shapes.")

            'add the required shapefields
            Setup.GISData.PointShapeFile.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 50, 0, IDFieldIdx)
            Setup.GISData.PointShapeFile.AddField("FIRST", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, FirstValIdx)
            Setup.GISData.PointShapeFile.AddField("LAST", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, LastValIdx)
            Setup.GISData.PointShapeFile.AddField("MAX", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MaxValIdx)
            Setup.GISData.PointShapeFile.AddField("MIN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MinValIdx)
            Setup.GISData.PointShapeFile.AddField("TSVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, TSValIdx)
            Setup.GISData.PointShapeFile.AddField("REFVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, RefValIdx)
            Setup.GISData.PointShapeFile.AddField("DIF", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, DifValIdx)
            Setup.GISData.PointShapeFile.AddField("ABSDIF", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, AbsDifValIdx)
            If WritePercentiles Then
                For j = 0 To 100 Step PercentileSize
                    Setup.GISData.PointShapeFile.AddField("pct" & j, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, NewIdx)
                    PercentileFields.Add(NewIdx)
                Next
            End If

            If IncludeTargetLevelsFromAreaShapefile Then
                Setup.GISData.PointShapeFile.AddField("ZP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, ZPFieldIdx)
                Setup.GISData.PointShapeFile.AddField("WP", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, WPFieldIdx)
            End If

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading water level statistics...", 0, 10, True)
            Dim iReach As Integer, nReaches As Integer = CFTopo.Reaches.Reaches.Count

            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values

                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReaches)

                'first fake the start and endpoint of this reach as if they are reach objects
                Dim bn As New clsSbkReachObject(Me.Setup, Me) With {
                .nt = enmNodetype.NodeCFGridpointFixed,
                .ID = myReach.bn.ID,
                .ci = myReach.Id,
                .lc = 0,
                .InUse = True
                    }

                Dim en As New clsSbkReachObject(Me.Setup, Me) With {
                .nt = enmNodetype.NodeCFGridpointFixed,
                .ID = myReach.en.ID,
                .ci = myReach.Id,
                .lc = myReach.getReachLength,
                .InUse = True
                    }

                If Not myReach.ReachObjects.ReachObjects.ContainsKey(bn.ID.Trim.ToUpper) Then myReach.ReachObjects.Add(bn)
                If Not myReach.ReachObjects.ReachObjects.ContainsKey(en.ID.Trim.ToUpper) Then myReach.ReachObjects.Add(en)

                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.isWaterLevelObject AndAlso Not NodesDone.ContainsKey(myObj.ID.Trim.ToUpper) Then 'AndAlso dtCollection.ContainsKey(myObj.ID.Trim.ToUpper) 


                        'new in v1.99: filtering by ID
                        Dim Match As Boolean = False
                        If Me.Setup.GeneralFunctions.TextMatchUsingWildcards(IDFilterList, myObj.ID, True) Then Match = True

                        If Match Then
                            If tsCollection.ContainsKey(myObj.ID.Trim.ToUpper) Then
                                ts = tsCollection.Item(myObj.ID.Trim.ToUpper)

                                Dim StartRowIdx As Long = SkipPercentage / 100 * (ts.Count - 1)
                                Dim EndRowIdx As Long = ts.Count - 1

                                myObj.calcXY(True)
                                myStats = New Dictionary(Of Integer, Double)
                                Setup.GeneralFunctions.SinglesListStats(ts, StartRowIdx, EndRowIdx, FirstVal, LastVal, MinVal, MaxVal, WritePercentiles, PercentileSize, myStats)

                                'create a new shape in the point shapefile
                                Dim newShape As New MapWinGIS.Shape
                                If Not newShape.Create(MapWinGIS.ShpfileType.SHP_POINT) Then MsgBox("Could not create shape.")
                                Call newShape.AddPoint(myObj.X, myObj.Y)
                                ShapeIdx = Setup.GISData.PointShapeFile.SF.sf.EditAddShape(newShape)
                                If ShapeIdx < 0 Then Throw New Exception("Error writing points to shapefile.")

                                'write the statistical values to their corresponding fields
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID) Then Me.Setup.Log.AddWarning("Could not write ID to shapefile.")
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(FirstValIdx, ShapeIdx, FirstVal) Then Me.Setup.Log.AddWarning("Could not write first value from hisfile to shapefile.")
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(LastValIdx, ShapeIdx, LastVal) Then Me.Setup.Log.AddWarning("Could not write last value from hisfile to shapefile.")
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(MinValIdx, ShapeIdx, MinVal) Then Me.Setup.Log.AddWarning("Could Not write minimum value From hisfile to shapefile.")
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(MaxValIdx, ShapeIdx, MaxVal) Then Me.Setup.Log.AddWarning("Could Not write maximum value From hisfile to shapefile.")
                                If WritePercentiles Then
                                    p = -1
                                    For k = 0 To 100 Step PercentileSize
                                        p += 1
                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(PercentileFields(p), ShapeIdx, myStats.Item(k)) Then Me.Setup.Log.AddWarning("Could not write maximum value from hisfile to shapefile.")
                                    Next
                                End If

                                'write the specific output for the chosen timestep
                                If ResultsTimestepIdx >= 0 Then
                                    TSVal = ts(ResultsTimestepIdx)
                                    If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(TSValIdx, ShapeIdx, TSVal) Then Me.Setup.Log.AddWarning("Could not write value for chosen timstep to shapefile.")
                                End If

                                'for the difference calculation we need to determine which results value to use
                                Select Case ResultsDiffType.Trim.ToUpper
                                    Case Is = "MINIMUM"
                                        resultsVal = MinVal
                                    Case Is = "MAXIMUM"
                                        resultsVal = MaxVal
                                    Case Is = "FIRST"
                                        resultsVal = FirstVal
                                    Case Is = "LAST"
                                        resultsVal = LastVal
                                    Case Is = "FROM TIMESTEP"
                                        resultsVal = TSVal
                                End Select

                                'also for the difference calculation we need to determine which reference value to use
                                'start with the the target levels since those need some special treatment (we will try to find the best matching target level value)
                                useLevels = Nothing
                                If IncludeTargetLevelsFromAreaShapefile Then
                                    Dim Diff As Double = 9.0E+99
                                    Dim myLevels As New Dictionary(Of String, clsTargetLevels)
                                    myReach.getTargetLevelsCollectionFromShapefile(myLevels, myObj.lc, -searchRadius, searchRadius, True, SkipPointsOutsideShapefile)

                                    'now select the most suitable target levels from the collection of target levels surrounding our object
                                    For Each myLevel As clsTargetLevels In myLevels.Values
                                        'find the nearest most suitable target level
                                        If ResultsCompareWP Then
                                            'the chosen timestep result is compared to winter target level
                                            If Math.Abs(myLevel.getWPOutlet - resultsVal) < Diff Then
                                                Diff = Math.Abs(myLevel.getWPOutlet - resultsVal)
                                                useLevels = myLevel
                                            End If
                                        ElseIf ResultsCompareZP Then
                                            If Math.Abs(myLevel.getZPOutlet - resultsVal) < Diff Then
                                                Diff = Math.Abs(myLevel.getZPOutlet - resultsVal)
                                                useLevels = myLevel
                                            End If
                                        End If
                                    Next
                                End If

                                'in order to compute the difference we need to calculate the reference value
                                If ResultsCompareTimestep AndAlso ResultsCompareTimestepIdx >= 0 Then
                                    refVal = ts(ResultsCompareTimestepIdx)
                                ElseIf ResultsCompareMax Then
                                    refVal = MaxVal
                                ElseIf ResultsCompareMin Then
                                    refVal = MinVal
                                ElseIf ResultsCompareWP AndAlso Not useLevels Is Nothing Then
                                    If Not useLevels Is Nothing Then refVal = useLevels.getWPOutlet Else refVal = TSVal
                                ElseIf ResultsCompareZP Then
                                    If Not useLevels Is Nothing Then refVal = useLevels.getZPOutlet Else refVal = TSVal
                                End If

                                'write our reference value, difference value and absolute difference to the shapefile
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(RefValIdx, ShapeIdx, refVal) Then Me.Setup.Log.AddWarning("Could not write reference value to shapefile.")
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(DifValIdx, ShapeIdx, (resultsVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write values difference to shapefile.")
                                If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(AbsDifValIdx, ShapeIdx, Math.Abs(resultsVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write absolute value of difference to shapefile.")

                                'now write our target levels (if found) to the shapefile and geoJSON
                                If Not useLevels Is Nothing Then
                                    If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(ZPFieldIdx, ShapeIdx, useLevels.getZPOutlet) Then Me.Setup.Log.AddWarning("Could not write summer targetlevel to shapefile.")
                                    If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(WPFieldIdx, ShapeIdx, useLevels.getWPOutlet) Then Me.Setup.Log.AddWarning("Could not write winter targetlevel to shapefile.")
                                    JSONContent &= "        { " & "%ID%: %" & myObj.ID & "%, %lat%: " & myObj.Lat & ", %lon%:" & myObj.Lon & ", %WP%:" & Math.Round(useLevels.getWPOutlet, 2) & ", %ZP%:" & Math.Round(useLevels.getZPOutlet, 2) & ", %Simulated%:" & Math.Round(resultsVal, 2) & ", %Diff%:" & Math.Round(resultsVal - refVal, 2) & "}," & vbCrLf
                                Else
                                    JSONContent &= "        { " & "%ID%: %" & myObj.ID & "%, %lat%: " & myObj.Lat & ", %lon%:" & myObj.Lon & ", %WP%: 0, %ZP%: 0, %Simulated%:" & Math.Round(resultsVal, 2) & ", %Diff%: " & Math.Round(resultsVal - refVal, 2) & "}," & vbCrLf
                                End If

                                NodesDone.Add(myObj.ID.Trim.ToUpper, myObj)

                                'when working with a datatable collection
                                tsCollection.Remove(myObj.ID.Trim.ToUpper) '//siebe 10-10-2017 to create more memory space
                                ts = Nothing

                            End If
                        End If
                    End If
                Next
            Next

            'finalize our JSON block
            JSONContent = JSONContent.Replace("%", Chr(34))
            JSONContent = Strings.Left(JSONContent, JSONContent.Length - 1) & "    ]" & vbCrLf & "}" & vbCrLf

            myReader.Close()
            Setup.GISData.PointShapeFile.SF.sf.StopEditingShapes(True, True)
            Setup.GISData.PointShapeFile.SF.Close()

            Setup.GISData.SubcatchmentDataSource.setEndPointInShapefile() 'close the the point-in-polygon mode
            Setup.GISData.SubcatchmentDataSource.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function ReachObjectResultsStatisticsToShapefile(ShapeFilePath As String, HisFilename As String, ParsList As List(Of String), SkipPercentage As Double, ResultsTimestepIdx As Integer, ComputeDifference As Boolean, ResultsCompareTimestep As Boolean, ResultsCompareTimestepIdx As Integer, ResultsCompareMax As Boolean, ResultsCompareMin As Boolean, WritePercentiles As Boolean, PercentileSize As Boolean, WriteFirst As Boolean, WriteLast As Boolean, WriteMax As Boolean, WriteMin As Boolean, Optional ByVal Multiplier As Double = 1, Optional ByVal WriteCulvertInverts As Boolean = False) As Boolean
        Try
            Dim myStats As Dictionary(Of Integer, Double) 'percentile vs value
            Dim i As Long = 0
            Dim j As Long, p As Long
            Dim NewIdx As Integer, ShapeIdx As Integer
            Dim n As Long = CFTopo.Reaches.Reaches.Count
            Dim FirstVal As Double, LastVal As Double, MinVal As Double, MaxVal As Double, TSVal As Double, refVal As Double
            Dim useLevels As clsTargetLevels = Nothing

            'Dim dt As DataTable
            Dim ts As List(Of Single)
            Dim IDFieldIdx As Integer, TypeFieldIdx As Integer, LastValIdx As Integer, FirstValIdx As Integer
            Dim MaxValIdx As Integer, MinValIdx As Integer
            Dim InvertUpIdx As Integer, InvertDnIdx As Integer
            Dim TSValIdx As Integer, RefValIdx As Integer, DifValIdx As Integer, AbsDifValIdx As Integer
            Dim PercentileFields As New List(Of Integer)
            'this function writes the statistics for the water levels in the current case to a shapefile.

            'read all hisfile results for the parameter waterlevel to a collection of datatables (one per location)
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\" & HisFilename, Me.Setup)
            myReader.OpenFile()
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = myReader.ReadHisHeader()

            'create the shapefields
            Setup.GISData.PointShapeFile = New clsPointShapeFile(Me.Setup)
            Setup.GISData.PointShapeFile.SF.sf.CreateNew(ShapeFilePath, MapWinGIS.ShpfileType.SHP_POINT)
            If Not Setup.GISData.PointShapeFile.SF.sf.StartEditingShapes(True) Then MsgBox("Could not start editing shapes.")

            Setup.GISData.PointShapeFile.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 50, 0, IDFieldIdx)
            Setup.GISData.PointShapeFile.AddField("OBJECTTYPE", MapWinGIS.FieldType.STRING_FIELD, 50, 0, TypeFieldIdx)

            Dim ShortList As New List(Of String)
            For Each PartOfParameterName As String In ParsList

                'reset the NodesDone collection. For each parameter we will start over.
                Dim NodesDone As New Dictionary(Of String, clsSbkReachObject) 'keeps track of which nodes have already been processed

                'create a short name for this parameter so we can use it as a field ID
                Dim ShortName As String = Me.Setup.GeneralFunctions.MakeUniqueID(ShortList, PartOfParameterName, 4, True)
                ShortList.Add(ShortName)

                'let's see if we can store the results in a dictionary of timeseries (less memory than a collection of datatables?)
                Dim tsCollection As New Dictionary(Of String, List(Of Single))
                tsCollection = myReader.ReadAllDataOneParameterToTimeSeriesDictionary(PartOfParameterName,, Multiplier)

                If WriteFirst Then Setup.GISData.PointShapeFile.AddField("FIRST_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, FirstValIdx)
                If WriteLast Then Setup.GISData.PointShapeFile.AddField("LAST_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, LastValIdx)
                If WriteMax Then Setup.GISData.PointShapeFile.AddField("MAX_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MaxValIdx)
                If WriteMin Then Setup.GISData.PointShapeFile.AddField("MIN_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MinValIdx)
                Setup.GISData.PointShapeFile.AddField("TSVAL_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, TSValIdx)

                If ComputeDifference Then
                    Setup.GISData.PointShapeFile.AddField("REF_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, RefValIdx)
                    Setup.GISData.PointShapeFile.AddField("DIF_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, DifValIdx)
                    Setup.GISData.PointShapeFile.AddField("ABSDIF" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, AbsDifValIdx)
                End If

                If WritePercentiles Then
                    For j = 0 To 100 Step PercentileSize
                        Setup.GISData.PointShapeFile.AddField("PCT" & j & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, NewIdx)
                        PercentileFields.Add(NewIdx)
                    Next
                End If

                'extras
                If WriteCulvertInverts Then
                    Setup.GISData.PointShapeFile.AddField("BOBUP_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, InvertUpIdx)
                    Setup.GISData.PointShapeFile.AddField("BOBDN_" & ShortName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, InvertDnIdx)
                End If

                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading results statistics...", 0, 10, True)
                Dim iReach As Integer, nReaches As Integer = CFTopo.Reaches.Reaches.Count

                For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values

                    iReach += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReaches)

                    If myHeader.Locations.Contains(myReach.bn.ID) Then
                        'first fake the start and endpoint of this reach as if they are reach objects
                        Dim bn As New clsSbkReachObject(Me.Setup, Me) With {
                        .nt = enmNodetype.NodeCFConnectionNode,
                        .ID = myReach.bn.ID,
                        .ci = myReach.Id,
                        .lc = 0,
                        .InUse = True
                            }
                        If Not myReach.ReachObjects.ReachObjects.ContainsKey(bn.ID.Trim.ToUpper) Then myReach.ReachObjects.Add(bn)
                    End If

                    If myHeader.Locations.Contains(myReach.en.ID) Then
                        Dim en As New clsSbkReachObject(Me.Setup, Me) With {
                        .nt = enmNodetype.NodeCFConnectionNode,
                        .ID = myReach.en.ID,
                        .ci = myReach.Id,
                        .lc = myReach.getReachLength,
                        .InUse = True
                            }
                        If Not myReach.ReachObjects.ReachObjects.ContainsKey(en.ID.Trim.ToUpper) Then myReach.ReachObjects.Add(en)
                    End If

                    i += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myHeader.Locations.Contains(myObj.ID) AndAlso Not NodesDone.ContainsKey(myObj.ID.Trim.ToUpper) Then

                            myObj.calcXY()

                            If tsCollection.ContainsKey(myObj.ID.Trim.ToUpper) Then
                                ts = tsCollection.Item(myObj.ID.Trim.ToUpper)

                                Dim StartRowIdx As Long = SkipPercentage / 100 * (ts.Count - 1)
                                Dim EndRowIdx As Long = ts.Count - 1

                                myObj.calcXY()
                                myStats = New Dictionary(Of Integer, Double)
                                Setup.GeneralFunctions.SinglesListStats(ts, StartRowIdx, EndRowIdx, FirstVal, LastVal, MinVal, MaxVal, WritePercentiles, PercentileSize, myStats)

                                ShapeIdx = Setup.GISData.PointShapeFile.GetShapeIdx(IDFieldIdx, myObj.ID)
                                If ShapeIdx < 0 Then
                                    'create a new shape in the point shapefile
                                    Dim newShape As New MapWinGIS.Shape
                                    If Not newShape.Create(MapWinGIS.ShpfileType.SHP_POINT) Then MsgBox("Could not create shape.")
                                    Call newShape.AddPoint(myObj.X, myObj.Y)
                                    ShapeIdx = Setup.GISData.PointShapeFile.SF.sf.EditAddShape(newShape)
                                    If ShapeIdx < 0 Then Throw New Exception("Error writing points to shapefile.")
                                    If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID) Then Me.Setup.Log.AddWarning("Could not write ID to shapefile.")
                                    If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(TypeFieldIdx, ShapeIdx, myObj.nt.ToString) Then Me.Setup.Log.AddWarning("Could not write Object Type to shapefile.")
                                End If

                                'write the statistical values to their corresponding fields
                                If WriteFirst Then If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(FirstValIdx, ShapeIdx, FirstVal) Then Me.Setup.Log.AddWarning("Could not write first value from hisfile to shapefile.")
                                If WriteLast Then If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(LastValIdx, ShapeIdx, LastVal) Then Me.Setup.Log.AddWarning("Could not write last value from hisfile to shapefile.")
                                If WriteMin Then If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(MinValIdx, ShapeIdx, MinVal) Then Me.Setup.Log.AddWarning("Could Not write minimum value From hisfile to shapefile.")
                                If WriteMax Then If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(MaxValIdx, ShapeIdx, MaxVal) Then Me.Setup.Log.AddWarning("Could Not write maximum value From hisfile to shapefile.")
                                If WritePercentiles Then
                                    p = -1
                                    For k = 0 To 100 Step 5
                                        p += 1
                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(PercentileFields(p), ShapeIdx, myStats.Item(k)) Then Me.Setup.Log.AddWarning("Could not write maximum value from hisfile to shapefile.")
                                    Next
                                End If

                                'write the specific output for the chosen timestep
                                If ResultsTimestepIdx >= 0 Then
                                    TSVal = ts(ResultsTimestepIdx)
                                    If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(TSValIdx, ShapeIdx, TSVal) Then Me.Setup.Log.AddWarning("Could not write value for chosen timstep to shapefile.")

                                    If ComputeDifference Then
                                        If ResultsCompareTimestep Then
                                            refVal = ts(ResultsCompareTimestepIdx)
                                        ElseIf ResultsCompareMax Then
                                            refVal = MaxVal
                                        ElseIf ResultsCompareMin Then
                                            refVal = MinVal
                                        End If

                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(RefValIdx, ShapeIdx, refVal) Then Me.Setup.Log.AddWarning("Could not write reference value to shapefile.")
                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(DifValIdx, ShapeIdx, (TSVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write values difference to shapefile.")
                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(AbsDifValIdx, ShapeIdx, Math.Abs(TSVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write absolute value of difference to shapefile.")
                                    End If
                                End If

                                '------------------------------------------------------------------------------------------------------------------------
                                'write the extras!
                                '------------------------------------------------------------------------------------------------------------------------
                                If WriteCulvertInverts AndAlso myObj.nt = enmNodetype.NodeCFCulvert Then
                                    Dim StrucDat As clsStructDatRecord = Nothing, StrucDef As clsStructDefRecord = Nothing, ContrDef As clsControlDefRecord = Nothing
                                    Me.CFData.Data.StructureData.GetStructureRecords(myObj.ID, StrucDat, StrucDef, ContrDef)
                                    If Not StrucDef Is Nothing Then
                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(InvertUpIdx, ShapeIdx, StrucDef.ll) Then Me.Setup.Log.AddWarning("Could not write culvert upstream invert level to shapefile.")
                                        If Not Setup.GISData.PointShapeFile.SF.sf.EditCellValue(InvertDnIdx, ShapeIdx, StrucDef.rl) Then Me.Setup.Log.AddWarning("Could not write culvert upstream invert level to shapefile.")
                                    End If
                                End If

                                'finally add this reach object to the "NodesDone" collection
                                NodesDone.Add(myObj.ID.Trim.ToUpper, myObj)

                                'when working with a datatable collection
                                tsCollection.Remove(myObj.ID.Trim.ToUpper) '//siebe 10-10-2017 to create more memory space
                                ts = Nothing

                            End If
                        End If
                    Next
                Next
            Next

            myReader.Close()
            Setup.GISData.PointShapeFile.SF.sf.StopEditingShapes(True, True)
            Setup.GISData.PointShapeFile.SF.Close()
            Setup.GISData.SubcatchmentDataSource.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function NetworkNTWReachSegmentResultsStatisticsToShapefile(ShapeFilePath As String, PartOfParameterName As String, SkipPercentage As Double, ResultsTimestepIdx As Integer, ComputeDifference As Boolean, ResultsCompareTimestep As Boolean, ResultsCompareTimestepIdx As Integer, ResultsCompareMax As Boolean, ResultsCompareMin As Boolean, WritePercentiles As Boolean, PercentileSize As Boolean, WriteFirst As Boolean, WriteLast As Boolean, WriteMax As Boolean, WriteMin As Boolean) As Boolean
        Try
            Dim myStats As New Dictionary(Of Integer, Double) 'percentile vs value
            Dim i As Long = 0
            Dim j As Long, p As Long
            Dim NewIdx As Integer, ShapeIdx As Integer
            Dim n As Long = CFTopo.Reaches.Reaches.Count
            Dim FirstVal As Double, LastVal As Double, MinVal As Double, MaxVal As Double, TSVal As Single, refVal As Double
            Dim useLevels As clsTargetLevels = Nothing

            'Dim dt As DataTable
            Dim ts As New List(Of Single)
            Dim NodesDone As New Dictionary(Of String, clsSbkReachObject) 'keeps track of which nodes have already been processed

            Dim IDFieldIdx As Integer, TypeFieldIdx As Integer, LastValIdx As Integer, FirstValIdx As Integer
            Dim MaxValIdx As Integer, MinValIdx As Integer
            Dim TSValIdx As Integer, RefValIdx As Integer, DifValIdx As Integer, AbsDifValIdx As Integer
            Dim PercentileFields As New List(Of Integer)
            'this function writes the statistics for the water levels in the current case to a shapefile.

            'read all hisfile results for the parameter waterlevel to a collection of datatables (one per location)
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\reachseg.his", Me.Setup)
            myReader.OpenFile()
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = myReader.ReadHisHeader()

            'let's see if we can store the results in a dictionary of timeseries (less memory than a collection of datatables?)
            Dim tsCollection As New Dictionary(Of String, List(Of Single))
            tsCollection = myReader.ReadAllDataOneParameterToTimeSeriesDictionary(PartOfParameterName)

            'create the shapefields
            Setup.GISData.PolyLineShapeFile = New clsPolyLineShapeFile(Me.Setup)
            Setup.GISData.PolyLineShapeFile.SF.sf.CreateNew(ShapeFilePath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            If Not Setup.GISData.PolyLineShapeFile.SF.sf.StartEditingShapes(True) Then MsgBox("Could not start editing shapes.")

            Setup.GISData.PolyLineShapeFile.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 50, 0, IDFieldIdx)
            Setup.GISData.PolyLineShapeFile.AddField("STRUCTURE", MapWinGIS.FieldType.STRING_FIELD, 50, 0, TypeFieldIdx)
            If WriteFirst Then Setup.GISData.PolyLineShapeFile.AddField("FIRST", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, FirstValIdx)
            If WriteLast Then Setup.GISData.PolyLineShapeFile.AddField("LAST", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, LastValIdx)
            If WriteMax Then Setup.GISData.PolyLineShapeFile.AddField("MAX", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MaxValIdx)
            If WriteMin Then Setup.GISData.PolyLineShapeFile.AddField("MIN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MinValIdx)
            Setup.GISData.PolyLineShapeFile.AddField("TSVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, TSValIdx)

            If ComputeDifference Then
                Setup.GISData.PolyLineShapeFile.AddField("REFVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, RefValIdx)
                Setup.GISData.PolyLineShapeFile.AddField("DIF", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, DifValIdx)
                Setup.GISData.PolyLineShapeFile.AddField("ABSDIF", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, AbsDifValIdx)
            End If

            If WritePercentiles Then
                For j = 0 To 100 Step PercentileSize
                    Setup.GISData.PolyLineShapeFile.AddField("pct" & j, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, NewIdx)
                    PercentileFields.Add(NewIdx)
                Next
            End If

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading results statistics...", 0, 10, True)
            Dim iSeg As Integer = 0, nSeg As Integer = NetworkNTW.ReachSegments.Count

            For Each mySegment As clsNetworkNTWReachSegment In NetworkNTW.ReachSegments.Values
                iSeg += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iSeg, nSeg)

                'retrieve the reach that this reach segment is part of
                Dim myReach As clsSbkReach = CFTopo.Reaches.GetReach(Me.Setup.GeneralFunctions.RemovePostfix(mySegment.getID, "_"))
                If tsCollection.ContainsKey(mySegment.getID.Trim.ToUpper) Then
                    ts = tsCollection.Item(mySegment.getID.Trim.ToUpper)
                    Dim StartRowIdx As Long = SkipPercentage / 100 * (ts.Count - 1)
                    Dim EndRowIdx As Long = ts.Count - 1
                    myStats = New Dictionary(Of Integer, Double)
                    Setup.GeneralFunctions.SinglesListStats(ts, StartRowIdx, EndRowIdx, FirstVal, LastVal, MinVal, MaxVal, WritePercentiles, PercentileSize, myStats)
                End If

                'create the shape (Polyline) and add it to the shapefile. Also retrieve the shape's index number
                Dim myShape As New MapWinGIS.Shape
                myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
                myShape = myReach.BuildShape(mySegment.FromNodeChainage, mySegment.ToNodeChainage)
                Setup.GISData.PolyLineShapeFile.SF.sf.EditAddShape(myShape)
                ShapeIdx = Setup.GISData.PolyLineShapeFile.SF.sf.NumShapes - 1

                'write the object ID to the shapefile
                Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(IDFieldIdx, ShapeIdx, mySegment.getID)

                'write information to the shapefile whether this shape contains a structure
                If mySegment.ContainsStructure Then
                    Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(TypeFieldIdx, ShapeIdx, "True")
                Else
                    Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(TypeFieldIdx, ShapeIdx, "False")
                End If

                'write the required statistical values to the shapefile
                If WriteFirst Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(FirstValIdx, ShapeIdx, FirstVal) Then Me.Setup.Log.AddWarning("Could not write first value from hisfile to shapefile.")
                If WriteLast Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(LastValIdx, ShapeIdx, LastVal) Then Me.Setup.Log.AddWarning("Could not write last value from hisfile to shapefile.")
                If WriteMin Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(MinValIdx, ShapeIdx, MinVal) Then Me.Setup.Log.AddWarning("Could Not write minimum value From hisfile to shapefile.")
                If WriteMax Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(MaxValIdx, ShapeIdx, MaxVal) Then Me.Setup.Log.AddWarning("Could Not write maximum value From hisfile to shapefile.")
                If WritePercentiles Then
                    p = -1
                    For k = 0 To 100 Step 5
                        p += 1
                        If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(PercentileFields(p), ShapeIdx, myStats.Item(k)) Then Me.Setup.Log.AddWarning("Could not write maximum value from hisfile to shapefile.")
                    Next
                End If

                'write the specific output for the chosen timestep
                If ResultsTimestepIdx >= 0 Then
                    TSVal = ts(ResultsTimestepIdx)
                    If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(TSValIdx, ShapeIdx, TSVal) Then Me.Setup.Log.AddWarning("Could not write value for chosen timstep to shapefile.")

                    If ComputeDifference Then
                        If ResultsCompareTimestep Then
                            refVal = ts(ResultsCompareTimestepIdx)
                        ElseIf ResultsCompareMax Then
                            refVal = MaxVal
                        ElseIf ResultsCompareMin Then
                            refVal = MinVal
                        End If

                        If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(RefValIdx, ShapeIdx, refVal) Then Me.Setup.Log.AddWarning("Could not write reference value to shapefile.")
                        If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(DifValIdx, ShapeIdx, (TSVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write values difference to shapefile.")
                        If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(AbsDifValIdx, ShapeIdx, Math.Abs(TSVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write absolute value of difference to shapefile.")
                    End If
                End If
            Next
            myReader.Close()
            Setup.GISData.PolyLineShapeFile.SF.sf.StopEditingShapes(True, True)
            Setup.GISData.PolyLineShapeFile.SF.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function ReachSegmentResultsStatisticsToShapefile(ShapeFilePath As String, PartOfParameterName As String, SkipPercentage As Double, ResultsTimestepIdx As Integer, ComputeDifference As Boolean, ResultsCompareTimestep As Boolean, ResultsCompareTimestepIdx As Integer, ResultsCompareMax As Boolean, ResultsCompareMin As Boolean, WritePercentiles As Boolean, PercentileSize As Boolean, WriteFirst As Boolean, WriteLast As Boolean, WriteMax As Boolean, WriteMin As Boolean, Optional ByVal Multiplier As Double = 1) As Boolean
        'this function reads results for a reach segment and exports them to a polyline shapefile
        'now the complicated thing about it is that the ID as displayed in reachseg.his is stored in the Network.NTW reachsegments
        'however: network.ntw only gives a small portion of the network that is represented by that particular ID. 
        'therefore we will also have to read the other network.* files in order to complete each reach segment
        Try
            'variables
            Dim myStats As Dictionary(Of Integer, Double) 'percentile vs value
            Dim i As Long = 0
            Dim j As Long, p As Long
            Dim NewIdx As Integer, ShapeIdx As Integer
            Dim n As Long = CFTopo.Reaches.Reaches.Count
            Dim FirstVal As Double, LastVal As Double, MinVal As Double, MaxVal As Double, TSVal As Single, refVal As Double
            Dim useLevels As clsTargetLevels = Nothing
            Dim ts As List(Of Single)
            Dim NodesDone As New Dictionary(Of String, clsSbkReachObject) 'keeps track of which nodes have already been processed
            Dim IDFieldIdx As Integer, TypeFieldIdx As Integer, LastValIdx As Integer, FirstValIdx As Integer
            Dim MaxValIdx As Integer, MinValIdx As Integer
            Dim TSValIdx As Integer, RefValIdx As Integer, DifValIdx As Integer, AbsDifValIdx As Integer
            Dim PercentileFields As New List(Of Integer)
            Dim NTWReachSegment As clsNetworkNTWReachSegment

            'read all hisfile results for the parameter waterlevel to a collection of datatables (one per location)
            Dim myReader As New clsHisFileBinaryReader(CaseDir & "\reachseg.his", Me.Setup)
            myReader.OpenFile()
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = myReader.ReadHisHeader()

            'let's see if we can store the results in a dictionary of timeseries (less memory than a collection of datatables?)
            Dim tsCollection As New Dictionary(Of String, List(Of Single))
            tsCollection = myReader.ReadAllDataOneParameterToTimeSeriesDictionary(PartOfParameterName,, Multiplier)

            'create the shapefile and its shapefields
            Setup.GISData.PolyLineShapeFile = New clsPolyLineShapeFile(Me.Setup)
            Setup.GISData.PolyLineShapeFile.SF.sf.CreateNew(ShapeFilePath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            If Not Setup.GISData.PolyLineShapeFile.SF.sf.StartEditingShapes(True) Then MsgBox("Could not start editing shapes.")
            Setup.GISData.PolyLineShapeFile.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 50, 0, IDFieldIdx)
            Setup.GISData.PolyLineShapeFile.AddField("STRUCTURE", MapWinGIS.FieldType.STRING_FIELD, 50, 0, TypeFieldIdx)
            If WriteFirst Then Setup.GISData.PolyLineShapeFile.AddField("FIRST", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, FirstValIdx)
            If WriteLast Then Setup.GISData.PolyLineShapeFile.AddField("LAST", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, LastValIdx)
            If WriteMax Then Setup.GISData.PolyLineShapeFile.AddField("MAX", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MaxValIdx)
            If WriteMin Then Setup.GISData.PolyLineShapeFile.AddField("MIN", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, MinValIdx)
            Setup.GISData.PolyLineShapeFile.AddField("TSVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, TSValIdx)

            If ComputeDifference Then
                Setup.GISData.PolyLineShapeFile.AddField("REFVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, RefValIdx)
                Setup.GISData.PolyLineShapeFile.AddField("DIF", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, DifValIdx)
                Setup.GISData.PolyLineShapeFile.AddField("ABSDIF", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, AbsDifValIdx)
            End If

            If WritePercentiles Then
                For j = 0 To 100 Step PercentileSize
                    Setup.GISData.PolyLineShapeFile.AddField("pct" & j, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 3, NewIdx)
                    PercentileFields.Add(NewIdx)
                Next
            End If


            'next we will walk through all reaches and their reach objects, from calculation point to calculation point
            Dim iReach As Integer
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading results statistics...", 0, 10, True)
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, CFTopo.Reaches.Reaches.Count)
                'NOTE: WE ASSUME THAT THE START- AND ENDNODE FOR THIS REACH HAVE ALREADY BEEN ADDED AS REACHOBJECTS
                'next we wil walk through each of the reach objects and decide the present reach segment
                For i = 0 To myReach.ReachObjects.ReachObjects.Values.Count - 2
                    If myReach.ReachObjects.ReachObjects.Values(i).isWaterLevelObject Then

                        'we have found a reach segment that will actually be represented in reachseg.his
                        'so find the corresponding network.ntw reachsegment
                        NTWReachSegment = NetworkNTW.GetReachSegmentByStartEndNode(myReach.ReachObjects.ReachObjects.Values(i).ID, myReach.ReachObjects.ReachObjects.Values(i + 1).ID)
                        If NTWReachSegment Is Nothing Then
                            Me.Setup.Log.AddError("Error: could not find Network.NTW Reachsegment starting with node " & myReach.ReachObjects.ReachObjects.Values(i).ID & " and ending with node " & myReach.ReachObjects.ReachObjects.Values(i + 1).ID & ".")
                            Continue For
                        End If

                        'move on to the next waterlevel point in order to seek the end point.  In the meantime: register if the reachsegment contains a structure
                        NTWReachSegment.FromNodeChainage = myReach.ReachObjects.ReachObjects.Values(i).lc
                        For j = i + 1 To myReach.ReachObjects.ReachObjects.Values.Count - 1
                            If myReach.ReachObjects.ReachObjects.Values(j).isWaterLevelObject Then
                                'found the actual endpoint of the reach segment so exit the loop!
                                NTWReachSegment.ToNodeChainage = myReach.ReachObjects.ReachObjects.Values(j).lc
                                i = j - 1 'set the counter back one digit so that the next iteration of i starts at the current value of j.
                                Exit For
                            ElseIf myReach.ReachObjects.ReachObjects.Values(j).isStructure Then
                                NTWReachSegment.ContainsStructure = True
                            End If

                        Next

                        'since we've found a valid reach segement we can start reading the results
                        If tsCollection.ContainsKey(NTWReachSegment.getID.Trim.ToUpper) Then
                            ts = tsCollection.Item(NTWReachSegment.getID.Trim.ToUpper)
                            Dim StartRowIdx As Long = SkipPercentage / 100 * (ts.Count - 1)
                            Dim EndRowIdx As Long = ts.Count - 1
                            myStats = New Dictionary(Of Integer, Double)
                            Setup.GeneralFunctions.SinglesListStats(ts, StartRowIdx, EndRowIdx, FirstVal, LastVal, MinVal, MaxVal, WritePercentiles, PercentileSize, myStats)

                            'create the shape (Polyline) and add it to the shapefile. Also retrieve the shape's index number
                            Dim myShape As New MapWinGIS.Shape
                            myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
                            myShape = myReach.BuildShape(NTWReachSegment.FromNodeChainage, NTWReachSegment.ToNodeChainage)
                            Setup.GISData.PolyLineShapeFile.SF.sf.EditAddShape(myShape)
                            ShapeIdx = Setup.GISData.PolyLineShapeFile.SF.sf.NumShapes - 1

                            'write the object ID to the shapefile
                            Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(IDFieldIdx, ShapeIdx, NTWReachSegment.getID)

                            'write information to the shapefile whether this shape contains a structure
                            If NTWReachSegment.ContainsStructure Then
                                Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(TypeFieldIdx, ShapeIdx, "True")
                            Else
                                Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(TypeFieldIdx, ShapeIdx, "False")
                            End If

                            'write the required statistical values to the shapefile
                            If WriteFirst Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(FirstValIdx, ShapeIdx, FirstVal) Then Me.Setup.Log.AddWarning("Could not write first value from hisfile to shapefile.")
                            If WriteLast Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(LastValIdx, ShapeIdx, LastVal) Then Me.Setup.Log.AddWarning("Could not write last value from hisfile to shapefile.")
                            If WriteMin Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(MinValIdx, ShapeIdx, MinVal) Then Me.Setup.Log.AddWarning("Could Not write minimum value From hisfile to shapefile.")
                            If WriteMax Then If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(MaxValIdx, ShapeIdx, MaxVal) Then Me.Setup.Log.AddWarning("Could Not write maximum value From hisfile to shapefile.")
                            If WritePercentiles Then
                                p = -1
                                For k = 0 To 100 Step 5
                                    p += 1
                                    If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(PercentileFields(p), ShapeIdx, myStats.Item(k)) Then Me.Setup.Log.AddWarning("Could not write maximum value from hisfile to shapefile.")
                                Next
                            End If

                            'write the specific output for the chosen timestep
                            If ResultsTimestepIdx >= 0 Then
                                TSVal = ts(ResultsTimestepIdx)
                                If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(TSValIdx, ShapeIdx, TSVal) Then Me.Setup.Log.AddWarning("Could not write value for chosen timstep to shapefile.")

                                If ComputeDifference Then
                                    If ResultsCompareTimestep Then
                                        refVal = ts(ResultsCompareTimestepIdx)
                                    ElseIf ResultsCompareMax Then
                                        refVal = MaxVal
                                    ElseIf ResultsCompareMin Then
                                        refVal = MinVal
                                    End If

                                    If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(RefValIdx, ShapeIdx, refVal) Then Me.Setup.Log.AddWarning("Could not write reference value to shapefile.")
                                    If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(DifValIdx, ShapeIdx, (TSVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write values difference to shapefile.")
                                    If Not Setup.GISData.PolyLineShapeFile.SF.sf.EditCellValue(AbsDifValIdx, ShapeIdx, Math.Abs(TSVal - refVal)) Then Me.Setup.Log.AddWarning("Could not write absolute value of difference to shapefile.")
                                End If
                            End If
                        End If
                    End If
                Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete", 0, 10, True)

            myReader.Close()
            Setup.GISData.PolyLineShapeFile.SF.sf.StopEditingShapes(True, True)
            Setup.GISData.PolyLineShapeFile.SF.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Friend Sub SetReferenceCase(ByRef refCase As ClsSobekCase)
        ReferenceCase = refCase
    End Sub

    Public Sub Export(ByVal Append As Boolean, TargetModel As GeneralFunctions.enmSimulationModel, ByVal RRTopoExport As Boolean, ByVal RRDataExport As Boolean, ByVal CFTopoExport As Boolean, ByVal CFDataExport As Boolean)
        If TargetModel = enmSimulationModel.SOBEK Then
            If RRTopoExport Then RRTopo.WriteNetwork(Append, Me.Setup.Settings.ExportDirSobekTopo)
            If RRDataExport Then RRData.ExportAll(Append, Me.Setup.Settings.ExportDirSobekRR)
            If CFTopoExport Then CFTopo.WriteNetwork(Append, Me.Setup.Settings.ExportDirSobekTopo)
            If CFDataExport Then CFData.ExportAll(Append, Me.Setup.Settings.ExportDirSobekFlow)
        ElseIf TargetModel = enmSimulationModel.DHYDRO Then
            If RRTopoExport Then RRTopo.WriteNetwork(Append, Me.Setup.Settings.ExportDirDHydro)
            If RRDataExport Then RRData.ExportAll(Append, Me.Setup.Settings.ExportDirDHydroRR)
            If CFTopoExport Then CFTopo.WriteNetwork(Append, Me.Setup.Settings.ExportDirDHydro)
            If CFDataExport Then CFData.ExportAll(Append, Me.Setup.Settings.ExportDirDHydro)
        End If
    End Sub

    Public Sub AddPrefix(ByVal Prefix As String)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Adding prefix to case " & CaseName, 0, 1)
        CFTopo.AddPrefix(Prefix)
        Me.Setup.GeneralFunctions.UpdateProgressBar("", 1, 4)
        CFData.AddPrefix(Prefix)
        Me.Setup.GeneralFunctions.UpdateProgressBar("", 2, 4)
        RRTopo.AddPrefix(Prefix)
        Me.Setup.GeneralFunctions.UpdateProgressBar("", 3, 4)
        RRData.AddPrefix(Prefix)
        Me.Setup.GeneralFunctions.UpdateProgressBar("", 4, 4)
    End Sub

    Public Function ReadAllNodesOfType(ByVal NodeType As enmNodetype) As List(Of String)
        'returns a full list of all nodes of a specified type inside the schematization
        'note: this is a quick read, so not an entire case read!
        Dim DataFile As clsSobekDataFile, Records As Collection, i As Long
        Dim NodesList As New List(Of String)

        'read all node types from network.obi.
        DataFile = New clsSobekDataFile(Me.Setup)
        Records = DataFile.Read(CaseDir & "\NETWORK.OBI", "OBID")
        If Records.Count > 0 Then
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Node Types...", 0, Records.Count)
            For i = 1 To Records.Count
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                Dim myRecord As New clsNetworkOBIOBIDRecord(Me.Setup)
                myRecord.Read(Records(i))
                If myRecord.GetNodeType = NodeType Then
                    NodesList.Add(myRecord.ID)
                End If
            Next i
        End If
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
        Return NodesList

    End Function

    Public Sub ReadNodeTypes()

        'builds a list of RR node types
        Dim myFile As String, NodeTypesFound As Boolean
        Dim myNtrPluvRecord As String = "", myID As String = "", myParentID As String = ""
        Dim tmpStr As String, myNum As Integer, myParentNum As Integer
        Dim MaxNumStandardType As Integer = -999
        Dim nCustomNodeTypes As Integer, myCustomNodeType As Integer
        Dim myToken As String, myValue As String, CheckSum As Integer
        Dim myType As clsSobekNodeType

        Try
            'set the path to the ntrpluv.ini file
            myFile = Me.Setup.SOBEKData.ActiveProject.ProgramsDir & "\ini\ntrpluv.ini"
            NodeTypes = New clsSobekNodeTypes

            If System.IO.File.Exists(myFile) Then
                'zoek het 'hoofdstuk' waar de knooptypes zijn gedefinieerd
                Dim ntrPluvIniFile As New StreamReader(myFile)
                While Not NodeTypesFound And Not ntrPluvIniFile.EndOfStream
                    myNtrPluvRecord = ntrPluvIniFile.ReadLine
                    If InStr(myNtrPluvRecord, "[Node Types]") > 0 Then NodeTypesFound = True
                End While

                'vul nu de klasse met knooptypes
                If NodeTypesFound Then
                    Do
                        myNtrPluvRecord = ntrPluvIniFile.ReadLine
                        If myNtrPluvRecord = "" Then Exit Do
                        'als we een knoopid tegenkomen, lees nummer en id
                        If InStr(myNtrPluvRecord, " ID=") > 0 Then
                            myNum = Convert.ToInt16(Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord))
                            myParentNum = myNum
                            tmpStr = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord, "=")
                            myID = Trim(myNtrPluvRecord)
                            myType = New clsSobekNodeType
                            myType.ID = myID
                            myType.ParentID = myType.ID 'we geven hem zichzelf tevens als parent type, dat is later makkelijk
                            myType.SbkNum = myNum
                            NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)
                            If myNum > MaxNumStandardType Then MaxNumStandardType = myNum
                        End If
                    Loop
                Else
                    Throw New Exception("Error reading file with nodetypes " & myFile)
                End If
                ntrPluvIniFile.Close()
            Else
                Me.Setup.Log.AddWarning("File Not found: " & myFile & ". custom node types can therefore Not be identified And processed.")
                Call PopulateStandardNodeTypes()
            End If

            'nu doen we hetzelfde voor de user defined types in ntrpluv.obj van de case
            myFile = CaseDir & "\ntrpluv.obj"

            If System.IO.File.Exists(myFile) Then
                NodeTypesFound = False
                'zoek het 'hoofdstuk' waar de knooptypes zijn gedefinieerd
                Dim ntrPluvObjFile As New StreamReader(myFile)
                While Not NodeTypesFound And Not ntrPluvObjFile.EndOfStream
                    myNtrPluvRecord = ntrPluvObjFile.ReadLine
                    If InStr(myNtrPluvRecord, "[User Node Types]") > 0 Then
                        NodeTypesFound = True
                        myNtrPluvRecord = ntrPluvObjFile.ReadLine
                        tmpStr = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord, "=")
                        nCustomNodeTypes = myNtrPluvRecord
                    End If
                End While

                'nu de custom knooptypes
                If NodeTypesFound Then
                    CheckSum = 0
                    While Not ntrPluvObjFile.EndOfStream Or InStr(myNtrPluvRecord, "[") > 0

                        myNtrPluvRecord = ntrPluvObjFile.ReadLine
                        If myNtrPluvRecord.Trim = "" Then Exit While

                        myCustomNodeType = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord)
                        myNum = MaxNumStandardType + myCustomNodeType
                        myToken = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord, "=")
                        myValue = myNtrPluvRecord

                        'maak het setje van ID en ParentID compleet
                        If myToken.ToLower = "parentid" Then
                            CheckSum += 1
                            myParentID = myValue.Trim
                        ElseIf myToken.ToLower = "id" Then
                            CheckSum += 1
                            myID = myValue.Trim
                        End If

                        If CheckSum = 2 Then
                            myType = New clsSobekNodeType With {
                            .ID = myID,
                            .ParentID = myParentID,
                            .SbkNum = myNum
                                }
                            NodeTypes.NodeTypes.Add(myID.Trim.ToUpper, myType)
                            CheckSum = 0
                        End If
                    End While
                Else
                    Throw New Exception("Error reading file with nodetypes " & myFile)
                End If
                ntrPluvObjFile.Close()
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub ReadNodeTypes of class clsSobekCase.")
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Sub


    Public Sub ReadBranchTypes()

        'builds a list of SOBEK branch types
        Dim myFile As String, BranchTypesFound As Boolean
        Dim myNtrPluvRecord As String = "", myID As String = "", myParentID As String = ""
        Dim tmpStr As String, myNum As Integer, myParentNum As Integer
        Dim MaxNumStandardType As Integer = -999
        Dim nCustomBranchTypes As Integer, myCustomBranchType As Integer
        Dim myToken As String, myValue As String, CheckSum As Integer
        Dim myType As clsSobekBranchType

        Try
            'set the path to the ntrpluv.ini file
            myFile = Me.Setup.SOBEKData.ActiveProject.ProgramsDir & "\ini\ntrpluv.ini"
            BranchTypes = New clsSobekBranchTypes

            If System.IO.File.Exists(myFile) Then
                'zoek het 'hoofdstuk' waar de taktypes zijn gedefinieerd
                Dim ntrPluvIniFile As New StreamReader(myFile)
                While Not BranchTypesFound And Not ntrPluvIniFile.EndOfStream
                    myNtrPluvRecord = ntrPluvIniFile.ReadLine
                    If InStr(myNtrPluvRecord, "[Branch Types]") > 0 Then BranchTypesFound = True
                End While

                'vul nu de klasse met taktypes
                If BranchTypesFound Then
                    Do
                        myNtrPluvRecord = ntrPluvIniFile.ReadLine
                        If myNtrPluvRecord = "" Then Exit Do
                        'als we een takid tegenkomen, lees nummer en id
                        If InStr(myNtrPluvRecord, " ID=") > 0 Then
                            myNum = Convert.ToInt16(Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord))
                            myParentNum = myNum
                            tmpStr = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord, "=")
                            myID = Trim(myNtrPluvRecord)
                            myType = New clsSobekBranchType With {
                            .ID = myID,
                            .ParentID = .ID, 'we geven hem zichzelf tevens als parent type, dat is later makkelijk
                            .SbkNum = myNum
                                }
                            BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)
                            If myNum > MaxNumStandardType Then MaxNumStandardType = myNum
                        End If
                    Loop
                Else
                    Throw New Exception("Error reading file with branchtypes " & myFile)
                End If
                ntrPluvIniFile.Close()
            Else
                Me.Setup.Log.AddWarning("File does Not found: " & myFile & ". custom branch types can therefore Not be identified And processed.")
                Call PopulateStandardBranchTypes()
            End If

            'nu doen we hetzelfde voor de user defined types in ntrpluv.obj van de case
            myFile = CaseDir & "\ntrpluv.obj"

            If System.IO.File.Exists(myFile) Then
                BranchTypesFound = False
                'zoek het 'hoofdstuk' waar de knooptypes zijn gedefinieerd
                Dim ntrPluvObjFile As New StreamReader(myFile)
                While Not BranchTypesFound And Not ntrPluvObjFile.EndOfStream
                    myNtrPluvRecord = ntrPluvObjFile.ReadLine
                    If InStr(myNtrPluvRecord, "[User Branch Types]") > 0 Then
                        BranchTypesFound = True
                        myNtrPluvRecord = ntrPluvObjFile.ReadLine
                        tmpStr = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord, "=")
                        nCustomBranchTypes = myNtrPluvRecord
                    End If
                End While

                'nu de custom branchtypes
                If BranchTypesFound Then
                    CheckSum = 0
                    While Not ntrPluvObjFile.EndOfStream Or InStr(myNtrPluvRecord, "[") > 0

                        myNtrPluvRecord = ntrPluvObjFile.ReadLine
                        If myNtrPluvRecord.Trim = "" Then Exit While

                        myCustomBranchType = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord)
                        myNum = MaxNumStandardType + myCustomBranchType
                        myToken = Me.Setup.GeneralFunctions.ParseString(myNtrPluvRecord, "=")
                        myValue = myNtrPluvRecord

                        'maak het setje van ID en ParentID compleet
                        If myToken.ToLower = "parentid" Then
                            CheckSum += 1
                            myParentID = myValue.Trim
                        ElseIf myToken.ToLower = "id" Then
                            CheckSum += 1
                            myID = myValue.Trim
                        End If

                        If CheckSum = 2 Then
                            myType = New clsSobekBranchType
                            myType.ID = myID
                            myType.ParentID = myParentID
                            myType.SbkNum = myNum
                            BranchTypes.BranchTypes.Add(myID.Trim.ToUpper, myType)
                            CheckSum = 0
                        End If
                    End While
                Else
                    Throw New Exception("Error reading file with nodetypes " & myFile)
                End If
                ntrPluvObjFile.Close()
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub ReadNodeTypes of class clsSobekCase.")
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Sub

    Public Sub PopulateStandardNodeTypes()
        Dim myType As clsSobekNodeType

        myType = New clsSobekNodeType With {
        .ID = "SBK_CONNECTIONNODE",
        .ParentID = "SBK_CONNECTIONNODE",
        .SbkNum = 1
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CONN&MEAS",
        .ParentID = "SBK_CONN&MEAS",
        .SbkNum = 2
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CONN&RUNOFF",
        .ParentID = "SBK_CONN&RUNOFF",
        .SbkNum = 3
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CONN&LAT",
        .ParentID = "SBK_CONN&LAT",
        .SbkNum = 4
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CONN&LAT&RUNOFF",
        .ParentID = "SBK_CONN&LAT&RUNOFF",
        .SbkNum = 5
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CONN&LAT&RUNOFF_COMB",
        .ParentID = "SBK_CONN&LAT&RUNOFF_COMB",
        .SbkNum = 6
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_EXTWEIR",
        .ParentID = "SBK_EXTWEIR",
        .SbkNum = 7
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_EXTORIFICE",
        .ParentID = "SBK_EXTORIFICE",
        .SbkNum = 8
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_EXTCULVERT",
        .ParentID = "SBK_EXTCULVERT",
        .SbkNum = 9
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_EXTFRICTION",
        .ParentID = "SBK_EXTFRICTION",
        .SbkNum = 10
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_EXTPUMP",
        .ParentID = "SBK_EXTPUMP",
        .SbkNum = 11
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CHANNELCONNECTION",
        .ParentID = "SBK_CHANNELCONNECTION",
        .SbkNum = 12
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CHANNEL_STORCONN&LAT",
        .ParentID = "SBK_CHANNEL_STORCONN&LAT",
        .SbkNum = 13
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_BOUNDARY",
        .ParentID = "SBK_BOUNDARY",
        .SbkNum = 14
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CHANNELLINKAGENODE",
        .ParentID = "SBK_CHANNELLINKAGENODE",
        .SbkNum = 15
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_GRIDPOINT",
        .ParentID = "SBK_GRIDPOINT",
        .SbkNum = 16
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_GRIDPOINTFIXED",
        .ParentID = "SBK_GRIDPOINTFIXED",
        .SbkNum = 17
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_MEASSTAT",
        .ParentID = "SBK_MEASSTAT",
        .SbkNum = 18
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_LATERALFLOW",
        .ParentID = "SBK_LATERALFLOW",
        .SbkNum = 19
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_PROFILE",
        .ParentID = "SBK_PROFILE",
        .SbkNum = 20
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_WEIR",
        .ParentID = "SBK_WEIR",
        .SbkNum = 21
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_UNIWEIR",
        .ParentID = "SBK_UNIWEIR",
        .SbkNum = 22
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_ORIFICE",
        .ParentID = "SBK_ORIFICE",
        .SbkNum = 23
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CULVERT",
        .ParentID = "SBK_CULVERT",
        .SbkNum = 24
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_FRICTION",
        .ParentID = "SBK_FRICTION",
        .SbkNum = 25
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_BRIDGE",
        .ParentID = "SBK_BRIDGE",
        .SbkNum = 26
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_PUMP",
        .ParentID = "SBK_PUMP",
        .SbkNum = 27
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_GENERALSTRUC",
        .ParentID = "SBK_GENERALSTRUC",
        .SbkNum = 28
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_DATABASESTRUCTURE",
        .ParentID = "SBK_DATABASESTRUCTURE",
        .SbkNum = 29
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_RIVERWEIR",
        .ParentID = "SBK_RIVERWEIR",
        .SbkNum = 30
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_RIVERADVANCEDWEIR",
        .ParentID = "SBK_RIVERADVANCEDWEIR",
        .SbkNum = 31
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_RIVERPUMP",
        .ParentID = "SBK_RIVERPUMP",
        .SbkNum = 32
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_CMPSTR",
        .ParentID = "SBK_CMPSTR",
        .SbkNum = 33
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_SBK-3B-REACH",
        .ParentID = "SBK_SBK-3B-REACH",
        .SbkNum = 34
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_SBK-3B-NODE",
        .ParentID = "SBK_SBK-3B-NODE",
        .SbkNum = 35
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_1D2DBOUNDARY",
        .ParentID = "SBK_1D2DBOUNDARY",
        .SbkNum = 36
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "FLS_GRID",
        .ParentID = "FLS_GRID",
        .SbkNum = 37
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "FLS_BOUNDARY",
        .ParentID = "FLS_BOUNDARY",
        .SbkNum = 38
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "FLS_HISTORY",
        .ParentID = "FLS_HISTORY",
        .SbkNum = 39
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "FLS_BREAKINGDAM",
        .ParentID = "FLS_BREAKINGDAM",
        .SbkNum = 40
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "FLS_BOUNDARYCORNER",
        .ParentID = "FLS_BOUNDARYCORNER",
        .SbkNum = 41
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "FLS_INIWLPOINT",
        .ParentID = "FLS_INIWLPOINT",
        .SbkNum = 42
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_PAVED",
        .ParentID = "3B_PAVED",
        .SbkNum = 43
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_UNPAVED",
        .ParentID = "3B_UNPAVED",
        .SbkNum = 44
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_GREENHOUSE",
        .ParentID = "3B_GREENHOUSE",
        .SbkNum = 45
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_OPENWATER",
        .ParentID = "3B_OPENWATER",
        .SbkNum = 46
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_BOUNDARY",
        .ParentID = "3B_BOUNDARY",
        .SbkNum = 47
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_PUMP",
        .ParentID = "3B_PUMP",
        .SbkNum = 48
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_WEIR",
        .ParentID = "3B_WEIR",
        .SbkNum = 49
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_ORIFICE",
        .ParentID = "3B_ORIFICE",
        .SbkNum = 50
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_FRICTION",
        .ParentID = "3B_FRICTION",
        .SbkNum = 51
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_QH-RELATION",
        .ParentID = "3B_QH-RELATION",
        .SbkNum = 52
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_CULVERT",
        .ParentID = "3B_CULVERT",
        .SbkNum = 53
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_SACRAMENTO",
        .ParentID = "3B_SACRAMENTO",
        .SbkNum = 54
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_INDUSTRY",
        .ParentID = "3B_INDUSTRY",
        .SbkNum = 55
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_WWTP",
        .ParentID = "3B_WWTP",
        .SbkNum = 56
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "WLM_SEPTICTANK",
        .ParentID = "WLM_SEPTICTANK",
        .SbkNum = 57
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_CONNECTIONNODE",
        .ParentID = "3B_CONNECTIONNODE",
        .SbkNum = 58
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_DRYWASTELOAD",
        .ParentID = "SBK_DRYWASTELOAD",
        .SbkNum = 59
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_RUNOFF",
        .ParentID = "3B_RUNOFF",
        .SbkNum = 60
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_HBV",
        .ParentID = "3B_HBV",
        .SbkNum = 63
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "3B_SCS",
        .ParentID = "3B_SCS",
        .SbkNum = 64
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekNodeType With {
        .ID = "SBK_EXTRARESISTANCE",
        .ParentID = "SBK_EXTRARESISTANCE",
        .SbkNum = 65
            }
        If Not NodeTypes.NodeTypes.ContainsKey(myType.ID.Trim.ToUpper) Then NodeTypes.NodeTypes.Add(myType.ID.Trim.ToUpper, myType)


    End Sub

    Public Sub PopulateStandardBranchTypes()
        Dim myType As clsSobekBranchType

        myType = New clsSobekBranchType
        myType.ID = "SBK_CHANNEL"
        myType.ParentID = "SBK_CHANNEL"
        myType.SbkNum = 1
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_CHANNEL&LAT"
        myType.ParentID = "SBK_CHANNEL&LAT"
        myType.SbkNum = 2
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_DAMBRK"
        myType.ParentID = "SBK_DAMBRK"
        myType.SbkNum = 3
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_PIPE"
        myType.ParentID = "SBK_PIPE"
        myType.SbkNum = 4
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_PIPE&RUNOFF"
        myType.ParentID = "SBK_PIPE&RUNOFF"
        myType.SbkNum = 5
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_DWAPIPE&RUNOFF"
        myType.ParentID = "SBK_DWAPIPE&RUNOFF"
        myType.SbkNum = 6
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_RWAPIPE&RUNOFF"
        myType.ParentID = "SBK_RWAPIPE&RUNOFF"
        myType.SbkNum = 7
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_PIPE&COMB"
        myType.ParentID = "SBK_PIPE&COMB"
        myType.SbkNum = 8
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_PIPE&MEAS"
        myType.ParentID = "SBK_PIPE&MEAS"
        myType.SbkNum = 9
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_INTWEIR"
        myType.ParentID = "SBK_INTWEIR"
        myType.SbkNum = 10
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_INTORIFICE"
        myType.ParentID = "SBK_INTORIFICE"
        myType.SbkNum = 11
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_INTCULVERT"
        myType.ParentID = "SBK_INTCULVERT"
        myType.SbkNum = 12
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_INTPUMP"
        myType.ParentID = "SBK_INTPUMP"
        myType.SbkNum = 13
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "FLS_LINEBOUNDARY"
        myType.ParentID = "FLS_LINEBOUNDARY"
        myType.SbkNum = 14
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "FLS_LINE1D2DBOUNDARY"
        myType.ParentID = "FLS_LINE1D2DBOUNDARY"
        myType.SbkNum = 15
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "FLS_LINEHISTORY"
        myType.ParentID = "FLS_LINEHISTORY"
        myType.SbkNum = 16
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "3B_LINK"
        myType.ParentID = "3B_LINK"
        myType.SbkNum = 17
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "3B_LINK_RWZI"
        myType.ParentID = "3B_LINK_RWZI"
        myType.SbkNum = 18
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "WLM_LINK"
        myType.ParentID = "WLM_LINK"
        myType.SbkNum = 19
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "3B_ROUTING_LINK"
        myType.ParentID = "3B_ROUTING_LINK"
        myType.SbkNum = 20
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "SBK_STREET"
        myType.ParentID = "SBK_STREET"
        myType.SbkNum = 21
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

        myType = New clsSobekBranchType
        myType.ID = "3B_UNPAVED_SURFACE_FLOW_LINK"
        myType.ParentID = "3B_UNPAVED_SURFACE_FLOW_LINK"
        myType.SbkNum = 22
        If Not BranchTypes.BranchTypes.ContainsKey(myType.ID.Trim.ToUpper) Then BranchTypes.BranchTypes.Add(myType.ID.Trim.ToUpper, myType)

    End Sub

    Friend Sub setCaseDir()
        'deze routine zoekt uit wat de casedirectory van de opgegeven sobekcase is
        Dim myRecord As String, cNum As Integer, cName As String
        Dim CaseList As New StreamReader(Me.SobekProject.ProjectDir & "\caselist.cmt")
        While Not CaseList.EndOfStream
            myRecord = CaseList.ReadLine
            cNum = Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1)
            cName = Me.Setup.GeneralFunctions.RemoveSurroundingQuotes(myRecord, True, False)
            If cName.ToLower.Trim = CaseName.ToLower.Trim Then
                CaseDir = Me.SobekProject.ProjectDir & "\" & cNum
                Exit Sub
            End If
        End While

    End Sub


    Public Sub exportLateralDat(ByVal Path As String, ByVal Append As Boolean)
        Using DatWriter As New StreamWriter(Path, Append)
            For Each myDat As clsLateralDatFLBRRecord In CFData.Data.LateralData.LateralDatFLBRRecords.records.Values
                If myDat.InUse Then
                    myDat.Write(DatWriter)
                End If
            Next
        End Using
    End Sub

    Public Sub SetLateralsInuseFalse()
        'Author: Siebe Bosch
        'Date: 20-8-2013
        'Description: Sets the variable InUse to False for all Lateral Reach Objects

        'all reach objects of the type lateral
        For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral Then myObj.InUse = False
            Next
        Next

        'all lateral.dat flbr records
        For Each myDat As clsLateralDatFLBRRecord In CFData.Data.LateralData.LateralDatFLBRRecords.records.Values
            myDat.InUse = False
        Next

    End Sub

    Public Sub BuildOpenWaterLaterals(ByVal Precip As Boolean, ByVal Evap As Boolean)

        Try

            Dim PrecLat As clsSbkReachObject
            Dim PrecDat As New clsLateralDatFLBRRecord(Me.Setup, Me)
            Dim EvapLat As clsSbkReachObject
            Dim EvapDat As New clsLateralDatFLBRRecord(Me.Setup, Me)
            Dim PrecCN As clsNetworkCNFLBRRecord
            Dim EvapCN As clsNetworkCNFLBRRecord

            Dim i As Long, n As Long = CFTopo.Reaches.Reaches.Values.Count
            Dim myPrefix As String, InUse As Boolean

            If Not Me.Setup.GISData.SubcatchmentDataSource.Open() Then Throw New Exception("Error reading area shapefile.")

            Me.Setup.GeneralFunctions.UpdateProgressBar("Building openwater laterals...", 0, 10)
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)

                InUse = True
                For Each myPrefix In Me.Setup.Settings.CFSettings.SkipReachPrefixes
                    If Left(myReach.Id.Trim.ToUpper, myPrefix.Trim.Length) = myPrefix.Trim.ToUpper Then InUse = False
                Next

                If InUse Then

                    'add precipitation as lateral.dat record
                    If Precip Then
                        PrecDat = New clsLateralDatFLBRRecord(Me.Setup, Me)
                        PrecDat.ID = Me.Setup.Settings.CFSettings.PrecipLateralPrefix & myReach.Id
                        PrecDat.sc = 0
                        PrecDat.lt = 0
                        PrecDat.dclt1 = 7
                        PrecDat.ir = 1
                        PrecDat.ii = 0
                        PrecDat.ar = myReach.calcMaxOpenWaterArea
                    End If

                    If Evap Then
                        'add evapiration as lateral.dat record
                        EvapDat = New clsLateralDatFLBRRecord(Me.Setup, Me)
                        EvapDat.ID = Me.Setup.Settings.CFSettings.EvapLateralPrefix & myReach.Id
                        EvapDat.sc = 0
                        EvapDat.lt = 0
                        EvapDat.dclt1 = 7
                        EvapDat.ir = 1
                        EvapDat.ms = "EVPow"
                        EvapDat.ii = 0
                        EvapDat.ar = myReach.calcMaxOpenWaterArea
                    End If


                    If myReach.calcMaxOpenWaterArea > 0 Then

                        If Precip Then
                            '------------------------------------------------------------------------------------------
                            'implement the precipitation lateral as a reach object
                            PrecDat.InUse = True
                            CFData.Data.LateralData.LateralDatFLBRRecords.records.Add(PrecDat.ID.Trim.ToUpper, PrecDat)

                            'add as reach object
                            PrecLat = New clsSbkReachObject(Me.Setup, Me)
                            PrecLat.ID = PrecDat.ID
                            PrecLat.ci = myReach.Id
                            PrecLat.lc = myReach.getReachLength / 3
                            PrecLat.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
                            PrecLat.InUse = True

                            myReach.OptimizeReachObjectLocation(PrecLat, 2)
                            myReach.ReachObjects.ReachObjects.Add(PrecLat.ID.Trim.ToUpper, PrecLat)

                            Dim myPoint As New MapWinGIS.Point
                            PrecLat.calcXY()
                            myPoint.x = PrecLat.X
                            myPoint.y = PrecLat.Y
                            PrecDat.ms = Setup.GISData.SubcatchmentDataSource.getTextValueByPoint(enmInternalVariable.MeteoStationID, myPoint)

                            'add as network.cn FLBR record
                            PrecCN = New clsNetworkCNFLBRRecord(Me.Setup)
                            PrecCN.ID = Me.Setup.Settings.CFSettings.PrecipLateralPrefix & myReach.Id
                            PrecCN.ci = myReach.Id
                            PrecCN.lc = PrecLat.lc
                            myReach.ReachObjects.AddNetworkCNFLBRRecord(PrecCN)
                            '------------------------------------------------------------------------------------------
                        End If

                        If Evap Then
                            '------------------------------------------------------------------------------------------
                            'implement the evaporation lateral as a reach object
                            EvapDat.InUse = True
                            CFData.Data.LateralData.LateralDatFLBRRecords.records.Add(EvapDat.ID.Trim.ToUpper, EvapDat)

                            'add as reach object
                            EvapLat = New clsSbkReachObject(Me.Setup, Me)
                            EvapLat.ID = EvapDat.ID
                            EvapLat.ci = myReach.Id
                            EvapLat.lc = myReach.getReachLength * 2 / 3
                            EvapLat.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
                            EvapLat.InUse = True

                            myReach.OptimizeReachObjectLocation(EvapLat, 2)
                            myReach.ReachObjects.ReachObjects.Add(EvapLat.ID.Trim.ToUpper, EvapLat)

                            'add as network.cn FLBR record
                            EvapCN = New clsNetworkCNFLBRRecord(Me.Setup)
                            EvapCN.ID = Me.Setup.Settings.CFSettings.EvapLateralPrefix & myReach.Id
                            EvapCN.ci = myReach.Id
                            EvapCN.lc = EvapLat.lc
                            myReach.ReachObjects.AddNetworkCNFLBRRecord(EvapCN)
                            '------------------------------------------------------------------------------------------
                        End If

                    End If
                End If
            Next

            Me.Setup.GISData.CatchmentDataSource.Close()

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)

        End Try

    End Sub

    Public Function Hydrology2Laterals(ByVal OutReachPrefixes As List(Of String), ByVal InReachPrefixes As List(Of String)) As Boolean
        Dim myPoint As New clsXYZ, i As Long, n As Long

        Try
            i = 0
            n = CFTopo.Reaches.Reaches.Count

            'first walk through all reaches. If a reach has a prefix that indicates that it's a storage reach
            'with a downstream connection to the main channels
            'in that case, we'll build a lateral node for the storage construction and snap it to the main reach

            Me.Setup.GeneralFunctions.UpdateProgressBar("Converting hydrological model components to lateral nodes.", 0, 10)
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values

                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)

                For Each Prefix As String In OutReachPrefixes
                    If Left(myReach.Id.Trim.ToUpper, Prefix.Trim.Length) = Prefix.Trim.ToUpper Then
                        LateralFromReachNode(myReach.en, myReach.Id)
                        Exit For
                    End If
                Next

                For Each Prefix As String In InReachPrefixes
                    If Left(myReach.Id.Trim.ToUpper, Prefix.Trim.Length) = Prefix.Trim.ToUpper Then
                        LateralFromReachNode(myReach.bn, myReach.Id)
                        Exit For
                    End If
                Next
            Next

            'Next we'll look for RR-on-flow-connections on the main reaches. In case we find one, we'll convert it's node type
            'to Lateral and build a record for it
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then

                            LateralFromReachObject(myObj, myReach)

                        End If
                    Next
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function Hydrology2Laterals")
            Return False
        End Try

    End Function

    Public Sub LateralFromReachObject(ByRef myObj As clsSbkReachObject, ByRef myReach As clsSbkReach)

        Dim myRecord As clsNetworkCNFLBRRecord
        Dim myDatRecord As clsLateralDatFLBRRecord
        Dim myPoint As New clsXYZ

        'change the node type to Lateral
        myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral

        'add a NetworkCNFLBRRecord to this reachobject
        myRecord = New clsNetworkCNFLBRRecord(Me.Setup)
        myRecord.ID = myObj.ID
        myRecord.ci = myReach.Id
        myRecord.lc = myObj.lc
        myReach.ReachObjects.AddNetworkCNFLBRRecord(myRecord)

        'now add a lateral.dat record and give it one datetime/value record
        myDatRecord = New clsLateralDatFLBRRecord(Me.Setup, Me)
        myDatRecord.ID = myRecord.ID
        myDatRecord.sc = 0
        myDatRecord.lt = 0
        myDatRecord.dclt1 = 1
        myDatRecord.dclt2 = 0
        myDatRecord.dclt3 = 0
        myDatRecord.pdin1 = 0
        myDatRecord.pdin2 = 0
        myDatRecord.TimeTable.AddDatevalPair(New Date(2000, 1, 1, 0, 0, 0), 0)
        If Not CFData.Data.LateralData.LateralDatFLBRRecords.records.ContainsKey(myDatRecord.ID.Trim.ToUpper) Then
            CFData.Data.LateralData.LateralDatFLBRRecords.records.Add(myDatRecord.ID.Trim.ToUpper, myDatRecord)
        Else
            Me.Setup.Log.AddWarning("Collection of lateral.dat FLBR records already contained a record with id " & myDatRecord.ID)
        End If
    End Sub

    Public Sub LateralFromReachNode(ByRef myNode As clsSbkReachNode, ByVal ID As String)

        Dim mySnapLocation As clsSbkReachObject
        Dim myRecord As clsNetworkCNFLBRRecord
        Dim myDatRecord As clsLateralDatFLBRRecord
        Dim myPoint As New clsXYZ
        Dim snapReach As clsSbkReach

        'the downstream node becomes a lateral node!
        myPoint.X = myNode.X
        myPoint.Y = myNode.Y

        'find a suitable snap location for this lateral node
        mySnapLocation = Me.CFTopo.FindNearestVectorPoint(myPoint, False, 2)

        'create a new lateral node and find a decent snap location for it
        myRecord = New clsNetworkCNFLBRRecord(Me.Setup)
        myRecord.ID = ID
        myRecord.ci = mySnapLocation.ci
        myRecord.lc = mySnapLocation.lc
        snapReach = CFTopo.Reaches.GetReach(myRecord.ci)
        snapReach.ReachObjects.AddNetworkCNFLBRRecord(myRecord)

        'now add a lateral.dat record and give it one datetime/value record
        myDatRecord = New clsLateralDatFLBRRecord(Me.Setup, Me)
        myDatRecord.ID = myRecord.ID
        myDatRecord.sc = 0
        myDatRecord.lt = 0
        myDatRecord.dclt1 = 1
        myDatRecord.dclt2 = 0
        myDatRecord.dclt3 = 0
        myDatRecord.pdin1 = 0
        myDatRecord.pdin2 = 0
        myDatRecord.TimeTable.AddDatevalPair(New Date(2000, 1, 1, 0, 0, 0), 0)
        If Not CFData.Data.LateralData.LateralDatFLBRRecords.records.ContainsKey(myDatRecord.ID.Trim.ToUpper) Then
            CFData.Data.LateralData.LateralDatFLBRRecords.records.Add(myDatRecord.ID.Trim.ToUpper, myDatRecord)
        Else
            Me.Setup.Log.AddWarning("Collection of lateral.dat FLBR records already contained a record with id " & myDatRecord.ID)
        End If

    End Sub

    Public Function RRCF2RRBoundary() As Boolean
        Dim myBnd As New clsRRNodeTPRecord(Me.Setup, Me)
        Try
            For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values

                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then

                            'find the corresponding node in the RR topology
                            myBnd = Me.RRTopo.GetNode(myObj.ID)

                            If Not myBnd Is Nothing Then
                                myBnd.nt.ParentID = "3B_BOUNDARY"
                            Else
                                'create a new RR-boundary based on this node
                                myBnd.nt.ParentID = "3B_BOUNDARY"
                                myBnd.ID = myObj.ID
                                myBnd.X = myObj.X
                                myBnd.Y = myObj.Y
                                Me.RRTopo.Nodes.Add(myBnd.ID.Trim.ToUpper, myBnd)
                            End If

                        End If
                    Next
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function RRCF2RRBoundary.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SetReachEndNodeToBoundary(ByVal Prefixes As List(Of String)) As Boolean
        Dim Prefix As String
        For Each myReach As clsSbkReach In CFTopo.Reaches.Reaches.Values
            If myReach.InUse Then
                For Each Prefix In Prefixes
                    If Left(myReach.Id.Trim.ToUpper, Prefix.Trim.Length) = Prefix.Trim.ToUpper Then
                        myReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary
                        Exit For
                    End If
                Next
            End If
        Next
        Return True
    End Function

    Public Function Read(ByVal ReadRRNetwork As Boolean, ByVal ReadCFNetwork As Boolean, ByVal ReadRRData As Boolean, ByVal ReadCFData As Boolean, ByVal ReadWQNetwork As Boolean, ByVal StructureReachPrefix As String, Optional ByVal ProgressBarNumber As Integer = 1) As Boolean

        Try
            Dim i As Short = 0

            InUse = True              'omdat deze case daadwerkelijk wordt ingelezen, zetten we InUse op TRUE

            'first read all node types and their numbers. Do the same for all link types and their numbers
            ReadNodeTypes()
            ReadBranchTypes()

            If ReadRRNetwork Then
                i += 1
                RRTopo = New clsRRTopology(Me.Setup, Me) 'v2.000: preventing from hitting existing keys when rereading
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading topology RR module...", i, 20, True, ProgressBarNumber)
                RRTopo.Import(CaseDir, Me)
                RRTopoRead = True
            End If

            i = 0
            If ReadRRData Then
                i += 1
                RRData = New clsRRData(Me.Setup, Me) 'v2.000: preventing from hitting existing keys when rereading
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading parameters unpaved...", i, 20, True, ProgressBarNumber)
                If Not RRData.Unpaved3B.Read() Then Me.Setup.Log.AddError("Error reading Unpaved.3b file.")
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading drainage definitions...", i, 20, True, ProgressBarNumber)
                RRData.UnpavedAlf.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading seepage definitions...", i, 20, True, ProgressBarNumber)
                RRData.UnpavedSep.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading infiltration definitions...", i, 20, True, ProgressBarNumber)
                RRData.UnpavedInf.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading storage definitions...", i, 20, True, ProgressBarNumber)
                RRData.UnpavedSto.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading table definitions...", i, 20, True, ProgressBarNumber)
                RRData.UnpavedTbl.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading sacramento definitions...", i, 20, True, ProgressBarNumber)
                RRData.Sacr3BSACR.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading sacramento capacities...", i, 20, True, ProgressBarNumber)
                RRData.Sacr3BCAPS.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading sacramento parameters...", i, 20, True, ProgressBarNumber)
                RRData.Sacr3BOPAR.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading sacramento unit hydrographs...", i, 20, True, ProgressBarNumber)
                RRData.Sacr3BUNIH.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading parameters paved...", i, 20, True, ProgressBarNumber)
                RRData.Paved3B.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading storage defs paved...", i, 20, True, ProgressBarNumber)
                RRData.PavedSTO.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading dwf defs paved...", i, 20, True, ProgressBarNumber)
                RRData.PavedDWA.Read(CaseDir, Me)
                RRDataRead = True
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading dwf defs paved...", i, 20, True, ProgressBarNumber)
                RRData.Greenhse3B.Read(CaseDir, Me)
                RRDataRead = True
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading dwf defs paved...", i, 20, True, ProgressBarNumber)
                RRData.GreenhseRF.Read(CaseDir, Me)
                RRDataRead = True
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading RR boundary records...", i, 20, True, ProgressBarNumber)
                RRData.Bound3B3B.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading RR boundary tables...", i, 20, True, ProgressBarNumber)
                RRData.BOund3BTBL.Read(CaseDir, Me)
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading RR WWTP data...", i, 20, True, ProgressBarNumber)
                RRData.WWTP3B.Read(CaseDir, Me)
            End If

            i = 0
            If ReadCFNetwork Then
                i += 1
                CFTopo = New clsCFTopology(Me.Setup, Me) 'v2.000: preventing from hitting existing keys when rereading
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading network Flow module", i, 20, True, ProgressBarNumber)
                CFTopo.Import(CaseDir, StructureReachPrefix)

                '//siebe: under construction
                'CFTopo.ImportByNetworkNTW(CaseDir)

                CFTopoRead = True
            End If

            If ReadCFData Then
                i += 1
                CFData = New clsCFData(Me.Setup, Me) 'v2.000: preventing from hitting existing keys when rereading
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading data Flow module", i, 20, True, ProgressBarNumber)
                CFData.Data.ImportAll(CaseDir, Me)
                CFDataRead = True
            End If

            'If ReadSFData Then
            '    i += 1
            '    Me.Setup.GeneralFunctions.UpdateProgressBar("Reading data Urban module", i, 20, True, ProgressBarNumber)
            '    SFData.Data.ImportAll(CaseDir, Me)
            '    SFDataRead = True
            'End If

            i += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("Import complete", i, 20, True, ProgressBarNumber)
            Return True

        Catch ex As Exception
            Dim log As String = "Error reading sobek case"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Return False
        End Try

    End Function

    Public Function ReadProfileData() As Boolean
        Try
            CFData.Data.ImportCrossSectionData(CaseDir, Me)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadProfileData of class clsSobekCase.")
            Me.Setup.Log.AddError(ex.Message)
        End Try
    End Function

    Friend Function WriteLateralDataFromRRInflow(ExportDir As String) As Boolean

        'lees QLAT.HIS 
        'Call CFData.Results.QLat.ReadAll()
        Setup.Log.AddError("De functie QLat.ReadAll in WriteLateralDataFromRRInflow is in onbruik geraakt.")

        'doorloop alle RRCF Connection Nodes
        Dim myReachNode As clsSbkReachNode
        Dim myReachObj As clsSbkReachObject
        Dim myReach As clsSbkReach
        Dim i As Integer

        i = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Building laterals from RR on CF Connections...", 0, 10)
        Using latWriter = New StreamWriter(ExportDir & "\lateral.dat")

            For Each myReach In CFTopo.Reaches.Reaches.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, CFTopo.Reaches.Reaches.Count)
                For Each myReachObj In myReach.ReachObjects.ReachObjects.Values
                    If myReachObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                        myReachObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral

                        Dim myLatFLBR As New clsLateralDatFLBRRecord(Me.Setup, Me)
                        myLatFLBR.sc = 0
                        myLatFLBR.lt = 0
                        myLatFLBR.dclt1 = 1
                        myLatFLBR.dclt2 = 0
                        myLatFLBR.pdin1 = 0
                        myLatFLBR.pdin2 = 0

                        Me.Setup.Log.AddError("De oude functie op SOBEK-resultaten uit te lezen is in onbruik geraakt. Review de functie WriteLateralDataFromRRInflow")
                        'vul nu de tabel met de resultaten uit de HIS-file
                        'If Not CFData.Results.QLat.toTimeTable(myReachObj.ID, "Disch", myLatFLBR.TimeTable) Then
                        '  Me.Setup.Log.AddError("Error creating timetable for SOBEK node " & myReachObj.ID)
                        'Else
                        '  myLatFLBR.Write(latWriter)
                        'End If
                    End If
                Next
            Next

            i = 0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building laterals from RR on CF Connection Nodes...", 0, 10)
            For Each myReachNode In CFTopo.ReachNodes.ReachNodes.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, CFTopo.Reaches.Reaches.Count)
                If myReachNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode Then

                    'transformeer het knooptype intern tot een Connection Node with Lateral Discharge and Storage
                    myReachNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor
                    Dim myLatFLNO As New clsLateralDatFLNORecord(Me.Setup, Me)
                    myLatFLNO.sc = 0
                    myLatFLNO.lt = 0
                    myLatFLNO.dclt1 = 1
                    myLatFLNO.dclt2 = 0
                    myLatFLNO.dclt3 = 0
                    myLatFLNO.pdin1 = 0
                    myLatFLNO.pdin2 = 0
                    myLatFLNO.ID = myReachNode.ID

                    Me.Setup.Log.AddError("De oude functie op SOBEK-resultaten uit te lezen is in onbruik geraakt. Review de functie WriteLateralDataFromRRInflow")
                    'vul nu de tabel met de resultaten uit de HIS-file
                    'If Not CFData.Results.QLat.toTimeTable(myReachNode.ID, "Disch", myLatFLNO.dcltTimeTable) Then
                    '    Me.Setup.Log.AddError("Error creating timetable for SOBEK node " & myReachNode.ID)
                    'Else
                    '    myLatFLNO.dcltTimeTable.buildDateValStrings(1, 5)
                    '    myLatFLNO.Write(latWriter)
                    'End If
                End If
            Next

        End Using

    End Function





End Class


