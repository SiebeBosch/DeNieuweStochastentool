Imports STOCHLIB.General

Public Class clsModelAreas

  Public RRPavedCSO As Double     'totaaloppervlak verhard in rioolgebieden binnen de catchment, dus ALLEEN verhard dat via een bekende overstort loost
  Public RRPavedNonCSO As Double  'oppervlak verhard buiten rioolgebieden
  Public RRUnpaved As Double      'oppervlak onverhard RR Unpaved
  Public RROther As Double        'oppervlak onverhard Wageningenmodel

  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
  End Sub

  Public Sub AssignRemainingAreaToWagMod(ByVal TotalArea As Double)
    Dim mySum As Double
    mySum = RRPavedCSO + RRPavedNonCSO + RRUnpaved
    RROther = TotalArea - mySum
  End Sub


End Class
