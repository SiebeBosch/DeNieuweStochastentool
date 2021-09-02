Public Class clsLanduseMap
    Public Source As GeneralFunctions.enmLanduseSource
    Public FrictionTable As clsFrictionTable


    Public Sub New(ByRef mySource As GeneralFunctions.enmLanduseSource)
        Select Case mySource
            Case GeneralFunctions.enmLanduseSource.fysiek_voorkomen_2019
                PopulateFysiekVoorkomen2019()
        End Select
    End Sub

    Public Function PopulateFysiekVoorkomen2019() As Boolean
        Try
            FrictionTable = New clsFrictionTable(GeneralFunctions.enmLanduseSource.fysiek_voorkomen_2019, GeneralFunctions.enmFrictionType.MANNING)
            FrictionTable.AddRecord(1, "Dak", 0.01, 0.0115, 0.013, "https://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/stelprdb1044171.pdf")
            FrictionTable.AddRecord(2, "Zand", 0.01, 0.014, 0.016, "https://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/stelprdb1044171.pdf")
            FrictionTable.AddRecord(3, "Half verhard", 0.01, 0.015, 0.02, "")
            FrictionTable.AddRecord(4, "Erf", 0.03, 0.04, 0.05, "")
            FrictionTable.AddRecord(5, "Gesloten verharding", 0.01, 0.0115, 0.013, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(6, "Onverhard", 0.05, 0.05, 0.05, "")
            FrictionTable.AddRecord(7, "Open verharding", 0.01, 0.012, 0.013, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(8, "Groenvoorziening", 0.035, 0.0475, 0.07, "http://www.fsl.orst.edu/geowater/FX3/help/8_Hydraulic_Reference/Mannings_n_Tables.htm")
            FrictionTable.AddRecord(9, "Naaldbos", 0.05, 0.1, 0.15, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(10, "Gras", 0.05, 0.1, 0.15, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(11, "Grasland", 0.05, 0.1, 0.15, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(12, "Struiken", 0.4, 0.4, 0.4, "https://www.researchgate.net/figure/Mannings-n-values-used-for-NLCD-map_tbl2_324125024")
            FrictionTable.AddRecord(13, "Natuurterrein", 0.13, 0.13, 0.13, "https://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/stelprdb1044171.pdf")
            FrictionTable.AddRecord(14, "Boomteelt", 0.1, 0.1, 0.1, "https://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/stelprdb1044171.pdf")
            FrictionTable.AddRecord(15, "Duin", 0.01, (0.01 + 0.16) / 2, 0.16, "")
            FrictionTable.AddRecord(16, "Heide", 0.1, 0.14, 0.16, "")
            FrictionTable.AddRecord(17, "Bouwland", 0.06, 0.11, 0.16, "https://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/stelprdb1044171.pdf")
            FrictionTable.AddRecord(18, "Houtwal", 0.16, 0.16, 0.16, "http://www.fsl.orst.edu/geowater/FX3/help/8_Hydraulic_Reference/Mannings_n_Tables.htm")
            FrictionTable.AddRecord(19, "Loofbos", 0.05, 0.1, 0.2, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(20, "Rietland", 0.1, 0.25, 0.4, "")
            FrictionTable.AddRecord(21, "Fruitteelt", 0.1, 0.1, 0.1, "")
            FrictionTable.AddRecord(22, "Gemengd bos", 0.05, 0.1, 0.2, "https://pubs.usgs.gov/wsp/2339/report.pdf")
            FrictionTable.AddRecord(23, "Braakland", 0.1, 0.4, 0.7, "https://www.nrcs.usda.gov/Internet/FSE_DOCUMENTS/stelprdb1044171.pdf")
            FrictionTable.AddRecord(26, "Moeras", 0.1, 0.25, 0.4, "")
            FrictionTable.AddRecord(27, "Kwelder", 0.1, 0.25, 0.4, "")
            FrictionTable.AddRecord(28, "Waterberm", 0.04, 0.045, 0.05, "")
            FrictionTable.AddRecord(29, "Water", 0.016, (0.016 + 0.05) / 2, 0.05, "http://www.fsl.orst.edu/geowater/FX3/help/8_Hydraulic_Reference/Mannings_n_Tables.htm")
            FrictionTable.AddRecord(30, "Overige", 0.015, (0.015 + 0.05) / 2, 0.05, "")
            FrictionTable.AddRecord(253, "Onbekend", 0.015, (0.015 + 0.05) / 2, 0.05, "")
            FrictionTable.AddRecord(254, "Water", 0.016, (0.016 + 0.05) / 2, 0.05, "http://www.fsl.orst.edu/geowater/FX3/help/8_Hydraulic_Reference/Mannings_n_Tables.htm")
        Catch ex As Exception

        End Try
    End Function

End Class
