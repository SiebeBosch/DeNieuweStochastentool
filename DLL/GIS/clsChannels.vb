Imports STOCHLIB.General

Public Class clsChannels
  Friend Channels As New Dictionary(Of String, clsChannelCategory)
  Private Setup As clsSetup

  Public Sub New(ByVal mySetup As clsSetup)
    Setup = mySetup
  End Sub

  Friend Function getAdd(ByVal myCategory As String) As clsChannelCategory
    Dim myCat As clsChannelCategory
    If Channels.ContainsKey(myCategory.Trim.ToUpper) Then
      Return Channels.Item(myCategory.Trim.ToUpper)
    Else
      myCat = New clsChannelCategory(Setup)
      myCat.ID = myCategory.Trim.ToUpper
      myCat.Table = New clsSobekTable(Setup)
      Channels.Add(myCat.ID, myCat)
      Return myCat
    End If
  End Function

End Class
