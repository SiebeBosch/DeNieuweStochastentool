Public Class clsStochasticGroundwaterClass

    Public ID As String
    Public p As Double
    Public RootFolder As String               'when copying files from a folder, all subdirs and their content will be copied to the modle
    Public RRFiles As New List(Of String)     'a list of filenames for the RR module
    Public FlowFiles As New List(Of String)   'a list of filenames for the Flow module
    Public RTCFiles As New List(Of String)    'a list of filenames for the RTC module
    Public Sub New(ByVal myID As String, ByVal myP As Double)
        ID = myID
        p = myP
    End Sub

    Public Sub SetFolder(myFolder As String)
        RootFolder = myFolder
    End Sub

    Public Sub AddRRFile(myFileName As String)
        RRFiles.Add(myFileName)
    End Sub
    Public Sub AddFlowFile(myFileName As String)
        FlowFiles.Add(myFileName)
    End Sub
    Public Sub AddRtcFile(myFileName As String)
        RTCFiles.Add(myFileName)
    End Sub

End Class
