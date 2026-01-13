<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUpdate
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
        lblPackageTitle = New Label()
        lblPackageValue = New Label()
        lblNotesTitle = New Label()
        txtReleaseNotes = New ThemedTextBox()
        panelProgress = New Panel()
        tableProgress = New TableLayoutPanel()
        progressDownload = New ProgressBar()
        tableStatus = New TableLayoutPanel()
        lblStatus = New Label()
        lblProgress = New Label()
        lblSpeed = New Label()
        flowButtons = New FlowLayoutPanel()
        btnCancel = New Button()
        btnDownload = New Button()
        btnRelease = New Button()
        tableRoot.SuspendLayout()
        tableDetails.SuspendLayout()
        panelProgress.SuspendLayout()
        tableProgress.SuspendLayout()
        tableStatus.SuspendLayout()
        flowButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' tableRoot
        ' 
        tableRoot.ColumnCount = 1
        tableRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableRoot.Controls.Add(lblTitle, 0, 0)
        tableRoot.Controls.Add(tableDetails, 0, 1)
        tableRoot.Controls.Add(lblNotesTitle, 0, 2)
        tableRoot.Controls.Add(txtReleaseNotes, 0, 3)
        tableRoot.Controls.Add(panelProgress, 0, 4)
        tableRoot.Controls.Add(flowButtons, 0, 5)
        tableRoot.Dock = DockStyle.Fill
        tableRoot.Location = New Point(0, 0)
        tableRoot.Name = "tableRoot"
        tableRoot.Padding = New Padding(16)
        tableRoot.RowCount = 6
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableRoot.Size = New Size(740, 560)
        tableRoot.TabIndex = 0
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        lblTitle.Location = New Point(16, 16)
        lblTitle.Margin = New Padding(3, 0, 3, 8)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(132, 21)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Update Available"
        ' 
        ' tableDetails
        ' 
        tableDetails.AutoSize = True
        tableDetails.ColumnCount = 2
        tableDetails.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tableDetails.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableDetails.Controls.Add(lblCurrentTitle, 0, 0)
        tableDetails.Controls.Add(lblCurrentValue, 1, 0)
        tableDetails.Controls.Add(lblLatestTitle, 0, 1)
        tableDetails.Controls.Add(lblLatestValue, 1, 1)
        tableDetails.Controls.Add(lblPackageTitle, 0, 2)
        tableDetails.Controls.Add(lblPackageValue, 1, 2)
        tableDetails.Dock = DockStyle.Top
        tableDetails.Location = New Point(16, 45)
        tableDetails.Margin = New Padding(0, 0, 0, 12)
        tableDetails.Name = "tableDetails"
        tableDetails.RowCount = 3
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableDetails.Size = New Size(708, 63)
        tableDetails.TabIndex = 1
        ' 
        ' lblCurrentTitle
        ' 
        lblCurrentTitle.AutoSize = True
        lblCurrentTitle.Location = New Point(0, 0)
        lblCurrentTitle.Margin = New Padding(0, 0, 12, 8)
        lblCurrentTitle.Name = "lblCurrentTitle"
        lblCurrentTitle.Size = New Size(54, 15)
        lblCurrentTitle.TabIndex = 0
        lblCurrentTitle.Text = "Current:"
        ' 
        ' lblCurrentValue
        ' 
        lblCurrentValue.AutoSize = True
        lblCurrentValue.Location = New Point(66, 0)
        lblCurrentValue.Margin = New Padding(0, 0, 0, 8)
        lblCurrentValue.Name = "lblCurrentValue"
        lblCurrentValue.Size = New Size(28, 15)
        lblCurrentValue.TabIndex = 1
        lblCurrentValue.Text = "N/A"
        ' 
        ' lblLatestTitle
        ' 
        lblLatestTitle.AutoSize = True
        lblLatestTitle.Location = New Point(0, 23)
        lblLatestTitle.Margin = New Padding(0, 0, 12, 8)
        lblLatestTitle.Name = "lblLatestTitle"
        lblLatestTitle.Size = New Size(44, 15)
        lblLatestTitle.TabIndex = 2
        lblLatestTitle.Text = "Latest:"
        ' 
        ' lblLatestValue
        ' 
        lblLatestValue.AutoSize = True
        lblLatestValue.Location = New Point(66, 23)
        lblLatestValue.Margin = New Padding(0, 0, 0, 8)
        lblLatestValue.Name = "lblLatestValue"
        lblLatestValue.Size = New Size(28, 15)
        lblLatestValue.TabIndex = 3
        lblLatestValue.Text = "N/A"
        ' 
        ' lblPackageTitle
        ' 
        lblPackageTitle.AutoSize = True
        lblPackageTitle.Location = New Point(0, 46)
        lblPackageTitle.Margin = New Padding(0, 0, 12, 0)
        lblPackageTitle.Name = "lblPackageTitle"
        lblPackageTitle.Size = New Size(55, 15)
        lblPackageTitle.TabIndex = 4
        lblPackageTitle.Text = "Package:"
        ' 
        ' lblPackageValue
        ' 
        lblPackageValue.AutoSize = True
        lblPackageValue.Location = New Point(66, 46)
        lblPackageValue.Margin = New Padding(0)
        lblPackageValue.Name = "lblPackageValue"
        lblPackageValue.Size = New Size(28, 15)
        lblPackageValue.TabIndex = 5
        lblPackageValue.Text = "N/A"
        ' 
        ' lblNotesTitle
        ' 
        lblNotesTitle.AutoSize = True
        lblNotesTitle.Location = New Point(16, 120)
        lblNotesTitle.Margin = New Padding(0, 0, 0, 6)
        lblNotesTitle.Name = "lblNotesTitle"
        lblNotesTitle.Size = New Size(77, 15)
        lblNotesTitle.TabIndex = 2
        lblNotesTitle.Text = "Release notes:"
        ' 
        ' txtReleaseNotes
        ' 
        txtReleaseNotes.Dock = DockStyle.Fill
        txtReleaseNotes.Location = New Point(16, 141)
        txtReleaseNotes.Margin = New Padding(0, 0, 0, 12)
        txtReleaseNotes.Multiline = True
        txtReleaseNotes.Name = "txtReleaseNotes"
        txtReleaseNotes.ReadOnly = True
        txtReleaseNotes.ScrollBars = ScrollBars.Vertical
        txtReleaseNotes.Size = New Size(708, 319)
        txtReleaseNotes.TabIndex = 3
        ' 
        ' panelProgress
        ' 
        panelProgress.Controls.Add(tableProgress)
        panelProgress.Dock = DockStyle.Fill
        panelProgress.Location = New Point(16, 472)
        panelProgress.Margin = New Padding(0, 0, 0, 12)
        panelProgress.Name = "panelProgress"
        panelProgress.Size = New Size(708, 53)
        panelProgress.TabIndex = 4
        ' 
        ' tableProgress
        ' 
        tableProgress.ColumnCount = 1
        tableProgress.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableProgress.Controls.Add(progressDownload, 0, 0)
        tableProgress.Controls.Add(tableStatus, 0, 1)
        tableProgress.Dock = DockStyle.Fill
        tableProgress.Location = New Point(0, 0)
        tableProgress.Name = "tableProgress"
        tableProgress.RowCount = 2
        tableProgress.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableProgress.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableProgress.Size = New Size(708, 53)
        tableProgress.TabIndex = 0
        ' 
        ' progressDownload
        ' 
        progressDownload.Dock = DockStyle.Fill
        progressDownload.Location = New Point(0, 0)
        progressDownload.Margin = New Padding(0, 0, 0, 6)
        progressDownload.Name = "progressDownload"
        progressDownload.Size = New Size(708, 20)
        progressDownload.TabIndex = 0
        ' 
        ' tableStatus
        ' 
        tableStatus.ColumnCount = 3
        tableStatus.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tableStatus.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        tableStatus.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        tableStatus.Controls.Add(lblStatus, 0, 0)
        tableStatus.Controls.Add(lblProgress, 1, 0)
        tableStatus.Controls.Add(lblSpeed, 2, 0)
        tableStatus.Dock = DockStyle.Fill
        tableStatus.Location = New Point(0, 26)
        tableStatus.Margin = New Padding(0)
        tableStatus.Name = "tableStatus"
        tableStatus.RowCount = 1
        tableStatus.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableStatus.Size = New Size(708, 27)
        tableStatus.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(0, 0)
        lblStatus.Margin = New Padding(0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(354, 27)
        lblStatus.TabIndex = 0
        lblStatus.Text = "Ready to download."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblProgress
        ' 
        lblProgress.Dock = DockStyle.Fill
        lblProgress.Location = New Point(354, 0)
        lblProgress.Margin = New Padding(0)
        lblProgress.Name = "lblProgress"
        lblProgress.Size = New Size(177, 27)
        lblProgress.TabIndex = 1
        lblProgress.Text = "0 / 0"
        lblProgress.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' lblSpeed
        ' 
        lblSpeed.Dock = DockStyle.Fill
        lblSpeed.Location = New Point(531, 0)
        lblSpeed.Margin = New Padding(0)
        lblSpeed.Name = "lblSpeed"
        lblSpeed.Size = New Size(177, 27)
        lblSpeed.TabIndex = 2
        lblSpeed.Text = "Speed: N/A"
        lblSpeed.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' flowButtons
        ' 
        flowButtons.AutoSize = True
        flowButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flowButtons.Controls.Add(btnCancel)
        flowButtons.Controls.Add(btnDownload)
        flowButtons.Controls.Add(btnRelease)
        flowButtons.Dock = DockStyle.Fill
        flowButtons.FlowDirection = FlowDirection.RightToLeft
        flowButtons.Location = New Point(16, 537)
        flowButtons.Margin = New Padding(0)
        flowButtons.Name = "flowButtons"
        flowButtons.Padding = New Padding(0)
        flowButtons.Size = New Size(708, 27)
        flowButtons.TabIndex = 5
        flowButtons.WrapContents = False
        ' 
        ' btnCancel
        ' 
        btnCancel.AutoSize = True
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.Location = New Point(620, 0)
        btnCancel.Margin = New Padding(6, 0, 0, 0)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(88, 27)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Close"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnDownload
        ' 
        btnDownload.AutoSize = True
        btnDownload.FlatStyle = FlatStyle.Flat
        btnDownload.Location = New Point(488, 0)
        btnDownload.Margin = New Padding(6, 0, 0, 0)
        btnDownload.Name = "btnDownload"
        btnDownload.Size = New Size(126, 27)
        btnDownload.TabIndex = 1
        btnDownload.Text = "Download && Install"
        btnDownload.UseVisualStyleBackColor = True
        ' 
        ' btnRelease
        ' 
        btnRelease.AutoSize = True
        btnRelease.FlatStyle = FlatStyle.Flat
        btnRelease.Location = New Point(370, 0)
        btnRelease.Margin = New Padding(6, 0, 0, 0)
        btnRelease.Name = "btnRelease"
        btnRelease.Size = New Size(112, 27)
        btnRelease.TabIndex = 2
        btnRelease.Text = "View Release"
        btnRelease.UseVisualStyleBackColor = True
        ' 
        ' frmUpdate
        ' 
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(740, 560)
        Controls.Add(tableRoot)
        MinimumSize = New Size(640, 520)
        Name = "frmUpdate"
        StartPosition = FormStartPosition.CenterParent
        Text = "OptiScaler Installer Updater"
        tableRoot.ResumeLayout(False)
        tableRoot.PerformLayout()
        tableDetails.ResumeLayout(False)
        tableDetails.PerformLayout()
        panelProgress.ResumeLayout(False)
        tableProgress.ResumeLayout(False)
        tableStatus.ResumeLayout(False)
        flowButtons.ResumeLayout(False)
        flowButtons.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tableRoot As TableLayoutPanel
    Friend WithEvents lblTitle As Label
    Friend WithEvents tableDetails As TableLayoutPanel
    Friend WithEvents lblCurrentTitle As Label
    Friend WithEvents lblCurrentValue As Label
    Friend WithEvents lblLatestTitle As Label
    Friend WithEvents lblLatestValue As Label
    Friend WithEvents lblPackageTitle As Label
    Friend WithEvents lblPackageValue As Label
    Friend WithEvents lblNotesTitle As Label
    Friend WithEvents txtReleaseNotes As ThemedTextBox
    Friend WithEvents panelProgress As Panel
    Friend WithEvents tableProgress As TableLayoutPanel
    Friend WithEvents progressDownload As ProgressBar
    Friend WithEvents tableStatus As TableLayoutPanel
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblProgress As Label
    Friend WithEvents lblSpeed As Label
    Friend WithEvents flowButtons As FlowLayoutPanel
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnDownload As Button
    Friend WithEvents btnRelease As Button
End Class
