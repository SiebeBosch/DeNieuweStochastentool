Option Explicit On
Imports System.Windows.Forms
Imports System.Globalization
Imports System.Data.SQLite
Imports System.IO

Namespace General
    Public Class clsSetup
        Public GISData As clsGISData                                            'bevat alle shapefiles en rasters!
        Public SOBEKData As clsSOBEK                                            'bevat alle SOBEK-modellen en -cases
        Public DIMRData As clsDIMR
        Public KNMIData As clsKNMIData                  'bevat alle data van het KNMI
        Public StochastenAnalyse As clsStochastenAnalyse
        Public TijdreeksAnalyse As clsTijdreeksAnalyse
        Public TijdreeksStatistiek As clsTijdreeksStatistiek
        Public ExtremeValuesStatistics As clsExtremeValuesStatistics
        Public IDFs As clsIDFs                  'een klasse met regenduurlijnen en hun eigenschappen

        Public HydroMathOperations As Dictionary(Of String, clsHydroMathOperation) 'here we keep track of all supported math operations for hydrological purposes

        Public BoundaryObjects As clsBoundaries             'een klasse met boundaries, hun ID, Name, X, Y en StrucType

        Public GridEditor As clsGridEditor                  'een klasse voor de Ultimate Grid Editor

        Public CSOLocations As clsRRInflowLocations
        Public SewageAreas As clsRRPavedNodes
        Public WWTPs As clsWWTPs
        Public WWTPDischargePoints As clsWWTPDischargePoints

        Public Database As String                           'pad naar de publieke database
        'Public AccessCon As New OleDb.OleDbConnection      'publieke databaseconnectie voor allerlei applicaties!
        Public SqliteCon As New SQLiteConnection            'publieke databaseconnectoie voor allerlei applicaties!

        Public Settings As clsSettings          'de algemene instellingen

        Public GeneralFunctions As GeneralFunctions      'algemene functies
        Public SmoothSavGoy As clsSavitzkyGolaySmoothing 'smoothing function, based on fitting to a 2nd-order polynomal

        Public ExcelFile As clsExcelBook        'een workbook in Excel voor resultaten
        Public Log As New clsLog                'de logfile

        Friend importDir As String              'export directory voor resultaten van bewerkingen
        Friend progressBar As ProgressBar
        Friend progressLabel As System.Windows.Forms.Label
        Friend progressBar2 As ProgressBar
        Friend progressLabel2 As System.Windows.Forms.Label

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

            'MsgBox("Hoera, we zitten in de DLL (juiste versie)")

            ' Set dot as decimal point:
            Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture

            ' Initializeren classes:
            GISData = New clsGISData(Me)            'bevat alle shapefiles en rasters!
            SOBEKData = New clsSOBEK(Me)            'bevat alle SOBEK-modellen en -cases
            DIMRData = New clsDIMR(Me)              'bevat een DIMR model/case
            KNMIData = New clsKNMIData(Me)          'bevat alle data van het KNMI

            WWTPs = New clsWWTPs(Me)                        'een klasse met AWZI's en hun eigenschappen
            BoundaryObjects = New clsBoundaries(Me)         'een klasse met boundaries en hun eigenschappen
            WWTPDischargePoints = New clsWWTPDischargePoints(Me)
            SmoothSavGoy = New clsSavitzkyGolaySmoothing

            Settings = New clsSettings()      'de algemene instellingen
            ExcelFile = New clsExcelBook(Me)
            GeneralFunctions = New GeneralFunctions(Me)  'algemene functies

            'now initialize our list of HydroMathOperations
            InitializeHydroMathOperations()

        End Sub

        Public Sub InitializeHydroMathOperations()
            HydroMathOperations = New Dictionary(Of String, clsHydroMathOperation)
            HydroMathOperations.Add("MIN", New clsHydroMathOperation(Me, "min", 2))             'minimum of a and b
            HydroMathOperations.Add("MAX", New clsHydroMathOperation(Me, "max", 2))             'maximum of a and b
            HydroMathOperations.Add("AVG", New clsHydroMathOperation(Me, "avg", 2))             'average of a and b
            HydroMathOperations.Add("SUM", New clsHydroMathOperation(Me, "sum", 2))             'sum of a and b
            HydroMathOperations.Add("DIF", New clsHydroMathOperation(Me, "dif", 2))             'difference between a and b
            HydroMathOperations.Add("ABS", New clsHydroMathOperation(Me, "abs", 1))             'absolute value of a
            HydroMathOperations.Add("DTM", New clsHydroMathOperation(Me, "dtm", 2))             'retrieving values from a digital terrain model
            HydroMathOperations.Add("IF", New clsHydroMathOperation(Me, "if", 3))               'logical operation
        End Sub

        Public Function Access2Sqlite(AccessPath As String, SQLitePath As String) As Boolean
            Try
                'v1.74: introduced this new function to convert 'old' Access databases for Channel Builder, Catchment Builder, Sobek Utilities, Tijdreekstool and Stochastentool
                'to SQLite.

                'create the file if it does not exist yet
                If Not System.IO.File.Exists(SQLitePath) Then
                    SQLiteConnection.CreateFile(SQLitePath)
                End If

                Dim AccessCon As New OleDb.OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & AccessPath & ";Persist Security Info=False;")
                Dim SQLiteCon As New SQLite.SQLiteConnection("Data Source=" & SQLitePath & ";Version=3;")

                'get a list of all user tables in the access database
                Dim TablesList As List(Of String) = GeneralFunctions.MDBGetTables(AccessCon)
                Dim IndexTable As New DataTable  'creates a list of all indexed columns in the database

                AccessCon.Open()
                IndexTable = AccessCon.GetSchema("Indexes") 'row("table_name") returns table name, row("index_name") returns index name and row("column_name") returns column name

                'now walk trough all tables and retrieve their columns
                Dim iTable As Integer = 0, nTable As Integer = TablesList.Count
                GeneralFunctions.UpdateProgressBar("Converting tables...", iTable, nTable, True)
                For Each myTable As String In TablesList

                    GeneralFunctions.UpdateProgressBar("Converting table " & myTable, iTable, nTable, True)
                    Dim SQLiteDataTypes As New Dictionary(Of String, GeneralFunctions.enmSQLiteDataType) 'use column name as key

                    'create the table in SQLite if it doesn't already exist
                    If Not GeneralFunctions.SQLiteTableExists(SQLiteCon, myTable) Then GeneralFunctions.SQLiteCreateTable(SQLiteCon, myTable)

                    'get a list of all columns 
                    Dim Fields As DataTable = GeneralFunctions.MDBGetColumns(AccessCon, myTable)

                    For Each row As DataRow In Fields.Rows
                        'cast the data type from the schema to the access mdb datatype
                        Dim ColName As String = row("column_name")
                        Dim DataType As String = row("data_type")  'we need to know the data type

                        'figure out if the current column has an index
                        Dim IndexName As String = ""
                        For Each idxRow As DataRow In IndexTable.Rows
                            If idxRow("table_name").ToString.Trim.ToUpper = myTable.Trim.ToUpper Then
                                If idxRow("column_name").ToString.Trim.ToUpper = ColName.Trim.ToUpper Then
                                    If idxRow("index_name").ToString.Trim <> "" Then
                                        'we create a unique new index name ourselves. So don't reuse the Index_name from the oledb
                                        IndexName = myTable & "_" & ColName
                                    End If
                                End If
                            End If
                        Next

                        'https://docs.microsoft.com/en-us/dotnet/api/system.data.oledb.oledbtype?view=dotnet-plat-ext-3.1#:~:text=A%20special%20data%20type%20that,This%20maps%20to%20Object.
                        Dim OleDbDataType As OleDb.OleDbType = DirectCast([Enum].Parse(GetType(OleDb.OleDbType), row("data_type")), OleDb.OleDbType) 'cast the string we get to its corresponding enum type

                        'convert the datatype to SQLite and add this field to the SQLite table if it doesn't already exist
                        Dim SQLiteDataType As GeneralFunctions.enmSQLiteDataType = GeneralFunctions.SqliteDataTypeFromOleDB(OleDbDataType)
                        If Not GeneralFunctions.SQLiteColumnExists(SQLiteCon, myTable, ColName) Then
                            GeneralFunctions.SQLiteCreateColumn(SQLiteCon, myTable, ColName, SQLiteDataType, IndexName)
                        End If
                        SQLiteDataTypes.Add(ColName.Trim.ToUpper, SQLiteDataType)
                        Log.AddMessage("Column " & ColName & " converted from OleDb to SQLite: from data type " & OleDbDataType.ToString & " to " & SQLiteDataType.ToString)
                    Next

                    'clear our new table before populating it; just in case it isn't empty. Otherwise we get in trouble with double instances and non unique id's
                    GeneralFunctions.SQLiteNoQuery(SQLiteCon, "DELETE FROM " & myTable & ";", False)


                    'now that our table has been created we can start populating it
                    'however to spare memory, limit ourselves to blocks of 10000 records
                    'so first figure out what the lowest and highest ID is
                    Dim nr As New DataTable
                    Dim startId As Long, endId As Long, maxRecords As Long
                    Dim blockNumber As Integer
                    Dim TableContainsData As Boolean
                    Dim TableContainsId As Boolean
                    If GeneralFunctions.MDBColumnExists(AccessCon, myTable, "Id") Then
                        TableContainsId = True
                        If GeneralFunctions.MDBQuery(AccessCon, "SELECT MIN(Id), MAX(Id) FROM " & myTable & ";", nr) Then
                            If IsDBNull(nr.Rows(0)(0)) OrElse IsDBNull(nr.Rows(0)(1)) Then
                                TableContainsData = False
                            Else
                                startId = nr.Rows(0)(0)
                                endId = nr.Rows(0)(1)
                                maxRecords = endId - startId + 1
                                TableContainsData = True
                            End If
                        End If
                    Else
                        'no column Id found.
                        TableContainsId = False
                        If GeneralFunctions.MDBQuery(AccessCon, "SELECT COUNT(*) FROM " & myTable & ";", nr) Then
                            If IsDBNull(nr.Rows(0)(0)) Then
                                TableContainsData = False
                            Else
                                maxRecords = nr.Rows(0)(0)
                                TableContainsData = True
                            End If
                        End If
                    End If

                    Dim FromBlockNumber As Integer = 1
                    Dim ToBlockNumber As Integer
                    Dim BlockSize As Integer = 100000

                    'if the table contains an Id field we can use it to copy its data block by block. Else we will copy the entire table at once
                    If TableContainsId Then ToBlockNumber = GeneralFunctions.RoundUD(maxRecords / BlockSize, 0, True) Else ToBlockNumber = 1

                    'if the table contains data, copy it to our new database
                    If TableContainsData Then
                        GeneralFunctions.UpdateProgressBar("Converting table " & myTable, 0, 10, True)

                        'walk through our database in blocks
                        For blockNumber = FromBlockNumber To ToBlockNumber
                            GeneralFunctions.UpdateProgressBar("Converting table " & myTable & ": block " & blockNumber & " of " & ToBlockNumber & ".", blockNumber - FromBlockNumber, (ToBlockNumber - FromBlockNumber) + 1, True)
                            Dim StartRecord As Long = startId + (blockNumber - 1) * BlockSize

                            Dim dt As New DataTable
                            Dim query As String
                            If TableContainsId Then
                                'table contains an Id field. Use it to copy the table content block by block
                                query = "SELECT * FROM " & myTable & " WHERE Id >= " & startId + (blockNumber - 1) * BlockSize & " AND Id < " & startId + (blockNumber) * BlockSize & ";"
                            Else
                                'table does not contain an Id-column. So copy everything at once
                                query = "SELECT * FROM " & myTable & ";"
                            End If

                            If GeneralFunctions.MDBQuery(AccessCon, query, dt, False) Then

                                'then insert the data. do this via a bulk insert
                                Using cmd As New SQLite.SQLiteCommand
                                    cmd.Connection = SQLiteCon
                                    Using transaction = SQLiteCon.BeginTransaction
                                        For i = 0 To dt.Rows.Count - 1
                                            GeneralFunctions.UpdateProgressBar("", i, dt.Rows.Count)
                                            Dim cmd1 As String, cmd2 As String
                                            cmd1 = "INSERT INTO " & myTable & " ("
                                            cmd2 = " VALUES ("
                                            For j = 0 To dt.Columns.Count - 1
                                                If SQLiteDataTypes.Item(dt.Columns(j).ColumnName.Trim.ToUpper) = GeneralFunctions.enmSQLiteDataType.SQLITETEXT Then
                                                    If Not IsDBNull(dt.Rows(i)(j)) Then
                                                        cmd1 &= dt.Columns(j).ColumnName & ","
                                                        If dt.Columns(j).DataType.Name.ToString = "DateTime" Then
                                                            cmd2 &= "'" & Format(dt.Rows(i)(j), "yyyy-MM-dd HH:mm:ss") & "'," 'text needs to be surrounded by single quotes
                                                        Else
                                                            cmd2 &= "'" & dt.Rows(i)(j) & "'," 'text needs to be surrounded by single quotes
                                                        End If
                                                    End If
                                                Else
                                                    If Not IsDBNull(dt.Rows(i)(j)) Then
                                                        cmd1 &= dt.Columns(j).ColumnName & ","
                                                        cmd2 &= dt.Rows(i)(j) & ","
                                                    End If
                                                End If
                                            Next
                                            cmd1 = Left(cmd1, cmd1.Length - 1) & ")"
                                            cmd2 = Left(cmd2, cmd2.Length - 1) & ");"

                                            cmd.CommandText = cmd1 & cmd2
                                            'cmd.CommandText = "INSERT INTO MCTABLES (MCID, ELEVATION, AREA) VALUES ('" & id & "'," & ElevationTable.XValues.Values(i) & "," & ElevationTable.Values1.Values(i) & ");"
                                            cmd.ExecuteNonQuery()
                                        Next
                                        transaction.Commit() 'this is where the bulk insert is finally executed.
                                    End Using
                                End Using
                            End If
                        Next
                    End If


                Next

                AccessCon.Close()
                SQLiteCon.Close()
                GeneralFunctions.UpdateProgressBar("Database conversion complete.", 0, 10, True)

                Return True
            Catch ex As Exception
                Me.Log.AddError("Error converting Access database to SQLite: " & ex.Message)
                Return False
            End Try
        End Function
        Public Function SetDatabaseConnection(ByVal DatabasePath As String) As Boolean
            'v1.83: changed this into a function with exception handling
            Try
                Database = DatabasePath
                If Not System.IO.File.Exists(DatabasePath) Then Throw New Exception("Error: database file not found: " & DatabasePath)
                If Right(DatabasePath, 3).Trim.ToUpper = "MDB" Then
                    Throw New Exception("Error: databases of type Access 2003 (.mdb) are no longer supported. Use a SQLite database (.db) instead.")
                ElseIf Right(DatabasePath, 2).Trim.ToUpper = "DB" Then
                    If SqliteCon.State = ConnectionState.Open Then SqliteCon.Close()
                    SqliteCon.ConnectionString = "Data Source=" & Database & ";Version=3;"
                    Log.AddMessage("Connectionstring to database has successfully been set: " & SqliteCon.ConnectionString)
                Else
                    Throw New Exception("Error: database type not recognized. Only Sqlite (.db) is supported.")
                End If
                Return True
            Catch ex As Exception
                Log.AddError(ex.Message)
                Return False
            End Try

        End Function

        Public Sub InitializeIDFs()
            IDFs = New clsIDFs(Me)
        End Sub

        Public Function GetExportDirFlowData(TargetModel As GeneralFunctions.enmSimulationModel) As String
            Select Case TargetModel
                Case GeneralFunctions.enmSimulationModel.SOBEK
                    Return Me.Settings.ExportDirSobekFlow
                Case GeneralFunctions.enmSimulationModel.DHYDRO
                    Return Me.Settings.ExportDirHydroFlow
            End Select
            Return String.Empty
        End Function

        Public Function GetExportDirRRData(TargetModel As GeneralFunctions.enmSimulationModel) As String
            Select Case TargetModel
                Case GeneralFunctions.enmSimulationModel.SOBEK
                    Return Me.Settings.ExportDirSobekRR
                Case GeneralFunctions.enmSimulationModel.DHYDRO
                    Return Me.Settings.ExportDirDHydroRR
            End Select
            Return String.Empty
        End Function

        Public Function GetExportDirTopo(TargetModel As GeneralFunctions.enmSimulationModel) As String
            Select Case TargetModel
                Case GeneralFunctions.enmSimulationModel.SOBEK
                    Return Me.Settings.ExportDirSobekTopo
                Case GeneralFunctions.enmSimulationModel.DHYDRO
                    Return Me.Settings.ExportDirHydroFlow
            End Select
            Return String.Empty
        End Function

        Public Function GetExportDirRoot(TargetModel As GeneralFunctions.enmSimulationModel) As String
            Select Case TargetModel
                Case GeneralFunctions.enmSimulationModel.SOBEK
                    Return Me.Settings.ExportDirSOBEK
                Case GeneralFunctions.enmSimulationModel.DHYDRO
                    Return Me.Settings.ExportDirDHydro
            End Select
            Return String.Empty
        End Function

        Public Function GetExportDirGIS() As String
            Return Me.Settings.ExportDirGIS
        End Function

        Public Sub setDatasetInformation(YearsAnalyzed As Integer, nEventsFit As Integer)
            ExtremeValuesStatistics.nYearsObserved = YearsAnalyzed
            ExtremeValuesStatistics.nEventsFit = nEventsFit
        End Sub

        Public Sub initializeExtremeValuesStatistics()
            ExtremeValuesStatistics = New clsExtremeValuesStatistics(Me)
        End Sub

        Public Sub InitializeStochasten()
            StochastenAnalyse = New clsStochastenAnalyse(Me)
        End Sub

        Public Sub InitializeTijdreeksen()
            TijdreeksAnalyse = New clsTijdreeksAnalyse(Me)
        End Sub

        Public Sub InitializeTijdreeksStatistiek()
            TijdreeksStatistiek = New clsTijdreeksStatistiek(Me)
        End Sub

        Public Sub InitializeGridEditor()
            GridEditor = New clsGridEditor(Me)
        End Sub

        Public Sub InitializeReachObjects()
            BoundaryObjects = New clsBoundaries(Me)
        End Sub

        Public Sub InitGisData()
            GISData = New clsGISData(Me)
        End Sub


        Public Sub InitSobekModel(ByVal cfData As Boolean, ByVal rrData As Boolean)
            If cfData Then
                Me.Settings.CFSettings.Initialize()
            End If

            If rrData Then
                Me.Settings.RRSettings.Initialize()
            End If
        End Sub

        Public Sub SetFieldsAreaShapefile(ByVal subcatchmentIDFieldIdx As Integer, ByVal catchmentIdx As Integer, ByVal strOutFieldIdx As Integer, Optional ByVal afvCoefFieldIdx As Integer = 0)
            Me.GISData.SubcatchmentDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.ID).ColIdx = subcatchmentIDFieldIdx
            Me.GISData.SubcatchmentDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.ParentID).ColIdx = catchmentIdx
            Me.GISData.SubcatchmentDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.StructureOutID).ColIdx = strOutFieldIdx
            Me.GISData.SubcatchmentDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.DischargeCoefficient).ColIdx = afvCoefFieldIdx
        End Sub

        Public Sub SetFieldsBODShapefile(ByVal BODEMCODEFieldIdx As Integer)
            Me.GISData.SoilShapefile.BodemCodeFieldIdx = BODEMCODEFieldIdx
        End Sub
        Public Function getDirFromPath(ByVal myPath As String) As String
            Return Me.GeneralFunctions.GetDirFromPath(myPath)
        End Function

        Public Sub SetFieldsLGNShapefile(ByVal landuseFieldIdx As Integer)
            Me.GISData.LanduseShapeFile.LandUseFieldIdx = landuseFieldIdx
        End Sub

        Public Sub setFieldsSewageAreaShapeFile(ByVal myAreaIDField As String)
            Me.GISData.SewageAreaDataSource.SetField(GeneralFunctions.enmInternalVariable.ID, myAreaIDField)
        End Sub


        Public Sub PassAreaShape(ByVal sfArea As MapWinGIS.Shapefile)
            Me.GISData.SubcatchmentDataSource.SetShapefileByPath(sfArea.Filename)
            Me.GISData.SubcatchmentDataSource.setshapefile(sfArea)
        End Sub

        Public Function BuiFromIDFCurve(BuiPath As String, StartDate As Date, TimestepMinutes As Integer, AddTimesteps As Integer, CumulativesMM As List(Of Double)) As Boolean
            Try
                Dim Bui As New clsBuiFile(Me)
                Dim ms As New clsMeteoStation(Me)
                ms.Name = "Precipitation"
                Bui.GetAddMeteoStation("P", "P")

                'set the timestep etc.
                Bui.StartDate = StartDate
                Bui.TimeStep = New TimeSpan(0, TimestepMinutes, 0)
                Bui.EndDate = Bui.StartDate.AddMinutes((CumulativesMM.Count + AddTimesteps) * TimestepMinutes)
                Bui.TotalSpan = Bui.EndDate.Subtract(Bui.StartDate)

                'first sort the cumulatives in ascending order
                CumulativesMM.Sort()

                'convert the cumulatives to volumes per timestep
                Dim VolumesMM As New List(Of Double)
                Dim lastVal As Double = 0
                For i = 0 To CumulativesMM.Count - 1
                    VolumesMM.Add(CumulativesMM(i) - lastVal)
                    lastVal = CumulativesMM(i)
                Next

                'now sort the volumes in ascending order
                VolumesMM.Sort()

                'finally we need to centralize the peak and then jump back and forth over the peak to place each next value in descending order
                'like so:
                '      --
                '    --  
                '        --
                '  --      
                '          --    
                '--          --

                Dim VolumesOrdered As New Dictionary(Of Integer, Double)
                Dim Idx As Integer
                For i = VolumesMM.Count - 1 To 0 Step -1
                    If i = VolumesMM.Count - 1 Then
                        Idx = 0
                        VolumesOrdered.Add(Idx, VolumesMM(i))
                    ElseIf Idx <= 0 Then
                        Idx = -(Idx - 1)
                        VolumesOrdered.Add(Idx, VolumesMM(i))
                    ElseIf Idx > 0 Then
                        Idx = -(Idx + 1)
                        VolumesOrdered.Add(Idx, VolumesMM(i))
                    End If
                Next

                Dim Values(VolumesOrdered.Count - 1 + AddTimesteps, 0) As Single   'dim a 2D array for our precipitation volumes

                'finally sort the dictionary by ascending key and write to a values array
                ' Get list of keys and sort them
                Dim keys As List(Of Integer) = VolumesOrdered.Keys.ToList
                keys.Sort()
                For i = 0 To keys.Count - 1
                    Values(i, 0) = VolumesOrdered.Item(keys(i))
                Next

                Bui.Values = Values
                Bui.Write(BuiPath)
                Return True
            Catch ex As Exception
                Log.AddError(ex.Message)
                Log.AddError("Error in function BuiFromIDFCurve.")
                Return False
            End Try
        End Function

        Public Sub passSewageAreaShape(ByVal sfSewageArea As MapWinGIS.Shapefile)
            Me.GISData.SewageAreaDataSource.Shapefile = sfSewageArea
        End Sub

        Public Sub PassCatchmentShape(ByVal sfCatchment As MapWinGIS.Shapefile)
            Me.GISData.CatchmentDataSource.Shapefile.sf = sfCatchment
        End Sub
        Public Sub PassLGNShape(ByVal sfLGN As MapWinGIS.Shapefile)
            Me.GISData.LanduseShapeFile.sf = sfLGN
        End Sub
        Public Sub PassSubCatchmentShape(ByVal sfSub As MapWinGIS.Shapefile)
            Me.GISData.SubcatchmentDataSource.SetShapefile(sfSub)
        End Sub
        Public Sub PassElevationGrid(ByVal grdElevation As MapWinGIS.Grid)
            Me.GISData.ElevationGrid.Path = grdElevation.Filename
            Me.GISData.ElevationGrid.Grid = grdElevation
            Me.GISData.ElevationGrid.CompleteMetaHeader()
        End Sub
        Public Sub PassLandUseGrid(ByVal grdLandUse As MapWinGIS.Grid)
            Me.GISData.LandUseGrid.Path = grdLandUse.Filename
            Me.GISData.LandUseGrid.Grid = grdLandUse
            Me.GISData.LandUseGrid.CompleteMetaHeader()
        End Sub
        Public Function ReadCatchmentsFromDataSource() As Boolean
            Try
                If Not Me.GISData.ReadCatchmentsFromDataSource() Then Throw New Exception("Error reading catchments from shapefile")
                Return True
            Catch ex As Exception
                Log.AddError(ex.Message)
                Return False
            End Try
        End Function
        Public Sub clipAreasFromCatchmentShapeFile()
            If Not Me.GISData.clipAreasFromCatchmentShapeFile Then Me.Log.AddError("Error clipping areas from catchment shapefile")
        End Sub
        Public Function readAreasFromShapeFile(Optional ByVal minArea As Double = 100, Optional ByVal maxNum As Integer = 0) As Boolean
            'leest de shapefile met polygonen in (Gebiedenshape) en stelt daaruit Areas samen (clsArea)
            If GISData.ReadSubcatchmentsShapeFile(minArea, maxNum) Then
                Return False
            Else
                Return False
            End If
        End Function


        Public Sub MatchSbkStructuresWithChannelBed(ByVal Culverts As Boolean, ByVal Bridges As Boolean)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.MatchElevationWithChannelBed(False, True)
        End Sub
        Public Sub BufferSobekReaches()
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.WriteBufferShapeFile2()
        End Sub
        Public Sub ExportProfilesPolylineShapefile(path As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.WriteCrossSectionsPolylineShapefile(path)
        End Sub
        Public Sub findSOBEKSnapLocations()
            'finds the snap locations for each area in Areas to the active case in sobek
            SOBEKData.ActiveProject.ActiveCase.CFTopo.SnapLateralsFromAreas()
        End Sub
        Public Sub writeLateralDataFromSobekRRResults(ExportDir As String)
            If Not SOBEKData.ActiveProject.ActiveCase.WriteLateralDataFromRRInflow(ExportDir) Then
                Log.AddError("Error writing lateral data from SOBEK-RR results")
            End If
        End Sub
        Public Sub readQLat()
            'leest QLAT.HIS uit de sobekresultaten
            Log.AddError("De functie readQLat is in onbruik geraakt.")
            'Call SOBEKData.ActiveProject.ActiveCase.CFData.Results.QLat.ReadAll()
        End Sub
        Public Sub BuildCalculationGrid(ByVal ApplyAttributeLength As Boolean, ByVal OverAll As Double, ByVal MergeDist As Double, ByVal Culverts As Double, ByVal Pumps As Double, ByVal Weirs As Double, ByVal Orifices As Double, ByVal Bridges As Double, ByVal OtherStructures As Double, ByVal QBounds As Double, ByVal HBounds As Double, Optional ByVal ReplaceCPs As Boolean = True, Optional ByVal replaceFixedCPs As Boolean = False, Optional ByVal Prefix As String = "fxcp", Optional ByVal AddIndexNumberToID As Boolean = True, Optional ByVal AddChainageToID As Boolean = False, Optional ByVal OptimizeReachObjectLocations As Boolean = True, Optional ByVal SkipReachesWithPrefix As String = "")
            Call SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.BuildCalculationGrid(ApplyAttributeLength, OverAll, MergeDist, Culverts, Pumps, Weirs, Orifices, Bridges, OtherStructures, QBounds, HBounds, ReplaceCPs, replaceFixedCPs, Prefix, AddIndexNumberToID, AddChainageToID,, SkipReachesWithPrefix)
        End Sub
        Public Sub ExportCalculationGrid()
            Call SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.ExportCalculationGrid()
        End Sub
        Public Sub SbkBuildElevationProfilesFromGrid(ByVal Distance As Integer, ByVal Profiles As Boolean, ByVal CalculationPoints As Boolean)
            'builds an elevation profile for each reach object from the elevation grid
            Call SOBEKData.ActiveProject.ActiveCase.CFTopo.BuildElevationProfilesFromElevationGrid(Distance, Profiles, CalculationPoints)
        End Sub

        Public Function AreaIDsUnique() As Boolean
            If GISData.SubcatchmentDataSource.IDsUnique() Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub MergeShapesByCatchment()
            If Me.GISData.SubcatchmentDataSource.Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ParentID) Then
                Me.GISData.SubcatchmentDataSource.CreateMergedShapePerCatchment(Me.GISData.Catchments)
            End If
        End Sub
        Public Function CleanUpActiveSobekCaseFlow(ByVal ProfDat As Boolean, ByVal ProfDef As Boolean, ByVal StructDat As Boolean, ByVal StructDef As Boolean, ByVal ControlDef As Boolean, ByVal BoundDat As Boolean, ByVal LatDat As Boolean, ByVal FrictDat As Boolean, ByVal NodesDat As Boolean) As Boolean
            Return Me.SOBEKData.ActiveProject.ActiveCase.CFData.CleanUp(ProfDat, ProfDef, StructDat, StructDef, ControlDef, BoundDat, LatDat, FrictDat, NodesDat)
        End Function
        Public Sub CleanUpActiveSobekCaseRR(ByVal Unpaved As Boolean, ByVal Draindef As Boolean)
            Me.SOBEKData.ActiveProject.ActiveCase.RRData.CleanUp(Unpaved, Draindef)
        End Sub
        Public Sub SetSobekSurfaceLevelToLowestEmbankments(ByVal YZ As Boolean)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.setSurfaceLevelToLowestEmbankment(YZ)
        End Sub
        Public Sub shiftObjectsByElevationgrid(ByVal Profiles As Boolean, ByVal Culverts As Boolean, ByVal Bridges As Boolean, ByVal Orifices As Boolean, ByVal UniWeirs As Boolean, ByVal Unpaved As Boolean, ByVal Paved As Boolean)
            If Profiles Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.shiftByElevationGrid()
            If Culverts Or Bridges Or Orifices Or UniWeirs Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.shiftByElevationGrid(Culverts, Bridges, Orifices, UniWeirs)
            If Unpaved Or Paved Then Me.SOBEKData.ActiveProject.ActiveCase.RRData.shiftByElevationGrid(Unpaved, Paved)
        End Sub
        Public Sub fixNonAscendingProfileData()
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDefRecords.FixNonAscending()
        End Sub
        Public Sub ReadSobekDataDetail(ByVal ProfDat As Boolean, ByVal ProfDef As Boolean, ByVal StructDat As Boolean, ByVal StructDef As Boolean, ByVal ControlDef As Boolean, ByVal BoundDat As Boolean, ByVal LatDat As Boolean, ByVal BoundLat As Boolean, ByVal FrictionDat As Boolean, ByVal InitDat As Boolean, ByVal NodesDat As Boolean)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ImportByFile(ProfDat, ProfDef, StructDat, StructDef, ControlDef, BoundDat, LatDat, BoundLat, FrictionDat, InitDat, NodesDat)
        End Sub
        Public Sub SetActiveCaseInUse()
            Me.SOBEKData.ActiveProject.ActiveCase.InUse = True
        End Sub
        Public Function ReadActiveCase(ByVal ReadRRNetwork As Boolean, ByVal ReadCFNetwork As Boolean, ByVal ReadRRData As Boolean, ByVal ReadCFData As Boolean, ByVal ReadWQTopo As Boolean, StructureReachPrefix As String)
            Me.SOBEKData.ActiveProject.ActiveCase.Initialize()
            Return Me.SOBEKData.ActiveProject.ActiveCase.Read(ReadRRNetwork, ReadCFNetwork, ReadRRData, ReadCFData, ReadWQTopo, StructureReachPrefix)
        End Function
        Public Function ReadActiveCaseComputationalGrid() As Boolean
            Return Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReadCalculationPointsByReach()
        End Function

        Public Sub RemoveActiveCaseReachesByPrefix(ByVal Prefix As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.RemoveReachesByPrefix(Prefix)
        End Sub
        Public Sub ReadActiveCaseResultsStats(ByVal WaterLevels As Boolean, ByVal WaterDepths As Boolean, ByVal Tides As Boolean)
            If WaterLevels Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Results.CalcPnt.CalcStats("waterlevel", True, Tides)
            If WaterDepths Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Results.CalcPnt.CalcStats("waterdepth", True, False)
        End Sub
        Public Function SbkBuildInitialDataFromGeodatasource(ByRef Geodatasource As clsGeoDatasource, TargetLevelType As GeneralFunctions.enmTargetLevelType, ByVal minDepth As Double, addDepth As Double, removeExisting As Boolean) As Boolean
            Try
                'old argument removed since it is part of geodatasource: , ByRef TargetLevelFieldsCollection As List(Of clsTargetLevelFields)
                If removeExisting Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.InitialData.InitialDatFLINRecords.records.Clear()
                If Not Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReachNodes.AssignTargetLevelsFromGeodataSource(Geodatasource) Then Throw New Exception("Error assigning target levels from shapefile.")
                If Not Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.BuildInitialDatRecordsFromTargetLevels(TargetLevelType, minDepth, addDepth) Then Throw New Exception("Error building initial.dat record from target levels.")
                Return True
            Catch ex As Exception
                Log.AddError(ex.Message)
                Log.AddError("Error in function SbkBuildInitialDataFromShapeFile of class clsSetup.")
                Return False
            End Try
        End Function
        Public Sub SbkWriteRRBoundData(ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.RRData.WriteBoundaryData(False, ExportDir)
        End Sub
        Public Sub SbkBuildRRBoundData(ByVal UseAreaShapeFile As Boolean, Optional ByVal minDepth As Double = 0)
            Me.SOBEKData.ActiveProject.ActiveCase.RRData.BuildRRCFBoundaryData(UseAreaShapeFile, minDepth)
        End Sub

        Public Sub SobekCompare()
            'voert een vergelijking tussen sobekmodellen uit en schrijft het resultaat naar Excel
            'we gaan ervan uit dat de activemodel het referentiemodel is
            Me.SOBEKData.CompareModels()
        End Sub

        Public Sub SetImportDir(ByVal myImportDir As String)
            Me.importDir = myImportDir
        End Sub
        Public Sub TruncateProfiles(ByVal myOption As Integer, ByVal WidthFieldIdx As Integer, ByVal ConstWidth As Integer, ByVal ToDist As Double, ByVal FromDist As Double, ByVal Tabulated As Boolean, ByVal Trapezium As Boolean, ByVal OpenCircle As Boolean, ByVal Sedredge As Boolean, ByVal ClosedCircle As Boolean, ByVal Type5 As Boolean, ByVal EggShape As Boolean, ByVal EggShape2 As Boolean, ByVal Rectangle As Boolean, ByVal Type9 As Boolean, ByVal YZ As Boolean, ByVal AsymTrap As Boolean)
            'dwarsprofielen in de actieve sobekcase afkappen aan de hand van waarden in de area shapefile
            Call Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.Truncate(myOption, WidthFieldIdx, ConstWidth, ToDist, FromDist, False, False, False, False, False, False, False, False, False, False, True, False)
        End Sub

        Public Sub TruncateProfilesByEmbankment(ByVal Tabulated As Boolean, ByVal YZ As Boolean)
            Call Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.TruncateByEmbankment(Tabulated, YZ)
        End Sub

        Public Sub TruncateProfilesByFixedIncrement(Increment As Double, ByVal Tabulated As Boolean, ByVal YZ As Boolean)
            Call Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.TruncateByFixedIncrement(Increment, Tabulated, YZ)
        End Sub

        Public Sub WriteProfileDat(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDatRecords.Write(Append, ExportDir)
        End Sub
        Public Sub WriteProfileDef(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDefRecords.Write(Append, ExportDir)
        End Sub
        Public Sub WriteControlDef(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.ControlDefRecords.Write(Append, ExportDir)
        End Sub
        Public Sub WriteStructDef(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Write(Append, ExportDir)
        End Sub
        Public Sub WriteStructDat(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Write(Append, ExportDir)
        End Sub
        Public Sub writeUnpaved3B(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Write(Append, ExportDir)
        End Sub
        Public Sub writepaved3B(ByVal Append As Boolean, ExportDir As String)
            Me.SOBEKData.ActiveProject.ActiveCase.RRData.Paved3B.Write(Append, ExportDir)
        End Sub
        Public Sub addError(ByVal myError As String)
            Me.Log.AddError(myError)
        End Sub
        Public Sub addWarning(ByVal myWarning As String)
            Me.Log.AddWarning(myWarning)
        End Sub
        Public Sub ExportSBK2ModHMS()
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.WriteMODHMS()
        End Sub
        Public Sub DisableControllers(Weirs As Boolean)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.DisableControllers(Weirs)
        End Sub
        Public Sub SetProgress(ByRef myProgressBar As ProgressBar, ByRef myProgressLabel As System.Windows.Forms.Label)
            Me.progressBar = myProgressBar
            Me.progressLabel = myProgressLabel
        End Sub

        Public Sub SetProgress2(ByRef myProgressBar As ProgressBar, ByRef myProgressLabel As System.Windows.Forms.Label)
            Me.progressBar2 = myProgressBar
            Me.progressLabel2 = myProgressLabel
        End Sub

        Public Sub setSbkLateralPrefix(ByVal LateralPrefix As String)
            Me.Settings.CFSettings.setLateralPrefix(LateralPrefix)
        End Sub
        Public Function getSbkLateralPrefix() As String
            Return Me.Settings.CFSettings.getLateralPrefix
        End Function

        Public Function getCatchment(ByVal myID As String) As clsCatchment
            If GISData.Catchments.Catchments.ContainsKey(myID.Trim.ToUpper) Then
                Return GISData.Catchments.Catchments.Item(myID.Trim.ToUpper)
            Else
                Return Nothing
            End If
        End Function
        Public Function getAddCatchment(ByVal myID As String) As clsCatchment
            Dim myCatchment As clsCatchment
            If GISData.Catchments.Catchments.ContainsKey(myID.Trim.ToUpper) Then
                Return GISData.Catchments.Catchments.Item(myID.Trim.ToUpper)
            Else
                myCatchment = New clsCatchment(Me)
                myCatchment.ID = myID
                GISData.Catchments.Catchments.Add(myCatchment.ID.Trim.ToUpper, myCatchment)
                Return GISData.Catchments.Catchments.Item(myID.Trim.ToUpper)
            End If
        End Function

        Public Function GetAddChannelCategory(ByVal myID As String, ByVal myChannelCat As clsChannelCategory) As clsChannelCategory
            If GISData.ChannelCategories.ContainsKey(myID.Trim.ToUpper) Then
                Return GISData.ChannelCategories.Item(myID.Trim.ToUpper)
            Else
                GISData.ChannelCategories.Add(myID.Trim.ToUpper, myChannelCat)
                Return GISData.ChannelCategories.Item(myID.Trim.ToUpper)
            End If
        End Function

        Public Function GetAddChannelsAsStructureCategory(ByVal myID As String, ByVal myChannelCat As clsChannelCategory) As clsChannelCategory
            If GISData.ChannelUsageCategories.ContainsKey(myID.Trim.ToUpper) Then
                Return GISData.ChannelUsageCategories.Item(myID.Trim.ToUpper)
            Else
                GISData.ChannelUsageCategories.Add(myID.Trim.ToUpper, myChannelCat)
                Return GISData.ChannelUsageCategories.Item(myID.Trim.ToUpper)
            End If
        End Function

        Public Function GetAddWWTP(ByVal myID As String) As clsWWTP
            Dim myWWTP As clsWWTP
            If WWTPs.WWTPs.ContainsKey(myID.Trim.ToUpper) Then
                Return WWTPs.WWTPs.Item(myID.Trim.ToUpper)
            Else
                myWWTP = New clsWWTP(Me)
                myWWTP.ID = myID
                myWWTP.InUse = True
                WWTPs.WWTPs.Add(myID.Trim.ToUpper, myWWTP)
                Return WWTPs.WWTPs.Item(myID.Trim.ToUpper)
            End If
        End Function

        Public Sub getWiderProfileDefinitionsFromOtherCases(ByVal YZ As Boolean)
            'verzamelt alle ingelezen (InUse = TRUE) cases behalve de ActiveCase
            Dim Cases As New List(Of ClsSobekCase)
            For Each myModel As clsSobekProject In SOBEKData.Projects.Values
                For Each mycase As ClsSobekCase In myModel.Cases.Values
                    If mycase.InUse And Not mycase.IsActiveCase Then
                        Cases.Add(mycase)
                    End If
                Next
            Next

            Call SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.getWidestProfileDefinitionsFromOtherCases(Cases, YZ)

        End Sub

        Public Function ReadMeteoBasePrecHourly(ByVal myPath As String) As Boolean
            KNMIData = New clsKNMIData(Me)
            Call KNMIData.readMBTextFile(myPath)
            Return True
        End Function

        Public Function ExportMeteoBasePrecHourly() As Boolean
            If KNMIData.writeMBTextFiles(Me.importDir) Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function CalcMeteoBaseHourlyStats(ByVal myArea As Double) As Boolean
            If KNMIData.calcHourlyEventStats(myArea) Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Sub getRRDataFromOtherCases(ByVal RRDrainDefs As Boolean, ByVal RRMeteo As Boolean, ByVal RRSeepDefs As Boolean, ByVal SoilType As Boolean, ByVal InitGW As Boolean, ByVal InfDefs As Boolean, ByVal UPStorDefs As Boolean, ByVal PVStorDefs As Boolean, ByVal DWADefs As Boolean, ByVal PVCaps As Boolean, ByVal PVPumpDirections As Boolean)
            'verzamelt alle ingelezen (InUse = TRUE) cases behalve de ActiveCase
            Dim Cases As New List(Of ClsSobekCase)
            For Each myModel As clsSobekProject In SOBEKData.Projects.Values
                For Each mycase As ClsSobekCase In myModel.Cases.Values
                    If mycase.InUse And Not mycase.IsActiveCase Then
                        Cases.Add(mycase)
                    End If
                Next
            Next

            Call SOBEKData.ActiveProject.ActiveCase.RRData.getUnpavedParametersFromOtherCases(Cases, RRMeteo, InitGW, SoilType)
            Call SOBEKData.ActiveProject.ActiveCase.RRData.getPavedParametersFromOtherCases(Cases, PVCaps, PVPumpDirections, RRMeteo)
            If RRDrainDefs Then Call SOBEKData.ActiveProject.ActiveCase.RRData.getDrainDefsFromOtherCases(Cases)
            If RRSeepDefs Then Call SOBEKData.ActiveProject.ActiveCase.RRData.getSeepageFromOtherCases(Cases)
            If InfDefs Then Call SOBEKData.ActiveProject.ActiveCase.RRData.getINFDefsFromOtherCases(Cases)
            If UPStorDefs Then Call SOBEKData.ActiveProject.ActiveCase.RRData.getUPStorDefsFromOtherCases(Cases)
            If PVStorDefs Then Call SOBEKData.ActiveProject.ActiveCase.RRData.getPVStorDefsFromOtherCases(Cases)
            If DWADefs Then Call SOBEKData.ActiveProject.ActiveCase.RRData.getDWADefsFromOtherCases(Cases)

        End Sub

        Public Function OptimizeReachObjectsLocations(MinimumDistanceBetweenReachObjects As Double) As Boolean
            Return Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.OptimizeAllReachObjectLocations(MinimumDistanceBetweenReachObjects)
        End Function
        Public Sub AddGroundLayerToProfiles(ByVal myValue As Double, Tabulated As Boolean, Trapezium As Boolean, OpenCircle As Boolean, Sedredge As Boolean, ClosedCircle As Boolean, EggShape As Boolean, Rectangular As Boolean, YZ As Boolean, AsymmetricalTrapezium As Boolean)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.AddGroundLayerToAll(myValue, Tabulated, Trapezium, OpenCircle, Sedredge, ClosedCircle, EggShape, Rectangular, YZ, AsymmetricalTrapezium)
        End Sub
        Public Sub AddGroundLayerToProfilesByPolygon(ByVal Shapefile As String, ByVal ValueField As String, Tabulated As Boolean, Trapezium As Boolean, OpenCircle As Boolean, Sedredge As Boolean, ClosedCircle As Boolean, EggShape As Boolean, Rectangular As Boolean, YZ As Boolean, AsymmetricalTrapezium As Boolean)
            Dim PolySF As New STOCHLIB.ClsPolyShapeFile(Me, Shapefile)
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.AddGroundLayerByPolygon(PolySF, ValueField, Tabulated, Trapezium, OpenCircle, Sedredge, ClosedCircle, EggShape, Rectangular, YZ, AsymmetricalTrapezium)
        End Sub
        Public Sub AddGroundLayerToCulverts()
            Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.AddGroundLayerToCulverts()
        End Sub
        Public Sub AddCrossSectionsToEmptyReaches()
            Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.AddCrossSectionsIfNotPresent()
        End Sub
        Public Sub AddCSOLocationsToNetwork()
            Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.AddCSOLocationsToNetwork()
        End Sub
        Public Function ExportFlowTopology(ByVal Append As Boolean, TargetModel As GeneralFunctions.enmSimulationModel) As Boolean
            Try
                Dim ExportDir As String
                Select Case TargetModel
                    Case GeneralFunctions.enmSimulationModel.SOBEK
                        ExportDir = Me.Settings.ExportDirSobekTopo
                        Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.Export(Append, ExportDir)
                    Case GeneralFunctions.enmSimulationModel.DHYDRO
                        ExportDir = Me.Settings.ExportDirDHydroRR
                End Select

                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Sub InitializeExport(ByVal RRData As Boolean, ByVal RRTopo As Boolean, ByVal CFData As Boolean, ByVal CFTopo As Boolean)
            Me.SOBEKData.InitializeExport(RRData, RRTopo, CFData, CFTopo)
        End Sub

        Public Sub InitializeExportDetail(ExportDir As String, ByVal ProfDat As Boolean, ByVal ProfDef As Boolean, ByVal StructDat As Boolean, ByVal StructDef As Boolean, ByVal FrictionDat As Boolean)
            If ProfDat Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDatRecords.InitializeExport(ExportDir)
            If ProfDef Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDefRecords.InitializeExport(ExportDir)
            If StructDat Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.InitializeExport(ExportDir)
            If StructDef Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.InitializeExport(ExportDir)
            If FrictionDat Then Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.FrictionData.InitializeExport(ExportDir)
        End Sub

        Public Function SetAddSobekProject(ByVal ProjectDir As String, ProgramsDir As String, Optional ReadCases As Boolean = True, Optional ByVal SetAsActiveProject As Boolean = True) As Boolean
            Return Me.SOBEKData.SetAddProject(ProjectDir, ProgramsDir, ReadCases, SetAsActiveProject)
        End Function

        Public Function SetDIMRProject(ByVal ProjectDir As String)
            Return Me.DIMRData.SetProject(ProjectDir)
        End Function

        Public Sub CreateSobekCase(ByVal ModelDir As String, ProgramsDir As String, ByVal CaseName As String)
            Me.SOBEKData.CreateCase(ModelDir, ProgramsDir, CaseName)
        End Sub

        Public Sub BuildSbkChannelsFromShapeFile()
            Me.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.BuildFromShapeFile()
            'Me.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.BuildFromShapeFile()
        End Sub

        Public Sub clearAllModels(ByVal SOBEK As Boolean, ByVal Wagmod As Boolean)
            If SOBEK Then Me.SOBEKData.Projects.Clear()
        End Sub

        Public Function GetActiveModelCases() As List(Of String)
            Dim cases As New List(Of String)
            For Each modelCase As ClsSobekCase In Me.SOBEKData.ActiveProject.Cases.Values
                cases.Add(modelCase.CaseName)
            Next modelCase
            Return cases
        End Function

        Public Function SetActiveProject(ByVal projectDir As String) As Boolean
            Return SOBEKData.SetActiveProject(projectDir)
        End Function

        Public Function SetActiveCase(ByVal caseName As String) As Boolean
            Return SOBEKData.ActiveProject.SetActiveCase(caseName)
        End Function

        Public Sub PopulateComboBoxSobekCases(ByRef cmb As System.Windows.Forms.ComboBox)
            cmb.Items.Clear()
            For Each mycase As ClsSobekCase In SOBEKData.ActiveProject.Cases.Values
                cmb.Items.Add(mycase.CaseName)
            Next
        End Sub

        Public Sub PopulateComboBoxSobekCases(ByVal SobekProjectDir As String, SobekProgramsDir As String, ByRef cmb As System.Windows.Forms.ComboBox)
            'in case we don't want the sobek project to be added to the list of projects, but just want to know which cases are in there
            cmb.Items.Clear()
            Dim myProject As New clsSobekProject(Me, SobekProjectDir, SobekProgramsDir, True)
            For Each mycase As ClsSobekCase In myProject.Cases.Values
                cmb.Items.Add(mycase.CaseName)
            Next
        End Sub

        Public Sub PopulateDataGridSobekCases(ByRef myGrid As DataGridView, ByVal ReferenceCaseComboBox As Boolean)
            Dim myRow As DataGridViewRow
            Dim myCell As DataGridViewComboBoxCell

            For Each myCase As STOCHLIB.ClsSobekCase In SOBEKData.ActiveProject.Cases.Values
                myGrid.Rows.Add(myCase.CaseName, False)
                myRow = myGrid.Rows.Item(myGrid.Rows.Count - 1)
                If ReferenceCaseComboBox Then
                    'populate the in-cell combo box
                    For Each iCase As STOCHLIB.ClsSobekCase In SOBEKData.ActiveProject.Cases.Values
                        myCell = myRow.Cells(2)
                        myCell.Items.Add(iCase.CaseName)
                    Next
                End If
            Next
        End Sub

        Public Sub PopulateComboBoxNumericShapeFields(ByVal ShapeFilePath As String, ByRef cmb As System.Windows.Forms.ComboBox, Optional ByVal PreSelect As String = "")
            Dim mySF As New MapWinGIS.Shapefile
            Dim myField As String, i As Integer

            'first clear the combobox
            cmb.Items.Clear()

            If mySF.Open(ShapeFilePath) Then
                For i = 0 To mySF.NumFields - 1
                    myField = mySF.Field(i).Name
                    If mySF.Field(i).Type = MapWinGIS.FieldType.DOUBLE_FIELD OrElse mySF.Field(i).Type = MapWinGIS.FieldType.INTEGER_FIELD Then
                        cmb.Items.Add(myField)
                    End If
                Next

                'preselectie op basis van eerdere selectie
                For Each myField In cmb.Items
                    If myField.Trim.ToUpper = PreSelect.Trim.ToUpper Then cmb.SelectedValue = myField
                Next
                mySF.Close()

            End If
        End Sub

        Public Sub PopulateComboBoxFromDictionary(ByRef myDict As Dictionary(Of Integer, String), ByRef cmb As ComboBox)
            Dim i As Long
            cmb.Items.Clear()
            For i = 0 To myDict.Values.Count - 1
                cmb.Items.Add(myDict.Values(i))
            Next
        End Sub
        Public Sub PopulateComboBoxFromList(ByRef myList As List(Of String), ByRef cmb As ComboBox)
            Dim i As Long
            cmb.Items.Clear()
            For i = 0 To myList.Count - 1
                cmb.Items.Add(myList(i))
            Next
        End Sub

        Public Sub WriteExcelFile(ByVal myPath As String, Optional ByVal Show As Boolean = True)
            Me.ExcelFile.Path = myPath
            Call Me.ExcelFile.Save(Show)
        End Sub

    End Class
End Namespace