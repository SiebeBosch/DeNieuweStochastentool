Imports System.Windows.Forms
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsTrapeziumProfileShapeFile
    Public Path As String
    Public sf As New MapWinGIS.Shapefile
    Public InUse As Boolean

    Public IdFieldIdx As Integer = -1
    Public BedLevelFieldIdx As Integer = -1
    Public BedWidthFieldIdx As Integer = -1
    Public SlopeFieldIdx As Integer = -1
    Public SurfaceLevelFieldIdx As Integer = -1
    Public SurfaceWidthFieldIdx As Integer = -1

    Public IdField As String
    Public BedLevelField As String
    Public BedWidthField As String
    Public SlopeField As String
    Public SurfaceLevelField As String
    Public SurfaceWidthField As String

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub
    Public Function Open() As Boolean
        If Not System.IO.File.Exists(Path) Then
            Return False
        ElseIf Not sf.Open(Path) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Sub Close()
        Me.sf.Close()
    End Sub

    Public Function setPath(ByVal myPath As String) As Boolean
        If System.IO.File.Exists(myPath) Then
            Path = myPath
            Return True
        Else
            Return False
        End If
    End Function

    Public Function setIDField(ByVal FieldName As String) As Boolean
        IdField = FieldName
        IdFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, IdField)
        Return True
    End Function

    Public Function setBedLevelField(ByVal FieldName As String) As Boolean
        BedLevelField = FieldName
        BedLevelFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, BedLevelField)
        Return True
    End Function

    Public Function setBedWidthField(ByVal FieldName As String) As Boolean
        BedWidthField = FieldName
        BedWidthFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, BedWidthField)
        Return True
    End Function

    Public Function setSlopeField(ByVal FieldName As String) As Boolean
        SlopeField = FieldName
        SlopeFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, SlopeField)
        Return True
    End Function

    Public Function setSurfaceLevelField(ByVal FieldName As String) As Boolean
        SurfaceLevelField = FieldName
        SurfaceLevelFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, SurfaceLevelField)
        Return True
    End Function

    Public Function setSurfaceWidthField(ByVal FieldName As String) As Boolean
        SurfaceWidthField = FieldName
        SurfaceWidthFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, SurfaceWidthField)
        Return True
    End Function

End Class
