Imports STOCHLIB.General
Public Class frmCopySettingsFromClimateScenario
    Private Setup As clsSetup
    Private TableName As String
    Private Fields As Dictionary(Of String, STOCHLIB.clsDataField)
    Private TargetScenario As String

    Public Sub New(ByRef mySetup As clsSetup, myTableName As String, myFields As Dictionary(Of String, STOCHLIB.clsDataField), myTargetScenario As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        TableName = myTableName
        Fields = myFields
        TargetScenario = myTargetScenario

    End Sub
    Private Sub frmCopySettingsFromClimateScenario_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.GeneralFunctions.PopulateComboboxWithKlimaatScenarios(cmbScenarios, True)
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        If cmbScenarios.Text = TargetScenario Then
            MsgBox("Doelscenario mag niet hetzelfde zijn als het bronscenario.")
        Else
            'first remove all existing records for the target scenario
            Dim i As Integer, j As Integer
            Dim querybase As String
            Dim Value As String

            Dim query As String = "DELETE FROM " & TableName & " WHERE KLIMAATSCENARIO='" & TargetScenario & "';"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            'now read all records from the source scenario
            query = "SELECT "
            For i = 0 To Fields.Count - 1
                query &= Fields.Values(i).GetID
                If i < Fields.Count - 1 Then query &= ", " Else query &= " "
            Next
            query &= "FROM " & TableName & " WHERE KLIMAATSCENARIO='" & cmbScenarios.Text & "';"

            Dim dt As New DataTable
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, False)

            'now replace the climate scenario and write these records back into the database
            querybase = "INSERT INTO " & TableName & " ("
            For i = 0 To Fields.Count - 1
                querybase &= Fields.Values(i).GetID
                If i < Fields.Count - 1 Then querybase &= ","
            Next
            querybase &= ") VALUES ("

            For i = 0 To dt.Rows.Count - 1
                query = querybase
                For j = 0 To Fields.Count - 1
                    Value = dt.Rows(i)(Fields.Values(j).GetID)
                    If Value = cmbScenarios.Text Then Value = TargetScenario
                    If Fields.Values(j).GetSQLiteType = STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT Then
                        query &= "'" & Value & "'"
                    Else
                        query &= Value
                    End If
                    If j < Fields.Count - 1 Then query &= ","
                Next
                query &= ");"
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
            Next

        End If
        MsgBox("Records voor tabel " & TableName & " zijn succesvol gekopieerd van " & cmbScenarios.Text & " naar " & TargetScenario & ".")
        Me.Close()
    End Sub
End Class