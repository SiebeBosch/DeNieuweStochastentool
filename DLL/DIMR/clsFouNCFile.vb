Option Explicit On
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General
Imports DocumentFormat.OpenXml.InkML
Imports System.IO


Public Class clsFouNCFile
    Private Setup As clsSetup
    Friend Path As String


    Dim Variables As sds.ReadOnlyVariableCollection

    Dim Mesh2d_fourier001_maxID As Integer = -1
    Dim Mesh1d_fourier001_maxID As Integer = -1
    Dim Mesh2d_fourier002_maxID As Integer = -1
    Dim Mesh1d_fourier002_maxID As Integer = -1
    Dim Mesh2d_face_xID As Integer = -1
    Dim Mesh2d_face_yID As Integer = -1
    Dim Mesh1d_node_xID As Integer = -1
    Dim Mesh1d_node_yID As Integer = -1
    Dim Mesh1d_node_idID As Integer = -1
    Dim Mesh1d_node_long_nameID As Integer = -1

    'define all variables for the data
    Dim Mesh2d_fourier001_max As Double()       'size: number of faces
    Dim Mesh1d_fourier001_max As Double()       'size: number of nodes
    Dim Mesh2d_fourier002_max As Double()       'size: number of faces
    Dim Mesh1d_fourier002_max As Double()       'size: number of nodes
    Friend Mesh2d_face_x As Double()       'size: number of faces
    Friend Mesh2d_face_y As Double()       'size: number of faces
    Dim Mesh1d_node_x As Double()       'size: number of nodes
    Dim Mesh1d_node_y As Double()       'size: number of nodes

    Dim Mesh1d_node_id As Byte(,)      'since this parameter is 2D (40 bytes per id) we also create an array with all ids
    Dim Mesh1d_node_ids As String()

    Dim Mesh1d_node_long_name As Byte(,)
    Dim Mesh1d_node_long_names As String()


    Public Sub New(myPath As String, ByRef mySetup As clsSetup)
        Path = myPath
        Setup = mySetup
    End Sub

    Public Function get2DMaximumWaterLevels() As Double()
        'returns the maximum water level per 2D cell
        Return Mesh2d_fourier002_max
    End Function

    Public Function Read() As Boolean
        Try
            'note: which variable is stored wehre in the fourier file is specified in the fm-folder in the .fou file
            'e.g.:
            '*var tsrts   sstop   numcyc  knfac   v0plu   layno   elp    
            ' wl  21600 - 1      0       1.0     0.0             min    
            ' wl  21600 - 1      0       1.0     0.0             max    
            ' uc  21600 - 1      0       1.0     0.0     1.0     max    
            ' in this example it's the second layer (mesh2d_fourier002_max) that contains the maximum waterlevels
            ' to do: implement a functionality to read this file and select the correct variable from our Fou.nc file.


            Dim dataset = sds.DataSet.Open(Path & "?openMode=readOnly")
            Dim myDataset As sds.DataSet() = dataset.GetLinkedDataSets
            Dim myDimensions As sds.ReadOnlyDimensionList = dataset.Dimensions
            Variables = dataset.Variables


            'set the names for all variables
            For i = 0 To dataset.Variables.Count - 1
                Debug.Print(dataset.Variables.Item(i).Name)
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier001_max" Then Mesh2d_fourier001_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_fourier001_max" Then Mesh1d_fourier001_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier002_max" Then Mesh2d_fourier002_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_fourier002_max" Then Mesh1d_fourier002_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_x" Then Mesh2d_face_xID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_y" Then Mesh2d_face_yID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_x" Then Mesh1d_node_xID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_y" Then Mesh1d_node_yID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_id" Then Mesh1d_node_idID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_long_name" Then Mesh1d_node_long_nameID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier001_max" Then Mesh2d_fourier001_maxID = dataset.Variables.Item(i).ID
            Next

            'and read the fourier file's content!
            If Mesh2d_fourier001_maxID >= 0 Then Mesh2d_fourier001_max = dataset.GetData(Of Double())(Mesh2d_fourier001_maxID)
            If Mesh1d_fourier001_maxID >= 0 Then Mesh1d_fourier001_max = dataset.GetData(Of Double())(Mesh1d_fourier001_maxID)
            If Mesh2d_fourier002_maxID >= 0 Then Mesh2d_fourier002_max = dataset.GetData(Of Double())(Mesh2d_fourier002_maxID)
            If Mesh1d_fourier002_maxID >= 0 Then Mesh1d_fourier002_max = dataset.GetData(Of Double())(Mesh1d_fourier002_maxID)
            If Mesh2d_face_xID >= 0 Then Mesh2d_face_x = dataset.GetData(Of Double())(Mesh2d_face_xID)
            If Mesh2d_face_yID >= 0 Then Mesh2d_face_y = dataset.GetData(Of Double())(Mesh2d_face_yID)
            If Mesh1d_node_xID >= 0 Then Mesh1d_node_x = dataset.GetData(Of Double())(Mesh1d_node_xID)
            If Mesh1d_node_yID >= 0 Then Mesh1d_node_y = dataset.GetData(Of Double())(Mesh1d_node_yID)
            If Mesh1d_node_idID >= 0 Then Mesh1d_node_id = dataset.GetData(Of Byte(,))(Mesh1d_node_idID)
            If Mesh1d_node_long_nameID >= 0 Then Mesh1d_node_long_name = dataset.GetData(Of Byte(,))(Mesh1d_node_long_nameID)

            'de id's zijn samengesteld uit een array van bytes
            Dim IDArray As Byte()
            ReDim Mesh1d_node_ids(UBound(Mesh1d_node_id, 1))
            For i = 0 To UBound(Mesh1d_node_id, 1)
                IDArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(Mesh1d_node_id, i)
                Mesh1d_node_ids(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)
            Next

            'idem voor de long_names
            Dim LongNameArray As Byte()
            ReDim Mesh1d_node_long_names(UBound(Mesh1d_node_long_name, 1))
            For i = 0 To UBound(Mesh1d_node_long_name, 1)
                LongNameArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(Mesh1d_node_long_name, i)
                Mesh1d_node_long_names(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(LongNameArray, True)
            Next

            Return True
        Catch ex As Exception
            Return True
        End Try
    End Function

    'Public Function FloodStatisticsToGeoJSON(ResultsPath As String, MaxDepth As Boolean, MaxVelocity As Boolean, TInund As Boolean, TMax As Boolean, T20cm As Boolean, T50cm As Boolean) As Boolean

    '    Try
    '        Dim Name As String = "DHydro"

    '        'now we write the results to a plain JSON file
    '        Dim jsonstr As String
    '        Dim idx As Integer = -1

    '        Using meshWriter As New StreamWriter(ResultsPath)
    '            meshWriter.WriteLine("{")
    '            meshWriter.WriteLine("""type"": ""FeatureCollection"",")
    '            meshWriter.WriteLine("""name"": """ & Name & """,")
    '            meshWriter.WriteLine("""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::" & "28992" & """} },")
    '            meshWriter.WriteLine("""features"": [")

    '            'walk through all cells and write them as a feature
    '            Dim nFaceNodes As Integer = UBound(FaceNodes, 1) + 1
    '            For i = 0 To nFaceNodes - 1

    '                'only if this cell contains depths>0 it will be written
    '                Dim maxDepthClass As Integer = 1 'first increment class will be skipped since it represents dry cells
    '                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, nFaceNodes)

    '                'check if we have an elevated waterdepth in this cell
    '                For k = 0 To TimeStamps.Count - 1
    '                    If waterDepths(k, i) > 1 Then
    '                        maxDepthClass = waterDepths(k, i)
    '                        Exit For
    '                    End If
    '                Next

    '                'again: skip the lowest class since it represents totally dry cells
    '                If maxDepthClass > 1 Then

    '                    'only if this cell contains depths>0 it will be written
    '                    Dim maxClass As Integer = 1 'first increment class will be skipped since it represents dry cells
    '                    For k = 0 To TimeStamps.Count - 1
    '                        If waterDepths(k, i) > 1 Then
    '                            maxClass = waterDepths(k, i)
    '                            Exit For
    '                        End If
    '                    Next

    '                    'again: skip the lowest class since it represents dry cells
    '                    If maxClass > 1 Then

    '                        idx += 1

    '                        If idx > 0 Then
    '                            'this is not the first line, so close the previous record with a comma and a CRLF
    '                            jsonstr = "," & vbCrLf
    '                        Else
    '                            jsonstr = ""
    '                        End If

    '                        'we have found a cell that contains water depths > 0. Write it to our JSON!
    '                        jsonstr &= "{ ""type"": ""Feature"", ""geometry"": { ""type"": ""MultiPolygon"", ""coordinates"": [[["

    '                        'write our face's geometry. Start with the first coordinate
    '                        'Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, 0) - 1), FaceNodesY(FaceNodes(i, 0) - 1), lat, lon)
    '                        jsonstr &= "[" & FaceNodesX(FaceNodes(i, 0) - 1) & "," & FaceNodesY(FaceNodes(i, 0) - 1) & "]"

    '                        'now write the remaining coordinates, if in use
    '                        For j = 1 To UBound(FaceNodes, 2)
    '                            'unused facenodes are indicted by -999 (e.g. triangular faces where ubound(facenodes,2) = 3)
    '                            'NOTE: the index numbers inside FaceNodes are 1-based!
    '                            If FaceNodes(i, j) > -999 Then
    '                                'Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, j) - 1), FaceNodesY(FaceNodes(i, j) - 1), lat, lon)
    '                                jsonstr &= ", [" & FaceNodesX(FaceNodes(i, j) - 1) & "," & FaceNodesY(FaceNodes(i, j) - 1) & "]"
    '                            End If
    '                        Next
    '                        jsonstr &= ", [" & FaceNodesX(FaceNodes(i, 0) - 1) & "," & FaceNodesY(FaceNodes(i, 0) - 1) & "]"
    '                        jsonstr &= "]]]}, ""properties"": { ""i"": " & idx

    '                        'for analysis in GIS we cannot write the results in arrays inside the geoJSON
    '                        Dim TsInundH As Integer = -1
    '                        Dim TsInundM As Integer = -1
    '                        Dim Ts20cm As Integer = -1
    '                        Dim Ts50cm As Integer = -1
    '                        Dim TsMax As Integer = -1
    '                        Dim MaxDepthVal As Double = 0
    '                        Dim MaxVelVal As Double = 0

    '                        For j = 1 To TimeStamps.Count - 1

    '                            If DepthClassValues(waterDepths(j, i) - 1) > MaxDepthVal Then
    '                                MaxDepthVal = DepthClassValues(waterDepths(j, i) - 1)
    '                                TsMax = j
    '                            End If
    '                            If VelocityClassValues(velocities(j, i) - 1) > MaxVelVal Then
    '                                MaxVelVal = VelocityClassValues(velocities(j, i) - 1)
    '                            End If

    '                            'as soon as our depth thresholds are exceeded, write the timestep
    '                            If DepthClassValues(waterDepths(j, i) - 1) > 0 AndAlso TsInundH = -1 Then
    '                                TsInundH = getHoursFromTimestepIdx(j)
    '                                TsInundM = getMinutesFromTimestepIdx(j)
    '                            End If
    '                            If DepthClassValues(waterDepths(j, i) - 1) >= 0.2 AndAlso Ts20cm = -1 Then
    '                                Ts20cm = getHoursFromTimestepIdx(j)
    '                            End If
    '                            If DepthClassValues(waterDepths(j, i) - 1) >= 0.5 AndAlso Ts50cm = -1 Then
    '                                Ts50cm = getHoursFromTimestepIdx(j)
    '                            End If
    '                        Next

    '                        jsonstr &= ", """ & GeneralFunctions.enmFloodFieldName.T_FLOOD_H.ToString & """: " & TsInundH & ", """ & GeneralFunctions.enmFloodFieldName.T_FLOOD_M.ToString & """: " & TsInundM & ", """ & GeneralFunctions.enmFloodFieldName.T_20CM_H.ToString & """: " & Ts20cm & ", """ & GeneralFunctions.enmFloodFieldName.T_50CM_H.ToString & """: " & Ts50cm & ", """ & GeneralFunctions.enmFloodFieldName.MAXD_CM.ToString & """: " & Convert.ToInt32(MaxDepthVal * 100) & ", """ & GeneralFunctions.enmFloodFieldName.MAXD_M.ToString & """: " & TsMax & ", """ & GeneralFunctions.enmFloodFieldName.MAXV_CMPS.ToString & """:" & Convert.ToInt16(MaxVelVal * 100) & "}}"

    '                        meshWriter.Write(jsonstr)

    '                    End If

    '                End If




    '            Next

    '            'write the last line ending
    '            meshWriter.Write(vbCrLf)

    '            Me.Setup.GeneralFunctions.UpdateProgressBar("Export to JSON complete.", 0, 10, True)

    '            meshWriter.WriteLine("]")
    '            meshWriter.WriteLine("}")
    '        End Using

    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function StatisticsToGeoJSON of class clsClassMapFile: " & ex.Message)
    '        Return False
    '    End Try

    'End Function


End Class
