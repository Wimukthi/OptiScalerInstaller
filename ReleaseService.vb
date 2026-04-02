Imports System.Net.Http
Imports System.Text.Json

Public Class ReleaseService
    ' Fetches OptiScaler release metadata from GitHub and chooses a safe/usable asset.
    Private Shared ReadOnly ReleaseRequestTimeout As TimeSpan = TimeSpan.FromSeconds(30)
    Private Const MaxRequestAttempts As Integer = 3

    Public Shared Async Function GetStableReleaseAsync() As Task(Of ReleaseInfo)
        Dim url As String = GetStableReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("Stable release URL is not set. Update it in Settings.")
        End If
        Return Await GetReleaseAsync(url)
    End Function

    Public Shared Async Function GetNightlyReleaseAsync() As Task(Of ReleaseInfo)
        Dim url As String = GetNightlyReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("Alternate release URL is not set. Update it in Settings.")
        End If
        Return Await GetReleaseAsync(url)
    End Function

    Public Shared Async Function GetComponentReleaseAsync() As Task(Of ReleaseInfo)
        Dim url As String = GetComponentReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("Component release URL is not set. Update it in settings.json.")
        End If
        Return Await GetReleaseAsync(url)
    End Function

    Private Shared Function GetStableReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.StableReleaseUrl
    End Function

    Private Shared Function GetNightlyReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.NightlyReleaseUrl
    End Function

    Private Shared Function GetComponentReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.ComponentReleaseUrl
    End Function

    ' Retrieves the release payload and selects the most installer-appropriate asset.
    Private Shared Async Function GetReleaseAsync(url As String) As Task(Of ReleaseInfo)
        Using client As New HttpClient() With {.Timeout = ReleaseRequestTimeout}
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")
            Dim json As String = Await GetStringWithRetryAsync(client, url)

            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = doc.RootElement
                Dim tagName As String = ""
                Dim htmlUrl As String = ""
                Dim assets As JsonElement

                If root.TryGetProperty("tag_name", assets) AndAlso assets.ValueKind = JsonValueKind.String Then
                    tagName = assets.GetString()
                End If
                If root.TryGetProperty("html_url", assets) AndAlso assets.ValueKind = JsonValueKind.String Then
                    htmlUrl = assets.GetString()
                End If

                Dim selectedAsset As GitHubAsset = Nothing
                If root.TryGetProperty("assets", assets) AndAlso assets.ValueKind = JsonValueKind.Array Then
                    selectedAsset = SelectBestAsset(assets)
                End If

                Dim result As New ReleaseInfo With {
                    .TagName = tagName,
                    .AssetName = "",
                    .DownloadUrl = "",
                    .Size = 0,
                    .HtmlUrl = htmlUrl,
                    .AssetDigest = ""
                }

                If selectedAsset IsNot Nothing Then
                    result.AssetName = selectedAsset.Name
                    result.DownloadUrl = selectedAsset.DownloadUrl
                    result.Size = selectedAsset.Size
                    result.AssetDigest = selectedAsset.Digest
                End If

                Return result
            End Using
        End Using
    End Function

    ' Retries transient HTTP failures to reduce flaky release checks on unstable links.
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

    Private Class GitHubAsset
        Public Property Name As String
        Public Property DownloadUrl As String
        Public Property Digest As String
        Public Property Size As Long
        Public Property Score As Integer
    End Class

    ' Chooses the archive most likely to be the actual OptiScaler package.
    Private Shared Function SelectBestAsset(assets As JsonElement) As GitHubAsset
        Dim best As GitHubAsset = Nothing

        For Each asset As JsonElement In assets.EnumerateArray()
            Dim candidate As GitHubAsset = ParseAsset(asset)
            If candidate Is Nothing OrElse String.IsNullOrWhiteSpace(candidate.DownloadUrl) Then
                Continue For
            End If

            If best Is Nothing OrElse candidate.Score > best.Score Then
                best = candidate
            End If
        Next

        Return best
    End Function

    Private Shared Function ParseAsset(asset As JsonElement) As GitHubAsset
        Dim name As String = GetJsonString(asset, "name")
        Dim downloadUrl As String = GetJsonString(asset, "browser_download_url")
        If String.IsNullOrWhiteSpace(name) OrElse String.IsNullOrWhiteSpace(downloadUrl) Then
            Return Nothing
        End If

        Dim size As Long = GetJsonInt64(asset, "size")
        Dim digest As String = GetJsonString(asset, "digest")
        Dim score As Integer = ScoreAsset(name)

        Return New GitHubAsset With {
            .Name = name,
            .DownloadUrl = downloadUrl,
            .Digest = digest,
            .Size = size,
            .Score = score
        }
    End Function

    ' Higher scores represent better install candidates.
    Private Shared Function ScoreAsset(name As String) As Integer
        If String.IsNullOrWhiteSpace(name) Then
            Return Integer.MinValue
        End If

        Dim lower As String = name.ToLowerInvariant()
        Dim score As Integer = 0

        If lower.Contains("optiscaler") Then
            score += 50
        End If

        If lower.EndsWith(".7z", StringComparison.OrdinalIgnoreCase) Then
            score += 100
        ElseIf lower.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) Then
            score += 80
        ElseIf lower.EndsWith(".rar", StringComparison.OrdinalIgnoreCase) Then
            score += 20
        Else
            score -= 30
        End If

        If lower.Contains("source") OrElse lower.Contains("symbol") OrElse lower.Contains("pdb") Then
            score -= 80
        End If

        If lower.Contains("checksum") OrElse lower.Contains("sha256") OrElse lower.Contains(".txt") Then
            score -= 120
        End If

        If lower.Contains("windows") OrElse lower.Contains("win") Then
            score += 10
        End If

        Return score
    End Function

    Private Shared Function GetJsonString(root As JsonElement, name As String) As String
        Dim value As JsonElement
        If root.TryGetProperty(name, value) AndAlso value.ValueKind = JsonValueKind.String Then
            Return value.GetString()
        End If

        Return String.Empty
    End Function

    Private Shared Function GetJsonInt64(root As JsonElement, name As String) As Long
        Dim value As JsonElement
        If root.TryGetProperty(name, value) AndAlso value.ValueKind = JsonValueKind.Number Then
            Dim parsed As Long
            If value.TryGetInt64(parsed) Then
                Return parsed
            End If
        End If

        Return 0
    End Function
End Class
