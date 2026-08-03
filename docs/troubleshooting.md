# Troubleshooting

## Where to look first

The log pane at the bottom of the window records every action, including why something was skipped. Most
problems are explained there.

Unhandled and handled-but-notable exceptions go to `Errors/Error_Log.txt` next to the executable, with a
source label, type, message, and stack trace. Entries there are not necessarily failures — the drive scanner
logs every folder it was denied access to and carries on.

**Settings → Export diagnostics** bundles the log, system info, a detected-games snapshot, your settings, and
the error log into a single zip. Attach that to a bug report.

## The app won't start

- **Nothing happens, or a startup error dialog appears.** Install the
  [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0). The release build is
  framework-dependent. The dialog names the source and the log path.
- **It starts but forgets settings.** It writes next to the executable. Extracted into `C:\Program Files`,
  those writes fail. Move it to a normal folder.

## Games aren't detected

- **Run a scan.** Launcher detection alone will not find everything. **Scan installed games** adds a
  drive scan on the drives you choose.
- **The game isn't on the list.** The table only shows games the OptiScaler community has tested. OptiScaler
  works with many titles that never got a list entry — use the Install tab directly with the game folder.
- **The launcher installed it somewhere unusual.** Uninstall-registry detection covers launchers without
  their own manifests (Battle.net-style installs), but the drive scan is the reliable fallback.
- **Still nothing.** **Add game manually**, browse to the executable, and the app force-matches it against
  the list and remembers it. If the folder name matches more than one entry, a picker asks which one.

## Detected, but the install goes to the wrong folder

For Unreal Engine titles, OptiScaler belongs next to the shipping executable in `Binaries\Win64` or
`Binaries\WinGDK`, not in the game root. The app retargets automatically when it recognises the layout, and
logs `Profile note: For Unreal builds, target the Binaries\Win64 folder used by the game executable.`

Games with separate DX11 and DX12 binaries (`x64_dx12`, `x64_dx11`, nested `bin`/`binaries` folders) are
probed for the right one. If it still picks wrong, set **Game folder** on the Install tab yourself.

## "No executable was found for this detected game"

Quick install needs to resolve an actual executable inside the detected folder. This appears when the folder
was found but nothing executable is in the expected places — a partially uninstalled game, or a leftover
folder skeleton. Use **Add game manually** and point at the real executable, or set the folder on the Install
tab directly.

## OptiScaler shows as not installed after installing

Detection requires strong markers — the install manifest, or OptiScaler metadata in the hook DLL. A stock
`dbghelp.dll` that happens to share a name is deliberately not treated as an install, to avoid false
positives.

Check that the install actually landed where the game loads DLLs from (see the Unreal note above), and that
the post-install verification in the log passed.

## The compatibility list is empty or stale

- **Refresh lists** re-fetches it. Failures are logged with the reason.
- A refresh that returns suspiciously few entries is rejected and the cached list is kept, so a bad upstream
  response cannot wipe your list.
- With no network at all, the bundled `Data/Compatibility-List.md` is used. It ships with the release and is
  older than the live wiki.
- If the wiki changes format, parsing may degrade before a fix ships. `scripts/TestCompatibilityParser.ps1
  -Live` is the canary for exactly that.

## The game crashes or the overlay doesn't appear

That is OptiScaler's domain rather than the installer's, but the usual first checks:

- Press <kbd>Insert</kbd> in-game to open the OptiScaler overlay. If it closes immediately, try
  <kbd>Alt</kbd>+<kbd>Insert</kbd>.
- Try a different **Hook Filename**. `dxgi.dll` suits most games; the compatibility list notes the
  exceptions.
- On AMD/Intel with DLSS inputs still missing, add [Fakenvapi](https://github.com/optiscaler/OptiScaler/wiki/Fakenvapi)
  from the Add-ons tab.
- Check the game's entry on the [OptiScaler wiki](https://github.com/optiscaler/OptiScaler/wiki) —
  **Open wiki page** goes straight there.

Uninstall restores whatever the manifest backed up, so reverting is safe.

## Anti-cheat

Do not use OptiScaler with online games. The Anti-cheat column is a best-effort scan for known signatures
(Easy Anti-Cheat including the `start_protected_game` bootstrapper, BattlEye, Riot Vanguard, PunkBuster).
A clean result is not a guarantee that modding the game is safe.

## Reporting a bug

Open an issue at [github.com/Wimukthi/OptiScalerInstaller/issues](https://github.com/Wimukthi/OptiScalerInstaller/issues)
with the diagnostics zip, the installer version from the title bar, the game and its platform, and what you
expected to happen.
