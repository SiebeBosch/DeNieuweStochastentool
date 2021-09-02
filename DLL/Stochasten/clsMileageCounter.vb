Imports STOCHLIB.General
Imports System.Windows.Forms
Public Enum enmStochastType
    Volume = 0
    Pattern = 1
    Groundwater = 2
    WaterLevel = 3
    Wind = 4
    Extra1 = 5
    Extra2 = 6
    Extra3 = 7
    Extra4 = 8
End Enum

Public Class clsMileageCounter
    Friend startNums As New List(Of Integer)
    Friend endNums As New List(Of Integer)
    Friend currentVal As New List(Of Integer)
    Friend Stochasts As New List(Of enmStochastType)   'keeps track of which stochast is represented by each digit
    Dim Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = Setup
    End Sub

    Public Sub Initialize()
        'sets all current values of the mileage counter to startnum -1
        Dim i As Integer
        For i = 0 To startNums.Count - 1
            currentVal(i) = startNums(i) - 1
        Next
    End Sub

    Public Sub AddDigit(StartVal As Integer, EndVal As Integer, Optional ByVal myType As enmStochastType = enmStochastType.Pattern)
        'adds another digit to the mileage counter
        Stochasts.Add(myType)
        startNums.Add(StartVal)
        endNums.Add(EndVal)
        currentVal.Add(StartVal - 1) 'ever counter is initialized by starting one value below the starting value
    End Sub

    Public Function GetDigitValue(DigitIdx As Integer) As Integer
        Return currentVal(DigitIdx)
    End Function


    Public Function MileageOneUp() As Boolean
        'werkt als een kilometerteller. Als het hectometergetal boven z'n maximum komt, springt hij terug naar nul
        'en gaat het getalletje ervoor een omhoog et cetera. Produceert TRUE bij succes
        'produceert FALSE als hij aan z'n eind is gekomen en niet verder kan ophogen
        Dim i As Integer, j As Integer
        Dim Done As Boolean, ThisIsTheEnd As Boolean

        'errorhandling
        If startNums.Count <> endNums.Count OrElse startNums.Count <> currentVal.Count Then
            Me.Setup.Log.AddError("Error in function MileageOneUp: the arrays must have the same size.")
            Return False
        End If

        'check whether the current state is possible. If not we'll assume it needs to be initialized and return the very first value
        For i = 0 To currentVal.Count - 1
            If currentVal(i) < startNums(i) OrElse currentVal(i) > endNums(i) Then
                For j = 0 To currentVal.Count - 1
                    currentVal(j) = startNums(j)
                Next
                Return True
            End If
        Next

        'check whether the state is currently at its end. If so, return false
        ThisIsTheEnd = True
        For i = 0 To currentVal.Count - 1
            If currentVal(i) < endNums(i) Then
                ThisIsTheEnd = False
                Exit For
            End If
        Next
        If ThisIsTheEnd Then Return False

        'walk through the list of numbers and adjust the state
        Done = False
        i = currentVal.Count
        While Not Done
            i -= 1
            If i < 0 Then
                Done = True
            ElseIf currentVal(i) < endNums(i) Then
                currentVal(i) += 1
                Done = True
            Else
                currentVal(i) = startNums(i)
            End If
        End While
        Return True

    End Function


End Class
