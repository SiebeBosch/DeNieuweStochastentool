Imports STOCHLIB.General
Imports GemBox.Spreadsheet
Imports MapWinGIS

Public Class ClsPolyShapeFile
    Implements MapWinGIS.ICallback

    Public Path As String
    Public sf As New MapWinGIS.Shapefile

    Public ValueField As String
    Public ValueFieldIdx As Integer = -1

    Private Setup As clsSetup


    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        sf.GlobalCallback = Me
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String)
        Setup = mySetup
        Path = myPath
        sf.GlobalCallback = Me
    End Sub

    Public Function calcTotalArea() As Double
        Dim TotalArea As Double = 0
        For i = 0 To sf.NumShapes - 1
            TotalArea += sf.Shape(i).Area
        Next
        Return TotalArea
    End Function
    Public Function GetFieldIdx(FieldName As String) As Integer
        Return sf.FieldIndexByName(FieldName)
    End Function

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.Setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Select Case ErrorMsg
            Case Is = "Table: Index Out of Bounds"
                'door negatieve veldindex in shapefile. Dit gebruiken we actief als feature, dus niet als foutmelding afhandelen.
            Case Is = "Shapefile: Resulting shapefile has no shapes"
                Me.Setup.Log.AddMessage("An operation on " & Path & " resulted in empty shapefile: " & ErrorMsg)
            Case Else
                Me.Setup.Log.AddError("Error returned from MapWinGIS Callback function: " & ErrorMsg)
                Me.Setup.Log.AddError(ErrorMsg)
        End Select
    End Sub


    Public Sub Create()
        sf.CreateNew(Path, MapWinGIS.ShpfileType.SHP_POLYGON)
    End Sub

    Public Sub StartEditing()
        sf.StartEditingShapes(True)
    End Sub

    Public Function SetPath(myPath As String) As Boolean
        Try
            Path = myPath
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function Open(Optional ByVal BeginPointInShapefile As Boolean = False) As Boolean
        If sf.Open(Path) Then
            If BeginPointInShapefile Then sf.BeginPointInShapefile()
            Return True
        Else
            Return False
        End If
    End Function

    Public Function Close(Optional ByVal EndPointInShapefile As Boolean = False) As Boolean
        If sf.Close Then
            If EndPointInShapefile Then sf.EndPointInShapefile()
            Return True
        Else
            Return False
        End If
    End Function

    Public Function Save() As Boolean
        'if filename is present in the sf object, then sf.StopEditingShapes will also save the file
        'sf.StopEditingShapes(True, True)

        'v1.820: if the previous routine did not result in a saved shapefile, perform the next
        'If Not System.IO.File.Exists(sf.Filename) Then
        If sf.SaveAs(Path) Then
            Return True
        Else
            Return False
        End If
        'Else
        'Return True
        'End If
    End Function

    Public Function AddTextField(Name As String) As Boolean
        Try
            sf.EditAddField(Name, MapWinGIS.FieldType.STRING_FIELD, 5, 20)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function AddShape(ID As String, Coords As List(Of clsCoordinaat)) As Boolean
        Try
            Dim newShape As New MapWinGIS.Shape
            newShape.Create(MapWinGIS.ShpfileType.SHP_POLYGON)
            For Each myCoord As clsCoordinaat In Coords
                newShape.AddPoint(myCoord.X, myCoord.Y)
            Next
            sf.EditAddShape(newShape)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setValueField(ByVal FieldName As String) As Boolean
        ValueField = FieldName
        ValueFieldIdx = Setup.GISData.getShapeFieldIdxFromFileName(Path, FieldName)
        If ValueFieldIdx >= 0 Then Return True Else Return False
    End Function


    Public Function SelectShapeByID(idFieldIdx As Integer, ID As String, ClearExisting As Boolean) As Boolean
        Try
            For i = 0 To sf.NumShapes - 1
                If sf.CellValue(idFieldIdx, i).ToString.Trim.ToUpper = ID.Trim.ToUpper Then
                    sf.ShapeSelected(i) = True
                ElseIf ClearExisting Then
                    sf.ShapeSelected(i) = False
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SelectShapeByID of class clsPolyShapefile.")
            Return False
        End Try
    End Function

End Class
