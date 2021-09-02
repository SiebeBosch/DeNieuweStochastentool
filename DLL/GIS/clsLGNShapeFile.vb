Imports STOCHLIB.General

Public Class clsLGNShapeFile
    Public Path As String
    Public sf As New MapWinGIS.Shapefile

    Public LandUseField As String
    Public LandUseFieldIdx As Integer
    Public LGNVersion As Integer

    Public ReplaceMissingValues As Integer 'the code that will be used to replace missing values

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        ReplaceMissingValues = 1 'set the default to grass to start with
    End Sub

    Public Sub setPath(ByVal myPath As String)
        Path = myPath
    End Sub

    Public Function Open() As Boolean
        Try
            If Not Me.sf.Filename = "" Then
                Me.setup.Log.AddWarning("Area shapefile is already open!")
            Else
                If Not Me.sf.Open(Path) Then Throw New Exception("Could not open Landuse Shapefile.")
            End If
            Return True

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub Close()
        Me.sf.Close()
    End Sub

    Public Function setLGNCodeField(ByVal FieldName As String) As Boolean
        LandUseField = FieldName
        LandUseFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, FieldName)

        If LandUseFieldIdx >= 0 Then
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub setVersion(ByVal Version As Integer)
        LGNVersion = Version
    End Sub

    Public Function getVersion() As Integer
        Return LGNVersion
    End Function

    Friend Function Read() As Boolean
        Dim i As Integer
        Dim errors As New List(Of String)

        If Not sf Is Nothing Then
            If Not String.IsNullOrEmpty(sf.Filename) Then
                Me.setup.Log.AddWarning("Land use shapefile is already open!")
            End If
        End If

        If Not sf.Open(Path) Then
            Dim log As String = "Error opening Land use shapefile: " & sf.ErrorMsg(sf.LastErrorCode)
            Me.setup.Log.AddError(log)
            Throw New Exception(log)
        End If

        'zoek het veld op met daarin de landgebruikscode
        Me.setup.progressLabel.Text = "Finding shapefield for landuse..."
        Me.setup.progressBar.Value = 0

        ' Paul Meems, 5 June 2012: Toegevoegd
        If LandUseFieldIdx = -1 AndAlso Not String.IsNullOrEmpty(LandUseField) Then
            For i = 0 To sf.NumFields - 1
                Me.setup.progressBar.Value = (i + 1) / (sf.NumFields + 1)
                If sf.Field(i).Name = LandUseField Then LandUseFieldIdx = i
            Next i
        End If

        'foutafhandeling
        If LandUseFieldIdx < 0 Then errors.Add("Fieldname " & LandUseField & " for landuse code not found in shapefile.")

        For i = 0 To errors.Count - 1
            Me.setup.Log.AddError(errors(i))
        Next i

        Return True

    End Function


    Public Function Fixup(newPath As String) As Boolean
        Try
            Dim newSf As New MapWinGIS.Shapefile
            If Not sf.Open(Path) Then Throw New Exception("Error reading shapefile " & Path)
            If Not sf.FixUpShapes2(False, newSf) Then Throw New Exception("Error fixing up shapefile " & Path & ". Original shapefiles still being used instead.")
            setup.GeneralFunctions.deleteShapeFile(newPath)     'if an existing shapefile resides here, remove it
            If Not newSf.SaveAs(newPath) Then Throw New Exception("Error saving fixed-up shapefile " & newPath & ". Original shapefiles still being used instead.")
            sf = newSf                                          'replace the existing shapefile by the fixed-up shapefile
            Call setPath(newPath)                               'change the path to the new path
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
