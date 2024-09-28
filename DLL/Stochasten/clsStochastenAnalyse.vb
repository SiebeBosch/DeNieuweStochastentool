Imports STOCHLIB.General
Imports System.Xml
Imports System.Windows.Forms
Imports System.IO
Imports System.Data.SQLite
Imports System.Net.WebRequestMethods
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Office2019.Drawing
Imports System.Transactions
Imports System.Runtime.InteropServices.WindowsRuntime
Imports System.IO.Compression
Imports System.Text
Imports GemBox.Spreadsheet
Imports DocumentFormat.OpenXml.Spreadsheet
Imports MapWinGIS
Imports Apache.Arrow
Imports Apache.Arrow.Ipc
Imports Apache.Arrow.Types
Imports Apache.Arrow.Memory
Imports Newtonsoft.Json.Linq
Imports Microsoft.VisualBasic.FileIO
Imports System.Linq

'Imports Ionic.Zip

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

    Public Function WriteFouTopographyJSON(JSONpath As String) As Boolean
        Try
            'first we need to find the output directory of an existing simulation so we can find and read the .FOU file
            Dim i As Long, n As Long
            n = Runs.Runs.Count
            i = 0
            For Each myRun As clsStochastenRun In Runs.Runs.Values
                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten lezen voor simulatie " & i & " van " & n, i, n, True)
                For Each myModel As clsSimulationModel In Models.Values                     'doorloop alle modellen die gedraaid zijn
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values    'doorloop alle bestanden onder dit model
                        If Right(myFile.FileName, 7).ToLower = "_fou.nc" Then

                            'error handling
                            If Not System.IO.File.Exists(myRun.OutputFilesDir & "\" & myFile.FileName) Then Throw New Exception("Fout: resultatenbestand niet gevonden: " & myRun.OutputFilesDir & "\" & myFile.FileName)

                            Dim path As String = myRun.OutputFilesDir & "\" & myFile.FileName
                            Dim myFouNC As New clsFouNCFile(path, Me.Setup)
                            If Not System.IO.File.Exists(path) Then Throw New Exception("Fourier file does not exist: " & path)
                            If Not myFouNC.Read() Then Throw New Exception("Error reading fourier file " & path)


                            Using jsWriter As New StreamWriter(JSONpath)
                                jsWriter.Write("let Mesh = ")
                                'now that we have read our Fou file, export it to a shapefile
                                Dim shpPath As String = Me.Setup.GeneralFunctions.getBaseFromFilename(JSONpath) & ".shp"
                                Dim SourceProjection As New MapWinGIS.GeoProjection
                                Dim Extents As New MapWinGIS.Extents
                                SourceProjection.ImportFromEPSG("28992")
                                Dim TargetProjection As New MapWinGIS.GeoProjection
                                TargetProjection.ImportFromEPSG("4326")
                                myFouNC.ReprojectAndWriteMeshToGeoJSON(shpPath, jsWriter, "BASE", SourceProjection, TargetProjection, Extents)
                            End Using

                            Exit For
                        End If
                    Next
                Next


                Exit For
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteFouTopographyJSON of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function WriteStochasticRunsJSON(runspath As String, ClimateScenario As String, Duration As Integer, configurationName As String) As Boolean
        Try
            Dim dt As New DataTable
            Dim query As String = "SELECT RUNID, RUNIDX, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4 FROM RUNS WHERE KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, True)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing runs...", 0, 10, True)


            'write the dataset to json
            If System.IO.File.Exists(runspath) Then System.IO.File.Delete(runspath)
            Using exceedanceWriter As New StreamWriter(runspath)
                exceedanceWriter.WriteLine("let runs = {")

                exceedanceWriter.WriteLine("  ""scenarios"": [")
                exceedanceWriter.WriteLine("    {")
                exceedanceWriter.WriteLine($"      ""ID"":""{configurationName}"",")
                exceedanceWriter.WriteLine("        ""runs"": [")
                Dim jsonStr As String = ""
                For i As Integer = 0 To dt.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, dt.Rows.Count)
                    jsonStr &= "          {""runid"":""" & dt.Rows(i)("RUNID").ToString() & ""","
                    jsonStr &= """runidx"":" & dt.Rows(i)("RUNIDX") & ","
                    jsonStr &= """seizoen"":""" & dt.Rows(i)("SEIZOEN").ToString() & ""","
                    jsonStr &= """volume"":""" & dt.Rows(i)("VOLUME").ToString() & ""","
                    jsonStr &= """patroon"":""" & dt.Rows(i)("PATROON").ToString() & ""","
                    jsonStr &= """gw"":""" & dt.Rows(i)("GW").ToString() & ""","
                    jsonStr &= """boundary"":""" & dt.Rows(i)("BOUNDARY").ToString() & ""","
                    jsonStr &= """wind"":""" & dt.Rows(i)("WIND").ToString() & ""","
                    jsonStr &= """extra1"":""" & If(dt.Rows(i)("EXTRA1") IsNot DBNull.Value, dt.Rows(i)("EXTRA1").ToString(), "") & ""","
                    jsonStr &= """extra2"":""" & If(dt.Rows(i)("EXTRA2") IsNot DBNull.Value, dt.Rows(i)("EXTRA2").ToString(), "") & ""","
                    jsonStr &= """extra3"":""" & If(dt.Rows(i)("EXTRA3") IsNot DBNull.Value, dt.Rows(i)("EXTRA3").ToString(), "") & ""","
                    jsonStr &= """extra4"":""" & If(dt.Rows(i)("EXTRA4") IsNot DBNull.Value, dt.Rows(i)("EXTRA4").ToString(), "") & """}"
                    If i < dt.Rows.Count - 1 Then
                        jsonStr &= "," & vbCrLf
                    Else
                        jsonStr &= vbCrLf
                    End If
                Next
                exceedanceWriter.WriteLine(jsonStr)
                exceedanceWriter.WriteLine("      ]")
                exceedanceWriter.WriteLine("    }")
                exceedanceWriter.WriteLine("  ]")
                exceedanceWriter.WriteLine("};")
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteStochasticRunsJSON: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function AddStochasticRunsJSON(runspath As String, ClimateScenario As String, Duration As Integer, configurationName As String) As Boolean
        'this function adds a new configuration to the existing runs.json file
        Try
            'first we'll read the existing runs.json file
            Dim dt As New DataTable
            Dim query As String = "SELECT RUNID, RUNIDX, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4 FROM RUNS WHERE KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, True)

            Dim jsonStr As String
            Using jsonReader As New StreamReader(runspath)
                jsonStr = jsonReader.ReadToEnd                      'read the entire file
                jsonStr = jsonStr.Replace("let runs = ", "").Trim   'remove our javascript variable declaration and any line breaks
                jsonStr = jsonStr.Substring(0, jsonStr.Length - 1)            'remove the trailing semicolon which is a javascript thing
            End Using

            Dim jsonObject As JObject = JObject.Parse(jsonStr)

            ' Create a new scenario object
            Dim newScenario As New JObject()
            newScenario("ID") = configurationName
            newScenario("runs") = New JArray()

            For i As Integer = 0 To dt.Rows.Count - 1
                Dim newRun As New JObject()
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, dt.Rows.Count)
                newRun("runid") = dt.Rows(i)("RUNID").ToString()
                newRun("runidx") = dt.Rows(i)("RUNIDX").ToString()
                newRun("seizoen") = dt.Rows(i)("SEIZOEN").ToString()
                newRun("volume") = dt.Rows(i)("VOLUME").ToString()
                newRun("patroon") = dt.Rows(i)("PATROON").ToString()
                newRun("gw") = dt.Rows(i)("GW").ToString()
                newRun("boundary") = dt.Rows(i)("BOUNDARY").ToString()
                newRun("wind") = dt.Rows(i)("WIND").ToString()
                newRun("extra1") = If(dt.Rows(i)("EXTRA1") Is DBNull.Value, dt.Rows(i)("EXTRA1").ToString(), "")
                newRun("extra2") = If(dt.Rows(i)("EXTRA2") Is DBNull.Value, dt.Rows(i)("EXTRA2").ToString(), "")
                newRun("extra3") = If(dt.Rows(i)("EXTRA3") Is DBNull.Value, dt.Rows(i)("EXTRA3").ToString(), "")
                newRun("extra4") = If(dt.Rows(i)("EXTRA4") Is DBNull.Value, dt.Rows(i)("EXTRA4").ToString(), "")
                CType(newScenario("runs"), JArray).Add(newRun)
            Next

            ' Add the new scenario to the existing scenarios array
            CType(jsonObject("scenarios"), JArray).Add(newScenario)

            ' Serialize the updated object back to JSON
            Dim updatedJsonString As String = jsonObject.ToString(Formatting.Indented)

            ' Print or use the updated JSON string
            Console.WriteLine("let runs = " & updatedJsonString & ";")

            ' Write the updated JSON string back to the file
            Using jsonWriter As New StreamWriter(runspath)
                jsonWriter.WriteLine("let runs = " & updatedJsonString & ";")
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteStochasticRunsJSON: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function WriteExceedanceData1DJSON(exceedancedatapath As String, ClimateScenario As String, Duration As Integer, configurationName As String) As Boolean
        Try
            Dim locdt As New DataTable
            Dim query As String = "SELECT DISTINCT LOCATIENAAM FROM HERHALINGSTIJDEN WHERE KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locdt, False)

            'Read the runs from our database and turn them into a dictionary for faster lookups
            Dim rdt As New DataTable
            query = "SELECT DISTINCT RUNID, RUNIDX FROM RUNS WHERE KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, rdt, False)
            Dim runIdDict As New Dictionary(Of String, Integer)
            For Each row As DataRow In rdt.Rows
                runIdDict(row("RUNID").ToString()) = CInt(row("RUNIDX"))
            Next

            'write the dataset to json
            If System.IO.File.Exists(exceedancedatapath) Then System.IO.File.Delete(exceedancedatapath)
            Using exceedanceWriter As New StreamWriter(exceedancedatapath)
                exceedanceWriter.WriteLine("let exceedancedata = {")
                exceedanceWriter.WriteLine("  ""scenarios"": [")
                exceedanceWriter.WriteLine("    {")
                exceedanceWriter.WriteLine($"      ""ID"":""{configurationName}"",")
                exceedanceWriter.WriteLine("      ""locations"": [")
                exceedanceWriter.WriteLine("        {")

                Me.Setup.GeneralFunctions.UpdateProgressBar("Writing results...", 0, 10, True)
                Dim resNum As Integer = 0
                For i = 0 To locdt.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, locdt.Rows.Count)

                    'add the exceedance data to the excedancedata.js file
                    exceedanceWriter.WriteLine("            " & Chr(34) & "ID" & Chr(34) & ": " & Chr(34) & locdt.Rows(i)("LOCATIENAAM") & Chr(34) & ",")
                    exceedanceWriter.WriteLine("            " & Chr(34) & "data" & Chr(34) & ": [")

                    'for this location we will retrieve the exceedance table
                    Dim dtHerh As New DataTable
                    query = "SELECT HERHALINGSTIJD, WAARDE, RUNID FROM HERHALINGSTIJDEN WHERE LOCATIENAAM='" & locdt.Rows(i)(0) & "' AND KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & " ORDER BY HERHALINGSTIJD;"
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtHerh, False)

                    ' Add RUNIDX column if it doesn't exist
                    If Not dtHerh.Columns.Contains("RUNIDX") Then
                        dtHerh.Columns.Add("RUNIDX", GetType(Integer))
                    End If

                    ' Look up the runidx for each runid and add it to our dtHerh table
                    For Each row As DataRow In dtHerh.Rows
                        Dim runId As String = row("RUNID").ToString()
                        If runIdDict.ContainsKey(runId) Then
                            row("RUNIDX") = runIdDict(runId)
                        Else
                            row("RUNIDX") = -999
                        End If
                    Next

                    'now that we have our exceedance table, we can start writing it to the exceedancedata.js file
                    Dim exceedanceStr As String = ""
                    For j = 0 To dtHerh.Rows.Count - 1
                        exceedanceStr = "                { %x%: " & Math.Round(dtHerh.Rows(j)(0), 2) & ", %y%: " & Math.Round(dtHerh.Rows(j)(1), 3) & ", %runidx%: " & dtHerh.Rows(j)("RUNIDX") & "}" ' & ", %stochasts%:{%SEIZOEN%:%" & dtHerh.Rows(j)("SEIZOEN") & "%, %VOLUME%: " & dtHerh.Rows(j)("VOLUME") & ", %PATROON%: %" & dtHerh.Rows(j)("PATROON") & "%, %GW%: %" & dtHerh.Rows(j)("GW") & "%, %BOUNDARY%:%" & dtHerh.Rows(j)("BOUNDARY") & "%, %WIND%" & ":%" & dtHerh.Rows(j)("WIND") & "%,%EXTRA1%:%" & dtHerh.Rows(j)("EXTRA1") & "%,%EXTRA2%:%" & dtHerh.Rows(j)("EXTRA2") & "%,%EXTRA3%:%" & dtHerh.Rows(j)("EXTRA3") & "%,%EXTRA4%:%" & dtHerh.Rows(j)("EXTRA4") & "%}}"
                        If j < dtHerh.Rows.Count - 1 Then exceedanceStr &= ","
                        exceedanceStr = exceedanceStr.Replace("%", Chr(34))
                        exceedanceWriter.WriteLine(exceedanceStr)
                    Next
                    exceedanceWriter.WriteLine("            ]")
                    If i < locdt.Rows.Count - 1 Then
                        'write the closing string for the previous result before proceeding to the next
                        exceedanceWriter.WriteLine("        }, {") 'prepare for the next location to be written
                    End If
                Next

                exceedanceWriter.WriteLine("        }") 'closing statement for the last location
                exceedanceWriter.WriteLine("      ]")

                exceedanceWriter.WriteLine("    }") 'closing statement for the last scenario
                exceedanceWriter.WriteLine("  ]") 'closing statement for the last scenario

                exceedanceWriter.WriteLine("};")

            End Using
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteExceedanceDataJSON of frmStochasten: " & ex.Message)
        End Try
    End Function


    Public Function AddExceedanceData1DJSON(exceedancedatapath As String, ClimateScenario As String, Duration As Integer, configurationName As String) As Boolean
        'this function adds a new configuration to the existing EXCEEDANCEDATA1D.JS FILE    
        Try
            Dim locdt As New DataTable
            Dim query As String = "SELECT DISTINCT LOCATIENAAM FROM HERHALINGSTIJDEN WHERE KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locdt, True)

            'first read our existing exceedance data file
            Dim jsonStr As String
            Using jsonReader As New StreamReader(exceedancedatapath)
                jsonStr = jsonReader.ReadToEnd                      'read the entire file
                jsonStr = jsonStr.Replace("let exceedancedata = ", "").Trim   'remove our javascript variable declaration and any line breaks
                jsonStr = jsonStr.Substring(0, jsonStr.Length - 1)            'remove the trailing semicolon which is a javascript thing
            End Using

            'parse the exising file
            Dim jsonObject As JObject = JObject.Parse(jsonStr)

            ' Create a new scenario object
            Dim newScenario As New JObject()
            newScenario("ID") = configurationName
            newScenario("locations") = New JArray()

            For i As Integer = 0 To locdt.Rows.Count - 1
                Dim newLoc As New JObject()
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, locdt.Rows.Count)

                newLoc("ID") = New JValue(locdt.Rows(i)("LOCATIENAAM").ToString())
                newLoc("data") = New JArray()

                'Retrieve the exceedance table for this location
                Dim dtHerh As New DataTable
                query = "SELECT HERHALINGSTIJD, WAARDE, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4 FROM HERHALINGSTIJDEN WHERE LOCATIENAAM='" & locdt.Rows(i)("LOCATIENAAM").ToString() & "' AND KLIMAATSCENARIO='" & ClimateScenario & "' AND DUUR=" & Duration & " ORDER BY HERHALINGSTIJD;"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtHerh, False)

                'Add exceedance data for this location
                For j As Integer = 0 To dtHerh.Rows.Count - 1
                    Dim dataPoint As New JObject()
                    dataPoint("x") = New JValue(Math.Round(Convert.ToDouble(dtHerh.Rows(j)("HERHALINGSTIJD")), 2))
                    dataPoint("y") = New JValue(Math.Round(Convert.ToDouble(dtHerh.Rows(j)("WAARDE")), 2))
                    dataPoint("stochasts") = New JObject From {
                    {"SEIZOEN", New JValue(dtHerh.Rows(j)("SEIZOEN").ToString())},
                    {"VOLUME", New JValue(Convert.ToDouble(dtHerh.Rows(j)("VOLUME")))},
                    {"PATROON", New JValue(dtHerh.Rows(j)("PATROON").ToString())},
                    {"GW", New JValue(dtHerh.Rows(j)("GW").ToString())},
                    {"BOUNDARY", New JValue(dtHerh.Rows(j)("BOUNDARY").ToString())},
                    {"WIND", New JValue(dtHerh.Rows(j)("WIND").ToString())},
                    {"EXTRA1", New JValue(dtHerh.Rows(j)("EXTRA1").ToString())},
                    {"EXTRA2", New JValue(dtHerh.Rows(j)("EXTRA2").ToString())},
                    {"EXTRA3", New JValue(dtHerh.Rows(j)("EXTRA3").ToString())},
                    {"EXTRA4", New JValue(dtHerh.Rows(j)("EXTRA4").ToString())}
        }
                    CType(newLoc("data"), JArray).Add(dataPoint)
                Next

                CType(newScenario("locations"), JArray).Add(newLoc)
            Next

            ' Add the new scenario to the existing scenarios array
            CType(jsonObject("scenarios"), JArray).Add(newScenario)

            ' Serialize the updated object back to JSON
            Dim updatedJsonString As String = jsonObject.ToString(Formatting.Indented)

            ' Write the updated JSON string back to the file
            Using jsonWriter As New StreamWriter(exceedancedatapath)
                jsonWriter.WriteLine("let exceedancedata = " & updatedJsonString & ";")
            End Using

            Return True


        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AddExceedanceData1DJSON of frmStochasten: " & ex.Message)
        End Try
    End Function

    Public Function WriteExceedanceLevels2DFromDBToJSON(exceedancedatapath As String, ClimateScenario As String, Duration As Integer, configurationName As String) As Boolean
        Try
            Dim locdt As New DataTable
            Dim TableName As String = get2DReturnPeriodsTableName(GeneralFunctions.enm2DParameter.waterlevel, KlimaatScenario.ToString, Duration)
            Dim query As String = $"SELECT DISTINCT FEATUREIDX FROM {TableName};"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locdt, True)

            'write the dataset to json
            If System.IO.File.Exists(exceedancedatapath) Then System.IO.File.Delete(exceedancedatapath)
            Using exceedanceWriter As New StreamWriter(exceedancedatapath)
                exceedanceWriter.WriteLine("let exceedancedata2D = {")

                exceedanceWriter.WriteLine("  ""scenarios"": [")
                exceedanceWriter.WriteLine("    {")
                exceedanceWriter.WriteLine($"      ""ID"":""{configurationName}"",")
                exceedanceWriter.WriteLine("      ""locations"": [")
                exceedanceWriter.WriteLine("        {")

                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten schrijven...", 0, 10, True)
                Dim resNum As Integer = 0
                For i = 0 To locdt.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, locdt.Rows.Count)

                    If locdt.Rows.Count = 0 Then
                        Me.Setup.Log.AddError("Unable to write exceedance levels 2D to JSON for featureidx " & locdt.Rows(i)("FEATUREIDX") & ": no data found.")
                        Continue For
                    End If

                    'add the exceedance data to the excedancedata.js file
                    exceedanceWriter.WriteLine("            ""idx"": """ & locdt.Rows(i)("FEATUREIDX") & """,")

                    Dim herhStr As String = "            ""T"": ["
                    Dim levelStr As String = "            ""h"": ["
                    Dim runStr As String = "            ""runidx"": ["

                    'for this location we will retrieve the exceedance table
                    Dim dtHerh As New DataTable
                    query = $"SELECT HERHALINGSTIJD, WAARDE, RUNIDX FROM {TableName} WHERE FEATUREIDX=" & locdt.Rows(i)("FEATUREIDX") & " ORDER BY HERHALINGSTIJD;"
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtHerh, False)
                    Dim n As Integer = dtHerh.Rows.Count

                    'only write those locations where the water level is increasing
                    If dtHerh.Rows(n - 1)("WAARDE") > dtHerh.Rows(0)("WAARDE") Then

                        'now that we have our exceedance table, we can start writing it to the exceedancedata.js file
                        Dim exceedanceStr As String = ""
                        For j = 0 To dtHerh.Rows.Count - 1
                            herhStr &= Math.Round(dtHerh.Rows(j)("HERHALINGSTIJD"), 2) & ","
                            levelStr &= Math.Round(dtHerh.Rows(j)("WAARDE"), 3) & ","
                            runStr &= dtHerh.Rows(j)("RUNIDX") & ","
                        Next

                        'remove the last comma and add our closing bracket
                        herhStr = herhStr.TrimEnd(New Char() {","c}) & "],"
                        levelStr = levelStr.TrimEnd(New Char() {","c}) & "],"
                        runStr = runStr.TrimEnd(New Char() {","c}) & "]"

                        exceedanceWriter.WriteLine(herhStr)
                        exceedanceWriter.WriteLine(levelStr)
                        exceedanceWriter.WriteLine(runStr)

                        If i < locdt.Rows.Count - 1 Then
                            'write the closing string for the previous result before proceeding to the next
                            exceedanceWriter.WriteLine("        }, {") 'prepare for the next location to be written
                        End If

                    End If
                Next
                exceedanceWriter.WriteLine("        }") 'closing statement for the last location
                exceedanceWriter.WriteLine("      ]")

                exceedanceWriter.WriteLine("    }") 'closing statement for the last scenario
                exceedanceWriter.WriteLine("  ]") 'closing statement for the last scenario


                exceedanceWriter.WriteLine("};")

            End Using
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteExceedanceDataJSON of frmStochasten: " & ex.Message)
        End Try
    End Function
    Public Function WriteExceedanceLevels2DFromCSVToJSON(exceedancedatapath As String, configurationName As String) As Boolean
        Try
            'Get the paths for the CSV files
            Dim ExceedanceData2DPath As String = getExceedanceTable2DPath(ResultsDir, KlimaatScenario, GeneralFunctions.enm2DParameter.waterlevel)
            Dim StochastsPath As String = getStochastsPath(ResultsDir)

            'Use HashSet for efficient distinct FEATUREIDX tracking
            Dim distinctFeatureIdxSet As New HashSet(Of String)()

            'Use Dictionary for efficient RUNID to RUNIDX mapping
            Dim runIdToIdxMap As New Dictionary(Of String, String)()

            'Read stochasts data for RUNID to RUNIDX mapping
            Using reader As New TextFieldParser(StochastsPath)
                reader.SetDelimiters(";")
                reader.HasFieldsEnclosedInQuotes = True
                Dim headers = reader.ReadFields() ' Read header once
                Dim runIdIndex = System.Array.IndexOf(headers, "RUNID")
                Dim runIdxIndex = System.Array.IndexOf(headers, "RUNIDX")

                If runIdIndex = -1 Or runIdxIndex = -1 Then
                    Throw New Exception("Required columns 'RUNID' or 'RUNIDX' not found in the stochasts file.")
                End If

                While Not reader.EndOfData
                    Dim fields = reader.ReadFields()
                    If fields IsNot Nothing AndAlso fields.Length > Math.Max(runIdIndex, runIdxIndex) Then
                        runIdToIdxMap(fields(runIdIndex)) = fields(runIdxIndex)
                    End If
                End While
            End Using

            'Write the dataset to JSON
            If System.IO.File.Exists(exceedancedatapath) Then System.IO.File.Delete(exceedancedatapath)
            Using exceedanceWriter As New StreamWriter(exceedancedatapath)
                exceedanceWriter.WriteLine("let exceedancedata2D = {")
                exceedanceWriter.WriteLine("  ""scenarios"": [")
                exceedanceWriter.WriteLine("    {")
                exceedanceWriter.WriteLine($"      ""ID"":""{configurationName}"",")
                exceedanceWriter.WriteLine("      ""locations"": [")

                'Process exceedance data in chunks
                Const ChunkSize As Integer = 10000
                Dim isFirstLocation As Boolean = True

                Using reader As New TextFieldParser(ExceedanceData2DPath)
                    reader.SetDelimiters(";")
                    reader.HasFieldsEnclosedInQuotes = True
                    Dim headers = reader.ReadFields()
                    Dim featureIdxIndex = System.Array.IndexOf(headers, "FEATUREIDX")
                    Dim herhalingstijdIndex = System.Array.IndexOf(headers, "HERHALINGSTIJD")
                    Dim waardeIndex = System.Array.IndexOf(headers, "WAARDE")
                    Dim runIdIndex = System.Array.IndexOf(headers, "RUNID")

                    If featureIdxIndex = -1 Or herhalingstijdIndex = -1 Or waardeIndex = -1 Or runIdIndex = -1 Then
                        Throw New Exception("Required columns not found in the exceedance data file.")
                    End If

                    Dim chunk As New List(Of String())(ChunkSize)
                    While Not reader.EndOfData
                        chunk.Clear()
                        While chunk.Count < ChunkSize AndAlso Not reader.EndOfData
                            Dim fields = reader.ReadFields()
                            If fields IsNot Nothing Then
                                chunk.Add(fields)
                            End If
                        End While

                        'Process chunk
                        Dim groupedData = chunk.GroupBy(Function(fields) fields(featureIdxIndex))
                        For Each group In groupedData
                            Dim featureIdx = group.Key

                            If distinctFeatureIdxSet.Add(featureIdx) Then
                                Dim sortedGroup = group.OrderBy(Function(fields) Double.Parse(fields(herhalingstijdIndex))).ToList()

                                'Only write those locations where the water level is increasing
                                If Double.Parse(sortedGroup.Last()(waardeIndex)) > Double.Parse(sortedGroup.First()(waardeIndex)) Then
                                    If Not isFirstLocation Then exceedanceWriter.WriteLine("        },")
                                    isFirstLocation = False

                                    exceedanceWriter.WriteLine("        {")
                                    exceedanceWriter.WriteLine($"            ""idx"": ""{featureIdx}"",")

                                    Dim herhValues = New List(Of String)()
                                    Dim levelValues = New List(Of String)()
                                    Dim runIdxValues = New List(Of String)()

                                    For Each fields In sortedGroup
                                        Dim herhalingstijd As Double
                                        Dim waarde As Double
                                        If Double.TryParse(fields(herhalingstijdIndex), herhalingstijd) AndAlso Double.TryParse(fields(waardeIndex), waarde) Then
                                            herhValues.Add(Math.Round(herhalingstijd, 2).ToString())
                                            levelValues.Add(Math.Round(waarde, 3).ToString("F3"))
                                            Dim runIdxValue As String = ""
                                            If runIdToIdxMap.TryGetValue(fields(runIdIndex), runIdxValue) Then
                                                runIdxValues.Add(runIdxValue)
                                            Else
                                                runIdxValues.Add("")
                                            End If
                                        End If
                                    Next

                                    exceedanceWriter.WriteLine($"            ""T"": [{String.Join(",", herhValues)}],")
                                    exceedanceWriter.WriteLine($"            ""h"": [{String.Join(",", levelValues)}],")
                                    exceedanceWriter.WriteLine($"            ""runidx"": [{String.Join(",", runIdxValues)}]")
                                End If
                            End If
                        Next
                    End While
                End Using

                exceedanceWriter.WriteLine("        }")
                exceedanceWriter.WriteLine("      ]")
                exceedanceWriter.WriteLine("    }")
                exceedanceWriter.WriteLine("  ]")
                exceedanceWriter.WriteLine("};")
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteExceedanceLevels2DFromCSVToJSON: " & ex.Message)
            Return False
        End Try
    End Function

    'Public Function WriteExceedanceLevels2DFromCSVToJSON(exceedancedatapath As String, configurationName As String) As Boolean
    '    Try
    '        'Get the paths for the CSV files
    '        Dim ExceedanceData2DPath As String = getExceedanceTable2DPath(ResultsDir, KlimaatScenario, GeneralFunctions.enm2DParameter.waterlevel)
    '        Dim StochastsPath As String = getStochastsPath(ResultsDir)

    '        'Read the exceedance data and stochasts into DataTables
    '        Dim exceedanceData As New DataTable
    '        Dim stochastsData As New DataTable

    '        Using reader As New TextFieldParser(ExceedanceData2DPath)
    '            reader.SetDelimiters(",")
    '            reader.HasFieldsEnclosedInQuotes = True

    '            'Read column names
    '            Dim columns() As String = reader.ReadFields()
    '            For Each column In columns
    '                exceedanceData.Columns.Add(column)
    '            Next

    '            'Read data
    '            While Not reader.EndOfData
    '                exceedanceData.Rows.Add(reader.ReadFields())
    '            End While
    '        End Using

    '        Using reader As New TextFieldParser(StochastsPath)
    '            reader.SetDelimiters(",")
    '            reader.HasFieldsEnclosedInQuotes = True

    '            'Read column names
    '            Dim columns() As String = reader.ReadFields()
    '            For Each column In columns
    '                stochastsData.Columns.Add(column)
    '            Next

    '            'Read data
    '            While Not reader.EndOfData
    '                stochastsData.Rows.Add(reader.ReadFields())
    '            End While
    '        End Using

    '        'Get distinct FEATUREIDX values
    '        Dim locdt = (From row In exceedanceData.Rows
    '                     Select CStr(row("FEATUREIDX"))).Distinct().ToList()

    '        'Write the dataset to JSON
    '        If System.IO.File.Exists(exceedancedatapath) Then System.IO.File.Delete(exceedancedatapath)
    '        Using exceedanceWriter As New StreamWriter(exceedancedatapath)
    '            exceedanceWriter.WriteLine("let exceedancedata2D = {")
    '            exceedanceWriter.WriteLine("  ""scenarios"": [")
    '            exceedanceWriter.WriteLine("    {")
    '            exceedanceWriter.WriteLine($"      ""ID"":""{configurationName}"",")
    '            exceedanceWriter.WriteLine("      ""locations"": [")

    '            Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten schrijven...", 0, 10, True)
    '            For i = 0 To locdt.Count - 1
    '                Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, locdt.Count)

    '                Dim featureIdx = locdt(i)
    '                Dim dtHerh As DataTable = exceedanceData.Clone() ' Create an empty table with the same schema

    '                ' Fill dtHerh with the filtered and sorted rows
    '                For Each row As DataRow In exceedanceData.Select($"FEATUREIDX = '{featureIdx}'", "HERHALINGSTIJD ASC")
    '                    dtHerh.ImportRow(row)
    '                Next

    '                If dtHerh.Rows.Count = 0 Then
    '                    Me.Setup.Log.AddError($"Unable to write exceedance levels 2D to JSON for featureidx {featureIdx}: no data found.")
    '                    Continue For
    '                End If

    '                'Only write those locations where the water level is increasing
    '                If CDbl(dtHerh.Rows(dtHerh.Rows.Count - 1)("WAARDE")) > CDbl(dtHerh.Rows(0)("WAARDE")) Then
    '                    exceedanceWriter.WriteLine("        {")
    '                    exceedanceWriter.WriteLine($"            ""idx"": ""{featureIdx}"",")

    '                    Dim herhStr As String = "            ""T"": ["
    '                    Dim levelStr As String = "            ""h"": ["
    '                    Dim runStr As String = "            ""runidx"": ["

    '                    For j = 0 To dtHerh.Rows.Count - 1
    '                        herhStr &= Math.Round(CDbl(dtHerh.Rows(j)("HERHALINGSTIJD")), 2) & ","
    '                        levelStr &= Math.Round(CDbl(dtHerh.Rows(j)("WAARDE")), 2) & ","
    '                        Dim runID = CStr(dtHerh.Rows(j)("RUNID"))
    '                        Dim runIdx = (From row In stochastsData.Rows
    '                                      Where CStr(row("RUNID")) = runID
    '                                      Select CStr(row("RUNIDX"))).FirstOrDefault()
    '                        runStr &= If(runIdx IsNot Nothing, runIdx, "") & ","
    '                    Next

    '                    'Remove the last comma and add our closing bracket
    '                    herhStr = herhStr.TrimEnd(","c) & "],"
    '                    levelStr = levelStr.TrimEnd(","c) & "],"
    '                    runStr = runStr.TrimEnd(","c) & "]"

    '                    exceedanceWriter.WriteLine(herhStr)
    '                    exceedanceWriter.WriteLine(levelStr)
    '                    exceedanceWriter.WriteLine(runStr)

    '                    If i < locdt.Count - 1 Then
    '                        exceedanceWriter.WriteLine("        },")
    '                    Else
    '                        exceedanceWriter.WriteLine("        }")
    '                    End If
    '                End If
    '            Next

    '            exceedanceWriter.WriteLine("      ]")
    '            exceedanceWriter.WriteLine("    }")
    '            exceedanceWriter.WriteLine("  ]")
    '            exceedanceWriter.WriteLine("};")
    '        End Using

    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function WriteExceedanceLevels2DFromCSVToJSON: " & ex.Message)
    '        Return False
    '    End Try
    'End Function




    Public Function AddExceedanceLevels2DJSON(exceedancedatapath As String, ClimateScenario As String, Duration As Integer, configurationName As String) As Boolean
        Try
            Dim locdt As New DataTable
            Dim TableName As String = get2DReturnPeriodsTableName(GeneralFunctions.enm2DParameter.waterlevel, KlimaatScenario.ToString, Duration)
            Dim query As String = $"SELECT DISTINCT FEATUREIDX FROM {TableName};"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locdt, True)

            'Read existing JSON file
            Dim jsonStr As String
            Using jsonReader As New StreamReader(exceedancedatapath)
                jsonStr = jsonReader.ReadToEnd()
                jsonStr = jsonStr.Replace("let exceedancedata2D = ", "").Trim()
                jsonStr = jsonStr.Substring(0, jsonStr.Length - 1) 'remove trailing semicolon
            End Using

            'Parse existing JSON
            Dim jsonObject As JObject = JObject.Parse(jsonStr)

            'Create new scenario object
            Dim newScenario As New JObject()
            newScenario("ID") = New JValue(configurationName)
            newScenario("locations") = New JArray()

            Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten schrijven...", 0, 10, True)
            For i As Integer = 0 To locdt.Rows.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, locdt.Rows.Count)

                If locdt.Rows.Count = 0 Then
                    Me.Setup.Log.AddError("Unable to write exceedance levels 2D to JSON for featureidx " & locdt.Rows(i)("FEATUREIDX") & ": no data found.")
                    Continue For
                End If

                'Retrieve exceedance table for this location
                Dim dtHerh As New DataTable
                query = $"SELECT HERHALINGSTIJD, WAARDE, RUNIDX FROM {TableName} WHERE FEATUREIDX=" & locdt.Rows(i)("FEATUREIDX") & " ORDER BY HERHALINGSTIJD;"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtHerh, False)
                Dim n As Integer = dtHerh.Rows.Count

                'Only write those locations where the water level is increasing
                If dtHerh.Rows(n - 1)("WAARDE") > dtHerh.Rows(0)("WAARDE") Then
                    Dim newLoc As New JObject()
                    newLoc("idx") = New JValue(locdt.Rows(i)("FEATUREIDX").ToString())

                    Dim herhArray As New JArray()
                    Dim levelArray As New JArray()
                    Dim runArray As New JArray()

                    For j As Integer = 0 To dtHerh.Rows.Count - 1
                        herhArray.Add(New JValue(Math.Round(Convert.ToDouble(dtHerh.Rows(j)("HERHALINGSTIJD")), 2)))
                        levelArray.Add(New JValue(Math.Round(Convert.ToDouble(dtHerh.Rows(j)("WAARDE")), 2)))
                        runArray.Add(New JValue(Convert.ToInt32(dtHerh.Rows(j)("RUNIDX"))))
                    Next

                    newLoc("T") = herhArray
                    newLoc("h") = levelArray
                    newLoc("runidx") = runArray

                    CType(newScenario("locations"), JArray).Add(newLoc)
                End If
            Next

            'Add new scenario to existing scenarios array
            CType(jsonObject("scenarios"), JArray).Add(newScenario)

            'Serialize updated object back to JSON
            Dim updatedJsonString As String = jsonObject.ToString(Formatting.Indented)

            'Write updated JSON string back to file
            Using jsonWriter As New StreamWriter(exceedancedatapath)
                jsonWriter.WriteLine("let exceedancedata2D = " & updatedJsonString & ";")
            End Using

            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AddExceedanceLevels2DJSON of frmStochasten: " & ex.Message)
            Return False
        End Try
    End Function



    Public Function ClassifyGroundwaterRRBySeason(ByVal seizoen As STOCHLIB.GeneralFunctions.enmSeason, ByVal Duration As Integer, ByRef myCase As ClsSobekCase, ByRef myDataGrid As Windows.Forms.DataGridView, seizoensnaam As String, ByRef Dates As List(Of Date), IncludeSeepage As Boolean, ExportDir As String) As Boolean
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
            Dim repGW As Double          'representative groundwater level for the class
            Dim sepRecord As clsUnpavedSEPRecord

            'carry out the POT analysis on the rainfall volumes
            If Not Setup.TijdreeksStatistiek.CalcPOTEvents(Duration, seizoen.ToString, False) Then Throw New Exception("Error executing a POT-analysis on precipitation volumes for the " & seizoen.ToString)

            'now store the starting timesteps for each rainfall event in a list
            'note: these time step indices are valid for ALL unpaved.3b records
            Dim myTijdreeks As clsModelTimeSeries = Setup.TijdreeksStatistiek.NeerslagReeksen.Values(0)
            mySeason = myTijdreeks.Seasons.GetByEnum(seizoen)
            myDuration = mySeason.GetDuration(Duration)
            TimeSteps = New List(Of Long)
            TimeSteps = myDuration.POTEvents.GetStartingTimeSteps

            'read the his results for all location and the previously defined initial timesteps per event
            Dim myGroundwaterLevel As New List(Of STOCHLIB.HisDataRow) 'all groundwater levels for a single timestep
            Dim myGroundwaterLevels As New List(Of List(Of STOCHLIB.HisDataRow)) 'all groundwater levels
            Dim mySeepageValue As New List(Of STOCHLIB.HisDataRow)     'all seepage values for a single timestep
            Dim mySeepageValues As New List(Of List(Of STOCHLIB.HisDataRow))        'all seepage values
            myCase.RRResults.UPFLODT.OpenFile()

            'first read the groundwater levels
            myPar = myCase.RRResults.UPFLODT.getParName("Groundw.Level")
            For ts = 0 To TimeSteps.Count - 1
                myDate = Dates(TimeSteps(ts))
                myGroundwaterLevel = myCase.RRResults.UPFLODT.ReadTimeStep(myDate, myPar)
                myGroundwaterLevels.Add(myGroundwaterLevel)
            Next

            'now read the seepage values, if required
            If IncludeSeepage Then
                myPar = myCase.RRResults.UPFLODT.getParName("Seepage")
                For ts = 0 To TimeSteps.Count - 1
                    myDate = Dates(TimeSteps(ts))
                    mySeepageValue = myCase.RRResults.UPFLODT.ReadTimeStep(myDate, myPar)
                    mySeepageValues.Add(mySeepageValue)
                Next
            End If

            myCase.RRResults.UPFLODT.Close()

            'now walk through all rows of the classification datagridview to decide which percentile we'll use
            For Each myRow As DataGridViewRow In myDataGrid.Rows

                If IncludeSeepage Then
                    'remove the existing seepage records from the case. We will create new ones
                    myCase.RRData.UnpavedSep.Records.Clear()
                End If

                'determine the boundaries of the current groundwater class
                lPercentile = myRow.Cells(1).Value
                uPercentile = myRow.Cells(2).Value
                repPerc = (lPercentile + uPercentile) / 2

                n = myCase.RRData.Unpaved3B.Records.Count
                j = 0

                'now that we have the starting time index numbers for the POT-events, we can get the groundwater levels from the starting times for each event
                For Each upRecord In myCase.RRData.Unpaved3B.Records.Values

                    Debug.Print("Processing unpaved record " & upRecord.ID)

                    sepRecord = New clsUnpavedSEPRecord(Me.Setup)
                    sepRecord.ID = upRecord.SP
                    sepRecord.nm = upRecord.SP
                    sepRecord.co = 1
                    sepRecord.ss = 0
                    myCase.RRData.UnpavedSep.Records.Add(sepRecord.ID.Trim.ToUpper, sepRecord)

                    j += 1
                    k = -1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Groundwater classification for season " & seizoen.ToString & " and class " & myRow.Cells("colNaam").Value & ".", j, n)
                    ws = Me.Setup.ExcelFile.GetAddSheet(seizoensnaam)
                    ws.ws.Cells(0, 0).Value = "Datum"
                    ws.ws.Cells(0, j).Value = upRecord.ID

                    'get the results for this record from the lists
                    Dim GW(0 To TimeSteps.Count - 1) As Double
                    For Each myResult In myGroundwaterLevels
                        For Each Result As STOCHLIB.HisDataRow In myResult
                            'Debug.Print("Location name is " & Result.LocationName)
                            If Result.LocationName = upRecord.ID Then
                                k += 1
                                GW(k) = Result.Value 'store the results in a temporary array to allow percentile computation later
                                ws.ws.Cells(k + 1, 0).Value = Result.TimeStep
                                ws.ws.Cells(k + 1, j).Value = Result.Value
                            End If
                        Next
                    Next

                    'get the seepage values for this record from the lists
                    Dim Seepage(0 To TimeSteps.Count - 1) As Double
                    If IncludeSeepage Then
                        k = -1
                        For Each myResult In mySeepageValues
                            For Each Result As STOCHLIB.HisDataRow In myResult
                                If Result.LocationName = upRecord.ID Then
                                    k += 1
                                    'we must still convert our seepage values from m3/s to mm/d
                                    Seepage(k) = Result.Value / upRecord.ga * 1000 * 3600 * 24   'store the results in a temporary array to allow percentile computation later
                                    'Seepage(k) = Result.Value 'store the results in a temporary array to allow percentile computation later
                                End If
                            Next
                        Next
                    End If

                    'update the unpaved.3b record
                    repGW = Me.Setup.GeneralFunctions.Percentile(GW, repPerc)
                    upRecord.ig = 0
                    upRecord.igconst = Math.Round(upRecord.lv - repGW, 2)

                    'update the unpaved.sep record
                    If IncludeSeepage Then

                        'the representative seepage value is linked to the groundwater levels inside this class
                        'this means that we must derive the median of the seapage values that are linked to the groundwater levels from thi sclass
                        Dim SeepageInClass As New List(Of Double)
                        For k = 0 To Seepage.Length - 1
                            If GW(k) >= Me.Setup.GeneralFunctions.Percentile(GW, lPercentile) And GW(k) <= Me.Setup.GeneralFunctions.Percentile(GW, uPercentile) Then
                                SeepageInClass.Add(Seepage(k))
                            End If
                        Next

                        'compute the median of the seepage values in this class and assign it to the seepage record
                        sepRecord.sp = Me.Setup.GeneralFunctions.PercentileFromList(SeepageInClass, 0.5)
                    End If

                    'write the statistics to Excel
                    ws = Me.Setup.ExcelFile.GetAddSheet(seizoensnaam & ".Stats")
                    Dim r As Integer = If(IncludeSeepage, myRow.Index * 4, myRow.Index * 3)
                    ws.ws.Cells(0, j + 1).Value = upRecord.ID
                    ws.ws.Cells(r + 1, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 1, 1).Value = "Percentiel " & repPerc
                    ws.ws.Cells(r + 1, j + 1).Value = Math.Round(repGW, 2)
                    ws.ws.Cells(r + 2, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 2, 1).Value = "Maaiveldhoogte (m NAP)"
                    ws.ws.Cells(r + 2, j + 1).Value = upRecord.lv
                    ws.ws.Cells(r + 3, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 3, 1).Value = "Grondwaterdiepte (m)"
                    ws.ws.Cells(r + 3, j + 1).Value = Math.Round(upRecord.lv - repGW, 2)
                    If IncludeSeepage Then
                        ws.ws.Cells(r + 4, 0).Value = myRow.Cells(0).Value
                        ws.ws.Cells(r + 4, 1).Value = "Kwel (mm/d)"
                        ws.ws.Cells(r + 4, j + 1).Value = Math.Round(sepRecord.sp, 2)
                    End If
                Next

                'set path to the adjusted unpaved.3b and unpaved.sep file and initialize writing the groundwater level values
                Dim mypath As String = ExportDir & "\" & seizoen.ToString & "_" & myRow.Cells(0).Value & "\"
                If Not System.IO.Directory.Exists(mypath) Then System.IO.Directory.CreateDirectory(mypath)
                myCase.RRData.Unpaved3B.Write(mypath & "unpaved.3b", False)
                If IncludeSeepage Then myCase.RRData.UnpavedSep.Write(mypath & "unpaved.sep", False)

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ClassifyGroundwaterBySeason of class frmClassifyGroundWater.")
            Return False
        End Try


    End Function

    Public Function ClassifyGroundwaterHBVBySeason(ModelParameters As clsModelParameterClass, TimeseriesStatistic As GeneralFunctions.enmTimestepStatistic, ByVal seizoen As STOCHLIB.GeneralFunctions.enmSeason, ByVal Duration As Integer, ByVal HBVReportPath As String, ByRef myDataGrid As Windows.Forms.DataGridView, seizoensnaam As String, ByRef Dates As List(Of Date), ExportDir As String) As Boolean
        Try
            Dim mySeason As clsSeason
            Dim myDuration As clsDuration
            Dim j As Long = 0
            Dim ws As clsExcelSheet

            'Dim parameters As New List(Of GeneralFunctions.enmModelParameter) From {GeneralFunctions.enmModelParameter.lz, GeneralFunctions.enmModelParameter.uz, GeneralFunctions.enmModelParameter.sm}

            'first carry out the POT analysis on the rainfall volumes
            If Not Setup.TijdreeksStatistiek.CalcPOTEvents(Duration, seizoen.ToString, False) Then Throw New Exception("Error executing a POT-analysis on precipitation volumes for the " & seizoen.ToString)

            'next we need to assign each of our POT events to its corresponding percentile class
            'for this we will read our percentiles from the datagrid
            Dim PercentileClassesTemplate As New clsPercentileClasses(Me.Setup)
            PercentileClassesTemplate.ReadFromDataGrid(myDataGrid)

            'now retrieve the combination of season and duration and initialize the parameter classification for that combination
            For Each mySeries As clsModelTimeSeries In Setup.TijdreeksStatistiek.NeerslagReeksen.Values
                mySeason = mySeries.Seasons.GetAdd(seizoen)
                myDuration = mySeason.GetAddDuration(Duration)

                'first we classify by the main parameter
                myDuration.POTEvents.CalculateParameterClassification(ModelParameters, TimeseriesStatistic, PercentileClassesTemplate)


                'finally we will write the instate.dat files for each combination percentile classes
                For Each myKey As String In myDuration.POTEvents.PercentileClassifications.Classifications.Keys
                    Dim instatepath As String = ExportDir & "\" & myDuration.DurationHours.ToString & "\" & seizoen.ToString & "\" & myKey & "\" & mySeries.Name & "\instate.dat"
                    Dim state01path As String = ExportDir & "\" & myDuration.DurationHours.ToString & "\" & seizoen.ToString & "\" & myKey & "\" & mySeries.Name & "\state_01.dat"

                    Select Case seizoen
                        Case GeneralFunctions.enmSeason.yearround
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 1, 1))
                        Case GeneralFunctions.enmSeason.meteowinterhalfyear
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 1, 1))
                        Case GeneralFunctions.enmSeason.meteosummerhalfyear
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 6, 1))
                        Case GeneralFunctions.enmSeason.meteosummerquarter
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 8, 10))
                        Case GeneralFunctions.enmSeason.meteoautumnquarter
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 11, 10))
                        Case GeneralFunctions.enmSeason.meteowinterquarter
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 2, 10))
                        Case GeneralFunctions.enmSeason.meteospringquarter
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 5, 10))
                        Case GeneralFunctions.enmSeason.hydrosummerhalfyear
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 6, 1))
                        Case GeneralFunctions.enmSeason.hydrowinterhalfyear
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 1, 1))
                        Case GeneralFunctions.enmSeason.marchthroughoctober
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 6, 1))
                        Case GeneralFunctions.enmSeason.novemberthroughfebruary
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 1, 1))
                        Case GeneralFunctions.enmSeason.aprilthroughaugust
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 6, 15))
                        Case GeneralFunctions.enmSeason.septemberthroughmarch
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 12, 15))
                        Case GeneralFunctions.enmSeason.growthseason
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2010, 6, 1))
                        Case GeneralFunctions.enmSeason.outsidegrowthseason
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2010, 1, 1))
                        Case Else
                            myDuration.POTEvents.PercentileClassifications.Classifications.Item(myKey).WriteHBVStateFiles(instatepath, state01path, New Date(2000, 1, 1))
                    End Select

                Next
            Next



            '    'now walk through all rows of the classification datagridview to decide which percentile we'll use
            '    j = 0
            '    For Each myRow As DataGridViewRow In myDataGrid.Rows
            '        j += 1

            '        Me.Setup.GeneralFunctions.UpdateProgressBar("Groundwater classification for season " & seizoen.ToString & " and class " & myRow.Cells("colNaam").Value & ".", j, n)

            '        'determine the boundaries of the current groundwater class
            '        lPercentile = myRow.Cells(1).Value
            '        uPercentile = myRow.Cells(2).Value
            '        repPerc = (lPercentile + uPercentile) / 2

            '        Dim repLZ As Double = Me.Setup.GeneralFunctions.Percentile(LZ, repPerc)
            '        Dim repUZ As Double = Me.Setup.GeneralFunctions.Percentile(UZ, repPerc)
            '        Dim repSM As Double = Me.Setup.GeneralFunctions.Percentile(SM, repPerc)

            '        ws = Me.Setup.ExcelFile.GetAddSheet(hbvsheet.Name & "_" & seizoensnaam)
            '        ws.ws.Cells(0, 0).Value = "Repr. percentile"
            '        ws.ws.Cells(0, 1).Value = "LZ"
            '        ws.ws.Cells(0, 2).Value = "UZ"
            '        ws.ws.Cells(0, 3).Value = "SM"
            '        ws.ws.Cells(j, 0).Value = repPerc
            '        ws.ws.Cells(j, 1).Value = repLZ
            '        ws.ws.Cells(j, 2).Value = repUZ
            '        ws.ws.Cells(j, 3).Value = repSM

            '        'set path to a HBV instate.dat file and initialize writing the groundwater and soil moisture values
            '        Dim mypath As String = ExportDir & "\" & seizoen.ToString & "_" & myRow.Cells(0).Value & "\" & hbvsheet.Name & "\instate.dat"

            '    Next
            'Next



            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ClassifyGroundwaterBySeason of class frmClassifyGroundWater.")
            Return False
        End Try


    End Function

    Public Function getParameterIndex(parameters As List(Of GeneralFunctions.enmModelParameter), Parameter As GeneralFunctions.enmModelParameter) As Integer
        If parameters.Contains(Parameter) Then
            Return parameters.IndexOf(Parameter)
        Else
            Return -1
        End If
    End Function

    Function GetParameterPercentileCombinations(ByVal parameters As List(Of String), ByVal percentileClasses As Dictionary(Of String, clsPercentileClass)) As Dictionary(Of String, clsParameterPercentileCombination)
        Try
            Dim ParameterPercentileCombinations As New Dictionary(Of String, clsParameterPercentileCombination)
            Dim currentCombination As New List(Of String)

            ' Start the recursive process
            AddParameterPercentileCombinationRecursive(parameters, percentileClasses, 0, "", New List(Of String), New List(Of clsPercentileClass), ParameterPercentileCombinations)

            Return ParameterPercentileCombinations
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetParameterPercentileCombinations of class clsStochastenAnalyse: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Sub AddParameterPercentileCombinationRecursive(ByVal parameters As List(Of String), ByVal percentileClasses As Dictionary(Of String, clsPercentileClass), ByVal currentIndex As Integer, ByVal currentName As String, ByVal currentParameters As List(Of String), ByVal currentPercentileClasses As List(Of clsPercentileClass), ByRef combinations As Dictionary(Of String, clsParameterPercentileCombination))
        If currentIndex >= parameters.Count Then
            ' Base case: all parameters processed
            Dim Combination As New clsParameterPercentileCombination(Me.Setup)
            Combination.Name = currentName.TrimEnd("_"c)
            Combination.Parameters.AddRange(currentParameters)
            Combination.PercentileClasses.AddRange(currentPercentileClasses)
            combinations.Add(Combination.Name.ToUpper, Combination)
        Else
            ' Recursive case: process each percentile class for the current parameter
            For Each percentileClass As clsPercentileClass In percentileClasses.Values
                Dim newName As String = If(currentName = "", "", currentName & "_") & parameters(currentIndex) & "_" & percentileClass.Name
                Dim newParameters As New List(Of String)(currentParameters) From {
                parameters(currentIndex)
            }
                Dim newPercentileClasses As New List(Of clsPercentileClass)(currentPercentileClasses) From {
                percentileClass
            }

                AddParameterPercentileCombinationRecursive(parameters, percentileClasses, currentIndex + 1, newName, newParameters, newPercentileClasses, combinations)
            Next
        End If
    End Sub


    'Function GetParameterPercentileCombinations(ByVal parameters As List(Of String), ByVal percentileClasses As Dictionary(Of String, clsPercentileClass)) As Dictionary(Of String, clsParameterPercentileCombination)
    '    Try
    '        Dim ParameterPercentileCombinations As New Dictionary(Of String, clsParameterPercentileCombination)
    '        If parameters.Count = 1 Then
    '            For Each percentileclass As clsPercentileClass In percentileClasses.Values
    '                Dim Combination As New clsParameterPercentileCombination(Me.Setup)
    '                Combination.Name = parameters(0) & "_" & percentileclass.Name
    '                Combination.Parameters.Add(parameters(0))
    '                Combination.PercentileClasses.Add(percentileclass)
    '                ParameterPercentileCombinations.Add(Combination.Name.Trim.ToUpper, Combination)
    '            Next
    '        ElseIf parameters.Count = 2 Then
    '            For Each firstPercentileclass As clsPercentileClass In percentileClasses.Values
    '                For Each secondPercentileClass As clsPercentileClass In percentileClasses.Values
    '                    Dim Combination As New clsParameterPercentileCombination(Me.Setup)
    '                    Combination.Name = parameters(0) & "_" & firstPercentileclass.Name & "_" & parameters(1) & "_" & secondPercentileClass.Name
    '                    Combination.Parameters.Add(parameters(0))
    '                    Combination.Parameters.Add(parameters(1))
    '                    Combination.PercentileClasses.Add(firstPercentileclass)
    '                    Combination.PercentileClasses.Add(secondPercentileClass)
    '                    ParameterPercentileCombinations.Add(Combination.Name.Trim.ToUpper, Combination)
    '                Next
    '            Next
    '        Else
    '            Throw New Exception("Error: invalid number of parameters to classify. Only 1 or 2 parameters are supported.")
    '        End If

    '        Return ParameterPercentileCombinations
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function GetParameterPercentileCombinations of class clsStochastenAnalyse: " & ex.Message
    '        Return Nothing
    '    End Try

    'End Function

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
                myModel = New clsSimulationModel(Me.Setup, dtModel.Rows(i)("MODELID"), dtModel.Rows(i)("MODELTYPE"), dtModel.Rows(i)("EXECUTABLE"), dtModel.Rows(i)("ARGUMENTS"), dtModel.Rows(i)("MODELDIR"), dtModel.Rows(i)("CASENAME"), dtModel.Rows(i)("TEMPWORKDIR"), dtModel.Rows(i)("RESULTSFILES_RR"), dtModel.Rows(i)("RESULTSFILES_FLOW"), dtModel.Rows(i)("RESULTSFILES_RTC"))
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
                            Dim FileNames As String = "", FileName As String = "", RootFolder
                            Setup.GeneralFunctions.UpdateProgressBar("Populating groundwater for " & mySeason.Name, 2, 10, True)
                            For Each GwRow As DataGridViewRow In grGroundwater.Rows
                                If GwRow.Cells("USE").Value = 1 Then
                                    myGW = New clsStochasticGroundwaterClass(GwRow.Cells("NAAM").Value, GwRow.Cells("KANS").Value)

                                    'v2.205: introducing multi-file support for this stochast. Also introduce distinction between RR, Flow and RTC
                                    If Not IsDBNull(GwRow.Cells("FOLDER").Value) Then
                                        RootFolder = GwRow.Cells("FOLDER").Value
                                        myGW.SetFolder(RootFolder)
                                    End If


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

        'code optimized by ChatGPT plus
        Try
            ' Initialize variables
            Dim myQuery As String = ""
            Dim pCum As Double = 0
            Dim i As Integer = 0
            Dim ValuesField As String = ""

            ' Determine the values field based on the filter
            Select Case Filter.Trim.ToUpper
                Case "MAX", "MAXVAL"
                    ValuesField = "MAXVAL"
                Case "MIN", "MINVAL"
                    ValuesField = "MINVAL"
                Case Else
                    Throw New ArgumentException("Timeseries filter on results not supported: " & Filter)
            End Select

            ' Create the Herh table and add columns
            dtHerh = New DataTable
            dtHerh.Columns.Add("Herhalingstijd", GetType(Double))
            dtHerh.Columns.Add("Waarde", GetType(Double))
            dtHerh.Columns.Add("RUNID", GetType(String))

            ' Build the SQL query using parameterized queries
            myQuery = "SELECT RUNID, " & ValuesField & ", P FROM RESULTATEN WHERE LOCATIENAAM = @LocationName AND KLIMAATSCENARIO = @KlimaatScenario AND DUUR = @Duration ORDER BY " & ValuesField & " ASC;"
            Using cmd As New SQLiteCommand(myQuery, Me.Setup.SqliteCon)
                cmd.Parameters.AddWithValue("@LocationName", LocationName)
                cmd.Parameters.AddWithValue("@KlimaatScenario", Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper)
                cmd.Parameters.AddWithValue("@Duration", Setup.StochastenAnalyse.Duration)
                Using reader As SQLiteDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        pCum += reader.GetDouble(reader.GetOrdinal("P"))
                        Dim value As Double = reader.GetDouble(reader.GetOrdinal(ValuesField))
                        Dim runId As String = reader.GetString(reader.GetOrdinal("RUNID"))
                        Dim herhTijd As Double
                        If Setup.StochastenAnalyse.VolumesAsFrequencies Then
                            Dim maxFreq As Double = 365.25 * 24 / Setup.StochastenAnalyse.Duration
                            If pCum < maxFreq Then
                                herhTijd = 1 / (maxFreq - pCum)
                            Else
                                ' Use a simple approximation for the highest value, which is not computable due to division by zero
                                herhTijd = dtHerh.Rows(i - 1)(0) + 1
                            End If
                        Else
                            herhTijd = 1 / -Math.Log(pCum)
                        End If
                        dtHerh.Rows.Add(herhTijd, value, runId)
                        i += 1
                    End While
                End Using
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalcExceedanceTable.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function CalcExceedanceTables2DFromCSV(ByRef dtHerh As Dictionary(Of Integer, DataTable), Parameter As GeneralFunctions.enm2DParameter) As Boolean
        Try
            'set the path to the 2D results

            Dim csvFile As String = getResults2DPath(ResultsDir, KlimaatScenario, Parameter)
            If Not System.IO.File.Exists(csvFile) Then Throw New ArgumentException("2D results file not found: " & csvFile)

            'read all relevant information about our runs
            Dim rt As New DataTable
            Dim query As String = $"SELECT RUNID, P FROM RUNS WHERE KLIMAATSCENARIO = '{KlimaatScenario}' AND DUUR = {Duration};"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, rt, True)

            'the csv file is comma-separated and contains no header. The first column contains the RunID, the remaining columns contain the values for each feature in the 2D mesh. FeatureIdx = column number -1

            'Create a temporary dictionary to store raw data
            Dim rawData As New Dictionary(Of Integer, List(Of Tuple(Of String, Double, Double)))

            'Read the CSV file
            Using reader As New Microsoft.VisualBasic.FileIO.TextFieldParser(csvFile)
                reader.TextFieldType = FileIO.FieldType.Delimited
                reader.SetDelimiters(",")

                While Not reader.EndOfData
                    Dim fields As String() = reader.ReadFields()
                    Dim runId As String = fields(0)

                    'Find the corresponding P value for this runId
                    Dim p As Double = rt.Select($"RUNID = '{runId}'")(0)("P")

                    For i As Integer = 1 To fields.Length - 1
                        Dim featureIdx As Integer = i - 1
                        Dim value As Double = Double.Parse(fields(i))

                        If Not rawData.ContainsKey(featureIdx) Then
                            rawData(featureIdx) = New List(Of Tuple(Of String, Double, Double))
                        End If

                        rawData(featureIdx).Add(New Tuple(Of String, Double, Double)(runId, value, p))
                    Next
                End While
            End Using

            'Process the raw data into exceedance tables
            For Each featureIdx In rawData.Keys
                Dim dt As New DataTable
                dt.Columns.Add("Herhalingstijd", GetType(Double))
                dt.Columns.Add("Waarde", GetType(Double))
                dt.Columns.Add("RUNID", GetType(String))
                dt.Columns.Add("FeatureIdx", GetType(Integer))

                Dim sortedData = rawData(featureIdx).OrderBy(Function(x) x.Item2).ToList()
                Dim pCum As Double = 0

                For Each item In sortedData
                    Dim runId As String = item.Item1
                    Dim value As Double = item.Item2
                    Dim p As Double = item.Item3

                    pCum += p

                    Dim herhTijd As Double
                    If Setup.StochastenAnalyse.VolumesAsFrequencies Then
                        Dim maxFreq As Double = 365.25 * 24 / Setup.StochastenAnalyse.Duration
                        If pCum < maxFreq Then
                            herhTijd = 1 / (maxFreq - pCum)
                        Else
                            herhTijd = If(dt.Rows.Count > 0, dt.Rows(dt.Rows.Count - 1)("Herhalingstijd") + 1, 1)
                        End If
                    Else
                        herhTijd = 1 / -Math.Log(pCum)
                    End If

                    dt.Rows.Add(herhTijd, value, runId, featureIdx)
                Next

                'Only keep features that have an increase in value
                If dt.Rows(dt.Rows.Count - 1)("Waarde") > dt.Rows(0)("Waarde") Then
                    dtHerh(featureIdx) = dt
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalcExceedanceTables2DFromCSV.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function CalcExceedanceTables2D(ByRef locationList As List(Of Integer), dtResults As Dictionary(Of Integer, DataTable)) As (Boolean, Dictionary(Of Integer, DataTable))
        Try
            Dim ValuesField As String = "WAARDE"
            Dim dtTemp As New Dictionary(Of Integer, DataTable)
            Dim finalList As New List(Of Integer)

            'for each feature, create a table with the exceedance values
            For Each Loc As Integer In locationList
                Dim dt As New DataTable
                dt.Columns.Add("Herhalingstijd", GetType(Double))
                dt.Columns.Add("Waarde", GetType(Double))
                dt.Columns.Add("RUNID", GetType(String))
                dt.Columns.Add("FEATUREIDX", GetType(String))
                dtTemp.Add(Loc, dt)
            Next

            For Each FeatureIdx As Integer In dtResults.Keys
                Dim myTable As DataTable = dtResults.Item(FeatureIdx)
                Dim pCum As Double = 0
                Dim i As Integer = 0

                ' Sort the table by WAARDE in ascending order
                myTable.DefaultView.Sort = ValuesField & " ASC"
                Dim sortedTable As DataTable = myTable.DefaultView.ToTable()

                For Each row As DataRow In sortedTable.Rows
                    Dim value As Double = row(ValuesField)
                    Dim runId As String = row("RUNID")
                    Dim p As Double = row("P")

                    pCum += p
                    Dim herhTijd As Double

                    If Setup.StochastenAnalyse.VolumesAsFrequencies Then
                        Dim maxFreq As Double = 365.25 * 24 / Setup.StochastenAnalyse.Duration
                        If pCum < maxFreq Then
                            herhTijd = 1 / (maxFreq - pCum)
                        Else
                            herhTijd = dtTemp(FeatureIdx).Rows(i - 1)("Herhalingstijd") + 1
                        End If
                    Else
                        herhTijd = 1 / -Math.Log(pCum)
                    End If

                    dtTemp(FeatureIdx).Rows.Add(herhTijd, value, runId, FeatureIdx)
                    i += 1
                Next
            Next

            ' Now only keep the features that have an increase in value
            For Each featureIdx As Integer In dtTemp.Keys
                Dim dt As DataTable = dtTemp.Item(featureIdx)
                If dt.Rows.Count > 0 AndAlso dt.Rows(dt.Rows.Count - 1)("Waarde") > dt.Rows(0)("Waarde") Then
                    finalList.Add(featureIdx)
                End If
            Next

            ' Update the locationList with only those locations that have an increase in value
            locationList = finalList

            Return (True, dtTemp)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalcExceedanceTables2D.")
            Me.Setup.Log.AddError(ex.Message)
            Return (False, Nothing)
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


    Public Function GetExceedanceValues2DFromDB(returnPeriods As List(Of Integer), ClimateScenario As String, Duration As Integer, parameter As GeneralFunctions.enm2DParameter) As Dictionary(Of Integer, List(Of Double))
        Try
            Dim returnDictionary As New Dictionary(Of Integer, List(Of Double))()
            Dim TableName As String = get2DReturnPeriodsTableName(parameter, KlimaatScenario.ToString, Duration)

            Dim dtLoc = New DataTable
            Dim query As String = $"SELECT DISTINCT FEATUREIDX FROM {TableName};"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtLoc, True)

            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Herhalingstijden uit de database lezen...", 0, 10, True)

            For i = 0 To dtLoc.Rows.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, dtLoc.Rows.Count)

                Dim dtHerh As New DataTable
                query = $"SELECT HERHALINGSTIJD, WAARDE FROM {TableName} WHERE FEATUREIDX=" & dtLoc.Rows(i)("FEATUREIDX") & " ORDER BY HERHALINGSTIJD;"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtHerh, False)

                If dtHerh.Rows.Count > 0 Then
                    Dim returnPeriodValues As New List(Of Double)()

                    For Each returnPeriod In returnPeriods
                        Dim waterlevel As Double = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, returnPeriod, 0, 1)
                        returnPeriodValues.Add(waterlevel)
                    Next

                    Dim featureIdx As Integer = Convert.ToInt32(dtLoc.Rows(i)("FEATUREIDX"))
                    returnDictionary.Add(featureIdx, returnPeriodValues)
                End If
            Next

            Me.Setup.SqliteCon.Close()
            Return returnDictionary
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetReturnPeriodsWaterlevels2D: " & ex.Message)
            Return New Dictionary(Of Integer, List(Of Double))()  ' Return empty dictionary on error
        End Try
    End Function

    Public Function GetExceedanceValues2DFromCSV(returnPeriods As List(Of Integer), parameter As GeneralFunctions.enm2DParameter) As Dictionary(Of Integer, List(Of Double))
        Try
            Dim ExceedanceTable2Dpath As String = getExceedanceTable2DPath(ResultsDir, KlimaatScenario, parameter)
            Dim returnDictionary As New Dictionary(Of Integer, List(Of Double))()

            Using reader As New StreamReader(ExceedanceTable2Dpath)
                ' Read and process the header
                Dim headers = reader.ReadLine().Split(";"c)
                Dim featureIdxIndex = System.Array.IndexOf(headers, "FEATUREIDX")
                Dim herhalingstijdIndex = System.Array.IndexOf(headers, "HERHALINGSTIJD")
                Dim waardeIndex = System.Array.IndexOf(headers, "WAARDE")

                ' Prepare a dictionary to store data for each FEATUREIDX
                Dim featureData As New Dictionary(Of Integer, List(Of KeyValuePair(Of Double, Double)))()

                ' Read and process the data
                While Not reader.EndOfStream
                    Dim fields = reader.ReadLine().Split(";"c)
                    Dim featureIdx = Integer.Parse(fields(featureIdxIndex))
                    Dim herhalingstijd = Double.Parse(fields(herhalingstijdIndex))
                    Dim waarde = Double.Parse(fields(waardeIndex))

                    If Not featureData.ContainsKey(featureIdx) Then
                        featureData(featureIdx) = New List(Of KeyValuePair(Of Double, Double))()
                    End If
                    featureData(featureIdx).Add(New KeyValuePair(Of Double, Double)(herhalingstijd, waarde))
                End While

                ' Process the data for each FEATUREIDX
                Me.Setup.GeneralFunctions.UpdateProgressBar("Herhalingstijden verwerken...", 0, featureData.Count, True)
                Dim processedCount = 0
                For Each kvp In featureData
                    Dim featureIdx = kvp.Key
                    Dim data = kvp.Value

                    ' Sort the data by herhalingstijd
                    data.Sort(Function(a, b) a.Key.CompareTo(b.Key))

                    Dim returnPeriodValues As New List(Of Double)()
                    For Each returnPeriod In returnPeriods
                        Dim waterlevel As Double = InterpolateFromList(data, returnPeriod)
                        returnPeriodValues.Add(waterlevel)
                    Next

                    returnDictionary.Add(featureIdx, returnPeriodValues)

                    processedCount += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", processedCount, featureData.Count)
                Next
            End Using

            Return returnDictionary
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetExceedanceValues2DFromCSV: " & ex.Message)
            Return New Dictionary(Of Integer, List(Of Double))()  ' Return empty dictionary on error
        End Try
    End Function

    Private Function InterpolateFromList(data As List(Of KeyValuePair(Of Double, Double)), targetX As Double) As Double
        ' Find the two closest points for interpolation
        Dim lowerPoint = data.LastOrDefault(Function(p) p.Key <= targetX)
        Dim upperPoint = data.FirstOrDefault(Function(p) p.Key >= targetX)

        ' If targetX is outside the range, return the closest value
        If lowerPoint.Equals(New KeyValuePair(Of Double, Double)()) Then Return upperPoint.Value
        If upperPoint.Equals(New KeyValuePair(Of Double, Double)()) Then Return lowerPoint.Value

        ' Perform linear interpolation
        Dim slope = (upperPoint.Value - lowerPoint.Value) / (upperPoint.Key - lowerPoint.Key)
        Return lowerPoint.Value + slope * (targetX - lowerPoint.Key)
    End Function


    'Public Function GetExceedanceValues2DFromCSV(returnPeriods As List(Of Integer), parameter As GeneralFunctions.enm2DParameter) As Dictionary(Of Integer, List(Of Double))
    '    Try
    '        'Set the paths to the CSV files
    '        Dim ExceedanceTable2Dpath As String = getExceedanceTable2DPath(ResultsDir, KlimaatScenario, parameter)
    '        Dim StochastsPath As String = getStochastsPath(ResultsDir)

    '        Dim returnDictionary As New Dictionary(Of Integer, List(Of Double))()

    '        'Read the exceedance data into a DataTable
    '        Dim exceedanceData As New DataTable

    '        Using reader As New TextFieldParser(ExceedanceTable2Dpath)
    '            reader.SetDelimiters(",")
    '            reader.HasFieldsEnclosedInQuotes = True

    '            'Read column names
    '            Dim columns() As String = reader.ReadFields()
    '            For Each column In columns
    '                exceedanceData.Columns.Add(column)
    '            Next

    '            'Read data
    '            While Not reader.EndOfData
    '                exceedanceData.Rows.Add(reader.ReadFields())
    '            End While
    '        End Using

    '        'Get distinct FEATUREIDX values
    '        Dim distinctFeatureIdx = (From row In exceedanceData.Rows
    '                                  Select CInt(row("FEATUREIDX"))).Distinct().ToList()

    '        Me.Setup.GeneralFunctions.UpdateProgressBar("Herhalingstijden uit het CSV-bestand lezen...", 0, 10, True)
    '        For i = 0 To distinctFeatureIdx.Count - 1
    '            Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, distinctFeatureIdx.Count)

    '            Dim featureIdx = distinctFeatureIdx(i)
    '            Dim dtHerh As DataTable = exceedanceData.Clone() ' Create an empty table with the same schema

    '            ' Fill dtHerh with the filtered and sorted rows
    '            For Each row As DataRow In exceedanceData.Select($"FEATUREIDX = '{featureIdx}'", "HERHALINGSTIJD ASC")
    '                dtHerh.ImportRow(row)
    '            Next

    '            If dtHerh.Rows.Count > 0 Then
    '                Dim returnPeriodValues As New List(Of Double)()
    '                For Each returnPeriod In returnPeriods
    '                    Dim waterlevel As Double = InterpolateFromDataTable(dtHerh, returnPeriod, "HERHALINGSTIJD", "WAARDE")
    '                    returnPeriodValues.Add(waterlevel)
    '                Next
    '                returnDictionary.Add(featureIdx, returnPeriodValues)
    '            End If
    '        Next

    '        Return returnDictionary
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function GetExceedanceValues2DFromCSV: " & ex.Message)
    '        Return New Dictionary(Of Integer, List(Of Double))()  ' Return empty dictionary on error
    '    End Try
    'End Function


    ' Helper function to interpolate values
    Private Function InterpolateFromDataTable(dt As DataTable, x As Double, xColumnName As String, yColumnName As String) As Double
        Dim xValues As New List(Of Double)()
        Dim yValues As New List(Of Double)()

        For Each row As DataRow In dt.Rows
            xValues.Add(CDbl(row(xColumnName)))
            yValues.Add(CDbl(row(yColumnName)))
        Next

        If x <= xValues(0) Then
            Return yValues(0)
        ElseIf x >= xValues(xValues.Count - 1) Then
            Return yValues(yValues.Count - 1)
        Else
            For i As Integer = 0 To xValues.Count - 2
                If x >= xValues(i) AndAlso x <= xValues(i + 1) Then
                    Dim x1 As Double = xValues(i)
                    Dim x2 As Double = xValues(i + 1)
                    Dim y1 As Double = yValues(i)
                    Dim y2 As Double = yValues(i + 1)
                    Return y1 + (y2 - y1) * (x - x1) / (x2 - x1)
                End If
            Next
        End If

        ' This should not happen if the data is properly sorted
        Throw New Exception("Interpolation failed")
    End Function


    Public Function CalculateExceedanceMesh(path As String, ClimateScenario As String, Duration As Integer) As Boolean
        'this function is designed to export our mesh to a GeoJSON for the webviewer. Inside, it will store the exceedance tables
        Try
            'first thing to do is to read the model's fourier file and turn it into a GeoJSON
            For Each Model As clsSimulationModel In Models.Values
                Select Case Model.ModelType
                    Case Is = GeneralFunctions.enmSimulationModel.DHYDRO, GeneralFunctions.enmSimulationModel.DIMR

                        'now we'll pick the results file from the first simulation and use that as a template to generate a geoJSON
                        Dim resultsFile As clsResultsFile = Model.ResultsFiles.getFourierFile
                        Dim ResultsFilePath As String = Runs.Runs.Values(0).OutputFilesDir & "\" & resultsFile.FileName

                        'check if the file exists
                        If resultsFile Is Nothing Then
                            Throw New Exception("Could Not find a Fourier file in the model results. Unable to generate exceedance mesh for webviewer.")
                        ElseIf Not System.IO.File.Exists(ResultsFilePath) Then
                            Throw New Exception("Could Not find a Fourier file in the model results. Unable to generate exceedance mesh for webviewer.")
                        End If

                        'we must take one of the fourier files as a template, read it and convert it to a geoJSON with exceedance tables
                        Dim fouFile As New clsFouNCFile(ResultsFilePath, Me.Setup)
                        If Not fouFile.Read() Then Throw New Exception("Error reading fourier file.")

                        'let's get our return periods from the database!
                        Dim ReturnPeriods As New List(Of Integer) From {10, 25, 50, 100}
                        Dim ExceedanceValues As Dictionary(Of Integer, List(Of Double)) = GetExceedanceValues2DFromCSV(ReturnPeriods, GeneralFunctions.enm2DParameter.depth)

                        fouFile.ReprojectAndWriteFloodLevelsMeshToWebJS(path, ReturnPeriods, ExceedanceValues)


                    Case Else
                        Throw New Exception("Model type not yet supported for exporting exceedance tables for 2D mesh.")
                End Select
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function CalculateExceedanceTables(process1D As Boolean, process2D As Boolean) As Boolean
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Overschrijdingstabellen berekenen...", 0, 10, True)

            'populate a table containing all stochast classes per run
            'for speed, also create a list of row indices
            Dim query As String
            Dim rundt As New DataTable, i As Integer
            Dim RunsList As New Dictionary(Of String, Integer)

            query = "SELECT RUNID, RUNIDX, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4 FROM RUNS WHERE KLIMAATSCENARIO='" & KlimaatScenario.ToString & "' AND DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, rundt, False)
            For i = 0 To rundt.Rows.Count - 1
                RunsList.Add(rundt.Rows(i)("RUNID"), i)
            Next

            If process1D Then ProcessExceedanceTables1D(RunsList, rundt)
            If process2D Then
                'we'll create exceedance table twice: once for waterlevel and once for depth
                'ProcessExceedanceTables2D(RunsList, rundt, GeneralFunctions.enm2DParameter.depth)
                'ProcessExceedanceTables2D(RunsList, rundt, GeneralFunctions.enm2DParameter.waterlevel)
                '20240910: skipping the database altogether when postprocessing 2D results. Everything goes via CSV now
                ProcessExceedanceTables2DFromCSVToCSV(RunsList, rundt, GeneralFunctions.enm2DParameter.depth)
                ProcessExceedanceTables2DFromCSVToCSV(RunsList, rundt, GeneralFunctions.enm2DParameter.waterlevel)
            End If

            Me.Setup.GeneralFunctions.UpdateProgressBar("Overschrijdingstabellen succesvol berekend.", 10, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalculateExceedanceTables of class clsStochastenAnalyse: " & ex.Message)
            Me.Setup.Log.ShowAll()
            Return False
        End Try
    End Function

    Public Function ProcessExceedanceTables1D(ByRef RunsList As Dictionary(Of String, Integer), ByRef rundt As DataTable) As Boolean
        Try
            Dim query As String
            Dim i As Integer
            Dim locdt As New DataTable, locIdx As Integer

            'now only read results locations
            query = "SELECT DISTINCT LOCATIENAAM, RESULTSTYPE FROM OUTPUTLOCATIONS;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locdt, False)

            'clear existing exceedance tables for this location and climate
            query = "DELETE FROM HERHALINGSTIJDEN WHERE DUUR=" & Duration & " AND KLIMAATSCENARIO='" & KlimaatScenario.ToString & "';"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

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
                    myCmd.Connection = Me.Setup.SqliteCon
                    Using transaction = Me.Setup.SqliteCon.BeginTransaction

                        For i = 0 To dtHerh.Rows.Count - 1

                            Dim RunID As String = dtHerh.Rows(i)("RUNID")
                            Dim RowIdx As Integer = RunsList.Item(RunID)

                            myCmd.CommandText = "INSERT INTO HERHALINGSTIJDEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, HERHALINGSTIJD, WAARDE, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & locdt.Rows(locIdx)(0).ToString & "','" & rundt.Rows(RowIdx)("RUNID") & "'," & dtHerh.Rows(i)(0) & "," & dtHerh.Rows(i)(1) & ",'" & rundt.Rows(RowIdx)("SEIZOEN") & "'," & rundt.Rows(RowIdx)("VOLUME") & ",'" & rundt.Rows(RowIdx)("PATROON") & "','" & rundt.Rows(RowIdx)("GW") & "','" & rundt.Rows(RowIdx)("BOUNDARY") & "','" & rundt.Rows(RowIdx)("WIND") & "','" & rundt.Rows(RowIdx)("EXTRA1") & "','" & rundt.Rows(RowIdx)("EXTRA2") & "','" & rundt.Rows(RowIdx)("EXTRA3") & "','" & rundt.Rows(RowIdx)("EXTRA4") & "');"
                            myCmd.ExecuteNonQuery()
                        Next

                        'insert the resulta for all return periods at once
                        transaction.Commit() 'this is where the bulk insert is finally executed.
                    End Using

                    'chatGPT Perform WAL checkpoint
                    Using cmd As New SQLite.SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", Me.Setup.SqliteCon)
                        cmd.ExecuteNonQuery()
                    End Using

                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ProceessExceedanceTables1D of class clsStochastenanalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ProcessExceedanceTables2DFromCSV(ByRef RunsList As Dictionary(Of String, Integer), ByRef rundt As DataTable, parameter As GeneralFunctions.enm2DParameter) As Boolean
        Try
            Dim i As Integer, Loc As Integer

            Me.Setup.GeneralFunctions.UpdateProgressBar($"Overschrijdingstabellen berekenen voor 2D parameter {parameter.ToString}...", 0, 10, True)

            'clear existing exceedance tables for this location and climate
            Dim HerhTableName As String
            HerhTableName = get2DReturnPeriodsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)
            Dim query As String = $"DELETE FROM {HerhTableName};"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            'first establish the path to our results file
            Dim ResultsPath As String = getResults2DPath(ResultsDir, KlimaatScenario, parameter)
            If Not System.IO.File.Exists(ResultsPath) Then Throw New Exception("2D results file not found: " & ResultsPath)

            'let's read the data in chunks of 1000 lines
            Dim chunkSize As Integer = 1000

            Using csvReader As New StreamReader(ResultsPath)
                'read the first line, containing the runIDs
                Dim runIDs As String() = csvReader.ReadLine().Split(","c).Skip(1).ToArray() 'skip the first column "FEATUREIDX"
                'read the second line, containing the frequencies associated with each run
                Dim frequencies As String() = csvReader.ReadLine().Split(","c).Skip(1).ToArray()

                'create a dictionary to map runIDs to their frequencies
                Dim runFrequencies As New Dictionary(Of String, Double)
                For i = 0 To runIDs.Length - 1
                    runFrequencies(runIDs(i)) = Double.Parse(frequencies(i))
                Next

                While Not csvReader.EndOfStream

                    Me.Setup.GeneralFunctions.UpdateProgressBar("", csvReader.BaseStream.Position, csvReader.BaseStream.Length)

                    'process the data in chunks
                    Dim chunk As New List(Of String())
                    Dim locationList As New List(Of Integer)
                    Dim dtResults As Dictionary(Of Integer, DataTable)

                    'create a new dictionary of datatables to store the current chunk of data
                    dtResults = New Dictionary(Of Integer, DataTable)

                    'read a chunk of data
                    For i = 0 To chunkSize - 1
                        Dim line As String = csvReader.ReadLine()
                        If line Is Nothing Then Exit For
                        chunk.Add(line.Split(","c))
                    Next

                    'process the chunk of data
                    For Each row In chunk
                        Dim featureIdx As Integer = Integer.Parse(row(0))
                        'each feature should get its own datatable  
                        Dim dt As New DataTable
                        dt.Columns.Add("RUNID", GetType(String))            'the run ID
                        dt.Columns.Add("P", GetType(Double))                'the probability associated with this run
                        dt.Columns.Add("WAARDE", GetType(Double))           'the value associated with this feature and run
                        dt.Columns.Add("FEATUREIDX", GetType(Integer))      'the index number of the 2D feature

                        'add the feature index to the locations list
                        If Not locationList.Contains(featureIdx) Then
                            locationList.Add(featureIdx)
                        End If

                        'add the new datatable for this feature to the dtResults dictionary
                        If Not dtResults.ContainsKey(featureIdx) Then
                            dtResults.Add(featureIdx, dt)
                        End If

                        'add each pair of runID, probability and value to the datatable
                        For i = 1 To row.Length - 1
                            dtResults(featureIdx).Rows.Add(runIDs(i - 1), runFrequencies(runIDs(i - 1)), Double.Parse(row(i)), featureIdx)
                        Next
                    Next

                    'now that we read the chunk of data, calculate the exceedance tables for these features
                    Dim res As (Boolean, Dictionary(Of Integer, DataTable))
                    res = Me.Setup.StochastenAnalyse.CalcExceedanceTables2D(locationList, dtResults)
                    If res.Item1 Then
                        Using transaction = Me.Setup.SqliteCon.BeginTransaction
                            Dim myCmd As New SQLite.SQLiteCommand
                            myCmd.Connection = Me.Setup.SqliteCon

                            Dim params As New Dictionary(Of String, Object) From {
                        {"@featureidx", ""},
                        {"@runid", ""},
                        {"@runidx", ""},
                        {"@herhalingstijd", 0},
                        {"@waarde", 0},
                        {"@seizoen", ""},
                        {"@volume", 0},
                        {"@patroon", ""},
                        {"@gw", ""},
                        {"@boundary", ""},
                        {"@wind", ""},
                        {"@extra1", ""},
                        {"@extra2", ""},
                        {"@extra3", ""},
                        {"@extra4", ""}
                    }
                            For Each Loc In locationList
                                Dim dt As DataTable = res.Item2(Loc)
                                For i = 0 To dt.Rows.Count - 1
                                    Dim RunID As String = dt.Rows(i)("RUNID")
                                    Dim RowIdx As Integer = RunsList.Item(RunID)

                                    params("@featureidx") = Loc
                                    params("@runid") = rundt.Rows(RowIdx)("RUNID")
                                    params("@runidx") = rundt.Rows(RowIdx)("RUNIDX")
                                    params("@herhalingstijd") = dt.Rows(i)(0)
                                    params("@waarde") = dt.Rows(i)(1)
                                    params("@seizoen") = rundt.Rows(RowIdx)("SEIZOEN")
                                    params("@volume") = rundt.Rows(RowIdx)("VOLUME")
                                    params("@patroon") = rundt.Rows(RowIdx)("PATROON")
                                    params("@gw") = rundt.Rows(RowIdx)("GW")
                                    params("@boundary") = rundt.Rows(RowIdx)("BOUNDARY")
                                    params("@wind") = rundt.Rows(RowIdx)("WIND")
                                    params("@extra1") = rundt.Rows(RowIdx)("EXTRA1")
                                    params("@extra2") = rundt.Rows(RowIdx)("EXTRA2")
                                    params("@extra3") = rundt.Rows(RowIdx)("EXTRA3")
                                    params("@extra4") = rundt.Rows(RowIdx)("EXTRA4")

                                    If Not InsertRecordWithParameters2D(myCmd, params, HerhTableName) Then Throw New Exception("Error inserting record with parameters 2D.")
                                Next
                            Next

                            transaction.Commit()
                        End Using

                        'Perform WAL checkpoint
                        Using cmd As New SQLite.SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", Me.Setup.SqliteCon)
                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    'clear the chunk for the next iteration
                    chunk.Clear()
                End While


            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ProcessExceedanceTables2DFromCSV of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ProcessExceedanceTables2DFromCSVToCSV(ByRef RunsList As Dictionary(Of String, Integer), ByRef rundt As DataTable, parameter As GeneralFunctions.enm2DParameter) As Boolean
        Try
            Dim i As Integer, Loc As Integer

            Me.Setup.GeneralFunctions.UpdateProgressBar($"Overschrijdingstabellen berekenen voor 2D parameter {parameter.ToString}...", 0, 10, True)

            'Determine the output CSV file names
            Dim exceedanceFileName As String = getExceedanceTable2DPath(ResultsDir, KlimaatScenario, parameter)
            Dim stochastsFileName As String = getStochastsPath(ResultsDir)

            'Dim exceedanceFileName As String = $"Overschrijdingstabellen_{parameter.ToString}_{Me.Setup.StochastenAnalyse.KlimaatScenario}_{Me.Setup.StochastenAnalyse.Duration}.csv"
            Dim stochastsPath As String = Path.Combine(ResultsDir, stochastsFileName)
            Dim exceedancePath As String = Path.Combine(ResultsDir, exceedanceFileName)

            'Delete existing output files if they exist
            For Each filePath In {stochastsPath, exceedancePath}
                If System.IO.File.Exists(filePath) Then
                    System.IO.File.Delete(filePath)
                End If
            Next

            'Determine the input file path
            Dim ResultsPath As String = getResults2DPath(ResultsDir, KlimaatScenario, parameter)
            If Not System.IO.File.Exists(ResultsPath) Then Throw New Exception("2D results file not found: " & ResultsPath)

            'Write stochasts file
            Using stochastsWriter As New StreamWriter(stochastsPath)
                stochastsWriter.WriteLine("RUNID;RUNIDX;SEIZOEN;VOLUME;PATROON;GW;BOUNDARY;WIND;EXTRA1;EXTRA2;EXTRA3;EXTRA4")
                For Each row As DataRow In rundt.Rows
                    stochastsWriter.WriteLine($"{row("RUNID")};{row("RUNIDX")};{row("SEIZOEN")};{row("VOLUME")};{row("PATROON")};{row("GW")};{row("BOUNDARY")};{row("WIND")};{row("EXTRA1")};{row("EXTRA2")};{row("EXTRA3")};{row("EXTRA4")}")
                Next
            End Using

            'Let's read the data in chunks of 1000 lines
            Dim chunkSize As Integer = 1000

            Using csvReader As New StreamReader(ResultsPath)
                Using exceedanceWriter As New StreamWriter(exceedancePath)
                    'Write the header for the exceedance CSV
                    exceedanceWriter.WriteLine("FEATUREIDX;RUNID;HERHALINGSTIJD;WAARDE")

                    'Read the first line, containing the runIDs
                    Dim runIDs As String() = csvReader.ReadLine().Split(";"c).Skip(1).ToArray() 'skip the first column "FEATUREIDX"
                    'Read the second line, containing the frequencies associated with each run
                    Dim frequencies As String() = csvReader.ReadLine().Split(";"c).Skip(1).ToArray()

                    'Create a dictionary to map runIDs to their frequencies
                    Dim runFrequencies As New Dictionary(Of String, Double)
                    For i = 0 To runIDs.Length - 1
                        runFrequencies(runIDs(i)) = Double.Parse(frequencies(i))
                    Next

                    While Not csvReader.EndOfStream
                        Me.Setup.GeneralFunctions.UpdateProgressBar("", csvReader.BaseStream.Position, csvReader.BaseStream.Length)

                        'Process the data in chunks
                        Dim chunk As New List(Of String())
                        Dim locationList As New List(Of Integer)
                        Dim dtResults As New Dictionary(Of Integer, DataTable)

                        'Read a chunk of data
                        For i = 0 To chunkSize - 1
                            Dim line As String = csvReader.ReadLine()
                            If line Is Nothing Then Exit For
                            chunk.Add(line.Split(";"c))
                        Next

                        'Process the chunk of data
                        For Each row In chunk
                            Dim featureIdx As Integer = Integer.Parse(row(0))
                            'Each feature should get its own datatable  
                            Dim dt As New DataTable
                            dt.Columns.Add("RUNID", GetType(String))
                            dt.Columns.Add("P", GetType(Double))
                            dt.Columns.Add("WAARDE", GetType(Double))
                            dt.Columns.Add("FEATUREIDX", GetType(Integer))

                            'Add the feature index to the locations list
                            If Not locationList.Contains(featureIdx) Then
                                locationList.Add(featureIdx)
                            End If

                            'Add the new datatable for this feature to the dtResults dictionary
                            If Not dtResults.ContainsKey(featureIdx) Then
                                dtResults.Add(featureIdx, dt)
                            End If

                            'Add each pair of runID, probability and value to the datatable
                            For i = 1 To row.Length - 1
                                dtResults(featureIdx).Rows.Add(runIDs(i - 1), runFrequencies(runIDs(i - 1)), Double.Parse(row(i)), featureIdx)
                            Next
                        Next

                        'Calculate the exceedance tables for these features
                        Dim res As (Boolean, Dictionary(Of Integer, DataTable))
                        res = Me.Setup.StochastenAnalyse.CalcExceedanceTables2D(locationList, dtResults)

                        If res.Item1 Then
                            For Each Loc In locationList
                                Dim dt As DataTable = res.Item2(Loc)
                                For i = 0 To dt.Rows.Count - 1
                                    Dim RunID As String = dt.Rows(i)("RUNID")
                                    'Write the data to the exceedance CSV file
                                    exceedanceWriter.WriteLine($"{Loc};{RunID};{String.Format("{0:N3}", dt.Rows(i)(0))};{String.Format("{0:N4}", dt.Rows(i)(1))}")
                                Next
                            Next
                        End If

                        'Clear the chunk for the next iteration
                        chunk.Clear()
                    End While
                End Using
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ProcessExceedanceTables2DFromCSVToCSV of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function




    'Public Function ProcessExceedanceTables2D(ByRef RunsList As Dictionary(Of String, Integer), ByRef rundt As DataTable, parameter As GeneralFunctions.enm2DParameter) As Boolean
    '    Try
    '        Me.Setup.GeneralFunctions.UpdateProgressBar($"Overschrijdingstabellen berekenen voor 2D parameter {parameter.ToString}...", 0, 10, True)

    '        Dim query As String
    '        Dim i As Integer
    '        Dim Loc As Integer
    '        Dim locdt As New DataTable, locIdx As Integer

    '        Dim ResultsTableName As String
    '        ResultsTableName = get2DResultsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)
    '        Dim HerhTableName As String
    '        HerhTableName = get2DReturnPeriodsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)

    '        'now only read results locations
    '        query = $"SELECT DISTINCT FEATUREIDX FROM {ResultsTableName};"
    '        Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locdt, False)

    '        'clear existing exceedance tables for this location and climate
    '        query = $"DELETE FROM {HerhTableName};"
    '        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

    '        'for each location in our results table we will now create an exceedance table and write it to the database
    '        Dim nLocs As Integer = locdt.Rows.Count
    '        Dim chunkSize As Integer = 1000
    '        For locIdx = 0 To locdt.Rows.Count - 1 Step chunkSize

    '            Me.Setup.GeneralFunctions.UpdateProgressBar("", locIdx + 1, nLocs)

    '            Dim dtHerh As New Dictionary(Of Integer, DataTable)

    '            ' Get the location names for the current chunk
    '            Dim locationChunk As New List(Of Integer)
    '            For i = locIdx To Math.Min(locIdx + chunkSize - 1, locdt.Rows.Count - 1)
    '                locationChunk.Add(locdt.Rows(i)("FEATUREIDX"))
    '            Next

    '            'retrieve all data for the current duration and climate scenario
    '            If Me.Setup.StochastenAnalyse.CalcExceedanceTables2D(locationChunk, "WAARDE", parameter, dtHerh, ResultsTableName, "FEATUREIDX") Then
    '                Using transaction = Me.Setup.SqliteCon.BeginTransaction
    '                    Dim myCmd As New SQLite.SQLiteCommand
    '                    myCmd.Connection = Me.Setup.SqliteCon

    '                    Dim params As New Dictionary(Of String, Object) From {
    '                {"@featureidx", ""},
    '                {"@runid", ""},
    '                {"@runidx", ""},
    '                {"@herhalingstijd", 0},
    '                {"@waarde", 0},
    '                {"@seizoen", ""},
    '                {"@volume", 0},
    '                {"@patroon", ""},
    '                {"@gw", ""},
    '                {"@boundary", ""},
    '                {"@wind", ""},
    '                {"@extra1", ""},
    '                {"@extra2", ""},
    '                {"@extra3", ""},
    '                {"@extra4", ""}
    '            }
    '                    For Each Loc In locationChunk
    '                        Dim dt As DataTable = dtHerh(Loc)
    '                        For i = 0 To dt.Rows.Count - 1
    '                            Dim RunID As String = dt.Rows(i)("RUNID")
    '                            Dim RowIdx As Integer = RunsList.Item(RunID)

    '                            params("@featureidx") = Loc
    '                            params("@runid") = rundt.Rows(RowIdx)("RUNID")
    '                            params("@runidx") = rundt.Rows(RowIdx)("RUNIDX")
    '                            params("@herhalingstijd") = dt.Rows(i)(0)
    '                            params("@waarde") = dt.Rows(i)(1)
    '                            params("@seizoen") = rundt.Rows(RowIdx)("SEIZOEN")
    '                            params("@volume") = rundt.Rows(RowIdx)("VOLUME")
    '                            params("@patroon") = rundt.Rows(RowIdx)("PATROON")
    '                            params("@gw") = rundt.Rows(RowIdx)("GW")
    '                            params("@boundary") = rundt.Rows(RowIdx)("BOUNDARY")
    '                            params("@wind") = rundt.Rows(RowIdx)("WIND")
    '                            params("@extra1") = rundt.Rows(RowIdx)("EXTRA1")
    '                            params("@extra2") = rundt.Rows(RowIdx)("EXTRA2")
    '                            params("@extra3") = rundt.Rows(RowIdx)("EXTRA3")
    '                            params("@extra4") = rundt.Rows(RowIdx)("EXTRA4")

    '                            If Not InsertRecordWithParameters2D(myCmd, params, HerhTableName) Then Throw New Exception("Error inserting record with parameters 2D.")
    '                        Next
    '                    Next

    '                    transaction.Commit()
    '                End Using

    '                'chatGPT Perform WAL checkpoint
    '                Using cmd As New SQLite.SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", Me.Setup.SqliteCon)
    '                    cmd.ExecuteNonQuery()
    '                End Using

    '            End If
    '        Next

    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function ProcessExceedanceTables2D of class clsStochastenAnalyse: " & ex.Message)
    '        Return False
    '    End Try
    'End Function


    Private Function InsertRecordWithParameters2D(ByRef myCmd As SQLite.SQLiteCommand, ByVal params As Dictionary(Of String, Object), ByVal tableName As String) As Boolean
        Try
            Dim query As String = "INSERT INTO " & tableName & " (FEATUREIDX, RUNID, RUNIDX, HERHALINGSTIJD, WAARDE, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4) VALUES (@featureidx, @runid, @runidx, @herhalingstijd, @waarde, @seizoen, @volume, @patroon, @gw, @boundary, @wind, @extra1, @extra2, @extra3, @extra4);"

            myCmd.CommandText = query
            myCmd.Parameters.Clear()

            For Each param As KeyValuePair(Of String, Object) In params
                myCmd.Parameters.AddWithValue(param.Key, param.Value)
            Next

            myCmd.ExecuteNonQuery()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function InsertRecordWithParameters2D: " & ex.Message)
            Return False
        End Try
    End Function



    Private Sub InsertRecordWithParameters(ByRef myCmd As SQLite.SQLiteCommand, ByVal params As Dictionary(Of String, Object), ByVal tableName As String)
        Dim query As String = "INSERT INTO " & tableName & " (KLIMAATSCENARIO, DUUR, LOCATIENAAM, HERHALINGSTIJD, WAARDE, SEIZOEN, VOLUME, PATROON, GW, BOUNDARY, WIND, EXTRA1, EXTRA2, EXTRA3, EXTRA4) VALUES (@klimaatscenario, @duration, @locatie, @herhalingstijd, @waarde, @seizoen, @volume, @patroon, @gw, @boundary, @wind, @extra1, @extra2, @extra3, @extra4);"

        myCmd.CommandText = query
        myCmd.Parameters.Clear()

        For Each param As KeyValuePair(Of String, Object) In params
            myCmd.Parameters.AddWithValue(param.Key, param.Value)
        Next

        myCmd.ExecuteNonQuery()
    End Sub

    Public Sub InitializeDatabaseConnection()
        If Me.Setup.SqliteCon.State = ConnectionState.Open Then
            Me.Setup.SqliteCon.Close()
        End If

        Me.Setup.SqliteCon.Open()

        ' Enable WAL mode
        Using cmd As New SQLite.SQLiteCommand("PRAGMA journal_mode=WAL;", Me.Setup.SqliteCon)
            cmd.ExecuteNonQuery()
        End Using

        ' Set an automatic checkpoint limit (e.g., every 1000 pages)
        'Using cmd As New SQLite.SQLiteCommand("PRAGMA wal_autocheckpoint=1000;", Me.Setup.SqliteCon)
        '    cmd.ExecuteNonQuery()
        'End Using
    End Sub

    Public Sub CloseDatabaseConnection()
        If Me.Setup.SqliteCon.State = ConnectionState.Open Then
            ' Perform a WAL checkpoint before closing
            Using cmd As New SQLite.SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", Me.Setup.SqliteCon)
                cmd.ExecuteNonQuery()
            End Using

            ' Close the database connection
            Me.Setup.SqliteCon.Close()
        End If
    End Sub



    Public Function ReadResults(ByRef con As SQLite.SQLiteConnection, results1D As Boolean, results2D As Boolean)
        Dim query As String

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten uitlezen...", 0, 10, True)

            'first thing to do is clear all results for the current scenario and duration from the database since the results will change by definition when new computations are added
            Dim cmd As New SQLite.SQLiteCommand
            cmd.Connection = con

            If results1D Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten uitlezen 1D...", 2, 10, True)
                'remove the existing simulationresults from the database for selected climate and duration
                query = "DELETE FROM RESULTATEN WHERE KLIMAATSCENARIO = '" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR = " & Setup.StochastenAnalyse.Duration.ToString.Trim & ";"
                Setup.GeneralFunctions.SQLiteNoQuery(con, query)

                readResults1D()
            End If

            If results2D Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten uitlezen 2D...", 4, 10, True)

                'remove the existing simulationresults from the database for selected climate and duration
                Dim TableName As String = get2DResultsTableName(GeneralFunctions.enm2DParameter.depth, Setup.StochastenAnalyse.KlimaatScenario.ToString, Setup.StochastenAnalyse.Duration)
                query = $"DELETE FROM {TableName} WHERE KLIMAATSCENARIO = '" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR = " & Setup.StochastenAnalyse.Duration.ToString.Trim & ";"
                Setup.GeneralFunctions.SQLiteNoQuery(con, query)

                'remove the existing simulationresults from the database for selected climate and duration
                TableName = get2DResultsTableName(GeneralFunctions.enm2DParameter.waterlevel, Setup.StochastenAnalyse.KlimaatScenario.ToString, Setup.StochastenAnalyse.Duration)
                query = $"DELETE FROM {TableName} WHERE KLIMAATSCENARIO = '" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR = " & Setup.StochastenAnalyse.Duration.ToString.Trim & ";"
                Setup.GeneralFunctions.SQLiteNoQuery(con, query)

                Results2DToCSV()
            End If


            If Me.Setup.Log.Errors.Count > 0 Then
                Me.Setup.Log.AddMessage("Reading simulation complete, but with errors! Please check.")
            Else
                Me.Setup.Log.AddMessage("Reading simulation results was completed successfully.")
            End If


            Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten met succes uitgelezen.", 10, 10, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.ShowAll()
            Return False
        End Try

    End Function


    Public Function get2DResultsTableName(parameter As GeneralFunctions.enm2DParameter, ClimateScenario As String, Duration As Integer) As String
        Try
            Select Case parameter
                Case GeneralFunctions.enm2DParameter.depth
                    Return ClimateScenario & "_" & Duration.ToString & "_MAXDIEPTE2D"
                Case GeneralFunctions.enm2DParameter.waterlevel
                    Return ClimateScenario & "_" & Duration.ToString & "_MAXHOOGTE2D"
                Case Else
                    Return Nothing
            End Select
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function get2DReturnPeriodsTableName(parameter As GeneralFunctions.enm2DParameter, ClimateScenario As String, Duration As Integer) As String
        Try
            Select Case parameter
                Case GeneralFunctions.enm2DParameter.depth
                    Return ClimateScenario & "_" & Duration.ToString & "_HERHDIEPTE2D"
                Case GeneralFunctions.enm2DParameter.waterlevel
                    Return ClimateScenario & "_" & Duration.ToString & "_HERHHOOGTE2D"
                Case Else
                    Return Nothing
            End Select
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function readResults1D() As Boolean
        Try
            Dim i As Long, n As Long

            'read the results
            n = Runs.Runs.Count
            i = 0
            For Each myRun As clsStochastenRun In Runs.Runs.Values                        'doorloop alle runs en lees de resultaatbestanden uit
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten lezen voor simulatie " & i & " van " & n, i, n, True)
                For Each myModel As clsSimulationModel In Models.Values                     'doorloop alle modellen die gedraaid zijn
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values    'doorloop alle bestanden onder dit model
                        If Right(myFile.FileName, 4).ToLower = ".his" Then                      'this is a Deltares HIS-file. Read it using the corresponding reader
                            readHIS(myRun, myFile)
                        ElseIf Right(myFile.FileName, 7).ToLower = "_his.nc" Then
                            readHISNC(myRun, myFile)
                        End If
                    Next
                Next
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function readResults1D of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function readHIS(ByRef myRun As clsStochastenRun, ByRef myFile As clsResultsFile) As Boolean
        Try
            Dim MaxInLastTimeStep As Boolean
            Dim Min As Double, Max As Double, tsMin As Long, tsMax As Long, Avg As Double
            Dim dtRes As New DataTable

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
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function readHIS of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function

    'Public Function readHISNC(ByRef myRun As clsStochastenRun, ByRef myFile As clsResultsFile) As Boolean
    '    Try
    '        Dim i As Long, n As Long, j As Long
    '        Dim MaxInLastTimeStep As Boolean
    '        Dim Min As Double, Max As Double, tsMin As Long, tsMax As Long, Avg As Double, mySum As Double
    '        Dim dtRes As New DataTable

    '        If Not System.IO.File.Exists(myRun.OutputFilesDir & "\" & myFile.FileName) Then Throw New Exception("Fout: resultatenbestand niet gevonden: " & myRun.OutputFilesDir & "\" & myFile.FileName)
    '        Dim myHisNC As New clsHisNCFile(myRun.OutputFilesDir & "\" & myFile.FileName, Me.Setup)

    '        If myFile.Parameters.Count = 0 Then
    '            Me.Setup.Log.AddWarning("no parameters specified for output file " & myFile.FileName & ".")
    '        Else
    '            For Each myPar As clsResultsFileParameter In myFile.Parameters.Values 'walk through all parameters associated with this HIS-file
    '                If myPar.Locations.Count = 0 Then
    '                    Throw New Exception("Error: no locations specified for parameter " & myPar.Name & " in output file " & myFile.FileName & ".")
    '                Else
    '                    '--------------------------------------------------------------------------------------------------------------------------------------------
    '                    '  writing the results for this run & parameter to the database
    '                    '--------------------------------------------------------------------------------------------------------------------------------------------
    '                    Dim Waterlevels As Double(,) = Nothing
    '                    Dim Times As Double() = Nothing            'timesteps, expressed in seconds w.r.t. RefDate as specified in the .MDU
    '                    Dim IDList As String() = Nothing
    '                    If Not myHisNC.ReadWaterLevelsAtObservationPoints(Waterlevels, Times, IDList) Then Throw New Exception("Error reading hisfile by parameter " & myPar.Name)

    '                    If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
    '                    Using myCmd As New SQLite.SQLiteCommand
    '                        myCmd.Connection = Me.Setup.SqliteCon
    '                        Using transaction = Me.Setup.SqliteCon.BeginTransaction

    '                            For i = 0 To IDList.Count - 1
    '                                Dim ID As String = IDList(i)

    '                                If myPar.Locations.ContainsKey(ID.Trim.ToUpper) Then
    '                                    Max = -9.0E+99
    '                                    Min = 9.0E+99
    '                                    mySum = 0
    '                                    n = 0

    '                                    For j = 0 To UBound(Waterlevels, 1)
    '                                        If (j + 1) / (UBound(Waterlevels, 1) + 1) * 100 >= ResultsStartPercentage Then
    '                                            n += 1
    '                                            mySum += Waterlevels(j, i)
    '                                            If Waterlevels(j, i) > Max Then
    '                                                Max = Waterlevels(j, i)
    '                                                tsMax = j
    '                                                If j = UBound(Waterlevels, 1) Then MaxInLastTimeStep = True
    '                                            End If
    '                                            If Waterlevels(j, i) < Min Then
    '                                                Min = Waterlevels(j, i)
    '                                                tsMin = j
    '                                            End If
    '                                        End If
    '                                    Next
    '                                    Avg = mySum / n

    '                                    'add the outcome of this run to the dictionary of results
    '                                    myCmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL, P) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & myPar.Locations.Item(ID.Trim.ToUpper).Name & "','" & myRun.ID & "'," & Max & "," & Min & "," & Avg & "," & myRun.P & ");"
    '                                    myCmd.ExecuteNonQuery()
    '                                    If MaxInLastTimeStep = True Then Me.Setup.Log.AddError("Maximum value in last timestep for simulation " & myRun.ID & " and location " & myPar.Locations.Item(ID.Trim.ToUpper).ID)

    '                                End If
    '                            Next

    '                            'insert the resulta for all locations at once
    '                            transaction.Commit() 'this is where the bulk insert is finally executed.
    '                        End Using
    '                    End Using

    '                End If
    '            Next
    '        End If

    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function readHISNC of class clsStochastenAnalyse: " & ex.Message)
    '        Return False
    '    End Try
    'End Function
    Public Function readHISNC(ByRef myRun As clsStochastenRun, ByRef myFile As clsResultsFile) As Boolean
        'this is the result of refactoring the code by Claude AI on 2024-06-27
        Try
            If Not System.IO.File.Exists(Path.Combine(myRun.OutputFilesDir, myFile.FileName)) Then
                Throw New FileNotFoundException("Results file not found", Path.Combine(myRun.OutputFilesDir, myFile.FileName))
            End If

            Dim myHisNC As New clsHisNCFile(Path.Combine(myRun.OutputFilesDir, myFile.FileName), Me.Setup)

            If myFile.Parameters.Count = 0 Then
                Me.Setup.Log.AddWarning("No parameters specified for output file " & myFile.FileName & ".")
                Return True
            End If

            For Each myPar As clsResultsFileParameter In myFile.Parameters.Values
                If myPar.Locations.Count = 0 Then
                    Throw New Exception("Error: no locations specified for parameter " & myPar.Name & " in output file " & myFile.FileName & ".")
                End If

                Dim Waterlevels(,) As Double = Nothing
                Dim Times() As Double = Nothing
                Dim IDList() As String = Nothing
                If Not myHisNC.ReadWaterLevelsAtObservationPoints(Waterlevels, Times, IDList) Then
                    Throw New Exception("Error reading hisfile by parameter " & myPar.Name)
                End If

                Dim resultsList As New List(Of ResultItem)

                For i As Integer = 0 To IDList.Length - 1
                    Dim ID As String = IDList(i).Trim.ToUpper
                    If myPar.Locations.ContainsKey(ID) Then
                        Dim result As ResultItem = ProcessWaterLevels(Waterlevels, i)
                        result.LocationName = myPar.Locations(ID).Name
                        resultsList.Add(result)
                    End If
                Next

                InsertResults(resultsList, myRun)
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function readHISNC of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Private Structure ResultItem
        Public LocationName As String
        Public Max As Double
        Public Min As Double
        Public Avg As Double
    End Structure

    Private Function ProcessWaterLevels(Waterlevels(,) As Double, columnIndex As Integer) As ResultItem
        Dim result As New ResultItem
        result.Max = Double.MinValue
        result.Min = Double.MaxValue
        Dim Sum As Double = 0
        Dim Count As Integer = 0
        Dim StartIndex As Integer = CInt(Waterlevels.GetLength(0) * ResultsStartPercentage / 100)

        For j As Integer = StartIndex To Waterlevels.GetLength(0) - 1
            Dim value As Double = Waterlevels(j, columnIndex)
            Sum += value
            Count += 1
            If value > result.Max Then result.Max = value
            If value < result.Min Then result.Min = value
        Next

        result.Avg = If(Count > 0, Sum / Count, 0)
        Return result
    End Function

    Private Sub InsertResults(resultsList As List(Of ResultItem), myRun As clsStochastenRun)
        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

        Using transaction = Me.Setup.SqliteCon.BeginTransaction()
            Using cmd As New SQLite.SQLiteCommand(Me.Setup.SqliteCon)
                cmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL, P) VALUES (@Klimaat, @Duur, @Locatie, @RunID, @Max, @Min, @Avg, @P)"

                cmd.Parameters.Add("@Klimaat", DbType.String)
                cmd.Parameters.Add("@Duur", DbType.Int64)
                cmd.Parameters.Add("@Locatie", DbType.String)
                cmd.Parameters.Add("@RunID", DbType.String)
                cmd.Parameters.Add("@Max", DbType.Double)
                cmd.Parameters.Add("@Min", DbType.Double)
                cmd.Parameters.Add("@Avg", DbType.Double)
                cmd.Parameters.Add("@P", DbType.Double)

                cmd.Parameters("@Klimaat").Value = Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper
                cmd.Parameters("@Duur").Value = Setup.StochastenAnalyse.Duration
                cmd.Parameters("@RunID").Value = myRun.ID
                cmd.Parameters("@P").Value = myRun.P

                For Each result As ResultItem In resultsList
                    cmd.Parameters("@Locatie").Value = result.LocationName
                    cmd.Parameters("@Max").Value = result.Max
                    cmd.Parameters("@Min").Value = result.Min
                    cmd.Parameters("@Avg").Value = result.Avg
                    cmd.ExecuteNonQuery()
                Next
            End Using

            transaction.Commit()
        End Using
    End Sub

    Public Function Results2DToCSV() As Boolean
        Try
            Dim i As Long, n As Long
            Dim depthResults As New List(Of List(Of Double))()
            Dim levelResults As New List(Of List(Of Double))()
            Dim runIDs As New List(Of String)()
            Dim probabilities As New List(Of Double)()

            'read the results. For now we will only support Fourier Files
            n = Runs.Runs.Count
            i = 0
            For Each myRun As clsStochastenRun In Runs.Runs.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Resultaten lezen voor simulatie " & i & " van " & n, i, n, True)
                runIDs.Add(myRun.ID)
                probabilities.Add(myRun.P)
                For Each myModel As clsSimulationModel In Models.Values
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                        If Right(myFile.FileName, 7).ToLower = "_fou.nc" Then
                            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                            FourierToLists(myRun, myFile, depthResults, levelResults)
                        End If
                    Next
                Next
            Next

            ' Write transposed results to CSV files
            WriteTransposedCSV(getResults2DPath(ResultsDir, KlimaatScenario, GeneralFunctions.enm2DParameter.depth), runIDs, probabilities, depthResults)
            WriteTransposedCSV(getResults2DPath(ResultsDir, KlimaatScenario, GeneralFunctions.enm2DParameter.waterlevel), runIDs, probabilities, levelResults)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function readResults2D of class clsStochastenAnalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function getExceedanceTable2DPath(ResultsDir As String, Klimaatscenario As GeneralFunctions.enmKlimaatScenario, Parameter As GeneralFunctions.enm2DParameter)
        Return ResultsDir & "\" & $"Overschrijdingstabellen_{Parameter.ToString}_{Klimaatscenario}.csv"
    End Function

    Public Function getStochastsPath(ResultsDir As String)
        Return ResultsDir & "\" & $"Stochasts_{Me.Setup.StochastenAnalyse.KlimaatScenario}.csv"
    End Function

    Public Function getResults2DPath(ResultsDir As String, Klimaatscenario As GeneralFunctions.enmKlimaatScenario, Parameter As GeneralFunctions.enm2DParameter) As String
        Dim ParStr As String
        Dim ClimStr As String = Klimaatscenario.ToString

        Select Case Parameter
            Case GeneralFunctions.enm2DParameter.depth
                ParStr = "_depths2D.csv"
            Case GeneralFunctions.enm2DParameter.waterlevel
                ParStr = "_levels2D.csv"
            Case Else
                'a non-recognized parameter, but let's create a path based on the parameter name anyway
                ParStr = "_" & Parameter.ToString
        End Select
        Return ResultsDir & "\" & ClimStr & ParStr
    End Function

    Public Function FourierToLists(ByRef myRun As clsStochastenRun, ByRef myFile As clsResultsFile, ByRef depthResults As List(Of List(Of Double)), ByRef levelResults As List(Of List(Of Double))) As Boolean
        Try
            Dim path As String = System.IO.Path.Combine(myRun.OutputFilesDir, myFile.FileName)
            If Not System.IO.File.Exists(path) Then
                Throw New FileNotFoundException("Fourier file does not exist", path)
            End If
            Dim myFouNC As New clsFouNCFile(path, Me.Setup)
            If Not myFouNC.Read() Then
                Throw New Exception("Error reading fourier file " & path)
            End If
            Dim MaxDepths As Double() = myFouNC.get2DMaxima(GeneralFunctions.enm2DParameter.depth)
            Dim MaxLevels As Double() = myFouNC.get2DMaxima(GeneralFunctions.enm2DParameter.waterlevel)

            ' Add results to lists
            If depthResults.Count = 0 Then
                For j As Integer = 0 To MaxDepths.Length - 1
                    depthResults.Add(New List(Of Double)())
                    levelResults.Add(New List(Of Double)())
                Next
            End If

            For j As Integer = 0 To MaxDepths.Length - 1
                depthResults(j).Add(MaxDepths(j))
                levelResults(j).Add(MaxLevels(j))
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function readFouToCSV of class clsStochastenanalyse: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub WriteTransposedCSV(fileName As String, runIDs As List(Of String), probabilities As List(Of Double), results As List(Of List(Of Double)))
        Using writer As New StreamWriter(fileName)
            ' Write header
            writer.WriteLine("FEATUREIDX;" & String.Join(";", runIDs))
            writer.WriteLine("P;" & String.Join(";", probabilities))

            ' Write data
            For i As Integer = 0 To results.Count - 1
                Dim rowBuilder As New StringBuilder(i.ToString())
                For Each value In results(i)
                    rowBuilder.Append(";").Append(String.Format("{0:N4}", value))
                Next
                writer.WriteLine(rowBuilder.ToString())
            Next
        End Using
    End Sub

    Public Function readFouToSQLite(ByRef myRun As clsStochastenRun, ByRef myFile As clsResultsFile) As Boolean
        'this function is the result of refactoring by Claude AI on 2024-06-27
        Try
            Dim path As String = System.IO.Path.Combine(myRun.OutputFilesDir, myFile.FileName)
            If Not System.IO.File.Exists(path) Then
                Throw New FileNotFoundException("Fourier file does not exist", path)
            End If

            Dim myFouNC As New clsFouNCFile(path, Me.Setup)
            If Not myFouNC.Read() Then
                Throw New Exception("Error reading fourier file " & path)
            End If

            Dim MaxDepths As Double() = myFouNC.get2DMaxima(GeneralFunctions.enm2DParameter.depth)
            Dim MaxLevels As Double() = myFouNC.get2DMaxima(GeneralFunctions.enm2DParameter.waterlevel)

            Using transaction = Me.Setup.SqliteCon.BeginTransaction()
                Using myCmd As New SQLite.SQLiteCommand(Me.Setup.SqliteCon)
                    InsertBatch(myCmd, MaxDepths, GeneralFunctions.enm2DParameter.depth, myRun)
                    InsertBatch(myCmd, MaxLevels, GeneralFunctions.enm2DParameter.waterlevel, myRun)
                End Using
                transaction.Commit()
            End Using

            'chatGPT Perform WAL checkpoint
            Using cmd As New SQLite.SQLiteCommand("PRAGMA wal_checkpoint(TRUNCATE);", Me.Setup.SqliteCon)
                cmd.ExecuteNonQuery()
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function readFouToSQLite of class clsStochastenanalyse: " & ex.Message)
            Return False
        End Try
    End Function

    'Private Sub InsertBatch(cmd As SQLite.SQLiteCommand, data As Double(), parameter As GeneralFunctions.enm2DParameter, myRun As clsStochastenRun)
    '    Dim sb As New StringBuilder()
    '    For j = 0 To data.Length - 1
    '        sb.AppendLine($"(@KlimaatScenario,@Duration,{j},@RunID,@Parameter,{data(j)},0,0,@P),")
    '    Next
    '    Dim batchInsert = $"INSERT INTO RESULTATEN2D (KLIMAATSCENARIO, DUUR, FEATUREIDX, RUNID, PARAMETER, MAXVAL, MINVAL, AVGVAL, P) VALUES {sb.ToString().TrimEnd(",")}"

    '    cmd.CommandText = batchInsert
    '    cmd.Parameters.AddWithValue("@KlimaatScenario", Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper)
    '    cmd.Parameters.AddWithValue("@Duration", Setup.StochastenAnalyse.Duration)
    '    cmd.Parameters.AddWithValue("@RunID", myRun.ID)
    '    cmd.Parameters.AddWithValue("@Parameter", parameter.ToString)
    '    cmd.Parameters.AddWithValue("@P", myRun.P)

    '    cmd.ExecuteNonQuery()
    'End Sub

    Private Function InsertBatch(cmd As SQLite.SQLiteCommand, data As Double(), parameter As GeneralFunctions.enm2DParameter, myRun As clsStochastenRun) As Boolean
        Try
            Const BatchSize As Integer = 100 ' Adjust this value as needed

            Dim TableName As String
            TableName = get2DResultsTableName(parameter, Setup.StochastenAnalyse.KlimaatScenario.ToString, Setup.StochastenAnalyse.Duration)

            cmd.Parameters.AddWithValue("@RunID", myRun.ID)
            cmd.Parameters.AddWithValue("@P", myRun.P)

            Dim sb As New StringBuilder()

            For i As Integer = 0 To data.Length - 1 Step BatchSize
                sb.Clear()
                sb.Append($"INSERT INTO {TableName} (FEATUREIDX, RUNID, WAARDE, P) VALUES ")

                For j As Integer = i To Math.Min(i + BatchSize - 1, data.Length - 1)
                    sb.AppendLine($"({j}, @RunID, @Val{j}, @P),")
                    cmd.Parameters.AddWithValue($"@Val{j}", data(j))
                Next

                ' Remove the trailing comma and add semicolon
                sb.Length -= 3
                sb.Append(";"c)

                cmd.CommandText = sb.ToString()
                cmd.ExecuteNonQuery()

                ' Clear the batch-specific parameters
                For j As Integer = i To Math.Min(i + BatchSize - 1, data.Length - 1)
                    cmd.Parameters.RemoveAt($"@Val{j}")
                Next
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Function ExportResults1D() As Boolean
        Try
            'open the database connection
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Dim cmd As New SQLite.SQLiteCommand
            cmd.Connection = Me.Setup.SqliteCon

            Dim r As Long = 0, c As Long = 0
            Dim i As Long, n As Long
            Dim dtLoc As New DataTable 'locations
            Dim dtRes As DataTable 'results
            Dim dtRuns As DataTable 'runs
            Dim dtHerh As DataTable

            'prepare a spreadsheet
            Dim ws As clsExcelSheet = Me.Setup.ExcelFile.GetAddSheet("1D_Herh" & "_" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "_" & Setup.StochastenAnalyse.Duration.ToString)
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
            dtLoc = GetUnique1DLocationNamesFromDB()

            'for every location, build an exceedance chart and export it to CSV
            'also write it to Excel, in a less detailed manner
            n = dtLoc.Rows.Count - 1
            Setup.GeneralFunctions.UpdateProgressBar("Exporting results per location...", 0, n)

            ' Create a new ZIP archive to store the CSV files
            If System.IO.File.Exists(ResultsDir & "\Results1D.zip") Then System.IO.File.Delete(ResultsDir & "\Results1D.zip")
            Using zip As ZipArchive = ZipFile.Open(ResultsDir & "\Results1D.zip", ZipArchiveMode.Create)
                For i = 0 To n

                    Setup.GeneralFunctions.UpdateProgressBar("", i, n)

                    'create a worksheet for this location
                    'ws = Me.Setup.ExcelFile.GetAddSheet(dtLoc.Rows(i)(0))

                    dtRes = New DataTable
                    dtRuns = New DataTable
                    dtHerh = New DataTable

                    ' Calculate an exceedance table for this location
                    If Not CalcExceedanceTable(dtLoc.Rows(i)(0), "MAXVAL", dtRes, dtHerh) Then
                        Setup.Log.AddError("Error calculating exceedance table for location: " & dtLoc.Rows(i)("LOCATIENAAM"))
                    Else
                        ' Write the CSV content to a MemoryStream
                        Using ms As New MemoryStream()
                            Using sw As New StreamWriter(ms, Encoding.UTF8, 1024, True)
                                Setup.GeneralFunctions.DataTable2CSVByStreamWriter(dtHerh, sw, ";")
                                sw.Flush()
                                ms.Position = 0

                                ' Create a new entry in the ZIP file for the CSV
                                Dim zipEntry As ZipArchiveEntry = zip.CreateEntry(dtLoc.Rows(i)("LOCATIENAAM") & ".csv")
                                Using entryStream As Stream = zipEntry.Open()
                                    ms.CopyTo(entryStream)
                                End Using
                            End Using
                        End Using

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
            End Using


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ExportResults of class clsStochastenAnalyse.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function ExportResults2DFromDB(parameter As GeneralFunctions.enm2DParameter) As Boolean
        Try
            ' Open the database connection
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Dim cmd As New SQLite.SQLiteCommand
            cmd.Connection = Me.Setup.SqliteCon

            ' Get the name of the table containing our return periods
            Dim HerhTableName As String
            HerhTableName = get2DReturnPeriodsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)

            Dim r As Long = 0, c As Long = 0
            Dim featureidx As Integer
            Dim i As Long, n As Long
            Dim dtLoc As New DataTable ' Locations
            Dim dtHerh As New Dictionary(Of Integer, DataTable) 'every feature gets its own datatable

            'prepare a spreadsheet
            Dim rowidx As Integer = 0
            Dim ws As clsExcelSheet = Me.Setup.ExcelFile.GetAddSheet("2D_Herh" & "_" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "_" & Setup.StochastenAnalyse.Duration.ToString)
            ws.ws.Cells(rowidx, 0).Value = "FEATUREIDX"
            ws.ws.Cells(rowidx, 1).Value = "Alias"
            ws.ws.Cells(rowidx, 2).Value = 0.01
            ws.ws.Cells(rowidx, 3).Value = 0.05
            ws.ws.Cells(rowidx, 4).Value = 0.1
            ws.ws.Cells(rowidx, 5).Value = 0.5
            ws.ws.Cells(rowidx, 6).Value = 1
            ws.ws.Cells(rowidx, 7).Value = 2
            ws.ws.Cells(rowidx, 8).Value = 3
            ws.ws.Cells(rowidx, 9).Value = 4
            ws.ws.Cells(rowidx, 10).Value = 5
            ws.ws.Cells(rowidx, 11).Value = 6
            ws.ws.Cells(rowidx, 12).Value = 7
            ws.ws.Cells(rowidx, 13).Value = 8
            ws.ws.Cells(rowidx, 14).Value = 9
            ws.ws.Cells(rowidx, 15).Value = 10
            ws.ws.Cells(rowidx, 16).Value = 15
            ws.ws.Cells(rowidx, 17).Value = 20
            ws.ws.Cells(rowidx, 18).Value = 25
            ws.ws.Cells(rowidx, 19).Value = 30
            ws.ws.Cells(rowidx, 20).Value = 35
            ws.ws.Cells(rowidx, 21).Value = 40
            ws.ws.Cells(rowidx, 22).Value = 45
            ws.ws.Cells(rowidx, 23).Value = 50
            ws.ws.Cells(rowidx, 24).Value = 60
            ws.ws.Cells(rowidx, 25).Value = 70
            ws.ws.Cells(rowidx, 26).Value = 80
            ws.ws.Cells(rowidx, 27).Value = 90
            ws.ws.Cells(rowidx, 28).Value = 100
            ws.ws.Cells(rowidx, 29).Value = 200
            ws.ws.Cells(rowidx, 30).Value = 300
            ws.ws.Cells(rowidx, 31).Value = 400
            ws.ws.Cells(rowidx, 32).Value = 500
            ws.ws.Cells(rowidx, 33).Value = 600
            ws.ws.Cells(rowidx, 34).Value = 700
            ws.ws.Cells(rowidx, 35).Value = 800
            ws.ws.Cells(rowidx, 36).Value = 900
            ws.ws.Cells(rowidx, 37).Value = 1000

            ' Get all unique location names from the database
            Setup.GeneralFunctions.UpdateProgressBar("Processing all cells...", 0, n)
            dtLoc = GetUnique2DLocationNamesFromDB(parameter)
            Dim nLoc As Integer = dtLoc.Rows.Count


            'Create a new ZIP archive
            If System.IO.File.Exists(ResultsDir & "\Results2D.zip") Then System.IO.File.Delete(ResultsDir & "\Results2D.zip")
            Using zip As ZipArchive = ZipFile.Open(ResultsDir & "\Results2D.zip", ZipArchiveMode.Create)

                'we work in chunks of 1000 features
                Dim chunksize As Integer = 1000

                For chunkStart As Integer = 0 To nLoc - 1 Step chunksize

                    Me.Setup.GeneralFunctions.UpdateProgressBar("", chunkStart, nLoc, False)
                    Dim chunkEnd As Integer = Math.Min(chunkStart + chunksize - 1, nLoc - 1)

                    ' Clear the dictionary for the new chunk
                    dtHerh.Clear()

                    ' Prepare the query for all features in the chunk
                    Dim query As String = $"SELECT RUNID, FEATUREIDX, HERHALINGSTIJD, WAARDE FROM {HerhTableName} WHERE FEATUREIDX IN ("
                    For i = chunkStart To chunkEnd
                        query += $"{dtLoc.Rows(i)("FEATUREIDX")}"
                        If i < chunkEnd Then query += ","
                    Next
                    query += ") ORDER BY FEATUREIDX, HERHALINGSTIJD"

                    Dim dt As New DataTable
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, False)

                    Dim prevFeature As Integer = -1
                    For i = 0 To dt.Rows.Count - 1
                        featureidx = dt.Rows(i)("FEATUREIDX")
                        If featureidx <> prevFeature Then
                            dtHerh.Add(featureidx, New DataTable)
                            dtHerh(featureidx).Columns.Add("HERHALINGSTIJD", GetType(Double))
                            dtHerh(featureidx).Columns.Add("WAARDE", GetType(Double))
                        End If
                        dtHerh(featureidx).Rows.Add(dt.Rows(i)("HERHALINGSTIJD"), dt.Rows(i)("WAARDE"))
                        prevFeature = featureidx
                    Next

                    'now that our chunk has been read, we can write the results to Excel
                    For Each key As Integer In dtHerh.Keys
                        'For i = 0 To dtHerh.Count - 1
                        dt = dtHerh.Item(key)
                        'each feature gets its own row
                        rowidx += 1
                        ws.ws.Cells(rowidx, 0).Value = key
                        ws.ws.Cells(rowidx, 1).Value = key
                        ws.ws.Cells(rowidx, 2).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 0.01, 0, 1)
                        ws.ws.Cells(rowidx, 3).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 0.05, 0, 1)
                        ws.ws.Cells(rowidx, 4).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 0.1, 0, 1)
                        ws.ws.Cells(rowidx, 5).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 0.5, 0, 1)
                        ws.ws.Cells(rowidx, 6).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 1, 0, 1)
                        ws.ws.Cells(rowidx, 7).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 2, 0, 1)
                        ws.ws.Cells(rowidx, 8).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 3, 0, 1)
                        ws.ws.Cells(rowidx, 9).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 4, 0, 1)
                        ws.ws.Cells(rowidx, 10).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 5, 0, 1)
                        ws.ws.Cells(rowidx, 11).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 6, 0, 1)
                        ws.ws.Cells(rowidx, 12).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 7, 0, 1)
                        ws.ws.Cells(rowidx, 13).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 8, 0, 1)
                        ws.ws.Cells(rowidx, 14).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 9, 0, 1)
                        ws.ws.Cells(rowidx, 15).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 10, 0, 1)
                        ws.ws.Cells(rowidx, 16).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 15, 0, 1)
                        ws.ws.Cells(rowidx, 17).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 20, 0, 1)
                        ws.ws.Cells(rowidx, 18).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 25, 0, 1)
                        ws.ws.Cells(rowidx, 19).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 30, 0, 1)
                        ws.ws.Cells(rowidx, 20).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 35, 0, 1)
                        ws.ws.Cells(rowidx, 21).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 40, 0, 1)
                        ws.ws.Cells(rowidx, 22).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 45, 0, 1)
                        ws.ws.Cells(rowidx, 23).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 50, 0, 1)
                        ws.ws.Cells(rowidx, 24).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 60, 0, 1)
                        ws.ws.Cells(rowidx, 25).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 70, 0, 1)
                        ws.ws.Cells(rowidx, 26).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 80, 0, 1)
                        ws.ws.Cells(rowidx, 27).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 90, 0, 1)
                        ws.ws.Cells(rowidx, 28).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 100, 0, 1)
                        ws.ws.Cells(rowidx, 29).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 200, 0, 1)
                        ws.ws.Cells(rowidx, 30).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 300, 0, 1)
                        ws.ws.Cells(rowidx, 21).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 400, 0, 1)
                        ws.ws.Cells(rowidx, 31).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 500, 0, 1)
                        ws.ws.Cells(rowidx, 32).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 600, 0, 1)
                        ws.ws.Cells(rowidx, 33).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 700, 0, 1)
                        ws.ws.Cells(rowidx, 34).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 800, 0, 1)
                        ws.ws.Cells(rowidx, 35).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 900, 0, 1)
                        ws.ws.Cells(rowidx, 36).Value = Setup.GeneralFunctions.InterpolateFromDataTable(dt, 1000, 0, 1)

                        'write the raw results for this feature to CSV
                        Using ms As New MemoryStream()
                            Using sw As New StreamWriter(ms, Encoding.UTF8, 1024, True)
                                Setup.GeneralFunctions.DataTable2CSVByStreamWriter(dt, sw, ";")
                                sw.Flush()
                                ms.Position = 0

                                ' Create a new entry in the ZIP file for the CSV
                                Dim zipEntry As ZipArchiveEntry = zip.CreateEntry(key & ".csv")
                                Using entryStream As Stream = zipEntry.Open()
                                    ms.CopyTo(entryStream)
                                End Using
                            End Using
                        End Using
                    Next

                Next
            End Using

            Me.Setup.SqliteCon.Close()
            Setup.GeneralFunctions.UpdateProgressBar("Export complete.", 0, n)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ExportResults2D of class clsStochastenAnalyse.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function ExportResults2DFromCSV(parameter As GeneralFunctions.enm2DParameter) As Boolean
        Try
            'Set the paths to the 2D results and the stochasts in csv format
            Dim ExceedanceTable2DPath As String = getExceedanceTable2DPath(ResultsDir, KlimaatScenario, parameter)
            Dim StochastsPath As String = getStochastsPath(ResultsDir)

            'Prepare a spreadsheet
            Dim rowidx As Integer = 0
            Dim ws As clsExcelSheet = Me.Setup.ExcelFile.GetAddSheet("2D_Herh" & "_" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "_" & Setup.StochastenAnalyse.Duration.ToString)

            'Write header row
            Dim headerValues() As Double = {0.01, 0.05, 0.1, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000}
            ws.ws.Cells(rowidx, 0).Value = "FEATUREIDX"
            ws.ws.Cells(rowidx, 1).Value = "Alias"
            For i As Integer = 0 To headerValues.Length - 1
                ws.ws.Cells(rowidx, i + 2).Value = headerValues(i)
            Next

            'Read the CSV file
            Dim exceedanceData As New DataTable
            Using reader As New TextFieldParser(ExceedanceTable2DPath)
                reader.SetDelimiters(",")
                reader.HasFieldsEnclosedInQuotes = True

                'Read column names
                Dim columns() As String = reader.ReadFields()
                For Each column In columns
                    exceedanceData.Columns.Add(column)
                Next

                'Read data
                While Not reader.EndOfData
                    exceedanceData.Rows.Add(reader.ReadFields())
                End While
            End Using

            'Get all unique location names
            Dim dtLoc = exceedanceData.DefaultView.ToTable(True, "FEATUREIDX")
            Dim nLoc As Integer = dtLoc.Rows.Count

            'Create a new ZIP archive
            If System.IO.File.Exists(ResultsDir & "\Results2D.zip") Then System.IO.File.Delete(ResultsDir & "\Results2D.zip")
            Using zip As ZipArchive = ZipFile.Open(ResultsDir & "\Results2D.zip", ZipArchiveMode.Create)

                'We work in chunks of 1000 features
                Dim chunksize As Integer = 1000

                For chunkStart As Integer = 0 To nLoc - 1 Step chunksize
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", chunkStart, nLoc, False)
                    Dim chunkEnd As Integer = Math.Min(chunkStart + chunksize - 1, nLoc - 1)

                    'Process each feature in the chunk
                    For i As Integer = chunkStart To chunkEnd
                        Dim featureIdx As Integer = CInt(dtLoc.Rows(i)("FEATUREIDX"))

                        'Filter data for this feature
                        Dim featureData = exceedanceData.Select($"FEATUREIDX = {featureIdx}", "HERHALINGSTIJD ASC")

                        If featureData.Length > 0 Then
                            'Create a DataTable for this feature
                            Dim dt As New DataTable()
                            dt.Columns.Add("HERHALINGSTIJD", GetType(Double))
                            dt.Columns.Add("WAARDE", GetType(Double))
                            For Each row In featureData
                                dt.Rows.Add(CDbl(row("HERHALINGSTIJD")), CDbl(row("WAARDE")))
                            Next

                            'Write to Excel
                            rowidx += 1
                            ws.ws.Cells(rowidx, 0).Value = featureIdx
                            ws.ws.Cells(rowidx, 1).Value = featureIdx
                            For j As Integer = 0 To headerValues.Length - 1
                                ws.ws.Cells(rowidx, j + 2).Value = InterpolateFromDataTable(dt, headerValues(j), "HERHALINGSTIJD", "WAARDE")
                            Next

                            'Write raw results to CSV in ZIP
                            Using ms As New MemoryStream()
                                Using sw As New StreamWriter(ms, Encoding.UTF8, 1024, True)
                                    Setup.GeneralFunctions.DataTable2CSVByStreamWriter(dt, sw, ";")
                                    sw.Flush()
                                    ms.Position = 0

                                    Dim zipEntry As ZipArchiveEntry = zip.CreateEntry($"{featureIdx}.csv")
                                    Using entryStream As Stream = zipEntry.Open()
                                        ms.CopyTo(entryStream)
                                    End Using
                                End Using
                            End Using
                        End If
                    Next
                Next
            End Using

            Setup.GeneralFunctions.UpdateProgressBar("Export complete.", 0, nLoc)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ExportResults2DFromCSV of class clsStochastenAnalyse.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function




    Public Function GetUnique1DLocationNamesFromDB() As DataTable
        Dim query As String = "SELECT DISTINCT LOCATIENAAM FROM OUTPUTLOCATIONS;"
        Dim dtLoc As New DataTable
        Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtLoc, False)
        Return dtLoc
    End Function
    Public Function GetUnique2DLocationNamesFromDB(parameter As GeneralFunctions.enm2DParameter) As DataTable
        Dim HerhTableName As String
        HerhTableName = get2DReturnPeriodsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)
        Dim query As String = $"SELECT DISTINCT FEATUREIDX FROM {HerhTableName};"
        Dim dtLoc As New DataTable
        Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtLoc, False)
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


    Public Sub UpdateVolumesCheckSum(ByRef grVolumesZomer As DataGridView, ByRef grVolumesWinter As DataGridView, ByRef lblChecksum As System.Windows.Forms.Label)
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

    Public Function getBuiVerloop(ByVal Patroon As String, SimulationModel As GeneralFunctions.enmSimulationModel) As Double()

        '------------------------------------------------------------------------
        'Author: Siebe Bosch
        'date: 17-8-2014
        'Description: gets the pattern for a rainfall event from the database
        '------------------------------------------------------------------------


        'duration must already have been set, and also the path to the database
        Dim myPatroon As Double()

        'we add one extra timestep after the event, hence not duration -1
        Select Case SimulationModel
            Case GeneralFunctions.enmSimulationModel.HBV
                'for HBV we'll add one extra (empty) timestep after the event in order to match with the start- and end date of the event
                ReDim myPatroon(Duration)
            Case Else
                ReDim myPatroon(Duration - 1)
        End Select
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

            Select Case SimulationModel
                Case GeneralFunctions.enmSimulationModel.HBV
                    For i = 0 To Duration - 1
                        myPatroon(i) = dt.Rows(i)(0)
                    Next
                    myPatroon(Duration) = 0 'add an empty timestep after the event. let op: dit is een extra tijdstap na de bui, maar het is nog maar de vraag of het volume van elke tijdstap representatief is voor de tijdstap vóór de datum of juist ná de datum
                Case Else
                    For i = 0 To Duration - 1
                        myPatroon(i) = dt.Rows(i)(0)
                    Next
            End Select

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

        'v2.3.5: auto-fill the first column
        myGrid.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader 'in the first column we display the run's unique ID so fill
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
                    If Not System.IO.File.Exists(myRun.OutputFilesDir & "\" & myFile.FileName) Then
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


    Public Function AssignMeteoStationNumbersFromHBVProject(ByRef myProject As clsHBVProject) As Boolean
        Try
            'this function assigns meteo station numbers to the meteo stations by reading the original meteo files from the project
            For Each myStation As clsMeteoStation In MeteoStations.MeteoStations.Values
                If myStation.StationType = GeneralFunctions.enmMeteoStationType.precipitation Then
                    'first check if a meteo file exists for this station
                    Dim MeteoFile As String = myProject.ProjectDir & "\" & myStation.Name & ".txt"
                    If Not System.IO.File.Exists(MeteoFile) Then Throw New Exception("No precipitation file named " & myStation.Name & ".txt found in project directory. Please make sure the meteo stations specified match the files in the project.")

                    'read the meteo file. The station number is in the second row of our HBV txt meteo files
                    Using myReader As New StreamReader(MeteoFile)
                        myReader.ReadLine()
                        myStation.Number = Convert.ToInt16(myReader.ReadLine())
                    End Using
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AssignMeteoStationNumbers of class clsHBVProject: " & ex.Message)
            Return False
        End Try
    End Function


End Class
