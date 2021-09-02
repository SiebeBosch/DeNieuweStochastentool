Imports STOCHLIB.General
Imports System.IO

Public Class clsWWTPDischargePoints

    Friend WWTPDischargePoints As New Dictionary(Of String, clsWWTPDischargePoint)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub


    Friend Function GetAdd(ByVal ID As String) As clsWWTPDischargePoint
        Dim WWTPDischargePoint As clsWWTPDischargePoint
        If WWTPDischargePoints.ContainsKey(ID.Trim.ToUpper) Then
            Return WWTPDischargePoints(ID.Trim.ToUpper)
        Else
            WWTPDischargePoint = New clsWWTPDischargePoint(Me.setup)
            WWTPDischargePoint.ID = ID
            WWTPDischargePoint.InUse = True
            WWTPDischargePoints.Add(ID.Trim.ToUpper, WWTPDischargePoint)
            Me.setup.Log.AddWarning("WWTP Discharge Point " & ID & " not found in shapefile. A new one was created")
            Return WWTPDischargePoint
        End If
    End Function


End Class
