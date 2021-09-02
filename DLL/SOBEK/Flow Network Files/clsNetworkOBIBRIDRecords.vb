Option Explicit On
Imports STOCHLIB.General

Public Class clsNetworkOBIBRIDRecords
    Public Records As New Dictionary(Of String, clsNetworkOBIBRIDRecord)
    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Public Sub Add(ByVal myRecord As clsNetworkOBIBRIDRecord)
        If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
            'already in collection
        Else
            Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
        End If
    End Sub

    Public Function GetReachType(ByVal ID As String) As STOCHLIB.GeneralFunctions.enmReachtype
        Dim myRecord As clsNetworkOBIBRIDRecord

        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
            Return myRecord.GetReachType
        Else
            'if no node type found, return a general reach type
            Me.Setup.Log.AddWarning("Reach " & ID & " not found in function GetReachType of class clsNetworkOBIBRIDRecords. Default reach type was assumed.")
            Return GeneralFunctions.enmReachtype.ReachCFChannel
        End If

    End Function

    Public Function GetRecord(ByVal ID As String) As clsNetworkOBIBRIDRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function
End Class
