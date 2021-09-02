Imports STOCHLIB.General
Public Class clsStruct3BRecord

    Dim ID As String
    Dim nm As String
    Dim ty As GeneralFunctions.enmRRStructureType
    Dim wt As Integer
    Dim sl As Double
    Dim dc As Double    'discharge coef
    Dim dt As Integer
    Dim cw As Double    'crest width
    Dim cw2 As Double
    Dim cl As Double    'initial crest elevation
    Dim cl2 As Double
    Dim cp As Double
    Dim ws As String
    Dim inConst As Integer
    Dim rt As GeneralFunctions.enmFlowDirection

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup


    End Sub

End Class
