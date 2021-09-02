Public Class cls2DDomain
    Public Domain As Integer
    Public ResultsCases As New Dictionary(Of String, cls2DResultsCase)
    Public lllat As Double  'latitude of the lower left corner
    Public lllon As Double  'longitude of the lower left corner
    Public urlat As Double  'latitude of the upper right corner
    Public urlon As Double  'longitude of the upper right corner

    Public Sub New(DomainNumber As Integer)
        Domain = DomainNumber
    End Sub
End Class
