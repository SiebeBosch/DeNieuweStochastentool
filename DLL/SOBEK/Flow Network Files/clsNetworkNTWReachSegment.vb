Imports STOCHLIB.General

Public Class clsNetworkNTWReachSegment
    Dim ID As String
    Dim Name As String
    Friend ReachID As String
    Friend REACHTYPE As clsNetworkNTWReachType
    Friend BN As clsNetworkNTWNode
    Friend EN As clsNetworkNTWNode

    Friend FromNodeChainage As Double      'since one node is used on multiple reach segments we'll store the chainage per reach segment
    Friend ToNodeChainage As Double        'since one node is used on multiple reach segments we'll store the chainage per reach segment
    Friend Length As Double
    Friend ContainsStructure As Boolean
    Private Setup As clsSetup
    Private SbkCase As clsSobekCase
    Private NetworkNTW As clsNetworkNTW

    Public Sub New(ByRef mySetup As clsSetup, ByRef mySbkCase As clsSobekCase, ByRef myNetworkNTW As clsNetworkNTW)
        Setup = mySetup
        SbkCase = mySbkCase
        NetworkNTW = myNetworkNTW
    End Sub

    'Public Function HasStructure() As Boolean
    '    Try
    '        Dim myReach As clsSbkReach = SbkCase.CFTopo.Reaches.GetReach(ReachID)
    '        If myReach Is Nothing Then Throw New Exception("Error retrieving reach for reachsegment " & ID)

    '        If myReach.HasStructure(FromNodeChainage, ToNodeChainage) OrElse Me.Setup.GeneralFunctions.isStructure(BN.GetNodeType) OrElse Me.Setup.GeneralFunctions.isStructure(EN.GetNodeType) Then
    '            Return True
    '        Else
    '            Return False
    '        End If

    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError(ex.Message)
    '        Return False
    '    End Try
    'End Function

    Public Function IsFlowReach() As Boolean
        Select Case REACHTYPE.getParentReachType
            Case Is = "SBK_CHANNEL"
                Return True
            Case Is = "SBK_CHANNEL&LAT"
                Return True
            Case Is = "SBK_PIPE"
                Return True
            Case Is = "SBK_PIPE&RUNOFF"
                Return True
            Case Is = "SBK_DWAPIPE&RUNOFF"
                Return True
            Case Is = "SBK_RWAPIPE&RUNOFF"
                Return True
            Case Is = "SBK_PIPE&MEAS"
                Return True
            Case Is = "SBK_INTWEIR"
                Return True
            Case Is = "SBK_INTORIFICE"
                Return True
            Case Is = "SBK_INTCULVERT"
                Return True
            Case Is = "SBK_INTPUMP"
                Return True
            Case Is = "SBK_STREET"
                Return True
            Case Is = "SBK_PIPE&INFILTRATION"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Function getParentReachType() As String
        Return REACHTYPE.getParentReachType
    End Function

    Public Function getID() As String
        Return ID
    End Function

    Public Function getUpstreamSegment() As clsNetworkNTWReachSegment
        For Each mySeg As clsNetworkNTWReachSegment In NetworkNTW.ReachSegments.Values
            If mySeg.EN.getID = BN.getID Then
                Return mySeg
            End If
        Next
        Return Nothing
    End Function

    Public Function Read(Record As String) As Boolean
        Dim RecordIdx As Integer, myStr As String
        Dim ReachTypeNum As Integer, ReachParentTypeStr As String = "", ReachTypeStr As String = ""
        Dim FromNodeID As String = "", FromNodeName As String = "", FromNodeTypeNum As Integer, FromNodeTypeStr As String = "", FromNodeX As Double, FromNodeY As Double
        Dim ToNodeID As String = "", ToNodeName As String = "", ToNodeTypeNum As Integer, ToNodeTypeStr As String = "", ToNodeX As Double, ToNodeY As Double

        Try
            While Not Record = ""
                myStr = Me.Setup.GeneralFunctions.ParseString(Record, ",", 2)
                Select Case RecordIdx
                    Case Is = 0
                        ID = myStr
                        ReachID = Setup.GeneralFunctions.RemovePostfix(ID, "_")
                    Case Is = 1
                        Name = myStr
                    Case Is = 3
                        ReachTypeNum = Convert.ToInt32(myStr)
                    Case Is = 4
                        ReachParentTypeStr = myStr
                    Case Is = 5
                        ReachTypeStr = myStr
                    Case Is = 10
                        Length = Convert.ToDouble(myStr)
                    Case Is = 14
                        FromNodeID = myStr
                    Case Is = 15
                        FromNodeName = myStr
                    Case Is = 18
                        FromNodeTypeNum = Convert.ToInt16(myStr)
                    Case Is = 19
                        FromNodeTypeStr = myStr
                    Case Is = 21
                        FromNodeX = Convert.ToDouble(myStr)
                    Case Is = 22
                        FromNodeY = Convert.ToDouble(myStr)
                    Case Is = 24
                        FromNodeChainage = Convert.ToDouble(myStr)
                    Case Is = 27
                        ToNodeID = myStr
                    Case Is = 28
                        ToNodeName = myStr
                    Case Is = 31
                        ToNodeTypeNum = Convert.ToInt16(myStr)
                    Case Is = 32
                        ToNodeTypeStr = myStr
                    Case Is = 34
                        ToNodeX = Convert.ToDouble(myStr)
                    Case Is = 35
                        ToNodeY = Convert.ToDouble(myStr)
                    Case Is = 37
                        ToNodeChainage = Convert.ToDouble(myStr)
                End Select

                RecordIdx += 1
            End While

            'as it appears, the ToNodeChainage returend is ZERO if the To-node is a connection node type. This means we will have to correct this here
            If ToNodeChainage = 0 AndAlso ToNodeTypeStr = "SBK_CHANNELCONNECTION" Then
                Dim myReach As clsSbkReach = SbkCase.CFTopo.Reaches.GetReach(ReachID)
                If Not myReach Is Nothing Then ToNodeChainage = myReach.getReachLength
            End If


            REACHTYPE = NetworkNTW.GetAddReachType(ReachTypeNum, ReachParentTypeStr, ReachTypeStr)

            BN = NetworkNTW.getAddNode(FromNodeID, FromNodeName, FromNodeTypeNum, FromNodeTypeStr, FromNodeX, FromNodeY)
            EN = NetworkNTW.getAddNode(ToNodeID, ToNodeName, ToNodeTypeNum, ToNodeTypeStr, ToNodeX, ToNodeY)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
