# OptiScaler Installer

A Windows desktop app that installs, updates, configures, and removes
[OptiScaler](https://github.com/optiscaler/OptiScaler) — and the companion
[OptiPatcher](https://github.com/optiscaler/OptiPatcher) plugin — for the games you already have installed.

It finds your games, tells you which ones OptiScaler is known to work with, picks sane defaults per game,
and keeps track of what it put where so uninstalling is clean.

![Game Detection](docs/screenshots/game-detection.png)

## Features

- **Game detection** — scans Steam, Epic, GOG, EA App, Ubisoft Connect, and the uninstall registry, with an
  optional launcher-independent drive scan for everything else.
- **Compatibility list** — the official OptiScaler tested-games list, showing detection status, installed
  OptiScaler/OptiPatcher versions, available updates, and anti-cheat hints.
- **Guided install** — hook DLL, GPU vendor, DLSS input spoofing, and frame-generation mode, with per-game
  profiles applied automatically and preflight validation before anything is written.
- **Add-ons** — Fakenvapi, Nukem frame generation, `nvngx_dlss.dll`, ReShade, Special K, and ASI plugins.
- **INI editor** — grouped form view over `OptiScaler.ini` with descriptions and validation, plus a raw text
  view, backups, and revert.
- **Bulk operations** — install or update OptiScaler and OptiPatcher across many detected games at once.
- **Clean uninstall** — every install writes a manifest, so removal restores what was there before.
- **Self-updating** — checks for new installer releases and shows the release notes in-app.

## Requirements

- Windows 10 or Windows 11 (x64)
- **.NET 10 Desktop Runtime** — the release build is framework-dependent, so this must be installed first.
  Download the **x64 Desktop Runtime** installer, not the SDK and not the ASP.NET runtime:
  [dotnet.microsoft.com/download/dotnet/10.0/runtime](https://dotnet.microsoft.com/download/dotnet/10.0/runtime?cid=getdotnetcore&os=windows&arch=x64)

## Install

1. Download the latest `OptiScalerInstaller-vX.Y.Z.W-win-x64.zip` from
   [Releases](https://github.com/Wimukthi/OptiScalerInstaller/releases).
2. Extract it anywhere. Keep the `Data` and `runtimes` folders next to the executable.
3. Run `OptiScalerInstaller.exe`.

Settings, logs, and caches are written next to the executable, so extract to a location you can write to —
not `C:\Program Files`.

### "Windows protected your PC"

The executable is not code-signed, so SmartScreen will warn on first launch. Choose **More info → Run
anyway**. If you would rather verify the download first, every release ships a `.sha256` file next to the
zip:

```powershell
Get-FileHash .\OptiScalerInstaller-vX.Y.Z.W-win-x64.zip -Algorithm SHA256
```

Compare the result against the published checksum. If it does not match, do not run it — download it again
from the Releases page, and only from there.

## Documentation

| Guide | What it covers |
| --- | --- |
| [Usage](docs/usage.md) | Every tab and dialog, and the workflows that run through them |
| [Configuration](docs/configuration.md) | `OptiScalerInstaller.settings.json` reference and the files the app writes |
| [Building](docs/building.md) | Building from source, the versioning scheme, and release packaging |
| [Troubleshooting](docs/troubleshooting.md) | Detection problems, install failures, and diagnostics |
| [Changelog](CHANGELOG.md) | Release history |

## Building

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet build OptiScalerInstaller.sln -c Release
```

See [docs/building.md](docs/building.md) for the test suite, the auto-incrementing version scheme, and how
release packages are produced.

## Warning

Do not use OptiScaler with online games. It injects a DLL into the game process, which anti-cheat systems
treat as tampering and may result in a ban. The app flags games with known anti-cheat signatures and warns
before installing, but that detection is best-effort — treat it as a hint, not a guarantee.

## Contributing

Bug reports and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for the build, test, and
review workflow. To report a security issue, see [SECURITY.md](SECURITY.md).

## License

[GNU General Public License v3.0](LICENSE).

## Credits

- [OptiScaler](https://github.com/optiscaler/OptiScaler) and its
  [compatibility list](https://github.com/optiscaler/OptiScaler/wiki/Compatibility-List) — the project this
  installer exists to serve.
- [OptiPatcher](https://github.com/optiscaler/OptiPatcher) — the DLSS-FG enabling plugin.
- Support the installer's development via [GitHub Sponsors](https://github.com/sponsors/Wimukthi).
