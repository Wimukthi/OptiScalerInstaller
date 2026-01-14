Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Friend Module ThemeManager
    ' Applies the custom light/dark palette to supported WinForms controls.
    Public Sub ApplyTheme(root As Control, mode As SystemColorMode)
        Dim palette As ThemePalette = ThemePalette.FromMode(mode)
        ApplyToControl(root, palette)
    End Sub

    Private Sub ApplyToControl(control As Control, palette As ThemePalette)
        If control Is Nothing Then
            Return
        End If

        If TypeOf control Is ThemedTextBox Then
            Dim themedTextBox As ThemedTextBox = DirectCast(control, ThemedTextBox)
            themedTextBox.BackColor = palette.Field
            themedTextBox.ForeColor = palette.Text
            themedTextBox.BorderColor = palette.Border
            themedTextBox.FocusBorderColor = palette.FocusBorder
        ElseIf TypeOf control Is ThemedListView Then
            Dim themedListView As ThemedListView = DirectCast(control, ThemedListView)
            themedListView.BackColor = palette.Surface
            themedListView.ForeColor = palette.Text
            themedListView.HeaderBackColor = palette.Surface
            themedListView.HeaderForeColor = palette.Text
            themedListView.RowBackColor = palette.Surface
            themedListView.RowAltBackColor = If(palette.IsDark, Color.FromArgb(36, 36, 36), palette.Surface)
            themedListView.RowForeColor = palette.Text
            themedListView.SelectedBackColor = If(palette.IsDark, Color.FromArgb(100, palette.FocusBorder), SystemColors.Highlight)
            themedListView.SelectedForeColor = If(palette.IsDark, palette.Text, SystemColors.HighlightText)
            themedListView.GridLineColor = If(palette.IsDark, Color.FromArgb(80, palette.Border), palette.Border)
        ElseIf TypeOf control Is ThemedGroupBox Then
            Dim themedGroupBox As ThemedGroupBox = DirectCast(control, ThemedGroupBox)
            themedGroupBox.BackColor = palette.Surface
            themedGroupBox.ForeColor = palette.Text
            themedGroupBox.BorderColor = palette.Border
        ElseIf TypeOf control Is Button Then
            Dim button As Button = DirectCast(control, Button)
            If palette.IsDark Then
                button.FlatStyle = FlatStyle.Flat
                button.FlatAppearance.BorderColor = palette.Border
                button.FlatAppearance.BorderSize = 1
                button.BackColor = palette.Surface
                button.ForeColor = palette.Text
                button.UseVisualStyleBackColor = False
            Else
                button.FlatStyle = FlatStyle.Standard
                button.UseVisualStyleBackColor = True
                button.ForeColor = palette.Text
            End If
        ElseIf TypeOf control Is Label Then
            control.ForeColor = If(control.Enabled, palette.Text, palette.MutedText)
        ElseIf TypeOf control Is CheckBox OrElse TypeOf control Is RadioButton Then
            control.ForeColor = If(control.Enabled, palette.Text, palette.MutedText)
        ElseIf TypeOf control Is TabControl OrElse TypeOf control Is TabPage OrElse TypeOf control Is Panel OrElse
            TypeOf control Is TableLayoutPanel OrElse TypeOf control Is FlowLayoutPanel OrElse TypeOf control Is SplitContainer Then
            control.BackColor = palette.Surface
            control.ForeColor = palette.Text
        ElseIf TypeOf control Is StatusStrip Then
            Dim strip As StatusStrip = DirectCast(control, StatusStrip)
            strip.BackColor = palette.Surface
            strip.ForeColor = palette.Text
            For Each item As ToolStripItem In strip.Items
                item.ForeColor = palette.Text
                item.BackColor = palette.Surface
            Next
        Else
            If TypeOf control Is Form Then
                control.BackColor = palette.Base
                control.ForeColor = palette.Text
            End If
        End If

        For Each child As Control In control.Controls
            ApplyToControl(child, palette)
        Next
    End Sub

    Private Structure ThemePalette
        Public Property IsDark As Boolean
        Public Property Base As Color
        Public Property Surface As Color
        Public Property Field As Color
        Public Property Border As Color
        Public Property FocusBorder As Color
        Public Property Text As Color
        Public Property MutedText As Color

        Public Shared Function FromMode(mode As SystemColorMode) As ThemePalette
            If mode = SystemColorMode.Dark Then
                Return New ThemePalette With {
                    .IsDark = True,
                    .Base = Color.FromArgb(24, 24, 24),
                    .Surface = Color.FromArgb(32, 32, 32),
                    .Field = Color.FromArgb(45, 45, 45),
                    .Border = Color.FromArgb(70, 70, 70),
                    .FocusBorder = Color.FromArgb(90, 120, 200),
                    .Text = Color.Gainsboro,
                    .MutedText = Color.FromArgb(140, 140, 140)
                }
            End If

            Return New ThemePalette With {
                .IsDark = False,
                .Base = SystemColors.Control,
                .Surface = SystemColors.Control,
                .Field = SystemColors.Window,
                .Border = SystemColors.ControlDark,
                .FocusBorder = SystemColors.Highlight,
                .Text = SystemColors.ControlText,
                .MutedText = SystemColors.GrayText
            }
        End Function
    End Structure
End Module
