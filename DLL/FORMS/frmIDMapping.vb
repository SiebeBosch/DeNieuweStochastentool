Imports STOCHLIB.General

Public Class frmIDMapping

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

        Me.Setup.SetProgress(prProgress, lblProgress)

        'initialize the datagridview with all existing mappings
        Dim mapTable As New DataTable
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT * FROM IDMAPPING;", mapTable, True)
        grIDMapping.DataSource = mapTable

    End Sub

    Private Sub FrmIDMapping_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'populate the first column
        Me.Setup.GeneralFunctions.populateDataGridViewComboBoxColumnFromQuery(grIDMapping.Columns.Item(0), "SELECT DISTINCT LOCATIONID FROM TIMESERIES;")
    End Sub

    Private Sub BtnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        grIDMapping.Rows.Add()
    End Sub

    Private Sub BtnRemove_Click(sender As Object, e As EventArgs) Handles btnRemove.Click
        For i = grIDMapping.Rows.Count - 1 To 0 Step -1
            If grIDMapping.Rows(i).Selected Then grIDMapping.Rows.RemoveAt(i)
        Next
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        'first clear the existing table
        Dim query As String = "DELETE From IDMAPPING;"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

        'now add all the rows we have in our datagridview
        Me.Setup.GeneralFunctions.UpdateProgressBar("Updating mapping table...", 0, 10, True)
        For i = 0 To grIDMapping.Rows.Count - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, grIDMapping.Rows.Count - 1)
            query = "INSERT INTO IDMAPPING (OBJECTID_OBSERVED,OBJECTID_MODEL) VALUES ('" & grIDMapping.Rows(i).Cells(0).Value & "','" & grIDMapping.Rows(i).Cells(1).Value & "');"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
        Next
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
    End Sub
End Class