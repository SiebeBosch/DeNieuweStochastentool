Public Class clsPumpOperationItem
    Friend Cap As Double 'capacity must be in m3/s

    'notice that we store the on/off levels as OBJECTS rather than DOUBLE. The reason is that a double type cannot store NULL
    'v2.00: creating an item, make sure the units are in m3/s for capacity, cm for margin and m + AD for levels
    Friend UpOnMargin As Double 'must be in cm
    Friend UpOffMargin As Double 'must be in cm
    Friend DownOnMargin As Double 'must be in cm
    Friend DownOffMargin As Double 'must be in cm
    Friend UpOnLevel As Double 'must be in m AD
    Friend UpOffLevel As Double 'must be in m AD
    Friend DownOnLevel As Double 'must be in  m AD
    Friend DownOffLevel As Double 'must be in m AD

    Public Sub New(myCapm3ps As Double, myUpOnMarginCM As Double, myUpOffMarginCM As Double, myDownOnMarginCM As Double, myDownOffMarginCM As Double, myUpOnLevel As Double, myUpOffLevel As Double, myDownOnLevel As Double, myDownOffLevel As Double)
        'new in v1.798: introducing unit conversion here
        Cap = myCapm3ps
        UpOnMargin = myUpOnMarginCM
        UpOffMargin = myUpOffMarginCM
        DownOnMargin = myDownOnMarginCM
        DownOffMargin = myDownOffMarginCM
        UpOnLevel = myUpOnLevel
        UpOffLevel = myUpOffLevel
        DownOnLevel = myDownOnLevel
        DownOffLevel = myDownOffLevel
    End Sub

End Class
