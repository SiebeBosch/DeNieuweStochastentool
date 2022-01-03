Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
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
            xml.Load(DIMR.ProjectDir & "\" & FileName)

            For Each node In xml.ChildNodes
                If node.Name = "dimrConfig" Then
                    For Each cNode As XmlNode In node.ChildNodes
                        If cNode.Name = "component" Then
                            If cNode.Attributes.ItemOf("name") IsNot Nothing Then
                                Dim myName As String = cNode.Attributes.GetNamedItem("name").InnerText
                                Select Case myName
                                    Case Is = "Rainfall Runoff"
                                        RR.InUse = True
                                        For Each dNode As XmlNode In cNode.ChildNodes
                                            If dNode.Name = "library" Then RR.SetLibrary(dNode.InnerText)
                                            If dNode.Name = "workingDir" Then RR.SetSubDir(dNode.InnerText)
                                            If dNode.Name = "inputFile" Then
                                                RR.SetInputFile(dNode.InnerText)
                                                RR.Read()
                                            End If
                                        Next
                                    Case Is = "Real-Time Control"
                                        RTC.InUse = True
                                        For Each dNode As XmlNode In cNode.ChildNodes
                                            If dNode.Name = "library" Then RTC.SetLibrary(dNode.InnerText)
                                            If dNode.Name = "workingDir" Then RTC.SetSubDir(dNode.InnerText)
                                            If dNode.Name = "inputFile" Then
                                                RTC.SetInputFile(dNode.InnerText)
                                            End If
                                        Next
                                    Case Is = "Flow1D"
                                        Flow1D.InUse = True
                                        For Each dNode As XmlNode In cNode.ChildNodes
                                            If dNode.Name = "library" Then Flow1D.SetLibrary(dNode.InnerText)
                                            If dNode.Name = "workingDir" Then Flow1D.SetSubDir(dNode.InnerText)
                                            If dNode.Name = "inputFile" Then
                                                Flow1D.SetInputFile(dNode.InnerText)
                                            End If
                                        Next
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
