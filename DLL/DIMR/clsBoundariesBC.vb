Imports STOCHLIB.General
Imports System.IO
Public Class clsBoundariesBC
    Private Setup As clsSetup
    Dim Path As String

    Dim Version As Integer
    Dim FileType As String
    Dim Comment As New List(Of String)
    Friend BoundaryConditions As New Dictionary(Of String, clsBoundaryCondition)

    Public Sub New(ByRef mySetup As clsSetup, myPath As String)
        Setup = mySetup
        Path = myPath
    End Sub

    Public Function Read() As Boolean
        Try
            Dim myLine As String
            Dim name As String
            Dim inGeneral As Boolean = False
            Dim inForcing As Boolean = False
            Dim myBoundaryCondition As New clsBoundaryCondition()

            Using bcReader As New StreamReader(Path)
                While Not bcReader.EndOfStream
                    myLine = bcReader.ReadLine.Trim
                    If Left(myLine, 1) = "#" Then
                        'commentaarrregel
                        Comment.Add(myLine)
                    ElseIf myLine = "[General]" Then
                        inGeneral = True
                        inForcing = False
                    ElseIf myLine = "[Forcing]" Then
                        inGeneral = False
                        inForcing = True
                    ElseIf inGeneral Then
                        If Left(myLine, 11) = "fileVersion" Then
                            Version = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                        ElseIf Left(myLine, 8) = "fileType" Then
                            FileType = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                        End If
                    ElseIf inForcing Then
                        If Left(myLine, 4) = "name" Then
                            name = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                            myBoundaryCondition = New clsBoundaryCondition(Me.Setup, name)
                        ElseIf Left(myLine, 8) = "function" Then
                            myBoundaryCondition.setFunction(Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine))
                        ElseIf Left(myLine, 17) = "timeInterpolation" Then
                            myBoundaryCondition.setTimeInterpolation(Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine))
                        ElseIf Left(myLine, 8) = "quantity" Then
                            Dim quantityName As String = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                            myLine = bcReader.ReadLine
                            Dim quantityUnit As String = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                            Dim quantity As New clsQuantity(quantityName, quantityUnit)
                            myBoundaryCondition.AddQuantity(quantityName.Trim.ToUpper, quantity)
                        Else
                            Dim XVal As Double = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine, " ")
                            Dim YVal As Double = Me.Setup.GeneralFunctions.ParsestringNumeric(myLine, " ")
                            myBoundaryCondition.AddValuePair(XVal, YVal)
                        End If
                    End If


                End While
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading boundaries.bc file: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function Write(path As String) As Boolean
        Try
            Using bcWriter As New StreamWriter(path)
                For Each myComment As String In Comment
                    bcWriter.WriteLine(myComment)
                Next
                bcWriter.WriteLine("[General]")
                bcWriter.WriteLine("fileVersion = " & Version)
                bcWriter.WriteLine("fileType = " & FileType)
                For Each myBC As clsBoundaryCondition In BoundaryConditions.Values
                    bcWriter.WriteLine("")
                    bcWriter.WriteLine("[Forcing]")
                    bcWriter.WriteLine("name       = " & myBC.name)
                    bcWriter.WriteLine("function   = " & myBC.bcfunction)
                    bcWriter.WriteLine("timeInterpolation = " & myBC.timeInterpolation)
                    For Each myQuantity As clsQuantity In myBC.quantities.Values
                        bcWriter.WriteLine("quantity   = " & myQuantity.name)
                        bcWriter.WriteLine("unit       = " & myQuantity.unit)
                    Next
                    For i = 0 To myBC.DataTable.XValues.Count - 1
                        bcWriter.WriteLine(myBC.DataTable.XValues.Values(i) & "    " & myBC.DataTable.Values1.Values(i))
                    Next
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error writing boundaries.bc file: " & ex.Message)
            Return False
        End Try
    End Function
End Class
