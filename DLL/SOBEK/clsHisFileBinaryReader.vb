Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports STOCHLIB.General

''' <summary>
''' His file (Sobek) reader
''' Reads binary and therefore bypasses ODSSVR20.DLL that does not work inside Visual Studio 2008 for selections of locations and/or locations/timesteps
''' Hence this is an alternative for clsHisFile
''' </summary>
Public Class clsHisFileBinaryReader
    Implements IDisposable
    Dim hisFileHeader As stHisFileHeader
    Private binaryReader As BinaryReader
    Private disposed As Boolean
    Private fileStream As FileStream
    Private Path As String
    Private HIA As clsHiaFile

    Dim State As GeneralFunctions.enmFileState

    'in case of read to memory (entire file)
    Dim hisData As Byte()
    Friend ms As MemoryStream

    Public Stats As clsHisFileStats

    Private Setup As clsSetup

    Public Sub New(ByVal myPath As String, ByRef mySetup As clsSetup)
        Path = myPath
        Setup = mySetup
        HIA = New clsHiaFile(Me.Setup, Left(Path, Len(Path) - 1) & "a")
        Stats = New clsHisFileStats(Me.Setup)
        hisFileHeader = New stHisFileHeader
    End Sub

    Public Function populateMultiCheckboxWithTimesteps(ByRef chk As Windows.Forms.CheckedListBox) As Boolean
        Try
            Dim myDates As List(Of Date) = ReadAllTimesteps()
            For Each myDate As Date In myDates
                chk.Items.Add(myDate)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function OpenFile(Optional ByVal ReadHisHeader As Boolean = True, Optional ByVal UpdateProgressBar As Boolean = False) As Boolean
        Try
            'clear the existing values and (re)read the hia-file
            HIA.Clear()
            If System.IO.File.Exists(HIA.Path) Then HIA.Read()
            If System.IO.File.Exists(Path) Then
                Open(Path, ReadHisHeader, UpdateProgressBar)
            Else
                Throw New Exception("Error: hisfile does not exist: " & Path)
            End If
            State = GeneralFunctions.enmFileState.Open
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function ReadToMemory() As Boolean
        'this function reads an entire HIS-file to memory
        'it will hoever read the hisfileheader separately since this process takes little time and generally only has to be done once anyway
        Try

            'open and close the file in order to just read the hisfileheader
            If Not OpenFile() Then
                Throw New Exception("Error opening hisfile: " & Path)
            Else
                Call FileClose()
            End If

            'now read the entire file content as a memory stream
            binaryReader = New BinaryReader(System.IO.File.OpenRead(Path))
            hisData = binaryReader.ReadBytes(binaryReader.BaseStream.Length)
            ms = New MemoryStream(hisData, 0, hisData.Length)
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetTimestepSize() As TimeSpan
        Dim mySpan As New TimeSpan
        mySpan = hisFileHeader.TimeSteps(1).Subtract(hisFileHeader.TimeSteps(0))
        Return mySpan
    End Function

    Public ReadOnly Property GetHisFileHeader() As stHisFileHeader
        Get
            Return hisFileHeader
        End Get
    End Property

    Public Function ReadAllData() As List(Of HisDataRow)
        Return ReadAllData(Nothing)
    End Function

    Public Function getParName(ByVal PartialName As String) As String
        For Each myParameter As String In hisFileHeader.parameters
            If InStr(myParameter.Trim.ToUpper, PartialName.Trim.ToUpper) > 0 Then
                Return myParameter
            End If
        Next
        Return ""
    End Function

    Public Function GetLocIdx(ByVal ID As String) As Long
        Dim i As Long
        For i = 0 To hisFileHeader.Locations.Count - 1
            If hisFileHeader.Locations.Item(i).ToString.Trim.ToUpper = ID.Trim.ToUpper Then
                Return i
            End If
        Next
    End Function


    Public Function GetLocationIdx(LocationID As String, ByRef LocationIdx As Integer) As Boolean
        Try
            LocationIdx = hisFileHeader.Locations.IndexOf(LocationID)

            'if location index not found, it could be a long ID. Check the .hia-file.
            If LocationIdx < 0 Then
                Dim myLongLoc As clsLongLocation = HIA.GetByID(LocationID)
                If Not myLongLoc Is Nothing Then
                    LocationIdx = myLongLoc.Num
                Else
                    Throw New Exception("Location not found in hisfile: " & LocationID)
                End If
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetLocationIdx fo class clsHisFileBinaryReader.")
            Return False
        End Try

    End Function

    Public Function GetParameterIdx(PartOfParameterName As String, ByRef Paridx As Integer) As Boolean
        Dim i As Integer, Par As String
        Try
            'look for the corresponding parameter index
            For i = 0 To hisFileHeader.parameters.Count - 1
                Par = hisFileHeader.parameters.Item(i)
                If InStr(Par.Trim.ToUpper, PartOfParameterName.Trim.ToUpper, CompareMethod.Text) > 0 Then
                    Paridx = i
                    Return True
                End If
            Next
            Throw New Exception("Parameter not found: " & PartOfParameterName)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function GetTimestepIdx(Timestep As DateTime, ByRef timeStepIndex As Integer) As Boolean
        Try
            timeStepIndex = hisFileHeader.TimeSteps.IndexOf(Timestep)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadAllTimesteps() As List(Of Date)
        Call ReadHisHeader()
        Return hisFileHeader.TimeSteps
    End Function


    Public Sub CalcStats(ByVal PartOfParName As String, ByVal MinMaxMean As Boolean, ByVal Tides As Boolean)

        'Author: Siebe Bosch
        'Date: 22-04-2013
        'Description: reads statistics for all locations from a HisFile
        Dim myTable As clsTimeTable
        Dim i As Long, n As Long

        'first open the file & HIA-file & read the header, which contains all locations
        Call OpenFile()

        Me.Setup.GeneralFunctions.UpdateProgressBar("Deriving statistics of computed water levels.", 0, 10)
        n = hisFileHeader.Locations.Count

        'Now calculate stats for each location
        For Each myLocation As String In hisFileHeader.Locations
            i += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
            myTable = New clsTimeTable(Me.Setup)
            If Not ReadAddLocationResults(myLocation, PartOfParName, myTable, 0, False) Then
                Me.Setup.Log.AddError("Could not read water levels for location " & myLocation)
            Else
                If MinMaxMean Then Call Stats.AddMinMaxMean(myLocation, myTable, 0)
                If Tides Then Call Stats.AddTides(myLocation, myTable, 0)
            End If
        Next
        Call Close()
    End Sub

    Public Function ReadAllData(ByVal parameterName As String) As List(Of HisDataRow)
        Dim dataRows = New List(Of HisDataRow)()
        binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
        For Each timeStep As Object In hisFileHeader.TimeSteps
            dataRows.AddRange(ReadTimeStep(timeStep, parameterName))
        Next
        Return dataRows
    End Function

    Public Function ReadAllDataOneParameterToDataTable(ByVal parameterName As String, Optional ByVal SkipFirstTimesteps As Integer = 0, Optional UpdateProgressBar As Boolean = True) As DataTable
        Dim dt As New DataTable, i As Long, ts As Long, n As Long = hisFileHeader.TimeSteps.Count
        Dim ParIdx As Integer, iPar As Integer, parameter As Object
        Dim timestep As Object

        'populate the datatable columns
        For i = 0 To hisFileHeader.Locations.Count - 1
            dt.Columns.Add(i, Type.GetType("System.Double"))
        Next

        'set the parameter index number
        For iPar = 0 To hisFileHeader.parameters.Count - 1
            parameter = hisFileHeader.parameters.Item(iPar)
            If parameterName Is Nothing OrElse Left(parameter.trim.tolower, parameterName.Trim.Length) = parameterName.Trim.ToLower Then
                ParIdx = iPar
                Exit For
            End If
        Next

        'read all data for the chosen parameter
        binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
        If UpdateProgressBar Then Me.Setup.GeneralFunctions.UpdateProgressBar("Reading hisfile...", 0, 10)
        ts = 0
        For Each timestep In hisFileHeader.TimeSteps
            ts += 1
            If ts > SkipFirstTimesteps Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("", ts, n)
                ReadTimeStepToDatatable(timestep, ParIdx, dt)
            End If
        Next
        Return dt
    End Function

    Public Function ReadAllDataOneParameterTo1DDataTable(ByVal parameterName As String) As DataTable
        Dim dt As New DataTable, ts As Long, n As Long = hisFileHeader.TimeSteps.Count
        Dim ParIdx As Integer, iPar As Integer, parameter As Object
        Dim timestep As Object

        'populate the columns
        dt.Columns.Add("ID", Type.GetType("System.String"))
        dt.Columns.Add("Value", Type.GetType("System.Double"))

        'set the parameter index number
        For iPar = 0 To hisFileHeader.parameters.Count - 1
            parameter = hisFileHeader.parameters.Item(iPar)
            If parameterName Is Nothing OrElse Left(parameter.trim.tolower, parameterName.Trim.Length) = parameterName.Trim.ToLower Then
                ParIdx = iPar
                Exit For
            End If
        Next

        'read all data for the chosen parameter
        binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
        Me.Setup.GeneralFunctions.UpdateProgressBar("Reading hisfile...", 0, 10)
        For Each timestep In hisFileHeader.TimeSteps
            ts += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", ts, n)
            ReadTimeStepTo1DDatatable(timestep, ParIdx, dt)
        Next
        Return dt
    End Function

    Public Function ReadAllDataOneParameterToTimeSeriesDictionary(ByVal ParameterName As String, Optional ByVal SkipFirstTimesteps As Long = 0, Optional Multiplier As Double = 1) As Dictionary(Of String, List(Of Single))
        Try
            Dim ts As List(Of Single)
            Dim tsCollection As New Dictionary(Of String, List(Of Single))

            Dim i As Long, tsIdx As Long, n As Long = hisFileHeader.TimeSteps.Count
            Dim ParIdx As Integer, iPar As Integer, parameter As Object
            Dim timestep As Object
            Dim dtCollection As New Dictionary(Of String, DataTable)

            'populate the timeseries collection
            For i = 0 To hisFileHeader.Locations.Count - 1
                ts = New List(Of Single)
                If Not tsCollection.ContainsKey(hisFileHeader.Locations(i).Trim.ToUpper) Then
                    tsCollection.Add(hisFileHeader.Locations(i).Trim.ToUpper, ts)
                Else
                    Me.Setup.Log.AddError("Multiple instances of the same location encountered in the resultsfile: " & hisFileHeader.Locations(i))
                End If
            Next

            'set the parameter index number
            For iPar = 0 To hisFileHeader.parameters.Count - 1
                parameter = hisFileHeader.parameters.Item(iPar)
                If ParameterName Is Nothing OrElse Left(parameter.trim.tolower, ParameterName.Trim.Length) = ParameterName.Trim.ToLower Then
                    ParIdx = iPar
                    Exit For
                End If
            Next

            'read all data for the chosen parameter
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading data from hisfile...", 0, 10)
            tsIdx = 0
            For Each timestep In hisFileHeader.TimeSteps
                tsIdx += 1
                If tsIdx > SkipFirstTimesteps Then
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", tsIdx, n)
                    ReadTimeStepToTimeSeriesCollection(timestep, ParIdx, tsCollection, Multiplier)
                End If
            Next
            Return tsCollection
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadDataOneParameterToTimeSeriesDictionary")
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function ReadAllDataOneParameterToDataTableCollection(ByVal parameterName As String, Optional ByVal SkipFirstTimesteps As Long = 0) As Dictionary(Of String, DataTable)
        Try
            Dim dt As New DataTable, i As Long, ts As Long, n As Long = hisFileHeader.TimeSteps.Count
            Dim ParIdx As Integer, iPar As Integer, parameter As Object
            Dim timestep As Object
            Dim dtCollection As New Dictionary(Of String, DataTable)

            'populate the datatables collection
            For i = 0 To hisFileHeader.Locations.Count - 1
                dt = New DataTable
                dt.Columns.Add("", Type.GetType("System.String"))
                dt.Columns.Add("", Type.GetType("System.Double"))
                If Not dtCollection.ContainsKey(hisFileHeader.Locations(i).Trim.ToUpper) Then
                    dtCollection.Add(hisFileHeader.Locations(i).Trim.ToUpper, dt)
                Else
                    Me.Setup.Log.AddError("Multiple instances of the same location encountered in the resultsfile: " & hisFileHeader.Locations(i))
                End If
            Next

            'set the parameter index number
            For iPar = 0 To hisFileHeader.parameters.Count - 1
                parameter = hisFileHeader.parameters.Item(iPar)
                If parameterName Is Nothing OrElse Left(parameter.trim.tolower, parameterName.Trim.Length) = parameterName.Trim.ToLower Then
                    ParIdx = iPar
                    Exit For
                End If
            Next

            'read all data for the chosen parameter
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading hisfile...", 0, 10)
            ts = 0
            For Each timestep In hisFileHeader.TimeSteps
                ts += 1
                If ts > SkipFirstTimesteps Then
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", ts, n)
                    ReadTimeStepToDatatableCollection(timestep, ParIdx, dtCollection)
                End If
            Next
            Return dtCollection
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Function ReadAllLocations(Optional ByVal Sort As Boolean = False) As List(Of String)
        Call ReadHisHeader()
        If Sort Then hisFileHeader.Locations.Sort()
        Return hisFileHeader.Locations
    End Function

    Public Function CountTimesteps() As Integer
        Call ReadHisHeader()
        Return hisFileHeader.TimeSteps.Count
    End Function

    Public Function ReadLocationsByWildcard(ByVal StringWithWildcards As String) As List(Of String)
        Call ReadHisHeader()
        Dim NewList As New List(Of String)
        For Each myLoc As String In hisFileHeader.Locations
            If myLoc Like StringWithWildcards Then NewList.Add(myLoc)
        Next
        Return NewList
    End Function


    Public Function ReadAllLocationsAsDictionary() As Dictionary(Of String, Long)
        Call ReadHisHeader()

        'reads all hisfile locations to a dictionary that contains the index number of each location
        Dim newDict As New Dictionary(Of String, Long)
        Dim i As Long = -1
        For Each myloc As String In hisFileHeader.Locations
            i += 1
            newDict.Add(myloc.Trim.ToUpper, i)
        Next
        Return newDict
    End Function

    Public Function ReadLocationSubset(startIdx As Long, endIdx As Long) As List(Of String)
        Dim i As Long, myList As New List(Of String)
        Call ReadHisHeader()

        'make sure the endIdx does not exceed the last location index number
        endIdx = Math.Min(endIdx, hisFileHeader.Locations.Count - 1)

        For i = startIdx To endIdx
            myList.Add(hisFileHeader.Locations(i))
        Next
        Return myList
    End Function

    Public Function ReadSpecificLocation(ID As String) As List(Of String)
        Dim i As Long, myList As New List(Of String)

        Call ReadHisHeader()
        For i = 0 To hisFileHeader.Locations.Count - 1
            If hisFileHeader.Locations(i).Trim.ToUpper = ID.Trim.ToUpper Then
                myList.Add(hisFileHeader.Locations(i))
            End If
        Next
        Return myList
    End Function

    Public Function ReadAllParameters() As List(Of String)
        Call ReadHisHeader()
        Return hisFileHeader.parameters
    End Function

    Public Function ReadTimeStep(ByVal timeStep As DateTime, ByVal parameterName As String) As List(Of HisDataRow)
        Dim rows = New List(Of HisDataRow)()
        Dim timeStepIndex As Integer = hisFileHeader.TimeSteps.IndexOf(timeStep)
        Dim position As Long = hisFileHeader.StreamStartDataPosition + (timeStepIndex * CLng(hisFileHeader.StreamDataBlockSize))

        binaryReader.BaseStream.Position = position

        binaryReader.ReadInt32()
        'needed for position (= deltaT as integer)
        For Each location As Object In hisFileHeader.Locations
            For Each parameter As Object In hisFileHeader.parameters
                Dim value As Double = Convert.ToDouble(binaryReader.ReadSingle())
                If parameterName Is Nothing OrElse Left(parameter.trim.tolower, parameterName.Trim.Length) = parameterName.Trim.ToLower Then
                    Dim row = New HisDataRow() With {.LocationName = location, .parameter = parameter, .TimeStep = timeStep, .Value = value}
                    rows.Add(row)
                End If
            Next
        Next

        Return rows
    End Function

    Public Sub ReadTimeStepToDatatable(ByVal timeStep As DateTime, ByVal parIdx As Integer, ByRef dt As DataTable)
        'Dim rows = New List(Of HisDataRow)()
        Dim timeStepIndex As Integer = hisFileHeader.TimeSteps.IndexOf(timeStep)
        Dim position As Integer = hisFileHeader.StreamStartDataPosition + (timeStepIndex * hisFileHeader.StreamDataBlockSize)
        Dim value As Double
        Dim Values(0 To hisFileHeader.Locations.Count - 1) As Double
        Dim iLoc As Integer = -1
        Dim iPar As Integer

        binaryReader.BaseStream.Position = position

        binaryReader.ReadInt32()
        'needed for position (= deltaT as integer)
        For Each location As Object In hisFileHeader.Locations
            iLoc += 1
            For iPar = 0 To hisFileHeader.parameters.Count - 1
                If iPar = parIdx Then
                    value = Convert.ToDouble(binaryReader.ReadSingle())
                    Values(iLoc) = value
                Else
                    binaryReader.BaseStream.Position += 4
                End If
            Next
        Next

        Dim dr As DataRow = dt.NewRow()
        For i As Integer = 0 To Values.Count - 1
            dr(i) = Values(i)
        Next
        dt.Rows.Add(dr)

    End Sub

    Public Sub ReadTimeStepTo1DDatatable(ByVal timeStep As DateTime, ByVal parIdx As Integer, ByRef dt As DataTable)
        'Dim rows = New List(Of HisDataRow)()
        Dim timeStepIndex As Integer = hisFileHeader.TimeSteps.IndexOf(timeStep)
        Dim position As Integer = hisFileHeader.StreamStartDataPosition + (timeStepIndex * hisFileHeader.StreamDataBlockSize)
        Dim iLoc As Integer = -1
        Dim iPar As Integer
        Dim dr As DataRow

        binaryReader.BaseStream.Position = position

        binaryReader.ReadInt32()
        'needed for position (= deltaT as integer)
        For Each location As Object In hisFileHeader.Locations
            iLoc += 1
            For iPar = 0 To hisFileHeader.parameters.Count - 1
                If iPar = parIdx Then
                    dr = dt.NewRow()
                    dr(0) = location.ToString
                    dr(1) = Convert.ToDouble(binaryReader.ReadSingle())
                    dt.Rows.Add(dr)
                Else
                    binaryReader.BaseStream.Position += 4
                End If
            Next
        Next
    End Sub


    Public Sub ReadTimeStepToDatatableCollection(ByVal timeStep As DateTime, ByVal parIdx As Integer, ByRef dtCollection As Dictionary(Of String, DataTable))
        'this routine writes the results of one parameter & one timestep from a hisfile to a collection of datatables
        'every location has its own datatable
        Dim timeStepIndex As Integer = hisFileHeader.TimeSteps.IndexOf(timeStep)
        Dim position As Integer = hisFileHeader.StreamStartDataPosition + (timeStepIndex * hisFileHeader.StreamDataBlockSize)
        Dim iPar As Integer
        Dim dr As DataRow
        Dim dt As DataTable

        binaryReader.BaseStream.Position = position

        binaryReader.ReadInt32()
        'needed for position (= deltaT as integer)
        For i = 0 To hisFileHeader.Locations.Count - 1
            dt = dtCollection.Item(hisFileHeader.Locations(i).Trim.ToUpper)
            For iPar = 0 To hisFileHeader.parameters.Count - 1
                If iPar = parIdx Then
                    dr = dt.NewRow()
                    dr(0) = hisFileHeader.Locations(i).ToString
                    dr(1) = Convert.ToDouble(binaryReader.ReadSingle())
                    dt.Rows.Add(dr)
                Else
                    binaryReader.BaseStream.Position += 4
                End If
            Next
        Next

    End Sub

    Public Sub ReadTimeStepToTimeSeriesCollection(ByVal timeStep As DateTime, ByVal parIdx As Integer, ByRef tsCollection As Dictionary(Of String, List(Of Single)), Optional ByVal Multiplier As Double = 1)
        'this routine writes the results of all locations, one parameter & one timestep from a hisfile to a collection of timeseries
        'please note that we store every timeseries only as a list of singles in order to preserve memory
        'the exact timesteps can always be recovered from the hisfile header
        Dim timeStepIndex As Integer = hisFileHeader.TimeSteps.IndexOf(timeStep)
        Dim position As Integer = hisFileHeader.StreamStartDataPosition + (timeStepIndex * hisFileHeader.StreamDataBlockSize)
        Dim iPar As Integer
        Dim ts As List(Of Single)

        binaryReader.BaseStream.Position = position

        binaryReader.ReadInt32()
        'needed for position (= deltaT as integer)
        For i = 0 To hisFileHeader.Locations.Count - 1
            ts = tsCollection.Item(hisFileHeader.Locations(i).Trim.ToUpper)
            For iPar = 0 To hisFileHeader.parameters.Count - 1
                If iPar = parIdx Then
                    ts.Add(binaryReader.ReadSingle * Multiplier)
                Else
                    binaryReader.BaseStream.Position += 4
                End If
            Next
        Next

    End Sub

    Public Function ReadLastTimeStep(ByVal parameterName As String) As List(Of HisDataRow)

        Call OpenFile()
        Call ReadHisHeader()

        Dim rows = New List(Of HisDataRow)()
        Dim timeStepIndex As Integer = hisFileHeader.TimeSteps.Count - 1
        Dim position As Integer = hisFileHeader.StreamStartDataPosition + (timeStepIndex * hisFileHeader.StreamDataBlockSize)

        binaryReader.BaseStream.Position = position

        binaryReader.ReadInt32()
        'needed for position (= deltaT as integer)
        For Each location As Object In hisFileHeader.Locations
            For Each parameter As Object In hisFileHeader.parameters
                Dim value As Double = Convert.ToDouble(binaryReader.ReadSingle())
                If parameterName Is Nothing OrElse InStr(parameter.ToString, parameterName, CompareMethod.Text) > 0 Then
                    Dim row = New HisDataRow() With {.LocationName = location, .parameter = parameter, .TimeStep = hisFileHeader.TimeSteps(timeStepIndex), .Value = value}
                    rows.Add(row)
                End If
            Next
        Next

        Close()

        Return rows
    End Function


    Public Function ReadAddLocationResults(ByVal location As String, ByVal PartOfParameterName As String, ByRef myResult As clsTimeTable, ByVal FieldIdx As Integer, Optional ByVal Multiplier As Double = 1, Optional ByVal UseRegEx As Boolean = False) As Boolean
        Try
            'Author: code courtesy of Deltares (Delft, The Netherlands) via mr. Klaas-Jan van Heeringen
            'Adjusted by Siebe Bosch on 6-4-2013
            'Description: Reads results from a hisfile and in case of existing results adds them up with existing values
            Dim Par As String = "", ParFound As Boolean = False
            Dim myDate As Date
            Dim myRecord As clsTimeTableRecord

            'assign an ID for the current table if not already assigned
            If myResult.ID = "" Then myResult.ID = location

            'if location index not found, it could be a long ID. Check the .hia-file.
            Dim locationIndex As Integer = hisFileHeader.Locations.IndexOf(location)
            If locationIndex < 0 Then
                Dim myLongLoc As clsLongLocation = HIA.GetByID(location)
                If Not myLongLoc Is Nothing Then
                    locationIndex = myLongLoc.Num
                Else
                    Return False
                End If
            End If

            'look for the right parameter index
            If UseRegEx Then
                For Each Par In hisFileHeader.parameters
                    If Me.Setup.GeneralFunctions.RegExMatch(PartOfParameterName, Par, True) Then
                        ParFound = True
                        Exit For
                    End If
                Next
            Else
                For Each Par In hisFileHeader.parameters
                    If InStr(Par.Trim.ToUpper, PartOfParameterName.Trim.ToUpper, CompareMethod.Text) > 0 Then
                        ParFound = True
                        Exit For
                    End If
                Next
            End If


            'if parameter index not found, then use the first parameter in the hisfile
            If Not ParFound Then
                Par = hisFileHeader.parameters.Item(0)
                Me.Setup.Log.AddError("No parameter in hisfile found containing " & PartOfParameterName & "; first parameter was used.")
            End If

            Dim parameterIndex As Integer = hisFileHeader.parameters.IndexOf(Par)
            'If parameterIndex < 0 Then parameterIndex = HIA.longParameters.Item(Par)

            Dim bitPosition = (locationIndex * hisFileHeader.parameters.Count + parameterIndex) * 4
            Dim [step] As Integer = 0

            'read the data
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
            While binaryReader.BaseStream.Position <= (binaryReader.BaseStream.Length - hisFileHeader.StreamDataBlockSize)

                binaryReader.ReadInt32()
                Dim block = binaryReader.ReadBytes(hisFileHeader.StreamDataBlockSize - 4)

                myDate = hisFileHeader.TimeSteps([step])
                myRecord = myResult.GetAddRecord(myDate)
                myRecord.AddToValue(FieldIdx, Convert.ToDouble(BitConverter.ToSingle(block, bitPosition)) * Multiplier)
                [step] += 1
            End While

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function




    Public Function ReadAddLocationResultsToArray(ByVal location As String, ByVal PartOfParameterName As String, ByRef Dates() As DateTime, ByRef Values() As Double, Multiplier As Double) As Boolean

        'Author: code courtesy of Deltares (Delft, The Netherlands) via mr. Klaas-Jan van Heeringen
        'Adjusted by Siebe Bosch on 6-4-2013
        'Description: Reads results from a hisfile and in case of existing results adds them up with existing values
        Dim Par As String = "", ParFound As Boolean = False

        Call OpenFile()

        'if location index not found, it could be a long ID. Check the .hia-file.
        Dim locationIndex As Integer = hisFileHeader.Locations.IndexOf(location)
        If locationIndex < 0 Then
            Dim myLongLoc As clsLongLocation = HIA.GetByID(location)
            If Not myLongLoc Is Nothing Then
                locationIndex = myLongLoc.Num
            Else
                Return False
            End If
        End If

        'look for the right parameter index
        For Each Par In hisFileHeader.parameters
            If InStr(Par.Trim.ToUpper, PartOfParameterName.Trim.ToUpper, CompareMethod.Text) > 0 Then
                ParFound = True
                Exit For
            End If
        Next

        'if parameter index not found, then use the first parameter in the hisfile
        If Not ParFound Then
            Par = hisFileHeader.parameters.Item(0)
            Me.Setup.Log.AddError("No parameter in hisfile found containing " & PartOfParameterName & "; first parameter was used.")
        End If

        Dim parameterIndex As Integer = hisFileHeader.parameters.IndexOf(Par)
        'If parameterIndex < 0 Then parameterIndex = HIA.longParameters.Item(Par)

        Dim bitPosition = (locationIndex * hisFileHeader.parameters.Count + parameterIndex) * 4
        Dim [step] As Integer = 0

        ReDim Dates(hisFileHeader.TimeSteps.Count - 1)
        ReDim Values(hisFileHeader.TimeSteps.Count - 1)

        'read the data
        binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
        While binaryReader.BaseStream.Position <= (binaryReader.BaseStream.Length - hisFileHeader.StreamDataBlockSize)
            binaryReader.ReadInt32()
            Dim block = binaryReader.ReadBytes(hisFileHeader.StreamDataBlockSize - 4)
            Dates([step]) = hisFileHeader.TimeSteps([step])
            Values([step]) = Convert.ToDouble(BitConverter.ToSingle(block, bitPosition)) * Multiplier
            [step] += 1
        End While

        Close()

        Return True

    End Function

    Public Function ReadAddLocationResultsToDataTable(ByVal location As String, ByVal PartOfParameterName As String, ByRef dt As DataTable, Optional ByVal multiplier As Double = 1, Optional ByVal ValColIdx As Integer = 1, Optional ByVal RelativeTime As Boolean = False, Optional ByVal CloseWhenDone As Boolean = True) As Boolean
        Try
            'Author: code courtesy of Deltares (Delft, The Netherlands) via mr. Klaas-Jan van Heeringen
            'Adjusted by Siebe Bosch on 6-4-2013
            'Description: Reads results from a hisfile and in case of existing results adds them up with existing values
            Dim Par As String = "", ParFound As Boolean = False

            'prevent reading the header too often. Especially with large files this can take a long time!
            If Not State = GeneralFunctions.enmFileState.Open Then
                If Not OpenFile() Then Throw New Exception("Error plotting chart: could Not open resultsfile: " & Path & ".")
            End If

            'if location index not found, it could be a long ID. Check the .hia-file.
            Dim locationIndex As Integer = hisFileHeader.Locations.IndexOf(location)
            If locationIndex < 0 Then
                Dim myLongLoc As clsLongLocation = HIA.GetByID(location)
                If Not myLongLoc Is Nothing Then
                    locationIndex = myLongLoc.Num
                Else
                    Return False
                End If
            End If

            'look for the right parameter index
            For Each Par In hisFileHeader.parameters
                If InStr(Par.Trim.ToUpper, PartOfParameterName.Trim.ToUpper, CompareMethod.Text) > 0 Then
                    ParFound = True
                    Exit For
                End If
            Next

            'if parameter index not found, then use the first parameter in the hisfile
            If Not ParFound Then
                Return False
            End If

            Dim parameterIndex As Integer = hisFileHeader.parameters.IndexOf(Par)
            'If parameterIndex < 0 Then parameterIndex = HIA.longParameters.Item(Par)

            Dim bitPosition = (locationIndex * hisFileHeader.parameters.Count + parameterIndex) * 4
            Dim [step] As Integer = 0
            Dim StepSizeHours As Double = hisFileHeader.TimeSteps(1).Subtract(hisFileHeader.TimeSteps(0)).TotalHours

            'read the data
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
            While binaryReader.BaseStream.Position <= (binaryReader.BaseStream.Length - hisFileHeader.StreamDataBlockSize)
                binaryReader.ReadInt32()
                Dim block = binaryReader.ReadBytes(hisFileHeader.StreamDataBlockSize - 4)

                'if the current date is alreay in the datatable, add our value to the desired column
                'else add a new row
                Dim rowIdx As Long
                If RelativeTime Then
                    rowIdx = Setup.GeneralFunctions.GetAddDataTableDoubleIdx(dt, [step] * StepSizeHours, 0)
                Else
                    rowIdx = Setup.GeneralFunctions.GetAddDataTableDateIdx(dt, hisFileHeader.TimeSteps([step]), 0)
                End If
                dt.Rows(rowIdx)(ValColIdx) = Convert.ToDouble(BitConverter.ToSingle(block, bitPosition)) * multiplier
                [step] += 1
            End While

            If CloseWhenDone Then Close()

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            MsgBox(ex.Message)
            Return False
        End Try


    End Function

    Public Function ReadValue(ByVal LocationID As String, ByVal PartOfParameterName As String, ByVal timeStep As DateTime, Optional ByVal ReadHeader As Boolean = True) As Double
        Dim Par As String = "", ParFound As Boolean = False
        Dim Locidx As Integer, ParIdx As Integer, TSIdx As Integer
        Call OpenFile(ReadHeader)

        'find the location index, parameter index and timestep index
        If Not GetLocationIdx(LocationID, Locidx) Then Throw New Exception("Could not find index number for location " & LocationID & " in hisfile.")
        If Not GetParameterIdx(PartOfParameterName, ParIdx) Then ParIdx = 0
        If Not GetTimestepIdx(timeStep, TSIdx) Then Throw New Exception("Could not find index number for tiemestep " & timeStep & " in hisfile.")

        'the HIS-file is organized as one block per timestep: streamdatablocksize
        'within each block we start with an int32 (4 bytes) that contains the timestep size in seconds
        'then we have have sub-blocks containing data for every location
        'within every location-block we have one value (4 byte floating point single) per paramter
        Dim position As Long = hisFileHeader.StreamStartDataPosition + (TSIdx * CLng(hisFileHeader.StreamDataBlockSize)) 'position at the start of the streamdatablock to be read

        position += 4                                               'skip over the timestepsize
        position += (Locidx * hisFileHeader.parameters.Count) * 4   'set the position at the start of the location to be read
        position += ParIdx * 4                                      'set position at the start of the value to be read
        binaryReader.BaseStream.Position = position
        Dim value As Double = Convert.ToDouble(binaryReader.ReadSingle())
        Return value
    End Function




    Public Function ReadLocationResultsToDataTable(ByVal locationID As String, ByVal PartOfParameterName As String, ByRef dt As DataTable, Optional ByVal multiplier As Double = 1, Optional ByVal ReadHeader As Boolean = True) As Boolean
        Try
            'Author: code courtesy of Deltares (Delft, The Netherlands) via mr. Klaas-Jan van Heeringen
            'Adjusted by Siebe Bosch on 6-4-2013
            'Description: Reads results from a hisfile and in case of existing results adds them up with existing values
            Dim Par As String = "", ParFound As Boolean = False
            Dim locationindex As Integer
            Dim parameterIndex As Integer

            'open the hisfile and read the header (if required)
            Call OpenFile(ReadHeader)

            'get the index number for the location
            If Not GetLocationIdx(locationID, locationindex) Then Throw New Exception("Error getting index number for hisfile location " & locationID)

            'get the full parametername
            If Not GetParameterIdx(PartOfParameterName, parameterIndex) Then
                parameterIndex = 0
                Me.Setup.Log.AddWarning("Error getting hisfile parameter based on the string " & PartOfParameterName & ". First parameter was used instead.")
            End If

            'if parameter index not found, then use the first parameter in the hisfile
            If Not ParFound Then
                Me.Setup.Log.AddError("No parameter in hisfile found containing " & PartOfParameterName & "; first parameter was used.")
            End If

            'find the bitposition for the given location, parameter
            Dim bitPosition = (locationindex * hisFileHeader.parameters.Count + parameterIndex) * 4
            Dim [step] As Integer = 0
            Dim StepSizeHours As Double = hisFileHeader.TimeSteps(1).Subtract(hisFileHeader.TimeSteps(0)).TotalHours

            'read the data
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
            While binaryReader.BaseStream.Position <= (binaryReader.BaseStream.Length - hisFileHeader.StreamDataBlockSize)
                binaryReader.ReadInt32()
                Dim block = binaryReader.ReadBytes(hisFileHeader.StreamDataBlockSize - 4)

                'add a new row to the datatable
                dt.Rows.Add(hisFileHeader.TimeSteps([step]), Convert.ToDouble(BitConverter.ToSingle(block, bitPosition)) * multiplier)
                [step] += 1
            End While

            Close()

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function


    Public Function ReadStochasticResults(ByVal location As String, ByVal PartOfParameterName As String, ByRef MaxInLastTimeStep As Boolean, ByRef Min As Double, ByRef tsMin As Long, ByRef Max As Double, ByRef tsMax As Long, ByRef Avg As Double, Optional ByVal SkipFirstPercentage As Integer = 0) As Boolean

        Try
            'Author: code courtesy of Deltares (Delft, The Netherlands) via mr. Klaas-Jan van Heeringen
            'Adjusted by Siebe Bosch on 28-8-2014
            'Description: Reads statistics from a hisfile and places them in a clsSobekTable class instance
            Dim Par As String = "", ParFound As Boolean = False

            Dim n As Long, Sum As Double, Val As Double
            Max = -999999999
            Min = 999999999
            Avg = 0
            MaxInLastTimeStep = False

            Call OpenFile()

            'if location index not found, it could be a long ID. Check the .hia-file.
            Dim locationIndex As Integer = hisFileHeader.Locations.IndexOf(location)
            If locationIndex < 0 Then
                Dim myLongLoc As clsLongLocation = HIA.GetByID(location)
                If Not myLongLoc Is Nothing Then
                    locationIndex = myLongLoc.Num
                Else
                    Return False
                End If
            End If

            'look for the right parameter index
            For Each Par In hisFileHeader.parameters
                If InStr(Par.Trim.ToUpper, PartOfParameterName.Trim.ToUpper, CompareMethod.Text) > 0 Then
                    ParFound = True
                    Exit For
                End If
            Next

            'if parameter index not found, then use the first parameter in the hisfile
            If Not ParFound Then
                Par = hisFileHeader.parameters.Item(0)
                Me.Setup.Log.AddError("No parameter in hisfile found containing " & PartOfParameterName & "; first parameter was used.")
            End If

            Dim parameterIndex As Integer = hisFileHeader.parameters.IndexOf(Par)
            'If parameterIndex < 0 Then parameterIndex = HIA.longParameters.Item(Par)

            Dim bitPosition = (locationIndex * hisFileHeader.parameters.Count + parameterIndex) * 4
            Dim [step] As Integer = 0
            Dim [nsteps] As Integer = (binaryReader.BaseStream.Length - hisFileHeader.StreamStartDataPosition) / hisFileHeader.StreamDataBlockSize

            'read the data
            binaryReader.BaseStream.Position = hisFileHeader.StreamStartDataPosition
            While binaryReader.BaseStream.Position <= (binaryReader.BaseStream.Length - hisFileHeader.StreamDataBlockSize)

                binaryReader.ReadInt32()
                Dim block = binaryReader.ReadBytes(hisFileHeader.StreamDataBlockSize - 4)

                If ([step] + 1) / [nsteps] * 100 >= SkipFirstPercentage Then
                    n += 1
                    Val = Convert.ToDouble(BitConverter.ToSingle(block, bitPosition))
                    Sum += Val
                    If Val < Min Then
                        Min = Val
                        tsMin = [step]
                    End If
                    If Val > Max Then
                        Max = Val
                        tsMax = [step]
                    End If
                End If

                [step] += 1

            End While

            If tsMax = [nsteps] - 1 Then MaxInLastTimeStep = True

            Close()

            If n = 0 Then
                Throw New Exception("No results found in hisfile: " & Path)
            Else
                Avg = Sum / n
            End If
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try


    End Function

    Public Function ReadStochasticResultsFromMemoryStream(ByRef hisReader As BinaryReader, ByVal location As String, ByVal PartOfParameterName As String, ByRef MaxInLastTimeStep As Boolean, ByRef Min As Double, ByRef tsMin As Long, ByRef Max As Double, ByRef tsMax As Long, ByRef Avg As Double, Optional ByVal SkipFirstPercentage As Integer = 0) As Boolean
        '=====================================================================================================================
        'this function will read the results of a stochastic run from memory. 
        'This should be MUCH faster than reading it from file
        'Therefore the prerequesits for this routine are that the file has already entirely been read to the memory stream
        'Also we assume that the hisfile header has already been read separately and is present in the variable hisfileheader
        'see also the function ReadFromMemory
        '=====================================================================================================================

        Try
            'Author: code courtesy of Deltares (Delft, The Netherlands) via mr. Klaas-Jan van Heeringen
            'Adjusted by Siebe Bosch on 28-8-2014
            'Description: Reads statistics from a hisfile in memory and places them in a clsSobekTable class instance
            Dim Par As String = "", ParFound As Boolean = False

            Dim n As Long, Sum As Double, Val As Double
            Max = -999999999
            Min = 999999999
            Avg = 0
            MaxInLastTimeStep = False

            'we assume that t
            'if location index not found, it could be a long ID. Check the .hia-file.
            Dim locationIndex As Integer = hisFileHeader.Locations.IndexOf(location)
            If locationIndex < 0 Then
                Dim myLongLoc As clsLongLocation = HIA.GetByID(location)
                If Not myLongLoc Is Nothing Then
                    locationIndex = myLongLoc.Num
                Else
                    Return False
                End If
            End If

            'look for the right parameter index
            For Each Par In hisFileHeader.parameters
                If InStr(Par.Trim.ToUpper, PartOfParameterName.Trim.ToUpper, CompareMethod.Text) > 0 Then
                    ParFound = True
                    Exit For
                End If
            Next

            'if parameter index not found, then use the first parameter in the hisfile
            If Not ParFound Then
                Par = hisFileHeader.parameters.Item(0)
                Me.Setup.Log.AddError("No parameter in hisfile found containing " & PartOfParameterName & "; first parameter was used.")
            End If

            Dim parameterIndex As Integer = hisFileHeader.parameters.IndexOf(Par)
            'If parameterIndex < 0 Then parameterIndex = HIA.longParameters.Item(Par)

            'read the data from the memory stream, using a streamreader
            Dim bitPosition = (locationIndex * hisFileHeader.parameters.Count + parameterIndex) * 4
            Dim [step] As Integer = 0
            Dim [nsteps] As Integer = (ms.Length - hisFileHeader.StreamStartDataPosition) / hisFileHeader.StreamDataBlockSize
            ms.Position = hisFileHeader.StreamStartDataPosition

            While ms.Position <= (ms.Length - hisFileHeader.StreamDataBlockSize)
                hisReader.ReadInt32()
                Dim block = hisReader.ReadBytes(hisFileHeader.StreamDataBlockSize - 4)

                If ([step] + 1) / [nsteps] * 100 >= SkipFirstPercentage Then
                    n += 1
                    Val = Convert.ToDouble(BitConverter.ToSingle(block, bitPosition))
                    Sum += Val
                    If Val < Min Then
                        Min = Val
                        tsMin = [step]
                    End If
                    If Val > Max Then
                        Max = Val
                        tsMax = [step]
                    End If
                End If

                [step] += 1

            End While

            If tsMax = [nsteps] - 1 Then MaxInLastTimeStep = True
            If n = 0 Then
                Throw New Exception("No results found in hisfile: " & Path)
            Else
                Avg = Sum / n
            End If

            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try


    End Function


    Public Function ReadHisHeader(Optional ByVal UpdateProgressbar As Boolean = False) As stHisFileHeader

        hisFileHeader = New stHisFileHeader
        HIA.Read()

        Dim myLocation As String

        If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("Reading his header...", 0, 10, True)
        If binaryReader Is Nothing Then binaryReader = New BinaryReader(System.IO.File.OpenRead(Path))

        binaryReader.BaseStream.Position = 0

        'read first 3 lines a 40 char: not needed
        binaryReader.ReadChars(120)

        'line with starttime and unit 
        Dim charArrayLine4 = binaryReader.ReadChars(40)
        Dim startTime = GetDateTimeLine4(charArrayLine4)

        'line with starttime and unit
        Dim timeStepUnitValue = GetTimeStepUnitValueLine4(charArrayLine4)

        'line with starttime and unit
        Dim timeStepUnit = GetTimeStepUnitLine4(charArrayLine4)

        'nparameters,nLocations
        Dim nparameters = binaryReader.ReadInt32()
        Dim nLocations = binaryReader.ReadInt32()

        'lst parameters
        If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("Reading his parameters...", 0, nparameters, True)
        hisFileHeader.parameters = New List(Of String)(nparameters)
        For i As Long = 0 To nparameters - 1
            If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("", i, nparameters)
            hisFileHeader.parameters.Add(New String(binaryReader.ReadChars(20)).Trim())
        Next

        'lst arguments (locations)
        If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("Reading his locations...", 0, nLocations, True)
        hisFileHeader.Locations = New List(Of String)(nLocations)
        For i As Integer = 0 To nLocations - 1
            If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("", i, nLocations)
            binaryReader.ReadInt32()
            ' loc nummer: not needed
            myLocation = New String(binaryReader.ReadChars(20)).Trim()
            If HIA.LongLocations.ContainsKey(i) Then
                myLocation = HIA.LongLocations.Item(i).ID
            End If
            hisFileHeader.Locations.Add(myLocation)
        Next

        'Timesteps (int = deltaTime)
        hisFileHeader.TimeSteps = New List(Of DateTime)()

        'deltaT as integer -> sizeof (int)
        Dim timeStepSize As Integer = 4 + (nLocations * nparameters * 4)

        hisFileHeader.StreamStartDataPosition = CInt(binaryReader.BaseStream.Position)
        hisFileHeader.StreamDataBlockSize = timeStepSize

        'set datetimes
        If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("Reading his date/times...", 0, binaryReader.BaseStream.Length, True)
        Dim blockSizeInBytes As Integer = timeStepSize - 4
        While binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - timeStepSize
            If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("", binaryReader.BaseStream.Position, binaryReader.BaseStream.Length)
            Dim timeStepValue = binaryReader.ReadInt32()
            hisFileHeader.TimeSteps.Add(startTime + GetTimeStepSpan(timeStepValue, timeStepUnitValue, timeStepUnit))
            binaryReader.ReadBytes(blockSizeInBytes)
        End While
        If UpdateProgressbar Then Me.Setup.GeneralFunctions.UpdateProgressBar("Hisfile header read.", 0, 10, True)
        Return hisFileHeader
    End Function

    Private Function GetDateTimeLine4(ByVal chars As Char()) As DateTime

        'T0: 1995.01.01 00:00:00  (scu=       1s)
        '0123456789012345678901234567890123456789
        'int year = int.Parse(new string(new[] { chars[4], chars[5], chars[6], chars[7] }));
        Dim year As Integer = Integer.Parse(chars(4) & chars(5) & chars(6) & chars(7))
        Dim month As Integer = Integer.Parse(chars(9) & chars(10))
        Dim day As Integer = Integer.Parse(chars(12) & chars(13))
        Dim hours As Integer = Integer.Parse(chars(15) & chars(16))
        Dim minutes As Integer = Integer.Parse(chars(18) & chars(19))
        Dim seconds As Integer = Integer.Parse(chars(21) & chars(22))
        Return New DateTime(year, month, day, hours, minutes, seconds)
    End Function

    Private Function GetTimeStepUnitValueLine4(ByVal chars As Char()) As Integer
        'T0: 1995.01.01 00:00:00  (scu=       1s)
        '0123456789012345678901234567890123456789
        Return Integer.Parse(New String(chars(30) & chars(31) & chars(32) & chars(33) & chars(34) & chars(35) & chars(36) & chars(37)))

        'oorspronkelijke C#-code:
        'Return Integer.Parse(New String(chars(30), chars(31), chars(32), chars(33), chars(34), chars(35),	chars(36), chars(37)}))
    End Function

    Private Function GetTimeStepUnitLine4(ByVal chars As Char()) As String
        'T0: 1995.01.01 00:00:00  (scu=       1s)
        '0123456789012345678901234567890123456789
        Return chars(38)

        'oorspronkelijke C#-code:
        'Return New String(New () {chars(38)})
    End Function

    Private Function GetTimeStepSpan(ByVal timeStepValue As Integer, ByVal timeStepUnitValue As Integer, ByVal timeStepUnit As String) As TimeSpan
        Select Case timeStepUnit.ToLower()
            Case "s"
                'siebe: with too large timespan using simply timestepunitvalue * timestepvalue in "seconds" won't work because of overflow
                Return New TimeSpan(0, timeStepUnitValue / 60 * timeStepValue, 0)
            Case "m"
                Return New TimeSpan(0, timeStepUnitValue * timeStepValue, 0)
            Case "h"
                Return New TimeSpan(timeStepUnitValue * timeStepValue, 0, 0)
            Case "d"
                Return New TimeSpan(timeStepUnitValue * timeStepValue, 0, 0, 0)
            Case Else
                Throw New ArgumentException(timeStepUnit & " is not supported as time unit argument")
        End Select
    End Function

    Private Function Open(ByVal Path As String, Optional ByVal ReadHeader As Boolean = True, Optional ByVal UpdateProgressBar As Boolean = False) As Boolean
        Try
            Close()
            fileStream = New FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            'FileStream = File.OpenRead(Path)
            binaryReader = New BinaryReader(fileStream)
            If ReadHeader Then hisFileHeader = ReadHisHeader(UpdateProgressBar)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub Close()
        If fileStream IsNot Nothing Then
            fileStream.Close()
            binaryReader.Close()
            fileStream = Nothing
            State = GeneralFunctions.enmFileState.Closed
        End If
    End Sub

    ''' <summary>
    ''' See <see cref="System.IDisposable.Dispose"/> for more information.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If Not disposed Then
            Dispose(True)
            GC.SuppressFinalize(Me)
        End If
    End Sub

    ''' <summary>
    ''' Called when the object is being disposed or finalized.
    ''' </summary>
    ''' <param name="disposing">True when the object is being disposed (and therefore can
    ''' access managed members); false when the object is being finalized without first
    ''' having been disposed (and therefore can only touch unmanaged members).</param>
    Protected Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            Close()
        End If

        disposed = True
    End Sub

    Public Structure stHisFileHeader
        Public Path As String
        Public TimeSteps As List(Of DateTime)
        Public Locations As List(Of String)
        Public parameters As List(Of String)
        Public StreamStartDataPosition As Integer
        Public StreamDataBlockSize As Integer
    End Structure

End Class

Public Class HisDataRow
    Public LocationName As String
    Public parameter As String
    Public TimeStep As DateTime
    Public Value As Double
End Class

