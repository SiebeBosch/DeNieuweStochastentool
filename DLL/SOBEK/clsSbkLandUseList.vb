Public Class clsSbkLandUseList

  Public SbkLandUseList As New Dictionary(Of Integer, clsSbkLandUse)

  Public Sub New()
    Dim i As Integer, mySbk As clsSbkLandUse
    For i = 1 To 19
      mySbk = New clsSbkLandUse
      mySbk.Num = i
      mySbk.Area = 0

      Select Case i
        Case Is = 1
          mySbk.Name = "grass"
        Case Is = 2
          mySbk.Name = "corn"
        Case Is = 3
          mySbk.Name = "potatoes"
        Case Is = 4
          mySbk.Name = "sugarbeet"
        Case Is = 5
          mySbk.Name = "grain"
        Case Is = 6
          mySbk.Name = "miscellaneous"
        Case Is = 7
          mySbk.Name = "non-arable land"
        Case Is = 8
          mySbk.Name = "greenhouse area"
        Case Is = 9
          mySbk.Name = "orchard"
        Case Is = 10
          mySbk.Name = "bulbous plants"
        Case Is = 11
          mySbk.Name = "foliage forest"
        Case Is = 12
          mySbk.Name = "pine forest"
        Case Is = 13
          mySbk.Name = "nature"
        Case Is = 14
          mySbk.Name = "fallow"
        Case Is = 15
          mySbk.Name = "vegetables"
        Case Is = 16
          mySbk.Name = "flowers"
        Case Is = 17
          mySbk.Name = "water"
        Case Is = 18
          mySbk.Name = "paved"
        Case Is = 19
          mySbk.Name = "glastuinbouw"
      End Select
      SbkLandUseList.Add(mySbk.Num, mySbk)
    Next
  End Sub

  Public Function AddArea(ByVal myCode As Integer, ByVal myArea As Double) As Boolean
    Dim mySbk As clsSbkLandUse
    If SbkLandUseList.ContainsKey(myCode) Then
      mySbk = SbkLandUseList.Item(myCode)
      mySbk.Area += myArea
      Return True
    Else
      Return False
    End If
  End Function

  Public Function getTotalArea() As Double
    Dim totalArea As Double = 0
    For Each mySbk As clsSbkLandUse In SbkLandUseList.Values
      totalArea += mySbk.Area
    Next
    Return totalArea
  End Function

  Public Function getArea(ByVal myCode As Integer) As Double
    If SbkLandUseList.ContainsKey(myCode) Then
      Return SbkLandUseList.Item(myCode).Area
    Else
      Return 0
    End If
  End Function

  Public Function getName(ByVal myCode As Integer) As String
    Dim mySbk As clsSbkLandUse
    If SbkLandUseList.ContainsKey(myCode) Then
      mySbk = SbkLandUseList.Item(myCode)
      Return mySbk.Name
    Else
      Return ""
    End If
  End Function

End Class
