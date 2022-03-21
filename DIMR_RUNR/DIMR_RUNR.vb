
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
            Setup = New clsSetup
            Dim DimrConfigPath As String
            Dim ExcelConfigPath As String
            Dim BatchFilePath As String

            SpreadsheetInfo.SetLicense("EVIG-1Y89-FYME-DPUJ")

            Console.WriteLine("Welkom bij DIMR_RUNR!")
            Console.WriteLine("Dit programma stuurt simulaties met de Deltares Integrated Model Runner (DIMR) aan.")
            Console.WriteLine("Het configureren van de gevraagde simulaties moet worden gedaan in een Excel-bestand.")
            Console.WriteLine("Zie hiertoe het meegeleverde voorbeeld in de map DIMR_Sample.")
            If args.Length <> 3 Then
                Console.WriteLine("Ongeldig aantal argumenten meegegeven aan de applicatie. Geef: pad naar dimr_config, pad naar excel-configuratie en pad naar de batchfile.")
            Else

                DimrConfigPath = args(0)
                ExcelConfigPath = args(1)
                BatchFilePath = args(2)

                Console.WriteLine("Pad naar de DIMR-configuratie: " & args(0))
                Console.WriteLine("Pad naar de Excel-configuratie: " & args(1))
                Console.WriteLine("Pad naar de batchfile: " & args(2))

                If Not System.IO.File.Exists(DimrConfigPath) Then Throw New Exception("Kritieke fout: opgegeven DIMR-configuratiebestand bestaat niet: " & DimrConfigPath)
                If Not System.IO.File.Exists(ExcelConfigPath) Then Throw New Exception("Kritieke fout: opgegeven Excel-configuratiebestand bestaat niet: " & ExcelConfigPath)
                If Not System.IO.File.Exists(BatchFilePath) Then Throw New Exception("Kritieke fout: opgegeven batchfile bestaat niet: " & BatchFilePath)

                Dim ModelDir As String = Path.GetDirectoryName(DimrConfigPath)
                Console.WriteLine("Modelmap afgeleid uit locatie DIMR-configuratiebestand: " & ModelDir)

                'first we will read our DIMR configuration
                Dim DIMR As New clsDIMR(Setup, Setup.GeneralFunctions.GetDirFromPath(DimrConfigPath))
                DIMR.readConfiguration()

                Console.WriteLine("DIMR-configuratiebestand met succes gelezen.")

                'next, read our Excel-file containg information about the required simulations and their input data
                If Not ReadExcelConfiguration(ExcelConfigPath) Then Throw New Exception("Kritieke fout: uitlezen van het Excel-configuratiebestand is niet geslaagd.")
                Console.WriteLine("Excel-configuratiebestand met succes gelezen.")

                'now execute the simulations one by one
                For Each Run As clsDIMRRun In Runs.Runs.Values

                    Console.WriteLine("Start uitvoering simulatie " & Run.GetName & "...")

                    '20220319: na goede pogingen met het aanpassen van bestanden ipv hele model kopieren moet ik constateren dat dit te foutgevoelig is
                    'daarom nu toch het hele model kopieren, met uitzondering van de output-directory

                    Dim RunDir As String = DIMR.ProjectDir & "\" & Run.GetName
                    If Not Directory.Exists(RunDir) Then Directory.CreateDirectory(RunDir)
                    Console.WriteLine("Directory creëren voor de simulatie: " & RunDir & "...")

                    Dim RunDIMR As clsDIMR
                    Console.WriteLine("Modelschematisatie kopiëren, uitgezonderd de resultatenmap...")
                    RunDIMR = DIMR.CloneCaseForCommandLineRun(RunDir)

                    'finally assign the newly created DIMR instance to our run and read the DIMR configuration
                    Run.SetDIMRProject(RunDIMR)
                    Run.DIMR.readConfiguration()

                    'notice!
                    'every run we wil NOT copy our entire model
                    'instead we will derive new files from our original files and refer to those in an adjusted .MDU and XML
                    'so:
                    'every run gets its own run.bat file, which will refer to a new config.xml file
                    'every run gets its own DIMR_Config.xml file, which will refer to a new .MDU file
                    'every new .MDU file will refer to newly created inputfiles for our run (structures.ini, boundaries.bc etc)
                    'AND to a newly created output dir for every run
                    '

                    ''set the results subdirectory for this run and create it if it doesn't exist
                    'Run.SetResultsDir(Run.GetName, DIMR.DIMRConfig.Flow1D.GetFullDir & "\" & Run.GetName)
                    'If Not Directory.Exists(Run.GetResultsFullDir) Then Directory.CreateDirectory(Run.GetResultsFullDir)
                    'Console.WriteLine("Uitvoermap voor simulatie " & Run.GetName & "ingesteld: " & Run.GetResultsSubDir)

                    ''so first we write our dimr_config.xml under a new name: dimr_config_runname.xml and set its path as an attribute in the Run
                    'Dim RunConfigFilename As String = "DIMR_Config_" & Run.GetName & ".xml"
                    'Dim RunConfigPath As String = Setup.getDirFromPath(DimrConfigPath) & "\" & RunConfigFilename
                    'If Not System.IO.File.Exists(BatchFilePath) Then Throw New Exception("Fout: DIMR_Config.xml niet gevonden: " & DimrConfigPath)
                    'File.Copy(DimrConfigPath, RunConfigPath, True) 'copy the original dimr_config.xml to a special one for this simulation
                    'Run.SetDimrconfigPath(RunConfigPath)
                    'Console.WriteLine("Kopie van de DIMR-configuratie aangemaakt voor simulatie " & Run.GetName & ": " & RunConfigFilename)

                    ''next we write our .bat file under a new name and change the xml reference inside it
                    'Dim RunBatPath As String = Setup.getDirFromPath(BatchFilePath) & "\run_" & Run.GetName & ".bat"
                    'If Not System.IO.File.Exists(BatchFilePath) Then Throw New Exception("Fout: batchfile niet gevonden: " & RunBatPath)
                    'File.Copy(BatchFilePath, RunBatPath, True)
                    'Setup.GeneralFunctions.ReplaceStringInFile(RunBatPath, "DIMR_Config.xml", RunConfigFilename)
                    'Run.SetRunBatPath(RunBatPath)
                    'Console.WriteLine("Kopie van de batchfile aangemaakt voor simulatie " & Run.GetName & ": " & RunBatPath)

                    ''now replace the old MDU filename with the new one inside our newly created DIMR_config
                    'Dim MDUFile As String = DIMR.DIMRConfig.Flow1D.GetInputFile()                       'original file
                    'Dim MDUFilePath As String = DIMR.DIMRConfig.Flow1D.GetFullDir & "\" & MDUFile       'original file, full path
                    'Dim RunMDUFile As String = Run.GetName & ".MDU"                                     'file for this run
                    'Dim RunMDUFilePath As String = DIMR.DIMRConfig.Flow1D.GetFullDir & "\" & RunMDUFile  'file for this run, full path

                    'Run.SetMDUFileName(RunMDUFile)
                    'File.Copy(MDUFilePath, RunMDUFilePath, True)
                    'If Not System.IO.File.Exists(BatchFilePath) Then Throw New Exception("Fout: MDU-file niet gevonden: " & MDUFilePath)
                    'Setup.GeneralFunctions.ReplaceStringInFile(RunConfigPath, MDUFile, RunMDUFile)
                    'Console.WriteLine("Kopie van de MDU-file aangemaakt voor simulatie " & Run.GetName & ": " & RunMDUFile)

                    'Dim EXTFile As String = Setup.GeneralFunctions.ReadAttributeFromMDUFile(RunMDUFilePath, "ExtForceFileNew")
                    'Dim EXTFilePath As String = DIMR.DIMRConfig.Flow1D.GetFullDir & "\" & EXTFile
                    'Dim RunEXTFile As String = Run.GetName & ".EXT"
                    'Dim RunEXTFilePath As String = DIMR.DIMRConfig.Flow1D.GetFullDir & "\" & RunEXTFile

                    'Run.SetEXTFileName(RunEXTFile)
                    'File.Copy(EXTFilePath, RunEXTFilePath, True)
                    'Setup.GeneralFunctions.ReplaceAttributeValueInDHydroFile(RunMDUFilePath, EXTFile, RunEXTFile, "[external forcing]")
                    'Console.WriteLine("Kopie van de EXT-file aangemaakt voor simulatie " & Run.GetName & ": " & RunEXTFile)



                    ''time to read and adjust our copied .MDU. We must change all references to:
                    ''- the output dir
                    ''- the structures.ini (if altered for this run)
                    ''- the boundaryconditions.bc (if altered for this run)
                    ''- the initalizationfile (if altered for this run)
                    ''- etc

                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    'for each file that must be altered or replaced in this run we must add it to the list of input files
                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    Console.WriteLine("Lijst samenstellen van alle aan te passen invoerbestanden voor simulatie " & Run.GetName & "...")
                    Console.WriteLine("")
                    For Each Scenario As clsDIMRScenario In Run.Scenarios.Values
                        For Each Operation As clsDIMRFileOperation In Scenario.Operations
                            If Not Run.InputFiles.ContainsKey(Operation.getFileName.Trim.ToUpper) Then
                                Run.InputFiles.Add(Operation.getFileName.Trim.ToUpper, Operation.getFileName)
                            End If
                        Next
                    Next
                    For Each Operation As clsDIMRFileOperation In Run.Operations
                        If Not Run.InputFiles.ContainsKey(Operation.getFileName.Trim.ToUpper) Then
                            Run.InputFiles.Add(Operation.getFileName.Trim.ToUpper, Operation.getFileName)
                        End If
                    Next


                    'For Each Operation As clsDIMRFileOperation In Run.Operations
                    '    If Not Run.InputFiles.ContainsKey(Operation.getFileName.Trim.ToUpper) Then
                    '        'copy this file and give it a new name by adding the Run's name to it
                    '        Dim OldName As String = Operation.getFileName
                    '        Dim NewName As String = Setup.GeneralFunctions.getBaseFromFilename(Operation.getFileName) & "_" & Run.GetName & "." & Setup.GeneralFunctions.getExtensionFromFileName(Operation.getFileName)
                    '        Dim OldPath As String = DIMR.ProjectDir & "\" & Operation.getModuleName & "\" & OldName
                    '        Dim NewPath As String = DIMR.ProjectDir & "\" & Operation.getModuleName & "\" & NewName
                    '        If Not File.Exists(OldPath) Then Throw New Exception("Fout: bestand niet gevonden: " & OldPath)
                    '        File.Copy(OldPath, NewPath, True)
                    '        Run.InputFiles.Add(Operation.getFileName.Trim.ToUpper, NewPath)

                    '        'adjust the reference to our file in the MDU file
                    '        'Setup.GeneralFunctions.ReplaceAttributeValueInDHydroFile(RunMDUFilePath, OldName, NewName)
                    '        If Not ReplaceFileReferenceInDHydroFiles(RunMDUFilePath, RunEXTFilePath, OldName, NewName) Then
                    '            Throw New Exception("Fout: geen verwijzing naar " & OldName & " gevonden in de .MDU-file of .EXT-file.")
                    '        End If
                    '    End If
                    'Next
                    ''--------------------------------------------------------------------------------------------------------------------------------------------


                    ''--------------------------------------------------------------------------------------------------------------------------------------------
                    ''we set the results subdir for our simulation in the MDU file
                    'Setup.GeneralFunctions.ReplaceAttributeInDHydroFile(RunMDUFilePath, "OutputDir", Run.GetResultsSubDir)
                    ''--------------------------------------------------------------------------------------------------------------------------------------------

                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    'now execute all operations for this run
                    'notice that we will use the copied files to make adjustments to!
                    'first execute all operations that take place on the level of each individual scenario
                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    For Each Scenario As clsDIMRScenario In Run.Scenarios.Values
                        For Each Operation As clsDIMRFileOperation In Scenario.Operations
                            If Not ImplementOperation(DIMR, Run, Operation) Then Throw New Exception("Kon bewerking " & Operation.getReplacementAction.ToString & ": " & Operation.getValue & " voor simulatie " & Run.GetName & " niet implementeren in het bronbestand bronbestanden")
                        Next
                    Next

                    For Each Operation As clsDIMRFileOperation In Run.Operations
                        If Not ImplementOperation(DIMR, Run, Operation) Then Throw New Exception("Kon bewerking " & Operation.getReplacementAction.ToString & ": " & Operation.getValue & " voor simulatie " & Run.GetName & " niet implementeren in het bronbestand bronbestanden")
                    Next

                    'Stop

                    'only run this simulation if there are no results present in the output dir
                    Dim OutputDir As String = Run.DIMR.FlowFM.getOutputFullDir
                    If System.IO.Directory.GetFiles(OutputDir).Length = 0 Then
                        Console.WriteLine("")
                        Console.WriteLine("Simulatie " & Run.GetName & " wordt gestart...")
                        Run.Execute()

                        'we only want to keep the files specified in the output section of the Excel configuration (saving disk space)
                        Dim Files As New Collection
                        Setup.GeneralFunctions.CollectAllFilesInDir(Run.DIMR.FlowFM.getOutputFullDir, False, "", Files)
                        For Each path As String In Files
                            Dim i As Integer
                            Dim Keep As Boolean = False
                            For i = 0 To OutputFiles.Count - 1

                                'here we convert the (user friendly *.extension) by the regular expression: .*\.extension
                                Dim Pattern As String = OutputFiles(i)
                                If Left(Pattern, 1) = "*" Then
                                    Pattern = Right(Pattern, Pattern.Length - 1) & "$"
                                End If

                                If Setup.GeneralFunctions.RegExMatch(Pattern, path, True) Then
                                    Keep = True
                                    Exit For
                                End If
                            Next
                            If Not Keep Then File.Delete(path)
                        Next
                    Else
                        Console.WriteLine("Simulation " & Run.GetName & " was skipped since its ouptut directory " & OutputDir & " already contains results")
                    End If
                Next

                Stop

            End If
        Catch ex As Exception
            Console.WriteLine("Error executing simulations: " & ex.Message)
        End Try

    End Sub


    Public Function ReplaceFileReferenceInDHydroFiles(RunMDUFilePath As String, RunEXTFilePath As String, OldName As String, NewName As String) As Boolean
        Try
            'adjust the reference to our file in the MDU file
            If Setup.GeneralFunctions.ReplaceAttributeValueInDHydroFile(RunMDUFilePath, OldName, NewName) Then
                Console.WriteLine("Verwijzing naar " & OldName & " in de .MDU file vervangen door " & NewName)
                Return True
            ElseIf Setup.GeneralFunctions.ReplaceAttributeValueInDHydroFile(RunEXTFilePath, OldName, NewName) Then
                Console.WriteLine("Verwijzing naar " & OldName & " in de .EXT file vervangen door " & NewName)
                Return True
            End If
            Throw New Exception("")
        Catch ex As Exception
            Console.WriteLine("Fout: kon verwijzing naar bestand " & OldName & " niet vinden in de .MDU of .EXT file " & ex.Message)
            Return False
        End Try

    End Function


    Public Function ImplementOperation(ByRef DIMR As clsDIMR, ByRef Run As clsDIMRRun, Operation As clsDIMRFileOperation) As Boolean
        Try
            Dim TargetPath As String = DIMR.ProjectDir & "\" & Run.GetName & "\" & Operation.getModuleName & "\" & Run.InputFiles.Item(Operation.getFileName.Trim.ToUpper)
            If Not File.Exists(TargetPath) Then Throw New Exception("het doelbestand voor een bewerking t.b.v. simulatie " & Run.GetName & " bestaat niet: " & TargetPath)

            If Operation.GetAction = GeneralFunctions.enmDIMRIniFileReplacementOperation.bestand Then

                'this action overwrites our entire input file with the given file
                'the target file is the file we have previously copied and renamed so it contains the current Run Name
                Dim SourcePath As String = DIMR.ProjectDir & "\" & Operation.getValue
                If Not File.Exists(SourcePath) Then Throw New Exception("bronbestand voor een bewerking van het type " & Operation.getReplacementAction.ToString & " t.b.v. simulatie " & Run.GetName & " bestaat niet: " & SourcePath)

                Console.WriteLine("Bronbestand vervangen t.b.v. simulatie " & Run.GetName & ": " & TargetPath & " door " & SourcePath)
                Console.WriteLine("")
                File.Copy(SourcePath, TargetPath, True)

            ElseIf Operation.GetAction = enmDIMRIniFileReplacementOperation.sectie Then
                Dim SourcePath As String = DIMR.ProjectDir & "\" & Operation.getValue
                If Not System.IO.File.Exists(SourcePath) Then Throw New Exception("bronbestand voor een bewerking van het type " & Operation.getReplacementAction.ToString & " t.b.v. simulatie " & Run.GetName & " bestaat niet:  " & SourcePath)

                'the source contains a snippet. Read it
                Dim Content As New List(Of String)
                Using snippetReader As New StreamReader(SourcePath)
                    While Not snippetReader.EndOfStream
                        Content.Add(snippetReader.ReadLine.Trim)
                    End While
                End Using

                'check the validity of our snippet
                If Left(Content(0), 1) <> "[" Then Throw New Exception("Ongeldig formaat van bestand: " & SourcePath & ". Er wordt een sectie verwacht, beginnend met een kop omgeven door [ en ]")

                'now replace the section with the same header in our target file
                Console.WriteLine("Sectie in bronbestand vervangen t.b.v. simulatie " & Run.GetName & ": " & Operation.getFileName & " met kop " & Operation.getHeader & " en " & Operation.getIdentifierAttributeName & " = " & Operation.getIdentifierAttributeValue & " vervangen door " & Operation.getValue)
                Console.WriteLine("")
                If Not Setup.GeneralFunctions.ReplaceSectionInDHydroFile(TargetPath, Operation.getHeader, Operation.getIdentifierAttributeName, Operation.getIdentifierAttributeValue, Content) Then
                    Throw New Exception("kon sectie " & Operation.getHeader & " met " & Operation.getIdentifierAttributeName & " = " & Operation.getIdentifierAttributeValue & " t.b.v. voor simulatie " & Run.GetName & " niet vervangen in het doelbestand: " & TargetPath)
                End If

            ElseIf Operation.GetAction = enmDIMRIniFileReplacementOperation.waarde Then

                Dim Value As String = Operation.getValue
                Console.WriteLine("Waarde in bronbestand vervangen t.b.v. simulatie " & Run.GetName & ": " & Operation.getFileName & " met kop " & Operation.getHeader & " en " & Operation.getIdentifierAttributeName & " = " & Operation.getIdentifierAttributeValue & " vervangen door " & Operation.getValue)
                Console.WriteLine("")
                If Not Setup.GeneralFunctions.ReplaceAttributeInDHydroFile(TargetPath, Operation.getHeader, Operation.getIdentifierAttributeName, Operation.getIdentifierAttributeValue, Operation.getAttributeName, Operation.getValue) Then
                    Throw New Exception("kon attribuutwaarde voor " & Operation.getIdentifierAttributeName & " t.b.v. simulatie " & Run.GetName & " niet vervangen in het doelbestand: " & TargetPath)
                End If

            End If
            Return True
        Catch ex As Exception
            Setup.Log.AddError("Fout bij het implementeren van de bewerking: " & ex.Message)
            Return False
        End Try
    End Function


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
                                Console.WriteLine("Leest configuratie voor scenario " & Scenario.Name & "...")
                            ElseIf ScenarioHeaders.Item(c) = "vervangen" Then
                                Operation = New clsDIMRFileOperation(Setup)              'create a new operation
                                Operation.SetAction(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "module" Then
                                Operation.setModuleName(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "bestand" Then
                                Operation.setFileName(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "sectiekop" Then
                                Operation.setHeader(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "selectie-attribuut" Then
                                Operation.setIdentifierAttributeName(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "selectie-waarde" Then
                                Operation.setIdentifierAttributeValue(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "vervangingsattribuut" Then
                                Operation.setAttributeName(worksheet.Cells(r, c).Value)
                            ElseIf ScenarioHeaders.Item(c) = "vervangingswaarde" Then
                                Operation.setValue(worksheet.Cells(r, c).Value)
                                'only add this operation if the filename has been specified
                                If Operation.getFileName IsNot Nothing AndAlso Operation.getFileName <> "" Then
                                    Scenario.AddOperation(Operation)                    'finalize this operation by adding it to the collection
                                End If
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

                        If Runs.Runs.Count > 0 Then
                            'write the previous run to the console
                            Console.WriteLine("Configuratie gelezen voor simulatie " & Runs.Runs.Values(Runs.Runs.Count - 1).GetName)
                        End If

                        Run = New clsDIMRRun()
                        For c = 0 To SimulationHeaders.Count - 1
                            If SimulationHeaders.Item(c) = "scenario" Then
                                Scenario = GetScenario(worksheet.Cells(r, c).Value) 'get the corresponding scenario 
                                Run.AddScenario(Scenario)
                            ElseIf SimulationHeaders.Item(c) = "vervangen" Then
                                Operation = New clsDIMRFileOperation(Setup)              'create a new operation
                                Operation.SetAction(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "module" Then
                                Operation.setModuleName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "bestand" Then
                                Operation.setFileName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "sectiekop" Then
                                Operation.setHeader(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "selectie-attribuut" Then
                                Operation.setIdentifierAttributeName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "selectie-waarde" Then
                                Operation.setIdentifierAttributeValue(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "vervangingsattribuut" Then
                                Operation.setAttributeName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c) = "vervangingswaarde" Then
                                Operation.setValue(worksheet.Cells(r, c).Value)
                                If Operation.getFileName IsNot Nothing AndAlso Operation.getFileName <> "" Then
                                    Run.AddOperation(Operation)                    'finalize this operation by adding it to the collection
                                End If
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
