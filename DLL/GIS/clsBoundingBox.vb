Public Class clsBoundingBox
    Public XLL As Double = 9.0E+99
    Public YLL As Double = 9.0E+99
    Public XUR As Double = -9.0E+99
    Public YUR As Double = -9.0E+99

    Public Function calcDistance(ByRef X As Double, ByRef Y As Double) As Double
        'Siebe Bosch, 13 may 2019
        'this function calculates the shortes distance for a given point to a bounding box. If the point is inside the box, a value of 0 is returned
        Dim Xdist As Double, Ydist As Double
        Select Case X
            Case Is < XLL
                Xdist = XLL - X
            Case Is > XUR
                Xdist = X - XUR
            Case Else
                Xdist = 0
        End Select

        Select Case Y
            Case Is < YLL
                Ydist = YLL - Y
            Case Is > YUR
                Ydist = Y - YUR
            Case Else
                Ydist = 0
        End Select

        Return Math.Sqrt(Xdist ^ 2 + Ydist ^ 2)

    End Function

End Class
