Public Class InstallerConfig
    Public Property GameExePath As String
    Public Property GameFolder As String
    Public Property HookName As String
    Public Property ConflictMode As ConflictMode
    Public Property Source As ReleaseSource
    Public Property StableRelease As ReleaseInfo
    Public Property NightlyRelease As ReleaseInfo
    Public Property LocalArchivePath As String
    Public Property GpuVendor As GpuVendor
    Public Property EnableDlssInputs As Boolean
    Public Property FgType As FgTypeSelection
    Public Property FakenvapiFolder As String
    Public Property NukemDllPath As String
    Public Property NvngxDllPath As String
    Public Property EnableReshade As Boolean
    Public Property ReshadeDllPath As String
    Public Property EnableSpecialK As Boolean
    Public Property SpecialKDllPath As String
    Public Property CreateSpecialKMarker As Boolean
    Public Property LoadAsiPlugins As Boolean
    Public Property PluginsPath As String
    Public Property DefaultIniMode As DefaultIniMode
    Public Property DefaultIniPath As String
End Class
