Option Strict On
Option Explicit On

Friend Class BulkGameOperationPlan
    Public Property GameName As String
    Public Property Platform As String
    Public Property InstallPath As String
    Public Property GameExePath As String
    Public Property AntiCheat As String
    Public Property OptiScalerStatus As String
    Public Property OptiPatcherStatus As String
    Public Property IsOptiScalerInstalled As Boolean
    Public Property IsOptiScalerUpdateAvailable As Boolean
    Public Property IsOptiPatcherInstalled As Boolean
    Public Property IsOptiPatcherSupported As Boolean
    Public Property IsOptiPatcherUpdateAvailable As Boolean
    Public Property DetectedGame As DetectedGame
    Public Property OptiScalerInfo As OptiScalerInstallInfo
    Public Property OptiPatcherInfo As OptiPatcherInstallInfo

    Public ReadOnly Property HasAntiCheatWarning As Boolean
        Get
            Return Not String.IsNullOrWhiteSpace(AntiCheat)
        End Get
    End Property
End Class
