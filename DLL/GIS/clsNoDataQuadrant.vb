Imports STOCHLIB.General

Public Class clsNoDataQuadrant
    Private Setup As clsSetup
    Private Raster As clsRaster

    Public ID As String
    Public rStart As Integer, rEnd As Integer
    Public cStart As Integer, cEnd As Integer
    Dim nRows As Integer
    Dim nCols As Integer

    Dim r As Integer, c As Integer

    Public Sub New(ByRef mySetup As clsSetup, ByRef myRaster As clsRaster, myID As String, startRow As Integer, endRow As Integer, startCol As Integer, endCol As Integer)
        Setup = mySetup
        Raster = myRaster
        ID = myID
        rStart = startRow
        rEnd = endRow
        cStart = startCol
        cEnd = endCol
        nRows = rEnd - rStart + 1 'the +1 is because the rstart is included in the selection
        nCols = cEnd - cStart + 1
    End Sub

    Dim Q1 As clsNoDataQuadrant 'upper left quadrant
    Dim Q2 As clsNoDataQuadrant 'upper right quadrant
    Dim Q3 As clsNoDataQuadrant 'lower left quadrant
    Dim Q4 As clsNoDataQuadrant 'lower right quadrant

    Public Function Evaluate(ByRef myBlocks As Dictionary(Of String, clsNoDataQuadrant)) As Boolean
        Try
            Debug.Print("Evaluating block " & ID)
            Debug.Print("sections " & rStart & "," & cStart & ";" & rEnd & "," & cEnd)
            If EntireQuadrantIsNodata() Then
                myBlocks.Add(Me.ID, Me)
                Return True
            ElseIf nRows > 1 OrElse nCols > 1 Then
                Subdivide(myBlocks) 'our quadrant needs further subdivision
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function EntireQuadrantIsNodata() As Boolean
        'this funciton check if this entire quadrant consists of nodata-values
        For r = rStart To rEnd
            For c = cStart To cEnd
                If Raster.Grid.Value(c, r) <> Raster.Grid.Header.NodataValue Then
                    Return False
                End If
            Next
        Next
        Return True
    End Function

    Public Function Subdivide(ByRef myBlocks As Dictionary(Of String, clsNoDataQuadrant)) As Boolean
        Try
            Dim rSplit As Integer, cSplit As Integer

            If rStart = 33 AndAlso rEnd = 34 AndAlso cStart = 13 AndAlso cEnd = 14 Then Stop

            If nRows > 1 AndAlso nCols > 1 Then
                'we have found a block that can be split in four quadrants
                rSplit = rStart + Me.Setup.GeneralFunctions.RoundUD(nRows / 2, 0, False) - 1 'let op hier. omdat rsplit zelf ook meedoet aan het eerste blok doen we -1
                cSplit = cStart + Me.Setup.GeneralFunctions.RoundUD(nCols / 2, 0, False) - 1 'let op hier. omdat csplit zelf ook meedoet aan het eerste blok doen we -1

                Q1 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".1", rStart, rSplit, cStart, cSplit)
                Q2 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".2", rStart, rSplit, cSplit + 1, cEnd)
                Q3 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".3", rSplit + 1, rEnd, cStart, cSplit)
                Q4 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".4", rSplit + 1, rEnd, cSplit + 1, cEnd)

                'and test these blocks
                Q1.Evaluate(myBlocks)
                Q2.Evaluate(myBlocks)
                Q3.Evaluate(myBlocks)
                Q4.Evaluate(myBlocks)
            ElseIf nRows > 1 Then
                'we can only subdivide rows in this stage
                rSplit = rStart + Me.Setup.GeneralFunctions.RoundUD(nRows / 2, 0, False)
                Q1 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".1", rStart, rSplit, cStart, cEnd)
                Q2 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".2", rSplit + 1, rEnd, cStart, cEnd)
            ElseIf nCols > 1 Then
                'we can only subdivide columns in this stage
                cSplit = cStart + Me.Setup.GeneralFunctions.RoundUD(nCols / 2, 0, False)
                Q1 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".1", rStart, rEnd, cStart, cSplit)
                Q2 = New clsNoDataQuadrant(Me.Setup, Raster, ID & ".2", rStart, rEnd, cSplit + 1, cSplit)
            Else
                'we're left with just one cell. Add it as a separate block
                If EntireQuadrantIsNodata() Then
                    myBlocks.Add(Me.ID, Me)
                End If
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function



End Class
