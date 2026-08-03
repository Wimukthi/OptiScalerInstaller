# Changelog

All notable changes to this project are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/). Version numbers are derived
from the build counter rather than semantic versioning — see
[docs/building.md](docs/building.md#versioning).

## [1.1.7.1] - 2026-08-03

### Fixed

- Settings tab group header rendered as "Update _Links" with an underlined L — the `&` in the caption was
  treated as a mnemonic prefix.
- "Auto-check installer updates on startup" had no visible or clickable check box. The startup toggles sat at
  hand-set coordinates that were narrower than their auto-sized captions, so the middle toggle covered the
  next one's box. All four now share a flow layout row that spaces them from their measured widths.
- Clipped hint text on the FSR4 INT8 (Manual) tab, where the `FsrAgilitySDKUpgrade` caption overlapped the
  note about restored INI keys. The note moved to its own line below the checkboxes.

## [1.1.7.0] - 2026-07-22

### Added

- `Fsr4ForceEnableInt8` toggle on the FSR4 INT8 tab.
- Searchable game picker for manual adds, shown when a folder name matches more than one compatibility-list
  entry.

### Changed

- Repurposed the FSR4 INT8 tab for RDNA2 and unofficial APUs, with GPU-generation-aware guidance and a
  redundancy warning: OptiScaler 0.9.4 auto-enables FSR 4.1.1 INT8 on RDNA3 desktop, and RDNA4 uses native
  FSR4.
- Verified install and add-on handling against OptiScaler 0.9.4 / FFX 2.3 SDK / FSR 4.1.1.
- Recorded the SharpCompress [GHSA-6c8g-7p36-r338](https://github.com/advisories/GHSA-6c8g-7p36-r338)
  exposure assessment as a NuGet audit suppression. The app extracts archives with its own
  path-traversal-guarded loop and never calls the vulnerable `WriteToDirectory`.

### Fixed

- Manual "Add game manually" no longer fails silently on ambiguous or unmatched titles — for example
  *God of War*, whose folder name is a prefix of both listed *God of War* entries.
- The bundled-Fakenvapi log now recognises `fakenvapi.dll`.

## [1.1.6.5] - 2026-05-26

### Fixed

- Game-specific wiki actions are disabled for compatibility rows without a wiki slug, so plain list rows no
  longer open the main list as if they had their own page.

## [1.1.6.4] - 2026-05-15

### Added

- Bulk game operations for detected supported games: bulk quick install and update for OptiScaler, and bulk
  install and update for supported OptiPatcher targets.

## [1.1.6.0] - 2026-04-30

### Added

- Keep-existing-`OptiScaler.ini` option for updates and reinstalls.
- JSON compatibility feed support, with parser diagnostics and cache validation.
- Release packaging gate: compatibility parser fixtures plus a live official-list canary.

### Changed

- The compatibility parser now handles the full official list, including plain table rows.

## [1.1.5.9] - 2026-04-26

### Fixed

- Markdown wiki-link parsing for entries whose game names or page slugs contain parentheses, which produced
  truncated URLs such as `Dead-Space-(2023`.

## [1.1.5.8] - 2026-04-24

### Changed

- Improved INI editor responsiveness and keyboard shortcuts.
- Updater release notes render as themed rich text, with Markdown headings, lists, code formatting, and
  clickable links.

## [1.1.5.7] - 2026-04-24

### Fixed

- Deep scan no longer aborts on corrupt or unreadable folders. File and folder enumeration is materialized
  inside exception handling, and per-drive failures are isolated.

## [1.1.5.2] - 2026-04-22

### Changed

- Consolidated the HTTP client across services to avoid socket exhaustion, with centralized retry.
- Moved WMI GPU detection off the UI thread for faster startup.
- Debounced game-search and game-folder text input for snappier filtering.
- Broadened anti-cheat detection: deeper folder scan, tighter Riot Vanguard tokens to prevent false
  positives, Easy Anti-Cheat `start_protected_game` bootstrapper coverage, and PunkBuster signatures.

### Fixed

- Compatibility list parsing handles the remote wiki table format and rejects anchor and section-anchor
  cross-references.
- Cache-preservation guards reject suspiciously low parse counts on remote list refreshes.

## [1.1.4.5] - 2026-04-10

### Fixed

- Detected-game install hangs, caused by UI-thread blocking during game-profile refresh.
- Missing wiki profile pages (404) are treated as non-fatal during profile fetch.
- Hardened wiki profile URL path handling.

## [1.1.4.3] - 2026-04-10

### Added

- Local `GameProfiles.json` catalog and profile service integration.
- Settings for the profile catalog URL and startup profile refresh.
- Automatic profile-driven install-option application, so detected game-specific defaults come from local
  profile data with offline-first behaviour.

## [1.1.3.7] - 2026-04-06

### Added

- Full OptiScaler INI editor: form and raw modes, validation, section grouping and filtering, descriptions
  from the reference docs, and backup/save/revert.
- Expanded Game Detection actions and quick-install flow.

### Changed

- Improved install and uninstall button-state gating.
- Refined OptiPatcher and INT8 detection status.
- Hardened Unreal-target preflight by auto-retargeting to `Binaries\Win64` or `WinGDK` when applicable.

## [1.1.1.1] - 2026-04-05

### Changed

- Drive selection is always explicit for deep-scan augmentation. The first-run one-time scan prompts for
  drives, and manual "Scan installed games" runs prompt instead of scanning every drive automatically.

## [1.1.0.9] - 2026-04-04

### Changed

- Merged launcher and deep scan into a single "Scan installed games" pipeline.
- Repurposed the secondary button into "Add game manually" — executable-based forced match with persistence.
- Improved install-state path selection and probing for nested DX11 and DX12 binary folders.

## [1.1.0.5] - 2026-04-04

### Fixed

- The FSR INT8 detected-games list is aligned with the canonical Game Detection result set, so counts and
  entries match. Duplicate and multi-path variants no longer appear only in the experimental list.

## [1.1.0.3] - 2026-04-04

### Added

- One-time first-start deep scan across all available drives.
- Uninstall-registry detection fallback for broader launcher coverage, including Battle.net-style installs.

### Changed

- Deep scan always augments launcher detection instead of replacing it.

### Fixed

- De-duplicated FSR INT8 detected-game entries after merged scans.

## [1.1.0.1] - 2026-04-04

### Changed

- Generalized deep-scan game-name matching for edge-case installs: better title tokenization for digits and
  apostrophes, ambiguity-safe relaxed prefix matching, and more executable probe paths (`x64_dx12`,
  `x64_dx11`, nested `bin`/`binaries` subfolders).

## [1.0.9.9] - 2026-04-04

### Fixed

- Manual deep-scan root normalization. Selected drives are scanned from their true roots (`G:\`) instead of
  drive-relative current directories, which previously missed launcher-independent installs.

## [1.0.9.7] - 2026-04-03

### Added

- Persistent deep-scan detected-games cache and a saved "Hide non-detected" preference.
- About → Sponsors tab with automatic public sponsor loading.

### Changed

- OptiScaler and OptiPatcher install-state columns refresh immediately after install and remove actions.
- Streamlined the Sponsors tab by removing extra helper and status text.

## [1.0.8.8]

### Added

- Async manual deep-drive scan with a drive picker dialog, for launcher-independent detection.
- "Hide non-detected" filter on the Game Detection tab.

### Changed

- Switched the drive picker list to a themed control to remove bright system borders.

### Removed

- Custom scan-folder setting and its UI.

## [1.0.8.2] - 2026-04-03

### Added

- "Sponsor Me" link in the About dialog, with themed link styling.

## [1.0.8.0] - 2026-04-03

### Changed

- Hardened release packaging with clean staged zip output, preventing stale top-level `win-x64` payloads.

### Fixed

- OptiPatcher detection uses the same nested install probing as OptiScaler.
- Improved detected-game folder matching for nested binary paths.

## [1.0.7.6] - 2026-04-03

### Added

- Full OptiPatcher integration: support-list sync, release fetch (stable, rolling, alternate), install and
  remove services with manifests, a detection column on Game Detection, supported-only enforcement, and an
  Install-tab "Install + OptiPatcher" guided flow with preflight and summary.

### Changed

- Improved OptiPatcher panel layout and ASI folder auto-setup.

## [1.0.6.7] - 2026-04-02

### Changed

- Hardened uninstall and remove safety with root-scoped manifest path validation.
- Added archive extraction traversal guards.
- Improved release asset selection and digest verification.
- Added retry and timeout handling for release, compatibility, and download fetches.
- Tightened startup background error handling.

## [1.0.6.1] - 2026-03-30

### Added

- FSR4 experimental tab visibility toggle for unsupported GPUs.
- Detected GPU model in the title bar.

### Changed

- Hardened GPU detection with vendor-ID matching.

### Fixed

- Runtime tab theming artifacts.

## [1.0.5.2] - 2026-03-27

### Added

- Startup auto-update-check toggle.
- OptiScaler project link in the About dialog.

### Changed

- Hardened OptiScaler detection and executable resolution: fewer false positives, stronger manifest
  validation, improved nested binary probing.

## [1.0.5.0] - 2026-03-26

### Fixed

- Fatal initialization errors now show an explicit dialog and terminate cleanly, instead of silently running
  as a background process.

## [1.0.4.9] - 2026-03-26

### Fixed

- Install-status detection for games that load from nested binary folders such as `Binaries\Win64`.

## [1.0.4.8] - 2026-03-26

### Fixed

- False-positive OptiScaler detection. Generic hook DLLs such as a stock `dbghelp.dll` now require strong
  install markers or OptiScaler metadata before being treated as installed.

## [1.0.4.7] - 2026-03-25

### Added

- About dialog metadata view.
- Anti-cheat detection and warnings.
- Startup compatibility auto-refresh option.
- Custom scan folder support.

## [1.0.4.2] - 2026-03-21

### Added

- Compatibility diff tracking with row highlighting.
- Game workaround templates.
- Archive SHA-256 provenance.
- Post-install verification reporting.

### Changed

- Stronger preflight validation.

## [1.0.3.9] - 2026-01-30

### Changed

- Renamed the nightly source to "alternate", and skipped its checks when unset.

## [1.0.3.8] - 2026-01-29

### Fixed

- Missing OptiScaler release URLs (404) no longer block other release data.

## [1.0.3.7] - 2026-01-29

### Added

- Default install presets.
- Diagnostics export.
- EA App and Ubisoft Connect detection.

### Changed

- Updater rollback safety.

## [1.0.3.5] - 2026-01-15

### Fixed

- Game Detection action button alignment, and the update button label when a new version is available.

## [1.0.3.4] - 2026-01-15

### Fixed

- Restored list view headers by correcting dock order in the Game Detection layout.

## [1.0.3.2] - 2026-01-14

### Fixed

- Stabilized the Game Detection header layout with layout panels, and fixed dock order.

## [1.0.2.8] - 2026-01-14

### Fixed

- Game Detection layout spacing, docked the header and footer, and persisted the dark theme by default.

## [1.0.1.7] - 2026-01-14

### Changed

- Versioning auto-bumps patch, minor, and major every 10, 100, and 1000 builds.

## [1.0.0.16] - 2026-01-14

### Added

- Silent release refresh and update check.
- Error logging.

## [1.0.0.15] - 2026-01-14

### Removed

- FSR4 compatibility detection and its settings and UI.

## [1.0.0.14] - 2026-01-14

### Changed

- Improved GPU auto-detection with a WMI fallback and adapter logging.

## [1.0.0.13] - 2026-01-13

### Added

- GPU vendor auto-detection.

### Fixed

- FSR4 list parsing.

## [1.0.0.11] - 2026-01-13

### Fixed

- Updater version comparison now respects the build revision.

## [1.0.0.9] - 2026-01-13

### Added

- Legacy uninstall fallback.

### Fixed

- Install root detection, and normalization of detected paths.

## [1.0.0.6] - 2026-01-13

First public release: detection list install status, default INI support, the installer updater, and install
action prompts.

[1.1.7.1]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.7.1
[1.1.7.0]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.7.0
[1.1.6.5]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.6.5
[1.1.6.4]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.6.4
[1.1.6.0]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.6.0
[1.1.5.9]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.5.9
[1.1.5.8]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.5.8
[1.1.5.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.5.7
[1.1.5.2]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.5.2
[1.1.4.5]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.4.5
[1.1.4.3]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.4.3
[1.1.3.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.3.7
[1.1.1.1]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.1.1
[1.1.0.9]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.0.9
[1.1.0.5]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.0.5
[1.1.0.3]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.0.3
[1.1.0.1]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.1.0.1
[1.0.9.9]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.9.9
[1.0.9.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.9.7
[1.0.8.2]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.8.2
[1.0.8.0]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.8.0
[1.0.7.6]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.7.6
[1.0.6.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.6.7
[1.0.6.1]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.6.1
[1.0.5.2]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.5.2
[1.0.5.0]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.5.0
[1.0.4.9]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.4.9
[1.0.4.8]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.4.8
[1.0.4.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.4.7
[1.0.4.2]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.4.2
[1.0.3.9]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.3.9
[1.0.3.8]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.3.8
[1.0.3.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.3.7
[1.0.3.5]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.3.5
[1.0.3.4]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.3.4
[1.0.3.2]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.3.2
[1.0.2.8]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.2.8
[1.0.1.7]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.1.7
[1.0.0.16]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.16
[1.0.0.15]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.15
[1.0.0.14]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.14
[1.0.0.13]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.13
[1.0.0.11]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.11
[1.0.0.9]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.9
[1.0.0.6]: https://github.com/Wimukthi/OptiScalerInstaller/releases/tag/v1.0.0.6
