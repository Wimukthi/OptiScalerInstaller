Imports System.IO
Imports System.Net.Http
Imports System.Text.Json

Public Class OptiPatcherSupportService
    ' Loads, caches, and parses OptiPatcher GameSupport markdown entries.
    Private Shared ReadOnly DefaultListPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "OptiPatcher-GameSupport.md")
    Private Shared ReadOnly CachePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScalerInstaller", "optipatcher-support.json")
    Private Shared ReadOnly RequestTimeout As TimeSpan = TimeSpan.FromSeconds(30)
    Private Const MaxRequestAttempts As Integer = 3

    Public Shared Function LoadSupportList() As List(Of OptiPatcherSupportEntry)
        Dim cached As List(Of OptiPatcherSupportEntry) = TryLoadCache()
        If cached IsNot Nothing AndAlso cached.Count > 0 Then
            Return cached
        End If

        If File.Exists(DefaultListPath) Then
            Try
                Dim content As String = File.ReadAllText(DefaultListPath)
                Return ParseSupportList(content)
            Catch ex As Exception
                ErrorLogger.Log(ex, "OptiPatcherSupportService.LoadSupportList.DefaultFile")
            End Try
        End If

        Return New List(Of OptiPatcherSupportEntry)()
    End Function

    Public Shared Async Function UpdateSupportListAsync() As Task(Of List(Of OptiPatcherSupportEntry))
        Dim listUrl As String = GetSupportListUrl()
        If String.IsNullOrWhiteSpace(listUrl) Then
            Throw New InvalidOperationException("OptiPatcher support list URL is not set. Update it in settings.")
        End If

        Using client As New HttpClient() With {.Timeout = RequestTimeout}
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Dim content As String = Await GetStringWithRetryAsync(client, listUrl)
            Dim entries As List(Of OptiPatcherSupportEntry) = ParseSupportList(content)
            SaveCache(entries)
            Return entries
        End Using
    End Function

    Public Shared Function BuildLookup(entries As IEnumerable(Of OptiPatcherSupportEntry)) As Dictionary(Of String, OptiPatcherSupportEntry)
        Dim map As New Dictionary(Of String, OptiPatcherSupportEntry)(StringComparer.OrdinalIgnoreCase)
        If entries Is Nothing Then
            Return map
        End If

        For Each entry As OptiPatcherSupportEntry In entries
            If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                Continue For
            End If

            Dim exactKey As String = entry.Name.Trim()
            If Not map.ContainsKey(exactKey) Then
                map(exactKey) = entry
            End If

            Dim normalized As String = NameNormalization.NormalizeName(entry.Name)
            If Not String.IsNullOrWhiteSpace(normalized) AndAlso Not map.ContainsKey(normalized) Then
                map(normalized) = entry
            End If

            Dim relaxed As String = NameNormalization.NormalizeRelaxedName(entry.Name)
            If Not String.IsNullOrWhiteSpace(relaxed) AndAlso Not map.ContainsKey(relaxed) Then
                map(relaxed) = entry
            End If
        Next

        Return map
    End Function

    Public Shared Function FindByGameName(lookup As Dictionary(Of String, OptiPatcherSupportEntry), gameName As String) As OptiPatcherSupportEntry
        If lookup Is Nothing OrElse String.IsNullOrWhiteSpace(gameName) Then
            Return Nothing
        End If

        Dim entry As OptiPatcherSupportEntry = Nothing
        If lookup.TryGetValue(gameName.Trim(), entry) Then
            Return entry
        End If

        Dim normalized As String = NameNormalization.NormalizeName(gameName)
        If Not String.IsNullOrWhiteSpace(normalized) AndAlso lookup.TryGetValue(normalized, entry) Then
            Return entry
        End If

        Dim relaxed As String = NameNormalization.NormalizeRelaxedName(gameName)
        If Not String.IsNullOrWhiteSpace(relaxed) AndAlso lookup.TryGetValue(relaxed, entry) Then
            Return entry
        End If

        Return Nothing
    End Function

    Public Shared Function ParseSupportList(content As String) As List(Of OptiPatcherSupportEntry)
        Dim entries As New List(Of OptiPatcherSupportEntry)()
        If String.IsNullOrWhiteSpace(content) Then
            Return entries
        End If

        For Each rawLine As String In content.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
            Dim line As String = rawLine.Trim()
            If Not line.StartsWith("|", StringComparison.Ordinal) Then
                Continue For
            End If

            If line.StartsWith("| Game ", StringComparison.OrdinalIgnoreCase) OrElse
               line.StartsWith("| ----", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            Dim columns As String() = line.Split("|"c)
            If columns.Length < 5 Then
                Continue For
            End If

            Dim gameName As String = columns(1).Trim()
            If String.IsNullOrWhiteSpace(gameName) Then
                Continue For
            End If

            If gameName.StartsWith("<!--", StringComparison.Ordinal) Then
                Continue For
            End If

            If gameName.Equals("Game", StringComparison.OrdinalIgnoreCase) OrElse
               gameName.Equals("GAME NAME", StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            Dim dlssToken As String = columns(2).Trim()
            Dim dlssFgToken As String = columns(3).Trim()
            Dim notes As String = columns(4).Trim()

            Dim entry As New OptiPatcherSupportEntry With {
                .Name = gameName,
                .DlssSupported = ParseDlssSupport(dlssToken),
                .DlssFgSupported = ParseDlssFgSupport(dlssFgToken),
                .Notes = notes
            }

            entries.Add(entry)
        Next

        Return entries
    End Function

    Private Shared Function ParseDlssSupport(value As String) As Boolean
        If String.IsNullOrWhiteSpace(value) Then
            Return False
        End If

        Dim token As String = value.Trim()
        If ContainsYesMarker(token) Then
            Return True
        End If

        If ContainsNoMarker(token) Then
            Return False
        End If

        Dim lower As String = token.ToLowerInvariant()
        If lower.Contains("yes") OrElse lower.Contains("true") OrElse lower.Contains("supported") Then
            Return True
        End If

        Return False
    End Function

    Private Shared Function ParseDlssFgSupport(value As String) As Boolean?
        If String.IsNullOrWhiteSpace(value) Then
            Return Nothing
        End If

        Dim token As String = value.Trim()
        If ContainsYesMarker(token) Then
            Return True
        End If

        If ContainsNoMarker(token) Then
            Return False
        End If

        Dim lower As String = token.ToLowerInvariant()
        If lower.Contains("n/a") OrElse lower.Equals("na") Then
            Return False
        End If
        If lower.Contains("yes") OrElse lower.Contains("true") OrElse lower.Contains("supported") Then
            Return True
        End If
        If lower.Contains("no") OrElse lower.Contains("false") Then
            Return False
        End If

        Return Nothing
    End Function

    Private Shared Function ContainsYesMarker(value As String) As Boolean
        Return ContainsCodePoint(value, &H2714, &H2705)
    End Function

    Private Shared Function ContainsNoMarker(value As String) As Boolean
        Return ContainsCodePoint(value, &H274C, &H26D4)
    End Function

    Private Shared Function ContainsCodePoint(value As String, ParamArray codePoints As Integer()) As Boolean
        If String.IsNullOrWhiteSpace(value) OrElse codePoints Is Nothing OrElse codePoints.Length = 0 Then
            Return False
        End If

        For Each ch As Char In value
            Dim code As Integer = AscW(ch)
            For Each target As Integer In codePoints
                If code = target Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function

    Private Shared Function GetSupportListUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.OptiPatcherSupportListUrl
    End Function

    Private Shared Function TryLoadCache() As List(Of OptiPatcherSupportEntry)
        If Not File.Exists(CachePath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(CachePath)
            Return JsonSerializer.Deserialize(Of List(Of OptiPatcherSupportEntry))(json)
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherSupportService.TryLoadCache")
            Return Nothing
        End Try
    End Function

    Private Shared Sub SaveCache(entries As List(Of OptiPatcherSupportEntry))
        Try
            Dim folder As String = Path.GetDirectoryName(CachePath)
            If Not Directory.Exists(folder) Then
                Directory.CreateDirectory(folder)
            End If

            Dim json As String = JsonSerializer.Serialize(entries, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(CachePath, json)
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherSupportService.SaveCache")
        End Try
    End Sub

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
End Class
