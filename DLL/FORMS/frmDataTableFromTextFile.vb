Imports STOCHLIB.General


Public Class frmDataTableFromTextFile
    Public CSV As clsCSVFile
    Private RequiredFields As Dictionary(Of GeneralFunctions.enmDataFieldType, clsDataField)

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, myFields As Dictionary(Of GeneralFunctions.enmDataFieldType, clsDataField))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        RequiredFields = myFields

    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click

        'let the user select a csv file
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

    Private Sub btnRead_Click(sender As Object, e As EventArgs) Handles btnRead.Click
        Try
            'before we can actually read data from the csv file we'll need to know which fields to use
            Dim i As Integer = -1
            For Each myField As clsDataField In RequiredFields.Values
                i += 1
                myField.TextFileColIdx = CSV.GetFieldIdx(grFields.Rows(i).Cells(2).Value)
            Next

            'finally we will read the csv content to multiple in-memory datatables
            CSV.ReadToDatatables(RequiredFields, 0)

        Catch ex As Exception
            MsgBox(ex.Message)
            MsgBox("Error reading CSV file. Please check all datatypes.")
        End Try
    End Sub

    Private Sub frmDataTableFromCSV_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'populate the combobox column of the datagridview with the fields as required by the user
        'Me.Setup.GeneralFunctions.populateDataGridViewComboBoxColumnFromList(grFields.Columns(0), RequiredFields)

        'populate the various field types our CSV can have
        Me.Setup.GeneralFunctions.populateDataGridViewComboBoxColumnWithEnumNames(grFields.Columns(1), GetType(GeneralFunctions.enmDataFieldType), True)

        For Each myField As clsDataField In RequiredFields.Values
            grFields.Rows.Add()
            Dim myRow As Windows.Forms.DataGridViewRow = grFields.Rows(grFields.Rows.Count - 1)
            myRow.Cells(0).Value = myField.ID
            myRow.Cells(1).Value = myField.FieldType.ToString
        Next

    End Sub
End Class