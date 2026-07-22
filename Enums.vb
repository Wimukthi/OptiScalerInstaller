Public Enum ReleaseSource
    ' Source for OptiScaler binaries.
    Stable
    Nightly
    LocalArchive
End Enum

Public Enum ConflictMode
    ' File conflict handling during install.
    BackupAndOverwrite
    Overwrite
    Skip
End Enum

Public Enum GpuVendor
    ' Simplified GPU vendor selection.
    Nvidia
    AmdIntel
    Unknown
End Enum

Public Enum AmdRdnaGeneration
    ' AMD RDNA generation, used to tailor the FSR4 INT8 experimental workflow.
    ' None = not an AMD RDNA adapter; Unknown = AMD RDNA but generation not resolved.
    None
    Rdna1
    Rdna2
    Rdna3
    Rdna4
    Unknown
End Enum

Public Enum FgTypeSelection
    ' Frame generation selection used in OptiScaler.ini.
    Auto
    None
    OptiFg
    Nukem
End Enum

Public Enum DefaultIniMode
    ' Controls how default INI templates are applied.
    Off
    Merge
    Replace
End Enum

Public Enum OptiPatcherSource
    ' Source for OptiPatcher plugin binary.
    Stable
    Rolling
    Alternate
    LocalFile
End Enum
