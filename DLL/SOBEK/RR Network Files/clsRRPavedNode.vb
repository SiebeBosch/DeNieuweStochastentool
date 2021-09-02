Imports STOCHLIB.General

Public Class clsRRPavedNode
    Friend ID As String
    Friend Name As String   'This contains the actual name for the sewage area, so use it as the key for the CSO-locations
    Friend X As Double
    Friend Y As Double
    Friend InUse As Boolean
    Friend CSOLocation As clsRRInflowLocation

    'a record of all the files that are used to represent this node type
    Friend NodeTpRecord As New clsRRNodeTPRecord(Me.setup)
    Friend Paved3BRecord As New clsPaved3BRecord(Me.setup)
    Friend PavedSTORecord As New clsPavedSTORecord(Me.setup)
    Friend PavedDWFRecord As New clsPavedDWARecord(Me.setup)
    Friend Bound3B3BRecord As New clsBound3B3BRecord(Me.setup)

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        setup = mySetup
    End Sub

    Friend Sub setCSOLocation()
        CSOLocation = Me.setup.CSOLocations.getLocation(ID)
        If Not CSOLocation Is Nothing Then CSOLocation.InUse = True
    End Sub

    Friend Function getCSOLocation() As clsRRInflowLocation
        CSOLocation = Me.setup.CSOLocations.getLocation(Name.Trim.ToUpper)
        If Not CSOLocation Is Nothing Then
            CSOLocation.InUse = True
            Return CSOLocation
        Else
            Return Nothing
        End If
    End Function

End Class
