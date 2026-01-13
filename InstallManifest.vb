Imports System.Text.Json.Serialization

Public Class InstallManifest
    Public Property InstallerVersion As String
    Public Property InstallTimeUtc As DateTime
    Public Property GameFolder As String
    Public Property HookName As String
    Public Property InstalledFiles As List(Of String)
    Public Property BackupFiles As Dictionary(Of String, String)
End Class
