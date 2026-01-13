Imports System.Net.Http
Imports System.Text.Json
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Public Class UpdateAssetInfo
    Public Property Name As String
    Public Property DownloadUrl As String
    Public Property Size As Long
End Class

Public Class UpdateReleaseInfo
    Public Property TagName As String
    Public Property Version As Version
    Public Property Title As String
    Public Property Notes As String
    Public Property HtmlUrl As String
    Public Property Assets As List(Of UpdateAssetInfo)
End Class

Public Structure DownloadProgressInfo
    Public ReadOnly BytesReceived As Long
    Public ReadOnly TotalBytes As Long
    Public ReadOnly SpeedBytesPerSec As Double
    Public ReadOnly Elapsed As TimeSpan

    Public Sub New(bytesReceived As Long, totalBytes As Long, speedBytesPerSec As Double, elapsed As TimeSpan)
        Me.BytesReceived = bytesReceived
        Me.TotalBytes = totalBytes
        Me.SpeedBytesPerSec = speedBytesPerSec
        Me.Elapsed = elapsed
    End Sub

    Public ReadOnly Property Percent As Integer
        Get
            If TotalBytes <= 0 Then
                Return 0
            End If
            Dim pct As Double = (CDbl(BytesReceived) / CDbl(TotalBytes)) * 100.0R
            Return CInt(Math.Max(0, Math.Min(100, Math.Round(pct))))
        End Get
    End Property
End Structure

Public Module UpdateService
    Public Async Function GetLatestReleaseAsync() As Task(Of UpdateReleaseInfo)
        Dim url As String = GetInstallerReleaseUrl()
        If String.IsNullOrWhiteSpace(url) Then
            Throw New InvalidOperationException("Installer release URL is not set. Update it in Settings.")
        End If

        Using client As New HttpClient()
            client.Timeout = TimeSpan.FromSeconds(15)
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")

            Dim json As String = Await client.GetStringAsync(url)
            Using doc As JsonDocument = JsonDocument.Parse(json)
                Dim root As JsonElement = doc.RootElement
                Dim release As New UpdateReleaseInfo()

                release.TagName = GetJsonString(root, "tag_name")
                release.Title = GetJsonString(root, "name")
                release.Notes = GetJsonString(root, "body")
                release.HtmlUrl = GetJsonString(root, "html_url")
                release.Version = ParseVersionSafe(release.TagName)

                Dim assets As New List(Of UpdateAssetInfo)()
                Dim assetsElement As JsonElement
                If root.TryGetProperty("assets", assetsElement) Then
                    For Each entry As JsonElement In assetsElement.EnumerateArray()
                        Dim name As String = GetJsonString(entry, "name")
                        Dim downloadUrl As String = GetJsonString(entry, "browser_download_url")
                        Dim size As Long = GetJsonLong(entry, "size")
                        If Not String.IsNullOrWhiteSpace(name) AndAlso Not String.IsNullOrWhiteSpace(downloadUrl) Then
                            assets.Add(New UpdateAssetInfo With {
                                .Name = name,
                                .DownloadUrl = downloadUrl,
                                .Size = size
                            })
                        End If
                    Next
                End If
                release.Assets = assets
                Return release
            End Using
        End Using
    End Function

    Public Function IsUpdateAvailable(currentVersion As Version, latestVersion As Version) As Boolean
        If currentVersion Is Nothing OrElse latestVersion Is Nothing Then
            Return False
        End If
        Return NormalizeVersionForCompare(latestVersion).CompareTo(NormalizeVersionForCompare(currentVersion)) > 0
    End Function

    Public Function SelectBestAsset(release As UpdateReleaseInfo) As UpdateAssetInfo
        If release Is Nothing OrElse release.Assets Is Nothing OrElse release.Assets.Count = 0 Then
            Return Nothing
        End If

        Dim archToken As String = If(Environment.Is64BitProcess, "x64", "x86")
        Dim best As UpdateAssetInfo = Nothing
        Dim bestScore As Integer = -1

        For Each asset As UpdateAssetInfo In release.Assets
            Dim name As String = asset.Name
            If String.IsNullOrWhiteSpace(name) Then
                Continue For
            End If

            Dim lower As String = name.ToLowerInvariant()
            Dim score As Integer = 0
            If lower.Contains("win") OrElse lower.Contains("windows") Then
                score += 2
            End If
            If lower.Contains(archToken) Then
                score += 3
            End If
            If lower.EndsWith(".zip") Then
                score += 3
            ElseIf lower.EndsWith(".exe") Then
                score += 2
            End If

            If score > bestScore Then
                bestScore = score
                best = asset
            End If
        Next

        Return best
    End Function

    Public Async Function DownloadAssetAsync(asset As UpdateAssetInfo,
                                            destinationPath As String,
                                            progress As IProgress(Of DownloadProgressInfo),
                                            token As CancellationToken) As Task
        If asset Is Nothing OrElse String.IsNullOrWhiteSpace(asset.DownloadUrl) Then
            Throw New InvalidOperationException("Missing download asset information.")
        End If

        Using client As New HttpClient()
            client.Timeout = TimeSpan.FromMinutes(10)
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Using response As HttpResponseMessage = Await client.GetAsync(asset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, token)
                response.EnsureSuccessStatusCode()

                Dim total As Long = If(response.Content.Headers.ContentLength, asset.Size)
                Dim buffer(81919) As Byte
                Dim bytesReadTotal As Long = 0
                Dim sw As Stopwatch = Stopwatch.StartNew()

                Using stream As Stream = Await response.Content.ReadAsStreamAsync(token)
                    Using output As FileStream = New FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None)
                        Do
                            Dim read As Integer = Await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token)
                            If read = 0 Then
                                Exit Do
                            End If
                            Await output.WriteAsync(buffer.AsMemory(0, read), token)
                            bytesReadTotal += read

                            If progress IsNot Nothing Then
                                Dim elapsed As TimeSpan = sw.Elapsed
                                Dim speed As Double = If(elapsed.TotalSeconds > 0, bytesReadTotal / elapsed.TotalSeconds, 0)
                                progress.Report(New DownloadProgressInfo(bytesReadTotal, total, speed, elapsed))
                            End If
                        Loop
                    End Using
                End Using

                If progress IsNot Nothing Then
                    Dim elapsed As TimeSpan = sw.Elapsed
                    Dim speed As Double = If(elapsed.TotalSeconds > 0, bytesReadTotal / elapsed.TotalSeconds, 0)
                    progress.Report(New DownloadProgressInfo(bytesReadTotal, total, speed, elapsed))
                End If
            End Using
        End Using
    End Function

    Public Function FormatBytes(value As Long) As String
        Dim units As String() = {"B", "KB", "MB", "GB", "TB"}
        Dim size As Double = value
        Dim unitIndex As Integer = 0
        While size >= 1024 AndAlso unitIndex < units.Length - 1
            size /= 1024
            unitIndex += 1
        End While
        Return $"{size:F1} {units(unitIndex)}"
    End Function

    Public Function ParseVersionSafe(text As String) As Version
        Dim clean As String = NormalizeVersionText(text)
        If String.IsNullOrWhiteSpace(clean) Then
            Return New Version(0, 0)
        End If

        Dim parts As String() = clean.Split("."c)
        Dim major As Integer = If(parts.Length > 0, ToIntSafe(parts(0)), 0)
        Dim minor As Integer = If(parts.Length > 1, ToIntSafe(parts(1)), 0)

        If parts.Length <= 2 Then
            Return New Version(major, minor)
        End If

        Dim build As Integer = ToIntSafe(parts(2))
        If parts.Length = 3 Then
            Return New Version(major, minor, build)
        End If

        Dim revision As Integer = ToIntSafe(parts(3))
        Return New Version(major, minor, build, revision)
    End Function

    Public Function NormalizeVersionForCompare(version As Version) As Version
        Dim build As Integer = If(version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version.Revision >= 0, version.Revision, 0)
        Return New Version(version.Major, version.Minor, build, revision)
    End Function

    Private Function NormalizeVersionText(text As String) As String
        If String.IsNullOrWhiteSpace(text) Then
            Return String.Empty
        End If

        Dim clean As String = text.Trim()
        Dim startIndex As Integer = 0
        While startIndex < clean.Length AndAlso Not Char.IsDigit(clean(startIndex))
            startIndex += 1
        End While

        If startIndex >= clean.Length Then
            Return String.Empty
        End If

        clean = clean.Substring(startIndex)
        Dim endIndex As Integer = clean.IndexOfAny(New Char() {" "c, "-"c, "+"c})
        If endIndex >= 0 Then
            clean = clean.Substring(0, endIndex)
        End If

        Return clean
    End Function

    Private Function ToIntSafe(text As String) As Integer
        Dim value As Integer
        If Integer.TryParse(text, value) Then
            Return value
        End If
        Return 0
    End Function

    Private Function GetInstallerReleaseUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.InstallerReleaseUrl
    End Function

    Private Function GetJsonString(root As JsonElement, name As String) As String
        Dim element As JsonElement
        If root.TryGetProperty(name, element) AndAlso element.ValueKind = JsonValueKind.String Then
            Return element.GetString()
        End If
        Return String.Empty
    End Function

    Private Function GetJsonLong(root As JsonElement, name As String) As Long
        Dim element As JsonElement
        If root.TryGetProperty(name, element) AndAlso element.ValueKind = JsonValueKind.Number Then
            Dim value As Long
            If element.TryGetInt64(value) Then
                Return value
            End If
        End If
        Return 0
    End Function
End Module
