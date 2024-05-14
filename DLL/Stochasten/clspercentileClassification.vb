Imports DocumentFormat.OpenXml.Spreadsheet
Imports System.IO
Imports STOCHLIB.General

Public Class clspercentileClassification
    'this class describes one classification of events by one or more percentileClasses
    'this can e.g. be one instance of a percentileClass
    'or, in case of multiple parameters, a combination of percentileClasses
    Private Setup As clsSetup

    Public Name As String
    Public Classes As New List(Of clsPercentileClass)

    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Function WriteHBVStateFiles(instatepath As String, state_01path As String, EventStartDate As Date) As Boolean
        'this function writes an instate.dat file (HBV initial state) for the given classification
        Try
            'first make sure the directory exists
            Dim directoryPath As String = Me.Setup.GeneralFunctions.DirFromFileName(instatepath)
            If Not System.IO.Directory.Exists(directoryPath) Then System.IO.Directory.CreateDirectory(directoryPath)

            'EventStartDate = EventStartDate.Subtract(New TimeSpan(0, 1, 0, 0)) 'subtract one hour to make sure the initial state is set before our event starts

            'then write the file
            Using instateWriter As New StreamWriter(instatepath)
                For i = 1 To 64
                    instateWriter.WriteLine("'!!'        ") 'honestly, no clue what this is for
                Next
                instateWriter.WriteLine("'state' 1")
                instateWriter.WriteLine($"'year' {Year(EventStartDate)}")
                instateWriter.WriteLine($"'month' {Month(EventStartDate)}")
                instateWriter.WriteLine($"'day' {Day(EventStartDate)}")
                instateWriter.WriteLine($"'hour' {Hour(EventStartDate)}")
                instateWriter.WriteLine("'!!'        ") 'honestly, no clue what this is for

                'figure out in which class our soil moisture value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.sm Then
                        instateWriter.WriteLine("'sm' 1 " & Strings.Format(Classes(i).RepresentativeValue, "0.00000"))
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.sm) Then
                        instateWriter.WriteLine("'sm' 1 " & Strings.Format(Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.sm), "0.00000"))
                    End If
                Next

                'figure out in which class our upper zone parameter value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.uz Then
                        instateWriter.WriteLine("'uz' 1 " & Strings.Format(Classes(i).RepresentativeValue, "0.00000"))
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.uz) Then
                        instateWriter.WriteLine("'uz' 1 " & Strings.Format(Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.uz), "0.00000"))
                    End If
                Next

                'figure out in which class our lower zone parameter value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.lz Then
                        instateWriter.WriteLine("'lz' 1 " & Strings.Format(Classes(i).RepresentativeValue, "0.00000"))
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.lz) Then
                        instateWriter.WriteLine("'lz' 1 " & Strings.Format(Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.lz), "0.00000"))
                    End If
                Next

                instateWriter.WriteLine("'wcomp' 1 0.00000")
                instateWriter.WriteLine("'wstr' 1 0.00000")
                instateWriter.WriteLine("'!!'        ") 'honestly, no clue what this is for
            End Using

            Using state01Writer As New StreamWriter(state_01path)
                state01Writer.WriteLine("'state' 0")
                state01Writer.WriteLine($"'year' {Year(EventStartDate)}")
                state01Writer.WriteLine($"'month' {Month(EventStartDate)}")
                state01Writer.WriteLine($"'day' {Day(EventStartDate)}")
                state01Writer.WriteLine($"'hour' {Hour(EventStartDate)}")
                state01Writer.WriteLine($"'sp'            1         1.10000")
                state01Writer.WriteLine($"'wc'            1         0.00000")
                'figure out in which class our soil moisture value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.sm Then
                        state01Writer.WriteLine("'sm'            1       " & Strings.Format(Classes(i).RepresentativeValue, "0.00000"))
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.sm) Then
                        state01Writer.WriteLine("'sm'            1       " & Strings.Format(Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.sm), "0.00000"))
                    End If
                Next

                'figure out in which class our upper zone parameter value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.uz Then
                        state01Writer.WriteLine("'uz'            1         " & Strings.Format(Classes(i).RepresentativeValue, "0.00000"))
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.uz) Then
                        state01Writer.WriteLine("'uz'            1         " & Strings.Format(Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.uz), "0.00000"))
                    End If
                Next

                'figure out in which class our lower zone parameter value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.lz Then
                        state01Writer.WriteLine("'lz'            1        " & Strings.Format(Classes(i).RepresentativeValue, "0.00000"))
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.lz) Then
                        state01Writer.WriteLine("'lz'            1        " & Strings.Format(Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.lz), "0.00000"))
                    End If
                Next

                state01Writer.WriteLine($"'wcomp'         1         0.00000")
                state01Writer.WriteLine($"'wstr'          1         0.00000")
                state01Writer.WriteLine($"'qcomp0'        1         0.00000")

            End Using



        Catch ex As Exception
            Me.Setup.Log.AddError("Error writing instate.dat file for classification " & Me.Name & ": " & ex.Message)
            Return False
        End Try
    End Function

End Class
