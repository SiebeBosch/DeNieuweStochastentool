Option Explicit On

Imports STOCHLIB.General
Imports System.IO
Imports MapWinGIS

Public Class clsSubcatchmentDataSource
    Public PolySF As clsShapeFile

    Public SubcatchmentIDField As String
    Public SubcatchmentIDFieldIdx As Integer = -1
    Public SubcatchmentNameField As String
    Public SubcatchmentNameFieldIdx As Integer = -1
    Public STRIDOutField As String
    Public MeteoStationIdField As String
    Public MeteoStationIdFieldIdx As Integer = -1
    Public OpenwaterAreaField As String
    Public OpenwaterAreaFieldIdx As Integer = -1
    Public FlushVolumeField As String
    Public FlushVolumeFieldIdx As Integer = -1
    Public SubcatchmentOpenConnectionField As String
    Public SubcatchmentOpenConnectionFieldIdx As Integer = -1

    'soil data
    Public SoilCodeField As String
    Public SoilCodeFieldIdx As Integer = -1

    'landuse data
    Public LanduseField As String
    Public LanduseFieldIdx As Integer = -1

    'catchment and subcatchment data
    Public CatchmentIDField As String
    Public CatchmentNameField As String
    Public InundationLevelField As String
    Public AfvCoefField As String
    Public EmergencyStopElevationField As String
    Public SelectionField As String


    Public CatchmentNameFieldIdx As Integer = -1
    Public STRIDOutFieldIdx As Integer = -1
    Public CatchmentIDFieldIdx As Integer = -1
    Public InundationLevelFieldIdx As Integer = -1
    Public AfvCoefFieldIdx As Integer = -1
    Public EmergencyStopElevationFieldIdx As Integer = -1
    Public SelectionFieldIdx As Integer = -1

    'rainfall runoff data
    Public RunoffField As String
    Public InfiltrationField As String
    Public Alpha1Field As String
    Public Alpha2Field As String
    Public Alpha3Field As String
    Public Alpha4Field As String
    Public Depth1Field As String
    Public Depth2Field As String
    Public Depth3Field As String

    Public RunoffFieldIdx As Integer = -1
    Public InfiltrationFieldIdx As Integer = -1
    Public Alpha1FieldIdx As Integer = -1
    Public Alpha2FieldIdx As Integer = -1
    Public Alpha3FieldIdx As Integer = -1
    Public Alpha4FieldIdx As Integer = -1
    Public Depth1FieldIdx As Integer = -1
    Public Depth2FieldIdx As Integer = -1
    Public Depth3FieldIdx As Integer = -1

    'sewage area data
    Public SewageAreaIdField As String
    Public SewageCategoryField As String
    Public WWTPIDField As String
    'Public StorageField As String
    Public POCField As String
    Public PavedAreaField As String 'the field that contains the (user defined) paved area as attribute value

    Public SewageAreaIdFieldIdx As Integer = -1
    Public SewageCategoryFieldidx As Integer = -1
    Public WWTPIDFieldIdx As Integer = -1
    'Public StorageFieldIdx As Integer = -1
    Public POCFieldIdx As Integer = -1
    Public PavedAreaFieldIdx As Integer = -1

    Friend TotalShape As New MapWinGIS.Shape                              'de hele shapefile samengevoegd tot één shape
    Private SelectionOperator As String
    Private SelectionOperand As Object

    Friend Setup As clsSetup



    Friend Sub New(ByRef mySetup As clsSetup, ByVal Path As String)
        Me.Setup = mySetup
        PolySF = New clsShapeFile(Me.Setup, Path)
    End Sub

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
        PolySF = New clsShapeFile(Me.Setup)
    End Sub

    Friend Sub Initialize(ByVal Path As String)
        PolySF = New clsShapeFile(Me.Setup, Path)
    End Sub

    Friend Sub Initialize()
        PolySF = New clsShapeFile(Me.Setup)
    End Sub


    Public Function Fix() As Boolean
        Dim fixedSf As New MapWinGIS.Shapefile
        If Not PolySF.sf.FixUpShapes(fixedSf) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function Fixup(newPath As String) As Boolean
        Try
            If Not PolySF.sf.Open(PolySF.Path) Then Throw New Exception("Error reading subcatchment shapefile")
            Dim fixSF As MapWinGIS.Shapefile = Nothing
            If Not PolySF.sf.FixUpShapes2(False, fixSF) Then Throw New Exception("Error fixing subcatchment shapefile: original shapefile still in use.")
            PolySF.sf = fixSF
            If Not fixSF.SaveAs(newPath) Then Throw New Exception("Error saving fixed subcatchment shapefile: original shapefile still in use.")
            PolySF.Path = newPath
            PolySF.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function setPath(ByVal myPath As String) As Boolean
        PolySF.Path = myPath
        Return True
    End Function







    Public Function getCatchmentIDByPoint(ByVal myPoint As MapWinGIS.Point) As String
        'IMPORTANT: PolySF.sf.BeginPointInShapefile() MUST have been set before using this function
        'also: the activity  must be closed by PolySF.sf.EndPointInShapefile()
        Try
            Dim ShpIdx As Integer = PolySF.sf.PointInShapefile(myPoint.x, myPoint.y)
            If ShpIdx >= 0 Then
                Return PolySF.sf.CellValue(CatchmentIDFieldIdx, ShpIdx)
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function


    Public Function SetSewageAreaIDField(ByVal FieldName As String) As Boolean
        SewageAreaIdField = FieldName
        SewageAreaIdFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If SewageAreaIdFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetSewageCategoryField(ByVal FieldName As String) As Boolean
        SewageCategoryField = FieldName
        SewageCategoryFieldidx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If SewageCategoryFieldidx >= 0 Then Return True Else Return False
    End Function

    Public Function SetSewagePavedAreaField(ByVal FieldName As String) As Boolean
        PavedAreaField = FieldName
        PavedAreaFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If PavedAreaFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetSewageAreaPOCField(ByVal FieldName As String) As Boolean
        POCField = FieldName
        POCFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If POCFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetLanduseField(ByVal FieldName As String) As Boolean
        LanduseField = FieldName
        LanduseFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If LanduseFieldIdx >= 0 Then Return True Else Return False
    End Function
    Public Function SetSoilCodeField(ByVal FieldName As String) As Boolean
        SoilCodeField = FieldName
        SoilCodeFieldIdx = PolySF.sf.FieldIndexByName(FieldName) 'setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If SoilCodeFieldIdx >= 0 Then Return True Else Return False
    End Function
    Public Function SetSubcatchmentIDField(ByVal FieldName As String) As Boolean
        SubcatchmentIDField = FieldName
        SubcatchmentIDFieldIdx = PolySF.sf.FieldIndexByName(FieldName)
        'SubcatchmentIDFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If SubcatchmentIDFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetSubcatchmentOpenConnectionField(ByVal FieldName As String) As Boolean
        SubcatchmentOpenConnectionField = FieldName
        SubcatchmentOpenConnectionFieldIdx = PolySF.sf.FieldIndexByName(FieldName)
        If SubcatchmentOpenConnectionFieldIdx >= 0 Then Return True Else Return False
    End Function


    Public Function SetAreaNameField(ByVal FieldName As String) As Boolean
        SubcatchmentNameField = FieldName
        SubcatchmentNameFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If SubcatchmentIDFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetCatchmentIDField(ByVal FieldName As String) As Boolean
        CatchmentIDField = FieldName
        CatchmentIDFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If CatchmentIDFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetCatchmentNameField(ByVal FieldName As String) As Boolean
        CatchmentNameField = FieldName
        CatchmentNameFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If CatchmentIDFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetKWKOutField(ByVal FieldName As String) As Boolean
        If FieldName Is Nothing OrElse FieldName.Trim = "" Then Return False
        STRIDOutField = FieldName
        STRIDOutFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If STRIDOutFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetMeteoStationIDField(ByVal FieldName As String) As Boolean
        If FieldName Is Nothing OrElse FieldName.Trim = "" Then Return False
        MeteoStationIdField = FieldName
        MeteoStationIdFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If MeteoStationIdFieldIdx >= 0 Then Return True Else Return False
    End Function
    Public Function SetOpenwaterAreaField(ByVal FieldName As String) As Boolean
        If FieldName Is Nothing OrElse FieldName.Trim = "" Then Return False
        OpenwaterAreaField = FieldName
        OpenwaterAreaFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If OpenwaterAreaFieldIdx >= 0 Then Return True Else Return False
    End Function
    Public Function SetFlushVolumeField(ByVal FieldName As String) As Boolean
        If FieldName Is Nothing OrElse FieldName.Trim = "" Then Return False
        FlushVolumeField = FieldName
        FlushVolumeFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If FlushVolumeFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetInundationLevelField(ByVal FieldName As String) As Boolean
        InundationLevelField = FieldName
        InundationLevelFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If InundationLevelFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetSelectionField(ByVal FieldName As String) As Boolean
        SelectionField = FieldName
        SelectionFieldIdx = PolySF.sf.FieldIndexByName(FieldName) 'setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If SelectionFieldIdx >= 0 Then Return True Else Return False
    End Function


    Public Function SetWWTPIDField(ByVal FieldName As String) As Boolean
        WWTPIDField = FieldName
        WWTPIDFieldIdx = PolySF.sf.FieldIndexByName(FieldName) 'setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If WWTPIDFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetRunoffField(ByVal FieldName As String) As Boolean
        RunoffField = FieldName
        RunoffFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If RunoffFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetInfiltrationField(ByVal FieldName As String) As Boolean
        InfiltrationField = FieldName
        InfiltrationFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If InfiltrationFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetAlpha1Field(ByVal FieldName As String) As Boolean
        Alpha1Field = FieldName
        Alpha1FieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Alpha1FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetAlpha2Field(ByVal FieldName As String) As Boolean
        Alpha2Field = FieldName
        Alpha2FieldIdx = PolySF.sf.FieldIndexByName(FieldName) 'setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Alpha2FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetAlpha3Field(ByVal FieldName As String) As Boolean
        Alpha3Field = FieldName
        Alpha3FieldIdx = PolySF.sf.FieldIndexByName(FieldName) 'setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Alpha3FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetAlpha4Field(ByVal FieldName As String) As Boolean
        Alpha4Field = FieldName
        Alpha4FieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Alpha4FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetDepth1Field(ByVal FieldName As String) As Boolean
        Depth1Field = FieldName
        Depth1FieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Depth1FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetDepth2Field(ByVal FieldName As String) As Boolean
        Depth2Field = FieldName
        Depth2FieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Depth2FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetDepth3Field(ByVal FieldName As String) As Boolean
        Depth3Field = FieldName
        Depth3FieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)
        If Depth3FieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function SetEmergencyStopElevationField(ByVal FieldName As String) As Boolean
        EmergencyStopElevationField = FieldName
        EmergencyStopElevationFieldIdx = PolySF.sf.FieldIndexByName(FieldName) ' setup.GISData.getShapeFieldIdxFromFileName(PolySF.Path, FieldName)

        If EmergencyStopElevationFieldIdx >= 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Function SetSubcatchmentIDFieldIdx(ByVal ShapeField As String) As Boolean
        Dim tmpField As String = ShapeField.Trim.ToUpper, i As Long
        For i = 0 To PolySF.sf.NumFields - 1
            If PolySF.sf.Field(i).Name.Trim.ToUpper = ShapeField Then
                SubcatchmentIDFieldIdx = i
                Return True
            End If
        Next
        Return False
    End Function

    Public Function GetShapeByAreaID(ByVal ID As String) As MapWinGIS.Shape
        Dim i As Long

        Try
            If Not PolySF.Open() Then Throw New Exception("Error opening shapefile.")
            For i = 0 To PolySF.sf.NumShapes - 1
                If PolySF.sf.CellValue(SubcatchmentIDFieldIdx, i).ToString.Trim.ToUpper = ID.Trim.ToUpper Then
                    Return PolySF.sf.Shape(i)
                End If
            Next
            Return Nothing
            PolySF.Close()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function GetShapeIdxByAreaID(ByVal ID As String) As Integer
        Dim i As Long
        Try
            If Not PolySF.Open() Then Throw New Exception("Error opening shapefile.")
            For i = 0 To PolySF.sf.NumShapes - 1
                If PolySF.sf.CellValue(SubcatchmentIDFieldIdx, i).ToString.Trim.ToUpper = ID.Trim.ToUpper Then
                    Return i
                End If
            Next
            Return -1
            PolySF.Close()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Friend Function findAreaIDFieldIdx(ByVal ShapeField As String) As Boolean
        'omdat dit een vrij specifiek veld is, permitteren we ons wat vrijheid bij de zoektocht
        Dim tmpField As String = ShapeField.Trim.ToUpper
        Dim i As Long

        For i = 0 To PolySF.sf.NumFields - 1
            If PolySF.sf.Field(i).Name.Trim.ToUpper = ShapeField.Trim.ToUpper Then
                SubcatchmentIDFieldIdx = i
                Return True
            End If
        Next

        'niet gevonden, dus zoek andere voordehandliggende opties
        For i = 0 To PolySF.sf.NumFields - 1
            If PolySF.sf.Field(i).Name.Trim.ToUpper = "GPGIDENT" Then
                SubcatchmentIDFieldIdx = i
                Return True
            End If
        Next

        For i = 0 To PolySF.sf.NumFields - 1
            If PolySF.sf.Field(i).Name.Trim.ToUpper = "GFEIDENT" Then
                SubcatchmentIDFieldIdx = i
                Return True
            End If
        Next

        For i = 0 To PolySF.sf.NumFields - 1
            If PolySF.sf.Field(i).Name.Trim.ToUpper = "GAFIDENT" Then
                SubcatchmentIDFieldIdx = i
                Return True
            End If
        Next

        Return False

    End Function

    Public Function getInundationLevel(ByVal Shapeidx As Long) As Double
        If InundationLevelFieldIdx >= 0 Then
            Return Me.PolySF.sf.CellValue(InundationLevelFieldIdx, Shapeidx)
        Else
            Return Double.NaN
        End If
    End Function


    Public Function getSelectedStatus(ByVal ShapeIdx As Long) As Boolean
        Dim myNum As Double = 0, myStr As String = "", Numeric As Boolean

        Try
            Numeric = IsNumeric(Me.PolySF.sf.CellValue(SelectionFieldIdx, ShapeIdx))
            If Numeric Then
                myNum = Me.PolySF.sf.CellValue(SelectionFieldIdx, ShapeIdx)
            Else
                myStr = Me.PolySF.sf.CellValue(SelectionFieldIdx, ShapeIdx).ToString.Trim.ToUpper
            End If

            If Numeric Then
                Select Case SelectionOperator
                    Case Is = ">"
                        Return myNum > SelectionOperand
                    Case Is = "<"
                        Return myNum < SelectionOperand
                    Case Is = ">="
                        Return myNum >= SelectionOperand
                    Case Is = "<="
                        Return myNum <= SelectionOperand
                    Case Is = "="
                        Return myNum = SelectionOperand
                    Case Else
                        Me.Setup.Log.AddError("Error: invalid operator for numeric field selection " & SelectionOperator & ".")
                        Throw New Exception("Error in sub getSelectedStatus in class clsGebiedenShapeFile.")
                End Select
            Else
                Select Case SelectionOperator
                    Case Is = "IS"
                        Return myNum = SelectionOperand
                    Case Is = "NOT"
                        Return myNum <> SelectionOperand
                    Case Else
                        Me.Setup.Log.AddError("Error: invalid operator for string field selection " & SelectionOperator & ".")
                        Throw New Exception("Error in sub getSelectedStatus in class clsGebiedenShapeFile.")
                End Select
            End If

            Me.Setup.Log.AddError("Unsupported selection string encountered: " & SelectionOperator)
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function GetSelectedShapeIdx() As Long
        'finds out if a shape has been selected by the user and returns the corresponding index number
        'if no selected shape found, it returns -1
        Dim i As Long
        PolySF.Open()
        For i = 0 To PolySF.sf.NumShapes - 1
            If PolySF.sf.ShapeSelected(i) Then Return i
        Next
        Return -1
    End Function

    Public Sub ExportTotalShape(ByVal FileName As String)
        Dim ShapeIdx As Long, FieldIdx As Long
        Try
            Dim TotalSF As New MapWinGIS.Shapefile
            If Not TotalSF.CreateNew(FileName, MapWinGIS.ShpfileType.SHP_POLYGON) Then Throw New Exception("Could not create shapefile for merged shapes.")
            If Not TotalSF.StartEditingShapes(True) Then Throw New Exception("Could not start editing shapes.")
            FieldIdx = TotalSF.EditAddField("ID", MapWinGIS.FieldType.STRING_FIELD, 10, 10)
            ShapeIdx = TotalSF.EditAddShape(TotalShape)
            If Not TotalSF.StopEditingShapes(True, True) Then Throw New Exception("Could not stop editing merged shapefile.")
            TotalSF.Save()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Sub


    Public Function IDsUnique() As Boolean

        'bepaalt of de Area Shapefile uitsluitend unieke Area ID's bevat
        Dim myIDs As New Dictionary(Of String, String)
        Dim myID As String, AllUnique As Boolean = True
        Dim i As Long

        If Open() Then
            For i = 0 To Me.PolySF.sf.NumShapes - 1
                myID = PolySF.sf.CellValue(SubcatchmentIDFieldIdx, i).ToString
                If myIDs.ContainsKey(myID.Trim.ToUpper) Then
                    Me.Setup.Log.AddError("Multiple instances for Area ID " & myID & " in area shapefile.")
                    AllUnique = False
                Else
                    myIDs.Add(myID.Trim.ToUpper, myID)
                End If
            Next
        End If
        Return AllUnique
    End Function

    ''' <summary>
    ''' Deze subroutine voegt shapes samen in één nieuwe shape
    ''' </summary>
    ''' <remarks></remarks>
    Public Function MergeAllShapes() As Boolean
        'this routine merges all shapes inside a shapefile into one TotalShape
        Dim Shape1 As New MapWinGIS.Shape, Shape2 As New MapWinGIS.Shape
        Dim utils As New MapWinGIS.Utils()
        Dim i As Long

        Try
            'hanteer een UNION om alle shapes in één samen te voegen
            If Not Open() Then Throw New Exception("Could not open subcatchments shapefile.")

            'read the number of shapes in this shapefile
            Dim numShapes = Me.PolySF.sf.NumShapes
            If numShapes = 0 Then Throw New Exception("Shapefile was emtpy.")

            'read the first shape
            TotalShape = Me.PolySF.sf.Shape(0)
            If Not TotalShape.IsValid Then TotalShape.FixUp(TotalShape)
            If Not TotalShape.IsValid Then Throw New Exception("Error in shapefile: first shape is corrupt and could not be fixed.")

            Me.Setup.GeneralFunctions.UpdateProgressBar("Merging all shapes...", 0, numShapes)

            'read the shapefile, starting with the second shape and merge with totalshape
            If numShapes > 1 Then
                For i = 1 To numShapes - 1
                    Console.WriteLine("Processing shape " & i + 1 & " from " & numShapes)

                    'set shape 1
                    Shape1 = TotalShape

                    'set shape 2 and attempt to fix if not valid
                    Shape2 = PolySF.sf.Shape(i)
                    If Not Shape2.IsValid Then Shape2.FixUp(Shape2)

                    'only if valid, merge with shape 1
                    If Shape2.IsValid Then
                        TotalShape = utils.ClipPolygon(MapWinGIS.PolygonOperation.UNION_OPERATION, Shape1, Shape2)
                        If Not TotalShape.IsValid Then
                            TotalShape.FixUp(TotalShape) 'als dit leidt tot een corrupte shape, probeer te fiksen
                            If Not TotalShape.IsValid Then TotalShape = Shape1 'als het fixen niet gelukt is, val terug op de oorspronkelijke totalshape, van voor de union
                        End If
                    End If

                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, numShapes)
                Next i
            End If

            Return True
        Catch ex As Exception
            Dim log As String = "Error in MergeAllShapes"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Return False
        Finally
            'If Not Shape1 Is Nothing Then Me.setup.GeneralFunctions.ReleaseComObject(Shape1, False) releasing this one screws up the TotalShape
            If Not Shape2 Is Nothing Then Me.Setup.GeneralFunctions.ReleaseComObject(Shape2, False)
            Me.Setup.GeneralFunctions.ReleaseComObject(utils, True)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Done merging shapes...", 0, 10)
        End Try
    End Function

    ''' <summary>
    ''' Deze subroutine voegt shapes behorende bij hetzelfde catchment samen tot één nieuwe shape per Catchment
    ''' </summary>
    ''' <remarks></remarks>




    Public Function Open(Optional ByVal setBeginPointInShapefile As Boolean = True) As Boolean
        Try
            'siebe: since v1.66 we will remove the shapefile itself from memory when closing. This is for memory clearing purposes
            'for that reason we built in a check when opening: create the shapefile object if it is empty
            If Not System.IO.File.Exists(Me.PolySF.Path) Then Throw New Exception("Subcatchment's shapefile does not exist and could not be opened:" & Me.PolySF.Path)
            If Me.PolySF.sf Is Nothing Then Me.PolySF.sf = New MapWinGIS.Shapefile
            If Me.PolySF.Path = "" Then
                Me.Setup.Log.AddWarning("Subcatchment shapefile is already open or path is empty.")
            Else
                If Not Me.PolySF.sf.Open(PolySF.Path) Then Throw New Exception("Could not open subcatchments shapefile.")
            End If

            'new in v1.71: we implemented the method BeginPointinShapefile in this function since we need it often!
            If setBeginPointInShapefile Then Me.PolySF.sf.BeginPointInShapefile()
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub Close(Optional ByVal setEndPointInShapefile As Boolean = True)
        'new in v1.71: we implemented the method BeginPointinShapefile in this function since we need it often!
        'v1.798: removed this part again since it caused a crash in situations where the shapefile was not open
        'If setEndPointInShapefile Then PolySF.sf.EndPointInShapefile()

        'siebe: since v1.66 we will remove the shapefile itself from memory when closing. This is for memory clearing purposes
        If Not Me.PolySF.sf Is Nothing Then
            Me.PolySF.sf.Close()
            Me.PolySF.sf = Nothing
        End If
    End Sub

    Public Function Read() As Boolean
        'doorloop alle velden in de shapefile van de gebieden
        Dim i As Integer
        Dim errors As New List(Of String)

        Try
            Read = True
            Call Me.Setup.GeneralFunctions.UpdateProgressBar("Finding shapefields...", 0, 100)

            STRIDOutFieldIdx = -999
            SubcatchmentIDFieldIdx = -999
            CatchmentIDFieldIdx = -999

            If Not PolySF.sf.Open(PolySF.Path) Then
                Me.Setup.Log.AddError("Could not open file " & PolySF.Path)
                Throw New Exception
            End If

            ' TODO: Dit hoeft alleen als de indexen nog niet gezet zijn tijdens de init:
            For i = 0 To PolySF.sf.NumFields - 1
                Call Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, PolySF.sf.NumFields)
                If PolySF.sf.Field(i).Name = STRIDOutField Then STRIDOutFieldIdx = i
                If PolySF.sf.Field(i).Name = SubcatchmentIDField Then SubcatchmentIDFieldIdx = i
                If PolySF.sf.Field(i).Name = CatchmentIDField Then CatchmentIDFieldIdx = i
                If PolySF.sf.Field(i).Name = AfvCoefField Then AfvCoefFieldIdx = i
            Next i

            'foutafhandeling
            If STRIDOutFieldIdx < 0 Then Me.Setup.Log.AddWarning("Field: " & STRIDOutField & " for outlet structure not found in shapefile.")
            If SubcatchmentIDFieldIdx < 0 Then Me.Setup.Log.AddWarning("Field: " & SubcatchmentIDField & " for subcatchment ID not found in shapefile.")
            If CatchmentIDFieldIdx < 0 Then Me.Setup.Log.AddWarning("Field: " & CatchmentIDField & " for Catchment name not found in shapefile.")
            If AfvCoefFieldIdx < 0 Then Me.Setup.Log.AddWarning("Field: " & AfvCoefField & " for discharge coefficient not found in shapefile.")
            If errors.Count > 0 Then Throw New Exception

        Catch ex As Exception
            Dim log As String = "Errors in sub Read of class clsGebiedenShapeFile"""
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            For i = 0 To errors.Count - 1
                Me.Setup.Log.AddError(errors(i))
            Next i
            ' TODO: Geen exception?
        End Try
    End Function

    Public Function IntersectsGrid(ByRef myGrid As MapWinGIS.Grid) As Boolean
        'this function finds out whether or not a grid intersects with the total shape of this shapefile
        'NOTE: is not perfect!!
        'author: siebe bosch
        'date: 22 jan 2014
        Dim myPoint As MapWinGIS.Point
        Dim myUtils As New MapWinGIS.Utils

        Try
            If TotalShape.Area = 0 Then Call MergeAllShapes()

            Dim i As Long

            myPoint = New MapWinGIS.Point
            myPoint.x = myGrid.Extents.xMin
            myPoint.y = myGrid.Extents.yMin
            If myUtils.PointInPolygon(TotalShape, myPoint) Then Return True

            myPoint = New MapWinGIS.Point
            myPoint.x = myGrid.Extents.xMin
            myPoint.y = myGrid.Extents.yMax
            If myUtils.PointInPolygon(TotalShape, myPoint) Then Return True

            myPoint = New MapWinGIS.Point
            myPoint.x = myGrid.Extents.xMax
            myPoint.y = myGrid.Extents.yMin
            If myUtils.PointInPolygon(TotalShape, myPoint) Then Return True

            myPoint = New MapWinGIS.Point
            myPoint.x = myGrid.Extents.xMax
            myPoint.y = myGrid.Extents.yMax
            If myUtils.PointInPolygon(TotalShape, myPoint) Then Return True

            For i = 0 To TotalShape.numPoints - 1
                myPoint = TotalShape.Point(i)
                If myPoint.x >= myGrid.Extents.xMin AndAlso myPoint.x <= myGrid.Extents.xMax AndAlso myPoint.y >= myGrid.Extents.yMin AndAlso myPoint.y <= myGrid.Extents.yMax Then
                    Return True
                End If
            Next
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in Function IntersectsGrid Of MyClass clsSubcatchmentDataSource.")
            Me.setup.Log.AddError(ex.Message)
        Finally
            myPoint = Nothing
            myUtils = Nothing
        End Try
    End Function


    Public Function GetCatchmentList() As Dictionary(Of String, String)
        'this function creates a list of all unique catchment ID's in the area shapefile
        Dim i As Long, myList As New Dictionary(Of String, String), myID As String
        For i = 0 To PolySF.sf.NumShapes
            myID = PolySF.sf.CellValue(CatchmentIDFieldIdx, i)
            If Not myList.ContainsKey(myID.Trim.ToUpper) Then
                myList.Add(myID.Trim.ToUpper, myID)
            End If
        Next
        Return myList
    End Function

    Public Function GetFieldIdx(ByVal NAME As String) As Integer
        Dim i As Long
        If NAME Is Nothing Then Return -1
        For i = 0 To PolySF.sf.NumFields - 1
            If PolySF.sf.Field(i).Name.Trim.ToUpper = NAME.Trim.ToUpper Then Return i
        Next
        Return -1
    End Function

End Class
