Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Friend Class ThemedGroupBox
    Inherits GroupBox

    Private _borderColor As Color = SystemColors.ControlDark

    Public Sub New()
        DoubleBuffered = True
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
    End Sub

    <DefaultValue(GetType(Color), "ControlDark")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        e.Graphics.Clear(BackColor)

        Dim textSize As Size = TextRenderer.MeasureText(Text, Font)
        Dim textRect As New Rectangle(8, 0, textSize.Width, textSize.Height)
        TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor)

        Dim borderRect As New Rectangle(0, textSize.Height \ 2, Width - 1, Height - (textSize.Height \ 2) - 1)

        Using pen As New Pen(_borderColor)
            e.Graphics.DrawLine(pen, borderRect.Left, borderRect.Top, textRect.Left - 2, borderRect.Top)
            e.Graphics.DrawLine(pen, textRect.Right + 2, borderRect.Top, borderRect.Right, borderRect.Top)
            e.Graphics.DrawLine(pen, borderRect.Left, borderRect.Top, borderRect.Left, borderRect.Bottom)
            e.Graphics.DrawLine(pen, borderRect.Right, borderRect.Top, borderRect.Right, borderRect.Bottom)
            e.Graphics.DrawLine(pen, borderRect.Left, borderRect.Bottom, borderRect.Right, borderRect.Bottom)
        End Using
    End Sub
End Class
