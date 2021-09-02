Option Explicit On

Imports STOCHLIB.General

Public Class clsFrictionDatCRFRRecord

  Public ID As String 'ID
  Public nm As String 'name
  Public cs As String 'cross section definition ID
  Public ltysTable As clsSobekTable 'list of Y-values for the friction sections e.g. -100 -40 < -40 20 < 20 100 <
  Public ftysTable As clsSobekTable 'friction values in positive direction containing type and value  e.g. manning 0.08,0.12,0.08 = 1 0.08 < 1 0.12 < 1 0.08 <
  Public frysTable As clsSobekTable 'friction values in negative direction containing type and value  e.g. manning 0.08,0.12,0.08 = 1 0.08 < 1 0.12 < 1 0.08 <

  Friend InUse As Boolean

  Private record As String
  Private Setup As clsSetup

  Public Sub New(ByVal mySetup As clsSetup)
    Me.Setup = mySetup
    ltysTable = New clsSobekTable(Me.Setup)
    ftysTable = New clsSobekTable(Me.Setup)
    frysTable = New clsSobekTable(Me.Setup)
  End Sub

    Friend Sub Add(FromY As Double, ToY As Double, Value As Double)
        ltysTable.AddDataPair(2, FromY, ToY)
        ftysTable.AddDataPair(2, 1, Value,,,,,, True)   'we forceren hier een unieke key voor de records omdat ze allemaal met waarde 1 beginnen
        frysTable.AddDataPair(2, 1, Value,,,,,, True)   'we forceren hier een unieke key voor de records omdat ze allemaal met waarde 1 beginnen
    End Sub

    Friend Function Read(ByVal myRecord As String) As Boolean
        Try
            Dim myStr As String
            record = myRecord

            While Not myRecord = ""
                myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Select Case myStr
                    Case Is = "id"
                        ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "nm"
                        nm = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "cs"
                        cs = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "lt"
                        myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If myStr = "ys" Then
                            ltysTable = Me.Setup.GeneralFunctions.ParseSobekTable(myRecord)
                        End If
                    Case Is = "ft"
                        myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If myStr = "ys" Then
                            ftysTable = Me.Setup.GeneralFunctions.ParseSobekTable(myRecord)
                        End If
                    Case Is = "fr"
                        myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If myStr = "ys" Then
                            frysTable = Me.Setup.GeneralFunctions.ParseSobekTable(myRecord)
                        End If
                End Select
            End While
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Friend Sub Build()
    Dim i As Integer

    record = "CRFR id '" & ID & "' nm '" & nm & "' cs '" & cs & "'" & vbCrLf

    record &= "lt ys" & vbCrLf
    record &= "TBLE" & vbCrLf
    For i = 0 To ltysTable.XValues.Count - 1
      record &= ltysTable.XValues.Values(i) & " " & ltysTable.Values1.Values(i) & " <" & vbCrLf
    Next
    record &= "tble" & vbCrLf

    record &= "ft ys" & vbCrLf
    record &= "TBLE" & vbCrLf
    For i = 0 To ftysTable.XValues.Count - 1
      record &= ftysTable.XValues.Values(i) & " " & ftysTable.Values1.Values(i) & " <" & vbCrLf
    Next
    record &= "tble" & vbCrLf

    record &= "fr ys" & vbCrLf
    record &= "TBLE" & vbCrLf
    For i = 0 To frysTable.XValues.Count - 1
      record &= frysTable.XValues.Values(i) & " " & frysTable.Values1.Values(i) & " <" & vbCrLf
    Next
    record &= "tble crfr"

  End Sub

  Friend Sub write(ByRef datWriter As System.IO.StreamWriter, Optional ByVal IsGlobal As Boolean = False)
    Call build()
    If IsGlobal Then record = "GLFR " & record & " glfr"
    datWriter.WriteLine(record)
  End Sub

  Friend Function SetByFloodplain(ByRef myDef As clsProfileDefRecord, ByVal FricType As STOCHLIB.GeneralFunctions.enmFrictionType, ByVal LeftPlainVal As Double, ByVal MainChannelVal As Double, ByVal RightPlainVal As Double) As Boolean
    'Date: 30-5-2013
    'Author: Siebe Bosch
    'Description: identifies two floodplains from a profile definition and sets the corresponding friction data
    'Note: only works for a cross section profile type 10 (YZ-profile)
    Dim StartY As Double, LeftY As Double, RightY As Double, EndY As Double

    If myDef.ty = 10 Then

      ID = myDef.ID
      nm = myDef.ID
      cs = myDef.ID

      'first, identify the floodplain sections
      StartY = myDef.ltyzTable.XValues.Values(0)
      LeftY = myDef.FindLeftFloodPlainSection
      RightY = myDef.FindRightFloodPlainSection
      EndY = myDef.ltyzTable.XValues.Values(myDef.ltyzTable.XValues.Count - 1)

      'set the friction sections
      ltysTable.AddDataPair(2, StartY, LeftY)
      ltysTable.AddDataPair(2, LeftY, RightY)
      ltysTable.AddDataPair(2, RightY, EndY)

      'set the friction values for positive flow direction. Do'nt use AddDataPair here because of the repeated equal friction types (used as key)
      ftysTable.XValues.Add("1", FricType)
      ftysTable.Values1.Add("1", LeftPlainVal)
      ftysTable.XValues.Add("2", FricType)
      ftysTable.Values1.Add("2", MainChannelVal)
      ftysTable.XValues.Add("3", FricType)
      ftysTable.Values1.Add("3", RightPlainVal)

      'set the friction values for negative flow direction
      frysTable.XValues.Add("1", FricType)
      frysTable.Values1.Add("1", LeftPlainVal)
      frysTable.XValues.Add("2", FricType)
      frysTable.Values1.Add("2", MainChannelVal)
      frysTable.XValues.Add("3", FricType)
      frysTable.Values1.Add("3", RightPlainVal)
      Return True
    Else
      Me.Setup.Log.AddError("Error setting floodplain frictions for profile definitions " & myDef.ID & ". Profile must be of type YZ.")
      Return False
    End If


  End Function



End Class
