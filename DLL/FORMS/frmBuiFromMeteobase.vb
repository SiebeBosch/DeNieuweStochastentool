Imports STOCHLIB.General

Public Class frmBuiFromMeteobase

    Dim Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub
    Private Sub FrmBuiFromMeteobase_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class