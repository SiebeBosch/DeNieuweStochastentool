Imports STOCHLIB.General

Public Class frmAddLevelsFromShapefile
    Private Setup As clsSetup
    Private con As SQLite.SQLiteConnection


    Public Sub New(ByRef mySetup As clsSetup, ByRef myCon As SQLite.SQLiteConnection)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        con = myCon
    End Sub
    Private Sub btnShapeFile_Click(sender As Object, e As EventArgs) Handles btnShapeFile.Click
        Try
            dlgOpenFile.Filter = "ESRI Shapefile|*.shp"
            dlgOpenFile.ShowDialog()
            txtShapeFile.Text = dlgOpenFile.FileName
            Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtShapeFile.Text, cmbZP, "ZP")
            Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtShapeFile.Text, cmbWP, "WP")
            Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtShapeFile.Text, cmbMTP, "MTP")
            Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtShapeFile.Text, cmbMV, "MV")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub btnExecute_Click(sender As Object, e As EventArgs) Handles btnExecute.Click
        Try
            Dim query As String = "SELECT * FROM OUTPUTLOCATIONS"
            Dim dt As New DataTable, pt As New MapWinGIS.Point
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, False)
            If dt.Rows.Count = 0 Then Throw New Exception("Lees eerst de waterstandslocaties uit het SOBEK-model")

            Dim i As Long, j As Long
            Dim ZPIdx As Integer = -1, WPIdx As Integer = -1, MTPIdx As Integer = -1, MVIdx As Integer = -1
            Dim myUtils As New MapWinGIS.Utils

            If System.IO.File.Exists(txtShapeFile.Text) Then
                Dim sf As New MapWinGIS.Shapefile
                sf.Open(txtShapeFile.Text)
                For i = 0 To sf.NumFields - 1
                    If sf.Field(i).Name = cmbZP.Text Then ZPIdx = i
                    If sf.Field(i).Name = cmbWP.Text Then WPIdx = i
                    If sf.Field(i).Name = cmbMTP.Text Then MTPIdx = i
                    If sf.Field(i).Name = cmbMV.Text Then MVIdx = i
                Next

                Me.Setup.GeneralFunctions.UpdateProgressBar("Referentiepeilen uit shapefile opvragen...", 0, 10)
                'walk through all locations and decide inside which shape they lie
                For i = 0 To dt.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, dt.Rows.Count)
                    pt.x = dt.Rows(i)("X")
                    pt.y = dt.Rows(i)("Y")
                    For j = 0 To sf.NumShapes - 1
                        If myUtils.PointInPolygon(sf.Shape(j), pt) Then
                            query = "UPDATE OUTPUTLOCATIONS SET "
                            If ZPIdx >= 0 Then query &= "ZP=" & sf.CellValue(ZPIdx, j) & ","
                            If WPIdx >= 0 Then query &= "WP=" & sf.CellValue(WPIdx, j) & ","
                            If Strings.Right(query, 1) = "," Then query = Strings.Left(query, query.Length - 1)
                            query &= " WHERE LOCATIEID='" & dt.Rows(i)("LOCATIEID") & "';"
                            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                            Exit For
                        End If
                    Next
                Next
            End If
            con.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Klaar.", 0, 10)
            Me.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
End Class