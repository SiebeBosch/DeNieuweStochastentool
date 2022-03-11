Imports System
Imports STOCHLIB
Imports System.Net
Imports Newtonsoft.Json
Imports GemBox
Imports GemBox.Spreadsheet
Imports System.Text
Imports System.IO.Packaging

'let op: Gembox vereist system.io.Packaging! Installeer die ook (via NuGET)

Module DIMRRUNR

    Dim ScenarioClasses As New Dictionary(Of String, clsDIMRScenarios)

    Sub Main(args As String())
        Dim ConfigPath As String

        Dim ScenarioHeaders As New Dictionary(Of Integer, String)
        Dim SimulationHeaders As New Dictionary(Of Integer, String)
        Dim OutputHeaders As New Dictionary(Of Integer, String)

        'Dim ScenarioClasses As New Dictionary(Of String, clsDIMRScenarios)    'use the level/niveau as key
        Dim Scenarios As clsDIMRScenarios
        Dim Scenario As clsDIMRScenario
        Dim Operation As clsDIMRFileOperation = Nothing

        'the simulations themselves
        Dim Runs As New clsDIMRRuns
        Dim Run As clsDIMRRun

        'a list of outputfiles that we need to store somewhere
        Dim OutputFiles As New List(Of String)

        SpreadsheetInfo.SetLicense("EVIG-1Y89-FYME-DPUJ")

        Console.WriteLine("Welkom bij DIMRRUNR!")
        Console.WriteLine("Dit programma stuurt simulaties met de Deltares Integrated Model Runner (DIMR) aan.")
        Console.WriteLine("Het configureren van de gevraagde simulaties gebeurt in een Excel-bestand.")
        If args.Length <> 1 Then
            Console.WriteLine("Ongeldig aantal argumenten meegegeven aan de applicatie. Geef alleen het pad naar het Excel-document.")
        Else
            ConfigPath = args(0)

            If System.IO.File.Exists(ConfigPath) Then

                Dim workbook = ExcelFile.Load(ConfigPath)

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
                            OutputFiles.add(worksheet.Cells(r, 0).Value)
                        End While

                    End If
                Next



                Console.WriteLine("Simulaties op basis van configuratiebestand: " & ConfigPath)

                Stop
            Else
                Console.WriteLine("Error: bestand niet gevonden: " & ConfigPath)
            End If

        End If
    End Sub

    Public Function GetScenario(ScenarioName As String) As clsDIMRScenario
        Dim i As Integer
        For i = 0 To ScenarioClasses.Values.Count - 1
            If ScenarioClasses.Values(i).Scenarios.ContainsKey(ScenarioName.Trim.ToUpper) Then
                Return ScenarioClasses.Values(i).Scenarios.Item(ScenarioName.Trim.ToUpper)
            End If
        Next
        Return Nothing
    End Function

    Friend Class Configuration
        Public Property dimr_config_path As String
        Public Property executable As String
        Public Property simulation_dirs As String()

        Public Property scenarios As Scenario()
    End Class


    Friend Class Scenario
        Public Property Name As String
        Public Property source_files As SourceFile()

        Public Property results_files As ResultsFile()

    End Class

    Friend Class SourceFile
        Public Property filename As String
        Public Property source_file As String

        Public Property target_module As String
    End Class

    Friend Class ResultsFile
        Public Property filename As String
        Public Property hydromodule As String
    End Class

End Module
