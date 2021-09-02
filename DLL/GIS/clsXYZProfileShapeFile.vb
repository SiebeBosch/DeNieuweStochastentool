Imports System.Windows.Forms
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsXYZProfileShapeFile
    Public Path As String
    Public sf As New MapWinGIS.Shapefile
    Public InUse As Boolean

    Public IdFieldIdx As Integer = -1
    Public PointOrderValueFieldIdx As Integer = -1
    Public ProfileCategoryFieldIdx As Integer = -1
    Public PointCategoryFieldIdx As Integer = -1
    Public ZValueFieldIdx As Integer = -1

    Public IdField As String
    Public PointOrderValueField As String
    Public ProfileCategoryField As String
    Public PointCategoryField As String
    Public ZValueField As String

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

    Public Function setProfileCategoryField(ByVal FieldName As String) As Boolean
        ProfileCategoryField = FieldName
        ProfileCategoryFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, ProfileCategoryField)
        Return True
    End Function

    Public Function setPointCategoryField(ByVal FieldName As String) As Boolean
        PointCategoryField = FieldName
        PointCategoryFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, PointCategoryField)
        Return True
    End Function

    Public Function setPointOrderField(ByVal FieldName As String) As Boolean
        PointOrderValueField = FieldName
        PointOrderValueFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, PointOrderValueField)
        Return True
    End Function

    Public Function setZValueField(ByVal FieldName As String) As Boolean
        ZValueField = FieldName
        ZValueFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, ZValueField)
        Return True
    End Function

    Public Function Read(ByRef Setup As clsSetup, ByRef prProgress As ProgressBar, ByRef lblProgress As Label) As Boolean
        'doorloop alle velden in de shapefile van de gebieden
        Dim i As Integer
        Dim errors As New List(Of String)

        Try
            Read = True
            Call Me.setup.GeneralFunctions.UpdateProgressBar("Finding shapefields...", 0, 100)

            IdFieldIdx = -999
            PointOrderValueFieldIdx = -999
            PointCategoryFieldIdx = -999
            ZValueFieldIdx = -999

            sf = New MapWinGIS.Shapefile
            If Not sf.Open(Path) Then
                Me.setup.Log.AddError("Could not open file " & Path)
                Throw New Exception
            End If

            ' TODO: Dit hoeft alleen als de indexen nog niet gezet zijn tijdens de init:
            For i = 0 To sf.NumFields - 1
                Call Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, sf.NumFields)
                If sf.Field(i).Name = IdField Then IdFieldIdx = i
                If sf.Field(i).Name = PointOrderValueField Then PointOrderValueFieldIdx = i
                If sf.Field(i).Name = PointCategoryField Then PointCategoryFieldIdx = i
                If sf.Field(i).Name = ZValueField Then ZValueFieldIdx = i
            Next i

            'foutafhandeling
            If IdFieldIdx < 0 Then Me.setup.Log.AddError("Field for XYZ Profile ID: " & IdField & " not found in shapefile.")
            If PointOrderValueFieldIdx < 0 Then Me.setup.Log.AddError("Field for XYZ Profile Point numbers: " & PointOrderValueFieldIdx & " not found in shapefile.")
            If PointCategoryFieldIdx < 0 Then Me.setup.Log.AddError("Field for XYZ Profile Point categories: " & PointCategoryFieldIdx & " not found in shapefile.")
            If ZValueFieldIdx < 0 Then Me.setup.Log.AddError("Field for XYZ Profile Z values: " & ZValueFieldIdx & " not found in shapefile.")
            If errors.Count > 0 Then Throw New Exception

        Catch ex As Exception
            Dim log As String = "Errors in sub Read of class clsXYZProfileShapeFile"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            For i = 0 To errors.Count - 1
                Me.setup.Log.AddError(errors(i))
            Next i
            ' TODO: Geen exception?
        End Try
    End Function




End Class
