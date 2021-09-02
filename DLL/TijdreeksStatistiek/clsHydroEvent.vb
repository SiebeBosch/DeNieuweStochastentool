Public Class clsHydroEvent
    Dim StartDate As DateTime
    Dim EndDate As DateTime
    Dim LocationID As String
    Dim Parameter As String
    Dim EventNum As Integer
    Public EventSum As Double
    Public EventMax As Double
    Public EventMin As Double
    Public VolumeClass As String
    Public BoundaryClass As String
    Public InitialClass As String
    Public PatternClass As String
    Public Values As List(Of Double)

    Public Sub New()
        Values = New List(Of Double)
    End Sub

    Public Sub SetEventNum(myNum As Integer)
        EventNum = myNum
    End Sub

    Public Function GetEventNum() As Integer
        Return EventNum
    End Function

    Public Function GetStartDate() As DateTime
        Return StartDate
    End Function

    Public Function GetEndDate() As DateTime
        Return EndDate
    End Function

    Public Sub SetStartDate(myDate As DateTime)
        StartDate = myDate
    End Sub

    Public Sub SetEndDate(mydate As DateTime)
        EndDate = mydate
    End Sub

    Public Sub SetLocationID(myLocationID As String)
        LocationID = myLocationID
    End Sub

    Public Function GetLocationID() As String
        Return LocationID
    End Function

    Public Sub SetParameter(myParameter As String)
        Parameter = myParameter
    End Sub

    Public Function GetParameter() As String
        Return Parameter
    End Function

    Public Function getDuration() As Integer
        Return Values.Count
    End Function

End Class
