Imports STOCHLIB.General

Imports MatFileHandler
Imports System.IO
Imports System.Data
Imports System.Reflection

''' <summary>
''' This class describes a Matlab .mat file. It is used to read the file and extract the data.
''' The class is used to read the results of a Sumaqua simulation.
''' In HydroToolbox we used the csmatio library; however this library is not capable of reading the opaque datasets as present in Sumaqua results files
''' Therefore we switched to the MatFilehandler library
''' </summary>

Public Class clsSumaquaResultsMatFile
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
            ' Read the .mat file
            Dim matFile As IMatFile
            Using fileStream As New FileStream(Path, FileMode.Open)
                Dim reader = New MatFileReader(fileStream)
                matFile = reader.Read()
            End Using

            ' Ensure the .mat file contains at least one variable
            If matFile.Variables.Count = 0 Then
                Throw New InvalidOperationException("The .mat file does not contain any variables.")
            End If

            ' Get the first variable (assuming it's the one we want)
            Dim variable = matFile.Variables.First()

            ' Get dimensions
            Dim dimensions As Integer() = variable.Value.Dimensions
            Dim nTim As Integer = dimensions(0)
            Dim nLoc As Integer = dimensions(1)

            ' Use the ConvertToDoubleArray method to get our data
            Dim flatData As Double() = variable.Value.ConvertToDoubleArray()

            ' Verify that the flat array size matches our dimensions
            If flatData.Length <> nTim * nLoc Then
                Throw New InvalidOperationException("Data size does not match expected dimensions.")
            End If

            'convert our flat data into a 2D array, where the first dimension represents the locations and the second dimension the timesteps
            Dim Data(nLoc - 1, nTim - 1) As Double
            For locIdx As Integer = 0 To nLoc - 1

                Dim locationData(nTim - 1) As Double
                For timIdx As Integer = 0 To nTim - 1
                    Data(locIdx, timIdx) = flatData(locIdx * nTim + timIdx)
                    locationData(timIdx) = flatData(locIdx * nTim + timIdx)
                Next

                ' Calculate statistics
                Dim maxValue As Double = locationData.Max()
                Dim minValue As Double = locationData.Min()
                Dim firstValue As Double = locationData.First()
                Dim lastValue As Double = locationData.Last()
                Dim meanValue As Double = locationData.Average()
                Dim medianValue As Double = CalculateMedian(locationData)

                ' Create a new clsSumaquaOutputLocationStatistics object
                Dim locationStats As New clsSumaquaOutputLocationStatistics(locIdx.ToString()) With {
                .Max = locationData.Max(),
                .Min = locationData.Min(),
                .First = locationData.First(),
                .Last = locationData.Last(),
                .Mean = locationData.Average(),
                .Median = CalculateMedian(locationData)
            }

                ' Add to OutputLocations dictionary
                OutputLocations.Add(locationStats.ID, locationStats)
            Next

            Return True
        Catch ex As Exception
            Console.WriteLine($"Error: {ex.Message}")
            Return False
        End Try
    End Function


    Private Function CalculateMedian(values() As Double) As Double
        Array.Sort(values)
        Dim count As Integer = values.Length
        If count Mod 2 = 0 Then
            ' Even number of elements, take the average of the two middle values
            Return (values(count \ 2 - 1) + values(count \ 2)) / 2.0
        Else
            ' Odd number of elements, take the middle value
            Return values(count \ 2)
        End If
    End Function


    'Friend Function Read(ByRef OutputLocations As Dictionary(Of String, clsSumaquaOutputLocationStatistics)) As Boolean
    '    Try
    '        Dim matFile As IMatFile
    '        Using fileStream As New FileStream(Path, FileMode.Open)
    '            Dim reader = New MatFileReader(fileStream)
    '            matFile = reader.Read()
    '        End Using

    '        ' Assuming the .mat file contains only one variable
    '        Dim variable = matFile.Variables.First()

    '        ' Retrieving number of timesteps and locations
    '        Dim nTim As Integer = variable.Value.Dimensions(0)
    '        Dim nLoc As Integer = variable.Value.Dimensions(1)




    '        '' Loop through all variables in the .mat file to retrieve their names
    '        'For Each matVariable In matFile.Variables
    '        '    Console.WriteLine($"Variable Name: {matVariable.Name}")
    '        '    Console.WriteLine($"Variable type: {matVariable.Value.GetType()}")

    '        '    Dim opaqueLinkType As Type = matVariable.Value.GetType()
    '        '    If opaqueLinkType.Name = "OpaqueLink" Then
    '        '        Console.WriteLine($"OpaqueLink Class Name: {opaqueLinkType.FullName}")

    '        '        ' Get all fields of the OpaqueLink class
    '        '        Dim fields = opaqueLinkType.GetFields(BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Instance)

    '        '        For Each field As FieldInfo In fields
    '        '            Console.WriteLine($"Field Name: {field.Name}")

    '        '            ' Try to get the value of the field
    '        '            Dim fieldValue = field.GetValue(matVariable.Value)
    '        '            If fieldValue IsNot Nothing Then
    '        '                Console.WriteLine($"Field Value: {fieldValue}")

    '        '                ' If the value is a string, treat it as a potential list of IDs
    '        '                If TypeOf fieldValue Is String Then
    '        '                    Dim valueString As String = CType(fieldValue, String)
    '        '                    Console.WriteLine($"OpaqueLink Value: {valueString}")

    '        '                    ' Check if it's a delimited list of IDs
    '        '                    Dim ids() As String = valueString.Split({","c, ";"c, Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

    '        '                    ' Print each ID to the console
    '        '                    For Each id As String In ids
    '        '                        Console.WriteLine($"Location ID: {id.Trim()}")
    '        '                    Next
    '        '                ElseIf TypeOf fieldValue Is Array Then
    '        '                    ' If it turns out to be an array instead of a simple string
    '        '                    For Each item In CType(fieldValue, Array)
    '        '                        Console.WriteLine($"OpaqueLink Array Item: {item}")
    '        '                    Next
    '        '                End If
    '        '            End If
    '        '        Next
    '        '    End If



    '        '    Select Case matVariable.Value.GetType().ToString()
    '        '        Case "MatFileHandler.MatNumericalArrayOf`1[System.Double]"
    '        '        Case "MatFileHandler.OpaqueLink"

    '        '        Case "time_array"

    '        '    End Select


    '        '' If the variable represents a data array, you can retrieve its details
    '        'If TypeOf matVariable.Value Is IArray Then
    '        '    Dim matArray = CType(matVariable.Value, MatArray)
    '        '    Console.WriteLine($"Dimensions: {String.Join(", ", matArray.Dimensions)}")
    '        '    Console.WriteLine($"Data Type: {matArray.Type}")
    '        'End If

    '        '' If the variable represents a structure, check if it has location names or parameter information
    '        'If TypeOf matVariable.Value Is MatStruct Then
    '        '    Dim matStruct = CType(matVariable.Value, MatStruct)

    '        '    ' Look for fields that might represent location names
    '        '    If matStruct.ContainsKey("locationNames") Then
    '        '        Dim locationNamesArray = matStruct("locationNames")
    '        '        If TypeOf locationNamesArray Is MatCellArray Then
    '        '            Dim cellArray = CType(locationNamesArray, MatCellArray)
    '        '            For i = 0 To cellArray.Elements.Count - 1
    '        '                Dim locationName = CType(cellArray.Elements(i), String)
    '        '                Console.WriteLine($"Location Name {i + 1}: {locationName}")
    '        '            Next
    '        '        End If
    '        '    End If

    '        '    ' Look for fields that might represent parameter names
    '        '    If matStruct.ContainsKey("parameterNames") Then
    '        '        Dim parameterNamesArray = matStruct("parameterNames")
    '        '        If TypeOf parameterNamesArray Is MatCellArray Then
    '        '            Dim cellArray = CType(parameterNamesArray, MatCellArray)
    '        '            For i = 0 To cellArray.Elements.Count - 1
    '        '                Dim parameterName = CType(cellArray.Elements(i), String)
    '        '                Console.WriteLine($"Parameter Name {i + 1}: {parameterName}")
    '        '            Next
    '        '        End If
    '        '    End If
    '        'End If
    '        Next

    '        Return True
    '    Catch ex As Exception
    '        Console.WriteLine($"An error occurred: {ex.Message}")
    '        Return False
    '    End Try
    'End Function




End Class
