Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative


Public Class clsDIMR
    Private Setup As clsSetup
    Public DIMRConfig As clsDIMRConfigFile      'the dimr_config.xml file
    Public BatchFilePath As String              'path to the batchfile needed to run this configuration

    Public RR As clsRRComponent
    Public FlowFM As clsFlowFMComponent
    Public RTC As clsRTCComponent

    Public ProjectDir As String

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myProjectDir As String)
        Setup = mySetup
        ProjectDir = myProjectDir
        DIMRConfig = New clsDIMRConfigFile(Me.Setup, Me)
        DIMRConfig.Read()
        'FlowFM = New clsFlowFMComponent(Me.Setup, Me)
        'RR = New clsRRComponent(Me.Setup, Me)
        'RTC = New clsRTCComponent(Me.Setup, Me)
    End Sub

    Public Function SetProject(myProjectDir As String) As Boolean
        Try
            ProjectDir = myProjectDir
            DIMRConfig = New clsDIMRConfigFile(Me.Setup, Me)
            'FlowFM = New clsFlowFMComponent(Me.Setup, Me)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error creating DIMR Project " + ex.Message)
            Return False
        End Try
    End Function

    Public Function readConfiguration() As Boolean
        Try
            If Not DIMRConfig.Read() Then Throw New Exception("Error reading DIMR Config File.")
            FlowFM.ReadMDU()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading DIMR Configuration: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadAll() As Boolean
        'reads the entire project
        Try
            If Not DIMRConfig.Read() Then Throw New Exception("Error reading DIMR Config File.")
            If Not FlowFM.ReadMDU() Then Throw New Exception("Error reading FlowFM MDU-file.")       'read the MDU file. This contains references to e.g. our _net.nc file we must read

            Return True
        Catch ex As Exception
            Return False
            Me.Setup.Log.AddError("Error reading DIMR Project: " & ex.Message)
        End Try
    End Function


    Public Function CloneCaseForCommandLineRun(SimulationDir As String, Optional ByVal StartDate As Date = Nothing, Optional ByVal EndDate As Date = Nothing) As clsDIMR
        'in this function we clone our entire DIMR project/case so it can be run from the command line
        'the function returns a complete new instance of a clsDIMR class
        Try
            Dim newDIMR As New clsDIMR(Setup, SimulationDir)

            'first create a new directory for our simulation
            If Not Directory.Exists(SimulationDir) Then Directory.CreateDirectory(SimulationDir)

            'first we'll copy the batchfile and the dimr_config to oru new directory
            Dim Files As New Collection
            Dim FileName As String
            Dim ModuleDir As String
            Setup.GeneralFunctions.CollectAllFilesInDir(ProjectDir, False, "", Files)

            For Each myFile As String In Files
                FileName = Setup.GeneralFunctions.FileNameFromPath(myFile)
                If Setup.GeneralFunctions.getExtensionFromFileName(myFile).Trim.ToLower = "bat" Then
                    newDIMR.BatchFilePath = SimulationDir & "\" & FileName
                    File.Copy(myFile, newDIMR.BatchFilePath, True)
                ElseIf FileName.Trim.ToLower = "dimr_config.xml" Then
                    newDIMR.DIMRConfig = New clsDIMRConfigFile(Setup, newDIMR)
                    File.Copy(myFile, SimulationDir & "\" & FileName, True)
                End If
            Next

            If FlowFM IsNot Nothing Then
                ModuleDir = SimulationDir & "\" & DIMRConfig.Flow1D.SubDir
                If Not Directory.Exists(ModuleDir) Then Directory.CreateDirectory(ModuleDir)
                Dim MDUFile As String = DIMRConfig.Flow1D.GetFullDir & "\" & DIMRConfig.Flow1D.GetInputFile
                Dim AttrVals As List(Of String) = Setup.GeneralFunctions.ReadAttributeValueFromDHydrofile(MDUFile, "OutputDir", "[output]")
                If AttrVals IsNot Nothing Then
                    Dim ExcludeDir As String = DIMRConfig.Flow1D.GetFullDir & "\" & AttrVals(0)
                    Me.Setup.GeneralFunctions.CopyDirectoryContent(DIMRConfig.Flow1D.GetFullDir, ModuleDir, True, ExcludeDir)
                End If

                'optionally change the start and end date for this simulation
                If Not StartDate = Nothing AndAlso Not EndDate = Nothing Then
                    FlowFM.WriteSimulationPeriod(StartDate, EndDate)
                End If
            End If

            If RR IsNot Nothing Then
                ModuleDir = SimulationDir & "\" & DIMRConfig.RR.SubDir
                If Not Directory.Exists(ModuleDir) Then Directory.CreateDirectory(ModuleDir)
                Me.Setup.GeneralFunctions.CopyDirectoryContent(DIMRConfig.RR.GetFullDir, ModuleDir, True)
                If Not StartDate = Nothing AndAlso Not EndDate = Nothing Then
                    RR.WriteSimulationPeriod(StartDate, EndDate)
                End If
            End If

            If RTC IsNot Nothing Then
                ModuleDir = SimulationDir & "\" & DIMRConfig.RTC.SubDir
                If Not Directory.Exists(ModuleDir) Then Directory.CreateDirectory(ModuleDir)
                Me.Setup.GeneralFunctions.CopyDirectoryContent(DIMRConfig.RTC.GetFullDir, ModuleDir, True)
                If Not StartDate = Nothing AndAlso Not EndDate = Nothing Then
                    RTC.WriteSimulationPeriod(StartDate, EndDate)
                End If
            End If

            ''now we must reset our projectdir to the directory our project was cloned to and read the project config
            ''so that we know which module are used and where they are located
            'ProjectDir = SimulationDir
            'readConfiguration()

            Return newDIMR
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function



End Class
