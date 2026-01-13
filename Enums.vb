Public Enum ReleaseSource
    Stable
    Nightly
    LocalArchive
End Enum

Public Enum ConflictMode
    BackupAndOverwrite
    Overwrite
    Skip
End Enum

Public Enum GpuVendor
    Nvidia
    AmdIntel
    Unknown
End Enum

Public Enum FgTypeSelection
    Auto
    None
    OptiFg
    Nukem
End Enum

Public Enum DefaultIniMode
    Off
    Merge
    Replace
End Enum
