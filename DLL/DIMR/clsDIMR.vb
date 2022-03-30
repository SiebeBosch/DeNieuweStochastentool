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
            FlowFM.ReadNetwork()
            FlowFM.ReadObservationpoints1D()
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

    Public Function getTMaxForXYLocation(X As Double, Y As Double, MaxSnappingDistance As Double, AddShiftSeconds As Integer, ByRef TimeMaxSecondsToReference As Integer, ByRef DateTimeMax As DateTime) As Boolean
        'this function retrieves the timestep at which the maximum waterlevel occurs near a given XY-location
        'it does so by finding the snappingpoint to the nearest branch
        'and then moving upstream until the first observationpoint is encountered
        'the simulation results for that observation point are then read from the _his.nc file
        'and the timestep where the maximum occurred is recieved
        'IMPORTANT: the function returns TMax as the number of seconds w.r.t. the RefDate as set in the .MDU file!!!
        Try
            Dim Results As Double() = Nothing
            Dim Times As Double() = Nothing             'expressed in seconds w.r.t. reference date as set in .MDU
            Dim TsMaxIdx As Integer = -1
            Dim SnapBranch As cls1DBranch = Nothing
            Dim SnapChainage As Double
            Dim SnapDistance As Double
            Dim MeshNode As cls1DMeshNode = Nothing
            Dim ObservationPoint As cls1DBranchObject = Nothing

            Dim Found As Boolean = False
            Dim ExcludeBranches As New List(Of String)
            While Not Found
                'start searching for a snap location, walk upstream and search for the nearest observation point
                'if not found, add the snapping reach to the ExcludeBranches List and try again
                If Not FlowFM.Network.FindSnapLocation(X, Y, MaxSnappingDistance, SnapBranch, SnapChainage, SnapDistance, ExcludeBranches) Then Exit While
                If FlowFM.GetFirstUpstreamObservationpoint(SnapBranch.ID, SnapChainage, ObservationPoint) Then
                    FlowFM.GetWaterlevelsForObservationpoint1D(ObservationPoint.ID, Results, Times)
                    Found = True
                Else
                    ExcludeBranches.Add(SnapBranch.ID)
                End If
            End While


            'get the start- and endtime of our simulation
            Dim ReferenceDate As DateTime
            Dim StartDate As DateTime
            Dim EndDate As DateTime
            FlowFM.GetSimulationPeriod(ReferenceDate, StartDate, EndDate)

            TsMaxIdx = Setup.GeneralFunctions.MaxIdxFromArrayOfDouble(Results)      'de tijdstapindex voor de hoogste waterstand bij het doorbraakpunt!
            TimeMaxSecondsToReference = Times(TsMaxIdx) + AddShiftSeconds              'tijdsmoment hoogste waterstand, uitgedrukt in seconden t.o.v. reference date
            DateTimeMax = ReferenceDate.AddSeconds(TimeMaxSecondsToReference)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getTMaxForXYLocation of class clsDIMR: " & ex.Message)
            Return False
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
