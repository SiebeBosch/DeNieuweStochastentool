Option Explicit On

Imports STOCHLIB.General

Public Class clsRRBrchTPRecord

    Friend ID As String
    Friend nm As String
    Friend ri As String
    Friend mt As Integer
    Friend mtString As String
    Friend bt As clsSobekBranchType
    Friend OBI As String
    Friend bn As String
    Friend en As String

    Friend InUse As Boolean

    Friend Flow As clsTimeTable
    'Public FlowYearSummaries As New Hashtable

    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        SbkCase = myCase
        Flow = New clsTimeTable(Me.setup)
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal iID As String, ByVal iName As String, ByVal FromNode As String, ByVal ToNode As String, ByVal BranchType As clsSobekBranchType, Optional ByVal SetInuse As Boolean = True)
        setup = mySetup
        ID = iID
        nm = iName
        bn = FromNode
        en = ToNode
        bt = BranchType
        InUse = SetInuse
    End Sub

    Friend Sub Read(ByRef setup As clsSetup, ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, Num As Integer
        Done = False

        InUse = True

        While Not Done
            myStr = setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr.ToLower
                Case "id"
                    ID = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ri"
                    ri = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "mt"
                    mt = setup.GeneralFunctions.ParseString(myRecord, " ")
                    mtString = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "bt"
                    Num = setup.GeneralFunctions.ParseString(myRecord, " ")
                    bt = SbkCase.BranchTypes.GetByNum(Num)
                Case "ObID"
                    OBI = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "bn"
                    bn = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "en"
                    en = setup.GeneralFunctions.ParseString(myRecord, " ")
                Case ""
                    Done = True
            End Select
        End While
    End Sub


End Class
