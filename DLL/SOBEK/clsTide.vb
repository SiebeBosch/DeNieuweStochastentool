Public Class clsTide

  Friend Enum enmTide
    High = 0
    Low = 1
    Inflow = 2
    OutFlow = 3
  End Enum

  Friend DateTime As Date
  Friend Value As Double
  Friend Tide As enmTide

End Class
