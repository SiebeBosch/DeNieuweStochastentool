Imports STOCHLIB.GeneralFunctions

Public Class clsRRLink

  Private i As Integer
  Public Flow As New List(Of clsSobekResult)
  Public FlowYearSummaries As New Hashtable

  Public Id As String
  Public Name As String
  Public BranchType As New clsSobekBranchType
  Public FromNode As clsRRNodeTPRecord
  Public ToNode As clsRRNodeTPRecord
  Public InUse As Boolean

  Public Sub New()
  End Sub


End Class
