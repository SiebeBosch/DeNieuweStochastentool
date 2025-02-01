Option Explicit On
Imports DocumentFormat.OpenXml.Bibliography
Imports STOCHLIB.General
Imports System.IO

''' <summary>
''' Geen constructor nodig
''' </summary>
''' <remarks></remarks>
Public Class clsBuiFile

    Public StartDate As DateTime
    Friend EndDate As DateTime
    Friend TimeStep As TimeSpan
    Friend TotalSpan As TimeSpan
    Public number As Integer 'the event number for this event in a series

    Friend MeteoStations As clsMeteoStations
    Friend Values(,) As Single 'all the data in an array instead of a (memory consuming) clsTimeTable

    Private setup As clsSetup

    'SIEBE: op 23-7 heb ik een eerste aanzet gemaakt om af te stappen van de clsTimeTable (te memory consuming) en over te stappen op een 2D-array
    'het object TimeTable zal dus op termijn worden uitgefaseerd, ten faveure van Values(,)
    'omdat een .bui-file alleen equidistante data tijdstippen bevat is een array dates() niet nodig

    Public Function getTotalSpan() As TimeSpan
        Return TotalSpan
    End Function

    Public Function GetStationByClosestStringMatch(ID As String) As String
        'this function finds the best matching station ID for a given object ID. It does so by computing the nearest match
        Dim bestMatch As Double = -9.0E+99, bestMS As String = MeteoStations.MeteoStations.Values(0).ID
        Dim curMatch As Double
        For Each myStation As clsMeteoStation In MeteoStations.MeteoStations.Values
            curMatch = Me.setup.GeneralFunctions.GetSimilarity(ID, myStation.Name)
            If curMatch > bestMatch Then
                bestMatch = curMatch
                bestMS = myStation.Name
            End If
        Next
        Return bestMS
    End Function

    Public Function getMaxWindowSum(WindowSizeSeconds As Integer) As Double
        Dim WindowTimesteps As Integer = WindowSizeSeconds / TimeStep.TotalSeconds
        Dim i As Long, j As Long, mySum As Double, maxSum As Double
        For i = 0 To UBound(Values, 1) - (WindowTimesteps - 1)
            mySum = 0
            For j = i To i + (WindowTimesteps - 1)
                mySum += Values(j, 0)
            Next
            If mySum > maxSum Then maxSum = mySum
        Next
        Return maxSum
    End Function

    Public Function GetVolumeStatistics(ByRef Volume As Double, ByRef Max As Double, ByRef Min As Double) As Boolean
        Try
            Dim r As Integer, Sum As Double, myMax As Double = -9.0E+99
            For r = 0 To UBound(Values, 1)
                Sum += Values(r, 0)
                If Values(r, 0) > myMax Then myMax = Values(r, 0)
            Next
            Volume = Sum
            Max = myMax
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function GetVolumeStatistics fo class clsBuiFile.")
            Return False
        End Try
    End Function

    Public Function IsSummer(winsumdate As Date, sumwindate As Date) As Boolean
        'this function returns a boolean to state if an even is summer or winter
        Dim refYear As Integer = DateAndTime.Year(winsumdate)
        Dim checkDate As New Date(refYear, DateAndTime.Month(StartDate), DateAndTime.Month(EndDate))
        If checkDate >= winsumdate AndAlso checkDate < sumwindate Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function Clone() As clsBuiFile
        Dim newBui As New clsBuiFile(Me.setup)
        Dim i As Long
        newBui.StartDate = StartDate
        newBui.EndDate = EndDate
        newBui.TimeStep = TimeStep
        newBui.TotalSpan = TotalSpan

        For i = 0 To MeteoStations.MeteoStations.Count - 1
            newBui.MeteoStations.MeteoStations.Add(MeteoStations.MeteoStations.Keys(i), MeteoStations.MeteoStations.Values(i))
        Next
        newBui.Values = Values
        Return newBui
    End Function

    Public Function combineEvent(ByRef EventCombine As clsBuiFile) As Boolean
        Try
            If Not StartDate = EventCombine.StartDate Then Throw New Exception("Error: events cannot be combined because start date differs.")
            If Not TotalSpan = EventCombine.TotalSpan Then Throw New Exception("Error: events cannot be combined because timespan differs.")
            Dim StartColIdx = MeteoStations.MeteoStations.Count
            Dim r As Long, c As Long

            For Each myMs As clsMeteoStation In EventCombine.MeteoStations.MeteoStations.Values
                MeteoStations.MeteoStations.Add(myMs.Name, myMs)
            Next

            'redim the array with precipitation values
            ReDim Preserve Values(0 To UBound(Values, 1), 0 To UBound(Values, 2) + EventCombine.MeteoStations.MeteoStations.Count)

            'now write the values
            For c = 0 To EventCombine.MeteoStations.MeteoStations.Count - 1
                For r = 0 To UBound(Values, 1)
                    Values(r, StartColIdx + c) = EventCombine.Values(r, c)
                Next
            Next

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function WriteToRKS(ByRef rksWriter As StreamWriter, Optional ByVal nDigits As Integer = 2) As Boolean
        Try
            'deze functie schrijft een .bui-file zoals hij binnen een RSK-file valt
            Dim i As Integer, myStr As String, nTim As Long
            Dim nStations As Integer = MeteoStations.MeteoStations.Count

            'determine the number formatting for the .bui file (e.g. "0.0000")
            Dim Formatting As String = "#.0"
            If nDigits > 1 Then
                For i = 2 To nDigits
                    Formatting &= "#"
                Next
            End If

            Me.setup.GeneralFunctions.UpdateProgressBar("Writing SOBEK BUI-file.", 0, 10)

            'zoek de informatie over tijdstappen op
            'let op: einddatum moet gecorrigeerd worden door er één tijdstap bij op te tellen (omdat die tijdstap zelf ook nog meetelt)
            'doorloop alle areas en schrijf de 'bui' weg
            rksWriter.WriteLine("*Event " & number)

            Dim dagen As Integer = TotalSpan.Days
            Dim uren As Integer = TotalSpan.Hours
            Dim seconds As Integer = TotalSpan.Seconds

            'schrijf de instellingen voor datum/tijd en tijdstap naar de buifile. Zet het begin op de daadwerkelijke start van de resultaten
            'en vul de tijdstappen met resultaten van een tijdstap verder
            rksWriter.WriteLine(" " & StartDate.Year & " " & StartDate.Month & " " & StartDate.Day & " " & StartDate.Hour & " " & StartDate.Minute & " 0 " & dagen & " " & uren & " " & seconds & " 0 ")

            'write the meteorological data
            nTim = GetnRecords()
            Me.setup.GeneralFunctions.UpdateProgressBar("Writing event to rks file.", 0, 10)

            For i = 0 To nTim - 1

                myStr = ""
                Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, nTim)

                For j = 0 To MeteoStations.MeteoStations.Count - 1
                    myStr &= (" " & Format(Values(i, j), Formatting))
                Next
                rksWriter.WriteLine(myStr)
            Next

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in sub WriteToRKS of class clsBuiFile.")
        End Try


    End Function

    Public Sub New(ByVal mySetup As clsSetup)
        Me.setup = mySetup
        'TimeTable = New clsTimeTable(Me.setup)
        MeteoStations = New clsMeteoStations(Me.setup)
    End Sub

    Public Function GetnRecords() As Long
        Return setup.GeneralFunctions.RoundUD(TotalSpan.TotalSeconds / TimeStep.TotalSeconds, 0, False)
    End Function

    Public Function InitializeRecords(myStartDate As DateTime, myEndDate As DateTime, ts As TimeSpan) As Boolean
        Try
            'this function initializes the records for this bui file
            If MeteoStations.MeteoStations.Count = 0 Then Throw New Exception("Error in function InitializeRecords of class clsBuiFile: Meteo stations not yet added.")

            'set startdate, enddate, total timespan and timestep
            StartDate = myStartDate
            EndDate = myEndDate
            TimeStep = ts
            TotalSpan = New TimeSpan
            TotalSpan = EndDate.Subtract(StartDate)
            Dim nRecords = GetnRecords()

            If nRecords > 0 Then
                ReDim Preserve Values(0 To nRecords - 1, 0 To MeteoStations.MeteoStations.Count - 1)
            Else
                Throw New Exception("Error initializing bui file. Number of records could not be computed.")
            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub AddMeteoStation(ByRef ms As clsMeteoStation)
        ms.Idx = MeteoStations.MeteoStations.Count
        Call MeteoStations.GetAdd(ms, ms.ID)
    End Sub

    Friend Function GetAddMeteoStation(ByVal Key As String, ByVal Station As String) As clsMeteoStation
        Dim myStation As New clsMeteoStation(Me.setup)
        myStation.ID = Station
        myStation.Idx = MeteoStations.MeteoStations.Count
        Call MeteoStations.MeteoStations.Add(Key, myStation)
        Return myStation
    End Function

    Public Function BuildSTOWATYPE(ByVal StationName As String, Season As String, Pattern As String, ByVal Vol As Double, Volume_Multiplier As Double, ByVal StartDate As DateTime, ByVal Fractie As Double(), ByVal Uitloop As Integer, gebiedsreductie As GeneralFunctions.enmGebiedsreductie, constantARF As Double, oppervlak As Double) As Boolean
        Try
            Dim i As Integer
            Dim ms As clsMeteoStation
            TimeStep = New TimeSpan(1, 0, 0)

            ms = GetAddMeteoStation(StationName.Trim.ToUpper, StationName)
            ms.gebiedsreductie = gebiedsreductie
            ms.ConstantFactor = constantARF
            ms.oppervlak = oppervlak
            Call InitializeRecords(StartDate, StartDate.AddHours(Fractie.Count + Uitloop), New TimeSpan(1, 0, 0))

            Select Case ms.gebiedsreductie
                Case Is = STOCHLIB.GeneralFunctions.enmGebiedsreductie.constante
                    'bui, eventueel met constante gebiedsreductiefactor
                    For i = 0 To Fractie.Count - 1
                        Values(i, ms.Idx) = Vol * ms.ConstantFactor * Volume_Multiplier * Fractie(i)
                    Next
                Case Is = GeneralFunctions.enmGebiedsreductie.oppervlak
                    'bui, bereken gebiedsreductie op basis van het oppervlak
                    'neem aan dat de tijdstapgrootte 1 uur is
                    Dim res As (Boolean, Double) = setup.Gebiedsreductie.CalculateByArea(Season, Pattern, Vol, Fractie.Count * 60, ms.oppervlak)
                    If res.Item1 Then
                        Vol = Vol * res.Item2 * Volume_Multiplier 'apply the areal reduction factor to the event's volume
                        For i = 0 To Fractie.Count - 1
                            Values(i, ms.Idx) = Vol * Fractie(i)
                        Next
                    Else
                        Me.setup.Log.AddError($"Unable to implement Areal Reduction Factor (ARF) for {Season}, {Pattern}, {Vol} mm at {ms.oppervlak} km2. Event volume remains unchanged.")
                    End If
                Case Is = GeneralFunctions.enmGebiedsreductie.geavanceerd
                    'bui, bereken gebiedsreductie op basis van het oppervlak
                    'echter alleen voor het kritische gedeelte van de bui!
                    Dim res As (Boolean, List(Of Double)) = setup.Gebiedsreductie.CalculateByAreaAdvanced(Season, Pattern, Vol * Volume_Multiplier, Fractie, Fractie.Count * 60, ms.oppervlak)

                    If res.Item1 Then
                        For i = 0 To Fractie.Count - 1
                            Values(i, ms.Idx) = res.Item2(i)
                        Next
                    Else
                        Me.setup.Log.AddError($"Unable to implement Areal Reduction Factor (ARF) for {Season}, {Pattern}, {Vol} mm at {ms.oppervlak} km2. Event volume remains unchanged.")
                    End If
                Case Else
                    Throw New Exception($"Error: unknown gebiedsreductie type. {ms.gebiedsreductie} not (yet) supported in generating .bui file.")
            End Select

            'uitloop
            For i = Fractie.Count To Fractie.Count + Uitloop - 1
                Values(i, ms.Idx) = 0
            Next
            Return True

        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildSTOWAType of class clsBuiFile: " & ex.Message)
            Return False
        End Try


    End Function


    Public Function BuildLongTermEVAP(ByVal seizoen As STOCHLIB.GeneralFunctions.enmSeason, ByVal Duration As Integer, ByVal Uitloop As Integer) As Boolean

        Dim i As Integer
        Dim ms As clsMeteoStation
        TimeStep = New TimeSpan(1, 0, 0)
        Dim Evap As Double

        Select Case seizoen
            Case Is = GeneralFunctions.enmSeason.hydrosummerhalfyear
                StartDate = New DateTime(2000, 7, 1, 0, 0, 0)
                Evap = 4
            Case Is = GeneralFunctions.enmSeason.hydrowinterhalfyear
                StartDate = New DateTime(2000, 1, 1, 0, 0, 0)
                Evap = 2
            Case Is = GeneralFunctions.enmSeason.meteosummerhalfyear
                StartDate = New DateTime(2000, 7, 1, 0, 0, 0)
                Evap = 4
            Case Is = GeneralFunctions.enmSeason.meteowinterhalfyear
                StartDate = New DateTime(2000, 1, 1, 0, 0, 0)
                Evap = 2
            Case Is = GeneralFunctions.enmSeason.meteospringquarter
                StartDate = New DateTime(2000, 4, 1, 0, 0, 0)
                Evap = 3
            Case Is = GeneralFunctions.enmSeason.meteosummerquarter
                StartDate = New DateTime(2000, 7, 1, 0, 0, 0)
                Evap = 4
            Case Is = GeneralFunctions.enmSeason.meteoautumnquarter
                StartDate = New DateTime(2000, 11, 1, 0, 0, 0)
                Evap = 3
            Case Is = GeneralFunctions.enmSeason.meteowinterquarter
                StartDate = New DateTime(2000, 1, 1, 0, 0, 0)
                Evap = 2
            Case Else
                StartDate = New DateTime(2000, 1, 1, 0, 0, 0)
                Evap = 2
        End Select

        'verdamping
        ms = GetAddMeteoStation("NEERSLAG", "Neerslag")
        For i = 0 To Duration - 1
            Values(i, 0) = Evap
        Next

        'uitloop
        For i = 1 To Uitloop
            Values(i, 0) = 0
        Next

    End Function

    Public Function Read(ByVal Path As String) As Boolean
        Try
            Dim myStr As String, nStations As Integer, i As Integer
            Dim nEvents As Integer, myVal As clsMeteoValue
            Dim StartYear As Integer, StartMonth As Integer, StartDay As Integer, StartHour As Integer, StartMinute As Integer, StartSecond As Integer
            Dim days As Integer, hours As Integer, minutes As Integer
            Dim DatesDone As Boolean = False

            Using buiReader As New StreamReader(Path)
                While Not buiReader.EndOfStream
                    myStr = buiReader.ReadLine.Trim
                    If Trim(myStr) = "*Aantal stations" Then
                        nStations = Convert.ToInt16(buiReader.ReadLine.Trim)
                    ElseIf Trim(myStr) = "*Namen van stations" Then
                        Dim NamesDone As Boolean = False
                        While Not NamesDone
                            myStr = buiReader.ReadLine.Trim
                            If Left(myStr, 1) = "*" Then
                                NamesDone = True
                            Else
                                Dim ms As New clsMeteoStation(Me.setup)
                                ms.ID = Me.setup.GeneralFunctions.RemoveBoundingQuotes(myStr)
                                ms.Name = ms.ID
                                AddMeteoStation(ms)
                            End If
                        End While
                    ElseIf Trim(myStr) = "*en het aantal seconden per waarnemingstijdstap" Then
                        myStr = buiReader.ReadLine.Trim
                        nEvents = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        Timestep = New TimeSpan(0, 0, Val(Me.setup.GeneralFunctions.ParseString(myStr, " ")))
                    ElseIf Trim(myStr) = "*Daarna voor elk station de neerslag in mm per tijdstap." Then
                        myStr = buiReader.ReadLine.Trim
                        StartYear = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        StartMonth = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        StartDay = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        StartHour = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        StartMinute = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        StartSecond = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        days = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        hours = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        minutes = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                        StartDate = New Date(StartYear, StartMonth, StartDay, StartHour, StartMinute, StartSecond)
                        TotalSpan = New TimeSpan(days * 24 + hours, minutes, 0)
                        EndDate = StartDate.Add(TotalSpan)
                        DatesDone = True

                    ElseIf DatesDone Then
                        myStr = buiReader.ReadLine.Trim
                        For i = 0 To MeteoStations.MeteoStations.Count - 1
                            myVal = New clsMeteoValue()
                            myVal.ValueObserved = Me.setup.GeneralFunctions.ParseString(myStr, " ")
                            MeteoStations.MeteoStations.Values(i).PrecipitationHourly.Add(i.ToString, myVal)
                        Next
                    End If
                End While
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function Read of class clsBuiFile.")
            Return False
        End Try
    End Function

    Public Sub Write(ByVal path As String, Optional ByVal nDigits As Integer = 2)

        Try
            'deze functie schrijft een .bui-file aan de hand van shapes in een shapefile plus een reeds gevulde lijst met stations
            Dim i As Integer, myStr As String, nTim As Long
            Dim nStations As Integer = MeteoStations.MeteoStations.Count
            Dim myStation As clsMeteoStation

            'determine the number formatting for the .bui file (e.g. "0.0000")
            Dim Formatting As String = "#.0"
            If nDigits > 1 Then
                For i = 2 To nDigits
                    Formatting &= "#"
                Next
            End If

            'Me.setup.GeneralFunctions.UpdateProgressBar("Writing SOBEK BUI-file.", 0, 10)

            'zoek de informatie over tijdstappen op
            If GetnRecords() = 0 Then
                Throw New Exception("Error: no records in bui-file. Cannot write.")
            Else
                'let op: einddatum moet gecorrigeerd worden door er één tijdstap bij op te tellen (omdat die tijdstap zelf ook nog meetelt)
                Using buiWriter As New StreamWriter(path, False)
                    'doorloop alle areas en schrijf de 'bui' weg
                    buiWriter.WriteLine("*Name of this file: " & path)
                    buiWriter.WriteLine("*Date and time of construction: " & Now & ".")
                    buiWriter.WriteLine("1")
                    buiWriter.WriteLine("*Aantal stations")
                    buiWriter.WriteLine(MeteoStations.MeteoStations.Count)
                    buiWriter.WriteLine("*Namen van stations")
                    For Each myStation In MeteoStations.MeteoStations.Values
                        buiWriter.WriteLine(Chr(39) & myStation.ID & Chr(39)) 'de stations
                    Next
                    buiWriter.WriteLine("*Aantal gebeurtenissen (omdat het 1 bui betreft is dit altijd 1)")
                    buiWriter.WriteLine("*en het aantal seconden per waarnemingstijdstap")
                    buiWriter.WriteLine(" 1  " & TimeStep.TotalSeconds)
                    buiWriter.WriteLine("*Elke commentaarregel wordt begonnen met een * (asterisk).")
                    buiWriter.WriteLine("*Eerste record bevat startdatum en -tijd, lengte van de gebeurtenis in dd hh mm ss")
                    buiWriter.WriteLine("*Het format is: yyyymmdd:hhmmss:ddhhmmss")
                    buiWriter.WriteLine("*Daarna voor elk station de neerslag in mm per tijdstap.")

                    Dim dagen As Integer = TotalSpan.Days
                    Dim uren As Integer = TotalSpan.Hours
                    Dim seconds As Integer = TotalSpan.Seconds

                    'schrijf de instellingen voor datum/tijd en tijdstap naar de buifile. Zet het begin op de daadwerkelijke start van de resultaten
                    'en vul de tijdstappen met resultaten van een tijdstap verder
                    buiWriter.WriteLine(" " & StartDate.Year & " " & StartDate.Month & " " & StartDate.Day & " " & StartDate.Hour & " " & StartDate.Minute & " 0 " & dagen & " " & uren & " " & seconds & " 0 ")

                    'write the meteorological data
                    nTim = GetnRecords()
                    'Me.setup.GeneralFunctions.UpdateProgressBar("Writing .bui file.", 0, 10)

                    For i = 0 To nTim - 1

                        myStr = ""
                        'Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, nTim)

                        For j = 0 To MeteoStations.MeteoStations.Count - 1
                            myStr &= (" " & Format(Values(i, j), Formatting))
                        Next
                        buiWriter.WriteLine(myStr)
                    Next

                    buiWriter.Close()
                End Using

            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in sub Write of class clsBuiFile.")
        End Try

    End Sub


    Public Sub WriteHBV(ByVal path As String, myStation As clsMeteoStation, Optional ByVal nDigits As Integer = 2)

        Try
            'deze functie schrijft een neerslaggebeurtnis in HBV bestandsformaat aan de hand van shapes in een shapefile plus een reeds gevulde lijst met stations
            Dim i As Integer, myStr As String, nTim As Long
            Dim nStations As Integer = MeteoStations.MeteoStations.Count

            'determine the number formatting for the .bui file (e.g. "0.0000")
            Dim Formatting As String = "0"
            If nDigits > 0 Then
                Formatting &= "."
                For i = 1 To nDigits
                    Formatting &= "#"
                Next
            End If

            Me.setup.GeneralFunctions.UpdateProgressBar("Writing HBV rainfall-file.", 0, 10)

            'zoek de informatie over tijdstappen op
            If GetnRecords() = 0 Then
                Throw New Exception("Error: no records in bui-file. Cannot write.")
            Else
                'let op: einddatum moet gecorrigeerd worden door er één tijdstap bij op te tellen (omdat die tijdstap zelf ook nog meetelt)
                Using buiWriter As New StreamWriter(path, False)
                    'doorloop alle areas en schrijf de 'bui' weg
                    buiWriter.WriteLine("'P'")
                    buiWriter.WriteLine(myStation.Number)
                    buiWriter.WriteLine($"'{myStation.Name}_P'")
                    buiWriter.WriteLine(24)

                    Dim dagen As Integer = TotalSpan.Days
                    Dim uren As Integer = TotalSpan.Hours
                    Dim seconds As Integer = TotalSpan.Seconds

                    'schrijf de instellingen voor datum/tijd en tijdstap naar de buifile. Zet het begin op de daadwerkelijke start van de resultaten
                    'en vul de tijdstappen met resultaten van een tijdstap verder
                    'buiWriter.WriteLine(" " & StartDate.Year & " " & StartDate.Month & " " & StartDate.Day & " " & StartDate.Hour & " " & StartDate.Minute & " 0 " & dagen & " " & uren & " " & seconds & " 0 ")

                    'write the meteorological data
                    nTim = GetnRecords()
                    'Me.setup.GeneralFunctions.UpdateProgressBar("Writing .bui file.", 0, 10)

                    Dim curDate As Date
                    For i = 0 To nTim - 1

                        curDate = StartDate.AddMinutes(TimeStep.TotalMinutes * i)
                        myStr = DateAndTime.Year(curDate) & " " & DateAndTime.Month(curDate) & " " & DateAndTime.Day(curDate) & " " & Hour(curDate) & " " & Format(Values(i, 0), Formatting)
                        buiWriter.WriteLine(myStr)
                    Next

                    buiWriter.Close()
                End Using

            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in sub WriteHBV of class clsBuiFile.")
        End Try

    End Sub

    Public Sub WriteAsCSV(ByVal path As String, Optional ByVal nDigits As Integer = 2)

        Try
            'deze functie schrijft een .csv-file aan de hand van shapes in een shapefile plus een reeds gevulde lijst met stations
            Dim i As Integer, j As Long, myStr As String, nTim As Long
            Dim myStation As clsMeteoStation
            Dim curDate As New DateTime

            'determine the number formatting for the .bui file (e.g. "0.0000")
            Dim Formatting As String = "#.0"
            If nDigits > 1 Then
                For i = 2 To nDigits
                    Formatting &= "#"
                Next
            End If

            Me.setup.GeneralFunctions.UpdateProgressBar("Writing CSV-file.", 0, 10)

            'zoek de informatie over tijdstappen op
            If GetnRecords() = 0 Then
                Throw New Exception("Error: no records in timetable. Cannot write .csv file.")
            Else
                'let op: einddatum moet gecorrigeerd worden door er één tijdstap bij op te tellen (omdat die tijdstap zelf ook nog meetelt)
                Using csvWriter As New StreamWriter(path, False)

                    'doorloop alle areas en schrijf de 'csv' weg
                    Dim tmpStr As String = "Datum/Tijd"

                    For j = 0 To MeteoStations.MeteoStations.Values.Count - 1
                        myStation = MeteoStations.MeteoStations.Values(j)
                        tmpStr &= ";" & myStation.ID
                    Next
                    csvWriter.WriteLine(tmpStr)

                    nTim = GetnRecords()
                    Me.setup.GeneralFunctions.UpdateProgressBar("Writing .csv file.", 0, 10)

                    For i = 0 To nTim - 1
                        myStr = ""
                        Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, nTim)
                        curDate = StartDate.AddSeconds(TimeStep.TotalSeconds * i)

                        tmpStr = Format(curDate, "yyyy/MM/dd HH:mm:ss")
                        For j = 0 To MeteoStations.MeteoStations.Values.Count - 1
                            tmpStr &= ";" & Format(Values(i, j), Formatting)
                        Next
                        csvWriter.WriteLine(tmpStr)
                    Next
                End Using

            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in sub Write of class clsBuiFile.")
        End Try

    End Sub

End Class
