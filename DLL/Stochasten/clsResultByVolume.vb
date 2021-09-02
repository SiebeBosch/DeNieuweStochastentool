Imports STOCHLIB.General

Public Class clsResultByVolume
    'deze klasse bevat voor één unieke combi van locatie en stochatencombinatie de lijst met volumes, bijbehorende stochastenrun en resultaat
    Public Results As New Dictionary(Of Double, clsStochastenResultaat) 'volume als key
    Public Key As String  'het id van de inliggende runs behalve het volume (dus IDexceptVolume)
    Public RunIDexceptVolume As String
    Public LocationName As String

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub AddResult(ByRef myRun As clsStochastenRun, ByVal myMin As Double, ByVal myMax As Double, ByVal myAvg As Double)
        RunIDexceptVolume = myRun.GetIDexceptVolume
        Dim Resultaat As New clsStochastenResultaat(myMin, myMax, myAvg, myRun, Setup)
        Results.Add(Resultaat.Run.VolumeClass.Volume, Resultaat)
    End Sub

    Public Function CalcVerdict() As Double
        Dim i As Long, myVerdict As Double

        'sorteer de dictionary by key (= volume)
        Dim Sorted = From pair In Results Order By pair.Key
        Results = Sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)

        myVerdict = 10
        For i = 1 To Results.Values.Count - 1
            If Results.Values(i).Max < Results.Values(i - 1).Max Then
                myVerdict += (Results.Values(i).Max - Results.Values(i - 1).Max) / 0.05 'iedere 5 cm daling is een punt aftrek
            End If
        Next
        myVerdict = Math.Max(1, myVerdict)
        Return myVerdict

    End Function

    Public Function GetAsDataTable() As DataTable
        'deze functie geeft de inhoud van de tabel terug in een datatabel
        Dim myTable As New DataTable
        Dim newRow As DataRow

        myTable.Columns.Add(New DataColumn("Volume", Type.GetType("System.Double")))
        myTable.Columns.Add(New DataColumn("Resultaat", Type.GetType("System.Double")))

        For Each myResult As clsStochastenResultaat In Results.Values
            newRow = myTable.NewRow
            newRow("Volume") = myResult.Run.VolumeClass.Volume
            newRow("Resultaat") = myResult.Max
            myTable.Rows.Add(newRow)
        Next
        Return myTable

    End Function


End Class
