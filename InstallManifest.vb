Imports System.Text.Json.Serialization

Public Class InstallManifest
    ' Records installer actions to support clean uninstalls and updates.
    Public Property InstallerVersion As String
    Public Property OptiScalerVersion As String
    Public Property OptiScalerSource As String
    Public Property InstallTimeUtc As DateTime
    Public Property GameFolder As String
    Public Property HookName As String
    Public Property ArchiveFileName As String
    Public Property ArchiveSourceUrl As String
    Public Property ArchiveSha256 As String
    Public Property ArchiveSizeBytes As Long
    Public Property PackageRoot As String
    Public Property VerificationTimeUtc As DateTime?
    Public Property InstalledFiles As List(Of String)
    Public Property BackupFiles As Dictionary(Of String, String)
End Class
