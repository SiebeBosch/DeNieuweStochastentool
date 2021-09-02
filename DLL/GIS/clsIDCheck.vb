Imports STOCHLIB.General

Public Class clsIDCheck
    Private Setup As clsSetup
    Public Files As New Dictionary(Of String, Integer) 'key = filename, value= fieldindex for the id
    Public IDs As New Dictionary(Of String, List(Of String)) 'key = ID, list=list of files in which an occurance was found

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub AddFile(FileName As String, FieldIndex As Integer)
        'adds a file to the collection
        If Not Files.ContainsKey(FileName.Trim.ToUpper) Then
            Files.Add(FileName.Trim.ToUpper, FieldIndex)
        End If
    End Sub

    Public Function ContainsDoubleIDs() As Boolean
        Try
            Dim sf As New MapWinGIS.Shapefile
            Dim i As Integer, j As Integer, ID As String
            Dim myFile As String
            Dim DoubleFound As Boolean = False

            Me.Setup.GeneralFunctions.UpdateProgressBar("Checking for double ID's...", 0, 10)
            For i = 0 To Files.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Files.Count - 1)
                myFile = Files.Keys(i)
                sf.Open(myFile)
                For j = 0 To sf.NumShapes - 1
                    ID = sf.CellValue(Files.Values(i), j)
                    If IDs.ContainsKey(ID.Trim.ToUpper) Then
                        IDs.Item(ID.Trim.ToUpper).Add(myFile)
                    Else
                        Dim newList As New List(Of String)
                        newList.Add(myFile)
                        IDs.Add(ID.Trim.ToUpper, newList)
                    End If
                Next
                sf.Close()
            Next

            For i = 0 To IDs.Count - 1
                If IDs.Values(i).Count > 1 Then
                    DoubleFound = True
                    Dim myStr As String = "Multiple instances of object ID " & IDs.Keys(i) & " in:" & vbCrLf
                    For j = 0 To IDs.Values(i).Count - 1
                        myStr &= IDs.Values(i).Item(j) & vbCrLf
                    Next
                    Me.Setup.Log.AddWarning(myStr)
                End If
            Next

            Me.Setup.GeneralFunctions.UpdateProgressBar("Checking complete.", 0, 10, True)
            Return DoubleFound
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function HasDoubles of class clsIDCheck.")
            Return False
        End Try
    End Function

End Class
