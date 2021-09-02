Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsRRTopology

    Public Nodes As New Dictionary(Of String, clsRRNodeTPRecord)
    Public Links As New Dictionary(Of String, clsRRBrchTPRecord)
    Public Records As New Collection

    ' TODO: deze objecten worden niet gedisposed! In de destructor zetten?
    Friend nodtpwriter As StreamWriter
    Friend lnktpwriter As StreamWriter

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Public Function CountUnpavedNodes() As Integer
        Dim count As Integer = 0
        For Each Node As clsRRNodeTPRecord In Nodes.Values
            If Node.nt.ParentID = "3B_UNPAVED" Then
                count += 1
            End If
        Next
        Return count
    End Function

    Public Function CountPavedNodes() As Integer
        Dim count As Integer = 0
        For Each Node As clsRRNodeTPRecord In Nodes.Values
            If Node.nt.ParentID = "3B_PAVED" Then
                count += 1
            End If
        Next
        Return count
    End Function

    Public Function CountGreenhouseNodes() As Integer
        Dim count As Integer = 0
        For Each Node As clsRRNodeTPRecord In Nodes.Values
            If Node.nt.ParentID = "3B_GREENHOUSE" Then
                count += 1
            End If
        Next
        Return count
    End Function

    Friend Function GetAddNode(ID As String, x As Double, y As Double, nt As clsSobekNodeType, SetInUse As Boolean) As clsRRNodeTPRecord
        'get or create the new node.tp record
        Dim newUP As clsRRNodeTPRecord
        newUP = GetNode(ID.Trim.ToUpper)
        If newUP Is Nothing Then
            newUP = New clsRRNodeTPRecord(Me.Setup)
            newUP.ID = ID
            newUP.InUse = SetInUse
            newUP.X = x
            newUP.Y = y
            newUP.nt = nt
            Nodes.Add(ID.Trim.ToUpper, newUP)
        Else
            newUP.InUse = SetInUse
        End If
        Return newUP
    End Function

    Friend Function GetAddLink(ID As String, UpNodeID As String, DnNodeId As String, bt As clsSobekBranchType, SetInUse As Boolean) As clsRRBrchTPRecord
        'get or create the new links.tp record
        Dim newLink As clsRRBrchTPRecord
        newLink = GetLink(ID)
        If newLink Is Nothing Then
            newLink = New clsRRBrchTPRecord(Me.Setup, Me.SbkCase)
            newLink.ID = ID
            newLink.InUse = SetInUse
            newLink.bn = UpNodeID
            newLink.en = DnNodeId
            newLink.bt = bt
            Links.Add(newLink.ID.Trim.ToUpper, newLink)
        End If
        Return newLink
    End Function
    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each Node As clsRRNodeTPRecord In Nodes.Values
            Node.ID = Prefix & Node.ID
            Debug.Print(Node.ID)
        Next
        For Each Link As clsRRBrchTPRecord In Links.Values
            Link.ID = Prefix & Link.ID
            Link.bn = Prefix & Link.bn
            Link.en = Prefix & Link.en
            Debug.Print(Link.ID & " " & Link.bn & " " & Link.en)

        Next
    End Sub

    Friend Function getDownstreamNode(ByVal ID As String) As clsRRNodeTPRecord
        For Each myLink As clsRRBrchTPRecord In Links.Values
            If myLink.bt.ParentID = "3B_LINK" Then
                If myLink.bn.Trim.ToUpper = ID.Trim.ToUpper Then
                    Return GetNode(myLink.en)
                End If
            End If
        Next
        Return Nothing
    End Function

    Friend Function DisableAllNodes(Optional ByVal NodeID As String = "") As Boolean
        Try
            For Each myNode As clsRRNodeTPRecord In Nodes.Values
                If myNode.ID.Trim.ToUpper = NodeID.Trim.ToUpper OrElse NodeID = "" Then
                    myNode.InUse = False
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function DisableAllLinks(Optional ByVal NodeID As String = "") As Boolean
        Try
            For Each myLink As clsRRBrchTPRecord In Links.Values
                If myLink.bn.Trim.ToUpper = NodeID.Trim.ToUpper OrElse NodeID = "" Then
                    myLink.InUse = False
                ElseIf myLink.en.Trim.ToUpper = NodeID.Trim.ToUpper OrElse NodeID = "" Then
                    myLink.InUse = False
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function GetNode(ByVal ID As String) As clsRRNodeTPRecord
        If Nodes.ContainsKey(ID.Trim.ToUpper) Then
            Return Nodes.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Function GetLink(ByVal ID As String) As clsRRBrchTPRecord
        If Links.ContainsKey(ID.Trim.ToUpper) Then
            Return Links.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Friend Function getUpstreamNodes(ByVal ID As String) As Dictionary(Of String, String)
        'Author: Siebe Bosch
        'Date: 4-4-2013
        'Description: retrieves all upstream RR-node ID's for a given node
        Dim myDict As New Dictionary(Of String, String)
        Dim Done As Boolean = False
        Dim myLink As clsRRBrchTPRecord
        Dim myNode As clsRRNodeTPRecord
        Dim myID As String

        'first level, directly connected
        myNode = GetNode(ID)
        For Each myLink In Links.Values
            If myLink.en.Trim.ToUpper = myNode.ID.Trim.ToUpper Then
                myDict.Add(myLink.bn.Trim.ToUpper, myLink.bn)
            End If
        Next

        'now there might be second or higher order connected nodes
        While Not Done
            Done = True                                                   'initialize done as true. Falsify if another new upstream node is found
            For Each myID In myDict.Values
                myNode = GetNode(myID)
                For Each myLink In Links.Values
                    If myLink.en.Trim.ToUpper = myNode.ID.Trim.ToUpper Then
                        If Not myDict.ContainsKey(myLink.bn.Trim.ToUpper) Then
                            myDict.Add(myLink.bn.Trim.ToUpper, myLink.bn)
                            Done = False                                          'we just added a new node so we're not sure if we're done yet!
                        End If
                    End If
                Next
            Next
        End While

        Return myDict

    End Function



    Friend Function getDownstreamNodes(ByVal ID As String) As Dictionary(Of String, String)
        'Author: Siebe Bosch
        'Date: 22-5-2017
        'Description: retrieves all downstream RR-node ID's for a given node
        Dim myDict As New Dictionary(Of String, String)
        Dim tmpDict As New Dictionary(Of String, String)
        Dim Done As Boolean = False
        Dim myLink As clsRRBrchTPRecord
        Dim myNode As clsRRNodeTPRecord
        Dim myID As String

        'first level, directly connected
        myNode = GetNode(ID)
        For Each myLink In Links.Values
            If myLink.bn.Trim.ToUpper = myNode.ID.Trim.ToUpper Then
                myDict.Add(myLink.en.Trim.ToUpper, myLink.en)
            End If
        Next

        'now there might be second or higher order connected nodes
        While Not Done
            Done = True                                                       'initialize done as true. Falsify if another new upstream node is found
            For Each myID In myDict.Values
                tmpDict = New Dictionary(Of String, String)
                myNode = GetNode(myID)
                For Each myLink In Links.Values
                    If myLink.bn.Trim.ToUpper = myNode.ID.Trim.ToUpper Then
                        If Not tmpDict.ContainsKey(myLink.en.Trim.ToUpper) Then
                            tmpDict.Add(myLink.en.Trim.ToUpper, myLink.en)
                        End If
                    End If
                Next
            Next
            For Each item In tmpDict
                If Not myDict.ContainsKey(item.Value.Trim.ToUpper) Then
                    Done = False
                    myDict.Add(item.Value.Trim.ToUpper, item.Value)
                End If
            Next
        End While

        Return myDict

    End Function

    Public Function WriteNetwork(ByVal Append As Boolean, ExportDirTopo As String) As Boolean

        Try
            'if we're not going to append, we'll first need to write the headers
            If Not Append Then Call InitializeExport(ExportDirTopo)

            Using nodtpwriter As New StreamWriter(ExportDirTopo & "\3b_nod.tp", Append)
                Using lnktpwriter As New StreamWriter(ExportDirTopo & "\3b_link.tp", Append)

                    If Append = False Then
                        nodtpwriter.WriteLine("BBB2.2")
                        lnktpwriter.WriteLine("BBB2.2")
                    End If

                    'schrijf de AWZI's en hun boundary
                    For Each myWWTP As clsWWTP In Me.Setup.WWTPs.WWTPs.Values
                        If myWWTP.InUse Then
                            nodtpwriter.WriteLine("NODE id '" & myWWTP.ID & "' ri '-1' mt 1 '14' nt 56 ObID '3B_WWTP' px " & myWWTP.X & " py " & myWWTP.Y & " node")
                            If myWWTP.Boundary.nt = GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                                nodtpwriter.WriteLine("NODE id '" & myWWTP.Boundary.ID & "' ri '-1' mt 1 '34' nt 56 ObID 'SBK_SBK-3B-REACH' px " & myWWTP.Boundary.X & " py " & myWWTP.Boundary.Y & " node")
                            ElseIf myWWTP.Boundary.nt = GeneralFunctions.enmNodetype.NodeRRBoundary Then
                                nodtpwriter.WriteLine("NODE id '" & myWWTP.Boundary.ID & "' ri '-1' mt 1 '47' nt 56 ObID '3B_BOUNDARY' px " & myWWTP.Boundary.X & " py " & myWWTP.Boundary.Y & " node")
                            End If
                        End If
                        'If myWWTP.InUse Then nodtpwriter.WriteLine("NODE id 'bnd" & myWWTP.ID & "' ri '-1' mt 1 '6' nt 47 ObID '3B_BOUNDARY' px " & myWWTP.X & " py " & myWWTP.Y - 500 & " node")
                    Next

                    For Each node As clsRRNodeTPRecord In Nodes.Values
                        If node.InUse Then nodtpwriter.WriteLine("NODE id '" & node.ID & "' nm '" & node.nm & "' ri '-1' mt 1 '1' nt " & node.nt.SbkNum & " ObID '" & node.nt.ID & "' px " & node.X & " py " & node.Y & " node")
                    Next

                    For Each link As clsRRBrchTPRecord In Links.Values
                        If link.InUse Then lnktpwriter.WriteLine("BRCH id '" & link.ID & "' nm '" & link.nm & "' ri '-1' mt 1 '0' bt " & link.bt.SbkNum & " ObID '" & link.bt.ParentID & "' bn '" & link.bn & "' en '" & link.en & "' brch")
                    Next

                End Using
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteNetwork of class clsRRTopology")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    Friend Sub InitializeExport(ExportDirTopo As String)
        'drie uitvoerbestanden
        Using bbbWriter = New StreamWriter(ExportDirTopo & "\network.bbb", False)
            bbbWriter.WriteLine("BBB2.2")
            bbbWriter.WriteLine("*")
            bbbWriter.WriteLine("* Configuration file")
            bbbWriter.WriteLine("* Filename for Rainfall-Runoff:")
            bbbWriter.WriteLine("* 1. Node data")
            bbbWriter.WriteLine("* 2. Link data")
            bbbWriter.WriteLine("* 3. Lateral data")
            bbbWriter.WriteLine("'.\3b_nod.tp'")
            bbbWriter.WriteLine("'.\3b_link.tp'")
            'bbbWriter.WriteLine("'" & ExportDir & "\3brunoff.tp" & "'")
        End Using

        Using nodtpwriter = New StreamWriter(ExportDirTopo & "\3b_nod.tp", False)
            nodtpwriter.WriteLine("BBB2.2")
        End Using


        Using lnktpwriter = New StreamWriter(ExportDirTopo & "\3b_link.tp", False)
            lnktpwriter.WriteLine("BBB2.2")
        End Using

    End Sub


    Friend Function Import(ByVal path As String, ByRef myModel As ClsSobekCase) As Boolean
        Dim i As Integer
        Dim Datafile As New clsSobekDataFile(Me.Setup)

        Try
            'leest nodes in
            Records = Datafile.Read(path & "\3B_NOD.TP", "NODE")
            If Records.Count > 0 Then
                For i = 1 To Records.Count
                    Dim NODERecord As New clsRRNodeTPRecord(Me.Setup, Me.SbkCase)
                    NODERecord.Read(Records(i))
                    If Not Nodes.ContainsKey(NODERecord.ID.Trim.ToUpper) Then Nodes.Add(NODERecord.ID.Trim.ToUpper, NODERecord)
                Next i
            End If

            'leest links in
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\3B_LINK.TP", "BRCH")
            If Records.Count > 0 Then
                For i = 1 To Records.Count
                    Dim BRCHRecord As New clsRRBrchTPRecord(Me.Setup, Me.SbkCase)
                    BRCHRecord.Read(Setup, Records(i))
                    If Not Links.ContainsKey(BRCHRecord.ID.Trim.ToUpper) Then Links.Add(BRCHRecord.ID.Trim.ToUpper, BRCHRecord)
                Next i
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error importing RR topology.")
            Return False
        End Try


    End Function
End Class
