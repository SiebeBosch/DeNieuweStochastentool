Public Class clsGISSettings

    Public DefaultLanduseFieldName As String = "LANDUSE"    'used for situations where landuse grid are being polygonized
    Public DefaultSoilTypeFieldName As String = "SOILTYPE"  'used for situations where soil grid is being polygonized

    Public UseElevationGrid As Boolean
    Public UseSeepageGrid As Boolean
    Public UseAreaShapeFile As Boolean
    Public UseSewageAreaShapeFile As Boolean
    Public UseCSOShapeFile As Boolean
    Public UseWWTPShapeFile As Boolean
    Public UseDischargepointsShapeFile As Boolean
    Public UseWeirShapeFile As Boolean
    Public UsePumpShapeFile As Boolean
    Public UseChannelShapeFile As Boolean
    Public UnderwaterProfileBasedOnUpstreamArea As Boolean
    Public useChannelAreaShapeFile As Boolean
    Public UseSoilData As Boolean
    Public UseLanduseData As Boolean

    Public ElevationMultiplier As Double = 1
    Public minShapeArea As Double

End Class
