Imports STOCHLIB.General

Imports MatFileHandler
Imports System.IO
Imports System.Data

''' <summary>
''' This class describes a Matlab .mat file. It is used to read the file and extract the data.
''' The class is used to read the results of a Sumaqua simulation.
''' In HydroToolbox we used the csmatio library; however this library is not capable of reading the opaque datasets as present in Sumaqua results files
''' Therefore we switched to the MatFilehandler library
''' </summary>

Public Class clsMatFile
    Private Setup As clsSetup
    Dim Path As String

    Dim Contents As DataTable

    Public Sub New(ByRef mySetup As clsSetup, myPath As String)
        Setup = mySetup
        Path = myPath
    End Sub
    Friend Function ReadSumaquaResultsStatistics(ByRef OutputLocations As Dictionary(Of String, clsSumaquaOutputLocationStatistics), ByVal Path As String) As Boolean
        Try
            Dim matFile As IMatFile
            Using fileStream As New FileStream(Path, FileMode.Open)
                Dim reader = New MatFileReader(fileStream)
                matFile = reader.Read()
            End Using

            ' Assuming the .mat file contains only one variable
            Dim variable = matFile.Variables.First()
            Dim data = variable.Value
            Dim dataArray As Double() = data.ConvertToDoubleArray

            Dim nTim As Integer = data.Dimensions(0)
            Dim nLoc As Integer = data.Dimensions(1)

            For iLoc As Integer = 0 To nLoc - 1
                Dim timeSeries As New List(Of Double)()

                ' Extract the time series for this location
                For iTim As Integer = 0 To nTim - 1
                    Dim index As Integer = iTim + iLoc * nTim
                    timeSeries.Add(dataArray(index))
                Next

                ' Now that we have the time series for the current location, calculate the statistics
                Dim myOutputLocation As New clsSumaquaOutputLocationStatistics(iLoc.ToString) With {
                .Max = timeSeries.Max(),
                .Min = timeSeries.Min(),
                .First = timeSeries.First(),
                .Last = timeSeries.Last(),
                .Mean = timeSeries.Average(),
                .Median = GetMedian(timeSeries)
            }

                OutputLocations.Add(iLoc.ToString, myOutputLocation)
            Next

            Return True
        Catch ex As Exception
            ' Handle errors appropriately
            Setup.Log.AddError($"Error reading .mat file: {ex.Message}")
            Return False
        End Try
    End Function

    Private Function GetMedian(values As List(Of Double)) As Double
        Dim sortedValues = values.OrderBy(Function(x) x).ToList()
        Dim size = sortedValues.Count
        If size Mod 2 = 0 Then
            ' Even number of items.
            Return (sortedValues(size \ 2 - 1) + sortedValues(size \ 2)) / 2
        Else
            ' Odd number of items.
            Return sortedValues(size \ 2)
        End If
    End Function


    Friend Function Read(ByRef OutputLocations As Dictionary(Of String, clsSumaquaOutputLocationStatistics)) As Boolean
        Try
            Dim matFile As IMatFile
            Using fileStream As New FileStream(Path, FileMode.Open)
                Dim reader = New MatFileReader(fileStream)
                matFile = reader.Read()
            End Using

            Dim Contents As New DataTable()

            ' Assuming the .mat file contains only one variable
            Dim variable = matFile.Variables.First()
            Dim nTim As Integer = variable.Value.Dimensions(0)
            Dim nLoc As Integer = variable.Value.Dimensions(1)


            ' Process the variable depending on its actual type and structure
            ' This step requires specific knowledge of the data structure
            ' For example, assuming it's a 2D numeric array, but you will need to adapt this part

            ' Example of adding columns to DataTable for demonstration purposes
            ' Adapt the number of columns and type based on your specific data structure
            For colIndex As Integer = 0 To 4 ' Placeholder for actual column count
                Contents.Columns.Add($"Column{colIndex}", GetType(Double))
            Next

            ' Populate the DataTable with data
            ' This is a placeholder loop - you'll need to replace it with actual data extraction logic
            For rowIndex As Integer = 0 To 9 ' Placeholder for actual row count
                Dim row(4) As Object ' Adjust the size based on actual data structure
                For colIndex As Integer = 0 To 4 ' Placeholder for actual column count
                    row(colIndex) = 0 ' Placeholder for actual data extraction
                Next
                Contents.Rows.Add(row)
            Next

            ' This is a simplified example that needs to be adjusted based on the actual data structure in your .mat file
            Return True
        Catch ex As Exception
            ' Handle errors appropriately
            Setup.Log.AddError($"Error reading .mat file: {ex.Message}")
            Return False
        End Try
    End Function





End Class
