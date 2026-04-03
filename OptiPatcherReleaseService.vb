Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.RegularExpressions

Public Class OptiPatcherReleaseService
    ' Fetches OptiPatcher release metadata and resolves an .asi download asset.
    Private Shared ReadOnly RequestTimeout As TimeSpan = TimeSpan.FromSeconds(30)
    Private Const MaxRequestAttempts As Integer = 3

    Public Shared Async Function GetStableReleaseAsync() As Task(Of ReleaseInfo)
        Dim url As String = GetStableReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("OptiPatcher stable release URL is not set.")
        End If

        Return Await GetReleaseAsync(url, True)
    End Function

    Public Shared Async Function GetRollingReleaseAsync() As Task(Of ReleaseInfo)
        Dim url As String = GetRollingReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("OptiPatcher rolling release URL is not set.")
        End If

        Return Await GetReleaseAsync(url, False)
    End Function

    Public Shared Async Function GetAlternateReleaseAsync() As Task(Of ReleaseInfo)
        Dim url As String = GetAlternateReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("OptiPatcher alternate release URL is not set.")
        End If

        Return Await GetReleaseAsync(url, False)
    End Function

    Private Shared Function GetStableReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.OptiPatcherStableReleaseUrl
    End Function

    Private Shared Function GetRollingReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.OptiPatcherRollingReleaseUrl
    End Function

    Private Shared Function GetAlternateReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.OptiPatcherAlternateReleaseUrl
    End Function

    Private Shared Async Function GetReleaseAsync(url As String, preferVersionedTag As Boolean) As Task(Of ReleaseInfo)
        Using client As New HttpClient() With {.Timeout = RequestTimeout}
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")
            Dim json As String = Await GetStringWithRetryAsync(client, url)

            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim releaseNode As JsonElement = ResolveReleaseNode(doc.RootElement, preferVersionedTag)
                If releaseNode.ValueKind = JsonValueKind.Undefined Then
                    Return Nothing
                End If

                Dim tagName As String = GetJsonString(releaseNode, "tag_name")
                Dim htmlUrl As String = GetJsonString(releaseNode, "html_url")

                Dim selectedAsset As GitHubAsset = Nothing
                Dim assetsNode As JsonElement
                If releaseNode.TryGetProperty("assets", assetsNode) AndAlso assetsNode.ValueKind = JsonValueKind.Array Then
                    selectedAsset = SelectBestAsiAsset(assetsNode)
                End If

                If selectedAsset Is Nothing Then
                    Return New ReleaseInfo With {
                        .TagName = tagName,
                        .HtmlUrl = htmlUrl
                    }
                End If

                Return New ReleaseInfo With {
                    .TagName = tagName,
                    .HtmlUrl = htmlUrl,
                    .AssetName = selectedAsset.Name,
                    .DownloadUrl = selectedAsset.DownloadUrl,
                    .Size = selectedAsset.Size,
                    .AssetDigest = selectedAsset.Digest
                }
            End Using
        End Using
    End Function

    Private Shared Function ResolveReleaseNode(root As JsonElement, preferVersionedTag As Boolean) As JsonElement
        If root.ValueKind = JsonValueKind.Object Then
            Return root
        End If

        If root.ValueKind <> JsonValueKind.Array Then
            Return Nothing
        End If

        Dim fallback As JsonElement = Nothing
        For Each item As JsonElement In root.EnumerateArray()
            If item.ValueKind <> JsonValueKind.Object Then
                Continue For
            End If

            If fallback.ValueKind = JsonValueKind.Undefined Then
                fallback = item
            End If

            Dim tagName As String = GetJsonString(item, "tag_name")
            If String.IsNullOrWhiteSpace(tagName) Then
                Continue For
            End If

            If preferVersionedTag AndAlso Regex.IsMatch(tagName, "^v\d+(\.\d+){1,3}[a-zA-Z0-9\-\.]*$", RegexOptions.CultureInvariant) Then
                Return item
            End If

            If Not preferVersionedTag Then
                Return item
            End If
        Next

        Return fallback
    End Function

    Private Class GitHubAsset
        Public Property Name As String
        Public Property DownloadUrl As String
        Public Property Digest As String
        Public Property Size As Long
        Public Property Score As Integer
    End Class

    Private Shared Function SelectBestAsiAsset(assets As JsonElement) As GitHubAsset
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

        Dim lower As String = name.ToLowerInvariant()
        Dim score As Integer = 0
        If lower.EndsWith(".asi", StringComparison.OrdinalIgnoreCase) Then
            score += 120
        Else
            score -= 100
        End If

        If lower.Contains("optipatcher") Then
            score += 80
        End If

        If lower.Contains("pdb") OrElse lower.Contains("source") OrElse lower.Contains("symbol") OrElse lower.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) Then
            score -= 120
        End If

        Return New GitHubAsset With {
            .Name = name,
            .DownloadUrl = downloadUrl,
            .Digest = GetJsonString(asset, "digest"),
            .Size = GetJsonInt64(asset, "size"),
            .Score = score
        }
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

