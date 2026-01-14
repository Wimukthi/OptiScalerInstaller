Imports System.IO
Imports System.IO.Compression
Imports System.Diagnostics
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Friend Partial Class frmUpdate
    ' Minimal update UI for downloading and applying installer releases.
    Inherits Form

    Private ReadOnly _release As UpdateReleaseInfo
    Private ReadOnly _asset As UpdateAssetInfo
    Private ReadOnly _currentVersion As Version
    Private _cts As CancellationTokenSource
    Private _tempRoot As String
    Private _downloadPath As String

    Public Sub New(release As UpdateReleaseInfo, currentVersion As Version)
        InitializeComponent()

        _release = release
        _currentVersion = currentVersion
        _asset = UpdateService.SelectBestAsset(release)

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmUpdate.LoadIcon")
        End Try

        AddHandler btnDownload.Click, AddressOf DownloadClicked
        AddHandler btnCancel.Click, AddressOf CancelClicked
        AddHandler btnRelease.Click, AddressOf OpenReleaseClicked

        PopulateReleaseInfo()
        ApplyTheme()

        If _asset Is Nothing Then
            btnDownload.Enabled = False
            lblStatus.Text = "No compatible update package was found."
        End If
    End Sub

    Private Sub PopulateReleaseInfo()
        lblCurrentValue.Text = FormatVersionDisplay(_currentVersion, Nothing)
        lblLatestValue.Text = If(_release IsNot Nothing, FormatVersionDisplay(_release.Version, _release.TagName), "Unknown")
        lblPackageValue.Text = If(_asset IsNot Nothing, _asset.Name, "No compatible asset found")
        txtReleaseNotes.Text = If(_release IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(_release.Notes), _release.Notes, "No release notes provided.")
        btnRelease.Enabled = (_release IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(_release.HtmlUrl))
    End Sub

    Private Sub ApplyTheme()
        Dim mode As SystemColorMode = ThemeSettings.GetPreferredColorMode()
        ThemeManager.ApplyTheme(Me, mode)
    End Sub

    Private Function FormatVersionDisplay(version As Version, tagName As String) As String
        If version Is Nothing Then
            Return If(String.IsNullOrWhiteSpace(tagName), "Unknown", tagName)
        End If

        Dim build As Integer = If(version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version.Revision >= 0, version.Revision, 0)
        Dim text As String = $"{version.Major}.{version.Minor}.{build}.{revision}"
        If version.Major = 0 AndAlso version.Minor = 0 AndAlso build = 0 AndAlso revision = 0 AndAlso Not String.IsNullOrWhiteSpace(tagName) Then
            Return tagName
        End If
        Return text
    End Function

    Private Async Sub DownloadClicked(sender As Object, e As EventArgs)
        If _asset Is Nothing OrElse _cts IsNot Nothing Then
            Return
        End If

        _cts = New CancellationTokenSource()
        btnDownload.Enabled = False
        btnCancel.Text = "Cancel"
        progressDownload.Value = 0
        lblStatus.Text = "Downloading update..."

        Try
            _tempRoot = CreateTempRoot()
            Directory.CreateDirectory(_tempRoot)
            _downloadPath = Path.Combine(_tempRoot, _asset.Name)

            Dim progress As New Progress(Of DownloadProgressInfo)(AddressOf UpdateProgress)
            Await UpdateService.DownloadAssetAsync(_asset, _downloadPath, progress, _cts.Token)

            If _cts.IsCancellationRequested Then
                lblStatus.Text = "Download canceled."
                btnCancel.Text = "Close"
                btnDownload.Enabled = True
                _cts = Nothing
                Return
            End If

            lblStatus.Text = "Preparing update..."
            Await ApplyUpdateAsync(_downloadPath)
        Catch ex As OperationCanceledException
            lblStatus.Text = "Download canceled."
            btnCancel.Text = "Close"
            btnDownload.Enabled = True
        Catch ex As Exception
            lblStatus.Text = "Update failed: " & ex.Message
            btnCancel.Text = "Close"
            btnDownload.Enabled = True
            ErrorLogger.Log(ex, "frmUpdate.DownloadUpdate")
        Finally
            _cts = Nothing
        End Try
    End Sub

    Private Sub CancelClicked(sender As Object, e As EventArgs)
        If _cts IsNot Nothing Then
            lblStatus.Text = "Canceling..."
            _cts.Cancel()
            Return
        End If
        Close()
    End Sub

    Private Sub OpenReleaseClicked(sender As Object, e As EventArgs)
        If _release Is Nothing OrElse String.IsNullOrWhiteSpace(_release.HtmlUrl) Then
            Return
        End If
        Try
            Process.Start(New ProcessStartInfo(_release.HtmlUrl) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmUpdate.OpenRelease")
        End Try
    End Sub

    Private Sub UpdateProgress(info As DownloadProgressInfo)
        progressDownload.Value = Math.Max(0, Math.Min(100, info.Percent))
        Dim totalText As String = If(info.TotalBytes > 0, UpdateService.FormatBytes(info.TotalBytes), "Unknown")
        lblProgress.Text = $"{UpdateService.FormatBytes(info.BytesReceived)} / {totalText}"
        Dim speedText As String = If(info.SpeedBytesPerSec > 0, UpdateService.FormatBytes(CLng(info.SpeedBytesPerSec)) & "/s", "N/A")
        lblSpeed.Text = "Speed: " & speedText
    End Sub

    Private Async Function ApplyUpdateAsync(downloadPath As String) As Task
        If String.IsNullOrWhiteSpace(downloadPath) OrElse Not File.Exists(downloadPath) Then
            Throw New FileNotFoundException("Downloaded update package was not found.")
        End If

        Dim targetDir As String = AppContext.BaseDirectory
        Dim exeName As String = Path.GetFileName(Application.ExecutablePath)
        Dim extension As String = Path.GetExtension(downloadPath).ToLowerInvariant()

        If Not CanWriteToFolder(targetDir) Then
            Throw New InvalidOperationException("The application folder is not writable. Please run as administrator.")
        End If

        If extension = ".exe" Then
            Process.Start(New ProcessStartInfo(downloadPath) With {.UseShellExecute = True})
            Application.Exit()
            Return
        End If

        If extension <> ".zip" Then
            Throw New InvalidOperationException("Unsupported update package format.")
        End If

        Dim extractRoot As String = Path.Combine(_tempRoot, "payload")
        If Directory.Exists(extractRoot) Then
            Directory.Delete(extractRoot, True)
        End If
        Directory.CreateDirectory(extractRoot)

        Await Task.Run(Sub() ZipFile.ExtractToDirectory(downloadPath, extractRoot))

        Dim exeCandidates As String() = Directory.GetFiles(extractRoot, exeName, SearchOption.AllDirectories)
        Dim exePath As String = If(exeCandidates.Length > 0, exeCandidates(0), "")
        Dim sourceDir As String = If(String.IsNullOrWhiteSpace(exePath), extractRoot, Path.GetDirectoryName(exePath))

        If String.IsNullOrWhiteSpace(sourceDir) OrElse Not Directory.Exists(sourceDir) Then
            Throw New InvalidOperationException("Unable to locate update files.")
        End If

        LaunchUpdateScript(sourceDir, targetDir, exeName, _tempRoot)
        Application.Exit()
    End Function

    Private Function CreateTempRoot() As String
        Dim baseDir As String = Path.Combine(Path.GetTempPath(), "OptiScalerInstallerUpdate")
        Dim id As String = Guid.NewGuid().ToString("N")
        Return Path.Combine(baseDir, id)
    End Function

    Private Function CanWriteToFolder(folder As String) As Boolean
        Try
            Dim testPath As String = Path.Combine(folder, "write-test.tmp")
            File.WriteAllText(testPath, "x")
            File.Delete(testPath)
            Return True
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmUpdate.CanWriteToFolder")
            Return False
        End Try
    End Function

    Private Sub LaunchUpdateScript(sourceDir As String, targetDir As String, exeName As String, tempRoot As String)
        Dim scriptPath As String = Path.Combine(tempRoot, "apply-update.cmd")
        Dim pid As Integer = Process.GetCurrentProcess().Id
        Dim exePath As String = Path.Combine(targetDir, exeName)

        Dim sb As New StringBuilder()
        sb.AppendLine("@echo off")
        sb.AppendLine("setlocal")
        sb.AppendLine($"set ""PID={pid}""")
        sb.AppendLine($"set ""SOURCE={sourceDir}""")
        sb.AppendLine($"set ""TARGET={targetDir.TrimEnd(Path.DirectorySeparatorChar)}""")
        sb.AppendLine(":wait")
        sb.AppendLine("tasklist /FI ""PID eq %PID%"" | find /I ""%PID%"" >nul")
        sb.AppendLine("if not errorlevel 1 (")
        sb.AppendLine("  timeout /t 1 /nobreak >nul")
        sb.AppendLine("  goto wait")
        sb.AppendLine(")")
        sb.AppendLine("robocopy ""%SOURCE%"" ""%TARGET%"" /E /COPY:DAT /R:2 /W:1 /NFL /NDL /NJH /NJS /NP >nul")
        sb.AppendLine($"start """" ""{exePath}""")
        sb.AppendLine("endlocal")

        File.WriteAllText(scriptPath, sb.ToString(), Encoding.ASCII)
        Dim startInfo As New ProcessStartInfo(scriptPath) With {
            .WorkingDirectory = tempRoot,
            .UseShellExecute = True,
            .WindowStyle = ProcessWindowStyle.Hidden
        }
        Process.Start(startInfo)
    End Sub
End Class
