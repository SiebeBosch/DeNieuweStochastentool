Imports STOCHLIB.General
Imports GemBox.Spreadsheet
Imports MapWinGIS

Public Class clsPolyLineShapeFile
    Implements MapWinGIS.ICallback

    Public SF As clsShapeFile
    Public IDField As String
    Public IDFieldIdx As Integer = -1
    Public ValField As String
    Public ValFieldIdx As Integer = -1
    Private setup As clsSetup

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Throw New NotImplementedException()
    End Sub

    Friend Sub CreateNew(ByRef mySetup As clsSetup, ByVal Path As String, DeleteExisting As Boolean)
        Try
            Me.setup = mySetup
            If System.IO.File.Exists(Path) Then
                If DeleteExisting Then
                    Me.setup.GeneralFunctions.deleteShapeFile(Path)
                Else
                    Throw New Exception("Shapefile already exists: " & Path)
                End If
            End If
            SF = New clsShapeFile(Me.setup, Path)
            SF.sf.GlobalCallback = Me
            SF.sf.CreateNew(Path, ShpfileType.SHP_POLYLINE)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function New of class clsPolyLineShapefile.")
        End Try
    End Sub

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        SF = New clsShapeFile(Me.setup)
    End Sub

    Public Function Open() As Boolean
        Return SF.Open()
    End Function

    Public Sub Close()
        SF.Close()
    End Sub

    Public Function setIDField(ByVal FieldName As String) As Boolean
        IDField = FieldName
        IDFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(SF.Path, FieldName)
        If IDFieldIdx >= 0 Then Return True Else Return False
    End Function

    Public Function CreateNew(myPath As String, Optional ByVal ReplaceExisting As Boolean = True) As Boolean
        Try
            If System.IO.File.Exists(myPath) Then
                If ReplaceExisting Then setup.GeneralFunctions.deleteShapeFile(myPath)
            End If
            SF.Path = myPath
            Return SF.sf.CreateNew(myPath, ShpfileType.SHP_POLYLINE)
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function AddField(ID As String, FieldType As MapWinGIS.FieldType, Width As Integer, Precision As Integer, ByRef FieldIdx As Integer) As MapWinGIS.Field
        SF.sf.EditAddField(ID, FieldType, Precision, Width)
        FieldIdx = SF.sf.NumFields - 1
        Return SF.sf.Field(FieldIdx)
    End Function

    Public Function getShapeIdxByShapeID(myID As String) As Long
        Dim i As Long
        SF.Open()   'just to make sure it really is open
        For i = 0 To SF.sf.NumShapes - 1
            If SF.sf.CellValue(IDFieldIdx, i).ToString.Trim.ToUpper = myID.Trim.ToUpper Then
                Return i
            End If
        Next
        Return -1
    End Function

    Public Function SetPath(myPath As String) As Boolean
        Try
            SF.Path = myPath
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function StartEditing(Fields As Boolean) As Boolean
        Return SF.sf.StartEditingShapes(Fields)
    End Function

    Public Function StopEditingShapes(ApplyChanges As Boolean, Fields As Boolean) As Boolean
        Return SF.sf.StopEditingShapes(IDFieldIdx, Fields)
    End Function

    Public Sub StopEditing(Shapes As Boolean, Table As Boolean)
        SF.sf.StopEditingShapes(Shapes)
        SF.sf.StopEditingTable(Table)
        'SF.sf.Save()
    End Sub

    Public Function SetValueField(myFieldName As String) As Integer
        Try
            Dim i As Integer
            If Not SF.Open Then Throw New Exception("Error opening point shapefile")
            For i = 0 To SF.sf.NumFields - 1
                If SF.sf.Field(i).Name.Trim.ToUpper = myFieldName.Trim.ToUpper Then
                    ValField = SF.sf.Field(i).Name
                    ValFieldIdx = i
                    Return ValFieldIdx
                End If
            Next
            Throw New Exception("Could not find field " & myFieldName & " in shapefile " & SF.Path)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return -1
        End Try

    End Function

    Public Function CountShapes() As Long
        Try
            'returns the xyz-value for each point in an array of xy-pairs
            If Not SF.Open() Then Throw New Exception("Error opening point shapefile")
            Return SF.sf.NumShapes
            SF.Close()
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return 0
        End Try
    End Function


    Public Function PointDataFromReachDist(ByVal ShapeIdx As Long, ByVal Dist As Double, ByRef X As Double, ByRef Y As Double, ByRef Angle As Double) As Boolean

        'Date: 16-6-2013
        'Author: Siebe Bosch
        'Description: searches for a given shape & distance on the shape the corresponding X- and Y-coordinates as well as the angle of the shape
        'on that particular location
        Dim i As Long

        Try
            Dim myShape As MapWinGIS.Shape
            Dim prevPoint As MapWinGIS.Point, prevDist As Double
            Dim nextPoint As MapWinGIS.Point, nextDist As Double
            myShape = SF.sf.Shape(ShapeIdx)

            prevDist = 0
            nextDist = 0

            For i = 0 To myShape.numPoints - 2
                prevPoint = myShape.Point(i)
                nextPoint = myShape.Point(i + 1)
                nextDist += Math.Sqrt((nextPoint.y - prevPoint.y) ^ 2 + (nextPoint.x - prevPoint.x) ^ 2)

                If nextDist >= Dist Then
                    'interpolate to find the XY-coordinate that belongs to the given distance on the reach
                    X = Setup.GeneralFunctions.Interpolate(prevDist, prevPoint.x, nextDist, nextPoint.x, Dist)
                    Y = Setup.GeneralFunctions.Interpolate(prevDist, prevPoint.y, nextDist, nextPoint.y, Dist)
                    Angle = Me.Setup.GeneralFunctions.LineAngleDegrees(prevPoint.x, prevPoint.y, nextPoint.x, nextPoint.y)
                    Angle = Me.Setup.GeneralFunctions.NormalizeAngle(Angle)
                    Return True
                End If
                prevDist = nextDist

            Next
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PointDataFromReachDist.")
            Return False
        End Try


    End Function


End Class
