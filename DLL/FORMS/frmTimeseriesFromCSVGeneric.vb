Imports STOCHLIB.General

Public Class frmTimeseriesFromCSVGeneric
    Private Setup As clsSetup
    Public CSV As clsTimeseriesTextFile
    Public ClearExistingData As Boolean
    Dim Delimiter As String
    Dim ContainsHeader As Boolean
    Public dt As DataTable

    Private Tablename As String
    Private SeriesIDFIeld As String
    Private LocationIDField As String
    Private ParameterField As String
    Private DatesField As String
    Private ValuesField As String


    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myTableName As String, mySeriesIDField As String, myLocationIDField As String, myParameterField As String, myDatesField As String, myValuesField As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

        'database table settings
        Tablename = myTableName
        SeriesIDFIeld = mySeriesIDField
        LocationIDField = myLocationIDField
        ParameterField = myParameterField
        DatesField = myDatesField
        ValuesField = myValuesField

    End Sub

    Public Sub SetSeriesID(SeriesID)
        radCustomSeriesID.Checked = True
        txtSeriesID.Text = SeriesID
        grSeriesID.Enabled = False
    End Sub

    Public Sub SetLocationID(LocationID)
        radCustomID.Checked = True
        txtID.Text = LocationID
        grLocationID.Enabled = False
    End Sub

    Public Sub SetParameter(Parameter)
        radCustomPar.Checked = True
        txtPar.Text = Parameter
        grParameter.Enabled = False
    End Sub

    Private Sub frmTimeseriesFromCSVGeneric_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.SetProgress(prProgress, lblProgress)
        If Not My.Settings.DatePickerValue = Nothing Then pckDate.Value = My.Settings.DatePickerValue
        'v1.797: removed the three rows below since we want to enforce certain settings from the calling function
        'txtSeriesID.Text = My.Settings.SeriesName
        'txtID.Text = My.Settings.ID
        'txtPar.Text = My.Settings.PAR
        txtTimestepSizeSeconds.Text = My.Settings.TimestepSizeSeconds
        'v1.797: removed the two rows below since we want to enforce certain settings from the calling function
        'radCustomSeriesID.Checked = My.Settings.CustomSeriesID
        'radCustomID.Checked = My.Settings.CustomID
        'radCustomPar.Checked = My.Settings.CustomPAR
        radCustomStartDate.Checked = My.Settings.CustomStartDate
    End Sub

    Public Function GetCSVPath() As String
        Return txtCSVFile.Text
    End Function
    Public Function GetDelimiter() As String
        Return Delimiter
    End Function
    Public Function GetIDField() As String
        If Not cmbIDCol.SelectedItem Is Nothing Then
            Return cmbIDCol.SelectedItem
        Else
            Return txtID.Text
        End If
    End Function
    Public Function GetParField() As String
        If Not cmbParCol.SelectedItem Is Nothing Then
            Return cmbParCol.SelectedItem
        Else
            Return txtPar.Text
        End If
    End Function
    Public Function GetDateField() As String
        Return cmbDateCol.SelectedItem
    End Function
    Public Function GetValField() As String
        Return cmbValCol.SelectedItem
    End Function
    Public Function GetDateFormatting() As String
        Return txtDateFormatting.Text
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
                Delimiter = frmDelimiter.txtDelimiter.Text
                ContainsHeader = frmDelimiter.chkContainsHeader.Checked
                txtCSVFile.Text = dlgOpenFile.FileName
                CSV = New clsTimeseriesTextFile(Me.Setup, dlgOpenFile.FileName, My.Settings.Delimiter, Tablename, SeriesIDFIeld, LocationIDField, ParameterField, DatesField, ValuesField)
                If Not CSV.ReadColumns(ContainsHeader) Then MsgBox("Error reading CSV file header.")
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbSeriesIDCol)
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbIDCol)
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbParCol)
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbDateCol)
                Setup.PopulateComboBoxFromList(CSV.Getcolumnslist, cmbValCol)
            End If
        End If
    End Sub

    Private Sub btnDateFormatting_Click(sender As Object, e As EventArgs) Handles btnDateFormatting.Click
        MsgBox("Date format e.g. yyyy/MM/dd hh:mm:ss. Note: month always in uppercase, minute in lowercase!")
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        'note: this routine PREPARES reading the data from CSV. The actual reading itself can be called afterwards.

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading timeseries from text file...", 0, 10, True)

            'My.Settings.DatePickerValue = pckDate.Value
            My.Settings.ID = txtID.Text
            My.Settings.SeriesName = txtSeriesID.Text
            My.Settings.PAR = txtPar.Text
            My.Settings.TimestepSizeSeconds = txtTimestepSizeSeconds.Text
            My.Settings.CustomSeriesID = radCustomSeriesID.Checked
            My.Settings.CustomID = radCustomID.Checked
            My.Settings.CustomPAR = radCustomPar.Checked
            My.Settings.CustomStartDate = radCustomStartDate.Checked
            My.Settings.Save()

            'error handling
            If radSeriesIDbyColumn.Checked AndAlso cmbSeriesIDCol.SelectedIndex < 0 Then Throw New Exception("Select a column that contains the Series ID.")
            If radIDByColumn.Checked AndAlso cmbIDCol.SelectedIndex < 0 Then Throw New Exception("Select a column that contains the location ID.")
            If radParByColumn.Checked AndAlso cmbParCol.SelectedIndex < 0 Then Throw New Exception("Select a column that contains the parameter.")
            If radDateByColumn.Checked AndAlso cmbDateCol.SelectedIndex < 0 Then Throw New Exception("Select a column that contains the date values.")
            If cmbValCol.SelectedIndex < 0 Then Throw New Exception("Select a column that contains the data values.")

            'CSV.setSeriesID(txtSeriesID.Text)

            'create a datatable
            'Setup.SetProgress(prProgress, lblProgress)

            'read the textfile's fields. start with the Series ID field
            'Important: the Fields are stored in the collection in the same order as the columns in the textfile. This means that the Field Index Number can be used as a lookup value in the file content
            If radSeriesIDbyColumn.Checked AndAlso cmbSeriesIDCol.SelectedIndex >= 0 Then
                Dim myField As New clsDataField()
                myField.ID = "SeriesID"
                myField.TextFileColIdx = cmbSeriesIDCol.SelectedIndex
                myField.DataType = Type.GetType("System.String")
                myField.FieldType = GeneralFunctions.enmDataFieldType.SERIESID
                CSV.AddField(myField)
            ElseIf radCustomSeriesID.Checked Then
                CSV.AddField(New clsDataField("SeriesID", GeneralFunctions.enmDataFieldType.SERIESID, txtSeriesID.Text))
            End If

            If radIDByColumn.Checked AndAlso cmbIDCol.SelectedIndex >= 0 Then
                Dim myField As New clsDataField()
                myField.ID = "ID"
                myField.TextFileColIdx = cmbIDCol.SelectedIndex
                myField.DataType = Type.GetType("System.String")
                myField.FieldType = GeneralFunctions.enmDataFieldType.LOCATIONID
                CSV.AddField(myField)
            ElseIf radCustomID.Checked Then
                CSV.AddField(New clsDataField("ID", GeneralFunctions.enmDataFieldType.LOCATIONID, txtID.Text))
            End If

            'read the parameter name field
            If radParByColumn.Checked Then
                Dim myField As New clsDataField()
                myField.ID = "PARAMETER"
                myField.TextFileColIdx = cmbParCol.SelectedIndex
                myField.DataType = Type.GetType("System.String")
                myField.FieldType = GeneralFunctions.enmDataFieldType.PARAMETER
                CSV.AddField(myField)
            ElseIf radCustomPar.Checked Then
                CSV.AddField(New clsDataField("PARAMETER", GeneralFunctions.enmDataFieldType.PARAMETER, txtPar.Text))
            End If

            'read the date values field
            If radDateByColumn.Checked Then
                Dim myField As New clsDataField()
                myField.ID = "DATETIME"
                myField.TextFileColIdx = cmbDateCol.SelectedIndex
                myField.DataType = Type.GetType("System.DateTime")
                myField.FieldType = GeneralFunctions.enmDataFieldType.DATETIME
                myField.DateFormatting = txtDateFormatting.Text
                CSV.AddField(myField)
            Else
                CSV.AddField(New clsDataField("DATETIME", GeneralFunctions.enmDataFieldType.DATETIME, New DateTime(pckDate.Value.Year, pckDate.Value.Month, pckDate.Value.Day, pckDate.Value.Hour, 0, 0), txtTimestepSizeSeconds.Text))
            End If

            'finally add the values field. This one is compulsory so no choice possisble for constant values or similar
            If cmbValCol.SelectedIndex >= 0 Then
                Dim myField As New clsDataField()
                myField.ID = "VALUE"
                myField.TextFileColIdx = cmbValCol.SelectedIndex
                myField.DataType = Type.GetType("System.Double")
                myField.FieldType = GeneralFunctions.enmDataFieldType.VALUE
                CSV.AddField(myField)
            Else
                Throw New Exception("Select a column containing the values.")
            End If

            'read the CSV file
            CSV.ReadContent()

            'and process its content
            CSV.ProcessContentAndStoreInMemory()

            'if requested we remove all existing series that carry the same ID as the series we just imported
            Me.Setup.GeneralFunctions.UpdateProgressBar("Removing old timeseries from database...", 0, 10, True)
            If chkClearExisting.Checked Then
                Dim query As String
                Dim i As Integer
                For Each Series As clsTimeSeries In CSV.ContentProcessed.Values
                    i += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Removing old timeseries from database...", i, CSV.ContentProcessed.Count, True)
                    query = "DELETE FROM " & Tablename & " WHERE " & SeriesIDFIeld & " = '" & Series.ID & "';"
                    Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
                Next
            End If
            Me.Close()

        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
        End Try
    End Sub
End Class