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

    Private ReadOnly SupportMarkerFiles As String() = {
        "OptiScaler.log"
    }

    Private ReadOnly SupportMarkerDirectories As String() = {
        "D3D12_Optiscaler",
        "DlssOverrides"
    }

    Private Const ManifestFileName As String = "OptiScalerInstaller.manifest.json"

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
            info.HookFilePath = GetPreferredHookPath(folderPath, manifest.HookName, True)
            info.Version = If(String.IsNullOrWhiteSpace(manifest.OptiScalerVersion), "", manifest.OptiScalerVersion)
            If String.IsNullOrWhiteSpace(info.Version) Then
                info.Version = TryGetFileVersion(info.HookFilePath)
            End If
            Return info
        End If

        Dim hasIni As Boolean = File.Exists(Path.Combine(folderPath, "OptiScaler.ini"))
        Dim hasRemoveBat As Boolean = File.Exists(Path.Combine(folderPath, "Remove OptiScaler.bat"))
        Dim hasSupportMarkers As Boolean = HasSupportInstallMarkers(folderPath)
        Dim specificHookPath As String = GetSpecificHookPath(folderPath, Nothing)
        Dim genericHookPath As String = GetVerifiedGenericHookPath(folderPath, Nothing, hasIni OrElse hasRemoveBat OrElse hasSupportMarkers)
        Dim hookPath As String = If(Not String.IsNullOrWhiteSpace(specificHookPath), specificHookPath, genericHookPath)

        ' Confidence score tuned to avoid false positives from stock game files.
        Dim score As Integer = 0
        If hasIni Then
            score += 70
        End If
        If hasRemoveBat Then
            score += 70
        End If
        If Not String.IsNullOrWhiteSpace(specificHookPath) Then
            score += 70
        End If
        If Not String.IsNullOrWhiteSpace(genericHookPath) Then
            score += 75
        End If
        If hasSupportMarkers Then
            score += 25
        End If

        If score < 70 Then
            Return info
        End If

        info.IsInstalled = True
        If hasIni Then
            info.Source = "OptiScaler.ini"
        ElseIf hasRemoveBat Then
            info.Source = "Remove OptiScaler.bat"
        ElseIf Not String.IsNullOrWhiteSpace(specificHookPath) Then
            info.Source = "OptiScaler hook"
        ElseIf Not String.IsNullOrWhiteSpace(genericHookPath) Then
            info.Source = "Verified hook file"
        ElseIf hasSupportMarkers Then
            info.Source = "OptiScaler support files"
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

                For Each grandChild As String In Directory.EnumerateDirectories(child, "*", SearchOption.TopDirectoryOnly)
                    AddKnownProbePaths(folders, grandChild)
                Next
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
            Path.Combine(baseFolder, "Binaries", "Win32"),
            Path.Combine(baseFolder, "Binaries", "WinGDK"),
            Path.Combine(baseFolder, "Engine", "Binaries", "Win64"),
            Path.Combine(baseFolder, "bin"),
            Path.Combine(baseFolder, "bin", "x64"),
            Path.Combine(baseFolder, "bin", "Win64"),
            Path.Combine(baseFolder, "x64"),
            Path.Combine(baseFolder, "Win64"),
            Path.Combine(baseFolder, "Win32"),
            Path.Combine(baseFolder, "WinGDK")
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
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestFileName)
        If Not File.Exists(manifestPath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(manifestPath)
            Dim manifest As InstallManifest = JsonSerializer.Deserialize(Of InstallManifest)(json)
            If IsManifestValid(gameFolder, manifest) Then
                Return manifest
            End If
        Catch ex As Exception
            ErrorLogger.Log(ex, "OptiScalerInstallDetector.TryLoadManifest")
        End Try

        Return Nothing
    End Function

    Private Function IsManifestValid(gameFolder As String, manifest As InstallManifest) As Boolean
        If manifest Is Nothing OrElse String.IsNullOrWhiteSpace(gameFolder) Then
            Return False
        End If

        If Not String.IsNullOrWhiteSpace(manifest.HookName) Then
            Dim hookPath As String = Path.Combine(gameFolder, manifest.HookName)
            If File.Exists(hookPath) Then
                Return True
            End If
        End If

        If manifest.InstalledFiles IsNot Nothing Then
            For Each installedPath As String In manifest.InstalledFiles
                If String.IsNullOrWhiteSpace(installedPath) Then
                    Continue For
                End If

                Try
                    Dim fullPath As String = Path.GetFullPath(installedPath)
                    If File.Exists(fullPath) OrElse Directory.Exists(fullPath) Then
                        Return True
                    End If
                Catch ex As Exception
                    ErrorLogger.Log(ex, "OptiScalerInstallDetector.IsManifestValid.Path")
                End Try
            Next
        End If

        Dim hasIni As Boolean = File.Exists(Path.Combine(gameFolder, "OptiScaler.ini"))
        Dim hasRemoveBat As Boolean = File.Exists(Path.Combine(gameFolder, "Remove OptiScaler.bat"))
        Dim hasSpecificHook As Boolean = Not String.IsNullOrWhiteSpace(GetSpecificHookPath(gameFolder, Nothing))
        Dim hasVerifiedGenericHook As Boolean = Not String.IsNullOrWhiteSpace(GetVerifiedGenericHookPath(gameFolder, Nothing, False))
        Return hasIni OrElse hasRemoveBat OrElse hasSpecificHook OrElse hasVerifiedGenericHook
    End Function

    Private Function HasSupportInstallMarkers(gameFolder As String) As Boolean
        For Each fileName As String In SupportMarkerFiles
            If File.Exists(Path.Combine(gameFolder, fileName)) Then
                Return True
            End If
        Next

        For Each folderName As String In SupportMarkerDirectories
            If Directory.Exists(Path.Combine(gameFolder, folderName)) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function GetPreferredHookPath(gameFolder As String, preferredHook As String, allowGenericFallback As Boolean) As String
        Dim specificHook As String = GetSpecificHookPath(gameFolder, preferredHook)
        If Not String.IsNullOrWhiteSpace(specificHook) Then
            Return specificHook
        End If

        Return GetVerifiedGenericHookPath(gameFolder, preferredHook, allowGenericFallback)
    End Function

    Private Function GetSpecificHookPath(gameFolder As String, preferredHook As String) As String
        If Not String.IsNullOrWhiteSpace(preferredHook) Then
            For Each name As String In SpecificHookNames
                If preferredHook.Equals(name, StringComparison.OrdinalIgnoreCase) Then
                    Dim preferredSpecific As String = Path.Combine(gameFolder, preferredHook)
                    If File.Exists(preferredSpecific) Then
                        Return preferredSpecific
                    End If
                    Exit For
                End If
            Next
        End If

        For Each name As String In SpecificHookNames
            Dim candidate As String = Path.Combine(gameFolder, name)
            If File.Exists(candidate) Then
                Return candidate
            End If
        Next

        Return ""
    End Function

    Private Function GetVerifiedGenericHookPath(gameFolder As String, preferredHook As String, allowGenericFallback As Boolean) As String
        If Not String.IsNullOrWhiteSpace(preferredHook) Then
            For Each name As String In GenericHookNames
                If preferredHook.Equals(name, StringComparison.OrdinalIgnoreCase) Then
                    Dim preferredPath As String = Path.Combine(gameFolder, preferredHook)
                    If File.Exists(preferredPath) AndAlso (allowGenericFallback OrElse IsLikelyOptiScalerBinary(preferredPath)) Then
                        Return preferredPath
                    End If
                    Exit For
                End If
            Next
        End If

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
