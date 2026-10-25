# AMD FSR 4 support

Reviewed 2026-10-25. Driver support, OptiScaler support, and a game's integration are separate requirements.
This page describes vendor and upstream documentation; it does not certify a particular game or machine.

## AMD driver support

AMD's [FSR Technologies page](https://www.amd.com/en/products/graphics/technologies/fidelityfx/super-resolution.html)
lists **FSR Upscaling 4.1** on Radeon RX 6000 Series desktop graphics and higher: RX 6000 (RDNA 2),
RX 7000 (RDNA 3), and RX 9000 (RDNA 4). AMD's [Adrenalin 26.10.1 release notes](https://www.amd.com/en/resources/support-articles/release-notes/RN-RAD-WIN-26-10-1.html),
last updated 2026-10-20, add support for these products:

- Radeon RX 6000 Series graphics products (RDNA 2)
- Ryzen AI Max 300 Series and Ryzen AI Max 400 Series processors
- Ryzen AI 9 300 Series and Ryzen AI 9 400 Series processors

The FSR page also lists Ryzen AI Max and Ryzen AI 9 laptop processors and Ryzen Z2A/Z2 Extreme handhelds.
The 26.10.1 notes give more specific processor families above; AMD recommends OEM drivers when system-specific
laptop features matter and says Adrenalin does not support handheld gaming devices. These lists do not promise
support for every APU or other Ryzen processor.

This announcement concerns **driver FSR upscaling**. It does not promise frame generation (FG), ray
reconstruction (RR), OptiScaler compatibility, or support in a game's own settings. Those are distinct features
and integrations.

## OptiScaler versions and configuration

AMD driver support does not change the capabilities of the OptiScaler build installed in a game. In
particular, do not apply current-master INI advice to an older stable package:

- **OptiScaler 0.9.4:** its [release notes](https://github.com/optiscaler/OptiScaler/releases) describe FSR
  4.1.1 / FFX 2.3 SDK INT8 support auto-enabled for RDNA3 desktop. The release says `Fsr4ForceEnableInt8`
  forces the shipped SDK upscaler on unsupported GPUs; it does not affect the driver's `amdxcffx64.dll`.
  Setting `Fsr4Update=true` on unsupported GPUs may select the internal FSR3 fallback. The release also says
  older GPUs that lack INT8 support will not work.
- **RDNA2 in 0.9.4:** upstream recommended continuing to use FSR 4.0.2c because of a driver compiler
  regression causing issues such as ghosting. That warning is scoped to the release's then-current driver
  state; recheck current upstream guidance before choosing a legacy package.
- **Newer builds:** current upstream [release notes](https://github.com/optiscaler/OptiScaler/releases)
  describe a more detailed force-model path, including `Fsr4ForceModel=2`; current
  [master OptiScaler.ini](https://github.com/optiscaler/OptiScaler/blob/master/OptiScaler.ini) documents
  `Fsr4GpuSupport` and `Fsr4Model`. These are not stable 0.9.4 keys. Use only the options documented by the
  exact OptiScaler build and shipped INI you installed. Do not infer that a key from master is recognized by
  an older release.

The installer retains an optional expert legacy package workflow, exposed through **Show legacy/manual
package tools**. Before copying files, it checks the target INI's `[FSR]` section for keys incompatible with the
legacy package and refuses the operation if found. It does not automatically add newer master-only
`Fsr4ForceModel` keys. A manual INT8 package is not mandatory just because a GPU supports driver FSR 4.1.
In 0.9.4, `Fsr4ForceEnableInt8` applies to the SDK upscaler only, not the driver DLL. Follow the exact
OptiScaler build's shipped INI and release notes.

The installer's basic frame-generation choices use the installed INI schema. **Auto** leaves existing FG
settings unchanged. **Off** disables FG; **OptiFG** selects the upscaler input with FSR FG output; **Nukem**
selects the supported Nukem input/output names for that build and still requires native DLSS FG input plus
the matching component. Unsupported or incomplete schemas produce a warning rather than invented keys.
The v10 nightly's integrated Fakenvapi, component subfolder, and advanced FG modes still need separate
installer adaptation and game validation; see the [dated review](amd-fsr-review-2026-10-25.md).

## Driver notes and known issues

AMD's 26.10.1 notes list these relevant issues as of 2026-10-20:

- Cyberpunk 2077 may crash or time out on some RX 9000 systems with AMD FSR Upscaling, Radeon Anti-Lag, and
  in-game frame generation enabled during video playback on a secondary display. AMD recommends disabling
  in-game frame generation for this case.
- Some overlay open/close actions may crash or time out while AMD's Performance Metrics Overlay is enabled;
  AMD recommends temporarily disabling that overlay.
- Switching applications or toggling overlays may crash or hang games using in-game AMD FSR Frame Generation
  on RX 9000; AMD recommends enabling V-Sync.

See the [full AMD release notes](https://www.amd.com/en/resources/support-articles/release-notes/RN-RAD-WIN-26-10-1.html)
for updates and the complete issue list. These are driver notes, not OptiScaler-specific diagnoses.
