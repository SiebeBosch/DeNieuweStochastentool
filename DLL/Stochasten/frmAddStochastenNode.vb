Imports STOCHLIB.General
Imports System.Windows.Forms

Public Class frmAddStochastenNode
    Public ModelID As String
    Public NodeID As String
    Private Setup As clsSetup
    Private ModelGrid As DataGridView

    Public Sub New(ByRef mySetup As clsSetup, ByRef myModelGrid As DataGridView)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        ModelGrid = myModelGrid

    End Sub

    Public Sub fromAddStochastenNode_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        cmbModelID.Items.Clear()
        For Each myRow As DataGridViewRow In ModelGrid.Rows
            cmbModelID.Items.Add(myRow.Cells("ModelID").Value)
        Next
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        ModelID = cmbModelID.Text
        NodeID = txtNodeID.Text
        Me.Close()
    End Sub
End Class