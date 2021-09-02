Imports System.Windows.Forms
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsChannelShapeFile
    Public Path As String
    Public sf As New MapWinGIS.Shapefile
    Public InUse As Boolean

    Public IdFieldIdx As Integer = -1
    Public FromNodeFieldIdx As Integer = -1
    Public ToNodeFieldIdx As Integer = -1
    Public CategoryFieldIdx As Integer = -1
    Public StructureCategoryFieldIdx As Integer = -1
    Public TypeFieldIdx As Integer = -1
    Public CapFieldIdx As Integer = -1
    Public NameFieldIdx As Integer = -1
    Public BWUpFieldIdx As Integer = -1
    Public BWDownFieldIdx As Integer = -1
    Public BWUpField2Idx As Integer = -1
    Public BWDownField2Idx As Integer = -1
    Public BLUPFieldIdx As Integer = -1
    Public BLDOWNFieldIdx As Integer = -1
    Public BLUPField2Idx As Integer = -1
    Public BLDOWNField2Idx As Integer = -1
    Public SLOPELFieldIdx As Integer = -1
    Public SLOPERFieldIdx As Integer = -1
    Public SLOPELField2Idx As Integer = -1
    Public SLOPERField2Idx As Integer = -1
    Public SlopeUpFieldIdx As Integer = -1
    Public SlopeDownFieldIdx As Integer = -1
    Public SlopeUpField2Idx As Integer = -1
    Public SlopeDownField2Idx As Integer = -1
    Public MVUpFieldIdx As Integer = -1
    Public MVDownFieldIdx As Integer = -1
    Public MVUpField2Idx As Integer = -1
    Public MVDownField2Idx As Integer = -1
    Public SWUpFieldIdx As Integer = -1              'surface width
    Public SWDownFieldIdx As Integer = -1            'surface width
    Public SWUpField2Idx As Integer = -1              'surface width
    Public SWDownField2Idx As Integer = -1            'surface width
    Public WLUpFieldIdx As Integer = -1              'waterlevel up
    Public WLDnFieldIdx As Integer = -1              'waterlevel down
    Public WLUpField2Idx As Integer = -1              'waterlevel up
    Public WLDnField2Idx As Integer = -1              'waterlevel down
    Public WSWUpFieldIdx As Integer = -1             'water surface width up
    Public WSWDnFieldIdx As Integer = -1             'water surface width down
    Public WSWUpField2Idx As Integer = -1             'water surface width up
    Public WSWDnField2Idx As Integer = -1             'water surface width down
    Public WDUpFieldIdx As Integer = -1              'water depth up
    Public WDDnFieldIdx As Integer = -1              'water depth down
    Public WDUpField2Idx As Integer = -1              'water depth up
    Public WDDnField2Idx As Integer = -1              'water depth down

    'wet berm section
    Public WetBermLeftLowestIdx As Integer = -1
    Public WetBermRightLowestIdx As Integer = -1
    Public WetBermLeftHighestIdx As Integer = -1
    Public WetBermRightHighestIdx As Integer = -1
    Public WetBermLeftSlopeIdx As Integer = -1
    Public WetBermRightSlopeIdx As Integer = -1
    Public WetBermLeftWidthIdx As Integer = -1
    Public WetBermRightWidthIdx As Integer = -1
    Public WetBermLeftSideSlopeIdx As Integer = -1
    Public WetBermRightSideSlopeIdx As Integer = -1

    'open fish passages section
    Public FishPassageWidthFieldIdx As Integer = -1
    Public FishPassageHighestElevationFieldIdx As Integer = -1
    Public FishPassageLowestElevationFieldIdx As Integer = -1
    Public FishPassagenStepsFieldIdx As Integer = -1

    Public IdField As String
    Public FromNodeField As String
    Public ToNodeField As String
    Public CategoryField As String           'channel category for selection
    Public StructureCategoryField As String  'a categorization field that indicates whether or not a channel is actually a long structure
    Public TypeField As String
    Public NameField As String
    Public CapField As String

    Public BWUpField As String
    Public BWDownField As String
    Public BWUpField2 As String
    Public BWDownField2 As String
    Public BLUPField As String
    Public BLDOWNField As String
    Public BLUPField2 As String
    Public BLDOWNField2 As String
    Public SLOPELField As String
    Public SLOPERField As String
    Public SLOPELField2 As String
    Public SLOPERField2 As String
    Public SlopeUpField As String
    Public SlopeDownField As String
    Public SlopeUpField2 As String
    Public SlopeDownField2 As String
    Public MVUpField As String
    Public MVDownField As String
    Public MVUpField2 As String
    Public MVDownField2 As String
    Public SWUpField As String
    Public SWDownField As String
    Public SWUpField2 As String
    Public SWDownField2 As String
    Public WLUpField As String              'waterlevel up
    Public WLDnField As String              'waterlevel down
    Public WLUpField2 As String              'waterlevel up
    Public WLDnField2 As String              'waterlevel down
    Public WSWUpField As String             'water surface width up
    Public WSWDnField As String             'water surface width down
    Public WSWUpField2 As String             'water surface width up
    Public WSWDnField2 As String             'water surface width down
    Public WDUpField As String              'water depth up
    Public WDDnField As String              'water depth down
    Public WDUpField2 As String              'water depth up
    Public WDDnField2 As String              'water depth down

    'the wet berm section
    Public WetBermLeftLowestField As String
    Public WetBermRightLowestField As String
    Public WetBermLeftHighestField As String
    Public WetBermRightHighestField As String
    Public WetBermLeftSlopeField As String
    Public WetBermRightSlopeField As String
    Public WetBermLeftWidthField As String
    Public WetBermRightWidthField As String

    'open fish passages section
    Public FishPassageWidthField As String
    Public FishPassageHighestElevationField As String
    Public FishPassageLowestElevationField As String
    Public FishPassagenStepsField As String

    Private Setup As clsSetup

    Friend Sub CreateNew(FileName As String)
        Try
            If System.IO.File.Exists(FileName) Then Setup.GeneralFunctions.deleteShapeFile(FileName)

            'add the required fields
            If Not sf.CreateNew(FileName, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Throw New Exception("Error creating new shapefile " & FileName)

            sf.SaveAs(FileName)
            If Not setPath(FileName) Then Throw New Exception("Error: could not set path of newly created shapefile " & FileName)
            Close()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try
    End Sub

    Friend Sub New(ByRef mysetup As clsSetup)
        Setup = mysetup
    End Sub

    Friend Sub New(ByRef mysetup As clsSetup, FileName As String)
        Setup = mysetup
        setPath(FileName)
    End Sub

    Public Function Open() As Boolean
        If Not System.IO.File.Exists(Path) Then
            Return False
        ElseIf Not sf.Open(Path) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Sub Close()
        Me.sf.Close()
    End Sub

    Public Function SetPath(ByVal myPath As String) As Boolean
        If System.IO.File.Exists(myPath) Then
            Path = myPath
            Return True
        Else
            Return False
        End If
    End Function



    Public Function setCategoryField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        CategoryField = FieldName
        If FromMemory Then
            CategoryFieldIdx = Me.Setup.GeneralFunctions.GetShapeFieldIdxByName(sf, CategoryField)
        Else
            CategoryFieldIdx = Setup.GISData.getShapeFieldIdxFromFileName(Path, CategoryField)
        End If
        Return True
    End Function

    Public Function setStructureCategoryField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        StructureCategoryField = FieldName
        If FromMemory Then
            StructureCategoryFieldIdx = Me.Setup.GeneralFunctions.GetShapeFieldIdxByName(sf, StructureCategoryField)
        Else
            StructureCategoryFieldIdx = Setup.GISData.getShapeFieldIdxFromFileName(Path, StructureCategoryField)
        End If
        Return True
    End Function

    Public Function setTypeField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        TypeField = FieldName
        If FromMemory Then
            TypeFieldIdx = Me.Setup.GeneralFunctions.GetShapeFieldIdxByName(sf, TypeField)
        Else
            TypeFieldIdx = Setup.GISData.getShapeFieldIdxFromFileName(Path, TypeField)
        End If
        Return True
    End Function

    Public Function CreateField(ByVal FieldName As String, FieldType As MapWinGIS.FieldType, Precision As Integer, Width As Integer) As Boolean
        If sf.EditAddField(FieldName, FieldType, Precision, Width) >= 0 Then Return True Else Return False
    End Function

    Public Function setIDField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        'v1.798: extended the posibilities for setting a shape field: now an option to choose between from File and from Memory
        'this was introduced because setting a field from file was not possible when the shapefile had just been created and not yet stored to disk
        IdField = FieldName
        IdFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setFromNodeField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        FromNodeField = FieldName
        FromNodeFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setToNodeField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        ToNodeField = FieldName
        ToNodeFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function getShapeFieldIdx(FieldName As String) As Integer
        Dim i As Integer
        For i = 0 To sf.NumFields - 1
            If sf.Field(i).Name = FieldName Then Return i
        Next
        Return -1
    End Function

    Public Function setCAPField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        CapField = FieldName
        CapFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setBedWidthUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BWUpField = FieldName
        BWUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setBedWidthUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BWUpField2 = FieldName
        BWUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function
    Public Function setBedWidthDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BWDownField = FieldName
        BWDownFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setBedWidthDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BWDownField2 = FieldName
        BWDownField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setWaterLevelUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WLUpField = FieldName
        WLUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function
    Public Function setWaterLevelUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WLUpField2 = FieldName
        WLUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterLevelDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WLDnField = FieldName
        WLDnFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterLevelDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WLDnField2 = FieldName
        WLDnField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterSurfaceWidthUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WSWUpField = FieldName
        WSWUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterSurfaceWidthUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WSWUpField2 = FieldName
        WSWUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterSurfaceWidthDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WSWDnField = FieldName
        WSWDnFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterSurfaceWidthDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WSWDnField2 = FieldName
        WSWDnField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterDepthUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WDUpField = FieldName
        WDUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterDepthUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WDUpField2 = FieldName
        WDUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterDepthDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WDDnField = FieldName
        WDDnFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWaterDepthDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        WDDnField2 = FieldName
        WDDnField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setBedLevelUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BLUPField = FieldName
        BLUPFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setBedLevelUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BLUPField2 = FieldName
        BLUPField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setBedLevelDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BLDOWNField = FieldName
        BLDOWNFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setBedLevelDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        BLDOWNField2 = FieldName
        BLDOWNField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSlopeUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SlopeUpField = FieldName
        SlopeUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function
    Public Function setSlopeUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SlopeUpField2 = FieldName
        SlopeUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSlopeDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SlopeDownField = FieldName
        SlopeDownFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSlopeDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SlopeDownField2 = FieldName
        SlopeDownField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfaceLevelUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        MVUpField = FieldName
        MVUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfaceLevelUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        MVUpField2 = FieldName
        MVUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfaceLevelDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        MVDownField = FieldName
        MVDownFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfaceLevelDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        MVDownField2 = FieldName
        MVDownField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfacewidthUpField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SWUpField = FieldName
        SWUpFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfacewidthUpField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SWUpField2 = FieldName
        SWUpField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfacewidthDownField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SWDownField = FieldName
        SWDownFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setSurfacewidthDownField2(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        SWDownField2 = FieldName
        SWDownField2Idx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
        Return True
    End Function

    Public Function setFishPassageWidthField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        FishPassageWidthField = FieldName
        FishPassageWidthFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setFishPassageHighestElevationField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        FishPassageHighestElevationField = FieldName
        FishPassageHighestElevationFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setFishPassageLowestElevationField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        FishPassageLowestElevationField = FieldName
        FishPassageLowestElevationFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setFishPassagenStepsField(ByVal FieldName As String, FromMemory As Boolean) As Boolean
        FishPassagenStepsField = FieldName
        FishPassagenStepsFieldIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, FieldName)
    End Function

    Public Function setWetBermFields(WetBermLeftLowestFieldName As String, WetBermRightLowestFieldName As String, WetBermLeftHighestFieldName As String, WetBermRightHighestFieldName As String, WetBermLeftSlopeFieldName As String, WetBermRightSlopeFieldName As String, WetBermLeftWidthFieldName As String, WetBermRightWidthFieldName As String, FromMemory As Boolean) As Boolean
        WetBermLeftLowestField = WetBermLeftLowestFieldName
        WetBermRightLowestField = WetBermRightLowestFieldName
        WetBermLeftHighestField = WetBermLeftHighestFieldName
        WetBermRightHighestField = WetBermRightHighestFieldName
        WetBermLeftSlopeField = WetBermLeftSlopeFieldName
        WetBermRightSlopeField = WetBermRightSlopeFieldName
        WetBermLeftWidthField = WetBermLeftWidthFieldName
        WetBermRightWidthField = WetBermRightWidthFieldName

        WetBermLeftLowestIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermLeftLowestField)
        WetBermRightLowestIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermRightLowestField)
        WetBermLeftHighestIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermLeftHighestField)
        WetBermRightHighestIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermRightHighestField)
        WetBermLeftSlopeIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermLeftSlopeField)
        WetBermRightSlopeIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermRightSlopeField)
        WetBermLeftWidthIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermLeftWidthField)
        WetBermRightWidthIdx = Setup.GISData.GetShapeFieldIdx(sf, Path, FromMemory, WetBermRightWidthField)
    End Function

    Public Function PointDataFromReachDist(ByVal ShapeIdx As Long, ByVal Dist As Double, ByRef X As Double, ByRef Y As Double, ByRef Angle As Double) As Boolean

        'Date: 16-6-2013
        'Author: Siebe Bosch
        'Description: searches for a given shape & distance on the shape the corresponding X- and Y-coordinates as well as the angle of the shape
        'on that particular location
        Dim i As Long

        Try
            Dim myShape As MapWinGIS.Shape
            Dim prevPoint As MapWinGIS.Point, prevDist As Double
            Dim nextPoint As MapWinGIS.Point, nextDist As Double
            myShape = sf.Shape(ShapeIdx)

            prevDist = 0
            nextDist = 0

            For i = 0 To myShape.numPoints - 2
                prevPoint = myShape.Point(i)
                nextPoint = myShape.Point(i + 1)
                nextDist += Math.Sqrt((nextPoint.y - prevPoint.y) ^ 2 + (nextPoint.x - prevPoint.x) ^ 2)

                If nextDist >= Dist Then
                    'interpolate to find the XY-coordinate that belongs to the given distance on the reach
                    X = setup.GeneralFunctions.Interpolate(prevDist, prevPoint.x, nextDist, nextPoint.x, Dist)
                    Y = setup.GeneralFunctions.Interpolate(prevDist, prevPoint.y, nextDist, nextPoint.y, Dist)
                    Angle = Me.setup.GeneralFunctions.LineAngleDegrees(prevPoint.x, prevPoint.y, nextPoint.x, nextPoint.y)
                    Angle = Me.setup.GeneralFunctions.NormalizeAngle(Angle)
                    Return True
                End If
                prevDist = nextDist

            Next
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function PointDataFromReachDist.")
            Return False
        End Try


    End Function

    Public Function Read(ByRef Setup As clsSetup, ByRef prProgress As ProgressBar, ByRef lblProgress As Label) As Boolean
        'doorloop alle velden in de shapefile van de gebieden
        Dim i As Integer
        Dim errors As New List(Of String)

        Try
            Read = True
            Call Me.setup.GeneralFunctions.UpdateProgressBar("Finding shapefields...", 0, 100)

            CategoryFieldIdx = -999

            sf = New MapWinGIS.Shapefile
            If Not sf.Open(Path) Then
                Me.setup.Log.AddError("Could not open file " & Path)
                Throw New Exception
            End If

            ' TODO: Dit hoeft alleen als de indexen nog niet gezet zijn tijdens de init:
            For i = 0 To sf.NumFields - 1
                Call Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, sf.NumFields)
                If sf.Field(i).Name = CategoryField Then CategoryFieldIdx = i
            Next i

            'foutafhandeling
            If CategoryFieldIdx < 0 Then Me.setup.Log.AddError("Field for channel category: " & CategoryField & " not found in shapefile.")
            If errors.Count > 0 Then Throw New Exception

        Catch ex As Exception
            Dim log As String = "Errors in sub Read of class clsGebiedenShapeFile"""
            Me.setup.Log.AddError(log + ": " + ex.Message)
            For i = 0 To errors.Count - 1
                Me.setup.Log.AddError(errors(i))
            Next i
            ' TODO: Geen exception?
        End Try
    End Function

    Public Function BuildFromActiveCase(ByVal ExportDir As String) As Boolean
        'Date: 17-10-2013
        'Author: Siebe Bosch
        'Description: converts all SOBEK Reaches in the active case to a shapefile
        Dim myReach As clsSbkReach
        Dim myShape As MapWinGIS.Shape
        Dim myPoint As clsSbkVectorPoint, newPoint As MapWinGIS.Point
        Dim PointIdx As Long, ReachIDIdx As Long, ShapeIdx As Long
        Dim iReach As Long, nReach As Long

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



End Class
