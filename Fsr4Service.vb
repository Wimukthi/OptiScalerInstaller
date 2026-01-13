Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.RegularExpressions

Public Class Fsr4Service
    Private Shared ReadOnly CachePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScalerInstaller", "fsr4.json")

    Public Shared Function LoadFsr4Normalized() As HashSet(Of String)
        Dim cached As List(Of String) = TryLoadCache()
        If cached Is Nothing OrElse cached.Count = 0 Then
            Return New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        End If

        Return NormalizeNames(cached)
    End Function

    Public Shared Async Function UpdateFsr4NormalizedAsync() As Task(Of HashSet(Of String))
        Dim listUrl As String = GetFsr4ListUrl()
        If String.IsNullOrWhiteSpace(listUrl) Then
            Throw New InvalidOperationException("FSR4 list URL is not set. Update it in Settings.")
        End If

        Using client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Dim content As String = Await client.GetStringAsync(listUrl)
            Dim names As List(Of String) = ParseFsr4List(content)
            SaveCache(names)
            Return NormalizeNames(names)
        End Using
    End Function

    Private Shared Function GetFsr4ListUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.Fsr4ListUrl
    End Function

    Private Shared Function TryLoadCache() As List(Of String)
        If Not File.Exists(CachePath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(CachePath)
            Dim names As List(Of String) = JsonSerializer.Deserialize(Of List(Of String))(json)
            Return names
        Catch
            Return Nothing
        End Try
    End Function

    Private Shared Sub SaveCache(names As List(Of String))
        Dim dir As String = Path.GetDirectoryName(CachePath)
        If Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If

        Dim json As String = JsonSerializer.Serialize(names, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(CachePath, json)
    End Sub

    Public Shared Function ParseFsr4List(content As String) As List(Of String)
        Dim names As New List(Of String)()
        Dim lines As String() = content.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
        Dim inTable As Boolean = False
        Dim sawHeader As Boolean = False
        Dim inComment As Boolean = False

        For Each rawLine As String In lines
            Dim line As String = rawLine.Trim()
            If line.Contains("<!--") Then
                inComment = True
            End If

            If inComment Then
                If line.Contains("-->") Then
                    inComment = False
                End If
                Continue For
            End If

            If Not line.StartsWith("|", StringComparison.Ordinal) Then
                If inTable AndAlso sawHeader Then
                    inTable = False
                    sawHeader = False
                End If
                Continue For
            End If

            Dim cells As String() = line.Split("|"c)
            If cells.Length < 2 Then
                Continue For
            End If

            Dim firstCell As String = cells(1).Trim()
            If String.IsNullOrWhiteSpace(firstCell) Then
                Continue For
            End If

            Dim headerKey As String = NormalizeHeader(firstCell)
            If headerKey = "GAME" OrElse headerKey = "GAMENAME" Then
                inTable = True
                sawHeader = True
                Continue For
            End If

            If Not inTable Then
                Continue For
            End If

            If IsSeparatorRow(firstCell) Then
                Continue For
            End If

            Dim name As String = ExtractMarkdownLabel(firstCell)
            If String.IsNullOrWhiteSpace(name) Then
                Continue For
            End If

            names.Add(name)
        Next

        Return names
    End Function

    Private Shared Function NormalizeHeader(value As String) As String
        Dim lettersOnly As String = Regex.Replace(value, "[^A-Za-z]", "")
        Return lettersOnly.ToUpperInvariant()
    End Function

    Private Shared Function IsSeparatorRow(value As String) As Boolean
        Return Regex.IsMatch(value, "^[\s:\-]+$")
    End Function

    Private Shared Function ExtractMarkdownLabel(value As String) As String
        Dim linkMatch As Match = Regex.Match(value, "\[(.*?)\]")
        If linkMatch.Success Then
            Return linkMatch.Groups(1).Value.Trim()
        End If

        Dim cleaned As String = Regex.Replace(value, "[`*_]", "")
        Return cleaned.Trim()
    End Function

    Private Shared Function NormalizeNames(names As IEnumerable(Of String)) As HashSet(Of String)
        Dim normalizedSet As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each name As String In names
            Dim normalized As String = NameNormalization.NormalizeRelaxedName(name)
            If Not String.IsNullOrWhiteSpace(normalized) Then
                normalizedSet.Add(normalized)
            End If
        Next
        Return normalizedSet
    End Function
End Class
