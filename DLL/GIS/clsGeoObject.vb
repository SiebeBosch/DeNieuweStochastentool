Imports STOCHLIB.General

Public Class clsGeoObject
    Private Setup As clsSetup
    Public ObjectType As GeneralFunctions.enmGeoObjectType
    Public ShapeIdx As Integer

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class
