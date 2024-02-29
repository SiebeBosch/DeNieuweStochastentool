Public Class clsPercentileClass
    Public Name As String
    Public Parameter As GeneralFunctions.enmModelParameter
    Public RepresentativePercentile As Double   'representative value for this class: expressed as a decimal between 0 and 1
    Public UboundPercentile As Double           'upper bound percentile for this class: expressed as a decimal between 0 and 1
    Public LBoundPercentile As Double           'lower bound percentile for this class: expressed as a decimal between 0 and 1
    Public UboundValue As Double                'upper bound value for this class
    Public LBoundValue As Double                'lower bound value for this class
    Public RepresentativeValue As Double        'representative value for this class
    Public EventIdxNums As New List(Of Integer)        ' List of Event Index Numbers belonging to this percentile
    Public SideParameterValues As New Dictionary(Of GeneralFunctions.enmModelParameter, Double) 'median value for each side parameter within this class
End Class
