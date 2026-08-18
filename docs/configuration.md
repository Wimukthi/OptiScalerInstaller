# Configuration

## Files the app writes

Everything lives next to `OptiScalerInstaller.exe`, so extract the release to a folder you can write to.

| Path | Purpose |
| --- | --- |
| `OptiScalerInstaller.settings.json` | Your settings. Created on first run by merging `Data/DefaultSettings.json`. |
| `OptiScalerInstaller.deep-scan-cache.json` | Games found by drive scan, so they stay detected across restarts. |
| `Errors/Error_Log.txt` | Structured error log. Handled exceptions are recorded here and execution continues. |
| `Data/` | Bundled offline fallbacks — the compatibility list, game profiles and templates, and the OptiScaler INI reference. Ships with the release; do not delete. |

Inside each patched game folder:

| Path | Purpose |
| --- | --- |
| `OptiScalerInstaller.manifest.json` | What the installer wrote, where it came from, and what it backed up. Uninstall reads this. |
| `OptiPatcherInstaller.manifest.json` | The same, for OptiPatcher. |
| `*.bak_<timestamp>` | Files backed up under the "Backup and overwrite" conflict mode. |

## Settings reference

Edit through the Settings tab, or open the file directly with **Open settings file**. Unknown keys are
preserved; missing keys fall back to the bundled defaults.

### Sources

| Key | Default | Description |
| --- | --- | --- |
| `CompatibilityListUrl` | OptiScaler wiki `Compatibility-List.md` | Tested-games list. A JSON feed is also accepted; Markdown is the fallback. |
| `WikiBaseUrl` | `https://github.com/optiscaler/OptiScaler/wiki/` | Base for per-game wiki links. |
| `StableReleaseUrl` | OptiScaler `releases/latest` | Stable OptiScaler source. |
| `NightlyReleaseUrl` | *(empty)* | Alternate OptiScaler source. Skipped entirely when unset. |
| `ComponentReleaseUrl` | *(empty)* | Optional feed for bundled components. No UI field; set it here. |
| `InstallerReleaseUrl` | This project's `releases/latest` | Where the installer checks for its own updates. |
| `GameProfilesCatalogUrl` | *(empty)* | Remote per-game profile catalog. Falls back to bundled `Data/GameProfiles.json`. |

### OptiPatcher

| Key | Default | Description |
| --- | --- | --- |
| `OptiPatcherSupportListUrl` | OptiPatcher `GameSupport.md` | Which games OptiPatcher supports. |
| `OptiPatcherStableReleaseUrl` | OptiPatcher `releases` | Stable plugin source. |
| `OptiPatcherRollingReleaseUrl` | OptiPatcher `releases/tags/rolling` | Rolling plugin source. |
| `OptiPatcherAlternateReleaseUrl` | *(empty)* | Third-party or self-hosted source. |
| `OptiPatcherPreferredSource` | `Rolling` | `Stable`, `Rolling`, `Alternate`, or `LocalFile`. |
| `OptiPatcherLocalPath` | *(empty)* | Path to a local `.asi` when the source is `LocalFile`. |

### Startup and UI

| Key | Default | Description |
| --- | --- | --- |
| `Theme` | `dark` | `dark` or `light`. |
| `AutoRefreshCompatibilityOnStartup` | `true` | Refresh the compatibility list on launch. |
| `AutoRefreshGameProfilesOnStartup` | `false` | Refresh the profile catalog on launch. |
| `AutoCheckInstallerUpdates` | `true` | Check for a newer installer on launch and show a non-intrusive notice. |
| `HasCompletedInitialDeepScan` | `false` | Set to `true` after the one-time first-run scan prompt. |
| `HideNonDetectedGames` | `false` | Remembers the Game Detection filter. |
| `ShowExperimentalTabOnUnsupportedGpu` | `false` | Force-show the FSR4 INT8 tab on GPUs where it would be hidden. |
| `HighlightCompatibilityChanges` | `true` | Highlight rows that changed on the last list refresh. |
| `EnableGameTemplates` | `true` | Auto-apply per-game workaround templates when a detected game is selected. |
| `WindowX`, `WindowY`, `WindowWidth`, `WindowHeight`, `WindowState` | *(unset)* | Window geometry, saved on exit. |

### Default install options

These seed the Install tab. They are what **Quick install** and bulk operations use.

| Key | Default | Description |
| --- | --- | --- |
| `DefaultPreset` | `Custom` | `Custom`, `NVIDIA + DLSS Inputs`, or `AMD/Intel (DLSS Inputs Off)`. |
| `DefaultHookName` | `dxgi.dll` | Also `winmm.dll`, `version.dll`, `dbghelp.dll`, `d3d12.dll`, `wininet.dll`, `winhttp.dll`, `OptiScaler.asi`. |
| `DefaultGpuVendor` | `Auto` | `Auto`, `Nvidia`, or `AmdIntel`. |
| `DefaultDlssInputs` | `true` | Enable DLSS input spoofing. |
| `DefaultFrameGeneration` | `Auto` | `Auto`, `None`, `OptiFg`, or `Nukem`. |
| `DefaultConflictMode` | `BackupAndOverwrite` | `BackupAndOverwrite`, `Overwrite`, or `Skip`. |
| `DefaultIniMode` | `Off` | `Off`, `Merge`, or `Replace` — how a global INI template is applied at install time. |
| `DefaultIniPath` | *(empty)* | Path to that template. |
| `PreserveExistingIniOnUpdate` | `true` | Keep the game's existing `OptiScaler.ini` on update or reinstall. |
| `ConfirmQuickInstall` | `true` | Show the install review dialog before a quick or bulk install writes anything. Set to `false` for one-click behaviour. No UI field; set it here. |

### FSR4 INT8

| Key | Default | Description |
| --- | --- | --- |
| `ExperimentalFsr4PackageFolder` | *(empty)* | Folder holding the manual INT8 package. |
| `ExperimentalFsr4EnableUpdate` | `true` | Set `Fsr4Update=true` when applying. |
| `ExperimentalFsr4EnableAgility` | `false` | Set `FsrAgilitySDKUpgrade=true` — helps some Windows 10 titles. |
| `ExperimentalFsr4ForceInt8` | *(unset)* | Set `Fsr4ForceEnableInt8=true` to force INT8 on RDNA2 and APUs. |

## Resetting

**Load defaults** on the Settings tab restores the bundled values without touching your window geometry.
To start completely clean, close the app and delete `OptiScalerInstaller.settings.json` and
`OptiScalerInstaller.deep-scan-cache.json`.
