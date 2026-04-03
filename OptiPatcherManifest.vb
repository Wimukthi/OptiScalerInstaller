Public Class OptiPatcherManifest
    ' Records OptiPatcher installer actions for clean update/remove operations.
    Public Property InstallerVersion As String
    Public Property InstallTimeUtc As DateTime
    Public Property GameFolder As String
    Public Property PluginFolder As String
    Public Property AsiPath As String
    Public Property BackupPath As String
    Public Property OptiPatcherVersion As String
    Public Property OptiPatcherSource As String
    Public Property AssetName As String
    Public Property SourceUrl As String
    Public Property AssetSha256 As String
End Class

Public Class OptiPatcherInstallInfo
    ' Summarizes detected OptiPatcher install state for a game folder.
    Public Property IsInstalled As Boolean
    Public Property Version As String
    Public Property Source As String
    Public Property AsiPath As String
    Public Property PluginFolder As String
    Public Property Manifest As OptiPatcherManifest
End Class

Public Class OptiPatcherInstallConfig
    ' Captures OptiPatcher install options from the UI.
    Public Property GameFolder As String
    Public Property Source As OptiPatcherSource
    Public Property StableRelease As ReleaseInfo
    Public Property RollingRelease As ReleaseInfo
    Public Property AlternateRelease As ReleaseInfo
    Public Property LocalAsiPath As String
    Public Property ConflictMode As ConflictMode
    Public Property PluginPathOverride As String
End Class

