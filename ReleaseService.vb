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
            Throw New InvalidOperationException("Nightly release URL is not set. Update it in Settings.")
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

    Private Shared Async Function GetReleaseAsync(url As String) As Task(Of ReleaseInfo)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Dim json As String = Await client.GetStringAsync(url)

            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = doc.RootElement
                Dim tagName As String = root.GetProperty("tag_name").GetString()
                Dim assets As JsonElement = root.GetProperty("assets")

                For Each asset As JsonElement In assets.EnumerateArray()
                    Dim name As String = asset.GetProperty("name").GetString()
                    Dim downloadUrl As String = asset.GetProperty("browser_download_url").GetString()
                    Dim size As Long = asset.GetProperty("size").GetInt64()

                    Return New ReleaseInfo With {
                        .TagName = tagName,
                        .AssetName = name,
                        .DownloadUrl = downloadUrl,
                        .Size = size
                    }
                Next

                Return New ReleaseInfo With {
                    .TagName = tagName,
                    .AssetName = "",
                    .DownloadUrl = "",
                    .Size = 0
                }
            End Using
        End Using
    End Function
End Class
