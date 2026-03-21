Imports System.Net.Http
Imports System.Text.Json

Public Class ReleaseService
    ' Fetches OptiScaler stable/nightly release metadata from GitHub.
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

    Private Shared Async Function GetReleaseAsync(url As String) As Task(Of ReleaseInfo)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Dim json As String = Await client.GetStringAsync(url)

            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = doc.RootElement
                Dim tagName As String = ""
                Dim htmlUrl As String = ""
                Dim assets As JsonElement
                Dim hasAssets As Boolean = False

                If root.TryGetProperty("tag_name", assets) AndAlso assets.ValueKind = JsonValueKind.String Then
                    tagName = assets.GetString()
                End If
                If root.TryGetProperty("html_url", assets) AndAlso assets.ValueKind = JsonValueKind.String Then
                    htmlUrl = assets.GetString()
                End If
                hasAssets = root.TryGetProperty("assets", assets)

                If hasAssets AndAlso assets.ValueKind = JsonValueKind.Array Then
                    For Each asset As JsonElement In assets.EnumerateArray()
                        Dim name As String = ""
                        Dim downloadUrl As String = ""
                        Dim digest As String = ""
                        Dim size As Long = 0

                        Dim value As JsonElement
                        If asset.TryGetProperty("name", value) AndAlso value.ValueKind = JsonValueKind.String Then
                            name = value.GetString()
                        End If
                        If asset.TryGetProperty("browser_download_url", value) AndAlso value.ValueKind = JsonValueKind.String Then
                            downloadUrl = value.GetString()
                        End If
                        If asset.TryGetProperty("size", value) AndAlso value.ValueKind = JsonValueKind.Number Then
                            value.TryGetInt64(size)
                        End If
                        If asset.TryGetProperty("digest", value) AndAlso value.ValueKind = JsonValueKind.String Then
                            digest = value.GetString()
                        End If

                        Return New ReleaseInfo With {
                            .TagName = tagName,
                            .AssetName = name,
                            .DownloadUrl = downloadUrl,
                            .Size = size,
                            .HtmlUrl = htmlUrl,
                            .AssetDigest = digest
                        }
                    Next
                End If

                Return New ReleaseInfo With {
                    .TagName = tagName,
                    .AssetName = "",
                    .DownloadUrl = "",
                    .Size = 0,
                    .HtmlUrl = htmlUrl,
                    .AssetDigest = ""
                }
            End Using
        End Using
    End Function
End Class
