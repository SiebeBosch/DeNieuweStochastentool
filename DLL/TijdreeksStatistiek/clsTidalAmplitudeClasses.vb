Imports STOCHLIB.General

Public Class clsTidalAmplitudeClasses
  'This class contains series of tidal events, subdivided into amplitude classes
  'Note: the key is used for the upper percentile

  Public Classes As New Dictionary(Of Double, clsTidalAmplitudeClass)
  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Classes = New Dictionary(Of Double, clsTidalAmplitudeClass)
    Setup = mySetup
  End Sub

End Class
