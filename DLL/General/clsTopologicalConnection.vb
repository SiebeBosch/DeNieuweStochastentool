Imports STOCHLIB.General

Public Class clsTopologicalConnection
    Private Setup As clsSetup

    Friend FromNode As clsXY                        'the area from which the connection takes place
    Friend ToNode As clsXY                          'the area to which the connection takes place
    Friend Structures As New Dictionary(Of String, clsTopologyStructurelocation)        'a list of structures that represent the connection

    Public Sub New(ByRef mySetup As clsSetup, ByRef myFrom As clsXY, ByRef myTo As clsXY, myStructure As clsXY)
        Setup = mySetup
        FromNode = myFrom
        ToNode = myTo
        If Not myStructure Is Nothing Then Structures.Add(myStructure.ID.Trim.ToUpper, myStructure)
    End Sub

    Public Function AddStructureHalfway(StructureID As String, StructureType As GeneralFunctions.enmNodetype) As Boolean
        Try
            'calculate the xy coordinate halfway our connection
            Dim X As Double = (FromNode.X + ToNode.X) / 2
            Dim Y As Double = (FromNode.Y + ToNode.Y) / 2
            Dim myStructure As New clsTopologyStructurelocation(Me.Setup)
            myStructure.ID = StructureID
            myStructure.Name = myStructure.ID
            myStructure.X = X
            myStructure.Y = Y
            myStructure.StructureType = StructureType
            Structures.Add(StructureID.Trim.ToUpper, myStructure)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class
