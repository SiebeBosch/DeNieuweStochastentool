Imports STOCHLIB.General
Imports System.IO
Public Class clsFlowFMComponent
    Private Setup As clsSetup
    Private DIMR As clsDIMR

    Friend MDUFile As clsMDUFile                                    'all model settings (e.g. filenames)
    Public Network As clsNetworkFile                                'the complete topological network for our model
    Public FouConfig As clsFouConfigFile

    Public Observationpoints1D As New Dictionary(Of String, cls1DBranchObject)
    Public CellCenterpoints2D As New List(Of clsXY)  'key = cell index number

    Dim HisNCFile As clsHisNCFile
    Dim MapNCFile As clsMapNCFile
    Dim FouNCFile As clsFouNCFile

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDIMR As clsDIMR)
        Setup = mySetup
        DIMR = myDIMR
    End Sub



    Public Function ReadNetwork() As Boolean
        Try
            'the path to the network file must be stored in the MDU file so read it
            If MDUFile Is Nothing Then Throw New Exception("Could not read network since the MDU File has not yet been read.")
            Dim filename As String = MDUFile.getAttributeValue("[geometry]", "NetFile")
            Dim path As String = getDirectory() & "\" & filename

            Network = New clsNetworkFile(path, Setup, Me)
            If Not Network.Read() Then Throw New Exception("Error reading FlowFM Network")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadNetwork of class clsFlowFMComponent: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetSimulationPeriod(ByRef ReferenceDate As DateTime, ByRef StartDate As DateTime, ByRef EndDate As DateTime) As Boolean
        Try
            If MDUFile Is Nothing Then ReadMDU()
            Return MDUFile.GetSimulationPeriod(ReferenceDate, StartDate, EndDate)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error Integer Function GetSimulationPeriod of class clsFlowFMComponent: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadCellCentersFromFouFile() As Boolean
        Try
            If MDUFile Is Nothing Then Throw New Exception("Could not read fourier (fou) file since the MDU File has not yet been read.")
            Dim Path As String = getOutputFullDir() & "\" & GetFouResultsFileName()

            If Not System.IO.File.Exists(Path) Then Throw New Exception("Fourier file Not found: " & Path)
            Dim FouFile As New clsFouNCFile(Path, Me.Setup)
            If Not FouFile.Read() Then Throw New Exception("Error reading Fou file: True" & Path)

            For i = 0 To FouFile.Mesh2d_face_x.Count - 1
                CellCenterpoints2D.Add(New clsXY(FouFile.Mesh2d_face_x(i), FouFile.Mesh2d_face_y(i)))
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetCellCentersFromFouFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadObservationpoints1D() As Boolean
        Try
            'the path to the network file must be stored in the MDU file so read it
            If MDUFile Is Nothing Then Throw New Exception("Could not read network since the MDU File has not yet been read.")
            Dim filenames As New List(Of String)
            filenames = MDUFile.getAttributeValues("[output]", "ObsFile")

            For Each FileName As String In filenames
                Dim path As String = getDirectory() & "\" & FileName
                If Strings.Right(FileName.Trim.ToLower, 3) = "obs" OrElse Strings.Right(FileName.Trim.ToLower, 3) = "ini" Then
                    'we are dealing with 1D observation points
                    Dim obsFile As New clsIniFile(Me.Setup)
                    obsFile.Read(path)
                    For Each myChapter As clsIniFileChapter In obsFile.Chapters.Values
                        If myChapter.Name.Trim.ToLower = "[observationpoint]" Then
                            Dim myObs As New cls1DBranchObject(Me.Setup, Me.Network)
                            For Each Attribute As clsIniFileAttribute In myChapter.Attributes.Values
                                If Attribute.Name.Trim.ToUpper = "NAME" Then
                                    myObs.ID = Attribute.GetValue
                                ElseIf Attribute.Name.Trim.ToUpper = "BRANCHID" Then
                                    myObs.SetBranchByID(Attribute.GetValue)
                                ElseIf Attribute.Name.Trim.ToUpper = "CHAINAGE" Then
                                    myObs.SetChainage(Attribute.GetValueAsDouble)
                                End If
                            Next
                            Observationpoints1D.Add(myObs.ID.Trim.ToUpper, myObs)
                        End If
                    Next

                ElseIf Strings.Right(FileName.Trim.ToLower, 3) = "xyn" Then
                    'we are dealing with 2D observation points
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadNetwork of class clsFlowFMComponent: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function getOutputFullDir() As String
        'read the MDU file
        If MDUFile Is Nothing Then ReadMDU()
        Return MDUFile.GetOutputFullDir
    End Function

    Public Function getOutputSubDir() As String
        If MDUFile Is Nothing Then ReadMDU()
        Return MDUFile.GetOutputSubDir
    End Function

    Public Function getDirectory() As String
        Return DIMR.ProjectDir & "\" & DIMR.DIMRConfig.Flow1D.SubDir
    End Function

    Public Function getSubDirectory() As String
        Return DIMR.DIMRConfig.Flow1D.SubDir
    End Function

    Public Function ReadMDU() As Boolean
        Try
            'this function reads all relevant content from the .MDU file
            'for now it just retrieves the name of our networkfile
            Dim path As String = DIMR.ProjectDir & "\" & DIMR.DIMRConfig.Flow1D.SubDir & "\" & DIMR.DIMRConfig.Flow1D.GetInputFile
            If Not System.IO.File.Exists(path) Then Throw New Exception("Path does not exist: " & path)
            MDUFile = New clsMDUFile(Me.Setup, DIMR, DIMR.DIMRConfig.Flow1D)
            MDUFile.Read(path)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading MDU file: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function WriteSimulationPeriod(StartDate As Date, EndDate As Date) As Boolean
        Try
            'here we read the entire content of the MDU file in memory. Then we replace the line stating the startdate
            Dim path As String = DIMR.ProjectDir & "\" & DIMR.DIMRConfig.Flow1D.SubDir & "\" & DIMR.DIMRConfig.Flow1D.GetInputFile
            Dim content As String, lines As String()
            Using mduReader As New StreamReader(path)
                content = mduReader.ReadToEnd()
            End Using
            lines = Split(content, vbCrLf)

            Using mduWriter As New StreamWriter(path)
                For Each myLine In lines
                    If Strings.Left(myLine.Trim, 7).ToLower = "refdate" Then
                        'write our new start date
                        mduWriter.WriteLine("RefDate                           = " & StartDate.ToString("yyyyMMdd") & "             # Reference date (yyyymmdd)")
                    Else
                        'leave the line untouched and write it
                        mduWriter.WriteLine(myLine)
                    End If
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error setting simulation period for FM Component: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function GetFirstUpstreamObservationpoint(BranchID As String, Chainage As Double, ByRef Observationpoint As cls1DBranchObject) As Boolean
        Try
            Dim MinDist As Double = 9.0E+99
            Dim Found As Boolean = False
            Dim ProcessedBranchIDs As New List(Of String)

            Observationpoint = Nothing

            'we create a stack of branches, walking upward until we find no more new branches
            'start with the current branch and check if we have an upstream observation point if so, we're done
            Dim myBranch As cls1DBranch = Network.Branches.Item(BranchID.Trim.ToUpper)
            ProcessedBranchIDs.Add(myBranch.ID)        'add this branch to the list of already processed branches

            Found = myBranch.getFirstUpstreamObservationPoint(Chainage, Observationpoint)
            'if not found we'll start building a stack of upstream branches

            While Not Found
                Dim upBranches As New List(Of cls1DBranch)
                'retrieve the next upstream branch and check if it has an observation point
                If Network.GetUpstreamBranches(myBranch, ProcessedBranchIDs, upBranches) Then
                    If upBranches.Count = 0 Then Exit While
                    For Each myBranch In upBranches
                        Found = myBranch.getFirstUpstreamObservationPoint(myBranch.calculateLength, Observationpoint)
                        If Found Then Exit While
                    Next
                End If

            End While





            Return Found

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetFirstUpstreamObservationpoint of class clsNetworkFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetFouResultsFileName() As String
        Dim FouResultsFileName As String = Strings.Replace(DIMR.DIMRConfig.Flow1D.GetInputFile(), ".mdu", "_fou.nc")
        Return FouResultsFileName
    End Function
    Public Function GetHisResultsFileName() As String
        Dim HisResultsFileName As String = Strings.Replace(DIMR.DIMRConfig.Flow1D.GetInputFile(), ".mdu", "_his.nc")
        Return HisResultsFileName
    End Function

    Public Function GetMapResultsFileName() As String
        Dim MapResultsFileName As String = Strings.Replace(DIMR.DIMRConfig.Flow1D.GetInputFile(), ".mdu", "_map.nc")
        Return MapResultsFileName
    End Function

    Public Function ReadFouConfigFile() As Boolean
        Try
            'this function reads the file containing our Fou File's configuration, usually named Maxima.fou
            Dim FileName As String = MDUFile.GetFouConfigFileName()

            FouConfig = New clsFouConfigFile(Me.Setup, Me, FileName)
            If Not FouConfig.Read() Then Throw New Exception("Error reading Fourier Configuation File.")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadFouConfigFile: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function GetWaterlevelsForObservationpoint1D(NodeID As String, ByRef Results As Double(), ByRef Times As Double()) As Boolean
        Try
            Dim IDList As String() = Nothing
            Dim Waterlevels As Double(,) = Nothing
            Results = Nothing
            Dim idx As Integer
            Dim ts As Integer

            'these results reside only in the _his.nc file
            If HisNCFile Is Nothing Then
                'this file has never been created or read
                Dim path As String = GetHisResultsFileName()
                path = getOutputFullDir() & "\" & path
                HisNCFile = New clsHisNCFile(path, Setup)
                HisNCFile.ReadWaterLevelsAtObservationPoints(Waterlevels, Times, IDList)
            Else
                'this file has already been read so just read it
                HisNCFile.ReadWaterLevelsAtObservationPoints(Waterlevels, Times, IDList)
            End If

            'find the timeseries for our requested ID and return it
            For idx = 0 To IDList.Count - 1
                If IDList(idx).Trim.ToUpper = NodeID.Trim.ToUpper Then
                    ReDim Results(0 To UBound(Waterlevels, 1))
                    For ts = 0 To UBound(Waterlevels, 1)
                        Results(ts) = Waterlevels(ts, idx)
                    Next
                    Return True
                End If
            Next


            Throw New Exception("Results for observation point " & NodeID & " not found in _his.nc file.")

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetWaterlevelsForObservationpoint1D of class clsFlowFMComponent: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetWaterlevelsForMeshNode(NodeID As String, ByRef Results As Double()) As Boolean
        Try
            'these results reside only in the _map.nc file unforunately
            If MapNCFile Is Nothing Then
                Dim path As String = GetMapResultsFileName()
                path = getOutputFullDir() & "\" & path
                MapNCFile = New clsMapNCFile(path, Setup)
                MapNCFile.GetWaterlevelsForMeshNode(NodeID, Results)
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetWaterlevelsForMeshNode: " & ex.Message)
            Return False
        End Try
    End Function




End Class
