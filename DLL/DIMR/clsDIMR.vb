Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative


Public Class clsDIMR
    Private Setup As clsSetup
    Public DIMRConfig As clsDIMRConfigFile

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
        FlowFM = New clsFlowFMComponent(Me.Setup, Me)
        RR = New clsRRComponent(Me.Setup, Me)
        RTC = New clsRTCComponent(Me.Setup, Me)
    End Sub

    Public Function SetProject(myProjectDir As String) As Boolean
        Try
            ProjectDir = myProjectDir
            DIMRConfig = New clsDIMRConfigFile(Me.Setup, Me)
            FlowFM = New clsFlowFMComponent(Me.Setup, Me)
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
            If Not FlowFM.ReadNetwork() Then Throw New Exception("Error reading FlowFM network.")             'read the network file
            If Not FlowFM.ReadObservationPoints() Then Throw New Exception("Error reading FlowFM Observation Points.")  'read all observation points in the model

            Return True
        Catch ex As Exception
            Return False
            Me.Setup.Log.AddError("Error reading DIMR Project: " & ex.Message)
        End Try
    End Function


    Public Function CloneCaseForCommandLineRun(TempWorkDir As String, StartDate As Date, EndDate As Date) As Boolean
        'in this function we clone our entire DIMR project/case so it can be run from the command line
        Try
            'first we'll copy all directory content to the designated folder
            Me.Setup.GeneralFunctions.CopyDirectoryContent(ProjectDir, TempWorkDir, True)

            'now we must reset our projectdir to the directory our project was cloned to and read the project config
            'so that we know which module are used and where they are located
            ProjectDir = TempWorkDir
            readConfiguration()

            FlowFM.WriteSimulationPeriod(StartDate, EndDate)
            RR.WriteSimulationPeriod(StartDate, EndDate)
            RTC.WriteSimulationPeriod(StartDate, EndDate)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



End Class
