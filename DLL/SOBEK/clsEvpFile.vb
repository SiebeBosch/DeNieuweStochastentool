Option Explicit On
Imports STOCHLIB.General
Imports System.IO

''' <summary>
''' Geen constructor nodig
''' </summary>
''' <remarks></remarks>
Public Class clsEvpFile
  'TODO converteer naar struct
  Friend StartDate As Double
  Friend EndDate As Double
  Friend Table As clsSobekTable

  Private setup As clsSetup

  Public Sub New(ByVal mySetup As clsSetup)
    Me.setup = mySetup
    Table = New clsSobekTable(Me.setup)
  End Sub

  Public Sub Write(ByVal path As String)

    'deze functie schrijft een .evp-file
    Dim i As Long, myDate As DateTime, myVal As Single
    Me.setup.GeneralFunctions.UpdateProgressBar("Writing SOBEK EVP-file.", 0, 10)

    Using evpWriter As New StreamWriter(path, False)
      'doorloop alle areas en schrijf de 'bui' weg
      evpWriter.WriteLine("*Name of this file: " & Me.setup.Settings.ExportDirRoot & "\model.evp")
      evpWriter.WriteLine("*Date and time of construction: " & Now & ".")
      evpWriter.WriteLine("*Evaporation file")
      evpWriter.WriteLine("*Meteo data: evaporation intensity in mm/day")
      evpWriter.WriteLine("*First record: start date, data in mm/day")
      evpWriter.WriteLine("*Datum (year month day), verdamping (mm/dag) voor elk weerstation")
      evpWriter.WriteLine("*jaar maand dag verdamping[mm]")
      For i = 0 To Table.Dates.Count - 1
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing SOBEK EVP-file.", i + 1, Table.Dates.Count)
        myDate = Table.Dates.Item(i)
        myVal = Table.Values1.Item(i)
        evpWriter.WriteLine(" " & Year(myDate) & "  " & Month(myDate) & "  " & Day(myDate) & " " & myVal)
      Next
    End Using

  End Sub

  Public Sub WriteAsCSV(ByVal path As String)

    'deze functie schrijft een .evp-file
    Dim i As Long, tmpStr As String
    Me.setup.GeneralFunctions.UpdateProgressBar("Writing CSV-file.", 0, 10)

    Using csvWriter As New StreamWriter(path, False)
      tmpStr = "Datum;Makkink"
      csvWriter.WriteLine(tmpStr)
      For i = 0 To Table.Dates.Count - 1
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing CSV-file.", i + 1, Table.Dates.Count)
        csvWriter.WriteLine(Format(Table.Dates.Item(i), "yyyy/MM/dd") & ";" & Table.Values1.Item(i))
      Next
    End Using

  End Sub

  Public Sub BuildSTOWATYPE(ByVal Evap() As Double, StartDate As datetime, ByVal UitloopHours As Integer)

        'NOTE: EVAPORATION FILES ALL HAVE DAILY VALUES!
        'werkelijke bui
        Dim i As Long
        For i = 0 To Evap.Count - 1
            Table.AddDatevalPair(StartDate.AddDays(i), Evap(i))
        Next

        'uitloop
        For i = Evap.Count To Evap.Count + setup.GeneralFunctions.RoundUD(UitloopHours / 24, 0, True) - 1
            Table.AddDatevalPair(StartDate.AddDays(i), 0)
        Next

  End Sub

End Class
