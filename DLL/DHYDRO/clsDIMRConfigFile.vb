Option Explicit On
Imports DocumentFormat.OpenXml.Wordprocessing
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Imports System.Runtime.InteropServices.ComTypes
Imports System.Xml

Public Class clsDIMRConfigFile
    Private Setup As clsSetup
    Private DIMR As clsDIMR

    Dim FileName As String = "DIMR_Config.xml"

    Public RR As clsDIMRCONFIGFRRComponent
    Public Flow1D As clsDIMRCONFIGFlow1DComponent
    Public RTC As clsDIMRCONFIGRTCComponent

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDIMR As clsDIMR)
        Setup = mySetup
        DIMR = myDIMR
        RR = New clsDIMRCONFIGFRRComponent(Me.Setup, Me.DIMR)
        RTC = New clsDIMRCONFIGRTCComponent(Me.Setup, Me.DIMR)
        Flow1D = New clsDIMRCONFIGFlow1DComponent(Me.Setup, Me.DIMR)
    End Sub




    Public Function Read() As Boolean
        Try
            Dim xml As New XmlDocument
            Dim node As XmlNode

            Dim library As String = ""
            Dim workingDir As String = ""
            Dim inputFile As String = ""

            xml.Load(DIMR.ProjectDir & "\" & FileName)

            For Each node In xml.ChildNodes
                If node.Name = "dimrConfig" Then
                    For Each cNode As XmlNode In node.ChildNodes
                        If cNode.Name = "component" Then
                            If cNode.Attributes.ItemOf("name") IsNot Nothing Then
                                Dim myName As String = cNode.Attributes.GetNamedItem("name").InnerText
                                For Each dNode As XmlNode In cNode.ChildNodes
                                    If dNode.Name = "library" Then library = dNode.InnerText
                                    If dNode.Name = "workingDir" Then workingDir = dNode.InnerText
                                    If dNode.Name = "inputFile" Then inputfile = dNode.InnerText
                                Next

                                Select Case library.Trim.ToLower
                                    Case Is = "dflowfm"
                                        DIMR.FlowFM = New clsFlowFMComponent(Me.Setup, DIMR)
                                        Flow1D.InUse = True
                                        Flow1D.SetLibrary(library)
                                        Flow1D.SetSubDir(workingDir)
                                        Flow1D.SetInputFile(inputFile)
                                    Case Is = "rr_dll"
                                        DIMR.RR = New clsRRComponent(Me.Setup, DIMR)
                                        RR.InUse = True
                                        RR.SetLibrary(library)
                                        RR.SetSubDir(workingDir)
                                        RR.SetInputFile(inputFile)
                                        RR.Read()
                                    Case Is = "fbctools_bmi"
                                        DIMR.RTC = New clsRTCComponent(Me.Setup, DIMR)
                                        RTC.InUse = True
                                        RTC.SetLibrary(library)
                                        RTC.SetSubDir(workingDir)
                                        RTC.SetInputFile(inputFile)
                                End Select
                            End If
                        End If
                    Next
                End If
            Next

            ''now that we have loaded the xml we only have to search for the existence of our modules
            'node = xml.DocumentElement.SelectSingleNode("/dimrConfig/control")


            'XmlNode node = doc.DocumentElement.SelectSingleNode("/book/title");


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading DIMRConfig file: " + ex.Message)
            Return False
        End Try
    End Function

End Class
