Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsSFAttributeData
    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        'TimeTables = New clsCFTimeTables(Me.Setup)          'bevat alle tijdsafhankelijke tabellen van de flow-module
        'LateralData = New clsCFLateraldata(Me.Setup, Me.SbkCase)        'bevat alle laterale data van de flowmodule (ook met storage)
        'ProfileData = New clsCFProfileData(Me.Setup, Me.SbkCase)        'bevat alle profieldata van de flowmodule
        'StructureData = New clsCFStructureData(Me.Setup, Me.SbkCase)    'bevat alle kunstwerkdata van de flowmodule
        'FrictionData = New clsCFFrictionData(Me.Setup, Me.SbkCase)      'bevat alle ruwheidsdata van de flowmodule
        'BoundaryData = New clsCFBoundaryData(Me.Setup, Me.SbkCase)      'bevat alle boundarydata van de flowmodule
        'NodesData = New clsCFNodesData(Me.Setup, Me.SbkCase)            'bevat alle data uit nodes.dat
        'InitialData = New clsCFInitialData(Me.Setup, Me.SbkCase)        'bevat alle data uit initial.dat
    End Sub

End Class
