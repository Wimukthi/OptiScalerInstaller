Imports System.IO
Imports System.Net.Http
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
        Dim listUrl As String = GetCompatibilityListUrl()
        If String.IsNullOrWhiteSpace(listUrl) Then
            Throw New InvalidOperationException("Compatibility list URL is not set. Update it in Settings.")
        End If

        Using client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Dim content As String = Await client.GetStringAsync(listUrl)
            Dim entries As List(Of CompatibilityEntry) = ParseCompatibilityList(content)
            SaveCache(entries)
            Return entries
        End Using
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
End Class
