Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks

Friend Partial Class frmAbout
    ' Displays installer metadata, author details, and release/version links.
    Inherits Form

    Private Const OptiScalerRepositoryUrl As String = "https://github.com/optiscaler/OptiScaler"
    Private Const SponsorUrl As String = "https://github.com/sponsors/Wimukthi"
    Private ReadOnly _currentVersion As Version
    Private ReadOnly _latestRelease As UpdateReleaseInfo
    Private ReadOnly _repositoryUrl As String
    Private ReadOnly _authorName As String
    Private _sponsorsLoaded As Boolean
    Private _sponsorsLoading As Boolean

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
            linkSponsorsPage.LinkColor = Color.DeepSkyBlue
            linkSponsorsPage.ActiveLinkColor = Color.CornflowerBlue
            linkSponsorsPage.VisitedLinkColor = Color.DodgerBlue
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
            linkSponsorsPage.LinkColor = Color.RoyalBlue
            linkSponsorsPage.ActiveLinkColor = Color.MediumBlue
            linkSponsorsPage.VisitedLinkColor = Color.Purple
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

        linkSponsorsPage.Text = "Open GitHub sponsors page"
        linkSponsorsPage.Links.Clear()
        linkSponsorsPage.Links.Add(0, linkSponsorsPage.Text.Length, SponsorUrl)
        linkSponsorsPage.Enabled = True

        lblSponsorsStatus.Text = "Loading sponsors..."
        UpdateSponsorsColumnWidths()
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

    Private Async Sub frmAbout_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If _sponsorsLoaded Then
            Return
        End If

        Await LoadSponsorsAsync(False)
    End Sub

    Private Async Function LoadSponsorsAsync(forceReload As Boolean) As Task
        If _sponsorsLoading Then
            Return
        End If
        If _sponsorsLoaded AndAlso Not forceReload Then
            Return
        End If

        _sponsorsLoading = True
        btnRefreshSponsors.Enabled = False
        lblSponsorsStatus.Text = "Loading public sponsors..."

        Try
            Dim fetch As SponsorsFetchResult = Await FetchSponsorsAsync()
            PopulateSponsors(fetch)
            _sponsorsLoaded = True
        Catch ex As Exception
            lvSponsors.BeginUpdate()
            lvSponsors.Items.Clear()
            lvSponsors.EndUpdate()
            lblSponsorsStatus.Text = "Unable to load sponsors right now."
            ErrorLogger.Log(ex, "frmAbout.LoadSponsors")
        Finally
            btnRefreshSponsors.Enabled = True
            _sponsorsLoading = False
        End Try
    End Function

    Private Async Function FetchSponsorsAsync() As Task(Of SponsorsFetchResult)
        Using client As New HttpClient()
            client.Timeout = TimeSpan.FromSeconds(20)
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html")

            Dim html As String = Await client.GetStringAsync(SponsorUrl)
            Return ParseSponsorsHtml(html)
        End Using
    End Function

    Private Function ParseSponsorsHtml(html As String) As SponsorsFetchResult
        Dim result As New SponsorsFetchResult With {
            .Sponsors = New List(Of SponsorProfile)()
        }

        If String.IsNullOrWhiteSpace(html) Then
            Return result
        End If

        Dim countMatch As Match = Regex.Match(html, "(?<count>\d+)\s+sponsor(?:s)?\s+(?:has|have)\s+funded", RegexOptions.IgnoreCase)
        If countMatch.Success Then
            Dim parsedCount As Integer
            If Integer.TryParse(countMatch.Groups("count").Value, parsedCount) Then
                result.PublicSponsorCount = Math.Max(0, parsedCount)
            End If
        End If

        Dim sectionHtml As String = html
        Dim sectionPattern As String = "<div[^>]*id=""sponsors""[^>]*>(?<body>[\s\S]*?)</remote-pagination>"
        Dim sectionMatch As Match = Regex.Match(html, sectionPattern, RegexOptions.IgnoreCase)
        If sectionMatch.Success Then
            sectionHtml = sectionMatch.Groups("body").Value
        End If

        Dim sponsorPattern As String = "<a[^>]+href=""/(?<slug>[A-Za-z0-9](?:[A-Za-z0-9-]{0,38}))""[^>]*>\s*<img[^>]+alt=""@(?<name>[^""]+)"""
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each match As Match In Regex.Matches(sectionHtml, sponsorPattern, RegexOptions.IgnoreCase)
            If Not match.Success Then
                Continue For
            End If

            Dim slug As String = WebUtility.HtmlDecode(match.Groups("slug").Value).Trim()
            Dim name As String = WebUtility.HtmlDecode(match.Groups("name").Value).Trim()
            If String.IsNullOrWhiteSpace(slug) Then
                Continue For
            End If
            If String.IsNullOrWhiteSpace(name) Then
                name = slug
            End If

            Dim profileUrl As String = "https://github.com/" & slug
            If Not seen.Add(profileUrl) Then
                Continue For
            End If

            result.Sponsors.Add(New SponsorProfile With {
                .DisplayName = name,
                .ProfileUrl = profileUrl
            })
        Next

        result.Sponsors = result.Sponsors.
            OrderBy(Function(item) item.DisplayName, StringComparer.OrdinalIgnoreCase).
            ToList()

        Return result
    End Function

    Private Sub PopulateSponsors(fetch As SponsorsFetchResult)
        Dim sponsors As List(Of SponsorProfile) = If(fetch?.Sponsors, New List(Of SponsorProfile)())

        lvSponsors.BeginUpdate()
        lvSponsors.Items.Clear()
        For Each sponsor As SponsorProfile In sponsors
            Dim item As New ListViewItem(sponsor.DisplayName)
            item.SubItems.Add(sponsor.ProfileUrl)
            item.Tag = sponsor
            lvSponsors.Items.Add(item)
        Next
        lvSponsors.EndUpdate()

        UpdateSponsorsColumnWidths()

        Dim loaded As Integer = sponsors.Count
        Dim publicCount As Integer? = If(fetch Is Nothing, CType(Nothing, Integer?), fetch.PublicSponsorCount)
        If loaded > 0 Then
            If publicCount.HasValue AndAlso publicCount.Value > loaded Then
                lblSponsorsStatus.Text = $"Loaded {loaded} public sponsor profile(s). Total public sponsors: {publicCount.Value}."
            Else
                lblSponsorsStatus.Text = $"Loaded {loaded} public sponsor profile(s)."
            End If
            Return
        End If

        If publicCount.HasValue Then
            If publicCount.Value <= 0 Then
                lblSponsorsStatus.Text = "No public sponsors listed yet."
            Else
                lblSponsorsStatus.Text = "No public sponsor profiles were exposed by GitHub (some sponsors may be private)."
            End If
        Else
            lblSponsorsStatus.Text = "No public sponsor profiles found."
        End If
    End Sub

    Private Sub UpdateSponsorsColumnWidths()
        If lvSponsors Is Nothing OrElse lvSponsors.Columns Is Nothing OrElse lvSponsors.Columns.Count < 2 Then
            Return
        End If

        Dim width As Integer = Math.Max(0, lvSponsors.ClientSize.Width)
        If width <= 10 Then
            Return
        End If

        Dim first As Integer = Math.Max(170, CInt(Math.Truncate(width * 0.34R)))
        Dim second As Integer = Math.Max(180, width - first - 6)
        lvSponsors.Columns(0).Width = first
        lvSponsors.Columns(1).Width = second
    End Sub

    Private Function GetSelectedSponsor() As SponsorProfile
        If lvSponsors Is Nothing OrElse lvSponsors.SelectedItems.Count = 0 Then
            Return Nothing
        End If
        Return TryCast(lvSponsors.SelectedItems(0).Tag, SponsorProfile)
    End Function

    Private Sub OpenUrl(url As String, context As String, failureMessage As String)
        If String.IsNullOrWhiteSpace(url) Then
            Return
        End If

        Try
            Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, context)
            MessageBox.Show(Me, failureMessage, "About", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub linkRepo_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkRepo.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = _repositoryUrl
        End If

        If String.IsNullOrWhiteSpace(target) Then
            Return
        End If

        OpenUrl(target, "frmAbout.OpenRepository", "Unable to open the repository link.")
    End Sub

    Private Sub linkOptiScaler_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkOptiScaler.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = OptiScalerRepositoryUrl
        End If

        If String.IsNullOrWhiteSpace(target) Then
            Return
        End If

        OpenUrl(target, "frmAbout.OpenOptiScalerRepository", "Unable to open the OptiScaler repository link.")
    End Sub

    Private Sub linkSponsor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkSponsor.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = SponsorUrl
        End If

        If String.IsNullOrWhiteSpace(target) Then
            Return
        End If

        OpenUrl(target, "frmAbout.OpenSponsor", "Unable to open the sponsor link.")
    End Sub

    Private Sub linkSponsorsPage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkSponsorsPage.LinkClicked
        Dim target As String = TryCast(e.Link.LinkData, String)
        If String.IsNullOrWhiteSpace(target) Then
            target = SponsorUrl
        End If
        OpenUrl(target, "frmAbout.OpenSponsorPage", "Unable to open the GitHub Sponsors page.")
    End Sub

    Private Async Sub btnRefreshSponsors_Click(sender As Object, e As EventArgs) Handles btnRefreshSponsors.Click
        Await LoadSponsorsAsync(True)
    End Sub

    Private Sub lvSponsors_DoubleClick(sender As Object, e As EventArgs) Handles lvSponsors.DoubleClick
        Dim sponsor As SponsorProfile = GetSelectedSponsor()
        If sponsor Is Nothing OrElse String.IsNullOrWhiteSpace(sponsor.ProfileUrl) Then
            Return
        End If

        OpenUrl(sponsor.ProfileUrl, "frmAbout.OpenSponsorProfile", "Unable to open sponsor profile.")
    End Sub

    Private Sub lvSponsors_Resize(sender As Object, e As EventArgs) Handles lvSponsors.Resize
        UpdateSponsorsColumnWidths()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Class SponsorProfile
        Public Property DisplayName As String
        Public Property ProfileUrl As String
    End Class

    Private Class SponsorsFetchResult
        Public Property PublicSponsorCount As Integer?
        Public Property Sponsors As List(Of SponsorProfile)
    End Class
End Class
