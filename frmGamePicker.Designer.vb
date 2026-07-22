<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmGamePicker
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblIntro = New Label()
        lblSearch = New Label()
        txtSearch = New ThemedTextBox()
        chkShowAll = New CheckBox()
        lvGames = New ThemedListView()
        colGame = New ColumnHeader()
        btnAdd = New Button()
        btnCancel = New Button()
        SuspendLayout()
        '
        ' lblIntro
        '
        lblIntro.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblIntro.Location = New Point(12, 12)
        lblIntro.Name = "lblIntro"
        lblIntro.Size = New Size(536, 40)
        lblIntro.TabIndex = 0
        lblIntro.Text = "Select the matching game from the compatibility list."
        '
        ' lblSearch
        '
        lblSearch.AutoSize = True
        lblSearch.Location = New Point(12, 62)
        lblSearch.Name = "lblSearch"
        lblSearch.Size = New Size(45, 15)
        lblSearch.TabIndex = 1
        lblSearch.Text = "Search"
        '
        ' txtSearch
        '
        txtSearch.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtSearch.Location = New Point(70, 57)
        txtSearch.Name = "txtSearch"
        txtSearch.PlaceholderText = "Type to filter games..."
        txtSearch.Size = New Size(478, 26)
        txtSearch.TabIndex = 2
        '
        ' chkShowAll
        '
        chkShowAll.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        chkShowAll.AutoSize = True
        chkShowAll.Location = New Point(12, 92)
        chkShowAll.Name = "chkShowAll"
        chkShowAll.Size = New Size(232, 19)
        chkShowAll.TabIndex = 3
        chkShowAll.Text = "Show all games (force match a listed game)"
        chkShowAll.UseVisualStyleBackColor = True
        '
        ' lvGames
        '
        lvGames.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvGames.BorderStyle = BorderStyle.None
        lvGames.Columns.AddRange(New ColumnHeader() {colGame})
        lvGames.FullRowSelect = True
        lvGames.HeaderStyle = ColumnHeaderStyle.None
        lvGames.HideSelection = False
        lvGames.Location = New Point(12, 120)
        lvGames.MultiSelect = False
        lvGames.Name = "lvGames"
        lvGames.OwnerDraw = False
        lvGames.ShowGroups = False
        lvGames.Size = New Size(536, 250)
        lvGames.TabIndex = 4
        lvGames.UseCompatibleStateImageBehavior = False
        lvGames.View = View.Details
        '
        ' colGame
        '
        colGame.Text = "Game"
        colGame.Width = 520
        '
        ' btnAdd
        '
        btnAdd.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnAdd.Location = New Point(350, 380)
        btnAdd.Name = "btnAdd"
        btnAdd.Size = New Size(96, 28)
        btnAdd.TabIndex = 5
        btnAdd.Text = "Add"
        btnAdd.UseVisualStyleBackColor = True
        '
        ' btnCancel
        '
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Location = New Point(452, 380)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(96, 28)
        btnCancel.TabIndex = 6
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        '
        ' frmGamePicker
        '
        AcceptButton = btnAdd
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(560, 420)
        Controls.Add(btnCancel)
        Controls.Add(btnAdd)
        Controls.Add(lvGames)
        Controls.Add(chkShowAll)
        Controls.Add(txtSearch)
        Controls.Add(lblSearch)
        Controls.Add(lblIntro)
        FormBorderStyle = FormBorderStyle.Sizable
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(420, 320)
        Name = "frmGamePicker"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Add Game Manually"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblIntro As Label
    Friend WithEvents lblSearch As Label
    Friend WithEvents txtSearch As ThemedTextBox
    Friend WithEvents chkShowAll As CheckBox
    Friend WithEvents lvGames As ThemedListView
    Friend WithEvents colGame As ColumnHeader
    Friend WithEvents btnAdd As Button
    Friend WithEvents btnCancel As Button
End Class
