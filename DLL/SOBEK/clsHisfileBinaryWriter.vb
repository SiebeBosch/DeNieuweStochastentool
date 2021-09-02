Imports System.IO

Public Class clsHisfileBinaryWriter

    Public Sub Create(FileName As String, Locs() As String, Pars() As String, Tim() As DateTime, Vals(,,) As Double)
        Dim writeStream = New FileStream(FileName, FileMode.Create)
        Using writeBinary As New BinaryWriter(writeStream)
            WriteHeader(writeBinary, Pars, Locs, Tim)
        End Using

    End Sub

    Public Sub WriteHeader(ByRef writer As BinaryWriter, ByRef Pars() As String, ByRef Locs() As String, ByRef Tims() As Date)
        Dim mySpan As TimeSpan = New TimeSpan
        mySpan = Tims(1).Subtract(Tims(0))

        '120 characters for explanation
        writer.Write("SOBEK                                   Fixed data calculation points           TITLE :20170516_GOEREE-OVERFLAKKEE v10  ")

        '40 characters to set the startdate and time
        writer.Write("T0: ") 'first 4 characters empty
        writer.Write(Tims(0).Year) 'positions 5 through 8
        writer.Write(".")
        writer.Write(Format(Tims(0).Month, "00")) 'positions 9 and 10
        writer.Write(".")
        writer.Write(Format(Tims(0).Day, "00"))
        writer.Write(" ")
        writer.Write(Format(Tims(0).Hour, "00"))
        writer.Write(":")
        writer.Write(Format(Tims(0).Minute, "00"))
        writer.Write(":")
        writer.Write(Format(Tims(0).Second, "00"))
        writer.Write("  (scu=") 'seven empty characters so we reach position #30
        Dim tsSeconds As String = mySpan.TotalSeconds.ToString
        While tsSeconds.Length < 8
            tsSeconds = " " & tsSeconds
        End While
        writer.Write(tsSeconds) '8 positions for the timestep size
        writer.Write("s)")      '2 positions for the timestep unit + closing bracket

        'write the number of parameters and then the number of locations, both as Int32's
        writer.Write(Convert.ToInt32(Pars.Count))
        writer.Write(Convert.ToInt32(Locs.Count))

        'write all parameter names, each in 20 characters
        For Each Par As String In Pars
            While Par.Length < 20
                Par = " " & Par
            End While
            writer.Write(Par)
        Next

        'write all location names, each in 20 characters
        For Each Loc As String In Locs
            While Loc.Length < 20
                Loc = " " & Loc
            End While
            writer.Write(Loc)
        Next

        For i = 0 To Tims.Count - 1
            writer.Write(Convert.ToInt32(i))
        Next

    End Sub

    Public Sub WriteData(ByRef writer As BinaryWriter, ByRef Pars() As String, ByRef Locs() As String, ByRef Tims() As Date)
        For Each Tim As DateTime In Tims
            For Each loc As String In Locs
                For Each par As String In Pars

                Next
            Next
        Next
    End Sub

End Class
