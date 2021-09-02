Imports STOCHLIB.General

Public Class clsCFStats

  'topology
  Friend nReaches As Long
  Friend nBridges As Long
  Friend nWeirs As Long
  Friend nCulverts As Long
  Friend nLaterals As Long
  Friend nMeas As Long
  Friend nOrifices As Long
  Friend nProfiles As Long
  Friend nPumps As Long
  Friend nRRCFConn As Long
  Friend nUniWeirs As Long
  Friend TotalReachLength As Double

  Private setup As clsSetup
  Private SbkCase As clsSobekCase

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
    Me.setup = mySetup
    Me.SbkCase = myCase
  End Sub
End Class
