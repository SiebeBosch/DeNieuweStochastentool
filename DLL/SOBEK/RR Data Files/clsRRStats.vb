Imports STOCHLIB.General

Public Class clsRRStats

  'unpaved
  Friend nUP As Long
  Friend avgUPMV As Double
  Friend minUPMV As Double
  Friend maxUPMV As Double
  Friend TotUPArea(16) As Double
  Friend SoilType(121) As Double

  'paved
  Friend nPV As Long
  Friend avgPVMV As Double
  Friend minPVMV As Double
  Friend maxPVMV As Double
  Friend TotPVArea As Double

  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

End Class
