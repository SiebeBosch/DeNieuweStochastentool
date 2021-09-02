
Option Explicit On

Imports STOCHLIB.General
Imports System.IO
Imports MapWinGIS

Public Class clsPointShapeFile
    Implements MapWinGIS.ICallback

    Public SF As clsShapeFile
    Public IDField As String
    Public IDFieldIdx As Integer = -1
    Public AreaIDField As String
    Public AreaIDFieldIdx As Integer = -1
    Public ValField As String
    Public ValFieldIdx As Integer = -1
    Private setup As clsSetup

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Throw New NotImplementedException()
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal Path As String)
        Me.setup = mySetup
        SF = New clsShapeFile(Me.setup, Path)
        SF.sf.GlobalCallback = Me
    End Sub


    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        SF = New clsShapeFile(Me.setup)
        SF.sf.GlobalCallback = Me
    End Sub

    Public Function GetFieldIdx(FieldName As String) As Integer
        Return SF.sf.FieldIndexByName(FieldName)
    End Function

    Public Function GetShapeIdx(FieldIdx As Integer, FieldValue As Object) As Integer
        Dim i As Integer
        Select Case SF.sf.Field(FieldIdx).Type
            Case Is = MapWinGIS.FieldType.STRING_FIELD
                For i = 0 To SF.sf.NumShapes - 1
                    If SF.sf.CellValue(FieldIdx, i) = CType(FieldValue, String) Then Return i
                Next
            Case Is = MapWinGIS.FieldType.INTEGER_FIELD
                For i = 0 To SF.sf.NumShapes - 1
                    If SF.sf.CellValue(FieldIdx, i) = CType(FieldValue, Integer) Then Return i
                Next
            Case Is = FieldType.DOUBLE_FIELD
                For i = 0 To SF.sf.NumShapes - 1
                    If SF.sf.CellValue(FieldIdx, i) = CType(FieldValue, Double) Then Return i
                Next
            Case Is = FieldType.DATE_FIELD
                For i = 0 To SF.sf.NumShapes - 1
                    If SF.sf.CellValue(FieldIdx, i) = CType(FieldValue, Date) Then Return i
                Next
            Case Is = FieldType.BOOLEAN_FIELD
                For i = 0 To SF.sf.NumShapes - 1
                    If SF.sf.CellValue(FieldIdx, i) = CType(FieldValue, Boolean) Then Return i
                Next
        End Select
        Return -1
    End Function

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

    Public Function setAreaIDField(ByVal FieldName As String) As Boolean
        areaIDField = FieldName
        areaIDFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(SF.Path, FieldName)
        If areaIDFieldIdx >= 0 Then Return True Else Return False
    End Function


    Public Function CreateNew(myPath As String, Optional ByVal ReplaceExisting As Boolean = True) As Boolean
        Try
            If System.IO.File.Exists(myPath) Then
                If ReplaceExisting Then setup.GeneralFunctions.deleteShapeFile(myPath)
            End If
            SF.Path = myPath
            Return SF.sf.CreateNew(myPath, ShpfileType.SHP_MULTIPOINT)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function AddField(ID As String, FieldType As MapWinGIS.FieldType, Width As Integer, Precision As Integer, ByRef FieldIdx As Integer) As MapWinGIS.Field
        SF.sf.EditAddField(ID, FieldType, Precision, Width)
        FieldIdx = SF.sf.NumFields - 1
        Return SF.sf.Field(FieldIdx)
    End Function

    Public Function getShapeIdxByAreaID(myAreaID As String) As Long
        Dim i As Long
        SF.Open()   'just to make sure it really is open
        For i = 0 To SF.sf.NumShapes - 1
            If SF.sf.CellValue(AreaIDFieldIdx, i).ToString.Trim.ToUpper = myAreaID.Trim.ToUpper Then
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

    Public Function GetXYArray() As Double(,)
        Try
            'returns the xyz-value for each point in an array of xy-pairs
            Dim i As Long
            If Not SF.Open() Then Throw New Exception("Error opening point shapefile")
            Dim XY(SF.sf.NumShapes - 1, 2) As Double
            For i = 0 To SF.sf.NumShapes - 1
                XY(i, 0) = SF.sf.Shape(i).Point(0).x
                XY(i, 1) = SF.sf.Shape(i).Point(0).y
                XY(i, 2) = SF.sf.CellValue(i, ValFieldIdx)
            Next
            SF.Close()
            Return XY
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function



End Class
