Public Class frmTimeSeriesFromCSV
    Public dt As DataTable
    Public Setup As General.clsSetup
    Dim Header As New List(Of String)
    Dim CSV As clsTimeseriesTextFile
    Dim TableName As String
    Dim IDColumnName As String
    Dim DateColumnName As String
    Dim ValueColumnName As String

    Dim Parameter As String 'parameter as a constant can be provided by the calling function

    Public Sub New(ByRef mySetup As General.clsSetup, ByVal myTableName As String, ByVal myIDColumnName As String, myDateColumnName As String, myValueColumnName As String, myParameter As String)

        'this version of New() lets the user specify the table, the ID column the Date column and the values column and a constant Parameter name

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        TableName = myTableName
        IDColumnName = myIDColumnName
        DateColumnName = myDateColumnName
        ValueColumnName = myValueColumnName
        Parameter = myParameter

    End Sub

    Private Sub frmTimeSeriesFromCSV_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        txtDateFormatting.Text = My.Settings.DateFormatting
        Me.Setup.GeneralFunctions.PopulateComboBoxWithPeriodicityTypes(cmbPeriodicity)
    End Sub

    Private Sub btnRead_Click(sender As Object, e As EventArgs) Handles btnRead.Click
        My.Settings.DateFormatting = txtDateFormatting.Text
        My.Settings.Save()

        If chkRemoveExisting.Checked Then
            Dim query As String = "DELETE FROM " & TableName & ";"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Setup.SqLiteCon, query)
        End If

        Dim Columns As New Dictionary(Of String, STOCHLIB.clsDatabaseColumn) 'key=column name from csv, value=database column name!
        Columns.Add(cmbIDCol.Text.Trim.ToUpper, New STOCHLIB.clsDatabaseColumn(IDColumnName, "", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, cmbIDCol.SelectedIndex))
        Columns.Add(cmbDateCol.Text.Trim.ToUpper, New STOCHLIB.clsDatabaseColumn(DateColumnName, "", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, cmbDateCol.SelectedIndex, txtDateFormatting.Text))
        Columns.Add(cmbValCol.Text.Trim.ToUpper, New STOCHLIB.clsDatabaseColumn(ValueColumnName, "", GeneralFunctions.enmSQLiteDataType.SQLITEREAL, cmbValCol.SelectedIndex))

        'set the chosen periodicity
        Dim myPeriodicity As GeneralFunctions.enmPeriodicity
        myPeriodicity = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmPeriodicity), cmbPeriodicity.Text), GeneralFunctions.enmPeriodicity)

        'write the csv results to the database
        If Not CSV.WriteToDatabaseCustom(Setup.SqliteCon, Columns, txtMultiplier.Text, TableName, Parameter, prProgress, myPeriodicity) Then MsgBox("Error writing data to database.")

        Me.Close()
    End Sub

    Public Function ReadHeader(Path As String, Delimiter As String) As Boolean
        Dim myStr As String
        Try
            Using csvReader As New System.IO.StreamReader(Path)
                myStr = csvReader.ReadLine()
                Header = New List(Of String)
                While Not myStr.Length = 0
                    Header.Add(Setup.GeneralFunctions.ParseString(myStr, Delimiter))
                End While
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        dlgOpenFile.Filter = "Textfiles|*.csv;*.txt|All files|*.*"
        dlgOpenFile.ShowDialog()
        If System.IO.File.Exists(dlgOpenFile.FileName) Then

            'first prompt the user for the delimiter character
            Dim frmDelimiter As New frmPickDelimiter
            frmDelimiter.ShowDialog()

            'now populate the comboboxes
            If frmDelimiter.DialogResult = System.Windows.Forms.DialogResult.OK Then
                CSV = New clsTimeseriesTextFile(Me.Setup, dlgOpenFile.FileName, My.Settings.Delimiter)
                txtCSVFile.Text = dlgOpenFile.FileName
                If Not CSV.ReadColumns(True) Then MsgBox("Error reading CSV file header.")
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbIDCol)
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbDateCol)
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbValCol)
            End If
        End If
    End Sub

    Private Sub btnDateFormatting_Click(sender As Object, e As EventArgs) Handles btnDateFormatting.Click
        MsgBox("Date format e.g. yyyy/MM/dd hh:mm:ss. Note: month always in uppercase, minute in lowercase!")
    End Sub
End Class