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

    Private Function NormalizeTokens(value As String, relaxed As Boolean) As String
        Dim sb As New StringBuilder()
        For Each token As String In Tokenize(value)
            If relaxed AndAlso SkipTokens.Contains(token) Then
                Continue For
            End If
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
        For Each ch As Char In value.ToLowerInvariant()
            If Char.IsLetterOrDigit(ch) Then
                current.Append(ch)
            ElseIf current.Length > 0 Then
                tokens.Add(current.ToString())
                current.Clear()
            End If
        Next

        If current.Length > 0 Then
            tokens.Add(current.ToString())
        End If

        Return tokens
    End Function
End Module
