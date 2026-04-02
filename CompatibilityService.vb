Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.RegularExpressions

Public Class CompatibilityService
    ' Loads, caches, and parses the OptiScaler compatibility list.
    Private Shared ReadOnly DefaultListPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Compatibility-List.md")
    Private Shared ReadOnly CachePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScalerInstaller", "compatibility.json")
    Private Shared ReadOnly RequestTimeout As TimeSpan = TimeSpan.FromSeconds(30)
    Private Const MaxRequestAttempts As Integer = 3

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
        Using client As New HttpClient() With {.Timeout = RequestTimeout}
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Dim content As String = Await GetStringWithRetryAsync(client, listUrl)
            Dim entries As List(Of CompatibilityEntry) = ParseCompatibilityList(content)
            SaveCache(entries)
            Return BuildUpdateResult(previousEntries, entries)
        End Using
    End Function

    ' Retries transient failures so startup refreshes are less brittle.
    Private Shared Async Function GetStringWithRetryAsync(client As HttpClient, url As String) As Task(Of String)
        Dim delay As TimeSpan = TimeSpan.FromMilliseconds(500)

        For attempt As Integer = 1 To MaxRequestAttempts
            Dim retry As Boolean = False
            Try
                Return Await client.GetStringAsync(url)
            Catch ex As HttpRequestException
                If attempt < MaxRequestAttempts Then
                    retry = True
                Else
                    Throw
                End If
            Catch ex As TaskCanceledException
                If attempt < MaxRequestAttempts Then
                    retry = True
                Else
                    Throw
                End If
            End Try

            If retry Then
                Await Task.Delay(delay)
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2)
            End If
        Next

        Return Await client.GetStringAsync(url)
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
        ' Extract markdown link entries and ignore non-wiki URLs.
        Dim entries As New List(Of CompatibilityEntry)()
        Dim regex As New Regex("\[(.*?)\]\((.*?)\)")

        For Each rawLine As String In content.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
            Dim match As Match = regex.Match(rawLine)
            If Not match.Success Then
                Continue For
            End If

            Dim name As String = match.Groups(1).Value.Trim()
            Dim slug As String = match.Groups(2).Value.Trim()

            If String.IsNullOrWhiteSpace(name) OrElse String.IsNullOrWhiteSpace(slug) Then
                Continue For
            End If

            If slug.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
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
End Class
