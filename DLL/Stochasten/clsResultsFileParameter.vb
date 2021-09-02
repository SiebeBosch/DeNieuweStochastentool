Imports STOCHLIB.General

Public Class clsResultsFileParameter

    Public Name As String
    Public Locations As New Dictionary(Of String, clsResultsFileLocation)
    Private Setup As clsSetup
    Private Model As clsSimulationModel

    Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel)
        Setup = mySetup
        Model = myModel
    End Sub

    Public Function GetAddLocation(ByVal myID As String, ByVal myName As String, ByVal myDuration As Integer) As clsResultsFileLocation
        If Not Locations.ContainsKey(myID.Trim.ToUpper) Then
            Dim myLoc As New clsResultsFileLocation(Me.Setup, Model, Me)
            myLoc.ID = myID
            myLoc.Name = myName
            myLoc.Duration = myDuration 'is necessary to compute the return period
            Locations.Add(myID.Trim.ToUpper, myLoc)
        End If
        Return Locations.Item(myID.Trim.ToUpper)
    End Function

End Class
