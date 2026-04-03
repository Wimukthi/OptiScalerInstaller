Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization

Friend Partial Class frmAbout
    ' Displays installer metadata, author details, and release/version links.
    Inherits Form

    Private Const OptiScalerRepositoryUrl As String = "https://github.com/optiscaler/OptiScaler"
    Private Const SponsorUrl As String = "https://github.com/sponsors/Wimukthi"
    Private ReadOnly _currentVersion As Version
    Private ReadOnly _latestRelease As UpdateReleaseInfo
    Private ReadOnly _repositoryUrl As String
    Private ReadOnly _authorName As String

    ' Constructor accepts current/local version plus latest known upstream release metadata.
    Public Sub New(currentVersion As Version, latestRelease As UpdateReleaseInfo, repositoryUrl As String, authorName As String)
        InitializeComponent()

        _currentVersion = If(currentVersion, New Version(0, 0))
        _latestRelease = latestRelease
        _repositoryUrl = If(repositoryUrl, String.Empty)
        _authorName = If(authorName, String.Empty)

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmAbout.LoadIcon")
        End Try

        ApplyTheme()
        PopulateValues()
    End Sub

    Private Sub ApplyTheme()
        Dim mode As SystemColorMode = ThemeSettings.GetPreferredColorMode()
        ThemeManager.ApplyTheme(Me, mode)

        If mode = SystemColorMode.Dark Then
            linkRepo.LinkColor = Color.DeepSkyBlue
            linkRepo.ActiveLinkColor = Color.CornflowerBlue
            linkRepo.VisitedLinkColor = Color.DodgerBlue
            linkOptiScaler.LinkColor = Color.DeepSkyBlue
            linkOptiScaler.ActiveLinkColor = Color.CornflowerBlue
            linkOptiScaler.VisitedLinkColor = Color.DodgerBlue
            linkSponsor.LinkColor = Color.DeepSkyBlue
            linkSponsor.ActiveLinkColor = Color.CornflowerBlue
            linkSponsor.VisitedLinkColor = Color.DodgerBlue
        Else
            linkRepo.LinkColor = Color.RoyalBlue
            linkRepo.ActiveLinkColor = Color.MediumBlue
            linkRepo.VisitedLinkColor = Color.Purple
            linkOptiScaler.LinkColor = Color.RoyalBlue
            linkOptiScaler.ActiveLinkColor = Color.MediumBlue
            linkOptiScaler.VisitedLinkColor = Color.Purple
            linkSponsor.LinkColor = Color.RoyalBlue
            linkSponsor.ActiveLinkColor = Color.MediumBlue
            linkSponsor.VisitedLinkColor = Color.Purple
        End If
    End Sub

    Private Sub PopulateValues()
        lblCurrentValue.Text = FormatVersionDisplay(_currentVersion, Nothing)

        If _latestRelease IsNot Nothing Then
            lblLatestValue.Text = FormatVersionDisplay(_latestRelease.Version, _latestRelease.TagName)
            If _latestRelease.PublishedAtUtc.HasValue Then
                lblReleaseDateValue.Text = _latestRelease.PublishedAtUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            Else
                lblReleaseDateValue.Text = "Unknown"
            End If
        Else
            lblLatestValue.Text = "Unknown"
            lblReleaseDateValue.Text = "Unknown"
        End If

        lblAuthorValue.Text = If(String.IsNullOrWhiteSpace(_authorName), "Unknown", _authorName)

        Dim repo As String = If(String.IsNullOrWhiteSpace(_repositoryUrl), String.Empty, _repositoryUrl.Trim())
        If String.IsNullOrWhiteSpace(repo) Then
            linkRepo.Text = "Not configured"
            linkRepo.Links.Clear()
            linkRepo.Enabled = False
        Else
            linkRepo.Text = repo
            linkRepo.Links.Clear()
            linkRepo.Links.Add(0, repo.Length, repo)
            linkRepo.Enabled = True
        End If

        linkOptiScaler.Text = OptiScalerRepositoryUrl
        linkOptiScaler.Links.Clear()
        linkOptiScaler.Links.Add(0, OptiScalerRepositoryUrl.Length, OptiScalerRepositoryUrl)
        linkOptiScaler.Enabled = True

        linkSponsor.Text = SponsorUrl
        linkSponsor.Links.Clear()
        linkSponsor.Links.Add(0, SponsorUrl.Length, SponsorUrl)
        linkSponsor.Enabled = True
    End Sub

    Private Function FormatVersionDisplay(version As Version, fallbackTag As String) As String
        If version Is Nothing Then
            Return If(String.IsNullOrWhiteSpace(fallbackTag), "Unknown", fallbackTag)
        End If

        Dim build As Integer = If(version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version.Revision >= 0, version.Revision, 0)
        Dim text As String = $"{version.Major}.{version.Minor}.{build}.{revision}"

        If version.Major = 0 AndAlso version.Minor = 0 AndAlso build = 0 AndAlso revision = 0 AndAlso Not String.IsNullOrWhiteSpace(fallbackTag) Then
            Return fallbackTag
        End If

        Return text
    End Function

    Private Sub linkRepo_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkRepo.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = _repositoryUrl
        End If

        If String.IsNullOrWhiteSpace(target) Then
            Return
        End If

        Try
            Process.Start(New ProcessStartInfo(target) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmAbout.OpenRepository")
            MessageBox.Show(Me, "Unable to open the repository link.", "About", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub linkOptiScaler_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkOptiScaler.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = OptiScalerRepositoryUrl
        End If

        If String.IsNullOrWhiteSpace(target) Then
            Return
        End If

        Try
            Process.Start(New ProcessStartInfo(target) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmAbout.OpenOptiScalerRepository")
            MessageBox.Show(Me, "Unable to open the OptiScaler repository link.", "About", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub linkSponsor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkSponsor.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = SponsorUrl
        End If

        If String.IsNullOrWhiteSpace(target) Then
            Return
        End If

        Try
            Process.Start(New ProcessStartInfo(target) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmAbout.OpenSponsor")
            MessageBox.Show(Me, "Unable to open the sponsor link.", "About", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub
End Class
