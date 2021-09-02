Public Class clsPOTSeries
  'bevat voor één object het resultaat van een volledige POT-analyse
  'key = indexnummer van het POT-resultaat, gesorteerd van hoog naar laag
  Public ID As String
  Public Results As New Dictionary(Of Integer, clsPOTResult)
End Class
