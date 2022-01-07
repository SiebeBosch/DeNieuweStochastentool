Imports STOCHLIB.General

Public Class frmTextFileToSQLite
    Public CSV As clsCSVFile
    Private RequiredFields As Dictionary(Of String, clsDataField) 'use the fieldname as key
    Private Setup As clsSetup
    Private TableName As String
    Private SQLiteCon As SQLite.SQLiteConnection
    Public Sub New(ByRef mySetup As clsSetup, ByRef con As SQLite.SQLiteConnection, myTableName As String, myFields As Dictionary(Of String, clsDataField))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        SQLiteCon = con
        RequiredFields = myFields
        TableName = myTableName

    End Sub

    Private Sub frmTextFileToSQLite_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Setup.SetProgress(prProgress, lblProgress)

        'populate the various field types our CSV can have
        Me.Setup.GeneralFunctions.populateDataGridViewComboBoxColumnWithEnumNames(grFields.Columns(1), GetType(GeneralFunctions.enmSQLiteDataType), True)

        For Each myField As clsDataField In RequiredFields.Values
            grFields.Rows.Add()
            Dim myRow As Windows.Forms.DataGridViewRow = grFields.Rows(grFields.Rows.Count - 1)
            myRow.Cells(0).Value = myField.ID
            myRow.Cells(1).Value = myField.SQLiteFieldType.ToString
        Next


    End Sub

    Private Sub BtnRead_Click(sender As Object, e As EventArgs) Handles btnRead.Click
        Try
            'before we can actually read data from the csv file we'll need to set from which column the data needs to be extracted
            Dim i As Integer
            For i = 0 To grFields.Rows.Count - 1
                If Not RequiredFields.ContainsKey(grFields.Rows(i).Cells(0).Value) Then Throw New Exception("Error: matching required fields with the specification.")
                Dim myField As clsDataField = RequiredFields.Item(grFields.Rows(i).Cells(0).Value)
                myField.TextFileColIdx = CSV.GetFieldIdx(grFields.Rows(i).Cells(2).Value)
            Next

            'finally we will read the csv content to multiple in-memory datatables
            CSV.ReadToSQLite(SQLiteCon, TableName, RequiredFields)

        Catch ex As Exception
            MsgBox(ex.Message)
            MsgBox("Error reading CSV file. Please check all datatypes.")
        End Try

    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        dlgOpenFile.ShowDialog()
        txtCSVFile.Text = dlgOpenFile.FileName

        'let the user choose the delimiter
        Dim myForm As New frmPickDelimiter
        myForm.ShowDialog()

        If myForm.DialogResult = Windows.Forms.DialogResult.OK Then
            CSV = New clsCSVFile(Me.Setup, txtCSVFile.Text, myForm.ContainsHeader, myForm.Delimiter)

            'now we'll have to populate the datagridview with the available columns
            CSV.ReadHeader()

            'populate the dropdownbox with all available csv columns
            Dim FieldList As New List(Of String)
            For Each myField As clsDataField In CSV.Columns.Values
                FieldList.Add(myField.ID)
            Next
            Me.Setup.GeneralFunctions.populateDataGridViewComboBoxColumnFromList(grFields.Columns(2), FieldList)

        End If

    End Sub
End Class