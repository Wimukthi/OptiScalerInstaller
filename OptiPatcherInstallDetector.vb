Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json

Public Module OptiPatcherInstallDetector
    ' Detects existing OptiPatcher installs via manifest and strong plugin file markers.
    Public Const ManifestFileName As String = "OptiPatcherInstaller.manifest.json"

    Public Function Detect(gameFolder As String) As OptiPatcherInstallInfo
        Dim info As New OptiPatcherInstallInfo()
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return info
        End If

        Dim manifest As OptiPatcherManifest = TryLoadManifest(gameFolder)
        If manifest IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(manifest.AsiPath) AndAlso File.Exists(manifest.AsiPath) Then
            info.IsInstalled = True
            info.Manifest = manifest
            info.Source = "Manifest"
            info.AsiPath = manifest.AsiPath
            info.PluginFolder = manifest.PluginFolder
            info.Version = If(String.IsNullOrWhiteSpace(manifest.OptiPatcherVersion), TryGetFileVersion(manifest.AsiPath), manifest.OptiPatcherVersion)
            Return info
        End If

        Dim pluginFolders As List(Of String) = ResolvePluginFolderCandidates(gameFolder)
        For Each folder As String In pluginFolders
            Dim candidate As String = Path.Combine(folder, "OptiPatcher.asi")
            If Not File.Exists(candidate) Then
                Continue For
            End If

            info.IsInstalled = True
            info.Source = "Plugin file"
            info.AsiPath = candidate
            info.PluginFolder = folder
            info.Version = TryGetFileVersion(candidate)
            Return info
        Next

        Return info
    End Function

    Public Function ResolvePluginFolderCandidates(gameFolder As String) As List(Of String)
        Dim folders As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        AddFolder(folders, seen, ResolvePluginFolderFromIni(gameFolder))
        AddFolder(folders, seen, Path.Combine(gameFolder, "plugins"))
        AddFolder(folders, seen, gameFolder)

        Return folders
    End Function

    Public Function ResolvePluginFolderFromIni(gameFolder As String) As String
        If String.IsNullOrWhiteSpace(gameFolder) Then
            Return ""
        End If

        Dim iniPath As String = Path.Combine(gameFolder, "OptiScaler.ini")
        If Not File.Exists(iniPath) Then
            Return ""
        End If

        Try
            Dim updates As List(Of IniUpdate) = IniFile.ReadValues(iniPath)
            For Each update As IniUpdate In updates
                If update Is Nothing Then
                    Continue For
                End If

                If Not String.Equals(update.Section, "Plugins", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                If Not String.Equals(update.Key, "Path", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Dim configuredPath As String = If(update.Value, "").Trim()
                If String.IsNullOrWhiteSpace(configuredPath) Then
                    Return ""
                End If

                Return NormalizePath(gameFolder, configuredPath)
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallDetector.ResolvePluginFolderFromIni")
        End Try

        Return ""
    End Function

    Public Function GetManifestPath(gameFolder As String) As String
        If String.IsNullOrWhiteSpace(gameFolder) Then
            Return ""
        End If

        Return Path.Combine(gameFolder, ManifestFileName)
    End Function

    Private Function TryLoadManifest(gameFolder As String) As OptiPatcherManifest
        Dim manifestPath As String = GetManifestPath(gameFolder)
        If String.IsNullOrWhiteSpace(manifestPath) OrElse Not File.Exists(manifestPath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(manifestPath)
            Dim manifest As OptiPatcherManifest = JsonSerializer.Deserialize(Of OptiPatcherManifest)(json)
            If manifest Is Nothing Then
                Return Nothing
            End If

            If String.IsNullOrWhiteSpace(manifest.AsiPath) Then
                Return Nothing
            End If

            Dim normalizedGameRoot As String = NormalizePath(gameFolder, ".")
            Dim normalizedAsiPath As String = NormalizePath(gameFolder, manifest.AsiPath)
            If String.IsNullOrWhiteSpace(normalizedGameRoot) OrElse String.IsNullOrWhiteSpace(normalizedAsiPath) Then
                Return Nothing
            End If

            If Not normalizedAsiPath.StartsWith(normalizedGameRoot, StringComparison.OrdinalIgnoreCase) Then
                Return Nothing
            End If

            manifest.AsiPath = normalizedAsiPath
            manifest.PluginFolder = NormalizePath(gameFolder, manifest.PluginFolder)
            Return manifest
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallDetector.TryLoadManifest")
            Return Nothing
        End Try
    End Function

    Private Sub AddFolder(target As List(Of String), seen As HashSet(Of String), value As String)
        If target Is Nothing OrElse seen Is Nothing Then
            Return
        End If

        If String.IsNullOrWhiteSpace(value) Then
            Return
        End If

        Dim normalized As String = NormalizePath("", value)
        If String.IsNullOrWhiteSpace(normalized) Then
            Return
        End If

        If seen.Contains(normalized) Then
            Return
        End If

        seen.Add(normalized)
        target.Add(normalized)
    End Sub

    Private Function NormalizePath(baseFolder As String, pathValue As String) As String
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
            ErrorLogger.Log(ex, "OptiPatcherInstallDetector.NormalizePath")
            Return ""
        End Try
    End Function

    Private Function TryGetFileVersion(path As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return ""
        End If

        Try
            Dim info As FileVersionInfo = FileVersionInfo.GetVersionInfo(path)
            Dim value As String = If(info.FileVersion, "").Trim()
            If String.IsNullOrWhiteSpace(value) Then
                value = If(info.ProductVersion, "").Trim()
            End If
            Return value
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiPatcherInstallDetector.TryGetFileVersion")
            Return ""
        End Try
    End Function
End Module
