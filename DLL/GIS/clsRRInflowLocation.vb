Imports STOCHLIB.General

Public Class clsRRInflowLocation

    Public ID As String                           'the actual ID of the inflow location (also used in dictionaries)
    Public SewageAreaID As String                 'the actual ID of the sewage area that this overflow location belongs to
    Public NodeID As String                       'the ID of the SOBEK-node represented by the inflow location
    Public X As Double                            'X-coordinate of the SOBEK-node represented by the inflow location
    Public Y As Double
    Public ReachObject As clsSbkReachObject
    Public ReachNode As clsSbkReachNode
    Public InUse As Boolean

    Friend RRCFConnection As clsSbkReachObject
    Friend RRCFConnNode As clsSbkReachNode
    Friend Lateral As clsSbkReachObject

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Friend Sub SetSobekCase(ByRef myCase As clsSobekCase)
        SbkCase = myCase
    End Sub


End Class
