
Imports System
Imports STOCHLIB
Imports System.Net
Imports GemBox
Imports GemBox.Spreadsheet
Imports System.Text
Imports System.IO.Packaging
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Module DIMR_RUNR

    Dim Setup As clsSetup
    Dim ScenarioClasses As New Dictionary(Of String, clsDIMRScenarios)
    Dim Runs As New clsDIMRRuns
    Dim OutputFiles As New List(Of String)  'a list of output files we'll copy to a designated folder after each successful simulation

    Sub Main(args As String())

        Try
            setup = New clsSetup
            Dim DimrConfigPath As String
            Dim ExcelConfigPath As String
            Dim BatchFilePath As String

            SpreadsheetInfo.SetLicense("EVIG-1Y89-FYME-DPUJ")

            Console.WriteLine("Welkom bij DIMRRUNR!")
            Console.WriteLine("Dit programma stuurt simulaties met de Deltares Integrated Model Runner (DIMR) aan.")
            Console.WriteLine("Het configureren van de gevraagde simulaties gebeurt in een Excel-bestand.")
            If args.Length <> 3 Then
                Console.WriteLine("Ongeldig aantal argumenten meegegeven aan de applicatie. Geef: pad naar dimr_config, pad naar excel-configuratie en pad naar de batchfile.")
            Else

                DimrConfigPath = args(0)
                ExcelConfigPath = args(1)
                BatchFilePath = args(2)

                If Not System.IO.File.Exists(DimrConfigPath) Then Throw New Exception("Kritieke fout: opgegeven DIMR-configuratiebestand bestaat niet: " & DimrConfigPath)
                If Not System.IO.File.Exists(ExcelConfigPath) Then Throw New Exception("Kritieke fout: opgegeven Excel-configuratiebestand bestaat niet: " & ExcelConfigPath)
                If Not System.IO.File.Exists(BatchFilePath) Then Throw New Exception("Kritieke fout: opgegeven batchfile bestaat niet: " & BatchFilePath)

                Dim ModelDir As String = Path.GetDirectoryName(BatchFilePath)

                'first we will read our DIMR configuration
                Dim DIMR As New clsDIMR(setup, setup.GeneralFunctions.GetDirFromPath(DimrConfigPath))
                DIMR.readConfiguration()




                If Not ReadExcelConfiguration(ExcelConfigPath) Then Throw New Exception("Kritieke fout: uitlezen van het Excel-configuratiebestand is niet geslaagd.")


                'execute the simulations one by one
                For Each Run As clsDIMRRun In Runs.Runs.Values

                    'notice!
                    'every run we wil NOT copy our entire model
                    'instead we will derive new files from our original files and refer to those in an adjusted .MDU and XML
                    'so:
                    'every run gets its own DIMR_Config.xml file, which will refer to a new .MDU file
                    'every new .MDU file will refer to newly created inputfiles for our run (structures.ini, boundaries.bc etc)
                    'AND to a newly created output dir for every run
                    '

                    'so first we write our dimr_config.xml under a new name: dimr_config_runname.xml and set its path as an attribute in the Run
                    Dim RunConfigPath As String = Setup.getDirFromPath(DimrConfigPath) & "\DIMR_config_" & Run.GetName & ".xml"
                    File.Copy(DimrConfigPath, RunConfigPath, True)
                    Run.SetDimrconfigPath(RunConfigPath)

                    'now replace the old MDU filename with the new one inside our copied DIMR_config
                    Dim MDUFile As String = DIMR.DIMRConfig.Flow1D.GetInputFile()
                    Dim RunMDUFile As String = Run.GetName & ".MDU"
                    Run.SetMDUFileName(RunMDUFile)
                    Setup.GeneralFunctions.ReplaceStringInFile(RunConfigPath, MDUFile, RunMDUFile)


                    Stop


                    'only run this simulation if there are no results present in the output dir
                    If Not System.IO.Directory.Exists(Run.ResultsDir) Then System.IO.Directory.CreateDirectory(Run.ResultsDir)
                    If System.IO.Directory.GetFiles(Run.ResultsDir).Length = 0 Then




                        Run.Execute(BatchFilePath, ModelDir)
                        'when complete, copy the results files to our resultsdir
                        For Each ResultsFile As String In OutputFiles

                        Next


                    Else
                        Console.WriteLine("Simulation " & Run.GetName & " was skipped since its results directory " & Run.ResultsDir & " is not empty")
                    End If
                Next

                Stop

            End If
        Catch ex As Exception

        End Try

    End Sub


    Public Function ReadExcelConfiguration(Path As String) As Boolean
        Try
            Dim ScenarioHeaders As New Dictionary(Of Integer, String)
            Dim SimulationHeaders As New Dictionary(Of Integer, String)
            Dim OutputHeaders As New Dictionary(Of Integer, String)

            'Dim ScenarioClasses As New Dictionary(Of String, clsDIMRScenarios)    'use the level/niveau as key
            Dim Scenarios As clsDIMRScenarios
            Dim Scenario As clsDIMRScenario
            Dim Operation As clsDIMRFileOperation = Nothing

            'the simulations themselves
            Dim Run As clsDIMRRun

            'a list of outputfiles that we need to store somewhere
            Dim workbook = ExcelFile.Load(Path)

            ' Iterate through all worksheets in an Excel workbook.
            For Each worksheet In workbook.Worksheets

                Dim r As Integer = 0
                Dim c As Integer = -1

                If worksheet.Name.Trim.ToLower = "scenario's" Then

                    'start by reading the headers
                    While Not worksheet.Cells(r, c + 1).Value = ""
                        c += 1
                        ScenarioHeaders.Add(c, worksheet.Cells(r, c).Value)
                    End While

                    'proceed with reading the content
                    While Not worksheet.Cells(r + 1, 0).Value = ""
                        r += 1
                        Scenario = New clsDIMRScenario()

                        For c = 0 To ScenarioHeaders.Count - 1
                            If ScenarioHeaders.Item(c) = "scenarioklasse" Then
                                Dim className As String = worksheet.Cells(r, c).Value
                                Scenario.ClassName = className
                                If ScenarioClasses.ContainsKey(className.Trim.ToUpper) Then
                                    Scenarios = ScenarioClasses.Item(className.Trim.ToUpper)
                                Else
                                    Scenarios = New clsDIMRScenarios()
                                    ScenarioClasses.Add(className.Trim.ToUpper, Scenarios)
                                End If
                            ElseIf ScenarioHeaders.Item(c) = "scenario" Then
                                Scenario.Name = worksheet.Cells(r, c).Value
                            ElseIf ScenarioHeaders.Item(c) = "actie" Then
                                Operation = New clsDIMRFileOperation()              'create a new operation
                                Operation.SetAction(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "bestandstype" Then
                                Operation.SetFileType(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "header" Then
                                Operation.setHeader(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "objectid" Then
                                Operation.setAttribute(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "attribuut" Then
                                Operation.setObjectID(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "waarde" Then
                                Operation.setValue(worksheet.Cells(r, c).Value)
                                Scenario.AddOperation(Operation)                    'finalize this operation by adding it to the collection
                            End If
                        Next

                        'when done, add our scenario to the corresponding class
                        ScenarioClasses.Item(Scenario.ClassName.Trim.ToUpper).Scenarios.Add(Scenario.Name.Trim.ToUpper, Scenario)

                    End While

                ElseIf worksheet.Name.Trim.ToLower = "simulaties" Then

                    'start by reading the headers
                    While Not worksheet.Cells(r, c + 1).Value = ""
                        c += 1
                        SimulationHeaders.Add(c, worksheet.Cells(r, c).Value)
                    End While

                    'proceed with reading the content
                    While Not worksheet.Cells(r + 1, 0).Value = ""
                        r += 1
                        Run = New clsDIMRRun()
                        For c = 0 To SimulationHeaders.Count - 1
                            If SimulationHeaders.Item(c) = "scenario" Then
                                Scenario = GetScenario(worksheet.Cells(r, c).Value) 'get the corresponding scenario 
                                Run.AddScenario(Scenario)
                            ElseIf SimulationHeaders.Item(c) = "actie" Then
                                Operation = New clsDIMRFileOperation()              'create a new operation
                                Operation.SetAction(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "bestandstype" Then
                                Operation.SetFileType(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "header" Then
                                Operation.setHeader(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "objectid" Then
                                Operation.setObjectID(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "attribuut" Then
                                Operation.setAttribute(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "waarde" Then
                                Operation.setValue(worksheet.Cells(r, c).Value)
                                Run.AddOperation(Operation)                    'finalize this operation by adding it to the collection
                            End If
                        Next
                        Runs.Runs.Add(Run.GetName.Trim.ToUpper, Run)
                    End While

                ElseIf worksheet.Name.Trim.ToLower = "output" Then

                    'start by reading the headers
                    While Not worksheet.Cells(r, c + 1).Value = ""
                        c += 1
                        OutputHeaders.Add(c, worksheet.Cells(r, c).Value)
                    End While

                    'proceed with reading the content
                    While Not worksheet.Cells(r + 1, 0).Value = ""
                        r += 1
                        OutputFiles.Add(worksheet.Cells(r, 0).Value)
                    End While

                End If
            Next
            Console.WriteLine("Inlezen van de Excel-configuratie succesvol.")
            Return True
        Catch ex As Exception
            Console.WriteLine("Fout bij het inlezen van de Excel-configuratie: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function GetScenario(ScenarioName As String) As clsDIMRScenario
        Dim i As Integer
        For i = 0 To ScenarioClasses.Values.Count - 1
            If ScenarioClasses.Values(i).Scenarios.ContainsKey(ScenarioName.Trim.ToUpper) Then
                Return ScenarioClasses.Values(i).Scenarios.Item(ScenarioName.Trim.ToUpper)
            End If
        Next
        Return Nothing
    End Function

End Module
