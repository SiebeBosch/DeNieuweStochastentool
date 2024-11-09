Imports STOCHLIB.General
Public Class frmAddModel
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Private Sub btnExecutable_Click(sender As Object, e As EventArgs) Handles btnExecutable.Click
        dlgOpenFile.Filter = "Exceutable files|*.exe;*.bat"
        Dim Result As DialogResult = dlgOpenFile.ShowDialog
        If Result = DialogResult.OK Then
            txtExecutable.Text = dlgOpenFile.FileName
        End If
    End Sub


    Private Sub btnModeldir_Click(sender As Object, e As EventArgs) Handles btnModeldir.Click
        Dim Result As DialogResult = dlgFolder.ShowDialog()
        If Result = DialogResult.OK Then
            txtModelDir.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnWorkdir_Click(sender As Object, e As EventArgs) Handles btnWorkdir.Click
        Dim Result As DialogResult = dlgFolder.ShowDialog()
        If Result = DialogResult.OK Then
            txtWorkdir.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnResultsDirRR_Click(sender As Object, e As EventArgs)
        Dim Result As DialogResult = dlgFolder.ShowDialog()
        If Result = DialogResult.OK Then
            txtResultsdirRR.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnResultsDirFlow_Click(sender As Object, e As EventArgs)
        Dim Result As DialogResult = dlgFolder.ShowDialog()
        If Result = DialogResult.OK Then
            txtResultsDirFlow.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Me.Setup.GeneralFunctions.UpdateProgressBar("Writing model specs to database...", 0, 10, True)

        Dim query As String = "SELECT DISTINCT MODELID FROM SIMULATIONMODELS;"
        Dim ModelID As Integer
        Dim dt As New DataTable
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)

        'find an available ID for our new model
        Dim i As Integer = 0
        Dim FreeSlotFound As Boolean = False
        While Not FreeSlotFound
            i += 1
            FreeSlotFound = True
            For j = 0 To dt.Rows.Count - 1
                If i = dt.Rows(j)(0) Then FreeSlotFound = False
            Next
            If FreeSlotFound Then                'found an available ID
                ModelID = i
            End If
        End While

        query = "INSERT INTO SIMULATIONMODELS (MODELID,MODELTYPE,EXECUTABLE,ARGUMENTS,MODELDIR,CASENAME,TEMPWORKDIR,RESULTSFILES_RR,RESULTSFILES_FLOW) VALUES (" & ModelID & ",'" & cmbModelType.Text & "','" & txtExecutable.Text & "','" & txtArguments.Text & "','" & txtModelDir.Text & "','" & txtCaseName.Text & "','" & txtWorkdir.Text & "','" & txtResultsdirRR.Text & "','" & txtResultsDirFlow.Text & "');"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 10, 10, True)
    End Sub

    Private Sub frmAddModel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.SetProgress(prProgress, lblProgress)

        cmbModelType.Items.Clear()
        Me.Setup.GeneralFunctions.PopulateComboBoxColumnWithEnumNames(cmbModelType, GetType(STOCHLIB.GeneralFunctions.enmSimulationModel))
    End Sub

    Private Sub hlpAddModel_Click(sender As Object, e As EventArgs) Handles hlpAddModel.Click
        openhelplink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/models.html#modelschematisatie-toevoegen")
    End Sub

    Public Sub OpenHelpLink(url As String)
        Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
    End Sub

End Class