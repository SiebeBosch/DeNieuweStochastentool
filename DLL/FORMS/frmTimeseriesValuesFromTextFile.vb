Imports STOCHLIB.General

Public Class frmTimeseriesValuesFromTextFile
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        dlgOpenFile.Filter = "Text files|*.csv;*.txt"
        dlgOpenFile.ShowDialog()
        txtCSVFile.Text = dlgOpenFile.FileNames(0)
        For i = 1 To dlgOpenFile.FileNames.Count - 1
            txtCSVFile.Text &= ";" & dlgOpenFile.FileNames(i)
        Next
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            Me.Cursor = System.Windows.Forms.Cursors.WaitCursor

            My.Settings.DatePickerValue = pckStartDate.Value
            My.Settings.Save()

            Dim ID As String = txtID.Text
            Dim Par As String = txtPar.Text
            Dim StartDate As DateTime = New DateTime(pckStartDate.Value.Year, pckStartDate.Value.Month, pckStartDate.Value.Day, pckStartDate.Value.Hour, 0, 0)
            Dim CurDate As DateTime = StartDate 'initialize the curdate variable
            Dim tsSeconds As Integer = txtTimestepSizeSeconds.Text
            Dim query As String
            Dim j As Integer = 0, k As Integer = 0

            'initialize the progress bar
            Me.Setup.SetProgress(prProgress, lblProgress)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading timeseries from text file. Please wait...", 0, 10, True)

            'remove all existing for the given location and parameter
            If chkRemoveExisting.Checked Then
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM TIMESERIES WHERE LOCATIONID='" & ID & "' AND PARAMETER='" & Par & "';")
            End If

            query = "INSERT INTO TIMESERIES (LOCATIONID, DATEANDTIME, PARAMETER, DATAVALUE) VALUES ('" & ID & "',@D1,'" & Par & "',@D2);"

            'create a list of input files
            Dim InputFiles As New List(Of String)
            Dim StreamReaders As New List(Of System.IO.StreamReader)
            Dim InputFileString As String = txtCSVFile.Text
            Dim StreamLength As Long
            While Not InputFileString = ""
                InputFiles.Add(Me.Setup.GeneralFunctions.ParseString(InputFileString, ";"))
                StreamReaders.Add(New System.IO.StreamReader(InputFiles.Item(InputFiles.Count - 1)))
                StreamLength = StreamReaders.Item(0).BaseStream.Length
            End While

            'implement bulk insert to speed things up
            Me.Setup.SqliteCon.Open()
            Using cmd As New SQLite.SQLiteCommand

                'initialize our sqlite connection, its command and its parameters
                cmd.Connection = Me.Setup.SqliteCon
                cmd.Parameters.Add("@D1", DbType.String)        'create a parameter for date
                cmd.Parameters.Add("@D2", DbType.Double)        'create a parameter for value
                cmd.CommandText = query                         'set the query. We'll set the values further on

                'we read the textfiles in one go
                Dim Dates As New List(Of String)
                Dim Values As New List(Of String)

                'read the values timestep by timestep
                Dim Done As Boolean = False
                Dim curVal As Double, curSum As Double = 0
                Dim StreamPosition As Long
                While Not Done
                    curSum = 0
                    For Each myReader As System.IO.StreamReader In StreamReaders
                        curVal = Me.Setup.GeneralFunctions.ForceNumeric(myReader.ReadLine(), "Value", 0, GeneralFunctions.enmMessageType.None)
                        curSum += curVal
                        If myReader.EndOfStream Then Done = True
                        StreamPosition = myReader.BaseStream.Position
                    Next

                    j += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", StreamPosition, StreamLength)
                    Dates.Add(CurDate.ToString("yyyy-MM-dd HH:mm:ss"))
                    Values.Add(curSum)

                    'write the results to our SQLite database
                    'we work in blocks of 10000 lines
                    If j >= 10000 Then
                        j = 0                   'reset the counter for the next block
                        'we completed another block. Write it to the database
                        Using transaction = Me.Setup.SqliteCon.BeginTransaction
                            For k = 0 To Values.Count - 1
                                cmd.Parameters(0).Value = Dates(k) ' = Me.Setup.GeneralFunctions.ForceNumeric(Values(k), "", Double.NaN)
                                cmd.Parameters(1).Value = Values(k) ' = Me.Setup.GeneralFunctions.ForceNumeric(Values(k), "", Double.NaN)
                                cmd.ExecuteNonQuery()
                            Next
                            transaction.Commit()
                        End Using

                        'clear the lists of dates and values
                        Dates = New List(Of String)
                        Values = New List(Of String)
                    End If

                    CurDate = CurDate.AddSeconds(tsSeconds)
                End While

            End Using

            'close all readers
            For Each myReader As System.IO.StreamReader In StreamReaders
                myReader.Close()
            Next


            MsgBox("Import completed successfully.")
            Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
            MsgBox("Error while importing text file.")
        Finally
            Me.Setup.SqliteCon.Close()
            Me.Cursor = System.Windows.Forms.Cursors.Default
        End Try

    End Sub

    Private Sub frmTimeseriesValuesFromTextFile_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If My.Settings.DatePickerValue > DateTime.MinValue AndAlso My.Settings.DatePickerValue < DateTime.MaxValue Then
            pckStartDate.Value = My.Settings.DatePickerValue
        End If
    End Sub
End Class