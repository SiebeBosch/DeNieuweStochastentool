Imports STOCHLIB.General
Imports System.IO
Public Class clsRTCComponent
    Private Setup As clsSetup
    Private DIMR As clsDIMR

    Friend RTCRuntimeConfig As clsRtcRuntimeConfig

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDIMR As clsDIMR)
        Setup = mySetup
        DIMR = myDIMR
    End Sub


    Public Function WriteSimulationPeriod(StartDate As Date, EndDate As Date) As Boolean
        Try
            'here we read the entire content of the xml file in memory. Then we replace the line stating the startdate
            Dim path As String = DIMR.ProjectDir & "\" & DIMR.DIMRConfig.RTC.SubDir & "\" & "rtcRuntimeConfig.xml"
            Dim content As String, lines As String()
            Using xmlReader As New StreamReader(path)
                content = xmlReader.ReadToEnd()
            End Using
            lines = Split(content, vbCrLf)

            Using iniWriter As New StreamWriter(path)
                For Each myLine In lines
                    If Strings.Left(myLine.Trim, 10).ToLower = "<startdate" Then
                        'write our new start date
                        iniWriter.WriteLine("      <startDate date=" & Chr(34) & StartDate.ToString("yyyy-MM-dd") & Chr(34) & " time=" & Chr(34) & StartDate.ToString("HH:mm:ss") & Chr(34) & " />")
                    ElseIf Strings.Left(myLine.Trim, 8).ToLower = "<enddate" Then
                        iniWriter.WriteLine("      <endDate date=" & Chr(34) & EndDate.ToString("yyyy-MM-dd") & Chr(34) & " time=" & Chr(34) & EndDate.ToString("HH:mm:ss") & Chr(34) & " />")
                    Else
                        'leave the line untouched and write it
                        iniWriter.WriteLine(myLine)
                    End If
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error setting simulation period for RTC Component: " & ex.Message)
            Return False
        End Try
    End Function

End Class
