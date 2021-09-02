Imports STOCHLIB.General
Imports System.IO


Public Class clsNetworkNTW
    'this class contains the entire content of a network.ntw file:
    'a text file representing the entire topology of a SOBEK 2 network
    'written by Siebe Bosch on 14 august 2016

    'network.ntw file format:
    'header: "NTW6.6","C:\SOBEK213\test.lit\CMTWORK\ntrpluv.ini","SOBEK-LITE, edit network"
    'reach specification
    'reach name for beginnode to endnode
    '"reachname","",reachID,reachType,"ReachDescription","",0,0,0,0,reachLength,0,0,0,"bnId","","",0 or 1,bnNodeTypeNum,"bnNodeTypeDescription","",x,y,0,0,"SYS_DEFAULT",0,"eind","","",0 or 1,enNodeTypeNum,"enNodeTypeDescription","",x,y,0,reachlength,"SYS_DEFAULT",0
    'end of reach specifications:
    '"*"
    'empty line
    '[Reach description]
    'empty line
    'nReaches
    '"ReachNum","","bnID","enID",0,nVectorPoints,xllcorner,yllcorner,xurcorner,yurcorner,reachlength,0,1000,-1
    'empty line
    '[Model connection node]
    '"1.00"
    'nNodeTypes,nNodeTypesUsed
    'bnTypeNumber,"bnTypeDescription","",1,"SOBEK","4"
    'enTypeNumber,"enTypeDescription","",3,"SOBEK","3","SOBEK","4","SOBEK","31"
    'empty line
    '[Model connection branch]
    '"1.00"
    'nReachTypes,nReachTypesUsed
    'reachTypeNum,"reachtypeDescription","",2,"SOBEK","0","SOBEK","31"
    'empty line
    '[Nodes with calculationpoint]
    '"1.00"
    '0
    'empty line
    '[Reach options]
    '"1.00"
    '0
    '[NTW properties]
    '"1.00"
    '3
    'v1=4
    'v2=1
    'v3-5
    'empty line

    Public Header As String ' = """NTW6.6""" &"," & """c:\SOBEK214\WSHD.lit\CMTWORK\ntrpluv.ini""" &"," & """SOBEK-LITE, edit network" & """

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Public NodeTypes As Dictionary(Of Integer, clsNetworkNTWNodeType)           'a list of all node types with their numbers
    Public ReachTypes As Dictionary(Of Integer, clsNetworkNTWReachType)         'a list of all reach types with their numbers

    'nodes and reach segments
    Public NetworkNodes As Dictionary(Of String, clsNetworkNTWNode)
    Public ReachSegments As Dictionary(Of String, clsNetworkNTWReachSegment)
    Public NetworkGrids As Dictionary(Of String, clsNetworkNTWASCIIGrid)

    Public ReachRecords As New Dictionary(Of String, String)
    Public ReachDescriptionRecords As New Dictionary(Of String, String)

    Public ModelConnectionNodes As New Dictionary(Of String, String)
    Public ModelConnectionBranches As New Dictionary(Of String, String)

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Setup = mySetup
        SbkCase = myCase
        NodeTypes = New Dictionary(Of Integer, clsNetworkNTWNodeType)
        ReachTypes = New Dictionary(Of Integer, clsNetworkNTWReachType)
        NetworkNodes = New Dictionary(Of String, clsNetworkNTWNode)
        ReachSegments = New Dictionary(Of String, clsNetworkNTWReachSegment)
        NetworkGrids = New Dictionary(Of String, clsNetworkNTWASCIIGrid)
    End Sub

    Public Function getAddNode(NodeID As String, NodeName As String, NodeTypeNum As Integer, NodeTypeStr As String, NodeX As Double, NodeY As Double) As clsNetworkNTWNode
        Try
            If Not NetworkNodes.ContainsKey(NodeID.Trim.ToUpper) Then
                Dim newNode As New clsNetworkNTWNode(Me, NodeID, NodeName, NodeTypeNum, NodeTypeStr, NodeX, NodeY)
                NetworkNodes.Add(NodeID.Trim.ToUpper, newNode)
                Return NetworkNodes.Item(NodeID.Trim.ToUpper)
            Else
                Return NetworkNodes.Item(NodeID.Trim.ToUpper)
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error retrieving or adding network.ntw node: " & NodeID)
            Return Nothing
        End Try
    End Function

    Public Function GetAddReachType(TypeNum As Integer, ParentTypeStr As String, TypeStr As String) As clsNetworkNTWReachType
        If Not ReachTypes.ContainsKey(TypeNum) Then
            Dim NewType As New clsNetworkNTWReachType(TypeNum, ParentTypeStr, TypeStr)
            ReachTypes.Add(TypeNum, NewType)
        End If
        Return ReachTypes.Item(TypeNum)
    End Function

    Public Function GetAddNodeType(TypeNum As Integer, TypeStr As String) As clsNetworkNTWNodeType
        If Not NodeTypes.ContainsKey(TypeNum) Then
            Dim NewType As New clsNetworkNTWNodeType(TypeNum, TypeStr)
            NodeTypes.Add(TypeNum, NewType)
        End If
        Return NodeTypes.Item(TypeNum)
    End Function

    Public Function Read() As Boolean
        Try
            Dim myLine As String, Done As Boolean = False
            Dim FileLength As Long
            Dim InReachDescription As Boolean = False
            Dim InGridDescription As Boolean = False

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading network.ntw file...", 0, FileLength)
            Using ntwReader As New StreamReader(Me.SbkCase.CaseDir & "\network.ntw")
                FileLength = ntwReader.BaseStream.Length

                'read the ntw file's header
                ReadHeader(ntwReader)
                Me.Setup.GeneralFunctions.UpdateProgressBar("", ntwReader.BaseStream.Position, FileLength)

                'read the reach segments and nodes
                ReadReachSegmentsAndNodes(ntwReader)
                Me.Setup.GeneralFunctions.UpdateProgressBar("", ntwReader.BaseStream.Position, FileLength)


                'the remainder of the file is actually announced
                While Not ntwReader.EndOfStream
                    myLine = ntwReader.ReadLine
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", ntwReader.BaseStream.Position, FileLength)
                    If myLine.Trim = "[D2Grid description]" Then
                        ReadGridDescriptions(ntwReader)
                    End If
                End While
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadHeader(ByRef ntwReader As StreamReader) As Boolean
        Dim Done As Boolean = False
        Dim myLine As String
        myLine = ntwReader.ReadLine
        Return True
    End Function

    Public Function ReadReachSegmentsAndNodes(ByRef ntwReader As StreamReader) As Boolean
        Try
            Dim Done As Boolean = False
            Dim NetworkNTWReachSegment As clsNetworkNTWReachSegment
            Dim myLine As String

            While Not Done
                myLine = ntwReader.ReadLine.Trim
                If myLine = """*""" Then
                    Done = True
                Else
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", ntwReader.BaseStream.Position, ntwReader.BaseStream.Length)
                    NetworkNTWReachSegment = New clsNetworkNTWReachSegment(Me.Setup, Me.SbkCase, Me)
                    NetworkNTWReachSegment.Read(myLine)

                    If Not NetworkNTWReachSegment.getID = "" Then
                        ReachSegments.Add(NetworkNTWReachSegment.getID.Trim.ToUpper, NetworkNTWReachSegment)
                    End If
                End If
            End While

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function GetReachSegmentByStartEndNode(StartID As String, EndId As String) As clsNetworkNTWReachSegment
        For Each mySeg As clsNetworkNTWReachSegment In ReachSegments.Values
            If mySeg.BN.GetID.Trim.ToUpper = StartID.Trim.ToUpper AndAlso mySeg.EN.GetID.Trim.ToUpper = EndId.Trim.ToUpper Then
                Return mySeg
            End If
        Next
        Return Nothing
    End Function

    Public Function ReadGridDescriptions(ByRef ntwReader As StreamReader) As Boolean
        Try
            Dim Done As Boolean = False
            Dim myLine As String, myGrid As clsNetworkNTWASCIIGrid
            While Not Done
                myLine = ntwReader.ReadLine()
                If myLine.Trim = "" Then
                    Done = True
                ElseIf Not IsNumeric(Me.Setup.GeneralFunctions.RemoveSurroundingQuotes(myLine.Trim, False, True)) Then
                    myGrid = New clsNetworkNTWASCIIGrid(Me.Setup, Me.SbkCase)
                    myGrid.ReadNetworkNTWRecord(myLine)
                    NetworkGrids.Add(myGrid.ID, myGrid)
                End If
            End While
            Return True
        Catch ex As Exception
            Return vbFalse
        End Try
    End Function

    Public Function Write(path As String) As Boolean
        Try
            Using ntwWriter As New StreamWriter(path)

                'write the header
                ntwWriter.WriteLine(Header)

                'write the reach topology
                For Each myReach As String In ReachRecords.Values
                    ntwWriter.WriteLine(myReach)
                Next
                ntwWriter.WriteLine("*")
                ntwWriter.WriteLine("")

                'write the reach descriptions
                ntwWriter.WriteLine("[Reach description]")
                ntwWriter.WriteLine("")
                ntwWriter.WriteLine(" " & ReachRecords.Count)
                For Each myReach As String In ReachDescriptionRecords.Values
                    ntwWriter.WriteLine(myReach)
                Next
                ntwWriter.WriteLine("")

                'write the types of connection nodes
                ntwWriter.WriteLine("[Model connection node]")
                ntwWriter.WriteLine(Setup.GeneralFunctions.SurroundWithCharacter("1.00", Chr(34)))
                ntwWriter.WriteLine("67," & ModelConnectionNodes.Count)
                For Each myNode As String In ModelConnectionNodes.Values
                    ntwWriter.WriteLine(myNode)
                Next

                'write the types of connection branches
                ntwWriter.WriteLine("")
                ntwWriter.WriteLine("[Model connection branch]")
                ntwWriter.WriteLine(Setup.GeneralFunctions.SurroundWithCharacter("1.00", Chr(34)))
                ntwWriter.WriteLine("23," & ModelConnectionBranches.Count)
                For Each myBranch As String In ModelConnectionBranches.Values
                    ntwWriter.WriteLine(myBranch)
                Next
                ntwWriter.WriteLine("")

                'write which nodes are also calculation point
                ntwWriter.WriteLine("[Nodes with calculationpoint]")
                ntwWriter.WriteLine(Setup.GeneralFunctions.SurroundWithCharacter("1.00", Chr(34)))
                ntwWriter.WriteLine("0")
                ntwWriter.WriteLine("")

                'write the reach options
                ntwWriter.WriteLine("[Reach options]")
                ntwWriter.WriteLine(Setup.GeneralFunctions.SurroundWithCharacter("1.00", Chr(34)))
                ntwWriter.WriteLine("0")
                ntwWriter.WriteLine("")

                'write the NTW properties
                ntwWriter.WriteLine("[NTW properties]")
                ntwWriter.WriteLine(Setup.GeneralFunctions.SurroundWithCharacter("1.00", Chr(34)))
                ntwWriter.WriteLine("3")
                ntwWriter.WriteLine("v1=4")
                ntwWriter.WriteLine("v2=2")
                ntwWriter.WriteLine("v3=5")

            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error writing Network.NTW file.")
            Return False
        End Try
    End Function

    Public Sub Build()
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If myReach.InUse Then
                BuildReachRecord(myReach)
                BuildReachDescriptionRecord(myReach)
            End If
        Next
        For Each myReachNode As clsSbkReachNode In SbkCase.CFTopo.ReachNodes.ReachNodes.Values
            If myReachNode.InUse Then
                BuildModelConnectionNodesRecord(myReachNode)
            End If
        Next
    End Sub

    Public Sub BuildModelConnectionNodesRecord(ByRef myReachNode As clsSbkReachNode)
        'maakt een lijst met alle in het model voorkomende typen knopen?
        Dim myStr As String
        myStr = Setup.GeneralFunctions.SurroundWithCharacter(myReachNode.NodeType.SbkNum, Chr(34)) & ","
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(myReachNode.NodeType.ID, Chr(34)) & ","
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("1", Chr(34)) & ","
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("SOBEK", Chr(34)) & ","
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("4", Chr(34))
        ModelConnectionNodes.Add(myReachNode.ID, myStr)
    End Sub

    Public Sub BuildModelConnectionBranchesRecord(ByRef myReach As clsSbkReach)
        'maakt een lijst met alle in het model voorkomende typen takken?
        Dim mystr As String
        mystr = Setup.GeneralFunctions.SurroundWithCharacter(myReach.ReachType.ToString, Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter(myReach.ReachType.ToString, Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter("2", Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter("SOBEK", Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter("0", Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter("SOBEK", Chr(34)) & ","
        mystr = Setup.GeneralFunctions.SurroundWithCharacter("31", Chr(34)) & ","
        ModelConnectionBranches.Add(myReach.Id, mystr)
    End Sub

    Public Sub BuildReachRecord(ByRef Reach As clsSbkReach)

        'start by adding the reach
        Dim myStr As String = ""
        myStr = Setup.GeneralFunctions.SurroundWithCharacter(Reach.Id, Chr(34)) & ","                       'id
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.Id, Chr(34)) & ","                      'name
        myStr &= ReachRecords.Count + 1 & ","                                                                    'index number
        myStr &= Reach.ReachType.ParentReachType & ","                                                                             'reach type number
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.ReachType.ParentReachType.ToString, Chr(34)) & ","             'reachtype written out
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","                            'empty string
        myStr &= "0,0,0,0,"                                                                                 'unknown
        myStr &= Reach.getReachLength & ","                                                                 'reach length
        myStr &= "0,0,0,"
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.bn.ID, Chr(34)) & ","                   'id of start node
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.bn.Name, Chr(34)) & ","                 'name of start node
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","                            'empty string
        myStr &= ReachRecords.Count + 1 & ","                                                                    'reach index?
        myStr &= Reach.bn.nt & ","                                                                          'type of start node (numeric)
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.bn.nt.ToString, Chr(34)) & ","          'type of start node written out
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","                            'empty string
        myStr &= Reach.bn.X & "," & Reach.bn.Y & ","                                                        'coordinate of start node
        myStr &= "0,0,"
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("SYS_DEFAULT", Chr(34)) & ","                 'some setting
        myStr &= "0,"
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.en.ID, Chr(34)) & ","                   'id of end node
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.en.Name, Chr(34)) & ","                 'name of end node
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","                            'empty string
        myStr &= ReachRecords.Count + 1 & ","                                                                    'reach index?
        myStr &= Reach.en.nt & ","                                                                          'type of end node (numeric)
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.en.nt.ToString, Chr(34)) & ","          'type of end node written out
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","                            'empty string
        myStr &= Reach.en.X & "," & Reach.en.Y & ","                                                        'coordinate of end node
        myStr &= "0,"
        myStr &= Reach.getReachLength & ","
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("SYS_DEFAULT", Chr(34)) & ","                 'some setting
        myStr &= "0,"
        ReachRecords.Add(Reach.Id.Trim.ToUpper, myStr)


    End Sub

    Public Sub BuildReachDescriptionRecord(ByRef Reach As clsSbkReach)
        'this routine adds a reach description to the network.ntw file
        Dim myStr As String = ""

        myStr = ""
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(ReachDescriptionRecords.Count + 1, Chr(34)) & ","   'reach number
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter("", Chr(34)) & ","                            'empty string
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.bn.ID, Chr(34)) & ","                   'id of start node
        myStr &= Setup.GeneralFunctions.SurroundWithCharacter(Reach.en.ID, Chr(34)) & ","                   'id of end node
        myStr &= "0,"                                                                                       'unknown
        myStr &= Reach.cpCount & ","                                                                        'the number of vectorpoints

        Reach.calcBoundingBox()
        myStr &= Reach.BoundingBox.XLL & "," & Reach.BoundingBox.YLL & "," & Reach.BoundingBox.XUR & "," & Reach.BoundingBox.YUR & "," & Reach.getReachLength & ","
        myStr &= "0,100,-1"
        ReachDescriptionRecords.Add(Reach.Id.Trim.ToUpper, myStr)

    End Sub

    Public Function BuildFromCase(ByRef myCase As clsSobekCase) As Boolean

        'build the header
        Header = Chr(34) & "NTW6.6" & Chr(34) & "," & Chr(34) & "c\SOBEK214\WSHD.lit\CMTWORK\ntrpluv.ini" & Chr(34) & "," & Chr(34) & "SOBEK-LITE, edit network" & Chr(34)

        'build the reaches section
        For Each myReach As clsSbkReach In myCase.CFTopo.Reaches.Reaches.Values
            If myReach.InUse Then
                BuildReachRecord(myReach)
            End If
        Next

        'build the reachnodes section
        For Each myNode As clsSbkReachNode In myCase.CFTopo.ReachNodes.ReachNodes.Values
            If myNode.InUse Then
            End If
        Next

    End Function



    Public Function ReadReaches() As Boolean
        Dim curLine As String, curReach As clsSbkReach
        Dim Done As Boolean = False

        Try
            Using NTWReader = New StreamReader(SbkCase.CaseDir & "\network.ntw")
                curLine = NTWReader.ReadLine 'just the header

                While Not Done
                    curLine = NTWReader.ReadLine
                    If curLine.Trim = Chr(34) & "*" & Chr(34) Then
                        Done = True
                    Else
                        curReach = parseReach(curLine)
                        SbkCase.CFTopo.Reaches.Reaches.Add(curReach.Id.Trim.ToUpper, curReach)
                    End If
                End While

            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function parseReach(myString As String) As clsSbkReach
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim tmp As String
        Dim bnID As String, enID As String

        'SIEBE: UNDER CONSTRUCTION!!!!!

        myReach.Id = Setup.GeneralFunctions.ParseString(myString, ",", 2)
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        myReach.num = Setup.GeneralFunctions.ParseString(myString, ",", 2)
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'is branch type number, but we'll use the next line to retrieve the branch type
        myReach.ReachType = SbkCase.BranchTypes.GetBranchType(Setup.GeneralFunctions.ParseString(myString, ",", 2))
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'contains the reach length, but this should be calculated, not read
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?

        bnID = Setup.GeneralFunctions.ParseString(myString, ",", 2)     'ID of the begin node
        myReach.bn = SbkCase.CFTopo.getAddReachNode(bnID)
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0 or 1? meaning unclear
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'node type number, but we'll get the type from its description (next line)
        myReach.bn.NodeType = SbkCase.NodeTypes.GetNodeType(Setup.GeneralFunctions.ParseString(myString, ",", 2))              'always empty?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        myReach.bn.X = Setup.GeneralFunctions.ParseString(myString, ",")
        myReach.bn.Y = Setup.GeneralFunctions.ParseString(myString, ",")
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always "SYS_DEFAULT"?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?


        enID = Setup.GeneralFunctions.ParseString(myString, ",", 2)     'ID of the end node
        myReach.en = SbkCase.CFTopo.getAddReachNode(enID)
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)      'type number for the reach end node e.g. 14
        myReach.en.NodeType = SbkCase.NodeTypes.GetNodeType(Setup.GeneralFunctions.ParseString(myString, ",", 2)) 'type description for the begin node e.g. SBK_BOUNDARY
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always empty?
        myReach.en.X = Setup.GeneralFunctions.ParseString(myString, ",")
        myReach.en.Y = Setup.GeneralFunctions.ParseString(myString, ",")
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always "SYS_DEFAULT"?
        tmp = Setup.GeneralFunctions.ParseString(myString, ",", 2)              'always 0?

        Return myReach




    End Function

End Class
