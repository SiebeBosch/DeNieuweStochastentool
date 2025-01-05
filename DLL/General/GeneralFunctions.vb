Imports System.IO
Imports System.Windows
Imports STOCHLIB.General
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.Data.OleDb
Imports Microsoft.VisualBasic.DateAndTime
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.DataVisualization
Imports HtmlAgilityPack
Imports Newtonsoft.Json
Imports MapWinGIS
Imports Microsoft.VisualBasic.CompilerServices
Imports System.Data.SQLite
Imports System.Text


Public Class GeneralFunctions
    Private setup As clsSetup
    Public unitConversion As clsUnitConversion

    Public Enum enmErrorLevel
        _Error = 0
        _Warning = 1
        _Message = 2
    End Enum

    Public Enum enmGebiedsreductie
        constante = 0
        oppervlak = 1
    End Enum

    Structure StrTimeSeries
        Public ID As String
        Public LOCATIONID As String
        Public PARAMETER As String
    End Structure
    Public Enum enmDIMRIniFileReplacementOperation
        bestand = 0         'replaces the entire file
        sectie = 1      'replaces one specific section, e.g. [boundary]
        waarde = 2        'replaces one specific value in one specific section. E.g. T0 in the section [Structure] for object with id "dambreak"
    End Enum


    Public Enum enmTimestepStatistic
        first = 0
        last = 1
        mean = 2
        median = 3
        min = 4
        max = 5
        sum = 6
    End Enum
    Public Enum enmModelParameter
        none = 0
        precipitation = 1
        evaporation = 2
        lz = 3 'lower zone (e.g. HBV)
        uz = 4 'upper zone (e.g. HBV)
        sm = 5 'soil moisture (e.g. HBV)
    End Enum

    Public Enum enmFouConfigVariable
        wl = 0
        uc = 0
    End Enum

    Public Enum enmHydroModule
        RR = 0
        FLOW = 1
        FLOW1D = 2
        FLOW2D = 3
        RTC = 4
    End Enum

    Public Enum enmValidationResult
        Successful = 0
        Unsuccessful = 1
        NotEvaluated = 2
        EvaluationFailed = 3
    End Enum

    Public Enum enmRRStructureType
        rectangular_weir = 9
    End Enum


    Public Enum enmValidationAction
        checkvalue = 0      'syntax and range check. Can involve checks like < x, >= y but also isnumber
        define = 1
        evaluate = 2
    End Enum
    Public Enum enmLGN6LanduseCode
        agrarisch_gras = 1
        mais = 2
        aardappelen = 3
        bieten = 4
        graan = 5
        overige_landbouwgewassen = 6
        glastuinbouw = 8
        boomgaarden = 9
        bollenteelt = 10
        loofbos = 11
        naaldbos = 12
        zoet_water = 16
        zout_water = 17
        bebouwing_in_primair_bebouwd_gebied = 18
        bebouwing_in_secundair_bebouwd_gebied = 19
        bos_in_primair_bebouwd_gebied = 20
        bos_in_secundair_bebouwd_gebied = 22
        gras_in_primair_bebouwd_gebied = 23
        kale_grond_in_bebouwd_buitengebied = 24
        hoofdwegen_en_spoorwegen = 25
        bebouwing_in_buitengebied = 26
        gras_in_secundair_bebouwd_gebied = 28
        kwelders = 30
        open_zand_in_kustgebied = 31
        duinen_met_lage_vegetatie = 32
        duinen_met_hoge_vegetatie = 33
        duinheide = 34
        open_stuifzand = 35
        heide = 36
        matig_vergraste_heide = 37
        sterk_vergraste_heide = 38
        hoogveen = 39
        bos_in_hoogveengebied = 40
        overige_moerasvegetatie = 41
        rietvegetatie = 42
        bos_in_moerasgebied = 43
        natuurgraslanden = 45
        boomkwekerijen = 61
        fruitkwekerijen = 62
    End Enum

    Public Enum enmHydraulicObjectCollections
        'this enum is used to distinguish the various tabs/object types in Channel Builder and Catchment Builder
        channels = 0
        rectangularweirs = 1
        outletpumps = 2
        inletpumps = 3
        mobilepumps = 4
        flushpumps = 5
        orifices = 6
        culverts = 7
        siphons = 8
        abutmentbridges = 9
        sluices = 10
        fixeddams = 11
        pillarbridges = 12
        trapezia = 13
        asymmetricaltrapezia = 14
        yzprofiles = 15
        tabulatedprofiles = 16
    End Enum

    Public Enum enmInternalVariable
        'these are all internal variables for that can be used inside custom variables and in validation rules
        'they area also used to categorize fields in datasources (e.g. shapefiles)
        None = 0                    'in case no variable defined
        ID = 1
        ParentID = 2 'e.g. catchment ID when ID represents subcatchment ID
        BedLevel = 11                'bed level as derived from the underlying channel
        MinCrest = 12                'minimum crest elevation
        MaxCrest = 13                'maximum crest elevation
        CrestWidth = 14              'crest width
        TotalWidth = 15              'total width
        ShoulderElevation = 16       'shoulder elevation (constructiehoogte)
        WinterCrestLevel = 17        'winter crest level 
        SummerCrestLevel = 18        'summer crest level
        WinterCrestCorrection = 19  'winter crest correction
        SummerCrestCorrection = 20  'summer crest correction
        DischargeCoefficient = 21   'discharge coefficient
        ContractionCoefficient = 22 'contraction coefficient
        NumberOfBarrels = 23        'number of barrels for a culvert or siphon
        Length = 24                 'length of a structure (e.g. culvert)
        InvertUp = 25               'Invert level upstream side (culvert and siphon)
        InvertDown = 26                 'Invert level downstream side (culvert and siphon)
        BarrelWidth = 27                'with of a culvert or orifice's barrel
        BarrelHeight = 28               'height of a culvert or orifice's barrel
        EntranceLossCoefficient = 29    'entrance loss coefficient of bridge, culvert or siphon
        ExitLossCoefficient = 30        'exit loss coefficient of bridge, culvert or siphon
        NumberOfPumps = 31              'number of pumps in a pumping station
        TotalCapacity = 32              'total pump capacity
        CapacityPump1 = 33              'capacity pump 1
        CapacityPump2 = 34              'capacity pump 2
        CapacityPump3 = 35              'capacity pump 3
        CapacityPump4 = 36              'capacity pump 4
        CapacityPump5 = 37              'capacity pump 5
        CapacityPump6 = 38              'capacity pump 6
        OnLevelPump1 = 39              'OnLevel pump 1
        OnLevelPump2 = 40              'OnLevel pump 2
        OnLevelPump3 = 41              'OnLevel pump 3
        OnLevelPump4 = 42              'OnLevel pump 4
        OnLevelPump5 = 43              'OnLevel pump 5
        OnLevelPump6 = 44              'OnLevel pump 6
        OffLevelPump1 = 45              'OffLevel pump 1
        OffLevelPump2 = 46              'OffLevel pump 2
        OffLevelPump3 = 47              'OffLevel pump 3
        OffLevelPump4 = 48              'OffLevel pump 4
        OffLevelPump5 = 49              'OffLevel pump 5
        OffLevelPump6 = 50              'OffLevel pump 6
        OnMarginPump1 = 51              'OnMargin pump 1
        OnMarginPump2 = 52              'OnMargin pump 2
        OnMarginPump3 = 53              'OnMargin pump 3
        OnMarginPump4 = 54              'OnMargin pump 4
        OnMarginPump5 = 55              'OnMargin pump 5
        OnMarginPump6 = 56              'OnMargin pump 6
        OffMarginPump1 = 57              'OffMargin pump 1
        OffMarginPump2 = 58              'OffMargin pump 2
        OffMarginPump3 = 59              'OffMargin pump 3
        OffMarginPump4 = 60              'OffMargin pump 4
        OffMarginPump5 = 61              'OffMargin pump 5
        OffMarginPump6 = 62              'OffMargin pump 6
        DefaultCapacityMMPD = 63        'default capacity for a pumping station
        DownstreamEmergencyStopLevel = 64   'the emergency stop level for waterlevels on downstream side (maalstop)
        HinterlandArea = 65             'the hinterland area in m2 for a given structure
        DefaultOnMarginPump1 = 66       'the default switch-on margin for pump 1
        DefaultOnMarginPump2 = 67
        DefaultOnMarginPump3 = 68
        DefaultOnMarginPump4 = 69
        DefaultOnMarginPump5 = 70
        DefaultOnMarginPump6 = 71
        DefaultOffMarginPump1 = 72
        DefaultOffMarginPump2 = 73
        DefaultOffMarginPump3 = 74
        DefaultOffMarginPump4 = 75
        DefaultOffMarginPump5 = 76
        DefaultOffMarginPump6 = 77
        LanduseCode = 78
        SoilType = 79
        ChannelArea = 80
        CrestLevel = 81
        LateralContractionCoefficient = 82
        StructureController = 83
        CalamityController = 84
        SewageAreaCategory = 85
        SewageAreaPOC = 86
        SewagePavedArea = 87
        SewageStorage = 88
        WWTP = 89
        StructureOutID = 90
        MeteoStationID = 91
        OpenwaterArea = 92
        FlushVolume = 93
        RRRunoffCoef = 94
        RRInflowCoef = 95
        RRAlpha1Coef = 96
        RRAlpha2Coef = 97
        RRAlpha3Coef = 98
        RRAlpha4Coef = 99
        RRDepth1 = 100
        RRDepth2 = 101
        RRDepth3 = 102
        OpenConnectionTo = 103
        ToAreaID = 104
        InundationLevel = 105
        FromAreaID = 106
        Name = 107
        SelectionCategory = 108
        StructureTypeCategory = 109
        FishPassageSelectionCategory = 110
        PumpReductionCategory = 111
        EmergencyCategory = 112
        DeckElevation = 113
        PillarWidth = 114
        ShapeFactor = 115
        StructureUsageCategory = 116
        NumberOfGates = 117
        MaximumGateHeight = 118
        ShapeCategory = 119
        CrestCorrectionSummer = 120
        CrestCorrectionWinter = 121
        CrestLevelSummer = 122
        CrestLevelWinter = 123
        FlowDirection = 124
        OnMarginDownstream = 125
        OffMarginDownstream = 126
        OnLevelDownstream = 127
        OffLevelDownstream = 128
        NumberOfSteps = 129
        InletCoefficient = 130
        OutletCoefficient = 132
        FishPassageWidth = 133
        HighestValue = 134
        LowestValue = 135
        FromNodeID = 136
        ToNodeID = 137
        ChannelUsageCategory = 138
        BedWidth = 139
        Slope = 140
        WaterSurfaceWidth = 141
        Waterlevel = 142
        Waterdepth = 143
        SurfaceWidth = 144
        SurfaceLevel = 145
        WetbermLeftWidth = 146
        WetbermRightWidth = 147
        WetbermLeftLowestElevation = 148
        WetbermRightLowestElevation = 149
        WetbermLeftHighestElevation = 150
        WetbermRightHighestElevation = 151
        WetbermLeftSideSlope = 152
        WetbermRightSideSlope = 153
        BedLevelUpstream = 154
        BedLevelDownstream = 155
        BedWidthUpstream = 156
        BedWidthDownstream = 157
        WaterlevelUpstream = 158
        WaterlevelDownstream = 159
        WaterSurfaceWidthUpstream = 160
        WaterSurfaceWidthDownstream = 161
        WaterDepthUpstream = 162
        WaterDepthDownstream = 163
        SlopeUpstream = 164
        SlopeDownstream = 165
        SurfaceLevelUpstream = 166
        SurfaceLevelDownstream = 167
        SurfaceWidthUpstream = 168
        SurfaceWidthDownstream = 169
        BoundaryCategory = 170
        BoundaryValue = 171
        ThresholdValue = 172
        RRMaxStorage = 173
        Material = 174
        FlowWidth = 175
        BottomElevation = 176
        BendLossCoefficient = 177
        ZPHighSideOutlet = 200              'summer target level on the high water side of this structure for outlet situations
        WPHighSideOutlet = 201              'winter target level on the high water side of this structure for outlet situations
        ZPLowSideOutlet = 202              'summer target level on the low water side of this structure for outlet situations
        WPLowSideOutlet = 203               'winter target level on the low water side of this structure for outlet situations
        ZPHighSideInlet = 204               'summer target level on the high water side of this structure for inlet situations
        WPHighSideInlet = 205               'winter target level on the high water side of this structure for inlet situations
        ZPLowSideInlet = 206                'summer target level on the low water side of this structure for inlet situations
        WPLowSideInlet = 207                'winter target level on the low water side of this structure for inlet situations
        ZPUpstreamOutlet = 208              'summer target level on the high water side of this structure for outlet situations
        WPUpstreamOutlet = 209              'winter target level on the high water side of this structure for outlet situations
        ZPDownstreamOutlet = 210              'summer target level on the low water side of this structure for outlet situations
        WPDownstreamOutlet = 211               'winter target level on the low water side of this structure for outlet situations
        ZPUpstreamInlet = 212               'summer target level on the high water side of this structure for inlet situations
        WPUpstreamInlet = 213               'winter target level on the high water side of this structure for inlet situations
        ZPDownstreamInlet = 214                'summer target level on the low water side of this structure for inlet situations
        WPDownstreamInlet = 215                'winter target level on the low water side of this structure for inlet situations
        WPOutlet = 216                      'for e.g. subcatchments there is no such thing as upstream or downstream. Just store the target levels
        WPInlet = 217                       'for e.g. subcatchments there is no such thing as upstream or downstream. Just store the target levels
        ZPOutlet = 218                      'for e.g. subcatchments there is no such thing as upstream or downstream. Just store the target levels
        ZPInlet = 219                       'for e.g. subcatchments there is no such thing as upstream or downstream. Just store the target levels
        NumberOfPoints = 220                'the number of points of which eg an YZ profile is made up of
        TotalLength = 221                   'the total yz profile length, achieved when adding up all individual segments
        StraightLength = 222                'the yz profile length in a straight line between first and last point
        LowestPoint = 223                   'the lowest point for a given cross section
        DefaultOnMarginDownstream = 224     'the default switch-on margin for downstream controlled (inlet) structures
        DefaultOffMarginDownstream = 225    'the default switch-off margin for downstream controlled (inlet) structures
        DefaultCapacityM3PS = 226           'default pump capacity, expressed in mm per day
        CapacityOrMultiplier = 227          'capacity or multiplier as used for mobile pumps
        LeftSlopeUpstream = 228
        RightSlopeUpstream = 229
        LeftSlopeDownstream = 230
        RightSlopeDownstream = 231
        Depth = 233
        LeftSlope = 234
        RightSlope = 235
        LeftWetBermLevelUpstream = 236
        LeftWetBermLevelDownstream = 237
        LeftWetBermWidthUpstream = 238
        LeftWetBermWidthDownstream = 239
        RightWetBermLevelUpstream = 240
        RightWetBermLevelDownstream = 241
        RightWetBermWidthUpstream = 242
        RightWetBermWidthDownstream = 243
        LeftWetBermlevel = 244
        LeftWetBermWidth = 245
        RightWetBermLevel = 246
        RightWetBermWidth = 247
        InletCapacity = 248
        DefaultInletCapFraction = 249 'the fraction of the total outlet capacity, used to calculate the inlet capacity
    End Enum

    Public Enum enmStructureByFunctionType
        Weir = 0
        Culvert = 1
        Orifice = 2
        Outletpump = 3
        Inletpump = 4
        FlushPump = 5
        Syphon = 6
        Sluice = 7
        AbutmentBridge = 8
        PillarBridge = 9
    End Enum

    Public Enum enmGeoDataSource
        Shapefile = 0
        GeoJSON = 1
        Grid = 2
        XLSX = 3
    End Enum

    Public Enum enmReachObjectType
        OutletPump = 0
        InletPump = 1
        FlushPump = 2
        MobilePump = 3
        Weir = 4
        Orifice = 5
        Sluice = 6
        Culvert = 7
        Siphon = 8
        AbutmentBridge = 9
        PillarBridge = 10
        UniWeir = 11
        TrapeziumProfile = 12
        AsymmetricalTrapezium = 13
        YZProfile = 14
        TabulatedProfile = 15
    End Enum
    Public Enum enmGeoDataType
        Point = 0
        Line = 1
        Polygon = 2
        Mixed = 3
        Grid = 4
    End Enum
    Public Enum enmGeoObjectType
        point = 0
        polyline = 1
        polygon = 2
        grid = 3
    End Enum

    Public Enum enm1DParameter
        unknown = 0
        waterlevel = 1
        depth = 2
        velocity = 3
        discharge = 4
        crest_level = 5
        gate_height = 6
        volume = 7
        internal_exchange = 8
    End Enum
    Public Enum enm2DParameter
        depth = 0
        waterlevel = 1
        velocity = 2
        u_velocity = 3
        v_velocity = 4
    End Enum

    Public Enum enmLanduseSource
        fysiek_voorkomen_2019 = 0 'by Nelen & Schuurmans for STOWA
    End Enum

    Public Enum enmMessageType
        ErrorMessage = 0
        Warning = 1
        Message = 2
        None = 3
    End Enum

    Public Enum enmAxisDateRange
        All = 0
        Observed = 1
        Simulated = 2
        Calibrationperiod = 3
    End Enum

    Public Enum enmQualityAssessmentParameter
        RMSE = 0
        NashSutcliffe = 1
    End Enum

    Public Enum enmAutokalMethod
        MonteCarlo = 0
        SensitivityAnalysis = 1
        GeneticAlgorithm = 2
    End Enum

    Public Enum enmResamplingMethod
        'lists the GDAL resampling methods
        nearest = 0
        bilinear = 1
        cubic = 2
        cubicspline = 3
        lanczos = 4
        average = 5
        mode = 6
    End Enum
    Public Enum enmLateralType
        'Integer '0 = constant, 1 = table, 6= rational,  7 = from rainfall station, 11 = from table lib
        ConstantValue = 0
        Tabulated = 1
        RationalMethod = 6
        RainfallStation = 7
        FromTableLibrary = 11
    End Enum

    Public Enum enmDatabaseType
        AccessMDB = 0
        SQLiteDB = 1
    End Enum

    Public Enum enmFileState
        Closed = 0
        Open = 1
    End Enum

    Public Enum enmExtrapolationMethod
        MakeZero = 0            'makes any value that exceeds the table that is being interpolated from equal to zero
        KeepConstant = 1        'continues the last value from the table that is being interpolated from
        ExtrapolateLinear = 2   'extrapolates a table 
    End Enum

    Public Enum enmReportComponentType
        TOC = 0
        Chapter = 1
        Paragraph = 2
        Image = 3
        Appendix = 4
    End Enum

    Public Enum enmReportFormat
        WORD = 0
        LATEX = 1
        HTML = 2
        REACT = 3   'my own format to load report content into my own react template
    End Enum

    Public Enum enmSTOWA2019ScenarioYear
        ANNO2019 = 0
        ANNO2050 = 1
        ANNO2085 = 2
    End Enum

    Public Enum enmSTOWA2019Scenario
        NONE = 0
        GL = 1
        GH = 2
        WL = 3
        WH = 4
    End Enum

    Public Enum enmSTOWA2019Season
        JAARROND = 0
        WINTER = 1
        GROEISEIZOEN = 2
    End Enum

    Public Enum enmGDALDataType
        ByteType = 0
        Int8 = 1
        Int16 = 2
        Int32 = 3
        Float32 = 4
    End Enum
    'Public Enum enmInternalVariable
    '    None = 0
    '    ID = 1
    '    ParentID = 2 'e.g. catchment ID when ID represents subcatchment ID
    '    LanduseCode = 3
    '    SoilType = 4
    '    ChannelArea = 5
    '    CrestLevel = 6
    '    CrestWidth = 7
    '    SummerOutOnLevel = 8
    '    SummerOutOffLevel = 9
    '    WinterOutOnLevel = 10
    '    WinterOutOffLevel = 11
    '    SummerInOnLevel = 12
    '    SummerInOffLevel = 13
    '    WinterInOnLevel = 14
    '    WinterInOffLevel = 15
    '    LateralContractionCoefficient = 16
    '    DischargeCoefficient = 17
    '    StructureController = 18
    '    CalamityController = 19
    '    SewageAreaCategory = 20
    '    SewageAreaPOC = 21
    '    SewagePavedArea = 22
    '    SewageStorage = 23
    '    WWTP = 24
    '    WinterOutTargetLevel = 25
    '    WinterInTargetLevel = 26
    '    SummerOutTargetLevel = 27
    '    SummerInTargetLevel = 28
    '    StructureOutID = 29
    '    MeteoStationID = 30
    '    OpenwaterArea = 31
    '    FlushVolume = 32
    '    RRRunoffCoef = 33
    '    RRInflowCoef = 34
    '    RRAlpha1Coef = 35
    '    RRAlpha2Coef = 36
    '    RRAlpha3Coef = 37
    '    RRAlpha4Coef = 38
    '    RRDepth1 = 39
    '    RRDepth2 = 40
    '    RRDepth3 = 41
    '    OpenConnectionTo = 42
    '    ToAreaID = 43
    '    InundationLevel = 44
    '    FromAreaID = 45
    '    Name = 46
    '    SelectionCategory = 47
    '    StructureTypeCategory = 48
    '    FishPassageSelectionCategory = 49
    '    PumpReductionCategory = 50
    '    EmergencyCategory = 51
    '    TotalCapacity = 58
    '    NumberOfPumps = 59
    '    HinterlandArea = 60
    '    DownstreamEmergencyStopLevel = 61
    '    WinterOutTargetLevelUp = 62
    '    WinterOutTargetLevelDown = 63
    '    SummerOutTargetLevelUp = 64
    '    SummerOutTargetLevelDown = 65
    '    WinterInTargetLevelUp = 66
    '    WinterInTargetLevelDown = 67
    '    SummerInTargetLevelUp = 68
    '    SummerInTargetLevelDown = 69
    '    NumberOfBarrels = 70
    '    Diameter = 71
    '    Height = 72
    '    Length = 73
    '    InvertUpstream = 74
    '    InvertDownstream = 75
    '    TargetLevelUpstream = 76
    '    TargetLevelDownstream = 77
    '    Width = 78
    '    DeckElevation = 79
    '    BedLevel = 80
    '    PillarWidth = 81
    '    ShapeFactor = 82
    '    StructureUsageCategory = 83
    '    NumberOfGates = 84
    '    MaximumGateHeight = 85
    '    ShapeCategory = 86
    '    MinimumCrestLevel = 87
    '    MaximumCrestLevel = 88
    '    TotalWidth = 89
    '    ShoulderElevation = 90
    '    CrestCorrectionSummer = 91
    '    CrestCorrectionWinter = 92
    '    CrestLevelSummer = 93
    '    CrestLevelWinter = 94
    '    FlowDirection = 95
    '    OnMarginDownstream = 98
    '    OffMarginDownstream = 99
    '    OnLevelDownstream = 102
    '    OffLevelDownstream = 103
    '    NumberOfSteps = 104
    '    InletCoefficient = 106
    '    OutletCoefficient = 107
    '    FishPassageWidth = 108
    '    HighestValue = 109
    '    LowestValue = 110
    '    FromNodeID = 111
    '    ToNodeID = 112
    '    ChannelUsageCategory = 113
    '    BedWidth = 114
    '    Slope = 115
    '    WaterSurfaceWidth = 116
    '    Waterlevel = 117
    '    Waterdepth = 118
    '    SurfaceWidth = 119
    '    SurfaceLevel = 120
    '    WetbermLeftWidth = 121
    '    WetbermRightWidth = 122
    '    WetbermLeftLowestElevation = 123
    '    WetbermRightLowestElevation = 124
    '    WetbermLeftHighestElevation = 125
    '    WetbermRightHighestElevation = 126
    '    WetbermLeftSideSlope = 127
    '    WetbermRightSideSlope = 128
    '    BedLevelUpstream = 129
    '    BedLevelDownstream = 130
    '    BedWidthUpstream = 131
    '    BedWidthDownstream = 132
    '    WaterlevelUpstream = 133
    '    WaterlevelDownstream = 134
    '    WaterSurfaceWidthUpstream = 135
    '    WaterSurfaceWidthDownstream = 136
    '    WaterDepthUpstream = 137
    '    WaterDepthDownstream = 138
    '    SlopeUpstream = 139
    '    SlopeDownstream = 140
    '    SurfaceLevelUpstream = 141
    '    SurfaceLevelDownstream = 142
    '    SurfaceWidthUpstream = 143
    '    SurfaceWidthDownstream = 144
    '    BoundaryCategory = 145
    '    BoundaryValue = 146
    '    ThresholdValue = 147
    '    RRMaxStorage = 148
    '    Material = 149
    '    CapacityPump1 = 150
    '    CapacityPump2 = 151
    '    CapacityPump3 = 152
    '    CapacityPump4 = 153
    '    CapacityPump5 = 154
    '    CapacityPump6 = 155
    '    OnLevelPump1 = 156
    '    OnLevelPump2 = 157
    '    OnLevelPump3 = 158
    '    OnLevelPump4 = 159
    '    OnLevelPump5 = 160
    '    OnLevelPump6 = 161
    '    OffLevelPump1 = 162
    '    OffLevelPump2 = 163
    '    OffLevelPump3 = 164
    '    OffLevelPump4 = 165
    '    OffLevelPump5 = 166
    '    OffLevelPump6 = 167
    '    OnMarginPump1 = 168
    '    OnMarginPump2 = 169
    '    OnMarginPump3 = 170
    '    OnMarginPump4 = 171
    '    OnMarginPump5 = 172
    '    OnMarginPump6 = 173
    '    OffMarginPump1 = 174
    '    OffMarginPump2 = 175
    '    OffMarginPump3 = 176
    '    OffMarginPump4 = 177
    '    OffMarginPump5 = 178
    '    OffMarginPump6 = 179
    'End Enum

    Public Sub SetGridValuesBelowThresholdNoData(myGrid As MapWinGIS.Grid, Threshold As Double)
        'we assume the grid is already open
        For r = 0 To myGrid.Header.NumberRows - 1
            For c = 0 To myGrid.Header.NumberCols - 1
                If myGrid.Value(c, r) <= Threshold Then myGrid.Value(c, r) = myGrid.Header.NodataValue
            Next
        Next
    End Sub

    Public Enum EnmStructureUsage
        PumpOutlet = 0  'pumping against gravity for outflow: controlling on low side, flow to high side
        PumpInlet = 1   'pumping agains gravity for inlet: controlling on high side, flow to high side
        GravityInlet = 2       'inlet with gravity: controlling on low side, flowing towards low side
        GravityOutlet = 3      'outlet using gravity: controlling on high side, flowing towards low side
    End Enum

    Public Enum enmHydrologicalSide
        Upstream = 1
        Downstream = 2
    End Enum

    Public Function OppositeHydrologicalSide(CurrentSide As enmHydrologicalSide)
        If CurrentSide = enmHydrologicalSide.Upstream Then
            Return enmHydrologicalSide.Downstream
        ElseIf CurrentSide = enmHydrologicalSide.Downstream Then
            Return enmHydrologicalSide.Upstream
        End If
        Return Nothing
    End Function

    Public Enum EnmProbabilityDistribution
        GumbelMax = 1
        GEV = 2
        GenPareto = 3
        Exponential = 4
        LogNormal = 5
    End Enum

    Public Enum EnmParameterOperation
        Add = 1             'adds the operand value to the current parameter value
        Multiply = 2        'multiplies the current parameter value with the operand value
        Replace = 3         'replaces the current parameter value by the operand value
        ValuesFromList = 4  'replaces the current parameter value by one of the values from a list
        WrapToken = 5       'wraps a token around a given value, in the xml styling: <mytoken>value</mytoken>
    End Enum

    Public Enum EnmLanduseTypeBuisdrainagekaart
        grasland = 1
        akkerbouw_tuinbouw = 2
        stedelijk = 3
        begraafplaats = 4
        boom_en_fruitkwekerij = 5
        boomgaard = 6
        golfterrein = 7
        glastuinbouw = 8
        sportterrein = 9
        vliegveld = 10
        ongedraineerd = 11
    End Enum

    Public Enum enmSoilTypeBuisdrainagekaart
        klei = 1
        zand = 2
        veen = 3
    End Enum

    Public Enum EnmSobekLanduseType
        grass = 1
        corn = 2
        potatoes = 3
        sugarbeet = 4
        grain = 5
        miscellaneous = 6
        non_arable_land = 7
        greenhouse = 8
        orchard = 9
        bulbous_plants = 10
        foliage_forest = 11
        pine_forest = 12
        nature = 13
        fallow = 14
        vegetables = 15
        flowers = 16

        'zelf toegevoegd:
        water = 17
        verhard = 18
        glastuinbouw = 19

        none = 999
    End Enum

    Public Enum EnmSobekSoilType
        sand_maximum = 1  'sand_maximum (µ=0.117 per m)'
        peat_maximum = 2  'peat_maximum (µ=0.078 per m)'
        clay_maximum = 3  'clay_maximum (µ=0.049 per m)'
        peat_average = 4  'peat_average (µ=0.067 per m)'
        sand_average = 5  'sand_average (µ=0.088 per m)'
        silt_maximum = 6  'silt_maximum (µ=0.051 per m)'
        peat_minimum = 7  'peat_minimum (µ=0.051 per m)'
        clay_average = 8  'clay_average (µ=0.036 per m)'
        sand_minimum = 9  'sand_minimum (µ=0.060 per m)'
        silt_average = 10  'silt_average (µ=0.038 per m)'
        clay_minimum = 11  'clay_minimum (µ=0.026 per m)'
        silt_minimum = 12  'silt_minimum (µ=0.021 per m)'
    End Enum

    Public Enum EnmCapsimSoilType
        Veengrond_met_veraarde_bovengrond = 101
        Veengrond_met_veraarde_bovengrond_zand = 102
        Veengrond_met_kleidek = 103
        Veengrond_met_kleidek_op_zand = 104
        Veengrond_met_zanddek_op_zand = 105
        Veengrond_op_ongerijpte_klei = 106
        Stuifzand = 107
        Podzol_Leemarm_fijn_zand = 108
        Podzol_zwak_lemig_fijn_zand = 109
        Podzol_zwak_lemig_fijn_zand_op_grof_zand = 110
        Podzol_lemig_keileem = 111
        Enkeerd_zwak_lemig_fijn_zand = 112
        Beekeerd_lemig_fijn_zand = 113
        Podzol_grof_zand = 114
        Zavel = 115
        Lichte_klei = 116
        Zware_klei = 117
        Klei_op_veen = 118
        Klei_op_zand = 119
        Klei_op_grof_zand = 120
        Leem = 121
        none = 999
    End Enum


    Public Enum EnmShapeValueType
        DbNull = 0
        EmptyString = 1
        Numeric = 2
    End Enum

    Public Enum enmGrondwatertrap
        I = 1
        II = 2
        IIB = 22
        III = 3
        IIIB = 33
        IV = 4
        V = 5
        VI = 6
        VII = 7
        VIII = 8
    End Enum

    Public Enum enmParameterStatus
        'an enum that keeps track of the reliability of the original parameter value (before replacing it)
        OK = 0
        DUBIOUS = 1
        CRITICAL = 2
    End Enum

    Public Enum enmAreaUnits
        'this enumerator was introduced in v1.890 for Catchment Builder to support sewage area
        M2 = 0  'meters squared
        HA = 1  'hectares
    End Enum
    Public Enum enmElevationUnits
        'this enumerator was introduced in v2.020 for Channel Builder to support an elevation grid
        M = 0 'above datum
        CM = 1 'above datum
    End Enum

    Public Enum enmPumpCapacityUnits
        'this enumerator was introduced in v1.71 for Channel Builder
        M3PS = 0    'cubic meters per second
        M3PM = 1    'cubic meters per minute
    End Enum

    Public Enum enmPumpOnOffMarginUnits
        CM = 0      'cm w.r.t. target level
        M = 1       'm w.r.t. target level
    End Enum

    Public Enum enmPeriodicity
        NONE = 0
        YEAR = 1
        MONTH = 2
        FOURWEEKS = 3
        WEEK = 4
        TIDALPERIODDAY = 5 '24:50
        DAY = 6
        TIDALPERIODHALFDAY = 7 '12:25
        HALFDAY = 8
        HOUR = 9
    End Enum

    Public Enum enmDataFieldType
        LOCATIONID = 0
        SERIESID = 1
        DATETIME = 2
        PARAMETER = 3
        VALUE = 4           'generic name for any field that contains values
        DISCHARGE = 5
        BASEFLOW = 6
        INTERFLOW = 7
        SURFACE_RUNOFF = 8
        WATERLEVEL = 9
        WATERDEPTH = 10
        VELOCITY = 11
        HEAD = 12
        PUMPNUMBER = 13
        PUMPFRACTION = 14
        SEASON = 15
        DURATION = 16
        PROBABILITY = 17
        PROBABILITYCORRECTED = 18
        CLIMATESCENARIO = 19
        USE = 20
        VOLUME = 21
    End Enum

    Public Enum enmSobekControllerType
        'controller type 0=time, 1 = hydraulic 2 = interval 3 = PID, 4 = relative time, 99 = follow target level (eigen bedenksel)
        TIME = 0
        HYDRAULIC = 1
        INTERVAL = 2
        PID = 3
        RELATIVE_TIME = 4
        TARGETLEVEL = 99
    End Enum

    Public Enum enmSetpointType
        'interval type 0=fixed, 1=variable
        FIXED = 0
        VARIABLE = 1
    End Enum

    Public Enum enmInterpolationType
        '0= block 1 = linear
        BLOCK = 0
        LINEAR = 1
    End Enum

    Public Enum enmObservedParameter
        'measured parametr: 0 = water level, 1 = discharge
        WATERLEVEL = 0
        DISCHARGE = 1
    End Enum

    Public Enum enmControlledParameter
        'controlled parameter 0 = crest level, 1 = crest width, 2 = gate height, 3 = pumpcap
        CRESTLEVEL = 0
        CRESTWIDTH = 1
        GATEHEIGHT = 2
        PUMPCAPACITY = 3
    End Enum

    Public Enum enmValidationSyntaxCondition
        ISNUMERIC = 0
    End Enum

    Public Enum enmValidationCategory
        CRITICAL = 0
        NON_CRITICAL = 1
        INFORMATIVE = 2
    End Enum

    Public Enum enmTimeSeriesFilter
        NONE = 0
        MAX = 1
        MIN = 2
        FIRST = 3
        LAST = 4
        SKIPFIRSTPERCENTAGE = 5
        ANNUALMAX = 6
        MONTHLYMAX = 7
        MONTHLYAVG = 8
        UPPER_PERCENTILE = 9
        LOWER_PERCENTILE = 10
        DAILYSUM = 11
        MAX_PER_EVENT = 12
    End Enum

    Public Enum enmTimeSeriesProcessing
        none = 0
        cumulative = 1
        monthlycumulative = 2
        annualcumulative = 3
        monthlysum = 4
        percentile = 5
    End Enum

    Public Enum enmCFBridgeType
        'bridge type 2=pillar, 3=abutment, 4=fixed bed, 5=soil bed
        PILLAR = 2
        ABUTMENT = 3
        FIXEDBED = 4
        SOILBED = 5
    End Enum
    Public Enum enmChartType
        Timeseries
        Histogram
        Scatterplot
        Piechart
    End Enum

    Public Enum enmSobekResultsObjectTypes
        Nodes
        ReachSegments
        LateralNodes
        Structures
        UnpavedNodes
        PavedNodes
        GreenhouseNodes
    End Enum

    Public Enum enmSobekNodeResultsTypes
        Waterlevel
        Waterdepth
    End Enum

    Public Enum enmSobekReachResultsTypes
        Discharge
        Velocity
    End Enum

    Public Enum enmSobekLateralResultsTypes
        LateralFlow
    End Enum

    Public Enum enmSobekStructureResultsTypes
        Discharge
        Velocity
        WaterlevelUp
        WaterlevelDown
        CrestLevel
        GateHeight
        Head
    End Enum

    Public Enum enmSobekUnpavedResultsTypes
        Level 'In fact it's groundw.level, but the . is not accepted in an enum. Also there is a parameter called groundw.outflow
        Outflow
        Precipitation
        ActualEvaporation
        PotentialEvaporation
    End Enum

    Public Enum enmSobekPavedResultsTypes
        Spill
    End Enum

    Public Enum enmSobekGreenhouseResultsTypes
        Outflow
    End Enum

    Public Enum enmSobekResultsType
        CalcpntH
        CalcpntD
        ReachsegQ
        ReachsegV
        StrucQ
        StrucH1
        StrucH2
        StrucCrest
        StrucGateHeight
        UPGroundwaterLevel
    End Enum

    Public Enum enmOleDBDataType
        'https://docs.microsoft.com/en-us/dotnet/api/system.data.oledb.oledbtype?view=dotnet-plat-ext-3.1#:~:text=A%20special%20data%20type%20that,This%20maps%20to%20Object.
        SQLVARCHAR = 129
        SQLVARCHAR255 = 130
        SQLDATE = 7
        SQLDOUBLE = 5
        SQLBIT = 128
        SQLFLOAT = 6
        SQLINT = 3
    End Enum

    Public Enum enmSQLiteDataType
        SQLITETEXT  'note: dates can be implemented as text in YYYY-MM-DD HH:MM:SS.SSSS format
        SQLITEINT
        SQLITEREAL
        SQLITENULL
        SQLITEBLOB
    End Enum

    Public Enum enmStatisticalFitSection
        'this enum keeps track of which part of a dataset was used when fitting: the entire set, only the high values above threshold (upper) or only the low values below threshold (lower)
        ALL = 0                 'complete fit
        UPPERCONF = 1           'upper confidence value
        LOWERCONF = 2           'lower confidence value
        LEFTSECTION = 3         'section left of threshold
        RIGHTSECTION = 4        'section rigth of threshold
    End Enum

    Public Enum enmChannelUsage
        CHANNEL = 0           'reguliere watergang
        LINESTRUCTURE = 1     'kunstwerkvak
        FISHPASSAGE = 2       'bekkenvispassage
    End Enum

    Public Enum enmInitialType
        DEPTH = 0
        WATERLEVEL = 1
    End Enum

    Public Enum enmPlottingPositionsMethod
        Weibull = 0
        Gringorton = 1
        BosLevenbach = 2
    End Enum

    Public Enum enmTargetLevelType
        WinterOut = 0
        WinterIn = 1
        SummerOut = 2
        SummerIn = 3
    End Enum

    Public Enum enmResultsType
        Timestep = 0
        Average = 1
        Minimum = 2
        Maximum = 3
        Percentile = 4
    End Enum

    Public Enum enmRasterType
        ASC = 0
        TIF = 1
        IMG = 2
    End Enum

    Public Enum enmLateralUsage
        PRECIPITATION = 0
        EVAPORATION = 1
        SEEPAGE = 2
        DRAINAGE = 3
        EMISSIONS = 4
    End Enum

    Public Enum enmSubcatchmentSurfaceAreaType
        GIS_TOTALAREA = 0
        LANDUSE_OPENWATERAREA = 1               'total openwater area according to landuse table
        LANDUSE_PAVEDUNTREATEDAREA = 2          'total paved untreated area
        LANDUSE_PAVEDTREATEDAREA = 3            'total paved area
        LANDUSE_UNPAVEDAREA = 4                 'total unpaved area
        LANDUSE_GREENHOUSEAREA = 5              'total greenhouse area
        LANDUSE_UNASSIGNEDAREA = 6              'total unassigned area
        LANDUSE_UNASSIGNEDAREA_NOCHANNELS = 7   'total unassigned area landuse except areas designated as 1D channel areas
    End Enum
    Public Enum enmSobekSewageType
        MIXED = 0                   'gemengd stelsel
        SEPARATED = 1               'gescheiden stelsel
        IMPROVED_SEPARATED = 2      'verbeterd gescheiden stelsel
    End Enum
    Public Enum enmBoundaryType
        H = 0
        Q = 1
        HT = 2
        QT = 3
    End Enum
    Public Enum enmBarrelShape
        CIRCULAR = 0
        RECTANGULAR = 1
    End Enum

    Public Enum enmBridgeShape
        RECTANGULAR = 0
        TABULATED = 1
    End Enum

    Public Enum enmCulvertController
        NONE = 0
        INLET = 1
        OUTLET = 2
        CALAMITY = 3
        SUMMEROPEN = 4
        WINTEROPEN = 5
    End Enum

    Public Enum enmOrificeUsage
        OPEN = 0
        CLOSED = 1
        INLET = 2
        OUTLET = 3
        CALAMITY_OUTLET = 4
    End Enum

    Public Enum enmSluiceUsage
        OPEN = 0
        CLOSED = 1
        INLET = 2
        OUTLET = 3
        CALAMITY_OUTLET = 4
    End Enum

    Public Enum enmFishPassType
        SILLS = 0
        GATES = 1
    End Enum

    Public Enum enmBridgeType
        PILLAR = 2
        ABUTMENT = 3
        FIXEDBED = 4
        SOILBED = 5
    End Enum

    Public Enum enmSiphonType
        SIPHON = 0
        INVERTED_SIPHON = 1
    End Enum

    Public Enum enmFlowDirection
        BOTH = 0
        POSITIVE = 1
        NEGATIVE = 2
        NONE = 3
    End Enum

    Public Enum enmPumpDirection
        PositiveUpstreamControl = 1
        PositiveDownstreamControl = 2
        PositiveBoth = 3
        NegativeUpstreamControl = -1
        NegativeDownstreamControl = -2
        NegativeBoth = -3
    End Enum

    Public Enum enmWeirEventsControllerCategory
        NONE = 0
        CALAMITY_RAISE = 1
        CALAMITY_DROP = 2
        FLUSH_CONSERVATION = 3
        INLET = 4
    End Enum

    Public Enum enmControllerCategory
        NONE = 0
        TIME = 1
        HYDRAULIC = 2
        INTERVAL = 3
        PID = 4
    End Enum

    Public Enum enmModelResultsType
        WaterLevel = 0
        Discharge = 1
        Volume = 2
        Depth = 3
        Velocity = 4
    End Enum

    Public Enum enmModelResultsAspect
        Maximum = 0
        Minimum = 1
        Sum = 2
    End Enum

    Public Enum enmLeftCensoringType
        None = 0
        Highest = 1
        TargetLevel = 2
        Auto = 3
    End Enum


    Public Enum enmStatisticalCensoring
        None = 0
        HighestNEvents = 1
        TargetLevel = 2
        AutoDetect = 3
    End Enum


    Public Enum enmPumpType
        outlet = 0
        inlet = 1
        flush = 2
        emergency = 3
    End Enum

    Public Enum enmControllerType
        TIME = 0
        HYDRAULIC = 1
        INTERVAL = 2
        PID = 3
        LEVEL = 99
    End Enum

    Public Enum enmResampleMethod
        CellCenter = 0
        Bilinear = 1
    End Enum

    Public Enum enmTidalComponent
        VerhoogdLaagwater = 0
        VerhoogdeMiddenstand = 1
        VerhoogdHoogwater = 2
    End Enum

    Public Enum enmDataSource
        NONE = 0
        METEOBASE = 1
        HIRLAM = 2
        WALRUS = 3
        SOBEKRR = 4
        SOBEKCF = 5
        OBSERVED = 6
        SIMULATED = 7
    End Enum

    Public Enum enmDataType
        DATUM = 0             'if the date needs to be written in a textfile
        Q_OBSERVED = 1
        Q_SIMULATED = 2
        Q_BASEFLOW = 3
        Q_INTERFLOW = 4
        Q_INLET = 5
        MAKKINK = 6
        PENMAN = 7
        SEEPAGE = 8
        EFFLUENT = 9
        CSO = 10
        ZEROES = 11              'if a textfile column needs to be written, but the content is in fact irrelevant
        PRECIPITATION = 12
    End Enum

    Public Enum enmTimeStepSize
        SECOND = 0
        MINUTE = 1
        HOUR = 2
        DAY = 3
        MONTH = 4
        YEAR = 5
    End Enum

    Public Enum enmDataUnit
        NONE = 0
        MMperDAY = 1
        MMperHOUR = 2
        M3perMIN = 3
        M3perSEC = 4
    End Enum

    Public Enum enmScriptType
        IMPORTHIRLAM = 0
        WRITETIMESERIESTXT = 1
        IMPORTTIMESERIESTXT = 2
    End Enum

    Public Enum enmDSSDates
        STARTDATE = 0
        NOWDATE = 1
        ENDDATE = 2
    End Enum

    Public Enum enmSimulationModel
        SOBEK = 0
        Custom = 1
        WALRUS = 2
        LINRES = 3
        WAGENINGENMODEL = 4
        DHYDRO = 5
        DIMR = 6
        DHYDROSERVER = 7
        SUMAQUA = 8
        HBV = 9
    End Enum

    Public Enum enmKlimaatScenario
        STOWA2014_HUIDIG = 0 'KNMI '14 scenario HUIDIG (zichtjaar 2014)
        STOWA2014_2030 = 1 'KNMI '14 scenario 2030
        STOWA2014_2050GL = 2 'KNMI '14 scenario 2050 GL
        STOWA2014_2050GH = 3 'KNMI '14 scenario 2050 GH
        STOWA2014_2050WL = 4 'KNMI '14 scenario 2050 WL
        STOWA2014_2050WH = 5 'KNMI '14 scenario 2050 WH
        STOWA2014_2085GL = 6 'KNMI '14 scenario 2085 GL
        STOWA2014_2085GH = 7 'KNMI '14 scenario 2085 GH
        STOWA2014_2085WL = 8 'KNMI '14 scenario 2085 WL
        STOWA2014_2085WH = 9 'KNMI '14 scenario 2085 WH
        STOWA2019_HUIDIG = 10 'STOWA '19 scenario Huidig (zichtjaar 2014)
        STOWA2019_2030_LOWER = 11 'STOWA '19 scenario 2030 lower
        STOWA2019_2030_CENTER = 12 'STOWA '19 scenario 2030 center
        STOWA2019_2030_UPPER = 13 'STOWA '19 scenario 2030 upper
        STOWA2019_2050GL_CENTER = 14 'STOWA '19 scenario 2050GL center
        STOWA2019_2050WL_CENTER = 15 'STOWA '19 scenario 2050WL center
        STOWA2019_2050GH_CENTER = 16 'STOWA '19 scenario 2050GH center
        STOWA2019_2050WH_CENTER = 17 'STOWA '19 scenario 2050WH center
        STOWA2024_HUIDIG = 18 'STOWA '24 scenario HUIDIG (zichtjaar 2024)
    End Enum

    Public Enum enmNeerslagPatroon
        ONGECLASSIFICEERD = 0
        HOOG = 1
        MIDDELHOOG = 2
        MIDDELLAAG = 3
        LAAG = 4
        KORT = 5
        LANG = 6
        UNIFORM = 7
    End Enum

    Public Enum enmMeteoStationType
        precipitation = 0
        evaporation = 1
    End Enum

    Public Enum enmHalfYear
        winter = 0
        summer = 1
    End Enum

    Public Enum enmSeason
        yearround = 0
        meteowinterhalfyear = 1 'meteorological winterhalfyear: 1-9 to 1-3
        meteosummerhalfyear = 2 'meteorological summerhalfyear: 1-3 to 1-9
        meteosummerquarter = 3  'metorological summer: 1-6 to 1-9
        meteoautumnquarter = 4  'meteorological autumn: 1-9 to 1-12
        meteowinterquarter = 5  'meteorological winter: 1-12 to 1-3
        meteospringquarter = 6  'meteorological spring: 1-3 to 1-6
        hydrosummerhalfyear = 7 'hydrological summer: 15-4 to 15-10
        hydrowinterhalfyear = 8 'hydrological winter: 15-10 to 15-4
        marchthroughoctober = 9 'march through october
        novemberthroughfebruary = 10 'november through february
        aprilthroughaugust = 11 'april through august
        septemberthroughmarch = 12 'september through march
        growthseason = 13 'march through october
        outsidegrowthseason = 14 'november through february
    End Enum

    Public Enum enmValueStatistic
        MEAN = 0
        MIN = 1
        MAX = 2
        FIRST = 3
        LAST = 4
    End Enum

    Public Enum enmGridFormat
        AAIGrid = 0 'ASCI grid
        GRIB2 = 1   'GRIB2 as used in HIRLAM
        NetCDF = 2  'NetCDF rasters
        HFA = 3     'ERDAS IMG rasters
        GTiff = 4   'GeoTiff
    End Enum

    Public Enum enmApplicationArea
        GIS = 0
        GISminSBK = 1
        GISminSBKPaved = 2
        GISminSBkCSOPaved = 3
    End Enum

    Public Enum enmKNMI14Scenario
        GL2050 = 0
        GH2050 = 1
        WL2050 = 2
        WH2050 = 3
        GL2085 = 4
        GH2085 = 5
        WL2085 = 6
        WH2085 = 7
    End Enum

    Public Enum enmRainfallRunoffModel
        SOBEKRR = 0
        SACRAMENTO = 1
        HBV = 2
        WAGENINGENMODEL = 3
        LATERAL = 4
        NONE = 99
    End Enum

    Public Enum enmRainfallRunoffConnection
        RRCFCONNECTION = 0  'the 1D Flow-component
        RROPENWATER = 1     'RR openwater node
    End Enum

    Public Enum enmSobekStructureType
        RiverWeir = 0
        AdvancedWeir = 1
        GeneralStructure = 2
        RiverPump = 3
        DatabaseStructure = 4
        Weir = 6
        Orifice = 7
        Pump = 9
        Culvert = 10
        UniversalWeir = 11
        Bridge = 12
        BreachGrowth1DDamBreakNode = 13
        BreachGrowth2DDamBreakNode = 112
    End Enum

    Public Enum enmReachtype
        ReachCFChannel = 1
        ReachCFChannelWithLateral = 2
        ReachOFDamBreak = 3
        ReachSFPipe = 4
        ReachSFPipeWithRunoff = 5
        ReachSFDWAPipeWithRunoff = 6
        ReachSFRWAPipeWithRunoff = 7
        ReachSFPipeAndComb = 8
        ReachSFPipeAndMeas = 9
        ReachSFInternalWeir = 10
        ReachSFInternalOrifice = 11
        ReachSFInternalCulvert = 12
        ReachSFInternalPump = 13
        ReachSFStreet = 14
        ReachOFLineBoundary = 15
        ReachOFLine1D2DBoundary = 16
    End Enum

    Public Enum enmNodetype
        NodeSFManhole = 1
        NodeSFManholeWithMeasurement = 2
        NodeSFManholeWithRunoff = 3
        NodeSFManholeWithLateralFlow = 4
        NodeSFManholeWithDischargeAndRunoff = 5
        NodeSFExternalPump = 11
        NodeCFConnectionNode = 12
        ConnNodeLatStor = 13
        NodeCFBoundary = 14
        NodeCFLinkage = 15
        NodeCFGridpoint = 16
        NodeCFGridpointFixed = 17
        MeasurementStation = 18
        NodeCFLateral = 19
        SBK_PROFILE = 20
        NodeCFWeir = 21
        NodeCFUniWeir = 22
        NodeCFOrifice = 23
        NodeCFCulvert = 24
        NodeCFBridge = 26
        NodeCFPump = 27
        NodeCFExtraResistance = 65
        NodeRRCFConnection = 34
        NodeRRCFConnectionNode = 35
        NodeRRPaved = 43
        NodeRRUnpaved = 44
        NodeRRGreenhouse = 45
        NodeRROpenWater = 46
        NodeRRBoundary = 47
        NodeRRPump = 48
        NodeRRIndustry = 49
        NodeRRSacramento = 54
        NodeRRWWTP = 56
        NodeRRWeir = 49
        NodeRROrifice = 50
        NodeRRFriction = 51
        NodeRRWageningenModel = 69

        'virtual node types for internal use
        Virtual = 777

    End Enum

    Public Enum enmNodeSubType
        'describes subtypes of sobek objects, if applicable
        None = 0
        Siphon = 1
        Inlet = 2
        HBoundary = 3
        QBoundary = 4
    End Enum

    Public Enum enmProfileType
        tabulated = 0
        trapezium = 1
        opencircle = 2
        sedredge = 3
        closedcircle = 4
        eggshape = 6
        eggshape2 = 7
        closedrectangular = 8
        yztable = 10
        asymmetricaltrapezium = 11
    End Enum

    Public Enum enmComparisonOperator
        eq = 0      'equal
        gt = 1      'greater than
        ge = 2      'greater or equal
        lt = 3      'less than
        le = 4      'less than or equal
        ne = 5      'not equal
    End Enum


    Public Enum enmHydroMathOperation
        SUM = 0
        AVG = 1
        MIN = 2
        MAX = 3         'maximum value of two values (par1, par2)
        DIF = 4         'difference between two values (par1 - par2)
        ABSDIF = 5      'absolute value of difference between par1 and par2
        DTM = 6         'retrieve elevation data from a DTM. Par1 = search radius, par2 = percentile
        IFF = 7         'IF is not supported, so make it IFF
    End Enum

    Public Enum enmFrictionType
        CHEZY = 0
        MANNING = 1
        STRICKLERKN = 2
        STRICKLERKS = 3
        WHITECOLEBROOK = 4
        BOSBIJKERK = 7
        GLOBALFRICTION = 99
    End Enum

    Public Enum enmEquationComponent
        Operators = 0
        Variables = 1
        GISOperations = 2
        Math = 3
        Logical = 4
    End Enum

    Public Enum enmConveyanceMethod
        SEGMENTED = 0
        OPENLUMPED = 1
        'let op: we nemen closed lumped niet mee omdat dit eisen stelt aan de profieldefinitie
    End Enum

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        unitConversion = New clsUnitConversion
    End Sub

    '------------------------------------------------------------------------------------------------------------------------------------------
    ' UNDER HERE FOLLOWS THE Levenshtein string matching algorithm
    ' found on https://stackoverflow.com/questions/31315231/how-to-compare-strings-for-percentage-match-using-vb-net
    '------------------------------------------------------------------------------------------------------------------------------------------
    Public Function GetSimilarity(string1 As String, string2 As String) As Single
        Dim dis As Single = ComputeDistance(string1, string2)
        Dim maxLen As Single = string1.Length
        If maxLen < string2.Length Then
            maxLen = string2.Length
        End If
        If maxLen = 0.0F Then
            Return 1.0F
        Else
            Return 1.0F - dis / maxLen
        End If
    End Function

    Public Function ConvertCapToM3PS(myCap As Double, CapUnit As enmPumpCapacityUnits) As Double
        Select Case CapUnit
            Case enmPumpCapacityUnits.M3PM
                'convert from m3/min to m3/s
                Return myCap / 60
            Case enmPumpCapacityUnits.M3PS
                'no conversion needed
                Return myCap
        End Select
    End Function

    Public Function DateFromValues(myYear As Integer, myMonth As Integer, myDay As Integer, myHour As Integer, myMinute As Integer, mySecond As Integer) As DateTime

        If mySecond = 60 Then
            mySecond = 0
            myMinute += 1
        End If

        If myMinute = 60 Then
            myMinute = 0
            myHour += 1
        End If

        If myHour = 24 Then
            myHour = 0
            myDay += 1
        End If

        If myDay > DaysInMonth(myYear, myMonth) Then
            myDay = 1
            myMonth += 1
        End If

        If myMonth > 12 Then
            myMonth = 1
            myYear += 1
        End If

        Return New DateTime(myYear, myMonth, myDay, myHour, myMinute, mySecond)

    End Function

    Public Function TriangleNormal(Vertices As List(Of clsXYZ)) As clsXYZ

        'this function calculates the normal for a triangle made up of three vertices xyz
        'the normal of a triangle is calculated as the vector cross product of two edges of that triangle
        'remember the left hand rule: if two fingers represent two sides of the triangle, the thumb points in the direction of the normal
        Dim VectorU As New clsXYZ(Vertices(1).X - Vertices(0).X, Vertices(1).Y - Vertices(0).Y, Vertices(1).Z - Vertices(0).Z)
        Dim VectorV As New clsXYZ(Vertices(2).X - Vertices(0).X, Vertices(2).Y - Vertices(0).Y, Vertices(2).Z - Vertices(0).Z)

        'pseudocode from: https://www.khronos.org/opengl/wiki/Calculating_a_Surface_Normal
        'Set Normal.x to (multiply U.y by V.z) minus (multiply U.z by V.y)
        'Set Normal.y to (multiply U.z by V.x) minus (multiply U.x by V.z)
        'Set Normal.z to (multiply U.x by V.y) minus (multiply U.y by V.x)

        Dim Normal As New clsXYZ With {
            .X = (VectorU.Y * VectorV.Z) - (VectorU.Z - VectorV.Y),
            .Y = (VectorU.Z * VectorV.X) - (VectorU.X - VectorV.Z),
            .Z = (VectorU.X * VectorV.Y) - (VectorU.Y - VectorV.X)
        }

        Return Normal

    End Function


    Public Function getMonthAverageTemperatureDeBilt(curDate As Date) As Double
        Select Case Month(curDate)
            Case 1
                Return 3.6
            Case 2
                Return 3.9
            Case 3
                Return 6.5
            Case 4
                Return 9.9
            Case 5
                Return 13.4
            Case 6
                Return 16.1
            Case 7
                Return 18.2
            Case 8
                Return 17.8
            Case 9
                Return 14.7
            Case 10
                Return 10.9
            Case 11
                Return 7.0
            Case 12
                Return 4.2
        End Select
    End Function


    Public Function Peilscheidend(TargetlevelsHigh As clsTargetLevels, TargetLevelsLow As clsTargetLevels) As Boolean
        'this function decides whether two areas have separated target levels. For this we compare the outlet targetlevels
        If TargetlevelsHigh.OutletHasValue AndAlso TargetLevelsLow.OutletHasValue Then
            If TargetlevelsHigh.getHighestOutletLevel <> TargetLevelsLow.getHighestOutletLevel Then
                Return True
            ElseIf TargetlevelsHigh.getLowestOutletLevel <> TargetLevelsLow.getLowestOutletLevel Then
                Return True
            Else
                Return False
            End If
        End If
        Return False
    End Function
    Public Function DaysInMonth(myYear As Integer, myMonth As Integer) As Integer
        Select Case myMonth
            Case Is = 1, 3, 5, 7, 8, 10, 12
                Return 31
            Case Is = 4, 6, 9, 11
                Return 30
            Case Is = 2
                If myYear Mod 4 = 0 Then
                    Return 29
                Else
                    Return 28
                End If
        End Select
    End Function

    Public Sub SQLiteCreateIndex(ByRef con As SQLite.SQLiteConnection, TableName As String, IndexName As String, FieldsCommaSeparated As String, Unique As Boolean)
        'Creates a SQLite table Index
        Dim query, uni As String
        uni = ""
        If Unique = True Then uni = "UNIQUE"
        query = "CREATE " & uni & " INDEX IF NOT EXISTS " & IndexName & " ON [" & TableName & "] (" & FieldsCommaSeparated & ")"
        Me.setup.Log.AddMessage("Database index created: " & query)
        SQLiteNoQuery(con, query)
    End Sub
    Public Sub SQLiteDropIndex(ByRef con As SQLite.SQLiteConnection, IndexName As String)
        'Deletes as SQLite table Index
        Dim query As String
        query = "DROP INDEX IF EXISTS " & IndexName
        Me.setup.Log.AddMessage("Database index dropped: " & query)
        SQLiteNoQuery(con, query)
    End Sub

    Public Function SqliteRenameTable(ByRef con As SQLite.SQLiteConnection, OldName As String, NewName As String) As Boolean
        Try
            Dim query As String = "ALTER TABLE " & OldName & " Rename TO " & NewName & ";"
            SQLiteNoQuery(con, query)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error renaming database table " & OldName & " to " & NewName)
            Return False
        End Try
    End Function

    Public Function UpgradeClimateScenarioInTables(TableName As String, ColumnName As String) As Boolean
        Try
            'rename all old climate scenario's with the new names
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "HUIDIG", enmKlimaatScenario.STOWA2014_HUIDIG.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "KL2030", enmKlimaatScenario.STOWA2014_2030.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "GL2050", enmKlimaatScenario.STOWA2014_2050GL.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "GH2050", enmKlimaatScenario.STOWA2014_2050GH.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "WL2050", enmKlimaatScenario.STOWA2014_2050WL.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "WH2050", enmKlimaatScenario.STOWA2014_2050WH.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "GL2085", enmKlimaatScenario.STOWA2014_2085GL.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "GH2085", enmKlimaatScenario.STOWA2014_2085GH.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "WL2085", enmKlimaatScenario.STOWA2014_2085WL.ToString)
            Me.setup.GeneralFunctions.SqliteRenameTextValue(Me.setup.SqliteCon, TableName, ColumnName, "WH2085", enmKlimaatScenario.STOWA2014_2085WH.ToString)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function UpgradeClimateScenarioInTables of class GeneralFunctions: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function SqliteRenameTextValue(ByRef con As SQLite.SQLiteConnection, TableName As String, ColumnName As String, OldValue As String, NewValue As String) As Boolean
        Try
            Dim query As String = "UPDATE " & TableName & " SET " & ColumnName & "='" & NewValue & "' WHERE " & ColumnName & "='" & OldValue & "';"
            SQLiteNoQuery(con, query, False)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error updating changing value " & OldValue & " to " & NewValue & " in table " & TableName)
            Return False
        End Try
    End Function


    Public Sub DatagridviewColumnsFill(ByRef myGrid As Windows.Forms.DataGridView, Optional ByVal ColumnsList As List(Of Integer) = Nothing)
        For i = 0 To myGrid.Columns.GetColumnCount(Forms.DataGridViewElementStates.Visible) - 1
            If ColumnsList Is Nothing OrElse ColumnsList.Contains(i) Then
                myGrid.Columns.Item(i).AutoSizeMode = Forms.DataGridViewAutoSizeColumnMode.Fill
            End If
        Next
    End Sub

    Public Function DataTableToJSON(ByRef dt As DataTable, ColumnTitles As List(Of String)) As String
        Try
            Return JsonConvert.SerializeObject(dt)
        Catch ex As Exception
            Debug.Print("Error in DataTableToJSON: " & ex.Message)
            Return String.Empty
        End Try

    End Function

    Public Function SqliteDataTypeFromOleDB(OleDbDataType As OleDb.OleDbType) As enmSQLiteDataType
        Select Case OleDbDataType
            Case OleDbType.BigInt
                Return enmSQLiteDataType.SQLITEINT
            Case OleDbType.Binary
                Return enmSQLiteDataType.SQLITEBLOB
            Case OleDbType.Boolean
                Return enmSQLiteDataType.SQLITEINT  '0 = false, 1 = true
            Case OleDbType.BSTR
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Char
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Currency
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Date
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.DBDate
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.DBTime
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.DBTimeStamp
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Decimal
                Return enmSQLiteDataType.SQLITEREAL
            Case OleDbType.Double
                Return enmSQLiteDataType.SQLITEREAL
            Case OleDbType.Empty
                Return enmSQLiteDataType.SQLITENULL
            Case OleDbType.Error
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Filetime
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Guid
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.IDispatch
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Integer
                Return enmSQLiteDataType.SQLITEINT
            Case OleDbType.IUnknown
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.LongVarBinary
                Return enmSQLiteDataType.SQLITEBLOB
            Case OleDbType.LongVarChar
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.LongVarWChar
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Numeric
                Return enmSQLiteDataType.SQLITEREAL
            Case OleDbType.PropVariant
                Return enmSQLiteDataType.SQLITEBLOB
            Case OleDbType.Single
                Return enmSQLiteDataType.SQLITEREAL
            Case OleDbType.SmallInt
                Return enmSQLiteDataType.SQLITEINT
            Case OleDbType.TinyInt
                Return enmSQLiteDataType.SQLITEINT
            Case OleDbType.UnsignedBigInt
                Return enmSQLiteDataType.SQLITEINT
            Case OleDbType.UnsignedInt
                Return enmSQLiteDataType.SQLITEINT
            Case OleDbType.VarBinary
                Return enmSQLiteDataType.SQLITEBLOB
            Case OleDbType.VarChar
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.Variant
                Return enmSQLiteDataType.SQLITEBLOB
            Case OleDbType.VarNumeric
                Return enmSQLiteDataType.SQLITEREAL
            Case OleDbType.VarWChar
                Return enmSQLiteDataType.SQLITETEXT
            Case OleDbType.WChar
                Return enmSQLiteDataType.SQLITETEXT
            Case Else
                Return enmSQLiteDataType.SQLITENULL
        End Select
    End Function
    Public Function DoubleFromShapefile(ByRef sf As MapWinGIS.Shapefile, FieldIdx As Long, RecordIdx As Long) As Double
        If IsNothing(sf.CellValue(FieldIdx, RecordIdx)) Then
            Return Double.NaN
        ElseIf Not IsNumeric(sf.CellValue(FieldIdx, RecordIdx)) Then
            Return Double.NaN
        Else
            Return sf.CellValue(FieldIdx, RecordIdx)
        End If
    End Function
    Public Function ShiftObjectPosition(NodeIdx As Integer, increment As Double, ByRef dx As Double, ByRef dy As Double) As Boolean
        Try
            Dim angle As Double
            Dim NodeNum As Integer = NodeIdx + 1

            'decide in which ring this node belongs
            Dim Fits As Boolean = False
            Dim nFitsInner As Integer = 0
            Dim nFitsTotal As Integer = 0
            Dim nSpots As Integer = 0
            Dim r As Integer = 0
            While Not Fits
                r += 1                  'increase the radius
                nSpots = r * 2 * 4     'e.g. in ring 1 we can fit 8 nodes, in ring 2 we can fit 16 nodes, in 3 24 etc.
                nFitsTotal += nSpots
                If NodeNum <= nFitsTotal Then
                    Fits = True
                Else
                    nFitsInner += nSpots
                End If
            End While

            'now that we know in which ring our node belongs we can determine the angle and thus its position
            NodeNum -= nFitsInner
            angle = (NodeNum - 1) * (360 / nSpots) 'angle in degrees

            'it's always that either abs(dx) = radius or abs(dy) = radius. This depends on the quadrant we're in
            Select Case angle
                Case Is <= 45
                    dy = r * increment
                    dx = Math.Tan(D2R(angle)) * dy
                'Case Is <= 90
                '    dx = r * increment
                '    dy = dx / Math.Tan(D2R(angle))
                Case Is <= 135
                    dx = r * increment
                    dy = dx / Math.Tan(D2R(angle))
                'Case Is <= 180
                '    dy = -r * increment
                '    dx = Math.Tan(D2R(angle)) * dy
                Case Is <= 225
                    dy = -r * increment
                    dx = Math.Tan(D2R(angle)) * dy
                'Case Is <= 270
                '    dx = -r * increment
                '    dy = dx / Math.Tan(D2R(angle))
                Case Is <= 315
                    dx = -r * increment
                    dy = dx / Math.Tan(D2R(angle))
                Case Else
                    dy = r * increment
                    dx = Math.Tan(D2R(angle)) * dy
            End Select

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function RRShiftNodePosition of class GeneralFunction.")
            Return False
        End Try
    End Function

    Public Function GridIDWGapFill(ByRef myGrid As clsRaster, SearchRadius As Double, power As Double) As Boolean
        Try
            'this function replaces all nodata-values in a given grid by an inverse distance weighted value
            'we assume the grid has already been opened before this function was called
            Dim startRow As Integer, startCol As Integer, endRow As Integer, endCol As Integer
            Dim myDist As Double
            Dim r As Integer, c As Integer, r2 As Integer, c2 As Integer
            Dim w As Double
            Dim Teller As Double, Noemer As Double
            Dim nRows As Integer = myGrid.Grid.Header.NumberRows
            Dim nCols As Integer = myGrid.Grid.Header.NumberCols
            UpdateProgressBar("Collecting nodata cells...", 0, 10, True)

            'create a  dictionary of dictionaries containing information of the cell having a valid value
            Dim Verdict As New Dictionary(Of Integer, Dictionary(Of Integer, Boolean)) 'first key = row, second key = col, value = IsValid

            For r = 0 To myGrid.Grid.Header.NumberRows - 1
                UpdateProgressBar("", r, myGrid.Grid.Header.NumberRows, True)
                Verdict.Add(r, New Dictionary(Of Integer, Boolean))
                For c = 0 To myGrid.Grid.Header.NumberCols - 1
                    If myGrid.Grid.Value(c, r) = myGrid.Grid.Header.NodataValue Then
                        Verdict.Item(r).Add(c, False)
                    Else
                        Verdict.Item(r).Add(c, True)
                    End If
                Next
            Next

            'calculate the number of cells involved with the search radius
            Dim SearchRadiusCells As Integer = SearchRadius / myGrid.Grid.Header.dX

            'now walk through all nodata-cells
            Dim TotalDist As Double = 0
            Dim TotalWeight As Double = 0

            UpdateProgressBar("Filling gaps in grid...", 0, 10, True)
            For r = 0 To Verdict.Count - 1
                UpdateProgressBar("", r, Verdict.Count)
                For c = 0 To Verdict.Item(r).Count - 1
                    If Verdict.Item(r).Item(c) = False Then
                        'We found a nodata-cell. Now perform our IDW-interpolation
                        TotalDist = 0
                        TotalWeight = 0
                        Teller = 0
                        Noemer = 0
                        startRow = Math.Max(0, r - SearchRadiusCells)
                        endRow = Math.Min(r + SearchRadiusCells, nRows - 1)
                        startCol = Math.Max(0, c - SearchRadiusCells)
                        endCol = Math.Min(c + SearchRadiusCells, nCols - 1)

                        'walk through all cells in our search range and calculate the total distance and total weight
                        For r2 = startRow To endRow
                            For c2 = startCol To endCol
                                If Verdict.Item(r2).Item(c2) Then
                                    'we found a cell that is eligable for contributing to our weighted value
                                    myDist = Pythagoras(r, c, r2, c2)
                                    If myDist <= SearchRadiusCells Then
                                        w = 1 / (myDist ^ power)          'het gewicht van dit punt (als functie van de afstand)
                                        Teller += w * myGrid.Grid.Value(c2, r2)
                                        Noemer += w
                                    End If
                                End If
                            Next
                        Next
                        If Noemer <> 0 Then
                            myGrid.Grid.Value(c, r) = Teller / Noemer
                        End If
                    End If
                Next
            Next

            UpdateProgressBar("Operation complete", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function GridIDWGapFill of class GeneralFunctions: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function SQLiteDropAllIndexesFromTable(ByRef myConnection As SQLite.SQLiteConnection, TableName As String) As Boolean
        Try
            If Not myConnection.State = ConnectionState.Open Then myConnection.Open()

            Dim dt As New DataTable
            Dim query = "SELECT name FROM sqlite_master WHERE type == 'index' AND tbl_name == '" & TableName & "';"
            SQLiteQuery(myConnection, query, dt)
            For i = 0 To dt.Rows.Count - 1
                SQLiteNoQuery(myConnection, "DROP INDEX " & dt.Rows(i)(0) & ";")
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error Dropping all indexes from SQLite table: " & ex.Message)
            Return False
        End Try

    End Function


    Public Sub CreateOrUpdateSQLiteTable(ByVal SQLiteCon As SQLiteConnection, ByVal TableName As String, ByVal Fields As Dictionary(Of String, clsSQLiteField))

        Dim sql As New StringBuilder()
        Dim checkTableExistsSql As String = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}';"

        Using cmd As New SQLiteCommand(checkTableExistsSql, SQLiteCon)
            Dim result = cmd.ExecuteScalar()

            ' Create table if it does not exist
            If result Is Nothing Then
                sql.Append($"CREATE TABLE {TableName} (")

                Dim fieldsSql As New List(Of String)
                For Each field In Fields.Values
                    fieldsSql.Add($"{field.FieldName} {clsSQLiteField.SQLiteDataTypeToString(field.DataType)}")
                Next

                sql.Append(String.Join(", ", fieldsSql))
                sql.Append(");")

                Using createTableCmd As New SQLiteCommand(sql.ToString(), SQLiteCon)
                    createTableCmd.ExecuteNonQuery()
                End Using
            End If
        End Using

        ' Check and create fields if they do not exist
        For Each field In Fields.Values
            Dim checkFieldExistsSql As String = $"PRAGMA table_info({TableName});"

            Dim fieldExists As Boolean = False
            Using cmd As New SQLiteCommand(checkFieldExistsSql, SQLiteCon)
                Using reader As SQLiteDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        If reader("name").ToString().ToUpper() = field.FieldName.ToUpper() Then
                            fieldExists = True
                            Exit While
                        End If
                    End While
                End Using
            End Using

            If Not fieldExists Then
                Dim addFieldSql As String = $"ALTER TABLE {TableName} ADD COLUMN {field.FieldName} {clsSQLiteField.SQLiteDataTypeToString(field.DataType)};"
                Using addFieldCmd As New SQLiteCommand(addFieldSql, SQLiteCon)
                    addFieldCmd.ExecuteNonQuery()
                End Using
            End If

            ' Check and create indexes
            If field.HasIndex Then
                Dim indexName As String = $"idx_{TableName}_{field.FieldName}"
                Dim checkIndexExistsSql As String = $"SELECT name FROM sqlite_master WHERE type='index' AND name='{indexName}';"

                Using cmd As New SQLiteCommand(checkIndexExistsSql, SQLiteCon)
                    Dim result = cmd.ExecuteScalar()

                    If result Is Nothing Then
                        Dim createIndexSql As String = $"CREATE INDEX {indexName} ON {TableName}({field.FieldName});"

                        Using createIndexCmd As New SQLiteCommand(createIndexSql, SQLiteCon)
                            createIndexCmd.ExecuteNonQuery()
                        End Using
                    End If
                End Using
            End If
        Next
    End Sub


    Public Sub CreateOrUpdateSQLiteCompositeIndex(ByVal SQLiteCon As SQLiteConnection, ByVal TableName As String, ByVal Fields As List(Of String))
        Dim indexName As String = $"idx_{TableName}_{String.Join("_", Fields)}"

        ' Check if the composite index exists
        Dim checkIndexExistsSql As String = $"SELECT name FROM sqlite_master WHERE type='index' AND name='{indexName}';"

        Using cmd As New SQLiteCommand(checkIndexExistsSql, SQLiteCon)
            Dim result = cmd.ExecuteScalar()

            ' If the index does not exist, create the composite index
            If result Is Nothing Then
                Dim createIndexSql As String = $"CREATE INDEX {indexName} ON {TableName}({String.Join(", ", Fields)});"

                Using createIndexCmd As New SQLiteCommand(createIndexSql, SQLiteCon)
                    createIndexCmd.ExecuteNonQuery()
                End Using
            End If
        End Using
    End Sub


    Public Function UpgradeTimeseriesToolDatabase(ByRef myConnection As SQLite.SQLiteConnection) As Boolean
        Try
            If Not myConnection.State = ConnectionState.Open Then myConnection.Open()

            '------------------------------------------------------------------------------------------------------------------------
            'remove depricated tables and tables that belong to other applications
            '------------------------------------------------------------------------------------------------------------------------
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "VOLUMEPATTERNCOMBINATIONS") Then setup.GeneralFunctions.SQLiteDropTable(myConnection, "VOLUMEPATTERNCOMBINATIONS")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "PATTERNS") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "PATTERNS")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "INITIALCONDITIONS") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "INITIALCONDITIONS")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "INPUTFILES") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "INPUTFILES")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "INPUTSERIES") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "INPUTSERIES")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "OUTPUTFILES") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "OUTPUTFILES")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "INFLOWCONDITIONS") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "INFLOWCONDITIONS")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "OUTPUTLOCATIONS") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "OUTPUTLOCATIONS")
            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "RESULTS") Then Me.setup.GeneralFunctions.SQLiteDropTable(myConnection, "RESULTS")

            '------------------------------------------------------------------------------------------------------------------------
            'update the timeseries table
            If Not setup.GeneralFunctions.SQLiteTableExists(myConnection, "TIMESERIES") Then setup.GeneralFunctions.SQLiteCreateTable(myConnection, "TIMESERIES")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "TIMESERIES", "CASENAME") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "TIMESERIES", "CASENAME", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "TSCASENAME")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "TIMESERIES", "LOCATIONID") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "TIMESERIES", "LOCATIONID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "TSLOCIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "TIMESERIES", "DATEANDTIME") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "TIMESERIES", "DATEANDTIME", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "TSDATEIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "TIMESERIES", "PARAMETER") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "TIMESERIES", "PARAMETER", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "TSPARIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "TIMESERIES", "DATAVALUE") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "TIMESERIES", "DATAVALUE", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            '------------------------------------------------------------------------------------------------------------------------

            'update the events table
            SQLiteDropAllIndexesFromTable(myConnection, "EVENTS")
            If Not setup.GeneralFunctions.SQLiteTableExists(myConnection, "EVENTS") Then setup.GeneralFunctions.SQLiteCreateTable(myConnection, "EVENTS")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "CASENAME") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "CASENAME", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "EVCASEIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "EVENTNUM") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "EVENTNUM", GeneralFunctions.enmSQLiteDataType.SQLITEINT, "EVNUMIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "EVENTSUM") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "EVENTSUM", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "EVENTMIN") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "EVENTMIN", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "EVENTMAX") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "EVENTMAX", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "DURATION") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "DURATION", GeneralFunctions.enmSQLiteDataType.SQLITEINT, "EVDURIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "LOCATIONID") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "LOCATIONID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "EVLOCIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "PARAMETER") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "PARAMETER", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "EVPARIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "STARTDATE") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "STARTDATE", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "EVSTARTDATEIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "VOLUMECLASS") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "VOLUMECLASS", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "BOUNDARYCLASS") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "BOUNDARYCLASS", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "INITIALCLASS") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "INITIALCLASS", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "PATTERNCLASS") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTS", "PATTERNCLASS", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)

            'remove depricated fields
            If setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "PATTERNID") Then setup.GeneralFunctions.SQLiteDropColumn(myConnection, "EVENTS", "PATTERNID")
            If setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "DATEANDTIME") Then setup.GeneralFunctions.SQLiteDropColumn(myConnection, "EVENTS", "DATEANDTIME")
            If setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "DATAVALUE") Then setup.GeneralFunctions.SQLiteDropColumn(myConnection, "EVENTS", "DATAVALUE")
            If setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "TIMESTEP") Then setup.GeneralFunctions.SQLiteDropColumn(myConnection, "EVENTS", "TIMESTEP")
            If setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTS", "INITIALVALUE") Then setup.GeneralFunctions.SQLiteDropColumn(myConnection, "EVENTS", "INITIALVALUE")

            'update the EVENTSERIES table
            If Not setup.GeneralFunctions.SQLiteTableExists(myConnection, "EVENTSERIES") Then setup.GeneralFunctions.SQLiteCreateTable(myConnection, "EVENTSERIES")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "EVENTNUM") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "EVENTNUM", GeneralFunctions.enmSQLiteDataType.SQLITEINT, "EVSERIESNUMIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "DURATION") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "DURATION", GeneralFunctions.enmSQLiteDataType.SQLITEINT, "EVSERIESDURIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "LOCATIONID") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "LOCATIONID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "EVSERIESLOCIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "PARAMETER") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "PARAMETER", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "EVPSERIESARIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "TIMESTEP") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "TIMESTEP", GeneralFunctions.enmSQLiteDataType.SQLITEINT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "DATAVALUE") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "DATAVALUE", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "EVENTSERIES", "PATTERNCLASS") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "EVENTSERIES", "PATTERNCLASS", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            setup.GeneralFunctions.SQLiteCreateIndex(myConnection, "EVENTSERIES", "EVENTSERIESIDX", "EVENTNUM, DURATION, LOCATIONID, PARAMETER", False)

            'update the patternclasses table. this contains the 'template' temporal patterns, based on the reference series
            If Not setup.GeneralFunctions.SQLiteTableExists(myConnection, "PATTERNCLASSES") Then Me.setup.GeneralFunctions.SQLiteCreateTable(myConnection, "PATTERNCLASSES")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "CLASSID") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "PATTERNCLASSES", "CLASSID", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "PATTCLASSIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "DURATION") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "PATTERNCLASSES", "DURATION", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT, "PATCLASSDURIDX")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "TIMESTEP") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "PATTERNCLASSES", "TIMESTEP", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "FRACTION") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "PATTERNCLASSES", "FRACTION", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "VOLUMECLASS") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "PATTERNCLASSES", "VOLUMECLASS")
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "P") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "PATTERNCLASSES", "P")
            setup.GeneralFunctions.SQLiteCreateIndex(myConnection, "PATTERNCLASSES", "PATTERNCLASSESINDEX", "PATTERNID, DURATION", False)

            'remove depricated fields
            If setup.GeneralFunctions.SQLiteColumnExists(myConnection, "PATTERNCLASSES", "PATTERNID") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "PATTERNCLASSES", "PATTERNID")


            'create the volume classes table
            If Not Me.setup.GeneralFunctions.SQLiteTableExists(myConnection, "VOLUMECLASSES") Then Me.setup.GeneralFunctions.SQLiteCreateTable(myConnection, "VOLUMECLASSES")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "VOLUMECLASS") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMECLASSES", "VOLUMECLASS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "VOLCLASSCLASSIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "DURATION") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMECLASSES", "DURATION", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT, "VOLCLASSDURIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "LOWERPERCENTILE") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMECLASSES", "LOWERPERCENTILE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "UPPERPERCENTILE") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMECLASSES", "UPPERPERCENTILE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "CLASSPERCENTILE") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMECLASSES", "CLASSPERCENTILE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "FREQ") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "VOLUMECLASSES", "FREQ")
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMECLASSES", "CLASSID") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "VOLUMECLASSES", "CLASSID")
            setup.GeneralFunctions.SQLiteCreateIndex(myConnection, "VOLUMECLASSES", "VOLUMECLASSINDEX", "VOLUMECLASS, DURATION", False)

            'create the volumes table if need be
            If Not Me.setup.GeneralFunctions.SQLiteTableExists(myConnection, "VOLUMES") Then Me.setup.GeneralFunctions.SQLiteCreateTable(myConnection, "VOLUMES")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "DURATION") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "DURATION", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT, "VOLDURIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "VOLUMECLASS") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "VOLUMECLASS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "VOLCLASSIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "LOCATIONID") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "LOCATIONID", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "VOLLOCIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "PARAMETER") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "PARAMETER", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "VOLPARIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "LBOUND") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "LBOUND", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "UBOUND") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "UBOUND", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "REPRESENTATIVE") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "VOLUMES", "REPRESENTATIVE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "VOLUMES", "VOLUME") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "VOLUMES", "VOLUME")
            setup.GeneralFunctions.SQLiteCreateIndex(myConnection, "VOLUMES", "VOLUMESINDEX", "DURATION, VOLUMECLASS, LOCATIONID, PARAMETER", False)

            'create the INITIALS table is need be
            If Not Me.setup.GeneralFunctions.SQLiteTableExists(myConnection, "INITIALS") Then Me.setup.GeneralFunctions.SQLiteCreateTable(myConnection, "INITIALS")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALS", "LOCATIONID") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALS", "LOCATIONID", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "INITLOCIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALS", "DURATION") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALS", "DURATION", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT, "INITDURIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALS", "EVENTNUM") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALS", "EVENTNUM", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT, "INITEVENTIDX")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALS", "DATEANDTIME") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALS", "DATEANDTIME", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALS", "DATAVALUE") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALS", "DATAVALUE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALS", "INITIALCLASS") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALS", "INITIALCLASS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            setup.GeneralFunctions.SQLiteCreateIndex(myConnection, "INITIALS", "INITIALSINDEX", "LOCATIONID, DURATION, EVENTNUM", False)

            'create the INITIALCLASSES table if need be
            If Not Me.setup.GeneralFunctions.SQLiteTableExists(myConnection, "INITIALCLASSES") Then Me.setup.GeneralFunctions.SQLiteCreateTable(myConnection, "INITIALCLASSES")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALCLASSES", "CLASSID") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALCLASSES", "CLASSID", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALCLASSES", "P") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "INITIALCLASSES", "P", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "INITIALCLASSES", "VOLUMECLASS") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "INITIALCLASSES", "VOLUMECLASS")

            'create or update the BOUNDARYSERIES table
            If Not setup.GeneralFunctions.SQLiteTableExists(myConnection, "BOUNDARYSERIES") Then setup.GeneralFunctions.SQLiteCreateTable(myConnection, "BOUNDARYSERIES")
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYSERIES", "EVENTNUM") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYSERIES", "EVENTNUM", GeneralFunctions.enmSQLiteDataType.SQLITEINT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYSERIES", "DURATION") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYSERIES", "DURATION", GeneralFunctions.enmSQLiteDataType.SQLITEINT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYSERIES", "BOUNDARYID") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYSERIES", "BOUNDARYID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYSERIES", "TIMESTEP") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYSERIES", "TIMESTEP", GeneralFunctions.enmSQLiteDataType.SQLITEINT)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYSERIES", "DATAVALUE") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYSERIES", "DATAVALUE", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Not setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYSERIES", "BOUNDARYCLASS") Then setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYSERIES", "BOUNDARYCLASS", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            setup.GeneralFunctions.SQLiteCreateIndex(myConnection, "BOUNDARYSERIES", "BOUNDARYSERIESIDX", "EVENTNUM, DURATION, BOUNDARYID", False)

            'create the BOUNDARYCLASSES table if need be
            If Not Me.setup.GeneralFunctions.SQLiteTableExists(myConnection, "BOUNDARYCLASSES") Then Me.setup.GeneralFunctions.SQLiteCreateTable(myConnection, "BOUNDARYCLASSES")
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYCLASSES", "CLASSID") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYCLASSES", "CLASSID", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            If Not Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYCLASSES", "P") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(myConnection, "BOUNDARYCLASSES", "P", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            If Me.setup.GeneralFunctions.SQLiteColumnExists(myConnection, "BOUNDARYCLASSES", "VOLUMECLASS") Then Me.setup.GeneralFunctions.SQLiteDropColumn(myConnection, "BOUNDARYCLASSES", "VOLUMECLASS")

            If setup.GeneralFunctions.SQLiteTableExists(myConnection, "WATERLEVELS") Then
                'convert the old data from the "WATERLEVELS" table to the new format
                Dim dt As New DataTable
                Dim query As String
                query = "SELECT OBJECTID, EVENTDATE, MAXVAL FROM WATERLEVELS ORDER BY OBJECTID, EVENTDATE;"
                If Not setup.GeneralFunctions.SQLiteQuery(myConnection, query, dt) Then Throw New Exception("Error executing query " & query)

                Dim i As Long, LastID As String = "", EventNum As Integer
                Me.setup.GeneralFunctions.UpdateProgressBar("Updating database...", 0, 10, True)
                For i = 0 To dt.Rows.Count - 1
                    If dt.Rows(i)(0) <> LastID Then
                        EventNum = 1
                        LastID = dt.Rows(i)(0)
                    Else
                        EventNum += 1
                    End If
                    Me.setup.GeneralFunctions.UpdateProgressBar("", i, dt.Rows.Count)
                    query = "INSERT INTO EVENTS (LOCATIONID, DATEANDTIME, PARAMETER, DATAVALUE, EVENTNUM, EVENTSUM, DURATION) VALUES ('" & dt.Rows(i)(0) & "','" & Strings.Format(dt.Rows(i)(1), "yyyy/MM/dd hh:mm:ss") & "','H'," & dt.Rows(i)(2) & "," & EventNum & ",0,1);"
                    If Not setup.GeneralFunctions.SQLiteNoQuery(myConnection, query, False) Then Throw New Exception("Error executing query " & query)
                Next
                Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

                'now we can drop the WATERLEVELS table
                setup.GeneralFunctions.SQLiteDropTable(myConnection, "WATERLEVELS")
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error upgrading Timeseriestool database: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function ShapefileIsLineType(ByRef sf As MapWinGIS.Shapefile) As Boolean
        Try
            Select Case sf.ShapefileType
                Case MapWinGIS.ShpfileType.SHP_POLYLINE, MapWinGIS.ShpfileType.SHP_POLYLINEM, MapWinGIS.ShpfileType.SHP_POLYLINEZ
                    Return True
                Case Else
                    Return False
            End Select
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function ShapefileIsLineType of class GeneralFunctions.")
            Return False
        End Try
    End Function
    Public Function IsFileLocked(FileName As String) As Boolean
        Dim file As New FileInfo(FileName)
        Dim stream = DirectCast(Nothing, FileStream)
        Try
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None)
        Catch generatedExceptionName As IOException
            'handle the exception your way
            Return True
        Finally
            If stream IsNot Nothing Then
                stream.Close()
            End If
        End Try
        Return False
    End Function

    Public Function HTMLToLatex(myHTML As String) As String
        Try

            'here we will parse the given HTML code and convert it into TEX
            Dim Doc As New HtmlDocument
            Dim caption As String = ""

            myHTML = myHTML.Replace("\", "\textbackslash ")     'replace \ by \textbackslash since \ is a reserved keyword in TEX
            myHTML = myHTML.Replace("_", "\_")                  'replace _ by \_ since _ is a reserved keyword in TEX
            myHTML = myHTML.Replace("%", "\%")                  'replace % by \% since % is a reserved keyword in TEX
            Doc.LoadHtml(myHTML)
            Dim TEXStr As String = ""

            For Each aNode As HtmlNode In Doc.DocumentNode.ChildNodes
                If aNode.Name = "i" Then
                    'italic
                    TEXStr &= "\emph{" & aNode.InnerText & "}"
                ElseIf aNode.Name = "ul" Then
                    'unordered list
                    TEXStr &= "\begin{itemize}" & vbCrLf
                    For Each bNode As HtmlNode In aNode.ChildNodes
                        If bNode.Name = "li" Then
                            TEXStr &= "\item " & bNode.InnerText & vbCrLf
                        End If
                    Next
                    TEXStr &= "\end{itemize}" & vbCrLf
                ElseIf aNode.Name = "table" Then
                    TEXStr &= "\begin{small}" & vbCrLf
                    TEXStr &= "\begin{center}" & vbCrLf
                    Dim nRows As Integer, nCols As Integer, tmpStr As String = ""
                    For Each bNode As HtmlNode In aNode.ChildNodes
                        If bNode.Name = "tr" Then
                            tmpStr = "||"
                            nRows += 1
                            nCols = 0
                            For Each cNode As HtmlNode In bNode.ChildNodes
                                If cNode.Name = "th" OrElse cNode.Name = "td" Then
                                    nCols += 1
                                    tmpStr &= " c"
                                End If
                            Next
                            tmpStr &= " ||"
                        ElseIf bNode.Name = "caption" Then
                            caption = bNode.InnerText
                        End If
                    Next
                    tmpStr = "{"
                    For i = 1 To nCols
                        tmpStr &= "p{" & Math.Round(1 / nCols, 2) & "\textwidth}" 'equally divide the columns over the textwidth
                    Next
                    tmpStr &= "}"
                    TEXStr &= "\begin{longtable}" & tmpStr & vbCrLf
                    TEXStr &= "\caption{" & caption & "\label{long}}\\" & vbCrLf
                    For Each bNode As HtmlNode In aNode.ChildNodes
                        If bNode.Name = "tr" Then
                            TEXStr &= "\hline" & vbCrLf
                            tmpStr = ""
                            For Each cNode As HtmlNode In bNode.ChildNodes
                                If cNode.Name = "th" OrElse cNode.Name = "td" Then
                                    If nCols > 5 AndAlso cNode.Name = "th" Then
                                        tmpStr &= "\mbox{\begin{turn}{90}" & cNode.InnerText.Trim & "\end{turn}}" & " & " 'test if \mbox keeps everything neatly together
                                    Else
                                        tmpStr &= "\mbox{" & cNode.InnerText.Trim & "}" & " & " 'test if \mbox keeps everything neatly together
                                    End If
                                End If
                            Next
                            tmpStr = Left(tmpStr, tmpStr.Length - 2) & " \\"
                            TEXStr &= tmpStr & vbCrLf
                            TEXStr &= "\hline" & vbCrLf
                        End If
                    Next
                    TEXStr &= "\end{longtable}" & vbCrLf
                    TEXStr &= "\end{center}" & vbCrLf
                    TEXStr &= "\end{small}" & vbCrLf
                Else
                    'no html item so just add the regular text. Wrap an \mbox around it
                    TEXStr &= aNode.InnerText & vbCrLf
                End If
            Next

            Return TEXStr
        Catch ex As Exception
            Return String.Empty
        End Try

    End Function

    Public Function PolarCoordinates(XCenter As Double, YCenter As Double, Radius As Double, AngleRadians As Double, ByRef x As Double, ByRef y As Double) As Boolean
        'sin(angle) = dx/radius
        'dx = sin(angle) * radius

        'cos(angle) = dy/radius
        'dy = cos(angle) * radius
        Try
            x = XCenter + Math.Sin(AngleRadians) * Radius
            y = YCenter + Math.Cos(AngleRadians) * Radius
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function PolarCoordinates: " & ex.Message)
            Return False
        End Try

    End Function
    Public Function ReadFileContentToString(path As String) As String
        Using txtReader As New StreamReader(path)
            Return txtReader.ReadToEnd
        End Using
    End Function

    Public Function WindowsSafeFilename(myStr As String) As String
        'converts a string to a new string that can safely be used in e.g. Windows paths
        myStr = Replace(myStr, "/", "_")
        myStr = Replace(myStr, "\", "_")
        myStr = Replace(myStr, "[", "(")    'Matlab runtime cannot handle [ and ]
        myStr = Replace(myStr, "]", ")")
        Return myStr
    End Function
    Public Function ParseStringToRegExList(RegExDelimitedString As String, Delimiter As String) As List(Of String)
        'creates a list of Regular Expressions based on a string containing multiple regular expressions that are separated by a delimiter
        Dim myList As New List(Of String)
        While Not RegExDelimitedString = ""
            myList.Add(ParseString(RegExDelimitedString, Delimiter))
        End While
        Return myList
    End Function

    Public Function OpenWebaddress(myAddress As String) As Boolean
        Try
            System.Diagnostics.Process.Start(myAddress)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ShapefileContainsMultipartPolygons(ByRef mySF As MapWinGIS.Shapefile) As Boolean
        For i = 0 To mySF.NumShapes - 1
            If mySF.Shape(i).NumParts > 1 Then Return True
        Next
        Return False
    End Function

    Public Function ShapeEntirelyWithinExtents(ByRef Shape As MapWinGIS.Shape, ByRef Extents As MapWinGIS.Extents) As Boolean
        Try
            If Shape.Extents.xMin < Extents.xMin Then Return False
            If Shape.Extents.xMin > Extents.xMax Then Return False
            If Shape.Extents.xMax > Extents.xMax Then Return False
            If Shape.Extents.xMax < Extents.xMin Then Return False
            If Shape.Extents.yMin < Extents.yMin Then Return False
            If Shape.Extents.yMin > Extents.yMax Then Return False
            If Shape.Extents.yMax > Extents.yMax Then Return False
            If Shape.Extents.yMax < Extents.yMin Then Return False
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function ShapeEntirelyWithinExtents of class GeneralFunctions.")
            Return False
        End Try
    End Function

    Public Function ShapeExtentsoverlap(ByRef FirstShapeExt As MapWinGIS.Extents, ByRef SecondExt As MapWinGIS.Extents) As Boolean
        If FirstShapeExt.xMin > SecondExt.xMax OrElse SecondExt.xMin > FirstShapeExt.xMax Then
            Return False
        ElseIf FirstShapeExt.yMax < SecondExt.yMin OrElse SecondExt.yMax < FirstShapeExt.yMin Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function GetIntFromString(myStr As String, ByRef MyInt As Integer, Optional SearchBackward As Boolean = True) As Boolean
        'returns the numeric (integer) part of a given string. Optionally start searching from the back or from the front
        Try
            Dim i As Integer, Found As Boolean = False
            Dim returnStr As String = ""
            If Not SearchBackward Then
                For i = 1 To myStr.Length
                    If IsNumeric(Mid(myStr, i, 1)) Then
                        returnStr &= Mid(myStr, i, 1)
                        Found = True
                    ElseIf Found Then
                        'we have processed the last numeric character in a row. Return the result
                        MyInt = Val(returnStr)
                        Exit For
                    End If
                Next
            Else
                For i = myStr.Length To 1 Step -1                   'note: string positions appear to be 1-based
                    If IsNumeric(Mid(myStr, i, 1)) Then
                        returnStr = Mid(myStr, i, 1) & returnStr
                        Found = True
                    ElseIf Found Then
                        'we have processed the last numeric character in a row. Return the result
                        MyInt = Val(returnStr)
                        Exit For
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function FixupShapefile(ByRef origSF As MapWinGIS.Shapefile, ByRef fixedSF As MapWinGIS.Shapefile, RemoveOriginalFromMemory As Boolean, SaveResultFile As Boolean, Optional ByVal fixedSFPath As String = "") As Boolean
        Try
            Dim InvalidShapesFound As Boolean = False
            If origSF.HasInvalidShapes Then
                InvalidShapesFound = True
                Me.setup.Log.AddMessage("Shapefile contains invalid shapes. An attempt To automatically fix Is executed: " & origSF.Filename & ".")
            End If
            origSF.FixUpShapes2(False, fixedSF)

            If RemoveOriginalFromMemory Then
                origSF.Close()
                origSF = Nothing
            End If

            If fixedSF.HasInvalidShapes Then
                Throw New Exception("Error: subcatchemnts shapefile has invalid shapes that could not be fixed automatically: " & origSF.Filename & ".")
            ElseIf InvalidShapesFound Then
                Me.setup.Log.AddMessage("Automatic repair of invalid shapes was succesful. The resulting shapefile has been written to " & fixedSFPath & ".")
            End If

            If SaveResultFile Then
                DeleteShapeFile(fixedSFPath)
                fixedSF.SaveAs(fixedSFPath)
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Private Function ComputeDistance(s As String, t As String) As Integer
        Dim n As Integer = s.Length
        Dim m As Integer = t.Length
        Dim distance As Integer(,) = New Integer(n, m) {}
        ' matrix
        Dim cost As Integer = 0
        If n = 0 Then
            Return m
        End If
        If m = 0 Then
            Return n
        End If
        'init1

        Dim i As Integer = 0

        While i <= n
            distance(i, 0) = System.Math.Min(System.Threading.Interlocked.Increment(i), i - 1)
        End While
        Dim j As Integer = 0
        While j <= m
            distance(0, j) = System.Math.Min(System.Threading.Interlocked.Increment(j), j - 1)
        End While
        'find min distance

        For i = 1 To n
            For j = 1 To m
                cost = (If(t.Substring(j - 1, 1) = s.Substring(i - 1, 1), 0, 1))
                distance(i, j) = Math.Min(distance(i - 1, j) + 1, Math.Min(distance(i, j - 1) + 1, distance(i - 1, j - 1) + cost))
            Next
        Next
        Return distance(n, m)
    End Function

    Public Function GENERATE_TIDAL_SINUS(Amplitude As Double, StartDate As DateTime, MeanWaterLevel As Double, CurDate As DateTime) As Double
        Dim Period As New TimeSpan(12, 25, 0) 'a tidal wave takes 12 hours and 25 minutes
        GENERATE_TIDAL_SINUS = Amplitude / 2 * Math.Sin(2 * 3.1415 / Period.TotalDays * (CurDate.Subtract(StartDate).TotalDays)) + MeanWaterLevel
    End Function


    '------------------------------------------------------------------------------------------------------------------------------------------
    ' END OF Levenshtein string matching algorithm
    '------------------------------------------------------------------------------------------------------------------------------------------
    Public Function MakeUniqueID(ExistingIDList As List(Of String), ID As String, maxLength As String, BaseLeft As Boolean) As String
        'a boolean baseleft is to decide whether the left side of the original id should be used as a basis for the new id. If not, we use the right side
        Dim Done As Boolean = False
        Dim i As Integer = 0

        While Not Done
            If BaseLeft Then
                If ID.Length > maxLength Then ID = Left(ID, maxLength)
            Else
                If ID.Length > maxLength Then ID = Right(ID, maxLength)
            End If

            If Not ExistingIDList.Contains(ID) Then
                Done = True
            Else
                i += 1
                ID = Left(ID, ID.Length - i.ToString.Length) & i.ToString
            End If

        End While
        Return ID
    End Function

    Public Function StringtoDouble(myStr As String, ByRef myValue As Double, DefaultValue As Double) As Boolean
        If IsNumeric(myStr) Then
            myValue = Val(myStr)
            Return True
        Else
            myValue = DefaultValue
            Return False
        End If
    End Function

    Public Function StringtoInt32(myStr As String, ByRef myValue As Integer, DefaultValue As Integer) As Boolean
        If IsNumeric(myStr) Then
            myValue = Convert.ToInt32(myStr)
            Return True
        Else
            myValue = DefaultValue
            Return False
        End If
    End Function

    Public Function Drainageweerstand(HalfMaatgevendeAfvoerMMPD As Double, Grondwaterdiepte As Double, DrainDiepte As Double, ByRef Weerstand As Double) As Boolean
        'berekent de drainageweerstand (d) voor een gegeven afvoer (mm/d), grondwaterdiepte en drainagediepte (m)
        Try
            If Grondwaterdiepte = 0 Then
                Weerstand = 0
            Else
                If HalfMaatgevendeAfvoerMMPD = 0 Then
                    'is waarschijnlijk een landgebruik van het type ongedraineerd. We kennen een default van 150d toe
                    Weerstand = 150
                Else
                    Weerstand = Math.Round(1000 * (DrainDiepte - Grondwaterdiepte) / HalfMaatgevendeAfvoerMMPD, 0)
                End If
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function Drainageweerstand of class GeneralFunctions.")
            Return False
        End Try
    End Function

    Public Function OntwateringsNorm(Landgebruik As EnmLanduseTypeBuisdrainagekaart, BodClass As GeneralFunctions.enmSoilTypeBuisdrainagekaart, ByRef GWdepth As Double, ByRef HMAmmpd As Double) As Boolean
        'deze functie baseert zich op de ontwerpdrooglegging van landbouwgebied. Zie publicatie Buisdrainagekaart 2015 (Alterra) tabel 5 en Cultuurtechnisch Vademecum
        'Massop, h.Th.L.en C. Schuiling, 2016.Buisdrainagekaart 2015; Update landelijke buisdrainagekaart
        'op basis van de landbouwmeitellingen van 2012. Wageningen, Alterra Wageningen UR (University &
        'Research centre), Alterra-rapport 2700. 54 blz.; 27 fig.; 11 tab.; 17 ref.
        'diepte in m
        'afvoer in mm/d
        Try
            Select Case Landgebruik
                Case EnmLanduseTypeBuisdrainagekaart.grasland
                    HMAmmpd = 7
                    GWdepth = 0.3
                Case EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    HMAmmpd = 7
                    GWdepth = 0.5
                Case EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    HMAmmpd = 7
                    GWdepth = 0.7
                Case EnmLanduseTypeBuisdrainagekaart.boomgaard
                    HMAmmpd = 7
                    GWdepth = 0.7
                Case EnmLanduseTypeBuisdrainagekaart.stedelijk
                    HMAmmpd = 5
                    GWdepth = 0.7
                Case EnmLanduseTypeBuisdrainagekaart.begraafplaats
                    HMAmmpd = 7
                    GWdepth = 1.15
                Case EnmLanduseTypeBuisdrainagekaart.golfterrein
                    HMAmmpd = 15
                    GWdepth = 0.5
                Case EnmLanduseTypeBuisdrainagekaart.sportterrein
                    HMAmmpd = 15
                    GWdepth = 0.5
                Case EnmLanduseTypeBuisdrainagekaart.vliegveld
                    HMAmmpd = 7
                    GWdepth = 0.5
                Case EnmLanduseTypeBuisdrainagekaart.glastuinbouw
                    HMAmmpd = 7
                    GWdepth = 0.5
                Case EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    If BodClass = enmSoilTypeBuisdrainagekaart.klei Then
                        HMAmmpd = 7
                        GWdepth = 0.1 'we nemen aan dat de grondwaterstand bij ongedraineerde klei bij HMA tot 10 cm onder maaiveld komt
                    ElseIf BodClass = enmSoilTypeBuisdrainagekaart.veen Then
                        HMAmmpd = 7
                        GWdepth = 0.1 'we nemen aan dat de grondwaterstand bij ongedraineerde veen bij HMA tot 10 cm onder maaiveld komt
                    ElseIf BodClass = enmSoilTypeBuisdrainagekaart.zand Then
                        HMAmmpd = 7
                        GWdepth = 0.5 'we nemen aan dat de grondwaterstand bij ongedraineerd zand bij HMA tot 50 cm onder maaiveld komt
                    End If
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function Ontwateringsnorm of class Generalfunctions.")
            Return False
        End Try
    End Function
    Public Function Grondwatertrap(GHGcmMinMV As Double, GLGcmMinMV As Double) As enmGrondwatertrap
        'implements https://nl.wikipedia.org/wiki/Grondwatertrap

        If GHGcmMinMV < 0.2 AndAlso GLGcmMinMV < 0.5 Then
            Return enmGrondwatertrap.I
        ElseIf GHGcmMinMV < 0.4 AndAlso GLGcmMinMV >= 0.5 AndAlso GLGcmMinMV < 0.8 Then
            Return enmGrondwatertrap.II
        ElseIf GHGcmMinMV >= 0.25 AndAlso GHGcmMinMV < 0.4 AndAlso GLGcmMinMV >= 0.5 AndAlso GLGcmMinMV < 0.8 Then
            Return enmGrondwatertrap.IIB
        ElseIf GHGcmMinMV < 0.4 AndAlso GLGcmMinMV >= 0.8 AndAlso GLGcmMinMV < 1.2 Then
            Return enmGrondwatertrap.III
        ElseIf GHGcmMinMV >= 0.25 AndAlso GHGcmMinMV < 0.4 AndAlso GLGcmMinMV >= 0.8 AndAlso GLGcmMinMV < 1.2 Then
            Return enmGrondwatertrap.IIIB
        ElseIf GHGcmMinMV >= 0.4 AndAlso GLGcmMinMV >= 0.8 AndAlso GLGcmMinMV < 1.2 Then
            Return enmGrondwatertrap.IV
        ElseIf GHGcmMinMV < 0.4 AndAlso GLGcmMinMV >= 1.2 Then
            Return enmGrondwatertrap.V
        ElseIf GHGcmMinMV >= 0.4 AndAlso GHGcmMinMV < 0.8 AndAlso GLGcmMinMV >= 1.2 Then
            Return enmGrondwatertrap.VI
        ElseIf GHGcmMinMV >= 0.8 AndAlso GHGcmMinMV < 1.4 Then
            Return enmGrondwatertrap.VII
        ElseIf GHGcmMinMV >= 1.4 Then
            Return enmGrondwatertrap.VIII
        Else
            Return Nothing
        End If
    End Function

    Public Sub DatabaseWaitForUnlock(Path As String, WaitMaxSeconds As Integer)
        Dim iWait As Long
        Dim lockFile As String = Path & ".lock"
        While System.IO.File.Exists(lockFile)
            iWait += 1
            System.Threading.Thread.Sleep(1000)
            If iWait > 10 Then
                System.IO.File.Delete(lockFile)
                Me.setup.Log.AddError("Database stayed locked for too long. The lock was forcibly removed and process continued.")
                Exit While
            End If
        End While
    End Sub

    Public Sub DatabaseReleaseLock(path As String)
        Dim lockFile As String = path & ".lock"
        If System.IO.File.Exists(lockFile) Then System.IO.File.Delete(lockFile)
    End Sub

    Public Sub DatabaseWriteLockFile(path As String, EventID As String)
        '--------------------------------------------------------------------------------------------
        '    lock the database by creating the .lock file alongside the database
        '--------------------------------------------------------------------------------------------
        Dim lockFile As String = path & ".lock"
        Using lockWriter As New StreamWriter(lockFile)
            lockWriter.WriteLine("Database locked for simulation " & EventID)
        End Using
    End Sub

    Public Function RegExMatch(ByVal Pattern As String, myText As String, IgnoreCase As Boolean) As Boolean
        Try
            'THIS FUNCTION USES THE BUILT-IN REGULAR EXPRESSIONS TO MATCH TEXT TO AN EXPRESSION (incl. e.g. wildcards)
            'v1.890: switched to the internal regex-namespace System.Text.Regularexpressions
            If Pattern.Trim = "*" Then Return True
            Dim options As New RegexOptions
            If IgnoreCase Then options = RegexOptions.IgnoreCase
            Dim mc As MatchCollection = Regex.Matches(myText, Pattern, options)
            Dim m As Match
            For Each m In mc
                Return True
            Next m
            Return False

            ''if not a wildcard is used at the start of the expression, add ^ to force that the text actually begins with the string 
            'If Not Left(Pattern, 1) = "*" Then Pattern = "^" & Pattern

            ''a shortcut for exact matches (this is not being picked up by the Regex class!)
            'If myText.ToUpper = Pattern.ToUpper Then
            '    If IgnoreCase Then
            '        Return True
            '    ElseIf myText = Pattern Then
            '        Return True
            '    End If
            'End If

            ''continue by checking the regular expression
            'If Pattern = "*" OrElse Pattern = "" Then
            '    Return True
            'Else
            '    Dim regex As Regex = New Regex(Pattern)
            '    Dim match As Match
            '    If IgnoreCase Then
            '        match = Regex.Match(myText, Pattern, RegexOptions.IgnoreCase)
            '    Else
            '        match = Regex.Match(myText, Pattern, RegexOptions.IgnoreCase)
            '    End If
            '    If match.Success Then
            '        Return True
            '    Else
            '        Return False
            '    End If
            'End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ShapeFileContainsInvalidShapes(path As String) As Boolean
        Dim tmpSF As New MapWinGIS.Shapefile
        tmpSF.Open(path)
        If tmpSF.HasInvalidShapes Then Return True Else Return False
        tmpSF.Close()
    End Function

    Public Function StripPrefixes(ID As String, PrefixList As List(Of String)) As String
        Dim Done As Boolean = False
        While Not Done
            Done = True
            For Each myPrefix As String In PrefixList
                If Left(ID, myPrefix.Length) = myPrefix Then
                    ID = Right(ID, ID.Length - myPrefix.Length)
                    Done = False
                End If
            Next
        End While
        Return ID
    End Function

    Public Function StripPostfixes(ID As String, PostfixList As List(Of String)) As String
        Dim Done As Boolean = False
        While Not Done
            Done = True
            For Each myPostfix As String In PostfixList
                If Right(ID, myPostfix.Length) = myPostfix Then
                    ID = Left(ID, ID.Length - myPostfix.Length)
                    Done = False
                End If
            Next
        End While
        Return ID
    End Function

    Public Function TextMatchUsingWildcards(Patterns As List(Of String), myString As String, IgnoreCase As Boolean) As Boolean
        'this function checks if a given string (myStr) is part of one of a list of string, using wildcards * (multiple characters) and/or ? (one character) 
        For Each myExpression As String In Patterns

            'hieronder een poging om wildcards te converteren naar regex (zie https://stackoverflow.com/questions/30299671/matching-strings-with-wildcard/30300521)
            'Dim myRegEx As String = "^" + Regex.Escape(myExpression).Replace("\\?", ".").Replace("\\*", ".*") + "$"
            'If RegExMatch(myExpression, myString, IgnoreCase) Then Return True

            If IgnoreCase Then
                If LikeOperator.LikeString(myString, myExpression, CompareMethod.Text) Then Return True
            Else
                If LikeOperator.LikeString(myString, myExpression, CompareMethod.Binary) Then Return True
            End If
        Next
        Return False
    End Function

    Public Function RegExListContains(Expressions As List(Of String), myString As String, IgnoreCase As Boolean) As Boolean
        'this function checks if a given string (myStr) is part of one of a list of string, using regular expressions
        For Each myExpression As String In Expressions
            If RegExMatch(myExpression, myString, IgnoreCase) Then Return True
        Next
        Return False
    End Function


    Public Function GetShapeFieldIdxByName(SF As MapWinGIS.Shapefile, FieldName As String, Optional ByVal IgnoreCase As Boolean = True) As Integer
        Dim i As Integer
        For i = 0 To SF.NumFields - 1
            If IgnoreCase Then
                If SF.Field(i).Name.Trim.ToUpper = FieldName.Trim.ToUpper Then
                    Return i
                End If
            Else
                If SF.Field(i).Name.Trim = FieldName.Trim Then
                    Return i
                End If
            End If
        Next
        Return -1
    End Function


    Public Function TimeseriesFiltering(dt As DataTable, Filter As enmTimeSeriesFilter, FilterValue As Double) As DataTable
        'this function performs a timeseries filter on a given datatable
        'we assume that the first column in the table contains datetime and the second column contains the values
        Try
            Dim nt = New DataTable
            nt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
            nt.Columns.Add(New DataColumn("Value", Type.GetType("System.Double")))
            Select Case Filter
                Case Is = enmTimeSeriesFilter.ANNUALMAX
                    Throw New Exception("Error: timeseries filter function ANNUALMAX not yet supported.")
                Case Is = enmTimeSeriesFilter.FIRST
                    Return DataTableTimeseriesTimestepValue(dt, 0, 1, 0)
                Case Is = enmTimeSeriesFilter.LAST
                    Return DataTableTimeseriesTimestepValue(dt, 0, 1, dt.Rows.Count - 1)
                Case Is = enmTimeSeriesFilter.LOWER_PERCENTILE
                    Return DataTableTimeseriesLowerPercentile(dt, 0, 1, 0, dt.Rows.Count - 1, FilterValue)
                Case Is = enmTimeSeriesFilter.UPPER_PERCENTILE
                    Return DataTableTimeseriesUpperPercentile(dt, 0, 1, 0, dt.Rows.Count - 1, FilterValue)
                Case Is = enmTimeSeriesFilter.MAX
                    Return DataTableTimeseriesMaxValue(dt, 0, 1, 0, dt.Rows.Count - 1)
                Case Is = enmTimeSeriesFilter.MIN
                    Return DataTableTimeseriesMinValue(dt, 0, 1, 0, dt.Rows.Count - 1)
                Case Is = enmTimeSeriesFilter.MONTHLYAVG
                    Throw New Exception("Error: timeseries filter function ANNUALMAX not yet supported.")
                Case Is = enmTimeSeriesFilter.MONTHLYMAX
                    Throw New Exception("Error: timeseries filter function ANNUALMAX not yet supported.")
                Case Is = enmTimeSeriesFilter.DAILYSUM
                    Return DataTableTimeseriesDailySum(dt, 0, 1, 0, dt.Rows.Count - 1)
                Case Is = enmTimeSeriesFilter.NONE
                    Return dt
                Case Is = enmTimeSeriesFilter.SKIPFIRSTPERCENTAGE
                    Return DataTableTimeseriesSubset(dt, 0, 1, Math.Round(dt.Rows.Count * FilterValue / 100, 0) - 1, dt.Rows.Count - 1)
                Case Is = enmTimeSeriesFilter.MAX_PER_EVENT
                    Return DataTableTimeseriesMaxPerEvent(dt, 0, 1, FilterValue)    'filtervalues = skip first x %
                Case Else
            End Select
            Return Nothing
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function TimeSeriesFiltering of class GeneralFunctions.")
            Return Nothing
        End Try
    End Function

    Public Function GetShapeValueAndTestIfNumeric(ByRef Shapefile As MapWinGIS.Shapefile, ByVal FieldIdx As Integer, ByVal RecordIdx As Integer, ByRef myValue As Double, ErrorIfNotNumeric As Boolean, WarningIfNotNumeric As Boolean, ReplaceByNodataIfNotNumeric As Boolean, Optional ByVal ObjectID As String = "", Optional ByVal ObjectPar As String = "") As Boolean
        'v2.114: replaced isDbNull by IsNothing
        If IsNothing(Shapefile.CellValue(FieldIdx, RecordIdx)) Then
            If ErrorIfNotNumeric Then Me.setup.Log.AddError("Nodata-value for parameter " & ObjectPar & " at object ID " & ObjectID)
            If WarningIfNotNumeric Then Me.setup.Log.AddWarning("Nodata-value for parameter " & ObjectPar & " at object ID " & ObjectID)
            If ReplaceByNodataIfNotNumeric Then myValue = Double.NaN
            Return False
        ElseIf IsNumeric(Shapefile.CellValue(FieldIdx, RecordIdx)) Then
            myValue = Shapefile.CellValue(FieldIdx, RecordIdx)
            Return True
        Else
            If ErrorIfNotNumeric Then Me.setup.Log.AddError("Non-numeric value " & Shapefile.CellValue(FieldIdx, RecordIdx) & " for " & ObjectPar & " at object ID " & ObjectID)
            If WarningIfNotNumeric Then Me.setup.Log.AddWarning("Non-numeric value " & Shapefile.CellValue(FieldIdx, RecordIdx) & " for " & ObjectPar & " at object ID " & ObjectID)
            If ReplaceByNodataIfNotNumeric Then myValue = Double.NaN
            Return False
        End If
    End Function

    Public Function TruncateString(myString As String, MaxLength As Integer) As String
        If myString.Length > MaxLength Then myString = Left(myString, MaxLength)
        Return myString
    End Function

    Public Function EmptyFilesInDir(Dir As String, RegExSelection As String) As Boolean
        Try
            Dim myFiles As IO.FileInfo() = ListFilesInDir(Dir, RegExSelection)
            Dim fi As IO.FileInfo

            For Each fi In myFiles
                Dim myWriter As New StreamWriter(fi.FullName, False)
                myWriter.Write("")
                myWriter.Close()
                'strFileSize = (Math.Round(fi.Length / 1024)).ToString()
                'Console.WriteLine("File Name: {0}", fi.Name)
                'Console.WriteLine("File Full Name: {0}", fi.FullName)
                'Console.WriteLine("File Size (KB): {0}", strFileSize)
                'Console.WriteLine("File Extension: {0}", fi.Extension)
                'Console.WriteLine("Last Accessed: {0}", fi.LastAccessTime)
                'Console.WriteLine("Read Only: {0}", (fi.Attributes.ReadOnly = True).ToString)
            Next
            Me.setup.Log.AddMessage("Cleared " & myFiles.Count & " files.")
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ListFilesInDir(FilesDir As String, RegExSelection As String) As IO.FileInfo()
        Dim strFileSize As String = ""
        Dim di As New IO.DirectoryInfo(FilesDir)
        Dim aryFi As IO.FileInfo() = di.GetFiles(RegExSelection)
        Return aryFi

    End Function

    Public Function TextMatchFromExpressionsList(ByVal myExpressions As List(Of String), mytext As String, ignorecase As Boolean) As Boolean
        For Each myExpression As String In myExpressions
            If RegExMatch(myExpression, mytext, ignorecase) Then Return True
        Next
        Return False
    End Function



    Public Function PopulateComboBoxColumnWithSobekInputFiles(ByRef myComboBox As Windows.Forms.ComboBox) As Boolean
        Try
            myComboBox.Items.Clear()
            myComboBox.Items.Add("unpaved.3b")
            myComboBox.Items.Add("unpaved.alf")
            myComboBox.Items.Add("unpaved.sep")
            myComboBox.Items.Add("unpaved.inf")
            myComboBox.Items.Add("unpaved.tbl")
            myComboBox.Items.Add("paved.3b")
            myComboBox.Items.Add("paved.dwa")
            myComboBox.Items.Add("greenhse.3b")
            myComboBox.Items.Add("profile.dat")
            myComboBox.Items.Add("profile.def")
            myComboBox.Items.Add("struct.dat")
            myComboBox.Items.Add("struct.def")
            myComboBox.Items.Add("control.def")
            myComboBox.Items.Add("boundary.dat")
            myComboBox.Items.Add("lateral.dat")
            myComboBox.Items.Add("boundlat.dat")
            myComboBox.Items.Add("nodes.dat")
            myComboBox.Items.Add("friction.dat")
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function
    Public Function PolygonToPolyline(ByRef myShape As MapWinGIS.Shape) As MapWinGIS.Shape
        Try
            Dim newShape As New MapWinGIS.Shape
            newShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
            For i = 0 To myShape.numPoints - 1
                newShape.AddPoint(myShape.Point(i).x, myShape.Point(i).y)
            Next
            Return newShape
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function ForceNumeric(myValue As String, Optional ByVal Parname As String = "#not specified#", Optional ByVal ReplaceValueIfNotNumeric As Double = 0, Optional ByVal MessageType As enmMessageType = enmMessageType.Message, Optional ByVal Multiplier As Double = 1) As Double
        If IsNumeric(myValue) Then
            Return Val(myValue) * Multiplier
        Else
            'v1.900: made a distinction on three levels of feedback: error, warning and message
            Dim Msg As String = "Provided value for parameter '" & Parname & "' was not numeric and has been replaced by " & ReplaceValueIfNotNumeric
            Select Case MessageType
                Case enmMessageType.ErrorMessage
                    Me.setup.Log.AddError(Msg)
                Case enmMessageType.Warning
                    Me.setup.Log.AddWarning(Msg)
                Case enmMessageType.Message
                    Me.setup.Log.AddMessage(Msg)
            End Select
            Return ReplaceValueIfNotNumeric
        End If
    End Function

    Public Function replaceFileExtensionInPath(Path As String, ReplaceStr As String, NewStr As String) As String
        Return Left(Path, Path.Length - ReplaceStr.Length) & NewStr
    End Function

    Public Function DistanceToBoundingBox(ByRef X As Double, ByRef Y As Double, ByRef Xmin As Double, ByRef Ymin As Double, ByRef Xmax As Double, ByRef Ymax As Double) As Double
        'Siebe Bosch, 13 may 2019
        'this function calculates the shortes distance for a given point to a bounding box. If the point is inside the box, a value of 0 is returned
        Dim Xdist As Double, Ydist As Double
        Select Case X
            Case Is < Xmin
                Xdist = Xmin - X
            Case Is > Xmax
                Xdist = X - Xmax
            Case Else
                Xdist = 0
        End Select

        Select Case Y
            Case Is < Ymin
                Ydist = Ymin - Y
            Case Is > Ymax
                Ydist = Y - Ymax
            Case Else
                Ydist = 0
        End Select

        Return Math.Sqrt(Xdist ^ 2 + Ydist ^ 2)

    End Function

    Public Function MakeOdd(myNumber As Integer, RoundUp As Boolean) As Integer
        If IsEven(myNumber) Then
            If RoundUp Then Return myNumber + 1 Else Return myNumber - 1
        Else
            Return myNumber
        End If
    End Function

    Public Function makeCharacterSequence(myChars As String, nInstances As Integer)
        Dim i As Integer
        Dim myString As String = ""
        For i = 1 To nInstances
            myString &= myChars
        Next
        Return myString
    End Function

    Public Function SwapVariables(ByRef A As Double, ByRef B As Double) As Boolean
        Try
            A = A + B
            B = B + A
            A = B - A
            B = B - 2 * A
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GumbelPDF(Value As Double, mu As Double, sigma As Double, ByRef f As Double) As Boolean
        Try
            Dim z As Double
            z = Math.Exp(-(Value - mu) / sigma)
            f = 1 / sigma * z * Math.Exp(-z)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReadXMLNamedItemToBoolean(ByRef XmlNode As Xml.XmlNode, Itemname As String) As Boolean
        Try
            Return BooleanFromText(XmlNode.Attributes.GetNamedItem(Itemname).InnerText)
        Catch ex As Exception
            Me.setup.Log.AddWarning("Could not find named item " & Itemname & " in xmlnode " & XmlNode.Name)
            Return False
        End Try
    End Function
    Public Function ReadXMLNamedItemToString(ByRef XmlNode As Xml.XmlNode, Itemname As String) As String
        Try
            Return XmlNode.Attributes.GetNamedItem(Itemname).InnerText
        Catch ex As Exception
            Me.setup.Log.AddWarning("Could not find named item " & Itemname & " in xmlnode " & XmlNode.Name)
            Return ""
        End Try
    End Function

    Public Function readXMLAttribute(xmlNode As Xml.XmlNode, attrName As String, ByRef Value As String, Optional ByVal ToUpper As Boolean = False) As Boolean
        Try
            Dim myNode As Xml.XmlNode = xmlNode.Attributes.GetNamedItem(attrName)
            If myNode IsNot Nothing Then
                If ToUpper Then
                    Value = myNode.InnerText.ToUpper
                Else
                    Value = myNode.InnerText
                End If
            Else
                Return False
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function readXMLAttributeBOOL(xmlNode As Xml.XmlNode, attrName As String, ByRef Value As Boolean) As Boolean
        Try
            Dim myNode As Xml.XmlNode = xmlNode.Attributes.GetNamedItem(attrName)
            If myNode Is Nothing Then Throw New Exception()
            Value = BooleanFromText(myNode.InnerText)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub writeXMLElement(ByRef xmlWriter As StreamWriter, ElementID As String, ElementValue As String, leadingspaces As Integer, Optional ByVal comment As String = "")
        Dim mySpaces = makeCharacterSequence(" ", leadingspaces)
        If comment <> "" Then comment = "<!--" & comment & "-->"
        xmlWriter.WriteLine(mySpaces & "<" & ElementID & ">" & ElementValue & "</" & ElementID & ">" & comment)
    End Sub

    Public Function DivisionsByInteger(nVals As Double) As List(Of Integer)
        Dim Results As New List(Of Integer)
        For i = 1 To nVals
            If nVals / i = Convert.ToInt64(nVals / i) Then
                Results.Add(i)
            End If
        Next
        Return Results
    End Function

    Public Function writexmlOpeningTag(ByRef xmlWriter As StreamWriter, elementID As String, leadingspaces As Integer) As Boolean
        Dim mySpaces = makeCharacterSequence(" ", leadingspaces)
        Dim myStr As String
        Try
            myStr = mySpaces & "<" & elementID & ">"
            xmlWriter.WriteLine(myStr)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function writexmlclosingTag(ByRef xmlWriter As StreamWriter, elementID As String, leadingspaces As Integer) As Boolean
        Dim mySpaces = makeCharacterSequence(" ", leadingspaces)
        Dim myStr As String
        Try
            myStr = mySpaces & "</" & elementID & ">"
            xmlWriter.WriteLine(myStr)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function writeXMLSingleLineElement(ByRef xmlWriter As StreamWriter, ElementID As String, ElementVal As String, leadingspaces As Integer, Optional ByVal Comment As String = "") As Boolean
        Dim mySpaces = makeCharacterSequence(" ", leadingspaces)
        Dim myStr As String
        Try
            If Not ElementVal Is Nothing Then
                myStr = mySpaces & "<" & ElementID & ">" & ElementVal.Trim.ToLower & "</" & ElementID & ">"
            Else
                myStr = mySpaces & "<" & ElementID & "></" & ElementID & ">"
            End If
            If Comment <> "" Then myStr &= " <!--" & Comment & "-->"
            xmlWriter.WriteLine(myStr)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function writeXMLElementWithAttributes(ByRef xmlWriter As StreamWriter, ElementID As String, leadingspaces As Integer, attrList As List(Of String), valsList As List(Of String), Optional Comment As String = "") As Boolean
        Try
            Dim mySpaces = makeCharacterSequence(" ", leadingspaces)
            Dim myStr As String = "<" & ElementID & " "
            Dim i As Integer
            For i = 0 To attrList.Count - 1
                myStr &= attrList(i) & "=" & Chr(34) & valsList(i) & Chr(34) & " "
            Next

            'v1.797: removed this clause since the emergency pump selection datagridview has an extra column to allow importing timeseries
            'If attrList.Count <> valsList.Count Then Throw New Exception("Error: Number of attributes and number of values not equal for xml element " & ElementID)
            myStr = mySpaces & myStr.Trim & "/>"
            If Comment <> "" Then myStr &= "<!--" & Comment & "-->"
            xmlWriter.WriteLine(myStr)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function writeXMLOpeningTagWithAttributes(ByRef xmlWriter As StreamWriter, ElementID As String, leadingspaces As Integer, attrList As List(Of String), valsList As List(Of String), Optional Comment As String = "") As Boolean
        Try
            Dim mySpaces = makeCharacterSequence(" ", leadingspaces)
            Dim myStr As String = "<" & ElementID & " "
            Dim i As Integer
            For i = 0 To attrList.Count - 1
                myStr &= attrList(i) & "=" & Chr(34) & valsList(i) & Chr(34) & " "
            Next

            myStr = mySpaces & myStr.Trim & ">"
            If Comment <> "" Then myStr &= "<!--" & Comment & "-->"
            xmlWriter.WriteLine(myStr)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GumbelCDF(Value As Double, mu As Double, sigma As Double, ByRef F As Double) As Boolean
        Try
            F = Math.Exp(-Math.Exp(-(Value - mu) / sigma))
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function MonthNumberAD(Year As Integer, Month As Integer) As Integer
        'calculates the month number since 0 AD
        Return (Year - 1) * 12 + Month
    End Function

    Public Function GEVPDF(Value As Double, mu As Double, sigma As Double, kappa As Double, ByRef f As Double) As Boolean
        Try
            Dim z As Double

            Select Case kappa
                Case Is = 0
                    If Not GumbelPDF(Value, mu, sigma, f) Then Throw New Exception("Error in Generalized Extremes Values distribution.")
                Case Is <> 0
                    'make sure all values are in the valid domain
                    z = (Value - mu) / sigma
                    If (1 + kappa * z) <= 0 Then Return False
                    f = 1 / sigma * Math.Exp(-(1 + kappa * z) ^ (-1 / kappa)) * (1 + kappa * z) ^ (-1 - 1 / kappa)
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function GEVCDF(Value As Double, mu As Double, sigma As Double, kappa As Double, ByRef F As Double) As Boolean
        Try
            Dim t As Double
            Dim z As Double

            Select Case kappa
                Case Is = 0
                    If Not GumbelCDF(Value, mu, sigma, F) Then Throw New Exception("Error in function GEVCDF.")
                Case Is <> 0
                    z = (Value - mu) / sigma
                    t = (1 + kappa * z) ^ (-1 / kappa)
                    F = Math.Exp(-t)
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GenParetoPDF(Value As Double, LocPar As Double, ScalePar As Double, ShapePar As Double, ByRef f As Double) As Boolean
        Try
            Dim z As Double = (Value - LocPar) / ScalePar
            If ShapePar = 0 Then
                f = 1 / ScalePar * Math.Exp(-z)
            Else
                If ScalePar = 0 Then Return False                                           'prevent division by zero
                If (1 + ShapePar * z) < 0 Then Return False                                 'prevent raising a negative number to a power
                f = 1 / ScalePar * (1 + ShapePar * z) ^ (-1 - 1 / ShapePar)
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GenParetoCDF(Value As Double, mu As Double, sigma As Double, kappa As Double, ByRef F As Double) As Boolean
        Try
            Dim z As Double = (Value - mu) / sigma
            'returns the exceedance probability for a given Generalized Pareto Distribution
            If kappa = 0 Then
                F = 1 - Math.Exp(-(Value - mu) / sigma)                            'exceedance probability
            Else
                F = 1 - (1 + kappa * z) ^ (-1 / kappa)            'exceedance probability
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ExponentialPDF(Value As Double, LocPar As Double, ScalePar As Double, ByRef f As Double) As Boolean
        Try
            f = ScalePar * Math.Exp(-ScalePar * (Value - LocPar))
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ExponentialCDF(Value As Double, LocPar As Double, ScalePar As Double, ByRef F As Double) As Boolean
        Try
            F = 1 - Math.Exp(-ScalePar * (Value - LocPar))
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function LogNormalPDF(Value As Double, LocPar As Double, ScalePar As Double, ShapePar As Double, ByRef f As Double) As Boolean
        Dim teller As Double, noemer As Double
        Try
            teller = Math.Exp(-0.5 * ((Math.Log(Value - ShapePar) - LocPar) / ScalePar) ^ 2)
            noemer = (Value - ShapePar) * ScalePar * Math.Sqrt(2 * Math.PI)
            f = teller / noemer
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function LogNormalCDF(Value As Double, LocPar As Double, ScalePar As Double, ShapePar As Double, ByRef F As Double) As Boolean
        Try
            'F = laplaceIntegral((Math.Log(Value) - LocPar) / ScalePar)
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function



    Public Function Akaike(LogLikelihood As Double, nPars As Integer, ByRef AIC As Double) As Boolean
        'this function calculates the Akaike Information Criterion (AIC)
        'based on the Log-Likelihood and number of parameters of a given probability distribution
        'nPars = the number of parameters used in the probability distribution function (e.g. Gumbel has 2 (mu and sigma) and Gen.Pareto has 3 (mu, sigma and teta)
        Try
            AIC = -2 * LogLikelihood + 2 * nPars
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function NASH_SUTCLIFFE(Observed As List(Of Double), Computed As List(Of Double), ByRef NS As Double, Optional ByVal Log As Boolean = False) As Boolean

        Try
            Dim Sum As Double, sumLog As Double, AvgObserved As Double, AvgLogObserved As Double
            Dim sumTeller As Double, sumNoemer As Double
            Dim r As Long

            Sum = 0
            For r = 0 To Observed.Count - 1
                Sum += Observed(r)
                If Observed(r) > 0 Then sumLog = sumLog + Math.Log(Observed(r))
            Next

            'calculate the average
            If Observed.Count = 0 Then Return False ' Throw New Exception("No measured data found to compare computed data with. Please check from- and to-dates and time series with measured data.")
            If Observed.Count <> Computed.Count Then Throw New Exception("Number Of observed values should be equal to number Of computed values.")

            AvgObserved = Sum / Observed.Count
            AvgLogObserved = sumLog / Observed.Count

            For r = 0 To Observed.Count - 1
                If Not Log Then
                    sumTeller = sumTeller + (Observed(r) - Computed(r)) ^ 2
                    sumNoemer = sumNoemer + (Observed(r) - AvgObserved) ^ 2
                Else
                    sumTeller = sumTeller + (Observed(r) - Computed(r)) ^ 2
                    sumNoemer = sumNoemer + (Observed(r) - AvgLogObserved) ^ 2
                End If
            Next
            If sumNoemer = 0 Then Return False
            NS = 1 - (sumTeller / sumNoemer)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function RMSE_MEAN(Observed As List(Of Double), Computed As List(Of Double), ByRef RMSE As Double) As Boolean

        Try
            Dim sumTeller As Double, sumNoemer As Double
            Dim r As Long

            'calculate the average
            If Observed.Count = 0 Then Return False ' Throw New Exception("No measured data found to compare computed data with. Please check from- and to-dates and time series with measured data.")
            If Observed.Count <> Computed.Count Then Throw New Exception("Number Of observed values should be equal to number Of computed values.")

            For r = 0 To Observed.Count - 1
                sumTeller = sumTeller + (Observed(r) - Computed(r)) ^ 2
            Next
            sumNoemer = Observed.Count

            If sumNoemer = 0 Then Return False
            RMSE = (sumTeller / sumNoemer)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function Average(NumericArray As Double()) As Double
        Try
            Dim mySum As Double, n As Long = NumericArray.Count
            For i = 0 To NumericArray.Count - 1
                mySum += NumericArray(i)
            Next
            If n = 0 Then Throw New Exception("Cannot compute average from emtpy array")
            Return mySum / n
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function GumbelInverse(p_ond As Double, mu As Double, sigma As Double) As Double
        Try
            If p_ond <= 0 Then Throw New Exception
            Return mu - sigma * (Math.Log(-1 * Math.Log(p_ond)))
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    'Public Function FitGumbelByPP(ByRef myDict As Dictionary(Of Double, clsExtremeValue), startIdx As Integer, endIdx As Integer, ByRef mu As Double, ByRef sigma As Double) As Boolean
    '    Dim iter As Integer, idx As Integer
    '    Dim sum As Double, min As Double = 9.0E+99, max As Double = -9.0E+99
    '    Dim lmin As Double, lmax As Double, smin As Double, smax As Double
    '    Dim s As Double, l As Double 'scale and location par
    '    Dim NS As Double, NSBest As Double = -9.0E+99
    '    Dim observed As List(Of Double), computed As List(Of Double)

    '    Try
    '        'first get some statistics from the dataset so we know the limitations of our parameters
    '        For Each myVal As clsExtremeValue In myDict.Values
    '            sum += myVal.Value
    '            If myVal.Value < min Then min = myVal.Value
    '            If myVal.Value > max Then max = myVal.Value
    '        Next
    '        lmin = min
    '        lmax = max
    '        smin = 0
    '        smax = max - min

    '        For iter = 1 To 10

    '            'calculate the nash sutcliffe coefficient for each combination of given parameter values
    '            For i = 1 To 10
    '                l = lmin + (i - 0.5) * (lmax - lmin) / 10
    '                For j = 1 To 10
    '                    s = smin + (j - 0.5) * (smax - smin) / 10
    '                    observed = New List(Of Double)
    '                    computed = New List(Of Double)
    '                    For idx = startIdx To endIdx
    '                        observed.Add(myDict.Values(idx).Value)
    '                        computed.Add(GumbelInverse(1 - 1 / myDict.Values(idx).PPWeibull, l, s))
    '                    Next

    '                    NS = NASH_SUTCLIFFE(observed, computed, False)
    '                    If NS > NSBest Then
    '                        NSBest = NS
    '                        mu = l
    '                        sigma = s
    '                    End If
    '                Next
    '            Next

    '            'prepare the next iteration by reducting lmin, lmax, smin and smax
    '            lmin = mu - (lmax - lmin) / 10
    '            lmax = mu + (lmax - lmin) / 10
    '            smin = sigma - (smax - smin) / 10
    '            smax = sigma + (smax - smin) / 10

    '        Next
    '        Return True
    '    Catch ex As Exception
    '        Return False
    '    End Try


    'End Function


    Public Function FitGumbelByPP(ReturnPeriods As List(Of Double), Values As List(Of Double), LangbeinApplied As Boolean, ByRef LocPar As Double, ByRef ScalePar As Double, ByRef FitResult As Double) As Boolean
        Dim myiteration As Integer, myidx As Integer
        Dim mySum As Double, myMin As Double = 9000000000.0, myMax As Double = -9000000000.0
        Dim lmin As Double, lmax As Double, smin As Double, smax As Double
        Dim mySigma As Double, myMu As Double 'scale and location par
        Dim myRMSE As Double, RMSEBest As Double = 9000000000.0
        Dim observed As List(Of Double), computed As List(Of Double), mycomputed As Double, p_onderschrijding As Double

        Try
            'first get some statistics from the dataset so we know the limitations of our parameters
            For Each myVal As Double In Values
                mySum += myVal
                If myVal < myMin Then myMin = myVal
                If myVal > myMax Then myMax = myVal
            Next
            lmin = myMin - 3 * (myMax - myMin)
            lmax = myMax + 3 * (myMax - myMin)
            smin = 0
            smax = myMax - myMin

            For myiteration = 1 To 10

                'calculate the nash sutcliffe coefficient for each combination of given parameter values
                For i = 1 To 10
                    myMu = lmin + (i - 0.5) * (lmax - lmin) / 10
                    For j = 1 To 10
                        mySigma = smin + (j - 0.5) * (smax - smin) / 10
                        observed = New List(Of Double)
                        computed = New List(Of Double)
                        For myidx = 0 To Values.Count - 1
                            'if langbein was applied to the return periods, we'll need to transform them back first
                            If LangbeinApplied Then
                                p_onderschrijding = Math.E ^ (-1 / ReturnPeriods(myidx))
                            Else
                                p_onderschrijding = 1 - 1 / ReturnPeriods(myidx)
                            End If

                            If p_onderschrijding > 0 Then
                                mycomputed = GumbelInverse(p_onderschrijding, myMu, mySigma)
                                If Not mycomputed = Nothing Then
                                    observed.Add(Values(myidx))
                                    computed.Add(mycomputed)
                                End If
                            End If
                        Next

                        If Not RMSE_MEAN(observed, computed, myRMSE) Then Continue For
                        If myRMSE < RMSEBest Then
                            RMSEBest = myRMSE
                            LocPar = myMu
                            ScalePar = mySigma
                        End If
                    Next
                Next

                'prepare the next iteration by reducting lmin, lmax, smin and smax
                Dim lstep As Double = (lmax - lmin) / 10
                Dim sstep As Double = (smax - smin) / 10

                lmin = LocPar - lstep
                lmax = LocPar + lstep
                smin = ScalePar - sstep
                smax = ScalePar + sstep

                'since we're usually fitting waterlevels we'll accept a difference of max 1 mm
                If (lmax - lmin) < 0.001 Then Exit For

            Next
            FitResult = RMSEBest
            Return True
        Catch ex As Exception
            Return False
        End Try


    End Function

    Public Function StandardDeviation(NumericArray As Double()) As Double
        'Function takes an array with numeric elements as a parameter
        'and calcuates the standard deviation
        Try
            Dim dblSum As Double, dblSumSqdDevs As Double, dblMean As Double
            Dim lngCount As Long, dblAnswer As Double
            Dim vElement As Double
            Dim lngStartPoint As Long, lngEndPoint As Long, lngCtr As Long

            'if NumericArray is not an array, this statement will
            'raise an error in the errorhandler

            lngCount = UBound(NumericArray)
            lngCount = 0

            'the check below will allow
            'for 0 or 1 based arrays.

            vElement = NumericArray(0)
            lngStartPoint = IIf(Err.Number = 0, 0, 1)
            lngEndPoint = UBound(NumericArray)

            'get sum and sample size
            For lngCtr = lngStartPoint To lngEndPoint
                vElement = NumericArray(lngCtr)
                If IsNumeric(vElement) Then
                    lngCount = lngCount + 1
                    dblSum = dblSum + CDbl(vElement)
                End If
            Next

            'get mean
            If lngCount > 1 Then
                dblMean = dblSum / lngCount

                'get sum of squared deviations
                For lngCtr = lngStartPoint To lngEndPoint
                    vElement = NumericArray(lngCtr)

                    If IsNumeric(vElement) Then
                        dblSumSqdDevs = dblSumSqdDevs +
                ((vElement - dblMean) ^ 2)
                    End If
                Next

                'divide result by sample size - 1 and get square root. 
                'this function calculates standard deviation of a sample.  
                'If your  set of values represents the population, use sample
                'size not sample size - 1

                If lngCount > 1 Then
                    lngCount = lngCount - 1 'eliminate for population values
                    dblAnswer = Math.Sqrt(dblSumSqdDevs / lngCount)
                End If

            End If

            Return dblAnswer
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
        End Try

    End Function

    Public Function ShapeCenterOnLine(ByRef myShape As MapWinGIS.Shape, ID As String) As MapWinGIS.Point
        'v1.850: added the ID argument in order to better track errors
        Dim i As Integer, curDist As Double, nextDist As Double, targetDist As Double
        Dim newPoint As New MapWinGIS.Point

        Try
            Select Case myShape.numPoints
                Case Is = 0
                    Throw New Exception("No points in shape for object " & ID)
                Case Is = 1
                    Return myShape.Point(0)
                Case Is = 2
                    newPoint.x = (myShape.Point(0).x + myShape.Point(1).x) / 2
                    newPoint.y = (myShape.Point(0).y + myShape.Point(1).y) / 2
                    Return newPoint
                Case Else
                    targetDist = myShape.Length / 2
                    For i = 0 To myShape.numPoints - 2
                        nextDist += Pythagoras(myShape.Point(i + 1).x, myShape.Point(i + 1).y, myShape.Point(i).x, myShape.Point(i).y)
                        If nextDist > targetDist Then
                            newPoint.x = Interpolate(curDist, myShape.Point(i).x, nextDist, myShape.Point(i + 1).x, targetDist)
                            newPoint.y = Interpolate(curDist, myShape.Point(i).y, nextDist, myShape.Point(i + 1).y, targetDist)
                            Return newPoint
                        End If
                        curDist = nextDist
                    Next
            End Select
            Throw New Exception("Center on line not found for shape with ID " & ID & ".")
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function ShapeCenterOnLine of class GeneralFunctions while processing object " & ID & ".")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function


    Public Function WeibullPlottingPosition(Idx As Integer, n As Integer, Optional ByVal ApplyLangbein As Boolean = True) As Double
        Try
            If ApplyLangbein Then
                Dim Fi As Double = Idx / (n + 1)
                Return 1 / -Math.Log(1 - Fi)
            Else
                Return 1 / ((Idx) / (n + 1))
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GringortenPlottingPosition(Idx As Integer, n As Integer, Optional ByVal ApplyLangbein As Boolean = True) As Double
        Try
            If ApplyLangbein Then
                Dim Fi As Double = ((Idx - 0.44) / (n + 0.12))
                Return 1 / -Math.Log(1 - Fi)
            Else
                Return 1 / ((Idx - 0.44) / (n + 0.12))
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function BosLevenbachPlottingPosition(Idx As Integer, n As Integer, Optional ByVal ApplyLangbein As Boolean = True) As Double
        Try
            If ApplyLangbein Then
                Dim Fi As Double = ((Idx - 0.3) / (n + 0.4))
                Return 1 / -Math.Log(1 - Fi)
            Else
                Return 1 / ((Idx - 0.3) / (n + 0.4))
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function ProfileTypeSelected(myType As enmProfileType, YZ As Boolean, Tabulated As Boolean, Trapezium As Boolean, AsymTrapezium As Boolean, ClosedCircle As Boolean, ClosedRectangle As Boolean, EggShape As Boolean, Eggshape2 As Boolean, OpenCircle As Boolean, Sedredge As Boolean) As Boolean
        Try
            Select Case myType
                Case Is = enmProfileType.asymmetricaltrapezium
                    If AsymTrapezium Then Return True
                Case Is = enmProfileType.closedcircle
                    If ClosedCircle Then Return True
                Case Is = enmProfileType.closedrectangular
                    If ClosedRectangle Then Return True
                Case Is = enmProfileType.eggshape
                    If EggShape Then Return True
                Case Is = enmProfileType.eggshape2
                    If Eggshape2 Then Return True
                Case Is = enmProfileType.opencircle
                    If OpenCircle Then Return True
                Case Is = enmProfileType.sedredge
                    If Sedredge Then Return True
                Case Is = enmProfileType.tabulated
                    If Tabulated Then Return True
                Case Is = enmProfileType.trapezium
                    If Trapezium Then Return True
                Case Is = enmProfileType.yztable
                    If YZ Then Return True
            End Select
            Return False
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function ProfileTypeSelected of class clsGeneralFunctions.")
            Return False
        End Try

    End Function

    Public Function SeasonFromString(myName As String) As enmSeason
        Select Case myName.ToLower
            Case Is = "zomer", "summer", "zomerhalfjaar", "summerhalfyear"
                Return enmSeason.hydrosummerhalfyear
            Case Is = "winter", "winterhalfjaar", "winterhalfyear"
                Return enmSeason.hydrowinterhalfyear
            Case Is = "meteowinter"
                Return enmSeason.meteowinterhalfyear
            Case Is = "meeteosummer", "meteozomer"
                Return enmSeason.meteosummerhalfyear
        End Select

    End Function

    Public Sub CheckUncheckAllListboxItems(ByRef cmb As System.Windows.Forms.CheckedListBox, ByVal Checked As Boolean)
        Dim i As Integer
        For i = 0 To cmb.Items.Count - 1
            cmb.SetItemChecked(i, Checked)
        Next
    End Sub

    Public Function ValueFromString(ByVal txt As String, ConvertEmptyToZero As Boolean) As Double
        If txt.Trim = "" Then
            If ConvertEmptyToZero Then Return 0 Else Return Nothing
        ElseIf IsNumeric(txt.Trim) Then
            Return Val(txt.Trim)
        Else
            Return Nothing
        End If
    End Function

    Public Function AngleDifferenceDegrees(firstangle As Double, secondangle As Double) As Double
        Dim difference As Double = secondangle - firstangle
        Select Case difference
            Case Is < -180
                difference += 360
            Case Is > 180
                difference -= 360
        End Select
        If secondangle = firstangle Then
            Return 0
        Else
            Return (Math.Abs(difference))
        End If
    End Function

    Public Function ReplaceInvalidCharactersInPath(myPath As String, Optional ByVal ReplaceString As String = "_") As String
        While InStr(myPath, "/") > 0
            Me.setup.Log.AddWarning("File path " & myPath & " was slighly adjusted to prevent invalid characters in its path.")
            myPath = Replace(myPath, "/", ReplaceString)
        End While

        While InStr(3, myPath, ":") > 2
            Me.setup.Log.AddWarning("File path " & myPath & " was slightly adjusted to prevent invalid characters in its path.")
            myPath = Left(myPath, 2) & Replace(myPath, ":", ReplaceString, 3)
        End While

        While InStr(1, myPath, ";") > 0
            Me.setup.Log.AddWarning("File path " & myPath & " was slightly adjusted to prevent invalid characters in its path.")
            myPath = Left(myPath, 2) & Replace(myPath, ";", ReplaceString, 3)
        End While

        Return myPath
    End Function

    Public Function AddShapeFileToFileCollection(ByRef myCollection As Dictionary(Of String, String), ShpPath As String) As Boolean
        Try
            If Not System.IO.File.Exists(ShpPath) Then Throw New Exception("shapefile does not exist.")
            Dim shxPath As String = Replace(ShpPath, ".shp", ".shx",,, CompareMethod.Text)
            Dim dbfPath As String = Replace(ShpPath, ".shp", ".dbf",,, CompareMethod.Text)
            If Not myCollection.ContainsKey(ShpPath.Trim.ToUpper) Then myCollection.Add(ShpPath.Trim.ToUpper, ShpPath)
            If Not myCollection.ContainsKey(shxPath.Trim.ToUpper) Then myCollection.Add(shxPath.Trim.ToUpper, shxPath)
            If Not myCollection.ContainsKey(dbfPath.Trim.ToUpper) Then myCollection.Add(dbfPath.Trim.ToUpper, dbfPath)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function AddShapeFileToFileCollection while processing shapefile " & ShpPath & ":" & ex.Message)
            Return False
        End Try
    End Function

    Public Function SurroundWithCharacter(ByVal myString As String, ByVal myChar As String) As String
        Return myChar & myString & myChar
    End Function

    Public Function LineFromPoints(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As clsLineDefinition
        Dim myLine As New clsLineDefinition(Me.setup)
        myLine.Calculate(X1, Y1, X2, Y2)
        Return myLine
    End Function

    Public Function datatableContainsString(ByRef dt As DataTable, Value As String, ColIdx As Integer) As Boolean
        Dim i As Long
        For i = 0 To dt.Rows.Count - 1
            If dt.Rows(i)(ColIdx) = Value Then Return True
        Next
        Return False
    End Function

    Public Function dataTableCountStrings(ByRef dt As DataTable, Value As String, ColIdx As Integer) As Long

        Dim rows As DataRow()
        rows = dt.Select(dt.Columns(ColIdx).ColumnName & " = '" & Value & "'")
        Return rows.Length

        'Dim i As Long, n As Long = 0
        'For i = 0 To dt.Rows.Count - 1
        '    If dt.Rows(i)(ColIdx) = Value Then n += 1
        'Next
        'Return n
    End Function

    Public Function GetAddDataTableDoubleIdx(ByRef dt As DataTable, myVal As Double, colIdx As Integer) As Long
        '========================================================================================================
        ' this function searches a datatable for an existing record with a given value of type Double
        ' it will return the index number for the row where that date was found
        ' if not found, -1 is returned
        '========================================================================================================
        Dim i As Long

        'look for existing row
        For i = 0 To dt.Rows.Count - 1
            If dt.Rows(i)(colIdx) = myVal Then
                Return i
            End If
        Next

        'not found, so add it
        Dim dtRow As DataRow
        dtRow = dt.NewRow()
        dt.Rows.Add(dtRow)
        dt.Rows(dt.Rows.Count - 1)(colIdx) = myVal
        Return dt.Rows.Count - 1

    End Function

    Public Function TextFileToList(ByVal Path As String, ByRef Records As List(Of String)) As Boolean
        Try
            Using myReader As New StreamReader(Path)
                Records.Add(myReader.ReadLine)
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Function GetAddDataTableDateIdx(ByRef dt As DataTable, myDate As Date, colIdx As Integer) As Long
        '========================================================================================================
        ' this function searches a datatable for an existing record with a given date
        ' it will return the index number for the row where that date was found
        ' if not found, -1 is returned
        '========================================================================================================
        Dim tsIdx As Long
        Dim FirstDate As DateTime, SecondDate As DateTime, timestep As TimeSpan, span As TimeSpan

        'look for existing row with required date. assume equidistant table
        If dt.Rows.Count > 1 AndAlso dt.Rows(dt.Rows.Count - 1)(colIdx) >= myDate Then
            FirstDate = dt.Rows(0)(colIdx)
            SecondDate = dt.Rows(1)(colIdx)
            timestep = SecondDate.Subtract(FirstDate)
            span = myDate.Subtract(FirstDate)

            'make sure the integer stays within bounds
            If timestep.TotalSeconds >= 3600 Then
                tsIdx = Math.Max(0, span.TotalHours / timestep.TotalHours)
            ElseIf timestep.TotalSeconds >= 60 Then
                tsIdx = Math.Max(0, span.TotalMinutes / timestep.TotalMinutes)
            Else
                tsIdx = Math.Max(0, span.TotalSeconds / timestep.TotalSeconds)
            End If

            If tsIdx < dt.Rows.Count AndAlso dt.Rows(tsIdx)(colIdx) = myDate Then
                Return tsIdx
            End If

        End If

        'not found, so add it
        Dim dtRow As DataRow
        dtRow = dt.NewRow()
        dt.Rows.Add(dtRow)
        dt.Rows(dt.Rows.Count - 1)(colIdx) = myDate
        Return dt.Rows.Count - 1

    End Function


    'Public Function DataTableStats(ByRef myTable As DataTable, query As String, ByRef myFirst As Double, ByRef myLast As Double, PercentileIncrement As Integer) As Dictionary(Of Integer, Double)
    '    Dim myStats As New Dictionary(Of Integer, Double)
    '    Dim rowIdx As Long, i As Long


    '    Dim foundRows() As DataRow
    '    foundRows = myTable.Select(query)

    '    'first retrieve the first and last values in the table
    '    myFirst = foundRows.GetValue(0)(1)
    '    myLast = foundRows(foundRows.Count - 1)(1)



    '    'Dim newList As New List(Of Double)
    '    'For i = 0 To myTable.Rows.Count - 1
    '    '    newList.Add(myTable.Rows(i)(ColIdx))
    '    'Next

    '    'For i = 0 To 100 Step PercentileIncrement
    '    '    rowIdx = i / 100 * (newList.Count - 1)
    '    '    If rowIdx < 0 Then rowIdx = 0
    '    '    If rowIdx > newList.Count - 1 Then rowIdx = newList.Count - 1
    '    '    myStats.Add(i, newList(rowIdx))
    '    'Next

    '    'create a derivative datatable that is sorted by the values in the column of interest
    '    'Dim ColName As String = myTable.Columns.Item(ColIdx).ColumnName
    '    'Dim dataView As New DataView(myTable)
    '    'dataView.Sort = ColName & " ASC"
    '    'Dim dataTable As DataTable = dataView.ToTable()

    '    'For i = 0 To 100 Step PercentileIncrement
    '    '    rowIdx = i / 100 * (dataTable.Rows.Count - 1)
    '    '    If rowIdx < 0 Then rowIdx = 0
    '    '    If rowIdx > dataTable.Rows.Count - 1 Then rowIdx = dataTable.Rows.Count - 1
    '    '    myStats.Add(i, dataTable.Rows(rowIdx)(ColIdx))
    '    'Next
    '    Return myStats
    'End Function

    Public Function DataTableStats(ByRef myTable As DataTable, StartIdx As Integer, EndIdx As Integer, ColIdx As Integer, ByRef myFirst As Double, ByRef myLast As Double, ByVal getMin As Boolean, ByRef myMin As Double, ByVal getMax As Double, ByRef myMax As Double, ByVal calcPercentiles As Boolean, ByVal PercentileIncrement As Integer) As Dictionary(Of Integer, Double)
        Dim myStats As New Dictionary(Of Integer, Double)
        Dim rowIdx As Long, i As Long

        StartIdx = Math.Max(0, StartIdx)
        EndIdx = Math.Min(EndIdx, myTable.Rows.Count - 1)

        'first retrieve the first and last values in the table
        myFirst = myTable.Rows(StartIdx)(ColIdx)
        myLast = myTable.Rows(EndIdx)(ColIdx)

        If getMin OrElse getMax OrElse calcPercentiles Then
            Dim newList As New List(Of Double)
            For i = StartIdx To EndIdx
                newList.Add(myTable.Rows(i)(ColIdx))
            Next

            newList.Sort()

            If getMin Then myMin = newList(0)
            If getMax Then myMax = newList(newList.Count - 1)

            If calcPercentiles Then
                If PercentileIncrement > 0 Then
                    For i = 0 To 100 Step PercentileIncrement
                        rowIdx = i / 100 * (newList.Count - 1)
                        If rowIdx < 0 Then rowIdx = 0
                        If rowIdx > newList.Count - 1 Then rowIdx = newList.Count - 1
                        myStats.Add(i, newList(rowIdx))
                    Next
                End If
            End If
        End If

        Return myStats
    End Function

    Public Function SinglesListStats(ByVal myList As List(Of Single), StartIdx As Integer, EndIdx As Integer, ByRef myFirst As Double, ByRef myLast As Double, ByRef myMin As Double, ByRef myMax As Double, ByVal calcPercentiles As Boolean, ByVal PercentileIncrement As Integer, ByRef Stats As Dictionary(Of Integer, Double)) As Boolean
        Try
            Stats = New Dictionary(Of Integer, Double)
            Dim rowIdx As Long, i As Long

            StartIdx = Math.Max(0, StartIdx)
            EndIdx = Math.Min(EndIdx, myList.Count - 1)

            'first retrieve the first and last values in the table
            myFirst = myList(StartIdx)
            myLast = myList(EndIdx)

            myList.Sort()

            myMin = myList(0)
            myMax = myList(myList.Count - 1)

            If calcPercentiles Then
                If PercentileIncrement > 0 Then
                    For i = 0 To 100 Step PercentileIncrement
                        rowIdx = i / 100 * (myList.Count - 1)
                        If rowIdx < 0 Then rowIdx = 0
                        If rowIdx > myList.Count - 1 Then rowIdx = myList.Count - 1
                        Stats.Add(i, myList(rowIdx))
                    Next
                End If
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub DataTableMinMax(ByRef myTable As DataTable, Columns As List(Of Integer), ByRef minY As Double, ByRef maxY As Double)
        Dim i As Long, j As Integer
        minY = 9000000000.0
        maxY = -9000000000.0
        For i = 0 To myTable.Rows.Count - 1
            For j = 0 To Columns.Count - 1
                If myTable.Rows(i)(Columns(j)) < minY Then minY = myTable.Rows(i)(Columns(j))
                If myTable.Rows(i)(Columns(j)) > maxY Then maxY = myTable.Rows(i)(Columns(j))
            Next
        Next
    End Sub

    Public Function DataTableMax(ByRef myTable As DataTable, Columns As List(Of Integer)) As Double
        Dim i As Long, j As Integer
        Dim maxY As Double = -9000000000.0
        For i = 0 To myTable.Rows.Count - 1
            For j = 0 To Columns.Count - 1
                If Not IsDBNull(myTable.Rows(i)(Columns(j))) AndAlso myTable.Rows(i)(Columns(j)) > maxY Then maxY = myTable.Rows(i)(Columns(j))
            Next
        Next
        Return maxY
    End Function

    Public Function DataTableMin(ByRef myTable As DataTable, Columns As List(Of Integer)) As Double
        Dim i As Long, j As Integer
        Dim minY As Double = 9000000000.0
        For i = 0 To myTable.Rows.Count - 1
            For j = 0 To Columns.Count - 1
                If Not IsDBNull(myTable.Rows(i)(Columns(j))) AndAlso myTable.Rows(i)(Columns(j)) < minY Then minY = myTable.Rows(i)(Columns(j))
            Next
        Next
        Return minY
    End Function

    Public Function DataTableAvg(ByRef myTable As DataTable, Columns As List(Of Integer)) As Double
        Try
            Dim i As Long, j As Integer
            Dim n As Long, myValue As Double
            For i = 0 To myTable.Rows.Count - 1
                For j = 0 To Columns.Count - 1
                    n += 1
                    myValue += myTable.Rows(i)(Columns(j))
                Next
            Next
            If n > 0 Then
                Return myValue / n
            Else
                Throw New Exception("Error retrieving average value from datatable. Value of zero was returned.")
            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return 0
        End Try
    End Function

    Public Function DataTableLeftCensoring(ByRef myTable As DataTable, ColumnIdx As Integer, Percentage As Double) As Boolean
        Try
            Dim Columns As New List(Of Integer)
            Columns.Add(ColumnIdx)
            Dim CensorVal As Double = DataTablePercentile(myTable, Columns, Percentage / 100)
            For i = myTable.Rows.Count - 1 To 0 Step -1
                If myTable.Rows(i)(ColumnIdx) <= CensorVal Then
                    myTable.Rows(i).Delete()
                End If
            Next
            myTable.AcceptChanges()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function DataTablePercentile(ByRef myTable As DataTable, Columns As List(Of Integer), Percentile As Double) As Double
        Try
            Dim i As Long, j As Integer
            Dim newTable As New List(Of Double)
            For i = 0 To myTable.Rows.Count - 1
                For j = 0 To Columns.Count - 1
                    newTable.Add(myTable.Rows(i)(Columns(j)))
                Next
            Next
            Return setup.GeneralFunctions.PercentileFromList(newTable, Percentile)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return 0
        End Try
    End Function


    Public Function DataTableMinMaxAsList(myTable As DataTable, Columns As List(Of Integer), ByRef minList As List(Of Double), ByRef maxList As List(Of Double), OrderAscending As Boolean, Optional ByVal AcceptDoubleValues As Boolean = True) As Boolean
        'this function retrieves the minimum and maximum values from each specified column of a datatable
        'and sorts them in ascending order
        Try
            Dim r As Integer, c As Integer
            Dim myMin As Double, myMax As Double
            For c = 0 To Columns.Count - 1
                myMin = 9.0E+99
                myMax = -9.0E+99
                For r = 0 To myTable.Rows.Count - 1
                    If myTable.Rows(r)(Columns(c)) < myMin Then myMin = myTable.Rows(r)(Columns(c))
                    If myTable.Rows(r)(Columns(c)) > myMax Then myMax = myTable.Rows(r)(Columns(c))
                Next
                minList.Add(myMin)
                maxList.Add(myMax)
            Next

            If OrderAscending Then
                minList.Sort()
                maxList.Sort()
                'Else
                '    minList.Reverse()
                '    maxList.Reverse()
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetDataTableTimeseriesIntersection(ByRef dt1 As DataTable, dt1ValColName As String, dt1DateColIdx As Integer, dt1ValColIdx As Integer, ByRef dt2 As DataTable, dt2ValColName As String, dt2DateColIdx As Integer, dt2ValColIdx As Integer) As DataTable
        Dim newTable As New DataTable
        Dim r1 As Integer, r2 As Integer
        newTable.Columns.Add("DATUM", Type.GetType("System.DateTime"))
        newTable.Columns.Add(dt1ValColName, Type.GetType("System.Single"))
        newTable.Columns.Add(dt2ValColName, Type.GetType("System.Single"))

        'try to reduce the search
        Dim dt1StartRowIdx As Integer = 0, dt1EndRowIdx As Integer = dt1.Rows.Count - 1
        Dim dt2StartRowIdx As Integer = 0, dt2EndRowIdx As Integer = dt2.Rows.Count - 1

        For r1 = 0 To dt1.Rows.Count - 1
            If dt1.Rows(r1)(dt1DateColIdx) >= dt2.Rows(0)(dt2DateColIdx) Then
                dt1StartRowIdx = r1
                Exit For
            End If
        Next

        For r1 = 0 To dt1.Rows.Count - 1
            If dt1.Rows(r1)(dt1DateColIdx) <= dt2.Rows(dt2.Rows.Count - 1)(dt2DateColIdx) Then
                dt1EndRowIdx = r1
            Else
                Exit For
            End If
        Next

        For r2 = 0 To dt2.Rows.Count - 1
            If dt2.Rows(r2)(dt2DateColIdx) > dt1.Rows(0)(dt1DateColIdx) Then
                dt2StartRowIdx = r2
                Exit For
            End If
        Next

        For r2 = 0 To dt2.Rows.Count - 1
            If dt2.Rows(r2)(dt1DateColIdx) <= dt1.Rows(dt1.Rows.Count - 1)(dt1DateColIdx) Then
                dt2EndRowIdx = r2
            Else
                Exit For
            End If
        Next

        Dim Done As Boolean
        r1 = dt1StartRowIdx
        r2 = dt2StartRowIdx
        Dim dt1CurDate As DateTime, dt1NextDate As DateTime
        Dim dt2curdate As DateTime, dt2NextDate As DateTime

        Me.setup.GeneralFunctions.UpdateProgressBar("Intersecting datatables", 0, 10, True)
        While Not Done
            Me.setup.GeneralFunctions.UpdateProgressBar("", r1 - dt1StartRowIdx, dt1EndRowIdx - dt1StartRowIdx)
            dt1CurDate = dt1.Rows(r1)(dt1DateColIdx)
            dt2curdate = dt2.Rows(r2)(dt2DateColIdx)
            dt1NextDate = dt1.Rows(r1 + 1)(dt1DateColIdx)
            dt2NextDate = dt2.Rows(r2 + 1)(dt2DateColIdx)

            If dt1NextDate < dt2curdate Then
                r1 += 1
            ElseIf dt1NextDate = dt2curdate Then
                r1 += 1
                If Not IsDBNull(dt1.Rows(r1)(dt1DateColIdx)) AndAlso Not IsDBNull(dt2.Rows(r2)(dt2DateColIdx)) Then
                    newTable.Rows.Add(dt1NextDate, dt1.Rows(r1)(dt1ValColIdx), dt2.Rows(r2)(dt2ValColIdx))
                End If
            ElseIf dt2NextDate < dt1CurDate Then
                r2 += 1
            ElseIf dt2NextDate = dt1CurDate Then
                r2 += 1
                If Not IsDBNull(dt1.Rows(r1)(dt1DateColIdx)) AndAlso Not IsDBNull(dt2.Rows(r2)(dt2DateColIdx)) Then
                    newTable.Rows.Add(dt1NextDate, dt1.Rows(r1)(dt1ValColIdx), dt2.Rows(r2)(dt2ValColIdx))
                End If
            ElseIf dt1NextDate > dt2curdate Then
                r1 += 1
            ElseIf dt2NextDate > dt1CurDate Then
                r2 += 1
            End If
            If r1 >= dt1EndRowIdx Then Done = True
            If r2 >= dt2EndRowIdx Then Done = True
        End While

        'Me.setup.GeneralFunctions.UpdateProgressBar("Intersecting datatables", 0, 10, True)
        'For r1 = dt1StartRowIdx To dt1EndRowIdx
        '    Me.setup.GeneralFunctions.UpdateProgressBar("", r1, dt1.Rows.Count)
        '    For r2 = dt2StartRowIdx To dt2EndRowIdx
        '        If dt1.Rows(r1)(dt1DateColIdx) = dt2.Rows(r2)(dt2DateColIdx) Then
        '            newTable.Rows.Add(dt1.Rows(r1)(dt1DateColIdx), dt1.Rows(r1)(dt1ValColIdx), dt2.Rows(r2)(dt2ValColIdx))
        '            Exit For
        '        End If
        '    Next
        'Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Intersection complete.", 0, 10, True)
        Return newTable
    End Function

    Public Function GetDataTableIntersection(ByRef dt1 As DataTable, dt1ValColName As String, dt1CompareColIdx As Integer, dt1ValColIdx As Integer, ByRef dt2 As DataTable, dt2ValColName As String, dt2CompareColIdx As Integer, dt2ValColIdx As Integer) As DataTable
        Dim newTable As New DataTable
        Dim r1 As Integer, r2 As Integer
        newTable.Columns.Add("COMPARE", Type.GetType("System.String"))
        newTable.Columns.Add(dt1ValColName, Type.GetType("System.Single"))
        newTable.Columns.Add(dt2ValColName, Type.GetType("System.Single"))

        'try to reduce the search
        Dim dt1StartRowIdx As Integer = 0, dt1EndRowIdx As Integer = dt1.Rows.Count - 1
        Dim dt2StartRowIdx As Integer = 0, dt2EndRowIdx As Integer = dt2.Rows.Count - 1

        For r1 = 0 To dt1.Rows.Count - 1
            If dt1.Rows(r1)(dt1CompareColIdx) >= dt2.Rows(0)(dt2CompareColIdx) Then
                dt1StartRowIdx = r1
                Exit For
            End If
        Next

        For r1 = 0 To dt1.Rows.Count - 1
            If dt1.Rows(r1)(dt1CompareColIdx) <= dt2.Rows(dt2.Rows.Count - 1)(dt2CompareColIdx) Then
                dt1EndRowIdx = r1
            Else
                Exit For
            End If
        Next

        For r2 = 0 To dt2.Rows.Count - 1
            If dt2.Rows(r2)(dt2CompareColIdx) > dt1.Rows(0)(dt1CompareColIdx) Then
                dt2StartRowIdx = r2
                Exit For
            End If
        Next

        For r2 = 0 To dt2.Rows.Count - 1
            If dt2.Rows(r2)(dt1CompareColIdx) <= dt1.Rows(dt1.Rows.Count - 1)(dt1CompareColIdx) Then
                dt2EndRowIdx = r2
            Else
                Exit For
            End If
        Next

        Dim Done As Boolean
        r1 = dt1StartRowIdx
        r2 = dt2StartRowIdx
        Dim dt1Cur As Object, dt1Next As Object
        Dim dt2cur As Object, dt2Next As Object

        Me.setup.GeneralFunctions.UpdateProgressBar("Intersecting datatables", 0, 10, True)
        While Not Done
            Me.setup.GeneralFunctions.UpdateProgressBar("", r1 - dt1StartRowIdx, dt1EndRowIdx - dt1StartRowIdx)
            dt1Cur = dt1.Rows(r1)(dt1CompareColIdx)
            dt2cur = dt2.Rows(r2)(dt2CompareColIdx)
            dt1Next = dt1.Rows(r1 + 1)(dt1CompareColIdx)
            dt2Next = dt2.Rows(r2 + 1)(dt2CompareColIdx)

            If dt1Next < dt2cur Then
                r1 += 1
            ElseIf dt1Next = dt2cur Then
                r1 += 1
                If Not IsDBNull(dt1.Rows(r1)(dt1CompareColIdx)) AndAlso Not IsDBNull(dt2.Rows(r2)(dt2CompareColIdx)) Then
                    newTable.Rows.Add(dt1Next, dt1.Rows(r1)(dt1ValColIdx), dt2.Rows(r2)(dt2ValColIdx))
                End If
            ElseIf dt2Next < dt1Cur Then
                r2 += 1
            ElseIf dt2Next = dt1Cur Then
                r2 += 1
                If Not IsDBNull(dt1.Rows(r1)(dt1CompareColIdx)) AndAlso Not IsDBNull(dt2.Rows(r2)(dt2CompareColIdx)) Then
                    newTable.Rows.Add(dt1Next, dt1.Rows(r1)(dt1ValColIdx), dt2.Rows(r2)(dt2ValColIdx))
                End If
            ElseIf dt1Next > dt2cur Then
                r1 += 1
            ElseIf dt2Next > dt1Cur Then
                r2 += 1
            End If
            If r1 >= dt1EndRowIdx Then Done = True
            If r2 >= dt2EndRowIdx Then Done = True
        End While

        Me.setup.GeneralFunctions.UpdateProgressBar("Intersection complete.", 0, 10, True)
        Return newTable
    End Function

    Public Function DatatableCumulate(ByVal dt As DataTable, DateColIdx As Integer, ValColIdx As Integer, ByVal StartRowIdx As Integer, ByVal EndRowIdx As Integer, assumeEquidistant As Boolean, multiplyByNumberOfSecondsPerTimestep As Boolean) As DataTable
        Dim newTable As New DataTable, row As DataRow = Nothing
        Dim ts As TimeSpan, startDate As DateTime, nextDate As DateTime, Multiplier As Double
        newTable = dt.Clone
        Dim CumVal As Double = 0
        Dim NonEquidistant As Boolean = False

        If multiplyByNumberOfSecondsPerTimestep Then
            If assumeEquidistant Then
                startDate = Convert.ToDateTime(dt.Rows(StartRowIdx)(DateColIdx))
                nextDate = Convert.ToDateTime(dt.Rows(StartRowIdx + 1)(DateColIdx))
                ts = nextDate.Subtract(startDate)
                Multiplier = ts.TotalSeconds
            Else
                NonEquidistant = True
            End If
        Else
            'als er niet vermenigvuldigd hoeft te worden, is een check op equidistantie ook niet nodig
            Multiplier = 1
        End If

        Me.setup.GeneralFunctions.UpdateProgressBar("Cumulating datatable...", 0, 10)
        For i = StartRowIdx To EndRowIdx - 1

            'compute the cumulative value
            Me.setup.GeneralFunctions.UpdateProgressBar("", i - StartRowIdx, EndRowIdx - StartRowIdx)
            If NonEquidistant Then
                Multiplier = Convert.ToDateTime(dt.Rows(i + 1)(DateColIdx)).Subtract(Convert.ToDateTime(dt.Rows(i)(DateColIdx))).TotalSeconds
                If Not IsDBNull(dt.Rows(i)(ValColIdx)) Then CumVal += dt.Rows(i)(ValColIdx) * Multiplier
            Else
                If Not IsDBNull(dt.Rows(i)(ValColIdx)) Then CumVal += dt.Rows(i)(ValColIdx) * Multiplier
            End If

            row = newTable.NewRow
            row.BeginEdit()
            For j = 0 To dt.Columns.Count - 1
                If j = ValColIdx Then
                    row(j) = CumVal
                Else
                    row(j) = dt.Rows(i)(j)
                End If
            Next
            row.EndEdit()
            newTable.Rows.Add(row)
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Complete.", 0, 10, True)
        Return newTable
    End Function

    Public Function DataTablesGetOverlapIdx(ByRef dt1 As DataTable, ByVal dt1ColIdx As Integer, ByRef dt1StartIdx As Integer, ByRef dt1EndIdx As Integer, ByRef dt2 As DataTable, ByVal dt2ColIdx As Integer, ByRef dt2StartIdx As Integer, ByRef dt2EndIdx As Integer) As Boolean
        Try
            Dim dt1StartVal As Object = dt1.Rows(0)(dt1ColIdx)
            Dim dt1EndVal As Object = dt1.Rows(dt1.Rows.Count - 1)(dt1ColIdx)
            Dim dt2StartVal As Object = dt2.Rows(0)(dt2ColIdx)
            Dim dt2EndVal As Object = dt2.Rows(dt2.Rows.Count - 1)(dt2ColIdx)
            Dim i1 As Integer

            If dt2StartVal > dt1EndVal OrElse dt2EndVal < dt1StartVal Then
                Throw New Exception("Error: no overlap found between datatables.")
            ElseIf dt2StartVal >= dt1StartVal Then
                dt2StartIdx = 0
                For i1 = 0 To dt1.Rows.Count - 1
                    If dt1.Rows(i1)(dt1ColIdx) >= dt2StartVal Then
                        dt1StartIdx = i1
                        Exit For
                    End If
                Next
            ElseIf dt2StartVal < dt1StartVal Then
                dt1StartIdx = 0
                For i2 = 0 To dt2.Rows.Count - 1
                    If dt2.Rows(i2)(dt2ColIdx) >= dt1StartVal Then
                        dt2StartIdx = i2
                        Exit For
                    End If
                Next
            End If

            If dt1EndVal >= dt2EndVal Then
                dt2EndIdx = dt2.Rows.Count - 1
                For i1 = dt1.Rows.Count - 1 To 0 Step -1
                    If dt1.Rows(i1)(dt1ColIdx) <= dt2EndVal Then
                        dt1EndIdx = i1
                        Exit For
                    End If
                Next
            ElseIf dt2EndVal >= dt1EndVal Then
                dt1EndIdx = dt1.Rows.Count - 1
                For i2 = dt2.Rows.Count - 1 To 0 Step -1
                    If dt2.Rows(i2)(dt2ColIdx) <= dt1EndVal Then
                        dt2EndIdx = i2
                        Exit For
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
        End Try
    End Function

    Public Function DataTableAnnualMax(ByVal dt As DataTable, DateColIdx As Integer, ValColIdx As Integer, StartIdx As Integer, EndIdx As Integer) As DataTable
        Dim maxdt As New DataTable
        Dim i As Long, curYear As Integer, lastYear As Integer = -999, curMax As Double
        maxdt.Columns.Add(New DataColumn("Year", Type.GetType("System.Int16")))
        maxdt.Columns.Add(New DataColumn("Maximum", Type.GetType("System.Double")))

        For i = StartIdx To EndIdx
            curYear = Year(dt.Rows(i)(DateColIdx))
            If curYear <> lastYear Then
                'store the previous year's maximum before proceeding
                If lastYear > -999 Then
                    maxdt.Rows.Add(lastYear, curMax)
                End If
                'now set the last year to be the current year and initialize the maximum for the current year
                lastYear = curYear
                curMax = dt.Rows(i)(ValColIdx)
            ElseIf dt.Rows(i)(ValColIdx) > curMax Then
                curMax = dt.Rows(i)(ValColIdx)
            End If
        Next
        Return maxdt
    End Function

    Public Function AdjustSobekInputFile(inputfile As String, token As String, tokennum As Integer, Operation As STOCHLIB.GeneralFunctions.EnmParameterOperation, OperationVal As Object) As List(Of String)
        'this function adjusts the value of a given token in a given sobek input file
        Dim myRecord As String, myToken As String, myVal As Object, newRecord As String
        Dim AdjustedFile As New List(Of String)
        Using myInputFile As New StreamReader(inputfile)
            While Not myInputFile.EndOfStream
                myRecord = myInputFile.ReadLine
                newRecord = ""
                While Not myRecord = ""
                    myToken = ParseString(myRecord, " ", 1)
                    If myToken = token Then
                        'token found
                        newRecord &= myToken & " "
                        For i = 1 To tokennum
                            myVal = ParseString(myRecord, " ", 1)
                            If i < tokennum Then
                                newRecord &= myVal & " "
                            Else
                                'value found. Finally apply the operation
                                Select Case Operation
                                    Case Is = EnmParameterOperation.Add
                                        myVal += OperationVal
                                    Case Is = EnmParameterOperation.Multiply
                                        myVal *= OperationVal
                                    Case Is = EnmParameterOperation.Replace
                                        myVal = OperationVal
                                    Case Is = EnmParameterOperation.WrapToken
                                        myVal = "<" & OperationVal & ">" & myVal & "</" & OperationVal & ">"
                                End Select
                                newRecord &= myVal & " "
                            End If
                        Next
                    Else
                        newRecord &= myToken & " "
                    End If
                End While
                AdjustedFile.Add(newRecord)
            End While
        End Using
        Return AdjustedFile
    End Function
    Public Function DataTableMonthlyAvg(ByVal dt As DataTable, DateColIdx As Integer, ValColIdx As Integer, StartIdx As Integer, EndIdx As Integer) As DataTable
        Dim avgdt As New DataTable, nPoints As Integer
        Dim i As Long, curYear As Integer, lastYear As Integer = -999, curAvg As Double
        Dim curMonth As Integer, lastMonth As Integer = -999
        avgdt.Columns.Add(New DataColumn("Year-Month", Type.GetType("System.Int16")))
        avgdt.Columns.Add(New DataColumn("Maximum", Type.GetType("System.Double")))

        For i = StartIdx To EndIdx
            curYear = Year(dt.Rows(i)(DateColIdx))
            curMonth = Month(dt.Rows(i)(DateColIdx))
            If Not IsDBNull(dt.Rows(i)(ValColIdx)) Then
                If curMonth <> lastMonth Then
                    'store the previous year/month's average before proceeding
                    If lastYear > -999 AndAlso lastMonth > -999 Then
                        'avgdt.Rows.Add(lastYear & "_" & Format(lastMonth, "00"), curAvg)
                        'avgdt.Rows.Add(Convert.ToInt64(lastYear & Format(lastMonth, "00")), curAvg)
                        If nPoints > 0 Then avgdt.Rows.Add(Me.setup.GeneralFunctions.MonthNumberAD(lastYear, lastMonth), curAvg / nPoints)
                    End If
                    'now set the last year/month to be the current year/month and initialize the average for the current year
                    lastYear = curYear
                    lastMonth = curMonth
                    curAvg = dt.Rows(i)(ValColIdx)
                    nPoints = 1
                Else
                    curAvg += dt.Rows(i)(ValColIdx)
                    nPoints += 1
                End If
            End If
        Next
        Return avgdt
    End Function

    Public Function DataTableTimeseriesLowerPercentile(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, ByRef StartIdx As Integer, ByRef EndIdx As Integer, myPercentile As Double) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long, valsList As New List(Of Double), PercentileVal As Double

        If StartIdx < 0 Then
            StartIdx = 0
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": non-existent record index " & StartIdx & ". Timespan was adjusted.")
        End If
        If EndIdx >= dt.Rows.Count Then
            EndIdx = dt.Rows.Count - 1
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": non-existent record index " & EndIdx & ". Timespan was adjusted.")
        End If

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Maximum", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'determine the value that corresponds with the requested percentile
        For i = StartIdx To EndIdx
            If Not IsDBNull(dt.Rows(i)(valColIdx)) Then valsList.Add(dt.Rows(i)(valColIdx))
        Next
        PercentileVal = Me.setup.GeneralFunctions.PercentileFromList(valsList, myPercentile)

        'now that we have the percentile value, filter the dataset
        Me.setup.GeneralFunctions.UpdateProgressBar("Computing lower percentile for timeseries...", 0, 10)
        For i = StartIdx To EndIdx
            Me.setup.GeneralFunctions.UpdateProgressBar("", i - StartIdx, EndIdx - StartIdx)
            If Not IsDBNull(dt.Rows(i)(valColIdx)) AndAlso dt.Rows(i)(valColIdx) <= PercentileVal Then
                resultsdt.Rows.Add(dt.Rows(i)(dateColIdx), dt.Rows(i)(valColIdx))
            Else
                resultsdt.Rows.Add(dt.Rows(i)(dateColIdx), DBNull.Value)
            End If
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete", 0, 10)

        Return resultsdt
    End Function

    Public Function DataTableTimeseriesUpperPercentile(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, StartIdx As Integer, EndIdx As Integer, myPercentile As Double) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long, valsList As New List(Of Double), PercentileVal As Double

        If StartIdx < 0 Then
            StartIdx = 0
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": invalid record index " & StartIdx & ".")
        End If
        If EndIdx >= dt.Rows.Count Then
            EndIdx = dt.Rows.Count - 1
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": non-existent record index " & EndIdx & ". Timespan was adjusted.")
        End If

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Maximum", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'determine the value that corresponds with the requested percentile
        For i = StartIdx To EndIdx
            If Not IsDBNull(dt.Rows(i)(valColIdx)) Then valsList.Add(dt.Rows(i)(valColIdx))
        Next
        PercentileVal = Me.setup.GeneralFunctions.PercentileFromList(valsList, 1 - myPercentile)

        'now that we have the percentile value, filter the dataset
        Me.setup.GeneralFunctions.UpdateProgressBar("Computing lower percentile for timeseries...", 0, 10)
        For i = StartIdx To EndIdx
            Me.setup.GeneralFunctions.UpdateProgressBar("", i - StartIdx, EndIdx - StartIdx)
            Me.setup.GeneralFunctions.UpdateProgressBar("", i - StartIdx, EndIdx - StartIdx)
            If Not IsDBNull(dt.Rows(i)(valColIdx)) AndAlso dt.Rows(i)(valColIdx) >= PercentileVal Then
                resultsdt.Rows.Add(dt.Rows(i)(dateColIdx), dt.Rows(i)(valColIdx))
            Else
                resultsdt.Rows.Add(dt.Rows(i)(dateColIdx), DBNull.Value)
            End If
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete", 0, 10)

        Return resultsdt
    End Function

    Public Function DataTableTimeseriesMaxValue(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, StartIdx As Integer, EndIdx As Integer) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long

        If StartIdx < 0 Then
            StartIdx = 0
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": invalid record index " & StartIdx & ".")
        End If
        If EndIdx >= dt.Rows.Count Then
            EndIdx = dt.Rows.Count - 1
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": non-existent record index " & EndIdx & ". Timespan was adjusted.")
        End If

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Maximum", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'determine the value that corresponds with the requested percentile
        Dim MaxVal As Double = -9.0E+99
        Dim MaxValDate As DateTime
        For i = StartIdx To EndIdx
            If Not IsDBNull(dt.Rows(i)(valColIdx)) AndAlso dt.Rows(i)(valColIdx) > MaxVal Then
                MaxVal = dt.Rows(i)(valColIdx)
                MaxValDate = dt.Rows(i)(dateColIdx)
            End If
        Next

        resultsdt.Rows.Add(MaxValDate, MaxVal)
        Return resultsdt
    End Function

    Public Function DataTableTimeseriesMinValue(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, StartIdx As Integer, EndIdx As Integer) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long

        If StartIdx < 0 Then
            StartIdx = 0
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": invalid record index " & StartIdx & ".")
        End If
        If EndIdx >= dt.Rows.Count Then
            EndIdx = dt.Rows.Count - 1
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": non-existent record index " & EndIdx & ". Timespan was adjusted.")
        End If

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Minimum", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'determine the value that corresponds with the requested percentile
        Dim MinVal As Double = 9.0E+99
        Dim MinValDate As DateTime
        For i = StartIdx To EndIdx
            If Not IsDBNull(dt.Rows(i)(valColIdx)) AndAlso dt.Rows(i)(valColIdx) > MinVal Then
                MinVal = dt.Rows(i)(valColIdx)
                MinValDate = dt.Rows(i)(dateColIdx)
            End If
        Next

        resultsdt.Rows.Add(MinValDate, MinVal)
        Return resultsdt
    End Function

    Public Function DataTableTimeseriesDailySum(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, StartIdx As Integer, EndIdx As Integer) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long

        If StartIdx < 0 Then
            StartIdx = 0
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": invalid record index " & StartIdx & ".")
        End If
        If EndIdx >= dt.Rows.Count Then
            EndIdx = dt.Rows.Count - 1
            Me.setup.Log.AddWarning("Error calculating percentile for timeseries " & dt.TableName & ": non-existent record index " & EndIdx & ". Timespan was adjusted.")
        End If

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Minimum", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'determine the value that corresponds with the requested percentile
        Dim myDate As DateTime = dt.Rows(StartIdx)(dateColIdx)
        Dim mySum As Double = dt.Rows(StartIdx)(valColIdx)
        For i = StartIdx + 1 To EndIdx
            If Not IsDBNull(dt.Rows(i)(valColIdx)) Then
                If Day(dt.Rows(i)(dateColIdx)) = Day(myDate) Then
                    'while we're in the same day, add the numbers up
                    mySum += dt.Rows(i)(valColIdx)
                Else
                    'date has changed. first store the previous date
                    resultsdt.Rows.Add(myDate, mySum)

                    'now set the new myDate and its value
                    myDate = dt.Rows(i)(dateColIdx)
                    mySum = dt.Rows(i)(valColIdx)
                End If
            End If
        Next
        'finalize the last processed date
        resultsdt.Rows.Add(myDate, mySum)

        Return resultsdt
    End Function

    Public Function FormatDateAsISO8601(myDate As DateTime, Optional ByVal IncludeMiliseconds As Boolean = False) As String
        'this function formats a datetime as an ISO8601 string.
        'this formatting is required when storing dates in SQLite database
        If IncludeMiliseconds Then
            Return myDate.ToString("yyyy-MM-dd HH:mm:ss.sss")
        Else
            Return myDate.ToString("yyyy-MM-dd HH:mm:ss")
        End If
    End Function
    Public Function DataTableTimeseriesMaxPerEvent(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, SkipPercentage As Double) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long

        'first decide the regular timestep size
        Dim FirstDate As DateTime = dt.Rows(0)(dateColIdx)
        Dim SecondDate As DateTime = dt.Rows(1)(dateColIdx)
        Dim CurDate As DateTime
        Dim PrevDate As DateTime
        Dim Timestep As New TimeSpan
        Timestep = SecondDate.Subtract(FirstDate)
        Dim CurTimestep As New TimeSpan
        Dim startidx As Integer, myMax As Double

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Minimum", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'initialize for the first event
        Dim Values As New List(Of Double)
        Values.Add(dt.Rows(0)(valColIdx))
        Dim EventDate As DateTime = dt.Rows(0)(dateColIdx)

        'Me.setup.GeneralFunctions.UpdateProgressBar("Retrieving maximum per event...", 0, 10, True)
        For i = 1 To dt.Rows.Count - 1
            'Me.setup.GeneralFunctions.UpdateProgressBar("", i, dt.Rows.Count)
            CurDate = dt.Rows(i)(dateColIdx)
            PrevDate = dt.Rows(i - 1)(dateColIdx)
            CurTimestep = CurDate.Subtract(PrevDate)

            If CurTimestep.TotalSeconds <> Timestep.TotalSeconds Then

                'we just started a new event. but First wrap up the old one
                startidx = SkipPercentage / 100 * Values.Count
                myMax = -9.0E+99
                For j = startidx To Values.Count - 1
                    If Values(j) > myMax Then
                        myMax = Values(j)
                    End If
                Next
                resultsdt.Rows.Add(EventDate, myMax)

                'now initialize our new event
                EventDate = dt.Rows(i)(dateColIdx)
                Values = New List(Of Double)
                Values.Add(dt.Rows(i)(valColIdx))
            Else
                Values.Add(dt.Rows(i)(valColIdx))
            End If
        Next

        'finalize the last event
        startidx = SkipPercentage / 100 * Values.Count
        myMax = -9.0E+99
        For j = startidx To Values.Count - 1
            If Values(j) > myMax Then
                myMax = Values(j)
            End If
        Next
        resultsdt.Rows.Add(EventDate, myMax)

        Return resultsdt
    End Function

    Public Function DataTableTimeseriesTimestepValue(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, tsIdx As Integer) As DataTable
        Dim resultsdt As New DataTable

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Value", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        'determine the value that corresponds with the requested percentile
        If tsIdx >= 0 AndAlso tsIdx < dt.Rows.Count Then
            If Not IsDBNull(dt.Rows(tsIdx)(valColIdx)) Then
                resultsdt.Rows.Add(dt.Rows(tsIdx)(dateColIdx), dt.Rows(tsIdx)(valColIdx))
            End If
        End If

        Return resultsdt
    End Function

    Public Function DataTableTimeseriesSubset(ByVal dt As DataTable, dateColIdx As Integer, valColIdx As Integer, startIdx As Integer, endIdx As Integer) As DataTable
        Dim resultsdt As New DataTable
        Dim i As Long

        'create a new datatable for the resulting timetable
        resultsdt.Columns.Add(New DataColumn("Datum", Type.GetType("System.DateTime")))
        resultsdt.Columns.Add(New DataColumn("Value", Type.GetType("System.Double")))
        resultsdt.TableName = dt.TableName

        For i = startIdx To endIdx
            If Not IsDBNull(dt.Rows(i)(valColIdx)) Then
                resultsdt.Rows.Add(dt.Rows(i)(dateColIdx), dt.Rows(i)(valColIdx))
            End If
        Next

        Return resultsdt

    End Function

    Public Function DataTableMonthlyMax(ByVal dt As DataTable, DateColIdx As Integer, ValColIdx As Integer, StartIdx As Integer, EndIdx As Integer) As DataTable
        Dim maxdt As New DataTable
        Dim i As Long, curYear As Integer, lastYear As Integer = -999, curMax As Double
        Dim curMonth As Integer, lastMonth As Integer = -999
        maxdt.Columns.Add(New DataColumn("Year-Month", Type.GetType("System.Int16")))
        maxdt.Columns.Add(New DataColumn("Maximum", Type.GetType("System.Double")))

        For i = StartIdx To EndIdx
            curYear = Year(dt.Rows(i)(DateColIdx))
            curMonth = Month(dt.Rows(i)(DateColIdx))
            If Not IsDBNull(dt.Rows(i)(ValColIdx)) Then
                If curMonth <> lastMonth Then
                    'store the previous year/month's average before proceeding
                    If lastYear > -999 AndAlso lastMonth > -999 Then
                        'avgdt.Rows.Add(lastYear & "_" & Format(lastMonth, "00"), curAvg)
                        'avgdt.Rows.Add(Convert.ToInt64(lastYear & Format(lastMonth, "00")), curAvg)
                        maxdt.Rows.Add(Me.setup.GeneralFunctions.MonthNumberAD(lastYear, lastMonth), curMax)
                    End If
                    'now set the last year/month to be the current year/month and initialize the average for the current year
                    lastYear = curYear
                    lastMonth = curMonth
                    curMax = dt.Rows(i)(ValColIdx)
                ElseIf dt.Rows(i)(ValColIdx) > curMax Then
                    curMax = dt.Rows(i)(ValColIdx)
                End If
            End If
        Next
        Return maxdt
    End Function

    Public Function dataTable2ListOfString(ByRef dt As DataTable, Optional ByVal ColIdx As Integer = 0) As List(Of String)
        Dim myList As New List(Of String)
        Dim i As Integer
        For i = 0 To dt.Rows.Count - 1
            myList.Add(dt.Rows(i)(ColIdx))
        Next
        Return myList
    End Function
    Public Function dataTable2ListOfDouble(ByRef dt As DataTable, Optional ByVal ColIdx As Integer = 0) As List(Of Double)
        Dim myList As New List(Of Double)
        Dim i As Integer
        For i = 0 To dt.Rows.Count - 1
            myList.Add(dt.Rows(i)(ColIdx))
        Next
        Return myList
    End Function


    Public Function getBaseFromFilename(FileName) As String
        For i = FileName.length To 2 Step -1
            If Mid(FileName, i, 1) = "." Then
                Return Left(FileName, i - 1)
            End If
        Next
        Return FileName
    End Function

    Public Function ParseStringMultiDelimiter(ByRef myString As String, Delimiters As List(Of String), Optional ByVal QuoteHandlingFlag As Integer = 1, Optional ByVal ResultMustBeNumeric As Boolean = False) As String
        'this function parses a string by accepting multiple delimiters
        'one condition: each delimiter must consist of exactly one character
        Dim quoteEven As Boolean
        Dim tmpString As String = "", tmpChar As String = ""

        myString = myString.Trim
        quoteEven = True

        'Quotehandlingflag: default = 1
        '0 = items between quotes are NOT being treated as separate items (parsing also between quotes)
        '1 = items between single quotes are being treated as separate items (no parsing between single quotes)
        '2 = items between double quotes are being treated as separate items (no parsing between double quotes)

        Dim i As Integer
        For i = 1 To Len(myString)
            'snoep een karakter af van de string
            tmpChar = Left(myString, 1)

            If (tmpChar = "'" And QuoteHandlingFlag = 1) Or (tmpChar = Chr(34) And QuoteHandlingFlag = 2) Then
                If quoteEven = True Then
                    quoteEven = False
                    tmpString = tmpString & tmpChar
                    myString = Right(myString, myString.Length - 1)
                Else
                    quoteEven = True 'dit betekent dat we klaar zijn
                    tmpString = Right(tmpString, tmpString.Length - 1) 'laat bij het teruggeven meteen de quotes maar weg!
                    myString = Right(myString, myString.Length - 1)
                End If
            ElseIf Delimiters.Contains(tmpChar) And quoteEven = True Then
                myString = Right(myString, myString.Length - 1)
                Return tmpString

                'If Not tmpString = "" Then
                '    myString = Right(myString, myString.Length - 1)
                '    'Return tmpString
                '    Exit For
                'Else
                '    myString = Right(myString, myString.Length - 1)
                'End If
            ElseIf myString <> "" Then
                myString = Right(myString, myString.Length - 1)
                tmpString = tmpString & tmpChar
            End If
        Next

        If ResultMustBeNumeric AndAlso Not IsNumeric(tmpString) Then
            myString = tmpString & Delimiters(0) & myString
            Me.setup.Log.AddWarning("Numeric value expected after token " & tmpString & " in string " & myString & ". Value of 0 was returned.")
            Return 0
        Else
            Return tmpString
        End If


    End Function

    Public Sub DataTable2CSV(ByRef dt As DataTable, path As String, delimiter As String)
        Dim i As Integer, j As Integer

        path = ReplaceInvalidCharactersInPath(path, "_")
        Using csvWriter As New StreamWriter(path)

            ' Write the CSV header
            Dim csvLine As New StringBuilder()
            For j = 0 To dt.Columns.Count - 1
                csvLine.Append(dt.Columns(j).ColumnName).Append(delimiter)
            Next
            csvWriter.WriteLine(csvLine.ToString())

            ' Write the CSV data
            For i = 0 To dt.Rows.Count - 1
                ' First the results
                csvLine.Clear()
                For j = 0 To dt.Columns.Count - 1
                    csvLine.Append(dt.Rows(i)(j)).Append(delimiter)
                Next

                csvWriter.WriteLine(csvLine.ToString())
            Next
        End Using
    End Sub

    Public Sub DataTable2CSVByStreamWriter(ByRef dt As DataTable, ByRef csvWriter As StreamWriter, delimiter As String)
        Dim i As Integer, j As Integer

        ' Write the CSV header
        Dim csvLine As New StringBuilder()
        For j = 0 To dt.Columns.Count - 1
            csvLine.Append(dt.Columns(j).ColumnName).Append(delimiter)
        Next
        csvWriter.WriteLine(csvLine.ToString())

        ' Write the CSV data
        For i = 0 To dt.Rows.Count - 1
            csvLine.Clear()
            For j = 0 To dt.Columns.Count - 1
                ' Ensure that any delimiters within the data are handled appropriately
                Dim data As String = dt.Rows(i)(j).ToString()
                If data.Contains(delimiter) Then
                    data = """" & data.Replace("""", """""") & """"
                End If
                csvLine.Append(data).Append(delimiter)
            Next

            csvWriter.WriteLine(csvLine.ToString())
        Next
    End Sub


    Public Sub PopulateComboBoxWithTimeSeriesProcessingOptions(ByRef myCombo As Windows.Forms.ComboBox)
        myCombo.Items.Clear()
        myCombo.Items.Add(enmTimeSeriesProcessing.none.ToString)
        myCombo.Items.Add(enmTimeSeriesProcessing.cumulative.ToString)
        myCombo.Items.Add(enmTimeSeriesProcessing.monthlycumulative.ToString)
        myCombo.Items.Add(enmTimeSeriesProcessing.annualcumulative.ToString)
        myCombo.Items.Add(enmTimeSeriesProcessing.monthlysum.ToString)
        myCombo.Items.Add(enmTimeSeriesProcessing.percentile.ToString)
    End Sub

    Public Function XMLSafeEquation(myEquation As String) As String
        'this function replaces characters from a math equation that are not supported by XML by their safe equivalent
        Dim newEquation As String = myEquation
        newEquation = Strings.Replace(newEquation, ">=", "&gt;=")
        newEquation = Strings.Replace(newEquation, "<=", "&lt;=")
        newEquation = Strings.Replace(newEquation, ">", "&gt;")
        newEquation = Strings.Replace(newEquation, "<", "&lt;")
        Return newEquation
    End Function
    Public Function AdjustDateForTimestep(ByVal myDate As DateTime, ByVal TimeStepSize As enmTimeStepSize) As DateTime
        'in some cases we're looking for daily values in hourly data. This means we'll have to match
        'e.g. 11/12/2012 18:00 with 11/12/2012

        'adjust the searched date in case of other timesteps e.g. day
        Select Case TimeStepSize
            Case Is = enmTimeStepSize.YEAR
                Return New DateTime(Year(myDate), 1, 1)
            Case Is = enmTimeStepSize.MONTH
                Return New DateTime(Year(myDate), Month(myDate), 1)
            Case Is = enmTimeStepSize.DAY
                Return New DateTime(Year(myDate), Month(myDate), Day(myDate))
            Case Is = enmTimeStepSize.HOUR
                Return New DateTime(Year(myDate), Month(myDate), Day(myDate), Hour(myDate), 0, 0)
            Case Is = enmTimeStepSize.MINUTE
                Return New DateTime(Year(myDate), Month(myDate), Day(myDate), Hour(myDate), Minute(myDate), 0)
            Case Is = enmTimeStepSize.SECOND
                Return myDate
        End Select

    End Function

    Public Function MultiplierFromTimeStepConversion(ByVal FromTimestep As enmTimeStepSize, ByVal ToTimestep As enmTimeStepSize) As Double
        'this function returns a multiplier that converts one timestep to another
        If FromTimestep.ToString = ToTimestep.ToString Then Return 1

        Select Case FromTimestep
            Case Is = enmTimeStepSize.SECOND
                Select Case ToTimestep
                    Case Is = enmTimeStepSize.MINUTE
                        Return 1 / 60
                    Case Is = enmTimeStepSize.HOUR
                        Return 1 / 3600
                    Case Is = enmTimeStepSize.DAY
                        Return 1 / (24 * 3600)
                    Case Is = enmTimeStepSize.MONTH
                        Return 1 / (30 * 24 * 3600)
                    Case Is = enmTimeStepSize.YEAR
                        Return 1 / (365.25 * 24 * 3600)
                End Select
            Case Is = enmTimeStepSize.MINUTE
                Select Case ToTimestep
                    Case Is = enmTimeStepSize.SECOND
                        Return 60
                    Case Is = enmTimeStepSize.HOUR
                        Return 1 / 60
                    Case Is = enmTimeStepSize.DAY
                        Return 1 / (24 * 60)
                    Case Is = enmTimeStepSize.MONTH
                        Return 1 / (30 * 24 * 60)
                    Case Is = enmTimeStepSize.YEAR
                        Return 1 / (365.25 * 24 * 60)
                End Select
            Case Is = enmTimeStepSize.HOUR
                Select Case ToTimestep
                    Case Is = enmTimeStepSize.SECOND
                        Return 3600
                    Case Is = enmTimeStepSize.MINUTE
                        Return 60
                    Case Is = enmTimeStepSize.DAY
                        Return 1 / 24
                    Case Is = enmTimeStepSize.MONTH
                        Return 1 / (30 * 24)
                    Case Is = enmTimeStepSize.YEAR
                        Return 1 / (365.25 * 24)
                End Select
            Case Is = enmTimeStepSize.DAY
                Select Case ToTimestep
                    Case Is = enmTimeStepSize.SECOND
                        Return 3600 * 24
                    Case Is = enmTimeStepSize.MINUTE
                        Return 60 * 24
                    Case Is = enmTimeStepSize.HOUR
                        Return 24
                    Case Is = enmTimeStepSize.MONTH
                        Return 1 / 30
                    Case Is = enmTimeStepSize.YEAR
                        Return 1 / 365.25
                End Select
            Case Is = enmTimeStepSize.MONTH
                Select Case ToTimestep
                    Case Is = enmTimeStepSize.SECOND
                        Return 3600 * 24 * 30
                    Case Is = enmTimeStepSize.MINUTE
                        Return 60 * 24 * 30
                    Case Is = enmTimeStepSize.HOUR
                        Return 24 * 30
                    Case Is = enmTimeStepSize.DAY
                        Return 30
                    Case Is = enmTimeStepSize.YEAR
                        Return 1 / 12
                End Select
            Case Is = enmTimeStepSize.YEAR
                Select Case ToTimestep
                    Case Is = enmTimeStepSize.SECOND
                        Return 3600 * 24 * 365.25
                    Case Is = enmTimeStepSize.MINUTE
                        Return 60 * 24 * 365.25
                    Case Is = enmTimeStepSize.HOUR
                        Return 24 * 365.25
                    Case Is = enmTimeStepSize.DAY
                        Return 365.25
                    Case Is = enmTimeStepSize.MONTH
                        Return 12
                End Select
        End Select
    End Function

    Public Function RainfallRunoffConnectionFromString(ByVal myModel As String) As enmRainfallRunoffConnection
        Select Case myModel.Trim.ToUpper
            Case Is = "RROPENWATER"
                Return enmRainfallRunoffConnection.RROPENWATER
            Case Is = "RRCFCONNECTION"
                Return enmRainfallRunoffConnection.RRCFCONNECTION
            Case Else
                Return enmRainfallRunoffConnection.RRCFCONNECTION
        End Select
    End Function

    Public Function RainfallRunoffModelFromString(ByVal myModel As String) As enmRainfallRunoffModel
        Select Case myModel.Trim.ToUpper
            Case Is = "NONE"
                Return STOCHLIB.GeneralFunctions.enmRainfallRunoffModel.NONE
            Case Is = "LATERAL"
                Return STOCHLIB.GeneralFunctions.enmRainfallRunoffModel.LATERAL
            Case Is = "SOBEKRR"
                Return STOCHLIB.GeneralFunctions.enmRainfallRunoffModel.SOBEKRR
            Case Is = "SACRAMENTO"
                Return STOCHLIB.GeneralFunctions.enmRainfallRunoffModel.SACRAMENTO
            Case Is = "HBV"
                Return STOCHLIB.GeneralFunctions.enmRainfallRunoffModel.HBV
            Case Is = "WAGENINGENMODEL"
                Return STOCHLIB.GeneralFunctions.enmRainfallRunoffModel.WAGENINGENMODEL
        End Select
    End Function

    Public Function DateFormattingByTimeStep(ByVal TimeStepStr As String) As String
        'determine in which format to write to the database based on the timestep size
        'of a time series. e.g. timestep of 1 hour can be written to database as yyyy/MM/dd HH
        'timestep of 1 day can simply be written to database as yyyy/MM/dd
        Select Case TimeStepStr.Trim.ToLower
            Case Is = "hour"
                Return "yyyy/MM/dd HH"
            Case Is = "day"
                Return "yyyy/MM/dd"
            Case Is = "month"
                Return "yyyy/MM"
            Case Is = "year"
                Return "yyyy"
        End Select
        Return ""
    End Function

    Public Function getMeteoStationTypeFromString(ByVal myString As String) As enmMeteoStationType
        Select Case myString.Trim.ToUpper
            Case Is = "RAINFALL"
                Return enmMeteoStationType.precipitation
            Case Is = "NEERSLAG"
                Return enmMeteoStationType.precipitation
            Case Is = "REGEN"
                Return enmMeteoStationType.precipitation
            Case Is = "PRECIPITATION"
                Return enmMeteoStationType.precipitation
            Case Is = "EVAPORATION"
                Return enmMeteoStationType.evaporation
            Case Is = "EVAPOTRANSPIRATION"
                Return enmMeteoStationType.evaporation
            Case Is = "VERDAMPING"
                Return enmMeteoStationType.evaporation
            Case Is = "EVAPOTRANSPIRATIE"
                Return enmMeteoStationType.evaporation
        End Select
    End Function

    Friend Function isStructure(nt As enmNodetype) As Boolean
        Select Case nt
            Case Is = enmNodetype.NodeCFBridge
                Return True
            Case Is = enmNodetype.NodeCFCulvert
                Return True
            Case Is = enmNodetype.NodeCFOrifice
                Return True
            Case Is = enmNodetype.NodeCFPump
                Return True
            Case Is = enmNodetype.NodeCFUniWeir
                Return True
            Case Is = enmNodetype.NodeCFWeir
                Return True
            Case Is = enmNodetype.NodeCFExtraResistance
                Return True
        End Select
        Return False

    End Function

    Public Function RemoveNumericPostfixes(ByVal NameBase As String, ByVal Delimiter As String) As String
        'this function removes multiple postfixes (preceeded by a delimiter) from a string
        'eg: REACH12_45_1 yields REACH12
        Dim Done As Boolean, PrevNameBase As String

        While Not Done
            PrevNameBase = NameBase
            NameBase = RemoveNumericPostFix(NameBase, Delimiter)
            If NameBase = PrevNameBase Then Return NameBase
        End While

        Return NameBase

    End Function

    Public Function RemoveNumericPostFix(ByVal NameBase As String, ByVal Delimiter As String) As String
        'this function removes a numeric postfix (preceeded by a delimiter) from a string
        'eg: REACH12_45 with delimiter "_" yields REACH12
        Dim i As Long
        For i = NameBase.Length To 1 Step -1
            If Mid(NameBase, i, 1) = Delimiter Then
                Return Left(NameBase, i - 1)
            ElseIf Not IsNumeric(Mid(NameBase, i, 1)) Then
                Exit For
            End If
        Next
        Return NameBase
    End Function

    Public Function RemovePostfix(ByVal NameBase As String, ByVal Delimiter As String) As String
        'this function removes a postfix (preceeded by a delimiter) from a string
        'eg: Reach12_s1 with delimiter "_" yields Reach12
        Dim i As Long
        For i = NameBase.Length To 1 Step -1
            If Mid(NameBase, i, 1) = Delimiter Then
                Return Left(NameBase, i - 1)
            End If
        Next
        'delimiter not found. Return the entire base
        Return NameBase
    End Function

    Public Function PopulateComboboxWithKlimaatScenarios(ByRef myCmb As Windows.Forms.ComboBox, Optional ByVal InitialSelectionFirstItem As Boolean = True) As Boolean
        Try
            Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmKlimaatScenario), InitialSelectionFirstItem)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error populating combobox with climate scenario's: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function AddToDate(ByVal RefDate As Date, ByVal AddNumber As Integer, ByVal AddUnits As String) As DateTime
        'set the absolute value for the start date
        Select Case AddUnits.Trim.ToLower
            Case Is = "uren", "hours"
                Return RefDate.AddHours(AddNumber)
            Case Is = "dagen", "days"
                Return RefDate.AddDays(AddNumber)
            Case Is = "weken", "weeks"
                Return RefDate.AddDays(7 * AddNumber)
            Case Is = "maanden", "months"
                Return RefDate.AddMonths(AddNumber)
            Case Is = "jaren", "years"
                Return RefDate.AddYears(AddNumber)
        End Select

        Return RefDate

    End Function


    Public Function LengthByTimeUnitConversion(ByVal myValue As Double, ByVal FromUnit As enmDataUnit, ByVal ToUnit As enmDataUnit) As Double
        If FromUnit = ToUnit Then Return myValue
        Select Case FromUnit
            Case Is = enmDataUnit.MMperDAY
                Select Case ToUnit
                    Case Is = enmDataUnit.MMperHOUR
                        Return myValue / 24
                End Select
            Case Is = enmDataUnit.MMperHOUR
                Select Case ToUnit
                    Case Is = enmDataUnit.MMperDAY
                        Return myValue * 24
                End Select
        End Select
    End Function

    Public Function BurnDateInStringByTemplate(ByRef myString As String, ByVal DateTemplate As String, ByVal mydate As DateTime) As Boolean
        'this function 'burns' a date inside an existing string, based on a given template for the formatting
        Try
            Dim DateString As String = DateTemplate

            If InStr(DateTemplate, "yyyy", CompareMethod.Binary) > 0 Then
                DateString = Replace(DateString, "yyyy", Year(mydate).ToString, , , CompareMethod.Text)
            End If

            If InStr(DateTemplate, "MM", CompareMethod.Binary) > 0 Then
                DateString = Replace(DateString, "MM", Format(Month(mydate), "00"), , , CompareMethod.Binary)
            End If

            If InStr(DateTemplate, "dd", CompareMethod.Text) > 0 Then
                DateString = Replace(DateString, "dd", Format(Day(mydate), "00"), , , CompareMethod.Text)
            End If

            If InStr(DateTemplate, "HH", CompareMethod.Text) > 0 Then
                DateString = Replace(DateString, "HH", Format(Hour(mydate), "00"), , , CompareMethod.Text)
            End If

            If InStr(DateTemplate, "mm", CompareMethod.Binary) > 0 Then
                DateString = Replace(DateString, "mm", Format(Minute(mydate), "00"), , , CompareMethod.Text)
            End If

            myString = Replace(myString, DateTemplate, DateString, , , CompareMethod.Text)

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function replaceFileNameInPath(path As String, NewFileName As String) As String
        Dim firstpos As Integer = 0, lastpos As Integer = 0

        'find the file extension
        While Not InStr(firstpos + 1, path, "\") = 0
            firstpos = InStr(firstpos + 1, path, "\")
        End While

        While Not InStr(lastpos + 1, path, ".") = 0
            lastpos = InStr(lastpos + 1, path, ".")
        End While

        If firstpos > 0 AndAlso lastpos > 0 Then
            Return Left(path, firstpos) & NewFileName & Right(path, path.Length - lastpos + 1)
        Else
            Return ""
        End If

    End Function

    Public Function getExtensionFromFileName(Path As String) As String
        Dim pointpos As Integer = 0
        While Not InStr(pointpos + 1, Path, ".") = 0
            pointpos = InStr(pointpos + 1, Path, ".")
        End While

        Return Right(Path, Path.Length - pointpos)

    End Function



    Public Function BurnIntInStringByTemplate(ByRef myString As String, ByVal ValTemplate As String, ByVal myValue As Long) As Boolean
        'this function 'burns' an integer inside an existing string, based on a given template for the formatting
        Try

            Dim FormattingString As String = ""
            Dim i As Integer
            For i = 1 To Len(ValTemplate)
                FormattingString &= "0"
            Next

            If InStr(myString, ValTemplate, CompareMethod.Text) > 0 Then
                myString = Replace(myString, ValTemplate, Format(myValue, FormattingString))
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function SobekProjectExists(ByVal ProjectDir As String) As Boolean
        Return System.IO.Directory.Exists(ProjectDir)
    End Function

    Public Function SobekCaseExists(ByVal ProjectDir As String, ProgramsDir As String, ByVal CaseName As String) As Boolean
        Dim myProject As New clsSobekProject(Me.setup, ProjectDir, ProgramsDir, True)
        Dim myCase As ClsSobekCase = myProject.GetCase(CaseName)
        If Not myCase Is Nothing Then
            Return True
        Else : Return False
        End If
    End Function

    Public Function DateFromFormattedStringGeneric(ByVal myString As String, ByVal DateFormatting As String, ByRef myDate As DateTime) As Boolean
        Try
            Dim Year As Integer, Month As Integer, Day As Integer, Hour As Integer, Minute As Integer, Second As Integer
            Dim YearPos1 As Integer, YearPos2 As Integer, YearPos3 As Integer, YearPos4 As Integer
            Dim MonthPos As Integer, DayPos As Integer, HourPos As Integer, HourPos2 As Integer, MinutePos As Integer, SecondPos As Integer

            myString = myString.Trim
            YearPos1 = InStr(DateFormatting, "yyyy", CompareMethod.Text)
            YearPos2 = InStr(DateFormatting, "yy", CompareMethod.Text)
            YearPos3 = InStr(DateFormatting, "jjjj", CompareMethod.Text)
            YearPos4 = InStr(DateFormatting, "jj", CompareMethod.Text)
            If YearPos1 > 0 Then
                Year = Mid(myString, YearPos1, 4)
            ElseIf YearPos2 > 0 Then
                Year = 2000 + Convert.ToInt16(Mid(myString, YearPos2, 2))
            ElseIf YearPos3 > 0 Then
                Year = Mid(myString, YearPos3, 4)
            ElseIf YearPos4 > 0 Then
                Year = 2000 + Convert.ToInt16(Mid(myString, YearPos4, 2))
            Else
                Throw New Exception("Could not retrieve year from string " & myString & " using date formatting " & DateFormatting)
            End If

            MonthPos = InStr(DateFormatting, "MM", CompareMethod.Binary)
            If MonthPos > 0 Then
                Month = Mid(myString, MonthPos, 2)
            Else
                Throw New Exception("Could not retrieve month from string " & myString & " using date formatting " & DateFormatting)
            End If

            DayPos = InStr(DateFormatting, "dd", CompareMethod.Text)
            If DayPos > 0 Then
                Day = Mid(myString, DayPos, 2)
            Else
                Throw New Exception("Could not retrieve day from string " & myString & " using date formatting " & DateFormatting)
            End If

            HourPos = InStr(DateFormatting, "hh", CompareMethod.Text)
            HourPos2 = InStr(DateFormatting, "uu", CompareMethod.Text)
            If HourPos > 0 Then
                Hour = Mid(myString, HourPos, 2)
            ElseIf HourPos2 > 0 Then
                Hour = Mid(myString, HourPos2, 2)
            Else
                Hour = 0
            End If

            MinutePos = InStr(DateFormatting, "mm", CompareMethod.Binary)
            If MinutePos > 0 Then
                Minute = Mid(myString, MinutePos, 2)
            Else
                Minute = 0
            End If

            SecondPos = InStr(DateFormatting, "ss", CompareMethod.Text)
            If SecondPos > 0 Then
                Second = Mid(myString, SecondPos, 2)
            Else
                Second = 0
            End If

            myDate = New DateTime(Year, Month, Day, Hour, Minute, Second)

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GDALOutputFormatfromfilename(path As String, ByRef GDALStr As String) As Boolean
        Try
            'this function returns the GDAL output format (-of) based on a provided file extension (e.g. ASC, TIF, IMG)
            Dim extension As String = getExtensionFromFileName(path).Trim.ToUpper
            Select Case extension
                Case "ASC"
                    GDALStr = "AAIGrid"
                Case "TIF"
                    GDALStr = "GTiff"
                Case "IMG"
                    GDALStr = "HFA"
                Case Else
                    GDALStr = "GTiff" 'default is geotiff
                    Throw New Exception("Error determining GDAL Output format string from file extension: " & path & ". File type not yet supported.")
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function DateFromFormattedString(ByVal myString As String, ByVal DateFormatting As String, ByRef myDate As DateTime) As Boolean
        Dim myYear As Integer, myMonth As Integer, myDay As Integer, myHour As Integer, myMinute As Integer, mySecond As Integer

        Try
            'string contains a date formatting code. Find it and replace it with the actual date value
            Select Case DateFormatting.Trim.ToLower
                Case Is = "yyyymmddhhmmss"
                    myYear = Left(myString, 4)
                    myMonth = Left(Right(myString, 10), 2)
                    myDay = Left(Right(myString, 8), 2)
                    myHour = Left(Right(myString, 6), 2)
                    myMinute = Left(Right(myString, 4), 2)
                    mySecond = Right(myString, 2)
                Case Is = "yyyymmddhhmm"
                    myYear = Left(myString, 4)
                    myMonth = Left(Right(myString, 8), 2)
                    myDay = Left(Right(myString, 6), 2)
                    myHour = Left(Right(myString, 4), 2)
                    myMinute = Right(myString, 2)
                    myMinute = 0
                Case Is = "yyyymmddhh"
                    myYear = Left(myString, 4)
                    myMonth = Left(Right(myString, 6), 2)
                    myDay = Left(Right(myString, 4), 2)
                    myHour = Right(myString, 2)
                    myMinute = 0
                    mySecond = 0
                Case Is = "yyyymmdd"
                    myYear = Left(myString, 4)
                    myMonth = Left(Right(myString, 4), 2)
                    myDay = Right(myString, 2)
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "yyyy/mm/dd"
                    myYear = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myDay = myString
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "yyyy/mm/dd hh"
                    myYear = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myDay = ParseString(myString, " ")
                    myHour = ParseString(myString, ": ")
                    myMinute = 0
                    mySecond = 0
                Case Is = "yyyy/mm/dd hh"
                    myYear = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myDay = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = 0
                    mySecond = 0
                Case Is = "yyyy-mm-dd hh:mm"
                    myYear = ParseString(myString, "-")
                    myMonth = ParseString(myString, "-")
                    myDay = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = myString
                    mySecond = 0
                Case Is = "yyyy/mm/dd hh:mm:ss"
                    myYear = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myDay = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = ParseString(myString, ":")
                    mySecond = myString
                Case Is = "yyyy-mm-dd hh:mm"
                    myYear = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myDay = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = ParseString(myString, ":")
                    mySecond = myString
                Case Is = "mm/dd/yyyy"
                    myMonth = ParseString(myString, "/")
                    myDay = ParseString(myString, "/")
                    myYear = myString
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "dd/mm/yyyy hh:mm"
                    myDay = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myYear = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = myString
                    mySecond = 0
                Case Is = "dd-mm-yyyy hh:mm:ss"
                    myDay = ParseString(myString, "-")
                    myMonth = ParseString(myString, "-")
                    myYear = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = ParseString(myString, ":")
                    mySecond = myString
                Case Is = "dd-mm-yyyy hh:mm"
                    myDay = ParseString(myString, "-")
                    myMonth = ParseString(myString, "-")
                    myYear = ParseString(myString, " ")
                    myHour = ParseString(myString, ":")
                    myMinute = ParseString(myString, ":")
                    mySecond = 0
                Case Is = "dd-mm-yyyy/hh:mm:ss"
                    myDay = ParseString(myString, "-")
                    myMonth = ParseString(myString, "-")
                    myYear = ParseString(myString, "/")
                    myHour = ParseString(myString, ":")
                    myMinute = ParseString(myString, ":")
                    mySecond = myString
                Case Is = "dd-mm-yyyy/hh:mm"
                    myDay = ParseString(myString, "-")
                    myMonth = ParseString(myString, "-")
                    myYear = ParseString(myString, "/")
                    myHour = ParseString(myString, ":")
                    myMinute = ParseString(myString, ":")
                    mySecond = 0
                Case Is = "dd/mm/yyyy"
                    myDay = ParseString(myString, "/")
                    myMonth = ParseString(myString, "/")
                    myYear = ParseString(myString, "/")
                Case Else
                    Throw New Exception("Error retrieving date from string. Probably the date formatting is not supported.")
            End Select

            myDate = Me.setup.GeneralFunctions.DateFromValues(myYear, myMonth, myDay, myHour, myMinute, mySecond)

            Return True

        Catch ex As Exception
            Me.setup.Log.AddError("Error converting date string to date/time: " & myString)
            Return False
        End Try


    End Function

    Public Function LongFromFormattedDateTimeString(ByVal myString As String, ByVal DateFormatting As String) As Long
        'this function returns a date/time string as a long
        'e.g. 2003/03/24 12:49:00 in yyyy/MM/dd hh:mm:ss-format becomes 20030224124900

        Dim pos As Integer
        Dim NewStr As String

        Try

            'get the year
            pos = InStr(DateFormatting, "yyyy", CompareMethod.Text)
            If pos > 0 Then
                NewStr = Mid(myString, pos, 4)
            Else
                Throw New Exception("Formatting for year not recognized in " & DateFormatting & ". Only 'yyyy' is supported.")
            End If

            'get the month
            pos = InStr(DateFormatting, "MM", CompareMethod.Binary)
            If pos > 0 Then
                NewStr &= Mid(myString, pos, 2)
            Else
                Throw New Exception("Formatting for year not recognized in " & DateFormatting & ". Only 'MM' is supported.")
            End If

            'get the day
            pos = InStr(DateFormatting, "dd", CompareMethod.Text)
            If pos > 0 Then
                NewStr &= Mid(myString, pos, 2)
            Else
                Throw New Exception("Formatting for year not recognized in " & DateFormatting & ". Only 'dd' is supported.")
            End If

            'get the hour
            pos = InStr(DateFormatting, "hh", CompareMethod.Text)
            If pos > 0 Then
                NewStr &= Mid(myString, pos, 2)
            Else
                'assume 0h
                NewStr &= "00"
            End If

            'get the minute
            pos = InStr(DateFormatting, "mm", CompareMethod.Binary)
            If pos > 0 Then
                NewStr &= Mid(myString, pos, 2)
            Else
                'assume 00
                NewStr &= "00"
            End If

            'get the second
            pos = InStr(DateFormatting, "ss", CompareMethod.Text)
            If pos > 0 Then
                NewStr &= Mid(myString, pos, 2)
            Else
                'assume 00
                NewStr &= "00"
            End If

            'return the outcome
            If IsNumeric(NewStr) Then
                Return Val(NewStr)
            Else
                Throw New Exception("Error converting date string to long: " & myString)
            End If

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
        End Try

    End Function

    Public Function MapWinGeoProjectionFromString(ByVal ProjectionString As String, ByRef GeoProjection As MapWinGIS.GeoProjection) As Boolean

        ProjectionString = RemoveBoundingQuotes(ProjectionString)
        GeoProjection = New MapWinGIS.GeoProjection
        If InStr(ProjectionString, "EPSG:", CompareMethod.Text) > 0 Then
            ProjectionString = Replace(ProjectionString, "EPSG:", "", , , CompareMethod.Text)
            If Not GeoProjection.ImportFromEPSG(ProjectionString) Then Return False
        ElseIf InStr(ProjectionString, "Proj4", CompareMethod.Text) > 0 Then
            If Not GeoProjection.ImportFromProj4(RemoveBoundingQuotes(ProjectionString)) Then Return False
        Else
            Me.setup.Log.AddError("Error: projection string not recognized or supported: " & ProjectionString)
            Return False
        End If

        Return True

    End Function

    Public Function SetValueInStringByFormat(ByVal myString As String, ByVal ValueFormatting As String, ByVal myValue As Double) As String
        'replaces a given series of strings by a number, formatted according to the number of characters
        Dim ReplaceFormat As String = "", i As Long
        For i = 1 To ValueFormatting.Length
            ReplaceFormat &= "0"
        Next

        If InStr(myString, ValueFormatting) > 0 Then
            Return Replace(myString, ValueFormatting, Format(myValue, ReplaceFormat))
        Else
            Me.setup.Log.AddError("Error in function SetValueByFormatInString: formatting string was not found in " & myString)
            Return myString
        End If

    End Function

    Public Function GetSobekNodeDescriptionFromEnum(ByVal myType As STOCHLIB.GeneralFunctions.enmNodetype) As String

        Select Case myType
            Case Is = enmNodetype.ConnNodeLatStor
                Return "SBK_CHANNEL_STORCONN&LAT"
            Case Is = enmNodetype.MeasurementStation
                Return "SBK_MEASSTAT"
            Case Is = enmNodetype.NodeCFBoundary
                Return "SBK_BOUNDARY"
            Case Is = enmNodetype.NodeCFBridge
                Return "SBK_BRIDGE"
            Case Is = enmNodetype.NodeCFCulvert
                Return "SBK_CULVERT"
            Case Is = enmNodetype.NodeCFGridpoint
                Return "SBK_GRIDPOINT"
            Case Is = enmNodetype.NodeCFGridpointFixed
                Return "SBK_GRIDPOINTFIXED"
            Case Is = enmNodetype.NodeCFLateral
                Return "SBK_LATERALFLOW"
            Case Is = enmNodetype.NodeCFLinkage
                Return "SBK_CHANNELLINKAGENODE"
            Case Is = enmNodetype.NodeCFOrifice
                Return "SBK_ORIFICE"
            Case Is = enmNodetype.NodeCFPump
                Return "SBK_PUMP"
            Case Is = enmNodetype.NodeCFUniWeir
                Return "SBK_UNIWEIR"
            Case Is = enmNodetype.NodeCFWeir
                Return "SBK_WEIR"
            Case Is = enmNodetype.NodeRRBoundary
                Return "3B_BOUNDARY"
            Case Is = enmNodetype.NodeRRCFConnection
                Return "SBK_SBK-3B-REACH"
            Case Is = enmNodetype.NodeRRCFConnectionNode 'manhole
                Return "SBK_SBK-3B-NODE"
            Case Is = enmNodetype.NodeRRFriction
                Return "3B_FRICTION"
            Case Is = enmNodetype.NodeRRGreenhouse
                Return "3B_GREENHOUSE"
            Case Is = enmNodetype.NodeRRIndustry
                Return "3B_INDUSTRY"
            Case Is = enmNodetype.NodeRROpenWater
                Return "3B_OPENWATER"
            Case Is = enmNodetype.NodeRROrifice
                Return "3B_ORIFICE"
            Case Is = enmNodetype.NodeRRPaved
                Return "3B_PAVED"
            Case Is = enmNodetype.NodeRRPump
                Return "3B_PUMP"
            Case Is = enmNodetype.NodeRRSacramento
                Return "3B_SACRAMENTO"
            Case Is = enmNodetype.NodeRRUnpaved
                Return "3B_UNPAVED"
            Case Is = enmNodetype.NodeRRWageningenModel
                Return Nothing
            Case Is = enmNodetype.NodeRRWeir
                Return "3B_WEIR"
            Case Is = enmNodetype.NodeRRWWTP
                Return "3B_WWTP"
            Case Is = enmNodetype.NodeSFExternalPump
                Return "SBK_EXTPUMP"
            Case Is = enmNodetype.NodeSFManhole
                Return "SBK_CONNECTIONNODE"
            Case Is = enmNodetype.NodeSFManholeWithLateralFlow
                Return "SBK_CONN&LAT"
            Case Is = enmNodetype.NodeSFManholeWithRunoff
                Return "SBK_CONN&RUNOFF"
            Case Is = enmNodetype.NodeCFConnectionNode
                Return "SBK_CHANNELCONNECTION"
            Case Is = enmNodetype.SBK_PROFILE
                Return "SBK_PROFILE"
            Case Is = enmNodetype.Virtual
                Return Nothing
            Case Else
                Return Nothing
        End Select

    End Function


    Public Sub NumberGridViewRows(ByRef myGridView As Forms.DataGridView, Optional ByVal Base1 As Boolean = False)
        ' Add row headers.
        If Base1 Then
            For i As Integer = 0 To myGridView.Rows.Count - 1
                myGridView.Rows(i).HeaderCell.Value = (i + 1).ToString()
            Next i
        Else
            For i As Integer = 0 To myGridView.Rows.Count - 1
                myGridView.Rows(i).HeaderCell.Value = i.ToString()
            Next i
        End If
    End Sub

    Public Function ListUniqueValuesFromDataTable(ByRef dt As DataTable, ByVal ColIdx As Integer) As List(Of String)
        Dim r As Long
        Dim newList As New List(Of String)
        For r = 0 To dt.Rows.Count - 1
            If Not IsDBNull(dt.Rows(r)(ColIdx)) AndAlso Not newList.Contains(dt.Rows(r)(ColIdx)) Then newList.Add(dt.Rows(r)(ColIdx))
        Next
        Return newList
    End Function



    Public Function ListUniqueValuesFromShapefile(ByVal Path As String, ByVal ShapeField As String, ByRef myList As List(Of String)) As Boolean
        Try
            Dim FieldIdx As Long, i As Long
            myList = New List(Of String)
            If Not System.IO.File.Exists(Path) Then Throw New Exception("Shapefile does not exist: " & Path)
            Dim mySf As New clsShapeFile(Me.setup, Path)
            If Not mySf.Open() Then Throw New Exception("Error reading shapefile: " & Path)
            FieldIdx = mySf.GetFieldIdx(ShapeField)
            If FieldIdx < 0 Then Throw New Exception("Field " & ShapeField & " not found in shapefile " & Path)

            For i = 0 To mySf.sf.NumShapes - 1
                If mySf.sf.CellValue(FieldIdx, i) Is Nothing Then
                    'siebe 27-8-2019 added the value of NULL as a reserved keyword
                    If Not myList.Contains("NULL") Then myList.Add("NULL")
                ElseIf Not myList.Contains(mySf.sf.CellValue(FieldIdx, i)) Then
                    myList.Add(mySf.sf.CellValue(FieldIdx, i))
                End If
            Next

            Return True
        Catch ex As Exception

            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function ShapefileToDatabase(ByRef con As SQLite.SQLiteConnection, ShapeFile As String, TableName As String, ShapeNumCol As String, PointIdxCol As String, XCol As String, YCol As String, Optional ByVal RDToWGS84 As Boolean = False) As Boolean
        Try
            If Not con.State = ConnectionState.Open Then con.Open()
            Dim sf As New MapWinGIS.Shapefile, i As Integer, j As Integer, PartIdx As Integer = -1
            Dim iShape As MapWinGIS.Shape, jShape As MapWinGIS.Shape
            Dim lat As Double, lon As Double
            Dim query As String

            If Not System.IO.File.Exists(ShapeFile) Then Throw New Exception("Shapefile does not exist: " & ShapeFile)
            If Not sf.Open(ShapeFile) Then Throw New Exception("Error: could not open shapefile: " & ShapeFile)
            UpdateProgressBar("Writing shapes to database...", 0, 10)

            Select Case sf.ShapefileType
                Case Is = MapWinGIS.ShpfileType.SHP_POLYGON
                    For i = 0 To sf.NumShapes - 1
                        UpdateProgressBar("Writing shapes to database...", i, sf.NumShapes - 1)
                        iShape = sf.Shape(i)
                        For j = 0 To iShape.NumParts - 1
                            PartIdx += 1
                            jShape = iShape.PartAsShape(j)
                            For k = 0 To jShape.numPoints - 1
                                If RDToWGS84 Then
                                    If Not RD2WGS84(jShape.Point(k).x, jShape.Point(k).y, lat, lon) Then Throw New Exception("Error converting coordinate to WGS84: " & jShape.Point(k).x & "," & jShape.Point(k).y)
                                    query = "INSERT INTO " & TableName & "(" & ShapeNumCol & "," & PointIdxCol & "," & XCol & "," & YCol & ") VALUES (" & PartIdx & "," & k & "," & lon & "," & lat & ");"
                                Else
                                    query = "INSERT INTO " & TableName & "(" & ShapeNumCol & "," & PointIdxCol & "," & XCol & "," & YCol & ") VALUES (" & PartIdx & "," & k & "," & jShape.Point(k).x & "," & jShape.Point(k).y & ");"
                                End If
                                If Not SQLiteNoQuery(con, query, False) Then Throw New Exception("Error inserting shape coordinate In database: " & query)
                            Next
                        Next
                    Next
                    UpdateProgressBar("Done.", 0, 10)
                Case Else
                    Throw New Exception("This type of shapefile is not yet supported for export to database: " & ShapeFile)
            End Select
            sf.Close()
            con.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub DeleteShapeFile(ByVal ShpPath As String)
        Dim myPath As String = ShpPath
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
        myPath = Replace(ShpPath, ".shp", ".shx", , , CompareMethod.Text)
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
        myPath = Replace(ShpPath, ".shp", ".dbf", , , CompareMethod.Text)
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
        myPath = Replace(ShpPath, ".shp", ".prj", , , CompareMethod.Text)
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
    End Sub

    Public Function CopyShapeFile(ByVal SourceShpPath As String, TargetShpPath As String, Optional ByVal Overwrite As Boolean = True) As Boolean
        Try
            Dim mySourcePath As String
            Dim myTargetPath As String
            If System.IO.File.Exists(SourceShpPath) Then System.IO.File.Copy(SourceShpPath, TargetShpPath, Overwrite) 'copy the shp file
            mySourcePath = Replace(SourceShpPath, ".shp", ".shx", , , CompareMethod.Text)
            myTargetPath = Replace(TargetShpPath, ".shp", ".shx", , , CompareMethod.Text)
            If System.IO.File.Exists(mySourcePath) Then System.IO.File.Copy(mySourcePath, myTargetPath, Overwrite) 'copy the shx file
            mySourcePath = Replace(SourceShpPath, ".shp", ".dbf", , , CompareMethod.Text)
            myTargetPath = Replace(TargetShpPath, ".shp", ".dbf", , , CompareMethod.Text)
            If System.IO.File.Exists(mySourcePath) Then System.IO.File.Copy(mySourcePath, myTargetPath, Overwrite) 'copy the dbf file
            mySourcePath = Replace(SourceShpPath, ".shp", ".prj", , , CompareMethod.Text)
            myTargetPath = Replace(TargetShpPath, ".shp", ".prj", , , CompareMethod.Text)
            If System.IO.File.Exists(mySourcePath) Then System.IO.File.Copy(mySourcePath, myTargetPath, Overwrite) 'copy the .prj file
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function CopyShapefile of class Generalfunctions: " & ex.Message)
            Return False
        End Try
    End Function


    Public Sub deleteGrid(ByVal GridPath As String)

        Dim myPath As String = GridPath
        Dim extention As String = Right(myPath, 4)

        'removes not only the grid file, but also the projection file, the hdr file (whatever that is) and the xml file.
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
        myPath = Replace(GridPath, extention, ".prj", , , CompareMethod.Text)
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
        myPath = Replace(GridPath, extention, ".hdr", , , CompareMethod.Text)
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
        myPath = Replace(GridPath, extention, extention & ".aux.xml", , , CompareMethod.Text)
        If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)

    End Sub

    Public Function Percentile(ByVal Values() As Double, ByVal myPercentile As Double) As Double
        'this routine calculates the requested percentile from an array of values
        'first we'll need to sort the array
        Dim lower As Long, upper As Long
        Dim nSteps As Long = Values.Count - 1
        Array.Sort(Values)
        lower = RoundUD(nSteps * myPercentile, 0, False)
        upper = RoundUD(nSteps * myPercentile, 0, True)
        Return Interpolate(lower / nSteps, Values(lower), upper / nSteps, Values(upper), myPercentile, False)
    End Function


    Public Function PercentileFromList(ByVal Values As List(Of Double), ByVal myPercentileDecimal As Double) As Double
        'this routine calculates the requested percentile from an array of values
        'first we'll need to sort the array
        Dim lower As Long, upper As Long
        Dim nSteps As Long = Values.Count - 1
        Values.Sort()
        lower = RoundUD(nSteps * myPercentileDecimal, 0, False)
        upper = RoundUD(nSteps * myPercentileDecimal, 0, True)
        Return Interpolate(lower / nSteps, Values(lower), upper / nSteps, Values(upper), myPercentileDecimal, False)
    End Function


    Public Function NextSurroundingCell(ByRef StepsDone As Integer, ByVal r As Integer, ByVal c As Integer, ByRef r1 As Integer, ByRef c1 As Integer) As Boolean
        'walks arount a grid cells and returns the next surrounding cell
        Try
            Select Case StepsDone
                Case Is = 0
                    'start with the cell straight above our cell
                    r1 = r - 1
                    c1 = c
                    StepsDone += 1
                    Return True
                Case Is = 1
                    'walk clockwise
                    r1 = r - 1
                    c1 = c + 1
                    StepsDone += 1
                    Return True
                Case Is = 2
                    r1 = r
                    c1 = c + 1
                    StepsDone += 1
                    Return True
                Case Is = 3
                    r1 = r + 1
                    c1 = c + 1
                    StepsDone += 1
                    Return True
                Case Is = 4
                    r1 = r + 1
                    c1 = c
                    StepsDone += 1
                    Return True
                Case Is = 5
                    r1 = r + 1
                    c1 = c - 1
                    StepsDone += 1
                    Return True
                Case Is = 6
                    r1 = r
                    c1 = c - 1
                    StepsDone += 1
                    Return True
                Case Is = 7
                    r1 = r - 1
                    c1 = c - 1
                    StepsDone += 1
                    Return True
                Case Else
                    Return False
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



    Public Sub MultiRenameSubDirs(StartDir As String, OldName As String, NewName As String)
        '=================================================================================
        'deze multi-rename-tool hernoemt (sub)directories die een gegeven naam hebben
        '=================================================================================

        Dim DirList As String()
        Dim OldDir As String, newDir As String

        UpdateProgressBar("Renaming in " & StartDir, 0, 10)

        DirList = Directory.GetDirectories(StartDir)
        For i = 0 To DirList.Count - 1
            UpdateProgressBar("", i, DirList.Count - 1)
            OldDir = DirList(i)

            If InStr(OldDir.Trim.ToUpper, "\" & OldName.Trim.ToUpper) > 0 Then
                newDir = Replace(DirList(i), "\" & OldName, "\" & NewName)
                FileSystem.Rename(OldDir, newDir)
                Call MultiRenameSubDirs(newDir, OldName, NewName)
            Else
                Call MultiRenameSubDirs(OldDir, OldName, NewName)
            End If
        Next
    End Sub

    Public Function CopyDirectoryContent(SourcePath As String, destinationPath As String, IncludeSubdirs As Boolean, Optional ByVal ExcludeContenFromDir As String = "") As Boolean
        Try
            Dim sourceDirectoryInfo As New System.IO.DirectoryInfo(SourcePath)

            ' If the destination folder don't exist then create it
            If Not System.IO.Directory.Exists(destinationPath) Then
                System.IO.Directory.CreateDirectory(destinationPath)
            End If

            Dim fileSystemInfo As System.IO.FileSystemInfo
            For Each fileSystemInfo In sourceDirectoryInfo.GetFileSystemInfos
                Dim destinationFileName As String =
                    System.IO.Path.Combine(destinationPath, fileSystemInfo.Name)

                ' Now check whether its a file or a folder and take action accordingly
                If TypeOf fileSystemInfo Is System.IO.FileInfo Then
                    System.IO.File.Copy(fileSystemInfo.FullName, destinationFileName, True)
                Else
                    ' Recursively call the mothod to copy all the neste folders
                    If Not fileSystemInfo.FullName = ExcludeContenFromDir Then
                        CopyDirectoryContent(fileSystemInfo.FullName, destinationFileName, IncludeSubdirs, ExcludeContenFromDir)
                    Else
                        'create the directory without the content
                        If Not Directory.Exists(destinationFileName) Then Directory.CreateDirectory(destinationFileName)
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function CollectAllFilesInDir(ByVal path As String, ByVal SubDirs As Boolean, ByVal RightSidePartOfFileName As String, ByRef Paths As Collection) As Boolean
        Dim dirInfo As New IO.DirectoryInfo(path)
        Dim fileObject As FileSystemInfo

        If SubDirs = True Then
            For Each fileObject In dirInfo.GetFileSystemInfos()
                If System.IO.Directory.Exists(fileObject.FullName) Then
                    CollectAllFilesInDir(fileObject.FullName, SubDirs, RightSidePartOfFileName, Paths)
                Else
                    If Right(fileObject.FullName, RightSidePartOfFileName.Length).ToUpper = RightSidePartOfFileName.ToUpper Then
                        Paths.Add(fileObject.FullName)
                    End If
                End If
            Next
        Else
            For Each fileObject In dirInfo.GetFileSystemInfos()
                If Not System.IO.Directory.Exists(fileObject.FullName) Then
                    If Right(fileObject.FullName, RightSidePartOfFileName.Length).ToUpper = RightSidePartOfFileName.ToUpper Then
                        Paths.Add(fileObject.FullName)
                    End If
                End If
            Next
        End If
        Return True
    End Function

    Public Sub SetComboBoxValue(ByRef myBox As Forms.ComboBox, ByVal myVal As String)
        myBox.SelectedIndex = myBox.FindStringExact(myVal)
    End Sub

    Public Sub CopyFromGrid(ByVal myGrid As Forms.DataGridView)

        myGrid.ClipboardCopyMode = Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText
        Forms.Clipboard.Clear()
        If myGrid.GetCellCount(Forms.DataGridViewElementStates.Selected) > 0 Then
            Try
                Forms.Clipboard.SetDataObject(myGrid.GetClipboardContent())
            Catch ex As System.Runtime.InteropServices.ExternalException
                MsgBox("The Clipboard could Not be accessed. Please try again.")
            End Try
        End If

    End Sub

    Public Function StatsFromDataTable(ByRef dt As DataTable, FieldName As String, ByRef minVal As Double, ByRef maxVal As Double, ByRef avgVal As Double) As Boolean
        Try
            Dim myVal As Double, myMax As Double = -9.0E+99, myMin As Double = 9.0E+99, mySum As Double, n As Long
            For i = 0 To dt.Rows.Count - 1
                If Not IsDBNull(dt.Rows(i)(FieldName)) Then
                    myVal = dt.Rows(i)(FieldName)
                    If myVal > myMax Then myMax = myVal
                    If myVal < myMin Then myMin = myVal
                    mySum += myVal
                    n += 1
                End If
            Next

            If n > 0 Then
                minVal = myMin
                maxVal = myMax
                avgVal = mySum / n
            Else
                Return False
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function FitRasterAroundExtents(extents As MapWinGIS.Extents, cellsize As Double, ByRef XMin As Double, Ymin As Double, Xmax As Double, Ymax As Double) As Boolean
        Try
            'given our extents, make sure to round down to the left and lower and round up to the right and upper
            'makes sure the minimum X coordinate is rounded down at 0 decimals
            XMin = RoundUD(extents.xMin, 0, False)
            Dim nCols As Integer = Me.setup.GeneralFunctions.RoundUD((extents.xMax - XMin) / cellsize, 0, True)
            Xmax = XMin + nCols * cellsize
            Ymin = RoundUD(extents.yMin, 0, False)
            Dim nRows As Integer = Me.setup.GeneralFunctions.RoundUD((extents.yMax - Ymin) / cellsize, 0, True)
            Ymax = Ymin + nRows * cellsize
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function PointToLineSnapping(x1 As Double, y1 As Double, x2 As Double, y2 As Double, pointx As Double, pointy As Double, SearchRadius As Double, ByRef ChainageOnSegment As Double, ByRef SnapDist As Double, Optional ByVal AllowDiagonalSnappingToEndPoints As Boolean = False) As Boolean
        '----------------------------------------------------------------------------------------------------------------------------------
        'This algorithm finds the snapping point of a point to a given line segment.
        'It does so by first rotating the line segment and the point around the starting point of the line segment such that the line segment ligns with the x-axis
        'AND that the line segment has a positive direction
        'if the x-coordinate of the (rotated) point lies within the x1 and x2 of the rotated segment, we have a perpendicular snapping situation
        'Author: Siebe Bosch
        'Date: 26 february 2017
        '----------------------------------------------------------------------------------------------------------------------------------
        Try
            'initalize SnapDist 
            SnapDist = 9.0E+33

            'carry out perpendicular snapping to the line. Do this by rotating the line and the starting point
            'perpendicular snapping is always preferred since it results in the shortest distance by definition
            Dim alpha As Double = LineAngleDegrees(x1, y1, x2, y2)              'angle of the line segment in degrees
            RotatePoint(x2, y2, x1, y1, 90 - alpha, x2, y2)                     'rotate the line so that it aligns with the x-axis
            RotatePoint(pointx, pointy, x1, y1, 90 - alpha, pointx, pointy)     'rotate the point-to-snap in a similar fashion

            If Math.Round(pointx, 2) >= Math.Round(x1, 2) AndAlso Math.Round(pointx, 2) <= Math.Round(x2, 2) AndAlso Math.Abs(pointy - y1) <= Math.Min(SearchRadius, SnapDist) Then 'this is safe since we rotated the line segment so it has a positive direction
                ChainageOnSegment = Math.Abs(pointx - x1)
                SnapDist = Math.Abs(pointy - y1)
                Return True
            ElseIf AllowDiagonalSnappingToEndPoints Then
                'no snapping perpendicular to the current segment found
                'Try the alternative method: snapping diagonally To the line's start- and endpoint
                'this feature has been introduced as optional in v1.798
                Dim DistToStartPoint As Double = Pythagoras(x1, y1, pointx, pointy)
                If DistToStartPoint < SearchRadius Then
                    SnapDist = DistToStartPoint
                    ChainageOnSegment = 0
                End If
                Dim DistToEndPOint As Double = Pythagoras(x2, y2, pointx, pointy)
                If DistToEndPOint < Math.Min(SearchRadius, DistToStartPoint) Then
                    SnapDist = DistToEndPOint
                    ChainageOnSegment = Pythagoras(x1, y1, x2, y2)
                End If
                'only return true if the snapping distance is smaller than the search radius
                If SnapDist <= SearchRadius Then Return True Else Return False
            Else
                Return False
            End If

        Catch ex As Exception
            Me.setup.Log.AddError("Error in function PointToLineSnapping.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    'Public Function PointToLineSnapDistance(x1 As Double, y1 As Double, x2 As Double, y2 As Double, pointx As Double, pointy As Double, ByRef snapDistance As Double, ByRef snapChainage As Double, ByRef Xsect As Double, ByRef Ysect As Double) As Boolean
    '    'this routine find the snap distance for a point to a line segment, specified by two points
    '    'it does so by first calculating the perpendicular line and see whether they cross
    '    'note: TRUE is only returned when the snapping point is located ON the line segment between (x1,y1) and (x2,y2)

    '    'first define the line segment that we'll snap to
    '    Dim myLine As New clsLineDefinition(Me.setup)
    '    myLine = LineFromPoints(x1, y1, x2, y2)

    '    'then define a line perpendicular to the line segment, through the point to be snapped
    '    Dim perpLine As New clsLineDefinition(Me.setup)
    '    perpLine.a = -1 / myLine.a
    '    perpLine.b = pointy - perpLine.a * pointx

    '    'find out where both lines intersect
    '    Call LineIntersection(myLine.a, myLine.b, perpLine.a, perpLine.b, Xsect, Ysect)
    '    snapDistance = Pythagoras(pointx, pointy, Xsect, Ysect)
    '    snapChainage = Pythagoras(x1, y1, Xsect, Ysect)

    '    If Xsect >= Math.Min(x1, x2) AndAlso Xsect <= Math.Max(x1, x2) AndAlso Ysect >= Math.Min(y1, y2) AndAlso Ysect <= Math.Max(y1, y2) Then
    '        Return True
    '    Else
    '        Return False
    '    End If

    'End Function

    Public Function WidthOfCircle(r As Double, zCenter As Double, z As Double) As Double
        'returns the with of a circle at a given elevation
        If Math.Abs(z - zCenter) > r Then
            Return 0
        Else
            Return 2 * Math.Sqrt(r ^ 2 - (Math.Abs(z - zCenter)) ^ 2)
        End If
    End Function

    Public Function Interpolate3Colors(fromColor As System.Drawing.Color, midColor As System.Drawing.Color, toColor As System.Drawing.Color, midVal As Double, myMin As Double, myMax As Double, myVal As Double) As System.Drawing.Color
        Dim newColor As New System.Drawing.Color
        Dim R As Integer, G As Integer, B As Integer

        If myVal < midVal Then
            R = Interpolate(myMin, fromColor.R, midVal, midColor.R, myVal)
            G = Interpolate(myMin, fromColor.G, midVal, midColor.G, myVal)
            B = Interpolate(myMin, fromColor.B, midVal, midColor.B, myVal)
        Else
            R = Interpolate(midVal, midColor.R, myMax, toColor.R, myVal)
            G = Interpolate(midVal, midColor.G, myMax, toColor.G, myVal)
            B = Interpolate(midVal, midColor.B, myMax, toColor.B, myVal)
        End If
        newColor = System.Drawing.Color.FromArgb(R, G, B)
        Return newColor
    End Function

    Public Function Interpolate2Colors(fromColor As System.Drawing.Color, toColor As System.Drawing.Color, myMin As Double, myMax As Double, myVal As Double) As System.Drawing.Color
        Dim newColor As New System.Drawing.Color
        Dim R As Integer, G As Integer, B As Integer

        R = Interpolate(myMin, fromColor.R, myMax, toColor.R, myVal)
        G = Interpolate(myMin, fromColor.G, myMax, toColor.G, myVal)
        B = Interpolate(myMin, fromColor.B, myMax, toColor.B, myVal)
        newColor = System.Drawing.Color.FromArgb(R, G, B)
        Return newColor
    End Function

    Public Function FrictionToChezy(FrictionType As GeneralFunctions.enmFrictionType, FrictionValue As Double, R As Double, ByRef Chezy As Double, Optional ByVal depth As Double = 0) As Boolean
        Try
            Select Case FrictionType
                Case enmFrictionType.BOSBIJKERK
                    If depth <= 0 Then
                        Throw New Exception("Error converting Bos & Bijkerk friction value " & FrictionValue & " to Chèzy. This conversion requires a water depth > 0.")
                    Else
                        Chezy = FrictionValue * depth ^ (1 / 3) * R ^ (1 / 6)
                    End If
                Case enmFrictionType.WHITECOLEBROOK
                    Chezy = 18 * Math.Log10(12 * R / FrictionValue)
                Case enmFrictionType.MANNING
                    Chezy = R ^ (1 / 6) / FrictionValue
                Case enmFrictionType.STRICKLERKN
                    Chezy = 25 * (R / FrictionValue) ^ (1 / 6)
                Case enmFrictionType.STRICKLERKS
                    Chezy = R ^ (1 / 6) * FrictionValue
                Case enmFrictionType.CHEZY
                    Chezy = FrictionValue
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function FrictionToChezy of class GeneralFunctions: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function CalculateFrictionLoss(length As Double, FrictionType As GeneralFunctions.enmFrictionType, FrictionValue As Double, R As Double, ByRef FrictionLoss As Double) As Boolean
        Try
            'first convert our roughness value to Chezy
            Dim Chezy As Double
            FrictionToChezy(FrictionType, FrictionValue, R, Chezy)

            'then calculates friction loss in accordance with the chezy formula
            If R <= 0 Then Throw New Exception("Invalid hydraulic radius of " & R & " provided.")
            FrictionLoss = (2 * 9.81 * length) / (Chezy ^ 2 * R)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function CalculateFrictionLoss of class GeneralFunctions: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function CalculateBridgeExitLoss(ExitLossCoef As Double, Abridge As Double, Achannel As Double, ByRef ExitLoss As Double) As Boolean
        Try
            If Achannel <= 0 Then Throw New Exception("Invalid wetted perimeter for channel " & Achannel & " provided.")
            ExitLoss = ExitLossCoef * (1 - Abridge / Achannel) ^ 2
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function CalculateBridgeExitLoss of class GeneralFunctions: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function InterpolateFromDataTable(ByRef myTable As DataTable, ByVal SearchValue As Double, ByVal searchCol As Integer, ByVal returnCol As Integer) As Double
        Dim i As Long

        'decide if the table is ascending or descending
        If myTable.Rows.Count > 1 Then
            If myTable.Rows(1)(searchCol) >= myTable.Rows(0)(searchCol) Then
                'in ascending tables
                If SearchValue <= myTable.Rows(0)(searchCol) Then
                    Return myTable.Rows(0)(returnCol)
                ElseIf SearchValue >= myTable.Rows(myTable.Rows.Count - 1)(searchCol) Then
                    Return myTable.Rows(myTable.Rows.Count - 1)(returnCol)
                Else
                    For i = 0 To myTable.Rows.Count - 2
                        If myTable.Rows(i)(searchCol) <= SearchValue AndAlso myTable.Rows(i + 1)(searchCol) >= SearchValue Then
                            Return Interpolate(myTable.Rows(i)(searchCol), myTable.Rows(i)(returnCol), myTable.Rows(i + 1)(searchCol), myTable.Rows(i + 1)(returnCol), SearchValue)
                        End If
                    Next
                End If
            Else
                If SearchValue >= myTable.Rows(0)(searchCol) Then
                    Return myTable.Rows(0)(returnCol)
                ElseIf SearchValue <= myTable.Rows(myTable.Rows.Count - 1)(searchCol) Then
                    Return myTable.Rows(myTable.Rows.Count - 1)(returnCol)
                Else
                    'in descending tables
                    For i = 0 To myTable.Rows.Count - 2
                        If myTable.Rows(i)(searchCol) >= SearchValue AndAlso myTable.Rows(i + 1)(searchCol) <= SearchValue Then
                            Return Interpolate(myTable.Rows(i)(searchCol), myTable.Rows(i)(returnCol), myTable.Rows(i + 1)(searchCol), myTable.Rows(i + 1)(returnCol), SearchValue)
                        End If
                    Next
                End If
            End If
        Else
            Return 0
        End If

    End Function

    Public Function AddTimeSeriesToChart(ByRef myChart As Charting.Chart, ByVal SeriesName As String, ByRef dt As DataTable, ByVal SecondaryAxis As Boolean, Optional ByVal LineWidth As Integer = 1) As Boolean
        Try
            Dim i As Long

            myChart.Series.Add(SeriesName)
            myChart.Series(SeriesName).ChartType = Charting.SeriesChartType.FastLine
            myChart.Series(SeriesName).BorderWidth = LineWidth

            myChart.Series(SeriesName).XValueType = Charting.ChartValueType.DateTime
            If SecondaryAxis Then myChart.Series(SeriesName).YAxisType = Charting.AxisType.Secondary
            For i = 0 To dt.Rows.Count - 1
                myChart.Series(SeriesName).Points.AddXY(dt.Rows(i)(0), dt.Rows(i)(1))
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function AddSeriesToBarChart(ByRef myChart As Charting.Chart, ByVal SeriesName As String, ByRef data As Dictionary(Of String, Double), ByVal SecondaryAxis As Boolean, Optional ByVal LineWidth As Integer = 1) As Boolean
        Try
            Dim i As Long

            myChart.Series.Add(SeriesName)
            myChart.Series(SeriesName).ChartType = Forms.DataVisualization.Charting.SeriesChartType.Column
            myChart.Series(SeriesName).BorderWidth = LineWidth
            myChart.ChartAreas(0).AxisX.Interval = 1

            myChart.Series(SeriesName).XValueType = Forms.DataVisualization.Charting.ChartValueType.String
            If SecondaryAxis Then myChart.Series(SeriesName).YAxisType = Forms.DataVisualization.Charting.AxisType.Secondary
            For i = 0 To data.Values.Count - 1
                myChart.Series(SeriesName).Points.AddXY(data.Keys(i), data.Values(i))
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function GetRandom(ByVal Min As Integer, ByVal Max As Integer) As Integer
        ' by making Generator static, we preserve the same instance '
        ' (i.e., do not create new instances with the same seed over and over) '
        ' between calls '
        Static Generator As System.Random = New System.Random()
        Return Generator.Next(Min, Max)
    End Function

    Public Function ParseTimeString(ByVal TimeStr As String, ByVal TimeFormat As String, ByRef myTime As TimeSpan) As Boolean
        Try
            Dim Hours As Integer, Minutes As Integer, Seconds As Integer
            Select Case TimeFormat.Trim.ToUpper
                Case Is = "HH:MM:SS", "H:M:S"
                    Hours = ParseString(TimeStr, ":")
                    Minutes = ParseString(TimeStr, ":")
                    Seconds = ParseString(TimeStr, ":")
                    myTime = New TimeSpan(Hours, Minutes, Seconds)
                Case Else
                    Throw New Exception("Error: time formatting not yet supported: " & TimeFormat)
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ParseDateString(ByVal DateStr As String, ByVal DateFormat As String, ByRef myDate As DateTime) As Boolean

        Dim myYear As Integer, myMonth As Integer, myDay As Integer, myHour As Integer, myMinute As Integer, mySecond As Integer

        Try

            Select Case DateFormat.ToUpper
                Case Is = "YYYYMMDD"
                    myYear = Left(DateStr, 4)
                    myMonth = Left(Right(DateStr, 4), 2)
                    myDay = Right(DateStr, 2)
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "YYYYMMDDHH"
                    myYear = Left(DateStr, 4)
                    myMonth = Left(Right(DateStr, 6), 2)
                    myDay = Left(Right(DateStr, 4), 2)
                    myHour = Right(DateStr, 2)
                    myMinute = 0
                    mySecond = 0
                Case Is = "YYYYMMDDHHMM"
                    myYear = Left(DateStr, 4)
                    myMonth = Right(Left(DateStr, 6), 2)
                    myDay = Right(Left(DateStr, 8), 2)
                    myHour = Right(Left(DateStr, 10), 2)
                    myMinute = Right(Left(DateStr, 12), 2)
                    mySecond = 0
                Case Is = "YYYYMMDDHHMMSS"
                    myYear = Left(DateStr, 4)
                    myMonth = Right(Left(DateStr, 6), 2)
                    myDay = Right(Left(DateStr, 8), 2)
                    myHour = Right(Left(DateStr, 10), 2)
                    myMinute = Right(Left(DateStr, 12), 2)
                    mySecond = Right(DateStr, 2)
                Case Is = "YYYY/MM/DD", "YYYY-MM-DD"
                    myYear = Left(DateStr, 4)
                    myMonth = Left(Right(DateStr, 5), 2)
                    myDay = Right(DateStr, 2)
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "YYYY/MM/DD HH:MM:SS", "YYYY-MM-DD HH:MM:SS"
                    myYear = Left(DateStr, 4)
                    myMonth = Left(Right(DateStr, 14), 2)
                    myDay = Left(Right(DateStr, 11), 2)
                    myHour = Left(Right(DateStr, 8), 2)
                    myMinute = Left(Right(DateStr, 5), 2)
                    mySecond = Right(DateStr, 2)
                Case Is = "YYYY/MM/DD HH:MM", "YYYY-MM-DD HH:MM"
                    myYear = Left(DateStr, 4)
                    myMonth = Left(Right(DateStr, 11), 2)
                    myDay = Left(Right(DateStr, 8), 2)
                    myHour = Left(Right(DateStr, 5), 2)
                    myMinute = Right(DateStr, 2)
                    mySecond = 0
                Case Is = "YYYY/MM/DD HH", "YYYY-MM-DD HH"
                    myYear = Left(DateStr, 4)
                    myMonth = Left(Right(DateStr, 8), 2)
                    myDay = Left(Right(DateStr, 5), 2)
                    myHour = Right(DateStr, 2)
                    myMinute = 0
                    mySecond = 0
                Case Is = "DD/MM/YYYY", "DD-MM-YYYY"
                    myYear = Right(DateStr, 4)
                    myMonth = Left(Right(DateStr, 7), 2)
                    myDay = Left(DateStr, 2)
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "MM/DD/YYYY", "MM-DD-YYYY"
                    myYear = Right(DateStr, 4)
                    myMonth = Left(DateStr, 2)
                    myDay = Left(Right(DateStr, 7), 2)
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "YYYY-MM-DD"
                    myYear = Left(DateStr, 4)
                    myMonth = Right(Left(DateStr, 7), 2)
                    myDay = Right(DateStr, 2)
                    myHour = 0
                    myMinute = 0
                    mySecond = 0
                Case Is = "YYYY-MM-DD:HH"
                    myYear = Left(DateStr, 4)
                    myMonth = Right(Left(DateStr, 7), 2)
                    myDay = Right(Left(DateStr, 10), 2)
                    myHour = Right(DateStr, 2)
                    myMinute = 0
                    mySecond = 0
                Case Else
                    Throw New Exception("Date formatting not recognize: " & DateFormat)
            End Select
            myDate = New DateTime(myYear, myMonth, myDay, myHour, myMinute, mySecond)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Date format not (yet) supported for parsing: " & DateFormat)
            Return False
        End Try





        'Dim yearstart As Integer, monthstart As Integer
        'Dim daystart As Integer, hourstart As Integer
        'Dim minstart As Integer, secstart As Integer

        'If (DateFormat = "" OrElse DateFormat.ToLower = "numeric") AndAlso IsNumeric(DateStr) Then
        '  Return Date.FromOADate(Val(DateStr))
        'Else
        '  yearstart = InStr(DateFormat, "y", CompareMethod.Text)
        '  If InStr(DateFormat, "yyyy", CompareMethod.Binary) > 0 Then
        '    myYear = Val(Mid(DateStr, yearstart, 4))
        '  Else
        '    myYear = Val("19" & Mid(DateStr, yearstart, 2))
        '  End If

        '  monthstart = InStr(DateFormat, "M", CompareMethod.Binary)
        '  If monthstart > 0 Then myMonth = Val(Mid(DateStr, monthstart, 2))

        '  daystart = InStr(DateFormat, "d", CompareMethod.Text)
        '  If daystart > 0 Then myDay = Val(Mid(DateStr, daystart, 2))

        '  hourstart = InStr(DateFormat, "h", CompareMethod.Text)
        '  If hourstart > 0 Then myHour = Val(Mid(DateStr, hourstart, 2))

        '  minstart = InStr(DateFormat, "M", CompareMethod.Binary)
        '  If minstart > 0 Then myMinute = Val(Mid(DateStr, minstart, 2))

        '  secstart = InStr(DateFormat, "s", CompareMethod.Text)
        '  If secstart > 0 Then mySecond = Val(Mid(DateStr, secstart, 2))

        '  myDate = New Date(myYear, myMonth, myDay, myHour, myMinute, mySecond)
        '  Return myDate
        'End If

    End Function

    Public Sub DataGridViewRowNumbers(ByRef myGrid As Forms.DataGridView)
        Dim i As Integer
        For i = 0 To myGrid.Rows.Count - 1
            myGrid.Rows(i).HeaderCell.Value = i + 1
        Next
    End Sub

    Public Function ListValuesFromDataGridViewComboboxColumn(ByRef myGrid As Windows.Forms.DataGridView, ColIdx As Integer) As List(Of String)
        Dim i As Integer, MyList As New List(Of String)
        For i = 0 To myGrid.Rows.Count - 1
            If Not myGrid.Rows(i).Cells(ColIdx).Value Is Nothing Then
                MyList.Add(myGrid.Rows(i).Cells(ColIdx).Value.ToString())
            Else
                MyList.Add("") 'we must add an empty value to keep our list aligned with the number of rows in the datatable
            End If
        Next
        Return MyList
    End Function

    Public Function PreselectDataGridComboboxColumnValues(ByRef myGrid As Windows.Forms.DataGridView, ColIdx As Integer, PumpFieldsList As List(Of String)) As Boolean
        Dim CellBox As Windows.Forms.DataGridViewComboBoxCell
        For i = 0 To myGrid.Rows.Count - 1
            CellBox = CType(myGrid.Rows(i).Cells(ColIdx), Windows.Forms.DataGridViewComboBoxCell)
            If PumpFieldsList.Count - 1 >= i AndAlso CellBox.Items.Contains(PumpFieldsList(i)) Then
                CellBox.Value = PumpFieldsList(i)
            End If
        Next
    End Function

    Public Function RemoveNonNumericCharactersFromString(myStr As String) As Double
        Dim NumVal As String = ""
        For i = 1 To myStr.Length
            If IsNumeric(Mid(myStr, i, 1)) OrElse Mid(myStr, i, 1) = "." Then
                NumVal &= Mid(myStr, i, 1)
            End If
        Next
        If IsNumeric(NumVal) Then
            Return Val(NumVal)
        Else
            Return Double.NaN
        End If
    End Function


    Public Function AbsoluteToRelativePath(ByVal referenceDir As [String], ByVal adjustPath As [String], ByRef Result As String) As Boolean
        Try
            If System.IO.File.Exists(referenceDir) Then Throw New Exception("Error in function AbsoluteToRelativePath: variable must be a directory, not a file.")
            If Right(referenceDir, 1) <> "\" Then referenceDir &= "\"

            If [String].IsNullOrEmpty(referenceDir) Then
                'Throw New ArgumentNullException("fromPath")
                Result = adjustPath
                Return True
            End If
            If [String].IsNullOrEmpty(adjustPath) Then
                Result = adjustPath
                Return True
                'Throw New ArgumentNullException("toPath")
            End If

            Dim fromUri As New Uri(referenceDir)
            Dim toUri As New Uri(adjustPath)

            If fromUri.Scheme <> toUri.Scheme Then
                Result = adjustPath
                Return True
            End If
            ' path can't be made relative.
            Dim relativeUri As Uri = fromUri.MakeRelativeUri(toUri)
            Dim relativePath As [String] = Uri.UnescapeDataString(relativeUri.ToString())

            If toUri.Scheme.ToUpperInvariant() = "FILE" Then
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            End If
            'relativePath = ".\" & relativePath

            Result = relativePath
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function
    Public Function RelativeToAbsolutePath(ByVal myPath As String, ByVal myRootDir As String) As String
        If myPath.Trim = "" Then
            Return String.Empty
        ElseIf Mid(myPath.Trim, 2, 2) = ":\" Then
            'the given path is not relative but absolute. Do not change it!
            Return myPath
        Else
            ' Ensure the root directory does not end with a backslash
            If myRootDir.EndsWith("\") Then
                myRootDir = myRootDir.TrimEnd("\")
            End If

            While Left(myPath, 3) = "..\"
                myPath = Mid(myPath, 4)
                Dim myDirInfo As DirectoryInfo = Directory.GetParent(myRootDir)
                If myDirInfo IsNot Nothing Then
                    myRootDir = myDirInfo.FullName
                End If
            End While

            If Left(myPath, 2) = ".\" Then
                myPath = Mid(myPath, 3)
            End If

            Return Path.Combine(myRootDir, myPath)
        End If
    End Function



    Public Function MileageOneUp(ByVal startNums As List(Of Integer), ByVal endNums As List(Of Integer), ByRef Stand As List(Of Integer)) As Boolean
        'werkt als een kilometerteller. Als het hectometergetal boven z'n maximum komt, springt hij terug naar nul
        'en gaat het getalletje ervoor een omhoog et cetera. Produceert TRUE bij succes
        'produceert FALSE als hij aan z'n eind is gekomen en niet verder kan ophogen
        Dim i As Integer, j As Integer
        Dim Done As Boolean, ThisIsTheEnd As Boolean

        'errorhandling
        If startNums.Count <> endNums.Count OrElse startNums.Count <> Stand.Count Then
            Me.setup.Log.AddError("Error in function MileageOneUp: the arrays must have the same size.")
            Return False
        End If

        'check whether the current state is possible. If not we'll assume it needs to be initialized and return the very first value
        For i = 0 To Stand.Count - 1
            If Stand(i) < startNums(i) OrElse Stand(i) > endNums(i) Then
                For j = 0 To Stand.Count - 1
                    Stand(j) = startNums(j)
                Next
                Return True
            End If
        Next

        'check whether the state is currently at its end. If so, return false
        ThisIsTheEnd = True
        For i = 0 To Stand.Count - 1
            If Stand(i) < endNums(i) Then
                ThisIsTheEnd = False
                Exit For
            End If
        Next
        If ThisIsTheEnd Then Return False

        'walk through the list of numbers and adjust the state
        Done = False
        i = Stand.Count
        While Not Done
            i -= 1
            If i < 0 Then
                Done = True
            ElseIf Stand(i) < endNums(i) Then
                Stand(i) += 1
                Done = True
            Else
                Stand(i) = startNums(i)
            End If
        End While
        Return True

    End Function

    Public Function DateInSeason(ByVal myDate As Date, ByVal Season As enmSeason) As Boolean
        'checks whether a given date lies within a season
        If Season = enmSeason.yearround Then
            Return True
        ElseIf Season = enmSeason.meteosummerhalfyear AndAlso Me.setup.GeneralFunctions.MeteorologischHalfJaar(myDate) = enmSeason.meteosummerhalfyear Then
            Return True
        ElseIf Season = enmSeason.meteowinterhalfyear AndAlso Me.setup.GeneralFunctions.MeteorologischHalfJaar(myDate) = enmSeason.meteowinterhalfyear Then
            Return True
        ElseIf Season = enmSeason.meteosummerquarter AndAlso Me.setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteosummerquarter Then
            Return True
        ElseIf Season = enmSeason.meteoautumnquarter AndAlso Me.setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteoautumnquarter Then
            Return True
        ElseIf Season = enmSeason.meteowinterquarter AndAlso Me.setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteowinterquarter Then
            Return True
        ElseIf Season = enmSeason.meteospringquarter AndAlso Me.setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteospringquarter Then
            Return True
        ElseIf Season = enmSeason.hydrosummerhalfyear AndAlso Me.setup.GeneralFunctions.HydrologischHalfJaar(myDate) = enmSeason.hydrosummerhalfyear Then
            Return True
        ElseIf Season = enmSeason.hydrowinterhalfyear AndAlso Me.setup.GeneralFunctions.HydrologischHalfJaar(myDate) = enmSeason.hydrowinterhalfyear Then
            Return True
        ElseIf Season = enmSeason.marchthroughoctober AndAlso (Month(myDate) >= 3 AndAlso Month(myDate) <= 10) Then
            Return True
        ElseIf Season = enmSeason.novemberthroughfebruary AndAlso (Month(myDate) <= 2 OrElse Month(myDate) >= 11) Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub POTANALYSIS(ByVal Dates() As DateTime, ByVal Values() As Double, ByVal DurationHours As Integer, ByVal MinimumHoursBetweenEvents As Integer, ByVal PotExceedanceFrequencyPerYear As Integer, ByVal IncludeSummer As Boolean, ByVal IncludeWinter As Boolean, ByRef PotSeries As clsPOTSeries) ' ByRef POTVals As List(Of Double), ByRef POTIdx As List(Of Long), ByRef PotDates As List(Of DateTime))
        '---------------------------------------------------------------------------------------------------------------------------------------------------
        'Datum: 29-7-2014
        'Auteur: Siebe Bosch
        'Deze routine indexeert de zwaarste gebeurtenissen uit een opgegeven range en schrijft de indexnummers naar een lijst
        'Bovendien maakt hij een overzicht van alle bijkomende volumes, eveneens in een lijst
        '---------------------------------------------------------------------------------------------------------------------------------------------------
        Dim i As Long, j As Long, myIdx As Integer
        Dim maxSum As Double, maxIdx As Long, maxDate As DateTime
        Dim StartDate As DateTime, SecondDate As DateTime, EndDate As DateTime, nDays As Long
        Dim MaxEvents As Long
        Dim TimeStepHours As Integer, DurationSteps As Integer
        Dim MinimumTimeStepsBetweenEvents As Integer
        Dim POTresult As clsPOTResult

        'zoek de start- en einddatum en bereken het gewenste aantal overschrijdingen van de POT-waarde
        StartDate = Dates(0)
        SecondDate = Dates(1)
        EndDate = Dates(Dates.Count - 1)

        nDays = EndDate.Subtract(StartDate).TotalDays
        TimeStepHours = SecondDate.Subtract(StartDate).TotalHours
        MaxEvents = nDays / 365.25 * PotExceedanceFrequencyPerYear
        DurationSteps = DurationHours / TimeStepHours
        MinimumTimeStepsBetweenEvents = MinimumHoursBetweenEvents / TimeStepHours

        'create a moving window array that contains the sum of each window
        Dim movingWindowSum() As Double, inUseSum() As Integer
        ReDim movingWindowSum(Dates.Count - 1)
        ReDim inUseSum(Dates.Count - 1)
        For i = 0 To Dates.Count - DurationSteps
            For j = i To i + DurationSteps - 1
                movingWindowSum(i) += Values(j)
            Next
        Next

        'next walk through the moving window array to find the highest volumes, starting with the maximum (rank 1) and moving up in rank (=lower volume)
        For myIdx = 1 To MaxEvents
            maxSum = 0

            'make a distinction between summer and winter if required
            If IncludeSummer = True And IncludeWinter = True Then
                For i = 0 To Dates.Count - DurationSteps
                    If movingWindowSum(i) = 0 Then
                        i = i + DurationSteps - 1
                    ElseIf movingWindowSum(i) > maxSum And inUseSum(i) = 0 Then
                        maxSum = movingWindowSum(i)
                        maxIdx = i
                        maxDate = Dates(i)
                    End If
                Next
            ElseIf IncludeSummer = True Then
                For i = 1 To Dates.Count - DurationSteps
                    If movingWindowSum(i) = 0 Then
                        i = i + DurationSteps - 1
                    ElseIf movingWindowSum(i) > maxSum And inUseSum(i) = 0 Then
                        If MeteorologischHalfJaar(Dates(i)) = enmSeason.meteosummerhalfyear Then
                            maxSum = movingWindowSum(i)
                            maxIdx = i
                            maxDate = Dates(i)
                        End If
                    End If
                Next
            ElseIf IncludeWinter = True Then
                For i = 1 To Dates.Count - DurationSteps
                    If movingWindowSum(i) = 0 Then
                        i = i + DurationSteps - 1
                    ElseIf movingWindowSum(i) > maxSum And inUseSum(i) = 0 Then
                        If MeteorologischHalfJaar(Dates(i)) = enmSeason.meteowinterhalfyear Then
                            maxSum = movingWindowSum(i)
                            maxIdx = i
                            maxDate = Dates(i)
                        End If
                    End If
                Next
            End If

            'zet de relevante velden in de inUse-array plus uitloop voor de minimumruimte tussen twee events op 'bezet'. Let op: ook een stuk terug! de array bevat immers een vooruitblik
            For i = maxIdx To Math.Min(maxIdx + DurationSteps - 1 + MinimumTimeStepsBetweenEvents, Dates.Count - 1)
                inUseSum(i) = 1
            Next
            For i = (maxIdx) To Math.Max(0, (maxIdx - DurationSteps + 1 - MinimumTimeStepsBetweenEvents)) Step -1
                inUseSum(i) = 1
            Next

            'store the maximum found and 'lock' the corresponding values by setting the index number in the inuseSum array
            POTresult = New clsPOTResult With {
                .Idx = maxIdx,
                .Waarde = maxSum,
                .Datum = maxDate
            }
            PotSeries.Results.Add(POTresult.Idx, POTresult)
        Next

    End Sub

    Public Function SeasonFromString(ByVal myString As String, ByRef mySeason As STOCHLIB.GeneralFunctions.enmSeason) As Boolean
        Select Case myString.Trim.ToLower
            Case ""
                Return False
            Case "year round", "yearround", "jaarrond", "jaar"
                mySeason = enmSeason.yearround
            Case "summerhalfyear", "summer half year", "summer halfyear", "zomerhalfjaar", "meteo summer halfyear"
                mySeason = enmSeason.meteosummerhalfyear
            Case "winterhalfyear", "winter half year", "winter halfyear", "winterhalfjaar", "meteo winter halfyear"
                mySeason = enmSeason.meteowinterhalfyear
            Case "summerquarter", "summer quarter", "zomerkwartaal", "meteo summer"
                mySeason = enmSeason.meteosummerquarter
            Case "autumnquarter", "autumn quarter", "herfstkwartaal", "herfst", "meteo autumn"
                mySeason = enmSeason.meteoautumnquarter
            Case "winterquarter", "winter quarter", "winterkwartaal", "meteo winter"
                mySeason = enmSeason.meteowinterquarter
            Case "springquarter", "spring quarter", "voorjaar", "lente", "voorjaarskwartaal", "meteo spring"
                mySeason = enmSeason.meteospringquarter
            Case "hydrozomer", "hydro summer", "hydro summer halfyear", "hydrosummerhalfyear", "hydrological summer", "hydrologische zomer", "hydrologisch zomerhalfjaar", "hydro zomerhalfjaar"
                mySeason = enmSeason.hydrosummerhalfyear
            Case "hydrozomer", "hydro winter", "hydro winter halfyear", "hydrowinterhalfyear", "hydrological winter", "hydrologische winter", "hydrologisch winterhalfjaar", "hydro winterhalfjaar"
                mySeason = enmSeason.hydrowinterhalfyear
            Case "mar-oct", "mar - oct", "march through october", "march - october", "march-october"
                mySeason = enmSeason.marchthroughoctober
            Case "nov-feb", "nov - feb", "november through february", "november - february", "november-february"
                mySeason = enmSeason.novemberthroughfebruary
            Case "growthseason"
                mySeason = enmSeason.growthseason
            Case "outsidegrowthseason"
                mySeason = enmSeason.outsidegrowthseason
            Case Else
                Return False
        End Select
        Return True
    End Function

    Public Function MaxIdxFromArrayOfDouble(ByRef myArray As Double()) As Integer
        'returns the index number of the highest value in an array of doubles
        Dim MaxIdx As Integer = -1
        Dim MaxVal As Double = -9.0E+99
        For i = 0 To myArray.Count - 1
            If myArray(i) > MaxVal Then
                MaxIdx = i
                MaxVal = myArray(i)
            End If
        Next
        Return MaxIdx
    End Function

    Public Function MeteorologischHalfJaar(ByRef myDate As DateTime) As enmSeason
        If Month(myDate) <= 3 OrElse Month(myDate) >= 10 Then
            Return enmSeason.meteowinterhalfyear
        Else
            Return enmSeason.meteosummerhalfyear
        End If
    End Function

    Public Function MeteorologischSeizoen(ByRef myDate As DateTime) As enmSeason
        'meteo lente: 1 maart
        If myDate.Month <= 2 OrElse myDate.Month >= 12 Then
            Return enmSeason.meteowinterquarter
        ElseIf myDate.Month >= 3 AndAlso myDate.Month <= 5 Then
            Return enmSeason.meteospringquarter
        ElseIf myDate.Month >= 6 AndAlso myDate.Month <= 8 Then
            Return enmSeason.meteosummerquarter
        ElseIf myDate.Month >= 9 AndAlso myDate.Month <= 11 Then
            Return enmSeason.meteoautumnquarter
        End If
    End Function

    Public Function SQLiteColumnNameValid(ColName As String) As Boolean
        If IsNumeric(Left(ColName, 1)) Then
            Return False
        ElseIf IsNumeric(Right(ColName, 1)) Then
            Return False
        Else
            Return True
        End If
    End Function
    Public Function HydrologischHalfJaar(ByRef myDate As DateTime, Optional ByVal WinZomMonth As Integer = 3, Optional ByVal WinZomDay As Integer = 15, Optional ByVal ZomWinMonth As Integer = 10, Optional ByVal ZomWinDay As Integer = 15) As enmSeason
        If myDate.Month < WinZomMonth Then
            Return enmSeason.hydrowinterhalfyear
        ElseIf myDate.Month > ZomWinMonth Then
            Return enmSeason.hydrowinterhalfyear
        ElseIf myDate.Month = WinZomMonth Then
            If myDate.Day < WinZomDay Then
                Return enmSeason.hydrowinterhalfyear
            Else
                Return enmSeason.hydrosummerhalfyear
            End If
        ElseIf myDate.Month = ZomWinMonth Then
            If myDate.Day < ZomWinDay Then
                Return enmSeason.hydrosummerhalfyear
            Else
                Return enmSeason.hydrowinterhalfyear
            End If
        Else
            Return enmSeason.hydrosummerhalfyear
        End If
    End Function

    Public Sub PasteClipBoardToDataGridView(ByVal myGrid As Forms.DataGridView)

        Dim s As String
        Dim tArr() As String

        Try

            s = Forms.Clipboard.GetText()
            Dim i, ii As Integer

            If InStr(s, vbCrLf) > 0 Then
                'tArr = s.Split(ControlChars.NewLine)
                tArr = s.Split(vbCrLf)
            Else
                tArr = s.Split(vbLf)
            End If

            Dim arT() As String
            Dim cc, iRow, iCol As Integer

            iRow = myGrid.SelectedCells(0).RowIndex
            iCol = myGrid.SelectedCells(0).ColumnIndex
            For i = 0 To tArr.Length - 1
                If tArr(i).Trim <> "" Then
                    arT = tArr(i).Split(vbTab)
                    cc = iCol
                    For ii = 0 To arT.Length - 1
                        If cc > myGrid.ColumnCount - 1 Then Exit For
                        If iRow > myGrid.Rows.Count - 1 Then Exit Sub
                        myGrid.Item(cc, iRow).Value = arT(ii).TrimStart
                        cc = cc + 1
                    Next
                    iRow = iRow + 1
                End If
            Next

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            MsgBox("Error pasting values into grid: " & ex.Message)
        End Try

    End Sub

    Public Sub WriteSTLHeader(ByRef writer As StreamWriter, ModelName As String)
        writer.WriteLine("solid " & ModelName)
    End Sub

    Public Sub WriteSTLFooter(ByRef writer As StreamWriter, ModelName As String)
        writer.WriteLine("endsolid " & ModelName)
    End Sub

    Public Function WriteSTLTriangle(ByRef writer As StreamWriter, p1 As clsXYZ, p2 As clsXYZ, p3 As clsXYZ) As Boolean
        Try
            'this function writes a triangle to STL format
            'important: p1 through p3 must be defined in CLOCKWISE direction when looking at the triangle from the OUTSIDE
            Dim Triangle = New List(Of clsXYZ)
            Dim Normal As clsXYZ
            Triangle.Add(p1)
            Triangle.Add(p2)
            Triangle.Add(p3)
            Normal = Me.setup.GeneralFunctions.TriangleNormal(Triangle)
            writer.WriteLine("facet normal " & Normal.X & " " & Normal.Y & " " & Normal.Z)
            writer.WriteLine("  outer loop")
            writer.WriteLine("    vertex " & p1.X & " " & p1.Y & " " & p1.Z)
            writer.WriteLine("    vertex " & p2.X & " " & p2.Y & " " & p2.Z)
            writer.WriteLine("    vertex " & p3.X & " " & p3.Y & " " & p3.Z)
            writer.WriteLine("  endloop")
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function WriteSTLPyramid(ByRef writer As StreamWriter, p1 As clsXYZ, p2 As clsXYZ, p3 As clsXYZ, ptopbot As clsXYZ) As Boolean
        Try
            'this function writes a pyramid to STL.
            'p1, p2 and p3 are the ground vertices. They must be defined in clockwise direction, looking from outside
            'ptopbot represents the top or bottom vertex (depending on the pyramid's orientation')
            WriteSTLTriangle(writer, p1, p2, p3)        'ground face
            WriteSTLTriangle(writer, p1, ptopbot, p2)      'face 1
            WriteSTLTriangle(writer, p3, ptopbot, p1)      'face 2
            WriteSTLTriangle(writer, p2, ptopbot, p3)      'face 3
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function WriteSTL3DBlock(ByRef writer As StreamWriter, p1bot As clsXYZ, p2bot As clsXYZ, p3bot As clsXYZ, p4bot As clsXYZ, p1top As clsXYZ, p2top As clsXYZ, p3top As clsXYZ, p4top As clsXYZ) As Boolean
        Try
            'a rectangular block is in fact made up out of 4 pyramids
            'the pbot's must be defined in clockwise direction when looking from outside the block
            'also: the ptop's must be exactly above the pbot's
            WriteSTLPyramid(writer, p1bot, p2bot, p4bot, p1top)
            WriteSTLPyramid(writer, p3bot, p4bot, p2bot, p3top)
            WriteSTLPyramid(writer, p2top, p3top, p1top, p2bot)
            WriteSTLPyramid(writer, p4top, p1top, p3top, p4bot)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Function FindDataGridViewCells(ByVal myGrid As Forms.DataGridView, ByVal RowHeader As String, ByVal ColHeader As String, ByRef RowIdx As Integer, ByRef ColIdx As Integer) As Boolean
        '---------------------------------------------------------------------------------
        'Deze functie zoekt een cel in een DataGridView op, op basis van headercellteksten
        '---------------------------------------------------------------------------------
        RowIdx = -1
        ColIdx = -1
        For Each row As Forms.DataGridViewRow In myGrid.Rows
            RowIdx += 1
            If myGrid.Rows(RowIdx).HeaderCell.Value.ToString.ToLower = RowHeader.ToLower Then
                For Each column As Forms.DataGridViewColumn In myGrid.Columns
                    ColIdx += 1
                    If myGrid.Columns(ColIdx).HeaderText.ToLower = ColHeader.ToLower Then
                        Return True
                    End If
                Next
            End If
        Next

    End Function


    Public Sub RemoveAllDataGridViewColumns(ByRef mygrid As Forms.DataGridView, Optional ByVal LeaveOneEmptyColumn As Boolean = False)
        Dim i As Integer
        If LeaveOneEmptyColumn = True Then
            mygrid.Columns.Add("", "")
            For i = mygrid.Columns.Count - 2 To 0 Step -1
                mygrid.Columns.Remove(mygrid.Columns(i).Name)
            Next
        Else
            For i = mygrid.Columns.Count - 1 To 0 Step -1
                mygrid.Columns.Remove(mygrid.Columns(i).Name)
            Next
        End If
    End Sub

    Public Sub RemoveAllDataGridViewRows(ByRef mygrid As Forms.DataGridView)
        Dim i As Integer
        For i = mygrid.Rows.Count - 1 To 0 Step -1
            mygrid.Rows.Remove(mygrid.Rows(i))
        Next
    End Sub


    Public Function populateDataGridViewComboBoxColumnFromQuery(ByRef myCol As System.Windows.Forms.DataGridViewComboBoxColumn, ByVal myQuery As String) As Boolean
        Dim myTable As New DataTable
        myCol.Items.Clear()
        If Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, myQuery, myTable, True) Then
            For i = 0 To myTable.Rows.Count - 1
                myCol.Items.Add(myTable.Rows(i)(0))
            Next
        End If
    End Function

    Public Function populateDataGridViewTextColumnFromQuery(ByRef myGrid As Windows.Forms.DataGridView, ColIdx As Integer, ByVal myQuery As String) As Boolean
        Dim myTable As New DataTable
        myGrid.Rows.Clear()
        If Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, myQuery, myTable, True) Then
            For i = 0 To myTable.Rows.Count - 1
                myGrid.Rows.Add()
                myGrid.Rows(myGrid.Rows.Count - 1).Cells(ColIdx).Value = myTable.Rows(i)(0)
            Next
        End If
    End Function

    Public Function populateDataGridViewComboBoxColumnFromList(ByRef myCol As System.Windows.Forms.DataGridViewComboBoxColumn, ByVal myList As List(Of String)) As Boolean
        myCol.Items.Clear()
        For Each myString As String In myList
            myCol.Items.Add(myString)
        Next
    End Function

    Public Function populateComboBoxFromList(ByRef myList As List(Of String), ByRef myComboBox As Windows.Forms.ComboBox) As Boolean
        myComboBox.Items.Clear()
        For Each myString As String In myList
            myComboBox.Items.Add(myString)
        Next
    End Function

    Public Function populateComboBoxFromDictionary(ByRef myList As Dictionary(Of String, String), ByRef myComboBox As Windows.Forms.ComboBox) As Boolean
        myComboBox.Items.Clear()
        For Each myString As String In myList.Values
            myComboBox.Items.Add(myString)
        Next
    End Function

    Public Function populateDataGridViewComboBoxColumnListWithShapeFieldValues(ByRef myDataGrid As Windows.Forms.DataGridView, ColIdx As Integer, ByVal ShapeFilePath As String, ByVal ShapeField As String) As Boolean
        'this function populates the dropdownlists in a datagridview combobox column with list of unique values from a given shapefield
        Try
            myDataGrid.Rows.Clear()
            Dim myList As New List(Of String)

            If ShapeFilePath = "" Then Throw New Exception("Error: empty shapefile path specified.")
            If Not System.IO.File.Exists(ShapeFilePath) Then Throw New Exception("Error: specified shapefile does not exist: " & ShapeFilePath)
            If ShapeField = "" Then Throw New Exception("Error: empty shape field specified.")
            If Not Me.setup.GeneralFunctions.ListUniqueValuesFromShapefile(ShapeFilePath, ShapeField, myList) Then Throw New Exception("Error populating unique values from shapefile for field " & ShapeField)

            'first make sure that the dropdown boxes are populated
            Dim myCol As Windows.Forms.DataGridViewComboBoxColumn = DirectCast(myDataGrid.Columns(ColIdx), Windows.Forms.DataGridViewComboBoxColumn)
            myCol.Items.Clear()
            For Each myValue As String In myList
                myCol.Items.Add(myValue)
            Next

            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
    End Function



    Public Function populateDataGridViewComboBoxColumnWithShapeFieldValues(ByRef myDataGrid As Windows.Forms.DataGridView, ColIdx As Integer, ByVal ShapeFilePath As String, ByVal ShapeField As String) As Boolean
        'this function populates the entire datagridview with a row for each unique value in a shapefield
        Try
            myDataGrid.Rows.Clear()
            Dim myList As New List(Of String)

            If ShapeFilePath = "" Then Throw New Exception("Error: empty shapefile path specified.")
            If Not System.IO.File.Exists(ShapeFilePath) Then Throw New Exception("Error: specified shapefile does not exist: " & ShapeFilePath)
            If ShapeField = "" Then Throw New Exception("Error: empty shape field specified.")
            If Not Me.setup.GeneralFunctions.ListUniqueValuesFromShapefile(ShapeFilePath, ShapeField, myList) Then Throw New Exception("Error populating unique values from shapefile for field " & ShapeField)

            'first make sure that the dropdown boxes are populated
            Dim myCol As Windows.Forms.DataGridViewComboBoxColumn = DirectCast(myDataGrid.Columns(ColIdx), Windows.Forms.DataGridViewComboBoxColumn)
            myCol.Items.Clear()
            For Each myValue As String In myList
                myCol.Items.Add(myValue)
            Next

            For Each myValue As String In myList
                Dim rowIndex As Integer = myDataGrid.Rows.Add
                myDataGrid.Rows(rowIndex).Cells(ColIdx).Value = myValue
            Next
            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
    End Function


    Public Function populateDataGridViewComboBoxShapeFields(ByVal ShapeFilePath As String, ByRef myCol As System.Windows.Forms.DataGridViewComboBoxColumn) As Boolean
        Try
            Dim mySF As New MapWinGIS.Shapefile
            Dim myField As String, i As Integer
            'first clear the combobox
            myCol.Items.Clear()
            myCol.Items.Add("") 'always ad an empty cell to allow the user to clear the selection
            If mySF.Open(ShapeFilePath) Then
                For i = 0 To mySF.NumFields - 1
                    myField = mySF.Field(i).Name.Trim.ToUpper  'siebe: made case insensitive.
                    myCol.Items.Add(myField)
                Next
                mySF.Close()
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function populateDataGridViewComboBoxShapeFields.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function populateComboboxWithTextFileColumns(Path As String, Delimiter As String, CSVContainsHeader As Boolean, ByRef cmb As System.Windows.Forms.ComboBox) As Boolean
        Try
            'we 'll read the first line of our csv
            Dim myCol As String
            cmb.Items.Clear()
            Using csvReader As New StreamReader(Path)
                Dim FirstLine As String = csvReader.ReadLine
                While Not FirstLine = ""
                    myCol = ParseString(FirstLine, Delimiter)
                    If CSVContainsHeader Then
                        cmb.Items.Add(myCol)
                    Else
                        cmb.Items.Add(cmb.Items.Count + 1)  'geen header, dus kolomnummer
                    End If
                End While
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function populatecomboboxWithTextFileColumns: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function PopulateComboBoxShapeFields(ByVal ShapeFilePath As String, ByRef cmb As System.Windows.Forms.ComboBox, ErrorLevel As enmErrorLevel, Optional ByVal PreSelect As String = "") As Boolean
        Try
            Dim mySF As New MapWinGIS.Shapefile
            Dim myField As String, i As Integer
            'first clear the combobox
            cmb.Items.Clear()
            If Not System.IO.File.Exists(ShapeFilePath) Then Throw New Exception("Shapefile does not exist: " & ShapeFilePath)
            If mySF.Open(ShapeFilePath) Then
                If mySF.NumFields = 0 Then Throw New Exception("Shapefile does not contain any fields: " & ShapeFilePath)
                cmb.Items.Add("")       'add an empty field so the user can deselect any previous selection in the GUI
                For i = 0 To mySF.NumFields - 1
                    myField = mySF.Field(i).Name.Trim.ToUpper  'siebe: made case insensitive.
                    cmb.Items.Add(myField)
                Next
                'preselectie op basis van eerdere selectie
                For Each myField In cmb.Items
                    If myField.Trim.ToUpper = PreSelect.Trim.ToUpper Then cmb.SelectedItem = myField
                Next

                mySF.Close()
            End If
            Return True
        Catch ex As Exception
            Select Case ErrorLevel
                Case enmErrorLevel._Error
                    Me.setup.Log.AddError("Error in function PopulateComboBoxShapeFields while processing " & ShapeFilePath & ": " & ex.Message)
                Case enmErrorLevel._Warning
                    Me.setup.Log.AddWarning("Warning in function PopulateComboBoxShapeFields while processing " & ShapeFilePath & ": " & ex.Message)
                Case enmErrorLevel._Message
                    Me.setup.Log.AddMessage("Message from function PopulateComboBoxShapeFields while processing " & ShapeFilePath & ": " & ex.Message)
            End Select
            Return False
        End Try
    End Function




    Public Sub AddToString(ByRef myStr As String, NewText As String, AddDoubleQuotes As Boolean, AddLineBreak As Boolean)
        If AddDoubleQuotes Then myStr &= Chr(34) & NewText & Chr(34) Else myStr &= NewText
        If AddLineBreak Then myStr &= vbCrLf
    End Sub

    Public Function CreateMapServerMAPLayerSnippet(LayerName As String, ShapeFile As String, SelectionField As String, SelectionValue As String, LabelField1 As String, LabelField2 As String, Label1Prefix As String, label2Prefix As String, multiLineLabel As Boolean, Symbol As String, FillColor As System.Drawing.Color, OutlineColor As System.Drawing.Color) As String
        Try
            Dim multiLineIndicator As String = ""
            If multiLineLabel Then multiLineIndicator = "!"
            Dim myStr As String = "  LAYER" & vbCrLf
            myStr &= "      NAME " & Chr(34) & LayerName & Chr(34) & vbCrLf
            myStr &= "      DATA " & Chr(34) & ShapeFile & Chr(34) & vbCrLf
            myStr &= "      STATUS ON" & vbCrLf
            myStr &= "      TYPE POINT" & vbCrLf
            myStr &= "      LABELITEM " & Chr(34) & SelectionField & Chr(34) & vbCrLf 'will be overruled by the TEXT item under CLASS
            myStr &= vbCrLf
            myStr &= "      METADATA" & vbCrLf
            myStr &= "        " & Chr(34) & "wms_title" & Chr(34) & " " & Chr(34) & LayerName & Chr(34) & vbCrLf
            myStr &= "        " & Chr(34) & "wms_srs" & Chr(34) & " " & Chr(34) & "EPSG:4326" & Chr(34) & vbCrLf
            myStr &= "        " & Chr(34) & "wms_name" & Chr(34) & " " & Chr(34) & LayerName & Chr(34) & vbCrLf
            myStr &= "        " & Chr(34) & "wms_server_version" & Chr(34) & " " & Chr(34) & "1.1.1" & Chr(34) & vbCrLf
            myStr &= "        " & Chr(34) & "wms_format" & Chr(34) & " " & Chr(34) & "image/png" & Chr(34) & vbCrLf
            myStr &= "        " & Chr(34) & "wms_onlineresource" & Chr(34) & " " & Chr(34) & "http://localhost/cgi-bin/mapserv.exe?map=c:/Websites/SWIMM/apps/STAMP/maps/STAMP/MALI.map&" & Chr(34) & vbCrLf
            myStr &= "      END" & vbCrLf
            myStr &= vbCrLf
            myStr &= "      PROJECTION" & vbCrLf
            myStr &= "        " & Chr(34) & "init=epsg:4326" & Chr(34) & vbCrLf
            myStr &= "      END" & vbCrLf
            myStr &= vbCrLf
            myStr &= "      CLASS" & vbCrLf
            myStr &= "        EXPRESSION ('[" & SelectionField & "]' = '" & SelectionValue & "')" & vbCrLf
            myStr &= "        TEXT '"
            If LabelField1 <> "" Then myStr &= Label1Prefix & "[" & LabelField1 & "]"
            If LabelField2 <> "" Then myStr &= multiLineIndicator & label2Prefix & "[" & LabelField2 & "]"
            myStr &= "'" & vbCrLf
            myStr &= "        STYLE" & vbCrLf
            myStr &= "          SYMBOL '" & Symbol & "' " & vbCrLf
            myStr &= "          SIZE 12 " & vbCrLf
            myStr &= "          COLOR " & FillColor.R & " " & FillColor.G & " " & FillColor.B & vbCrLf
            myStr &= "          OUTLINECOLOR " & OutlineColor.R & " " & OutlineColor.G & " " & OutlineColor.B & vbCrLf
            myStr &= "          ANTIALIAS TRUE" & vbCrLf
            myStr &= "        END" & vbCrLf
            myStr &= "        LABEL" & vbCrLf
            If multiLineLabel Then myStr &= "          WRAP '" & multiLineIndicator & "'" & vbCrLf
            myStr &= "          COLOR 0 0 0 " & vbCrLf
            myStr &= "          FONT " & Chr(34) & "vera_sans" & Chr(34) & vbCrLf
            myStr &= "          TYPE truetype" & vbCrLf
            myStr &= "          SIZE 8" & vbCrLf
            myStr &= "          POSITION UC" & vbCrLf
            myStr &= "        END" & vbCrLf
            myStr &= "      END" & vbCrLf
            myStr &= "  END #LAYER" & vbCrLf
            Return myStr
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function CreateOpenLayersLayerSnippet(LayerName As String, varPrefix As String) As String
        Try
            Dim myStr As String = "            " & varPrefix & LayerName & " = New ol.layer.Image({" & vbCrLf
            myStr &= "                source:  New ol.source.ImageWMS({" & vbCrLf
            myStr &= "                    //url: localURL," & vbCrLf
            myStr &= "                    url: serverURL," & vbCrLf
            myStr &= "                    params:  {'LAYERS': '" & LayerName & "'}" & vbCrLf
            myStr &= "                    serverType: 'mapserver'" & vbCrLf
            myStr &= "                })" & vbCrLf
            myStr &= "            });" & vbCrLf
            Return myStr
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function PopulateListWithShapeFields(ByVal ShapeFilePath As String, ByRef List As List(Of String)) As Boolean
        Try
            Dim mySF As New MapWinGIS.Shapefile
            Dim myField As String, i As Integer
            'first clear the combobox
            List.Clear()
            If mySF.Open(ShapeFilePath) Then
                For i = 0 To mySF.NumFields - 1
                    myField = mySF.Field(i).Name
                    List.Add(myField)
                Next
                mySF.Close()
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function ParentDirFromDir(ByVal myDir As String) As String
        Dim ParentDir As String = ""
        Dim SlashPos As Integer, newPos As Integer
        If Right(myDir, 1) = "\" Then myDir = Left(myDir, myDir.Length - 1)

        'find the last slash
        SlashPos = 0
        newPos = InStr(myDir, "\")
        While newPos > SlashPos
            SlashPos = newPos
            newPos = InStr(SlashPos + 1, myDir, "\")
        End While

        Return Left(myDir, SlashPos - 1)

    End Function

    Function TransposeDataTable(ByVal dtOriginal As DataTable) As DataTable
        Dim dtReflection As New DataTable("Reflection")
        For i As Integer = 0 To dtOriginal.Rows.Count - 1
            dtReflection.Columns.Add(dtOriginal.Rows(i)(0))
        Next
        Dim row As DataRow
        For j As Integer = 1 To dtOriginal.Columns.Count - 1
            row = dtReflection.NewRow
            For k As Integer = 0 To dtOriginal.Rows.Count - 1
                row(k) = dtOriginal.Rows(k)(j)
            Next
            dtReflection.Rows.Add(row)
        Next
        Return dtReflection
    End Function

    Public Sub SetCellColor(ByRef ws As clsExcelSheet, rowidx As Integer, colidx As Integer, myColor As Drawing.Color)
        ws.ws.Cells(rowidx, colidx).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, myColor, myColor)
    End Sub
    Public Function BooleanToNL(ByVal myVal As Boolean) As String
        If myVal Then
            Return "ja"
        Else
            Return "nee"
        End If
    End Function
    Public Function BooleanFromText(ByVal myStr As String) As Boolean
        myStr = myStr.Trim.ToUpper

        Select Case myStr
            Case Is = "TRUE"
                Return True
            Case Is = "WAAR"
                Return True
            Case Is = "False"
                Return False
            Case Is = "ONWAAR"
                Return False
            Case Else
                Return False
        End Select

    End Function

    Public Function HasPrefix(ByVal myStr As String, ByVal myPrefix As String, ByVal CaseSensitive As Boolean) As Boolean

        'empty string is no valid prefix, so we return true immediately
        If myPrefix = "" Then Return False

        myStr = myStr.Trim
        myPrefix = myPrefix.Trim

        If CaseSensitive Then
            If Left(myStr, myPrefix.Length) = myPrefix Then
                Return True
            Else
                Return False
            End If
        Else
            If Left(myStr.ToUpper, myPrefix.Length) = myPrefix.ToUpper Then
                Return True
            Else
                Return False
            End If
        End If

    End Function
    Public Function HasPrefixFromList(ByVal myStr As String, ByVal Prefixes As List(Of String), ByVal caseSensitive As Boolean) As Boolean
        For Each myPrefix As String In Prefixes
            If Me.setup.GeneralFunctions.HasPrefix(myStr, myPrefix, caseSensitive) Then Return True
        Next
        Return False
    End Function

    'Public Function EvaluateEquation(equation As String, ByRef Result As Double) As Boolean
    '    'in this function we will evaluate an equation.
    '    'the following operators are supported: * / + -
    '    'e.g. 4*5
    '    'also operators max(a,b), min(a,b) are supported
    '    Try
    '        Dim Operand1 As Double, Operand2 As Double
    '        If IsNumeric(equation) Then
    '            Result = Val(equation)
    '        ElseIf Left(equation, 3).ToLower = "max" Then
    '            equation = ParseString(equation, "(")
    '            Operand1 = ParseString(equation, ",")
    '            Operand2 = ParseString(equation, ")")
    '            Result = Math.Max(Operand1, Operand2)
    '        ElseIf Left(equation, 3).ToLower = "min" Then
    '            equation = ParseString(equation, "(")
    '            Operand1 = ParseString(equation, ",")
    '            Operand2 = ParseString(equation, ")")
    '            Result = Math.Min(Operand1, Operand2)
    '        ElseIf InStr(equation, "*") > 0 Then
    '            Operand1 = ParseString(equation, "*")
    '            Operand2 = equation
    '            Result = Operand1 * Operand2
    '        ElseIf InStr(equation, "/") > 0 Then
    '            Operand1 = ParseString(equation, "/")
    '            Operand2 = equation
    '            Result = Operand1 / Operand2
    '        ElseIf InStr(equation, "+") > 0 Then
    '            Operand1 = ParseString(equation, "+")
    '            Operand2 = equation
    '            Result = Operand1 + Operand2
    '        ElseIf InStr(2, equation, "-") > 0 Then        'let op: instr(2 voorkomt dat je de min in een negatief eerste getal interpreteert als operator
    '            Operand1 = Strings.Left(equation, InStr(2, equation, "-") - 1)
    '            Operand2 = Strings.Right(equation, equation.Length - InStr(2, equation, "-"))
    '            Result = Operand1 - Operand2
    '        End If

    '        Return True
    '    Catch ex As Exception
    '        Return False
    '    End Try
    'End Function


    Public Function LineIntersection(ByVal a1 As Double, ByVal b1 As Double, ByVal a2 As Double, ByVal b2 As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        'finds the intersection point for two lines, both defined as y = ax + b
        'and returns the X and Y-coordinate of that point

        'y = a1 * x + b1
        'y = a2 * x + b2
        'a1 * x + b1 = a2 * x + b2
        'x(a1 - a2) = b2 - b1
        'x = (b2-b1)/(a1-a2)

        If (a1 - a2) <> 0 Then
            X = (b2 - b1) / (a1 - a2)
            Y = a1 * X + b1
            Return True
        Else
            Return False
        End If

    End Function

    Public Function sendEmail(ByVal toAddress As String, ByVal Subject As String, ByVal Body As String)
        Try
            Dim message As System.Net.Mail.MailMessage
            Dim smtp As New System.Net.Mail.SmtpClient("mail.meteobase.nl")
            Dim fromMailAddress As System.Net.Mail.MailAddress
            Dim toMailAddress As System.Net.Mail.MailAddress

            smtp.Port = 26
            fromMailAddress = New System.Net.Mail.MailAddress("info@meteobase.nl")
            toMailAddress = New System.Net.Mail.MailAddress(toAddress)
            message = New System.Net.Mail.MailMessage With {
                .From = fromMailAddress
            }
            message.To.Add(toMailAddress)
            message.Subject = Subject
            message.Body = Body

            smtp.EnableSsl = False
            smtp.UseDefaultCredentials = False
            smtp.Credentials = New System.Net.NetworkCredential("info@meteobase.nl", "@g3ntM327")
            smtp.DeliveryMethod = Net.Mail.SmtpDeliveryMethod.Network

            smtp.Send(message)

            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function getSubDirFromPath(ByVal myPath As String, ByVal RootDir As String) As String
        Dim myDir As String = Path.GetDirectoryName(myPath)
        Return Replace(myDir, RootDir, "")
    End Function

    Public Function DirFromFileName(ByVal mypath As String) As String
        Return Path.GetDirectoryName(mypath)
    End Function

    ''' <summary>
    ''' This function validates the groundwater table provided
    ''' </summary>
    ''' <param name="myGT"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function ValidateGT(ByVal myGT As String, ByVal meanSL As Double, ByVal WP As Double) As String
        myGT = myGT.Trim.ToUpper
        myGT.Replace("*", "")
        Select Case myGT.Trim.ToUpper
            Case Is = "I"
                Return "I"
            Case Is = "II"
                Return "II"
            Case Is = "III"
                Return "III"
            Case Is = "IV"
                Return "IV"
            Case Is = "V"
                Return "V"
            Case Is = "VI"
                Return "VI"
            Case Is = "VII"
                Return "VII"
            Case Else
                'roughly estimate a ground water table by the difference between the mean surface level and the target level
                'in reality these are the groundwater tables:
                'GT:   GHG            GLG
                'I                < 50
                'II               50 - 80
                'III   < 40       80 - 120
                'IV    > 40       80 - 120
                'V     < 40       > 120
                'VI    40 - 80  > 120
                'VII   80       > 120
                If (meanSL - WP) <= 0.5 Then
                    Return "I"
                ElseIf (meanSL - WP) <= 1.2 Then
                    Return "III"
                ElseIf (meanSL - WP) <= 1.5 Then
                    Return "V"
                Else
                    Return "V"
                End If
        End Select
    End Function

    Friend Sub DischargeUnitConversion(ByRef Value As Double, ByVal DistFrom As String, ByVal DistTo As String, ByVal TimeFrom As String, ByVal TimeTo As String, Optional ByVal AreaM2 As Double = 0)
        'converteert de waarde van Value van de ene eenheid naar de andere
        'bijvoorbeeld m3/s naar mm/h
        If DistFrom = DistTo AndAlso TimeFrom = TimeTo Then Exit Sub

        Select Case DistFrom
            Case Is = "m3"
                Select Case DistTo
                    Case Is = "mm"
                        Value = Value / AreaM2 * 1000
                    Case Is = "m"
                        Value = Value / AreaM2
                End Select

            Case Is = "mm"
                Select Case DistTo
                    Case Is = "m3"
                        Value = Value / 1000 * AreaM2
                    Case Is = "m"
                        Value = Value / 1000
                End Select

            Case Is = "m"
                Select Case DistTo
                    Case Is = "m3"
                        Value = Value * AreaM2
                    Case Is = "mm"
                        Value = Value * 1000
                End Select

        End Select

        Select Case TimeFrom
            Case Is = "y"
                Select Case TimeTo
                    Case Is = "d"
                        Value = Value / 365
                    Case Is = "h"
                        Value = Value / 365 / 24
                    Case Is = "min"
                        Value = Value / 365 / 24 / 60
                    Case Is = "s"
                        Value = Value / 365 / 24 / 3600
                End Select

            Case Is = "d"
                Select Case TimeTo
                    Case Is = "y"
                        Value = Value * 365
                    Case Is = "h"
                        Value = Value / 24
                    Case Is = "min"
                        Value = Value / 24 / 60
                    Case Is = "s"
                        Value = Value / 24 / 3600
                End Select

            Case Is = "h"
                Select Case TimeTo
                    Case Is = "y"
                        Value = Value * 24 * 365
                    Case Is = "d"
                        Value = Value * 24
                    Case Is = "min"
                        Value = Value / 60
                    Case Is = "s"
                        Value = Value / 3600
                End Select

            Case Is = "s"
                Select Case TimeTo
                    Case Is = "y"
                        Value = Value * 3600 * 24 * 365
                    Case Is = "d"
                        Value = Value * 3600 * 24
                    Case Is = "h"
                        Value = Value * 3600
                    Case Is = "min"
                        Value = Value * 60
                End Select
        End Select
    End Sub


    Public Function RD2WGS84(ByVal X As Double, ByVal Y As Double, ByRef Lat As Double, ByRef Lon As Double) As Boolean
        'converteert RD-coordinaten naar Lat/Long (WGS84)
        'maakt gebruik van de routines van Ejo Schrama: schrama @geo.tudelft.nl
        Dim phi As Double
        Dim lambda As Double
        Dim PhiWGS As Double
        Dim LambdaWGS As Double

        Call RD2BESSEL(X, Y, phi, lambda)
        Call BESSEL2WGS84(phi, lambda, PhiWGS, LambdaWGS)
        Lat = PhiWGS
        Lon = LambdaWGS
        Return True

    End Function

    Public Function removePrefixFromID(ByVal ID As String, ByVal Prefixes As List(Of String)) As String
        Dim Prefix As String

        'Author: Siebe Bosch
        'Date: 11-7-2013
        'Description: removes a prefix from an ID and returns the result
        For Each Prefix In Prefixes
            If Left(ID.Trim.ToUpper, Prefix.Trim.Length) = Prefix.Trim.ToUpper Then
                Return Right(ID.Trim, ID.Trim.Length - Prefix.Trim.Length)
            End If
        Next
        Return ID.Trim 'prefix not found so return the original string

    End Function

    Public Function WGS842RD(ByVal Lat As Double, ByVal Lon As Double, Optional ByRef X As Double = 0, Optional ByRef y As Double = 0) As String
        'converteert WGS84-coordinaten (Lat/Long) naar RD
        'maakt gebruik van de routines van Ejo Schrama: schrama @geo.tudelft.nl
        Dim phiBes As Double
        Dim LambdaBes As Double
        Call WGS842BESSEL(Lat, Lon, phiBes, LambdaBes)
        Call BESSEL2RD(phiBes, LambdaBes, X, y)
        WGS842RD = X & "," & y

    End Function

    Public Function RemoveBoundingQuotes(ByVal myStr As String) As String
        If Left(myStr, 1) = Chr(34) OrElse Left(myStr, 1) = "'" Then myStr = Right(myStr, myStr.Length - 1)
        If Right(myStr, 1) = Chr(34) OrElse Right(myStr, 1) = "'" Then myStr = Left(myStr, myStr.Length - 1)
        Return myStr
    End Function

    Public Function RemoveBoundingCharacters(ByVal myStr As String, Character As String) As String
        If Left(myStr, 1) = Character Then myStr = Right(myStr, myStr.Length - 1)
        If Right(myStr, 1) = Character Then myStr = Left(myStr, myStr.Length - 1)
        Return myStr
    End Function

    Public Function GetBooleanFromString(ByVal myStr As String) As Boolean
        Dim tmpStr As String = myStr.Trim.ToUpper
        Select Case tmpStr
            Case Is = "TRUE"
                Return True
            Case Is = "WAAR"
                Return True
            Case Is = "FALSE"
                Return False
            Case Is = "UNTRUE"
                Return False
            Case Is = "ONWAAR"
                Return False
            Case Else
                Return False
        End Select
    End Function

    Public Sub RD2BESSEL(ByVal X As Double, ByVal y As Double, ByRef phi As Double, ByRef lambda As Double)

        'converteert RD-coordinaten naar phi en lambda voor een Bessel-functie
        'code is geheel gebaseerd op de routines van Ejo Schrama's software:
        'schrama@geo.tudelft.nl

        Dim x0 As Double
        Dim y0 As Double
        Dim k As Double
        Dim bigr As Double
        Dim m As Double
        Dim n As Double
        Dim lambda0 As Double
        Dim phi0 As Double
        Dim l0 As Double
        Dim b0 As Double
        Dim e As Double
        Dim a As Double

        Dim d_1 As Double, d_2 As Double, r As Double, sa As Double, ca As Double, psi As Double, cpsi As Double, spsi As Double
        Dim sb As Double, cb As Double, b As Double, sdl As Double, dl As Double, w As Double, q As Double
        Dim dq As Double, i As Long, pi As Double

        x0 = 155000
        y0 = 463000
        k = 0.9999079
        bigr = 6382644.571
        m = 0.003773953832
        n = 1.00047585668

        pi = Math.PI
        'pi = 3.14159265358979
        lambda0 = pi * 0.0299313271611111
        phi0 = pi * 0.289756447533333
        l0 = pi * 0.0299313271611111
        b0 = pi * 0.289561651383333

        e = 0.08169683122
        a = 6377397.155

        d_1 = X - x0
        d_2 = y - y0
        r = Math.Sqrt(d_1 ^ 2 + d_2 ^ 2)

        If r <> 0 Then
            sa = d_1 / r
            ca = d_2 / r
        Else
            sa = 0
            ca = 0
        End If

        psi = Math.Atan2(r, k * 2 * bigr) * 2
        cpsi = Math.Cos(psi)
        spsi = Math.Sin(psi)

        sb = ca * Math.Cos(b0) * spsi + Math.Sin(b0) * cpsi
        d_1 = sb
        cb = Math.Sqrt(1 - d_1 ^ 2)
        b = Math.Acos(cb)
        sdl = sa * spsi / cb
        dl = Math.Asin(sdl)
        lambda = dl / n + lambda0
        w = Math.Log(Math.Tan(b / 2 + pi / 4))
        q = (w - m) / n

        phi = Math.Atan(Math.Exp(1) ^ q) * 2 - pi / 2 'phi prime
        For i = 1 To 4
            dq = e / 2 * Math.Log((e * Math.Sin(phi) + 1) / (1 - e * Math.Sin(phi)))
            phi = Math.Atan(Math.Exp(1) ^ (q + dq)) * 2 - pi / 2
        Next

        lambda = lambda / pi * 180
        phi = phi / pi * 180

    End Sub

    Public Function RemovetrailingbackslashFromDir(path As String) As String
        If Strings.Right(path, 1) = "\" Then
            Return Strings.Left(path, path.Length - 1)
        Else
            Return path
        End If
    End Function


    Public Sub BESSEL2WGS84(ByVal phi As Double, ByVal lambda As Double, ByRef PhiWGS As Double, ByRef LamWGS As Double)
        Dim dphi As Double, dlam As Double, phicor As Double, lamcor As Double

        dphi = phi - 52
        dlam = lambda - 5
        phicor = (-96.862 - dphi * 11.714 - dlam * 0.125) * 0.00001
        lamcor = (dphi * 0.329 - 37.902 - dlam * 14.667) * 0.00001
        PhiWGS = phi + phicor
        LamWGS = lambda + lamcor


    End Sub

    Public Sub WGS842BESSEL(ByVal PhiWGS As Double, ByVal LamWGS As Double, ByRef phi As Double, ByRef lambda As Double)
        Dim dphi As Double, dlam As Double, phicor As Double, lamcor As Double

        dphi = PhiWGS - 52
        dlam = LamWGS - 5
        phicor = (-96.862 - dphi * 11.714 - dlam * 0.125) * 0.00001
        lamcor = (dphi * 0.329 - 37.902 - dlam * 14.667) * 0.00001
        phi = PhiWGS - phicor
        lambda = LamWGS - lamcor

    End Sub

    Public Sub BESSEL2RD(ByVal phiBes As Double, ByVal lamBes As Double, ByRef X As Double, ByRef y As Double)

        'converteert Lat/Long van een Bessel-functie naar X en Y in RD
        'code is geheel gebaseerd op de routines van Ejo Schrama's software:
        'schrama@geo.tudelft.nl

        Dim x0 As Double
        Dim y0 As Double
        Dim k As Double
        Dim bigr As Double
        Dim m As Double
        Dim n As Double
        Dim lambda0 As Double
        Dim phi0 As Double
        Dim l0 As Double
        Dim b0 As Double
        Dim e As Double
        Dim a As Double

        Dim d_1 As Double, d_2 As Double, r As Double, sa As Double, ca As Double, cpsi As Double, spsi As Double
        Dim b As Double, dl As Double, w As Double, q As Double
        Dim dq As Double, pi As Double, phi As Double, lambda As Double, s2psihalf As Double, cpsihalf As Double, spsihalf As Double
        Dim tpsihalf As Double

        x0 = 155000
        y0 = 463000
        k = 0.9999079
        bigr = 6382644.571
        m = 0.003773953832
        n = 1.00047585668

        pi = Math.PI
        'pi = 3.14159265358979
        lambda0 = pi * 0.0299313271611111
        phi0 = pi * 0.289756447533333
        l0 = pi * 0.0299313271611111
        b0 = pi * 0.289561651383333

        e = 0.08169683122
        a = 6377397.155

        phi = phiBes / 180 * pi
        lambda = lamBes / 180 * pi

        q = Math.Log(Math.Tan(phi / 2 + pi / 4))
        dq = e / 2 * Math.Log((e * Math.Sin(phi) + 1) / (1 - e * Math.Sin(phi)))
        q = q - dq
        w = n * q + m
        b = Math.Atan(Math.Exp(1) ^ w) * 2 - pi / 2
        dl = n * (lambda - lambda0)
        d_1 = Math.Sin((b - b0) / 2)
        d_2 = Math.Sin(dl / 2)
        s2psihalf = d_1 * d_1 + d_2 * d_2 * Math.Cos(b) * Math.Cos(b0)
        cpsihalf = Math.Sqrt(1 - s2psihalf)
        spsihalf = Math.Sqrt(s2psihalf)
        tpsihalf = spsihalf / cpsihalf
        spsi = spsihalf * 2 * cpsihalf
        cpsi = 1 - s2psihalf * 2
        sa = Math.Sin(dl) * Math.Cos(b) / spsi
        ca = (Math.Sin(b) - Math.Sin(b0) * cpsi) / (Math.Cos(b0) * spsi)
        r = k * 2 * bigr * tpsihalf
        X = Math.Round(r * sa + x0, 0)
        y = Math.Round(r * ca + y0, 0)

    End Sub

    Friend Function IDsSimilar(ByVal refID As String, ByVal myID As String) As Boolean

        'alles in bovenkast zetten
        refID = refID.Trim.ToUpper
        myID = myID.Trim.ToUpper

        'leading zeroes toevoegen om de ID's te uniformeren
        refID = AddLeadingZeroesToID(refID, "KGM", 5)
        refID = AddLeadingZeroesToID(refID, "KST", 5)
        refID = AddLeadingZeroesToID(refID, "KDU", 5)

        myID = AddLeadingZeroesToID(myID, "KGM", 5)
        myID = AddLeadingZeroesToID(myID, "KST", 5)
        myID = AddLeadingZeroesToID(myID, "KDU", 5)

        'compares two ID's and returns a boolean whether they are similar
        If refID = myID Then
            Return True
        ElseIf refID Like "*" & myID Then
            Return True
        ElseIf refID Like "*" & myID Then
            Return True
        Else
            Return False
        End If

    End Function

    Friend Function AddLeadingZeroesToID(ByVal ID, ByVal Identifier, ByVal nChars) As String
        'deze functie vult een ID zo aan dat je een vast aantal karakters achter de identifier krigjt
        'bijvoorbeeld: KGM001 met Identifier KGM en nChars=5 wordt KGM00001

        Dim iLoc As Integer = InStr(ID, Identifier)
        Dim rStr As String, lStr As String, i As Long

        If iLoc > 0 Then
            lStr = Left(ID, iLoc - 1)
            rStr = Right(ID, ID.length - Identifier.length + 1)
            For i = nChars To rStr.Length Step -1
                rStr = "0" & rStr
            Next
            Return lStr & Identifier & rStr
        Else
            Return ID
        End If
    End Function

    Public Function Pythagoras(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As Double
        Return Math.Sqrt((X2 - X1) ^ 2 + (Y2 - Y1) ^ 2)
    End Function

    Public Function GetDirFromPath(ByVal path As String) As String
        Try
            Return path.Substring(0, path.LastIndexOf("\"))
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Friend Function GetFileNameFromPath(ByVal path As String) As String
        Try
            Return path.Substring(path.LastIndexOf("\") + 1)
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Sub SyncSobekTables(ByRef TargetTable As clsSobekTable, ByRef RefTable As clsSobekTable, ByVal ForceMinValsFromReferenceTableToTargetTable As Boolean)
        'Author: Siebe Bosch
        'Date: 4-10-2013
        'Desctription: synchronizes two sobek tables by matching their x-values
        Dim i As Integer
        Dim myList As New List(Of Double)
        Dim myDates As New List(Of Date)
        Dim newTargetTable As New clsSobekTable(Me.setup)
        Dim newRefTable As New clsSobekTable(Me.setup)
        Dim myX As Double, myYTarget As Double, myYRef As Double
        Dim myDate As Date

        'if one of both tables is empty or nothing, simply return the other
        If TargetTable Is Nothing Then
            TargetTable = RefTable
        ElseIf RefTable Is Nothing Then
            RefTable = TargetTable
        ElseIf TargetTable.Values1.Count = 0 Then
            TargetTable = RefTable
        ElseIf RefTable.Values1.Count = 0 Then
            RefTable = TargetTable
            'we'll have to interpolate after all
        ElseIf TargetTable.XValues.Values.Count > 0 Then

            'first create a dictionary with all x-values from both tables
            For i = 0 To TargetTable.XValues.Values.Count - 1
                If Not myList.Contains(TargetTable.XValues.Values(i)) Then myList.Add(TargetTable.XValues.Values(i))
            Next
            For i = 0 To RefTable.XValues.Values.Count - 1
                If Not myList.Contains(RefTable.XValues.Values(i)) Then myList.Add(RefTable.XValues.Values(i))
            Next

            'then sort it
            myList.Sort()

            'fill the new tables with interpolated values from themselves. Unless we force the minimum from the reference table to the target table
            For i = 0 To myList.Count - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", (i + 1), myList.Count)
                myX = myList.Item(i)
                myYTarget = TargetTable.InterpolateFromXValues(myX, 1)
                myYRef = RefTable.InterpolateFromXValues(myX, 1)
                newRefTable.AddDataPair(2, myX, myYRef)
                If ForceMinValsFromReferenceTableToTargetTable AndAlso myYRef > myYTarget Then
                    newTargetTable.AddDataPair(2, myX, myYRef)
                Else
                    newTargetTable.AddDataPair(2, myX, myYTarget)
                End If
            Next

        ElseIf TargetTable.Dates.Values.Count > 0 Then

            'first create a dictionary with all dates from both tables
            For i = 0 To TargetTable.Dates.Values.Count - 1
                If Not myDates.Contains(TargetTable.Dates.Values(i)) Then myDates.Add(TargetTable.Dates.Values(i))
            Next
            For i = 0 To RefTable.Dates.Values.Count - 1
                If Not myDates.Contains(RefTable.Dates.Values(i)) Then myDates.Add(RefTable.Dates.Values(i))
            Next

            'then sort it
            myDates.Sort()

            'fill it with the sum of the values from both tables
            For i = 0 To myDates.Count - 1
                myDate = myDates.Item(i)
                myYTarget = TargetTable.InterpolateFromDates(myDate, 1)
                myYRef = RefTable.InterpolateFromDates(myDate, 1)
                newRefTable.AddDatevalPair(myDate, myYRef)
                If ForceMinValsFromReferenceTableToTargetTable AndAlso myYRef > myYTarget Then
                    newTargetTable.AddDatevalPair(myDate, myYRef)
                Else
                    newTargetTable.AddDatevalPair(myDate, myYTarget)
                End If
            Next

        End If

        RefTable = newRefTable
        TargetTable = newTargetTable

    End Sub


    Public Function SumOfDataTablesFromDictionary(ByRef Tables As Dictionary(Of String, DataTable)) As DataTable
        Dim t As Integer, c As Integer, r As Long, newTable As New DataTable, newRow As DataRow

        Try
            If Tables.Count = 0 Then Throw New Exception("Error in function SumDataTablesFromDictionary: dictionary of tables was emtpy.")

            'use the first table as a basis. The title and data type for the first column will be extracte from that table
            newTable = New DataTable
            For c = 0 To Tables.Values(0).Columns.Count - 1
                newTable.Columns.Add(Tables.Values(0).Columns(c).Caption, Tables.Values(0).Columns(c).DataType)
            Next
            For r = 0 To Tables.Values(0).Rows.Count - 1
                newRow = newTable.NewRow()
                For c = 0 To Tables.Values(0).Columns.Count - 1
                    newRow(c) = Tables.Values(0).Rows(r)(c)
                Next
                newTable.Rows.Add(newRow)
            Next

            For t = 1 To Tables.Count - 1
                For r = 0 To newTable.Rows.Count - 1
                    For c = 0 To Tables.Values(t).Columns.Count - 1
                        newTable.Rows(r)(c) += Tables.Values(t).Rows(r)(c)
                    Next
                Next
            Next

            Return newTable
        Catch ex As Exception
            Me.setup.Log.AddError("in function SumOfDataTablesFromDictionary.")
            Return Nothing
        End Try


    End Function

    Public Function MergeDataTablesFromDictionary(ByRef Tables As Dictionary(Of String, DataTable)) As DataTable
        Dim i As Integer, newTable As New DataTable, myTable As DataTable, newRow As DataRow
        Dim r As Long

        Try
            'use the first table as a basis. The title and data type for the first column will be extracte from that table
            newTable = New DataTable
            newTable.Columns.Add(Tables.Values(0).Columns(0).Caption, Tables.Values(0).Columns(0).DataType)

            'add the columns
            For i = 0 To Tables.Values.Count - 1
                myTable = Tables.Values(i)
                newTable.Columns.Add(Tables.Keys(i), myTable.Columns(1).DataType)
            Next

            'add the rows
            For r = 0 To Tables.Values(0).Rows.Count - 1
                newRow = newTable.NewRow()
                newRow(0) = Tables.Values(0).Rows(r)(0)

                For i = 0 To Tables.Values.Count - 1
                    myTable = Tables.Values(i)
                    newRow(i + 1) = Convert.ToDouble(myTable.Rows(r)(1))
                Next
                newTable.Rows.Add(newRow)
            Next

            Return newTable
        Catch ex As Exception
            Me.setup.Log.AddError("error merging datatables.")
            Return Nothing
        End Try


    End Function

    Public Function AggregateDataTablesFromDictionary(ByRef Tables As Dictionary(Of String, DataTable), ValueColIdx As Integer) As DataTable
        'this function aggregates the values from multiple datatables that reside inside a dictionary
        'this means that for each row, the values from the values column for each of the tables are added up and returned in a new table
        Dim newTable As New DataTable, myTable As DataTable, newRow As DataRow
        Dim r As Long, mySum As Double

        Try
            'use the first table as a basis. The title and data type for the first column will be extracte from that table
            newTable = New DataTable
            newTable.Columns.Add(Tables.Values(0).Columns(0).Caption, Tables.Values(0).Columns(0).DataType)
            newTable.Columns.Add("Value", Tables.Values(0).Columns(ValueColIdx).DataType)

            'add the rows
            For r = 0 To Tables.Values(0).Rows.Count - 1
                newRow = newTable.NewRow()
                newRow(0) = Tables.Values(0).Rows(r)(0)
                mySum = 0
                For Each myTable In Tables.Values
                    mySum += Convert.ToDouble(myTable.Rows(r)(ValueColIdx))
                Next
                newRow(1) = mySum
                newTable.Rows.Add(newRow)
            Next

            Return newTable
        Catch ex As Exception
            Me.setup.Log.AddError("error aggregating data from datatables dictionary.")
            Return Nothing
        End Try


    End Function

    Friend Function mergeSobekTables(ByRef Table1 As clsSobekTable, ByRef Table2 As clsSobekTable) As clsSobekTable
        'Author: Siebe Bosch
        'Date: 4-10-2013
        'Desctription: merges two sobek tables by adding their values and returning the results in a new table
        Dim i As Integer
        Dim myList As New List(Of Double)
        Dim myDates As New List(Of Date)
        Dim newTable As New clsSobekTable(setup)
        Dim myX As Double, myY1 As Double, myY2 As Double
        Dim myDate As Date

        'if one of both tables is empty or nothing, simply return the other
        If Table1 Is Nothing Then
            Return Table2
        ElseIf Table2 Is Nothing Then
            Return Table1
        ElseIf Table1.Values1.Count = 0 Then
            Return Table2
        ElseIf Table2.Values1.Count = 0 Then
            Return Table1

            'if both tables have identical x values, it's simple
        ElseIf Table1.XValues.Count > 0 AndAlso Table1.XValues.Count = Table2.XValues.Count AndAlso
               Table1.XValues.Values(0) = Table2.XValues.Values(0) AndAlso
               Table1.XValues.Values(Table1.XValues.Count - 1) = Table2.XValues.Values(Table2.XValues.Count - 1) Then

            For i = 0 To Table1.XValues.Count - 1
                newTable.AddDataPair(2, Table1.XValues.Values(i), Table1.Values1.Values(i) + Table2.Values1.Values(i))
            Next

            'if both tables have identical dates, it's simple
        ElseIf Table1.Dates.Count > 0 AndAlso Table1.Dates.Count = Table2.Dates.Count AndAlso
               Table1.Dates.Values(0) = Table2.Dates.Values(0) AndAlso
               Table1.Dates.Values(Table1.Dates.Count - 1) = Table2.Dates.Values(Table2.Dates.Count - 1) Then

            For i = 0 To Table1.Dates.Count - 1
                newTable.AddDatevalPair(Table1.Dates.Values(i), Table1.Values1.Values(i) + Table2.Values1.Values(i))
            Next

            'we'll have to interpolate after all
        ElseIf Table1.XValues.Values.Count > 0 Then

            'first create a dictionary with all x-values from both tables
            For i = 0 To Table1.XValues.Values.Count - 1
                If Not myList.Contains(Table1.XValues.Values(i)) Then myList.Add(Table1.XValues.Values(i))
            Next
            For i = 0 To Table2.XValues.Values.Count - 1
                If Not myList.Contains(Table2.XValues.Values(i)) Then myList.Add(Table2.XValues.Values(i))
            Next

            'then sort it
            myList.Sort()

            'fill it with the sum of the values from both tables
            For i = 0 To myList.Count - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", (i + 1), myList.Count)
                myX = myList.Item(i)
                myY1 = Table1.InterpolateFromXValues(myX, 1)
                myY2 = Table2.InterpolateFromXValues(myX, 1)
                newTable.AddDataPair(2, myX, myY1 + myY2)
            Next

        ElseIf Table1.Dates.Values.Count > 0 Then

            'first create a dictionary with all dates from both tables
            For i = 0 To Table1.Dates.Values.Count - 1
                If Not myDates.Contains(Table1.Dates.Values(i)) Then myDates.Add(Table1.Dates.Values(i))
            Next
            For i = 0 To Table2.Dates.Values.Count - 1
                If Not myDates.Contains(Table2.Dates.Values(i)) Then myDates.Add(Table2.Dates.Values(i))
            Next

            'then sort it
            myDates.Sort()

            'fill it with the sum of the values from both tables
            For i = 0 To myDates.Count - 1
                myDate = myDates.Item(i)
                myY1 = Table1.InterpolateFromDates(myDate, 1)
                myY2 = Table2.InterpolateFromDates(myDate, 1)
                newTable.AddDatevalPair(myDate, myY1 + myY2)
            Next

        End If

        Return newTable

    End Function

    Public Function FitShapeFieldLength(FieldName As String, ByRef newName As String) As Boolean
        'this funtion attempts to ebbreviate a fieldname so that it fits the maximum length of a shapefield: 10 characters
        'if insufficient compression is achieved, the remaining parameter will be truncated
        Try
            newName = Replace(FieldName, "[", "")
            newName = Replace(newName, "]", "")
            If newName.Length <= 10 Then
                Return True
            Else
                If newName.Length > 10 Then newName = Replace(newName, "Right", "Rgt",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Left", "Lf",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Fraction", "Frc",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Wetberm", "Wtbrm",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Water", "Wtr",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Surface", "Srf",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Default", "Df",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Level", "Lv",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Capacity", "Cp",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Cap", "Cp",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Emergency", "Emg",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Hinterland", "Hnt",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Margin", "Mg",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Inlet", "In",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Outlet", "Out",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Upstream", "Up",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Downstream", "Dn",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "Down", "Dn",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "winter", "Win",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "zomer", "Zom",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "summer", "Sum",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "targetlevel", "TL",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "streefpeil", "SP",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "construction", "Cons",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "hoogte", "Hgt",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "height", "Hgt",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "breedte", "Br",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "width", "w",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "coefficient", "Coef",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "minimum", "Min",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "maximum", "Max",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "shoulder", "Shld",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "elevation", "Elev",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "total", "Tot",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "discharge", "Disch",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "contraction", "Contr",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "correction", "Cor",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "crest", "Cr",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "level", "Lvl",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "numberof", "n",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "entrance", "entr",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "length", "len",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "straight", "str",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "laagste", "min",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "hoogste", "max",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "oe", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "ie", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "ei", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "ij", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "ui", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "eu", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "u", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "o", "",,, CompareMethod.Text)
                If newName.Length > 10 Then newName = Replace(newName, "i", "",,, CompareMethod.Text)
                If newName.Length > 10 Then
                    Me.setup.Log.AddWarning("Unable to abbreviate parameter name " & FieldName & " to fit maximum shapefield length of 10: " & newName & " truncated to " & Strings.Right(newName, 10) & ".")
                    newName = Strings.Right(newName, 10) 'if all else fails we will simply truncate our remaining string
                End If
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error reducing length of shapefield name " & FieldName & " to fit Shapefile requirements.")
            Return False
        End Try

    End Function

    Friend Function mergeUnderWaterAndAboveWaterTables(ByVal ID As String, ByVal UnderWaterTable As clsSobekTable, ByVal AboveWaterTable As clsSobekTable) As clsSobekTable
        'Author: Siebe Bosch
        'Date: 20-2-2015
        'Desctription: merges two yz tables. typically meant for combining an existing sobek table with an elevation grid table
        'v1.74: or the other way around: addin an underwater profile to an existing YZ profile

        '---------------------------------------------------'
        '                                                   '
        '\_____                                            /'
        '      \_______                           ________/ '
        '              \_________________________/          '
        '                                                   '
        '                         +                         '
        '                                                   '
        '                    \         /                    '
        '                     \       /                     '
        '                      \_____/                      '
        '                                                   '
        '                         =                         '
        '                                                   '
        '\_____                                            /'
        '      \_______                           ________/ '
        '              \_____           _________/          '
        '                    \         /                    '
        '                     \       /                     '
        '                      \_____/                      '
        '                                                   '
        '---------------------------------------------------'

        Dim curShift As Double, dist As Double
        Dim UnderWaterTable2 As New clsSobekTable(Me.setup)
        Dim AboveWaterTable2 As New clsSobekTable(Me.setup)
        Dim newTable As New clsSobekTable(Me.setup)
        Dim UnderWaterWidth As Double, AboveWaterWidth As Double
        Dim minShift As Double, maxShift As Double
        Dim myArea As Double, bestHorizontalShift As Double = 0, bestArea As Double = 9.0E+99
        Dim uVal As Double, aVal As Double 'resp. underwater, above water
        Dim nSteps As Integer = 40, nUnder As Integer = UnderWaterTable.XValues.Values.Count, nAbove As Integer = AboveWaterTable.XValues.Values.Count


        Try

            'derived parameters
            Dim underwaterStepSize As Double = (UnderWaterTable.XValues.Values(nUnder - 1) - UnderWaterTable.XValues.Values(0)) / nSteps
            UnderWaterWidth = UnderWaterTable.XValues.Values(nUnder - 1) - UnderWaterTable.XValues.Values(0)
            AboveWaterWidth = AboveWaterTable.XValues.Values(nAbove - 1) - AboveWaterTable.XValues.Values(0)

            If UnderWaterWidth < AboveWaterWidth Then
                'now we will iterate in order to find the configuration where the difference between both profiles is the smallest (expressed in m2)
                'we do this by shifting the under water profile horizontally
                'minShift = AboveWaterTable.XValues.Values(0) - UnderWaterTable.XValues.Values(0)
                'maxShift = AboveWaterTable.XValues.Values(nAbove - 1) - UnderWaterTable.XValues.Values(nUnder - 1)
                minShift = UnderWaterTable.XValues.Values(nUnder - 1) - AboveWaterTable.XValues.Values(nAbove - 1)
                maxShift = UnderWaterTable.XValues.Values(0) - AboveWaterTable.XValues.Values(0)
                If minShift > maxShift Then Me.setup.GeneralFunctions.SwapVariables(minShift, maxShift)

                'step by step shift the underwater profile and recalculate the area between both profiles. store the best (smallest area) result  
                For curShift = minShift To maxShift Step (maxShift - minShift) / nSteps
                    myArea = 0
                    For dist = UnderWaterTable.XValues.Values(0) To UnderWaterTable.XValues.Values(nUnder - 1) Step underwaterStepSize
                        aVal = AboveWaterTable.InterpolateFromXValues(dist - curShift, 1)
                        uVal = UnderWaterTable.InterpolateFromXValues(dist, 1)
                        myArea += Math.Max(aVal - uVal, 0)
                    Next
                    If myArea < bestArea Then
                        bestArea = myArea
                        bestHorizontalShift = curShift
                    End If
                Next

                AboveWaterTable2 = AboveWaterTable.Clone(bestHorizontalShift)
                newTable = mergeAlignedYZTables(ID, UnderWaterTable, AboveWaterTable2)
                Return newTable
            Else
                Me.setup.Log.AddWarning("Warning: could Not merge under water profile with above water profile since under water profile was the widest one.")
                Return UnderWaterTable
            End If
        Catch ex As Exception
            Me.setup.Log.AddError("Error merging yz tables for ID " & ID)
            Return Nothing
        End Try

    End Function

    Friend Function mergeAlignedYZTables(ByVal ID As String, ByVal InnerTable As clsSobekTable, ByVal OuterTable As clsSobekTable) As clsSobekTable
        'Author: Siebe Bosch
        'Date: 20-2-2015
        'Desctription: merges two yz tables. typically meant for combining an existing sobek table with an elevation grid table
        '---------------------------------------------------'
        '                                                   '
        '\_____                                            /'
        '      \_______                           ________/ '
        '              \_________________________/          ' 
        '                         +                         ' 
        '                    \         /                    '
        '                     \       /                     '
        '                      \_____/                      ' 
        '                         =                         ' 
        '\_____                                            /'
        '      \_______                           ________/ '
        '              \_____           _________/          '
        '                    \         /                    '
        '                     \       /                     '
        '                      \_____/                      ' 
        '---------------------------------------------------'
        'note that the inner table must fall completely inside the outer table!

        Try
            Dim newTable As New clsSobekTable(Me.setup)
            'Dim xSection1 As Double = OuterTable.InterpolateFromXValues(InnerTable.XValues.Values(0))
            'Dim xSection2 As Double = OuterTable.InterpolateFromXValues(InnerTable.XValues.Values(InnerTable.XValues.Values.Count - 1))

            For i = 0 To OuterTable.XValues.Values.Count - 1
                'If OuterTable.XValues.Values(i) > xSection1 Then Exit For
                'v2.0 replaced the row above by > innertable.xvalues.values(0)
                If OuterTable.XValues.Values(i) >= InnerTable.XValues.Values(0) Then Exit For
                newTable.AddDataPair(2, OuterTable.XValues.Values(i), OuterTable.Values1.Values(i))
            Next
            'the innertable remains untouched
            For i = 0 To InnerTable.XValues.Values.Count - 1
                newTable.AddDataPair(2, InnerTable.XValues.Values(i), InnerTable.Values1.Values(i))
            Next
            'only add points from the outer table when they are located to the right of our innertable
            For i = 0 To OuterTable.XValues.Values.Count - 1
                If OuterTable.XValues.Values(i) > InnerTable.XValues.Values(InnerTable.XValues.Values.Count - 1) Then
                    newTable.AddDataPair(2, OuterTable.XValues.Values(i), OuterTable.Values1.Values(i))
                End If
            Next
            Return newTable
        Catch ex As Exception
            Me.setup.Log.AddError("Error in Function mergeAlignedYZTables")
            Return Nothing
        End Try
    End Function

    Public Function CountCharacter(ByVal myString As String, ByVal ch As String, Optional ByVal IgnoreCase As Boolean = True) As Integer
        'this function counts the number of times a given character occurs in a given string
        Dim n As Integer = 0
        If IgnoreCase Then
            For i = 1 To myString.Length
                If Strings.Mid(myString, i, 1).ToUpper = ch.ToUpper Then
                    n += 1
                End If
            Next
        Else
            For i = 1 To myString.Length
                If Strings.Mid(myString, i, 1) = ch Then
                    n += 1
                End If
            Next
        End If
        Return n
    End Function


    Public Function GetExpressionsFromBooleanEquation(ByVal myEquation As String, ByRef LeftExpression As String, ByRef RightExpression As String) As Boolean
        Try
            'if our equation is empty we simply empty our expressions and return true
            If myEquation Is Nothing OrElse myEquation.Length = 0 Then
                LeftExpression = ""
                RightExpression = ""
                Return True
            End If

            'this function returns the right side of our equation
            Dim DelimiterPos As Integer
            If InStr(myEquation, ">=", CompareMethod.Text) > 0 Then
                DelimiterPos = InStr(myEquation, ">=", CompareMethod.Text)
                LeftExpression = Strings.Left(myEquation, DelimiterPos - 1)
                RightExpression = Strings.Right(myEquation, myEquation.Length - DelimiterPos - 1)
                Return True
            ElseIf InStr(myEquation, "<=", CompareMethod.Text) > 0 Then
                DelimiterPos = InStr(myEquation, "<=", CompareMethod.Text)
                LeftExpression = Strings.Left(myEquation, DelimiterPos - 1)
                RightExpression = Strings.Right(myEquation, myEquation.Length - DelimiterPos - 1)
                Return True
            ElseIf InStr(myEquation, "!=", CompareMethod.Text) > 0 Then
                DelimiterPos = InStr(myEquation, "!=", CompareMethod.Text)
                LeftExpression = Strings.Left(myEquation, DelimiterPos - 1)
                RightExpression = Strings.Right(myEquation, myEquation.Length - DelimiterPos - 1)
                Return True
            ElseIf InStr(myEquation, "=", CompareMethod.Text) > 0 Then
                DelimiterPos = InStr(myEquation, "=", CompareMethod.Text)
                LeftExpression = Strings.Left(myEquation, DelimiterPos - 1)
                RightExpression = Strings.Right(myEquation, myEquation.Length - DelimiterPos)
                Return True
            ElseIf InStr(myEquation, ">", CompareMethod.Text) > 0 Then
                DelimiterPos = InStr(myEquation, ">", CompareMethod.Text)
                LeftExpression = Strings.Left(myEquation, DelimiterPos - 1)
                RightExpression = Strings.Right(myEquation, myEquation.Length - DelimiterPos)
                Return True
            ElseIf InStr(myEquation, "<", CompareMethod.Text) > 0 Then
                DelimiterPos = InStr(myEquation, "<", CompareMethod.Text)
                LeftExpression = Strings.Left(myEquation, DelimiterPos - 1)
                RightExpression = Strings.Right(myEquation, myEquation.Length - DelimiterPos)
                Return True
            End If
            Throw New Exception("Invalid boolean equation. No supported operator found: " & myEquation)
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function GetExpressions of class GeneralFunctions: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function EvaluateBooleanEquation(ParentExpression As String, myEquation As String, LeftParName As String, RightParName As String, ByRef LeftParVal As Double, ByRef RightParVal As Double, ByRef Success As enmValidationResult, ByRef FinalComment As String, Optional ByVal ObjectID As String = "", Optional ByVal XCoord As Double = 0, Optional ByVal YCoord As Double = 0, Optional ByVal TempDir As String = "c:\temp") As Boolean
        Try
            'this function evaluates a boolean condition: two sides of an equation that are compared by an operator
            Dim myOperator As enmComparisonOperator
            Dim LeftPar As String = "", RightPar As String = ""

            'if the equation to be evaluated is empty, we assume that the condition is met
            If myEquation Is Nothing OrElse myEquation.Length = 0 Then
                Success = enmValidationResult.Successful
                Return True
            End If

            'perform the evaluation itself
            If InStr(myEquation, ">=") > 0 Then
                myOperator = GeneralFunctions.enmComparisonOperator.ge
                LeftPar = Me.setup.GeneralFunctions.ParseString(myEquation, ">")
                RightPar = Strings.Right(myEquation, myEquation.Length - 1)
            ElseIf InStr(myEquation, "<=") > 0 Then
                myOperator = GeneralFunctions.enmComparisonOperator.le
                LeftPar = Me.setup.GeneralFunctions.ParseString(myEquation, "<")
                RightPar = Strings.Right(myEquation, myEquation.Length - 1)
            ElseIf InStr(myEquation, "!=") > 0 Then
                myOperator = GeneralFunctions.enmComparisonOperator.ne
                LeftPar = Me.setup.GeneralFunctions.ParseString(myEquation, "!")
                RightPar = Strings.Right(myEquation, myEquation.Length - 1)
            ElseIf InStr(myEquation, "=") > 0 Then
                myOperator = GeneralFunctions.enmComparisonOperator.eq
                LeftPar = Me.setup.GeneralFunctions.ParseString(myEquation, "=")
                RightPar = myEquation
            ElseIf InStr(myEquation, "<") > 0 Then
                myOperator = GeneralFunctions.enmComparisonOperator.lt
                LeftPar = Me.setup.GeneralFunctions.ParseString(myEquation, "<")
                RightPar = myEquation
            ElseIf InStr(myEquation, ">") > 0 Then
                myOperator = GeneralFunctions.enmComparisonOperator.gt
                LeftPar = Me.setup.GeneralFunctions.ParseString(myEquation, ">")
                RightPar = myEquation
            End If




            'One special case: If our right-hand expression contains !=NaN we will evaluate this separately
            If RightPar.Trim.ToUpper = "NAN" AndAlso myOperator = enmComparisonOperator.ne Then
                'we're obviously dealing with a syntax validation here. So just evaluate the left hand par and check whether it is not NaN
                EvaluateMathExpression(ObjectID, ParentExpression, XCoord, YCoord, TempDir, LeftPar, LeftParVal)
                If Double.IsNaN(LeftParVal) Then
                    Success = enmValidationResult.Unsuccessful
                    Return True
                Else
                    Success = enmValidationResult.Successful
                    Return True
                End If
            Else
                'now both sides can still be complex mathematical expressions, so evaluate them as such
                EvaluateMathExpression(ObjectID, ParentExpression, XCoord, YCoord, TempDir, LeftPar, LeftParVal)
                EvaluateMathExpression(ObjectID, ParentExpression, XCoord, YCoord, TempDir, RightPar, RightParVal)

                'for reqular equations NaN values prevent us from evaluating
                If Double.IsNaN(LeftParVal) Then
                    FinalComment = "Parameter " & LeftParName & " is not numeric."
                    Success = enmValidationResult.EvaluationFailed
                    Return True
                End If
                If Double.IsNaN(RightParVal) Then
                    FinalComment = "Parameter " & RightParName & " is not numeric."
                    Success = enmValidationResult.EvaluationFailed
                    Return True
                End If
            End If

            'if we end up here, we have numerical values on both sides of our equation
            Select Case myOperator
                Case GeneralFunctions.enmComparisonOperator.eq
                    If (LeftParVal = RightParVal) Then Success = GeneralFunctions.enmValidationResult.Successful Else Success = GeneralFunctions.enmValidationResult.Unsuccessful
                Case GeneralFunctions.enmComparisonOperator.ge
                    If (LeftParVal >= RightParVal) Then Success = GeneralFunctions.enmValidationResult.Successful Else Success = GeneralFunctions.enmValidationResult.Unsuccessful
                Case GeneralFunctions.enmComparisonOperator.gt
                    If (LeftParVal > RightParVal) Then Success = GeneralFunctions.enmValidationResult.Successful Else Success = GeneralFunctions.enmValidationResult.Unsuccessful
                Case GeneralFunctions.enmComparisonOperator.le
                    If (LeftParVal <= RightParVal) Then Success = GeneralFunctions.enmValidationResult.Successful Else Success = GeneralFunctions.enmValidationResult.Unsuccessful
                Case GeneralFunctions.enmComparisonOperator.lt
                    If (LeftParVal < RightParVal) Then Success = GeneralFunctions.enmValidationResult.Successful Else Success = GeneralFunctions.enmValidationResult.Unsuccessful
                Case GeneralFunctions.enmComparisonOperator.ne
                    If (LeftParVal <> RightParVal) Then Success = GeneralFunctions.enmValidationResult.Successful Else Success = GeneralFunctions.enmValidationResult.Unsuccessful
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error evaluating Boolean Equation " & myEquation & " for location " & ObjectID & ": " & ex.Message)
            Return False
        End Try
    End Function

    Public Function EvaluateMathExpression(ObjectID As String, ParentExpression As String, XCoord As Double, YCoord As Double, TempDir As String, Expression As String, ByRef Result As Double) As Boolean
        Try
            'this is a generic function that evaluates a mathematical expression
            'it supports operators ^, *, /, + and - and the use of parentheses
            'TRUE is returned if the expression could successfully be evaluated.
            'author: Siebe Bosch
            'date: 16-5-2021

            If Expression Is Nothing Then Return False

            Dim FragmentResult As Double
            Dim OriginalExpression As String = Expression
            Dim LeftParVal As Double, RightParVal As Double

            'first make sure our number of ( matches the number of )
            Dim maxIter As Integer = 20, iIter As Integer = 0
            Dim nOpen As Integer = CountCharacter(Expression, "(")
            Dim nClose As Integer = CountCharacter(Expression, ")")
            If nOpen <> nClose Then Throw New Exception("Invalid mathematical expression while processing expression '" & OriginalExpression & "' derived from parent expression '" & ParentExpression & "' for object " & ObjectID & ". Number of opening parentheses does not match number of closing parentheses: " & Expression)

            Dim Done As Boolean = False

            While Not Done
                'the entire expression is solved as soon as the expression has become numeric in its entierity
                iIter += 1

                'if our expression is or has become numeric, we're done here
                If MakeExpressionNumeric(Expression, Result) Then
                    Return True
                Else
                    'find the centermost parentheses and, if found, process the block inside and replace its result in the expression
                    Dim startpos As Integer, endpos As Integer
                    If Not IdentifyCenterMostParenthesesInMathExpression(Expression, startpos, endpos) Then Throw New Exception("Error identifying center parentheses in math expression: " & Expression & ".")

                    If endpos > startpos Then

                        'if parentheses are found, this can mean one of two things:
                        '1: inside the parentheses there is a mathematical expression such as x + y
                        '2: the parentheses encompass the contents of a mathematical operation such as min/max/avg etc.
                        Dim OperationFound As Boolean = False

                        'first we will search for an operation string. do this by walking backward from the opening bracket
                        Dim OperationStr As String = ""
                        Dim OperationStartPos As Integer
                        Dim Operation As clsHydroMathOperation ' GeneralFunctions.enmHydroMathOperation
                        Dim LeftPar As String, RightPar As String
                        Dim CommaPos As Integer
                        For OperationStartPos = startpos - 1 To 1 Step -1
                            OperationStr = (Mid(Expression, OperationStartPos, 1) & OperationStr).Trim.ToUpper

                            'as soon as we find an operator string that matches a supported operation we can start evaluating what's between the brackets
                            If setup.HydroMathOperations.ContainsKey(OperationStr.Trim.ToUpper) Then
                                'operation found. Execute the operation and replace that part of the expression by its result
                                Operation = setup.HydroMathOperations.Item(OperationStr.Trim.ToUpper)
                                OperationFound = True

                                'here we wil evaluate all possible math expressions
                                If Operation.nArgs = 1 Then
                                    LeftPar = Mid(Expression, startpos + 1, endpos - startpos - 1)

                                    'the expression might still require an evaluation
                                    Me.setup.GeneralFunctions.EvaluateMathExpressionWithoutParentheses(ObjectID, ParentExpression, LeftPar, LeftParVal)

                                    FragmentResult = Operation.EvaluateOneParameterOperation(LeftParVal, ObjectID, TempDir, XCoord, YCoord)

                                    'finally replace the expression by our fragment's result
                                    Expression = Strings.Replace(Expression, Mid(Expression, OperationStartPos, endpos - OperationStartPos + 1), FragmentResult,,, CompareMethod.Text)

                                ElseIf Operation.nArgs = 2 Then
                                    CommaPos = InStr(startpos, Expression, ";")
                                    LeftPar = Mid(Expression, startpos + 1, CommaPos - startpos - 1)
                                    RightPar = Mid(Expression, CommaPos + 1, endpos - CommaPos - 1)

                                    'both expressions might still require an evaluation
                                    Me.setup.GeneralFunctions.EvaluateMathExpressionWithoutParentheses(ObjectID, ParentExpression, LeftPar, LeftParVal)
                                    Me.setup.GeneralFunctions.EvaluateMathExpressionWithoutParentheses(ObjectID, ParentExpression, RightPar, RightParVal)
                                    FragmentResult = Operation.EvaluateTwoParametersOperation(LeftParVal, RightParVal, ObjectID, TempDir, XCoord, YCoord)

                                    'finally replace the expression by our fragment's result
                                    Expression = Strings.Replace(Expression, Mid(Expression, OperationStartPos, endpos - OperationStartPos + 1), FragmentResult,,, CompareMethod.Text)

                                    Exit For

                                End If
                            End If

                        Next

                        If Not OperationFound Then
                            'since the paremtheses did not represent a hydrological operation, we will interpret the interior as a pure mathematical expression
                            'final step: actually evaluate the equation
                            Dim FragmentString As String = Mid(Expression, startpos, endpos - startpos + 1)
                            Dim FragmentExpression As String = Mid(Expression, startpos + 1, endpos - startpos - 1).Trim
                            If EvaluateMathExpressionWithoutParentheses(ObjectID, ParentExpression, FragmentExpression, FragmentResult) Then
                                'now replace the fragment by our newly computed result
                                Expression = Strings.Replace(Expression, Mid(Expression, startpos, FragmentString.Length), FragmentResult,,, CompareMethod.Text)
                            Else
                                'v2.107: we skip this exception since it happens too often when evaluating e.g. an alternative
                                Return False
                                'Throw New Exception("Could not evaluate fragment " & FragmentString & " of math expression " & Expression & ".")
                            End If
                        End If
                    Else
                        'no parentheses found. This means we can process our expression entirely as an expression without parentheses
                        If Not EvaluateMathExpressionWithoutParentheses(ObjectID, ParentExpression, Expression, Result) Then
                            Return False
                        End If
                    End If

                    'failsafe
                    If iIter >= maxIter Then Throw New Exception("Error processing math expression " & Expression & ": no solution found in " & maxIter & " iterations.")
                End If

            End While
            Throw New Exception("No solution found when evaluating math expression " & OriginalExpression & ".")
            Result = Double.NaN
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function EvaluateMathExpression:" & ex.Message)
            Return False
        End Try
    End Function

    Public Function MakeExpressionNumeric(Expression As String, ByRef Result As Double) As Boolean
        Try
            'this function coerces an expression (string) to a numeric (double precision) value
            'if unsuccessful, the byref argument is changed into Double.Nan
            'reason is that val(expression) returns 0 when expression is surrounded by ( and )
            'also val(expression) returns 0 when Expression yields NaN
            Expression = Expression.Trim.ToUpper
            If IsNumeric(Expression) Then
                If Expression = "NAN" Then
                    Result = Double.NaN
                ElseIf InStr(Expression, "(") > 0 Then
                    Result = Val(Mid(Expression, 2, Expression.Length - 2))
                Else
                    Result = Val(Expression)
                End If
                Return True
            ElseIf Expression.ToUpper.Trim = "NAN" Then
                Result = Double.NaN
                Return True
            Else
                Result = Double.NaN
                Return False
            End If
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function IsNumericAdvanced while performing an isnumeric check on " & Expression & ".")
            Return False
        End Try
    End Function

    Public Function EvaluateMathExpressionWithoutParentheses(ObjectID As String, ParentExpression As String, ByRef Expression As String, ByRef Result As Double) As Boolean
        Try
            'this function processes a math expression under the assumption that no parentheses are used.
            'this means: MeneerVanDaleWachtOpAntwoord!
            Dim Done As Boolean = False, nIter As Integer = 20
            Dim iIter As Integer = 0
            Dim LeftOperand As String = "", RightOperand As String = ""
            Dim StartPos As Integer, EndPos As Integer
            Dim TempResult As Double
            While Not Done
                iIter += 1

                'we start searching for an operator, starting from position 2 and thus skipping any leading +'es or -'ses
                Expression = Expression.Trim                            'we must trim to avoid false identification of a first - sign as an operator
                Dim powerPos As Integer = InStr(2, Expression, "^")
                Dim multPos As Integer = InStr(2, Expression, "*")
                Dim divPos As Integer = InStr(2, Expression, "/")
                Dim addPos As Integer = InStr(2, Expression, "+")

                'v2.114: this routine falsely interpreted the - sign after E as a subtraction
                Dim subtractPos As Integer = 0 ' = InStr(2, Expression, "-")
                Dim LastChar As String = Strings.Mid(Expression, 1, 1).ToUpper
                Dim curChar As String
                For i = 2 To Expression.Length
                    curChar = Strings.Mid(Expression, i, 1)
                    If curChar = "-" AndAlso Not LastChar = "E" Then
                        subtractPos = i
                        Exit For
                    End If
                    LastChar = curChar
                Next

                If MakeExpressionNumeric(Expression, Result) Then
                    Done = True
                Else
                    If powerPos > 0 Then
                        GetOperandLeftFromOperator(Expression, powerPos, LeftOperand, StartPos)
                        GetOperandRightFromOperator(Expression, powerPos, RightOperand, EndPos)     'endpos is the position of the last numerical value BEFORE next operators
                        If Not EvaluateTwoParameterExpression(LeftOperand, "^", RightOperand, TempResult) Then
                            Return False
                        End If
                        Expression = Strings.Replace(Expression, Mid(Expression, StartPos, EndPos - StartPos + 1), TempResult,,, CompareMethod.Text)
                    ElseIf multPos > 0 Then
                        GetOperandLeftFromOperator(Expression, multPos, LeftOperand, StartPos)
                        GetOperandRightFromOperator(Expression, multPos, RightOperand, EndPos)
                        If Not EvaluateTwoParameterExpression(LeftOperand, "*", RightOperand, TempResult) Then
                            Return False
                        End If
                        Expression = Strings.Replace(Expression, Mid(Expression, StartPos, EndPos - StartPos + 1), TempResult,,, CompareMethod.Text)
                    ElseIf divPos > 0 Then
                        GetOperandLeftFromOperator(Expression, divPos, LeftOperand, StartPos)
                        GetOperandRightFromOperator(Expression, divPos, RightOperand, EndPos)
                        If Not EvaluateTwoParameterExpression(LeftOperand, "/", RightOperand, TempResult) Then
                            Return False
                        End If
                        Expression = Strings.Replace(Expression, Mid(Expression, StartPos, EndPos - StartPos + 1), TempResult,,, CompareMethod.Text)
                    ElseIf addPos > 0 Then
                        GetOperandLeftFromOperator(Expression, addPos, LeftOperand, StartPos)
                        GetOperandRightFromOperator(Expression, addPos, RightOperand, EndPos)
                        If Not EvaluateTwoParameterExpression(LeftOperand, "+", RightOperand, TempResult) Then
                            Return False
                        End If
                        Expression = Strings.Replace(Expression, Mid(Expression, StartPos, EndPos - StartPos + 1), TempResult,,, CompareMethod.Text)
                    ElseIf subtractPos > 0 Then
                        'this is a special case. 
                        GetOperandLeftFromOperator(Expression, subtractPos, LeftOperand, StartPos)
                        GetOperandRightFromOperator(Expression, subtractPos, RightOperand, EndPos)
                        If Not EvaluateTwoParameterExpression(LeftOperand, "-", RightOperand, TempResult) Then
                            Return False
                        End If
                        Expression = Strings.Replace(Expression, Mid(Expression, StartPos, EndPos - StartPos + 1), TempResult,,, CompareMethod.Text)
                    End If
                End If

                'failsafe
                If iIter >= 20 Then Throw New Exception("Could not solve math expression " & Expression & " in " & nIter & " iterations.")
            End While
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function EvaluateMathExpressionWithoutParentheses while processing:" & Expression & ", which was derived from parent expression " & ParentExpression & " for object " & ObjectID & ": " & ex.Message)
            Return False
        End Try
    End Function

    Public Function EvaluateTwoParameterExpression(LeftVal As Double, myOperator As String, RightVal As Double, ByRef Result As Double) As Boolean
        Try
            If Double.IsNaN(LeftVal) Then
                Return False
            End If
            If Double.IsNaN(RightVal) Then
                Return False
            End If

            Select Case myOperator
                Case Is = "^"
                    Result = LeftVal ^ RightVal
                    Return True
                Case Is = "*"
                    Result = LeftVal * RightVal
                    Return True
                Case Is = "/"
                    Result = LeftVal / RightVal
                    Return True
                Case Is = "+"
                    Result = LeftVal + RightVal
                    Return True
                Case Is = "-"
                    Result = LeftVal - RightVal
                    Return True
                Case Else
                    Throw New Exception("Operator in math expression not supported: " & myOperator)
            End Select

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function EvaluateTwoParameterExpression: " & ex.Message)
            Return False
        End Try
    End Function



    Public Function GetOperandLeftFromOperator(Expression As String, OperatorPos As Integer, ByRef Operand As String, ByRef OperandStartPos As Integer) As Boolean
        Try
            'we will walk in left direction from the operator position until we meet another mathematical operator (+ - * / ^)
            'note: sqr is NOT supported but needs to be expressed as a power. e.g. sqrt(2) = 4^0.5
            Dim i As Integer, myChar As String
            Operand = ""
            For i = OperatorPos - 1 To 1 Step -1
                myChar = Mid(Expression, i, 1)
                Select Case myChar
                    Case Is = "^"
                        Exit For
                    Case Is = "*"
                        Exit For
                    Case Is = "/"
                        Exit For
                    Case Is = "+"
                        If i = 1 Then
                            Operand = myChar & Operand 'only in case this character is the very first one in the expression, will we include it
                            OperandStartPos = i
                            Exit For
                        ElseIf i > 1 AndAlso Mid(Expression, i - 1, 1).Trim.ToUpper = "E" Then
                            'this is the exponent of our numerical value. We'll need to continue walking left
                            Operand = myChar & Operand  'glue this character to the left side of our operand so far
                        Else
                            Exit For
                        End If
                    Case Is = "-"
                        If i = 1 Then
                            Operand = myChar & Operand 'only in case this character is the very first one in the expression, will we include it
                            OperandStartPos = i
                            Exit For
                        ElseIf i > 1 AndAlso Mid(Expression, i - 1, 1).Trim.ToUpper = "E" Then
                            'this is the exponent of our numerical value. We'll need to continue walking left
                            Operand = myChar & Operand  'glue this character to the left side of our operand so far
                        Else
                            Exit For
                        End If
                    Case Else
                        Operand = myChar & Operand 'glue this character to the left side of our operand so far
                        OperandStartPos = i
                End Select
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in functon GetOperandLeftFromOperator: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetOperandRightFromOperator(Expression As String, OperatorPos As Integer, ByRef Operand As String, ByRef OperandEndPos As Integer) As Boolean
        Try
            'we will walk in right direction from the operator position until we meet another mathematical operator (+ - * / ^)
            'note: sqr is NOT supported but needs to be expressed as a power. e.g. sqrt(2) = 4^0.5
            Dim i As Integer, myChar As String
            Operand = ""
            For i = OperatorPos + 1 To Expression.Length
                myChar = Mid(Expression, i, 1)
                Select Case myChar
                    Case Is = "^"
                        Exit For
                    Case Is = "*"
                        Exit For
                    Case Is = "/"
                        Exit For
                    Case Is = "+"
                        If i = OperatorPos + 1 Then
                            Operand = Operand & myChar 'only in case this character is the very first one in the operand, will we include it
                        ElseIf i > OperatorPos + 1 AndAlso Strings.Right(Operand, 1).Trim.ToUpper = "E" Then
                            Operand = Operand & myChar 'only in case this character is the very first one in the operand, will we include it
                            'the previous character was E so we're dealing with an exponent. simply continue the loop since we haven't finished identifying our number yet
                        Else
                            Exit For
                        End If
                    Case Is = "-"
                        If i = OperatorPos + 1 Then
                            Operand = Operand & myChar 'only in case this character is the very first one in the operand, will we include it
                        ElseIf i > OperatorPos + 1 AndAlso Strings.Right(Operand, 1).Trim.ToUpper = "E" Then
                            Operand = Operand & myChar 'only in case this character is the very first one in the operand, will we include it
                            'the previous character was E so we're dealing with an exponent. simply continue the loop since we haven't finished identifying our number yet
                        Else
                            Exit For
                        End If
                    Case Else
                        Operand = Operand & myChar 'glue this character to the right side of our operand so far
                        OperandEndPos = i
                End Select
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in functon GetOperandRightFromOperator: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetOperandPositionsSurroundingOperator(expression As String, OperatorPos As Integer, ByRef LeftPos As Integer, ByRef RightPos As Integer) As Boolean
        Try
            If OperatorPos > 0 Then
                For i = OperatorPos To 1 Step -1
                    If Not IsNumeric(Mid(expression, i, 1)) Then Exit For
                    LeftPos = i
                Next
                For i = OperatorPos To expression.Length
                    If Not IsNumeric(Mid(expression, i, 1)) Then Exit For
                    RightPos = i
                Next
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function IdentifyCenterMostParenthesesInMathExpression(Expression As String, ByRef startpos As Integer, ByRef endpos As Integer) As Boolean
        Try
            Dim nOpen As Integer = CountCharacter(Expression, "(")
            Dim nClose As Integer = CountCharacter(Expression, ")")
            If nOpen <> nClose Then Throw New Exception("Invalid expression. Number of opening and closing brackets in expression are not equal: " & Expression & ".")

            Dim i As Integer
            startpos = 0                            'initialize the start position as 0
            endpos = InStr(Expression, ")")         'the first instance of a closing bracket must be the center one
            If endpos > 0 Then
                For i = endpos - 1 To 1 Step -1     'walk backwards to find the corresponding opening bracket
                    If Strings.Mid(Expression, i, 1) = "(" Then
                        startpos = i
                        Return True
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function IdentifyCenterMostParenthesesInMathExpression while processing expression " & Expression & ": " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadAttributeFromMDUFile(path As String, myAttributeName As String) As String
        Try
            Dim myStr As String, tmpStr As String
            Dim isPos As Integer
            Dim AttrPos As Integer
            Using iniReader As New StreamReader(path)
                While Not iniReader.EndOfStream
                    myStr = iniReader.ReadLine
                    AttrPos = InStr(myStr, myAttributeName, CompareMethod.Text)
                    isPos = InStr(myStr, "=", CompareMethod.Text)
                    If AttrPos > 0 AndAlso isPos > AttrPos Then
                        'found!
                        tmpStr = ParseString(myStr, "=")
                        Return myStr.Trim
                    End If
                End While
            End Using
            Return ""
        Catch ex As Exception
            Me.setup.Log.AddError("Error reading attribute " & myAttributeName & " from MDU file " & path)
            Return ""
        End Try
    End Function

    Public Function ReadAttributeValueFromDHydrofile(path As String, myAttributeName As String, Optional ByVal Chapter As String = "") As List(Of String)
        'this function reads the value as assigned to a 
        Try
            Dim myLine As String
            Dim myResults As New List(Of String)
            Dim Attr As String
            Dim CurrentChapter As String = ""
            Using iniReader As New StreamReader(path)
                While Not iniReader.EndOfStream
                    myLine = iniReader.ReadLine.Trim
                    If Left(myLine, 1) = "[" AndAlso Right(myLine, 1) = "]" Then
                        CurrentChapter = myLine
                    Else
                        Attr = ParseString(myLine, "=").Trim.ToLower
                        If Attr = myAttributeName.Trim.ToLower Then
                            If Chapter = "" OrElse CurrentChapter.Trim.ToLower = Chapter.Trim.ToLower Then
                                While Not myLine = ""
                                    myResults.Add(ParseString(myLine, " "))
                                End While
                            End If
                        End If
                    End If
                End While
            End Using
            Return myResults
        Catch ex As Exception
            Me.setup.Log.AddError("Error reading attribute value from D-Hydro file: " & path & ", Attribute: " & myAttributeName)
            Return Nothing
        End Try

    End Function

    Public Function ReplaceSectionInDHydroFile(Path As String, SectionHeader As String, IdentifierAttribute As String, IdentifierValue As String, SectionContent As List(Of String)) As Boolean
        Try
            Dim myLine As String
            Dim OldContent As New List(Of String)
            Dim SectionWritten As Boolean = False
            Dim CurrentChapter As String = ""
            Dim CurrentChapterStartIdx As Integer = -1
            Dim CurrentIdentifier As String = ""
            Dim CurrentValue As String = ""
            Dim ChapterStartIdx As Integer = -1
            Dim ChapterEndIdx As Integer = -1

            'read the file's content
            Using iniReader As New StreamReader(Path)
                While Not iniReader.EndOfStream
                    OldContent.Add(iniReader.ReadLine.Trim)
                End While
            End Using

            'find the start- and endindex of our desired section
            For i = 0 To OldContent.Count - 1
                If Left(OldContent(i), 1) = "[" AndAlso InStr(OldContent(i), "]") > 0 Then
                    CurrentChapter = OldContent(i).Trim.ToLower        'keep track of which chapter we're in
                    CurrentChapterStartIdx = i
                    If ChapterStartIdx >= 0 Then
                        ChapterEndIdx = i - 1                         'it looks like we found a new chapter. So close up!
                        Exit For
                    End If
                Else
                    'parse the line and determine the attribute we're currently in
                    myLine = OldContent(i)
                    Dim myStr As String = setup.GeneralFunctions.ParseString(myLine, "=")
                    If myStr.Trim.ToLower = IdentifierAttribute.Trim.ToLower Then
                        CurrentIdentifier = myStr.Trim.ToLower
                    End If
                    If CurrentChapter.Trim.ToLower = SectionHeader.Trim.ToLower AndAlso CurrentIdentifier.Trim.ToLower = IdentifierAttribute.Trim.ToLower Then
                        CurrentValue = setup.GeneralFunctions.ParseString(myLine)
                        If CurrentValue.Trim.ToLower = IdentifierValue.Trim.ToLower Then
                            ChapterStartIdx = CurrentChapterStartIdx
                        End If
                    End If
                End If
            Next

            'for situations where our chapter is the last one
            If ChapterStartIdx >= 0 AndAlso ChapterEndIdx = -1 Then
                ChapterEndIdx = OldContent.Count - 1
            End If

            If ChapterStartIdx = -1 Then
                Throw New Exception("Fout: de gevraagde sectie met kop " & SectionHeader & " en " & IdentifierAttribute & " = " & IdentifierValue & " niet gevonden in het modelbestand " & Path)
            End If

            'and write the adjusted content back to the same file
            Using iniWriter As New StreamWriter(Path)
                For i = 0 To ChapterStartIdx - 1
                    iniWriter.WriteLine(OldContent(i))
                Next
                For Each myLine In SectionContent
                    iniWriter.WriteLine(myLine)
                Next
                For i = ChapterEndIdx + 1 To OldContent.Count - 1
                    iniWriter.WriteLine(OldContent(i))
                Next
            End Using

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function replaceSectionInDHydroFile: " & ex.Message)
            Return False
        End Try
    End Function



    Public Function ReplaceAttributeInDHydroFile(Path As String, SectionHeader As String, IdentifierAttribute As String, IdentifierValue As String, AttributeName As String, NewValue As String) As Boolean
        Try
            Dim myLine As String
            Dim myStr As String
            Dim Content As New List(Of String)
            Dim SectionWritten As Boolean = False
            Dim CurrentChapter As String = ""
            Dim CurrentIdentifierAttr As String = ""
            Dim CurrentIdentifierValue As String = ""
            Dim CurrentValue As String = ""

            'read the file's content
            Using iniReader As New StreamReader(Path)
                While Not iniReader.EndOfStream
                    Content.Add(iniReader.ReadLine.Trim)
                End While
            End Using

            'find the start- and endindex of our desired section
            For i = 0 To Content.Count - 1
                If Left(Content(i), 1) = "[" AndAlso InStr(Content(i), "]") > 0 Then
                    CurrentChapter = Content(i)
                ElseIf CurrentChapter.Trim.ToLower = SectionHeader.Trim.ToLower Then
                    'parse the line and determine the attribute we're currently in
                    myLine = Content(i)
                    myStr = setup.GeneralFunctions.ParseString(myLine, "=")
                    If myStr.Trim.ToLower = IdentifierAttribute.Trim.ToLower Then
                        CurrentIdentifierAttr = myStr.Trim.ToLower
                        CurrentIdentifierValue = myLine.Trim.ToLower
                    End If
                    'notice that the identifier attribute and the attribute to be adjusted might be the same. So no ElseIf here!
                    If myStr.Trim.ToLower = AttributeName.Trim.ToLower Then
                        If CurrentChapter.Trim.ToLower = SectionHeader.Trim.ToLower AndAlso CurrentIdentifierValue.Trim.ToLower = IdentifierValue.Trim.ToLower Then
                            Content(i) = AttributeName & " = " & NewValue
                        End If
                    End If
                End If
            Next

            'and write the adjusted content back to the same file
            Using iniWriter As New StreamWriter(Path)
                For i = 0 To Content.Count - 1
                    iniWriter.WriteLine(Content(i))
                Next
            End Using

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function ReplaceAttributeInDHydroFile: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function ReplaceAttributeValueInDHydroFile(Path As String, OldValue As String, NewValue As String, Optional ByVal Chapter As String = "") As Boolean
        Try
            Dim Found As Boolean = False
            Dim Content As New List(Of String)
            Dim currentChapter As String = ""           'keeps track of which chapter we're in
            Dim i As Integer
            Using iniReader As New StreamReader(Path)
                While Not iniReader.EndOfStream
                    Content.Add(iniReader.ReadLine.Trim)
                End While
            End Using

            Using iniWriter As New StreamWriter(Path)
                For i = 0 To Content.Count - 1

                    'keep track of in which chapter we are
                    If Left(Content(i), 1) = "[" AndAlso Right(Content(i), 1) = "]" Then
                        currentChapter = Content(i)
                    End If

                    Dim isPos As Integer = InStr(Content(i), "=")
                    If isPos > 0 AndAlso InStr(Content(i), OldValue, CompareMethod.Text) > isPos Then

                        'if we're in the right chapter (or if chapter is not relevant), replace the attribut value
                        If (Chapter = "" OrElse Chapter.Trim.ToLower = currentChapter.Trim.ToLower) Then
                            Found = True
                            'we need to parse this line so we can replace the requested filename inside
                            Dim OldLine As String = Content(i)
                            Dim NewLine As String = ""
                            Dim myFileName As String = ""
                            Dim Attribute As String = ParseString(OldLine, "=")
                            NewLine = Attribute & " = "
                            While Not OldLine = ""
                                'parse the attribute values by space (multiple filenames possible)
                                myFileName = ParseString(OldLine, " ")
                                If myFileName.Trim.ToLower = OldValue.Trim.ToLower Then
                                    NewLine &= NewValue & " "
                                Else
                                    NewLine &= myFileName
                                End If
                            End While
                            Content(i) = NewLine
                        End If

                    End If

                    'write each line back to the file
                    iniWriter.WriteLine(Content(i))

                Next
            End Using
            Return Found
        Catch ex As Exception
            Me.setup.Log.AddError("Error replacing attribute value in D-Hydro file: " & Path & ", Old value: " & OldValue & ", replaced by: " & NewValue)
            Return False
        End Try
    End Function

    Friend Function mergeYZTables(ByVal ID As String, ByVal InnerTable As clsSobekTable, ByVal ShiftInnterTableToCenter As Boolean, ByVal OuterTable As clsSobekTable, ByVal ShiftOuterTableToCenter As Boolean) As clsSobekTable
        'Author: Siebe Bosch
        'Date: 20-2-2015
        'Desctription: merges two yz tables. typically meant for combining an existing sobek table with an elevation grid table

        '---------------------------------------------------'
        '                                                   '
        '\_____                                            /'
        '      \_______                           ________/ '
        '              \_________________________/          '
        '                                                   '
        '                         +                         '
        '                                                   '
        '                    \         /                    '
        '                     \       /                     '
        '                      \_____/                      '
        '                                                   '
        '                         =                         '
        '                                                   '
        '\_____                                            /'
        '      \_______                           ________/ '
        '              \_____           _________/          '
        '                    \         /                    '
        '                     \       /                     '
        '                      \_____/                      '
        '                                                   '
        '---------------------------------------------------'

        Dim i As Long
        Dim myLowestDist As Double
        Dim InnerTable2 As New clsSobekTable(Me.setup)
        Dim OuterTable2 As New clsSobekTable(Me.setup)
        Dim newTable As New clsSobekTable(Me.setup)

        Try

            Debug.Print(ID)

            'shift the inner table sideways to get the lowest point at distance = 0
            If ShiftInnterTableToCenter Then
                myLowestDist = InnerTable.getYForLowestX(0, 1)
                For i = 0 To InnerTable.XValues.Count - 1
                    InnerTable2.AddDataPair(4, InnerTable.XValues.Values(i) - myLowestDist, InnerTable.Values1.Values(i), InnerTable.Values2.Values(i), InnerTable.Values3.Values(i))
                Next
            Else
                InnerTable2 = InnerTable.Clone
            End If

            'shift the outer table sideways to get the lowest point at distance = 0
            If ShiftOuterTableToCenter Then
                myLowestDist = OuterTable.getYForLowestX(0, 1)
                For i = 0 To OuterTable.XValues.Count - 1
                    OuterTable2.AddDataPair(4, OuterTable.XValues.Values(i) - myLowestDist, OuterTable.Values1.Values(i), OuterTable.Values2.Values(i), OuterTable.Values3.Values(i))
                Next
            Else
                OuterTable2 = OuterTable.Clone
            End If

            'now that we've got two perfectly aligned tables, we can start to merge them.
            Dim LeftIntersection As clsXY 'where X = distance from the heart and Y the elevation value
            Dim RightIntersection As clsXY 'where X = distance from the heart and Y the elevation value
            LeftIntersection = InnerTable2.GetLeftIntersectionWithTable(ID, OuterTable2, 0, 1)
            RightIntersection = InnerTable2.GetRightIntersectionWithTable(ID, OuterTable2, 0, 1)

            If LeftIntersection Is Nothing Then
                Me.setup.Log.AddError("Could not merge YZ tables for object " & ID & " since left side intersection was not found. Table remained unchanged.")
                Return InnerTable
            ElseIf RightIntersection Is Nothing Then
                Me.setup.Log.AddError("Could not merge YZ tables for object " & ID & " since right side intersection was not found. Table remained unchanged.")
                Return InnerTable
            Else
                '----------------------------------------------------------------------------------------------------------------
                '       START BUILDING THE COMBINED TABLE
                '----------------------------------------------------------------------------------------------------------------
                newTable = New clsSobekTable(Me.setup)

                'left wing first, extracted from the outer table
                For i = 0 To OuterTable2.XValues.Count - 1
                    If OuterTable2.XValues.Values(i) >= LeftIntersection.X Then Exit For
                    newTable.AddDataPair(4, OuterTable2.XValues.Values(i), OuterTable2.Values1.Values(i), OuterTable2.Values2.Values(i), OuterTable2.Values3.Values(i))
                Next

                'the left intersection point. The exact XY-coordinate is irrelevenat for this location
                newTable.AddDataPair(4, LeftIntersection.X, LeftIntersection.Y, 0, 0)

                'center, extracted from the inner table
                For i = 0 To InnerTable2.XValues.Count - 1
                    If InnerTable2.XValues.Values(i) > LeftIntersection.X AndAlso InnerTable2.XValues.Values(i) < RightIntersection.X Then
                        newTable.AddDataPair(4, InnerTable2.XValues.Values(i), InnerTable2.Values1.Values(i), 0, 0)
                    End If
                Next

                'the right intersection point
                newTable.AddDataPair(4, RightIntersection.X, RightIntersection.Y, 0, 0)

                'right wing last, extracted from the outer table
                For i = 0 To OuterTable2.XValues.Count - 1
                    If OuterTable2.XValues.Values(i) > RightIntersection.X Then
                        newTable.AddDataPair(4, OuterTable2.XValues.Values(i), OuterTable2.Values1.Values(i), OuterTable2.Values2.Values(i), OuterTable2.Values3.Values(i))
                    End If
                Next
                '----------------------------------------------------------------------------------------------------------------
                '       BUILDING COMPLETE
                '----------------------------------------------------------------------------------------------------------------
                Return newTable
            End If
        Catch ex As Exception
            Me.setup.Log.AddError("Error merging yz tables for ID " & ID)
            Return Nothing
        End Try

    End Function

    Public Function CSV2MDB(ByVal DatabaseType As enmDatabaseType, ByVal TableName As String, ByVal Path As String, ByVal Delimiter As String, ByVal DateFormatting As String, ByVal SeriesName As String)

        'reads as CSV file and writes the result to an MDB database

        Try
            Dim myStr As String, tmpStr As String, HeaderColIdx As Integer, curIdx As Integer, i As Integer
            Dim myDate As DateTime, myVal As Double
            Dim nNonNumeric As Long, r As Long
            Dim query As String

            Using csvReader As New StreamReader(Path)

                'find the column index for the data
                myStr = csvReader.ReadLine
                curIdx = 0
                While Not myStr = ""
                    curIdx += 1
                    tmpStr = Me.setup.GeneralFunctions.ParseString(myStr, Delimiter)
                    If tmpStr.Trim.ToUpper = SeriesName.Trim.ToUpper Then
                        HeaderColIdx = curIdx
                        Exit While
                    End If
                End While

                If HeaderColIdx = 0 Then
                    Throw New Exception("Error: could not find data column " & SeriesName & " in csv file: " & Path)
                Else
                    While Not csvReader.EndOfStream
                        r += 1

                        'update the progress bar only once every 1000 rows
                        If r / 1000 = Math.Round(r / 1000) Then
                            Me.setup.GeneralFunctions.UpdateProgressBar("Reading csv file...", csvReader.BaseStream.Position, csvReader.BaseStream.Length, True)
                        End If

                        myStr = csvReader.ReadLine
                        For i = 1 To HeaderColIdx
                            tmpStr = Me.setup.GeneralFunctions.ParseString(myStr, Delimiter)
                            If i = 1 Then
                                Call ParseDateString(tmpStr, DateFormatting, myDate)
                            ElseIf i = HeaderColIdx Then
                                If IsNumeric(tmpStr) Then
                                    myVal = Convert.ToDouble(tmpStr)
                                Else
                                    nNonNumeric += 1
                                End If
                            End If
                        Next

                        query = "INSERT INTO " & TableName & " (DATUM," & SeriesName & ") VALUES ('" & myDate & "'," & myVal & ");"
                        SQLiteNoQuery(setup.SqliteCon, query, False)


                    End While
                End If
            End Using
            setup.SqliteCon.Close() 'only close the connection after reading the entire file

            If nNonNumeric > 0 Then Me.setup.Log.AddWarning("csv file " & Path & " contained " & nNonNumeric & " non-numeric values for time series " & SeriesName & ".")

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Public Function SQLCreateTimeTable(ByVal TableName As String) As Boolean
        Dim myQuery As String

        Try
            'first remove the old one. then create the new one
            myQuery = "DROP TABLE " & TableName & ";"
            Me.setup.GeneralFunctions.SQLiteNoQuery(setup.SqliteCon, myQuery)
            myQuery = "CREATE TABLE " & TableName & " (DATUMTIJD DATETIME, WAARDE DOUBLE);"
            Me.setup.GeneralFunctions.SQLiteNoQuery(setup.SqliteCon, myQuery)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function BaseFlowFilter(ByVal TotalFlow As DataTable, ByVal K As Double, ByVal W As Double, ByRef InterFlowResult As DataTable, ByRef BaseFlowResult As DataTable) As Boolean
        'this routine filters the baseflow out of the total discharge
        'it does so by applying the method by prof. Patrick Willems (Leuven University) as implemented in his tool Wetspro
        'the routine assumes that the fist column contains date/time and the second the discharge
        Dim alpha As Double, a As Double, b As Double, c As Double, v As Double
        Dim i As Long, iPar As Long
        Dim InterFlow As New DataTable
        Dim BaseFlow As New DataTable
        Dim prevTotalFlow As Double, prevInterFlow As Double, prevBaseFlow As Double
        Dim newRow As DataRow
        Dim nSteps As Long = TotalFlow.Rows.Count

        Try
            'add columns to the newly created datatables
            InterFlow.Columns.Add(New DataColumn("DatumTijd", Type.GetType("System.DateTime")))
            InterFlow.Columns.Add(New DataColumn("Waarde", Type.GetType("System.Double")))
            BaseFlow.Columns.Add(New DataColumn("DatumTijd", Type.GetType("System.DateTime")))
            BaseFlow.Columns.Add(New DataColumn("Waarde", Type.GetType("System.Double")))

            Me.setup.GeneralFunctions.UpdateProgressBar("Executing baseflow filter...", 0, 10)

            For iPar = 1 To 2
                For i = 0 To TotalFlow.Rows.Count - 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", i, nSteps)

                    'retrieve the total, inter and baseflow from the previous timestep
                    If i = 0 Then
                        prevTotalFlow = 0
                        prevInterFlow = 0
                        prevBaseFlow = 0
                    Else
                        prevTotalFlow = TotalFlow.Rows(i - 1)(1)
                        prevInterFlow = InterFlow.Rows(i - 1)(1)
                        If iPar = 2 Then prevBaseFlow = BaseFlow.Rows(i - 1)(1) 'data only available if ipar = 2
                    End If

                    If iPar = 1 Then 'interflow
                        alpha = Math.Exp(-1 / K)
                        v = (1 - W) / W
                        a = ((2 + v) * alpha - v) / (2 + v - v * alpha)
                        b = 2 / (2 + v - v * alpha)
                        c = 0.5 * v
                        'curFlow.InterFlow = a * prevFlow.InterFlow + b * (curFlow.Value - alpha * prevFlow.Value)
                        newRow = InterFlow.NewRow
                        newRow("DatumTijd") = TotalFlow.Rows(i)(0)
                        newRow("Waarde") = a * prevInterFlow + b * (TotalFlow.Rows(i)(1) - alpha * prevTotalFlow)
                        InterFlow.Rows.Add(newRow)

                    ElseIf iPar = 2 Then  'baseflow
                        alpha = Math.Exp(-1 / K)
                        v = (1 - W) / W
                        a = ((2 + v) * alpha - v) / (2 + v - v * alpha)
                        b = 2 / (2 + v - v * alpha)
                        c = 0.5 * v
                        newRow = BaseFlow.NewRow
                        newRow("DatumTijd") = TotalFlow.Rows(i)(0)
                        newRow("Waarde") = alpha * prevBaseFlow + c * (1 - alpha) * (prevInterFlow + InterFlow.Rows(i)("Waarde"))
                        BaseFlow.Rows.Add(newRow)
                    End If
                Next
            Next

            BaseFlowResult = BaseFlow
            InterFlowResult = InterFlow
            Return True

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    'Public Sub CompactAccessDatabase(ByVal oldPath As String, ByVal newPath As String)
    '    Dim arg1 As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & oldPath
    '    Dim arg2 As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" & newPath & ";Jet OLEDB:Engine Type=5"
    '    Dim jro As JRO.JetEngine
    '    jro = New JRO.JetEngine()
    '    jro.CompactDatabase(arg1, arg2)
    '    MsgBox("Finished Compacting Database!")
    'End Sub


    Public Function PointShapefileFromDatatable(ByRef dt As DataTable, idcolidx As Integer, xcolidx As Integer, ycolidx As Integer) As clsPointShapeFile
        Try
            Dim PointSF As New clsPointShapeFile(Me.setup)
            Dim myShape As MapWinGIS.Shape
            Dim IDIdx As Integer, IDList As New List(Of String)

            PointSF.SF.sf.CreateNew("", MapWinGIS.ShpfileType.SHP_POINT)
            PointSF.SF.sf.StartEditingShapes(True)
            PointSF.AddField("OBJECTID", MapWinGIS.FieldType.STRING_FIELD, 12, 4, IDIdx)

            For i = 0 To dt.Rows.Count - 1
                myShape = New MapWinGIS.Shape
                myShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
                myShape.AddPoint(dt.Rows(i)(xcolidx), dt.Rows(i)(ycolidx))
                PointSF.SF.sf.EditAddShape(myShape)
                PointSF.SF.sf.EditCellValue(IDIdx, PointSF.SF.sf.NumShapes - 1, dt.Rows(i)(idcolidx))
            Next

            PointSF.SF.sf.StopEditingShapes(False, True)
            'PointSF.SF.sf.StopEditingTable(False)
            'PointSF.Close()
            Return PointSF
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function MDBNoQuery(ByRef con As OleDb.OleDbConnection, ByVal myQuery As String, Optional ByVal CloseAfterwards As Boolean = True, Optional ByVal RetryIfException As Boolean = True) As Boolean
        'queries an MBD (Access) database and returns the results in a datatable
        Try
            Dim da As OleDb.OleDbDataAdapter
            Dim myTable As New System.Data.DataTable
            If Not con.State = ConnectionState.Open Then con.Open()

            da = New OleDb.OleDbDataAdapter(myQuery, con.ConnectionString)
            da.Fill(myTable)
            Return True
        Catch ex As Exception
            'SIEBE 12-3-2019
            'in sommige gevallen heeft de connectie een lees/schrijfprobleem en wordt een uitzondering opgeroepen. Een oplossing blijkt te zijn om de verbinding even te sluiten en dan weer te openen
            If RetryIfException Then
                Me.setup.Log.AddWarning("Warning: instable database connection. Connection was closed and opened again to execute query: " & myQuery)
                con.Close()
                MDBNoQuery(con, myQuery, CloseAfterwards, False)
                Return True
            Else
                Me.setup.Log.AddError("Error executing query " & myQuery)
                Me.setup.Log.AddError(ex.Message)
                Return False
            End If

            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try

    End Function

    Public Function MDBQuery(ByRef con As OleDb.OleDbConnection, ByVal myQuery As String, ByRef myTable As System.Data.DataTable, Optional ByVal CloseAfterwards As Boolean = True, Optional ByVal RetryIfException As Boolean = True) As Boolean
        'queries a DB (SQLite) database and returns the results in a datatable
        Try
            Dim da As OleDb.OleDbDataAdapter
            If Not con.State = ConnectionState.Open Then con.Open()

            da = New OleDb.OleDbDataAdapter(myQuery, con.ConnectionString)
            da.Fill(myTable)
            Return True
        Catch ex As Exception
            'SIEBE 12-3-2019
            'in sommige gevallen heeft de connectie een lees/schrijfprobleem en wordt een uitzondering opgeroepen. Een oplossing blijkt te zijn om de verbinding even te sluiten en dan weer te openen
            If RetryIfException Then
                Me.setup.Log.AddWarning("Warning: instable database connection. Connection was closed and opened again to execute query: " & myQuery)
                con.Close()
                MDBQuery(con, myQuery, myTable, CloseAfterwards, False)
                Return True
            Else
                Me.setup.Log.AddError("Error executing query " & myQuery)
                Me.setup.Log.AddError(ex.Message)
                Return False
            End If

            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try

    End Function

    Public Function SQLiteNoQuery(ByRef con As System.Data.SQLite.SQLiteConnection, ByVal myQuery As String, Optional ByVal CloseAfterwards As Boolean = True, Optional ByVal RetryIfException As Boolean = True, Optional ByRef nAffected As Integer = 0) As Boolean
        'queries an MBD (Access) database and returns the results in a datatable
        Try
            Dim da As SQLite.SQLiteDataAdapter
            Dim myTable As New System.Data.DataTable
            If Not con.State = ConnectionState.Open Then con.Open()
            da = New SQLite.SQLiteDataAdapter(myQuery, con.ConnectionString)
            da.Fill(myTable)
            nAffected = myTable.Rows.Count
            Return True
        Catch ex As Exception
            'SIEBE 12-3-2019
            'in sommige gevallen heeft de connectie een lees/schrijfprobleem en wordt een uitzondering opgeroepen. Een oplossing blijkt te zijn om de verbinding even te sluiten en dan weer te openen
            'v1.76: added error handling for situations where a second attempt to execute query did not work
            If RetryIfException Then
                Me.setup.Log.AddWarning("Warning: instable database connection. Connection was closed and opened again to execute query: " & myQuery)
                con.Close()
                If Not SQLiteNoQuery(con, myQuery, CloseAfterwards, False) Then
                    Me.setup.Log.AddError("Error executing query " & myQuery)
                    Me.setup.Log.AddError(ex.Message)
                    Return False
                Else
                    Return True
                End If
            Else
                Me.setup.Log.AddError("Error executing query " & myQuery)
                Me.setup.Log.AddError(ex.Message)
                Return False
            End If
            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try

    End Function

    Public Function SQLiteQuery(ByRef con As System.Data.SQLite.SQLiteConnection, ByVal myQuery As String, ByRef myTable As System.Data.DataTable, Optional ByVal CloseAfterwards As Boolean = True, Optional ByVal RetryIfException As Boolean = True) As Boolean
        'queries a DB (SQLite) database and returns the results in a datatable
        Try
            Dim da As SQLite.SQLiteDataAdapter
            If Not con.State = ConnectionState.Open Then con.Open()

            da = New SQLite.SQLiteDataAdapter(myQuery, con.ConnectionString)
            da.Fill(myTable)
            Return True
        Catch ex As Exception
            'SIEBE 12-3-2019
            'in sommige gevallen heeft de connectie een lees/schrijfprobleem en wordt een uitzondering opgeroepen. Een oplossing blijkt te zijn om de verbinding even te sluiten en dan weer te openen
            If RetryIfException Then
                Me.setup.Log.AddWarning("Warning: instable database connection. Connection was closed and opened again to execute query: " & myQuery)
                con.Close()
                SQLiteQuery(con, myQuery, myTable, CloseAfterwards, False)
                Return True
            Else
                Me.setup.Log.AddError("Error executing query " & myQuery)
                Me.setup.Log.AddError(ex.Message)
                Return False
            End If

            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try

    End Function


    Public Function MDBDatesNoQuery(ByRef con As OleDbConnection, query As String, pars As List(Of String), Dates As List(Of DateTime), Optional ByVal CloseAfterwards As Boolean = True) As Boolean
        Try
            'query = "INSERT INTO EVENTS (LOCATIONID, DATEANDTIME, PARAMETER, DATAVALUE, EVENTNUM, EVENTSUM, DURATION) VALUES ('" & cmbLocation.Text & "',@DT,'" & cmbParameter.Text & "'," & Values(j) & "," & EventNum & "," & EventSum & "," & txtDuration.Text & ");"
            If con.State = ConnectionState.Closed Then con.Open()
            Dim cmd As New OleDbCommand With {
                .Connection = con,
                .CommandText = query
            }
            cmd.Parameters.Clear()
            For i = 0 To pars.Count - 1
                cmd.Parameters.Add(pars(i), System.Data.OleDb.OleDbType.DBTimeStamp).Value = Dates(i)
            Next
            cmd.ExecuteNonQuery()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try
    End Function


    Public Function SQLiteDatesNoQuery(ByRef con As SQLite.SQLiteConnection, query As String, pars As List(Of String), Dates As List(Of DateTime), Optional ByVal CloseAfterwards As Boolean = True) As Boolean
        Try
            'query = "INSERT INTO EVENTS (LOCATIONID, DATEANDTIME, PARAMETER, DATAVALUE, EVENTNUM, EVENTSUM, DURATION) VALUES ('" & cmbLocation.Text & "',@DT,'" & cmbParameter.Text & "'," & Values(j) & "," & EventNum & "," & EventSum & "," & txtDuration.Text & ");"
            If con.State = ConnectionState.Closed Then con.Open()
            Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = con,
                .CommandText = query
            }
            cmd.Parameters.Clear()
            For i = 0 To pars.Count - 1
                cmd.Parameters.Add(pars(i), System.Data.OleDb.OleDbType.DBTimeStamp).Value = Dates(i)
            Next
            cmd.ExecuteNonQuery()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try
    End Function

    Public Function MDBDatesQuery(ByRef con As OleDbConnection, ByRef dt As DataTable, query As String, pars As List(Of String), dates As List(Of Date), Optional ByVal CloseAfterwards As Boolean = True) As Boolean
        'LET OP: GÉÉN quotes rond de variabelen plaatsen in query. Dan werkt de query niet. Voorbeeld van een goed werkende query:
        'cmd.CommandText = "SELECT DATEANDTIME, DATAVALUE FROM TIMESERIES WHERE LOCATIONID='POL1' AND PARAMETER='OVS' AND DATEANDTIME >= " & pars(0) & " AND DATEANDTIME <= " & pars(1) & ";"
        Try
            If con.State = ConnectionState.Closed Then con.Open()
            Dim dAdapter = New OleDbDataAdapter()
            Dim cmd As New OleDbCommand(query, con)
            For i = 0 To pars.Count - 1
                cmd.Parameters.AddWithValue(pars(i), dates(i)) ''  SqlDbType.DateTime).Value = dates(i)
            Next
            dAdapter.SelectCommand = cmd
            dt = New DataTable
            cmd.CommandText = query
            dAdapter.Fill(dt)
            Return True
        Catch ex As Exception
            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try
    End Function


    Public Function SQLiteDatesQuery(ByRef con As SQLite.SQLiteConnection, ByRef dt As DataTable, query As String, pars As List(Of String), dates As List(Of Date), Optional ByVal CloseAfterwards As Boolean = True) As Boolean
        'LET OP: GÉÉN quotes rond de variabelen plaatsen in query. Dan werkt de query niet. Voorbeeld van een goed werkende query:
        'cmd.CommandText = "SELECT DATEANDTIME, DATAVALUE FROM TIMESERIES WHERE LOCATIONID='POL1' AND PARAMETER='OVS' AND DATEANDTIME >= " & pars(0) & " AND DATEANDTIME <= " & pars(1) & ";"
        Try
            If con.State = ConnectionState.Closed Then con.Open()
            Dim dAdapter = New SQLite.SQLiteDataAdapter()
            Dim cmd As New SQLite.SQLiteCommand(query, con)
            For i = 0 To pars.Count - 1
                cmd.Parameters.AddWithValue(pars(i), dates(i)) ''  SqlDbType.DateTime).Value = dates(i)
            Next
            dAdapter.SelectCommand = cmd
            dt = New DataTable
            cmd.CommandText = query
            dAdapter.Fill(dt)
            Return True
        Catch ex As Exception
            Return False
        Finally
            If CloseAfterwards Then con.Close()
        End Try
    End Function

    Public Function IsEven(myNumber As Integer) As Boolean
        If myNumber Mod 2 = 0 Then Return True Else Return False
    End Function


    Public Function SQLiteColumnExists(ByRef con As SQLite.SQLiteConnection, TableName As String, ColumnName As String) As Boolean
        Try
            Dim TableSchema As Object, col As Object
            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("COLUMNS")
            col = TableSchema.Select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME='" & ColumnName & "'")
            If (col.Length > 0) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function MDBGetTables(ByRef connection As OleDbConnection) As List(Of String)
        Dim userTables As DataTable = Nothing
        Dim tablesList As New List(Of String)
        ' We only want user tables, not system tables
        Dim restrictions() As String = New String(3) {}
        restrictions(3) = "Table"
        connection.Open()
        ' Get list of user tables
        'userTables = connection.GetSchema("Tables", New String() {Nothing, Nothing, "TABLE"})
        userTables = connection.GetSchema("Tables", restrictions)
        connection.Close()
        ' Add list of table names to listBox
        Dim i As Integer
        For i = 0 To userTables.Rows.Count - 1 Step i + 1
            tablesList.Add(userTables.Rows(i)(2).ToString)
            'System.Console.WriteLine(userTables.Rows(i)(2).ToString())
        Next
        Return tablesList
    End Function


    Public Function MDBGetColumns(ByRef connection As OleDbConnection, tableName As String) As DataTable
        Try
            Dim filterValues = {Nothing, Nothing, tableName, Nothing}
            If Not connection.State = ConnectionState.Open Then connection.Open()
            Return connection.GetSchema("Columns", filterValues)
        Catch ex As Exception
            Return Nothing
        Finally
            connection.Close()
        End Try
    End Function

    Public Function MDBColumnExists(ByRef con As OleDbConnection, TableName As String, ColumnName As String) As Boolean
        Try
            Dim TableSchema As Object, col As Object
            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("COLUMNS")
            col = TableSchema.Select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME='" & ColumnName & "'")
            If (col.Length > 0) Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function MDBTableExists(ByRef con As OleDbConnection, TableName As String) As Boolean
        'Dim schema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, New Object() {Nothing, Nothing, Nothing, TableName})
        'Return schema.Rows.OfType(Of DataRow)().Any(Function(r) r.ItemArray(2).ToString().ToLower() = TableName.ToLower())
        Dim myTable As Object, TableSchema As Object

        If Not con.State = ConnectionState.Open Then con.Open()
        TableSchema = con.GetSchema("TABLES")

        myTable = TableSchema.select("TABLE_NAME='" & TableName & "'")
        Return (myTable.length > 0)
    End Function



    Public Function MDBCreateTable(ByRef con As System.Data.OleDb.OleDbConnection, TableName As String) As Boolean
        Try
            Dim myTable As Object, TableSchema As Object
            Dim query As String

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")

            myTable = TableSchema.select("TABLE_NAME='" & TableName & "'")
            If myTable.length = 0 Then
                query = "CREATE TABLE " & TableName & ";"
                setup.GeneralFunctions.MDBNoQuery(con, query, False)
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error creating table " & TableName & " in database.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SQLiteTableExists(ByRef con As SQLite.SQLiteConnection, TableName As String) As Boolean
        Dim dt As New DataTable
        Dim query As String = "SELECT * FROM sqlite_master WHERE type='table' AND name='" & TableName & "';"
        setup.GeneralFunctions.SQLiteQuery(con, query, dt, False)
        Return (dt.Rows.Count > 0)
    End Function

    Public Function SQLiteCreateTable(ByRef con As System.Data.SQLite.SQLiteConnection, TableName As String) As Boolean
        'this function creates a new table and includes a primary key field "id" for it
        Try
            Dim query As String

            If Not con.State = ConnectionState.Open Then con.Open()

            query = "CREATE TABLE " & TableName & "(" & vbCrLf
            query &= Chr(34) & "id" & Chr(34) & " INTEGER NOT NULL," & vbCrLf
            query &= "PRIMARY KEY(" & Chr(34) & "id" & Chr(34) & " AUTOINCREMENT)" & vbCrLf
            query &= ");"
            If Not setup.GeneralFunctions.SQLiteNoQuery(con, query, False) Then Throw New Exception("Error executing query " & query & ".")

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error creating table " & TableName & " In database.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function MDBCreateColumn(ByRef con As System.Data.OleDb.OleDbConnection, TableName As String, ColumnName As String, DataType As enmOleDBDataType, Optional ByVal COLINDEXNAME As String = "") As Boolean
        Try
            Dim TableSchema As Object, query As String
            Dim ColSchema As Object, col As Object
            Dim DataTypeStr As String = ""

            Select Case DataType
                Case Is = enmOleDBDataType.SQLVARCHAR
                    DataTypeStr = "VARCHAR"
                Case Is = enmOleDBDataType.SQLVARCHAR255
                    DataTypeStr = "VARCHAR(255)"
                Case Is = enmOleDBDataType.SQLDATE
                    DataTypeStr = "Date"
                Case Is = enmOleDBDataType.SQLDOUBLE
                    DataTypeStr = "Double"
                Case Is = enmOleDBDataType.SQLBIT
                    DataTypeStr = "BIT"
                Case Is = enmOleDBDataType.SQLFLOAT
                    DataTypeStr = "FLOAT"
                Case Is = enmOleDBDataType.SQLINT
                    DataTypeStr = "INT"
            End Select

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")
            ColSchema = con.GetSchema("COLUMNS")

            'create the column if it does not exist
            col = ColSchema.Select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME ='" & ColumnName & "'")
            If col.Length = 0 Then
                query = "ALTER TABLE " & TableName & " ADD COLUMN " & ColumnName & " " & DataTypeStr & ";"
                setup.GeneralFunctions.MDBNoQuery(con, query, False)

                'column was created. Now set it as indexed, if required
                If COLINDEXNAME <> "" Then
                    query = "CREATE INDEX " & COLINDEXNAME & " ON " & TableName & " (" & ColumnName & ");"
                    setup.GeneralFunctions.MDBNoQuery(con, query, False)
                End If
            End If


            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error creating column " & ColumnName & " in database table " & TableName)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SQLiteCreateDatabase(Path As String, OverWriteExisting As Boolean) As System.Data.SQLite.SQLiteConnection
        Dim SQLiteConnection As System.Data.SQLite.SQLiteConnection
        If System.IO.File.Exists(Path) AndAlso OverWriteExisting Then System.IO.File.Delete(Path)
        If Not My.Computer.FileSystem.FileExists(Path) Then
            ' Create the SQLite database
            SQLiteConnection = New System.Data.SQLite.SQLiteConnection("Data Source=" & Path & ";Version=3;")
            System.Data.SQLite.SQLiteConnection.CreateFile(Path)
            SQLiteConnection.Open()
            SQLiteConnection.Close()
        Else
            SQLiteConnection = New System.Data.SQLite.SQLiteConnection("Data Source=" & Path & ";Version=3;")
        End If
        Return SQLiteConnection
    End Function

    Public Function SQLiteCreateColumn(ByRef con As System.Data.SQLite.SQLiteConnection, TableName As String, ColumnName As String, DataType As enmSQLiteDataType, Optional ByVal COLINDEXNAME As String = "") As Boolean
        Try
            Dim TableSchema As Object, query As String
            Dim ColSchema As Object, col As Object
            Dim DataTypeStr As String = ""

            Select Case DataType
                Case Is = enmSQLiteDataType.SQLITETEXT
                    DataTypeStr = "TEXT"
                Case Is = enmSQLiteDataType.SQLITEREAL
                    DataTypeStr = "REAL"
                Case Is = enmSQLiteDataType.SQLITEINT
                    DataTypeStr = "INTEGER"
            End Select

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")
            ColSchema = con.GetSchema("COLUMNS")

            'create the column if it does not exist
            col = ColSchema.Select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME ='" & ColumnName & "'")
            If col.Length = 0 Then
                query = "ALTER TABLE " & TableName & " ADD COLUMN " & ColumnName & " " & DataTypeStr & ";"
                setup.GeneralFunctions.SQLiteNoQuery(con, query, False)

                'column was created. Now set it as indexed, if required
                If COLINDEXNAME <> "" Then
                    query = "CREATE INDEX " & COLINDEXNAME & " ON " & TableName & " (" & ColumnName & ");"
                    setup.GeneralFunctions.SQLiteNoQuery(con, query, False)
                End If
            End If


            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error creating column " & ColumnName & " in database table " & TableName)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SQLiteDeleteColumn(ByRef con As System.Data.SQLite.SQLiteConnection, TableName As String, ColumnName As String) As Boolean
        Try
            Dim TableSchema As Object
            Dim ColSchema As Object, col As Object
            Dim BackupTable As String = TableName & "_backup"

            'remove the backuptable if already exists
            If Me.setup.GeneralFunctions.SQLiteTableExists(Me.setup.SqliteCon, BackupTable) Then Me.setup.GeneralFunctions.SQLiteDropTable(Me.setup.SqliteCon, BackupTable)

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")
            ColSchema = con.GetSchema("COLUMNS")

            'SQLite does not support extensive alter table methods. So we will clone the table except for the column to delete
            Dim myQuery As String = "CREATE TABLE " & BackupTable & " As Select "

            col = ColSchema.Select("TABLE_NAME='" & TableName & "' AND NOT COLUMN_NAME ='" & ColumnName & "'")
            For i = 0 To col.length - 1
                myQuery &= col(i).item(3)
                If i < col.length - 1 Then myQuery &= ","
            Next
            myQuery &= " FROM " & TableName & ";"
            setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)

            'remove the original table
            SQLiteDropTable(Me.setup.SqliteCon, TableName)

            'rename the backuptable
            myQuery = "ALTER TABLE " & BackupTable & " RENAME TO " & TableName & ";"
            SQLiteNoQuery(Me.setup.SqliteCon, myQuery, True)

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error removing table " & ColumnName & " from database table " & TableName)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function MDBDeleteColumn(ByRef con As System.Data.OleDb.OleDbConnection, TableName As String, ColumnName As String) As Boolean
        Try
            Dim TableSchema As Object, query As String
            Dim ColSchema As Object, col As Object

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")
            ColSchema = con.GetSchema("COLUMNS")

            'create the column if it does not exist
            col = ColSchema.Select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME ='" & ColumnName & "'")
            If col.Length > 0 Then
                query = "ALTER TABLE " & TableName & " DROP " & ColumnName & ";"
                setup.GeneralFunctions.MDBNoQuery(con, query, False)
            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error removing table " & ColumnName & " from database table " & TableName)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SQLiteDropColumn(ByRef con As System.Data.SQLite.SQLiteConnection, TableName As String, ColumnName As String) As Boolean
        'NOTE: ALTER TABLE DROP COLUMN cannot be used in SQLITE
        'instead we need to copy the enrire table except for the column to be removed...
        'https://www.techonthenet.com/sqlite/tables/alter_table.php#:~:text=Drop%20column%20in%20table,data%20into%20the%20new%20table.

        Try
            Dim ColSchema As DataTable, col As DataRow()
            Dim TableCols As DataRow()
            Dim query As String
            Dim TempTable As String = TableName & "_old"

            'remove _old table if exists
            If SQLiteTableExists(con, TempTable) Then SQLiteDropTable(con, TempTable)

            If Not con.State = ConnectionState.Open Then con.Open()

            'get a list of columns and make a subselection for our current table
            ColSchema = con.GetSchema("COLUMNS")
            TableCols = ColSchema.Select("TABLE_NAME='" & TableName & "'") 'deelselectie met alle kolommen uit de gevraagde tabel

            col = ColSchema.Select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME='" & ColumnName & "'")
            If col.Length > 0 Then

                query = "PRAGMA foreign_keys = off;" & vbCrLf
                query &= "BEGIN TRANSACTION;" & vbCrLf

                'rename the original table
                query &= "ALTER TABLE " & TableName & " RENAME TO " & TempTable & ";" & vbCrLf

                'create the new table under the original name
                query &= "CREATE TABLE " & TableName & " ("
                For i = 0 To TableCols.Count - 1
                    If Not TableCols(i)("COLUMN_NAME") = ColumnName Then
                        query &= TableCols(i)("COLUMN_NAME") & " " & TableCols(i)("DATA_TYPE") & ","
                    End If
                Next
                query = Left(query, query.Length - 1) & ");" & vbCrLf 'remove the last , and add a closing bracket

                'insert the data from our old table except our column that needs to be dropped into the new table
                query &= "INSERT INTO " & TableName & " ("
                For i = 0 To TableCols.Count - 1
                    If Not TableCols(i)("COLUMN_NAME") = ColumnName Then
                        query &= TableCols(i)("COLUMN_NAME") & ","
                    End If
                Next
                query = Left(query, query.Length - 1) & ") SELECT " 'remove the last , and add the closing bracket, followed by the select statement
                For i = 0 To TableCols.Count - 1
                    If Not TableCols(i)("COLUMN_NAME") = ColumnName Then
                        query &= TableCols(i)("COLUMN_NAME") & ","
                    End If
                Next
                query = Left(query, query.Length - 1) & " FROM " & TempTable & ";" & vbCrLf
                query &= "COMMIT;" & vbCrLf
                query &= "PRAGMA foreign_keys =On;"

                'execute the query
                If Not SQLiteNoQuery(con, query, False) Then Throw New Exception("Could not remove database column " & ColumnName & " from table " & TableName & ".")

                'finally remove the temporary table
                If Not SQLiteDropTable(con, TempTable) Then Throw New Exception("Could not remove intermediate table " & TempTable & " from database in the process of removing column " & ColumnName & " from " & TableName)

            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error deleting column " & ColumnName & " from table " & TableName & " In database.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function MDBDropColumn(ByRef con As System.Data.OleDb.OleDbConnection, TableName As String, ColumnName As String) As Boolean
        Try
            Dim ColSchema As Object, col As Object
            Dim query As String

            If Not con.State = ConnectionState.Open Then con.Open()
            ColSchema = con.GetSchema("COLUMNS")

            col = ColSchema.select("TABLE_NAME='" & TableName & "' AND COLUMN_NAME='" & ColumnName & "'")
            If col.length > 0 Then
                query = "ALTER TABLE " & TableName & " DROP COLUMN " & ColumnName & ";"
                setup.GeneralFunctions.MDBNoQuery(con, query, False)
            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error deleting column " & ColumnName & " from table " & TableName & " in database.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function MDBDropTable(ByRef con As System.Data.OleDb.OleDbConnection, TableName As String) As Boolean
        Try
            Dim TableSchema As Object, Table As Object
            Dim query As String

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")

            Table = TableSchema.select("TABLE_NAME='" & TableName & "'")
            If Table.length > 0 Then
                query = "DROP TABLE " & TableName & ";"
                setup.GeneralFunctions.MDBNoQuery(con, query, False)
            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error deleting table " & TableName & " from database.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SQLiteDropTable(ByRef con As System.Data.SQLite.SQLiteConnection, TableName As String) As Boolean
        Try
            Dim TableSchema As Object, Table As Object
            Dim query As String

            If Not con.State = ConnectionState.Open Then con.Open()
            TableSchema = con.GetSchema("TABLES")

            Table = TableSchema.select("TABLE_NAME='" & TableName & "'")
            If Table.length > 0 Then
                query = "DROP TABLE " & TableName & ";"
                If Not setup.GeneralFunctions.SQLiteNoQuery(con, query, False) Then Throw New Exception("Could not delete table " & TableName & " from database.")
            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub CopyDataGridViewToClipboard(ByRef dgv As Windows.Forms.DataGridView)
        'https://www.codeproject.com/Tips/491462/Copying-DataGridView-Contents-To-Clipboard
        'This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)
        Dim s As String = ""
        Dim oCurrentCol As Windows.Forms.DataGridViewColumn    'Get header
        oCurrentCol = dgv.Columns.GetFirstColumn(Windows.Forms.DataGridViewElementStates.Visible)
        Do
            s &= oCurrentCol.HeaderText & vbTab
            oCurrentCol = dgv.Columns.GetNextColumn(oCurrentCol,
         Windows.Forms.DataGridViewElementStates.Visible, Windows.Forms.DataGridViewElementStates.None)
        Loop Until oCurrentCol Is Nothing
        s = s.Substring(0, s.Length - 1)
        s &= Environment.NewLine    'Get rows
        For Each row As Windows.Forms.DataGridViewRow In dgv.Rows
            oCurrentCol = dgv.Columns.GetFirstColumn(Windows.Forms.DataGridViewElementStates.Visible)
            Do
                If row.Cells(oCurrentCol.Index).Value IsNot Nothing Then
                    s &= row.Cells(oCurrentCol.Index).Value.ToString
                End If
                s &= vbTab
                oCurrentCol = dgv.Columns.GetNextColumn(oCurrentCol,
              Windows.Forms.DataGridViewElementStates.Visible, Windows.Forms.DataGridViewElementStates.None)
            Loop Until oCurrentCol Is Nothing
            s = s.Substring(0, s.Length - 1)
            s &= Environment.NewLine
        Next    'Put to clipboard
        Dim o As New Forms.DataObject
        o.SetText(s)
        Forms.Clipboard.SetDataObject(o, True)
    End Sub

    Public Function GetSobekTimeseries(ObjectID As String, ByVal ResultsType As enmSobekResultsType, Optional ByVal LeftCensoringPercentage As Double = 0) As DataTable
        Try
            Dim dt As New DataTable
            dt.Columns.Add("DATUM", Type.GetType("System.DateTime"))
            dt.Columns.Add(ObjectID, Type.GetType("System.Single"))

            'read the waterlevels from hisfile
            Dim myReader As clsHisFileBinaryReader

            If ResultsType = enmSobekResultsType.CalcpntH Then
                myReader = New clsHisFileBinaryReader(Me.setup.SOBEKData.ActiveProject.ActiveCase.GetCaseDir & "\CALCPNT.HIS", Me.setup)
                myReader.ReadLocationResultsToDataTable(ObjectID, "Waterl", dt)
                If LeftCensoringPercentage > 0 Then Me.setup.GeneralFunctions.DataTableLeftCensoring(dt, 1, LeftCensoringPercentage)
            ElseIf ResultsType = enmSobekResultsType.StrucQ Then
                myReader = New clsHisFileBinaryReader(Me.setup.SOBEKData.ActiveProject.ActiveCase.GetCaseDir & "\STRUC.HIS", Me.setup)
                myReader.ReadLocationResultsToDataTable(ObjectID, "Disch", dt)
                If LeftCensoringPercentage > 0 Then Me.setup.GeneralFunctions.DataTableLeftCensoring(dt, 1, LeftCensoringPercentage)
            Else
                Throw New Exception("Error: results type not yet supported: " & ResultsType.ToString)
            End If
            Return dt
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Function SobekTimeseriesToDatatable(ByRef ht As DataTable, ByRef SbkCase As ClsSobekCase, HisfileColIdx As Integer, HisParColIdx As Integer, HisLocColIdx As Integer, Optional ByVal LeftCensoringPercentage As Double = 0) As DataTable
        Try
            Dim dt As New DataTable
            Dim myReader As clsHisFileBinaryReader

            dt.Columns.Add("DATUM", Type.GetType("System.DateTime"))
            dt.Columns.Add("WAARDE", Type.GetType("System.Single"))


            Dim HisLoc As String = ht.Rows(0)(HisLocColIdx), HisFile As String = ht.Rows(0)(HisfileColIdx), HisPar As String = ht.Rows(0)(HisParColIdx)
            myReader = New clsHisFileBinaryReader(SbkCase.GetCaseDir & "\" & HisFile, Me.setup)
            myReader.ReadLocationResultsToDataTable(HisLoc, HisPar, dt)

            For i = 1 To ht.Rows.Count - 1
                Dim dti As New DataTable
                dti.Columns.Add("DATUM", Type.GetType("System.DateTime"))
                dti.Columns.Add("WAARDE", Type.GetType("System.Single"))
                myReader = New clsHisFileBinaryReader(SbkCase.GetCaseDir & "\" & ht.Rows(i)(HisfileColIdx), Me.setup)
                myReader.ReadLocationResultsToDataTable(ht.Rows(i)(HisLocColIdx), ht.Rows(i)(HisParColIdx), dti)

                For r = 0 To dt.Rows.Count - 1
                    dt.Rows(r)(1) += dti.Rows(r)(1)
                Next

            Next
            Return dt
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function AddScatterPlot(ByRef chtResults As Forms.DataVisualization.Charting.Chart, SeriesName As String, xAxisTitle As String, yAxisTitle As String, ByRef dt As DataTable, xColIdx As Integer, yColIdx As Integer) As Boolean
        Try
            Dim series As New Forms.DataVisualization.Charting.Series(SeriesName) With {
                .ChartArea = chtResults.ChartAreas(0).Name
            }
            chtResults.ChartAreas(0).AxisX.Title = xAxisTitle
            chtResults.ChartAreas(0).AxisY.Title = yAxisTitle
            chtResults.ChartAreas(0).AxisX.Enabled = True
            chtResults.ChartAreas(0).AxisX.MinorGrid.Enabled = True
            chtResults.ChartAreas(0).AxisX.LabelStyle.Angle = 90
            chtResults.ChartAreas(0).AxisX.IsLabelAutoFit = True
            chtResults.ChartAreas(0).AxisX.LabelStyle.Font = New System.Drawing.Font("Times New Roman", 8.0F)
            chtResults.ChartAreas(0).AxisX.LabelStyle.Interval = 1
            series.ChartType = Forms.DataVisualization.Charting.SeriesChartType.FastPoint
            For i = 0 To dt.Rows.Count - 1
                series.Points.AddXY(dt.Rows(i)(xColIdx), dt.Rows(i)(yColIdx))
            Next
            'chtResults.Series(SeriesName).Points.AddXY(2, 3)
            'chtResults.Series(SeriesName).Points.AddXY(4, 5)


            chtResults.Series.Add(series)
            chtResults.Series(chtResults.Series.Count - 1).XValueType = Forms.DataVisualization.Charting.ChartValueType.Single
            chtResults.ChartAreas(0).AxisX.Minimum = chtResults.ChartAreas(0).AxisY.Minimum
            chtResults.ChartAreas(0).AxisX.Maximum = chtResults.ChartAreas(0).AxisY.Maximum
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function AddColumnChartSeries(ByRef chtResults As Forms.DataVisualization.Charting.Chart, SeriesName As String, ByRef dt As DataTable, XColIdx As Integer, YColIdx As Integer) As Boolean
        Dim i As Integer

        Dim series As New Forms.DataVisualization.Charting.Series(SeriesName) With {
            .ChartArea = chtResults.ChartAreas(0).Name
        }
        chtResults.ChartAreas(0).AxisX.MajorTickMark.Interval = 1
        series.ChartType = Forms.DataVisualization.Charting.SeriesChartType.Column

        For i = 0 To dt.Rows.Count - 1
            series.Points.AddXY(dt.Rows(i)(XColIdx), dt.Rows(i)(YColIdx))
        Next
        chtResults.Series.Add(series)

    End Function

    Public Function AddHistogramChartSeries(ByRef chtResults As Forms.DataVisualization.Charting.Chart, SeriesName As String, minVal As Double, maxVal As Double, Bins As Integer, ByRef dt As DataTable, ColIdx As Integer, StartIdx As Integer, EndIdx As Integer, ByRef yMin As Integer, ByRef yMax As Integer) As Boolean
        Dim curMin As Double, curMax As Double, curLabel As String, curCount As Integer
        Dim chartMin As Double, chartMax As Double
        Dim i As Integer, j As Integer

        Dim series As New Forms.DataVisualization.Charting.Series(SeriesName) With {
            .ChartArea = chtResults.ChartAreas(0).Name
        }
        chtResults.ChartAreas(0).AxisX.MajorTickMark.Interval = 1
        series.ChartType = Forms.DataVisualization.Charting.SeriesChartType.Column
        series.IsValueShownAsLabel = False

        ' Formatting X Axis
        chtResults.ChartAreas(0).AxisX.MajorGrid.Enabled = False
        ' CArea.AxisX.IntervalOffset = 0.99
        ' CArea.AxisX.IntervalOffsetType = DateTimeIntervalType.Months
        ' CArea.AxisX.LabelStyle.Format = "MMM"
        ' CArea.AxisX.IntervalType = DateTimeIntervalType.Months
        ' CArea.AxisX.Interval = 1
        chtResults.ChartAreas(0).AxisX.LabelStyle.Angle = 90
        chtResults.ChartAreas(0).AxisX.IsLabelAutoFit = True
        chtResults.ChartAreas(0).AxisX.LabelStyle.Font = New System.Drawing.Font("Times New Roman", 8.0F)
        chtResults.ChartAreas(0).AxisX.LabelStyle.Interval = 1

        'Dim dtable As DataTable = New DataTable
        'dtable.Columns.Add("range")
        'dtable.Columns.Add("amount")
        Me.setup.GeneralFunctions.GetAxisValuesFromRange(minVal, maxVal, chartMin, chartMax)

        For i = 0 To Bins + 1
            curMin = chartMin + i * (chartMax - chartMin) / Bins
            curMax = chartMin + (i + 1) * (chartMax - chartMin) / Bins
            curLabel = Format(curMin, "0.00") & "-" & Format(curMax, "0.00")
            curCount = 0
            For j = 0 To dt.Rows.Count - 1
                If Not IsDBNull(dt.Rows(j)(ColIdx)) AndAlso dt.Rows(j)(ColIdx) >= curMin AndAlso dt.Rows(j)(ColIdx) < curMax Then
                    curCount += 1
                End If
            Next
            'Dim row As DataRow
            'row = dtable.NewRow
            'row("range") = curLabel
            'row("amount") = curCount
            'dtable.Rows.Add(row)
            series.Points.AddXY(curLabel, curCount)

            If i = 0 Then
                yMin = curCount
                yMax = curCount
            Else
                If curCount > yMax Then yMax = curCount
                If curCount < yMin Then yMin = curCount
            End If

        Next
        chtResults.Series.Add(series)

    End Function

    Public Sub on_boxplot(ByRef chtResults As Forms.DataVisualization.Charting.Chart, ByRef ObservedTable As DataTable)
        Dim xValue As List(Of String) = New List(Of String) From {"Observed"}

        chtResults.Series.Clear()
        chtResults.Series.Add("BoxPlotSeries")

        For i As Int32 = 0 To xValue.Count - 1
            chtResults.Series.Add(xValue(i))
        Next

        For i = 0 To ObservedTable.Rows.Count - 1
            chtResults.Series(xValue(0)).Points.AddY(ObservedTable.Rows(i)(1))
        Next

        chtResults.Series("BoxPlotSeries").ChartType = Forms.DataVisualization.Charting.SeriesChartType.BoxPlot
        chtResults.Series("BoxPlotSeries")("BoxPlotSeries") = xValue(0)
        chtResults.ChartAreas.Add("BoxPlot")
        chtResults.Series("BoxPlotSeries").ChartArea = "BoxPlot"
        chtResults.Series("BoxPlotSeries")("BoxPlotWhiskerPercentile") = "0"
        chtResults.Series("BoxPlotSeries")("BoxPlotPercentile") = "25"
        chtResults.Series("BoxPlotSeries")("BoxPlotShowAverage") = "true"
        chtResults.Series("BoxPlotSeries")("BoxPlotShowMedian") = "true"
        chtResults.Series("BoxPlotSeries")("BoxPlotShowUnusualValues") = "true"
        chtResults.Series("BoxPlotSeries")("MaxPixelPointWidth") = "15"
        chtResults.Series("BoxPlotSeries").BorderWidth = 2
        'Chart.Show()
    End Sub




    Public Function AddBoxPlot(ByRef chtResults As Forms.DataVisualization.Charting.Chart, ObjectID As String, ByRef ResultsTable As DataTable, ResultsColIdx As Integer, ByVal ResultsType As enmSobekResultsType) As Boolean
        Try
            chtResults.Series.Clear()
            chtResults.Series.Add("BoxPlotSeries")

            'read the waterlevels from hisfile
            Dim yAxisTitle As String

            If ResultsType = enmSobekResultsType.CalcpntH Then
                yAxisTitle = "Water level (m above datum)"
            ElseIf ResultsType = enmSobekResultsType.StrucQ Then
                yAxisTitle = "Discharge (m3/s)"
            Else
                Throw New Exception("Error: results type not yet supported: " & ResultsType.ToString)
            End If

            'set the x axis and its formatting
            chtResults.ChartAreas(0).AxisX.Title = "Result"
            chtResults.ChartAreas(0).AxisX.TextOrientation = Forms.DataVisualization.Charting.TextOrientation.Rotated270
            chtResults.ChartAreas(0).AxisY.Title = yAxisTitle

            Dim cols As New List(Of Integer)
            cols.Add(ResultsColIdx)
            Dim myMin As Double = setup.GeneralFunctions.DataTableMin(ResultsTable, cols)
            Dim myMax As Double = setup.GeneralFunctions.DataTableMax(ResultsTable, cols)
            setup.GeneralFunctions.GetAxisValuesFromRange(myMin, myMax, chtResults.ChartAreas(0).AxisY.Minimum, chtResults.ChartAreas(0).AxisY.Maximum)

            chtResults.Titles.Add(ObjectID)
            chtResults.Series.Add(ResultsTable.TableName)
            'chtResults.Series(ResultsTable.TableName).YValueMembers = ResultsTable.Columns(ResultsColIdx).ColumnName
            For i = 0 To ResultsTable.Rows.Count - 1
                chtResults.Series(ResultsTable.TableName).Points.AddY(ResultsTable.Rows(i)(ResultsColIdx))
            Next
            chtResults.Series(ResultsTable.TableName).ChartType = Forms.DataVisualization.Charting.SeriesChartType.BoxPlot
            chtResults.Series("BoxPlotSeries").ChartType = Forms.DataVisualization.Charting.SeriesChartType.BoxPlot
            chtResults.Series("BoxPlotSeries")("BoxPlotSeries") = ResultsTable.TableName
            chtResults.ChartAreas.Add("BoxPlot")
            chtResults.Series("BoxPlotSeries").ChartArea = "BoxPlot"
            chtResults.Series(chtResults.Series.Count - 1)("BoxPlotWhiskerPercentile") = "0"
            chtResults.Series(chtResults.Series.Count - 1)("BoxPlotPercentile") = "25"
            chtResults.Series(chtResults.Series.Count - 1)("BoxPlotShowAverage") = "true"
            chtResults.Series(chtResults.Series.Count - 1)("BoxPlotShowMedian") = "true"
            chtResults.Series(chtResults.Series.Count - 1)("BoxPlotShowUnusualValues") = "true"
            chtResults.Series(chtResults.Series.Count - 1)("MaxPixelPointWidth") = "15"
            chtResults.Series(chtResults.Series.Count - 1).BorderWidth = 2

            chtResults.ChartAreas.RemoveAt(0)

            'chtResults.Series(chtResults.Series.Count - 1).XValueType = Forms.DataVisualization.Charting.ChartValueType.String            'set the x axis type to datetime
            'chtResults.DataSource = ResultsTable
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



    Public Function AddSobekSeriesToChart(ByRef chtResults As Forms.DataVisualization.Charting.Chart, ObjectID As String, ByVal ResultsType As enmSobekResultsType) As Boolean
        Try
            Dim dt As New DataTable
            dt.Columns.Add("DATUM", Type.GetType("System.DateTime"))
            dt.Columns.Add(ObjectID, Type.GetType("System.Single"))
            'xxxxx


            'Dim x As New System.DateTime(2008, 11, 21)
            'Chart1.Series(0).Points.AddXY(x.ToOADate(), 34)

            'read the waterlevels from hisfile
            Dim myReader As clsHisFileBinaryReader
            Dim yAxisTitle As String

            If ResultsType = enmSobekResultsType.CalcpntH Then
                myReader = New clsHisFileBinaryReader(Me.setup.SOBEKData.ActiveProject.ActiveCase.GetCaseDir & "\CALCPNT.HIS", Me.setup)
                myReader.ReadLocationResultsToDataTable(ObjectID, "Waterl", dt)
                yAxisTitle = "Water level (m above datum)"
            ElseIf ResultsType = enmSobekResultsType.StrucQ Then
                myReader = New clsHisFileBinaryReader(Me.setup.SOBEKData.ActiveProject.ActiveCase.GetCaseDir & "\STRUC.HIS", Me.setup)
                myReader.ReadLocationResultsToDataTable(ObjectID, "Disch", dt)
                yAxisTitle = "Discharge (m3/s)"
            Else
                Throw New Exception("Error: results type not yet supported: " & ResultsType.ToString)
            End If

            'set the x axis and its formatting
            chtResults.ChartAreas(0).AxisX.Title = "Date/Time"
            chtResults.ChartAreas(0).AxisX.LabelStyle.Format = "dd-MM-yy"
            chtResults.ChartAreas(0).AxisX.TextOrientation = Forms.DataVisualization.Charting.TextOrientation.Horizontal
            chtResults.ChartAreas(0).AxisY.Title = yAxisTitle

            Dim cols As New List(Of Integer)
            cols.Add(1)
            Dim myMin As Double = setup.GeneralFunctions.DataTableMin(dt, cols)
            Dim myMax As Double = setup.GeneralFunctions.DataTableMax(dt, cols)
            setup.GeneralFunctions.GetAxisValuesFromRange(myMin, myMax, chtResults.ChartAreas(0).AxisY.Minimum, chtResults.ChartAreas(0).AxisY.Maximum)

            chtResults.Titles.Add(ObjectID)
            chtResults.Series.Add("Computed")
            chtResults.Series("Computed").XValueMember = "DATUM"
            chtResults.Series("Computed").YValueMembers = ObjectID
            chtResults.Series("Computed").ChartType = Forms.DataVisualization.Charting.SeriesChartType.Line
            chtResults.Series(chtResults.Series.Count - 1).XValueType = Forms.DataVisualization.Charting.ChartValueType.DateTime            'set the x axis type to datetime
            chtResults.DataSource = dt
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function AddTimeseriesChartSeries(ByRef chtResults As Forms.DataVisualization.Charting.Chart, SeriesName As String, ObjectID As String, ByRef dt As DataTable) As Boolean
        Try
            Dim i As Long
            chtResults.Series.Add(SeriesName)
            chtResults.Series(SeriesName).XValueMember = "DATUM"
            chtResults.Series(SeriesName).YValueMembers = ObjectID
            chtResults.ChartAreas(0).AxisY.MinorGrid.Enabled = True
            chtResults.ChartAreas(0).AxisY.MinorGrid.Enabled = True
            chtResults.ChartAreas(0).AxisY.MinorGrid.Enabled = True
            chtResults.ChartAreas(0).AxisY.MinorGrid.Enabled = True
            chtResults.Series(SeriesName).ChartType = Forms.DataVisualization.Charting.SeriesChartType.Line
            chtResults.Series(chtResults.Series.Count - 1).XValueType = Forms.DataVisualization.Charting.ChartValueType.DateTime            'set the x axis type to datetime
            If Not dt Is Nothing Then
                For i = 0 To dt.Rows.Count - 1
                    'chtResults.Series(SeriesName).Points.AddXY(Convert.ToDateTime(dt.Rows(i)(0), dt.Rows(i)(1)))
                    chtResults.Series(SeriesName).Points.AddXY(Convert.ToDateTime(dt.Rows(i)(0)), dt.Rows(i)(1))
                Next
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function AddObservedValuesToChart(ByRef chtResults As Forms.DataVisualization.Charting.Chart, ChartArea As String, ObjectID As String, ByVal ResultsType As enmSobekResultsType, ByRef dt As DataTable) As Boolean
        Try
            Dim i As Long
            chtResults.Series.Add("Observed")
            chtResults.Series("Observed").ChartType = Forms.DataVisualization.Charting.SeriesChartType.Line
            For i = 0 To dt.Rows.Count - 1
                chtResults.Series("Observed").Points.AddXY(dt.Rows(i)(0), dt.Rows(i)(1))
                chtResults.Series("Observed").ChartArea = ChartArea
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReplaceStringContentByToken(ByRef myContent As String, ByVal Token As String, ByVal newVal As Double) As Boolean
        Dim pos1 As Long, pos2 As Long
        Dim lines() As String, i As Long

        'aangepast op 7 nov 2015 door Siebe Bosch. het bewerken van de hele filecontent in een keer bleek te traag
        'daarom nu een split by harde return toegevoegd, en regel voor regel afhandelen

        Try
            lines = Split(myContent, vbCrLf)
            For i = 0 To lines.Count - 1
                While InStr(lines(i), Token, CompareMethod.Text) > 0
                    pos1 = InStr(lines(i), Token, CompareMethod.Text)
                    pos2 = InStr(pos1 + 1, lines(i), Token, CompareMethod.Text)
                    If pos1 > 0 AndAlso pos2 > pos1 Then
                        lines(i) = Left(lines(i), pos1 - 1) & newVal & Right(lines(i), lines(i).Length - pos2 - Token.Length + 1)
                    Else
                        Throw New Exception("Error replacing string content by token " & Token & ". Token is probably not symmetric around value to be adjusted.")
                    End If
                End While
            Next
            myContent = lines(0)
            For i = 1 To lines.Count - 1
                myContent &= vbCrLf & lines(i)
            Next

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetExponentFromNumber(ByVal myNumber As Double) As Integer
        Dim myPower As Integer = 0

        If myNumber < 1 Then
            While Not myNumber > 1
                myPower -= 1
                myNumber *= 10
            End While
        ElseIf myNumber >= 1 Then
            While Not myNumber / 10 < 1
                myPower += 1
                myNumber /= 10
            End While
        End If
        Return myPower

    End Function

    Public Function GetAxisValuesFromRange(ByVal min As Double, ByVal max As Double, ByRef chartMin As Double, ByRef chartMax As Double) As Boolean
        Dim myRange As Double
        Dim myExponent As Integer
        Dim n As Integer
        Try
            chartMin = min
            chartMax = max
            myRange = Math.Abs(max - min)

            If myRange = 0 Then
                If min = 0 Then
                    chartMin = min - 1
                    chartMax = min + 1
                    Return True
                Else
                    'min and max are equal so find a decent min and max around it
                    chartMin = setup.GeneralFunctions.RoundUD(Math.Min(min / 2, min * 2), 0, False)
                    chartMax = setup.GeneralFunctions.RoundUD(Math.Max(min / 2, min * 2), 0, True)
                    Return True
                End If
            End If

            myExponent = GetExponentFromNumber(myRange)
            If myExponent < 0 Then
                While RoundUD(min, 0, False) = RoundUD(max, 0, False)
                    min *= 10
                    max *= 10
                    n += 1
                End While
                chartMin = RoundUD(min, 0, False) / 10 ^ n
                chartMax = RoundUD(max, 0, True) / 10 ^ n
            ElseIf myExponent >= 0 Then
                chartMin = RoundUD(min / 10 ^ (myExponent - 1), 0, False) * 10 ^ (myExponent - 1)
                chartMax = RoundUD(max / 10 ^ (myExponent - 1), 0, True) * 10 ^ (myExponent - 1)
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function PopulateGridFromDatabaseQuery(ByRef con As SQLite.SQLiteConnection, ByVal query As String, ByRef myGrid As Forms.DataGridView, Optional ByVal Transpose As Boolean = False) As Boolean
        'note: if only unique values are required, use SELECT DISTINCT in the SQL-query
        Try
            Dim dt As DataTable
            Dim da As SQLite.SQLiteDataAdapter

            If Not con.State = ConnectionState.Open Then con.Open()

            dt = New DataTable
            da = New SQLite.SQLiteDataAdapter(query, con)
            da.Fill(dt)

            If Transpose Then dt = Me.setup.GeneralFunctions.TransposeDataTable(dt)

            myGrid.DataSource = dt
            con.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function PeriodicityEnumToString(Periodicity As enmPeriodicity) As String
        'this function converts the periodicity in enum format to the string value as required by SOBEK
        Select Case Periodicity
            Case Is = enmPeriodicity.NONE
                Return ""
            Case Is = enmPeriodicity.HOUR
                Return "0;01:00:00"
            Case Is = enmPeriodicity.HALFDAY
                Return "0;12:00:00"
            Case Is = enmPeriodicity.TIDALPERIODHALFDAY
                Return "0;12:25:00"
            Case Is = enmPeriodicity.DAY
                Return "1;00:00:00"
            Case Is = enmPeriodicity.TIDALPERIODDAY
                Return "1;00:50:00"
            Case Is = enmPeriodicity.WEEK
                Return "7;00:00:00"
            Case Is = enmPeriodicity.FOURWEEKS
                Return "28;00:00:00"
            Case Is = enmPeriodicity.MONTH
                Return "30;00:00:00"
            Case Is = enmPeriodicity.YEAR
                Return "365;00:00:00"
            Case Else
                Return ""
        End Select
    End Function



    Public Sub PopulateComboBoxWithTimeseriesStatisticOptions(ByRef myCmb As Windows.Forms.ComboBox, Optional InitialSelectionFirstItem As Boolean = False)
        Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmHydroMathOperation), InitialSelectionFirstItem)
    End Sub

    Public Sub PopulateComboboxWithPumpCapacityUnits(ByRef myCmb As Windows.Forms.ComboBox, Optional InitialSelectionFirstItem As Boolean = False)
        Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmPumpCapacityUnits), InitialSelectionFirstItem)
    End Sub

    Public Sub PopulateComboboxWithPumpOnOffLevelUnits(ByRef myCmb As Windows.Forms.ComboBox, Optional InitialSelectionFirstItem As Boolean = False)
        Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmPumpOnOffMarginUnits), InitialSelectionFirstItem)
    End Sub

    Public Sub PopulateComboBoxWithPeriodicityTypes(ByRef myCmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmPeriodicity))
    End Sub

    Public Sub PopulateComboBoxWithFrictionTypes(ByRef myCmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmFrictionType))
    End Sub

    Public Sub PopulateComboBoxWithElevationUnits(ByRef myCMB As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(myCMB, GetType(enmElevationUnits), True)
    End Sub
    Public Sub PopulateComboBoxWithConveyanceMethods(ByRef myCmb As Windows.Forms.ComboBox, Optional PreselectFirstItem As Boolean = True)
        Call PopulateComboBoxWithEnumNames(myCmb, GetType(enmConveyanceMethod))
        If PreselectFirstItem Then
            myCmb.SelectedIndex = 0
        End If
    End Sub

    Public Sub PopulateComboboxWithSobekResultsAtTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekResultsObjectTypes))
    End Sub

    Public Function GetHisFileFromResultsAt(ResultsAt As enmSobekResultsObjectTypes) As String
        Select Case ResultsAt
            Case enmSobekResultsObjectTypes.Nodes
                Return "calcpnt.his"
            Case enmSobekResultsObjectTypes.ReachSegments
                Return "reachseg.his"
            Case enmSobekResultsObjectTypes.Structures
                Return "struc.his"
            Case enmSobekResultsObjectTypes.LateralNodes
                Return "qlat.his"
            Case enmSobekResultsObjectTypes.UnpavedNodes
                Return "upflowdt.his"
            Case enmSobekResultsObjectTypes.PavedNodes
                Return "pvstordt.his"
            Case enmSobekResultsObjectTypes.GreenhouseNodes
                Return "greenhse.his"
            Case Else
                Return ""
        End Select
    End Function

    Public Sub PopulateComboboxWithSobekNodeResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekNodeResultsTypes))
    End Sub

    Public Function gethisParFromResultsType(ResultsType As enmModelResultsType) As String
        Select Case ResultsType
            Case enmModelResultsType.WaterLevel
                Return "Waterl"
            Case enmModelResultsType.Depth
                Return "Depth"
            Case enmModelResultsType.Discharge
                Return "Disch"
            Case enmModelResultsType.Velocity
                Return "Veloc"
            Case Else
                Return ""
        End Select
    End Function

    Public Sub PopulateCheckedListBoxWithSobekNodeResultsTypes(ByRef mychk As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mychk, GetType(enmSobekNodeResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithSobekReachResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekReachResultsTypes))
    End Sub

    Public Sub PopulateCheckedListBoxWithSobekReachResultsTypes(ByRef mychk As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mychk, GetType(enmSobekReachResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithSobekStructureResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekStructureResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithValidationTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmValidationAction))
    End Sub

    Public Sub PopulateComboboxWithSobekResultsParameters(ResultsAt As enmSobekResultsObjectTypes, ByRef cmbParameter As Windows.Forms.ComboBox)
        'this function populates a combobox with all available parameters for a given sobek resultstype
        cmbParameter.Items.Clear()
        Select Case ResultsAt
            Case Is = enmSobekResultsObjectTypes.Nodes
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekNodeResultsTypes(cmbParameter)
            Case Is = enmSobekResultsObjectTypes.ReachSegments
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekReachResultsTypes(cmbParameter)
            Case Is = enmSobekResultsObjectTypes.LateralNodes
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekLateralResultsTypes(cmbParameter)
            Case Is = enmSobekResultsObjectTypes.Structures
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekStructureResultsTypes(cmbParameter)
            Case Is = enmSobekResultsObjectTypes.UnpavedNodes
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekUnpavedResultsTypes(cmbParameter)
            Case Is = enmSobekResultsObjectTypes.PavedNodes
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekPavedResultsTypes(cmbParameter)
            Case Is = enmSobekResultsObjectTypes.GreenhouseNodes
                Me.setup.GeneralFunctions.PopulateComboboxWithSobekGreenhouseResultsTypes(cmbParameter)
        End Select

    End Sub
    Public Sub PopulateComboboxWithHisParameters(ByRef mycmb As Windows.Forms.ComboBox, ByVal HisFilePath As String)
        Dim HisReader As New clsHisFileBinaryReader(HisFilePath, Me.setup)
        Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = HisReader.ReadHisHeader
        mycmb.Items.Clear()
        For i = 0 To myHeader.parameters.Count - 1
            mycmb.Items.Add(myHeader.parameters(i))
        Next
    End Sub

    Public Sub PopulateCheckedListboxWithHisParameters(ByRef mychk As Windows.Forms.CheckedListBox, ByVal HisFilePath As String)
        Dim HisReader As New clsHisFileBinaryReader(HisFilePath, Me.setup)
        Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = HisReader.ReadHisHeader
        mychk.Items.Clear()
        For i = 0 To myHeader.parameters.Count - 1
            mychk.Items.Add(myHeader.parameters(i))
        Next
    End Sub


    Public Sub PopulateCheckedListBoxWithSobekStructureResultsTypes(ByRef mychk As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mychk, GetType(enmSobekStructureResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithSobekLateralResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekLateralResultsTypes))
    End Sub

    Public Sub PopulateCheckedListBoxWithSobekLateralResultsTypes(ByRef mychk As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mychk, GetType(enmSobekLateralResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithSobekUnpavedResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekUnpavedResultsTypes))
    End Sub

    Public Sub PopulateCheckedListBoxWithSobekUnpavedResultsTypes(ByRef mycmb As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mycmb, GetType(enmSobekUnpavedResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithSobekPavedResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekPavedResultsTypes))
    End Sub

    Public Sub PopulateCheckedListBoxWithSobekPavedResultsTypes(ByRef mycmb As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mycmb, GetType(enmSobekPavedResultsTypes))
    End Sub

    Public Sub PopulateComboboxWithSobekGreenhouseResultsTypes(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxWithEnumNames(mycmb, GetType(enmSobekGreenhouseResultsTypes))
    End Sub

    Public Sub PopulateCheckedListBoxWithSobekGreenhouseResultsTypes(ByRef mycmb As Windows.Forms.CheckedListBox)
        Call populateCheckedListBoxWithEnumNames(mycmb, GetType(enmSobekGreenhouseResultsTypes))
    End Sub

    Public Sub populateDataGridViewComboBoxWithFrictionTypes(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmFrictionType), ConvertToUppercase)
    End Sub

    Public Sub populateComboBoxWithEquationComponents(ByRef mycmb As Windows.Forms.ComboBox)
        Call PopulateComboBoxColumnWithEnumNames(mycmb, GetType(enmEquationComponent))
    End Sub

    Public Sub populateDataGridViewComboBoxWithTimeseriesFilterType(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmTimeSeriesFilter), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithControlledParameters(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmControlledParameter), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithSyntaxConditions(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmValidationSyntaxCondition), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithValidationCategories(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmValidationCategory), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithRectangularWeirInternalVariables(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmInternalVariable), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithObservedParameters(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmObservedParameter), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithControllerTypes(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertToUppercase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmControllerType), ConvertToUppercase)
    End Sub

    Public Sub populateDataGridViewComboBoxWithWeirEventControllerTypes(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ConvertTOUpperCase As Boolean)
        Call populateDataGridViewComboBoxColumnWithEnumNames(myCol, GetType(enmWeirEventsControllerCategory), ConvertTOUpperCase)
    End Sub
    Public Sub PopulateComboBoxWithEnumNames(ByRef myCmb As Windows.Forms.ComboBox, ByVal myEnumType As System.Type, Optional InitialSelectionFirstItem As Boolean = False)
        'this sub populates a combobox with all members of an enumerator
        'please note that the routine needs to be fed with the Type, which can be retrieved via:
        'GetType(enmMyEnumerator)
        Dim items As Array
        items = System.Enum.GetNames(myEnumType)
        For Each item As String In items
            myCmb.Items.Add(item.ToString.Trim)
        Next
        If InitialSelectionFirstItem Then
            myCmb.SelectedIndex = 0
        End If
    End Sub
    Public Sub PopulateComboBoxColumnWithEnumNames(ByRef myCmb As Windows.Forms.ComboBox, ByVal myEnumType As System.Type, Optional ByVal SelectedText As String = "")
        'this sub populates a datagridview's combobox column with all members of an enumerator
        'please note that the routine needs to be fed with the Type, which can be retrieved via:
        'GetType(enmMyEnumerator)
        Dim items As Array
        items = System.Enum.GetNames(myEnumType)
        For Each item As String In items
            myCmb.Items.Add(item.ToString.Trim)
        Next

        'preselect the required value, if assigned
        If Not SelectedText = "" Then
            myCmb.SelectedIndex = myCmb.FindStringExact(SelectedText)
        End If

    End Sub




    Public Sub populateCheckedListBoxWithEnumNames(ByRef myChk As Windows.Forms.CheckedListBox, ByVal myEnumType As System.Type)
        'this sub populates a checkedlistbox with all members of an enumerator
        'please note that the routine needs to be fed with the Type, which can be retrieved via:
        'GetType(enmMyEnumerator)
        Dim items As Array
        items = System.Enum.GetNames(myEnumType)
        Dim item As String
        For Each item In items
            myChk.Items.Add(item.ToString.Trim.ToUpper)
        Next
    End Sub

    Public Sub populateDataGridViewComboBoxColumnWithEnumNames(ByRef myCol As Windows.Forms.DataGridViewComboBoxColumn, ByVal myEnumType As System.Type, ConvertToUppercase As Boolean)
        Dim items As Array
        items = System.Enum.GetNames(myEnumType)
        Dim item As String
        For Each item In items
            If ConvertToUppercase Then
                myCol.Items.Add(item.ToString.Trim.ToUpper)
            Else
                myCol.Items.Add(item.ToString.Trim)
            End If
        Next
    End Sub


    Public Sub PopulateListBoxWithEnumNames(ByRef myLst As Windows.Forms.ListBox, ByVal myEnumType As System.Type)
        'this sub populates a combo box with all members of an enumerator
        'please note that the routine needs to be fed with the Type, which can be retrieved via:
        'GetType(enmMyEnumerator)
        Dim items As Array
        items = System.Enum.GetNames(myEnumType)
        Dim item As String
        For Each item In items
            myLst.Items.Add(item.ToString)
        Next
    End Sub

    Public Sub PopulateListBoxWithDatabaseEntries(ByRef myListBox As Windows.Forms.ListBox, ByVal TableID As String, ByVal FieldID As String)
        Dim dt As New DataTable, i As Long
        Dim query As String = "SELECT DISTINCT " & FieldID & " FROM " & TableID & ";"
        SQLiteQuery(setup.SqliteCon, query, dt)

        For i = 0 To dt.Rows.Count - 1
            myListBox.Items.Add(dt.Rows(i)(0))
        Next

    End Sub

    Public Function PopulateComboboxFromDatabaseQuery(ByRef con As SQLite.SQLiteConnection, ByVal query As String, ByRef myBox As Forms.ComboBox, Optional ByVal formatting As String = "", Optional ByVal SelectedText As String = "", Optional ByVal PreselectFirstItem As Boolean = False) As Boolean
        'note: if only unique values are required, use SELECT DISTINCT in the SQL-query
        Dim r As Long

        Try
            Dim dt As DataTable
            Dim da As SQLite.SQLiteDataAdapter

            If Not con.State = ConnectionState.Open Then con.Open()

            dt = New DataTable
            da = New SQLite.SQLiteDataAdapter(query, con)
            da.Fill(dt)

            myBox.Items.Clear()

            If formatting = "" Then
                For r = 0 To dt.Rows.Count - 1
                    myBox.Items.Add(dt.Rows(r)(0))
                Next
            Else
                For r = 0 To dt.Rows.Count - 1
                    myBox.Items.Add(Format(dt.Rows(r)(0), formatting).Trim)
                Next
            End If

            If Not SelectedText = "" Then
                myBox.SelectedIndex = myBox.FindStringExact(SelectedText)
            ElseIf PreselectFirstItem Then
                myBox.SelectedIndex = 0
            End If

            con.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function PopulateCheckedListBoxFromDatabaseQuery(ByRef con As SQLite.SQLiteConnection, ByVal query As String, ByRef myCheckedListBox As Forms.CheckedListBox, Optional ByVal formatting As String = "") As Boolean
        ' This function populates a CheckedListBox with data from a database query.
        Dim r As Long

        Try
            Dim dt As DataTable
            Dim da As SQLite.SQLiteDataAdapter

            ' Ensure the database connection is open
            If Not con.State = ConnectionState.Open Then con.Open()

            ' Create a DataTable to hold the query results
            dt = New DataTable
            da = New SQLite.SQLiteDataAdapter(query, con)
            da.Fill(dt)

            ' Clear existing items in the CheckedListBox
            myCheckedListBox.Items.Clear()

            ' Add items to the CheckedListBox based on query results
            If formatting = "" Then
                For r = 0 To dt.Rows.Count - 1
                    myCheckedListBox.Items.Add(dt.Rows(r)(0).ToString().Trim())
                Next
            Else
                For r = 0 To dt.Rows.Count - 1
                    myCheckedListBox.Items.Add(Format(dt.Rows(r)(0), formatting).Trim())
                Next
            End If

            ' Close the database connection
            con.Close()
            Return True
        Catch ex As Exception
            ' Handle any errors
            Return False
        End Try
    End Function


    Friend Function mergeStorageTablesByMaximum(ByRef Table1 As clsSobekTable, ByRef Table2 As clsSobekTable) As clsSobekTable

        'SIEBE: QUICK FIX FOR SCHELDESTROMEN. REMOVE AGAIN IMMEDIATELY!!!!!
        'Return Table1

        'Author: Siebe Bosch
        'Date: 4-10-2013
        'Description: merges two sobek tables by adding the maximum value from both to a new table
        Dim i As Integer
        Dim myList As New List(Of Double)
        Dim myDates As New List(Of Date)
        Dim newTable As New clsSobekTable(setup)
        Dim myX As Double, myY1 As Double, myY2 As Double

        Me.setup.GeneralFunctions.UpdateProgressBar("Merging storage tables by maximum...", 0, 10)

        'if one of both tables is nothing or empty, simply return the other
        If Table1 Is Nothing Then
            Return Table2
        ElseIf Table2 Is Nothing Then
            Return Table1
        ElseIf Table1.Values1.Count = 0 Then
            Return Table2
        ElseIf Table2.Values1.Count = 0 Then
            Return Table1

            'if the x-values from both tables are equal, it's simple
        ElseIf Table1.XValues.Count > 0 AndAlso Table1.XValues.Count = Table2.XValues.Count AndAlso
               Table1.XValues.Values(0) = Table2.XValues.Values(0) AndAlso
               Table1.XValues.Values(Table1.XValues.Count - 1) = Table2.XValues.Values(Table2.XValues.Count - 1) Then

            For i = 0 To Table1.XValues.Count - 1
                newTable.AddDataPair(2, Table1.XValues.Values(i), Math.Max(Table1.Values1.Values(i), Table2.Values1.Values(i)))
            Next

            'we'll have to interpolate
        ElseIf Table1.XValues.Values.Count > 0 Then
            'build a dictionary with all x-values
            For i = 0 To Table1.XValues.Values.Count - 1
                If Not myList.Contains(Table1.XValues.Values(i)) Then myList.Add(Table1.XValues.Values(i))
            Next
            For i = 0 To Table2.XValues.Values.Count - 1
                If Not myList.Contains(Table2.XValues.Values(i)) Then myList.Add(Table2.XValues.Values(i))
            Next

            'sort it
            myList.Sort()

            'maak nu de nieuwe tabel aan
            For i = 0 To myList.Count - 1

                Me.setup.GeneralFunctions.UpdateProgressBar("", (i + 1), myList.Count)
                myX = myList.Item(i)
                'if the x-value lies under the range of one of the two tables, simply return the value from the other
                'else: if the x-value lies above the range of one of the two table, simply return the maximum from both
                If myX < Table1.XValues.Values(0) Then
                    newTable.AddDataPair(2, myX, Table2.getValue1(myX))
                ElseIf myX < Table2.XValues.Values(0) Then
                    newTable.AddDataPair(2, myX, Table1.getValue1(myX))
                ElseIf myX > Table1.XValues.Values(Table1.XValues.Values.Count - 1) Then
                    newTable.AddDataPair(2, myX, Math.Max(Table2.getValue1(myX), Table1.Values1.Values(Table1.Values1.Count - 1)))
                ElseIf myX > Table2.XValues.Values(Table2.XValues.Values.Count - 1) Then
                    newTable.AddDataPair(2, myX, Math.Max(Table1.getValue1(myX), Table2.Values1.Values(Table2.Values1.Count - 1)))
                Else
                    myY1 = Table1.InterpolateFromXValues(myX, 1)
                    myY2 = Table2.InterpolateFromXValues(myX, 1)
                    newTable.AddDataPair(2, myX, Math.Max(myY1, myY2))
                End If
            Next
        End If

        Return newTable

    End Function

    Public Function StackStorageTables(ByRef BottomTable As clsSobekTable, ByRef TopTable As clsSobekTable) As clsSobekTable
        'Author: Siebe Bosch
        'Date: 4-10-2013
        'Description: stacks two sobek tables by taking one leading table and placing the second on top of the first
        'e.g. storage table openwater is leading, so storage table land is only applied on top of the range of the openwater table
        Dim i As Integer
        Dim newTable As New clsSobekTable(setup)
        Dim maxX As Double, maxOpp As Double

        'if one of both tables is nothing or empty, simply return the other
        If BottomTable Is Nothing Then
            Return TopTable
        ElseIf TopTable Is Nothing Then
            Return BottomTable
        ElseIf BottomTable.Values1.Count = 0 Then
            Return TopTable
        ElseIf TopTable.Values1.Count = 0 Then
            Return BottomTable
        Else
            maxX = BottomTable.XValues.Values(BottomTable.XValues.Values.Count - 1)
            maxOpp = BottomTable.Values1.Values(BottomTable.Values1.Values.Count - 1)

            For i = 0 To BottomTable.XValues.Count - 1
                newTable.AddDataPair(2, BottomTable.XValues.Values(i), BottomTable.Values1.Values(i))
            Next

            For i = 0 To TopTable.XValues.Count - 1
                If TopTable.XValues.Values(i) > maxX AndAlso TopTable.Values1.Values(i) > maxOpp Then
                    newTable.AddDataPair(2, TopTable.XValues.Values(i), TopTable.Values1.Values(i))
                End If
            Next

            Return newTable
        End If

    End Function


    Public Function ReadIniFileProperty(myLine As String) As String
        Dim HashPos As Integer = Strings.InStr(myLine, "#")
        If HashPos > 0 Then
            myLine = Strings.Left(myLine, HashPos - 1)
            myLine = myLine.Trim
        End If
        Dim IsPos As Integer = Strings.InStr(myLine, "=")
        Dim myValue As String = Strings.Right(myLine, myLine.Length - IsPos)
        myValue = myValue.Trim
        Return myValue
    End Function

    Friend Sub OffsetPoint(ByVal X As Double, ByVal Y As Double, ByVal myAngle As Double, ByVal myDist As Double, ByRef newX As Double, ByRef newY As Double, ByVal LeftSide As Boolean)
        If LeftSide = False Then myAngle = NormalizeAngle(myAngle + 180)
        myAngle = myAngle - 90 'de loodlijn
        myAngle = NormalizeAngle(myAngle)
        Dim Kwadrant As Integer
        If myAngle = 0 OrElse myAngle = 360 Then
            newX = X
            newY = Y + myDist
        ElseIf myAngle = 90 Then
            newX = X + myDist
            newY = Y
        ElseIf myAngle = 180 Then
            newX = X
            newY = Y - myDist
        ElseIf myAngle = 270 Then
            newX = X - myDist
            newY = Y
        Else
            If 0 < myAngle AndAlso myAngle < 90 Then
                Kwadrant = 1
                newX = X + Math.Sin(setup.GeneralFunctions.D2R(myAngle)) * myDist
                newY = Y + Math.Cos(setup.GeneralFunctions.D2R(myAngle)) * myDist
            ElseIf 90 < myAngle AndAlso myAngle < 180 Then
                Kwadrant = 2
                myAngle = myAngle - 90
                newX = X + Math.Cos(setup.GeneralFunctions.D2R(myAngle)) * myDist
                newY = Y - Math.Sin(setup.GeneralFunctions.D2R(myAngle)) * myDist
            ElseIf 180 < myAngle AndAlso myAngle <= 270 Then
                Kwadrant = 3
                myAngle = myAngle - 180
                newX = X - Math.Sin(setup.GeneralFunctions.D2R(myAngle)) * myDist
                newY = Y - Math.Cos(setup.GeneralFunctions.D2R(myAngle)) * myDist
            Else
                Kwadrant = 4
                myAngle = myAngle - 270
                newX = X - Math.Cos(setup.GeneralFunctions.D2R(myAngle)) * myDist
                newY = Y + Math.Sin(setup.GeneralFunctions.D2R(myAngle)) * myDist
            End If
        End If
    End Sub


    Friend Function GetMapWinGridCellCenterXY(ByVal XLLCenter As Double, ByVal YLLCenter As Double, ByVal DX As Double, ByVal DY As Double, ByVal nRows As Integer, ByVal nCols As Integer, ByVal rowIdx As Integer, ByVal colIdx As Integer, ByRef X As Double, ByRef Y As Double) As Boolean
        ' Siebe Bosch 8 July: adjusted
        ' MapWindow telt rijen van boven naar beneden (gechecked!)
        ' let op: gaat ervan uit dat de rowidx en colidx 0-based zijn
        X = XLLCenter + DX * colIdx
        Y = YLLCenter + ((nRows - 1) * DY) - (DY * rowIdx)
        Return True
    End Function

    Friend Shared Function ConvertoDate(ByVal dateString As String, ByRef result As DateTime) As Boolean
        Try
            Dim supportedFormats() As String = New String() {"MM/dd/yyyy", "MM/dd/yy", "ddMMMyyyy", "dMMMyyyy"}

            result = DateTime.ParseExact(dateString, supportedFormats, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None)

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Friend Function ConvertToDateTime(ByVal datestring As String, ByVal formats As String) As DateTime
        Dim Year As Integer, Month As Integer, Day As Integer, Hour As Integer, Minute As Integer, Second As Integer
        Dim dateObject As DateTime

        If formats = "yyyy/MM/dd;HH:mm:ss" Then
            Year = Left(datestring, 4)
            Month = Mid(datestring, 6, 2)
            Day = Mid(datestring, 9, 2)
            Hour = Mid(datestring, 12, 2)
            Minute = Mid(datestring, 15, 2)
            Second = Right(datestring, 2)
        End If
        dateObject = New DateTime(Year, Month, Day, Hour, Minute, Second)
        Return dateObject

    End Function

    Public Function ParseDouble(ByRef myString As String, Optional ByVal Delimiter As String = " ") As Double
        Dim Done As Boolean, NewString As String = "", newChar As String
        myString = myString.Trim()
        While Not Done
            newChar = Left(myString, 1)
            myString = Right(myString, myString.Length - 1)
            If newChar = Delimiter Then
                Done = True
            Else
                NewString &= newChar
            End If
        End While
        Return Convert.ToDouble(NewString)

    End Function

    Public Function RemoveDoubleCharactersFromString(ByRef myString As String, myCharacter As String, Optional ByVal QuoteHandlingFlag As Integer = 1) As Boolean
        Try
            'Quotehandlingflag: default = 1
            '0 = items between quotes are NOT being treated as separate items (parsing also between quotes)
            '1 = items between single quotes are being treated as separate items (no parsing between single quotes)
            '2 = items between double quotes are being treated as separate items (no parsing between double quotes)
            Dim i As Integer
            Dim newStr As String = ""
            Dim BetweenQuotes As Boolean = False
            Dim lastChar As String = "", curChar As String = ""
            For i = 0 To myString.Length - 1
                curChar = myString.Substring(i, 1)
                If curChar = "'" AndAlso QuoteHandlingFlag = 1 Then BetweenQuotes = Not BetweenQuotes
                If curChar = Chr(34) AndAlso QuoteHandlingFlag = 2 Then BetweenQuotes = Not BetweenQuotes
                If Not curChar = myCharacter Then
                    newStr &= curChar
                Else
                    If Not lastChar = myCharacter Then
                        newStr &= myCharacter
                    Else
                        If BetweenQuotes Then newStr &= myCharacter
                    End If
                End If
                lastChar = curChar
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function IncludebyRegEx(ID As String, IgnoreCase As Boolean, IncludeByID As Boolean, IncludeList As List(Of String), ExcludeByID As Boolean, ExcludeList As List(Of String)) As Boolean
        'this function allows for inclusion or exclusion of ID's by a given set of Regular Expressions lists
        Dim Include As Boolean

        'initialize this ID as not included
        IncludebyRegEx = False

        'now change that ordeal based on a potential text match with the includeList
        If Not IncludeByID Then
            Include = True
        ElseIf setup.GeneralFunctions.TextMatchFromExpressionsList(IncludeList, ID, IgnoreCase) Then
            Include = True
        End If

        'finally change that ordeal again, based on text match with the excludelist
        If ExcludeByID Then
            If setup.GeneralFunctions.TextMatchFromExpressionsList(ExcludeList, ID, IgnoreCase) Then
                Include = False
            End If
        End If

        Return Include

    End Function


    Public Function ParseList(myString As String, Optional ByVal Delimiter As String = " ", Optional ByVal QuoteHandlingFlag As Integer = 1, Optional ByVal ResultMustBeNumeric As Boolean = False) As List(Of String)
        Dim myList As New List(Of String)
        While Not myString = ""
            myList.Add(ParseString(myString, Delimiter, QuoteHandlingFlag, ResultMustBeNumeric))
        End While
        Return myList
    End Function


    Public Function ParseString(ByRef myString As String, Optional ByVal Delimiter As String = " ",
                                Optional ByVal QuoteHandlingFlag As Integer = 1, Optional ByVal ResultMustBeNumeric As Boolean = False) As String

        Dim quoteEven As Boolean
        Dim tmpString As String = "", tmpChar As String = ""

        myString = myString.Trim
        quoteEven = True

        'Quotehandlingflag: default = 1
        '0 = items between quotes are NOT being treated as separate items (parsing also between quotes)
        '1 = items between single quotes are being treated as separate items (no parsing between single quotes)
        '2 = items between double quotes are being treated as separate items (no parsing between double quotes)

        Dim i As Integer
        For i = 1 To Len(myString)
            'snoep een karakter af van de string
            tmpChar = Left(myString, 1)

            If (tmpChar = "'" And QuoteHandlingFlag = 1) Or (tmpChar = Chr(34) And QuoteHandlingFlag = 2) Then
                If quoteEven = True Then
                    quoteEven = False
                    tmpString = tmpString & tmpChar
                    myString = Right(myString, myString.Length - 1)
                Else
                    quoteEven = True 'dit betekent dat we klaar zijn
                    tmpString = Right(tmpString, tmpString.Length - 1) 'laat bij het teruggeven meteen de quotes maar weg!
                    myString = Right(myString, myString.Length - 1)
                End If
            ElseIf tmpChar = Delimiter And quoteEven = True Then
                myString = Right(myString, myString.Length - 1)
                Return tmpString

                'If Not tmpString = "" Then
                '    myString = Right(myString, myString.Length - 1)
                '    'Return tmpString
                '    Exit For
                'Else
                '    myString = Right(myString, myString.Length - 1)
                'End If
            ElseIf myString <> "" Then
                myString = Right(myString, myString.Length - 1)
                tmpString = tmpString & tmpChar
            End If
        Next

        If ResultMustBeNumeric AndAlso Not IsNumeric(tmpString) Then
            myString = tmpString & Delimiter & myString
            Me.setup.Log.AddWarning("Numeric value expected after token " & tmpString & " in string " & myString & ". Value of 0 was returned.")
            Return 0
        Else
            Return tmpString
        End If


    End Function

    Public Function ParseStringToDictionary(ByVal myString As String, ByVal Delimiter As String) As Dictionary(Of String, String)
        'siebe bosch, 22-2-2015
        'parses a string and returns the results in a list of strings
        Dim newDict As New Dictionary(Of String, String)
        Dim myID As String
        While Not myString = ""
            myID = ParseString(myString, Delimiter)
            If Not newDict.ContainsKey(myID.Trim.ToUpper) Then
                newDict.Add(myID.Trim.ToUpper, myID.Trim)
            End If
        End While
        Return newDict
    End Function

    Public Function ParseStringToList(ByVal myString As String, ByVal Delimiter As String) As List(Of String)
        'siebe bosch, 24-3-2015
        'parses a string and returns the results in a list of strings
        Dim newList As New List(Of String)
        Dim myID As String
        While Not myString = ""
            myID = ParseString(myString, Delimiter)
            newList.Add(myID)
        End While
        Return newList
    End Function

    Public Function SplitTimeSpan(ByVal TimeSpan As clsTimeSpan, ByVal divider As Long) As Dictionary(Of Long, clsTimeSpan)
        'splits a given timespan e.g. 12 to 200404 into (almost) equal parts
        Dim SegLength As Long = Me.setup.GeneralFunctions.RoundUD((TimeSpan.GetLastTS - TimeSpan.GetFirstTS + 1) / divider, 0, False)
        Dim myDict As New Dictionary(Of Long, clsTimeSpan), mySpan As clsTimeSpan
        Dim i As Long, tsstart As Long, tsend As Long

        For i = 1 To divider
            tsstart = TimeSpan.GetFirstTS + (i - 1) * SegLength
            If i = divider Then
                tsend = TimeSpan.GetLastTS
            Else
                tsend = Math.Min(TimeSpan.GetFirstTS + i * SegLength - 1, TimeSpan.GetLastTS)
            End If
            mySpan = New clsTimeSpan(tsstart, tsend)
            myDict.Add(i, mySpan)
        Next
        Return myDict
    End Function

    Public Function IsEven(ByVal myNum As Long) As Boolean
        If (myNum And 1) = 1 Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function Interpolate(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double,
                                ByVal X3 As Double, Optional ByVal BlockInterpolate As Boolean = False, Optional ByVal AllowExtrapolation As Boolean = False) As Double

        If X3 < X1 And X3 < X2 Then 'is niet interpoleren maar extrapoleren
            If AllowExtrapolation Then
                Return Extrapolate(X1, Y1, X2, Y2, X3)
            Else
                Return Y1
            End If
        ElseIf X3 > X2 And X3 > X1 Then 'is niet interpoleren maar extrapoleren
            If AllowExtrapolation Then
                Return Extrapolate(X1, Y1, X2, Y2, X3)
            Else
                Return Y2
            End If
        ElseIf X1 = X2 Then
            Return Y1
        Else
            If BlockInterpolate = True Then
                Return Y1
            Else
                Return Y1 + (Y2 - Y1) / (X2 - X1) * (X3 - X1)
            End If
        End If
    End Function

    Friend Function Extrapolate(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double, ByVal X3 As Double) As Double
        'extrapolates linearly

        Dim Rico As Double = 0
        If X3 > X2 Then
            Rico = (Y2 - Y1) / (X2 - X1)
            Extrapolate = Y2 + (X3 - X2) * Rico
        ElseIf X3 < X1 Then
            Rico = (Y2 - Y1) / (X2 - X1)
            Extrapolate = Y1 - (X1 - X3) * Rico
        Else
            Extrapolate = -999
        End If

    End Function



    Public Function RoundUD(ByVal numD As Double, ByVal nDecimals As Integer, ByVal Up As Boolean) As Decimal

        Try
            Dim r As Double = 10 ^ (nDecimals)
            numD = r * numD
            Dim RoundedDown As Long = Math.Round(numD, 0)
            If RoundedDown > numD Then RoundedDown -= 1
            Dim Diff As Double = numD - RoundedDown

            If Diff > 0 Then
                If Up Then
                    Return (RoundedDown + 1) / r
                Else
                    Return (RoundedDown) / r
                End If
            Else
                Return (RoundedDown) / r
            End If
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function RoundUD")
        End Try

    End Function

    Friend Function IDFROMSTRING(ByVal myStr As String, Optional ByVal Prefix As String = "", Optional ByVal CutoffString As String = "", Optional ByVal RemovePrefix As Boolean = True) As String
        Dim PrefixPos As Integer
        Dim tmpstr As String
        Dim ID As String = ""

        If Not CutoffString = "" And Not Prefix = "" Then
            'net zolang parsen tot we tegenkomen wat we nodig hebben
            While Not myStr = ""
                tmpstr = Me.ParseString(myStr, CutoffString)
                PrefixPos = InStr(1, tmpstr, Prefix, CompareMethod.Text)
                If PrefixPos > 0 Then
                    ID = Right(tmpstr, Len(tmpstr) - PrefixPos + 1)
                    If RemovePrefix Then
                        Return Right(ID, ID.Length - Prefix.Length)
                    Else
                        Return ID
                    End If

                End If
            End While
        Else
            Me.setup.Log.AddWarning("Functie zonder prefix of afbreekstring nog niet ondersteund.")
        End If

        Return String.Empty
    End Function

    Friend Function ParsestringNumeric(ByRef myString As String, Optional ByVal delimiter As String = " ") As Double
        Dim tmpString As String = "", tmpChar As String = ""
        Dim i As Long

        myString = myString.Trim
        For i = 1 To Len(myString)
            'snoep een karakter af van de string
            tmpChar = Mid(myString, i, 1)
            If tmpChar = delimiter Then
                If IsNumeric(tmpString) Then
                    myString = Right(myString, myString.Length - tmpString.Length - 1)
                    Return Val(tmpString)
                End If
            Else
                tmpString &= tmpChar
            End If
        Next i

        If IsNumeric(tmpString) Then
            Return Val(tmpString)
        Else
            Return -999
        End If

    End Function

    Public Function DateIntIsValid(ByVal dateint As Integer) As Boolean
        Dim myYear As Integer
        Dim myMonth As Integer
        Dim myDay As Integer

        myYear = Left(dateint, 4)
        myMonth = Left(Right(dateint, 4), 2)
        myDay = Right(dateint, 2)

        If myYear < 1900 OrElse myYear > 2100 Then
            Return False
        ElseIf myMonth > 12 OrElse myMonth < 1 Then
            Return False
        ElseIf myDay < 1 Or myDay > 31 Then
            Return False
        ElseIf myDay > 30 AndAlso (myMonth = 2 Or myMonth = 4 Or myMonth = 6 Or myMonth = 9 Or myMonth = 11) Then
            Return False
        ElseIf myMonth = 2 And myDay > 29 Then
            Return False
        ElseIf myMonth = 2 And myDay > 28 And Not IsLeapYear(myYear) Then
            Return False
        End If
        Return True
    End Function

    Public Function IsLeapYear(ByVal myYear As Integer) As Boolean
        If myYear / 4 = Math.Round(myYear / 4, 0) Then
            Return True
        Else
            Return False
        End If
    End Function


    Friend Shared Function ConvertoDateTime(ByVal dateString As String, ByRef result As DateTime,
                                            Optional ByRef format As String = "dd/MM/yyyy") As Boolean
        Dim Year As Integer, Month As Integer, Day As Integer, Hour As Integer, Minute As Integer, Second As Integer

        Try
            'try to find the year
            If InStr(format, "yyyy") > 0 Then
                Year = Mid(dateString, InStr(format, "yyyy"), 4)
            ElseIf InStr(format, "yy") > 0 Then
                Year = 2000 + Val(Mid(dateString, InStr(format, "yyyy"), 2))
            Else
                Throw New Exception("Error parsing string " & dateString & " to datetime using formatting " & format)
            End If

            'try to find the month
            If InStr(format, "MM", CompareMethod.Binary) > 0 Then
                Month = Mid(dateString, InStr(format, "MM"), 2)
            ElseIf InStr(format, "M") > 0 Then
                Month = Mid(dateString, InStr(format, "M"), 1)
            Else
                Throw New Exception("Error parsing string " & dateString & " to datetime using formatting " & format)
            End If

            'try to find the day
            If InStr(format, "dd", CompareMethod.Text) > 0 Then
                Day = Mid(dateString, InStr(format, "dd", CompareMethod.Text), 2)
            ElseIf InStr(format, "d", CompareMethod.Text) > 0 Then
                Day = Mid(dateString, InStr(format, "d", CompareMethod.Text), 1)
            Else
                Throw New Exception("Error parsing string " & dateString & " to datetime using formatting " & format)
            End If

            'try to find the hour
            If InStr(format, "hh", CompareMethod.Text) > 0 Then
                Hour = Mid(dateString, InStr(format, "hh", CompareMethod.Text), 2)
            ElseIf InStr(format, "h", CompareMethod.Text) > 0 Then
                Hour = Mid(dateString, InStr(format, "h", CompareMethod.Text), 1)
            Else
                Hour = 0
            End If

            'try to find the minute
            If InStr(format, "mm", CompareMethod.Binary) > 0 Then
                Minute = Mid(dateString, InStr(format, "mm"), 2)
            ElseIf InStr(format, "m") > 0 Then
                Minute = Mid(dateString, InStr(format, "m"), 1)
            Else
                Minute = 0
            End If

            'try to find the second
            If InStr(format, "ss") > 0 Then
                Second = Mid(dateString, InStr(format, "ss"), 2)
            ElseIf InStr(format, "s") > 0 Then
                Second = Mid(dateString, InStr(format, "s"), 1)
            Else
                Second = 0
            End If

            result = New DateTime(Year, Month, Day, Hour, Minute, Second)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function FileNameFromPath(ByVal path) As String
        Dim done As Boolean
        Dim mypos As Integer, prevpos As Integer
        mypos = 0
        prevpos = 0

        While Not done
            prevpos = mypos
            mypos = InStr(mypos + 1, path, "\")
            If mypos = 0 Then
                done = True
            End If
        End While

        Return Right(path, Len(path) - prevpos)

    End Function

    ' Paul Meems: deze functie komt twee keer voor in deze classe
    Private Function ParsestringDubbel(ByRef myString As String, Optional ByVal Delimiter As String = " ",
                                Optional ByVal QuoteHandlingFlag As Integer = 1) As String

        Dim quoteEven As Boolean
        Dim tmpString As String = "", tmpChar As String = ""

        quoteEven = True

        'Quotehandlingflag: default = 1
        '0 = items between quotes are NOT being treated as separate items (parsing also between quotes)
        '1 = items between single quotes are being treated as separate items (no parsing between single quotes)
        '2 = items between double quotes are being treated as separate items (no parsing between double quotes)

        Dim i As Integer
        For i = 1 To Len(myString)
            'snoep een karakter af van de string
            tmpChar = Left(myString, 1)

            If (tmpChar = "'" And QuoteHandlingFlag = 1) Or (tmpChar = Chr(34) And QuoteHandlingFlag = 2) Then
                If quoteEven = True Then
                    quoteEven = False
                    tmpString = tmpString & tmpChar
                    myString = Right(myString, myString.Length - 1)
                Else
                    quoteEven = True 'dit betekent dat we klaar zijn
                    tmpString = Right(tmpString, tmpString.Length - 1) 'laat bij het teruggeven meteen de quotes maar weg!
                    myString = Right(myString, myString.Length - 1).Trim
                    Return tmpString
                End If
            ElseIf tmpChar = Delimiter And quoteEven = True Then
                If Not tmpString = "" Then
                    myString = Right(myString, myString.Length - 1)
                    Return tmpString
                Else
                    myString = Right(myString, myString.Length - 1)
                End If
            Else
                myString = Right(myString, myString.Length - 1)
                tmpString = tmpString & tmpChar
            End If
        Next i

        Return tmpString

    End Function

    Public Function BNAString(ByVal ID As String, ByVal Name As String, ByVal X As Double, ByVal Y As Double) As String
        Return Chr(34) & ID & Chr(34) & "," & Chr(34) & Name & Chr(34) & ",1," & X & "," & Y
    End Function

    Public Function LpSpHa2M3pS(ByVal LpSpHa As Double, ByVal AreaM2 As Double) As Double
        Dim AreaHa As Double = AreaM2 / 10000
        Return LpSpHa * AreaHa / 1000
    End Function

    Public Function MMPD2M3PS(ByVal MMpD As Double, ByVal Opp As Double) As Double
        'converteert milimeters per dag naar kuubs per seconde
        Return (Opp * MMpD / 1000) / (24 * 3600)
    End Function

    Public Function mmph2m3ps(ByVal mm As Double, ByVal opp As Double) As Double
        'converteert milimeters per uur naar kuubs per seconde
        Return (opp * mm / 1000) / (3600)
    End Function

    Public Function m3ps2mmps(ByVal m3ps As Double, ByVal opp As Double) As Double
        'converteert m3/s naar mm/s
        If opp > 0 Then
            Return 1000 * m3ps / opp
        Else
            Return 0
        End If
    End Function

    Public Function m3ps2mmpd(ByVal m3ps As Double, ByVal opp As Double) As Double
        'converteert m3/s naar mm/d
        If opp > 0 Then
            Return 1000 * 3600 * 24 * m3ps / opp
        Else
            Return 0
        End If
    End Function

    Friend Function m3ps2mmph(ByVal m3ps As Double, ByVal opp As Double) As Double
        'converteert m3/s naar mm/h
        If opp > 0 Then
            Return 1000 * 3600 * m3ps / opp
        Else
            Return 0
        End If
    End Function

    Friend Function ParseTable(ByRef myString As String) As String
        Dim startPos As Integer, endPos As Integer
        startPos = InStr(myString, "TBLE", CompareMethod.Binary)
        endPos = InStr(myString, "tble", CompareMethod.Binary)
        Dim table As String = ""

        If startPos > 0 AndAlso endPos > 0 Then
            table = Mid(myString, startPos, endPos - startPos + 4)
            myString = Right(myString, myString.Length - endPos - 3)
        End If
        Return table

    End Function

    Friend Function AutoCorrectPath(ByRef myPath As String) As Boolean
        If Right(myPath, 1) = "\" Then myPath = Left(myPath, myPath.Length - 1)
        If Directory.Exists(myPath) Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function RemoveSurroundingQuotes(ByVal myString As String, SingleQuotes As Boolean, DoubleQuotes As Boolean) As String
        Dim tmpStr As String, Done As Boolean
        tmpStr = Trim(myString)

        While Not Done
            Done = True
            If SingleQuotes AndAlso Left(tmpStr, 1) = "'" Then
                tmpStr = Right(tmpStr, tmpStr.Length - 1)
                Done = False
            End If
            If SingleQuotes AndAlso Right(tmpStr, 1) = "'" Then
                tmpStr = Left(tmpStr, tmpStr.Length - 1)
                Done = False
            End If
            If DoubleQuotes AndAlso Left(tmpStr, 1) = Chr(34) Then
                tmpStr = Right(tmpStr, tmpStr.Length - 1)
                Done = False
            End If
            If DoubleQuotes AndAlso Right(tmpStr, 1) = Chr(34) Then
                tmpStr = Left(tmpStr, tmpStr.Length - 1)
                Done = False
            End If
        End While

        Return tmpStr
    End Function


    Friend Function RotatePoint(ByVal Xold As Double, ByVal Yold As Double, ByVal Xorigin As Double, ByVal Yorigin As Double,
                              ByVal Degrees As Double, ByRef Xnew As Double, ByRef Ynew As Double) As Boolean
        Dim r As Double, dy As Double, dx As Double
        'roteert een punt ten opzichte van zijn oorsprong

        dy = (Yold - Yorigin)
        dx = (Xold - Xorigin)
        r = Math.Sqrt(dx ^ 2 + dy ^ 2)

        'If dx = 0 Then dx = 0.00000000000001
        'theta = Math.Atan(dy / dx)


        Dim curangle As Double = LineAngleDegrees(Xorigin, Yorigin, Xold, Yold)
        Dim newangle As Double = curangle + Degrees

        dx = Math.Sin(D2R(newangle)) * r
        dy = Math.Cos(D2R(newangle)) * r

        Xnew = Xorigin + dx
        Ynew = Yorigin + dy


        ''Xnew = r * Math.Cos(theta - D2R(Degrees)) + Xorigin
        ''Ynew = r * Math.Sin(theta - D2R(Degrees)) + Yorigin

        'Xnew = r * Math.Sin(theta + D2R(Degrees)) + Xorigin
        'Ynew = r * Math.Cos(theta + D2R(Degrees)) + Yorigin

        Return True
    End Function

    Friend Function D2R(ByVal Angle As Double) As Double
        'graden naar radialen
        D2R = Angle / 180 * Math.PI
    End Function

    Friend Function R2D(ByVal Angle As Double) As Double
        'radialen naar graden
        R2D = Angle * 180 / Math.PI
    End Function

    Public Function LineAngleDegrees(ByVal X1 As Double, ByVal Y1 As Double, ByVal X2 As Double, ByVal Y2 As Double) As Double
        'berekent de hoek van een lijn tussen twee xy co-ordinaten
        Dim dX As Double, dY As Double

        dX = Math.Abs(X2 - X1)
        dY = Math.Abs(Y2 - Y1)

        If dX = 0 Then
            If dY = 0 Then
                Return 0
            ElseIf Y2 > Y1 Then
                Return 0
            ElseIf Y2 < Y1 Then
                Return 180
            End If
        ElseIf dY = 0 Then
            If X2 > X1 Then
                Return 90
            ElseIf X2 < X1 Then
                Return 270
            End If
        Else
            If X2 > X1 And Y2 > Y1 Then 'eerste kwadrant
                Return R2D(Math.Atan(dX / dY))
            ElseIf X2 > X1 And Y2 < Y1 Then 'tweede kwadrant
                Return 90 + R2D(Math.Atan(dY / dX))
            ElseIf X2 < X1 And Y2 < Y1 Then 'derde kwadrant
                Return 180 + R2D(Math.Atan(dX / dY))
            Else 'vierde kwadrant
                Return 270 + R2D(Math.Atan(dY / dX))
            End If
        End If

    End Function

    Friend Function AngleDifferenceDeg(ByVal Angle1 As Double, Angle2 As Double, Optional ByVal AllowReverse As Boolean = False) As Double
        Angle1 = NormalizeAngle(Angle1)
        Angle2 = NormalizeAngle(Angle2)
        Dim AngleDiff As Double = NormalizeAngle(Angle1 - Angle2) 'make sure the angle difference is anywhere between 0 and 360

        If AllowReverse Then
            'e.g. 180 = 0
            '170 = 10
            If AngleDiff >= 180 Then AngleDiff -= 180
            AngleDiff = Math.Min(AngleDiff, Math.Abs(180 - AngleDiff))
        Else
            'e.g. 350 = -10 so make it the absolute value of -10
            AngleDiff = Math.Min(AngleDiff, Math.Abs(360 - AngleDiff))
        End If
        Return AngleDiff

    End Function


    Friend Function NormalizeAngle(ByVal myAngle As Double) As Double

        'Me.setup.Log.AddDebugMessage("In NormalizeAngle. Angle: " & myAngle)

        If (Math.Abs(myAngle) > 360 * 10) Then
            Throw New ArgumentException("The input angle is much too big", "myAngle")
        End If

        While myAngle > 360
            myAngle -= 360
        End While

        While myAngle < 0
            myAngle += 360
        End While

        Return myAngle

    End Function

    Public Function QWEIR(ByVal Width As Double, ByVal DischCoef As Double, ByVal H1 As Double, ByVal H2 As Double, ByVal Z As Double, Optional ByVal LatContrCoef As Double = 1) As Double
        Dim Hup As Double, Hdown As Double, Multiplier As Double

        'Free flow: als h2 - z < 2/3 * (h1 -z)
        If H1 >= H2 Then
            Hup = H1
            Hdown = H2
            Multiplier = 1
        Else
            Hup = H2
            Hdown = H1
            Multiplier = -1
        End If

        If Hup <= Z Then
            Return 0
        ElseIf Hdown < Z Or (Hdown - Z) < 2 / 3 * (Hup - Z) Then
            'Free flow: Q = c * B * 2/3 * SQRT(2/3 * g) * (h1 - z)^1.5
            Return Multiplier * DischCoef * LatContrCoef * Width * 2 / 3 * Math.Sqrt(2 / 3 * 9.81) * (Hup - Z) ^ 1.5
        Else
            'Drowned flow: Q = c * B * (h2 -z) * SQRT(2 * g *(h1 - h2))
            Return Multiplier * DischCoef * LatContrCoef * Width * (Hdown - Z) * Math.Sqrt(2 * 9.81 * (Hup - Hdown))
        End If

    End Function


    Public Function QORIFICE(ByVal Z As Double, ByVal w As Double, ByVal gh As Double, ByVal mu As Double, ByVal cw As Double, ByVal H1 As Double, ByVal H2 As Double) As Double
        'Z = crest level
        'W = width
        'gh = gate height (openningshoogte)
        'mu = contraction coef (standaard 0.63)
        'cw = lateral contraction coef
        'h1 = waterstand bovenstrooms
        'h2 = waterstand benedenstrooms
        'ce = afvoercoefficient. standaard 1.5

        Dim Af As Double
        Dim ce As Double
        Dim g As Double
        Dim u As Double 'stroomsnelheid over de kruin. Moet eigenlijk iteratief worden bepaald maar ik zet hem even op 1
        u = 1
        ce = 1.5
        g = 9.81

        'bepaal of hij verdronken of vrij is
        If (H1 - Z) >= (3 / 2 * gh) Then   'orifice flow
            If H2 <= (Z + gh) Then 'free orifice flow
                Af = w * mu * gh
                Return cw * w * mu * gh * Math.Sqrt(2 * g * (H1 - (Z + mu * gh)))
            ElseIf H2 > (Z + gh) Then 'submerged orifice flow
                Af = w * mu * gh
                Return cw * w * mu * gh * Math.Sqrt(2 * g * (H1 - H2))
            End If
        ElseIf (H1 - Z) < (3 / 2 * gh) Then 'weir flow
            If (H1 - Z) > (3 / 2 * (H2 - Z)) Then 'free weir flow
                Af = w * 2 / 3 * (H1 - Z)
                Return cw * w * 2 / 3 * Math.Sqrt(2 / 3 * g * (H1 - Z) ^ 3 / 2)
            ElseIf (H1 - Z) <= (3 / 2 * (H2 - Z)) Then 'submerged weir flow
                Af = w * (H1 - Z - u ^ 2 / (2 * g))
                Return ce * cw * w * (H1 - Z - (u ^ 2 / (2 * g))) * Math.Sqrt(2 * g * (H1 - H2))
            End If
        Else
            MsgBox("Error: kon niet bepalen of orifice verdronken of vrij was.")
        End If


    End Function

    Public Function calcOrificeWidth(ByVal Q As Double, ByVal mu As Double, ByVal cw As Double, ByVal Z As Double, ByVal H1 As Double, ByVal gh As Double) As Double
        'calculates the desired dimensions for an orifice given a discharge and dH under free flow conditions
        'NOTE: assumes free orifice flow condition, thus (h1 - z) > 3/2*gh and h2 < (z + gh)
        'QORIFICE = cw * w * mu * gh * Math.Sqrt(2 * g * (H1 - (Z + mu * gh)))
        Dim w As Double, g As Double = 9.81

        w = Q / (cw * mu * gh * Math.Sqrt(2 * g * (H1 - (Z + mu * gh))))
        Return w

    End Function

    Public Function calcWeirWidth(ByVal Q As Double, ByVal ce As Double, ByVal sc As Double, ByVal overstorthoogte As Double) As Double
        'calculates the desired dimensions for a weir given a discharge and (h1 - z) under free flow conditions
        'QWEIR = ce * sc * w * 2/3 * Math.Sqrt(2/3 * g) * (h1 - z)^(3/2)
        'ce = discharge coef
        'cw = lateral contraction coef
        Dim w As Double, g As Double = 9.81

        w = Q / (ce * sc * (2 / 3) * Math.Sqrt(2 / 3 * g) * (overstorthoogte) ^ (3 / 2))
        Return w

    End Function


    Friend Sub SortCollectionOfObjects(ByRef col As Collection, ByVal psSortPropertyName As String,
                                       ByVal pbAscending As Boolean, Optional ByVal psKeyPropertyName As String = "")

        ' This is a cool function. I found this on Freevbcode.com. It was a VB6 function so I had to VB.NET-ify it. 
        ' Which was pretty simple. It sorts a collection by a property Ascending or Decending
        ' The Objects were originally declared as Variants. VB.Net has eliminated the Variant type so they must be declared as type Object. 
        ' Also Objects cannot be used with the Set keyword, so I had to remove the set keyword. Other than that I did not have to make any changes.

        Dim obj As Object, i As Integer, j As Integer
        Dim iMinMaxIndex As Integer, vMinMax As Object, vValue As Object
        Dim bSortCondition As Boolean, bUseKey As Boolean, sKey As String

        'als de propertyname leeg is, gebruiken we geen key
        bUseKey = (psKeyPropertyName <> "")

        'doorloop de collection
        For i = 1 To col.Count - 1
            obj = col(i)
            vMinMax = CallByName(obj, psSortPropertyName, vbGet)
            iMinMaxIndex = i

            'doorloop de collection vanaf i tot het eind nogmaals
            For j = i + 1 To col.Count
                obj = col(j)
                vValue = CallByName(obj, psSortPropertyName, vbGet)

                If (pbAscending) Then
                    bSortCondition = (vValue < vMinMax)
                Else
                    bSortCondition = (vValue > vMinMax)
                End If

                If (bSortCondition) Then
                    vMinMax = vValue
                    iMinMaxIndex = j
                End If

                obj = Nothing
            Next j

            If (iMinMaxIndex <> i) Then
                obj = col(iMinMaxIndex)

                col.Remove(iMinMaxIndex)
                If (bUseKey) Then
                    sKey = CStr(CallByName(obj, psKeyPropertyName, vbGet))
                    col.Add(obj, sKey, i)
                Else
                    col.Add(obj, , i)
                End If

                obj = Nothing
            End If

            obj = Nothing
        Next i

    End Sub

    Friend Sub SortCollectionOfDouble(ByRef col As Collection, ByVal psSortPropertyName As String,
                                      ByVal pbAscending As Boolean, Optional ByVal psKeyPropertyName As String = "")

        ' This is a cool function. I found this on Freevbcode.com. It was a VB6 function so I had to VB.NET-ify it. 
        ' Which was pretty simple. It sorts a collection by a property Ascending or Decending
        ' The Objects were originally declared as Variants. VB.Net has eliminated the Variant type so they must be declared as type Object. 
        ' Also Objects cannot be used with the Set keyword, so I had to remove the set keyword. Other than that I did not have to make any changes.

        Dim dbl As Double, i As Integer, j As Integer
        Dim iMinMaxIndex As Integer, vMinMax As Object, vValue As Object
        Dim bSortCondition As Boolean, bUseKey As Boolean

        'als de propertyname leeg is, gebruiken we geen key
        bUseKey = (psKeyPropertyName <> "")

        'doorloop de collection
        For i = 1 To col.Count - 1
            vMinMax = col(i)
            iMinMaxIndex = i

            'doorloop de collection vanaf i tot het eind nogmaals
            For j = i + 1 To col.Count
                vValue = col(j)

                If (pbAscending) Then
                    bSortCondition = (vValue < vMinMax)
                Else
                    bSortCondition = (vValue > vMinMax)
                End If

                If (bSortCondition) Then
                    vMinMax = vValue
                    iMinMaxIndex = j
                End If

            Next j

            If (iMinMaxIndex <> i) Then
                dbl = col(iMinMaxIndex)
                col.Remove(iMinMaxIndex)
                If (bUseKey) Then
                    col.Add(dbl, Nothing, i)
                Else
                    col.Add(dbl, Nothing, i)
                End If

            End If

        Next i

    End Sub

    Friend Function FormatI10(ByVal myVal As Long) As String
        Dim myStr As String = myVal.ToString.Trim
        Dim i As Long

        If myStr.Length > 10 Then
            Return Format(myVal, "0E00")
        Else
            For i = myStr.Length + 1 To 10
                myStr = " " & myStr
            Next
        End If
        Return myStr
    End Function

    Public Sub UpdateProgressBar(ByVal lblText As String, ByVal i As Long, ByVal n As Long, Optional ByVal ForceUpdate As Boolean = False, Optional ByVal ProgressBarNumber As Integer = 1)

        If ProgressBarNumber = 1 Then
            If Me.setup.progressBar IsNot Nothing AndAlso Me.setup.progressLabel IsNot Nothing Then
                If n = 0 Then n = 1 'veiligheidsmaatregel om exception te voorkomen
                If i > n Then i = n 'veiligheidsmaatregel om exception te voorkomen
                If i < 0 Then i = 0 'veiligheidsmaatregel om exception te voorkomen

                'als ie leeg is, laten we de string zoals ie is. Zo niet, dan vervangen we de string en forceren we een progressbar update
                If lblText <> String.Empty Then
                    Me.setup.progressLabel.Text = lblText
                    ForceUpdate = True
                End If

                'werk de progressbar bij
                If ForceUpdate OrElse Math.Round(i / n * 100, 0) >= (Me.setup.progressBar.Value + 1) Then
                    Me.setup.progressBar.Value = i / n * 100
                    'Call FlushMemory()
                    Forms.Application.DoEvents()
                End If
            End If
        Else
            If Not Me.setup.progressBar2 Is Nothing AndAlso Not Me.setup.progressLabel2 Is Nothing Then
                If n = 0 Then n = 1 'veiligheidsmaatregel om exception te voorkomen
                If i > n Then i = n 'veiligheidsmaatregel om exception te voorkomen
                If i < 0 Then i = 0 'veiligheidsmaatregel om exception te voorkomen

                'als ie leeg is, laten we de string zoals ie is. Zo niet, dan vervangen we de string en forceren we een progressbar update
                If lblText <> String.Empty Then
                    Me.setup.progressLabel2.Text = lblText
                    ForceUpdate = True
                End If

                'werk de progressbar bij
                If ForceUpdate OrElse Math.Round(i / n * 100, 0) >= (Me.setup.progressBar2.Value + 1) Then
                    Me.setup.progressBar2.Value = i / n * 100
                    Call FlushMemory()
                    Forms.Application.DoEvents()
                End If
            End If
        End If
    End Sub


    Public Sub UpdateCustomProgressBar(ByRef pr As Windows.Forms.ProgressBar, ByVal i As Long, ByVal n As Long, Optional ByVal ForceUpdate As Boolean = False)
        If Not Me.setup.progressBar Is Nothing AndAlso Not Me.setup.progressLabel Is Nothing Then
            If n = 0 Then n = 1 'veiligheidsmaatregel om exception te voorkomen
            If i > n Then i = n 'veiligheidsmaatregel om exception te voorkomen
            If i < 0 Then i = 0

            If ForceUpdate OrElse Math.Round(i / n * 100, 0) > (pr.Value + 1) Then
                pr.Value = i / n * 100
                Call FlushMemory()
                Forms.Application.DoEvents()
            End If
        End If

    End Sub

    Public Sub UpdateProgressBarConsole(ByVal i As Long, ByVal n As Long)
        Dim Cur As Integer = RoundUD(100 * (i / n), 0, True)
        Dim Nxt As Integer = RoundUD(100 * (i + 1) / n, 0, True)

        'we'll write 50 times # in total. Therefore only write if the next number is even
        If Cur <> Nxt AndAlso IsEven(Nxt) Then
            Console.Write(Chr(35))
        End If
    End Sub

    Public Sub FlushMemory()
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()
    End Sub

    Public Sub Wait(ByVal interval As Integer)
        'loops for a specified period of time (miliseconds)
        Dim sw As New Stopwatch
        sw.Start()
        Do While sw.ElapsedMilliseconds < interval
            'allow UI to remain responsive
            Forms.Application.DoEvents()
        Loop
        sw.Stop()
    End Sub

    Public Function Lgn2Sobek(ByVal LGNCODE As Integer, LGNVersion As Integer, ByRef SBKCode As Integer, ByRef Description As String) As Boolean
        Try
            Select Case LGNVersion
                Case Is = 5
                    SBKCode = Lgn5ToSobek(LGNCODE, Description)
                Case Is = 6
                    SBKCode = Lgn6ToSobek(LGNCODE, Description)
                Case Else
                    Throw New Exception("Error: LGN version not (yet) supported.")
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function Lgn2Sobek of class GeneralFunctions.")
            Return False
        End Try
    End Function

    Friend Function Lgn5ToSobek(ByVal LGNCODE As Integer, ByRef Description As String) As Integer

        '1 = grass
        '2 = corn
        '3 = potatoes
        '4 = sugarbeet
        '5 = grain
        '6 = miscellaneous
        '7 = non-arable land
        '8 = greenhouse area
        '9 = orchard
        '10 = bulbous plants
        '11 = foliage forest
        '12 = pine forest
        '13 = nature
        '14 = fallow
        '15 = vegetables
        '16 = flowers

        'zelf toegevoegd:
        '17 = water
        '18 = verhard
        '19 = glastuinbouw

        Select Case LGNCODE
            Case Is = 0 'bestaat eigenlijk niet, dus maak er maar gras van
                Return 1
                Description = "Onbekend"
            Case Is = 1 'gras
                Return 1
                Description = "Gras"
            Case Is = 2 'mais
                Return 2
                Description = "Mais"
            Case Is = 3 'aardappelen
                Return 3
                Description = "Aardappelen"
            Case Is = 4 'suikerbiet
                Return 4
                Description = "Suikerbiet"
            Case Is = 5 'graan
                Return 5
                Description = "Graan"
            Case Is = 6 'overige landbouwgewassen
                Return 6
                Description = "Overige landbouwgewassen"
            Case Is = 8 'glastuinbouw
                Return 19
                Description = "Glastuinbouw"
            Case Is = 9 'boomgaard
                Return 9
                Description = "Boomgaard"
            Case Is = 10 'bollenteelt
                Return 10
                Description = "Bollenteelt"
            Case Is = 11 'loofbos
                Return 11
                Description = "Loofbos"
            Case Is = 12 'naaldbos
                Return 12
                Description = "Naaldbos"
            Case Is = 16 'zoet water
                Return 17
                Description = "Zoet water"
            Case Is = 17 'zout water
                Return 17
                Description = "Zout water"
            Case Is = 18 'stedelijk bebouwd
                Return 18
                Description = "Stedelijk bebouwd"
            Case Is = 19 'bebouwd buitengebied
                Return 18
                Description = "Bebouwd buitengebied"
            Case Is = 20 'loofbos in bebouwd gebied
                Return 1
                Description = "loofbos in bebouwd gebied"
            Case Is = 21 'naaldbos in bebouwd gebied
                Return 1
                Description = "naaldbos in bebouwd gebied"
            Case Is = 22 'bos met dichte bebouwing
                Return 18
                Description = "bos met dichte bebouwing"
            Case Is = 23 'gras in bebouwd gebied
                Return 1
                Description = "gras in bebouwd gebied"
            Case Is = 24 'kale grond in bebouwd buitengebied
                Return 1
                Description = "kale grond in bebouwd buitengebied"
            Case Is = 25 'hoofdwegen en spoorwegen
                Return 18
                Description = "hoofdwegen en spoorwegen"
            Case Is = 26 'bebouwing in agrarisch gebied
                Return 18
                Description = "bebouwing in agrarisch gebied"
            Case Is = 30 'kwelders
                Return 13
                Description = "kwelders"
            Case Is = 35 'open stuifzand
                Return 13
                Description = "open stuifzand"
            Case Is = 36 'heide
                Return 13
                Description = "heide"
            Case Is = 37 'matig vergraste heide
                Return 13
                Description = "matig vergraste heide"
            Case Is = 38 'sterk vergraste heide
                Return 13
                Description = "sterk vergraste heide"
            Case Is = 39 'hoogveen
                Return 13
                Description = "hoogveen"
            Case Is = 40 'bos in hoogveen
                Return 13
                Description = "bos in hoogveen"
            Case Is = 41 'overige moerasvegetatie
                Return 13
                Description = "overige moerasvegetatie"
            Case Is = 42 'rietvegetatie
                Return 13
                Description = "rietvegetatie"
            Case Is = 43 'bos in moerasgebied
                Return 13
                Description = "bos in moerasgebied"
            Case Is = 45 'overig open begroeid natuurgebied
                Return 13
                Description = "overig open begroeid natuurgebied"
            Case Is = 46 'kale grond in natuurgebied
                Return 13
                Description = "kale grond in natuurgebied"
            Case Else
                Return 1
                Description = "Onbekend"
        End Select

    End Function

    Friend Function Landgebruik2019NSToSobek(ByVal LGCODE As Integer) As Integer
        'Landgebruikstypen in SOBEK:
        '1 = grass
        '2 = corn
        '3 = potatoes
        '4 = sugarbeet
        '5 = grain
        '6 = miscellaneous
        '7 = non-arable land
        '8 = greenhouse area
        '9 = orchard
        '10 = bulbous plants
        '11 = foliage forest
        '12 = pine forest
        '13 = nature
        '14 = fallow
        '15 = vegetables
        '16 = flowers

        'zelf toegevoegd:
        '17 = water
        '18 = verhard
        '19 = glastuinbouw


        Select Case LGCODE
            Case Is = 2 'woonfunctie
                Return 18
            Case Is = 3 'Celfunctie
                Return 18
            Case Is = 4 'Industriefunctie
                Return 18
            Case Is = 5 'Kantoorfunctie
                Return 18
            Case Is = 6 'Winkelfunctie
                Return 18
            Case Is = 7 'Kas
                Return 19
            Case Is = 8 'Logiesfunctie
                Return 18
            Case Is = 9 'Bijeenkomstfunctie
                Return 18
            Case Is = 10 'Sportfunctie
                Return 18
            Case Is = 11 'Onderwijsfunctie
                Return 18
            Case Is = 12 'Gezondheidszorgfunctie
                Return 18
            Case Is = 13 'Overige gebruiksfunctie
                Return 18
            Case Is = 14 'Overige gebruiksfunctie
                Return 18
            Case Is = 15 'Woongebied
                Return 18
            Case Is = 16 'Bedrijventerrein
                Return 18
            Case Is = 17 'Dagrecreatief terrein
                Return 18
            Case Is = 18 'Verblijfsrecreatief terrein
                Return 18
            Case Is = 19 'Sportterrein
                Return 18
            Case Is = 20 'Begraafplaats
                Return 1
            Case Is = 20 'Begraafplaats
                Return 1

            Case Else
                Return 1
        End Select

    End Function
    Friend Function Lgn6ToSobek(ByVal LGNCODE As Integer, ByRef Description As String) As Integer

        'legenda: http://www.wageningenur.nl/nl/Expertises-Dienstverlening/Onderzoeksinstituten/Alterra/Faciliteiten-Producten/Kaarten-en-GISbestanden/LGN-1/Bestanden/LGN6/LGN6-legenda.htm

        'Landgebruikstypen in SOBEK:
        '1 = grass
        '2 = corn
        '3 = potatoes
        '4 = sugarbeet
        '5 = grain
        '6 = miscellaneous
        '7 = non-arable land
        '8 = greenhouse area
        '9 = orchard
        '10 = bulbous plants
        '11 = foliage forest
        '12 = pine forest
        '13 = nature
        '14 = fallow
        '15 = vegetables
        '16 = flowers

        'zelf toegevoegd:
        '17 = water
        '18 = verhard
        '19 = glastuinbouw

        Select Case LGNCODE
            Case Is = 0 'bestaat eigenlijk niet, dus maak er maar gras van
                Description = "Onbekend"
                Return 1
            Case Is = 1 'agrarisch gras
                Description = "Agrarisch gras"
                Return 1
            Case Is = 2 'mais
                Description = "Mais"
                Return 2
            Case Is = 3 'aardappelen
                Description = "Aardappelen"
                Return 3
            Case Is = 4 'bieten
                Description = "Bieten"
                Return 4
            Case Is = 5 'graan
                Description = "Graan"
                Return 5
            Case Is = 6 'overige landbouwgewassen
                Description = "Overige landbouwgewassen"
                Return 6
            Case Is = 8 'glastuinbouw
                Description = "Glastuinbouw"
                Return 19
            Case Is = 9 'boomgaarden
                Description = "Boomgaarden"
                Return 9
            Case Is = 10 'bollenteelt
                Description = "Bollenteelt"
                Return 10
            Case Is = 11 'loofbos
                Description = "Loofbos"
                Return 11
            Case Is = 12 'naaldbos
                Description = "Naaldbos"
                Return 12
            Case Is = 16 'zoet water
                Description = "Zoet water"
                Return 17
            Case Is = 17 'zout water
                Description = "Zout water"
                Return 17
            Case Is = 18 'Bebouwing in primair bebouwd gebied
                Description = "Bebouwing in primair bebouwd gebied"
                Return 18
            Case Is = 19 'Bebouwing in secundair bebouwd gebied
                Description = "Bebouwing in secundair bebouwd gebied"
                Return 18
            Case Is = 20 'Bos in primair bebouwd gebied
                Description = "Bos in primair bebouwd gebied"
                Return 11
            Case Is = 22 'Bos in secundair bebouwd gebied
                Description = "Bos in secundair bebouwd gebied"
                Return 11
            Case Is = 23 'gras in primair bebouwd gebied
                Description = "Gras in primair bebouwd gebied"
                Return 1
            Case Is = 24 'kale grond in bebouwd buitengebied
                Description = "kale grond in bebouwd buitengebied"
                Return 7
            Case Is = 25 'hoofdwegen en spoorwegen
                Description = "hoofdwegen en spoorwegen"
                Return 18
            Case Is = 26 'Bebouwing in buitengebied
                Description = "Bebouwing in buitengebied"
                Return 18
            Case Is = 28 'Gras in secundair bebouwd gebied
                Description = "Gras in secundair bebouwd gebied"
                Return 1
            Case Is = 30 'kwelders
                Description = "Kwelders"
                Return 13
            Case Is = 31 'Open zand in kustgebied
                Description = "Open zand in kustgebied"
                Return 7
            Case Is = 32 'Duinen met een lage vegetatie
                Description = "Duinen met een lage vegetatie"
                Return 13
            Case Is = 32 'Duinen met een hoge vegetatie
                Description = "Duinen met een hoge vegetatie"
                Return 13
            Case Is = 32 'Duinheide
                Description = "Duinheide"
                Return 13
            Case Is = 35 'open stuifzand en/of rivierzand
                Description = "open stuifzand en/of rivierzand"
                Return 7
            Case Is = 36 'heide
                Description = "Heide"
                Return 13
            Case Is = 37 'matig vergraste heide
                Description = "matig vergraste heide"
                Return 13
            Case Is = 38 'sterk vergraste heide
                Description = "sterk vergraste heide"
                Return 13
            Case Is = 39 'hoogveen
                Description = "Hoogveen"
                Return 13
            Case Is = 40 'bos in hoogveengebied
                Description = "Bos in hoogveengebied"
                Return 13
            Case Is = 41 'overige moerasvegetatie
                Description = "overige moerasvegetatie"
                Return 13
            Case Is = 42 'rietvegetatie
                Description = "rietvegetatie"
                Return 13
            Case Is = 43 'bos in moerasgebied
                Description = "bos in moerasgebied"
                Return 13
            Case Is = 45 'Natuurgraslanden
                Description = "Natuurgraslanden"
                Return 13
            Case Is = 61 'Boomkwekerijen
                Description = "Boomkwekerijen"
                Return 9
            Case Is = 62 'Fruitkwekerijen
                Description = "Fruitkwekerijen"
                Return 9
            Case Else
                Description = "Onbekend"
                Return 1
        End Select

    End Function


    Public Function LGN6TONBW(ByVal LGNCODE As Integer) As Integer

        'legenda: http://www.wageningenur.nl/nl/Expertises-Dienstverlening/Onderzoeksinstituten/Alterra/Faciliteiten-Producten/Kaarten-en-GISbestanden/LGN-1/Bestanden/LGN6/LGN6-legenda.htm

        Select Case LGNCODE
            Case Is = 1 'agrarisch gras
                Return 3
            Case Is = 2, 3, 4, 5, 6 'mais, aardappelen, bieten, graan, overige landbouwgewassen
                Return 1
            Case Is = 8, 9, 10 'glastuinbouw, boomgaarden, bollenteelt
                Return 2
            Case Is = 11, 12 'loofbos, naaldbos
                Return 4
            Case Is = 16, 17 'zoet water, zout water
                Return 0
            Case Is = 18, 19 'Bebouwing in primair bebouwd gebied, bebouwing in secundair bebouwd gebied
                Return 5
            Case Is = 20, 22 'Bos in primair bebouwd gebied, bos in secundair bebouwd gebied
                Return 4
            Case Is = 23 'gras in primair bebouwd gebied
                Return 3
            Case Is = 24, 25, 26 'kale grond in bebouwd buitengebied, hoofdwegen en spoorwegen, bebouwing in buitengebied
                Return 5
            Case Is = 28 'Gras in secundair bebouwd gebied
                Return 3
            Case Is = 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 41, 42, 43, 44, 45 'kwelders, open zand in kustgebied, duinen met lage vegetatie, duinen met hoge vegetatie, duinheid, open stuifzand, heide, matig vergraste heide, sterk vergraste heide, hoogveen, bos in hoogveengebied, overige moerasvegetatie, rietvegetatie, bos in moerasgebied, natuurgraslanden
                Return 4
            Case Is = 61, 62 'boomkwekerijen en fruitkwekerijen
                Return 2
            Case Else
                Return 3
        End Select

    End Function


    Public Function Landgebruik2019NSBuisdrainagekaart(Landusecode As Integer, ByRef Description As String, ByRef LandUseClass As EnmLanduseTypeBuisdrainagekaart, ByRef Sbk As EnmSobekLanduseType) As Boolean
        Try
            Select Case Landusecode
                Case Is = 2 'Woonfunctie
                    Description = "Woonfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 3 'Celfunctie
                    Description = "Celfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 4 'Industriefunctie
                    Description = "Industriefunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 5 'Kantoorfunctie
                    Description = "Kantoorfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 6 'Winkelfunctie
                    Description = "Winkelfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 7 'Kas
                    Description = "Kas"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.glastuinbouw
                    Sbk = EnmSobekLanduseType.greenhouse
                Case Is = 8 'Logiesfunctie
                    Description = "Logiesfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 9 'Bijeenkomstfunctie
                    Description = "Bijeenkomstfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 10 'Sportfunctie
                    Description = "Sportfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.sportterrein
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 11 'Onderwijsfunctie
                    Description = "Onderwijsfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 12 'Gezondheidszorgfunctie
                    Description = "Gezondheidszorgfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 13 'Overige gebruiksfunctie
                    Description = "Overige gebruiksfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 14 'Overige gebruiksfunctie
                    Description = "Overige gebruiksfunctie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 15 'Woongebied
                    Description = "Woongebied"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 16 'Bedrijventerrein
                    Description = "Bedrijventerrein"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 17 'Dagrecreatief terrein
                    Description = "Dagrecreatief terrein"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.sportterrein
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 18 'Verblijfsrecreatief terrein
                    Description = "Verblijfsrecreatief terrein"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.sportterrein
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 19 'Sportterrein
                    Description = "Sportterrein"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.sportterrein
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 20 'Begraafplaats
                    Description = "Begraafplaats"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.begraafplaats
                    Sbk = EnmSobekLanduseType.fallow
                Case Is = 21 'Volkstuinen
                    Description = "Volkstuinen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 22 'Glastuinbouw
                    Description = "Glastuinbouw"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.glastuinbouw
                    Sbk = EnmSobekLanduseType.greenhouse
                Case Is = 24 'Overig
                    Description = "Overig"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 25 'Snelweg
                    Description = "Snelweg"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 26 'Regionale weg
                    Description = "Regionale weg"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 27 'Verkeerseiland
                    Description = "Verkeerseiland"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 28 'Lokale weg
                    Description = "Lokale weg"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 29 'Overige wegdelen
                    Description = "Overige wegdelen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 30 'Vliegveld
                    Description = "Vliegveld"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.vliegveld
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 31 'Spoor
                    Description = "Spoor"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.non_arable_land
                Case Is = 31 'Spoorbaan
                    Description = "Spoorbaan"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.non_arable_land
                Case Is = 32 'Transformatorstation
                    Description = "Transformatorstation"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 33 'Opslagtank
                    Description = "Opslagtank"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 34 'Bezinkbak
                    Description = "Bezinkbak"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 35 'Bassins
                    Description = "Bassins"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.water
                Case Is = 39 'Bos/Natuur
                    Description = "Bos/Natuur"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.foliage_forest
                Case Is = 40 'Groenvoorziening
                    Description = "Groenvoorziening"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 41 'Overig Gras/Groen
                    Description = "Overig Gras/Groen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 42 'Gras
                    Description = "Gras"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 43 'Bos/Natuur
                    Description = "Bos/Natuur"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.nature
                Case Is = 44 'Berm
                    Description = "Berm"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 45 'Spoorberm
                    Description = "Spoorberm"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 51 'Binnenwater
                    Description = "Binnenwater"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.water
                Case Is = 52 'Buitenwater
                    Description = "Buitenwater"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.water
                Case Is = 55 'Aardbeien, op stelling
                    Description = "Aardbeien, op stelling"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.miscellaneous
                Case Is = 56 'Aardbeien, open grond
                    Description = "Aardbeien, open grond"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.miscellaneous
                Case Is = 57 'Aardperen
                    Description = "Aardperen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.potatoes
                Case Is = 58 'Agrarisch gras en veevoeders
                    Description = "Agrarisch gras en veevoeders"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 59 'Akkerland
                    Description = "Akkerland"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.fallow
                Case Is = 60 'Andijvie
                    Description = "Andijvie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 61 'Appelen
                    Description = "Appelen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 62 'Asperges
                    Description = "Asperges"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 63 'Augurk
                    Description = "Augurk"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 64 'Bladrammenas
                    Description = "Bladrammenas"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 65 'Blauwebessen
                    Description = "Blauwebessen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 66 'Bloembollen
                    Description = "Bloembollen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.bulbous_plants
                Case Is = 67 'Bloembollen en sierteelt
                    Description = "Bloembollen en sierteelt"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.bulbous_plants
                Case Is = 68 'Bloemkool
                    Description = "Bloemkool"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 69 'Boerenkool
                    Description = "Boerenkool"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 70 'Boom en heesterkweek
                    Description = "Boom en heesterkweek"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 71 'Bos en haagplanten
                    Description = "Bos en haagplanten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boomgaard
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 72 'Bospeen
                    Description = "Bospeen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 73 'Braak
                    Description = "Braak"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.fallow
                Case Is = 74 'Broccoli
                    Description = "Broccoli"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 75 'Bruinebonen
                    Description = "Bruinebonen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 76 'Buxus
                    Description = "Buxus"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 77 'Chinesekool
                    Description = "Chinesekool"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 78 'Cichorei
                    Description = "Cichorei"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 79 'Consumptieaardappelen
                    Description = "Consumptieaardappelen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.potatoes
                Case Is = 80 'Courgette
                    Description = "Courgette"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 81 'Cranberry
                    Description = "Cranberry"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd 'nb cranberries worden op arme zandgrond in moerasachtige toestanden geteelt
                    Sbk = EnmSobekLanduseType.water
                Case Is = 82 'Engels raaigras
                    Description = "Engels raaigras"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 83 'Erwten
                    Description = "Erwten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 84 'Frambozen
                    Description = "Frambozen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 85 'Fruitteelt
                    Description = "Fruitteelt"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 86 'Gerst
                    Description = "Gerst"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grain
                Case Is = 87 'Granen
                    Description = "Granen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grain
                Case Is = 88 'Grasland, natuurlijk
                    Description = "Grasland, natuurlijk"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 89 'Graszoden
                    Description = "Graszoden"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 90 'Groente in open grond
                    Description = "Groente in open grond"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 91 'Haver
                    Description = "Haver"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grain
                Case Is = 92 'Hennep
                    Description = "Hennep"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 93 'IJsbergsla
                    Description = "IJsbergsla"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 94 'Kapucijners
                    Description = "Kapucijners"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 95 'Kersen
                    Description = "Kersen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 96 'Kersen, zuur
                    Description = "Kersen, zuur"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 97 'Kerstbomen
                    Description = "Kerstbomen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 98 'Klaver
                    Description = "Klaver"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.miscellaneous
                Case Is = 99 'Knoflook
                    Description = "Knoflook"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 100 'Knolselderij
                    Description = "Knolselderij"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 101 'Knolvenkel
                    Description = "Knolvenkel"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 102 'Komkommer
                    Description = "Komkommer"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 103 'Koolraap
                    Description = "Koolraap"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 104 'Koolrabi
                    Description = "Koolrabi"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 105 'Koolzaad
                    Description = "Koolzaad"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 106 'Kruiden
                    Description = "Kruiden"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 107 'Laanbomen
                    Description = "Laanbomen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 108 'Luzerne
                    Description = "Luzerne"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.miscellaneous
                Case Is = 109 'Mais, corncob
                    Description = "Mais, corncob"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.corn
                Case Is = 110 'Mais, energie
                    Description = "Mais, energie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.corn
                Case Is = 111 'Mais, korrel
                    Description = "Mais, korrel"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.corn
                Case Is = 112 'Mais, snij
                    Description = "Mais, snij"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.corn
                Case Is = 113 'Mais, suiker
                    Description = "Mais, suiker"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.corn
                Case Is = 114 'Miscanthus
                    Description = "Miscanthus"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.miscellaneous
                Case Is = 115 'Natuur
                    Description = "Natuur"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.nature
                Case Is = 116 'Notenbomen
                    Description = "Notenbomen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boomgaard
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 117 'Paksoi
                    Description = "Paksoi"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 118 'Pastinaak
                    Description = "Pastinaak"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 119 'Peren
                    Description = "Peren"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 120 'Peulen
                    Description = "Peulen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 121 'Pompoen
                    Description = "Pompoen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 122 'Pootaardappelen
                    Description = "Pootaardappelen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.potatoes
                Case Is = 123 'Prei
                    Description = "Prei"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 124 'Pronkbonen
                    Description = "Pronkbonen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 125 'Pruimen
                    Description = "Pruimen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 126 'Rabarber
                    Description = "Rabarber"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 127 'Radijs
                    Description = "Radijs"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 128 'Rietzwenkgras
                    Description = "Rietzwenkgras"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 129 'Rode Bieten
                    Description = "Rode Bieten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 130 'Rodebessen
                    Description = "Rodebessen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 131 'Rodekool
                    Description = "Rodekool"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 132 'Rogge
                    Description = "Rogge"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 133 'Rozen
                    Description = "Rozen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.flowers
                Case Is = 134 'Schorseneren
                    Description = "Schorseneren"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 135 'Selderij
                    Description = "Selderij"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 136 'Sierconiferen
                    Description = "Sierconiferen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 137 'Sierheesters en klimplanten
                    Description = "Sierheesters en klimplanten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 138 'Sla
                    Description = "Sla"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 139 'Sojabonen
                    Description = "Sojabonen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 140 'Sperziebonen
                    Description = "Sperziebonen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 141 'Spinazie
                    Description = "Spinazie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 142 'Spitskool
                    Description = "Spitskool"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 144 'Spruitjes
                    Description = "Spruitjes"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 145 'Suikerbieten
                    Description = "Suikerbieten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.sugarbeet
                Case Is = 146 'Tarwe
                    Description = "Tarwe"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grain
                Case Is = 147 'Trek en besheesters
                    Description = "Trek en besheesters"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 148 'Triticale
                    Description = "Triticale"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grain
                Case Is = 149 'Tuinbonen
                    Description = "Tuinbonen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 150 'Uien
                    Description = "Uien"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 151 'Valeriaan
                    Description = "Valeriaan"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd       'valeriaan groeit in natte slootkanten
                    Sbk = EnmSobekLanduseType.miscellaneous
                Case Is = 152 'Vaste Planten
                    Description = "Vaste Planten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.flowers
                Case Is = 153 'Vlas
                    Description = "Vlas"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 154 'Voederbieten
                    Description = "Voederbieten"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.sugarbeet
                Case Is = 155 'Waspeen
                    Description = "Waspeen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 156 'Water
                    Description = "Water"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.water
                Case Is = 157 'Weidehooi
                    Description = "Weidehooi"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.grass
                Case Is = 158 'Wijndruiven
                    Description = "Wijndruiven"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 159 'Winterpeen
                    Description = "Winterpeen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 160 'Witlof
                    Description = "Witlof"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 161 'Wittekool
                    Description = "Wittekool"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 162 'Wortelpeterselie
                    Description = "Wortelpeterselie"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.vegetables
                Case Is = 163 'Zetmeelaardappelen
                    Description = "Zetmeelaardappelen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                    Sbk = EnmSobekLanduseType.potatoes
                Case Is = 164 'Zwartebessen
                    Description = "Zwartebessen"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                    Sbk = EnmSobekLanduseType.orchard
                Case Is = 165 'Voetgangersgebied
                    Description = "Voetgangersgebied"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 166 'Fietspad
                    Description = "Fietspad"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                    Sbk = EnmSobekLanduseType.verhard
                Case Is = 253 'Onbekend
                    Description = "Onbekend"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.fallow
                Case Is = 254 'Top10 - Water
                    Description = "Top10 - Water"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                    Sbk = EnmSobekLanduseType.water
                Case Else
                    Description = "Onbekend"
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                    Sbk = EnmSobekLanduseType.grass
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in Function LGN62LanduseBuisdrainagekaart.")
            Return False
        End Try

    End Function

    Public Function LGN62LanduseBuisdrainagekaart(lgcode As Integer, ByRef Description As String, ByRef LandUseClass As EnmLanduseTypeBuisdrainagekaart, ByRef SobekLanduseCode As Integer) As Boolean
        Try
            'first retrieve the SOBEK LanduseCode
            SobekLanduseCode = Lgn6ToSobek(lgcode, Description)

            'the find the corresponding landuse type conform buisdrainagekaart
            Select Case lgcode
                Case Is = 1
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                Case Is = 2
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                Case Is = 3
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                Case Is = 4
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                Case Is = 5
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                Case Is = 6
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 7
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 8
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.glastuinbouw
                Case Is = 9
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boomgaard
                Case Is = 10
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                Case Is = 11
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 12
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 13
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 14
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 15
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.akkerbouw_tuinbouw
                Case Is = 16
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 17
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 18
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                Case Is = 19
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                Case Is = 20
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 22
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 23
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                Case Is = 24
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 25
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 26
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.stedelijk
                Case Is = 28
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
                Case Is = 30
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 30
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 31
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 32
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 35
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 36
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 37
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 37
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 39
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 40
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 41
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 41
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 42
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 43
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 45
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.ongedraineerd
                Case Is = 61
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                Case Is = 62
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.boom_en_fruitkwekerij
                Case Else
                    LandUseClass = EnmLanduseTypeBuisdrainagekaart.grasland
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in Function LGN62LanduseBuisdrainagekaart.")
            Return False
        End Try

    End Function

    Public Function ReplaceStringInFile(Path As String, OriginalString As String, ReplacementString As String) As Boolean
        Try
            Dim myContent As String

            Using myReader As New StreamReader(Path)
                myContent = myReader.ReadToEnd
            End Using

            myContent = myContent.Replace(OriginalString, ReplacementString)

            Using myWriter As New StreamWriter(Path)
                myWriter.Write(myContent)
            End Using

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error Integer Function ReplaceStringInFile: " & ex.Message)
            Return False
        End Try
    End Function
    Public Function Bod2KleiZandVeen(ByVal EERSTE_BOD As String, ByRef SoilClass As enmSoilTypeBuisdrainagekaart) As Boolean
        Try
            Dim CapSimCode As Long
            CapSimCode = Bod2Capsim(EERSTE_BOD)
            Select Case CapSimCode
                Case Is = 101
                    SoilClass = enmSoilTypeBuisdrainagekaart.veen
                Case Is = 102
                    SoilClass = enmSoilTypeBuisdrainagekaart.veen
                Case Is = 103
                    SoilClass = enmSoilTypeBuisdrainagekaart.veen
                Case Is = 104
                    SoilClass = enmSoilTypeBuisdrainagekaart.veen
                Case Is = 105
                    SoilClass = enmSoilTypeBuisdrainagekaart.veen
                Case Is = 106
                    SoilClass = enmSoilTypeBuisdrainagekaart.veen
                Case Is = 107
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 108
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 109
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 110
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 111
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 112
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 113
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 114
                    SoilClass = enmSoilTypeBuisdrainagekaart.zand
                Case Is = 115
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
                Case Is = 116
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
                Case Is = 117
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
                Case Is = 118
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
                Case Is = 119
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
                Case Is = 120
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
                Case Is = 121
                    SoilClass = enmSoilTypeBuisdrainagekaart.klei
            End Select

            Return True

        Catch ex As Exception
            Me.setup.Log.AddError("Could not classify soil type " & EERSTE_BOD & " to klei, zand or veen.")
            Return False
        End Try

    End Function

    Public Function Bod2Capsim(ByVal EERSTE_BOD As String) As Integer

        'converteert bodemtypes uit de Bodemkaart Nederland naar het corresponderende CAPSIM bodemnummer in SOBEK
        'Veengronden: code V
        'Moerige gronden: code W
        'Podzolgronden: code H en Y
        'BrikGronden: code B
        'Dikke eerdgronden: code EZ EL en EK
        'Kalkloze zandgronden: code Z
        'Kalkhoudende zandgronden: code Z...A
        'Kalkhoudende bijzonder lutumarme gronden: code S...A
        'Niet gerijpte minerale gronden: code MO-zeeklei, RO-rivierklei
        'Zeekleigronden: code M
        'Rivierkleigronden: code R
        'Oude rivierkleigronden: code KR
        'Leemgronden: code L
        'Mariene afzettingen ouder dan Pleistoceen: code MA, MK, MZ
        'Fluviatiele afzttingen ouder dan Pleistoceen: code FG, FK
        'Kalksteenverweringsgronden: code KM, KK, KS
        'Ondiepe keileemgronden: code KX
        'Overige oude kleigronden: code KT
        'Grindgronden: code G
        EERSTE_BOD = setup.GeneralFunctions.ParseString(EERSTE_BOD, "/")
        If EERSTE_BOD = "" Then Return 101
        If EERSTE_BOD = "|a GROEVE" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "|b AFGRAV" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "|c OPHOOG" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "|d EGAL" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "|e VERWERK" Then Return 113 'willekeurig bedenksel
        If EERSTE_BOD = "|f TERP" Then Return 116 'lichte klei, klopt wel enigszins met omgeving
        If EERSTE_BOD = "|g MOERAS" Then Return 101 'veengrond ligt voor de hand
        If EERSTE_BOD = "|g WATER" Then Return 101 'tsja
        If EERSTE_BOD = "|h BEBOUW" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "|h DIJK" Then Return 119 'klei op zand aangenomen
        If EERSTE_BOD = "|i BOVLAND" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "|j MYNSTRT" Then Return 108 'podzol=leemarm grof zand
        If EERSTE_BOD = "AAKp" Then Return 119
        If EERSTE_BOD = "AAP" Then Return 105
        If EERSTE_BOD = "ABk" Then Return 119
        If EERSTE_BOD = "ABkt" Then Return 119
        If EERSTE_BOD = "ABl" Then Return 121
        If EERSTE_BOD = "ABv" Then Return 105
        If EERSTE_BOD = "ABvg" Then Return 105
        If EERSTE_BOD = "ABvt" Then Return 105
        If EERSTE_BOD = "ABvx" Then Return 105
        If EERSTE_BOD = "ABz" Then Return 113
        If EERSTE_BOD = "ABzt" Then Return 113
        If EERSTE_BOD = "AEk9" Then Return 116
        If EERSTE_BOD = "AEm5" Then Return 115
        If EERSTE_BOD = "AEm8" Then Return 116
        If EERSTE_BOD = "AEm9A" Then Return 116
        If EERSTE_BOD = "AEp6A" Then Return 116
        If EERSTE_BOD = "AEp7A" Then Return 116
        If EERSTE_BOD = "AFz" Then Return 113
        If EERSTE_BOD = "Aha" Then Return 121
        If EERSTE_BOD = "AHc" Then Return 121
        If EERSTE_BOD = "AHk" Then Return 121
        If EERSTE_BOD = "AHl" Then Return 121
        If EERSTE_BOD = "Ahs" Then Return 121
        If EERSTE_BOD = "AHt" Then Return 121
        If EERSTE_BOD = "AHv" Then Return 121
        If EERSTE_BOD = "AHz" Then Return 121
        If EERSTE_BOD = "AK" Then Return 119
        If EERSTE_BOD = "AKp" Then Return 119
        If EERSTE_BOD = "ALu" Then Return 116
        If EERSTE_BOD = "AM" Then Return 119
        If EERSTE_BOD = "AMm" Then Return 115
        If EERSTE_BOD = "AO" Then Return 119
        If EERSTE_BOD = "AOg" Then Return 119
        If EERSTE_BOD = "AOp" Then Return 119
        If EERSTE_BOD = "AOv" Then Return 119
        If EERSTE_BOD = "AP" Then Return 101
        If EERSTE_BOD = "App" Then Return 102
        If EERSTE_BOD = "AQ" Then Return 107
        If EERSTE_BOD = "AR" Then Return 119
        If EERSTE_BOD = "AS" Then Return 107
        If EERSTE_BOD = "aVc" Then Return 101
        If EERSTE_BOD = "AVk" Then Return 105
        If EERSTE_BOD = "AVo" Then Return 101
        If EERSTE_BOD = "aVp" Then Return 102
        If EERSTE_BOD = "aVpg" Then Return 102
        If EERSTE_BOD = "aVpx" Then Return 102
        If EERSTE_BOD = "aVs" Then Return 101
        If EERSTE_BOD = "aVz" Then Return 102
        If EERSTE_BOD = "aVzt" Then Return 102
        If EERSTE_BOD = "aVzx" Then Return 102
        If EERSTE_BOD = "AWg" Then Return 116
        If EERSTE_BOD = "AWo" Then Return 106
        If EERSTE_BOD = "AWv" Then Return 106
        If EERSTE_BOD = "AZ1" Then Return 114
        If EERSTE_BOD = "AZW0A" Then Return 107
        If EERSTE_BOD = "AZW0Al" Then Return 107
        If EERSTE_BOD = "AZW0Av" Then Return 107
        If EERSTE_BOD = "AZW1A" Then Return 119
        If EERSTE_BOD = "AZW1Ar" Then Return 119
        If EERSTE_BOD = "AZW1Aw" Then Return 119
        If EERSTE_BOD = "AZW5A" Then Return 119
        If EERSTE_BOD = "AZW6A" Then Return 119
        If EERSTE_BOD = "AZW6Al" Then Return 116
        If EERSTE_BOD = "AZW6Alv" Then Return 118
        If EERSTE_BOD = "AZW7Al" Then Return 116
        If EERSTE_BOD = "AZW7Alw" Then Return 116
        If EERSTE_BOD = "AZW7Alwp" Then Return 119
        If EERSTE_BOD = "AZW8A" Then Return 116
        If EERSTE_BOD = "AZW8Al" Then Return 116
        If EERSTE_BOD = "AZW8Alw" Then Return 116
        If EERSTE_BOD = "bEZ21" Then Return 112
        If EERSTE_BOD = "bEZ21g" Then Return 112
        If EERSTE_BOD = "bEZ21x" Then Return 112
        If EERSTE_BOD = "bEZ23" Then Return 112
        If EERSTE_BOD = "bEZ23g" Then Return 112
        If EERSTE_BOD = "bEZ23t" Then Return 112
        If EERSTE_BOD = "bEZ23x" Then Return 112
        If EERSTE_BOD = "bEZ30" Then Return 112
        If EERSTE_BOD = "bEZ30x" Then Return 112
        If EERSTE_BOD = "bgMn15C" Then Return 115
        If EERSTE_BOD = "bgMn25C" Then Return 115
        If EERSTE_BOD = "bgMn53C" Then Return 117
        If EERSTE_BOD = "BKd25" Then Return 115
        If EERSTE_BOD = "BKd25x" Then Return 115
        If EERSTE_BOD = "BKd26" Then Return 115
        If EERSTE_BOD = "BKh25" Then Return 115
        If EERSTE_BOD = "BKh25x" Then Return 115
        If EERSTE_BOD = "BKh26" Then Return 115
        If EERSTE_BOD = "BKh26x" Then Return 115
        If EERSTE_BOD = "BLb6" Then Return 121
        If EERSTE_BOD = "BLb6g" Then Return 121
        If EERSTE_BOD = "BLb6k" Then Return 121
        If EERSTE_BOD = "BLb6s" Then Return 121
        If EERSTE_BOD = "BLd5" Then Return 121
        If EERSTE_BOD = "BLd5g" Then Return 121
        If EERSTE_BOD = "BLd5t" Then Return 121
        If EERSTE_BOD = "BLd6" Then Return 121
        If EERSTE_BOD = "BLd6m" Then Return 121
        If EERSTE_BOD = "BLh5m" Then Return 121
        If EERSTE_BOD = "BLh6" Then Return 121
        If EERSTE_BOD = "BLh6g" Then Return 121
        If EERSTE_BOD = "BLh6m" Then Return 121
        If EERSTE_BOD = "BLh6s" Then Return 121
        If EERSTE_BOD = "BLn5m" Then Return 121
        If EERSTE_BOD = "BLn5t" Then Return 121
        If EERSTE_BOD = "BLn6" Then Return 121
        If EERSTE_BOD = "BLn6g" Then Return 121
        If EERSTE_BOD = "BLn6m" Then Return 121
        If EERSTE_BOD = "BLn6s" Then Return 121
        If EERSTE_BOD = "bMn15A" Then Return 115
        If EERSTE_BOD = "bMn15C" Then Return 115
        If EERSTE_BOD = "bMn25A" Then Return 115
        If EERSTE_BOD = "bMn25C" Then Return 115
        If EERSTE_BOD = "bMn35A" Then Return 116
        If EERSTE_BOD = "bMn45A" Then Return 117
        If EERSTE_BOD = "bMn56Cp" Then Return 119
        If EERSTE_BOD = "bMn85C" Then Return 116
        If EERSTE_BOD = "bMn86C" Then Return 117
        If EERSTE_BOD = "bRn46C" Then Return 117
        If EERSTE_BOD = "BZd23" Then Return 113
        If EERSTE_BOD = "BZd24" Then Return 113
        If EERSTE_BOD = "cHd21" Then Return 108
        If EERSTE_BOD = "cHd21g" Then Return 110
        If EERSTE_BOD = "cHd21x" Then Return 111
        If EERSTE_BOD = "cHd23" Then Return 113
        If EERSTE_BOD = "cHd23x" Then Return 111
        If EERSTE_BOD = "cHd30" Then Return 114
        If EERSTE_BOD = "cHn21" Then Return 109
        If EERSTE_BOD = "cHn21g" Then Return 110
        If EERSTE_BOD = "cHn21t" Then Return 111
        If EERSTE_BOD = "cHn21w" Then Return 111
        If EERSTE_BOD = "cHn21x" Then Return 111
        If EERSTE_BOD = "cHn23" Then Return 113
        If EERSTE_BOD = "cHn23g" Then Return 110
        If EERSTE_BOD = "cHn23t" Then Return 111
        If EERSTE_BOD = "cHn23wx" Then Return 111
        If EERSTE_BOD = "cHn23x" Then Return 111
        If EERSTE_BOD = "cHn30" Then Return 114
        If EERSTE_BOD = "cHn30g" Then Return 114
        If EERSTE_BOD = "cY21" Then Return 109
        If EERSTE_BOD = "cY21g" Then Return 110
        If EERSTE_BOD = "cY21x" Then Return 111
        If EERSTE_BOD = "cY23" Then Return 113
        If EERSTE_BOD = "cY23g" Then Return 113
        If EERSTE_BOD = "cY23x" Then Return 111
        If EERSTE_BOD = "cY30" Then Return 114
        If EERSTE_BOD = "cY30g" Then Return 114
        If EERSTE_BOD = "cZd21" Then Return 108
        If EERSTE_BOD = "cZd21g" Then Return 110
        If EERSTE_BOD = "cZd23" Then Return 113
        If EERSTE_BOD = "cZd30" Then Return 114
        If EERSTE_BOD = "dgMn58Cv" Then Return 117
        If EERSTE_BOD = "dgMn83C" Then Return 117
        If EERSTE_BOD = "dgMn88Cv" Then Return 117
        If EERSTE_BOD = "dhVb" Then Return 101
        If EERSTE_BOD = "dhVk" Then Return 106
        If EERSTE_BOD = "dhVr" Then Return 101
        If EERSTE_BOD = "dkVc" Then Return 103
        If EERSTE_BOD = "dMn86C" Then Return 117
        If EERSTE_BOD = "dMv41C" Then Return 118
        If EERSTE_BOD = "dMv61C" Then Return 118
        If EERSTE_BOD = "dpVc" Then Return 103
        If EERSTE_BOD = "dVc" Then Return 101
        If EERSTE_BOD = "dVd" Then Return 101
        If EERSTE_BOD = "dVk" Then Return 106
        If EERSTE_BOD = "dVr" Then Return 101
        If EERSTE_BOD = "dWo" Then Return 106
        If EERSTE_BOD = "dWol" Then Return 106
        If EERSTE_BOD = "EK19" Then Return 115
        If EERSTE_BOD = "EK19p" Then Return 119
        If EERSTE_BOD = "EK19x" Then Return 115
        If EERSTE_BOD = "EK76" Then Return 117
        If EERSTE_BOD = "EK79" Then Return 116
        If EERSTE_BOD = "EK79v" Then Return 116
        If EERSTE_BOD = "EK79w" Then Return 116
        If EERSTE_BOD = "EL5" Then Return 121
        If EERSTE_BOD = "eMn12Ap" Then Return 119
        If EERSTE_BOD = "eMn15A" Then Return 115
        If EERSTE_BOD = "eMn15Ap" Then Return 119
        If EERSTE_BOD = "eMn22A" Then Return 119
        If EERSTE_BOD = "eMn22Ap" Then Return 119
        If EERSTE_BOD = "eMn25A" Then Return 115
        If EERSTE_BOD = "eMn25Ap" Then Return 119
        If EERSTE_BOD = "eMn25Av" Then Return 118
        If EERSTE_BOD = "eMn35A" Then Return 116
        If EERSTE_BOD = "eMn35Ap" Then Return 119
        If EERSTE_BOD = "eMn35Av" Then Return 118
        If EERSTE_BOD = "eMn35Awp" Then Return 119
        If EERSTE_BOD = "eMn45A" Then Return 117
        If EERSTE_BOD = "eMn45Ap" Then Return 117
        If EERSTE_BOD = "eMn45Av" Then Return 118
        If EERSTE_BOD = "eMn52Cg" Then Return 119
        If EERSTE_BOD = "eMn52Cp" Then Return 119
        If EERSTE_BOD = "eMn52Cwp" Then Return 119
        If EERSTE_BOD = "eMn56Av" Then Return 118
        If EERSTE_BOD = "eMn82A" Then Return 119
        If EERSTE_BOD = "eMn82Ap" Then Return 119
        If EERSTE_BOD = "eMn82C" Then Return 119
        If EERSTE_BOD = "eMn82Cp" Then Return 119
        If EERSTE_BOD = "eMn86A" Then Return 117
        If EERSTE_BOD = "eMn86Av" Then Return 118
        If EERSTE_BOD = "eMn86C" Then Return 117
        If EERSTE_BOD = "eMn86Cv" Then Return 118
        If EERSTE_BOD = "eMn86Cw" Then Return 117
        If EERSTE_BOD = "eMo20A" Then Return 119
        If EERSTE_BOD = "eMo20Ap" Then Return 119
        If EERSTE_BOD = "eMo80A" Then Return 116
        If EERSTE_BOD = "eMo80Ap" Then Return 119
        If EERSTE_BOD = "eMo80C" Then Return 116
        If EERSTE_BOD = "eMo80Cv" Then Return 118
        If EERSTE_BOD = "eMOb72" Then Return 119
        If EERSTE_BOD = "eMOb75" Then Return 116
        If EERSTE_BOD = "eMOo05" Then Return 115
        If EERSTE_BOD = "eMv41C" Then Return 118
        If EERSTE_BOD = "eMv51A" Then Return 118
        If EERSTE_BOD = "eMv61C" Then Return 118
        If EERSTE_BOD = "eMv61Cp" Then Return 118
        If EERSTE_BOD = "eMv81A" Then Return 118
        If EERSTE_BOD = "eMv81Ap" Then Return 118
        If EERSTE_BOD = "epMn55A" Then Return 115
        If EERSTE_BOD = "epMn85A" Then Return 116
        If EERSTE_BOD = "epMo50" Then Return 115
        If EERSTE_BOD = "epMo80" Then Return 116
        If EERSTE_BOD = "epMv81" Then Return 118
        If EERSTE_BOD = "epRn56" Then Return 117
        If EERSTE_BOD = "epRn59" Then Return 119
        If EERSTE_BOD = "epRn86" Then Return 117
        If EERSTE_BOD = "eRn45A" Then Return 117
        If EERSTE_BOD = "eRn46A" Then Return 117
        If EERSTE_BOD = "eRn46Av" Then Return 118
        If EERSTE_BOD = "eRn47C" Then Return 117
        If EERSTE_BOD = "eRn52A" Then Return 119
        If EERSTE_BOD = "eRn66A" Then Return 117
        If EERSTE_BOD = "eRn66Av" Then Return 118
        If EERSTE_BOD = "eRn82A" Then Return 119
        If EERSTE_BOD = "eRn94C" Then Return 117
        If EERSTE_BOD = "eRn95A" Then Return 116
        If EERSTE_BOD = "eRn95Av" Then Return 118
        If EERSTE_BOD = "eRo40A" Then Return 117
        If EERSTE_BOD = "eRv01A" Then Return 118
        If EERSTE_BOD = "eRv01C" Then Return 118
        If EERSTE_BOD = "EZ50A" Then Return 107
        If EERSTE_BOD = "EZ50Av" Then Return 107
        If EERSTE_BOD = "EZg21" Then Return 112
        If EERSTE_BOD = "EZg21g" Then Return 112
        If EERSTE_BOD = "EZg21v" Then Return 112
        If EERSTE_BOD = "EZg21w" Then Return 112
        If EERSTE_BOD = "EZg23" Then Return 112
        If EERSTE_BOD = "EZg23g" Then Return 112
        If EERSTE_BOD = "EZg23t" Then Return 112
        If EERSTE_BOD = "EZg23tw" Then Return 112
        If EERSTE_BOD = "EZg23w" Then Return 112
        If EERSTE_BOD = "EZg23wg" Then Return 112
        If EERSTE_BOD = "EZg23wt" Then Return 112
        If EERSTE_BOD = "EZg30" Then Return 112
        If EERSTE_BOD = "EZg30g" Then Return 112
        If EERSTE_BOD = "EZg30v" Then Return 112
        If EERSTE_BOD = "fABk" Then Return 119
        If EERSTE_BOD = "fAFk" Then Return 119
        If EERSTE_BOD = "fAFz" Then Return 113
        If EERSTE_BOD = "faVc" Then Return 101
        If EERSTE_BOD = "faVz" Then Return 102
        If EERSTE_BOD = "faVzt" Then Return 102
        If EERSTE_BOD = "FG" Then Return 114
        If EERSTE_BOD = "fHn21" Then Return 109
        If EERSTE_BOD = "fhVc" Then Return 101
        If EERSTE_BOD = "fhVd" Then Return 101
        If EERSTE_BOD = "fhVz" Then Return 102
        If EERSTE_BOD = "fiVc" Then Return 105
        If EERSTE_BOD = "fiVz" Then Return 105
        If EERSTE_BOD = "fiWp" Then Return 105
        If EERSTE_BOD = "fiWz" Then Return 105
        If EERSTE_BOD = "FKk" Then Return 121
        If EERSTE_BOD = "fkpZg23" Then Return 119
        If EERSTE_BOD = "fkpZg23g" Then Return 120
        If EERSTE_BOD = "fkpZg23t" Then Return 119
        If EERSTE_BOD = "fKRn1" Then Return 119
        If EERSTE_BOD = "fKRn1g" Then Return 120
        If EERSTE_BOD = "fKRn2g" Then Return 120
        If EERSTE_BOD = "fKRn8" Then Return 119
        If EERSTE_BOD = "fKRn8g" Then Return 120
        If EERSTE_BOD = "fkVc" Then Return 103
        If EERSTE_BOD = "fkVs" Then Return 103
        If EERSTE_BOD = "fkVz" Then Return 104
        If EERSTE_BOD = "fkWz" Then Return 104
        If EERSTE_BOD = "fkWzg" Then Return 104
        If EERSTE_BOD = "fkZn21" Then Return 119
        If EERSTE_BOD = "fkZn23" Then Return 119
        If EERSTE_BOD = "fkZn23g" Then Return 120
        If EERSTE_BOD = "fkZn30" Then Return 120
        If EERSTE_BOD = "fMn56Cp" Then Return 119
        If EERSTE_BOD = "fMn56Cv" Then Return 118
        If EERSTE_BOD = "fpLn5" Then Return 121
        If EERSTE_BOD = "fpRn59" Then Return 119
        If EERSTE_BOD = "fpRn86" Then Return 117
        If EERSTE_BOD = "fpVc" Then Return 103
        If EERSTE_BOD = "fpVs" Then Return 103
        If EERSTE_BOD = "fpVz" Then Return 104
        If EERSTE_BOD = "fpZg21" Then Return 109
        If EERSTE_BOD = "fpZg21g" Then Return 110
        If EERSTE_BOD = "fpZg23" Then Return 113
        If EERSTE_BOD = "fpZg23g" Then Return 113
        If EERSTE_BOD = "fpZg23t" Then Return 111
        If EERSTE_BOD = "fpZg23x" Then Return 111
        If EERSTE_BOD = "fpZn21" Then Return 109
        If EERSTE_BOD = "fpZn23tg" Then Return 111
        If EERSTE_BOD = "fRn15C" Then Return 115
        If EERSTE_BOD = "fRn62C" Then Return 119
        If EERSTE_BOD = "fRn62Cg" Then Return 120
        If EERSTE_BOD = "fRn95C" Then Return 116
        If EERSTE_BOD = "fRo60C" Then Return 116
        If EERSTE_BOD = "fRv01C" Then Return 118
        If EERSTE_BOD = "fVc" Then Return 101
        If EERSTE_BOD = "fvWz" Then Return 102
        If EERSTE_BOD = "fvWzt" Then Return 102
        If EERSTE_BOD = "fvWztx" Then Return 102
        If EERSTE_BOD = "fVz" Then Return 102
        If EERSTE_BOD = "fZn21" Then Return 107
        If EERSTE_BOD = "fZn21g" Then Return 107
        If EERSTE_BOD = "fZn23" Then Return 113
        If EERSTE_BOD = "fZn23-F" Then Return 113
        If EERSTE_BOD = "fZn23g" Then Return 113
        If EERSTE_BOD = "fzVc" Then Return 105
        If EERSTE_BOD = "fzVz" Then Return 105
        If EERSTE_BOD = "fzVzt" Then Return 105
        If EERSTE_BOD = "fzWp" Then Return 105
        If EERSTE_BOD = "fzWz" Then Return 105
        If EERSTE_BOD = "fzWzt" Then Return 105
        If EERSTE_BOD = "gbEZ21" Then Return 112
        If EERSTE_BOD = "gbEZ30" Then Return 112
        If EERSTE_BOD = "gcHd30" Then Return 114
        If EERSTE_BOD = "gcHn21" Then Return 109
        If EERSTE_BOD = "gcHn30" Then Return 114
        If EERSTE_BOD = "gcY21" Then Return 109
        If EERSTE_BOD = "gcY23" Then Return 113
        If EERSTE_BOD = "gcY30" Then Return 114
        If EERSTE_BOD = "gcZd30" Then Return 114
        If EERSTE_BOD = "gHd21" Then Return 108
        If EERSTE_BOD = "gHd30" Then Return 114
        If EERSTE_BOD = "gHn21" Then Return 109
        If EERSTE_BOD = "gHn21t" Then Return 111
        If EERSTE_BOD = "gHn21x" Then Return 111
        If EERSTE_BOD = "gHn23" Then Return 113
        If EERSTE_BOD = "gHn23x" Then Return 111
        If EERSTE_BOD = "gHn30" Then Return 114
        If EERSTE_BOD = "gHn30t" Then Return 114
        If EERSTE_BOD = "gHn30x" Then Return 114
        If EERSTE_BOD = "gKRd1" Then Return 119
        If EERSTE_BOD = "gKRd7" Then Return 119
        If EERSTE_BOD = "gKRn1" Then Return 119
        If EERSTE_BOD = "gKRn2" Then Return 119
        If EERSTE_BOD = "gLd6" Then Return 121
        If EERSTE_BOD = "gLh6" Then Return 121
        If EERSTE_BOD = "gMK" Then Return 115
        If EERSTE_BOD = "gMn15C" Then Return 115
        If EERSTE_BOD = "gMn25C" Then Return 115
        If EERSTE_BOD = "gMn25Cv" Then Return 115
        If EERSTE_BOD = "gMn52C" Then Return 119
        If EERSTE_BOD = "gMn52Cp" Then Return 119
        If EERSTE_BOD = "gMn52Cw" Then Return 119
        If EERSTE_BOD = "gMn53C" Then Return 117
        If EERSTE_BOD = "gMn53Cp" Then Return 119
        If EERSTE_BOD = "gMn53Cpx" Then Return 119
        If EERSTE_BOD = "gMn53Cv" Then Return 118
        If EERSTE_BOD = "gMn53Cw" Then Return 117
        If EERSTE_BOD = "gMn53Cwp" Then Return 119
        If EERSTE_BOD = "gMn58C" Then Return 117
        If EERSTE_BOD = "gMn58Cv" Then Return 117
        If EERSTE_BOD = "nkZn50A" Then Return 119
        If EERSTE_BOD = "gMn82C" Then Return 119
        If EERSTE_BOD = "gMn83C" Then Return 117
        If EERSTE_BOD = "gMn83Cp" Then Return 117
        If EERSTE_BOD = "gMn83Cv" Then Return 118
        If EERSTE_BOD = "gMn83Cw" Then Return 117
        If EERSTE_BOD = "gMn83Cwp" Then Return 117
        If EERSTE_BOD = "gMn85C" Then Return 116
        If EERSTE_BOD = "gMn85Cv" Then Return 118
        If EERSTE_BOD = "gMn85Cwl" Then Return 116
        If EERSTE_BOD = "gMn88C" Then Return 117
        If EERSTE_BOD = "gMn88Cl" Then Return 117
        If EERSTE_BOD = "gMn88Clv" Then Return 118
        If EERSTE_BOD = "gMn88Cv" Then Return 118
        If EERSTE_BOD = "gMn88Cw" Then Return 117
        If EERSTE_BOD = "gpZg23x" Then Return 111
        If EERSTE_BOD = "gpZg30" Then Return 114
        If EERSTE_BOD = "gpZn21" Then Return 109
        If EERSTE_BOD = "gpZn21x" Then Return 111
        If EERSTE_BOD = "gpZn23x" Then Return 111
        If EERSTE_BOD = "gpZn30" Then Return 114
        If EERSTE_BOD = "gRd10A" Then Return 119
        If EERSTE_BOD = "gRn15A" Then Return 119
        If EERSTE_BOD = "gRn94Cv" Then Return 117
        If EERSTE_BOD = "gtZd30" Then Return 114
        If EERSTE_BOD = "gvWp" Then Return 102
        If EERSTE_BOD = "gY21" Then Return 109
        If EERSTE_BOD = "gY21g" Then Return 109
        If EERSTE_BOD = "gY23" Then Return 113
        If EERSTE_BOD = "gY30" Then Return 114
        If EERSTE_BOD = "gY30-F" Then Return 114
        If EERSTE_BOD = "gY30-G" Then Return 114
        If EERSTE_BOD = "gZb30" Then Return 114
        If EERSTE_BOD = "gZd21" Then Return 107
        If EERSTE_BOD = "gZd30" Then Return 114
        If EERSTE_BOD = "gzEZ21" Then Return 112
        If EERSTE_BOD = "gzEZ23" Then Return 112
        If EERSTE_BOD = "gzEZ30" Then Return 112
        If EERSTE_BOD = "gZn30" Then Return 114
        If EERSTE_BOD = "Hd21" Then Return 108
        If EERSTE_BOD = "Hd21g" Then Return 108
        If EERSTE_BOD = "Hd21x" Then Return 108
        If EERSTE_BOD = "Hd23" Then Return 113
        If EERSTE_BOD = "Hd23g" Then Return 110
        If EERSTE_BOD = "Hd23x" Then Return 111
        If EERSTE_BOD = "Hd30" Then Return 114
        If EERSTE_BOD = "Hd30g" Then Return 114
        If EERSTE_BOD = "hEV" Then Return 101
        If EERSTE_BOD = "Hn21" Then Return 109
        If EERSTE_BOD = "Hn21-F" Then Return 109
        If EERSTE_BOD = "Hn21g" Then Return 110
        If EERSTE_BOD = "Hn21gx" Then Return 110
        If EERSTE_BOD = "Hn21t" Then Return 111
        If EERSTE_BOD = "Hn21v" Then Return 109
        If EERSTE_BOD = "Hn21w" Then Return 109
        If EERSTE_BOD = "Hn21wg" Then Return 109
        If EERSTE_BOD = "Hn21x" Then Return 111
        If EERSTE_BOD = "Hn21x-F" Then Return 111
        If EERSTE_BOD = "Hn21xg" Then Return 111
        If EERSTE_BOD = "Hn23" Then Return 113
        If EERSTE_BOD = "Hn23-F" Then Return 113
        If EERSTE_BOD = "Hn23g" Then Return 110
        If EERSTE_BOD = "Hn23t" Then Return 111
        If EERSTE_BOD = "Hn23x" Then Return 111
        If EERSTE_BOD = "Hn23x-F" Then Return 111
        If EERSTE_BOD = "Hn23xg" Then Return 111
        If EERSTE_BOD = "Hn30" Then Return 114
        If EERSTE_BOD = "Hn30g" Then Return 114
        If EERSTE_BOD = "Hn30x" Then Return 114
        If EERSTE_BOD = "hRd10A" Then Return 119
        If EERSTE_BOD = "hRd10C" Then Return 119
        If EERSTE_BOD = "hRd90A" Then Return 116
        If EERSTE_BOD = "hVb" Then Return 101
        If EERSTE_BOD = "hVc" Then Return 101
        If EERSTE_BOD = "hVcc" Then Return 101
        If EERSTE_BOD = "hVd" Then Return 101
        If EERSTE_BOD = "hVk" Then Return 106
        If EERSTE_BOD = "hVkl" Then Return 106
        If EERSTE_BOD = "hVr" Then Return 101
        If EERSTE_BOD = "hVs" Then Return 101
        If EERSTE_BOD = "hVsc" Then Return 101
        If EERSTE_BOD = "hVz" Then Return 102
        If EERSTE_BOD = "hVzc" Then Return 102
        If EERSTE_BOD = "hVzg" Then Return 102
        If EERSTE_BOD = "hVzx" Then Return 102
        If EERSTE_BOD = "hZd20A" Then Return 107
        If EERSTE_BOD = "iVc" Then Return 105
        If EERSTE_BOD = "iVp" Then Return 105
        If EERSTE_BOD = "iVpc" Then Return 105
        If EERSTE_BOD = "iVpg" Then Return 105
        If EERSTE_BOD = "iVpt" Then Return 105
        If EERSTE_BOD = "iVpx" Then Return 105
        If EERSTE_BOD = "iVs" Then Return 105
        If EERSTE_BOD = "iVz" Then Return 105
        If EERSTE_BOD = "iVzg" Then Return 105
        If EERSTE_BOD = "iVzt" Then Return 105
        If EERSTE_BOD = "iVzx" Then Return 105
        If EERSTE_BOD = "iWp" Then Return 105
        If EERSTE_BOD = "iWpc" Then Return 105
        If EERSTE_BOD = "iWpg" Then Return 105
        If EERSTE_BOD = "iWpt" Then Return 105
        If EERSTE_BOD = "iWpx" Then Return 105
        If EERSTE_BOD = "iWz" Then Return 105
        If EERSTE_BOD = "iWzt" Then Return 105
        If EERSTE_BOD = "iWzx" Then Return 105
        If EERSTE_BOD = "kcHn21" Then Return 119
        If EERSTE_BOD = "kgpZg30" Then Return 120
        If EERSTE_BOD = "kHn21" Then Return 119
        If EERSTE_BOD = "kHn21g" Then Return 120
        If EERSTE_BOD = "kHn21x" Then Return 119
        If EERSTE_BOD = "kHn23" Then Return 119
        If EERSTE_BOD = "kHn23x" Then Return 119
        If EERSTE_BOD = "kHn30" Then Return 120
        If EERSTE_BOD = "KK" Then Return 121
        If EERSTE_BOD = "KM" Then Return 121
        If EERSTE_BOD = "kMn43C" Then Return 117
        If EERSTE_BOD = "kMn43Cp" Then Return 117
        If EERSTE_BOD = "kMn43Cpx" Then Return 117
        If EERSTE_BOD = "kMn43Cv" Then Return 118
        If EERSTE_BOD = "kMn43Cwp" Then Return 117
        If EERSTE_BOD = "kMn48C" Then Return 117
        If EERSTE_BOD = "kMn48Cl" Then Return 117
        If EERSTE_BOD = "kMn48Clv" Then Return 118
        If EERSTE_BOD = "kMn48Cv" Then Return 118
        If EERSTE_BOD = "kMn48Cvl" Then Return 118
        If EERSTE_BOD = "kMn48Cw" Then Return 117
        If EERSTE_BOD = "kMn63C" Then Return 117
        If EERSTE_BOD = "kMn63Cp" Then Return 119
        If EERSTE_BOD = "kMn63Cpx" Then Return 119
        If EERSTE_BOD = "kMn63Cv" Then Return 118
        If EERSTE_BOD = "kMn63Cwp" Then Return 119
        If EERSTE_BOD = "kMn68C" Then Return 117
        If EERSTE_BOD = "kMn68Cl" Then Return 117
        If EERSTE_BOD = "kMn68Cv" Then Return 118
        If EERSTE_BOD = "kpZg20A" Then Return 119
        If EERSTE_BOD = "kpZg21" Then Return 119
        If EERSTE_BOD = "kpZg21g" Then Return 120
        If EERSTE_BOD = "kpZg23" Then Return 119
        If EERSTE_BOD = "kpZg23g" Then Return 120
        If EERSTE_BOD = "kpZg23t" Then Return 119
        If EERSTE_BOD = "kpZg23x" Then Return 119
        If EERSTE_BOD = "kpZn21" Then Return 119
        If EERSTE_BOD = "kpZn21g" Then Return 120
        If EERSTE_BOD = "kpZn23" Then Return 119
        If EERSTE_BOD = "kpZn23x" Then Return 119
        If EERSTE_BOD = "KRd1" Then Return 119
        If EERSTE_BOD = "KRd1g" Then Return 120
        If EERSTE_BOD = "KRd7" Then Return 119
        If EERSTE_BOD = "KRd7g" Then Return 120
        If EERSTE_BOD = "KRn1" Then Return 119
        If EERSTE_BOD = "KRn1g" Then Return 120
        If EERSTE_BOD = "KRn2" Then Return 119
        If EERSTE_BOD = "KRn2g" Then Return 120
        If EERSTE_BOD = "KRn2w" Then Return 119
        If EERSTE_BOD = "KRn8" Then Return 119
        If EERSTE_BOD = "KRn8g" Then Return 120
        If EERSTE_BOD = "KS" Then Return 115
        If EERSTE_BOD = "kSn13A" Then Return 119
        If EERSTE_BOD = "kSn13Av" Then Return 119
        If EERSTE_BOD = "kSn13Aw" Then Return 119
        If EERSTE_BOD = "kSn14A" Then Return 119
        If EERSTE_BOD = "kSn14Ap" Then Return 119
        If EERSTE_BOD = "kSn14Av" Then Return 119
        If EERSTE_BOD = "kSn14Aw" Then Return 119
        If EERSTE_BOD = "kSn14Awp" Then Return 119
        If EERSTE_BOD = "KT" Then Return 115
        If EERSTE_BOD = "kVb" Then Return 103
        If EERSTE_BOD = "kVc" Then Return 103
        If EERSTE_BOD = "kVcc" Then Return 103
        If EERSTE_BOD = "kVd" Then Return 103
        If EERSTE_BOD = "kVk" Then Return 106
        If EERSTE_BOD = "kVr" Then Return 103
        If EERSTE_BOD = "kVs" Then Return 103
        If EERSTE_BOD = "kVsc" Then Return 103
        If EERSTE_BOD = "kVz" Then Return 104
        If EERSTE_BOD = "kVzc" Then Return 104
        If EERSTE_BOD = "kVzx" Then Return 104
        If EERSTE_BOD = "kWp" Then Return 104
        If EERSTE_BOD = "kWpg" Then Return 104
        If EERSTE_BOD = "kWpx" Then Return 104
        If EERSTE_BOD = "kWz" Then Return 104
        If EERSTE_BOD = "kWzg" Then Return 104
        If EERSTE_BOD = "kWzx" Then Return 104
        If EERSTE_BOD = "KX" Then Return 115
        If EERSTE_BOD = "kZb21" Then Return 119
        If EERSTE_BOD = "kZb23" Then Return 119
        If EERSTE_BOD = "kZn10A" Then Return 119
        If EERSTE_BOD = "kZn10Av" Then Return 119
        If EERSTE_BOD = "kZn21" Then Return 119
        If EERSTE_BOD = "kZn21g" Then Return 120
        If EERSTE_BOD = "kZn21p" Then Return 119
        If EERSTE_BOD = "kZn21r" Then Return 119
        If EERSTE_BOD = "kZn21w" Then Return 119
        If EERSTE_BOD = "kZn21x" Then Return 119
        If EERSTE_BOD = "kZn23" Then Return 119
        If EERSTE_BOD = "kZn30" Then Return 120
        If EERSTE_BOD = "kZn30A" Then Return 120
        If EERSTE_BOD = "kZn30Ar" Then Return 120
        If EERSTE_BOD = "kZn30x" Then Return 120
        If EERSTE_BOD = "kZn40A" Then Return 119
        If EERSTE_BOD = "kZn40Ap" Then Return 119
        If EERSTE_BOD = "kZn40Av" Then Return 119
        If EERSTE_BOD = "kZn50A" Then Return 119
        If EERSTE_BOD = "kZn50Ap" Then Return 119
        If EERSTE_BOD = "kZn50Ar" Then Return 119
        If EERSTE_BOD = "Ld5" Then Return 121
        If EERSTE_BOD = "Ld5g" Then Return 121
        If EERSTE_BOD = "Ld5m" Then Return 121
        If EERSTE_BOD = "Ld5t" Then Return 121
        If EERSTE_BOD = "Ld6" Then Return 121
        If EERSTE_BOD = "Ld6a" Then Return 121
        If EERSTE_BOD = "Ld6g" Then Return 121
        If EERSTE_BOD = "Ld6k" Then Return 121
        If EERSTE_BOD = "Ld6m" Then Return 121
        If EERSTE_BOD = "Ld6s" Then Return 121
        If EERSTE_BOD = "Ld6t" Then Return 121
        If EERSTE_BOD = "Ldd5" Then Return 121
        If EERSTE_BOD = "Ldd5g" Then Return 121
        If EERSTE_BOD = "Ldd6" Then Return 121
        If EERSTE_BOD = "Ldh5" Then Return 121
        If EERSTE_BOD = "Ldh5g" Then Return 121
        If EERSTE_BOD = "Ldh5t" Then Return 121
        If EERSTE_BOD = "Ldh6" Then Return 121
        If EERSTE_BOD = "Ldh6m" Then Return 121
        If EERSTE_BOD = "lFG" Then Return 114
        If EERSTE_BOD = "lFK" Then Return 121
        If EERSTE_BOD = "lFKk" Then Return 121
        If EERSTE_BOD = "Lh5" Then Return 121
        If EERSTE_BOD = "Lh5g" Then Return 121
        If EERSTE_BOD = "Lh6g" Then Return 121
        If EERSTE_BOD = "Lh6s" Then Return 121
        If EERSTE_BOD = "lKK" Then Return 116
        If EERSTE_BOD = "lKM" Then Return 116
        If EERSTE_BOD = "lKRd7" Then Return 119
        If EERSTE_BOD = "lKS" Then Return 121
        If EERSTE_BOD = "Ln5" Then Return 121
        If EERSTE_BOD = "Ln5g" Then Return 121
        If EERSTE_BOD = "Ln5m" Then Return 121
        If EERSTE_BOD = "Ln5t" Then Return 121
        If EERSTE_BOD = "Ln6a" Then Return 121
        If EERSTE_BOD = "Ln6m" Then Return 121
        If EERSTE_BOD = "Ln6t" Then Return 121
        If EERSTE_BOD = "Lnd5" Then Return 121
        If EERSTE_BOD = "Lnd5g" Then Return 121
        If EERSTE_BOD = "Lnd5m" Then Return 121
        If EERSTE_BOD = "Lnd5t" Then Return 121
        If EERSTE_BOD = "Lnd6" Then Return 121
        If EERSTE_BOD = "Lnd6v" Then Return 121
        If EERSTE_BOD = "Lnh6" Then Return 121
        If EERSTE_BOD = "MA" Then Return 116
        If EERSTE_BOD = "mcY23" Then Return 113
        If EERSTE_BOD = "mcY23x" Then Return 111
        If EERSTE_BOD = "mHd23" Then Return 113
        If EERSTE_BOD = "mHn21x" Then Return 111
        If EERSTE_BOD = "mHn23x" Then Return 111
        If EERSTE_BOD = "MK" Then Return 116
        If EERSTE_BOD = "mKK" Then Return 116
        If EERSTE_BOD = "mKRd7" Then Return 119
        If EERSTE_BOD = "mKX" Then Return 115
        If EERSTE_BOD = "mLd6s" Then Return 121
        If EERSTE_BOD = "mLh6s" Then Return 121
        If EERSTE_BOD = "Mn12A" Then Return 119
        If EERSTE_BOD = "Mn12Ap" Then Return 119
        If EERSTE_BOD = "Mn12Av" Then Return 119
        If EERSTE_BOD = "Mn12Awp" Then Return 119
        If EERSTE_BOD = "Mn15A" Then Return 115
        If EERSTE_BOD = "Mn15Ap" Then Return 119
        If EERSTE_BOD = "Mn15Av" Then Return 118
        If EERSTE_BOD = "Mn15Aw" Then Return 115
        If EERSTE_BOD = "Mn15Awp" Then Return 119
        If EERSTE_BOD = "Mn15C" Then Return 115
        If EERSTE_BOD = "Mn15Clv" Then Return 118
        If EERSTE_BOD = "Mn15Cv" Then Return 118
        If EERSTE_BOD = "Mn15Cw" Then Return 115
        If EERSTE_BOD = "Mn22A" Then Return 119
        If EERSTE_BOD = "Mn22Alv" Then Return 115
        If EERSTE_BOD = "Mn22Ap" Then Return 119
        If EERSTE_BOD = "Mn22Av" Then Return 115
        If EERSTE_BOD = "Mn22Aw" Then Return 119
        If EERSTE_BOD = "Mn22Awp" Then Return 119
        If EERSTE_BOD = "Mn22Ax" Then Return 119
        If EERSTE_BOD = "Mn25A" Then Return 115
        If EERSTE_BOD = "Mn25Alv" Then Return 115
        If EERSTE_BOD = "Mn25Ap" Then Return 119
        If EERSTE_BOD = "Mn25Av" Then Return 118
        If EERSTE_BOD = "Mn25Aw" Then Return 115
        If EERSTE_BOD = "Mn25Awp" Then Return 119
        If EERSTE_BOD = "Mn25C" Then Return 115
        If EERSTE_BOD = "Mn25Cp" Then Return 119
        If EERSTE_BOD = "Mn25Cv" Then Return 118
        If EERSTE_BOD = "Mn25Cw" Then Return 115
        If EERSTE_BOD = "Mn35A" Then Return 116
        If EERSTE_BOD = "Mn35Ap" Then Return 119
        If EERSTE_BOD = "Mn35Av" Then Return 118
        If EERSTE_BOD = "Mn35Aw" Then Return 116
        If EERSTE_BOD = "Mn35Awp" Then Return 119
        If EERSTE_BOD = "Mn35Ax" Then Return 116
        If EERSTE_BOD = "Mn45A" Then Return 117
        If EERSTE_BOD = "Mn45Ap" Then Return 119
        If EERSTE_BOD = "Mn45Av" Then Return 118
        If EERSTE_BOD = "Mn52C" Then Return 119
        If EERSTE_BOD = "Mn52Cp" Then Return 119
        If EERSTE_BOD = "Mn52Cpx" Then Return 119
        If EERSTE_BOD = "Mn52Cwp" Then Return 119
        If EERSTE_BOD = "Mn52Cx" Then Return 119
        If EERSTE_BOD = "Mn56A" Then Return 117
        If EERSTE_BOD = "Mn56Ap" Then Return 119
        If EERSTE_BOD = "Mn56Av" Then Return 118
        If EERSTE_BOD = "Mn56Aw" Then Return 117
        If EERSTE_BOD = "Mn56C" Then Return 117
        If EERSTE_BOD = "Mn56Cp" Then Return 119
        If EERSTE_BOD = "Mn56Cv" Then Return 118
        If EERSTE_BOD = "Mn56Cwp" Then Return 119
        If EERSTE_BOD = "Mn82A" Then Return 119
        If EERSTE_BOD = "Mn82Ap" Then Return 119
        If EERSTE_BOD = "Mn82C" Then Return 119
        If EERSTE_BOD = "Mn82Cp" Then Return 119
        If EERSTE_BOD = "Mn82Cpx" Then Return 119
        If EERSTE_BOD = "Mn82Cwp" Then Return 119
        If EERSTE_BOD = "Mn85C" Then Return 116
        If EERSTE_BOD = "Mn85Clwp" Then Return 119
        If EERSTE_BOD = "Mn85Cp" Then Return 119
        If EERSTE_BOD = "Mn85Cv" Then Return 118
        If EERSTE_BOD = "Mn85Cw" Then Return 116
        If EERSTE_BOD = "Mn85Cwp" Then Return 119
        If EERSTE_BOD = "Mn86A" Then Return 117
        If EERSTE_BOD = "Mn86Al" Then Return 117
        If EERSTE_BOD = "Mn86Av" Then Return 118
        If EERSTE_BOD = "Mn86Aw" Then Return 117
        If EERSTE_BOD = "Mn86C" Then Return 117
        If EERSTE_BOD = "Mn86Cl" Then Return 117
        If EERSTE_BOD = "Mn86Clv" Then Return 117
        If EERSTE_BOD = "Mn86Clw" Then Return 117
        If EERSTE_BOD = "Mn86Clwp" Then Return 119
        If EERSTE_BOD = "Mn86Cp" Then Return 119
        If EERSTE_BOD = "Mn86Cv" Then Return 118
        If EERSTE_BOD = "Mn86Cw" Then Return 117
        If EERSTE_BOD = "Mn86Cwp" Then Return 119
        If EERSTE_BOD = "Mo10A" Then Return 115
        If EERSTE_BOD = "Mo10Av" Then Return 115
        If EERSTE_BOD = "Mo20A" Then Return 115
        If EERSTE_BOD = "Mo20Av" Then Return 115
        If EERSTE_BOD = "Mo50C" Then Return 115
        If EERSTE_BOD = "Mo80A" Then Return 116
        If EERSTE_BOD = "Mo80Ap" Then Return 119
        If EERSTE_BOD = "Mo80Av" Then Return 118
        If EERSTE_BOD = "Mo80C" Then Return 116
        If EERSTE_BOD = "Mo80Cl" Then Return 116
        If EERSTE_BOD = "Mo80Cp" Then Return 119
        If EERSTE_BOD = "Mo80Cv" Then Return 118
        If EERSTE_BOD = "Mo80Cvl" Then Return 118
        If EERSTE_BOD = "Mo80Cw" Then Return 116
        If EERSTE_BOD = "Mo80Cwp" Then Return 119
        If EERSTE_BOD = "MOb12" Then Return 119
        If EERSTE_BOD = "MOb15" Then Return 115
        If EERSTE_BOD = "MOb72" Then Return 119
        If EERSTE_BOD = "MOb75" Then Return 116
        If EERSTE_BOD = "MOo02" Then Return 119
        If EERSTE_BOD = "MOo02v" Then Return 119
        If EERSTE_BOD = "MOo05" Then Return 115
        If EERSTE_BOD = "Mv41C" Then Return 118
        If EERSTE_BOD = "Mv41Cl" Then Return 118
        If EERSTE_BOD = "Mv41Cp" Then Return 118
        If EERSTE_BOD = "Mv41Cv" Then Return 118
        If EERSTE_BOD = "Mv51A" Then Return 118
        If EERSTE_BOD = "Mv51Al" Then Return 118
        If EERSTE_BOD = "Mv51Ap" Then Return 118
        If EERSTE_BOD = "Mv61C" Then Return 118
        If EERSTE_BOD = "Mv61Cl" Then Return 118
        If EERSTE_BOD = "Mv61Cp" Then Return 118
        If EERSTE_BOD = "Mv81A" Then Return 118
        If EERSTE_BOD = "Mv81Al" Then Return 118
        If EERSTE_BOD = "Mv81Ap" Then Return 118
        If EERSTE_BOD = "mY23" Then Return 113
        If EERSTE_BOD = "mY23x" Then Return 111
        If EERSTE_BOD = "mZb23x" Then Return 111
        If EERSTE_BOD = "MZk" Then Return 121
        If EERSTE_BOD = "MZz" Then Return 107
        If EERSTE_BOD = "nAO" Then Return 119
        If EERSTE_BOD = "nkZn21" Then Return 119
        If EERSTE_BOD = "nkZn50Ab" Then Return 119
        If EERSTE_BOD = "nMn15A" Then Return 115
        If EERSTE_BOD = "nMn15Av" Then Return 115
        If EERSTE_BOD = "nMo10A" Then Return 115
        If EERSTE_BOD = "nMo10Av" Then Return 118
        If EERSTE_BOD = "nMo80A" Then Return 116
        If EERSTE_BOD = "nMo80Aw" Then Return 116
        If EERSTE_BOD = "nMv61C" Then Return 118
        If EERSTE_BOD = "npMo50l" Then Return 115
        If EERSTE_BOD = "npMo80l" Then Return 116
        If EERSTE_BOD = "nSn13A" Then Return 113
        If EERSTE_BOD = "nSn13Av" Then Return 113
        If EERSTE_BOD = "nvWz" Then Return 102
        If EERSTE_BOD = "nZn21" Then Return 107
        If EERSTE_BOD = "nZn40A" Then Return 107
        If EERSTE_BOD = "nZn50A" Then Return 107
        If EERSTE_BOD = "nZn50Ab" Then Return 107
        If EERSTE_BOD = "ohVb" Then Return 101
        If EERSTE_BOD = "ohVc" Then Return 101
        If EERSTE_BOD = "ohVk" Then Return 106
        If EERSTE_BOD = "ohVs" Then Return 101
        If EERSTE_BOD = "opVb" Then Return 103
        If EERSTE_BOD = "opVc" Then Return 103
        If EERSTE_BOD = "opVk" Then Return 106
        If EERSTE_BOD = "opVs" Then Return 103
        If EERSTE_BOD = "pKRn1" Then Return 119
        If EERSTE_BOD = "pKRn1g" Then Return 120
        If EERSTE_BOD = "pKRn2" Then Return 119
        If EERSTE_BOD = "pKRn2g" Then Return 120
        If EERSTE_BOD = "pLn5" Then Return 121
        If EERSTE_BOD = "pLn5g" Then Return 121
        If EERSTE_BOD = "pMn52A" Then Return 119
        If EERSTE_BOD = "pMn52C" Then Return 119
        If EERSTE_BOD = "pMn52Cp" Then Return 119
        If EERSTE_BOD = "pMn55A" Then Return 115
        If EERSTE_BOD = "pMn55Av" Then Return 118
        If EERSTE_BOD = "pMn55Aw" Then Return 115
        If EERSTE_BOD = "pMn55C" Then Return 115
        If EERSTE_BOD = "pMn55Cp" Then Return 119
        If EERSTE_BOD = "pMn56C" Then Return 117
        If EERSTE_BOD = "pMn56Cl" Then Return 117
        If EERSTE_BOD = "pMn82A" Then Return 119
        If EERSTE_BOD = "pMn82C" Then Return 119
        If EERSTE_BOD = "pMn85A" Then Return 116
        If EERSTE_BOD = "pMn85Aw" Then Return 116
        If EERSTE_BOD = "pMn85C" Then Return 116
        If EERSTE_BOD = "pMn85Cv" Then Return 118
        If EERSTE_BOD = "pMn86C" Then Return 117
        If EERSTE_BOD = "pMn86Cl" Then Return 117
        If EERSTE_BOD = "pMn86Cv" Then Return 118
        If EERSTE_BOD = "pMn86Cw" Then Return 117
        If EERSTE_BOD = "pMn86Cwl" Then Return 117
        If EERSTE_BOD = "pMo50" Then Return 115
        If EERSTE_BOD = "pMo50l" Then Return 115
        If EERSTE_BOD = "pMo50w" Then Return 115
        If EERSTE_BOD = "pMo80" Then Return 116
        If EERSTE_BOD = "pMo80l" Then Return 116
        If EERSTE_BOD = "pMo80v" Then Return 118
        If EERSTE_BOD = "pMv51" Then Return 118
        If EERSTE_BOD = "pMv81" Then Return 118
        If EERSTE_BOD = "pMv81l" Then Return 118
        If EERSTE_BOD = "pMv81p" Then Return 118
        If EERSTE_BOD = "pRn56p" Then Return 119
        If EERSTE_BOD = "pRn56v" Then Return 118
        If EERSTE_BOD = "pRn56wp" Then Return 119
        If EERSTE_BOD = "pRn59" Then Return 119
        If EERSTE_BOD = "pRn59p" Then Return 119
        If EERSTE_BOD = "pRn59t" Then Return 119
        If EERSTE_BOD = "pRn59w" Then Return 119
        If EERSTE_BOD = "pRn86" Then Return 117
        If EERSTE_BOD = "pRn86p" Then Return 119
        If EERSTE_BOD = "pRn86t" Then Return 117
        If EERSTE_BOD = "pRn86v" Then Return 118
        If EERSTE_BOD = "pRn86w" Then Return 117
        If EERSTE_BOD = "pRn86wp" Then Return 119
        If EERSTE_BOD = "pRn89v" Then Return 118
        If EERSTE_BOD = "pRv81" Then Return 118
        If EERSTE_BOD = "pVb" Then Return 103
        If EERSTE_BOD = "pVc" Then Return 103
        If EERSTE_BOD = "pVcc" Then Return 103
        If EERSTE_BOD = "pVd" Then Return 103
        If EERSTE_BOD = "pVk" Then Return 106
        If EERSTE_BOD = "pVr" Then Return 103
        If EERSTE_BOD = "pVs" Then Return 103
        If EERSTE_BOD = "pVsc" Then Return 103
        If EERSTE_BOD = "pVsl" Then Return 103
        If EERSTE_BOD = "pVz" Then Return 104
        If EERSTE_BOD = "pVzx" Then Return 104
        If EERSTE_BOD = "pZg20A" Then Return 107
        If EERSTE_BOD = "pZg20Ar" Then Return 107
        If EERSTE_BOD = "pZg21" Then Return 109
        If EERSTE_BOD = "pZg21g" Then Return 110
        If EERSTE_BOD = "pZg21r" Then Return 111
        If EERSTE_BOD = "pZg21t" Then Return 111
        If EERSTE_BOD = "pZg21w" Then Return 109
        If EERSTE_BOD = "pZg21x" Then Return 111
        If EERSTE_BOD = "pZg23" Then Return 113
        If EERSTE_BOD = "pZg23g" Then Return 113
        If EERSTE_BOD = "pZg23r" Then Return 113
        If EERSTE_BOD = "pZg23t" Then Return 111
        If EERSTE_BOD = "pZg23w" Then Return 113
        If EERSTE_BOD = "pZg23x" Then Return 111
        If EERSTE_BOD = "pZg30" Then Return 114
        If EERSTE_BOD = "pZg30p" Then Return 114
        If EERSTE_BOD = "pZg30r" Then Return 114
        If EERSTE_BOD = "pZg30x" Then Return 114
        If EERSTE_BOD = "pZn21" Then Return 109
        If EERSTE_BOD = "pZn21g" Then Return 110
        If EERSTE_BOD = "pZn21t" Then Return 111
        If EERSTE_BOD = "pZn21tg" Then Return 109
        If EERSTE_BOD = "pZn21v" Then Return 109
        If EERSTE_BOD = "pZn21x" Then Return 111
        If EERSTE_BOD = "pZn23" Then Return 113
        If EERSTE_BOD = "pZn23g" Then Return 110
        If EERSTE_BOD = "pZn23gx" Then Return 110
        If EERSTE_BOD = "pZn23t" Then Return 111
        If EERSTE_BOD = "pZn23v" Then Return 113
        If EERSTE_BOD = "pZn23w" Then Return 113
        If EERSTE_BOD = "pZn23x" Then Return 111
        If EERSTE_BOD = "pZn23x-F" Then Return 111
        If EERSTE_BOD = "pZn30" Then Return 114
        If EERSTE_BOD = "pZn30g" Then Return 114
        If EERSTE_BOD = "pZn30r" Then Return 114
        If EERSTE_BOD = "pZn30w" Then Return 114
        If EERSTE_BOD = "pZn30x" Then Return 114
        If EERSTE_BOD = "Rd10A" Then Return 119
        If EERSTE_BOD = "Rd10Ag" Then Return 119
        If EERSTE_BOD = "Rd10C" Then Return 119
        If EERSTE_BOD = "Rd10Cg" Then Return 120
        If EERSTE_BOD = "Rd10Cm" Then Return 119
        If EERSTE_BOD = "Rd10Cp" Then Return 119
        If EERSTE_BOD = "Rd90A" Then Return 116
        If EERSTE_BOD = "Rd90C" Then Return 116
        If EERSTE_BOD = "Rd90Cg" Then Return 120
        If EERSTE_BOD = "Rd90Cm" Then Return 116
        If EERSTE_BOD = "Rd90Cp" Then Return 119
        If EERSTE_BOD = "Rn14C" Then Return 117
        If EERSTE_BOD = "Rn15A" Then Return 115
        If EERSTE_BOD = "Rn15C" Then Return 115
        If EERSTE_BOD = "Rn15Cg" Then Return 115
        If EERSTE_BOD = "Rn15Ct" Then Return 115
        If EERSTE_BOD = "Rn15Cw" Then Return 115
        If EERSTE_BOD = "Rn42Cg" Then Return 119
        If EERSTE_BOD = "Rn42Cp" Then Return 119
        If EERSTE_BOD = "Rn44C" Then Return 117
        If EERSTE_BOD = "Rn44Cv" Then Return 118
        If EERSTE_BOD = "Rn44Cw" Then Return 117
        If EERSTE_BOD = "Rn45A" Then Return 117
        If EERSTE_BOD = "Rn46A" Then Return 117
        If EERSTE_BOD = "Rn46Av" Then Return 118
        If EERSTE_BOD = "Rn46Aw" Then Return 117
        If EERSTE_BOD = "Rn47C" Then Return 117
        If EERSTE_BOD = "Rn47Cg" Then Return 120
        If EERSTE_BOD = "Rn47Cp" Then Return 119
        If EERSTE_BOD = "Rn47Cv" Then Return 118
        If EERSTE_BOD = "Rn47Cw" Then Return 117
        If EERSTE_BOD = "Rn47Cwp" Then Return 119
        If EERSTE_BOD = "Rn52A" Then Return 120
        If EERSTE_BOD = "Rn52Ag" Then Return 120
        If EERSTE_BOD = "Rn62C" Then Return 119
        If EERSTE_BOD = "Rn62Cg" Then Return 120
        If EERSTE_BOD = "Rn62Cp" Then Return 119
        If EERSTE_BOD = "Rn62Cwp" Then Return 119
        If EERSTE_BOD = "Rn66A" Then Return 117
        If EERSTE_BOD = "Rn66Av" Then Return 118
        If EERSTE_BOD = "Rn67C" Then Return 117
        If EERSTE_BOD = "Rn67Cg" Then Return 120
        If EERSTE_BOD = "Rn67Cp" Then Return 119
        If EERSTE_BOD = "Rn67Cv" Then Return 118
        If EERSTE_BOD = "Rn67Cwp" Then Return 119
        If EERSTE_BOD = "Rn82A" Then Return 119
        If EERSTE_BOD = "Rn82Ag" Then Return 120
        If EERSTE_BOD = "Rn94C" Then Return 117
        If EERSTE_BOD = "Rn94Cv" Then Return 118
        If EERSTE_BOD = "Rn95A" Then Return 116
        If EERSTE_BOD = "Rn95Av" Then Return 118
        If EERSTE_BOD = "Rn95C" Then Return 116
        If EERSTE_BOD = "Rn95Cg" Then Return 120
        If EERSTE_BOD = "Rn95Cm" Then Return 116
        If EERSTE_BOD = "Rn95Cp" Then Return 119
        If EERSTE_BOD = "Ro40A" Then Return 117
        If EERSTE_BOD = "Ro40Av" Then Return 118
        If EERSTE_BOD = "Ro40C" Then Return 117
        If EERSTE_BOD = "Ro40Cv" Then Return 118
        If EERSTE_BOD = "Ro40Cw" Then Return 117
        If EERSTE_BOD = "Ro60A" Then Return 116
        If EERSTE_BOD = "Ro60C" Then Return 116
        If EERSTE_BOD = "ROb72" Then Return 119
        If EERSTE_BOD = "ROb75" Then Return 116
        If EERSTE_BOD = "Rv01A" Then Return 118
        If EERSTE_BOD = "Rv01C" Then Return 118
        If EERSTE_BOD = "Rv01Cg" Then Return 118
        If EERSTE_BOD = "Rv01Cp" Then Return 118
        If EERSTE_BOD = "saVc" Then Return 101
        If EERSTE_BOD = "saVz" Then Return 102
        If EERSTE_BOD = "sHn21" Then Return 109
        If EERSTE_BOD = "shVz" Then Return 102
        If EERSTE_BOD = "skVc" Then Return 103
        If EERSTE_BOD = "skWz" Then Return 104
        If EERSTE_BOD = "Sn13A" Then Return 113
        If EERSTE_BOD = "Sn13Ap" Then Return 113
        If EERSTE_BOD = "Sn13Av" Then Return 113
        If EERSTE_BOD = "Sn13Aw" Then Return 113
        If EERSTE_BOD = "Sn13Awp" Then Return 113
        If EERSTE_BOD = "Sn14A" Then Return 113
        If EERSTE_BOD = "Sn14Ap" Then Return 113
        If EERSTE_BOD = "Sn14Av" Then Return 113
        If EERSTE_BOD = "spVc" Then Return 103
        If EERSTE_BOD = "spVz" Then Return 104
        If EERSTE_BOD = "sVc" Then Return 101
        If EERSTE_BOD = "sVk" Then Return 106
        If EERSTE_BOD = "sVp" Then Return 102
        If EERSTE_BOD = "sVs" Then Return 101
        If EERSTE_BOD = "svWp" Then Return 102
        If EERSTE_BOD = "svWz" Then Return 102
        If EERSTE_BOD = "svWzt" Then Return 102
        If EERSTE_BOD = "sVz" Then Return 102
        If EERSTE_BOD = "sVzt" Then Return 102
        If EERSTE_BOD = "sVzx" Then Return 102
        If EERSTE_BOD = "tZd21" Then Return 107
        If EERSTE_BOD = "tZd21g" Then Return 110
        If EERSTE_BOD = "tZd21v" Then Return 107
        If EERSTE_BOD = "tZd23" Then Return 113
        If EERSTE_BOD = "Vb" Then Return 101
        If EERSTE_BOD = "Vc" Then Return 101
        If EERSTE_BOD = "Vd" Then Return 101
        If EERSTE_BOD = "Vk" Then Return 106
        If EERSTE_BOD = "Vo" Then Return 101
        If EERSTE_BOD = "Vp" Then Return 102
        If EERSTE_BOD = "Vpx" Then Return 102
        If EERSTE_BOD = "Vr" Then Return 101
        If EERSTE_BOD = "Vs" Then Return 101
        If EERSTE_BOD = "Vsc" Then Return 101
        If EERSTE_BOD = "vWp" Then Return 102
        If EERSTE_BOD = "vWpg" Then Return 102
        If EERSTE_BOD = "vWpt" Then Return 102
        If EERSTE_BOD = "vWpx" Then Return 102
        If EERSTE_BOD = "vWz" Then Return 102
        If EERSTE_BOD = "vWzg" Then Return 102
        If EERSTE_BOD = "vWzr" Then Return 102
        If EERSTE_BOD = "vWzt" Then Return 102
        If EERSTE_BOD = "vWzx" Then Return 102
        If EERSTE_BOD = "Vz" Then Return 102
        If EERSTE_BOD = "Vzc" Then Return 102
        If EERSTE_BOD = "Vzg" Then Return 102
        If EERSTE_BOD = "Vzt" Then Return 102
        If EERSTE_BOD = "Vzx" Then Return 102
        If EERSTE_BOD = "Wg" Then Return 106
        If EERSTE_BOD = "Wgl" Then Return 106
        If EERSTE_BOD = "Wo" Then Return 106
        If EERSTE_BOD = "Wol" Then Return 106
        If EERSTE_BOD = "Wov" Then Return 106
        If EERSTE_BOD = "Y21" Then Return 109
        If EERSTE_BOD = "Y21g" Then Return 110
        If EERSTE_BOD = "Y21x" Then Return 111
        If EERSTE_BOD = "Y23" Then Return 113
        If EERSTE_BOD = "Y23b" Then Return 113
        If EERSTE_BOD = "Y23g" Then Return 110
        If EERSTE_BOD = "Y23x" Then Return 111
        If EERSTE_BOD = "Y30" Then Return 114
        If EERSTE_BOD = "Y30x" Then Return 114
        If EERSTE_BOD = "Zb20A" Then Return 107
        If EERSTE_BOD = "Zb21" Then Return 109
        If EERSTE_BOD = "Zb21g" Then Return 110
        If EERSTE_BOD = "Zb23" Then Return 113
        If EERSTE_BOD = "Zb23g" Then Return 113
        If EERSTE_BOD = "Zb23t" Then Return 111
        If EERSTE_BOD = "Zb23x" Then Return 111
        If EERSTE_BOD = "Zb30" Then Return 114
        If EERSTE_BOD = "Zb30A" Then Return 114
        If EERSTE_BOD = "Zb30g" Then Return 114
        If EERSTE_BOD = "Zd20A" Then Return 107
        If EERSTE_BOD = "Zd20Ab" Then Return 107
        If EERSTE_BOD = "Zd21" Then Return 107
        If EERSTE_BOD = "Zd21g" Then Return 107
        If EERSTE_BOD = "Zd23" Then Return 113
        If EERSTE_BOD = "Zd30" Then Return 114
        If EERSTE_BOD = "Zd30A" Then Return 114
        If EERSTE_BOD = "zEZ21" Then Return 112
        If EERSTE_BOD = "zEZ21g" Then Return 112
        If EERSTE_BOD = "zEZ21t" Then Return 112
        If EERSTE_BOD = "zEZ21w" Then Return 112
        If EERSTE_BOD = "zEZ21x" Then Return 112
        If EERSTE_BOD = "zEZ23" Then Return 112
        If EERSTE_BOD = "zEZ23g" Then Return 112
        If EERSTE_BOD = "zEZ23t" Then Return 112
        If EERSTE_BOD = "zEZ23w" Then Return 112
        If EERSTE_BOD = "zEZ23x" Then Return 112
        If EERSTE_BOD = "zEZ30" Then Return 112
        If EERSTE_BOD = "zEZ30g" Then Return 112
        If EERSTE_BOD = "zEZ30x" Then Return 112
        If EERSTE_BOD = "zgHd30" Then Return 114
        If EERSTE_BOD = "zgMn15C" Then Return 115
        If EERSTE_BOD = "zgMn88C" Then Return 117
        If EERSTE_BOD = "zgY30" Then Return 114
        If EERSTE_BOD = "zHd21" Then Return 108
        If EERSTE_BOD = "zHd21g" Then Return 108
        If EERSTE_BOD = "zHn21" Then Return 108
        If EERSTE_BOD = "zHn23" Then Return 109
        If EERSTE_BOD = "zhVk" Then Return 106
        If EERSTE_BOD = "zKRn1g" Then Return 120
        If EERSTE_BOD = "zKRn2" Then Return 119
        If EERSTE_BOD = "zkVc" Then Return 103
        If EERSTE_BOD = "zkWp" Then Return 104
        If EERSTE_BOD = "zMn15A" Then Return 115
        If EERSTE_BOD = "zMn22Ap" Then Return 119
        If EERSTE_BOD = "zMn25Ap" Then Return 119
        If EERSTE_BOD = "zMn56Cp" Then Return 117
        If EERSTE_BOD = "zMo10A" Then Return 115
        If EERSTE_BOD = "zMv41C" Then Return 118
        If EERSTE_BOD = "zMv61C" Then Return 118
        If EERSTE_BOD = "Zn10A" Then Return 107
        If EERSTE_BOD = "Zn10Ap" Then Return 107
        If EERSTE_BOD = "Zn10Av" Then Return 107
        If EERSTE_BOD = "Zn10Aw" Then Return 107
        If EERSTE_BOD = "Zn10Awp" Then Return 107
        If EERSTE_BOD = "Zn21" Then Return 107
        If EERSTE_BOD = "Zn21-F" Then Return 107
        If EERSTE_BOD = "Zn21g" Then Return 107
        If EERSTE_BOD = "Zn21-H" Then Return 107
        If EERSTE_BOD = "Zn21p" Then Return 107
        If EERSTE_BOD = "Zn21r" Then Return 107
        If EERSTE_BOD = "Zn21t" Then Return 107
        If EERSTE_BOD = "Zn21v" Then Return 107
        If EERSTE_BOD = "Zn21w" Then Return 107
        If EERSTE_BOD = "Zn21x" Then Return 107
        If EERSTE_BOD = "Zn21x-F" Then Return 107
        If EERSTE_BOD = "Zn23" Then Return 113
        If EERSTE_BOD = "Zn23-F" Then Return 113
        If EERSTE_BOD = "Zn23g" Then Return 113
        If EERSTE_BOD = "Zn23g-F" Then Return 113
        If EERSTE_BOD = "Zn23-H" Then Return 113
        If EERSTE_BOD = "Zn23p" Then Return 113
        If EERSTE_BOD = "Zn23r" Then Return 113
        If EERSTE_BOD = "Zn23t" Then Return 111
        If EERSTE_BOD = "Zn23x" Then Return 111
        If EERSTE_BOD = "Zn30" Then Return 114
        If EERSTE_BOD = "Zn30A" Then Return 114
        If EERSTE_BOD = "Zn30Ab" Then Return 114
        If EERSTE_BOD = "Zn30Ag" Then Return 114
        If EERSTE_BOD = "Zn30Ar" Then Return 114
        If EERSTE_BOD = "Zn30g" Then Return 114
        If EERSTE_BOD = "Zn30r" Then Return 114
        If EERSTE_BOD = "Zn30v" Then Return 114
        If EERSTE_BOD = "Zn30x" Then Return 114
        If EERSTE_BOD = "Zn40A" Then Return 107
        If EERSTE_BOD = "Zn40Ap" Then Return 107
        If EERSTE_BOD = "Zn40Ar" Then Return 107
        If EERSTE_BOD = "Zn40Av" Then Return 107
        If EERSTE_BOD = "Zn50A" Then Return 107
        If EERSTE_BOD = "Zn50Ab" Then Return 107
        If EERSTE_BOD = "Zn50Ap" Then Return 107
        If EERSTE_BOD = "Zn50Ar" Then Return 107
        If EERSTE_BOD = "Zn50Aw" Then Return 107
        If EERSTE_BOD = "zpZn23w" Then Return 113
        If EERSTE_BOD = "zRd10A" Then Return 119
        If EERSTE_BOD = "zRn15C" Then Return 115
        If EERSTE_BOD = "zRn47Cwp" Then Return 117
        If EERSTE_BOD = "zRn62C" Then Return 119
        If EERSTE_BOD = "zSn14A" Then Return 113
        If EERSTE_BOD = "zVc" Then Return 105
        If EERSTE_BOD = "zVp" Then Return 105
        If EERSTE_BOD = "zVpg" Then Return 105
        If EERSTE_BOD = "zVpt" Then Return 105
        If EERSTE_BOD = "zVpx" Then Return 105
        If EERSTE_BOD = "zVs" Then Return 105
        If EERSTE_BOD = "zVz" Then Return 105
        If EERSTE_BOD = "zVzg" Then Return 105
        If EERSTE_BOD = "zVzt" Then Return 105
        If EERSTE_BOD = "zVzx" Then Return 105
        If EERSTE_BOD = "zWp" Then Return 105
        If EERSTE_BOD = "zWpg" Then Return 105
        If EERSTE_BOD = "zWpt" Then Return 105
        If EERSTE_BOD = "zWpx" Then Return 105
        If EERSTE_BOD = "zWz" Then Return 105
        If EERSTE_BOD = "zWzg" Then Return 105
        If EERSTE_BOD = "zWzt" Then Return 105
        If EERSTE_BOD = "zWzx" Then Return 105
        If EERSTE_BOD = "zY21" Then Return 108
        If EERSTE_BOD = "zY21g" Then Return 108
        If EERSTE_BOD = "zY23" Then Return 109
        If EERSTE_BOD = "zY30" Then Return 114
        Return 0

    End Function

    Friend Function ParseSobekTable(ByRef myRecord As String) As clsSobekTable

        Dim tableString As String = Me.ParseTable(myRecord)

        Dim myTable As New clsSobekTable(Me.setup)
        myTable.Read(tableString)
        Return myTable

    End Function

    Friend Function ParseSobekTableVariant(ByRef myRecord As String) As clsSobekTableVariant

        Dim tableString As String = Me.ParseTable(myRecord)

        Dim myTable As New clsSobekTableVariant(Me.setup)
        myTable.Read(tableString)
        Return myTable

    End Function

    'Requires reference to MapWinGIS
    'mwSourceGrid is already instantiated and opened from an existing grid file
    'SourceGrid is dimensioned as Dim SourceGrid(MaxCol, MaxRow) as Float
    Public Function ArrayFromMapWindowGrid(ByRef mwSourceGrid As MapWinGIS.Grid) As Single(,)
        Dim m_mrow As Integer, m_mcol As Integer
        Dim row, col As Integer
        Dim vals() As Single
        m_mrow = mwSourceGrid.Header.NumberRows - 1
        m_mcol = mwSourceGrid.Header.NumberCols - 1
        Dim SourceGrid(m_mcol, m_mrow) As Single
        For row = 0 To m_mrow
            ReDim vals(m_mcol)
            mwSourceGrid.GetRow(row, vals(0))
            For col = 0 To m_mcol
                SourceGrid(col, row) = vals(col)
            Next
        Next
        Return SourceGrid
    End Function

    <SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
    Public Sub ReleaseComObject(ByVal obj As Object, ByVal collect As Boolean)
        'Me.setup.Log.AddDebugMessage("Releasing " & obj.GetType.FullName & " Collect is " & collect)

        While True
            If (Marshal.ReleaseComObject(obj) <= 0) Then
                Exit While
            End If
        End While

        If collect Then
            'Me.setup.Log.AddMessage("Memory used before collection: " & GC.GetTotalMemory(False))
            GC.Collect()
        End If

        'Dim numBytes As Long = GC.GetTotalMemory(True)
        'Me.setup.Log.AddMessage("Memory used after releasing: " & Math.Round(numBytes / 1024 / 1024, 1).ToString())

    End Sub

    Public Function ShellandWait(ByVal ProcessPath As String, ByVal args As String) As Boolean
        Try
            Dim objProcess As System.Diagnostics.Process
            objProcess = New System.Diagnostics.Process()
            objProcess.StartInfo.FileName = ProcessPath
            objProcess.StartInfo.Arguments = args
            objProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal
            objProcess.Start()
            'Wait until the process passes back an exit code 
            'While Not objProcess.HasExited
            '    System.Threading.Thread.Sleep(1000)
            'End While
            objProcess.WaitForExit()
            Return True
        Catch ex As Exception
            Console.WriteLine("Error running process" & ProcessPath)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function FileInUse(ByVal sFile As String) As Boolean
        Dim thisFileInUse As Boolean = False
        If System.IO.File.Exists(sFile) Then
            Try
                Using f As New IO.FileStream(sFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                    ' thisFileInUse = False
                End Using
            Catch
                thisFileInUse = True
            End Try
        End If
        Return thisFileInUse
    End Function

    Public Sub ControlProcesses(ByRef ProcessCollection As List(Of System.Diagnostics.Process), ByVal MaxSimultaneously As Integer, ByVal WaitMilliseconds As Integer, ByVal SafetyValveMiliseconds As Integer)
        Dim mySW As New System.Diagnostics.Stopwatch
        Dim k As Long

        mySW.Start()
        Dim nUnfinished As Integer = 1000
        While nUnfinished > 0
            nUnfinished = 0
            For k = 0 To ProcessCollection.Count - 1
                If Not ProcessCollection(k).HasExited Then nUnfinished += 1
            Next
            If nUnfinished >= MaxSimultaneously Then setup.GeneralFunctions.Wait(WaitMilliseconds)
            If mySW.ElapsedMilliseconds > SafetyValveMiliseconds Then nUnfinished = 0 'safety valve
        End While
        mySW.Stop()
    End Sub

    Public Function calChannelElevationPoint(ByVal ChanBedWidth As Double, ByVal ChanSlope As Double, ByVal ChanDepth As Double, ByVal mv As Double, ByVal Dist As Double, ByRef Inside As Boolean) As Double
        'returns the elevation level inside an ideal channel, given the distance from its center line
        'note: channelslope is defined as horizontal/vertical
        'the boolean Inside is meant to distinquish points that fall inside the channel (computed level < specified surface level) or outside
        Dim bl As Double = mv - ChanDepth 'bed level
        Dim cl As Double
        Dim OnSlopeDist As Double = Dist - ChanBedWidth / 2

        If OnSlopeDist <= 0 Then
            Inside = True
            Return bl
        Else
            'calculate the designed channel level and compare it to the specified surface level
            cl = bl + OnSlopeDist / ChanSlope
            If cl > mv Then
                'computed level exceeds specified surface level so we're outside the channel. return the surface level
                Inside = False
                Return mv
            Else
                'computed level underceeds specified surface level, so we're inside the channel. Return the computed value
                Inside = True
                Return cl
            End If
        End If

    End Function

    Public Shared Function LastDirFromDir(ByVal myPath As String)
        Dim Dirs As String()
        If Right(myPath, 1) = "\" Then myPath = Left(myPath, myPath.Length - 1)
        Dirs = Split(myPath, "\")
        Return Dirs(Dirs.Count - 1)
    End Function

    Public Shared Sub DeleteAllDirectoryContents(ByVal myDir As String)

        'first remove all files in the current dir
        Dim myFile As String
        For Each myFile In Directory.GetFiles(myDir)
            File.Delete(myFile)
        Next

        'then remove all files in the current dir
        Dim mySubDir As String
        For Each mySubDir In Directory.GetDirectories(myDir)
            Directory.Delete(mySubDir, True)
        Next

    End Sub

    Public Shared Sub DirectoryCopy(ByVal sourceDirName As String, ByVal destDirName As String, ByVal copySubDirs As Boolean)

        ' Get the subdirectories for the specified directory. 
        Dim dir As DirectoryInfo = New DirectoryInfo(sourceDirName)
        Dim dirs As DirectoryInfo() = dir.GetDirectories()

        If Not dir.Exists Then
            Throw New DirectoryNotFoundException(
                "Source directory does not exist or could not be found: " _
                + sourceDirName)
        End If

        ' If the destination directory doesn't exist, create it. 
        If Not Directory.Exists(destDirName) Then
            Directory.CreateDirectory(destDirName)
        End If
        ' Get the files in the directory and copy them to the new location. 
        Dim files As FileInfo() = dir.GetFiles()
        For Each file In files
            Dim temppath As String = Path.Combine(destDirName, file.Name)
            file.CopyTo(temppath, False)
        Next file

        ' If copying subdirectories, copy them and their contents to new location. 
        If copySubDirs Then
            For Each subdir In dirs
                Dim temppath As String = Path.Combine(destDirName, subdir.Name)
                DirectoryCopy(subdir.FullName, temppath, copySubDirs)
            Next subdir
        End If
    End Sub

    Public Shared Function ReplaceSectionInFile(ByVal filename As String, ByVal HeadString As String, ByVal TailString As String, ByVal ReplaceString As String, ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod) As Boolean
        Try
            Dim myContent As String
            Dim HeadPos As Long = 0, TailPos As Long = 1
            Dim LeftSection As String, RightSection As String

            'read the file content and replace the string
            Using myReader As New StreamReader(filename)
                myContent = myReader.ReadToEnd()

                While TailPos > HeadPos
                    HeadPos = InStr(HeadPos + 1, myContent, HeadString, CompareMethod)
                    TailPos = InStr(HeadPos + 1, myContent, TailString, CompareMethod)

                    If HeadPos = 0 OrElse TailPos = 0 Then
                        'we're done here. No match found
                        HeadPos = TailPos
                    ElseIf HeadPos < TailPos AndAlso HeadPos > 0 Then
                        'match found!
                        LeftSection = Left(myContent, HeadPos - 1)
                        RightSection = Right(myContent, myContent.Length - TailPos - TailString.Length)
                        myContent = LeftSection & ReplaceString & RightSection

                        'reset the search area for the next round
                        HeadPos = TailPos
                        TailPos = TailPos + 1
                    End If

                End While


            End Using

            'write the file
            Using myWriter As New StreamWriter(filename)
                myWriter.Write(myContent)
            End Using

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function ReplaceSectionInLine(ByVal filename As String, ByVal HeadString As String, ByVal TailString As String, ByVal ReplaceString As String, ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod) As Boolean
        Try
            Dim myLine As String
            Dim HeadPos As Long = 0, TailPos As Long = 1
            Dim LeftSection As String, RightSection As String
            Dim newContent As String = ""

            'read the file content LINE BY LINE and replace the string
            Using myReader As New StreamReader(filename)
                myLine = myReader.ReadLine

                While TailPos > HeadPos
                    HeadPos = InStr(HeadPos + 1, myLine, HeadString, CompareMethod)
                    TailPos = InStr(HeadPos + 1, myLine, TailString, CompareMethod)
                    If HeadPos = 0 OrElse TailPos = 0 Then
                        'we're done here. No match found
                        HeadPos = TailPos
                    ElseIf HeadPos < TailPos AndAlso HeadPos > 0 Then
                        'match found!
                        LeftSection = Left(myLine, HeadPos - 1)
                        RightSection = Right(myLine, myLine.Length - TailPos - TailString.Length)
                        myLine = LeftSection & ReplaceString & RightSection

                        'reset the search area for the next round
                        HeadPos = TailPos
                        TailPos = TailPos + 1
                    End If
                End While

                'add the line to the new content
                If newContent = "" Then
                    newContent = myLine
                Else
                    newContent &= vbCrLf & myLine
                End If

            End Using

            'write the file
            Using myWriter As New StreamWriter(filename)
                myWriter.WriteLine(newContent)
            End Using

        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function GetRowFrom2DArrayOfByte(ByRef Array2D As Byte(,), RowIdx As Integer) As Byte()
        Dim i As Integer
        Dim Result As Byte()
        ReDim Result(Array2D.GetLength(1) - 1)
        For i = 0 To Array2D.GetLength(1) - 1
            Result(i) = Array2D(RowIdx, i)
        Next
        Return Result
    End Function


    Public Function CharCodeBytesToString(ByVal bytes() As Byte, SkipSpaces As Boolean) As String
        Dim Result As String = ""
        For Each myByte As Byte In bytes
            If Not SkipSpaces OrElse Not myByte = 32 Then
                Result &= Chr(myByte)
            End If
        Next
        Return Result
    End Function

    Public Shared Function ReplaceStringInFile(ByVal FileName As String, ByVal ReplaceStr As String, ByVal ReplaceByStr As String, ByVal CompareMethod As Microsoft.VisualBasic.CompareMethod) As Boolean
        Try
            Dim myContent As String

            'read the file content and replace the string
            Using myReader As New StreamReader(FileName)
                myContent = myReader.ReadToEnd()
                myContent = Replace(myContent, ReplaceStr, ReplaceByStr, , , CompareMethod)
            End Using

            'write the file
            Using myWriter As New StreamWriter(FileName)
                myWriter.Write(myContent)
            End Using

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function DirectoryRelativeToRoot(ByVal Path As String) As String
        Dim Root As String = Directory.GetDirectoryRoot(Path)
        Path = Replace(Path, Root, "")
        Return Path
    End Function

    Public Sub PopulateDataGridFromAccess(ByRef myGrid As Forms.DataGridView, ByVal myPath As String, ByVal query As String)
        Dim cn As New SQLite.SQLiteConnection
        Dim da As SQLite.SQLiteDataAdapter
        Dim dt As New DataTable

        cn.ConnectionString = "Data Source=" & myPath & ";Version=3;"
        cn.Open()

        'populate the grid containing summer volumes
        da = New SQLite.SQLiteDataAdapter(query, cn)
        da.Fill(dt)
        myGrid.DataSource = dt

        cn.Close()
    End Sub


    Public Sub QuerySQLiteDataBase(ByVal myPath As String, ByVal myTable As String, ByVal queries As List(Of String))
        Dim cn As New SQLite.SQLiteConnection
        Dim da As SQLite.SQLiteDataAdapter
        Dim dt As New DataTable
        Dim ds As New DataSet
        Dim myQuery As String

        cn.ConnectionString = "Data Source=" & myPath & ";Version=3;"
        cn.Open()

        'execute the query

        'populate the grid containing summer volumes
        For Each myQuery In queries
            da = New SQLite.SQLiteDataAdapter(myQuery, cn)
            da.Update(ds)
        Next

        cn.Close()
    End Sub
    Public Function PopulateGridExtentTextfieldsFromExistingGrid(Path As String, ByRef txtxmin As System.Windows.Forms.TextBox, txtymin As System.Windows.Forms.TextBox, txtxmax As System.Windows.Forms.TextBox, txtymax As System.Windows.Forms.TextBox) As Boolean
        Try
            Dim myGrid As New clsRaster(Me.setup, Path)
            myGrid.ReadHeader(MapWinGIS.GridDataType.UnknownDataType, True)
            myGrid.CompleteMetaHeader()
            txtxmin.Text = myGrid.XLLCorner
            txtxmax.Text = myGrid.XURCorner
            txtymin.Text = myGrid.YLLCorner
            txtymax.Text = myGrid.YURCorner
        Catch ex As Exception

        End Try
    End Function


    Public Sub UpdateObservedDataTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table OBSERVEDDATA", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "OBSERVEDDATA") Then SQLiteCreateTable(Me.setup.SqliteCon, "OBSERVEDDATA")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "OBSERVEDDATA", "SERIESID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "OBSERVEDDATA", "SERIESID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "OBSERVEDDATA_SERIESIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "OBSERVEDDATA", "LOCATIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "OBSERVEDDATA", "LOCATIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "OBSERVEDDATA_LOCIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "OBSERVEDDATA", "PARAMETER") Then SQLiteCreateColumn(Me.setup.SqliteCon, "OBSERVEDDATA", "PARAMETER", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "OBSERVEDDATA_PARIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "OBSERVEDDATA", "DATEANDTIME") Then SQLiteCreateColumn(Me.setup.SqliteCon, "OBSERVEDDATA", "DATEANDTIME", GeneralFunctions.enmOleDBDataType.SQLDATE, "OBSERVEDDATA_DATEIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "OBSERVEDDATA", "SOURCE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "OBSERVEDDATA", "SOURCE", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "OBSERVEDDATA_SRCIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "OBSERVEDDATA", "DATAVALUE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "OBSERVEDDATA", "DATAVALUE", GeneralFunctions.enmOleDBDataType.SQLDOUBLE)
    End Sub

    Public Sub UpdateEmergencyPumpDataTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table EMERGENCYPUMPTABLES", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES") Then SQLiteCreateTable(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "SERIESID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "SERIESID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "EMERGENCYPUMPTABLES_SERIESIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "OBJECTID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "LOCATIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "EMERGENCYPUMPTABLES_LOCIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "PARAMETER") Then SQLiteCreateColumn(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "PARAMETER", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "EMERGENCYPUMPTABLES_PARIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "DATEANDTIME") Then SQLiteCreateColumn(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "DATEANDTIME", GeneralFunctions.enmOleDBDataType.SQLDATE, "EMERGENCYPUMPTABLES_DATEIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "DATAVALUE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "EMERGENCYPUMPTABLES", "DATAVALUE", GeneralFunctions.enmOleDBDataType.SQLDOUBLE)
    End Sub


    Public Sub UpdateSensitivityRunsTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table SENSITIVITYRUNS", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "SENSITIVITYRUNS") Then SQLiteCreateTable(Me.setup.SqliteCon, "SENSITIVITYRUNS")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYRUNS", "SESSIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYRUNS", "SESSIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "SENSRUNS_SESSIONIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYRUNS", "TOKEN") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYRUNS", "TOKEN", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "SENSRUNS_TOKENIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYRUNS", "STEPIDX") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYRUNS", "STEPIDX", GeneralFunctions.enmOleDBDataType.SQLINT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYRUNS", "RUNID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYRUNS", "RUNID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
    End Sub


    Public Sub UpdateMonteCarloRunsTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table MONTECARLORUNS", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "MONTECARLORUNS") Then SQLiteCreateTable(Me.setup.SqliteCon, "MONTECARLORUNS")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLORUNS", "SESSIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLORUNS", "SESSIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "MCRUNS_SESSIONIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLORUNS", "RUNID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLORUNS", "RUNID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLORUNS", "TOKEN") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLORUNS", "TOKEN", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLORUNS", "OPERATOR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLORUNS", "OPERATOR", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLORUNS", "OPERAND") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLORUNS", "OPERAND", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
    End Sub

    Public Sub UpdateSensitivityAnalysisTable(progressPercentage As Integer)
        UpdateProgressBar("Updating table SENSITIVITYANALYSIS", progressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "SENSITIVITYANALYSIS") Then SQLiteCreateTable(Me.setup.SqliteCon, "SENSITIVITYANALYSIS")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "SESSIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "SESSIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "SENSEAN_SESSIONIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "RUNID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "RUNID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "SENSEAN_RUNIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "SERIESID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "SERIESID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "SENSEAN_SERIESIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "DATEANDTIME") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "DATEANDTIME", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "DATAVALUE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SENSITIVITYANALYSIS", "DATAVALUE", GeneralFunctions.enmOleDBDataType.SQLDOUBLE)
    End Sub

    Public Sub UpdateMonteCarloSamplingTable(progressPercentage As Integer)
        UpdateProgressBar("Updating table MONTECARLOSAMPLING", progressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "MONTECARLOSAMPLING") Then SQLiteCreateTable(Me.setup.SqliteCon, "MONTECARLOSAMPLING")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "SESSIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "SESSIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "MONTECARLO_SESSIONIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "RUNID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "RUNID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "MONTECARLO_RUNIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "SERIESID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "SERIESID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "MONTECARLO_SERIESIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "DATEANDTIME") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "DATEANDTIME", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "DATAVALUE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MONTECARLOSAMPLING", "DATAVALUE", GeneralFunctions.enmOleDBDataType.SQLDOUBLE)
    End Sub


    Public Sub UpdateAutokalSettingsTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table AUTOKALSETTINGS", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "AUTOKALSETTINGS") Then SQLiteCreateTable(Me.setup.SqliteCon, "AUTOKALSETTINGS")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "SESSIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "SESSIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "AUTOKAL_SESSIONIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "DATEFROM") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "DATEFROM", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "DATETO") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "DATETO", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "TEMPLATEFILESDIR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "TEMPLATEFILESDIR", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "RESULTSDIR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "RESULTSDIR", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "MAXSIMULATIONS") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "MAXSIMULATIONS", GeneralFunctions.enmOleDBDataType.SQLINT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "MAXPARALLEL") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "MAXPARALLEL", GeneralFunctions.enmOleDBDataType.SQLINT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "METHOD") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "METHOD", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "MODELID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "MODELID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "AUTOKALSETTINGS", "ASSESSMENT") Then SQLiteCreateColumn(Me.setup.SqliteCon, "AUTOKALSETTINGS", "ASSESSMENT", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
    End Sub


    Public Sub UpdateLocationsTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table LOCATIONS", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "LOCATIONS") Then SQLiteCreateTable(Me.setup.SqliteCon, "LOCATIONS")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "LOCATIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "LOCATIONID", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "LOCATIONS_LOCIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "PARAMETER") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "PARAMETER", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "LOCATIONS_PARIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "X") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "X", GeneralFunctions.enmOleDBDataType.SQLDOUBLE)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "Y") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "Y", GeneralFunctions.enmOleDBDataType.SQLDOUBLE)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "HISFILE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "HISFILE", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "HISPAR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "HISPAR", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "LOCATIONS", "HISLOC") Then SQLiteCreateColumn(Me.setup.SqliteCon, "LOCATIONS", "HISLOC", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255)
    End Sub
    Public Sub UpdateMapLayersTable(ProgressPercentage As Integer)
        UpdateProgressBar("Updating table MAPLAYERS", ProgressPercentage, 100, True)
        If Not Me.setup.GeneralFunctions.SQLiteTableExists(Me.setup.SqliteCon, "MAPLAYERS") Then Me.setup.GeneralFunctions.SQLiteCreateTable(Me.setup.SqliteCon, "MAPLAYERS")
        If Not Me.setup.GeneralFunctions.SQLiteColumnExists(Me.setup.SqliteCon, "MAPLAYERS", "PATH") Then Me.setup.GeneralFunctions.SQLiteCreateColumn(Me.setup.SqliteCon, "MAPLAYERS", "PATH", GeneralFunctions.enmOleDBDataType.SQLVARCHAR255, "MAPLAYERS_MAPIDX")
    End Sub

    Public Sub UpdateModelSeriesSetupTable(ProgressPercentage As Integer)
        'stelt de tabel samen waarin de tijdreeksen voor modelresultaten worden samengesteld
        'alle rijen die samen tot één SERIESID behoren worden opgeteld
        UpdateProgressBar("Updating table MODELSERIESSETUP", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "MODELSERIESSETUP") Then SQLiteCreateTable(Me.setup.SqliteCon, "MODELSERIESSETUP")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "SERIESID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "SERIESID", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "MODELID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "MODELID", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "RESULTSFILE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "RESULTSFILE", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "LOCATION") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "LOCATION", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "PARAMETER") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "PARAMETER", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "MULTIPLIER") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "MULTIPLIER", enmSQLiteDataType.SQLITEREAL)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "MODELSERIESSETUP", "PLOTCUMULATIVE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "MODELSERIESSETUP", "PLOTCUMULATIVE", enmSQLiteDataType.SQLITEINT)
    End Sub
    Public Sub UpdateSimulationModelsTable(ProgressPercentage As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE THE TABLE SIMULATIONMODELS IN THE DATABASE
        '------------------------------------------------------------------------------------
        UpdateProgressBar("Updating table SIMULATIONMODELS", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "SIMULATIONMODELS") Then SQLiteCreateTable(Me.setup.SqliteCon, "SIMULATIONMODELS")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "MODELID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "MODELID", enmSQLiteDataType.SQLITETEXT, "SIM_MODELIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "MODELTYPE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "MODELTYPE", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "EXECUTABLE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "EXECUTABLE", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "ARGUMENTS") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "ARGUMENTS", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "MODELDIR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "MODELDIR", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "CASENAME") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "CASENAME", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "SIMULATIONMODELS", "TEMPWORKDIR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "SIMULATIONMODELS", "TEMPWORKDIR", enmSQLiteDataType.SQLITETEXT)
    End Sub


    Public Sub UpdateCalibrationParametersTable(ProgressPercentage As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE THE TABLE CALIBRATIONPARAMETERS IN THE DATABASE
        '------------------------------------------------------------------------------------
        UpdateProgressBar("Updating table CALIBRATIONPARAMETERS", ProgressPercentage, 100, True)
        If Not SQLiteTableExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS") Then SQLiteCreateTable(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS")


        'remove some old structures if present
        If SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "IDENTIFIER") Then SQLiteDropColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "IDENTIFIER")
        If SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "STEPSIZE") Then SQLiteDropColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "STEPSIZE")
        If SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "PARID") Then SQLiteDropColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "PARID")
        If SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "FILEID") Then SQLiteDropColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "FILEID")

        'add the required columns
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "SESSIONID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "SESSIONID", enmSQLiteDataType.SQLITETEXT, "CALIBRATIONPARAMETERS_SESSIONIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "MODELID") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "MODELID", enmSQLiteDataType.SQLITETEXT, "CALIBRATIONPARAMETERS_MODELIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "FILENAME") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "FILENAME", enmSQLiteDataType.SQLITETEXT, "CALIBRATIONPARAMETERS_FILENAMEIDX")
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "TOKEN") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "TOKEN", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "PARAMETERTYPE") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "PARAMETERTYPE", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "ADJUSTMENTMETHOD") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "ADJUSTMENTMETHOD", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "FROMVAL") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "FROMVAL", enmSQLiteDataType.SQLITEREAL)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "TOVAL") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "TOVAL", enmSQLiteDataType.SQLITEREAL)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "REFVAL") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "REFVAL", enmSQLiteDataType.SQLITEREAL)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "LOGARITHMIC") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "LOGARITHMIC", enmSQLiteDataType.SQLITEINT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "VALUESFROMLIST") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "VALUESFROMLIST", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "NSTEPS") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "NSTEPS", enmSQLiteDataType.SQLITEINT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "SMALLERTHANPAR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "SMALLERTHANPAR", enmSQLiteDataType.SQLITETEXT)
        If Not SQLiteColumnExists(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "GREATERTHANPAR") Then SQLiteCreateColumn(Me.setup.SqliteCon, "CALIBRATIONPARAMETERS", "GREATERTHANPAR", enmSQLiteDataType.SQLITETEXT)
    End Sub

    Public Shared Sub EnsureDirectoryPathExists(DirectoryPath As String)


        Dim directoryInfo As DirectoryInfo = New DirectoryInfo(DirectoryPath)

        ' Check if the directory exists
        If Not directoryInfo.Exists Then
            ' Create all directories and subdirectories in the specified path
            Directory.CreateDirectory(DirectoryPath)
        End If
    End Sub

End Class

