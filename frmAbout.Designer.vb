<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAbout
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        tableRoot = New TableLayoutPanel()
        lblTitle = New Label()
        tabAboutMain = New TabControl()
        tabPageAbout = New TabPage()
        tableDetails = New TableLayoutPanel()
        lblCurrentTitle = New Label()
        lblCurrentValue = New Label()
        lblLatestTitle = New Label()
        lblLatestValue = New Label()
        lblReleaseDateTitle = New Label()
        lblReleaseDateValue = New Label()
        lblRepoTitle = New Label()
        linkRepo = New LinkLabel()
        lblOptiScalerTitle = New Label()
        linkOptiScaler = New LinkLabel()
        lblAuthorTitle = New Label()
        lblAuthorValue = New Label()
        lblSponsorTitle = New Label()
        linkSponsor = New LinkLabel()
        tabPageSponsors = New TabPage()
        sponsorsLayout = New TableLayoutPanel()
        lblSponsorsIntro = New Label()
        lvSponsors = New ThemedListView()
        colSponsorName = New ColumnHeader()
        colSponsorProfile = New ColumnHeader()
        lblSponsorsStatus = New Label()
        panelSponsorsActions = New Panel()
        btnRefreshSponsors = New Button()
        linkSponsorsPage = New LinkLabel()
        btnClose = New Button()
        tableRoot.SuspendLayout()
        tabAboutMain.SuspendLayout()
        tabPageAbout.SuspendLayout()
        tableDetails.SuspendLayout()
        tabPageSponsors.SuspendLayout()
        sponsorsLayout.SuspendLayout()
        panelSponsorsActions.SuspendLayout()
        SuspendLayout()
        '
        ' tableRoot
        '
        tableRoot.ColumnCount = 1
        tableRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableRoot.Controls.Add(lblTitle, 0, 0)
        tableRoot.Controls.Add(tabAboutMain, 0, 1)
        tableRoot.Controls.Add(btnClose, 0, 2)
        tableRoot.Dock = DockStyle.Fill
        tableRoot.Location = New Point(0, 0)
        tableRoot.Name = "tableRoot"
        tableRoot.Padding = New Padding(16)
        tableRoot.RowCount = 3
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.Size = New Size(640, 305)
        tableRoot.TabIndex = 0
        '
        ' lblTitle
        '
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        lblTitle.Location = New Point(16, 16)
        lblTitle.Margin = New Padding(0, 0, 0, 12)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(172, 21)
        lblTitle.TabIndex = 0
        lblTitle.Text = "OptiScaler Installer"
        '
        ' tabAboutMain
        '
        tabAboutMain.Controls.Add(tabPageAbout)
        tabAboutMain.Controls.Add(tabPageSponsors)
        tabAboutMain.Dock = DockStyle.Fill
        tabAboutMain.Location = New Point(16, 49)
        tabAboutMain.Margin = New Padding(0, 0, 0, 12)
        tabAboutMain.Name = "tabAboutMain"
        tabAboutMain.SelectedIndex = 0
        tabAboutMain.Size = New Size(688, 321)
        tabAboutMain.TabIndex = 1
        '
        ' tabPageAbout
        '
        tabPageAbout.Controls.Add(tableDetails)
        tabPageAbout.Location = New Point(4, 24)
        tabPageAbout.Name = "tabPageAbout"
        tabPageAbout.Padding = New Padding(12)
        tabPageAbout.Size = New Size(680, 293)
        tabPageAbout.TabIndex = 0
        tabPageAbout.Text = "About"
        tabPageAbout.UseVisualStyleBackColor = True
        '
        ' tableDetails
        '
        tableDetails.ColumnCount = 2
        tableDetails.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tableDetails.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableDetails.Controls.Add(lblCurrentTitle, 0, 0)
        tableDetails.Controls.Add(lblCurrentValue, 1, 0)
        tableDetails.Controls.Add(lblLatestTitle, 0, 1)
        tableDetails.Controls.Add(lblLatestValue, 1, 1)
        tableDetails.Controls.Add(lblReleaseDateTitle, 0, 2)
        tableDetails.Controls.Add(lblReleaseDateValue, 1, 2)
        tableDetails.Controls.Add(lblRepoTitle, 0, 3)
        tableDetails.Controls.Add(linkRepo, 1, 3)
        tableDetails.Controls.Add(lblOptiScalerTitle, 0, 4)
        tableDetails.Controls.Add(linkOptiScaler, 1, 4)
        tableDetails.Controls.Add(lblAuthorTitle, 0, 5)
        tableDetails.Controls.Add(lblAuthorValue, 1, 5)
        tableDetails.Controls.Add(lblSponsorTitle, 0, 6)
        tableDetails.Controls.Add(linkSponsor, 1, 6)
        tableDetails.Dock = DockStyle.Fill
        tableDetails.Location = New Point(12, 12)
        tableDetails.Margin = New Padding(0)
        tableDetails.Name = "tableDetails"
        tableDetails.RowCount = 7
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.Size = New Size(656, 269)
        tableDetails.TabIndex = 0
        '
        ' lblCurrentTitle
        '
        lblCurrentTitle.AutoSize = True
        lblCurrentTitle.Location = New Point(0, 0)
        lblCurrentTitle.Margin = New Padding(0, 0, 12, 10)
        lblCurrentTitle.Name = "lblCurrentTitle"
        lblCurrentTitle.Size = New Size(86, 15)
        lblCurrentTitle.TabIndex = 0
        lblCurrentTitle.Text = "Current version"
        '
        ' lblCurrentValue
        '
        lblCurrentValue.AutoSize = True
        lblCurrentValue.Location = New Point(98, 0)
        lblCurrentValue.Margin = New Padding(0, 0, 0, 10)
        lblCurrentValue.Name = "lblCurrentValue"
        lblCurrentValue.Size = New Size(28, 15)
        lblCurrentValue.TabIndex = 1
        lblCurrentValue.Text = "N/A"
        '
        ' lblLatestTitle
        '
        lblLatestTitle.AutoSize = True
        lblLatestTitle.Location = New Point(0, 25)
        lblLatestTitle.Margin = New Padding(0, 0, 12, 10)
        lblLatestTitle.Name = "lblLatestTitle"
        lblLatestTitle.Size = New Size(80, 15)
        lblLatestTitle.TabIndex = 2
        lblLatestTitle.Text = "Latest version"
        '
        ' lblLatestValue
        '
        lblLatestValue.AutoSize = True
        lblLatestValue.Location = New Point(98, 25)
        lblLatestValue.Margin = New Padding(0, 0, 0, 10)
        lblLatestValue.Name = "lblLatestValue"
        lblLatestValue.Size = New Size(28, 15)
        lblLatestValue.TabIndex = 3
        lblLatestValue.Text = "N/A"
        '
        ' lblReleaseDateTitle
        '
        lblReleaseDateTitle.AutoSize = True
        lblReleaseDateTitle.Location = New Point(0, 50)
        lblReleaseDateTitle.Margin = New Padding(0, 0, 12, 10)
        lblReleaseDateTitle.Name = "lblReleaseDateTitle"
        lblReleaseDateTitle.Size = New Size(68, 15)
        lblReleaseDateTitle.TabIndex = 4
        lblReleaseDateTitle.Text = "Release date"
        '
        ' lblReleaseDateValue
        '
        lblReleaseDateValue.AutoSize = True
        lblReleaseDateValue.Location = New Point(98, 50)
        lblReleaseDateValue.Margin = New Padding(0, 0, 0, 10)
        lblReleaseDateValue.Name = "lblReleaseDateValue"
        lblReleaseDateValue.Size = New Size(28, 15)
        lblReleaseDateValue.TabIndex = 5
        lblReleaseDateValue.Text = "N/A"
        '
        ' lblRepoTitle
        '
        lblRepoTitle.AutoSize = True
        lblRepoTitle.Location = New Point(0, 75)
        lblRepoTitle.Margin = New Padding(0, 0, 12, 10)
        lblRepoTitle.Name = "lblRepoTitle"
        lblRepoTitle.Size = New Size(61, 15)
        lblRepoTitle.TabIndex = 6
        lblRepoTitle.Text = "Repository"
        '
        ' linkRepo
        '
        linkRepo.AutoEllipsis = True
        linkRepo.AutoSize = True
        linkRepo.Location = New Point(98, 75)
        linkRepo.Margin = New Padding(0, 0, 0, 10)
        linkRepo.Name = "linkRepo"
        linkRepo.Size = New Size(28, 15)
        linkRepo.TabIndex = 7
        linkRepo.TabStop = True
        linkRepo.Text = "N/A"
        ' 
        ' lblOptiScalerTitle
        ' 
        lblOptiScalerTitle.AutoSize = True
        lblOptiScalerTitle.Location = New Point(0, 100)
        lblOptiScalerTitle.Margin = New Padding(0, 0, 12, 10)
        lblOptiScalerTitle.Name = "lblOptiScalerTitle"
        lblOptiScalerTitle.Size = New Size(89, 15)
        lblOptiScalerTitle.TabIndex = 8
        lblOptiScalerTitle.Text = "OptiScaler repo"
        ' 
        ' linkOptiScaler
        ' 
        linkOptiScaler.AutoEllipsis = True
        linkOptiScaler.AutoSize = True
        linkOptiScaler.Location = New Point(98, 100)
        linkOptiScaler.Margin = New Padding(0, 0, 0, 10)
        linkOptiScaler.Name = "linkOptiScaler"
        linkOptiScaler.Size = New Size(28, 15)
        linkOptiScaler.TabIndex = 9
        linkOptiScaler.TabStop = True
        linkOptiScaler.Text = "N/A"
        ' 
        ' lblAuthorTitle
        ' 
        lblAuthorTitle.AutoSize = True
        lblAuthorTitle.Location = New Point(0, 125)
        lblAuthorTitle.Margin = New Padding(0, 0, 12, 0)
        lblAuthorTitle.Name = "lblAuthorTitle"
        lblAuthorTitle.Size = New Size(44, 15)
        lblAuthorTitle.TabIndex = 10
        lblAuthorTitle.Text = "Author"
        ' 
        ' lblAuthorValue
        ' 
        lblAuthorValue.AutoSize = True
        lblAuthorValue.Location = New Point(98, 125)
        lblAuthorValue.Margin = New Padding(0)
        lblAuthorValue.Name = "lblAuthorValue"
        lblAuthorValue.Size = New Size(28, 15)
        lblAuthorValue.TabIndex = 11
        lblAuthorValue.Text = "N/A"
        '
        ' lblSponsorTitle
        '
        lblSponsorTitle.AutoSize = True
        lblSponsorTitle.Location = New Point(0, 150)
        lblSponsorTitle.Margin = New Padding(0, 0, 12, 0)
        lblSponsorTitle.Name = "lblSponsorTitle"
        lblSponsorTitle.Size = New Size(50, 15)
        lblSponsorTitle.TabIndex = 12
        lblSponsorTitle.Text = "Sponsor"
        '
        ' linkSponsor
        '
        linkSponsor.AutoEllipsis = True
        linkSponsor.AutoSize = True
        linkSponsor.Location = New Point(98, 150)
        linkSponsor.Margin = New Padding(0)
        linkSponsor.Name = "linkSponsor"
        linkSponsor.Size = New Size(28, 15)
        linkSponsor.TabIndex = 13
        linkSponsor.TabStop = True
        linkSponsor.Text = "N/A"
        '
        ' tabPageSponsors
        '
        tabPageSponsors.Controls.Add(sponsorsLayout)
        tabPageSponsors.Location = New Point(4, 24)
        tabPageSponsors.Name = "tabPageSponsors"
        tabPageSponsors.Padding = New Padding(12)
        tabPageSponsors.Size = New Size(680, 293)
        tabPageSponsors.TabIndex = 1
        tabPageSponsors.Text = "Sponsors"
        tabPageSponsors.UseVisualStyleBackColor = True
        '
        ' sponsorsLayout
        '
        sponsorsLayout.ColumnCount = 1
        sponsorsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        sponsorsLayout.Controls.Add(lvSponsors, 0, 0)
        sponsorsLayout.Controls.Add(panelSponsorsActions, 0, 1)
        sponsorsLayout.Dock = DockStyle.Fill
        sponsorsLayout.Location = New Point(12, 12)
        sponsorsLayout.Margin = New Padding(0)
        sponsorsLayout.Name = "sponsorsLayout"
        sponsorsLayout.RowCount = 2
        sponsorsLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        sponsorsLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        sponsorsLayout.Size = New Size(656, 269)
        sponsorsLayout.TabIndex = 0
        '
        ' lblSponsorsIntro
        '
        lblSponsorsIntro.AutoSize = True
        lblSponsorsIntro.Location = New Point(0, 0)
        lblSponsorsIntro.Margin = New Padding(0, 0, 0, 8)
        lblSponsorsIntro.Name = "lblSponsorsIntro"
        lblSponsorsIntro.Size = New Size(412, 15)
        lblSponsorsIntro.TabIndex = 0
        lblSponsorsIntro.Text = "Public sponsors are loaded automatically from your GitHub Sponsors page."
        '
        ' lvSponsors
        '
        lvSponsors.BorderStyle = BorderStyle.None
        lvSponsors.Columns.AddRange(New ColumnHeader() {colSponsorName, colSponsorProfile})
        lvSponsors.Dock = DockStyle.Fill
        lvSponsors.FullRowSelect = True
        lvSponsors.HeaderStyle = ColumnHeaderStyle.Nonclickable
        lvSponsors.HideSelection = False
        lvSponsors.Location = New Point(0, 0)
        lvSponsors.Margin = New Padding(0)
        lvSponsors.MultiSelect = False
        lvSponsors.Name = "lvSponsors"
        lvSponsors.OwnerDraw = False
        lvSponsors.ShowGroups = False
        lvSponsors.Size = New Size(656, 243)
        lvSponsors.TabIndex = 1
        lvSponsors.UseCompatibleStateImageBehavior = False
        lvSponsors.View = View.Details
        '
        ' colSponsorName
        '
        colSponsorName.Text = "Sponsor"
        colSponsorName.Width = 220
        '
        ' colSponsorProfile
        '
        colSponsorProfile.Text = "Profile"
        colSponsorProfile.Width = 420
        '
        ' lblSponsorsStatus
        '
        lblSponsorsStatus.AutoSize = True
        lblSponsorsStatus.Location = New Point(0, 232)
        lblSponsorsStatus.Margin = New Padding(0, 6, 0, 8)
        lblSponsorsStatus.Name = "lblSponsorsStatus"
        lblSponsorsStatus.Size = New Size(111, 15)
        lblSponsorsStatus.TabIndex = 2
        lblSponsorsStatus.Text = "Loading sponsors..."
        '
        ' panelSponsorsActions
        '
        panelSponsorsActions.AutoSize = True
        panelSponsorsActions.AutoSizeMode = AutoSizeMode.GrowAndShrink
        panelSponsorsActions.Controls.Add(btnRefreshSponsors)
        panelSponsorsActions.Controls.Add(linkSponsorsPage)
        panelSponsorsActions.Dock = DockStyle.Fill
        panelSponsorsActions.Location = New Point(0, 243)
        panelSponsorsActions.Margin = New Padding(0)
        panelSponsorsActions.Name = "panelSponsorsActions"
        panelSponsorsActions.Size = New Size(656, 26)
        panelSponsorsActions.TabIndex = 3
        '
        ' btnRefreshSponsors
        '
        btnRefreshSponsors.Location = New Point(0, 0)
        btnRefreshSponsors.Name = "btnRefreshSponsors"
        btnRefreshSponsors.Size = New Size(100, 26)
        btnRefreshSponsors.TabIndex = 0
        btnRefreshSponsors.Text = "Refresh"
        btnRefreshSponsors.UseVisualStyleBackColor = True
        '
        ' linkSponsorsPage
        '
        linkSponsorsPage.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        linkSponsorsPage.AutoSize = True
        linkSponsorsPage.Location = New Point(508, 6)
        linkSponsorsPage.Name = "linkSponsorsPage"
        linkSponsorsPage.Size = New Size(148, 15)
        linkSponsorsPage.TabIndex = 1
        linkSponsorsPage.TabStop = True
        linkSponsorsPage.Text = "Open GitHub sponsors page"
        '
        ' btnClose
        '
        btnClose.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnClose.Location = New Point(609, 385)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(95, 30)
        btnClose.TabIndex = 2
        btnClose.Text = "Close"
        btnClose.UseVisualStyleBackColor = True
        '
        ' frmAbout
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(720, 435)
        Controls.Add(tableRoot)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "frmAbout"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "About"
        tableRoot.ResumeLayout(False)
        tableRoot.PerformLayout()
        tabAboutMain.ResumeLayout(False)
        tabPageAbout.ResumeLayout(False)
        tableDetails.ResumeLayout(False)
        tableDetails.PerformLayout()
        tabPageSponsors.ResumeLayout(False)
        sponsorsLayout.ResumeLayout(False)
        sponsorsLayout.PerformLayout()
        panelSponsorsActions.ResumeLayout(False)
        panelSponsorsActions.PerformLayout()
        ResumeLayout(False)

    End Sub

    Friend WithEvents tableRoot As TableLayoutPanel
    Friend WithEvents lblTitle As Label
    Friend WithEvents tabAboutMain As TabControl
    Friend WithEvents tabPageAbout As TabPage
    Friend WithEvents tableDetails As TableLayoutPanel
    Friend WithEvents lblCurrentTitle As Label
    Friend WithEvents lblCurrentValue As Label
    Friend WithEvents lblLatestTitle As Label
    Friend WithEvents lblLatestValue As Label
    Friend WithEvents lblReleaseDateTitle As Label
    Friend WithEvents lblReleaseDateValue As Label
    Friend WithEvents lblRepoTitle As Label
    Friend WithEvents linkRepo As LinkLabel
    Friend WithEvents lblOptiScalerTitle As Label
    Friend WithEvents linkOptiScaler As LinkLabel
    Friend WithEvents lblAuthorTitle As Label
    Friend WithEvents lblAuthorValue As Label
    Friend WithEvents lblSponsorTitle As Label
    Friend WithEvents linkSponsor As LinkLabel
    Friend WithEvents tabPageSponsors As TabPage
    Friend WithEvents sponsorsLayout As TableLayoutPanel
    Friend WithEvents lblSponsorsIntro As Label
    Friend WithEvents lvSponsors As ThemedListView
    Friend WithEvents colSponsorName As ColumnHeader
    Friend WithEvents colSponsorProfile As ColumnHeader
    Friend WithEvents lblSponsorsStatus As Label
    Friend WithEvents panelSponsorsActions As Panel
    Friend WithEvents btnRefreshSponsors As Button
    Friend WithEvents linkSponsorsPage As LinkLabel
    Friend WithEvents btnClose As Button
End Class
