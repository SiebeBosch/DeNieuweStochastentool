Option Explicit On
Imports STOCHLIB.General
Imports MapWinGIS
Imports Microsoft.Research.Science.Data.Imperative
Imports sds = Microsoft.Research.Science.Data

Public Class clsClassMapFile
    Implements MapWinGIS.ICallback
    Private Setup As clsSetup

    Dim Extent As clsSpatialExtent
    Dim Utils As New MapWinGIS.Utils

    Dim Path As String

    Dim SimulationT0 As Date = Nothing      'the start of the simulation as datetime
    Dim DambreakT0Seconds As Long = 0               'T0 of the first dambreak, expressed in seconds since the SimulationT0. Default 0

    Friend DataSet As sds.DataSet
    Friend DataSets As sds.DataSet()
    Friend Variables As Microsoft.Research.Science.Data.ReadOnlyVariableCollection
    Friend Dimensions As sds.ReadOnlyDimensionList


    'NOTE: it looks like the classnumbers mentioned in waterDepths are not 0-based but 1-based
    Friend TimeStamps As Double()       'timestamp in seconds w.r.t. the start of the simulation
    Friend waterDepths As SByte(,)
    Friend velocities As SByte(,)      'cell center velocities
    Friend DepthClassBounds As Double(,)
    Friend DepthClassValues As Double()         'for each class a depth value that represents this class
    Friend VelocityClassBounds As Double(,)
    Friend VelocityClassValues As Double()     'for each class a velocity value that represents this class

    'retrieve the mesh nodes that each face is made up of
    Friend FaceNodes As Integer(,)
    Friend FaceNodesX As Double()
    Friend FaceNodesY As Double()
    Friend MeshFacesX As Double()   'centerpoints for all faces
    Friend MeshFacesY As Double()   'centerpoints for all faces


    Public Sub New(ByRef mySetup As clsSetup, myPath As String, mySimulationT0 As Date)
        Setup = mySetup
        Path = myPath
        SimulationT0 = mySimulationT0
        Extent = New clsSpatialExtent(Me.Setup)
        Utils.GlobalCallback = Me
    End Sub

    Public Function getExtent() As clsSpatialExtent
        Return Extent
    End Function

    Public Function getnTimesteps() As Integer
        Return TimeStamps.Count
    End Function

    Public Function getTimestamps() As Double()
        Return TimeStamps
    End Function

    Public Function getStartDate() As Date
        Return SimulationT0
    End Function

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.Setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, False)
    End Sub


    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Select Case ErrorMsg
            Case Is = "Table: Index Out of Bounds"
                'door negatieve veldindex in shapefile. Dit gebruiken we actief als feature, dus niet als foutmelding afhandelen.
            Case Else
                Me.Setup.Log.AddError("Error returned from MapWinGIS Callback function: " & ErrorMsg)
                Me.Setup.Log.AddError(ErrorMsg)
        End Select
    End Sub

    Public Function ReadTimestamps() As Boolean
        Try
            'it looks like the index number of given datasets differs between versions
            Dim TimeStampsIdx As Integer

            'reads a _clm.nc file
            'DataSet = sds.DataSet.Open(Path & "?openMode=readOnly")
            DataSet = sds.DataSet.Open(Path)
            DataSets = DataSet.GetLinkedDataSets

            Variables = DataSet.Variables
            Dimensions = DataSet.Dimensions

            'assign the variables
            For i = 0 To Variables.Count - 1
                If Variables(i).Name = "time" Then
                    TimeStampsIdx = Variables(i).ID
                    Exit For
                End If
            Next

            'NOTE: it looks like the classnumbers mentioned in waterDepths are not 0-based but 1-based
            TimeStamps = DataSet.GetData(Of Double())(TimeStampsIdx)       'timestamp in seconds w.r.t. T0 of the simulation

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadTimestamps of class clsClassMapFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function Read() As Boolean
        Try
            'it looks like the index number of given datasets differs between versions
            Dim TimeStampsIdx As Integer
            Dim WaterDepthsIdx As Integer
            Dim VelocitiesIdx As Integer
            Dim DepthClassBoundsIdx As Integer
            Dim VelocityClassBoundsIdx As Integer
            Dim FaceNodesIdx As Integer
            Dim FaceNodesXIdx As Integer
            Dim FaceNodesYIdx As Integer
            Dim FacesXCoordIdx As Integer
            Dim FacesYCoordIdy As Integer

            'v2.4.5
            If Not System.IO.File.Exists(Path) Then
                Throw New Exception("file does not exist: " & Path)
            End If

            'reads a _clm.nc file
            DataSet = sds.DataSet.Open(Path)
            'DataSet = sds.DataSet.Open(Path & "?openMode=readOnly")
            DataSets = DataSet.GetLinkedDataSets

            Variables = DataSet.Variables
            Dimensions = DataSet.Dimensions

            'assign the variables
            For i = 0 To Variables.Count - 1
                If Variables(i).Name = "Mesh2d_waterdepth" Then WaterDepthsIdx = Variables(i).ID
                If Variables(i).Name = "Mesh2d_ucmag" Then VelocitiesIdx = Variables(i).ID
                If Variables(i).Name = "time" Then TimeStampsIdx = Variables(i).ID
                If Variables(i).Name = "class_bounds_hs" Then DepthClassBoundsIdx = Variables(i).ID
                If Variables(i).Name = "class_bounds_ucmag" Then VelocityClassBoundsIdx = Variables(i).ID
                If Variables(i).Name = "Mesh2d_face_nodes" Then FaceNodesIdx = Variables(i).ID
                If Variables(i).Name = "Mesh2d_node_x" Then FaceNodesXIdx = Variables(i).ID
                If Variables(i).Name = "Mesh2d_node_y" Then FaceNodesYIdx = Variables(i).ID
                If Variables(i).Name = "Mesh2d_face_x" Then FacesXCoordIdx = Variables(i).ID
                If Variables(i).Name = "Mesh2d_face_y" Then FacesYCoordIdy = Variables(i).ID
            Next

            'NOTE: it looks like the classnumbers mentioned in waterDepths are not 0-based but 1-based
            TimeStamps = DataSet.GetData(Of Double())(TimeStampsIdx)       'timestamp in seconds w.r.t. T0 of the simulation
            'Dim datatype As String = DataSet.Variables(WaterDepthsIdx).TypeOfData.Name.ToString().ToLower()

            waterDepths = DataSet.GetData(Of SByte(,))(WaterDepthsIdx)
            velocities = DataSet.GetData(Of SByte(,))(VelocitiesIdx)
            DepthClassBounds = DataSet.GetData(Of Double(,))(DepthClassBoundsIdx)
            VelocityClassBounds = DataSet.GetData(Of Double(,))(VelocityClassBoundsIdx)

            'retrieve the mesh nodes that each face is made up of
            FaceNodes = DataSet.GetData(Of Integer(,))(FaceNodesIdx)
            FaceNodesX = DataSet.GetData(Of Double())(FaceNodesXIdx)
            FaceNodesY = DataSet.GetData(Of Double())(FaceNodesYIdx)
            MeshFacesX = DataSet.GetData(Of Double())(FacesXCoordIdx)
            MeshFacesY = DataSet.GetData(Of Double())(FacesYCoordIdy)

            'set our mesh's extent
            Dim MinX As Double = FaceNodesX.Min
            Dim MaxX As Double = FaceNodesX.Max
            Dim MinY As Double = FaceNodesY.Min
            Dim MaxY As Double = FaceNodesY.Max
            Extent = New clsSpatialExtent(Me.Setup, MinX, MinY, MaxX, MaxY)

            'determine a representative value for each depth class
            ReDim DepthClassValues(0 To UBound(DepthClassBounds, 1))
            DepthClassValues(0) = DepthClassBounds(0, 0)
            For i = 1 To UBound(DepthClassBounds, 1) - 1
                DepthClassValues(i) = (DepthClassBounds(i, 0) + DepthClassBounds(i, 1)) / 2
            Next
            DepthClassValues(DepthClassValues.Length - 1) = DepthClassBounds(UBound(DepthClassBounds, 1), 0)

            'determine a representative value for each velocity class
            ReDim VelocityClassValues(0 To UBound(VelocityClassBounds, 1))
            VelocityClassValues(0) = VelocityClassBounds(0, 0)
            For i = 1 To UBound(VelocityClassBounds, 1) - 1
                VelocityClassValues(i) = (VelocityClassBounds(i, 0) + VelocityClassBounds(i, 1)) / 2
            Next
            VelocityClassValues(VelocityClassValues.Length - 1) = VelocityClassBounds(UBound(VelocityClassBounds, 1), 0)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Read of class clsClassMapFile: " & ex.Message)
            Return False
        End Try
    End Function



    Public Function GetActiveCellIndicesAsList() As List(Of String)
        'this function returns a list of cell index numbers (cast to a string) for all cells that become active (depth > 0) during any of the timesteps
        Dim IndexList As New List(Of String)
        Dim n As Integer = UBound(FaceNodes, 1)
        Try
            'loop through all cells
            For i = 0 To UBound(FaceNodes, 1)
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                For k = 0 To TimeStamps.Count - 1
                    If waterDepths(k, i) > 1 Then 'only classes > 1 are not dry
                        IndexList.Add(i.ToString)
                        Exit For
                    End If
                Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return IndexList
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetActiveCellIndices of class clsClassMapFile: " & ex.Message)
            Return IndexList
        End Try
    End Function

    Public Function GetActiveCellIndices() As Dictionary(Of String, Boolean)
        'this function returns a list of cell index numbers (cast to a string) for all cells that become active (depth > 0) during any of the timesteps
        'notice: the boolean in this dictionary does not have any meaning. Reason is that keeping track of true/false required too much resources 
        'however switching to a list of strings was not fast enough either when checking is a cell was present

        'let op: de boolean in deze dictionary heeft geen betekenis. Iedere actieve cel wordt gewoon toegevoegd aan de dictionary
        'De keuze voor een dictionary is gebaseerd op het feit dat een List(of String) te traag was bij het opzoeken of een betreffende cel daar al deel van uitmaakte

        Dim IndexList As New Dictionary(Of String, Boolean)
        Dim n As Integer = UBound(FaceNodes, 1)
        Try
            'loop through all cells
            For i = 0 To UBound(FaceNodes, 1)
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                'IndexList.Add(i.ToString, False)
                For k = 0 To TimeStamps.Count - 1
                    If waterDepths(k, i) > 1 Then 'only classes > 1 are not dry
                        IndexList.Add(i.ToString, True)
                        'IndexList.Item(i.ToString) = True
                        Exit For
                    End If
                Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return IndexList
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetActiveCellIndices of class clsClassMapFile: " & ex.Message)
            Return IndexList
        End Try
    End Function

    Public Function WriteMeshToGeoJSON(ByRef jsWriter As System.IO.StreamWriter, ScenarioName As String, ByRef ActiveCells As Dictionary(Of String, Boolean)) As Boolean
        'this function generates a JSON object representing our current 2D Mesh
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing mesh to GeoJSON...", 0, 10, True)
            Dim lat As Double, lon As Double
            Dim T0 As Double = 0

            jsWriter.WriteLine("   {")
            jsWriter.WriteLine("        ""type"": ""FeatureCollection"",")
            jsWriter.WriteLine("        ""name"": """ & ScenarioName & """,")
            jsWriter.WriteLine("        ""timesteps"": " & TimeStamps.Count & ",")
            jsWriter.WriteLine("		""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:OGC:1.3:CRS84""} },")

            'retrieve the extent and convert it to web mercator
            Dim WebExtent As New clsSpatialExtent(Me.Setup)
            WebExtent = Extent.RDToWebMercator()
            If WebExtent IsNot Nothing Then
                jsWriter.WriteLine("		""extent"": {")
                jsWriter.WriteLine("		    ""minLat"":" & WebExtent.YMin & ",")
                jsWriter.WriteLine("		    ""minLng"":" & WebExtent.XMin & ",")
                jsWriter.WriteLine("		    ""maxLat"":" & WebExtent.YMax & ",")
                jsWriter.WriteLine("		    ""maxLng"":" & WebExtent.XMax & ",")
                jsWriter.WriteLine("		},")
            End If

            Dim idx As Integer = -1
            jsWriter.WriteLine("        ""features"": [")                      'open the array containing the features

            'walk through each cell and write it as a feature
            Dim lastidx As Integer = UBound(FaceNodes, 1)
            For i = 0 To lastidx
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, lastidx, True)

                'only if our cell is active will we write its topology
                If ActiveCells.ContainsKey(i.ToString) Then
                    idx += 1

                    'we have found a cell that contains water depths > 0. Write it to our JSON!
                    jsWriter.Write("            { ""type"": ""Feature"", ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [[")

                    'write our face's geometry. Start with the first coordinate
                    Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, 0) - 1), FaceNodesY(FaceNodes(i, 0) - 1), lat, lon)
                    jsWriter.Write("[" & lon & "," & lat & "]")

                    'now write the remaining coordinates, if in use
                    For j = 1 To UBound(FaceNodes, 2)
                        'unused facenodes are indicted by -999 (e.g. triangular faces where ubound(facenodes,2) = 3)
                        'NOTE: the index numbers inside FaceNodes are 1-based!
                        If FaceNodes(i, j) > -999 Then
                            Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, j) - 1), FaceNodesY(FaceNodes(i, j) - 1), lat, lon)
                            jsWriter.Write(", [" & lon & "," & lat & "]")
                        End If
                    Next

                    '20220324: add the starting point once more to close the loop. This is required for the mapbox vector layer library
                    Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, 0) - 1), FaceNodesY(FaceNodes(i, 0) - 1), lat, lon)
                    jsWriter.Write(",[" & lon & "," & lat & "]")

                    'also write the cell index number + its centerpoint
                    Me.Setup.GeneralFunctions.RD2WGS84(MeshFacesX(i), MeshFacesY(i), lat, lon)
                    jsWriter.Write("]]}, ""properties"": { ""i"": " & i & "," & """idx"": " & idx & ",""lat"": " & lat & ", ""lon"": " & lon & "}}")
                    If i < UBound(FaceNodes, 1) Then jsWriter.Write(",")                     'more features to be added
                    jsWriter.Write(vbCrLf)

                End If
            Next

            jsWriter.WriteLine("            ]")
            jsWriter.WriteLine("        }")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteWebviewerMeshJSON of class clsClassMapFile: " & ex.Message)
            Return False
        End Try

    End Function



    Public Function WriteMaxDepthByExistingShapefileToDictionary(ByRef ActiveCells As Dictionary(Of String, Boolean), WaterDepthUnits As GeneralFunctions.enmElevationUnits, NaNs As Dictionary(Of Integer, Double)) As Dictionary(Of Integer, Double)
        Try
            Dim CellId As String, CellIdx As Integer, RecordIdx As Integer = 0
            Dim maxVal As Double = 0, curVal As Double
            Dim ResultsTable As Dictionary(Of Integer, Double) = NaNs   'initialize results by referring to our dictionary of NaN-values

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing maximum depth to memory...", 0, 10, True)
            For Each CellId In ActiveCells.Keys
                CellIdx = Convert.ToInt32(CellId)
                maxVal = 0
                Me.Setup.GeneralFunctions.UpdateProgressBar("", CellIdx, ActiveCells.Count)

                'we found an active cell! write its result to the shapefile, assuming that the key matches the shape's index number
                For timestepidx = 0 To UBound(TimeStamps)
                    curVal = DepthClassValues(waterDepths(timestepidx, CellIdx) - 1)
                    If curVal > maxVal Then maxVal = curVal
                Next

                Select Case WaterDepthUnits
                    Case Is = GeneralFunctions.enmElevationUnits.M
                        ResultsTable.Item(RecordIdx) = maxVal
                    Case Is = GeneralFunctions.enmElevationUnits.CM
                        ResultsTable.Item(RecordIdx) = maxVal * 100
                    Case Else
                        Throw New Exception("Waterdepth units not supported: " & WaterDepthUnits.ToString)
                End Select
                RecordIdx += 1
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Maximum depth stored in memory...", 0, 10, True)

            Return ResultsTable
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteMaxDepthByExistingShapefileToDictionary: " & ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function WriteDepthForActiveCellsToDictionary(TimestepIdx As Integer, ByRef ActiveCells As Dictionary(Of String, Boolean), WaterDepthUnits As GeneralFunctions.enmElevationUnits, NaNs As Dictionary(Of Integer, Double), TrackProgress As Boolean) As Dictionary(Of Integer, Double)
        Try
            Dim CellId As String, CellIdx As Integer, RecordIdx As Integer = 0
            Dim ResultsTable As Dictionary(Of Integer, Double) = NaNs   'initialize results by referring to our dictionary of NaN-values
            If TrackProgress Then Me.Setup.GeneralFunctions.UpdateProgressBar("Writing depth for timestep " & TimestepIdx & " to memory...", 0, 10, True)
            For Each CellId In ActiveCells.Keys
                CellIdx = Convert.ToInt32(CellId)
                If TrackProgress Then Me.Setup.GeneralFunctions.UpdateProgressBar("", CellIdx, ActiveCells.Count)
                Select Case WaterDepthUnits
                    Case Is = GeneralFunctions.enmElevationUnits.M
                        ResultsTable.Item(RecordIdx) = DepthClassValues(waterDepths(TimestepIdx, CellIdx) - 1)
                    Case Is = GeneralFunctions.enmElevationUnits.CM
                        ResultsTable.Item(RecordIdx) = DepthClassValues(waterDepths(TimestepIdx, CellIdx) - 1) * 100
                    Case Else
                        Throw New Exception("Waterdepth units not supported: " & WaterDepthUnits.ToString)
                End Select
                RecordIdx += 1
            Next
            If TrackProgress Then Me.Setup.GeneralFunctions.UpdateProgressBar("Depth for timestep " & TimestepIdx & " stored in memory...", 0, 10, True)
            Return ResultsTable
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteDepthForActiveCellsToDictionary: " & ex.Message)
            Return Nothing
        End Try
    End Function



    Public Function WriteMaxVelocityByExistingShapefileToDictionary(ByRef ActiveCells As Dictionary(Of String, Boolean), NaNs As Dictionary(Of Integer, Double)) As Dictionary(Of Integer, Double)
        Try
            Dim CellId As String, CellIdx As Integer, RecordIdx As Integer = 0
            Dim maxVal As Double = 0, curVal As Double
            Dim ResultsTable As Dictionary(Of Integer, Double) = NaNs   'initialize results by referring to our dictionary of NaN-values

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing maximum velocity to memory...", 0, 10, True)
            For Each CellId In ActiveCells.Keys
                CellIdx = Convert.ToInt32(CellId)
                maxVal = 0
                Me.Setup.GeneralFunctions.UpdateProgressBar("", CellIdx, ActiveCells.Count)

                'we found an active cell! write its result to the shapefile, assuming that the key matches the shape's index number
                For timestepidx = 0 To UBound(TimeStamps)
                    curVal = VelocityClassValues(velocities(timestepidx, CellIdx) - 1)
                    If curVal > maxVal Then maxVal = curVal
                Next
                ResultsTable.Item(RecordIdx) = maxVal
                RecordIdx += 1
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Maximum velocity stored in memory...", 0, 10, True)

            Return ResultsTable
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteMaxVelocityByExistingShapefileToDictionary: " & ex.Message)
            Return Nothing
        End Try

    End Function




End Class
