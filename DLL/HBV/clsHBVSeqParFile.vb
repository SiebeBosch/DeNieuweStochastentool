Imports STOCHLIB.General
Public Class clsHBVSeqParFile
    Private Setup As clsSetup
    Dim bYear As Integer
    Dim bMonth As Integer
    Dim bDay As Integer
    Dim bHour As Integer
    Dim eYear As Integer
    Dim eMonth As Integer
    Dim eDay As Integer
    Dim eHour As Integer
    Dim instate As Integer = 1
    Dim outstate As Integer = 1
    Dim numseq As Integer = 1
    Dim iType As String = "'sim'"
    Dim regulation As String = "'off'"
    Dim wstupdate As String = "'off'"
    Dim autoreg As String = "'on'"

    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Sub Build(StartDate As Date, EndDate As Date)
        bYear = StartDate.Year
        bMonth = StartDate.Month
        bDay = StartDate.Day
        bHour = StartDate.Hour
        eYear = EndDate.Year
        eMonth = EndDate.Month
        eDay = EndDate.Day
        eHour = EndDate.Hour
    End Sub

    Public Function Write(path As String) As Boolean
        Using myWriter As New System.IO.StreamWriter(path)
            myWriter.WriteLine("")
            myWriter.WriteLine("'group'")
            myWriter.WriteLine("'byear     ' " & bYear)
            myWriter.WriteLine("'bmonth    ' " & bMonth)
            myWriter.WriteLine("'bday      ' " & bDay)
            myWriter.WriteLine("'bhour     ' " & bHour)
            myWriter.WriteLine("'eyear     ' " & eYear)
            myWriter.WriteLine("'emonth    ' " & eMonth)
            myWriter.WriteLine("'eday      ' " & eDay)
            myWriter.WriteLine("'ehour     ' " & eHour)
            myWriter.WriteLine("'instate   ' " & instate)
            myWriter.WriteLine("'outstate  ' " & outstate)
            myWriter.WriteLine("'numseq    ' " & numseq)
            myWriter.WriteLine("'type      ' " & iType)
            myWriter.WriteLine("'regulation' " & regulation)
            myWriter.WriteLine("'wstupdate ' " & wstupdate)
            myWriter.WriteLine("'autoreg   ' " & autoreg)
        End Using
    End Function


End Class
