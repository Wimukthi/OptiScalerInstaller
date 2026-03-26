Imports System.IO
Imports System.Diagnostics
Imports System.Text.Json

Public Class OptiScalerInstallInfo
    ' Summarizes detected OptiScaler install state for a game folder.
    Public Property IsInstalled As Boolean
    Public Property Version As String
    Public Property Source As String
    Public Property Manifest As InstallManifest
    Public Property HookFilePath As String
    Public Property InstallFolder As String
End Class

Public Module OptiScalerInstallDetector
    ' Detects existing OptiScaler installs via manifest or strong file markers.
    Private ReadOnly SpecificHookNames As String() = {
        "OptiScaler.dll",
        "OptiScaler.asi"
    }

    Private ReadOnly GenericHookNames As String() = {
        "dxgi.dll",
        "winmm.dll",
        "version.dll",
        "dbghelp.dll",
        "d3d12.dll",
        "wininet.dll",
        "winhttp.dll"
    }

    Private ReadOnly StrongMarkerFiles As String() = {
        "OptiScaler.ini",
        "OptiScaler.log",
        "Remove OptiScaler.bat",
        "OptiScalerInstaller.manifest.json"
    }

    Private ReadOnly StrongMarkerDirectories As String() = {
        "D3D12_Optiscaler",
        "DlssOverrides",
        "Licenses"
    }

    Public Function Detect(gameFolder As String) As OptiScalerInstallInfo
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return New OptiScalerInstallInfo()
        End If

        Dim direct As OptiScalerInstallInfo = DetectInFolder(gameFolder)
        If direct IsNot Nothing AndAlso direct.IsInstalled Then
            Return direct
        End If

        For Each nestedFolder As String In GetNestedProbeFolders(gameFolder)
            Dim nested As OptiScalerInstallInfo = DetectInFolder(nestedFolder)
            If nested IsNot Nothing AndAlso nested.IsInstalled Then
                If String.IsNullOrWhiteSpace(nested.InstallFolder) Then
                    nested.InstallFolder = nestedFolder
                End If

                Dim relative As String = TryGetRelativeFolder(gameFolder, nested.InstallFolder)
                If Not String.IsNullOrWhiteSpace(relative) Then
                    If String.IsNullOrWhiteSpace(nested.Source) Then
                        nested.Source = relative
                    Else
                        nested.Source &= " @ " & relative
                    End If
                End If
                Return nested
            End If
        Next

        Return direct
    End Function

    Private Function DetectInFolder(folderPath As String) As OptiScalerInstallInfo
        Dim info As New OptiScalerInstallInfo()
        info.InstallFolder = folderPath

        If String.IsNullOrWhiteSpace(folderPath) OrElse Not Directory.Exists(folderPath) Then
            Return info
        End If

        Dim manifest As InstallManifest = TryLoadManifest(folderPath)
        If manifest IsNot Nothing Then
            info.IsInstalled = True
            info.Manifest = manifest
            info.Source = "Manifest"
            info.HookFilePath = GetHookFilePath(folderPath, manifest.HookName, True)
            info.Version = If(String.IsNullOrWhiteSpace(manifest.OptiScalerVersion), "", manifest.OptiScalerVersion)
            If String.IsNullOrWhiteSpace(info.Version) Then
                info.Version = TryGetFileVersion(info.HookFilePath)
            End If
            Return info
        End If

        Dim iniPath As String = Path.Combine(folderPath, "OptiScaler.ini")
        Dim hasIni As Boolean = File.Exists(iniPath)
        Dim hasStrongMarkers As Boolean = HasStrongInstallMarkers(folderPath)
        Dim hookPath As String = GetHookFilePath(folderPath, Nothing, hasIni OrElse hasStrongMarkers)

        If Not hasIni AndAlso Not hasStrongMarkers AndAlso String.IsNullOrWhiteSpace(hookPath) Then
            Return info
        End If

        info.IsInstalled = True
        If hasIni Then
            info.Source = "OptiScaler.ini"
        ElseIf hasStrongMarkers Then
            info.Source = "OptiScaler files"
        Else
            info.Source = "Hook file"
        End If
        info.HookFilePath = hookPath
        info.Version = TryGetFileVersion(hookPath)
        Return info
    End Function

    Private Function GetNestedProbeFolders(gameFolder As String) As IEnumerable(Of String)
        Dim folders As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        AddKnownProbePaths(folders, gameFolder)

        Try
            For Each child As String In Directory.EnumerateDirectories(gameFolder, "*", SearchOption.TopDirectoryOnly)
                AddKnownProbePaths(folders, child)
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiScalerInstallDetector.GetNestedProbeFolders")
        End Try

        Return folders
    End Function

    Private Sub AddKnownProbePaths(target As HashSet(Of String), baseFolder As String)
        If String.IsNullOrWhiteSpace(baseFolder) OrElse target Is Nothing Then
            Return
        End If

        Dim candidates As String() = {
            Path.Combine(baseFolder, "Binaries", "Win64"),
            Path.Combine(baseFolder, "Engine", "Binaries", "Win64"),
            Path.Combine(baseFolder, "bin"),
            Path.Combine(baseFolder, "bin", "x64"),
            Path.Combine(baseFolder, "x64"),
            Path.Combine(baseFolder, "Win64")
        }

        For Each candidate As String In candidates
            If Directory.Exists(candidate) Then
                target.Add(candidate)
            End If
        Next
    End Sub

    Private Function TryGetRelativeFolder(rootFolder As String, childFolder As String) As String
        If String.IsNullOrWhiteSpace(rootFolder) OrElse String.IsNullOrWhiteSpace(childFolder) Then
            Return String.Empty
        End If

        Try
            Dim relative As String = IO.Path.GetRelativePath(rootFolder, childFolder)
            If String.IsNullOrWhiteSpace(relative) OrElse relative = "." Then
                Return String.Empty
            End If
            Return relative
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiScalerInstallDetector.TryGetRelativeFolder")
            Return String.Empty
        End Try
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
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiScalerInstallDetector.TryLoadManifest")
        End Try

        Return New InstallManifest()
    End Function

    Private Function HasStrongInstallMarkers(gameFolder As String) As Boolean
        For Each fileName As String In StrongMarkerFiles
            If File.Exists(Path.Combine(gameFolder, fileName)) Then
                Return True
            End If
        Next

        For Each folderName As String In StrongMarkerDirectories
            If Directory.Exists(Path.Combine(gameFolder, folderName)) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function GetHookFilePath(gameFolder As String, preferredHook As String, allowGenericFallback As Boolean) As String
        If Not String.IsNullOrWhiteSpace(preferredHook) Then
            Dim preferredPath As String = Path.Combine(gameFolder, preferredHook)
            If File.Exists(preferredPath) AndAlso (allowGenericFallback OrElse IsLikelyOptiScalerBinary(preferredPath)) Then
                Return preferredPath
            End If
        End If

        For Each name As String In SpecificHookNames
            Dim candidate As String = Path.Combine(gameFolder, name)
            If File.Exists(candidate) Then
                Return candidate
            End If
        Next

        For Each name As String In GenericHookNames
            Dim candidate As String = Path.Combine(gameFolder, name)
            If File.Exists(candidate) AndAlso (allowGenericFallback OrElse IsLikelyOptiScalerBinary(candidate)) Then
                Return candidate
            End If
        Next

        Return ""
    End Function

    Private Function IsLikelyOptiScalerBinary(path As String) As Boolean
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return False
        End If

        Try
            Dim fileName As String = IO.Path.GetFileName(path)
            If fileName.Equals("OptiScaler.dll", StringComparison.OrdinalIgnoreCase) OrElse
               fileName.Equals("OptiScaler.asi", StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If

            Dim meta As FileVersionInfo = FileVersionInfo.GetVersionInfo(path)
            Dim fields As String() = {
                meta.FileDescription,
                meta.ProductName,
                meta.CompanyName,
                meta.OriginalFilename,
                meta.InternalName,
                meta.Comments
            }

            For Each value As String In fields
                If Not String.IsNullOrWhiteSpace(value) AndAlso value.Contains("optiscaler", StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiScalerInstallDetector.IsLikelyOptiScalerBinary")
        End Try

        Return False
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
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiScalerInstallDetector.TryGetFileVersion")
            Return ""
        End Try
    End Function
End Module
