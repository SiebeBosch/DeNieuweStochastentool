Imports STOCHLIB.General

Public Class clsSpatialExtent
    Public XMin As Double
    Public XMax As Double
    Public YMin As Double
    Public YMax As Double

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myXMin As Double, myYMin As Double, myXMax As Double, myYMax As Double)
        Setup = mySetup
        XMin = myXMin
        YMin = myYMin
        XMax = myXMax
        YMax = myYMax
    End Sub

    Public Function RDToWebMercator() As clsSpatialExtent
        Dim LatMin As Double, LonMin As Double, LatMax As Double, LonMax As Double
        Setup.GeneralFunctions.RD2WGS84(XMin, YMin, LatMin, LonMin)
        Setup.GeneralFunctions.RD2WGS84(XMax, YMax, LatMax, LonMax)
        Dim myExtent As New clsSpatialExtent(Me.Setup, LonMin, LatMin, LonMax, LatMax)
        Return myExtent
    End Function


End Class
