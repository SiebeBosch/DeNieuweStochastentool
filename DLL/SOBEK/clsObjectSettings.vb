Option Explicit On

Friend Class clsObjectSettings

  Friend RRUnpaved As String = "3B_UNPAVED"
  Friend RRPaved As String = "3B_PAVED"
  Friend RRCFConnection As String = "SBK_SBK-3B-REACH"
  Friend Enum enmNodetype
    NodeRRUnpaved = 44
    NodeRRsacramento = 54
    NodeRRPaved = 43
    NodeRRWWTP = 56
    NodeRRGreenhouse = 45
    NodeRRBoundary = 46
    NodeRRPump = 47
    NodeRRIndustry = 48
    NodeRRCFConnectionNode = 35
    NodeRRCFConnection = 34
    NodeRRWeir = 49
    NodeRROrifice = 50
    NodeRRFriction = 51
    NodeCFConnectionNode = 1
    NodeCFConnectionNodeWithFlow = 2
    NodeCFBoundary = 3
    NodeCFCrossSection = 4
    NodeCFMeasurement = 5
    NodeCFLateral = 6
    NodeCFProfile = 7
    NodeCFWeir = 8
    NodeCFUniversalWeir = 9
    NodeCFOrifice = 10
    NodeCFCulvert = 11
    NodeCFBridge = 12
    NodeCFPump = 13
  End Enum

  Friend NodeTypeDescription As String
  Friend NodeType As enmNodetype
  Friend Execute As Boolean
  Friend SheetName As String
  Friend Prefix As String
  Friend AdjustLevels As Boolean

End Class
