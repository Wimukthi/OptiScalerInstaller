Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class MainForm
    Private fsrSupportGuidance As Label
    Private showLegacyFsrTools As CheckBox
    Private fsrPackageOperationInProgress As Boolean

    Private Sub InitializeFsrSupportUi()
        If fsrSupportGuidance IsNot Nothing Then Return

        ' Establish the parent width before anchoring children; otherwise they grow by the
        ' difference from the GroupBox default width and the guidance runs off the page.
        Dim support As New ThemedGroupBox With {.Text = "FSR Upscaling 4.1 support", .Size = New Size(experimentalLayout.ClientSize.Width, 170), .Dock = DockStyle.Fill, .Margin = New Padding(0, 0, 0, 8)}
        Dim contentWidth As Integer = Math.Max(300, support.ClientSize.Width - 24)
        fsrSupportGuidance = New Label With {.Location = New Point(12, 24), .Size = New Size(contentWidth, 52), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right, .Text = AmdFsrSupportService.BuildGuidance(gpuDetectionCandidates)}
        support.Controls.Add(fsrSupportGuidance)
        support.Controls.Add(New Label With {.Location = New Point(12, 82), .Size = New Size(contentWidth, 20), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right, .Text = "Driver upscaling, OptiScaler SDK models, frame generation and ray regeneration have separate requirements."})

        Dim links As New FlowLayoutPanel With {.Location = New Point(9, 105), .Size = New Size(contentWidth, 25), .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right}
        For Each item In New (String, String)() {("AMD 26.10.1 notes", AmdFsrSupportService.DriverNotesUrl), ("FSR support guide", AmdFsrSupportService.SupportGuideUrl), ("OptiScaler releases", "https://github.com/optiscaler/OptiScaler/releases")}
            Dim url As String = item.Item2
            Dim link As New LinkLabel With {.Text = item.Item1, .AutoSize = True, .Margin = New Padding(3, 3, 18, 3)}
            AddHandler link.LinkClicked,
                Sub(sender, args)
                    If url = AmdFsrSupportService.SupportGuideUrl Then
                        DocumentationService.Show(Me, InstallerDocumentation.AmdFsrSupport)
                    Else
                        OpenExternalUrl(url)
                    End If
                End Sub
            links.Controls.Add(link)
        Next
        support.Controls.Add(links)
        showLegacyFsrTools = New CheckBox With {.Text = "Show legacy/manual package tools", .AutoSize = True, .Location = New Point(12, 139)}
        AddHandler showLegacyFsrTools.CheckedChanged, Sub(sender, args) UpdateLegacyFsrToolsVisibility()
        support.Controls.Add(showLegacyFsrTools)

        ' GPU guidance and manifest-backed package status answer different questions.
        experimentalLayout.SuspendLayout()
        experimentalLayout.Controls.Clear()
        experimentalLayout.RowStyles.Clear()
        experimentalLayout.RowCount = 4
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 178))
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        experimentalLayout.Controls.Add(support, 0, 0)
        experimentalLayout.Controls.Add(grpFsr4Package, 0, 1)
        experimentalLayout.Controls.Add(grpFsr4Options, 0, 2)
        experimentalLayout.Controls.Add(grpFsr4Actions, 0, 3)
        UpdateLegacyFsrToolsVisibility()
        experimentalLayout.ResumeLayout(True)
    End Sub

    Private Sub UpdateLegacyFsrToolsVisibility()
        Dim showTools As Boolean = showLegacyFsrTools IsNot Nothing AndAlso showLegacyFsrTools.Checked
        grpFsr4Package.Visible = showTools
        grpFsr4Options.Visible = showTools
        experimentalLayout.RowStyles(1).Height = If(showTools, 124, 0)
        experimentalLayout.RowStyles(2).Height = If(showTools, 132, 0)
        experimentalLayout.MinimumSize = New Size(0, If(showTools, 880, 620))
        tabExperimental.AutoScrollMinSize = New Size(1180, If(showTools, 880, 620))
        btnFsr4Apply.Enabled = showTools AndAlso Not fsrPackageOperationInProgress
    End Sub
End Class
