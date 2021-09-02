Public Class clsWaterBalanceGridCellCollection
    'this class holds four collections of grid cells that together form the basis of a 2D water balance
    'we distinguish 4 kinds of grid cells: left side of a designated polygon/area, right, up and down
    'notice that the same cell can be present in multiple collections at once
    Friend LeftEdgeCells As New List(Of Tuple(Of Integer, Integer))     'grid cells that represent the left edge of the underlying polygon/area
    Friend RightEdgeCells As New List(Of Tuple(Of Integer, Integer))     'grid cells that represent the right edge of the underlying polygon/area
    Friend UpperEdgeCells As New List(Of Tuple(Of Integer, Integer))     'grid cells that represent the upper edge of the underlying polygon/area
    Friend LowerEdgeCells As New List(Of Tuple(Of Integer, Integer))     'grid cells that represent the lower edge of the underlying polygon/area

End Class
