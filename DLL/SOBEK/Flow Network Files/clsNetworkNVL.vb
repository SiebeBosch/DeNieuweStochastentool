Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports STOCHLIB.General

'contains a binary reader for the network.nvl files in SOBEK. Reverse engineered by Siebe Bosch, august 14, 2016
'network.nvl is a binary file that is built-up along the following rules:

'header in positions 0 through 5
'nreaches as int32 in positions 6 through 9
'max number of vector points in one single reach in positions 10 through 13
'then number of vector points for the first reach as int32 in positions 14 through 17
'if more than one reach: number of vector points in positions 18 through 21
'etc. etc

'then the bounding box for all reaches. in case of just one reach:
'xll as double in 18 through 25 
'yll as double in 26 through 33
'xur as double in 34 through 41
'yur as double in 42 through 49

'then the xy-coordinates for each vector point of the every reach, including start and endpoint
'in case of just one reach:
'x as double in 50 throug 57
'y as double in 58 through 55
'etc. etc.

Public Class clsNetworkNVL

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase
    Private binaryReader As BinaryReader
    Private fileStream As FileStream

    Private nReaches As Long    'number of reaches in the schematization
    Private maxvp As Long       'the largest number of vectorpoints in one single reach
    Private Records As New List(Of clsNetworkNVLRecord)

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Setup = mySetup
        SbkCase = myCase
    End Sub

    Public Sub ReadAll()


        Dim i As Long, j As Long, myRecord As clsNetworkNVLRecord
        Dim xy As clsXY

        Open()

        'read the number of reaches and the max number of vector points on one single reach
        binaryReader.BaseStream.Position = 6
        nReaches = binaryReader.ReadInt32
        maxvp = binaryReader.ReadInt32

        'read the number of vector points on every reach
        For i = 0 To nReaches - 1
            myRecord = New clsNetworkNVLRecord
            myRecord.nVectorPoints = binaryReader.ReadInt32
            Records.Add(myRecord)
        Next

        'read the bounding box for every reach
        For i = 0 To nReaches - 1
            myRecord = Records(i)

            'read the bounding box for the current reach
            myRecord.bbox.XLL = binaryReader.ReadDouble
            myRecord.bbox.YLL = binaryReader.ReadDouble
            myRecord.bbox.XUR = binaryReader.ReadDouble
            myRecord.bbox.YUR = binaryReader.ReadDouble
        Next

        'read the vector points for each reach
        For i = 0 To nReaches - 1
            myRecord = Records(i)
            For j = 0 To myRecord.nVectorPoints - 1
                xy = New clsXY
                xy.X = binaryReader.ReadDouble
                xy.Y = binaryReader.ReadDouble
                myRecord.cp.Add(xy)
            Next
        Next
        Close()

    End Sub


    Public Sub Open()
        Close()
        fileStream = File.OpenRead(SbkCase.CaseDir & "\network.nvl")
        binaryReader = New BinaryReader(fileStream)
    End Sub

    Public Sub Close()
        If fileStream IsNot Nothing Then
            fileStream.Close()
            binaryReader.Close()
            fileStream = Nothing
        End If
    End Sub

End Class
