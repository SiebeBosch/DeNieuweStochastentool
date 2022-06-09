Imports STOCHLIB.General
Imports System.IO
Public Class clsMDUFile
    Inherits clsIniFile

    Private DIMR As ClsDIMR
    Private DIMRConfigComponent As clsDIMRConfigComponent

    'Friend NetFile As String   'specifies the name of the geometry file (.nc)
    'Friend StructureFile As String 'specifies the name of the structures file (usually structures.ini)

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDIMR As ClsDIMR, ByRef myConfigComponent As clsDIMRConfigComponent)
        MyBase.New(mySetup)
        DIMR = myDIMR
        DIMRConfigComponent = myConfigComponent
    End Sub

    Public Function GetSimulationPeriod(ByRef ReferenceDate As DateTime, ByRef StartDate As DateTime, ByRef EndDate As DateTime) As Boolean
        Try
            'the start and enddates of our FlowFM simulation are stored in the .mdu file under [time]
            Dim RefDate As String = getAttributeValue("[time]", "RefDate")
            Dim TStart As String = getAttributeValue("[time]", "TStart")
            Dim TStop As String = getAttributeValue("[time]", "TStop")

            ReferenceDate = New DateTime(Strings.Left(RefDate, 4), Strings.Mid(RefDate, 5, 2), Strings.Right(RefDate, 2))
            StartDate = ReferenceDate.AddSeconds(Convert.ToInt32(TStart))
            EndDate = ReferenceDate.AddSeconds(Convert.ToInt32(TStop))

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetSimulationPeriod of class clsMDUFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function getPath() As String
        Return DIMRConfigComponent.GetFullDir & "\" & DIMRConfigComponent.GetInputFile
    End Function

    Public Function GetFouConfigFileName() As String
        Return getAttributeValue("[Output]", "FouFile", 0, "Maxima.fou")
    End Function

    Public Function GetOutputSubDir() As String
        Return getAttributeValue("[Output]", "outputdir", 0, "output")
    End Function

    Public Function GetNetFileName() As String
        Return getAttributeValue("[geometry]", "NetFile", 0, GetFileNameBase() & "_net.nc")
    End Function

    Public Function getNetFileFullPath() As String
        Return DIMRConfigComponent.GetFullDir & "\" & GetNetFileName()
    End Function

    Public Function GetHisFileName() As String
        Return getAttributeValue("[output]", "HisFile", 0, GetFileNameBase() & "_his.nc")
    End Function

    Public Function getHisFileFullPath() As String
        Return DIMRConfigComponent.GetFullDir & "\" & GetHisFileName()
    End Function

    Public Function GetClassMapFileName() As String
        Return getAttributeValue("[output]", "ClassMapFile", 0, GetFileNameBase() & "_clm.nc")
    End Function

    Public Function getClassMapFileFullPath() As String
        Return GetOutputFullDir() & "\" & GetClassMapFileName()
    End Function

    Public Function GetFouFileName() As String
        Return getAttributeValue("[output]", "FouFile", 0, GetFileNameBase() & "_fou.nc")
    End Function

    Public Function GetFouFileFullPath() As String
        Return GetOutputFullDir() & "\" & GetFouFileName()
    End Function

    Public Function GetMapFileName() As String
        Return getAttributeValue("[output]", "MapFile", 0, GetFileNameBase() & "_map.nc")
    End Function

    Public Function getMapFileFullPath() As String
        Return GetOutputFullDir() & "\" & GetMapFileName()
    End Function
    Public Function getStructuresFileName() As String
        Return getAttributeValue("[geometry]", "StructureFile", 0, "structures.ini")
    End Function
    Public Function getStructuresFileFullPath() As String
        Return DIMRConfigComponent.GetFullDir & "\" & getStructuresFileName()
    End Function

    Public Function getFilename() As String
        Return Me.Setup.GeneralFunctions.GetFileNameFromPath(DIMRConfigComponent.GetInputFile)
    End Function


    Public Function GetFileNameBase() As String
        Dim Filename As String = getFilename()
        Dim FileNameBase As String = Me.Setup.GeneralFunctions.getBaseFromFilename(Filename)
        Return FileNameBase
    End Function

    'Public Function Read() As Boolean
    '    Try
    '        Dim myLine As String
    '        Using mduReader As New StreamReader(getPath)
    '            While Not mduReader.EndOfStream
    '                myLine = mduReader.ReadLine.Trim.ToLower
    '                If Left(myLine.Trim.ToLower, 7) = "netfile" Then
    '                    NetFile = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
    '                ElseIf Left(myLine.Trim.ToLower, 7) = "obsfile" Then
    '                    ObsFile = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
    '                    If ObsFile = "" Then ObsFile = "Observationpoints.ini"
    '                ElseIf Left(myLine.Trim.ToLower, 9) = "outputdir" Then
    '                    OutputDir = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
    '                    If OutputDir = "" Then OutputDir = "output"
    '                ElseIf Left(myLine.Trim.ToLower, 7) = "hisfile" Then
    '                    HisFile = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
    '                    If HisFile = "" Then HisFile = GetFileNameBase() & "_nc.nc"       'assume the classmapfile has the same name as the MDU itself
    '                ElseIf Left(myLine.Trim.ToLower, 12) = "classmapfile" Then
    '                    ClmFile = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
    '                    If ClmFile = "" Then ClmFile = GetFileNameBase() & "_clm.nc"       'assume the classmapfile has the same name as the MDU itself
    '                ElseIf Left(myLine.Trim.ToLower, 13) = "structurefile" Then
    '                    StructureFile = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
    '                    If StructureFile = "" Then StructureFile = "structures.ini"       'assume the classmapfile has the same name as the MDU itself
    '                End If
    '            End While
    '        End Using
    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error reading MDU file: " & ex.Message)
    '        Return False
    '    End Try

    'End Function

    Public Function GetOutputFullDir() As String
        Return DIMRConfigComponent.GetFullDir & "\" & GetOutputSubDir()
    End Function





End Class
