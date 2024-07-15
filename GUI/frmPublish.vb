Imports Ionic.Zip
Imports System.IO
Imports STOCHLIB
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class frmPublish

    Private Setup As clsSetup
    Dim export1D As Boolean
    Dim export2D As Boolean
    Dim Climate As String
    Dim Duration As Integer
    Dim PeilgebiedenSF As String

    Public Sub New(ByVal Setup As clsSetup, iExport1D As Boolean, iExport2D As Boolean, iClimate As String, iDuration As Integer, iPeilgebiedenSF As String)
        InitializeComponent()
        Me.Setup = Setup
        export1D = iExport1D
        export2D = iExport2D
        Climate = iClimate
        Duration = iDuration
        PeilgebiedenSF = iPeilgebiedenSF

    End Sub

    Private Sub frmPublish_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.SetProgress(prProgress, lblProgress)
    End Sub

    Private Sub btnPubliceren_Click(sender As Object, e As EventArgs) Handles btnPubliceren.Click

        Try

            If radNewWebviewer.Checked Then

                'this routine copies all source files for the viewer (HTML + CSS + JS) to an export directory specified by the user
                'it then executes the HTML file

                dlgFolder.ShowDialog()
                Dim ExtractDir As String = dlgFolder.SelectedPath
                Dim ViewerDir As String = ExtractDir & "\Stochastenviewer"
                If Not Directory.Exists(ViewerDir) Then Directory.CreateDirectory(ViewerDir)

                Me.Cursor = Cursors.WaitCursor

                Dim Paths As New Collection
                Dim ZipPath As String = ""

                'make sure our progress bar has been set
                Me.Setup.SetProgress(prProgress, lblProgress)

                If Debugger.IsAttached Then
                    'in debug mode we will retrieve the zip file from our GITHUB directory
                    ZipPath = "c:\GITHUB\DeNieuweStochastentool\InnoSetup\Stochastenviewer.zip"
                    If Not System.IO.File.Exists(ZipPath) Then Throw New Exception("Error: could not find Stochastenviewer.zip in debug directory " & ZipPath)
                Else
                    'in release mode we will retrieve the zip file from within our application directory
                    ZipPath = My.Application.Info.DirectoryPath & "\viewer\Stochastenviewer.zip"
                    If Not System.IO.File.Exists(ZipPath) Then Throw New Exception("Error: could not find Stochastenviewer.zip in application directory " & My.Application.Info.DirectoryPath)
                End If

                'in our exe dir there should be a zip-file containing our viewer's source
                Dim Zip As New ZipFile(ZipPath)
                Zip.ExtractAll(ExtractDir, ExtractExistingFileAction.OverwriteSilently)

                'write the locations and all results to the JSON files
                Call WriteStochastsJSON(ViewerDir & "\js\stochasts.js")
                Call WriteSubcatchmentsJSON(ViewerDir & "\js\subcatchments.js")

                Setup.StochastenAnalyse.WriteStochasticRunsJSON(ViewerDir & "\js\runs.js", Climate, Duration, txtConfigurationName.Text)

                If export2D Then
                    'write the 2D mesh to a Mesh.js file and the 2D mesh results to a Meshresults.js
                    Setup.StochastenAnalyse.CalculateExceedanceMesh(ViewerDir & "\js\exceedancemesh.js", Climate, Duration)
                    Call WriteExceedanceData2DJSON(ViewerDir & "\js\exceedancedata2D.js", txtConfigurationName.Text)
                End If

                If export1D Then
                    Call WriteLocationsJSON(ViewerDir & "\js\locations.js")
                    Call WriteResultsJSON(ViewerDir & "\js\results.js")
                    Call WriteExceedanceData1DJSON(ViewerDir & "\js\exceedancedata.js")
                End If

                Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
                Me.Cursor = Cursors.Default

                'execute the html file in the default browser
                System.Diagnostics.Process.Start(ViewerDir & "\index.html")

            ElseIf radExistingWebviewer.Checked Then
                'first we'll browse to the existing webviewer directory
                Dim res As DialogResult = dlgFolder.ShowDialog()
                If res = DialogResult.OK Then
                    Dim ViewerDir As String = dlgFolder.SelectedPath

                    'make sure our directory contains a subir called js
                    If Not Directory.Exists(ViewerDir & "\js") Then Throw New Exception("Error: could not find subdirectory js in selected directory " & ViewerDir)

                    'we'll append the run data to the runs.js file
                    Setup.StochastenAnalyse.AddStochasticRunsJSON(ViewerDir & "\js\runs.js", Climate, Duration, txtConfigurationName.Text)
                    Setup.StochastenAnalyse.AddExceedanceData1DJSON(ViewerDir & "\js\exceedancedata.js", Climate, Duration, txtConfigurationName.Text)
                    Setup.StochastenAnalyse.AddExceedanceLevels2DJSON(ViewerDir & "\js\exceedancedata2D.js", Climate, Duration, txtConfigurationName.Text)

                    'execute the html file in the default browser
                    System.Diagnostics.Process.Start(ViewerDir & "\index.html")

                End If
            End If


        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            MsgBox(ex.Message)
        End Try

    End Sub


    Public Function WriteStochastsJSON(path As String) As Boolean
        Try
            Dim vt As New DataTable, query As String
            If System.IO.File.Exists(path) Then System.IO.File.Delete(path)
            Using stochastWriter As New StreamWriter(path)
                stochastWriter.WriteLine("let stochasts = {")
                stochastWriter.WriteLine("    " & Chr(34) & "stochasts" & Chr(34) & ": [")

                'process the climate scenarios
                'vt = New DataTable
                'query = "SELECT DISTINCT KLIMAATSCENARIO FROM RUNS WHERE DUUR=" & cmbDuration.Text & ";"
                'Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                'WriteStochastJSON(stochastWriter, "Klimaat", vt, False)

                'process the stochast Seizoen
                vt = New DataTable
                query = "SELECT DISTINCT SEIZOEN FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Seizoen", vt, False)

                'process the stochast Volume
                vt = New DataTable
                query = "SELECT DISTINCT VOLUME FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Volume", vt, False)

                'process the stochast Patroon
                vt = New DataTable
                query = "SELECT DISTINCT PATROON FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Patroon", vt, False)

                'process the stochast Initial (groundwater)
                vt = New DataTable
                query = "SELECT DISTINCT GW FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Init", vt, False)

                'process the stochast Boundary
                vt = New DataTable
                query = "SELECT DISTINCT BOUNDARY FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Boundary", vt, False)

                'process the stochast Wind
                vt = New DataTable
                query = "SELECT DISTINCT WIND FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Wind", vt, False)

                'process the stochast Extra1
                vt = New DataTable
                query = "SELECT DISTINCT EXTRA1 FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Extra1", vt, False)

                'process the stochast Extra2
                vt = New DataTable
                query = "SELECT DISTINCT EXTRA2 FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Extra2", vt, False)

                'process the stochast Extra3
                vt = New DataTable
                query = "SELECT DISTINCT EXTRA3 FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Extra3", vt, False)

                'process the stochast Extra4
                vt = New DataTable
                query = "SELECT DISTINCT EXTRA4 FROM RUNS WHERE DUUR=" & Duration & ";"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, vt, False)
                WriteStochastJSON(stochastWriter, "Extra4", vt, True)

                stochastWriter.WriteLine("    ]")
                stochastWriter.WriteLine("}")

            End Using
        Catch ex As Exception

        End Try

    End Function


    Public Function WriteSubcatchmentsJSON(path As String) As Boolean
        Try
            'start by transforming the subcatchments shapefile into a geojson file. Later we will read this file into memory and write it to our data.js
            Dim utils As New MapWinGIS.Utils
            If System.IO.File.Exists(path) Then System.IO.File.Delete(path)
            utils.OGR2OGR(PeilgebiedenSF, path, "-f GeoJSON -t_srs EPSG:4326")

            'read the subcatchments json to memory and convert it into JS content
            Dim areaJSON As String = "let subcatchments =" & vbCrLf
            Using areaReader As New StreamReader(path)
                areaJSON &= areaReader.ReadToEnd
            End Using

            'write the subcatchments json contents to a .js file
            Using areaWriter As New StreamWriter(path)
                areaWriter.Write(areaJSON)
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Function WriteExceedanceData2DJSON(path As String, configurationName As String) As Boolean
        Try
            Return Me.Setup.StochastenAnalyse.WriteExceedanceLevels2DJSON(path, Climate, Duration, configurationName)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteFouExceedanceJSON: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function WriteLocationsJSON(locationsdatapath As String) As Boolean
        Try
            'now reread our results locations!
            Dim dtLoc = New DataTable
            Dim query As String = "SELECT MODELID, MODULE, RESULTSFILE, MODELPAR, LOCATIEID, LOCATIENAAM, RESULTSTYPE, X, Y, LAT, LON, WP, ZP FROM OUTPUTLOCATIONS;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtLoc, True)

            'write the dataset to json
            If System.IO.File.Exists(locationsdatapath) Then System.IO.File.Delete(locationsdatapath)
            Using locationsWriter As New StreamWriter(locationsdatapath)

                locationsWriter.WriteLine("let locations = {")
                locationsWriter.WriteLine("    " & Chr(34) & "locations" & Chr(34) & ": [")

                Me.Setup.GeneralFunctions.UpdateProgressBar("Writing results...", 0, 10, True)
                Dim resNum As Integer = 0

                'process each location and write it to the locations.js
                For i = 0 To dtLoc.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, dtLoc.Rows.Count)

                    'for this location we will retrieve the exceedance table
                    Dim dtHerh As New DataTable
                    query = "SELECT HERHALINGSTIJD, WAARDE FROM HERHALINGSTIJDEN WHERE LOCATIENAAM='" & dtLoc.Rows(i)("LOCATIENAAM") & "' AND KLIMAATSCENARIO='" & Climate & "' AND DUUR=" & Duration & " ORDER BY HERHALINGSTIJD;"
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtHerh, False)

                    If dtHerh.Rows.Count > 0 Then
                        'now that we have our exceedance table, we can start writing it to the data.js file
                        'NOTE: we replace double quotes first by percentage %. Later we will replace all % characters again with double quotes
                        Dim T10 As Double = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 10, 0, 1)
                        Dim T25 As Double = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 25, 0, 1)
                        Dim T50 As Double = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 50, 0, 1)
                        Dim T100 As Double = Setup.GeneralFunctions.InterpolateFromDataTable(dtHerh, 100, 0, 1)
                        Dim ZPString As String, WPString As String
                        If IsDBNull(dtLoc(i)("ZP")) Then ZPString = "%ZP%: null" Else ZPString = "%ZP%:" & dtLoc(i)("ZP")
                        If IsDBNull(dtLoc(i)("WP")) Then WPString = "%WP%: null" Else WPString = "%WP%:" & dtLoc(i)("WP")
                        Dim locationsString As String = "        { %ID%: %" & dtLoc.Rows(i)("LOCATIENAAM") & "%, %lat%: " & dtLoc.Rows(i)("lat") & ", %lon%: " & dtLoc.Rows(i)("lon") & ", " & WPString & ", " & ZPString & ", %T10%: " & T10 & ", %T25%: " & T25 & ", %T50%: " & T50 & ", %T100%: " & T100 & "}"
                        If i < dtLoc.Rows.Count - 1 Then locationsString &= ","
                        locationsWriter.WriteLine(locationsString.Replace("%", Chr(34)))
                    End If
                Next
                locationsWriter.WriteLine("    ]")
                locationsWriter.WriteLine("};")
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Function WriteResultsJSON(path As String) As Boolean
        Try
            Dim query As String, runsdt As New DataTable, locsdt As New DataTable
            Dim runidx As Integer, residx As Integer
            Dim Klimaat As String, Seizoen As String, Volume As String, Patroon As String, Init As String, Boundary As String, Wind As String, Extra1 As String, Extra2 As String, Extra3 As String, Extra4 As String
            Dim resultsString As String

            query = "SELECT RUNID, RUNIDX, KLIMAATSCENARIO, SEIZOEN,VOLUME,PATROON,GW,BOUNDARY,WIND,EXTRA1,EXTRA2,EXTRA3,EXTRA4 FROM RUNS WHERE DUUR=" & Duration & " AND KLIMAATSCENARIO='" & Climate & "';"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, runsdt, False)

            query = "SELECT DISTINCT LOCATIENAAM FROM RESULTATEN WHERE DUUR=" & Duration & ";"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, locsdt, True)

            If System.IO.File.Exists(path) Then System.IO.File.Delete(path)
            Using resultsWriter As New StreamWriter(path)

                resultsString = "let results = {"
                resultsWriter.WriteLine(resultsString)
                resultsString = "  ""runs"": ["
                resultsWriter.WriteLine(resultsString)

                Me.Setup.GeneralFunctions.UpdateProgressBar("Writing results to JSON...", 0, 10, True)
                For runidx = 0 To runsdt.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", runidx + 1, runsdt.Rows.Count)
                    'write the current run including its results to JSON
                    If runsdt.Rows(runidx)("KLIMAATSCENARIO") = "" Then Klimaat = "null" Else Klimaat = runsdt.Rows(runidx)("KLIMAATSCENARIO")
                    If runsdt.Rows(runidx)("SEIZOEN") = "" Then Seizoen = "null" Else Seizoen = runsdt.Rows(runidx)("SEIZOEN")
                    If runsdt.Rows(runidx)("VOLUME").ToString = "" Then Volume = "null" Else Volume = runsdt.Rows(runidx)("VOLUME").ToString
                    If runsdt.Rows(runidx)("Patroon") = "" Then Patroon = "null" Else Patroon = runsdt.Rows(runidx)("PATROON")
                    If runsdt.Rows(runidx)("GW") = "" Then Init = "null" Else Init = runsdt.Rows(runidx)("GW")
                    If runsdt.Rows(runidx)("BOUNDARY") = "" Then Boundary = "null" Else Boundary = runsdt.Rows(runidx)("BOUNDARY")
                    If runsdt.Rows(runidx)("WIND") = "" Then Wind = "null" Else Wind = runsdt.Rows(runidx)("WIND")
                    If runsdt.Rows(runidx)("EXTRA1") = "" Then Extra1 = "null" Else Extra1 = runsdt.Rows(runidx)("EXTRA1")
                    If runsdt.Rows(runidx)("EXTRA2") = "" Then Extra2 = "null" Else Extra2 = runsdt.Rows(runidx)("EXTRA2")
                    If runsdt.Rows(runidx)("EXTRA3") = "" Then Extra3 = "null" Else Extra3 = runsdt.Rows(runidx)("EXTRA3")
                    If runsdt.Rows(runidx)("EXTRA4") = "" Then Extra4 = "null" Else Extra4 = runsdt.Rows(runidx)("EXTRA4")

                    'write the runID and the stochasts
                    resultsString = "    {""ID"": """ & runsdt.Rows(runidx)("RUNID") & """, "
                    resultsString &= """RUNIDX"": """ & runsdt.Rows(runidx)("RUNIDX") & """, "
                    resultsString &= """Klimaat"": """ & Klimaat & """, "
                    resultsString &= """Seizoen"": """ & Seizoen & """, "
                    resultsString &= """Volume"": " & Volume & ", "
                    resultsString &= """Patroon"": """ & Patroon & """, "
                    resultsString &= """Init"": """ & Init & """, "
                    resultsString &= """Boundary"": """ & Boundary & """, "
                    resultsString &= """Wind"": """ & Wind & """, "
                    resultsString &= """Extra1"": """ & Extra1 & """, "
                    resultsString &= """Extra2"": """ & Extra2 & """, "
                    resultsString &= """Extra3"": """ & Extra3 & """, "
                    resultsString &= """Extra4"": """ & Extra4 & """, "
                    resultsString &= """results"": ["
                    resultsWriter.WriteLine(resultsString)

                    'write the result for each location for this run
                    query = "SELECT LOCATIENAAM, MAXVAL FROM RESULTATEN WHERE RUNID='" & runsdt.Rows(runidx)("RUNID") & "' AND DUUR = " & Duration & ";"
                    Dim resdt As New DataTable
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, resdt, False)
                    For residx = 0 To resdt.Rows.Count - 1
                        resultsString = "     {""location"": """ & resdt.Rows(residx)(0) & """,""value"":" & resdt.Rows(residx)(1) & "}"
                        If residx < resdt.Rows.Count - 1 Then resultsString &= ","
                        resultsWriter.WriteLine(resultsString)
                    Next
                    resultsWriter.WriteLine("    ]")
                    resultsString = "  }"
                    If runidx < runsdt.Rows.Count - 1 Then resultsString &= ","
                    resultsWriter.WriteLine(resultsString)
                Next
                resultsWriter.WriteLine("  ]")
                resultsWriter.WriteLine("};")
            End Using
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function WriteExceedanceData1DJSON(path As String) As Boolean
        Try
            Return Me.Setup.StochastenAnalyse.WriteExceedanceData1DJSON(path, Climate, Duration, txtConfigurationName.Text)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteExceedanceData1DJSON: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function WriteStochastJSON(ByRef myWriter As StreamWriter, Name As String, dt As DataTable, IsLastStochast As Boolean) As Boolean
        Try
            Dim i As Integer
            Dim myStr As String = "      {%name%: %" & Name & "%, %values%: ["
            myWriter.WriteLine(myStr.Replace("%", Chr(34)))
            For i = 0 To dt.Rows.Count - 1
                myStr = "        {%value%: %" & dt.Rows(i)(0) & "%}"
                If i < dt.Rows.Count - 1 Then myStr &= ","
                myWriter.WriteLine(myStr.Replace("%", Chr(34)))
            Next
            myWriter.WriteLine("      ]")
            myStr = "    }"
            If Not IsLastStochast Then myStr &= "," 'since our stochast is part of an array, add a comma so long as we're not processing the last stochast
            myWriter.WriteLine(myStr.Replace("%", Chr(34)))
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



End Class