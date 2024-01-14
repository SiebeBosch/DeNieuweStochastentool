
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

    Public Sub New(ByRef mySetup As clsSetup, myProjectDir As String, ReadConfig As Boolean)
        Setup = mySetup
        ProjectDir = myProjectDir
        BasinFile = New clsHBVBasinFile(Me.Setup, ProjectDir & "\basin.par")
        If ReadConfig Then ReadConfiguration()
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myProjectDir As String, ByVal ProgramsDir As String, ReadConfig As Boolean)
        Me.Setup = mySetup
        Me.ProjectDir = myProjectDir
        Me.ProgramsDir = ProgramsDir
        BasinFile = New clsHBVBasinFile(Me.Setup, Path.Combine(ProjectDir, "basin.par"))
        If ReadConfig Then ReadConfiguration()
    End Sub

    Public Function ReadConfiguration() As Boolean
        Try
            BasinFile.Read()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading HBV Project: " & ex.Message)
            Return False
        End Try
    End Function

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


    Public Function CloneAndAdjustCaseForCommandLineRun(SimulationDir As String, Optional ByVal StartDate As Date = Nothing, Optional ByVal EndDate As Date = Nothing) As Boolean
        'in this function we clone our entire HBV project/case so it can be run from the command line
        Try

            'first create a new directory for our simulation
            If Not Directory.Exists(SimulationDir) Then Directory.CreateDirectory(SimulationDir)

            'let's copy the entire project, so all files and subdirectories and their contents to our new directory
            My.Computer.FileSystem.CopyDirectory(ProjectDir, SimulationDir, True)

            'now, for each subbasin we must write an adjusted comp.key file, containing the start and end date of the simulation
            For Each basin As clsHBVSubBasin In BasinFile.Basins.Values
                Dim CompKeyFile As String = Path.Combine(SimulationDir, basin.BasinDir, "comp.key")

                'we'll now writ the comp.key file for this subbasin
                'example file contents:
                'byear'             2011
                'bmonth'               1
                'bday'                 1
                'bhour'                0
                'eyear'             2012
                'emonth'               2
                'eday'                 1
                'ehour'                0
                'timestep'            24
                'outfield'             1
                'qcout     ' 'totmean   '           1           1  'mean      '
                'prec      ' 'totmean   '           2           1  'sum       '

                'not sure if it's necessary but we make sure the length of each line is 26 characters
                Using compWriter As New StreamWriter(CompKeyFile, False)
                    compWriter.WriteLine("'byear'             " & StartDate.Year)
                    compWriter.WriteLine("'bmonth'              " & If(StartDate.Month.ToString.Length = 1, " " & StartDate.Month, StartDate.Month))
                    compWriter.WriteLine("'bday'                " & If(StartDate.Day.ToString.Length = 1, " " & StartDate.Day, StartDate.Day))
                    compWriter.WriteLine("'bhour'               " & If(StartDate.Hour.ToString.Length = 1, " " & StartDate.Hour, StartDate.Hour))
                    compWriter.WriteLine("'eyear'             " & EndDate.Year)
                    compWriter.WriteLine("'emonth'              " & If(EndDate.Month.ToString.Length = 1, " " & EndDate.Month, EndDate.Month))
                    compWriter.WriteLine("'eday'                " & If(EndDate.Day.ToString.Length = 1, " " & EndDate.Day, EndDate.Day))
                    compWriter.WriteLine("'ehour'               " & If(EndDate.Hour.ToString.Length = 1, " " & EndDate.Hour, EndDate.Hour))
                    compWriter.WriteLine("'timestep'            24")
                    compWriter.WriteLine("'outfield'             1")
                    compWriter.WriteLine("'qcout     ' 'totmean   '           1           1  'mean      '")
                    compWriter.WriteLine("'prec      ' 'totmean   '           2           1  'sum       '")
                End Using
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



End Class
