Imports STOCHLIB.General

Public Class clsTidalElevatedClasses
  'This class contains series of tidal events, subdivided into amplitude classes
  'Note: the key is used for the upper percentile

  Public Classes As New Dictionary(Of Double, clsTidalElevatedClass)
  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Classes = New Dictionary(Of Double, clsTidalElevatedClass)
    Setup = mySetup
  End Sub

  Public Function GetClassByValue(ByVal Value As Double) As clsTidalElevatedClass
    For Each theClass As clsTidalElevatedClass In Classes.Values
      If Value >= theClass.lBoundVal AndAlso Value <= theClass.uBoundVal Then
        Return theClass
      End If
    Next

    'if we end up here, the value lies either below the lowest or above the highest
    'assume the first class represents the lowest value and the last class the highest
    If Value <= Classes.Values(0).lBoundVal Then
      Return Classes.Values(0)
    ElseIf Value >= Classes.Values(Classes.Count - 1).uBoundVal Then
      Return Classes.Values(Classes.Count - 1)
    Else
      Return Nothing
    End If

    Return Nothing
  End Function

End Class
