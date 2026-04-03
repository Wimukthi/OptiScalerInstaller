Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Text.RegularExpressions

Public Class OptiPatcherInstallerService
    ' Installs, updates, removes, and validates OptiPatcher.asi for a selected game.
    Private Shared ReadOnly DownloadTimeout As TimeSpan = TimeSpan.FromMinutes(5)
    Private Const MaxDownloadAttempts As Integer = 3

    Private Class SourceFileResult
        Public Property FilePath As String
        Public Property Release As ReleaseInfo
        Public Property AssetName As String
        Public Property SourceUrl As String
    End Class

    Public Shared Async Function InstallAsync(config As OptiPatcherInstallConfig, log As Action(Of String)) As Task(Of OptiPatcherManifest)
        ValidateConfig(config)

        Dim iniPath As String = Path.Combine(config.GameFolder, "OptiScaler.ini")
        If Not File.Exists(iniPath) Then
            Throw New InvalidOperationException("OptiScaler.ini was not found in the selected game folder. Install OptiScaler first.")
        End If

        Dim tempRoot As String = Path.Combine(Path.GetTempPath(), "OptiScalerInstaller", "optipatcher")
        Directory.CreateDirectory(tempRoot)

        Dim source As SourceFileResult = Await ResolveSourceFileAsync(config, tempRoot, log)
        Dim downloadedTempFile As Boolean = Not String.Equals(config.LocalAsiPath, source.FilePath, StringComparison.OrdinalIgnoreCase)
        Try
            If Not File.Exists(source.FilePath) Then
                Throw New FileNotFoundException("OptiPatcher source file was not found.")
            End If

            Dim extension As String = Path.GetExtension(source.FilePath)
            If Not String.Equals(extension, ".asi", StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Selected OptiPatcher file is not an .asi plugin.")
            End If

            Dim sourceSha As String = ComputeSha256(source.FilePath)
            VerifyReleaseDigest(source.Release, sourceSha, log)

            Dim pluginFolder As String = ResolvePluginFolder(config, iniPath)
            Directory.CreateDirectory(pluginFolder)
            Dim destination As String = Path.Combine(pluginFolder, "OptiPatcher.asi")

            Dim backupPath As String = ""
            If File.Exists(destination) Then
                Select Case config.ConflictMode
                    Case ConflictMode.Skip
                        log?.Invoke("OptiPatcher.asi already exists and conflict mode is Skip. No file changes were made.")
                        EnsureAsiLoadingEnabled(config.GameFolder, iniPath, pluginFolder, config.PluginPathOverride, log)
                        Return Nothing
                    Case ConflictMode.BackupAndOverwrite
                        backupPath = destination & ".bak_" & DateTime.Now.ToString("yyyyMMddHHmmss")
                        File.Copy(destination, backupPath, True)
                        log?.Invoke("Backed up existing OptiPatcher.asi to " & Path.GetFileName(backupPath) & ".")
                    Case ConflictMode.Overwrite
                        ' Intentionally overwrite in-place.
                End Select
            End If

            File.Copy(source.FilePath, destination, True)
            EnsureAsiLoadingEnabled(config.GameFolder, iniPath, pluginFolder, config.PluginPathOverride, log)
            log?.Invoke("Installed OptiPatcher.asi to " & destination)

            Dim manifest As New OptiPatcherManifest With {
                .InstallerVersion = GetInstallerVersion(),
                .InstallTimeUtc = DateTime.UtcNow,
                .GameFolder = config.GameFolder,
                .PluginFolder = pluginFolder,
                .AsiPath = destination,
                .BackupPath = backupPath,
                .OptiPatcherVersion = If(source.Release Is Nothing, "", source.Release.TagName),
                .OptiPatcherSource = config.Source.ToString(),
                .AssetName = source.AssetName,
                .SourceUrl = source.SourceUrl,
                .AssetSha256 = sourceSha
            }

            SaveManifest(config.GameFolder, manifest)
            Return manifest
        Finally
            If downloadedTempFile Then
                Try
                    If File.Exists(source.FilePath) Then
                        File.Delete(source.FilePath)
                    End If
                Catch ex As Exception
                    ErrorLogger.Log(ex, "OptiPatcherInstallerService.CleanupTempSource")
                End Try
            End If
        End Try
    End Function

    Public Shared Async Function RemoveAsync(gameFolder As String, log As Action(Of String), Optional allowUnmanagedRemoval As Boolean = True) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return False
        End If

        Dim manifestPath As String = OptiPatcherInstallDetector.GetManifestPath(gameFolder)
        If File.Exists(manifestPath) Then
            Dim manifest As OptiPatcherManifest = Nothing
            Try
                Dim json As String = Await File.ReadAllTextAsync(manifestPath)
                manifest = JsonSerializer.Deserialize(Of OptiPatcherManifest)(json)
            Catch ex As Exception
                ErrorLogger.Log(ex, "OptiPatcherInstallerService.Remove.ReadManifest")
            End Try

            Dim removed As Boolean = False
            If manifest IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(manifest.AsiPath) AndAlso File.Exists(manifest.AsiPath) Then
                File.Delete(manifest.AsiPath)
                log?.Invoke("Removed OptiPatcher.asi.")
                removed = True
            End If

            If manifest IsNot Nothing AndAlso
               Not String.IsNullOrWhiteSpace(manifest.BackupPath) AndAlso
               File.Exists(manifest.BackupPath) AndAlso
               Not String.IsNullOrWhiteSpace(manifest.AsiPath) Then
                File.Copy(manifest.BackupPath, manifest.AsiPath, True)
                File.Delete(manifest.BackupPath)
                log?.Invoke("Restored backup plugin.")
                removed = True
            End If

            Try
                File.Delete(manifestPath)
            Catch ex As Exception
                ErrorLogger.Log(ex, "OptiPatcherInstallerService.Remove.DeleteManifest")
            End Try

            Return removed
        End If

        If Not allowUnmanagedRemoval Then
            Return False
        End If

        Dim detected As OptiPatcherInstallInfo = OptiPatcherInstallDetector.Detect(gameFolder)
        If detected Is Nothing OrElse Not detected.IsInstalled OrElse String.IsNullOrWhiteSpace(detected.AsiPath) Then
            Return False
        End If

        If File.Exists(detected.AsiPath) Then
            File.Delete(detected.AsiPath)
            log?.Invoke("Removed unmanaged OptiPatcher plugin file.")
            Return True
        End If

        Return False
    End Function

    Private Shared Sub ValidateConfig(config As OptiPatcherInstallConfig)
        If config Is Nothing Then
            Throw New ArgumentNullException(NameOf(config))
        End If

        If String.IsNullOrWhiteSpace(config.GameFolder) OrElse Not Directory.Exists(config.GameFolder) Then
            Throw New InvalidOperationException("Game folder is not valid.")
        End If

        If config.Source = OptiPatcherSource.LocalFile AndAlso (String.IsNullOrWhiteSpace(config.LocalAsiPath) OrElse Not File.Exists(config.LocalAsiPath)) Then
            Throw New InvalidOperationException("Select a local OptiPatcher .asi file.")
        End If
    End Sub

    Private Shared Async Function ResolveSourceFileAsync(config As OptiPatcherInstallConfig, tempRoot As String, log As Action(Of String)) As Task(Of SourceFileResult)
        If config.Source = OptiPatcherSource.LocalFile Then
            log?.Invoke("Using local OptiPatcher file: " & config.LocalAsiPath)
            Return New SourceFileResult With {
                .FilePath = config.LocalAsiPath,
                .AssetName = Path.GetFileName(config.LocalAsiPath),
                .SourceUrl = ""
            }
        End If

        Dim release As ReleaseInfo = Nothing
        Select Case config.Source
            Case OptiPatcherSource.Stable
                release = config.StableRelease
                If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
                    release = Await OptiPatcherReleaseService.GetStableReleaseAsync()
                End If
            Case OptiPatcherSource.Rolling
                release = config.RollingRelease
                If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
                    release = Await OptiPatcherReleaseService.GetRollingReleaseAsync()
                End If
            Case OptiPatcherSource.Alternate
                release = config.AlternateRelease
                If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
                    release = Await OptiPatcherReleaseService.GetAlternateReleaseAsync()
                End If
        End Select

        If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
            Throw New InvalidOperationException("OptiPatcher release download URL is not available.")
        End If

        Dim safeName As String = If(String.IsNullOrWhiteSpace(release.AssetName), "OptiPatcher.asi", release.AssetName)
        Dim destination As String = Path.Combine(tempRoot, Guid.NewGuid().ToString("N") & "_" & safeName)
        log?.Invoke("Downloading OptiPatcher " & If(release.TagName, "") & "...")
        Await DownloadFileAsync(release.DownloadUrl, destination)
        log?.Invoke("OptiPatcher download complete.")

        Return New SourceFileResult With {
            .FilePath = destination,
            .Release = release,
            .AssetName = safeName,
            .SourceUrl = release.DownloadUrl
        }
    End Function

    Private Shared Async Function DownloadFileAsync(url As String, destination As String) As Task
        Dim delay As TimeSpan = TimeSpan.FromMilliseconds(500)
        For attempt As Integer = 1 To MaxDownloadAttempts
            Dim retry As Boolean = False
            Try
                Using client As New HttpClient() With {.Timeout = DownloadTimeout}
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
                    Using response As HttpResponseMessage = Await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                        response.EnsureSuccessStatusCode()
                        Using input As Stream = Await response.Content.ReadAsStreamAsync()
                            Using output As FileStream = File.Create(destination)
                                Await input.CopyToAsync(output)
                            End Using
                        End Using
                    End Using
                End Using
                Return
            Catch ex As Exception
                If IsTransientDownloadException(ex) AndAlso attempt < MaxDownloadAttempts Then
                    retry = True
                    ErrorLogger.Log(ex, "OptiPatcherInstallerService.Download.Retry")
                Else
                    Throw
                End If
            End Try

            If retry Then
                Await Task.Delay(delay)
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2)
            End If
        Next
    End Function

    Private Shared Function IsTransientDownloadException(ex As Exception) As Boolean
        If ex Is Nothing Then
            Return False
        End If

        If TypeOf ex Is TaskCanceledException Then
            Return True
        End If

        Dim http As HttpRequestException = TryCast(ex, HttpRequestException)
        If http IsNot Nothing Then
            If Not http.StatusCode.HasValue Then
                Return True
            End If

            Dim code As Integer = CInt(http.StatusCode.Value)
            If code >= 500 OrElse code = 408 OrElse code = 429 Then
                Return True
            End If
        End If

        Return False
    End Function

    Private Shared Function ResolvePluginFolder(config As OptiPatcherInstallConfig, iniPath As String) As String
        Dim gameFolder As String = config.GameFolder
        Dim fromOverride As String = NormalizePath(gameFolder, config.PluginPathOverride)
        If Not String.IsNullOrWhiteSpace(fromOverride) Then
            Return fromOverride
        End If

        Dim fromIni As String = OptiPatcherInstallDetector.ResolvePluginFolderFromIni(gameFolder)
        If Not String.IsNullOrWhiteSpace(fromIni) Then
            Return fromIni
        End If

        Return Path.Combine(gameFolder, "plugins")
    End Function

    Private Shared Sub EnsureAsiLoadingEnabled(gameFolder As String,
                                               iniPath As String,
                                               pluginFolder As String,
                                               pluginPathOverride As String,
                                               log As Action(Of String))
        Dim updates As New List(Of IniUpdate) From {
            New IniUpdate With {.Section = "Plugins", .Key = "LoadAsiPlugins", .Value = "true"}
        }

        Dim currentPath As String = ReadIniValue(iniPath, "Plugins", "Path")
        Dim pathToPersist As String = If(pluginPathOverride, "").Trim()

        If String.IsNullOrWhiteSpace(pathToPersist) Then
            If String.IsNullOrWhiteSpace(currentPath) Then
                Dim relative As String = GetRelativePathSafe(gameFolder, pluginFolder)
                If String.IsNullOrWhiteSpace(relative) OrElse relative = "." Then
                    relative = "plugins"
                End If
                pathToPersist = relative
            End If
        End If

        If Not String.IsNullOrWhiteSpace(pathToPersist) Then
            updates.Add(New IniUpdate With {.Section = "Plugins", .Key = "Path", .Value = pathToPersist})
        End If

        IniFile.SetValues(iniPath, updates)
        log?.Invoke("Enabled ASI plugin loading in OptiScaler.ini.")
    End Sub

    Private Shared Function ReadIniValue(path As String, section As String, key As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return ""
        End If

        Try
            For Each entry As IniUpdate In IniFile.ReadValues(path)
                If entry Is Nothing Then
                    Continue For
                End If

                If String.Equals(entry.Section, section, StringComparison.OrdinalIgnoreCase) AndAlso
                   String.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase) Then
                    Return If(entry.Value, "")
                End If
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallerService.ReadIniValue")
        End Try

        Return ""
    End Function

    Private Shared Function NormalizePath(baseFolder As String, pathValue As String) As String
        If String.IsNullOrWhiteSpace(pathValue) Then
            Return ""
        End If

        Try
            Dim candidate As String = pathValue.Trim().Replace(IO.Path.AltDirectorySeparatorChar, IO.Path.DirectorySeparatorChar)
            If Not IO.Path.IsPathRooted(candidate) AndAlso Not String.IsNullOrWhiteSpace(baseFolder) Then
                candidate = IO.Path.Combine(baseFolder, candidate)
            End If

            candidate = IO.Path.GetFullPath(candidate)
            Return candidate.TrimEnd(IO.Path.DirectorySeparatorChar, IO.Path.AltDirectorySeparatorChar)
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallerService.NormalizePath")
            Return ""
        End Try
    End Function

    Private Shared Function GetRelativePathSafe(baseFolder As String, value As String) As String
        Try
            If String.IsNullOrWhiteSpace(baseFolder) OrElse String.IsNullOrWhiteSpace(value) Then
                Return ""
            End If

            Dim relative As String = Path.GetRelativePath(baseFolder, value)
            Return relative.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallerService.GetRelativePathSafe")
            Return ""
        End Try
    End Function

    Private Shared Sub SaveManifest(gameFolder As String, manifest As OptiPatcherManifest)
        If manifest Is Nothing OrElse String.IsNullOrWhiteSpace(gameFolder) Then
            Return
        End If

        Try
            Dim path As String = OptiPatcherInstallDetector.GetManifestPath(gameFolder)
            Dim json As String = JsonSerializer.Serialize(manifest, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(path, json)
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallerService.SaveManifest")
        End Try
    End Sub

    Private Shared Function ComputeSha256(path As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return ""
        End If

        Using stream As FileStream = File.OpenRead(path)
            Using sha As SHA256 = SHA256.Create()
                Dim hash As Byte() = sha.ComputeHash(stream)
                Return Convert.ToHexString(hash).ToLowerInvariant()
            End Using
        End Using
    End Function

    Private Shared Sub VerifyReleaseDigest(release As ReleaseInfo, sha256 As String, log As Action(Of String))
        If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.AssetDigest) Then
            Return
        End If

        Dim expected As String = ""
        If Not TryExtractSha256Digest(release.AssetDigest, expected) Then
            log?.Invoke("OptiPatcher digest format not recognized; skipping digest validation.")
            Return
        End If

        If String.IsNullOrWhiteSpace(sha256) Then
            Throw New InvalidOperationException("Failed to calculate OptiPatcher file checksum.")
        End If

        If Not sha256.Equals(expected, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Downloaded OptiPatcher checksum mismatch.")
        End If
    End Sub

    Private Shared Function TryExtractSha256Digest(digestText As String, ByRef sha256 As String) As Boolean
        sha256 = ""
        If String.IsNullOrWhiteSpace(digestText) Then
            Return False
        End If

        Dim cleaned As String = digestText.Trim()
        If cleaned.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) Then
            cleaned = cleaned.Substring("sha256:".Length)
        End If
        cleaned = cleaned.Trim().ToLowerInvariant()

        If Regex.IsMatch(cleaned, "^[a-f0-9]{64}$", RegexOptions.CultureInvariant) Then
            sha256 = cleaned
            Return True
        End If

        Return False
    End Function

    Private Shared Function GetInstallerVersion() As String
        Dim version As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        If version Is Nothing Then
            Return "0.0.0"
        End If
        Return version.ToString()
    End Function
End Class
