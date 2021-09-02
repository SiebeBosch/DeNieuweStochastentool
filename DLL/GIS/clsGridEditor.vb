Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Imports MapWinGIS

Public Class clsGridEditor
    Implements MapWinGIS.ICallback

    Public TargetGrid As clsRaster
    Public SourceGrid As clsRaster 'let op: we lezen het brongrid in als clsASCIIgrid ipv MapWindow-grid omwille van de snelheid
    Public SelectionGrid As clsRaster
    Public SelectionShapeFile As clsSubcatchmentDataSource
    Friend Utils As MapWinGIS.Utils

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        TargetGrid = New clsRaster(Me.setup)
        SourceGrid = New clsRaster(Me.setup)
        SelectionGrid = New clsRaster(Me.setup)
        SelectionShapeFile = New clsSubcatchmentDataSource(Me.setup)
        Utils = New MapWinGIS.Utils
    End Sub

    Public Sub SaveTargetGrid()
        Try
            TargetGrid.Grid.Save()
        Catch ex As Exception
        Finally
            SourceGrid.Grid.Close()
            SelectionGrid.Grid.Close()
            TargetGrid.Grid.Close()
        End Try
    End Sub

    Public Sub ChangeNodataValue(ByVal NewVal As Long)
        Dim r As Long, c As Long

        setup.GeneralFunctions.UpdateProgressBar("Changing nodatavalue for target grid.", 0, 10)
        For r = 0 To TargetGrid.nRow - 1
            setup.GeneralFunctions.UpdateProgressBar("", (r + 1), TargetGrid.nRow)
            For c = 0 To TargetGrid.nCol - 1
                If TargetGrid.Grid.Value(c, r) = TargetGrid.NoDataVal Then TargetGrid.Grid.Value(c, r) = NewVal
            Next
        Next
        setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)

        TargetGrid.Grid.Header.NodataValue = NewVal
        TargetGrid.NoDataVal = NewVal

    End Sub


    Public Sub SetValuesFromShapeFile()
        Dim n As Long
        Dim r As Integer, c As Integer
        Dim x As Double, y As Double
        Dim ShapeIdx As Integer

        SelectionShapeFile.Open()
        SelectionShapeFile.PolySF.sf.BeginPointInShapefile()
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing shapefile values to target grid.", 0, n)
        For r = 0 To TargetGrid.Grid.Header.NumberRows - 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", r, TargetGrid.Grid.Header.NumberRows - 1)
            For c = 0 To TargetGrid.Grid.Header.NumberCols - 1
                TargetGrid.getXYCenterFromRC(r, c, x, y)
                ShapeIdx = SelectionShapeFile.PolySF.GetShapeIdxByCoord(x, y,ShapeIdx)
                If ShapeIdx >= 0 Then
                    TargetGrid.Grid.PutFloatWindow(r, r, c, c, SelectionShapeFile.PolySF.sf.CellValue(SelectionShapeFile.SelectionFieldIdx, ShapeIdx))
                End If
            Next
        Next
        SelectionShapeFile.PolySF.sf.EndPointInShapefile()
        SelectionShapeFile.Close()
    End Sub


    Public Sub resetSelectionGrid()
        SelectionGrid = New clsRaster(Me.setup)
    End Sub

    Public Sub resetSelectionShapeFile()
        SelectionShapeFile = New clsSubcatchmentDataSource(Me.setup)
    End Sub

    Public Function setSelectionGrid(ByVal myPath As String) As Boolean
        SelectionGrid.Path = myPath
        If System.IO.File.Exists(SelectionGrid.Path) Then
            If Not SelectionGrid.Read(False) Then
                Return False
            Else
                Return True
            End If
        Else
            Return False
        End If
    End Function

    Public Function setSelectionShapefile(ByVal myPath As String) As Boolean
        SelectionShapeFile.PolySF.Path = myPath
        If System.IO.File.Exists(SelectionShapeFile.PolySF.Path) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function setSelectionShapeField(ByVal myField As String, ByRef ShapeFieldIdx As Integer) As Boolean
        Dim i As Integer
        Try
            If Not SelectionShapeFile.Open() Then Throw New Exception("Error: could not open shapefile " & SelectionShapeFile.PolySF.Path)
            For i = 0 To SelectionShapeFile.PolySF.sf.NumFields - 1
                If SelectionShapeFile.PolySF.sf.Field(i).Name.Trim.ToUpper = myField.Trim.ToUpper Then
                    SelectionShapeFile.SelectionFieldIdx = i
                    ShapeFieldIdx = i
                    Return True
                End If
            Next
            SelectionShapeFile.Close()
            Throw New Exception("Could not find shapefield " & myField & " in shapefile " & SelectionShapeFile.PolySF.Path)
        Catch ex As Exception
            SelectionShapeFile.Close()
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function setSourceGrid(ByVal myPath As String) As Boolean
        SourceGrid.Path = myPath
        If System.IO.File.Exists(SourceGrid.Path) Then
            Return True
        Else
            Return False
        End If
    End Function


    Public Function setTargetGrid(ByVal myPath As String) As Boolean
        TargetGrid.Path = myPath
        If System.IO.File.Exists(TargetGrid.Path) Then
            Return True
        Else
            Return False
        End If
    End Function




    Public Sub InterpolateNodata()
        If Not Utils.GridInterpolateNoData(TargetGrid.Grid) Then Me.setup.Log.AddError("Error interpolating nodatavalue for grid.")
    End Sub

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Throw New NotImplementedException()
    End Sub
End Class
