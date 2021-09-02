Public Class clsTimeSeriesRecord

  Public iDateTime As DateTime
  Public Value As Decimal

  Public Sub New(ByVal myDateTime As DateTime, ByVal myValue As Decimal)
    iDateTime = myDateTime
    Value = myValue
  End Sub


End Class
