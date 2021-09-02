Public Class clsTwoPointsPattern
    Dim Points As New List(Of clsPatternPoint)

    Public Function Create(Timesteps As Integer, Point1Idx As Integer, Point2Idx As Integer, Point1Perc As Double) As Boolean
        'given the number of timesteps and the position of both points in the timeseries and the percentage value for the first point we can now calculate the percentage for the 
        Try
            'first triangle is known. Calculate its fraction of the total
            Dim LeftTrianglePercentage As Double = (Point1Idx + 1 * Point1Perc) / 2 * (Point1Idx + 1) / Timesteps
            Dim Remainder = 100 - LeftTrianglePercentage

            'the remainder consists of the right triangle, the triangle of the mid section and the rectangle of the mid section
            'Remainder = (Point2idx - Point1idx)*(math.min(Point1Perc, Point2Perc) + math.abs(Point1Perc-Point2Perc)*(Point2Idx - Point1Idx)/2) + ((Timesteps-1)-Point2Idx)*Point2Perc/2




        Catch ex As Exception

        End Try


    End Function


End Class
