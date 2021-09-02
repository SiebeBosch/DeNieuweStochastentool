Imports System.Windows.Forms
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsBoundaryShapefile
    Public Path As String
    Public sf As New MapWinGIS.Shapefile
    Public InUse As Boolean

    Public IdFieldIdx As Integer = -1
    Public SelectionFieldIdx As Integer = 1
    Public CategoryFieldIdx As Integer = -1
    Public ValueFieldIdx As Integer = -1

    Public IdField As String
    Public SelectionField As String
    Public CategoryField As String
    Public ValueField As String

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub CreateNew(FileName As String)
        Try
            If System.IO.File.Exists(FileName) Then setup.GeneralFunctions.deleteShapeFile(FileName)

            'add the required fields
            If Not sf.CreateNew(FileName, MapWinGIS.ShpfileType.SHP_POINT) Then Throw New Exception("Error creating new shapefile " & FileName)

            sf.SaveAs(FileName)
            If Not setPath(FileName) Then Throw New Exception("Error: could not set path of newly created shapefile " & FileName)
            Close()
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
        End Try
    End Sub

    Public Function CreateField(ByVal FieldName As String, FieldType As MapWinGIS.FieldType, Precision As Integer, Width As Integer) As Boolean
        If sf.EditAddField(FieldName, FieldType, Precision, Width) >= 0 Then Return True Else Return False
    End Function

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

    Public Function setIDFieldFromFile(ByVal FieldName As String) As Boolean
        IdField = FieldName
        IdFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, IdField)
        Return True
    End Function
    Public Function setIDFieldFromMemory(ByVal FieldName As String) As Boolean
        IdField = FieldName
        IdFieldIdx = Me.setup.GeneralFunctions.GetShapeFieldIdxByName(Me.sf, IdField)
        Return True
    End Function

    Public Function setSelectionCategoryFieldFromFile(ByVal FieldName As String) As Boolean
        SelectionField = FieldName
        SelectionFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, SelectionField)
        Return True
    End Function

    Public Function setSelectionCategoryFieldFromMemory(ByVal FieldName As String) As Boolean
        SelectionField = FieldName
        SelectionFieldIdx = setup.GeneralFunctions.GetShapeFieldIdxByName(Me.sf, SelectionField)
        Return True
    End Function

    Public Function setCategoryFieldFromFile(ByVal FieldName As String) As Boolean
        CategoryField = FieldName
        CategoryFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, CategoryField)
        Return True
    End Function
    Public Function setCategoryFieldFromMemory(ByVal FieldName As String) As Boolean
        CategoryField = FieldName
        CategoryFieldIdx = Me.setup.GeneralFunctions.GetShapeFieldIdxByName(sf, CategoryField)
        Return True
    End Function

    Public Function setValueFieldFromMemory(ByVal FieldName As String) As Boolean
        ValueField = FieldName
        ValueFieldIdx = Me.setup.GeneralFunctions.GetShapeFieldIdxByName(sf, ValueField)
        Return True
    End Function

    Public Function setValueFieldFromFile(ByVal FieldName As String) As Boolean
        ValueField = FieldName
        ValueFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, ValueField)
        Return True
    End Function

    Public Function Read(ByRef Setup As clsSetup, ByRef prProgress As ProgressBar, ByRef lblProgress As Label) As Boolean
        'doorloop alle velden in de shapefile van de gebieden
        Dim i As Integer
        Dim errors As New List(Of String)

        Try
            Read = True
            Call Me.setup.GeneralFunctions.UpdateProgressBar("Finding shapefields...", 0, 100)

            CategoryFieldIdx = -999
            IdFieldIdx = -999
            ValueFieldIdx = -999
            SelectionFieldIdx = -999

            sf = New MapWinGIS.Shapefile
            If Not sf.Open(Path) Then
                Me.setup.Log.AddError("Could not open file " & Path)
                Throw New Exception
            End If

            ' TODO: Dit hoeft alleen als de indexen nog niet gezet zijn tijdens de init:
            For i = 0 To sf.NumFields - 1
                Call Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, sf.NumFields)
                If sf.Field(i).Name = CategoryField Then CategoryFieldIdx = i
            Next i

            'foutafhandeling
            If IdFieldIdx < 0 Then Me.setup.Log.AddError("Field for boundary id: " & IdField & " not found in shapefile.")
            If CategoryFieldIdx < 0 Then Me.setup.Log.AddError("Field for boundary category: " & CategoryField & " not found in shapefile.")
            If SelectionFieldIdx < 0 Then Me.setup.Log.AddError("Field for boundary selection: " & SelectionField & " not found in shapefile.")
            If ValueFieldIdx < 0 Then Me.setup.Log.AddError("Field for boundary value: " & ValueField & " not found in shapefile.")
            If errors.Count > 0 Then Throw New Exception

        Catch ex As Exception
            Dim log As String = "Errors in sub Read of class clsGebiedenShapeFile"""
            Me.setup.Log.AddError(log + ": " + ex.Message)
            For i = 0 To errors.Count - 1
                Me.setup.Log.AddError(errors(i))
            Next i
            ' TODO: Geen exception?
        End Try
    End Function




End Class
