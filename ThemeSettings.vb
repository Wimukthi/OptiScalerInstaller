Option Strict On
Option Explicit On

Imports System.Windows.Forms

Friend Module ThemeSettings
    Public Function GetPreferredColorMode() As SystemColorMode
        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim value As String = settings.Theme
        If String.Equals(value, "dark", StringComparison.OrdinalIgnoreCase) Then
            Return SystemColorMode.Dark
        End If
        If String.Equals(value, "classic", StringComparison.OrdinalIgnoreCase) Then
            Return SystemColorMode.Classic
        End If
        If String.Equals(value, "system", StringComparison.OrdinalIgnoreCase) Then
            Return SystemColorMode.System
        End If

        Return SystemColorMode.System
    End Function

    Public Sub SavePreferredColorMode(mode As SystemColorMode)
        Dim value As String
        Select Case mode
            Case SystemColorMode.Dark
                value = "dark"
            Case SystemColorMode.Classic
                value = "classic"
            Case Else
                value = "system"
        End Select

        Dim settings As AppSettingsModel = AppSettings.Load()
        settings.Theme = value
        AppSettings.Save(settings)
    End Sub
End Module
