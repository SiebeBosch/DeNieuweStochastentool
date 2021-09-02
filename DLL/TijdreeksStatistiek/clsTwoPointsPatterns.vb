Public Class clsTwoPointsPatterns
    Public Sub New(Timesteps As Integer)
        'make sure our points only lay in the timesteps between the first and the last
        For i = 1 To Timesteps - 2
            For j = i + 1 To Timesteps - 2
                'now that we defined the timesteps, calculate all possible combinations of percentages
                For Perc = 0 To 100 Step 10
                    Dim myPattern As New clsTwoPointsPattern()
                    myPattern.create(Timesteps, i, j, Perc)
                Next
            Next
        Next
    End Sub




End Class
