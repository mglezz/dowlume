# Platform support

[English](#) · [Español](es/PLATFORM_SUPPORT.md)

I support Windows x64 in the published [0.3.0 release](RELEASE_CANDIDATE_0.3.0.md). The 0.3.0 source contains platform adapters, native CI, and experimental packages. I do not claim stable macOS/Linux support: their file operations remain disabled. See the [roadmap](../ROADMAP.md) and [0.3.0 notes](RELEASE_NOTES_0.3.0.md).

## Matrix

| Target | 0.3.0 scope | Remaining release gate |
| --- | --- | --- |
| Windows x64 | Existing file operations, profile location, Explorer, tray and login startup; installer and portable ZIP | 0.3.0 automation and overall functional confirmation completed; manual environment and new clean-machine evidence limits remain documented in the acceptance record. Repeat affected checks for future changes. |
| macOS 15, Apple Silicon | Native ARM64 CI; self-contained `.app` in ZIP; folder preview, settings, both languages, history, Finder reveal; separate preview profile | 0.4.0: native file safety, APFS case-sensitive/insensitive and Unicode behavior, permissions, desktop lifecycle, signed/notarized distribution, installation and upgrade acceptance. Newer macOS versions require their own evidence. |
| macOS Intel | Excluded | No x64 implementation, CI job or package. I may reconsider it in a later release with separate acceptance. |
| Ubuntu 24.04 x64 | Native CI; self-contained `.tar.gz`; preview capabilities; XDG Downloads discovery; containing-folder reveal; X11 launch under Xvfb | 0.5.0: ext4 file safety, GNOME desktop acceptance, permissions, autostart, package integration and upgrade; XWayland and native Wayland require separate testing. |
| Other Linux targets | Excluded from the initial scope | Separate evidence for other distributions, desktops and architectures. |

My macOS/Linux archives are development previews, not accepted installers. The macOS bundle has an ad-hoc signature, without Developer ID or notarization. I verify its signature before and after extracting the ZIP, then launch that extracted application with a disposable profile. The Linux launch uses Xvfb, which does not validate a real GNOME session, dialogs, file-manager interactions or accessibility. Both reports explicitly leave manual desktop and clean-machine acceptance false. I do not ask users to disable system security.

The upstream [Avalonia matrix](https://docs.avaloniaui.net/docs/supported-platforms) and [Apple distribution requirements](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution) inform future support; they do not prove Dowlume's native safety. I keep the narrower matrix above until I have direct evidence.

## Implemented foundation

I select startup, file-manager, profile and tray capabilities in `DesktopPlatform`. Windows keeps `%LOCALAPPDATA%\Dowlume`; macOS uses `~/Library/Application Support/Dowlume.Preview`; Linux uses the .NET local application-data location, normally `$XDG_DATA_HOME/Dowlume.Preview` or `~/.local/share/Dowlume.Preview`. The previews never import Windows profiles automatically. Stored paths are specific to their original OS.

I read Linux `user-dirs.dirs` as bounded data and expand only its supported `$HOME` form; I never source it as a shell script. Windows retains its known-folder API; macOS currently suggests `~/Downloads`, with explicit folder selection available. Native file-manager commands receive literal argument lists, without a shell. Linux opens the containing folder because portable file selection is unavailable.

I expose no tray-hiding or login-startup action on the previews. If Windows tray creation fails, I retain a visible window and record the error. Closing the window drains work and releases the profile lock.

`IFileMover` separates orchestration from mutation. `SafeFileMover` rejects non-Windows moves before creating directories. The organizer also rejects organization, monitoring reconciliation, review decisions, Undo and recovery before journaling or modifying files. Preview never replays pending operations on these hosts. Preferences and the isolated SQLite profile remain writable. The [file-operation contract](FILE_OPERATION_CONTRACT.md) records the native work still required.

## macOS work toward 0.4.0

I have started native read and identity validation on Apple Silicon, including APFS case-sensitive/insensitive fixtures. The current source uses these reads for preview hashes; published 0.3.0 binaries remain unchanged. File mutations remain disabled. I record the scope and pending distribution requirements in [macOS implementation](MACOS_IMPLEMENTATION.md).

## Evidence and test interpretation

I run shared tests on native Windows x64, macOS ARM64 and Ubuntu x64 runners. Five Windows-only tests explicitly report skipped on other systems; the Linux case-distinct watcher regression explicitly reports skipped elsewhere. Shared orchestration tests use a disposable, test-assembly-only mover outside Windows. That mover is not shipped and is not evidence of native race protection. Production refusal has separate tests. Fixed schema-1 fixtures cover 0.2.0 English/Spanish settings, custom rules, history and closed-database backups.

Each preview includes source revision, dirty-tree status, runtime, build environment, checksums and an archive launch report. Only clean, committed builds are release candidates. Native CI artifacts retain their SDK/environment and TRX reports. I repeat affected Windows package checks against the hash-pinned 0.2.0 installer. No prior manual acceptance approves changed binaries.

## Build and system dependencies

I build on the native target with the repository's .NET 10 SDK and Python 3.13:

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-preview
# On Ubuntu 24.04 x64 instead:
python3 scripts/Build-PlatformPreview.py --runtime linux-x64 --output artifacts/linux-preview
```

The output directory must not exist. The script packages an isolated staging directory, extracts the archive and checks native launch/clean exit. macOS needs Xcode command-line tools for `codesign` and `ditto`. Linux CI installs `xvfb`, `xauth`, `libx11-6`, `libice6`, `libsm6`, `libfontconfig1`, `libgl1`, `libegl1`, `libxrandr2`, `libxi6`, `libxcursor1`, `libxrender1`, `libxext6`, and `libxfixes3`. Interactive reveal also requires `/usr/bin/xdg-open` (`xdg-utils`) and a functioning desktop file manager. Including .NET does not remove native library requirements.

## Gates for the ports

Before enabling file operations, I validate identity by volume, Unicode/case collisions, no-overwrite moves, writers/replacements, links/mounts, metadata, denied permissions, low disk space and interrupted cross-volume recovery. Before stable distribution I accept both languages, dialogs, file-manager behavior, restart, shutdown, single-instance refusal, optional login startup, installation, upgrade and uninstall on the exact packages. I start Linux desktop acceptance with Ubuntu 24.04 GNOME; native Wayland is not accepted by an X11 smoke test.

Native recycling remains disabled. I preserve historical evidence and checkpoints, and do not repeat retired capacity probes. Network shares, cloud placeholders, links and cross-OS profile import remain unsupported.
