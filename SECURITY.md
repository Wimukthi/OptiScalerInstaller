# Security Policy

## Supported versions

Only the latest release receives fixes. Upgrade before reporting a problem — the installer's built-in update
check, or the [Releases](https://github.com/Wimukthi/OptiScalerInstaller/releases) page, will tell you if you
are behind.

## Reporting a vulnerability

Report privately through
[GitHub Security Advisories](https://github.com/Wimukthi/OptiScalerInstaller/security/advisories/new).
Please don't open a public issue for a vulnerability.

Include what you did, what happened, and the installer version. A proof of concept helps.

This is a solo-maintained project, so response times depend on availability. You will get an acknowledgement
as soon as it is seen.

## Scope

This app downloads archives over HTTPS, extracts them, and writes files into folders you select. The parts
worth scrutiny:

- **Archive extraction** — `ExtractArchive` in `InstallerService.vb` iterates entries itself and enforces
  `TryGetSafeExtractionPath` / `IsPathInsideRoot`, rejecting traversal and rooted entry paths.
- **Uninstall** — path removal is scoped to the install root and validated against the manifest, so a
  tampered manifest cannot delete outside the game folder.
- **Downloads** — release assets are verified against the digest published with the release where one
  exists, and the source URL, size, and SHA-256 are recorded in the install manifest.
- **Configurable URLs** — every remote endpoint is user-configurable. Pointing them at hosts you don't trust
  is outside the threat model.

## Known advisories

`SharpCompress` 0.38.0 carries [GHSA-6c8g-7p36-r338](https://github.com/advisories/GHSA-6c8g-7p36-r338), a
zip-slip in `IArchive.WriteToDirectory()`. This app never calls that method — extraction goes through the
guarded loop described above — so it is not exposed. No fixed version was available at the time of writing,
and the audit warning is suppressed in `OptiScalerInstaller.vbproj` with the assessment recorded alongside
it. If you believe that assessment is wrong, please report it.

## What this tool is not

OptiScaler works by getting a DLL loaded into a game process. Anti-cheat systems treat that as tampering.
The Anti-cheat column and install-time warnings are a best-effort signature scan, not a safety guarantee.
Using this with online games risks a ban, and that is a normal consequence rather than a vulnerability.
