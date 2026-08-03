# Contributing

Bug reports, compatibility findings, and pull requests are all welcome.

## Reporting bugs

Open an issue at [github.com/Wimukthi/OptiScalerInstaller/issues](https://github.com/Wimukthi/OptiScalerInstaller/issues)
and include:

- The installer version from the title bar.
- The game, its platform, and its install path.
- What you expected versus what happened.
- A diagnostics bundle — **Settings → Export diagnostics** collects the log, system info, a detected-games
  snapshot, your settings, and the error log.

Check [docs/troubleshooting.md](docs/troubleshooting.md) first; it covers the common cases.

Problems with OptiScaler itself — crashes, upscaling quality, per-game quirks — belong in the
[OptiScaler](https://github.com/optiscaler/OptiScaler) repository, not here. This project only installs and
configures it.

## Development setup

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on Windows. Visual Studio 2022
or 2026 with the .NET desktop development workload is optional but makes the form designers usable.

```bash
dotnet build OptiScalerInstaller.sln -c Release
powershell -ExecutionPolicy Bypass -File ./scripts/TestCompatibilityParser.ps1 -Live
```

See [docs/building.md](docs/building.md) for the versioning scheme, the repository layout, and release
packaging.

## Pull requests

- Keep changes focused. One behavioural change per pull request is easier to review and to revert.
- Match the surrounding style: VB.NET with `Imports` at the top, `Friend`/`Public` as the existing files use
  them, and short comments explaining *why* rather than restating the code.
- Run the build and the compatibility parser tests before opening the PR. The live canary is what catches
  upstream wiki format changes, so run it with `-Live`.
- Add a fixture to `tests/CompatibilityParser.Tests` for any parser change.
- Update the docs when you change behaviour a user can see, and add a `CHANGELOG.md` entry under the next
  version.
- Don't commit build output. `bin/`, `obj/`, `artifacts/`, and `Release/` are ignored.

`BuildVersion.txt` increments on every local build, so it will show up as modified. Only commit it as part of
preparing a release; otherwise discard the change.

## Working on the UI

Forms are Windows Forms with generated `*.Designer.vb` files. Edit those through the designer where you can —
hand-editing generated layout code is easy to get wrong and hard to review.

Custom controls in `Themed*.vb` exist because the stock controls don't honour dark mode. New UI should use
them and go through `ThemeManager` rather than hard-coding colours.

## License

By contributing you agree that your contributions are licensed under the
[GNU General Public License v3.0](LICENSE).
