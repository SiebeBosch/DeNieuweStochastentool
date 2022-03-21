Imports STOCHLIB.General
Imports System.IO

Public Class clsIniFile
    Friend Setup As clsSetup
    Friend Chapters As Dictionary(Of Integer, clsIniFileChapter) 'unfortunately we cannot use a string a key since chapters can all have same name


    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        Chapters = New Dictionary(Of Integer, clsIniFileChapter)
    End Sub

    Public Function Read(path As String) As Boolean
        Try
            'this function reads an INI file from a D-Hydro modelschematization.
            'it simply reads the generic structure of this file type. The iterpretation is handled by a separate class for each ini file that inherits this class.
            If Not System.IO.File.Exists(path) Then Throw New Exception("File does not exist: " & path)
            Dim myStr As String
            Dim myItem As String
            Dim i As Integer = 0
            Dim myChapter As clsIniFileChapter = Nothing
            Dim myAttribute As clsIniFileAttribute = Nothing
            Dim myAttr As String

            'we noticed that the values can be separated by space or tab
            Dim Delimiters As New List(Of String)
            Delimiters.Add(" ")
            Delimiters.Add(vbTab)

            Using iniReader As New StreamReader(path)
                While Not iniReader.EndOfStream
                    myStr = iniReader.ReadLine.Trim
                    i = -1
                    Debug.Print(myStr)
                    If Strings.Left(myStr, 1) = "#" OrElse Strings.Left(myStr, 1) = "*" Then
                        'these are comments. No action
                    Else
                        While Not myStr = ""
                            'parse the string until nothing left
                            myItem = Me.Setup.GeneralFunctions.ParseString(myStr, "=").Trim
                            i += 1

                            If InStr(myItem, "[") = 1 Then
                                'we have found a new chapter
                                myChapter = New clsIniFileChapter(myItem)
                                Chapters.Add(Chapters.Count, myChapter)

                            ElseIf i = 0 Then
                                'we have found an attribute name
                                myAttribute = New clsIniFileAttribute(myItem)
                                myChapter.AddAttribute(myAttribute)

                            ElseIf i = 1 Then
                                'we have found one or more values for the attribute. Parse them by spaces
                                While Not myItem = ""
                                    myAttr = Me.Setup.GeneralFunctions.ParseStringMultiDelimiter(myItem, Delimiters)
                                    myAttribute.AddValue(myAttr)
                                End While
                            End If

                        End While
                    End If

                End While
            End Using
            Return True

        Catch ex As Exception

        End Try
    End Function

    Public Function countAttributeValues(ChapterName As String, AttributeName As String) As Integer
        'this function counts the number of attribute values for a given chapter + attribute in our ini file
        For Each myChapter As clsIniFileChapter In Chapters.Values
            If myChapter.Name.Trim.ToUpper = ChapterName.Trim.ToUpper Then
                If myChapter.Attributes.ContainsKey(AttributeName.Trim.ToUpper) Then
                    Return myChapter.Attributes.Item(AttributeName.Trim.ToUpper).CountValues
                End If
            End If
        Next
        Return 0
    End Function

    Public Function getAttributeValue(ChapterName As String, AttributeName As String, Optional ByVal ValueIdx As Integer = 0, Optional ByVal DefaultWhenMissing As String = "") As String

        'this function retrieves an attribute value from our ini file, given its chapter name (including the brackets!) and the attribute's name
        For Each myChapter As clsIniFileChapter In Chapters.Values
            If myChapter.Name.Trim.ToUpper = ChapterName.Trim.ToUpper Then
                If myChapter.Attributes.ContainsKey(AttributeName.Trim.ToUpper) Then
                    If myChapter.Attributes.Item(AttributeName.Trim.ToUpper).CountValues > ValueIdx Then
                        Return myChapter.Attributes.Item(AttributeName.Trim.ToUpper).GetValue(ValueIdx)
                    End If
                End If
            End If
        Next

        'attibute not found. Return the default as provided as an argument when calling this function
        Return DefaultWhenMissing

    End Function


    Public Function getAttributeValues(ChapterName As String, AttributeName As String) As List(Of String)
        'this function retrieves ALL values for a given attribute in our ini file, given its chapter name (including the brackets!) and the attribute's name
        For Each myChapter As clsIniFileChapter In Chapters.Values
            If myChapter.Name.Trim.ToUpper = ChapterName.Trim.ToUpper Then
                If myChapter.Attributes.ContainsKey(AttributeName.Trim.ToUpper) Then
                    For i = 0 To myChapter.Attributes.Item(AttributeName.Trim.ToUpper).CountValues - 1
                        Return myChapter.Attributes.Item(AttributeName.Trim.ToUpper).getValues
                    Next
                End If
            End If
        Next

        'attibute not found. Return the default as provided as an argument when calling this function
        Return New List(Of String)

    End Function


End Class
