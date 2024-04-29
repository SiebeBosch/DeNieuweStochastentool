Imports STOCHLIB.General
Public Class clsHBVTempHourlyFile

    Private Setup As clssetup
    Dim Header1 As String = "'T'"   'name of the parameter?
    Dim Header2 As Integer = 3  'no clue
    Dim Header3 As String = "'380_T'"   'name of the meteo station?
    Dim Header4 As Integer = 24  'number of records per day?

    Dim Records As New Dictionary(Of Date, Double)

    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Sub Build(StartDate As Date, DurationHours As Integer)
        Dim curDate As Date, temp As Double
        Records.Clear()
        For i = 0 To DurationHours + 1
            curDate = StartDate.AddHours(i)
            temp = Setup.GeneralFunctions.getMonthAverageTemperatureDeBilt(curDate)
            Records.Add(curDate, temp)
        Next
    End Sub

    Public Function Write(path As String) As Boolean
        Try
            Using myWriter As New System.IO.StreamWriter(path)
                myWriter.WriteLine(Header1)
                myWriter.WriteLine(Header2)
                myWriter.WriteLine(Header3)
                myWriter.WriteLine(Header4)
                For Each rec In Records
                    myWriter.WriteLine(rec.Key.ToString("yyyy M d H") & " " & rec.Value.ToString("0.0"))
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Write of class clsHBVTempHourlyFile: " & ex.Message)
            Return False
        End Try
    End Function

End Class
