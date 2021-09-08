Imports System.IO
Imports STOCHLIB.General

Public Class clsTimeseriesTextFile
    'this class describes multiple timeseries inside one delimited textfile format
    'it contains methods to read, write and proess these files
    'Key aspect is that a timeseries needs at least: ID, Parameter, Date/Time and Value
    'The class supports the absence of the ID, Parameter and Date/Time columns in the text file and offers replacement with constant values

    Private Setup As clsSetup

    Friend Path As String
    Friend HasHeader As Boolean
    Public Delimiter As String

    Private TableName As String
    Private SeriesIDField As String
    Private LocationIDFIeld As String
    Private ParameterField As String
    Private DateField As String
    Private ValuesField As String

    'timeseries csv's typically have four fields (or columns if you will):
    'ID, parameter, date/time and value
    Private SeriesID As String
    Public Fields As New Dictionary(Of GeneralFunctions.enmDataFieldType, clsDataField)           'a dictionary of actually used datafields where the key = the datafield type
    Public Columns As New Dictionary(Of Integer, clsDataField)                                    'a list of all columns in the textfile, with the column index number as key

    'we will store the results in a separate object for each ID & Parameter combination
    Public ContentRaw As List(Of List(Of Object))               'raw content of the file in memory (after splitting by delimiter)
    Public ContentProcessed As New Dictionary(Of GeneralFunctions.StrTimeSeries, clsTimeSeries)        'structure: item1 = ID, item2 = Parameter

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String, ByVal myDelimiter As String)
        Setup = mySetup
        Path = myPath
        Delimiter = myDelimiter
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String, ByVal myDelimiter As String, myTableName As String, mySeriesIDField As String, myLocationIDField As String, myParameterField As String, myDateField As String, myValuesField As String)
        Setup = mySetup
        Path = myPath
        Delimiter = myDelimiter
        TableName = myTableName
        SeriesIDField = mySeriesIDField
        LocationIDFIeld = myLocationIDField
        ParameterField = myParameterField
        DateField = myDateField
        ValuesField = myValuesField
    End Sub

    Public Sub setSeriesID(myID As String)
        SeriesID = myID
    End Sub

    Public Function getSeriesID() As String
        Return SeriesID
    End Function

    Public Function GetField(FieldIdx As Integer) As clsDataField
        Return Fields(FieldIdx)
    End Function

    Public Function AddField(myField As clsDataField) As Integer
        Fields.Add(myField.FieldType, myField)
    End Function

    Public Function Getcolumnslist() As List(Of String)
        Dim myList As New List(Of String)
        For Each myColumn As clsDataField In Columns.Values
            myList.Add(myColumn.ID)
        Next
        Return myList
    End Function

    Public Function ReadContent() As Boolean ' List(Of List(Of Object))
        Try
            'this function returns the content of the csv file in a list-of-list format
            'highest order list = rows, the nested list = columns inside each row
            'it uses the Object type for the variables so it's very flexible yet expensive in terms of memory use
            ContentRaw = New List(Of List(Of Object))
            Dim RowContent As New List(Of Object)
            Dim myRow As String
            Using myReader As New StreamReader(Path)
                If HasHeader Then myReader.ReadLine() 'skip the header
                While Not myReader.EndOfStream
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", myReader.BaseStream.Position, myReader.BaseStream.Length)
                    myRow = myReader.ReadLine
                    RowContent = New List(Of Object)
                    While Not myRow.Length = 0
                        RowContent.Add(Setup.GeneralFunctions.ParseString(myRow, Delimiter))
                    End While
                    ContentRaw.Add(RowContent)
                End While
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ReadContent of class clsTimeseriesTextFile.")
            Return False
        End Try
    End Function

    Public Function ProcessContentAndStoreInMemory() As Boolean
        'this function processes the raw contents of the csv-file and stores the resulting structure + data in-memory
        Try
            Dim ID As String, SeriesID As String, Par As String, SeriesMeta As GeneralFunctions.StrTimeSeries
            Dim myDate As DateTime, myValue As Object
            Dim Series As clsTimeSeries

            Me.Setup.GeneralFunctions.UpdateProgressBar("Processing text file content...", 0, 10, True)
            For r = 0 To ContentRaw.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r, ContentRaw.Count)

                'get the Series ID, ID and Parameter name
                SeriesID = getValue(r, Fields.Item(GeneralFunctions.enmDataFieldType.SERIESID))
                ID = getValue(r, Fields.Item(GeneralFunctions.enmDataFieldType.LOCATIONID))
                Par = getValue(r, Fields.Item(GeneralFunctions.enmDataFieldType.PARAMETER))

                'retrieve the timeseries that the current ID and Par belong to
                'create the key for our dictionary
                SeriesMeta.ID = SeriesID
                SeriesMeta.LOCATIONID = ID
                SeriesMeta.PARAMETER = Par

                'get or add a timeseries to store
                If Not ContentProcessed.ContainsKey(SeriesMeta) Then
                    Series = New clsTimeSeries(Me.Setup, SeriesID, ID, Par)
                    ContentProcessed.Add(SeriesMeta, Series)
                Else
                    Series = ContentProcessed.Item(SeriesMeta)
                End If

                myDate = getValue(r, Fields.Item(GeneralFunctions.enmDataFieldType.DATETIME))
                myValue = getValue(r, Fields.Item(GeneralFunctions.enmDataFieldType.VALUE))
                If IsNumeric(myValue) AndAlso Not myDate = Nothing Then
                    Series.addRecord(myDate, myValue)
                End If

            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Data has been successfully written to the database.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetValueByColumns(RowIdx As Integer, ColIdx As Integer) As Object
        Return ContentRaw(RowIdx)(ColIdx)
    End Function

    Public Function getValue(RowIdx As Integer, DataField As clsDataField, Optional ByVal tsIdx As Long = 0) As Object
        'this function returns the value for a given row and field in our delimited textfile
        'please notice that it is possible that certain fields don't exist in the textfile. In those cases we'll revert to the given constant value
        Dim myDate As DateTime

        Try
            Select Case DataField.FieldType
                Case Is = GeneralFunctions.enmDataFieldType.DATETIME
                    'we are dealing with a date, so return it as a formatted date
                    If DataField.TextFileColIdx >= 0 Then
                        If Not Me.Setup.GeneralFunctions.DateFromFormattedString(ContentRaw(RowIdx)(DataField.TextFileColIdx), DataField.DateFormatting, myDate) Then
                            Throw New Exception("Error retrieving a valid date From String: " & ContentRaw(RowIdx)(DataField.TextFileColIdx))
                        Else
                            Return myDate
                        End If
                    Else
                        Return DataField.StartDate.AddSeconds(DataField.TimestepSeconds * tsIdx)
                    End If
                Case Else
                    'return the value 'as is'
                    If DataField.TextFileColIdx >= 0 Then
                        Return ContentRaw(RowIdx)(DataField.TextFileColIdx)
                    Else
                        Return DataField.ConstantValue
                    End If
            End Select


            If DataField.TextFileColIdx >= 0 Then
                Return ContentRaw(RowIdx)(DataField.TextFileColIdx)
            ElseIf DataField.FieldType = GeneralFunctions.enmDataFieldType.DATETIME Then
                Return DataField.StartDate.AddSeconds(DataField.TimestepSeconds * tsIdx)
            Else
                Return DataField.ConstantValue
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try


    End Function

    Public Function PopulateComboBoxByHeader(ByRef cmbBox As Windows.Forms.ComboBox) As Boolean
        cmbBox.Items.Clear()
        Dim i As Integer
        For i = 0 To Columns.Count - 1
            cmbBox.Items.Add(Columns(i).ID)
        Next
    End Function

    Public Function ReadColumns(ContainsHeader As Boolean) As Boolean
        Try
            HasHeader = ContainsHeader
            Using myReader As New System.IO.StreamReader(Path)
                Dim myHeader As String = myReader.ReadLine
                Dim i As Integer = -1
                While Not myHeader.Length = 0
                    i += 1
                    If ContainsHeader Then
                        Columns.Add(i, New clsDataField(Setup.GeneralFunctions.ParseString(myHeader, Delimiter)))
                    Else
                        Setup.GeneralFunctions.ParseString(myHeader, Delimiter)
                        Columns.Add(i, New clsDataField("Column" & (i + 1).ToString))
                    End If
                End While
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetFieldIdx(Name As String) As Integer
        For i = 0 To Fields.Count - 1
            If Fields.Values(i).ID.Trim.ToUpper = Name.Trim.ToUpper Then Return i
        Next
        Return -1
    End Function

    Public Function GetColumnIdx(Name As String) As Integer
        For i = 0 To Columns.Count - 1
            If Columns.Values(i).ID.Trim.ToUpper = Name.Trim.ToUpper Then Return i
        Next
        Return -1
    End Function

    Public Function ReadSingles() As DataTable
        Dim dt As New DataTable
        Dim colIdx As Integer, rowIdx As Integer = -1
        Dim myRow As String, myVal As Single
        Using myReader As New StreamReader(Path)
            myRow = myReader.ReadLine
            colIdx = -1
            rowIdx += 1
            dt.Rows.Add()
            While Not myRow.Length = 0
                colIdx += 1
                If dt.Columns.Count < colIdx + 1 Then dt.Columns.Add(colIdx, System.Type.GetType("System.Single"))
                myVal = Setup.GeneralFunctions.ParseString(myRow, ",")
                dt.Rows(rowIdx)(colIdx) = myVal
            End While
        End Using
        Return dt
    End Function
    Public Sub WriteToDatabase()
        'this function writes each timeseries as extracted from the textfile to a database.
        Me.Setup.GeneralFunctions.UpdateProgressBar("Writing text file contents to the database...", 0, 10, True)
        Dim i As Integer
        For Each mySeries As STOCHLIB.clsTimeSeries In ContentProcessed.Values
            i += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, ContentProcessed.Count, True)
            mySeries.WriteToDatabase(TableName, SeriesIDField, LocationIDFIeld, ParameterField, DateField, ValuesField)
        Next
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
    End Sub

    Public Function WriteToDatabaseCustom(ByRef con As SQLite.SQLiteConnection, cols As Dictionary(Of String, clsDatabaseColumn), Multiplier As Double, ByVal TableName As String, ByVal Parameter As String, ByRef prProgress As Windows.Forms.ProgressBar, Optional ByVal Periodicity As GeneralFunctions.enmPeriodicity = GeneralFunctions.enmPeriodicity.NONE) As Boolean
        'this function writes the entire content of a csv file to the database
        'note: the dictionary cols should consist of: Key = CSV column name and Value = Database column name
        'the dictionary colTypes should consist of: Key = Database column name, Value=IsText? (true/false)
        Dim myLine As String, i As Integer, myQuery1 As String = "", myQuery2 As String = "", myQuery As String, myVal As Object
        Dim ColNameDB As String
        Dim col As clsDatabaseColumn

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Importing data from csv file.", 0, 10, True)

            ''remove all old data
            'myQuery = "DELETE FROM " & TableName & ";"
            'Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)

            Using csvReader As New StreamReader(Path)
                myLine = csvReader.ReadLine     'read the first line beause that's the header
                Dim Length As Long = csvReader.BaseStream.Length
                Dim pos As Long = 0, LineNum As Integer = 0
                Me.Setup.GeneralFunctions.UpdateCustomProgressBar(prProgress, pos, Length, True)
                While Not csvReader.EndOfStream
                    myLine = csvReader.ReadLine
                    LineNum += 1

                    'parse this line to a list of objects
                    Dim myList As New List(Of String)
                    myList = Me.Setup.GeneralFunctions.ParseStringToList(myLine, Delimiter)

                    Me.Setup.GeneralFunctions.UpdateCustomProgressBar(prProgress, csvReader.BaseStream.Position, Length, False)
                    myQuery1 = "INSERT INTO " & TableName & " ("
                    myQuery2 = "VALUES ("

                    'error handling
                    If myList.Count < cols.Count Then
                        Me.Setup.Log.AddError("Error reading line number " & LineNum & " from " & Path & ".")
                        Continue While
                    End If

                    For i = 0 To cols.Count - 1
                        col = cols.Values(i)

                        'first retrieve the database column in which the value must be inserted
                        ColNameDB = col.ID
                        myQuery1 &= ColNameDB & ","

                        'then retieve the value we'll need to insert
                        If col.SourceDataColumnIdx >= 0 Then
                            myVal = myList(col.SourceDataColumnIdx)
                        Else
                            myVal = col.ConstantVal
                        End If

                        'if column is text type, add quotes around the value. if it's a date type, format it according to the required formatting
                        Select Case col.DataType
                            Case Is = GeneralFunctions.enmSQLiteDataType.SQLITETEXT
                                myQuery2 &= "'" & col.Prefix & myVal & "'"
                            Case Else
                                If Not IsNumeric(myVal) Then
                                    Me.Setup.Log.AddError("Error: value in row " & LineNum & " and column " & col.ID & " is not numeric: " & myVal & ".")
                                    Continue While
                                End If
                                myQuery2 &= myVal * Multiplier
                        End Select
                        myQuery2 &= ","
                    Next

                    'add the parameter and periodicity
                    myQuery1 &= "PARAMETER, PERIODICITY"
                    myQuery2 &= "'" & Parameter & "','" & Setup.GeneralFunctions.PeriodicityEnumToString(Periodicity) & "'"

                    myQuery1 &= ") "
                    myQuery2 &= ");"

                    'finalize the query and execute it
                    myQuery = myQuery1 & myQuery2
                    If Not Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False) Then Throw New Exception("Error writing csv data to database.")
                End While

            End Using
            con.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error writing csv content to database.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        Finally
            Me.Setup.Log.write(Me.Setup.Settings.ExportDirRoot & "\logfile.txt", True)
        End Try

    End Function


End Class
