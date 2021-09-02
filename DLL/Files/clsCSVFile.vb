Imports System.IO
Imports STOCHLIB.General

Public Class clsCSVFile
    Private Setup As clsSetup

    Friend Path As String
    Friend ContainsHeader As Boolean
    Public Delimiter As String
    Public Columns As Dictionary(Of Integer, clsDataField)       'key=column index. contains a description of each column inside the csv file
    Public UsedFields As Dictionary(Of STOCHLIB.GeneralFunctions.enmDataFieldType, clsDataField) 'key= the datafieldtype.
    Public RemoveBoundingQuotesFromStrings As Boolean

    Public Content As List(Of List(Of Object))                   'rows of columns
    Public DataTables As Dictionary(Of String, DataTable)        'for multiple datatables in one csv

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String, ByVal myHasHeader As Boolean, ByVal myDelimiter As String)
        Setup = mySetup
        Path = myPath
        ContainsHeader = myHasHeader
        Delimiter = myDelimiter
        Columns = New Dictionary(Of Integer, clsDataField)
    End Sub

    Public Function GetColumnsList() As List(Of String)
        Dim myList As New List(Of String)
        For Each myCol As clsDataField In Columns.Values
            myList.Add(myCol.ID)
        Next
        Return myList
    End Function

    Public Function ReadToDatatables(RequiredFields As Dictionary(Of STOCHLIB.GeneralFunctions.enmDataFieldType, clsDataField), IDFieldIdx As Integer) As Boolean
        Try
            Dim myLine As String
            DataTables = New Dictionary(Of String, DataTable)
            Dim myDt As DataTable, ID As String
            Dim tmpDt As New DataTable
            Dim myRecord As List(Of String)

            'here we will take over the list of required fields and assign them as the used fields
            UsedFields = RequiredFields

            Using csvReader As New StreamReader(Path)
                If ContainsHeader Then myLine = csvReader.ReadLine                 'parse this header line

                'read the csv content to datatables, by ID
                While Not csvReader.EndOfStream
                    myLine = csvReader.ReadLine

                    'parse the current record
                    myRecord = New List(Of String)
                    While Not myLine = ""
                        myRecord.Add(Me.Setup.GeneralFunctions.ParseString(myLine, Delimiter))
                    End While

                    'get the ID for the current record
                    'and get/add the appropriate datatable for this object
                    ID = myRecord(UsedFields.Values(IDFieldIdx).TextFileColIdx)
                    If Not DataTables.ContainsKey(ID.Trim.ToUpper) Then
                        myDt = New DataTable
                        For Each myField As clsDataField In UsedFields.Values
                            myDt.Columns.Add(myField.ID, myField.DataType)
                        Next
                        DataTables.Add(ID.Trim.ToUpper, myDt)
                    End If

                    'retrieve the CSV value and store it in the appropriate datatable
                    myDt = DataTables.Item(ID.Trim.ToUpper)
                    Dim row As DataRow = myDt.NewRow()
                    For i = 0 To UsedFields.Count - 1
                        row(i) = myRecord(UsedFields.Values(i).TextFileColIdx)
                    Next
                    myDt.Rows.Add(row)

                End While
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function ReadToSQLite(ByRef con As SQLite.SQLiteConnection, TableName As String, RequiredFields As Dictionary(Of String, clsDataField)) As Boolean
        Try
            'this function read the contents of a CSV file to a given SQLite table
            Dim myLine As String
            Dim myRecord As List(Of String)
            Dim queryStart As String

            'initialize the progress bar
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading CSV file content to database...", 0, 10, True)

            'build up the insert query
            queryStart = "INSERT INTO " & TableName & "("
            For i = 0 To RequiredFields.Count - 2
                queryStart &= RequiredFields.Keys(i) & ","
            Next
            queryStart &= RequiredFields.Keys(RequiredFields.Keys.Count - 1) & ") VALUES ("

            'bulk insert our excedance table
            If Not con.State = ConnectionState.Open Then con.Open()
            Dim myCmd As New SQLite.SQLiteCommand
            myCmd.Connection = con
            Using transaction = con.BeginTransaction

                Dim query As String
                Using csvReader As New StreamReader(Path)
                    If ContainsHeader Then myLine = csvReader.ReadLine                 'parse this header line
                    'read the csv content to datatables, by ID
                    While Not csvReader.EndOfStream
                        myLine = csvReader.ReadLine
                        query = queryStart
                        Me.Setup.GeneralFunctions.UpdateProgressBar("", csvReader.BaseStream.Position, csvReader.BaseStream.Length)

                        'parse the current record
                        myRecord = New List(Of String)
                        While Not myLine = ""
                            myRecord.Add(Me.Setup.GeneralFunctions.ParseString(myLine, Delimiter))
                        End While

                        'for each field, look its column from the text file up and insert the value
                        For i = 0 To RequiredFields.Count - 1
                            If RequiredFields.Values(i).SQLiteFieldType = GeneralFunctions.enmSQLiteDataType.SQLITETEXT Then
                                query &= "'" & myRecord.Item(RequiredFields.Values(i).TextFileColIdx) & "'"
                            Else
                                query &= myRecord.Item(RequiredFields.Values(i).TextFileColIdx) & ""
                            End If
                            If i < RequiredFields.Count - 1 Then query &= ","
                        Next
                        query &= ");"
                        myCmd.CommandText = query
                        myCmd.ExecuteNonQuery()
                    End While
                End Using

                'insert the resulta for all return periods at once
                transaction.Commit() 'this is where the bulk insert is finally executed.
            End Using
            con.Close()

            Me.Setup.GeneralFunctions.UpdateProgressBar("Data import complete", 0, 10, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function getValue(RowIdx As Integer, ColIdx As Integer) As Object
        Return Content(RowIdx)(ColIdx)
    End Function

    Public Function PopulateDataGridViewComboBoxColumnByHeader(ByRef myCol As System.Windows.Forms.DataGridViewComboBoxColumn) As Boolean
        myCol.Items.Clear()
        For i = 0 To Columns.Count - 1
            myCol.Items.Add(Columns(i).ID)
        Next
    End Function

    Public Function PopulateComboBoxByHeader(ByRef cmbBox As Windows.Forms.ComboBox) As Boolean
        cmbBox.Items.Clear()
        Dim i As Integer
        For i = 0 To Columns.Count - 1
            cmbBox.Items.Add(Columns(i).ID)
        Next
    End Function

    Public Function ReadHeader() As Boolean
        Try
            Using myReader As New System.IO.StreamReader(Path)
                Dim myHeader As String = myReader.ReadLine
                Dim i As Integer = -1
                While Not myHeader.Length = 0
                    i += 1
                    Columns.Add(i, New clsDataField(Setup.GeneralFunctions.ParseString(myHeader, Delimiter)))
                End While
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetFieldIdx(Name As String) As Integer
        For i = 0 To Columns.Count - 1
            If Columns.Values(i).ID.Trim.ToUpper = Name.Trim.ToUpper Then Return i
        Next
        Return -1
    End Function

    Public Function ReadContent() As List(Of List(Of Object))
        'this function returns the content of the csv file in a list-of-list format
        'it uses the Object type for the variables so it's very flexible yet expensive in terms of memory use
        Content = New List(Of List(Of Object))
        Dim RowContent As New List(Of Object)
        Dim myRow As String
        Using myReader As New StreamReader(Path)
            If ContainsHeader Then myReader.ReadLine() 'skip the header
            If RemoveBoundingQuotesFromStrings Then
                While Not myReader.EndOfStream
                    myRow = myReader.ReadLine
                    RowContent = New List(Of Object)
                    While Not myRow.Length = 0
                        RowContent.Add(Setup.GeneralFunctions.RemoveBoundingQuotes(Setup.GeneralFunctions.ParseString(myRow, Delimiter)))
                    End While
                    Content.Add(RowContent)
                End While
            Else
                While Not myReader.EndOfStream
                    myRow = myReader.ReadLine
                    RowContent = New List(Of Object)
                    While Not myRow.Length = 0
                        RowContent.Add(Setup.GeneralFunctions.ParseString(myRow, Delimiter))
                    End While
                    Content.Add(RowContent)
                End While
            End If
        End Using
        Return Content
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

    Public Function WriteToDatabase(ByRef con As SQLite.SQLiteConnection, cols As Dictionary(Of String, clsDatabaseColumn), ByVal TableName As String, ByRef prProgress As Windows.Forms.ProgressBar) As Boolean
        'this function writes the entire content of a csv file to the database
        'note: the dictionary cols should consist of: Key = CSV column name and Value = Database column name
        'the dictionary colTypes should consist of: Key = Database column name, Value=IsText? (true/false)
        Dim myLine As String, i As Integer, myQuery1 As String = "", myQuery2 As String = "", myQuery As String, myVal As Object
        Dim ColNameCSV As String, ColNameDB As String, Done As Boolean, nDone As Integer
        Dim col As clsDatabaseColumn

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Importing data from csv file.", 0, 10, True)

            ''remove all old data
            'myQuery = "DELETE FROM " & TableName & ";"
            'Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)

            'v1.76: introduced a bulk insert for this type of data to speed up the process
            If Not con.State = ConnectionState.Open Then con.Open()
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = con
                Using transaction = Me.Setup.SqliteCon.BeginTransaction

                    Using csvReader As New StreamReader(Path)
                        myLine = csvReader.ReadLine     'read the first line beause that's the header
                        Dim Length As Long = csvReader.BaseStream.Length
                        Dim pos As Long = 0
                        Me.Setup.GeneralFunctions.UpdateCustomProgressBar(prProgress, pos, Length, True)

                        While Not csvReader.EndOfStream
                            myLine = csvReader.ReadLine
                            Me.Setup.GeneralFunctions.UpdateCustomProgressBar(prProgress, csvReader.BaseStream.Position, Length, False)
                            myQuery1 = "INSERT INTO " & TableName & " ("
                            myQuery2 = "VALUES ("
                            i = 0

                            nDone = 0
                            Done = False
                            While Not Done
                                myVal = Setup.GeneralFunctions.ParseString(myLine, Delimiter)
                                ColNameCSV = Columns.Item(i).ID
                                If cols.ContainsKey(ColNameCSV.Trim.ToUpper) Then
                                    col = cols.Item(ColNameCSV.Trim.ToUpper)

                                    ColNameDB = col.ID
                                    myQuery1 &= ColNameDB & ","

                                    'if column is text type, add quotes around the value
                                    Select Case col.DataType
                                        Case Is = GeneralFunctions.enmSQLiteDataType.SQLITETEXT
                                            myQuery2 &= "'" & cols.Item(ColNameCSV.Trim.ToUpper).Prefix & myVal & "'"
                                            'Case Is = GeneralFunctions.enmSQLDataType.SQLDATE
                                            '    'v1.72: this could be a problem!
                                            '    'cast the string by using the given formatting
                                            '    GeneralFunctions.ConvertoDateTime(myVal, myDate, col.DateFormatting)
                                            '    myQuery2 &= "'" & myDate & "'"
                                        Case Else
                                            'v1.76: auto-replacing comma by dot for numerical values
                                            myQuery2 &= Replace(myVal, ",", ".")    'make sure the decimal separator is a .
                                    End Select
                                    myQuery2 &= ","
                                    nDone += 1
                                End If
                                i += 1
                                If myLine = "" Then Done = True
                                If nDone = cols.Count Then Done = True

                            End While
                            myQuery1 = Left(myQuery1, myQuery1.Length - 1) & ") "
                            myQuery2 = Left(myQuery2, myQuery2.Length - 1) & ");"

                            'finalize the query and execute it
                            myQuery = myQuery1 & myQuery2
                            cmd.CommandText = myQuery
                            cmd.ExecuteNonQuery()

                            'If Not Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False) Then Throw New Exception("Error writing csv data to database.")
                        End While
                    End Using

                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using


            con.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error writing csv content to database.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


End Class
