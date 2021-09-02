Option Explicit On

''' <summary>
''' Geen constructor nodig
''' </summary>
''' <remarks></remarks>
Public Class clsRRSettings

  'algemene instellingen, vooral voor het exporteren van een model
  Friend RRUnpavedSettings As New clsObjectSettings
  Friend RRGreenhouseSettings As New clsObjectSettings
  Friend RRWWTPSettings As New clsObjectSettings
  Friend RRPavedSettings As New clsObjectSettings
  Friend RRCFSettings As New clsObjectSettings

  Friend BranchPrefix As String

  ''' <summary>
  ''' TODO: Siebe invullen
  ''' </summary>
  Friend Sub Initialize()

    RRUnpavedSettings.NodeType = clsObjectSettings.enmNodetype.NodeRRUnpaved
    RRUnpavedSettings.Prefix = "up"
    RRUnpavedSettings.NodeTypeDescription = "3B_UNPAVED"

    RRPavedSettings.NodeType = clsObjectSettings.enmNodetype.NodeRRPaved
    RRPavedSettings.Prefix = "pv"
    RRPavedSettings.NodeTypeDescription = "3B_PAVED"

    RRGreenhouseSettings.NodeType = clsObjectSettings.enmNodetype.NodeRRGreenhouse
    RRGreenhouseSettings.Prefix = "gr"
    RRGreenhouseSettings.NodeTypeDescription = "3B_GREENHOUSE"

    RRCFSettings.NodeType = clsObjectSettings.enmNodetype.NodeRRCFConnection
    RRCFSettings.Prefix = "rrcf"
    RRCFSettings.NodeTypeDescription = "SBK_SBK-3B-REACH"

    RRWWTPSettings.NodeType = clsObjectSettings.enmNodetype.NodeRRWWTP
    RRWWTPSettings.Prefix = "wwtp"
    RRWWTPSettings.NodeTypeDescription = "3B_WWTP"
  End Sub

End Class
