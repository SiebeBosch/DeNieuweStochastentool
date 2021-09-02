Imports STOCHLIB.General

Public Class clsParallelReach
    Public ID As String
    Public ReverseReachDirection As Boolean                 'does the reach for this structure reach need to be reversed? (e.g. for inlet structures)
    Public Struc As clsSbkReachObject
    Public SerialStructureOffset As Double
    Public ProfileUp As clsSbkReachObject
    Public ProfileDown As clsSbkReachObject

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase, ByVal myID As String, Reverse As Boolean, StructID As String, myNodeType As GeneralFunctions.enmNodetype, myNodeSubType As GeneralFunctions.enmNodeSubType, makeStructure As Boolean, makeProfileUp As Boolean, makeProfileDown As Boolean, Optional ByVal mySerialStructureOffset As Double = 10)

        Setup = mySetup
        SbkCase = myCase
        ID = myID
        ReverseReachDirection = Reverse

        Struc = New clsSbkReachObject(Me.setup, myCase)
        Struc.ID = StructID
        Struc.nt = myNodeType
        Struc.SubType = myNodeSubType
        Struc.InUse = makeStructure

        SerialStructureOffset = mySerialStructureOffset

        ProfileUp = New clsSbkReachObject(Me.Setup, Me.SbkCase)
        ProfileUp.ID = StructID & "up"
        ProfileUp.nt = GeneralFunctions.enmNodetype.SBK_PROFILE
        ProfileUp.InUse = makeProfileUp

        ProfileDown = New clsSbkReachObject(Me.Setup, Me.SbkCase)
        ProfileDown.ID = StructID & "dn"
        ProfileDown.nt = GeneralFunctions.enmNodetype.SBK_PROFILE
        ProfileDown.InUse = makeProfileDown
    End Sub
End Class
