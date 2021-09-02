Imports STOCHLIB.General
Imports MapWinGIS

Public Class clsBodemShapeFile
    Implements MapWinGIS.ICallback

    Public Path As String
    Public sf As New MapWinGIS.Shapefile

    Public GTField As String
    Public BodemCodeField As String
    Public BodemCodeFieldIdx As Integer = -1
    Public GTFieldIdx As Integer = -1
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        sf.GlobalCallback = Me
    End Sub


    Public Sub setPath(ByVal myPath As String)
        Path = myPath
    End Sub

    Public Function Open() As Boolean
        Try
            If Not Me.sf.Filename = "" Then
                Me.setup.Log.AddWarning("Area shapefile is already open!")
            Else
                If Not Me.sf.Open(Path) Then Throw New Exception("Could not open soil Shapefile.")
            End If
            Return True

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub Close()
        sf.Close()
    End Sub

    Public Function setBODEMCODEField() As Boolean
        If setup.GISData.getShapeFieldIdxFromFileName(Path, "BODEMCODE") >= 0 Then
            BodemCodeField = "BODEMCODE"
            BodemCodeFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, "BODEMCODE")
            Return True
        ElseIf setup.GISData.getShapeFieldIdxFromFileName(Path, "EERSTE_BOD") >= 0 Then
            BodemCodeField = "EERSTE_BOD"
            BodemCodeFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, "EERSTE_BOD")
            Return True
        Else
            Me.setup.Log.AddError("Error finding shapefield for soil code: expected BODEMCODE or EERSTE_BOD.")
            Return False
        End If
    End Function

    'Public Function setBODLetterField(ByVal FieldName As String) As Boolean
    '  BodFieldCijfer = FieldName
    '  BODFieldLetterIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, FieldName)

    '  If BODFieldLetterIdx >= 0 Then
    '    Return True
    '  Else
    '    Return False
    '  End If

    'End Function

    'Public Function setBODCijferField(ByVal FieldName As String) As Boolean
    '  BodFieldCijfer = FieldName
    '  BODFieldCijferIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, FieldName)

    '  If BODFieldCijferIdx >= 0 Then
    '    Return True
    '  Else
    '    Return False
    '  End If

    'End Function

    Public Function setBODGTField(ByVal FieldName As String) As Boolean
        GTField = FieldName
        GTFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, FieldName)

        If GTFieldIdx >= 0 Then
            Return True
        Else
            Return False
        End If

    End Function


    Friend Function Read() As Boolean

        If Not sf Is Nothing Then
            If Not String.IsNullOrEmpty(sf.Filename) Then
                Me.setup.Log.AddWarning("Soiltypes shapefile is already open!")
                Return False
            End If
        End If

        If Not sf.Open(Path) Then
            Dim log As String = "Failed to open shapefile: " + sf.ErrorMsg(sf.LastErrorCode)
            Me.setup.Log.AddError(log)
            Throw New Exception(log)
        End If

        Return True
    End Function


    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Select Case ErrorMsg
            Case Is = "Table: Index Out of Bounds"
                'door negatieve veldindex in shapefile. Dit gebruiken we actief als feature, dus niet als foutmelding afhandelen.
            Case Else
                Me.setup.Log.AddError("Error returned from MapWinGIS Callback function: " & ErrorMsg)
                Me.setup.Log.AddError(ErrorMsg)
        End Select
    End Sub
End Class
