# Building

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows — the project targets `net10.0-windows` and uses Windows Forms
- Optional: Visual Studio 2022 or 2026 with the .NET desktop development workload, for the form designers

## Build and run

```bash
dotnet build OptiScalerInstaller.sln -c Release
```

The app is a framework-dependent build; running it needs the .NET 10 Desktop Runtime, which the SDK
includes.

```bash
./bin/Release/net10.0-windows/OptiScalerInstaller.exe
```

Use `-c Debug` and `bin/Debug/net10.0-windows/` for a debug build.

## Tests

`tests/CompatibilityParser.Tests` is a console runner that exercises the compatibility-list parser against
fixtures — Markdown tables with linked and plain rows, parenthesised wiki slugs, the JSON catalog format, the
bundled list, and ambiguous manual-add matching.

```bash
powershell -ExecutionPolicy Bypass -File ./scripts/TestCompatibilityParser.ps1
```

Add `-Live` to also fetch the official compatibility list and assert it still parses. That canary is what
catches upstream wiki format changes.

```bash
powershell -ExecutionPolicy Bypass -File ./scripts/TestCompatibilityParser.ps1 -Live
```

The script builds against a throwaway copy of `BuildVersion.txt`, so running tests does not bump the version.

## Versioning

`BuildVersion.txt` holds a single integer that increments on every successful build. The MSBuild targets in
`OptiScalerInstaller.vbproj` derive the four-part version from it:

| Part | Formula | Increments |
| --- | --- | --- |
| Major | `1 + build / 1000` | every 1000 builds |
| Minor | `(build % 1000) / 100` | every 100 builds |
| Patch | `(build % 100) / 10` | every 10 builds |
| Revision | `build % 10` | every build |

Build 170 is `v1.1.7.0`. The same value is used for `AssemblyVersion`, `FileVersion`, and
`InformationalVersion`, and is what the release packager names the zip after.

Because every build bumps the counter, `BuildVersion.txt` shows up as modified after local builds. Commit it
with the release; discard it otherwise.

## Release packaging

```bash
powershell -ExecutionPolicy Bypass -File ./scripts/BuildReleasePackage.ps1
```

The script:

1. Runs the compatibility parser tests including the live canary, and refuses to continue if they fail.
2. Cleans and rebuilds in Release.
3. Reads the version from the built executable.
4. Stages the payload — the executable, its dependencies, `Data/`, `runtimes/`, `LICENSE`, and `README.md` —
   into `artifacts/staging/`, failing loudly if anything required is missing.
5. Strips volatile runtime files (`*.settings.json`, logs) that a local run may have left behind.
6. Writes `Release/OptiScalerInstaller-v<version>-win-x64.zip` and a matching `.sha256` file beside it.

The executable is not code-signed, so publish the `.sha256` alongside the zip on the release page. It is the
only way a user can confirm the download is the file that was built here.

Useful switches: `-SkipCompatibilityValidation` skips the parser gate, `-SkipBuild` packages whatever is
already in `bin/`, `-SkipClean` keeps the existing output.

Publishing the zip to GitHub Releases is a manual step; there is no CI pipeline.

## Repository layout

```
*.vb                            Application source (flat, at the repository root)
  MainForm.*                    Main window — tabs, detection table, install flow
  frm*.vb                       Dialogs: About, Bulk Operations, Drive Selection,
                                Game Picker, INI Editor, Update
  *Service.vb                   Detection, install, release, compatibility, profile services
  Themed*.vb, ThemeManager.vb   Dark/light theming for WinForms controls
Data/                           Bundled offline data, copied to the output directory
Icons/                          Application icon
docs/                           This documentation and its screenshots
scripts/                        Test and release-packaging scripts
tests/                          Compatibility parser test project
```

## Dependencies

| Package | Used for |
| --- | --- |
| SharpCompress | Extracting `.7z` OptiScaler archives |
| System.Management | WMI GPU detection fallback |

`OptiScalerInstaller.vbproj` suppresses the NuGet audit warning for SharpCompress advisory
[GHSA-6c8g-7p36-r338](https://github.com/advisories/GHSA-6c8g-7p36-r338), with the exposure assessment
recorded in a comment next to the suppression: the advisory covers `IArchive.WriteToDirectory()`, which this
app never calls. `ExtractArchive` in `InstallerService.vb` iterates entries itself and rejects traversal and
rooted paths. The test project inherits the package transitively and repeats the suppression.
