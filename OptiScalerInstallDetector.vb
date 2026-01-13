Imports System.IO
Imports System.Diagnostics
Imports System.Text.Json

Public Class OptiScalerInstallInfo
    Public Property IsInstalled As Boolean
    Public Property Version As String
    Public Property Source As String
    Public Property Manifest As InstallManifest
    Public Property HookFilePath As String
End Class

Public Module OptiScalerInstallDetector
    Private ReadOnly KnownHookNames As String() = {
        "OptiScaler.dll",
        "dxgi.dll",
        "winmm.dll",
        "version.dll",
        "dbghelp.dll",
        "d3d12.dll",
        "wininet.dll",
        "winhttp.dll",
        "OptiScaler.asi"
    }

    Public Function Detect(gameFolder As String) As OptiScalerInstallInfo
        Dim info As New OptiScalerInstallInfo()
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return info
        End If

        Dim manifest As InstallManifest = TryLoadManifest(gameFolder)
        If manifest IsNot Nothing Then
            info.IsInstalled = True
            info.Manifest = manifest
            info.Source = "Manifest"
            info.HookFilePath = GetHookFilePath(gameFolder, manifest.HookName)
            info.Version = If(String.IsNullOrWhiteSpace(manifest.OptiScalerVersion), "", manifest.OptiScalerVersion)
            If String.IsNullOrWhiteSpace(info.Version) Then
                info.Version = TryGetFileVersion(info.HookFilePath)
            End If
            Return info
        End If

        Dim iniPath As String = Path.Combine(gameFolder, "OptiScaler.ini")
        Dim hookPath As String = GetHookFilePath(gameFolder, Nothing)

        If Not File.Exists(iniPath) AndAlso String.IsNullOrWhiteSpace(hookPath) Then
            Return info
        End If

        info.IsInstalled = True
        info.Source = If(File.Exists(iniPath), "OptiScaler.ini", "Files")
        info.HookFilePath = hookPath
        info.Version = TryGetFileVersion(hookPath)
        Return info
    End Function

    Private Function TryLoadManifest(gameFolder As String) As InstallManifest
        Dim manifestPath As String = Path.Combine(gameFolder, "OptiScalerInstaller.manifest.json")
        If Not File.Exists(manifestPath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(manifestPath)
            Dim manifest As InstallManifest = JsonSerializer.Deserialize(Of InstallManifest)(json)
            If manifest IsNot Nothing Then
                Return manifest
            End If
        Catch
        End Try

        Return New InstallManifest()
    End Function

    Private Function GetHookFilePath(gameFolder As String, preferredHook As String) As String
        If Not String.IsNullOrWhiteSpace(preferredHook) Then
            Dim preferredPath As String = Path.Combine(gameFolder, preferredHook)
            If File.Exists(preferredPath) Then
                Return preferredPath
            End If
        End If

        For Each name As String In KnownHookNames
            Dim candidate As String = Path.Combine(gameFolder, name)
            If File.Exists(candidate) Then
                Return candidate
            End If
        Next

        Return ""
    End Function

    Private Function TryGetFileVersion(path As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return ""
        End If

        Try
            Dim info As FileVersionInfo = FileVersionInfo.GetVersionInfo(path)
            Dim version As String = info.FileVersion
            If String.IsNullOrWhiteSpace(version) Then
                version = info.ProductVersion
            End If
            Return If(version, "")
        Catch
            Return ""
        End Try
    End Function
End Module
