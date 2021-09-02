Option Explicit On

''' <summary>
''' Geen constructor nodig
''' </summary>
''' <remarks></remarks>
Public Class clsSettings
    'TODO: Converteer naar struct
    Public CFSettings As New clsCFSettings
    Public RRSettings As New clsRRSettings
    Public GISSettings As New clsGISSettings
    Public ClimatologicalSettings As New clsClimatologicalSettings

    Public RootDir As String            'the root directory for the current application (usually where the XML configuration file and/or database are located
    Public ExportDirRoot As String
    Public ExportDirSOBEK As String
    Public ExportDirDHydro As String

    Public ExportDirSobekTopo As String
    Public ExportDirSobekFlow As String
    Public ExportDirSobekRR As String

    Public ExportDirDHydroRR As String
    Public ExportDirHydroFlow As String

    Public ExportDirGIS As String
    Public ExportDirLogs As String

    Public Sub SetRootDir(ByVal myPath As String)
        rootdir = myPath
    End Sub

    Public Sub SetExportDirs(ByVal myPath As String, ByVal MakeSobekSubDirs As Boolean, ByVal MakeDHydroSubDirs As Boolean, ByVal ClearAllSubdirs As Boolean, ByVal MakeTemporaryGisDir As Boolean, ByVal ClearRootDir As Boolean)
        'this sub sets all export directories for Topology, Flow Data and RR Data
        'v1.890: made a split between clearing the root dir and clearing all subdirs
        'by default from now on the root dir will NOT be cleared
        Dim Files As String(), myFile As String

        ExportDirRoot = myPath

        If Not System.IO.Directory.Exists(ExportDirRoot) Then System.IO.Directory.CreateDirectory(ExportDirRoot)
        If ClearRootDir Then
            Files = System.IO.Directory.GetFiles(ExportDirRoot)
            For Each myFile In Files
                System.IO.File.Delete(myFile)
            Next
        End If



        If MakeTemporaryGisDir Then
            ExportDirGIS = myPath & "\GIS"
            If Not System.IO.Directory.Exists(ExportDirGIS) Then System.IO.Directory.CreateDirectory(ExportDirGIS)
        End If

        If MakeDHydroSubDirs Then
            ExportDirDHydro = myPath & "\D-Hydro"
            If Not System.IO.Directory.Exists(ExportDirDHydro) Then System.IO.Directory.CreateDirectory(ExportDirDHydro)
            ExportDirDHydroRR = ExportDirDHydro & "\RR"
            If Not System.IO.Directory.Exists(ExportDirDHydroRR) Then System.IO.Directory.CreateDirectory(ExportDirDHydroRR)
            ExportDirHydroFlow = ExportDirDHydro & "\DFLOWFM"
            If Not System.IO.Directory.Exists(ExportDirHydroFlow) Then System.IO.Directory.CreateDirectory(ExportDirHydroFlow)
        End If

        If MakeSobekSubDirs Then
            ExportDirSOBEK = myPath & "\SOBEK"
            If Not System.IO.Directory.Exists(ExportDirSOBEK) Then System.IO.Directory.CreateDirectory(ExportDirSOBEK)

            ExportDirSobekTopo = ExportDirSOBEK & "\Topo"
            ExportDirSobekFlow = ExportDirSOBEK & "\FlowData"
            ExportDirSobekRR = ExportDirSOBEK & "\RRData"
            ExportDirLogs = ExportDirSOBEK & "\Logs"
            If Not System.IO.Directory.Exists(ExportDirSobekTopo) Then System.IO.Directory.CreateDirectory(ExportDirSobekTopo)
            If Not System.IO.Directory.Exists(ExportDirSobekFlow) Then System.IO.Directory.CreateDirectory(ExportDirSobekFlow)
            If Not System.IO.Directory.Exists(ExportDirSobekRR) Then System.IO.Directory.CreateDirectory(ExportDirSobekRR)
            If Not System.IO.Directory.Exists(ExportDirLogs) Then System.IO.Directory.CreateDirectory(ExportDirLogs)

            If ClearAllSubdirs Then
                Files = System.IO.Directory.GetFiles(ExportDirSobekTopo)
                For Each myFile In Files
                    System.IO.File.Delete(myFile)
                Next
                Files = System.IO.Directory.GetFiles(ExportDirSobekFlow)
                For Each myFile In Files
                    System.IO.File.Delete(myFile)
                Next
                Files = System.IO.Directory.GetFiles(ExportDirSobekRR)
                For Each myFile In Files
                    System.IO.File.Delete(myFile)
                Next
                Files = System.IO.Directory.GetFiles(ExportDirDHydroRR)
                For Each myFile In Files
                    System.IO.File.Delete(myFile)
                Next
            End If
        End If

    End Sub


End Class
