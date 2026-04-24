Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

<DefaultEvent("TextChanged")>
Friend Class ThemedRichTextBox
    ' UserControl wrapper that renders a themed border around a RichTextBox.
    Inherits UserControl

    Private ReadOnly _richTextBox As RichTextBox
    Private _borderColor As Color = SystemColors.ControlDark
    Private _focusBorderColor As Color = SystemColors.Highlight

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)

        Padding = New Padding(6, 4, 6, 4)
        BackColor = SystemColors.Window
        ForeColor = SystemColors.WindowText
        MinimumSize = New Size(0, 48)

        _richTextBox = New RichTextBox() With {
            .BorderStyle = BorderStyle.None,
            .DetectUrls = True,
            .Dock = DockStyle.Fill,
            .ReadOnly = True,
            .ScrollBars = RichTextBoxScrollBars.Vertical,
            .ShortcutsEnabled = True,
            .TabStop = False,
            .WordWrap = True,
            .BackColor = BackColor,
            .ForeColor = ForeColor
        }

        Controls.Add(_richTextBox)
        TabStop = True

        AddHandler _richTextBox.TextChanged, AddressOf HandleTextChanged
        AddHandler _richTextBox.GotFocus, Sub() Invalidate()
        AddHandler _richTextBox.LostFocus, Sub() Invalidate()
    End Sub

    <Browsable(False)>
    Public ReadOnly Property InnerRichTextBox As RichTextBox
        Get
            Return _richTextBox
        End Get
    End Property

    <Browsable(True)>
    Public Overrides Property Text As String
        Get
            Return _richTextBox.Text
        End Get
        Set(value As String)
            If _richTextBox.Text <> value Then
                _richTextBox.Text = value
            End If
        End Set
    End Property

    <Browsable(True), DefaultValue(True)>
    Public Property [ReadOnly] As Boolean
        Get
            Return _richTextBox.ReadOnly
        End Get
        Set(value As Boolean)
            _richTextBox.ReadOnly = value
        End Set
    End Property

    <Browsable(True), DefaultValue(True)>
    Public Property DetectUrls As Boolean
        Get
            Return _richTextBox.DetectUrls
        End Get
        Set(value As Boolean)
            _richTextBox.DetectUrls = value
        End Set
    End Property

    <Browsable(True), DefaultValue(GetType(RichTextBoxScrollBars), "Vertical")>
    Public Property ScrollBars As RichTextBoxScrollBars
        Get
            Return _richTextBox.ScrollBars
        End Get
        Set(value As RichTextBoxScrollBars)
            _richTextBox.ScrollBars = value
        End Set
    End Property

    <Browsable(True), DefaultValue(True)>
    Public Property WordWrap As Boolean
        Get
            Return _richTextBox.WordWrap
        End Get
        Set(value As Boolean)
            _richTextBox.WordWrap = value
        End Set
    End Property

    <Browsable(True), DefaultValue(GetType(Color), "ControlDark")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(True), DefaultValue(GetType(Color), "Highlight")>
    Public Property FocusBorderColor As Color
        Get
            Return _focusBorderColor
        End Get
        Set(value As Color)
            _focusBorderColor = value
            Invalidate()
        End Set
    End Property

    Public Sub AppendText(value As String)
        _richTextBox.AppendText(value)
    End Sub

    Public Sub Clear()
        _richTextBox.Clear()
    End Sub

    Public Sub SelectAll()
        _richTextBox.SelectAll()
    End Sub

    Public Sub ScrollToTop()
        _richTextBox.SelectionStart = 0
        _richTextBox.SelectionLength = 0
        _richTextBox.ScrollToCaret()
    End Sub

    Protected Overrides Sub OnBackColorChanged(e As EventArgs)
        MyBase.OnBackColorChanged(e)
        If _richTextBox Is Nothing Then
            Return
        End If
        _richTextBox.BackColor = BackColor
        Invalidate()
    End Sub

    Protected Overrides Sub OnForeColorChanged(e As EventArgs)
        MyBase.OnForeColorChanged(e)
        If _richTextBox Is Nothing Then
            Return
        End If
        _richTextBox.ForeColor = ForeColor
        Invalidate()
    End Sub

    Protected Overrides Sub OnEnter(e As EventArgs)
        MyBase.OnEnter(e)
        _richTextBox.Focus()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        _richTextBox.Focus()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        Dim color As Color = If(_richTextBox.Focused, _focusBorderColor, _borderColor)
        Using pen As New Pen(color)
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)
            e.Graphics.DrawRectangle(pen, rect)
        End Using
    End Sub

    Private Sub HandleTextChanged(sender As Object, e As EventArgs)
        OnTextChanged(e)
    End Sub
End Class
