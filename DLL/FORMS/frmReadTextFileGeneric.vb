Imports STOCHLIB.General

Public Class frmReadTextFileGeneric
    Private Setup As clsSetup

    Dim CSVFile As clsCSVFile

    'please note: both Content and Fields are accessible from outside this class
    Public Fields As New Dictionary(Of String, clsDataField)               'key = column header
    Public Content As List(Of List(Of Object))                              'contains the entire textfile's content

    Public Sub New(ByRef mySetup As clsSetup, FieldsList As List(Of String))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

        'initialize the containers
        Fields = New Dictionary(Of String, clsDataField)
        Content = New List(Of List(Of Object))

        Dim i As Integer
        For i = 0 To FieldsList.Count - 1
            grFields.Rows.Add(FieldsList.Item(i))
        Next

    End Sub

    Private Sub btnReadTextfile_Click(sender As Object, e As EventArgs) Handles btnReadTextfile.Click
        Dim frmDel As New frmPickDelimiter()
        dlgOpenFile.Filter = "All files|*.*"
        dlgOpenFile.ShowDialog()
        txtTextFile.Text = dlgOpenFile.FileName

        frmDel.ShowDialog()
        If frmDel.DialogResult = Windows.Forms.DialogResult.OK Then
            CSVFile = New clsCSVFile(Me.Setup, txtTextFile.Text, frmDel.ContainsHeader, frmDel.Delimiter)
            CSVFile.ReadHeader()
            CSVFile.PopulateDataGridViewComboBoxColumnByHeader(grFields.Columns(2))
        End If
    End Sub

    Private Sub frmReadTextFileGeneric_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim myList As New List(Of String)
        myList.Add(System.Type.GetType("System.Int32").ToString)
        myList.Add(System.Type.GetType("System.Int64").ToString)
        myList.Add(System.Type.GetType("System.Double").ToString)
        myList.Add(System.Type.GetType("System.Boolean").ToString)
        myList.Add(System.Type.GetType("System.String").ToString)
        Setup.GeneralFunctions.populateDataGridViewComboBoxColumnFromList(grFields.Columns(1), myList)
    End Sub

    Private Sub btnRead_Click(sender As Object, e As EventArgs) Handles btnRead.Click
        'we will make a list of the column index numbers where each of the required fields are
        Dim myField As clsDataField
        Dim myRow As System.Windows.Forms.DataGridViewRow
        Dim myCmb As System.Windows.Forms.DataGridViewComboBoxCell

        CSVFile.RemoveBoundingQuotesFromStrings = chkRemoveBoundingQuotes.Checked

        Me.Setup.GeneralFunctions.UpdateProgressBar("Configuring fields...", 0, 10, True)
        For i = 0 To grFields.Rows.Count - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, grFields.Rows.Count, True)
            myField = New clsDataField()
            myRow = grFields.Rows(i)
            myCmb = myRow.Cells(2)
            myField.ID = myRow.Cells(0).Value.ToString
            myField.DataType = System.Type.GetType(myRow.Cells(1).Value.ToString)
            myField.TextFileColIdx = myCmb.Items.IndexOf(myCmb.Value)
            Fields.Add(myField.ID.Trim.ToUpper, myField)
        Next

        'read the entire csv file's content (for reasons of speed)
        Dim fullContent As New List(Of List(Of Object))
        fullContent = CSVFile.ReadContent

        'now parse the content and return it, organized by chosen field
        Me.Setup.GeneralFunctions.UpdateProgressBar("Parsing file content...", 0, 10, True)
        For i = 0 To fullContent.Count - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, fullContent.Count)
            Dim contentRow As New List(Of Object)
            For j = 0 To Fields.Count - 1
                contentRow.Add(fullContent(i)(Fields.Values(j).TextFileColIdx))
            Next
            Content.Add(contentRow)
        Next
        Me.Close()
    End Sub
End Class