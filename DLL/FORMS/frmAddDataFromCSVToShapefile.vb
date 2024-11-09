Imports System.Windows.Forms
Imports STOCHLIB.General
Imports System.IO
Public Class frmAddDataFromCSVToShapefile
    Private Setup As clsSetup
    Dim CSV As clsCSVFile

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub
    Private Sub btnSF_Click(sender As Object, e As EventArgs) Handles btnSF.Click
        dlgOpenFile.Filter = "ESRI Shapefile|*.shp"
        Dim Result As DialogResult = dlgOpenFile.ShowDialog
        If Result = Windows.Forms.DialogResult.OK Then
            txtSF.Text = dlgOpenFile.FileName
            Me.Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtSF.Text, cmbIDField, GeneralFunctions.enmErrorLevel._Message)
            Me.Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtSF.Text, cmbShapeField, GeneralFunctions.enmErrorLevel._Message)
        End If
    End Sub

    Private Sub btnCSV_Click(sender As Object, e As EventArgs) Handles btnCSV.Click

        dlgOpenFile.Filter = "CSV file|*.csv"
        Dim Result As DialogResult = dlgOpenFile.ShowDialog
        If Result = DialogResult.OK Then

            txtCSV.Text = dlgOpenFile.FileName

            Dim DelimiterPicker As New frmPickDelimiter
            DelimiterPicker.ShowDialog()

            If DelimiterPicker.DialogResult = DialogResult.OK Then

                CSV = New clsCSVFile(Me.Setup, txtCSV.Text, DelimiterPicker.ContainsHeader, DelimiterPicker.Delimiter)

                'we know our delimiter so populate our comoboboxes with all available columns
                Me.Setup.GeneralFunctions.populateComboboxWithTextFileColumns(txtCSV.Text, CSV.Delimiter, CSV.ContainsHeader, cmbIDCol)
                Me.Setup.GeneralFunctions.populateComboboxWithTextFileColumns(txtCSV.Text, CSV.Delimiter, CSV.ContainsHeader, cmbFieldNameCol)
                Me.Setup.GeneralFunctions.populateComboboxWithTextFileColumns(txtCSV.Text, CSV.Delimiter, CSV.ContainsHeader, cmbValueCol)

            End If
        End If
    End Sub

    Private Sub frmShapefileImportCSV_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.SetProgress(prProgress, lblProgress)
    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click

        'read our CSV file
        If CSV.ContainsHeader Then CSV.ReadHeader()
        CSV.ReadContent()

        Me.Setup.GeneralFunctions.UpdateProgressBar("Processing CSV file data...", 0, 10, True)

        'read the shapefile and prepare for writing to its attribute table
        Dim sf As New clsShapeFile(Me.Setup, txtSF.Text)
        sf.Open()
        sf.sf.StartEditingTable()

        Dim myRecord As List(Of Object)
        Dim iRecord As Integer = 0, nRecord As Integer = CSV.Content.Count
        Dim ObjectID As String, ShapeField As String = "", Value As Double
        Dim IDColIdx As Integer, FieldColIdx As Integer, ValColIdx As Integer       'the index numbers of the columns in the csv file
        Dim IDFieldIdx As Integer, ValFieldIdx As Integer                           'the index numbers of the fields in the shapefile
        Dim nAdjustments As Integer = 0

        IDColIdx = CSV.GetFieldIdx(cmbIDCol.Text)
        ValColIdx = CSV.GetFieldIdx(cmbValueCol.Text)

        If radShapefield.Checked Then
            'the user has specified one shapefield to write all values to
            ShapeField = cmbShapeField.Text
        Else
            'the user has indicated that the shapefield for each record is specified in the CSV file
            'therefore the assignment for ShapeFIeld will be done inside the loop. Here we only store the csv column index that holds our shapefield name
            FieldColIdx = CSV.GetFieldIdx(cmbFieldNameCol.Text)
        End If

        IDFieldIdx = sf.GetFieldIdx(cmbIDField.Text)
        If IDFieldIdx >= 0 Then
            For Each myRecord In CSV.Content
                iRecord += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iRecord, nRecord)
                ObjectID = myRecord.Item(IDColIdx)
                If radShapeFieldCol.Checked Then ShapeField = myRecord.Item(FieldColIdx) 'only if the csv file contains the shapefield names must we determine our field every record
                Value = myRecord.Item(ValColIdx)
                ValFieldIdx = sf.GetFieldIdx(ShapeField)

                'now search our shapefile for any record with the same ID as ObjectID and write the value
                If ValFieldIdx >= 0 Then
                    For i = 0 To sf.sf.NumShapes - 1
                        If sf.sf.CellValue(IDFieldIdx, i) = ObjectID Then
                            'we found a matching ID!
                            sf.sf.EditCellValue(ValFieldIdx, i, Value)
                            nAdjustments += 1
                        End If
                    Next
                End If
            Next
        End If

        sf.sf.StopEditingTable(True)
        sf.Close()

        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

        MsgBox("Operation complete. " & nAdjustments & " values have been replaced in the shapefile.")
        Me.Close()

    End Sub
End Class