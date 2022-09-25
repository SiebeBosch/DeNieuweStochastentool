Imports STOCHLIB.General
Imports System.Xml
Imports System.Windows.Forms
Imports System.IO


Public Class clsStochastenAnalyse

    'Directories
    Public ResultsDir As String     'directory for the results
    Public InputFilesDir As String  'directory where all stochastic combinations are written
    Public OutputFilesDir As String 'directory where all results files are written

    'Simulation settings
    Public MaxParallel As Integer
    Public Duration As Integer 'duration of the events in hours
    Public DurationAdd As Integer 'additional time after each simulation (hours)
    Public ResultsStartPercentage As Integer 'tells the results reader to skip the first part of the results (to avoid taking initial values into account)

    Public Models As New Dictionary(Of Integer, clsSimulationModel)
    Public Runs As New clsStochastenRuns(Setup, Me)

    'reference to the datagrids used on the form
    Public SeasonGrid As DataGridView
    Public VolumeGrids As List(Of DataGridView)
    Public PatternGrids As List(Of DataGridView)
    Public GroundwaterGrids As List(Of DataGridView)
    Public WaterLevelGrid As DataGridView
    Public WindGrid As DataGridView
    Public Extra1Grids As List(Of DataGridView)
    Public Extra2Grids As List(Of DataGridView)
    Public Extra3Grids As List(Of DataGridView)
    Public Extra4Grids As List(Of DataGridView)

    'meteo stations
    Public MeteoStations As clsMeteoStations

    'settings
    Public StochastsConfigFile As String
    Public SubcatchmentShapefile As String
    Public AllowCrashedResults As Boolean
    Public XMLFile As String
    Public KlimaatScenario As STOCHLIB.GeneralFunctions.enmKlimaatScenario
    Public VolumesAsFrequencies As Boolean = False  'TRUE refers to the old method where volumes were expressed as frequencies in stead of probabilities

    'let op: omdat het aantal klassen van stochasten binnen het ene seizoen kan afwijken tov die in een ander seizoen
    'zijn de overige stochasten ondergebracht in de klassemodule clsStochastSeason
    Public Seasons As Dictionary(Of String, clsStochasticSeasonClass)
    Public MileageCounters As New Dictionary(Of String, clsMileageCounter)  'for each season a separate mileage counter

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        Models = New Dictionary(Of Integer, clsSimulationModel)
        MeteoStations = New clsMeteoStations(Me.Setup)
        Seasons = New Dictionary(Of String, clsStochasticSeasonClass)
    End Sub

    Public Sub SetSettings(myKlimaatScenario As String, myDuration As Integer)
        KlimaatScenario = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmKlimaatScenario), myKlimaatScenario.Trim.ToUpper), GeneralFunctions.enmKlimaatScenario)
        Duration = myDuration
    End Sub

    Public Sub ClearGrids()

        If Not VolumeGrids Is Nothing Then VolumeGrids.Clear()
        If Not PatternGrids Is Nothing Then PatternGrids.Clear()
        If Not GroundwaterGrids Is Nothing Then GroundwaterGrids.Clear()
        If Not Extra1Grids Is Nothing Then Extra1Grids.Clear()
        If Not Extra2Grids Is Nothing Then Extra2Grids.Clear()
        If Not Extra3Grids Is Nothing Then Extra3Grids.Clear()
        If Not Extra4Grids Is Nothing Then Extra4Grids.Clear()
    End Sub

    Public Sub Initialize()
        Models = New Dictionary(Of Integer, clsSimulationModel)
        Runs = New clsStochastenRuns(Setup, Me)
    End Sub

    Public Function GetAddSeasonClass(ID As String, P As Double, Use As Boolean) As clsStochasticSeasonClass
        If Seasons.ContainsKey(ID.Trim.ToUpper) Then
            Return Seasons.Item(ID.Trim.ToUpper)
        Else
            Dim mySeason = New clsStochasticSeasonClass(Me.Setup)
            mySeason.Name = ID
            mySeason.P = P
            mySeason.Use = Use
            Seasons.Add(mySeason.Name.Trim.ToUpper, mySeason)
            Return mySeason
        End If
    End Function


    Public Function ClassifyGroundwaterBySeason(ByVal seizoen As STOCHLIB.GeneralFunctions.enmSeason, ByVal Duration As Integer, ByRef myCase As ClsSobekCase, ByRef myDataGrid As Windows.Forms.DataGridView, seizoensnaam As String, ByRef Dates As List(Of Date), ExportDir As String) As Boolean
        Try
            Dim mySeason As clsSeason
            Dim myDuration As clsDuration
            Dim TimeSteps As List(Of Long)
            Dim ts As Long, j As Long, k As Long, n As Long
            Dim myDate As Date, myPar As String
            Dim ws As clsExcelSheet
            Dim lPercentile As Double      'lower boundary percentile for this class
            Dim uPercentile As Double      'upper boundary percentile for this class
            Dim repPerc As Double          'percentile for the class
            Dim repVal As Double          'representative value for the class


            'carry out the POT analysis on the rainfall volumes
            If Not Setup.TijdreeksStatistiek.CalcPOTEvents(Duration, seizoen.ToString, False) Then Throw New Exception("Error executing a POT-analysis on precipitation volumes for the " & seizoen.ToString)

            'now store the starting timesteps for each rainfall event in a list
            'note: these time step indices are valid for ALL unpaved.3b records
            Dim myTijdreeks As clsRainfallSeries = Setup.TijdreeksStatistiek.NeerslagReeksen.Values(0)
            mySeason = myTijdreeks.Seasons.GetByEnum(seizoen)
            myDuration = mySeason.GetDuration(Duration)
            TimeSteps = New List(Of Long)
            TimeSteps = myDuration.POTEvents.GetStartingTimeSteps

            'read the his results for all location and the previously defined initial timesteps per event
            Dim myResult As New List(Of STOCHLIB.HisDataRow)
            Dim myResults As New List(Of List(Of STOCHLIB.HisDataRow))
            myCase.RRResults.UPFLODT.OpenFile()
            myPar = myCase.RRResults.UPFLODT.getParName("Groundw.Level")
            For ts = 0 To TimeSteps.Count - 1
                myDate = Dates(TimeSteps(ts))
                myResult = myCase.RRResults.UPFLODT.ReadTimeStep(myDate, myPar)
                myResults.Add(myResult)
            Next
            myCase.RRResults.UPFLODT.Close()

            'now walk through all rows of the classification datagridview to decide which percentile we'll use
            For Each myRow As DataGridViewRow In myDataGrid.Rows

                'determine the boundaries of the current groundwater class
                lPercentile = myRow.Cells(1).Value
                uPercentile = myRow.Cells(2).Value
                repPerc = (lPercentile + uPercentile) / 2

                n = myCase.RRData.Unpaved3B.Records.Count
                j = 0

                'now that we have the starting time index numbers for the POT-events, we can get the groundwater levels from the starting times for each event
                For Each myRecord In myCase.RRData.Unpaved3B.Records.Values

                    j += 1
                    k = -1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Groundwater classification for season " & seizoen.ToString & " and class " & myRow.Cells("colNaam").Value & ".", j, n)
                    ws = Me.Setup.ExcelFile.GetAddSheet(seizoensnaam)
                    ws.ws.Cells(0, 0).Value = "Datum"
                    ws.ws.Cells(0, j).Value = myRecord.ID

                    'get the results for this record from the lists
                    Dim GW(0 To TimeSteps.Count - 1) As Double
                    For Each myResult In myResults
                        For Each Result As STOCHLIB.HisDataRow In myResult
                            If Result.LocationName = myRecord.ID Then
                                k += 1
                                GW(k) = Result.Value 'store the results in a temporary array to allow percentile computation later
                                ws.ws.Cells(k + 1, 0).Value = Result.TimeStep
                                ws.ws.Cells(k + 1, j).Value = Result.Value
                            End If
                        Next
                    Next

                    'update the unpaved.3b record
                    repVal = Me.Setup.GeneralFunctions.Percentile(GW, repPerc)
                    myRecord.ig = 0
                    myRecord.igconst = Math.Round(myRecord.lv - repVal, 2)

                    'write the statistics to Excel
                    ws = Me.Setup.ExcelFile.GetAddSheet(seizoensnaam & ".Stats")
                    Dim r As Integer = myRow.Index * 3
                    ws.ws.Cells(0, j + 1).Value = myRecord.ID
                    ws.ws.Cells(r + 1, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 1, 1).Value = "Percentiel " & repPerc
                    ws.ws.Cells(r + 1, j + 1).Value = Math.Round(repVal, 2)
                    ws.ws.Cells(r + 2, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 2, 1).Value = "Maaiveldhoogte (m NAP)"
                    ws.ws.Cells(r + 2, j + 1).Value = myRecord.lv
                    ws.ws.Cells(r + 3, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 3, 1).Value = "Grondwaterdiepte (m)"
                    ws.ws.Cells(r + 3, j + 1).Value = Math.Round(myRecord.lv - repVal, 2)
                Next

                'set path to the adjusted unpaved.3b file and initialize writing the groundwater level values
                Dim mypath As String = ExportDir & "\" & seizoen.ToString & "_" & myRow.Cells(0).Value & "\"
                If Not System.IO.Directory.Exists(mypath) Then System.IO.Directory.CreateDirectory(mypath)
                myCase.RRData.Unpaved3B.Write(mypath & "unpaved.3b", False)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ClassifyGroundwaterBySeason of class frmClassifyGroundWater.")
            Return False
        End Try


    End Function


    Public Function PopulateModelsAndLocationsFromDB(ByRef con As SQLite.SQLiteConnection) As Boolean
        Try
            Dim myQuery As String, dtModel As New DataTable, dtLoc As New DataTable, i As Integer, j As Integer, myModel As clsSimulationModel
            Dim ModelID As Integer, myResultsFile As clsResultsFile = Nothing, myModelPar As clsResultsFileParameter, myModelLoc As clsResultsFileLocation
            Dim myModule As STOCHLIB.GeneralFunctions.enmHydroModule

            'clear all existing models from memory
            Models.Clear()

            myQuery = "SELECT * FROM SIMULATIONMODELS"
            Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtModel)
            For i = 0 To dtModel.Rows.Count - 1
                myModel = New clsSimulationModel(Me.Setup, dtModel.Rows(i)("MODELID"), dtModel.Rows(i)("MODELTYPE"), dtModel.Rows(i)("EXECUTABLE"), dtModel.Rows(i)("ARGUMENTS"), dtModel.Rows(i)("MODELDIR"), dtModel.Rows(i)("CASENAME"), dtModel.Rows(i)("TEMPWORKDIR"), dtModel.Rows(i)("RESULTSFILES_RR"), dtModel.Rows(i)("RESULTSFILES_FLOW"))
                Models.Add(myModel.Id, myModel)

                'while we're in this model, read all outputlocations
                myQuery = "SELECT * FROM OUTPUTLOCATIONS"
                Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtLoc)
                For j = 0 To dtLoc.Rows.Count - 1
                    ModelID = dtLoc.Rows(j)("MODELID")
                    If ModelID = myModel.Id Then
                        myModule = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmHydroModule), dtLoc.Rows(j)("MODULE").ToString.Trim.ToUpper), GeneralFunctions.enmHydroModule)
                        myResultsFile = myModel.ResultsFiles.GetAdd(dtLoc.Rows(j)("RESULTSFILE"), myModule)
                        myResultsFile.FullPath = ResultsDir & "\" & dtLoc.Rows(j)("RESULTSFILE")
                        myModelPar = myResultsFile.GetAddParameter(dtLoc.Rows(j)("MODELPAR"))
                        myModelLoc = myModelPar.GetAddLocation(dtLoc.Rows(j)("LOCATIEID"), dtLoc.Rows(j)("LOCATIENAAM"), Duration)
                    End If
                Next
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error populating list of simulationmodels from database.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function Build_Populate_Cases_JSON(ByRef DIMRData As clsDIMR, ZipPath As String) As String

    End Function



    Public Function WriteModelsToDB(ByRef con As SQLite.SQLiteConnection) As Boolean
        Dim myQuery As String
        Dim RRFiles As String = "", FLOWFiles As String = ""
        myQuery = "DELETE FROM SIMULATIONMODELS"
        Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)

        For Each myModel As clsSimulationModel In Models.Values

            For i = 0 To myModel.ResultsFiles.Files.Count - 1
                If myModel.ResultsFiles.Files(i).HydroModule = GeneralFunctions.enmHydroModule.RR Then
                    If RRFiles.Length > 0 Then RRFiles = RRFiles & ";"
                    RRFiles &= myModel.ResultsFiles.Files(i).FileName
                ElseIf myModel.ResultsFiles.Files(i).HydroModule = GeneralFunctions.enmHydroModule.FLOW Then
                    If FLOWFiles.Length > 0 Then FLOWFiles = FLOWFiles & ";"
                    FLOWFiles &= myModel.ResultsFiles.Files(i).FileName
                End If
            Next

            myQuery = "INSERT INTO SIMULATIONMODELS (MODELID, MODELTYPE, EXECUTABLE, ARGUMENTS, MODELDIR, CASENAME, TEMPWORKDIR, RESULTSFILES_RR, RESULTSFILES_FLOW) VALUES ('" & myModel.Id & "','" & myModel.ModelType.ToString & "','" & myModel.Exec & "','" & myModel.Args & "','" & myModel.ModelDir & "','" & myModel.CaseName & "','" & myModel.TempWorkDir & "','" & RRFiles & "','" & FLOWFiles & "');"
            Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)
        Next
        con.Close()

    End Function

    Public Function PopulateStochastsFromDB(ByRef con As SQLite.SQLiteConnection) As Boolean
        '--------------------------------------------------------------------------------------------------------
        'this function populates the stochastic classes from the database


        'LET OP: IS NOG NIET AF!!!!!!!!!!!!!!!!!!


        '--------------------------------------------------------------------------------------------------------
        Dim myQuery As String, dtSeasons As DataTable, dtStoch As DataTable, i As Integer, j As Integer
        Dim mySeason As clsStochasticSeasonClass

        dtSeasons = New DataTable
        myQuery = "Select * FROM SEIZOENEN"
        Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtSeasons)

        Seasons = New Dictionary(Of String, clsStochasticSeasonClass)
        For i = 0 To dtSeasons.Rows.Count - 1
            If dtSeasons.Rows(i)("USE") Then
                Seasons.Add(dtSeasons.Rows(i)("SEASON").ToString.Trim.ToUpper, New clsStochasticSeasonClass(Me.Setup, dtSeasons.Rows(i)("SEASON"), dtSeasons.Rows(i)("EVENTSTART"), dtSeasons.Rows(i)("KANS")))
            End If
            mySeason = Seasons.Item(dtSeasons.Rows(i)("SEASON").ToString.Trim.ToUpper)
            With mySeason

                'add the volumes that are in use
                dtStoch = New DataTable
                myQuery = "Select VOLUME, KANS WHERE DUUR = " & Duration & " And SEIZOEN = '" & .Name & "' AND USE=-1;"
                Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtStoch)
                For j = 0 To dtStoch.Rows.Count - 1
                    .GetAddVolumeClass(dtStoch.Rows(j)(0), dtStoch.Rows(j)(1))
                Next

                'add the patterns that are in use
                dtStoch = New DataTable
                myQuery = "SELECT PATROON, KANS WHERE DUUR = " & Duration & " And SEIZOEN = '" & .Name & "' AND USE=-1;"
                Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtStoch)
                For j = 0 To dtStoch.Rows.Count - 1
                    .GetAddPatternClass(dtStoch.Rows(j)(0), dtStoch.Rows(j)(1))
                Next





            End With
        Next



    End Function

    Public Function PopulateFromDataGridView() As Boolean

        '--------------------------------------------------------------------------------------------------------
        'this function populates the stochastic classes from the datagridviews on the form:
        'SeasonsGrid, VolumesGrid etc. etc.
        '--------------------------------------------------------------------------------------------------------

        Try
            'this routine walks through all of the stochasts provided and decides whether or not they are in use
            Dim i As Integer = -1
            Dim mySeason As clsStochasticSeasonClass
            Dim myVolumeClass As clsStochasticVolumeClass
            Dim myPattern As clsStochasticPatternClass
            Dim myGW As clsStochasticGroundwaterClass
            Dim myWL As clsStochasticWaterLevelClass
            Dim myWind As clsStochasticWindClass
            Dim Extra1 As clsStochasticExtraClass
            Dim Extra2 As clsStochasticExtraClass
            Dim Extra3 As clsStochasticExtraClass
            Dim Extra4 As clsStochasticExtraClass

            Seasons = New Dictionary(Of String, clsStochasticSeasonClass)
            With Setup.StochastenAnalyse
                For Each KlimaatRow As DataGridViewRow In SeasonGrid.Rows
                    i += 1
                    If KlimaatRow.Cells("USE").Value = 1 Then

                        'add a new instance of the class clsSeason and add it to the dictionary
                        mySeason = New clsStochasticSeasonClass(Me.Setup)
                        mySeason.Name = KlimaatRow.Cells("SEASON").Value
                        mySeason.P = KlimaatRow.Cells("KANS").Value
                        mySeason.EventStart = KlimaatRow.Cells("EVENTSTART").Value

                        With mySeason

                            'add the volumes that are in use
                            Dim grVolume As DataGridView = VolumeGrids.Item(i)
                            Setup.GeneralFunctions.UpdateProgressBar("Populating volumes for " & mySeason.Name, 0, 10, True)
                            For Each VolumeRow As DataGridViewRow In grVolume.Rows
                                'Dim checkCell As DataGridViewCheckBoxCell = CType(VolumeRow.Cells("USE"), DataGridViewCheckBoxCell)
                                'If checkCell.Value = True Then
                                If VolumeRow.Cells("USE").Value = 1 Then
                                    'If CType(VolumeRow.Cells("USE"), DataGridViewCheckBoxCell).checked = True Then
                                    myVolumeClass = New clsStochasticVolumeClass(VolumeRow.Cells("KANSCORR").Value)
                                    myVolumeClass.Volume = VolumeRow.Cells("VOLUME").Value
                                    Dim c As Integer = -1
                                    For Each myCol As DataGridViewColumn In grVolume.Columns
                                        c += 1
                                        'only after index 7 the columns contain values
                                        If c >= 7 Then
                                            myVolumeClass.AddLocation(myCol.Name, VolumeRow.Cells(c).Value)
                                        End If
                                    Next
                                    mySeason.VolumeUse = True
                                    mySeason.Volumes.Add(myVolumeClass.Volume, myVolumeClass)
                                End If
                            Next

                            'add the patterns that are in use
                            Dim grPattern As DataGridView = PatternGrids.Item(i)
                            Setup.GeneralFunctions.UpdateProgressBar("Populating patterns for " & mySeason.Name, 1, 10, True)
                            For Each PatternRow As DataGridViewRow In grPattern.Rows
                                If PatternRow.Cells("USE").Value = 1 AndAlso Not IsDBNull(PatternRow.Cells("KANS").Value) Then
                                    myPattern = New clsStochasticPatternClass(PatternRow.Cells("PATROON").Value, PatternRow.Cells("KANSCORR").Value)
                                    mySeason.Patterns.Add(myPattern.Patroon.ToString, myPattern)
                                    mySeason.PatternUse = True
                                End If
                            Next


                            'add the groundwater classes that are in use
                            Dim grGroundwater As DataGridView = GroundwaterGrids.Item(i)
                            Dim FileNames As String = "", FileName As String = ""
                            Setup.GeneralFunctions.UpdateProgressBar("Populating groundwater for " & mySeason.Name, 2, 10, True)
                            For Each GwRow As DataGridViewRow In grGroundwater.Rows
                                If GwRow.Cells("USE").Value = 1 Then
                                    myGW = New clsStochasticGroundwaterClass(GwRow.Cells("NAAM").Value, GwRow.Cells("KANS").Value)

                                    'v2.205: introducing multi-file support for this stochast. Also introduce distinction between RR, Flow and RTC
                                    If Not IsDBNull(GwRow.Cells("RRFILES").Value) Then
                                        FileNames = GwRow.Cells("RRFILES").Value
                                        While FileNames.Length > 0
                                            FileName = Me.Setup.GeneralFunctions.ParseString(FileNames, ";")
                                            myGW.AddRRFile(FileName)
                                        End While
                                    End If

                                    If Not IsDBNull(GwRow.Cells("FLOWFILES").Value) Then
                                        FileNames = GwRow.Cells("FLOWFILES").Value
                                        While FileNames.Length > 0
                                            FileName = Me.Setup.GeneralFunctions.ParseString(FileNames, ";")
                                            myGW.AddFlowFile(FileName)
                                        End While
                                    End If

                                    If Not IsDBNull(GwRow.Cells("RTCFILES").Value) Then
                                        FileNames = GwRow.Cells("RTCFILES").Value
                                        While FileNames.Length > 0
                                            FileName = Me.Setup.GeneralFunctions.ParseString(FileNames, ";")
                                            myGW.AddRtcFile(FileName)
                                        End While
                                    End If

                                    mySeason.Groundwater.Add(myGW.ID, myGW)
                                    mySeason.GroundwaterUse = True
                                End If
                            Next

                            'add the waterlevel boundary classes that are in use
                            Setup.GeneralFunctions.UpdateProgressBar("Populating boundaries for " & mySeason.Name, 3, 10, True)
                            For Each wlRow As DataGridViewRow In WaterLevelGrid.Rows
                                If wlRow.Cells("USE").Value = 1 Then
                                    myWL = New clsStochasticWaterLevelClass(wlRow.Cells("NAAM").Value, wlRow.Cells("KANS").Value)
                                    mySeason.WaterLevels.Add(myWL.ID, myWL)
                                    mySeason.WaterLevelsUse = True
                                End If
                            Next

                            'add the wind classes that are in use
                            Setup.GeneralFunctions.UpdateProgressBar("Populating wind for " & mySeason.Name, 4, 10, True)
                            For Each windRow As DataGridViewRow In WindGrid.Rows
                                If windRow.Cells("USE").Value = 1 Then
                                    myWind = New clsStochasticWindClass(windRow.Cells("NAAM").Value, windRow.Cells(KlimaatScenario.ToString.Trim.ToUpper).Value)
                                    mySeason.Wind.Add(myWind.ID, myWind)
                                    mySeason.WindUse = True
                                End If
                            Next

                            'add the extra 1 classes that are in use
                            Setup.GeneralFunctions.UpdateProgressBar("Populating extra1 for " & mySeason.Name, 5, 10, True)
                            Dim Extra1Grid As DataGridView = Extra1Grids.Item(i)
                            For Each extra1Row As DataGridViewRow In Extra1Grid.Rows
                                If extra1Row.Cells("USE").Value = 1 Then
                                    Extra1 = New clsStochasticExtraClass(extra1Row.Cells("NAAM").Value, extra1Row.Cells("KANS").Value, extra1Row.Cells("RRFILES").Value, extra1Row.Cells("FLOWFILES").Value, extra1Row.Cells("RTCFILES").Value)
                                    mySeason.Extra1.Add(Extra1.ID, Extra1)
                                    mySeason.Extra1Use = True
                                End If
                            Next

                            'add the extra 2 classes that are in use
                            Setup.GeneralFunctions.UpdateProgressBar("Populating extra2 for " & mySeason.Name, 6, 10, True)
                            Dim Extra2Grid As DataGridView = Extra2Grids.Item(i)
                            For Each extra2Row As DataGridViewRow In Extra2Grid.Rows
                                If extra2Row.Cells("USE").Value = 1 Then
                                    Extra2 = New clsStochasticExtraClass(extra2Row.Cells("NAAM").Value, extra2Row.Cells("KANS").Value, extra2Row.Cells("RRFILES").Value, extra2Row.Cells("FLOWFILES").Value, extra2Row.Cells("RTCFILES").Value)
                                    mySeason.Extra2.Add(Extra2.ID, Extra2)
                                    mySeason.Extra2Use = True
                                End If
                            Next

                            'add the extra 3 classes that are in use
                            Setup.GeneralFunctions.UpdateProgressBar("Populating extra3 for " & mySeason.Name, 7, 10, True)
                            Dim Extra3Grid As DataGridView = Extra3Grids.Item(i)
                            For Each extra3Row As DataGridViewRow In Extra3Grid.Rows
                                If extra3Row.Cells("USE").Value = 1 Then
                                    Extra3 = New clsStochasticExtraClass(extra3Row.Cells("NAAM").Value, extra3Row.Cells("KANS").Value, extra3Row.Cells("RRFILES").Value, extra3Row.Cells("FLOWFILES").Value, extra3Row.Cells("RTCFILES").Value)
                                    mySeason.Extra3.Add(Extra3.ID, Extra3)
                                    mySeason.Extra3Use = True
                                End If
                            Next

                            'add the extra 4 classes that are in use
                            Setup.GeneralFunctions.UpdateProgressBar("Populating extra4 for " & mySeason.Name, 8, 10, True)
                            Dim Extra4Grid As DataGridView = Extra4Grids.Item(i)
                            For Each Extra4Row As DataGridViewRow In Extra4Grid.Rows
                                If Extra4Row.Cells("USE").Value = 1 Then
                                    Extra4 = New clsStochasticExtraClass(Extra4Row.Cells("NAAM").Value, Extra4Row.Cells("KANS").Value, Extra4Row.Cells("RRFILES").Value, Extra4Row.Cells("FLOWFILES").Value, Extra4Row.Cells("RTCFILES").Value)
                                    mySeason.Extra4.Add(Extra4.ID, Extra4)
                                    mySeason.Extra4Use = True
                                End If
                            Next

                        End With

                        Setup.GeneralFunctions.UpdateProgressBar(mySeason.Name & " complete.", 0, 10, True)
                        Seasons.Add(mySeason.Name, mySeason)
                    End If
                Next
            End With
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function GetAllLocationsJSON() As String
        Try
            Dim query As String = "SELECT DISTINCT LOCATIEID, LAT, LON FROM OUTPUTLOCATIONS;"
            Dim dt As New DataTable
            Dim Columns As New List(Of String)
            Columns.Add("ID")
            Columns.Add("LAT")
            Columns.Add("LON")
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)
            Return Me.Setup.GeneralFunctions.DataTableToJSON(dt, Columns)
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function

    Public Function CalcExceedanceTable(ByRef LocationName As String, Filter As String, ByRef dtResults As DataTable, ByRef dtHerh As DataTable) As Boolean

        'retrieves the results for the current location and generates an exceedance chart

        Try
            Dim myQuery As String
            Dim pCum As Double = 0
            Dim i As Integer
            Dim ValuesField As String
            dtHerh = New DataTable

            If Filter.Trim.ToUpper = "MAX" OrElse Filter.Trim.ToUpper = "MAXVAL" Then
                ValuesField = "MAXVAL"
            ElseIf Filter.Trim.ToUpper = "MIN" OrElse Filter.Trim.ToUpper = "MINVAL" Then
                ValuesField = "MINVAL"
            Else
                Throw New Exception("Timeseries filter on results not supported: " & Filter)
            End If

            'retrieve the values for each on this location, sorted by ascending value
            myQuery = "Select RUNID, " & ValuesField & ", P FROM RESULTATEN WHERE LOCATIENAAM='" & LocationName & "' AND KLIMAATSCENARIO='" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR=" & Setup.StochastenAnalyse.Duration & " ORDER BY " & ValuesField & " ASC;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, myQuery, dtResults, False)

            'retrieve all additional information for each run from the RUNS table
            'myQuery = "SELECT * FROM RUNS WHERE KLIMAATSCENARIO='" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR=" & Setup.StochastenAnalyse.Duration & ";"
            'Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, myQuery, dtRuns, False)
            'If dtRuns.Rows.Count = 0 OrElse dtResults.Rows.Count = 0 Then Throw New Exception("Error retrieving locations from database for location name=" & LocationName)

            'add columns to the new datatable
            dtHerh.Columns.Add(New DataColumn("Herhalingstijd", Type.GetType("System.Double")))
            dtHerh.Columns.Add(New DataColumn("Waarde", Type.GetType("System.Double")))
            dtHerh.Columns.Add(New DataColumn("RUNID", Type.GetType("System.String")))

            'compute the return period and store in the new table
            'note: skip the last point since that will cause a divisio by zero. hence the count-2
            For i = 0 To dtResults.Rows.Count - 2
                pCum += dtResults.Rows(i)("P")

                ''search the identical run in the RUNS table
                'For j = 0 To dtRuns.Rows.Count - 1
                '    ''ALERT: SIEBE NOG CORRIGEREN
                '    'pCum += 0.01
                '    If dtRuns.Rows(j)("RUNID") = dtResults.Rows(i)("RUNID") Then
                '        pCum += dtRuns.Rows(j)("P")
                '        Exit For
                '    End If
                'Next

                dtHerh.Rows.Add()
                dtHerh.Rows(i)(1) = dtResults.Rows(i)(ValuesField)      'write the value for this data pair
                dtHerh.Rows(i)(2) = dtResults.Rows(i)("RUNID")          'write the runid for this data pair

                If Setup.StochastenAnalyse.VolumesAsFrequencies Then
                    Dim maxFreq As Double = 365.25 * 24 / Duration
                    If pCum < maxFreq Then
                        dtHerh.Rows(i)(0) = 1 / (maxFreq - pCum)
                    Else
                        'de hoogste is niet vast te stellen want maxFreq- fCum = 0, dus deling door nul
                        'daarom hier een eenvoudige benadering door T(i-1) + 1 te nemen
                        'maar in het wegschrijven van de resultaten slaan we deze sowieso over
                        dtHerh.Rows(i)(0) = dtHerh.Rows(i - 1)(0) + 1   'write the return period for this data pair
                    End If
                Else
                    dtHerh.Rows(i)(0) = 1 / -Math.Log(pCum) 'herhalingstijd = 1/-ln(onderschrijdingskans)
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function calcExceedanceTable.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function getLevelsFromLocation(ByRef LocationName As String, TableName As String, ZPField As String, WPField As String, MaxAllowedLevelField As String, SurfaceLevelField As String, ByRef dt As DataTable) As Boolean

        'retrieves the results for the current location and returns them in a datatable
        Try
            Dim myQuery As String
            dt = New DataTable

            'retrieve the values for each on this location, sorted by ascending value
            myQuery = "Select " & ZPField & "," & WPField & "," & MaxAllowedLevelField & "," & SurfaceLevelField & " FROM " & TableName & " WHERE LOCATIEID='" & LocationName & "';"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, myQuery, dt)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getTargetLevels.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function getModelByID(ByVal modelID As Integer)
        For Each myModel In Models.Values
            If myModel.Id = modelID Then Return myModel
        Next
        Return Nothing
    End Function

    Public Function GetFileInfoByLocationName(ByRef con As SQLite.SQLiteConnection, ByVal LocationName As String, ByRef FileName As String, ByRef Parameter As String) As Boolean
        'deze functie geeft op basis van een locatie ID terug welk resultatenbestand daaraan is gekoppeld alsmede de parameter
        Dim ID As String = ""

        Try
            'first read the location ID from the database
            Dim dt As New DataTable
            Dim query As String = "SELECT * FROM OUTPUTLOCATIONS WHERE LOCATIENAAM='" & LocationName & "';"
            Setup.GeneralFunctions.SQLiteQuery(con, query, dt)

            If dt.Rows.Count > 0 Then
                ID = dt.Rows(0)("LOCATIEID")
            Else
                Throw New Exception("Error: no location information in the database for " & LocationName)
            End If

            For Each myModel As clsSimulationModel In Me.Setup.StochastenAnalyse.Models.Values
                For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                    For Each myPar As clsResultsFileParameter In myFile.Parameters.Values
                        For Each myLoc As clsResultsFileLocation In myPar.Locations.Values
                            If myLoc.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                                FileName = myFile.FileName
                                Parameter = myPar.Name
                                Return True
                            End If
                        Next
                    Next
                Next
            Next

            Throw New Exception("Location Not found In model output:  " & ID)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetFileINfoByLocationID for location " & ID)
            Return False
        End Try
    End Function

    Public Function CalculateExceedanceTables(ByRef con As SQLite.SQLiteConnection) As Boolean
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Overschrijdingstabellen berekenen...", 0, 10, True)

            'now only read results locations
            Dim locdt As New DataTable, locIdx As Integer
            Dim query As String = "SELECT DISTINCT LOCATIENAAM, RESULTSTYPE FROM OUTPUTLOCATIONS;"
            Setup.GeneralFunctions.SQLiteQuery(con, query, locdt, False)

            'populate a table containing all stochast classes per run
            'for speed, also create a list of row indices
            Dim rundt As New DataTable, i As Integer
            Dim RunsList As New Dictionary(Of String, Integer)
            query = "SELECT RUNID, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4 FROM RUNS WHERE KLIMAATSCENARIO='" & KlimaatScenario.ToString & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(con, query, rundt, False)
            For i = 0 To rundt.Rows.Count - 1
                RunsList.Add(rundt.Rows(i)(0), i)
            Next

            'clear existing exceedance tables for this location and climate
            query = "DELETE FROM HERHALINGSTIJDEN WHERE DUUR=" & Duration & " AND KLIMAATSCENARIO='" & KlimaatScenario.ToString & "';"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(con, query, False)

            'for each location in our results table we will now create an exceedance table and write it to the database
            Dim nLocs As Integer = locdt.Rows.Count
            For locIdx = 0 To locdt.Rows.Count - 1

                Me.Setup.GeneralFunctions.UpdateProgressBar("", locIdx + 1, nLocs)

                Dim dtRuns As New DataTable
                Dim dtResults As New DataTable
                Dim dtHerh As New DataTable

                'retrieve all data for the current duration and climat scenario
                If Me.Setup.StochastenAnalyse.CalcExceedanceTable(locdt.Rows(locIdx)("LOCATIENAAM"), locdt.Rows(locIdx)("RESULTSTYPE"), dtResults, dtHerh) Then

                    'bulk insert our excedance table
                    Dim myCmd As New SQLite.SQLiteCommand
                    myCmd.Connection = con
                    Using transaction = con.BeginTransaction

                        For i = 0 To dtHerh.Rows.Count - 1

                            Dim RunID As String = dtHerh.Rows(i)("RUNID")
                            Dim RowIdx As Integer = RunsList.Item(RunID)

                            myCmd.CommandText = "INSERT INTO HERHALINGSTIJDEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, HERHALINGSTIJD, WAARDE, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & locdt.Rows(locIdx)(0).ToString & "'," & dtHerh.Rows(i)(0) & "," & dtHerh.Rows(i)(1) & ",'" & rundt.Rows(RowIdx)("SEIZOEN") & "'," & rundt.Rows(RowIdx)("VOLUME") & ",'" & rundt.Rows(RowIdx)("PATROON") & "','" & rundt.Rows(RowIdx)("GW") & "','" & rundt.Rows(RowIdx)("BOUNDARY") & "','" & rundt.Rows(RowIdx)("WIND") & "','" & rundt.Rows(RowIdx)("EXTRA1") & "','" & rundt.Rows(RowIdx)("EXTRA2") & "','" & rundt.Rows(RowIdx)("EXTRA3") & "','" & rundt.Rows(RowIdx)("EXTRA4") & "');"
                            myCmd.ExecuteNonQuery()
                        Next

                        'insert the resulta for all return periods at once
                        transaction.Commit() 'this is where the bulk insert is finally executed.
                    End Using
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Overschrijdingstabellen succesvol berekend.", 10, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalculateExceedanceTables of class clsStochastenAnalyse: " & ex.Message)
            Me.Setup.Log.ShowAll()
            Return False
        End Try
    End Function

    Public Function ReadResults(ByRef con As SQLite.SQLiteConnection)
        Dim i As Long, n As Long
        Dim MaxInLastTimeStep As Boolean
        Dim Min As Double, Max As Double, tsMin As Long, tsMax As Long, Avg As Double, mySum As Double
        Dim dtRes As New DataTable, query As String

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten lezen...", 0, 10, True)

            'first thing to do is clear all results for the current scenario and duration from the database since the results will change by definition when new computations are added
            If Not con.State = ConnectionState.Open Then con.Open()
            Dim cmd As New SQLite.SQLiteCommand
            cmd.Connection = con

            'remove the existing simulationresults from the database for selected climate and duration
            query = "DELETE FROM RESULTATEN WHERE KLIMAATSCENARIO = '" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR = " & Setup.StochastenAnalyse.Duration.ToString.Trim & ";"
            Setup.GeneralFunctions.SQLiteNoQuery(con, query)

            'read the results
            n = Runs.Runs.Count
            i = 0
            For Each myRun As clsStochastenRun In Runs.Runs.Values                        'doorloop alle runs en lees de resultaatbestanden uit
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten lezen voor simulatie " & i & " van " & n, i, n, True)
                For Each myModel As clsSimulationModel In Models.Values                     'doorloop alle modellen die gedraaid zijn
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values    'doorloop alle bestanden onder dit model
                        If Right(myFile.FileName, 4).ToLower = ".his" Then                      'this is a Deltares HIS-file. Read it using the corresponding reader
                            If Not System.IO.File.Exists(myRun.OutputFilesDir & "\" & myFile.FileName) Then Throw New Exception("Fout: resultatenbestand niet gevonden: " & myRun.OutputFilesDir & "\" & myFile.FileName)
                            Using myHisReader As New clsHisFileBinaryReader(myRun.OutputFilesDir & "\" & myFile.FileName, Setup)
                                If myFile.Parameters.Count = 0 Then
                                    Me.Setup.Log.AddWarning("no parameters specified for output file " & myFile.FileName & ".")
                                Else
                                    For Each myPar As clsResultsFileParameter In myFile.Parameters.Values 'walk through all parameters associated with this HIS-file
                                        If myPar.Locations.Count = 0 Then
                                            Throw New Exception("Error: no locations specified for parameter " & myPar.Name & " in output file " & myFile.FileName & ".")
                                        Else
                                            '--------------------------------------------------------------------------------------------------------------------------------------------
                                            '  writing the results for this run to the database
                                            '  start by reading the entire file to memory. This is supposed to be MUCH faster than reading from file every time
                                            '--------------------------------------------------------------------------------------------------------------------------------------------

                                            'database needs to be populated or refreshed for this run. read the hisfile to memory
                                            myHisReader.ReadToMemory()
                                            Using hisReader As New BinaryReader(myHisReader.ms)

                                                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                                                Using myCmd As New SQLite.SQLiteCommand
                                                    myCmd.Connection = Me.Setup.SqliteCon
                                                    Using transaction = Me.Setup.SqliteCon.BeginTransaction

                                                        'walk through each location and retreive the stochastic results
                                                        For Each myLoc As clsResultsFileLocation In myPar.Locations.Values  'walk through all locations associated with this HIS-file and parameter
                                                            If Not myHisReader.ReadStochasticResultsFromMemoryStream(hisReader, myLoc.ID, myPar.Name, MaxInLastTimeStep, Min, tsMin, Max, tsMax, Avg, ResultsStartPercentage) Then
                                                                Me.Setup.Log.AddError("Could not read results for location " & myLoc.ID & " and parameter " & myPar.Name & ".")
                                                            Else
                                                                'add the outcome of this run to the dictionary of results
                                                                myCmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL, P) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & myLoc.Name & "','" & myRun.ID & "'," & Max & "," & Min & "," & Avg & "," & myRun.P & ");"
                                                                myCmd.ExecuteNonQuery()
                                                                If MaxInLastTimeStep = True Then Me.Setup.Log.AddError("Maximum value in last timestep for simulation " & myRun.ID & " and location " & myLoc.ID)
                                                            End If
                                                        Next

                                                        'insert the results for all locations at once
                                                        transaction.Commit() 'this is where the bulk insert is finally executed.
                                                    End Using
                                                End Using

                                            End Using
                                            myHisReader.Close()

                                        End If
                                    Next
                                End If

                            End Using

                        ElseIf Right(myfile.FileName, 7).ToLower = "_fou.nc" Then

                            'this is a D-Hydro Fourier file!
                            Dim path As String = myRun.OutputFilesDir & "\" & myFile.FileName
                            Dim myFouNC As New clsFouNCFile(path, Me.Setup)
                            If Not System.IO.File.Exists(path) Then Throw New Exception("Fourier file does not exist: " & path)
                            If Not myFouNC.Read() Then Throw New Exception("Error reading fourier file " & path)

                            'retrieve the maximum water levels from our Fourier file
                            Dim Maxima As Double() = myFouNC.get2DMaximumWaterLevels()

                            If myFile.Parameters.Values.Count = 0 Then
                                Me.Setup.Log.AddWarning("no parameters specified for output file " & myFile.FileName & ".")
                            Else
                                For Each myPar As clsResultsFileParameter In myFile.Parameters.Values 'walk through all parameters associated with this HIS-file
                                    If myPar.Locations.Count = 0 Then
                                        Throw New Exception("Error: no locations specified for parameter " & myPar.Name & " in output file " & myFile.FileName & ".")
                                    Else
                                        '--------------------------------------------------------------------------------------------------------------------------------------------
                                        '  writing the results for this run & parameter to the database
                                        '--------------------------------------------------------------------------------------------------------------------------------------------
                                        'Dim Waterlevels As Double(,) = Nothing
                                        'Dim Times As Double() = Nothing            'timesteps, expressed in seconds w.r.t. RefDate as specified in the .MDU
                                        'Dim IDList As String() = Nothing
                                        'If Not myHisNC.ReadWaterLevelsAtObservationPoints(Waterlevels, Times, IDList) Then Throw New Exception("Error reading hisfile by parameter " & myPar.Name)

                                        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                                        Using myCmd As New SQLite.SQLiteCommand
                                            myCmd.Connection = Me.Setup.SqliteCon
                                            Using transaction = Me.Setup.SqliteCon.BeginTransaction

                                                For i = 0 To Maxima.Count - 1
                                                    Dim ID As String = i.ToString       'the index number of each cell is also considered its ID

                                                    If myPar.Locations.ContainsKey(ID.Trim.ToUpper) Then
                                                        'v2.2.2: support for maxima from Fourier files added. Other params not yet
                                                        Max = Maxima(i)
                                                        Min = 0
                                                        Avg = 0

                                                        'add the outcome of this run to the dictionary of results
                                                        myCmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL, P) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & myPar.Locations.Item(ID.Trim.ToUpper).Name & "','" & myRun.ID & "'," & Max & "," & Min & "," & Avg & "," & myRun.P & ");"
                                                        myCmd.ExecuteNonQuery()

                                                    End If
                                                Next

                                                'insert the resulta for all locations at once
                                                transaction.Commit() 'this is where the bulk insert is finally executed.
                                            End Using
                                        End Using

                                    End If
                                Next
                            End If

                        ElseIf Right(myFile.FileName, 7).ToLower = "_his.nc" Then
                            If Not System.IO.File.Exists(myRun.OutputFilesDir & "\" & myFile.FileName) Then Throw New Exception("Fout: resultatenbestand niet gevonden: " & myRun.OutputFilesDir & "\" & myFile.FileName)
                            Dim myHisNC As New clsHisNCFile(myRun.OutputFilesDir & "\" & myFile.FileName, Me.Setup)

                            If myFile.Parameters.Count = 0 Then
                                Me.Setup.Log.AddWarning("no parameters specified for output file " & myFile.FileName & ".")
                            Else
                                For Each myPar As clsResultsFileParameter In myFile.Parameters.Values 'walk through all parameters associated with this HIS-file
                                    If myPar.Locations.Count = 0 Then
                                        Throw New Exception("Error: no locations specified for parameter " & myPar.Name & " in output file " & myFile.FileName & ".")
                                    Else
                                        '--------------------------------------------------------------------------------------------------------------------------------------------
                                        '  writing the results for this run & parameter to the database
                                        '--------------------------------------------------------------------------------------------------------------------------------------------
                                        Dim Waterlevels As Double(,) = Nothing
                                        Dim Times As Double() = Nothing            'timesteps, expressed in seconds w.r.t. RefDate as specified in the .MDU
                                        Dim IDList As String() = Nothing
                                        If Not myHisNC.ReadWaterLevelsAtObservationPoints(Waterlevels, Times, IDList) Then Throw New Exception("Error reading hisfile by parameter " & myPar.Name)

                                        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                                        Using myCmd As New SQLite.SQLiteCommand
                                            myCmd.Connection = Me.Setup.SqliteCon
                                            Using transaction = Me.Setup.SqliteCon.BeginTransaction

                                                For i = 0 To IDList.Count - 1
                                                    Dim ID As String = IDList(i)

                                                    If myPar.Locations.ContainsKey(ID.Trim.ToUpper) Then
                                                        Max = -9.0E+99
                                                        Min = 9.0E+99
                                                        mySum = 0
                                                        n = 0

                                                        For j = 0 To UBound(Waterlevels, 1)
                                                            If (j + 1) / (UBound(Waterlevels, 1) + 1) * 100 >= ResultsStartPercentage Then
                                                                n += 1
                                                                mySum += Waterlevels(j, i)
                                                                If Waterlevels(j, i) > Max Then
                                                                    Max = Waterlevels(j, i)
                                                                    tsMax = j
                                                                    If j = UBound(Waterlevels, 1) Then MaxInLastTimeStep = True
                                                                End If
                                                                If Waterlevels(j, i) < Min Then
                                                                    Min = Waterlevels(j, i)
                                                                    tsMin = j
                                                                End If
                                                            End If
                                                        Next
                                                        Avg = mySum / n

                                                        'add the outcome of this run to the dictionary of results
                                                        myCmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL, P) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & myPar.Locations.Item(ID.Trim.ToUpper).Name & "','" & myRun.ID & "'," & Max & "," & Min & "," & Avg & "," & myRun.P & ");"
                                                        myCmd.ExecuteNonQuery()
                                                        If MaxInLastTimeStep = True Then Me.Setup.Log.AddError("Maximum value in last timestep for simulation " & myRun.ID & " and location " & myPar.Locations.Item(ID.Trim.ToUpper).ID)

                                                    End If
                                                Next

                                                'insert the resulta for all locations at once
                                                transaction.Commit() 'this is where the bulk insert is finally executed.
                                            End Using
                                        End Using

                                    End If
                                Next
                            End If
                        End If
                    Next
                Next
            Next


            If Me.Setup.Log.Errors.Count > 0 Then
                Me.Setup.Log.AddMessage("Reading simulation complete, but with errors! Please check.")
            Else
                Me.Setup.Log.AddMessage("Reading simulation results was completed successfully.")
            End If


            con.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten met succes uitgelezen.", 10, 10, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.ShowAll()
            Return False
        End Try

    End Function


    Public Function ExportResults(ByRef con As SQLite.SQLiteConnection) As Boolean
        Try
            'open the database connection
            If Not con.State = ConnectionState.Open Then con.Open()
            Dim cmd As New SQLite.SQLiteCommand
            cmd.Connection = con

            Dim r As Long = 0, c As Long = 0
            Dim i As Long, n As Long
            Dim dtLoc As New DataTable 'locations
            Dim dtRes As DataTable 'results
            Dim dtRuns As DataTable 'runs
            Dim dtHerh As DataTable

            'prepare a spreadsheet
            Dim ws As clsExcelSheet = Me.Setup.ExcelFile.GetAddSheet("Herh" & "_" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "_" & Setup.StochastenAnalyse.Duration.ToString)
            ws.ws.Cells(0, 0).Value = "ID"
            ws.ws.Cells(1, 0).Value = "Alias"
            ws.ws.Cells(2, 0).Value = 0.01
            ws.ws.Cells(3, 0).Value = 0.05
            ws.ws.Cells(4, 0).Value = 0.1
            ws.ws.Cells(5, 0).Value = 0.5
            ws.ws.Cells(6, 0).Value = 1
            ws.ws.Cells(7, 0).Value = 2
            ws.ws.Cells(8, 0).Value = 3
            ws.ws.Cells(9, 0).Value = 4
            ws.ws.Cells(10, 0).Value = 5
            ws.ws.Cells(11, 0).Value = 6
            ws.ws.Cells(12, 0).Value = 7
            ws.ws.Cells(13, 0).Value = 8
            ws.ws.Cells(14, 0).Value = 9
            ws.ws.Cells(15, 0).Value = 10
            ws.ws.Cells(16, 0).Value = 15
            ws.ws.Cells(17, 0).Value = 20
            ws.ws.Cells(18, 0).Value = 25
            ws.ws.Cells(19, 0).Value = 30
            ws.ws.Cells(20, 0).Value = 35
            ws.ws.Cells(21, 0).Value = 40
            ws.ws.Cells(22, 0).Value = 45
            ws.ws.Cells(23, 0).Value = 50
            ws.ws.Cells(24, 0).Value = 60
            ws.ws.Cells(25, 0).Value = 70
            ws.ws.Cells(26, 0).Value = 80
            ws.ws.Cells(27, 0).Value = 90
            ws.ws.Cells(28, 0).Value = 100
            ws.ws.Cells(29, 0).Value = 200
            ws.ws.Cells(30, 0).Value = 300
            ws.ws.Cells(31, 0).Value = 400
            ws.ws.Cells(32, 0).Value = 500
            ws.ws.Cells(33, 0).Value = 600
            ws.ws.Cells(34, 0).Value = 700
            ws.ws.Cells(35, 0).Value = 800
            ws.ws.Cells(36, 0).Value = 900
            ws.ws.Cells(37, 0).Value = 1000

            'get all unique location names from the database
            dtLoc = GetUniqueLocationNamesFromDB(con)

            'for every location, build an exceedance chart and export it to CSV
            'also write it to Excel, in a less detailed manner
            n = dtLoc.Rows.Count - 1
            Setup.GeneralFunctions.UpdateProgressBar("Exporting results per location...", 0, n)
            For i = 0 To dtLoc.Rows.Count - 1
                Setup.GeneralFunctions.UpdateProgressBar("", i, n)

                'create a worksheet for this location
                'ws = Me.Setup.ExcelFile.GetAddSheet(dtLoc.Rows(i)(0))

                dtRes = New DataTable
                dtRuns = New DataTable
                dtHerh = New DataTable

                'calculate an exceedance table for this location
                If Not CalcExceedanceTable(dtLoc.Rows(i)(0), "MAXVAL", dtRes, dtHerh) Then
                    Setup.Log.AddError("Error calculating exceedance table for location: " & dtLoc.Rows(i)("LOCATIENAAM"))
                Else
                    'also retrieve the runs
                    Setup.GeneralFunctions.DataTable2CSV(dtRes, dtRuns, ResultsDir & "\" & dtLoc.Rows(i)(0) & ".csv", ";")
                    ws.ws.Cells(0, i + 1).Value = dtLoc.Rows(i)(0)
                    ws.ws.Cells(1, i + 1).Value = dtLoc.Rows(i)(0)
                    ws.ws.Cells(2, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 0.01, 0, 1)
                    ws.ws.Cells(3, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 0.05, 0, 1)
                    ws.ws.Cells(4, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 0.1, 0, 1)
                    ws.ws.Cells(5, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 0.5, 0, 1)
                    ws.ws.Cells(6, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 1, 0, 1)
                    ws.ws.Cells(7, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 2, 0, 1)
                    ws.ws.Cells(8, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 3, 0, 1)
                    ws.ws.Cells(9, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 4, 0, 1)
                    ws.ws.Cells(10, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 5, 0, 1)
                    ws.ws.Cells(11, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 6, 0, 1)
                    ws.ws.Cells(12, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 7, 0, 1)
                    ws.ws.Cells(13, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 8, 0, 1)
                    ws.ws.Cells(14, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 9, 0, 1)
                    ws.ws.Cells(15, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 10, 0, 1)
                    ws.ws.Cells(16, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 15, 0, 1)
                    ws.ws.Cells(17, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 20, 0, 1)
                    ws.ws.Cells(18, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 25, 0, 1)
                    ws.ws.Cells(19, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 30, 0, 1)
                    ws.ws.Cells(20, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 35, 0, 1)
                    ws.ws.Cells(21, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 40, 0, 1)
                    ws.ws.Cells(22, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 45, 0, 1)
                    ws.ws.Cells(23, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 50, 0, 1)
                    ws.ws.Cells(24, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 60, 0, 1)
                    ws.ws.Cells(25, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 70, 0, 1)
                    ws.ws.Cells(26, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 80, 0, 1)
                    ws.ws.Cells(27, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 90, 0, 1)
                    ws.ws.Cells(28, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 100, 0, 1)
                    ws.ws.Cells(29, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 200, 0, 1)
                    ws.ws.Cells(30, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 300, 0, 1)
                    ws.ws.Cells(31, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 400, 0, 1)
                    ws.ws.Cells(32, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 500, 0, 1)
                    ws.ws.Cells(33, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 600, 0, 1)
                    ws.ws.Cells(34, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 700, 0, 1)
                    ws.ws.Cells(35, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 800, 0, 1)
                    ws.ws.Cells(36, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 900, 0, 1)
                    ws.ws.Cells(37, i + 1).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 1000, 0, 1)
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ExportResults of class clsStochastenAnalyse.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function GetUniqueLocationNamesFromDB(ByRef con As SQLite.SQLiteConnection) As DataTable
        Dim query As String = "SELECT DISTINCT LOCATIENAAM FROM OUTPUTLOCATIONS;"
        Dim dtLoc As New DataTable
        Setup.GeneralFunctions.SQLiteQuery(con, query, dtLoc)
        Return dtLoc
    End Function

    Public Function GetLocationResultsFromDB(ByRef con As SQLite.SQLiteConnection, LocationName As String) As DataTable

        'retrieves the results for the given location name
        Dim dtResults As New DataTable
        Dim myQuery As String

        myQuery = "SELECT * FROM RESULTATEN WHERE LOCATIENAAM='" & LocationName & "' AND KLIMAATSCENARIO='" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR=" & Setup.StochastenAnalyse.Duration & ";"
        Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtResults)
        Return dtResults

    End Function

    Public Function GetRunProbabilitiesFromDB(ByRef con As SQLite.SQLiteConnection) As DataTable

        'retrieve the probabilities for all runs
        Dim dtRuns As New DataTable 'runs
        Dim myQuery As String

        myQuery = "SELECT RUNID, P FROM RUNS WHERE KLIMAATSCENARIO='" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR=" & Setup.StochastenAnalyse.Duration & ";"
        Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dtRuns)
        Return dtRuns
    End Function


    Public Function ResultInDB(ByRef con As OleDb.OleDbConnection, RunID As String) As Long
        'this routine checks whether the database already contains results for the current model, parameter and stochastic run
        'it returns the number of database entries so that we can match it with the number of locations
        Try
            Dim query As String = "SELECT * From RESULTATEN WHERE KLIMAATSCENARIO='" & KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR=" & Duration.ToString.Trim & " AND  RUNID='" & RunID & "';"
            Dim dt As New DataTable
            Dim da As New OleDb.OleDbDataAdapter(query, con)
            da.Fill(dt)
            Return dt.Rows.Count
        Catch ex As Exception
            Me.Setup.Log.AddError("Could Not check availability Of result In database.")
            Return 0
        End Try

    End Function


    Public Sub UpdateVolumesCheckSum(ByRef grVolumesZomer As DataGridView, ByRef grVolumesWinter As DataGridView, ByRef lblChecksum As Label)
        Dim i As Long, SumZomer As Double, SumWinter As Double, Sum As Double
        For i = 0 To grVolumesZomer.Rows.Count - 1
            If Not IsDBNull(grVolumesZomer.Rows(i).Cells(3).Value) Then SumZomer += grVolumesZomer.Rows(i).Cells(3).Value
        Next
        For i = 0 To grVolumesWinter.Rows.Count - 1
            If Not IsDBNull(grVolumesWinter.Rows(i).Cells(3).Value) Then SumWinter += grVolumesWinter.Rows(i).Cells(3).Value
        Next
        Sum = Math.Round(SumZomer + SumWinter, 5)

        If Setup.StochastenAnalyse.VolumesAsFrequencies Then
            lblChecksum.Text = "Checksum=" & Math.Round((Sum * Duration) / (365.25 * 24), 5)
        Else
            lblChecksum.Text = "Checksum=" & Sum
        End If

    End Sub

    Public Function getBuiVerloop(ByVal Patroon As STOCHLIB.GeneralFunctions.enmNeerslagPatroon) As Double()

        '------------------------------------------------------------------------
        'Author: Siebe Bosch
        'date: 17-8-2014
        'Description: gets the pattern for a rainfall event from the database
        '------------------------------------------------------------------------

        'duration must already have been set, and also the path to the database
        Dim myPatroon As Double()
        ReDim myPatroon(Duration - 1)
        Dim query As String, i As Integer

        'connect to the database and retrieve the values
        Try

            Dim cn As New SQLite.SQLiteConnection
            Dim da As SQLite.SQLiteDataAdapter
            Dim dt As New DataTable
            cn.ConnectionString = "Data Source=" & StochastsConfigFile & ";Version=3;"

            cn.Open()
            query = "Select FRACTIE from NEERSLAGVERLOOP where DUUR=" & Duration.ToString.Trim & " And PATROON='" & Patroon.ToString.ToUpper & "' ORDER BY UUR;"
            da = New SQLite.SQLiteDataAdapter(query, cn)
            da.Fill(dt)
            cn.Close()

            For i = 0 To Duration - 1
                myPatroon(i) = dt.Rows(i)(0)
            Next
            Return myPatroon
        Catch err As Exception
            Me.Setup.Log.AddError(err.Message)
            Return Nothing
        End Try

    End Function

    Public Sub RefreshRunsGrid(ByRef myGrid As DataGridView, ByRef btnCharts As Button)
        Dim RunID As String, Done As Boolean, AllComplete As Boolean
        Dim myRun As clsStochastenRun, myRow As DataGridViewRow
        Dim myModel As clsSimulationModel, myFile As clsResultsFile
        Dim i As Long = 0, n As Long = myGrid.Rows.Count
        Dim ColIdx As Integer = myGrid.Columns("DONE").Index

        Me.Setup.GeneralFunctions.UpdateProgressBar("Simulatieresultaten inventariseren.", 0, 10, True)

        myGrid.Columns(ColIdx).AutoSizeMode = DataGridViewAutoSizeColumnMode.None 'temporarily set the autosize mode for our column to none. This speeds up the routine below
        AllComplete = True
        For Each myRow In myGrid.Rows
            i += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
            RunID = myRow.Cells("ID").Value
            myRun = Runs.Runs.Item(RunID.Trim.ToUpper)

            'set to true if all output files are present
            Done = True
            For Each myModel In Models.Values
                For Each myFile In myModel.ResultsFiles.Files.Values
                    If Not File.Exists(myRun.OutputFilesDir & "\" & myFile.FileName) Then
                        Done = False
                        AllComplete = False
                        Exit For
                    End If
                Next
            Next
            myRow.Cells(ColIdx).Value = Done
        Next
        myGrid.Columns(ColIdx).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells

        Me.Setup.GeneralFunctions.UpdateProgressBar("Inventarisatie gereed.", 1, 1, True)
        If AllComplete Then
            btnCharts.Enabled = True
        Else
            btnCharts.Enabled = False
        End If

    End Sub


End Class
