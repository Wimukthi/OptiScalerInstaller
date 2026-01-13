Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

<DefaultEvent("TextChanged")>
Friend Class ThemedTextBox
    Inherits UserControl

    Private ReadOnly _textBox As TextBox
    Private _borderColor As Color = SystemColors.ControlDark
    Private _focusBorderColor As Color = SystemColors.Highlight

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)

        Padding = New Padding(6, 3, 6, 3)
        BackColor = SystemColors.Window
        ForeColor = SystemColors.WindowText
        MinimumSize = New Size(0, 24)

        _textBox = New TextBox() With {
            .BorderStyle = BorderStyle.None,
            .Dock = DockStyle.Fill,
            .Multiline = False,
            .BackColor = BackColor,
            .ForeColor = ForeColor,
            .TabStop = False
        }

        Controls.Add(_textBox)
        TabStop = True

        AddHandler _textBox.TextChanged, AddressOf HandleTextChanged
        AddHandler _textBox.GotFocus, Sub() Invalidate()
        AddHandler _textBox.LostFocus, Sub() Invalidate()
    End Sub

    <Browsable(True)>
    Public Overrides Property Text As String
        Get
            Return _textBox.Text
        End Get
        Set(value As String)
            If _textBox.Text <> value Then
                _textBox.Text = value
            End If
        End Set
    End Property

    <Browsable(True), DefaultValue(False)>
    Public Property [ReadOnly] As Boolean
        Get
            Return _textBox.ReadOnly
        End Get
        Set(value As Boolean)
            _textBox.ReadOnly = value
        End Set
    End Property

    <Browsable(True), DefaultValue(False)>
    Public Property Multiline As Boolean
        Get
            Return _textBox.Multiline
        End Get
        Set(value As Boolean)
            _textBox.Multiline = value
        End Set
    End Property

    <Browsable(True), DefaultValue(GetType(ScrollBars), "None")>
    Public Property ScrollBars As ScrollBars
        Get
            Return _textBox.ScrollBars
        End Get
        Set(value As ScrollBars)
            _textBox.ScrollBars = value
        End Set
    End Property

    <Browsable(True), DefaultValue(True)>
    Public Property WordWrap As Boolean
        Get
            Return _textBox.WordWrap
        End Get
        Set(value As Boolean)
            _textBox.WordWrap = value
        End Set
    End Property

    <Browsable(True), DefaultValue(False)>
    Public Property UseSystemPasswordChar As Boolean
        Get
            Return _textBox.UseSystemPasswordChar
        End Get
        Set(value As Boolean)
            _textBox.UseSystemPasswordChar = value
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
        _textBox.AppendText(value)
    End Sub

    Public Sub Clear()
        _textBox.Clear()
    End Sub

    Public Sub ScrollToEnd()
        _textBox.SelectionStart = _textBox.TextLength
        _textBox.SelectionLength = 0
        _textBox.ScrollToCaret()
    End Sub

    Protected Overrides Sub OnBackColorChanged(e As EventArgs)
        MyBase.OnBackColorChanged(e)
        If _textBox Is Nothing Then
            Return
        End If
        _textBox.BackColor = BackColor
        Invalidate()
    End Sub

    Protected Overrides Sub OnForeColorChanged(e As EventArgs)
        MyBase.OnForeColorChanged(e)
        If _textBox Is Nothing Then
            Return
        End If
        _textBox.ForeColor = ForeColor
        Invalidate()
    End Sub

    Protected Overrides Sub OnEnter(e As EventArgs)
        MyBase.OnEnter(e)
        _textBox.Focus()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        _textBox.Focus()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        Dim color As Color = If(_textBox.Focused, _focusBorderColor, _borderColor)
        Using pen As New Pen(color)
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)
            e.Graphics.DrawRectangle(pen, rect)
        End Using
    End Sub

    Private Sub HandleTextChanged(sender As Object, e As EventArgs)
        OnTextChanged(e)
    End Sub
End Class
