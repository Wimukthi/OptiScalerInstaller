# AMD/OptiScaler installer review - 2026-10-25

The checkout was clean and matched origin/main at `cf82e625` before this work. Development and validation
use `C:\AxiomBuild\OptiScalerInstaller` on branch `fix/amd-fsr41-help`.

## Findings and changes

| Finding | Change |
| --- | --- |
| RDNA 2 guidance prescribed a separate legacy INT8 package. | The FSR 4 Support page explains official RX 6000/7000/9000 upscaling support and makes manual tools an explicit opt-in. |
| A GPU generation was treated as sufficient advice for APUs. | Desktop and mobile/APU guidance are separate; named Ryzen AI families and OEM driver requirements are documented. |
| Legacy controls could add obsolete keys to a newer INI or change the same key in another section. | Preflight checks the installed `[FSR]` schema before copying, rejects ambiguous duplicates, and preserves section-aware restoration and old manifests. |
| Vulkan driver FSR files were not recognized. | Added `amdvkffx64.dll` to package detection and version candidates. This detects a file, not a working renderer. |
| The editor described an older `Fsr4Model` preset mapping and used `[Main]` for fallback FSR definitions. | Added version-qualified legacy/current reference keys, corrected model versus preset mappings, and corrected fallback sections. |
| Frame-generation options wrote and verified the obsolete `FGType` key even for 0.9.4. | Use the installed schema's frame-generation input/output keys; preserve custom configuration when Auto is selected. |
| Help About/Updates silently depended on Settings-tab buttons being selectable. | Menu and button handlers call shared actions directly; About opens offline; manual update checks guard against duplicates. |
| Documentation depended on remote URLs. | Bundled four guides with a topic selector, F1 access, AMD FSR help, and an online option. Release packages include the guides. |

## Sources checked

- [AMD FSR technologies](https://www.amd.com/en/products/graphics/technologies/fidelityfx/super-resolution.html):
  RX 6000/7000/9000 ML upscaling support; other Redstone features have different requirements.
- [Adrenalin 26.10.1 notes, 2026-10-20](https://www.amd.com/en/resources/support-articles/release-notes/RN-RAD-WIN-26-10-1.html):
  RX 6000 and named Ryzen AI expansion, plus relevant frame-generation/overlay issues.
- [OptiScaler releases](https://github.com/optiscaler/OptiScaler/releases): stable remains 0.9.4 at review time.
- [Stable 0.9.4 INI](https://github.com/optiscaler/OptiScaler/blob/v0.9.4/OptiScaler.ini):
  `Fsr4Update` and `Fsr4ForceEnableInt8` controls.
- [Current upstream INI](https://github.com/optiscaler/OptiScaler/blob/master/OptiScaler.ini):
  `Fsr4GpuSupport`, `Fsr4Model`, and the driver Vulkan models. These are not assumed to work in older builds.

## Practical limits and next work

The installer does not infer an Adrenalin marketing version from Windows driver numbers, force a model from
GPU name alone, or change system drivers. Users should check the installed driver and the active in-game
OptiScaler backend. Game rendering, frame pacing and image quality still require game-specific validation.
The retained legacy/manual workflow needs a package and INI that match the user's OptiScaler build.

The stable feed remains the official latest-release endpoint. Newer builds can be selected through the
existing alternate source or local archive options; nightly-specific keys are not silently applied to stable.

The WIP v10 nightly also moves bundled components into an `OptiScaler/` subfolder and integrates Fakenvapi.
Generic copying preserves the folder tree, but the installer's root-oriented add-on detection/copy paths
and external Fakenvapi options need further adaptation and end-to-end validation before claiming full v10
support. New FG modes such as DLSSG output/Enabler combinations also need dedicated controls. The current
change fixes schema-aware mappings for the existing basic choices; it does not implement every v10 mode.
