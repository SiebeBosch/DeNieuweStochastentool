
Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative


Public Class clsSumaquaProject
    Private Setup As clsSetup
    Public ProjectDir As String
    Public ProjectName As String
    Public ProgramsDir As String

    Public BasinFile As clsHBVBasinFile

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myProjectDir As String, myProjectName As String)
        Setup = mySetup
        ProjectDir = myProjectDir
        ProjectName = myProjectName
    End Sub

    Public Function SetProject(myProjectDir As String, myProjectName As String) As Boolean
        Try
            ProjectDir = myProjectDir
            ProjectName = myProjectName
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function CloneAndAdjustCaseForCommandLineRun(SimulationDir As String, Optional ByVal StartDate As Date = Nothing, Optional ByVal EndDate As Date = Nothing) As Boolean
        'in this function we clone our entire SUMAQUA project/case so it can be run from the command line
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
