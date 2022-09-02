Public Class clsStochasticExtraClass
    Public ID As String
    Public p As Double
    Public RRFiles As String
    Public FlowFiles As String
    Public RTCFiles As String

    Public Sub New(ByVal myID As String, ByVal myP As Double, ByVal myRRFiles As String, myFlowFiles As String, myRTCFiles As String)
        ID = myID
        p = myP
        RRFiles = myRRFiles
        FlowFiles = myFlowFiles
        RTCFiles = myRTCFiles
    End Sub

    Public Sub New(ByVal myID As String, ByVal myP As Double)
        ID = myID
        p = myP
    End Sub
End Class
