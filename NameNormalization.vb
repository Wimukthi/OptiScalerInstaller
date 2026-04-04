Imports System.Text

Friend Module NameNormalization
    ' Normalizes titles for looser matching against compatibility entries.
    Private ReadOnly SkipTokens As HashSet(Of String) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "game", "of", "the", "year", "goty", "ultimate", "edition", "definitive", "remastered",
        "deluxe", "complete", "collector", "collectors", "director", "directors", "cut",
        "enhanced", "anniversary", "gold", "platinum", "bundle", "remake", "redux", "hd",
        "classic", "premium", "pack", "remaster"
    }

    Public Function NormalizeName(value As String) As String
        Return NormalizeTokens(value, False)
    End Function

    Public Function NormalizeRelaxedName(value As String) As String
        Return NormalizeTokens(value, True)
    End Function

    Public Function TokenizeRelaxed(value As String) As List(Of String)
        Dim tokens As List(Of String) = Tokenize(value)
        If tokens.Count = 0 Then
            Return tokens
        End If

        Dim filtered As New List(Of String)()
        For Each token As String In tokens
            If SkipTokens.Contains(token) Then
                Continue For
            End If
            filtered.Add(token)
        Next

        Return filtered
    End Function

    Private Function NormalizeTokens(value As String, relaxed As Boolean) As String
        Dim sb As New StringBuilder()
        Dim tokens As List(Of String) = If(relaxed, TokenizeRelaxed(value), Tokenize(value))
        For Each token As String In tokens
            sb.Append(token)
        Next
        Return sb.ToString()
    End Function

    Public Function Tokenize(value As String) As List(Of String)
        Dim tokens As New List(Of String)()
        If String.IsNullOrWhiteSpace(value) Then
            Return tokens
        End If

        Dim current As New StringBuilder()
        Dim previousKind As Integer = 0 ' 0=none, 1=letter, 2=digit
        For Each ch As Char In value.ToLowerInvariant()
            If ch = "'"c Then
                Continue For
            End If

            If Char.IsLetterOrDigit(ch) Then
                Dim kind As Integer = If(Char.IsDigit(ch), 2, 1)
                If current.Length > 0 AndAlso previousKind <> 0 AndAlso kind <> previousKind Then
                    tokens.Add(current.ToString())
                    current.Clear()
                End If
                current.Append(ch)
                previousKind = kind
            ElseIf current.Length > 0 Then
                tokens.Add(current.ToString())
                current.Clear()
                previousKind = 0
            Else
                previousKind = 0
            End If
        Next

        If current.Length > 0 Then
            tokens.Add(current.ToString())
        End If

        Return tokens
    End Function
End Module
