Option Explicit On

Imports STOCHLIB.General

Friend Class clsInitialDatRecord

  Friend ID As String
  Friend Name As String
  Friend ss As Double
  Friend ci As String 'carrier ID
  Friend lc As Double 'location
  Friend q_lq As Integer  'initial discharge: 0 = const, 2 = function of location on branch
  Friend q_lqConstant As Double 'constant initial discharge
  Friend ty As enmInitialType '1 water level, 0 water depth
  Friend lvll As Integer 'initial level/depth: 0 = const, 2 = function of location on branch
  Friend lvllConstant As Double 'constant initial level/depth
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub
  Friend Enum enmInitialType
    WaterDepth = 0
    WaterLevel = 1
    GlobalValue = 99
  End Enum
  Friend Sub Read(ByVal myRecord As String)
    Dim Done As Boolean, myStr As String, tmp As String
    Done = False

    While Not Done
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case myStr
        Case Is = "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "nm"
          Name = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "ci"
          ci = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "ss"
          ss = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "lc"
          lc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "q_"
          tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          If tmp = "lq" Then
            'de constante waarde zit in de tweede waarde
            q_lq = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            q_lqConstant = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          End If
        Case Is = "ty"
          ty = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "lv"
          tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          If tmp = "ll" Then
            lvll = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            lvllConstant = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          End If
        Case Is = ""
          Done = True
      End Select
    End While
  End Sub
End Class
