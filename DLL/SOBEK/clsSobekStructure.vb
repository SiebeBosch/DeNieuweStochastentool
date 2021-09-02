Option Explicit On

Public Class clsSobekStructure

  'this class contains a general description of sobek structures. Not an official record inside a case, but merely
  'for temporary storage of structure data which can later be exported or embedded in an existing sobek schematisation

  Public ID As String
  Public Name As String
  Public Type As STOCHLIB.GeneralFunctions.enmSobekStructureType

  Public Inlet As Boolean
  Public Cap1 As Double
  Public Cap2 As Double
  Public Width As Double
  Public InDummy As Boolean

  Public X As Double
  Public Y As Double


End Class
