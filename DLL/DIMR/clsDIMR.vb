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
            If Not FlowFM.ReadNetwork() Then Throw New Exception("Error reading FlowFM Network")
            If Not FlowFM.ReadObservationpoints1D() Then Throw New Exception("Error reading FlowFM Observation points")
            If Not FlowFM.ReadStructures1D() Then Throw New Exception("Error reading FlowFM Structures")
            Return True
        Catch ex As Exception
            Return False
            Me.Setup.Log.AddError("Error reading DIMR Project: " & ex.Message)
        End Try
    End Function

    Public Function FindBestMatchingRestartfile(TStart As DateTime, ByRef RestartFilePath As String) As Boolean
        'this function retrieves a restart file from these simulation's results, matching a given start time as close as possible
        Try
            'so first thing to do is create a list of all .rst files in our output dir
            Dim RstList As New Collection
            Setup.GeneralFunctions.CollectAllFilesInDir(FlowFM.getOutputFullDir, False, "rst.nc", RstList)
            Dim BestMatch As String = ""
            Dim BestDiff As Long = Long.MaxValue
            Dim CurDiff As Integer
            Dim Found As Boolean = False

            For Each Path As String In RstList

                Dim myDate As New DateTime
                GetDateFromRestartFilename(Path, myDate)

                'calculate the time difference with our TStart
                CurDiff = TStart.Subtract(myDate).TotalSeconds
                If CurDiff > 0 AndAlso CurDiff < BestDiff Then
                    BestMatch = Path
                    BestDiff = CurDiff
                    Found = True
                End If
            Next

            RestartFilePath = BestMatch
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FindBestMatchingRestartfile of class clsDIMR: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function GetDateFromRestartFilename(path As String, ByRef myDate As DateTime) As Boolean
        Try
            'this function retrieves the date from a restart file (_rst.nc)
            'assuming the naming format is: modelname_yyyymmdd_hhmmss_rst.nc
            Dim Filename As String = Me.Setup.GeneralFunctions.FileNameFromPath(path)
            Dim tmpStr As String
            Dim dateStr As String
            Dim timeStr As String
            tmpStr = Me.Setup.GeneralFunctions.ParseString(Filename, "_") 'first is the domain name
            dateStr = Me.Setup.GeneralFunctions.ParseString(Filename, "_")  'this is the date string, formatted yyyymmdd
            timeStr = Me.Setup.GeneralFunctions.ParseString(Filename, "_")  'this is the time string, formatted hhmmss
            myDate = New DateTime(Strings.Left(dateStr, 4), Strings.Mid(dateStr, 5, 2), Strings.Right(dateStr, 2), Strings.Left(timeStr, 2), Strings.Mid(timeStr, 3, 2), Strings.Right(timeStr, 2))
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetDateFromRestartFilename of class clsDIMR: " & ex.Message)
            Return False
        End Try

    End Function


    Public Function getTMaxFrom1DFor2DXYLocation(X As Double, Y As Double, MaxSnappingDistance As Double, AddShiftSeconds As Integer, ByRef TimeMaxSecondsToReference As Integer, ByRef DateTimeMax As DateTime) As Boolean
        'this function retrieves the timestep at which the maximum waterlevel occurs near a given XY-location
        'it does so by finding the snappingpoint to the nearest branch
        'and then moving upstream until the first observationpoint is encountered
        'the simulation results for that observation point are then read from the _his.nc file
        'and the timestep where the maximum occurred is recieved
        'IMPORTANT: the function returns TMax as the number of seconds w.r.t. the RefDate as set in the .MDU file!!!
        Try
            Dim Waterlevels As Double() = Nothing
            Dim Discharges As Double() = Nothing
            Dim Times As Double() = Nothing             'expressed in seconds w.r.t. reference date as set in .MDU
            Dim TsMaxIdx As Integer = -1
            Dim SnapBranch As cls1DBranch = Nothing
            Dim SnapChainage As Double
            Dim SnapDistance As Double
            Dim MeshNode As cls1DMeshNode = Nothing
            Dim ObservationPoint As cls1DBranchObject = Nothing

            'start searching for a snap location, walk upstream and search for the nearest observation point
            If Not FlowFM.Network.Find1DSnapLocationVia1D2DLinks(X, Y, MaxSnappingDistance, SnapBranch, SnapChainage, SnapDistance) Then Throw New Exception("Kan snapping point op 1D netwerk niet vinden vanuit de breslocatie " & X & ", " & Y)
            If Not FlowFM.GetFirstUpstreamObservationpoint(SnapBranch.ID, SnapChainage, ObservationPoint) Then Throw New Exception("Kan bovenstrooms observationpoint niet vinden.")
            FlowFM.GetResultsForObservationpoint1D(ObservationPoint.ID, Waterlevels, Discharges, Times)

            'get the start- and endtime of our simulation
            Dim ReferenceDate As DateTime
            Dim StartDate As DateTime
            Dim EndDate As DateTime
            FlowFM.GetSimulationPeriod(ReferenceDate, StartDate, EndDate)

            TsMaxIdx = Setup.GeneralFunctions.MaxIdxFromArrayOfDouble(Waterlevels)                      'de tijdstapindex voor de hoogste waterstand bij het doorbraakpunt!
            TimeMaxSecondsToReference = Math.Max(0, Times(TsMaxIdx) + AddShiftSeconds)              'tijdsmoment hoogste waterstand, uitgedrukt in seconden t.o.v. reference date
            DateTimeMax = ReferenceDate.AddSeconds(TimeMaxSecondsToReference)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getTMaxForXYLocation of class clsDIMR: " & ex.Message)
            Return False
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

    Public Function CloneAndAdjustCaseForCommandLineRun(SimulationDir As String, Optional ByVal StartDate As Date = Nothing, Optional ByVal EndDate As Date = Nothing) As clsDIMR
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
                ElseIf FileName.Trim.ToLower.Contains("dimr_config") AndAlso FileName.Trim.ToLower.Contains(".xml") Then ' = "dimr_config.xml" Then
                    newDIMR.DIMRConfig = New clsDIMRConfigFile(Setup, newDIMR)

                    'here we read the entire content of the DIMR_Config file in memory. Then we replace the line stating the startdate
                    'Dim path As String = DIMR.ProjectDir & "\" & FileName
                    Dim content As String, lines As String()
                    Using xmlReader As New StreamReader(myFile)
                        content = xmlReader.ReadToEnd()
                    End Using
                    lines = Split(content, vbCrLf)

                    Using xmlWriter As New StreamWriter(SimulationDir & "\" & FileName)
                        For Each myLine In lines
                            If Strings.Left(myLine.Trim, 6).ToLower = "<time>" Then
                                'read the three numbers between the tags
                                myLine = Replace(myLine.Trim, "<time>", "")
                                myLine = Replace(myLine.Trim, "</time>", "")
                                Dim myNumbers As String() = Split(myLine.Trim, " ")
                                'replace the line with the new startdate
                                xmlWriter.WriteLine($"        <time>{myNumbers(0)} {myNumbers(1)} {EndDate.Subtract(StartDate).TotalSeconds}</time>")
                            Else
                                'leave the line untouched and write it
                                xmlWriter.WriteLine(myLine)
                            End If
                        Next
                    End Using

                    'File.Copy(myFile, SimulationDir & "\" & FileName, True)
                End If
            Next


            newDIMR.readConfiguration()



            If FlowFM IsNot Nothing Then
                ModuleDir = SimulationDir & "\" & DIMRConfig.Flow1D.SubDir
                If Not Directory.Exists(ModuleDir) Then Directory.CreateDirectory(ModuleDir)
                Dim MDUFile As String = DIMRConfig.Flow1D.GetFullDir & "\" & DIMRConfig.Flow1D.GetInputFile
                Dim AttrVals As List(Of String) = Setup.GeneralFunctions.ReadAttributeValueFromDHydrofile(MDUFile, "OutputDir", "[output]")
                If AttrVals Is Nothing OrElse AttrVals.Count = 0 Then
                    Throw New Exception("Error reading OutputDir from MDU file.")
                End If
                Dim ExcludeDir As String = DIMRConfig.Flow1D.GetFullDir & "\" & AttrVals(0)
                Me.Setup.GeneralFunctions.CopyDirectoryContent(DIMRConfig.Flow1D.GetFullDir, ModuleDir, True, ExcludeDir)

                'optionally change the start and end date for this simulation
                If Not StartDate = Nothing AndAlso Not EndDate = Nothing Then
                    newDIMR.FlowFM.WriteSimulationPeriod(StartDate, EndDate)
                End If
            End If

            If RR IsNot Nothing Then
                ModuleDir = SimulationDir & "\" & DIMRConfig.RR.SubDir
                If Not Directory.Exists(ModuleDir) Then Directory.CreateDirectory(ModuleDir)
                Me.Setup.GeneralFunctions.CopyDirectoryContent(DIMRConfig.RR.GetFullDir, ModuleDir, True)
                If Not StartDate = Nothing AndAlso Not EndDate = Nothing Then
                    newDIMR.RR.WriteSimulationPeriod(StartDate, EndDate)
                End If
            End If

            If RTC IsNot Nothing Then
                ModuleDir = SimulationDir & "\" & DIMRConfig.RTC.SubDir
                If Not Directory.Exists(ModuleDir) Then Directory.CreateDirectory(ModuleDir)
                Me.Setup.GeneralFunctions.CopyDirectoryContent(DIMRConfig.RTC.GetFullDir, ModuleDir, True)
                If Not StartDate = Nothing AndAlso Not EndDate = Nothing Then
                    newDIMR.RTC.WriteSimulationPeriod(StartDate, EndDate)
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
