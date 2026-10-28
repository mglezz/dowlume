# Platform support

[English](#) · [Español](../es/user/platform-support.md)

[Version 0.4.0](../releases/acceptance/0.4.0.md) is the stable macOS Apple Silicon and Windows release within the scope below. Manual confirmation covered rc.1 on both platforms; native regression and final-package automation covered the rc.2 shutdown correction. Older 0.3.0 macOS/Linux binaries remain restricted previews, and published Linux 0.4.0 remains restricted. The 0.5.0 source enables Linux operations within the candidate scope below; its native CI and package automation passed, while final manual acceptance remains pending. See [0.4.0 notes](../releases/0.4.0.md).

| Target | Current implementation | Distribution and acceptance |
| --- | --- | --- |
| Windows x64 | Organization, monitoring, review, eligible Undo, recovery, Explorer, tray and login startup | Installer and portable ZIP; automated upgrade checks from hash-pinned 0.3.0 passed, with manual functional confirmation on rc.1 and automated regression/package acceptance of the rc.2 shutdown correction. |
| macOS 15+ Apple Silicon | Complete supported workflow on local APFS within one volume; native dialogs/Finder, application/Dock menus, reopen/quit, optional login startup, both languages and preview-profile migration | Self-contained `.app` ZIP, ad-hoc signed. Native CI targets macOS 15; local development also runs on 26.6.2. Manual functional confirmation and its evidence limits are recorded separately from package automation. |
| macOS Intel | Excluded | No x64 package or support claim. |
| Ubuntu 24.04 x64 (0.5.0 candidate) | Native same-mount local ext4 workflow, identity-based recovery, XDG Downloads and optional autostart, file-manager reveal; no tray | Self-contained `.deb` and `.tar.gz` packaging prepared. Native execution and the extracted portable workflow passed in CI; installation/update and real GNOME X11/Wayland acceptance are pending. See [Linux acceptance](../development/linux-acceptance.md). |
| Fedora / Arch x64 | Same portable glibc build and ext4 restrictions; no distribution allowlist | Unverified; no RPM/AUR package yet. Btrfs/XFS and other filesystems are rejected. Separate evidence is required before widening official support. |

Native recycling remains disabled on every platform. Historical evidence is retained; retired capacity probes must not run. Profiles cannot be imported across operating systems.

## Functional priorities and deferred signing

The roadmap prioritizes complete application workflows on Windows, macOS Apple Silicon, and Linux before publisher signing and macOS notarization in final stabilization. Signing is not a prerequisite for functional 0.4.0/0.5.0. macOS packages retain ad-hoc signing, with signature verification before and after extraction. Installation does not remove quarantine or disable Gatekeeper. Native safety, functional acceptance, and installation constraints have separate evidence.

## macOS scope

macOS support covers ordinary local APFS folders outside synchronization services. Source and destination must be on the same volume. The native adapter rejects cross-volume transfers, symbolic/hard links, nonregular files, known cloud-provider paths/attributes, network/non-APFS storage and unavailable volumes before preparing a move. A folder not recognized as cloud-managed is not certified safe for synchronization. It uses persistent native identity for path containment, exclusive no-overwrite rename and identity plus SHA-256 for recovery. Cooperative locks cannot exclude arbitrary POSIX writers; detected races retain files and require attention. See [implementation](../architecture/macos.md) and [contract](../architecture/file-operations.md).

Windows data resides in `%LOCALAPPDATA%\Dowlume`, macOS data in `~/Library/Application Support/Dowlume`, and Linux 0.5.0 data in `$XDG_DATA_HOME/Dowlume` or `~/.local/share/Dowlume`. The old Linux `Dowlume.Preview` profile remains untouched without automatic migration. The first macOS launch copies a closed `Dowlume.Preview` profile only if the new profile does not exist; the source is preserved. Custom `--data-directory` launches bypass migration and login registration.

Closing the macOS window exits after draining work. Native Hide and Dock reopen remain available while the application runs. Optional login startup uses a per-user LaunchAgent for an installed app, with an ownership marker and literal executable arguments. It applies at the next login and can be overridden by system settings. Windows keeps its tray and registry startup behavior. Linux closes after draining active work and has no tray. Optional startup uses `$XDG_CONFIG_HOME/autostart/com.mglezz.dowlume.desktop` (default `~/.config/autostart`). It starts monitoring paused. See [Linux implementation and limitations](../architecture/linux.md).

## Evidence and packages

CI runs shared tests on Windows x64, macOS ARM64 and Ubuntu x64. Shared fixture movers are not native file-safety evidence. Separate macOS read/move tests run on disposable APFS images with both case policies, including full-volume and race cases. Windows-only tests explicitly skip elsewhere. Schema-1 fixtures preserve both languages, settings, tracked files and history.

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package
# On Ubuntu 24.04 x64:
python3 scripts/Build-Linux.py --output artifacts/linux-candidate
python3 scripts/Test-Linux.py --output artifacts/linux-native --archive artifacts/linux-candidate/Dowlume-0.5.0-linux-x64.tar.gz
```

Packages are built on each native target with .NET SDK 10.0.401 and Python 3.13 in CI. macOS requires Xcode command-line tools for the C adapter, icon conversion, signing and archiving. Linux CI installs `xvfb`, `xauth`, `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libgl1`, `libegl1`, `libxrandr2`, `libxi6`, `libxcursor1`, `libxrender1`, `libxext6`, and `libxfixes3`. Interactive Linux reveal also needs `/usr/bin/xdg-open` (`xdg-utils`) and a desktop file manager.

The output directory must be new. Each archive includes runtime, language resources, offline guides, revision and build information. Reports include archive SHA-256, dirty-tree status and explicit manual/clean-machine acceptance fields. The extracted macOS binary exercises the native lifecycle, watcher, reopened journal and English/Spanish tabs in a new disposable profile. Only clean committed builds qualify as release candidates; automated workflow checks do not imply actual-login or clean-machine acceptance.
