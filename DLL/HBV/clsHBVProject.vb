
Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative


Public Class clsHBVProject
    Private Setup As clsSetup
    Public ProjectDir As String
    Public ProgramsDir As String

    Public BasinFile As clsHBVBasinFile

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myProjectDir As String)
        Setup = mySetup
        ProjectDir = myProjectDir
        BasinFile = New clsHBVBasinFile(Me.Setup, ProjectDir & "\basin.par")
        BasinFile.Read()
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myProjectDir As String, ByVal ProgramsDir As String)
        Me.Setup = mySetup
        Me.ProjectDir = myProjectDir
        Me.ProgramsDir = ProgramsDir
    End Sub

    Public Function SetProject(myProjectDir As String) As Boolean
        Try
            ProjectDir = myProjectDir
            BasinFile = New clsHBVBasinFile(Me.Setup, ProjectDir & "\basin.par")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error creating DIMR Project " + ex.Message)
            Return False
        End Try
    End Function

    Public Function readConfiguration() As Boolean
        Try
            'If Not DIMRConfig.Read() Then Throw New Exception("Error reading DIMR Config File.")
            'FlowFM.ReadMDU()
            'FlowFM.ReadNetwork()
            'FlowFM.ReadObservationpoints1D()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading DIMR Configuration: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadAll() As Boolean
        'reads the entire project
        Try
            'If Not DIMRConfig.Read() Then Throw New Exception("Error reading DIMR Config File.")
            'If Not FlowFM.ReadMDU() Then Throw New Exception("Error reading FlowFM MDU-file.")       'read the MDU file. This contains references to e.g. our _net.nc file we must read
            'If Not FlowFM.ReadNetwork() Then Throw New Exception("Error reading FlowFM Network")
            'If Not FlowFM.ReadObservationpoints1D() Then Throw New Exception("Error reading FlowFM Observation points")
            Return True
        Catch ex As Exception
            Return False
            Me.Setup.Log.AddError("Error reading DIMR Project: " & ex.Message)
        End Try
    End Function



    Public Function CopyCase(SimulationDir As String)
        'in this function we clone our entire DIMR project/case so it can be run from the command line
        'the function returns a complete new instance of a clsDIMR class
        Try
            Dim newDIMR As New clsDIMR(Setup, SimulationDir)

            'first create a new directory for our simulation
            If Not Directory.Exists(SimulationDir) Then Directory.CreateDirectory(SimulationDir)
            My.Computer.FileSystem.CopyDirectory(ProjectDir, SimulationDir, True)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function AssignMeteoStationNumbers() As Boolean
        Try
            'this function assigns meteo station numbers to the meteo stations by reading the original meteo files from the project
            For Each myStation As clsMeteoStation In Me.Setup.StochastenAnalyse.MeteoStations.MeteoStations.Values
                If myStation.StationType = GeneralFunctions.enmMeteoStationType.precipitation Then
                    'first check if a meteo file exists for this station
                    Dim MeteoFile As String = Me.ProjectDir & "\" & myStation.Name & ".txt"
                    If Not File.Exists(MeteoFile) Then Throw New Exception("No precipitation file named " & myStation.Name & ".txt found in project directory. Please make sure the meteo stations specified match the files in the project.")

                    'read the meteo file. The station number is in the second row
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

    Public Function CloneAndAdjustCaseForCommandLineRun(SimulationDir As String, Optional ByVal StartDate As Date = Nothing, Optional ByVal EndDate As Date = Nothing) As Boolean
        'in this function we clone our entire HBV project/case so it can be run from the command line
        Try

            'first create a new directory for our simulation
            If Not Directory.Exists(SimulationDir) Then Directory.CreateDirectory(SimulationDir)

            'let's copy the entire project, so all files and subdirectories and their contents to our new directory
            My.Computer.FileSystem.CopyDirectory(ProjectDir, SimulationDir, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



End Class
