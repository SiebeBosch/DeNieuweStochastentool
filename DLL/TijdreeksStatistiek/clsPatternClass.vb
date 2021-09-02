Public Class clsPatternEventClass
    Dim P As Double 'the probability
    Dim ID As String 'identifies the pattern by percentage classes
    Dim Segments As New List(Of clsPatternSegment)      'lists for each temporal segment which value class applies
    Dim Events As List(Of Integer) 'all events that obey this pattern. The integer values represent the event number

    Public Sub New(myID As String, mySegments As List(Of clsPatternSegment), EventNum As Integer)
        ID = myID
        Segments = mySegments
        Events = New List(Of Integer)
        Events.Add(EventNum)
    End Sub

    Public Function CountEvents() As Integer
        Return Events.Count
    End Function

    Public Function GetSegments() As List(Of clsPatternSegment)
        Return Segments
    End Function


    Public Sub AddEvent(EventNum As Integer)
        Events.Add(EventNum)
    End Sub

    Public Function getID() As String
        ID = ""
        For Each mySeg As clsPatternSegment In Segments
            ID &= mySeg.getID
        Next
        Return ID
    End Function

End Class
