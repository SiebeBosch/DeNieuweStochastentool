Imports STOCHLIB.General
Imports System.IO

Public Class clsRRComponent
    Private Setup As clsSetup
    Private DIMR As clsDIMR
    Friend IniFile As clsDelft3BIniFile

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDIMR As clsDIMR)
        Setup = mySetup
        DIMR = myDIMR
    End Sub

    Public Function WriteSimulationPeriod(StartDate As Date, EndDate As Date) As Boolean
        Try
            'here we read the entire content of the MDU file in memory. Then we replace the line stating the startdate
            Dim path As String = DIMR.ProjectDir & "\" & DIMR.DIMRConfig.RR.SubDir & "\" & "delft_3b.ini"
            Dim content As String, lines As String()
            Using iniReader As New StreamReader(path)
                content = iniReader.ReadToEnd()
            End Using
            lines = Split(content, vbCrLf)

            Using iniWriter As New StreamWriter(path)
                For Each myLine In lines
                    If Strings.Left(myLine.Trim, 9).ToLower = "starttime" Then
                        'write our new start date
                        iniWriter.WriteLine("StartTime='" & StartDate.ToString("yyyy/MM/dd;HH:mm:ss") & "'")
                    ElseIf Strings.Left(myLine.Trim, 7).ToLower = "endtime" Then
                        iniWriter.WriteLine("EndTime='" & EndDate.ToString("yyyy/MM/dd;HH:mm:ss") & "'")
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
