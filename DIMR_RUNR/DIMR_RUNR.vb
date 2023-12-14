
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
Imports System.Reflection

Imports System.Threading

Module DIMR_RUNR

    Dim Setup As clsSetup
    Dim ScenarioClasses As New Dictionary(Of String, clsDIMRScenarios)
    Dim Runs As New clsDIMRRuns
    Dim OutputFiles As New List(Of String)  'a list of output files we'll copy to a designated folder after each successful simulation

    Sub Main(args As String())

        Try
            Setup = New clsSetup
            Dim DimrConfigPath As String = ""
            Dim ExcelConfigPath As String = ""
            Dim BatchFilePath As String = ""
            Dim GemboxLicensePath As String = ""
            Dim MaxThreads As Integer = 1
            Dim GemboxKey As String = ""
            Dim Thread1 As System.Threading.Thread = Nothing
            Dim Thread2 As System.Threading.Thread = Nothing
            Dim Thread3 As System.Threading.Thread = Nothing
            Dim Thread4 As System.Threading.Thread = Nothing
            Dim execAssembly As Assembly = Assembly.GetCallingAssembly()
            Dim name As AssemblyName = execAssembly.GetName()

            Console.WriteLine("Welcome to DIMR_RUNR!")
            'Console.WriteLine(String.Format("{0}{1} {2:0}.{3:0} for .Net ({4}){0}", Environment.NewLine, name.Name, name.Version.Major.ToString(), name.Version.Minor.ToString(), execAssembly.ImageRuntimeVersion))
            Console.WriteLine(String.Format("{0}{1} {2:0}.{3:0} for .Net ({4}){0}", Environment.NewLine, name.Name, name.Version.Major.ToString(), name.Version.Minor.ToString(), execAssembly.ImageRuntimeVersion))
            Console.WriteLine()
            Console.WriteLine("This program controls simulations with the Deltares Integrated Model Runner (DIMR).")
            Console.WriteLine("Configuration of the required simulations must be done in an een Excel-document.")
            Console.WriteLine("See the examle in the DIMR_Sample folder.")

            If args.Length = 0 Then
                Console.WriteLine("Enter the path to DIMR_Config.xml")
                DimrConfigPath = Console.ReadLine

                Console.WriteLine("Enter the path to the Excel-configuration file")
                ExcelConfigPath = Console.ReadLine

                Console.WriteLine("Enter the path to the batchfile")
                BatchFilePath = Console.ReadLine

                Console.WriteLine("Enter the maximum number of simultaneous simulations")
                MaxThreads = Setup.GeneralFunctions.ForceNumeric(Console.ReadLine, "maxThreads", 1)

                Console.WriteLine("Enter the path to the text file containing your Gembox Spreadsheets licentie")
                GemboxLicensePath = Console.ReadLine

            ElseIf args.Length = 5 Then

                DimrConfigPath = args(0)
                ExcelConfigPath = args(1)
                BatchFilePath = args(2)
                MaxThreads = args(3)
                GemboxLicensePath = args(4)

                Console.WriteLine("Path to the DIMR-configuratie: " & DimrConfigPath)
                Console.WriteLine("Path to the de Excel-configuratie: " & ExcelConfigPath)
                Console.WriteLine("Path to the batchfile: " & BatchFilePath)
                Console.WriteLine("Maximum number of simultane berekeningen: " & MaxThreads)
                Console.WriteLine("Path to the Gembox Spreadsheets license: " & GemboxLicensePath)
            Else
                Throw New Exception("Invalid number of arguments. Required are: path to dimr_config, path to Excel-configuration, path to batchfile, number of simultaneous simulations and path to Gembox Spreadsheets license.")
            End If

            If Not System.IO.File.Exists(DimrConfigPath) Then Throw New Exception("Critical error: specified DIMR-configuration file does not exist: " & DimrConfigPath)
            If Not System.IO.File.Exists(ExcelConfigPath) Then Throw New Exception("Critical error: specified Excel-configuration file does not exist: " & ExcelConfigPath)
            If Not System.IO.File.Exists(BatchFilePath) Then Throw New Exception("Critical error: specified batchfile does not exist: " & BatchFilePath)
            If Not System.IO.File.Exists(GemboxLicensePath) Then Throw New Exception("Critical error: secified path to licentie voor Gembox License does not exist: " & GemboxLicensePath)

            Using licReader As New StreamReader(GemboxLicensePath)
                GemboxKey = licReader.ReadToEnd
            End Using
            SpreadsheetInfo.SetLicense(GemboxKey)

            Dim ModelDir As String = Path.GetDirectoryName(DimrConfigPath)
            Console.WriteLine("Model directory as derived from location of DIMR-configuration file: " & ModelDir)

            'first we will read our DIMR configuration
            Dim DIMR As New clsDIMR(Setup, Setup.GeneralFunctions.GetDirFromPath(DimrConfigPath))
            DIMR.readConfiguration()
            Console.WriteLine("DIMR-configuration file successfully read.")

            'next, read our Excel-file containg information about the required simulations and their input data
            If Not ReadExcelConfiguration(ExcelConfigPath) Then Throw New Exception("Critical error: reading the Excel-configuration file not successful.")
            Console.WriteLine("Excel-configuration file successfully read.")

            'now execute the simulations one by one
            For Each Run As clsDIMRRun In Runs.Runs.Values

                'only execute this simulation if there are no results present in the output dir
                Dim OutputDir As String = ModelDir & "\" & Run.GetName & "\" & DIMR.FlowFM.getSubDirectory & "\" & DIMR.FlowFM.getOutputSubDir  '  Run.DIMR.FlowFM.getOutputFullDir
                Console.WriteLine("Outputdir has been set to " & OutputDir)
                If Not Directory.Exists(OutputDir) OrElse Directory.GetFiles(OutputDir).Length = 0 Then
                    Console.WriteLine("")
                    Console.WriteLine("Simulatie " & Run.GetName & " wordt voorbereid...")

                    Dim RunDir As String = DIMR.ProjectDir & "\" & Run.GetName
                    If Not Directory.Exists(RunDir) Then Directory.CreateDirectory(RunDir)
                    Console.WriteLine("Creating directory for simulation: " & RunDir & "...")

                    Dim RunDIMR As clsDIMR
                    Console.WriteLine("Copying model schematization, except for results dir...")
                    RunDIMR = DIMR.CloneAndAdjustCaseForCommandLineRun(RunDir)

                    'finally assign the newly created DIMR instance to our run and read the DIMR configuration
                    Run.SetDIMRProject(RunDIMR)
                    Run.DIMR.readConfiguration()

                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    'for each file that must be altered or replaced in this run we must add it to the list of input files
                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    Console.WriteLine("Populating list of all input files that need adjustment before simulating " & Run.GetName & "...")
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
                    '--------------------------------------------------------------------------------------------------------------------------------------------


                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    'now execute all operations for this run
                    'notice that we will use the copied files to make adjustments to!
                    'first execute all operations that take place on the level of each individual scenario
                    '--------------------------------------------------------------------------------------------------------------------------------------------
                    For Each Scenario As clsDIMRScenario In Run.Scenarios.Values
                        For Each Operation As clsDIMRFileOperation In Scenario.Operations
                            If Not ImplementOperation(DIMR, Run, Operation) Then Throw New Exception("Unable to implement action " & Operation.getReplacementAction.ToString & ": " & Operation.getValue & " for simulation " & Run.GetName & " in the source file")
                        Next
                    Next

                    For Each Operation As clsDIMRFileOperation In Run.Operations
                        If Not ImplementOperation(DIMR, Run, Operation) Then Throw New Exception("Unable to implement operation " & Operation.getReplacementAction.ToString & ": " & Operation.getValue & " for simulatie " & Run.GetName & " in the source file")
                    Next
                    '--------------------------------------------------------------------------------------------------------------------------------------------


                    '--------------------------------------------------------------------------------------------------------------------------------------------

                    'wait for a thread to become available for this run!
                    Dim ThreadFound As Boolean = False
                    While Not ThreadFound
                        'wait for any of our threads to become available
                        If Thread1 Is Nothing OrElse Thread1.ThreadState = ThreadState.Stopped Then
                            Console.WriteLine("Simulation " & Run.GetName() & " starting on thread 1...")
                            Thread1 = New Thread(AddressOf Run.ExecuteAndRemoveUnNecessaryOutputFiles)
                            Thread1.Start()
                            ThreadFound = True
                        ElseIf MaxThreads > 1 AndAlso (Thread2 Is Nothing OrElse Thread2.ThreadState = ThreadState.Stopped) Then
                            Console.WriteLine("Simulation " & Run.GetName() & " starting on thread 2...")
                            Thread2 = New Thread(AddressOf Run.ExecuteAndRemoveUnNecessaryOutputFiles)
                            Thread2.Start()
                            ThreadFound = True
                        ElseIf MaxThreads > 2 AndAlso (Thread3 Is Nothing OrElse Thread3.ThreadState = ThreadState.Stopped) Then
                            Console.WriteLine("Simulation " & Run.GetName() & " starting on thread 3...")
                            Thread3 = New Thread(AddressOf Run.ExecuteAndRemoveUnNecessaryOutputFiles)
                            Thread3.Start()
                            ThreadFound = True
                        ElseIf MaxThreads > 3 AndAlso (Thread4 Is Nothing OrElse Thread4.ThreadState = ThreadState.Stopped) Then
                            Console.WriteLine("Simulation " & Run.GetName() & " starting on thread 4...")
                            Thread4 = New Thread(AddressOf Run.ExecuteAndRemoveUnNecessaryOutputFiles)
                            Thread4.Start()
                            ThreadFound = True
                        Else
                            Setup.GeneralFunctions.Wait(10000)   'wait 10 seconds before checking again
                        End If
                    End While

                Else
                    Console.WriteLine("Simulation " & Run.GetName & " was skipped since the output folder " & OutputDir & " already contains results")
                End If
                '--------------------------------------------------------------------------------------------------------------------------------------------
            Next

        Catch ex As Exception
            Console.WriteLine("Error executing simulation: " & ex.Message)
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
                Dim value As String = DIMR.ProjectDir & "\" & Operation.getValue

                'this is a simple file copy operation
                If Not File.Exists(value) Then Throw New Exception("bronbestand voor een bewerking van het type " & Operation.getReplacementAction.ToString & " t.b.v. simulatie " & Run.GetName & " bestaat niet: " & value)

                Console.WriteLine("Bronbestand vervangen t.b.v. simulatie " & Run.GetName & ": " & TargetPath & " door " & value)
                Console.WriteLine("")
                File.Copy(value, TargetPath, True)


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

                'hier deteceteren we of de waarde middels een functie moet worden opgevraagd of dat het rechtstreeks kan
                If IsNumeric(Value) Then
                    If Not Setup.GeneralFunctions.ReplaceAttributeInDHydroFile(TargetPath, Operation.getHeader, Operation.getIdentifierAttributeName, Operation.getIdentifierAttributeValue, Operation.getAttributeName, Operation.getValue) Then
                        Throw New Exception("kon attribuutwaarde voor " & Operation.getIdentifierAttributeName & " t.b.v. simulatie " & Run.GetName & " niet vervangen in het doelbestand: " & TargetPath)
                    End If
                ElseIf Strings.Left(Value, 4) = "DIMR" Then

                    'we are dealing with more complex matters. It might be a function call inside the DIMR class

                    Dim FunctionName As String = ""
                    Dim DIMRConfigPath As String = ""
                    Dim Args As New List(Of String)
                    ParseDIMRFunctionAndArgumentsFromString(Value, DIMRConfigPath, FunctionName, Args)

                    If FunctionName.Trim.ToUpper = "GETTMAXFORXYLOCATION" Then

                        Console.WriteLine("Bijbehorende TMax zoeken voor opgegeven breslocatie.")
                        Console.WriteLine("")

                        'parse the arguments required for this function
                        Dim X As Double = Convert.ToDouble(Args(0))
                        Dim Y As Double = Convert.ToDouble(Args(1))
                        Dim MaxSearchRadius As Double = Convert.ToDouble(Args(2))
                        Dim AddShiftSeconds As Double = Convert.ToDouble(Args(3))

                        'and execute the function 
                        Dim TMaxRef As Integer
                        Dim DateMaxRef As DateTime

                        'for this we will first read our reference DIMR configuration
                        Dim refDIMR As New clsDIMR(Setup, Setup.GeneralFunctions.GetDirFromPath(DIMRConfigPath))
                        refDIMR.readConfiguration()

                        'v2.5.0: finally implemented the AddShiftSeconds!
                        refDIMR.getTMaxFrom1DFor2DXYLocation(X, Y, MaxSearchRadius, AddShiftSeconds, TMaxRef, DateMaxRef)

                        'now we must translate our date-time of the maximum waterlevel back to the corresponding reference time in our model-to run
                        Dim RefDate As DateTime
                        Dim StartDate As DateTime
                        Dim EndDate As DateTime
                        DIMR.FlowFM.GetSimulationPeriod(RefDate, StartDate, EndDate)
                        Dim T0 As Integer = DateMaxRef.Subtract(RefDate).TotalSeconds

                        Run.BreachDateTime = DateMaxRef     'we store the date we found as a property in our run so we won't have to recompute in other operations

                        'finally write this value to the appropriate chapter in the appropriate file
                        If Not Setup.GeneralFunctions.ReplaceAttributeInDHydroFile(TargetPath, Operation.getHeader, Operation.getIdentifierAttributeName, Operation.getIdentifierAttributeValue, Operation.getAttributeName, T0) Then
                            Throw New Exception("kon attribuutwaarde voor " & Operation.getIdentifierAttributeName & " t.b.v. simulatie " & Run.GetName & " niet vervangen in het doelbestand: " & TargetPath)
                        End If

                    ElseIf FunctionName.Trim.ToUpper = "FINDBESTMATCHINGRESTARTFILE" Then

                        'Console.WriteLine("Zoekt de eerste restartfile voorafgaand aan het bresmoment ...")
                        'Console.WriteLine("")

                        'Dim HoursPreceeding As Double = Convert.ToDouble(Args(0))

                        ''check if we already have TBreach at our disposal. If not, recompute
                        'If Run.BreachDateTime = Nothing Then
                        'End If


                        ''get the start- and endtime of our simulation
                        'Dim RstFilePath As String = ""
                        'Dim rstFilePathRelative As String = ""
                        'Dim ReferenceDate As DateTime
                        'Dim StartDate As DateTime
                        'Dim EndDate As DateTime
                        'If Not DIMR.FlowFM.GetSimulationPeriod(ReferenceDate, StartDate, EndDate) Then Throw New Exception("Kon simulatieperiode niet bepalen voor simulatie " & Run.GetName)

                        'Dim refDIMR As New clsDIMR(Setup, Setup.GeneralFunctions.GetDirFromPath(DIMRConfigPath))
                        'If Not refDIMR.FindBestMatchingRestartfile(StartDate, RstFilePath) Then Throw New Exception("Error finding matching restart file for simulation " & Run.GetName)

                        ''now that we have a restart file we  must make it a relative path w.r.t. the flow directory of our simulation and write it to our input file
                        'If Not Setup.GeneralFunctions.AbsoluteToRelativePath(Run.DIMR.FlowFM.getDirectory, RstFilePath, rstFilePathRelative) Then Throw New Exception("Kon restart-file niet vinden bij simulatie " & Run.GetName)
                        'If Not Setup.GeneralFunctions.ReplaceAttributeInDHydroFile(TargetPath, Operation.getHeader, Operation.getIdentifierAttributeName, Operation.getIdentifierAttributeValue, Operation.getAttributeName, rstFilePathRelative) Then
                        '        Throw New Exception("kon attribuutwaarde voor " & Operation.getIdentifierAttributeName & " t.b.v. simulatie " & Run.GetName & " niet vervangen in het doelbestand: " & TargetPath)
                        '    End If
                        'End If
                    End If
                Else
                    'replacement value is not numeric
                    If Not Setup.GeneralFunctions.ReplaceAttributeInDHydroFile(TargetPath, Operation.getHeader, Operation.getIdentifierAttributeName, Operation.getIdentifierAttributeValue, Operation.getAttributeName, Operation.getValue) Then
                        Throw New Exception("kon attribuutwaarde voor " & Operation.getIdentifierAttributeName & " t.b.v. simulatie " & Run.GetName & " niet vervangen in het doelbestand: " & TargetPath)
                    End If

                End If
            End If
            Return True
        Catch ex As Exception
            Setup.Log.AddError("Fout bij het implementeren van de bewerking: " & ex.Message)
            Console.WriteLine("Fout bij het implementeren van de bewerking: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ParseDIMRFunctionAndArgumentsFromString(Value As String, ByRef DIMRConfigPath As String, ByRef FunctionName As String, ByRef Args As List(Of String)) As Boolean
        'this function parses a DIMR function call as specified in the Excel configuration file
        Try
            'strip down and identify the function, its arguments and the reference case
            Dim myStr As String
            Args = New List(Of String)
            myStr = Setup.GeneralFunctions.ParseString(Value, "(") 'start by stripping 'DIMR' off
            DIMRConfigPath = Setup.GeneralFunctions.ParseString(Value, ")").Replace("""", "")
            myStr = Setup.GeneralFunctions.ParseString(Value, ":")
            FunctionName = Setup.GeneralFunctions.ParseString(Value, "(")
            Value = Strings.Left(Value, Value.Length - 1) 'stripping the ) off too
            While Not Value = ""
                Args.Add(Setup.GeneralFunctions.ParseString(Value, ";"))
            End While
            Return True
        Catch ex As Exception
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
                                If Not Operation.SetAction(worksheet.Cells(r, c).Value) Then
                                    'the scenario has been added, but the operation/action is empty, so we'll just leave it 'as is'
                                End If
                            ElseIf ScenarioHeaders.Item(c) = "subdir" Then
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

                        Run = New clsDIMRRun(Setup)
                        For c = 0 To SimulationHeaders.Count - 1
                            If SimulationHeaders.Item(c).Trim.ToLower = "simulation name" Then
                                'new in v2.4: user can specify his own name for each simulation
                                Run.setName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "scenario" Then
                                Scenario = GetScenario(worksheet.Cells(r, c).Value) 'get the corresponding scenario 
                                If Scenario IsNot Nothing Then Run.AddScenario(Scenario)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "vervangen" Then
                                Operation = New clsDIMRFileOperation(Setup)              'create a new operation
                                If Not Operation.SetAction(worksheet.Cells(r, c).Value) Then
                                    'the simulation has been added, but it has no additional operations involved so we'll just leave it 'as is'
                                End If
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "subdir" Then
                                Operation.setModuleName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "bestand" Then
                                Operation.setFileName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "sectiekop" Then
                                Operation.setHeader(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "selectie-attribuut" Then
                                Operation.setIdentifierAttributeName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "selectie-waarde" Then
                                Operation.setIdentifierAttributeValue(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "vervangingsattribuut" Then
                                Operation.setAttributeName(worksheet.Cells(r, c).Value)
                            ElseIf SimulationHeaders.Item(c).Trim.ToLower = "vervangingswaarde" Then
                                Operation.setValue(worksheet.Cells(r, c).Value)
                                If Operation.getFileName IsNot Nothing AndAlso Operation.getFileName <> "" Then
                                    Run.AddOperation(Operation)                    'finalize this operation by adding it to the collection
                                End If
                            End If
                        Next

                        Dim RunName As String = Run.GetOrCreateName() 'if no name given, create one based on the underlying scenario's
                        If Runs.Runs.ContainsKey(RunName.Trim.ToUpper) Then Throw New Exception("Fout: een simulatie met hetzelfde ID bestaat meerdere keren. Controleer de scenarioklassen en scenario's: " & Run.GetName)
                        Runs.Runs.Add(RunName.Trim.ToUpper, Run)
                        Console.WriteLine("Configuratie gelezen voor simulatie " & RunName)

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

            'assign the required outputfiles to each of the runs. This is necessary since we cannot use arguments when initializing a thread
            For Each Run In Runs.Runs.Values
                Run.SetOutputFiles(OutputFiles)
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
        If ScenarioName Is Nothing Then Return Nothing
        For i = 0 To ScenarioClasses.Values.Count - 1
            If ScenarioClasses.Values(i).Scenarios.ContainsKey(ScenarioName.Trim.ToUpper) Then
                Return ScenarioClasses.Values(i).Scenarios.Item(ScenarioName.Trim.ToUpper)
            End If
        Next
        Return Nothing
    End Function

End Module
