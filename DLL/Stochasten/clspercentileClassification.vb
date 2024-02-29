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

    Public Function WriteInstateDatFile(path As String) As Boolean
        'this function writes an instate.dat file (HBV initial state) for the given classification
        Try
            If Not System.IO.Directory.Exists(path) Then System.IO.Directory.CreateDirectory(path)
            Using datWriter As New StreamWriter(path)
                For i = 1 To 64
                    datWriter.WriteLine("'!!'        ") 'honestly, no clue what this is for
                Next
                datWriter.WriteLine("'state' 1")
                datWriter.WriteLine("'year' 1900")
                datWriter.WriteLine("'month' 1")
                datWriter.WriteLine("'hour' 1")
                datWriter.WriteLine("'!!'        ") 'honestly, no clue what this is for

                'figure out in which class our soil moisture value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.sm Then
                        datWriter.WriteLine("'sm' 1 " & Classes(i).RepresentativeValue)
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.sm) Then
                        datWriter.WriteLine("'sm' 1 " & Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.sm))
                    End If
                Next

                'figure out in which class our upper zone parameter value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.uz Then
                        datWriter.WriteLine("'uz' 1 " & Classes(i).RepresentativeValue)
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.uz) Then
                        datWriter.WriteLine("'uz' 1 " & Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.uz))
                    End If
                Next

                'figure out in which class our lower zone parameter value resides and write it to the file
                For i = 0 To Classes.Count - 1
                    If Classes(i).Parameter = GeneralFunctions.enmModelParameter.lz Then
                        datWriter.WriteLine("'lz' 1 " & Classes(i).RepresentativeValue)
                    ElseIf Classes(i).SideParameterValues.ContainsKey(GeneralFunctions.enmModelParameter.lz) Then
                        datWriter.WriteLine("'lz' 1 " & Classes(i).SideParameterValues.Item(GeneralFunctions.enmModelParameter.lz))
                    End If
                Next

                datWriter.WriteLine("'wcomp' 1 0.00000")
                datWriter.WriteLine("'wstr' 1 0.00000")
                datWriter.WriteLine("'!!'        ") 'honestly, no clue what this is for
            End Using
        Catch ex As Exception

        End Try
    End Function

End Class
