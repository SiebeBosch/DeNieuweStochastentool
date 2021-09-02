Imports STOCHLIB.General

Public Class clsLineSegment
    Dim startPoint As MapWinGIS.Point
    Dim endPoint As MapWinGIS.Point
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByRef mystart As MapWinGIS.Point, ByRef myend As MapWinGIS.Point)
        Setup = mySetup
        startPoint = mystart
        endPoint = myend
    End Sub

    Public Function makeLineDefinition() As clsLineDefinition
        Return New clsLineDefinition(Me.Setup, startPoint, endPoint)
    End Function

    Public Function Intersects(otherSegment As clsLineSegment, ByRef Chainage As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        'this function intersects a line segment with another line segments
        'it returns true if an intersection was found, but also the corresponding chainage and X,Y coordinate
        Dim myLineDef As clsLineDefinition = makeLineDefinition()
        Dim newLineDef As clsLineDefinition = otherSegment.makeLineDefinition
        myLineDef.Intersect(newLineDef, X, Y)

        Dim xMin As Double, yMin As Double, xMax As Double, yMax As Double
        xMin = Math.Max(Math.Min(startPoint.x, endPoint.x), Math.Min(otherSegment.startPoint.x, otherSegment.endPoint.x))
        yMin = Math.Max(Math.Min(startPoint.y, endPoint.y), Math.Min(otherSegment.startPoint.y, otherSegment.endPoint.y))
        xMax = Math.Min(Math.Max(startPoint.x, endPoint.x), Math.Max(otherSegment.startPoint.x, otherSegment.endPoint.x))
        yMax = Math.Min(Math.Max(startPoint.y, endPoint.y), Math.Max(otherSegment.startPoint.y, otherSegment.endPoint.y))

        If X >= xMin AndAlso X <= xMax AndAlso Y >= yMin AndAlso Y <= yMax Then
            Chainage = Setup.GeneralFunctions.Pythagoras(X, Y, startPoint.x, startPoint.y)
            Return True
        End If
        Return False
    End Function

End Class
