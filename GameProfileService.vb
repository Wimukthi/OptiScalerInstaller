Option Strict On
Option Explicit On

Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Text.RegularExpressions
Imports System.Collections.Concurrent

Public Class GameInstallProfile
    ' Offline-first install profile that can apply per-game defaults and path hints.
    Public Property Name As String
    Public Property Slug As String
    Public Property Aliases As List(Of String)
    Public Property HookName As String
    Public Property GpuVendor As String
    Public Property DlssInputs As Boolean?
    Public Property FrameGeneration As String
    Public Property ConflictMode As String
    Public Property InstallSubfolderHints As List(Of String)
    Public Property ExecutableNameHints As List(Of String)
    Public Property Notes As String
    Public Property Source As String
    Public Property LastUpdatedUtc As DateTime?
End Class

Friend Module GameProfileService
    ' Built-in profiles ship with the app; cached profiles are optional remote overlays.
    Private ReadOnly ProfilesPath As String = Path.Combine(AppContext.BaseDirectory, "Data", "GameProfiles.json")
    Private ReadOnly LegacyTemplatesPath As String = Path.Combine(AppContext.BaseDirectory, "Data", "GameTemplates.json")
    Private ReadOnly CachePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScalerInstaller", "game-profiles.json")
    Private ReadOnly SyncRoot As New Object()
    Private ReadOnly RequestTimeout As TimeSpan = TimeSpan.FromSeconds(30)
    Private ReadOnly WikiProfileMaxAge As TimeSpan = TimeSpan.FromHours(24)
    Private ReadOnly SupportedHookNames As String() = {"dxgi.dll", "winmm.dll", "version.dll", "dbghelp.dll", "d3d12.dll", "wininet.dll", "winhttp.dll", "OptiScaler.asi"}
    Private _profiles As List(Of GameInstallProfile)
    Private ReadOnly InFlightWikiRefresh As New ConcurrentDictionary(Of String, Byte)(StringComparer.OrdinalIgnoreCase)

    Private Class ProfileCatalogWrapper
        Public Property Profiles As List(Of GameInstallProfile)
    End Class

    Public Function FindProfile(game As DetectedGame) As GameInstallProfile
        If game Is Nothing Then
            Return Nothing
        End If

        Dim profiles As List(Of GameInstallProfile) = LoadProfiles()
        If profiles.Count = 0 Then
            Return Nothing
        End If

        Dim slugCandidate As String = ""
        If game.MatchedEntry IsNot Nothing Then
            slugCandidate = NormalizeSlugToken(game.MatchedEntry.Slug)
        End If

        Dim nameCandidates As New List(Of String)()
        AddDistinct(nameCandidates, game.DisplayName)
        AddDistinct(nameCandidates, game.SourceName)
        If game.MatchedEntry IsNot Nothing Then
            AddDistinct(nameCandidates, game.MatchedEntry.Name)
        End If

        Dim best As GameInstallProfile = Nothing
        Dim bestScore As Integer = Integer.MinValue

        For Each profile As GameInstallProfile In profiles
            If profile Is Nothing Then
                Continue For
            End If

            Dim score As Integer = ScoreProfile(profile, slugCandidate, nameCandidates)
            If score > bestScore Then
                bestScore = score
                best = profile
            End If
        Next

        ' Require a strong match to avoid cross-title false positives.
        If bestScore < 100 Then
            Return Nothing
        End If

        Return best
    End Function

    Public Async Function RefreshDetectedGameProfilesAsync(games As IEnumerable(Of DetectedGame),
                                                           wikiBaseUrl As String,
                                                           Optional log As Action(Of String) = Nothing) As Task(Of Integer)
        If games Is Nothing Then
            Return 0
        End If

        Dim candidates As List(Of DetectedGame) = games.
            Where(Function(game) game IsNot Nothing AndAlso game.MatchedEntry IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(game.MatchedEntry.Slug)).
            GroupBy(Function(game) NormalizeSlugToken(game.MatchedEntry.Slug), StringComparer.OrdinalIgnoreCase).
            Select(Function(group) group.First()).
            ToList()

        If candidates.Count = 0 Then
            Return 0
        End If

        Dim updatedCount As Integer = 0
        For Each game As DetectedGame In candidates
            Dim updated As Boolean = Await EnsureProfileForGameAsync(game, wikiBaseUrl, False, log)
            If updated Then
                updatedCount += 1
            End If
        Next

        Return updatedCount
    End Function

    Public Async Function EnsureProfileForGameAsync(game As DetectedGame,
                                                    wikiBaseUrl As String,
                                                    Optional forceRefresh As Boolean = False,
                                                    Optional log As Action(Of String) = Nothing) As Task(Of Boolean)
        If game Is Nothing OrElse game.MatchedEntry Is Nothing OrElse String.IsNullOrWhiteSpace(game.MatchedEntry.Slug) Then
            Return False
        End If

        Dim slugToken As String = NormalizeSlugToken(game.MatchedEntry.Slug)
        If String.IsNullOrWhiteSpace(slugToken) Then
            Return False
        End If

        Dim rawWikiUrl As String = BuildWikiRawUrl(wikiBaseUrl, game.MatchedEntry.Slug)
        If String.IsNullOrWhiteSpace(rawWikiUrl) Then
            Return False
        End If

        If Not InFlightWikiRefresh.TryAdd(slugToken, 0) Then
            Return False
        End If

        Try
            Dim cachedProfile As GameInstallProfile = FindCachedWikiProfileBySlug(slugToken)
            If Not forceRefresh AndAlso cachedProfile IsNot Nothing AndAlso cachedProfile.LastUpdatedUtc.HasValue Then
                Dim age As TimeSpan = DateTime.UtcNow - cachedProfile.LastUpdatedUtc.Value
                If age >= TimeSpan.Zero AndAlso age <= WikiProfileMaxAge Then
                    Return False
                End If
            End If

            Dim markdown As String = Await DownloadWikiMarkdownAsync(rawWikiUrl)
            If String.IsNullOrWhiteSpace(markdown) Then
                Return False
            End If

            Dim existingProfile As GameInstallProfile = FindProfile(game)
            Dim wikiProfile As GameInstallProfile = BuildWikiDerivedProfile(game, markdown, existingProfile)
            If wikiProfile Is Nothing Then
                Return False
            End If

            Dim changed As Boolean = UpsertCacheProfile(wikiProfile)
            If changed AndAlso log IsNot Nothing Then
                log("Game profile updated from wiki: " & wikiProfile.Name)
            End If

            If changed Then
                Reload()
            End If

            Return changed
        Catch ex As Exception
            ErrorLogger.Log(ex, "GameProfileService.EnsureProfileForGameAsync")
            Return False
        Finally
            Dim ignored As Byte = 0
            InFlightWikiRefresh.TryRemove(slugToken, ignored)
        End Try
    End Function

    Public Async Function RefreshCatalogFromUrlAsync(url As String) As Task(Of Integer)
        Dim trimmedUrl As String = If(url, "").Trim()
        If String.IsNullOrWhiteSpace(trimmedUrl) Then
            Throw New InvalidOperationException("Game profile catalog URL is empty.")
        End If

        Dim content As String = Await HttpClientHelper.GetStringWithRetryAsync(trimmedUrl)
        Dim profiles As List(Of GameInstallProfile) = ParseProfilesJson(content, "remote")
        If profiles.Count = 0 Then
            Throw New InvalidDataException("Profile catalog response did not contain any valid profiles.")
        End If

        SaveCache(profiles)
        Reload()
        Return profiles.Count
    End Function

    Public Function GetProfileCount() As Integer
        Return LoadProfiles().Count
    End Function

    Public Sub Reload()
        SyncLock SyncRoot
            _profiles = Nothing
        End SyncLock
    End Sub

    Private Function ScoreProfile(profile As GameInstallProfile,
                                  slugCandidate As String,
                                  nameCandidates As List(Of String)) As Integer
        Dim bestScore As Integer = Integer.MinValue

        Dim profileSlug As String = NormalizeSlugToken(profile.Slug)
        If Not String.IsNullOrWhiteSpace(profileSlug) AndAlso
           Not String.IsNullOrWhiteSpace(slugCandidate) AndAlso
           String.Equals(profileSlug, slugCandidate, StringComparison.OrdinalIgnoreCase) Then
            bestScore = Math.Max(bestScore, 240)
        End If

        If Not String.IsNullOrWhiteSpace(profile.Name) Then
            For Each candidate As String In nameCandidates
                bestScore = Math.Max(bestScore, ScoreName(profile.Name, candidate, 190))
            Next
        End If

        If profile.Aliases IsNot Nothing Then
            For Each aliasValue As String In profile.Aliases
                For Each candidate As String In nameCandidates
                    bestScore = Math.Max(bestScore, ScoreName(aliasValue, candidate, 180))
                Next
            Next
        End If

        Return bestScore
    End Function

    Private Function ScoreName(expectedValue As String, candidateValue As String, exactScore As Integer) As Integer
        Dim expected As String = If(expectedValue, "").Trim()
        Dim candidate As String = If(candidateValue, "").Trim()
        If String.IsNullOrWhiteSpace(expected) OrElse String.IsNullOrWhiteSpace(candidate) Then
            Return Integer.MinValue
        End If

        If String.Equals(expected, candidate, StringComparison.OrdinalIgnoreCase) Then
            Return exactScore
        End If

        Dim expectedRelaxed As String = NameNormalization.NormalizeRelaxedName(expected)
        Dim candidateRelaxed As String = NameNormalization.NormalizeRelaxedName(candidate)
        If Not String.IsNullOrWhiteSpace(expectedRelaxed) AndAlso
           String.Equals(expectedRelaxed, candidateRelaxed, StringComparison.OrdinalIgnoreCase) Then
            Return exactScore - 5
        End If

        Dim expectedTokens As List(Of String) = NameNormalization.TokenizeRelaxed(expected)
        Dim candidateTokens As List(Of String) = NameNormalization.TokenizeRelaxed(candidate)
        If expectedTokens.Count = 0 OrElse candidateTokens.Count = 0 Then
            Return Integer.MinValue
        End If

        Dim expectedSet As New HashSet(Of String)(expectedTokens, StringComparer.OrdinalIgnoreCase)
        Dim candidateSet As New HashSet(Of String)(candidateTokens, StringComparer.OrdinalIgnoreCase)
        Dim overlap As Integer = expectedSet.Intersect(candidateSet, StringComparer.OrdinalIgnoreCase).Count()

        If overlap = 0 Then
            Return Integer.MinValue
        End If

        Dim minTokenCount As Integer = Math.Min(expectedSet.Count, candidateSet.Count)
        If minTokenCount <= 1 Then
            Return Integer.MinValue
        End If

        Dim overlapRatio As Double = CDbl(overlap) / CDbl(minTokenCount)
        If overlapRatio < 0.8R Then
            Return Integer.MinValue
        End If

        Dim sizePenalty As Integer = Math.Abs(expectedSet.Count - candidateSet.Count) * 10
        Return Math.Max(110, exactScore - 70 - sizePenalty)
    End Function

    Private Function LoadProfiles() As List(Of GameInstallProfile)
        SyncLock SyncRoot
            If _profiles IsNot Nothing Then
                Return _profiles
            End If

            Dim builtInProfiles As List(Of GameInstallProfile) = TryLoadProfilesFromFile(ProfilesPath, "built-in")
            If builtInProfiles.Count = 0 Then
                builtInProfiles = TryLoadLegacyTemplates()
            End If

            Dim cachedProfiles As List(Of GameInstallProfile) = TryLoadProfilesFromFile(CachePath, "cache")
            _profiles = MergeProfiles(builtInProfiles, cachedProfiles)
            Return _profiles
        End SyncLock
    End Function

    Private Function TryLoadProfilesFromFile(path As String, source As String) As List(Of GameInstallProfile)
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return New List(Of GameInstallProfile)()
        End If

        Try
            Dim content As String = File.ReadAllText(path)
            Return ParseProfilesJson(content, source)
        Catch ex As Exception
            ErrorLogger.Log(ex, "GameProfileService.TryLoadProfilesFromFile")
            Return New List(Of GameInstallProfile)()
        End Try
    End Function

    Private Function ParseProfilesJson(content As String, source As String) As List(Of GameInstallProfile)
        If String.IsNullOrWhiteSpace(content) Then
            Return New List(Of GameInstallProfile)()
        End If

        Try
            Dim direct As List(Of GameInstallProfile) = JsonSerializer.Deserialize(Of List(Of GameInstallProfile))(content)
            If direct IsNot Nothing AndAlso direct.Count > 0 Then
                Return NormalizeProfiles(direct, source)
            End If
        Catch
            ' Fall through and attempt wrapped format.
        End Try

        Try
            Dim wrapper As ProfileCatalogWrapper = JsonSerializer.Deserialize(Of ProfileCatalogWrapper)(content)
            If wrapper IsNot Nothing AndAlso wrapper.Profiles IsNot Nothing Then
                Return NormalizeProfiles(wrapper.Profiles, source)
            End If
        Catch ex As Exception
            ErrorLogger.Log(ex, "GameProfileService.ParseProfilesJson")
        End Try

        Return New List(Of GameInstallProfile)()
    End Function

    Private Function TryLoadLegacyTemplates() As List(Of GameInstallProfile)
        Dim migrated As New List(Of GameInstallProfile)()
        If Not File.Exists(LegacyTemplatesPath) Then
            Return migrated
        End If

        Try
            Dim content As String = File.ReadAllText(LegacyTemplatesPath)
            Dim templates As List(Of GameWorkaroundTemplate) = JsonSerializer.Deserialize(Of List(Of GameWorkaroundTemplate))(content)
            If templates Is Nothing Then
                Return migrated
            End If

            For Each template As GameWorkaroundTemplate In templates
                If template Is Nothing OrElse String.IsNullOrWhiteSpace(template.Name) Then
                    Continue For
                End If

                migrated.Add(New GameInstallProfile With {
                    .Name = template.Name,
                    .Aliases = If(template.Aliases, New List(Of String)()),
                    .HookName = template.HookName,
                    .GpuVendor = template.GpuVendor,
                    .DlssInputs = template.DlssInputs,
                    .FrameGeneration = template.FrameGeneration,
                    .ConflictMode = template.ConflictMode,
                    .Notes = template.Notes,
                    .Source = "legacy"
                })
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "GameProfileService.TryLoadLegacyTemplates")
        End Try

        Return NormalizeProfiles(migrated, "legacy")
    End Function

    Private Function MergeProfiles(baseProfiles As List(Of GameInstallProfile),
                                   overlayProfiles As List(Of GameInstallProfile)) As List(Of GameInstallProfile)
        Dim merged As New Dictionary(Of String, GameInstallProfile)(StringComparer.OrdinalIgnoreCase)

        AddProfileSet(merged, baseProfiles)
        AddProfileSet(merged, overlayProfiles)

        Return merged.Values.OrderBy(Function(p) p.Name, StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    Private Sub AddProfileSet(target As Dictionary(Of String, GameInstallProfile), profiles As List(Of GameInstallProfile))
        If profiles Is Nothing Then
            Return
        End If

        For Each profile As GameInstallProfile In profiles
            If profile Is Nothing OrElse String.IsNullOrWhiteSpace(profile.Name) Then
                Continue For
            End If

            Dim key As String = BuildProfileKey(profile)
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            target(key) = profile
        Next
    End Sub

    Private Function BuildProfileKey(profile As GameInstallProfile) As String
        If profile Is Nothing Then
            Return ""
        End If

        Dim slug As String = NormalizeSlugToken(profile.Slug)
        If Not String.IsNullOrWhiteSpace(slug) Then
            Return "slug:" & slug
        End If

        Dim normalizedName As String = NameNormalization.NormalizeRelaxedName(profile.Name)
        If String.IsNullOrWhiteSpace(normalizedName) Then
            Return ""
        End If

        Return "name:" & normalizedName
    End Function

    Private Function NormalizeProfiles(input As List(Of GameInstallProfile), source As String) As List(Of GameInstallProfile)
        Dim normalized As New List(Of GameInstallProfile)()
        If input Is Nothing Then
            Return normalized
        End If

        For Each profile As GameInstallProfile In input
            If profile Is Nothing OrElse String.IsNullOrWhiteSpace(profile.Name) Then
                Continue For
            End If

            profile.Name = profile.Name.Trim()
            profile.Slug = NormalizeSlugDisplay(profile.Slug)
            profile.Source = If(String.IsNullOrWhiteSpace(profile.Source), source, profile.Source)
            profile.Aliases = NormalizeStringList(profile.Aliases)
            profile.InstallSubfolderHints = NormalizeStringList(profile.InstallSubfolderHints)
            profile.ExecutableNameHints = NormalizeStringList(profile.ExecutableNameHints)
            normalized.Add(profile)
        Next

        Return normalized
    End Function

    Private Function NormalizeStringList(values As List(Of String)) As List(Of String)
        Dim result As New List(Of String)()
        If values Is Nothing Then
            Return result
        End If

        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each rawValue As String In values
            Dim value As String = If(rawValue, "").Trim()
            If String.IsNullOrWhiteSpace(value) Then
                Continue For
            End If

            If seen.Add(value) Then
                result.Add(value)
            End If
        Next

        Return result
    End Function

    Private Function NormalizeSlugDisplay(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim normalized As String = value.Trim().Replace("\\", "/")
        Do While normalized.StartsWith("/", StringComparison.Ordinal)
            normalized = normalized.Substring(1)
        Loop

        Return normalized
    End Function

    Private Function NormalizeSlugToken(value As String) As String
        Dim display As String = NormalizeSlugDisplay(value)
        If String.IsNullOrWhiteSpace(display) Then
            Return ""
        End If

        Return display.Replace("/", "-").Replace("_", "-").Trim().ToLowerInvariant()
    End Function

    Private Async Function DownloadWikiMarkdownAsync(url As String) As Task(Of String)
        Using response As HttpResponseMessage = Await HttpClientHelper.ApiClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(False)
            If response.StatusCode = System.Net.HttpStatusCode.NotFound Then
                Return ""
            End If

            response.EnsureSuccessStatusCode()
            Return Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
        End Using
    End Function

    Private Function BuildWikiRawUrl(wikiBaseUrl As String, slug As String) As String
        Dim slugValue As String = NormalizeSlugDisplay(slug)
        If String.IsNullOrWhiteSpace(slugValue) Then
            Return ""
        End If
        Dim encodedSlug As String = String.Join("/",
                                                slugValue.
                                                    Split("/"c).
                                                    Select(Function(part) part.Trim()).
                                                    Where(Function(part) Not String.IsNullOrWhiteSpace(part)).
                                                    Select(Function(part) Uri.EscapeDataString(part)))
        If String.IsNullOrWhiteSpace(encodedSlug) Then
            Return ""
        End If

        Dim baseValue As String = If(wikiBaseUrl, "").Trim()
        If String.IsNullOrWhiteSpace(baseValue) Then
            Return ""
        End If

        Dim match As Match = Regex.Match(baseValue, "github\.com/(?<owner>[^/\s]+)/(?<repo>[^/\s]+)/wiki/?", RegexOptions.IgnoreCase)
        If match.Success Then
            Dim owner As String = match.Groups("owner").Value
            Dim repo As String = match.Groups("repo").Value
            Return $"https://raw.githubusercontent.com/wiki/{owner}/{repo}/{encodedSlug}.md"
        End If

        match = Regex.Match(baseValue, "raw\.githubusercontent\.com/wiki/(?<owner>[^/\s]+)/(?<repo>[^/\s]+)/?", RegexOptions.IgnoreCase)
        If match.Success Then
            Dim owner As String = match.Groups("owner").Value
            Dim repo As String = match.Groups("repo").Value
            Return $"https://raw.githubusercontent.com/wiki/{owner}/{repo}/{encodedSlug}.md"
        End If

        Return ""
    End Function

    Private Function BuildWikiDerivedProfile(game As DetectedGame,
                                             markdown As String,
                                             baseProfile As GameInstallProfile) As GameInstallProfile
        If game Is Nothing OrElse game.MatchedEntry Is Nothing Then
            Return Nothing
        End If

        Dim profile As New GameInstallProfile With {
            .Name = game.MatchedEntry.Name,
            .Slug = NormalizeSlugDisplay(game.MatchedEntry.Slug),
            .Aliases = If(baseProfile?.Aliases, New List(Of String)()),
            .HookName = If(baseProfile?.HookName, ""),
            .GpuVendor = If(baseProfile?.GpuVendor, ""),
            .DlssInputs = If(baseProfile Is Nothing, CType(Nothing, Boolean?), baseProfile.DlssInputs),
            .FrameGeneration = If(baseProfile?.FrameGeneration, ""),
            .ConflictMode = If(baseProfile?.ConflictMode, ""),
            .InstallSubfolderHints = If(baseProfile?.InstallSubfolderHints, New List(Of String)()),
            .ExecutableNameHints = If(baseProfile?.ExecutableNameHints, New List(Of String)()),
            .Notes = "Auto-derived from wiki page.",
            .Source = "wiki-auto",
            .LastUpdatedUtc = DateTime.UtcNow
        }

        Dim extractedHook As String = ExtractHookName(markdown)
        If Not String.IsNullOrWhiteSpace(extractedHook) Then
            profile.HookName = extractedHook
        End If

        Dim extractedInstallHints As List(Of String) = ExtractInstallSubfolderHints(markdown)
        If extractedInstallHints.Count > 0 Then
            profile.InstallSubfolderHints = MergeStringLists(profile.InstallSubfolderHints, extractedInstallHints)
        End If

        Dim extractedExeHints As List(Of String) = ExtractExecutableNameHints(markdown)
        If extractedExeHints.Count > 0 Then
            profile.ExecutableNameHints = MergeStringLists(profile.ExecutableNameHints, extractedExeHints)
        End If

        Dim extractedFg As String = ExtractFrameGenerationHint(markdown)
        If Not String.IsNullOrWhiteSpace(extractedFg) Then
            profile.FrameGeneration = extractedFg
        End If

        Dim extractedDlss As Boolean? = ExtractDlssInputsHint(markdown)
        If extractedDlss.HasValue Then
            profile.DlssInputs = extractedDlss
        End If

        profile.Aliases = NormalizeStringList(profile.Aliases)
        profile.InstallSubfolderHints = NormalizeStringList(profile.InstallSubfolderHints)
        profile.ExecutableNameHints = NormalizeStringList(profile.ExecutableNameHints)

        Return profile
    End Function

    Private Function ExtractHookName(markdown As String) As String
        If String.IsNullOrWhiteSpace(markdown) Then
            Return ""
        End If

        Dim lines As String() = markdown.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
        For Each line As String In lines
            Dim lower As String = line.ToLowerInvariant()
            If lower.Contains("rename") OrElse lower.Contains("hook") OrElse lower.Contains("dll to") Then
                For Each hook As String In SupportedHookNames
                    If lower.Contains(hook.ToLowerInvariant()) Then
                        Return hook
                    End If
                Next
            End If
        Next

        For Each hook As String In SupportedHookNames
            If Regex.IsMatch(markdown, "\b" & Regex.Escape(hook) & "\b", RegexOptions.IgnoreCase) Then
                Return hook
            End If
        Next

        Return ""
    End Function

    Private Function ExtractInstallSubfolderHints(markdown As String) As List(Of String)
        Dim hints As New List(Of String)()
        If String.IsNullOrWhiteSpace(markdown) Then
            Return hints
        End If

        If Regex.IsMatch(markdown, "binaries[\\/]+win64", RegexOptions.IgnoreCase) Then
            hints.Add("Binaries\Win64")
        End If
        If Regex.IsMatch(markdown, "binaries[\\/]+wingdk", RegexOptions.IgnoreCase) Then
            hints.Add("Binaries\WinGDK")
        End If
        If Regex.IsMatch(markdown, "bin[\\/]+x64_dx12", RegexOptions.IgnoreCase) Then
            hints.Add("bin\x64_dx12")
        End If
        If Regex.IsMatch(markdown, "bin[\\/]+x64", RegexOptions.IgnoreCase) Then
            hints.Add("bin\x64")
        End If

        Return NormalizeStringList(hints)
    End Function

    Private Function ExtractExecutableNameHints(markdown As String) As List(Of String)
        Dim hints As New List(Of String)()
        If String.IsNullOrWhiteSpace(markdown) Then
            Return hints
        End If

        For Each match As Match In Regex.Matches(markdown, "\b([A-Za-z0-9_\-\.]+\.exe)\b", RegexOptions.IgnoreCase)
            If match Is Nothing OrElse String.IsNullOrWhiteSpace(match.Value) Then
                Continue For
            End If

            Dim exeName As String = match.Value.Trim()
            Dim lower As String = exeName.ToLowerInvariant()
            If lower.Contains("launcher") OrElse lower.Contains("setup") OrElse lower.Contains("unins") Then
                Continue For
            End If
            hints.Add(exeName)
        Next

        Return NormalizeStringList(hints)
    End Function

    Private Function ExtractFrameGenerationHint(markdown As String) As String
        If String.IsNullOrWhiteSpace(markdown) Then
            Return ""
        End If

        If Regex.IsMatch(markdown, "\bnukem\b", RegexOptions.IgnoreCase) Then
            Return "Nukem"
        End If
        If Regex.IsMatch(markdown, "\boptifg\b", RegexOptions.IgnoreCase) Then
            Return "OptiFG"
        End If

        Return ""
    End Function

    Private Function ExtractDlssInputsHint(markdown As String) As Boolean?
        If String.IsNullOrWhiteSpace(markdown) Then
            Return Nothing
        End If

        If Regex.IsMatch(markdown, "dxgi\s*=\s*false", RegexOptions.IgnoreCase) OrElse
           Regex.IsMatch(markdown, "disable(?:d)?\s+dlss\s+inputs", RegexOptions.IgnoreCase) Then
            Return False
        End If

        If Regex.IsMatch(markdown, "enable(?:d)?\s+dlss\s+inputs", RegexOptions.IgnoreCase) Then
            Return True
        End If

        Return Nothing
    End Function

    Private Function MergeStringLists(baseValues As List(Of String), extraValues As List(Of String)) As List(Of String)
        Dim merged As New List(Of String)()
        If baseValues IsNot Nothing Then
            merged.AddRange(baseValues)
        End If
        If extraValues IsNot Nothing Then
            merged.AddRange(extraValues)
        End If
        Return NormalizeStringList(merged)
    End Function

    Private Function FindCachedWikiProfileBySlug(slugToken As String) As GameInstallProfile
        If String.IsNullOrWhiteSpace(slugToken) Then
            Return Nothing
        End If

        Dim cacheProfiles As List(Of GameInstallProfile) = TryLoadProfilesFromFile(CachePath, "cache")
        For Each profile As GameInstallProfile In cacheProfiles
            If profile Is Nothing Then
                Continue For
            End If

            If Not String.Equals(NormalizeSlugToken(profile.Slug), slugToken, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            If String.IsNullOrWhiteSpace(profile.Source) OrElse
               profile.Source.StartsWith("wiki", StringComparison.OrdinalIgnoreCase) Then
                Return profile
            End If
        Next

        Return Nothing
    End Function

    Private Function UpsertCacheProfile(profile As GameInstallProfile) As Boolean
        If profile Is Nothing OrElse String.IsNullOrWhiteSpace(profile.Name) Then
            Return False
        End If

        Dim cacheProfiles As List(Of GameInstallProfile) = TryLoadProfilesFromFile(CachePath, "cache")
        Dim key As String = BuildProfileKey(profile)
        If String.IsNullOrWhiteSpace(key) Then
            Return False
        End If

        Dim updated As Boolean = False
        Dim output As New List(Of GameInstallProfile)()
        Dim replaced As Boolean = False

        For Each existing As GameInstallProfile In cacheProfiles
            If existing Is Nothing Then
                Continue For
            End If

            If String.Equals(BuildProfileKey(existing), key, StringComparison.OrdinalIgnoreCase) Then
                If Not ProfilesEquivalent(existing, profile) Then
                    output.Add(profile)
                    updated = True
                Else
                    output.Add(existing)
                End If
                replaced = True
            Else
                output.Add(existing)
            End If
        Next

        If Not replaced Then
            output.Add(profile)
            updated = True
        End If

        If updated Then
            SaveCache(output)
        End If

        Return updated
    End Function

    Private Function ProfilesEquivalent(left As GameInstallProfile, right As GameInstallProfile) As Boolean
        If left Is Nothing OrElse right Is Nothing Then
            Return False
        End If

        If Not String.Equals(If(left.Name, ""), If(right.Name, ""), StringComparison.OrdinalIgnoreCase) Then Return False
        If Not String.Equals(If(left.Slug, ""), If(right.Slug, ""), StringComparison.OrdinalIgnoreCase) Then Return False
        If Not String.Equals(If(left.HookName, ""), If(right.HookName, ""), StringComparison.OrdinalIgnoreCase) Then Return False
        If Not String.Equals(If(left.GpuVendor, ""), If(right.GpuVendor, ""), StringComparison.OrdinalIgnoreCase) Then Return False
        If left.DlssInputs <> right.DlssInputs Then Return False
        If Not String.Equals(If(left.FrameGeneration, ""), If(right.FrameGeneration, ""), StringComparison.OrdinalIgnoreCase) Then Return False
        If Not String.Equals(If(left.ConflictMode, ""), If(right.ConflictMode, ""), StringComparison.OrdinalIgnoreCase) Then Return False
        If Not String.Equals(If(left.Notes, ""), If(right.Notes, ""), StringComparison.Ordinal) Then Return False
        If Not ListEquals(left.Aliases, right.Aliases) Then Return False
        If Not ListEquals(left.InstallSubfolderHints, right.InstallSubfolderHints) Then Return False
        If Not ListEquals(left.ExecutableNameHints, right.ExecutableNameHints) Then Return False
        Return True
    End Function

    Private Function ListEquals(left As List(Of String), right As List(Of String)) As Boolean
        Dim normalizedLeft As List(Of String) = NormalizeStringList(If(left, New List(Of String)()))
        Dim normalizedRight As List(Of String) = NormalizeStringList(If(right, New List(Of String)()))
        If normalizedLeft.Count <> normalizedRight.Count Then
            Return False
        End If

        For i As Integer = 0 To normalizedLeft.Count - 1
            If Not String.Equals(normalizedLeft(i), normalizedRight(i), StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Sub SaveCache(profiles As List(Of GameInstallProfile))
        Try
            Dim targetDirectory As String = Path.GetDirectoryName(CachePath)
            If Not String.IsNullOrWhiteSpace(targetDirectory) AndAlso Not Directory.Exists(targetDirectory) Then
                Directory.CreateDirectory(targetDirectory)
            End If

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True
            }
            Dim json As String = JsonSerializer.Serialize(profiles, options)
            File.WriteAllText(CachePath, json)
        Catch ex As Exception
            ErrorLogger.Log(ex, "GameProfileService.SaveCache")
        End Try
    End Sub

    Private Sub AddDistinct(items As List(Of String), value As String)
        Dim trimmed As String = If(value, "").Trim()
        If String.IsNullOrWhiteSpace(trimmed) Then
            Return
        End If

        For Each existing As String In items
            If String.Equals(existing, trimmed, StringComparison.OrdinalIgnoreCase) Then
                Return
            End If
        Next

        items.Add(trimmed)
    End Sub
End Module
