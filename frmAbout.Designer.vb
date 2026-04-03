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
        btnClose = New Button()
        tableRoot.SuspendLayout()
        tableDetails.SuspendLayout()
        SuspendLayout()
        '
        ' tableRoot
        '
        tableRoot.ColumnCount = 1
        tableRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableRoot.Controls.Add(lblTitle, 0, 0)
        tableRoot.Controls.Add(tableDetails, 0, 1)
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
        tableDetails.Location = New Point(16, 49)
        tableDetails.Margin = New Padding(0, 0, 0, 12)
        tableDetails.Name = "tableDetails"
        tableDetails.RowCount = 7
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.Size = New Size(608, 194)
        tableDetails.TabIndex = 1
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
        ' btnClose
        '
        btnClose.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnClose.Location = New Point(529, 255)
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
        ClientSize = New Size(640, 305)
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
        tableDetails.ResumeLayout(False)
        tableDetails.PerformLayout()
        ResumeLayout(False)

    End Sub

    Friend WithEvents tableRoot As TableLayoutPanel
    Friend WithEvents lblTitle As Label
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
    Friend WithEvents btnClose As Button
End Class
