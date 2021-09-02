Imports STOCHLIB.General
Imports System.IO

Public Class clsRksFile

    Dim Path As String
    Dim BaseEvent As clsBuiFile
    Public Events As New Dictionary(Of Integer, clsBuiFile) 'in its basics an RKS file is nothing more than a bunch of .BUI files in one
    Dim DataSetToggle As Integer '1 voor default dataset, 0 voor volledige reeks voor de overige invoer
    Public nStations As Integer
    Dim nEvents As Integer  'numer of events in the series
    Dim nSeconds As Integer 'number of seconds per timestep

    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        BaseEvent = New clsBuiFile(Me.Setup)
    End Sub

    Public Function CombineRKS(ByRef RKScombine As STOCHLIB.clsRksFile) As Boolean
        Try
            Dim myEvent As clsBuiFile, addEvent As clsBuiFile
            nStations += RKScombine.nStations
            For i = 0 To Events.Count - 1
                myEvent = Events.Item(i)
                addEvent = RKScombine.Events.Item(i)
                myEvent.combineEvent(addEvent)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function CombineRKS of class clsRksFile.")
            Return False
        End Try

    End Function

    Public Sub AddStations(ms As clsMeteoStations)
        nStations += ms.MeteoStations.Count
        For Each myEvent As clsBuiFile In Events.Values
            For i = 0 To ms.MeteoStations.Count - 1
                myEvent.MeteoStations.MeteoStations.Add(ms.MeteoStations.Item(i).Name, ms.MeteoStations.Item(i))
            Next
            ReDim Preserve myEvent.Values(UBound(myEvent.Values, 0), UBound(myEvent.Values, 1) + ms.MeteoStations.Count)
        Next
    End Sub

    Public Function GetStations() As clsMeteoStations
        Return Events.Item(0).MeteoStations
    End Function


    Public Function Read(mypath As String) As Boolean
        Try
            Path = mypath
            Dim myLine As String, i As Integer, j As Integer, ms As clsMeteoStation
            Dim tmpStr As String
            Dim myEvent As clsBuiFile
            Dim myYear As Integer, myMonth As Integer, myDay As Integer, myHour As Integer, myMinute As Integer, mySecond As Integer
            Dim days As Integer, hours As Integer, minutes As Integer, seconds As Integer

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading RKS file..", 0, 10)
            Using rksReader As New StreamReader(Path)
                While Not rksReader.EndOfStream
                    myLine = rksReader.ReadLine.Trim
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", rksReader.BaseStream.Position, rksReader.BaseStream.Length)

                    If Left(myLine, 8) = "*Gebruik" Then
                        myLine = rksReader.ReadLine
                        DataSetToggle = Setup.GeneralFunctions.ParsestringNumeric(myLine)
                    ElseIf Left(myLine, 16) = "*Aantal stations" Then
                        myLine = rksReader.ReadLine
                        nStations = Setup.GeneralFunctions.ParsestringNumeric(myLine)
                    ElseIf Left(myLine, 19) = "*Namen van stations" Then
                        For i = 1 To nStations
                            ms = New clsMeteoStation(Me.Setup)
                            myLine = rksReader.ReadLine.Trim
                            ms.ID = Me.Setup.GeneralFunctions.RemoveSurroundingQuotes(myLine, True, False)
                            ms.Name = ms.ID
                            BaseEvent.AddMeteoStation(ms)
                        Next
                        'v2.203: sometimes the string contains "*aantal gebeurtenissen" first
                    ElseIf Left(myLine, 7).Trim.ToLower = "*en het" OrElse Left(myLine, 22).Trim.tolower = "*aantal gebeurtenissen" Then
                        myLine = rksReader.ReadLine.Trim
                        nEvents = Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        nSeconds = Setup.GeneralFunctions.ParsestringNumeric(myLine)
                    ElseIf Left(myLine, 6).Trim.ToLower = "*event" Then
                        myEvent = BaseEvent.Clone
                        tmpStr = Me.Setup.GeneralFunctions.ParseString(myLine, " ")
                        myEvent.number = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine, " ")
                        myEvent.TimeStep = New TimeSpan(0, 0, nSeconds)

                        'v2.203: sometimes people have another comment line. So read until we find a non-commented line
                        Dim CommentLine As Boolean = True
                        While CommentLine
                            myLine = rksReader.ReadLine.Trim
                            If Left(myLine, 1) <> "*" Then CommentLine = False
                        End While

                        myYear = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        myMonth = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        myDay = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        myHour = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        myMinute = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        mySecond = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)

                        days = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        hours = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        minutes = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        seconds = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine)
                        myEvent.TotalSpan = New TimeSpan(days, hours, minutes, seconds)
                        Dim nSteps As Integer = myEvent.TotalSpan.TotalSeconds / nSeconds
                        myEvent.StartDate = New Date(myYear, myMonth, myDay, myHour, myMinute, mySecond)
                        myEvent.EndDate = myEvent.StartDate.Add(myEvent.TotalSpan)

                        '((myEvent.EndDate - myEvent.StartDate) / nSteps, nStations)
                        ReDim myEvent.Values(0 To nSteps - 1, 0 To nStations - 1)
                        For i = 0 To nSteps - 1
                            myLine = rksReader.ReadLine.Trim
                            For j = 0 To nStations - 1
                                myEvent.Values(i, j) = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine, " ")
                            Next
                        Next
                        Events.Add(Events.Count, myEvent)
                    End If

                End While
            End Using
            Me.Setup.GeneralFunctions.UpdateProgressBar("File has been read successfully.", 0, 10)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function Write(mypath As String) As Boolean
        Try
            Using rksWriter As New StreamWriter(mypath)
                'doorloop alle areas en schrijf de 'bui' weg
                rksWriter.WriteLine("*Name of this file: " & mypath)
                rksWriter.WriteLine("*Date and time of construction: " & Now & ".")
                rksWriter.WriteLine("*Enige algemene wenken:")
                rksWriter.WriteLine("*Gebruik de default dataset (1) of de volledige reeks (0) voor overige invoer")
                rksWriter.WriteLine("0")
                rksWriter.WriteLine("*Aantal stations")
                rksWriter.WriteLine(Events.Item(0).MeteoStations.MeteoStations.Count)
                rksWriter.WriteLine("*Namen van stations")
                For Each myStation In Events.Item(0).MeteoStations.MeteoStations.Values
                    rksWriter.WriteLine(Chr(39) & myStation.ID & Chr(39)) 'de stations
                Next
                rksWriter.WriteLine("*Aantal gebeurtenissen")
                rksWriter.WriteLine("*en het aantal seconden per waarnemingstijdstap")
                rksWriter.WriteLine(" " & Events.Count & "  " & Events.Item(0).TimeStep.TotalSeconds)
                rksWriter.WriteLine("*Elke commentaarregel wordt begonnen met een * (asterisk).")
                rksWriter.WriteLine("*Eerste record bevat startdatum en -tijd, lengte van de gebeurtenis in dd hh mm ss")
                rksWriter.WriteLine("*Het format is: yyyymmdd:hhmmss:ddhhmmss")
                rksWriter.WriteLine("*Daarna voor elk station de neerslag in mm per tijdstap.")
                For Each myEvent As clsBuiFile In Events.Values
                    myEvent.WriteToRKS(rksWriter)
                Next
            End Using
        Catch ex As Exception

        End Try
    End Function


End Class
