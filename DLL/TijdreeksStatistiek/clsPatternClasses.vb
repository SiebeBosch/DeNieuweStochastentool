Imports STOCHLIB.General
Public Class clsPatternClasses
    Friend Setup As clsSetup
    Friend Duration As Integer
    Dim PatternBlock As clsTemporalPatternBlock

    Public Sub New(ByRef mySetup As clsSetup, ByVal myDuration As Integer)
        Setup = mySetup
        Duration = myDuration
    End Sub

    Public Function Generate(Divisions As List(Of Integer), PercentageValues As List(Of Integer)) As Boolean
        'this function creates a set of temporal pattern classes, based on a given list of temporal subdivisions and percentage classes
        'we will start by setting the parent block. this is just one block that will contain the sum of the entire event

        Dim nBranches As Integer = 1
        Dim iBranch As Integer = 1
        For i = 0 To Divisions.Count - 1
            nBranches *= Divisions(i)
        Next


        PatternBlock = New clsTemporalPatternBlock(Me.Setup, 100)       'the mother block represents a full 100% by herself
        PatternBlock.CreateBranches(Duration, Divisions)

        PatternBlock.Subdivide(Duration, Divisions, PercentageValues, iBranch, nBranches)
    End Function



End Class
