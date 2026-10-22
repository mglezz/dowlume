# Platform support

[English](#) · [Español](es/PLATFORM_SUPPORT.md)

I keep [0.3.0](RELEASE_CANDIDATE_0.3.0.md) as the published stable release until I accept and publish the 0.4.0 packages. The current 0.4.0 source enables the macOS workflow described below; older 0.3.0 macOS/Linux binaries remain restricted previews. See [0.4.0 notes](RELEASE_NOTES_0.4.0.md) and [acceptance](RELEASE_CANDIDATE_0.4.0.md).

| Target | Current implementation | Distribution and remaining acceptance |
| --- | --- | --- |
| Windows x64 | Organization, monitoring, review, eligible Undo, recovery, Explorer, tray and login startup | Installer and portable ZIP; I repeat automated upgrade checks from hash-pinned 0.3.0 and require exact-package manual confirmation. Previous acceptance does not approve changed binaries. |
| macOS 15+ Apple Silicon | Complete supported workflow on local APFS within one volume; native dialogs/Finder, application/Dock menus, reopen/quit, optional login startup, both languages and preview-profile migration | Self-contained `.app` ZIP, ad-hoc signed. Native CI targets macOS 15; local development also runs on 26.6.2. Exact-package manual installation, permissions and actual-login checks remain separately recorded. |
| macOS Intel | Excluded | No x64 package or support claim. |
| Ubuntu 24.04 x64 | Restricted preview: folder preview, settings, history, both languages, XDG Downloads discovery and containing-folder reveal | Self-contained `.tar.gz`; X11 launch under Xvfb. Mutations, recovery, login startup and tray hiding remain disabled for the 0.5.0 milestone. Xvfb does not accept a real GNOME session, XWayland or native Wayland. |
| Other Linux targets | Excluded from the initial scope | Separate native evidence is required for other distributions, desktops and architectures. |

Native recycling remains disabled on every platform. I preserve the historical evidence and do not repeat retired capacity probes. I do not import profiles across operating systems.

## Functional priorities and deferred signing

I complete application workflows on Windows, macOS Apple Silicon and Linux before publisher signing and macOS notarization in final stabilization. Signing is not a prerequisite for functional 0.4.0/0.5.0. I retain ad-hoc signing on macOS, report its actual status and verify it before/after extraction. I never remove quarantine or disable Gatekeeper. I record native safety, functional acceptance and installation constraints separately.

## macOS scope

I support ordinary local APFS folders outside synchronization services. Source and destination must be on the same volume. I reject cross-volume transfers, symbolic/hard links, nonregular files, known cloud-provider paths/attributes, network/non-APFS storage and unavailable volumes before preparing a move. A folder not recognized as cloud-managed is not certified safe for synchronization. I use persistent native identity for path containment, exclusive no-overwrite rename and identity plus SHA-256 for recovery. Cooperative locks cannot exclude arbitrary POSIX writers; detected races retain files and require attention. See [implementation](MACOS_IMPLEMENTATION.md) and [contract](FILE_OPERATION_CONTRACT.md).

I store Windows data in `%LOCALAPPDATA%\Dowlume`, macOS data in `~/Library/Application Support/Dowlume`, and Linux preview data in the .NET local application-data location, normally `$XDG_DATA_HOME/Dowlume.Preview` or `~/.local/share/Dowlume.Preview`. The first macOS launch copies a closed `Dowlume.Preview` profile only if the new profile does not exist; I preserve the source. Custom `--data-directory` launches bypass migration and login registration.

Closing the macOS window exits after draining work. Native Hide and Dock reopen remain available while the application runs. Optional login startup uses a per-user LaunchAgent for an installed app, with an ownership marker and literal executable arguments. It applies at the next login and can be overridden by system settings. Windows keeps its tray and registry startup behavior. Linux has neither tray hiding nor startup registration.

## Evidence and packages

I run shared tests on Windows x64, macOS ARM64 and Ubuntu x64. Shared fixture movers are not native file-safety evidence. Separate macOS read/move tests run on disposable APFS images with both case policies, including full-volume and race cases. Windows-only tests explicitly skip elsewhere. Schema-1 fixtures preserve both languages, settings, tracked files and history.

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package
# On Ubuntu 24.04 x64:
python3 scripts/Build-PlatformPreview.py --runtime linux-x64 --output artifacts/linux-preview
```

I build on each native target with .NET SDK 10.0.401 and Python 3.13 in CI. macOS requires Xcode command-line tools for the C adapter, icon conversion, signing and archiving. Linux CI installs `xvfb`, `xauth`, `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libgl1`, `libegl1`, `libxrandr2`, `libxi6`, `libxcursor1`, `libxrender1`, `libxext6`, and `libxfixes3`. Interactive Linux reveal also needs `/usr/bin/xdg-open` (`xdg-utils`) and a desktop file manager.

The output directory must be new. Each archive includes runtime, language resources, offline guides, revision and build information. Reports include archive SHA-256, dirty-tree status and explicit manual/clean-machine acceptance fields. The extracted macOS binary exercises the native lifecycle, watcher, reopened journal and English/Spanish tabs in a new disposable profile. Only clean committed builds qualify as release candidates; automated workflow checks do not imply actual-login or clean-machine acceptance.
