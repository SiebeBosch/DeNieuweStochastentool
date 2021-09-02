Imports STOCHLIB.General

Public Class clsClimateScenario

  Public Vol10x As Double            'volume 10x per jaar overschreden (op basis POT-analyse)
  Public Vol5x As Double             'volume 5x per jaar overschreden (op basis POT-analyse)
  Public Vol2x As Double             'volume 2x per jaar overschreden (op basis POT-analyse)
  Public Vol1x As Double             'volume 1x per jaar overschreden (op basis POT-analyse)

  Public mu As Double         'locatieparameter voor de Gumbel-kansverdeling, geldig voor T>1
  Public beta As Double       'schaalparameter voor de Gumbel-kansverdeling, geldig voor T>1
  Public lambda As Double     'derder parameter?

  Private Setup As clsSetup
  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup

  End Sub

End Class
