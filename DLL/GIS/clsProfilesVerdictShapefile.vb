Imports STOCHLIB.General

Public Class clsProfilesVerdictShapefile
    Inherits clsPolyLineShapeFile

    Public ProfileTypeFieldIdx As Integer = -1
    Public NPointsFieldIdx As Integer = -1
    Public X1FieldIdx As Integer = -1
    Public Y1FieldIdx As Integer = -1
    Public X2FieldIdx As Integer = -1
    Public Y2FieldIdx As Integer = -1
    Public VerdictFieldIdx As Integer = -1
    Public CommentFieldIdx As Integer = -1

    Private Setup As clsSetup

    Public Sub New(mySetup As clsSetup, Filename As String)
        MyBase.New(mySetup)
        SetPath(Filename)
        Setup = mySetup
        If System.IO.File.Exists(Filename) Then Setup.GeneralFunctions.deleteShapeFile(Filename)

        'add the required fields
        SF.sf.CreateNew(Filename, MapWinGIS.ShpfileType.SHP_POLYLINE)
        StartEditing(True)
        AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 80, 0, IDFieldIdx)
        AddField("PROFILETYPE", MapWinGIS.FieldType.STRING_FIELD, 80, 0, ProfileTypeFieldIdx)
        AddField("NPOINTS", MapWinGIS.FieldType.INTEGER_FIELD, 10, 0, NPointsFieldIdx)
        AddField("X1", MapWinGIS.FieldType.DOUBLE_FIELD, 20, 10, X1FieldIdx)
        AddField("Y1", MapWinGIS.FieldType.DOUBLE_FIELD, 20, 10, Y1FieldIdx)
        AddField("X2", MapWinGIS.FieldType.DOUBLE_FIELD, 20, 10, X2FieldIdx)
        AddField("Y2", MapWinGIS.FieldType.DOUBLE_FIELD, 20, 10, Y2FieldIdx)
        AddField("VERDICT", MapWinGIS.FieldType.INTEGER_FIELD, 10, 0, VerdictFieldIdx)
        AddField("COMMENT", MapWinGIS.FieldType.STRING_FIELD, 180, 0, CommentFieldIdx)
        SF.sf.SaveAs(Filename)
        Close()
    End Sub

    Public Function AddVerdict(ByVal Coords As List(Of Tuple(Of Double, Double)), ID As String, ProfileType As String, Verdict As Integer, Comment As String) As Boolean
        Try
            If Coords.Count < 2 Then
                Me.Setup.Log.AddError("Error adding profile with id " & ID & " to profiles verdict shapefile. Number of vertices: " & Coords.Count & ".")
                Return False
            End If

            Dim myLine As New MapWinGIS.Shape, ShapeIdx As Integer
            myLine.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
            For i = 0 To Coords.Count - 1
                myLine.AddPoint(Coords.Item(i).Item1, Coords.Item(i).Item2)
            Next

            ShapeIdx = SF.sf.EditAddShape(myLine)
            If ShapeIdx < 0 Then
                Me.Setup.Log.AddError("Error adding profile with id " & ID & " to profiles verdict shapefile. No valid shape index was returned when adding the shape.")
                Return False
            End If

            SF.sf.EditCellValue(IDFieldIdx, ShapeIdx, ID)
            SF.sf.EditCellValue(ProfileTypeFieldIdx, ShapeIdx, ProfileType)
            SF.sf.EditCellValue(NPointsFieldIdx, ShapeIdx, Coords.Count)
            SF.sf.EditCellValue(X1FieldIdx, ShapeIdx, Coords.Item(0).Item1)
            SF.sf.EditCellValue(Y1FieldIdx, ShapeIdx, Coords.Item(0).Item2)
            SF.sf.EditCellValue(X2FieldIdx, ShapeIdx, Coords.Item(Coords.Count - 1).Item1)
            SF.sf.EditCellValue(Y2FieldIdx, ShapeIdx, Coords.Item(Coords.Count - 1).Item2)
            SF.sf.EditCellValue(VerdictFieldIdx, ShapeIdx, Verdict)
            SF.sf.EditCellValue(CommentFieldIdx, ShapeIdx, Comment)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AddVerdict of class clsProfilesVerdictShapefile.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
