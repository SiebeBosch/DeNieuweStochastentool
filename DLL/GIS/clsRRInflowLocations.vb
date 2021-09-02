Imports STOCHLIB.General

Public Class clsRRInflowLocations

  '--------------------------------------------------------------------------------
  'Author: Siebe Bosch
  'Date: 4-4-2013
  'Description: a class module to store all SOBEK-RR inflow locations within a catchment
  '--------------------------------------------------------------------------------
  Friend RRInflowLocations As New Dictionary(Of String, clsRRInflowLocation)
  Friend ContributingArea As Double

  Private Setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.Setup = mySetup
  End Sub


    Friend Function getLocation(ByVal myID As String) As clsRRInflowLocation
    If RRInflowLocations.ContainsKey(myID.Trim.ToUpper) Then
      Return RRInflowLocations.Item(myID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function

  Friend Function getRRCFIds() As Dictionary(Of String, String)

    Dim myDict = New Dictionary(Of String, String)
    For Each myInflowLocation As clsRRInflowLocation In RRInflowLocations.Values
      myDict.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation.NodeID.Trim)
    Next

    Return myDict
  End Function

End Class
