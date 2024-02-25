Public Class clsPercentileClass
    Public RepresentativePercentile As Double   'representative value for this class: expressed as a decimal between 0 and 1
    Public UboundPercentile As Double           'upper bound percentile for this class: expressed as a decimal between 0 and 1
    Public LBoundPercentile As Double           'lower bound percentile for this class: expressed as a decimal between 0 and 1
    Public UboundValue As Double                'upper bound value for this class
    Public LBoundValue As Double                'lower bound value for this class
    Public EventNums As New List(Of Integer)        ' List of Event Numbers belonging to this percentile
End Class
