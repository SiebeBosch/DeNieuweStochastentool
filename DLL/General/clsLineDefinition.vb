Imports STOCHLIB.General

Public Class clsLineDefinition

    'y = ax + b
    Public a As Double
    Public b As Double
    Public alphaDeg As Double 'angle in degrees; important in case line direction is relevant

    Public startPoint As New MapWinGIS.Point
    Public endPoint As New MapWinGIS.Point
    Public leftPoint As New MapWinGIS.Point
    Public rightPoint As New MapWinGIS.Point
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myStartPoint As MapWinGIS.Point, ByVal myEndPoint As MapWinGIS.Point)

        Setup = mySetup

        startPoint = myStartPoint
        endPoint = myEndPoint

        'store both points as resp. the starting point and endpoint of the line
        If startPoint.x <= endPoint.x Then
            leftPoint = startPoint
            rightPoint = endPoint
        Else
            leftPoint = endPoint
            rightPoint = startPoint
        End If

        a = (rightPoint.y - leftPoint.y) / (rightPoint.x - leftPoint.x)
        Call calculateB(startPoint.x, startPoint.y)

        'calculates the angle of the line in degrees. relevant in case the line's direction is relevant
        alphaDeg = Me.Setup.GeneralFunctions.LineAngleDegrees(myStartPoint.x, myStartPoint.y, myEndPoint.x, myEndPoint.y)

    End Sub

    Public Sub Calculate(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double)
        a = (Y2 - Y1) / (X2 - X1)
        Call calculateB(X1, Y1)
    End Sub

    Public Sub calculateB(ByVal X As Double, ByVal Y As Double)
        b = Y - a * X
    End Sub

    Public Function MakeLeftPerpendicular(ByVal Xintersect As Double, ByVal Yintersect As Double, ByVal Length As Double) As clsLineDefinition
        Dim perpLine As New clsLineDefinition(Me.Setup)
        perpLine.a = -1 / a             'the a of the perpendicular line = -1/a of the original
        perpLine.calculateB(Xintersect, Yintersect)
        perpLine.alphaDeg = Me.Setup.GeneralFunctions.NormalizeAngle(alphaDeg - 90)
        perpLine.startPoint = New MapWinGIS.Point
        perpLine.startPoint.x = Xintersect
        perpLine.startPoint.y = Yintersect
        Return perpLine
    End Function

    Public Sub getMinDistFromPoint(ByVal X As Double, ByVal Y As Double, ByRef Dist As Double, ByRef Chainage As Double)
        Dim startPointDist As Double = Me.Setup.GeneralFunctions.Pythagoras(startPoint.x, startPoint.y, X, Y)
        Dim endPointDist As Double = Me.Setup.GeneralFunctions.Pythagoras(endPoint.x, endPoint.y, X, Y)

        'this routine calculates the shortest distance from a given point to a given line segment
        'it does so by creating a perpendicular line to the line segment
        'it also takes into account the starting- and ending point of the segment

        Try

            Dim iX As Double, iY As Double

            'this routine first calculates the distance to a point, perpendicular to the current line
            'create a new line, perpendicular to the one we're in right now
            Dim perpLine As New clsLineDefinition(Me.Setup)
            perpLine.a = -1 / a             'the a of the perpendicular line = -1/a of the original
            perpLine.calculateB(X, Y)

            'the intersecting point can now be calculated
            Call Intersect(perpLine, iX, iY)  'iX and iY are the co-ordinates of the intersection

            If iX >= Math.Min(startPoint.x, endPoint.x) AndAlso iX <= Math.Max(startPoint.x, endPoint.x) AndAlso iY >= Math.Min(startPoint.y, endPoint.y) AndAlso iY <= Math.Max(startPoint.y, endPoint.y) Then
                'only return the intersection point if the perpendicular line actually intersects the original inside it's start-end-domain
                Dist = Me.Setup.GeneralFunctions.Pythagoras(X, Y, iX, iY)
                Chainage = Me.Setup.GeneralFunctions.Pythagoras(startPoint.x, startPoint.y, iX, iY)
            Else
                If startPointDist < endPointDist Then
                    Dist = startPointDist
                    Chainage = 0
                Else
                    Dist = endPointDist
                    Chainage = Me.Setup.GeneralFunctions.Pythagoras(startPoint.x, startPoint.y, endPoint.x, endPoint.y)
                End If
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub getMinDistFromPoint of class clsLineDefinition.")
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Sub

    Public Sub Intersect(ByVal myLine As clsLineDefinition, ByRef X As Double, ByRef Y As Double)
        'calculates the intersection point of two straight lines
        'y = ax + b so a1x + b1 = a2x + b2
        '(a1 - a2)x = b2 - b1 so x = (b2 - b1)/(a1 - a2)
        X = (myLine.b - b) / (a - myLine.a)
        Y = a * X + b
    End Sub

End Class
