Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Friend Class clsSobekDataFile
    Friend Path As String
    Friend FileContent As String
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function ReadOld(ByVal path As String, ByVal token As String) As Collection
        ' Check if the file exists:
        If Not System.IO.File.Exists(path) Then
            setup.Log.AddError("File: " & path & " does not yet exist. Please open the SOBEK model, save the network and save the case. Then restart the application.")
            Throw New FileNotFoundException("Cannot the file", path)
        End If

        Me.setup.GeneralFunctions.UpdateProgressBar("Reading  " & path & ".", 0, 10)

        Try
            'This routine reads any standard SOBEK Datafile that contains Records and Tokens, and returns a collection of strings
            Dim n As Long, i As Long, Done As Boolean, tmpstr As String, myRecords As New Collection
            Dim pstart As Integer, pend As Integer

            Dim myFile As New StreamReader(path)
            FileContent = myFile.ReadToEnd()
            myFile.Close()

            FileContent = Replace(Trim(FileContent), vbCrLf, " ")
            While Not InStr(1, FileContent, "  ") <= 0
                FileContent = Replace(FileContent, "  ", " ")
            End While

            Done = False
            tmpstr = FileContent
            If Not FileContent Is Nothing Then
                n = FileContent.Length
                While Not Done
                    pstart = InStr(1, tmpstr, UCase(token) & " ", vbBinaryCompare)
                    pend = InStr(1, tmpstr, " " & LCase(token), vbBinaryCompare) + 1
                    If pstart > 0 And pend > pstart Then
                        myRecords.Add(Mid(tmpstr, pstart, pend - pstart + Len(token)))
                        tmpstr = Left(tmpstr, pstart - 1) & Right(tmpstr, Len(tmpstr) - pend - Len(token) + 1).Trim
                    Else
                        Done = True
                    End If
                    If Not tmpstr Is Nothing Then i = tmpstr.Length
                    Me.setup.GeneralFunctions.UpdateProgressBar("", (n - i), n, True)  'we forceren hier telkens een update van de progress bar omdat het in stappen van hele records gaat
                End While
            End If
            Return myRecords
        Catch ex As Exception
            Dim log As String = "Error in Read of clsSobekDataFile"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try
    End Function

    Friend Function Read(ByVal path As String, ByVal token As String) As Collection
        ' Check if the file exists:
        Dim RecordsSplit() As String
        Dim ErrMsg As String

        If Not System.IO.File.Exists(path) Then
            ErrMsg = "File: " & path & " does not yet exist. Please open the SOBEK model, save the network and save the case. Then restart the application."
            setup.Log.AddError(ErrMsg)
            Throw New FileNotFoundException(ErrMsg)
        End If

        Me.setup.GeneralFunctions.UpdateProgressBar("Reading  " & path & ".", 0, 10)

        Try

            'This routine reads any standard SOBEK Datafile that contains Records and Tokens, and returns a collection of strings
            Dim n As Long, i As Long, myRecords As New Collection
            Dim pstart As Integer, myRecord As String

            Dim myFile As New StreamReader(path)
            FileContent = myFile.ReadToEnd()
            myFile.Close()

            FileContent = Replace(Trim(FileContent), vbCrLf, " ") 'vervang alle harde returns door een spatie
            While Not InStr(1, FileContent, "  ") <= 0
                FileContent = Replace(FileContent, "  ", " ")
            End While

            'splits de filecontent op naar token (onderkast! + de spatie die erachter kwam agv het vervangen van de vbCrLf)
            RecordsSplit = Split(FileContent, " " & token.Trim.ToLower & " ", -1, vbBinaryCompare)

            n = RecordsSplit.Count
            For i = 0 To n - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, n, False)
                'Debug.Print(i)
                pstart = InStr(RecordsSplit(i), token.Trim.ToUpper & " ", CompareMethod.Binary)
                If pstart > 0 Then
                    myRecord = Right(RecordsSplit(i), RecordsSplit(i).Length - pstart - token.Length + 1)
                    myRecords.Add(myRecord.Trim)
                End If
            Next
            Return myRecords

        Catch ex As Exception
            Dim log As String = "Error in Read of clsSobekDataFile while processing " & path
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try

    End Function

    Friend Function ReadByRecordType(ByVal path As String, ByVal RecordType As String, ByVal Records As Collection) As Boolean
        'This routine reads any standard SOBEK Datafile that contains Records and tokens, and returns an array of strings
        Dim RecordsArray() As String
        Dim i As Long

        ReadByRecordType = False

        Dim myFile As New StreamReader(path)
        FileContent = myFile.ReadToEnd()
        myFile.Close()

        FileContent = Trim(Replace(FileContent, vbCrLf, " ")) 'read file and remove hard returns by spaces
        While Not InStr(1, FileContent, "  ") <= 0
            FileContent = Replace(FileContent, "  ", " ") 'remove double spaces
        End While

        RecordsArray = Split(FileContent, " " & LCase(RecordType), -1, vbBinaryCompare)
        For i = 0 To UBound(RecordsArray) - 1
            If Trim(Len(RecordsArray(i))) > 0 Then Call Records.Add(RecordsArray(i))
        Next
        Return True

    End Function

End Class
