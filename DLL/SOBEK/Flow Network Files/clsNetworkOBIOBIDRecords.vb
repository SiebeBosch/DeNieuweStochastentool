Option Explicit On
Imports STOCHLIB.General

Public Class clsNetworkOBIOBIDRecords
    Public Records As New Dictionary(Of String, clsNetworkOBIOBIDRecord)
    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Public Sub Add(ByVal myRecord As clsNetworkOBIOBIDRecord)
        If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            'already in collection
        Else
            Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
        End If
    End Sub

    Public Function GetNodeType(ByVal ID As String) As STOCHLIB.GeneralFunctions.enmNodetype
        Dim myRecord As clsNetworkOBIOBIDRecord

        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
            Return myRecord.GetNodeType
        Else
            'if no node type found, return a general node type
            Return GeneralFunctions.enmNodetype.NodeCFGridpoint
        End If

    End Function

    Public Function GetRecord(ByVal ID As String) As clsNetworkOBIOBIDRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function
End Class
