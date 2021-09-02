Imports STOCHLIB.General
Imports System.Data.SQLite


Public Class frmLocationsFromShapefile
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub
    Private Sub frmLocationsFromShapefile_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub btnShapefile_Click(sender As Object, e As EventArgs) Handles btnShapefile.Click
        dlgOpenFile.Filter = "ESRI Shapefile|*.shp"
        dlgOpenFile.ShowDialog()
        txtShapefile.Text = dlgOpenFile.FileName
        Me.Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtShapefile.Text, cmbIDField)
    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click

        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

        Dim mySource As New clsGeoDatasource(Me.Setup, False)
        mySource.SetPrimaryDatasource(txtShapefile.Text)
        mySource.Open()
        mySource.SetField(GeneralFunctions.enmInternalVariable.ID, cmbIDField.Text)

        For i = 0 To mySource.GetNumberOfRecords - 1
            Dim ID = mySource.GetTextValue(i, GeneralFunctions.enmInternalVariable.ID)
            Dim X As Double = mySource.Shapefile.sf.Shape(i).Center.x
            Dim Y As Double = mySource.Shapefile.sf.Shape(i).Center.y
            Dim Lat As Double, Lon As Double
            Me.Setup.GeneralFunctions.RD2WGS84(X, Y, Lat, Lon)

            Dim query As String = "INSERT INTO " & txtTableName.Text & "(" & txtLocationsField.Text & "," & txtXField.Text & "," & txtYField.Text & "," & txtLatField.Text & "," & txtLonField.Text & ") VALUES ('" & ID & "'," & X & "," & Y & "," & Lat & "," & Lon & ");"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
        Next

        mySource.Close()
        Me.Setup.SqliteCon.Close()

        MsgBox("Operation complete.")
    End Sub
End Class