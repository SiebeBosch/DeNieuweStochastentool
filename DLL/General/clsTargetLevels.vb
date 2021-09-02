Public Class clsTargetLevels
    Dim ShapeID As String   'shape ID
    Dim WPOutlet As Double = Double.NaN 'winter target level for outlet situations. We initialize this value to a NaN so we can determine if a value has been set in the first place
    Dim WPInlet As Double = Double.NaN  'winter target level for inlet situations. We initialize this value to a NaN so we can determine if a value has been set in the first place
    Dim ZPOutlet As Double = Double.NaN 'summer target level for outlet situations. We initialize this value to a NaN so we can determine if a value has been set in the first place
    Dim ZPInlet As Double = Double.NaN 'summer target level for inlet situations. We initialize this value to a NaN so we can determine if a value has been set in the first place

    Public Sub setZP(myZP As Double)
        ZPOutlet = myZP
        ZPInlet = myZP
    End Sub
    Public Sub setwP(mywP As Double)
        WPOutlet = mywP
        WPInlet = mywP
    End Sub
    Public Sub setZPOutlet(myZP As Double)
        ZPOutlet = myZP
    End Sub
    Public Sub setZPinlet(myZP As Double)
        ZPInlet = myZP
    End Sub

    Public Function getLowestLevel() As Double
        Dim Lowest As Double = Double.NaN
        If WPOutletHasValue() Then Lowest = getWPOutlet()
        If ZPOutletHasValue() AndAlso getZPOutlet() < Lowest Then Lowest = getZPOutlet()
        If WPInletHasValue() AndAlso getWPInlet() < Lowest Then Lowest = getWPInlet()
        If ZPInletHasValue() AndAlso getZPInlet() < Lowest Then Lowest = getZPInlet()
        Return Lowest
    End Function

    Public Function getLowestOutletLevel() As Double
        Dim Lowest As Double = Double.NaN
        If WPOutletHasValue() Then Lowest = getWPOutlet()
        If ZPOutletHasValue() AndAlso getZPOutlet() < Lowest Then Lowest = getZPOutlet()
        Return Lowest
    End Function

    Public Function getHighestOutletLevel() As Double
        Dim Highest As Double = Double.NaN
        If WPOutletHasValue() Then Highest = getWPOutlet()
        If ZPOutletHasValue() AndAlso getZPOutlet() > Highest Then Highest = getZPOutlet()
        Return Highest
    End Function

    Public Sub setWPOutlet(myWP As Double)
        WPOutlet = myWP
    End Sub
    Public Sub setWPinlet(myWP As Double)
        WPInlet = myWP
    End Sub

    Public Sub setShapeID(myID As String)
        ShapeID = myID
    End Sub

    Public Function HasValue() As Boolean
        Return (WPOutletHasValue() Or ZPOutletHasValue() Or WPInletHasValue() Or ZPInletHasValue())
    End Function
    Public Function OutletHasValue() As Boolean
        Return (WPOutletHasValue() Or ZPOutletHasValue())
    End Function
    Public Function WPOutletHasValue() As Boolean
        If Double.IsNaN(WPOutlet) Then Return False Else Return True
    End Function

    Public Function WPInletHasValue() As Boolean
        If Double.IsNaN(WPInlet) Then Return False Else Return True
    End Function

    Public Function ZPOutletHasValue() As Boolean
        If Double.IsNaN(ZPOutlet) Then Return False Else Return True
    End Function
    Public Function ZPInletHasValue() As Boolean
        If Double.IsNaN(ZPOutlet) Then Return False Else Return True
    End Function

    Public Function getZPOutlet() As Double
        Return ZPOutlet
    End Function
    Public Function getWPOutlet() As Double
        Return WPOutlet
    End Function
    Public Function getZPInlet() As Double
        Return ZPInlet
    End Function
    Public Function getWPInlet() As Double
        Return WPInlet
    End Function

    Public Function GetID() As String
        Return ShapeID
    End Function

End Class
