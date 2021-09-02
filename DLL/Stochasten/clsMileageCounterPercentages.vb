Imports STOCHLIB.General

Public Class clsMileageCounterPercentages
    Friend percentages As New List(Of Integer)   'a list of all percentage classes that we support
    Friend nDigits As Integer

    Friend currentVal As New List(Of Integer)
    Friend Stochasts As New List(Of enmStochastType)   'keeps track of which stochast is represented by each digit
    Dim Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = Setup
    End Sub

    Public Sub Initialize()
        'initializes the currentVal array and
        'sets all current values of the mileage counter to the starting percentage -1
        Dim i As Integer
        For i = 0 To nDigits - 1
            currentVal.Add(percentages(0) - 1)
        Next
    End Sub

    Public Function GetDigitValue(DigitIdx As Integer) As Integer
        Return currentVal(DigitIdx)
    End Function


    Public Function MileageOneUp() As Boolean
        'werkt als een kilometerteller. Als het hectometergetal boven z'n maximum komt, springt hij terug naar nul
        'en gaat het getalletje ervoor een omhoog et cetera. Produceert TRUE bij succes
        'produceert FALSE als hij aan z'n eind is gekomen en niet verder kan ophogen
        Dim i As Integer, j As Integer, pIdx As Integer
        Dim StepsComplete As Boolean, ThisIsTheEnd As Boolean

        'check whether the current state is possible. If not we'll assume it needs to be initialized and return the very first value
        For i = 0 To currentVal.Count - 1
            If currentVal(i) < percentages(0) OrElse currentVal(i) > percentages(percentages.Count - 1) Then
                For j = 0 To currentVal.Count - 1
                    currentVal(j) = percentages(0)
                Next
                Return True
            End If
        Next

        'not all steps result in a valid sum of <= 100 for all values up to the last
        'therefore we will loop the loop until a valid sum Is found
        Dim SumValid As Boolean = False
        While Not SumValid

            'walk backwards through the list of numbers and adjust their state
            StepsComplete = False
            i = currentVal.Count - 1 'we skip the last one since that one has to close the gap
            While Not StepsComplete
                i -= 1
                If i < 0 Then
                    StepsComplete = True
                Else
                    'figure out which percentage class we're currently in
                    pIdx = percentages.IndexOf(currentVal(i))

                    If pIdx < percentages.Count - 1 Then
                        pIdx += 1
                        currentVal(i) = percentages(pIdx)
                        StepsComplete = True
                    Else
                        currentVal(i) = percentages(0)
                    End If
                End If
            End While

            'now that all values are set, we can move on to the last one and 'close the gap' (all values should add up to 100)
            Dim Sum As Double
            For i = 0 To currentVal.Count - 2
                Sum += currentVal(i)
            Next

            If Sum <= 100 Then
                currentVal(currentVal.Count - 1) = 100 - Sum
                SumValid = True
            End If

            'check whether the state is currently at its end. If so, return false
            ThisIsTheEnd = True
            For i = 0 To currentVal.Count - 1
                If currentVal(i) < percentages(percentages.Count - 1) Then
                    ThisIsTheEnd = False
                    Exit For
                End If
            Next
            If ThisIsTheEnd Then Return False

        End While



        Return True

    End Function
End Class
