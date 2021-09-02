Imports STOCHLIB.General

Public Class clsTemporalPatternBlock

    Dim BlockPercentage As Double                        'the percentage of this block in relation to its sister blocks
    Dim Blocks As List(Of clsTemporalPatternBlock)  'each block can contain subblocks
    Dim Levels As New Dictionary(Of Integer, clsTemporalPatternBranches)


    Dim Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, myPercentage As Double)
        Setup = mySetup
        BlockPercentage = myPercentage
        Blocks = New List(Of clsTemporalPatternBlock)
    End Sub

    Public Sub CreateBranches(Duration As Integer, Divisions As List(Of Integer))

        Dim newLevel As clsTemporalPatternBranches
        Dim Divider As Integer = 1
        For i = 0 To Divisions.Count - 1
            Divider *= Divisions.Item(i)
            newLevel = New clsTemporalPatternBranches(Duration, Divider)
            Levels.Add(i + 1, newLevel)
        Next
    End Sub

    Public Function Subdivide(Duration As Integer, ByVal Divisions As List(Of Integer), Percentages As List(Of Integer), ParentBranchNum As Integer, nBranches As Integer) As Boolean
        'the divisions list represents a tree-like structure to split the event into multiple parts and then subparts
        'e.g. 3,3,2 on a 72 hour event represents a split in 3 parts (3 x 24 = 72 hours), then every part in 3 parts (3 x 8 = 24 hours) and finally 2 parts (2 x 4 = 8 hours)
        Try
            'start by determining the subdivision we make here
            Dim myDivision As Integer = Divisions.Item(0)
            Dim i As Integer, BlockSize As Integer, nBlocks As Integer, mySum As Double
            Dim Counter As New clsMileageCounter(Me.Setup)
            Dim newBlock As clsTemporalPatternBlock
            Dim CombiNumber As Integer
            Counter.Initialize()

            'create a list of subdivisions for the next step (so skip the current one)
            Dim subDivisions As New List(Of Integer)
            If Divisions.Count > 1 Then
                For i = 1 To Divisions.Count - 1
                    subDivisions.Add(Divisions(i))
                Next
            End If

            'no now we'll install the mileage counter
            If Not Duration Mod myDivision = 0 Then Throw New Exception("Error: duration of event, divided by number of divisions should result in a whole number.")

            BlockSize = Duration / myDivision
            nBlocks = Duration / BlockSize
            For i = 0 To nBlocks - 2                        'since the percentages must add up to 100 we can do with one digit less
                Counter.AddDigit(0, Percentages.Count - 1)
            Next

            While Counter.MileageOneUp()
                mySum = 0
                For i = 0 To nBlocks - 2
                    mySum += Percentages(Counter.currentVal(i))
                Next

                'only in case of a valid combination!
                If mySum <= 100 Then
                    'we found a valid pattern. Store it and subdivide if need be
                    For i = 0 To nBlocks - 2
                        newBlock = New clsTemporalPatternBlock(Me.Setup, Percentages(Counter.currentVal(i)))
                        Blocks.Add(newBlock)
                    Next

                    'add the closing block that adds up to 100
                    newBlock = New clsTemporalPatternBlock(Me.Setup, 100 - mySum)
                    Blocks.Add(newBlock)


                    'now that we havee valid blocks, subdivide them if need be
                    For i = 0 To nBlocks - 1
                        CombiNumber = ParentBranchNum * (i + 1)
                        If subDivisions.Count > 0 Then
                            'keep track of whether we are inside the last block (both parent and current). This will tell us when to write to database
                            Blocks(i).Subdivide(BlockSize, subDivisions, Percentages, CombiNumber, nBranches)
                        End If
                        If CombiNumber = nBranches Then
                            'we have processed all branches, so we're ready to write this pattern to the database
                            Dim values As String = ""
                            For j = 1 To Duration
                                values &= "(" & Duration & ",'" & "pitje" & "'," & j & "," & 0.5 & "),"
                            Next
                            Dim query As String = "INSERT INTO PATTERNCLASSES (DURATION, PATTERNID, TIMESTEP, FRACTION) VALUES " & values
                        End If
                    Next

                End If
            End While


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
