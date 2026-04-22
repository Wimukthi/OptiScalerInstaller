Imports System.IO
Imports System.Text.Json
Imports System.Text.RegularExpressions

Public Class CompatibilityService
    ' Loads, caches, and parses the OptiScaler compatibility list.
    Private Shared ReadOnly DefaultListPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Compatibility-List.md")
    Private Shared ReadOnly CachePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScalerInstaller", "compatibility.json")

    Public Shared Function LoadCompatibilityList() As List(Of CompatibilityEntry)
        Dim cached As List(Of CompatibilityEntry) = TryLoadCache()
        If cached IsNot Nothing AndAlso cached.Count > 0 Then
            Return cached
        End If

        If File.Exists(DefaultListPath) Then
            Dim content As String = File.ReadAllText(DefaultListPath)
            Return ParseCompatibilityList(content)
        End If

        Return New List(Of CompatibilityEntry)()
    End Function

    Public Shared Async Function UpdateCompatibilityListAsync() As Task(Of List(Of CompatibilityEntry))
        Dim result As CompatibilityUpdateResult = Await UpdateCompatibilityListWithDiffAsync()
        If result Is Nothing OrElse result.Entries Is Nothing Then
            Return New List(Of CompatibilityEntry)()
        End If

        Return result.Entries
    End Function

    Public Shared Async Function UpdateCompatibilityListWithDiffAsync() As Task(Of CompatibilityUpdateResult)
        Dim listUrl As String = GetCompatibilityListUrl()
        If String.IsNullOrWhiteSpace(listUrl) Then
            Throw New InvalidOperationException("Compatibility list URL is not set. Update it in Settings.")
        End If

        Dim previousEntries As List(Of CompatibilityEntry) = LoadCompatibilityList()
        Dim content As String = Await HttpClientHelper.GetStringWithRetryAsync(listUrl)
        Dim entries As List(Of CompatibilityEntry) = ParseCompatibilityList(content)

        If IsCountSuspicious(previousEntries.Count, entries.Count) Then
            ErrorLogger.LogMessage(
                $"Parsed {entries.Count} entries but expected at least {previousEntries.Count \ 2} (previous: {previousEntries.Count}). Cache preserved.",
                "", "CompatibilityService.UpdateValidation")
            Return BuildUpdateResult(previousEntries, previousEntries)
        End If

        SaveCache(entries)
        Return BuildUpdateResult(previousEntries, entries)
    End Function

    Private Shared Function GetCompatibilityListUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.CompatibilityListUrl
    End Function

    Private Shared Function TryLoadCache() As List(Of CompatibilityEntry)
        If Not File.Exists(CachePath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(CachePath)
            Dim entries As List(Of CompatibilityEntry) = JsonSerializer.Deserialize(Of List(Of CompatibilityEntry))(json)
            Return entries
        Catch ex As Exception
            ErrorLogger.Log(ex, "CompatibilityService.TryLoadCache")
            Return Nothing
        End Try
    End Function

    Private Shared Sub SaveCache(entries As List(Of CompatibilityEntry))
        Dim dir As String = Path.GetDirectoryName(CachePath)
        If Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If

        Dim json As String = JsonSerializer.Serialize(entries, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(CachePath, json)
    End Sub

    Public Shared Function ParseCompatibilityList(content As String) As List(Of CompatibilityEntry)
        ' Handles both table format (remote wiki) and standalone link format (bundled file).
        ' Only extracts game links from the name column/position to avoid cross-references.
        Dim entries As New List(Of CompatibilityEntry)()
        Dim linkRegex As New Regex("\[(.*?)\]\((.*?)\)")

        For Each rawLine As String In content.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
            Dim line As String = rawLine.Trim()
            If String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If

            ' Determine which part of the line holds the game name.
            Dim nameCell As String

            If line.StartsWith("|", StringComparison.Ordinal) Then
                ' Table row: only search for a game link in the first data column.
                If IsTableHeaderOrSeparator(line) Then
                    Continue For
                End If

                Dim columns As String() = line.Split("|"c)
                If columns.Length < 2 Then
                    Continue For
                End If

                nameCell = columns(1).Trim()
            ElseIf line.StartsWith("[", StringComparison.Ordinal) Then
                ' Standalone markdown link (bundled file format).
                nameCell = line
            Else
                ' Skip prose, headings, blockquotes, and other non-entry lines.
                Continue For
            End If

            If String.IsNullOrWhiteSpace(nameCell) Then
                Continue For
            End If

            Dim match As Match = linkRegex.Match(nameCell)
            If Not match.Success Then
                Continue For
            End If

            Dim name As String = match.Groups(1).Value.Trim()
            Dim slug As String = match.Groups(2).Value.Trim()

            If String.IsNullOrWhiteSpace(name) OrElse String.IsNullOrWhiteSpace(slug) Then
                Continue For
            End If

            ' Skip names that are too short to be real game titles.
            If name.Length <= 2 Then
                Continue For
            End If

            If slug.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            ' Slugs containing '#' are anchor or section references, not game pages.
            If slug.Contains("#"c) Then
                Continue For
            End If

            If name.Equals("Template", StringComparison.OrdinalIgnoreCase) OrElse slug.Equals("CL-Template", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            entries.Add(New CompatibilityEntry With {.Name = name, .Slug = slug})
        Next

        Return entries
    End Function

    Private Shared Function BuildUpdateResult(previousEntries As List(Of CompatibilityEntry),
                                              newEntries As List(Of CompatibilityEntry)) As CompatibilityUpdateResult
        If previousEntries Is Nothing Then
            previousEntries = New List(Of CompatibilityEntry)()
        End If
        If newEntries Is Nothing Then
            newEntries = New List(Of CompatibilityEntry)()
        End If

        Dim previousByName As New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
        For Each entry As CompatibilityEntry In previousEntries
            If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                Continue For
            End If

            If Not previousByName.ContainsKey(entry.Name) Then
                previousByName(entry.Name) = entry
            End If
        Next

        Dim newByName As New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
        For Each entry As CompatibilityEntry In newEntries
            If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                Continue For
            End If

            If Not newByName.ContainsKey(entry.Name) Then
                newByName(entry.Name) = entry
            End If
        Next

        Dim added As New List(Of String)()
        Dim removed As New List(Of String)()
        Dim changed As New List(Of String)()

        For Each kvp As KeyValuePair(Of String, CompatibilityEntry) In newByName
            If Not previousByName.ContainsKey(kvp.Key) Then
                added.Add(kvp.Key)
                Continue For
            End If

            Dim oldEntry As CompatibilityEntry = previousByName(kvp.Key)
            If oldEntry Is Nothing Then
                Continue For
            End If

            If Not String.Equals(oldEntry.Slug, kvp.Value.Slug, StringComparison.OrdinalIgnoreCase) Then
                changed.Add(kvp.Key)
            End If
        Next

        For Each kvp As KeyValuePair(Of String, CompatibilityEntry) In previousByName
            If Not newByName.ContainsKey(kvp.Key) Then
                removed.Add(kvp.Key)
            End If
        Next

        added.Sort(StringComparer.OrdinalIgnoreCase)
        removed.Sort(StringComparer.OrdinalIgnoreCase)
        changed.Sort(StringComparer.OrdinalIgnoreCase)

        Return New CompatibilityUpdateResult With {
            .Entries = newEntries,
            .AddedNames = added,
            .RemovedNames = removed,
            .ChangedNames = changed
        }
    End Function

    Private Shared Function IsTableHeaderOrSeparator(line As String) As Boolean
        Return line.StartsWith("| Game ", StringComparison.OrdinalIgnoreCase) OrElse
               line.StartsWith("| GAME", StringComparison.OrdinalIgnoreCase) OrElse
               line.StartsWith("| ----", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function IsCountSuspicious(previousCount As Integer, newCount As Integer) As Boolean
        If previousCount < 20 Then
            Return False
        End If
        Return newCount < previousCount \ 2
    End Function
End Class
