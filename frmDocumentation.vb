Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

Friend Class frmDocumentation
    Inherits Form

    Private ReadOnly _selector As ComboBox
    Private ReadOnly _content As ThemedRichTextBox
    Private ReadOnly _openOnline As Button

    Public Sub New(initialDocument As InstallerDocumentation)
        Text = "OptiScaler Installer Documentation"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(700, 480)
        Size = New Size(900, 680)
        ShowInTaskbar = False

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmDocumentation.LoadIcon")
        End Try

        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(10), .ColumnCount = 2, .RowCount = 2}
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        _selector = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
        For Each document As InstallerDocumentation In DocumentationService.GetAvailableDocuments()
            _selector.Items.Add(New DocumentationChoice(document, DocumentationService.GetDocumentTitle(document)))
        Next
        _selector.DisplayMember = NameOf(DocumentationChoice.Title)

        _openOnline = New Button With {.Text = "Open online", .AutoSize = True, .Dock = DockStyle.Fill}
        _content = New ThemedRichTextBox With {.Dock = DockStyle.Fill, .ReadOnly = True, .DetectUrls = True, .ScrollBars = RichTextBoxScrollBars.Vertical}
        layout.Controls.Add(_selector, 0, 0)
        layout.Controls.Add(_openOnline, 1, 0)
        layout.Controls.Add(_content, 0, 1)
        layout.SetColumnSpan(_content, 2)
        Controls.Add(layout)

        AddHandler _selector.SelectedIndexChanged, AddressOf DocumentSelected
        AddHandler _openOnline.Click, AddressOf OpenOnlineClicked
        AddHandler _content.InnerRichTextBox.LinkClicked, AddressOf ContentLinkClicked

        ThemeManager.ApplyTheme(Me, ThemeSettings.GetPreferredColorMode())
        For index As Integer = 0 To _selector.Items.Count - 1
            If DirectCast(_selector.Items(index), DocumentationChoice).Document = initialDocument Then
                _selector.SelectedIndex = index
                Exit For
            End If
        Next
        If _selector.SelectedIndex < 0 Then _selector.SelectedIndex = 0
    End Sub

    Private Sub DocumentSelected(sender As Object, e As EventArgs)
        Dim choice As DocumentationChoice = TryCast(_selector.SelectedItem, DocumentationChoice)
        If choice Is Nothing Then Return
        Dim document As InstallerDocumentation = choice.Document
        ReleaseNotesRenderer.Render(DocumentationService.LoadDocument(document), _content.InnerRichTextBox, ThemeSettings.GetPreferredColorMode())
        _openOnline.Tag = DocumentationService.GetOnlineUrl(document)
        _openOnline.Enabled = DocumentationService.IsAllowedWebUrl(CStr(_openOnline.Tag))
    End Sub

    Private Sub OpenOnlineClicked(sender As Object, e As EventArgs)
        OpenWebUrl(TryCast(_openOnline.Tag, String))
    End Sub

    Private Sub ContentLinkClicked(sender As Object, e As LinkClickedEventArgs)
        OpenWebUrl(e.LinkText)
    End Sub

    Private Sub OpenWebUrl(value As String)
        If Not DocumentationService.IsAllowedWebUrl(value) Then Return
        Try
            Process.Start(New ProcessStartInfo(value) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmDocumentation.OpenWebUrl")
            MessageBox.Show(Me, "Could not open the web link: " & ex.Message, "Documentation", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
    End Sub

    Private NotInheritable Class DocumentationChoice
        Public ReadOnly Document As InstallerDocumentation
        Public ReadOnly Property Title As String
        Public Sub New(document As InstallerDocumentation, title As String)
            Me.Document = document
            Me.Title = title
        End Sub
    End Class
End Class
