# Platform support

[English](#) · [Español](es/PLATFORM_SUPPORT.md)

I support Windows x64 in the published 0.2.0 release. I plan common foundations in 0.3.0, macOS support in 0.4.0, and Linux support in 0.5.0, followed by stabilization toward 1.0. These are planned outcomes; I have not released or accepted macOS/Linux packages. The [roadmap](../ROADMAP.md#planned-releases-toward-10) defines the exit criteria.

## Support matrix and scope

| Target | Current status | Planned acceptance scope |
| --- | --- | --- |
| Windows x64 | Published and accepted in [0.2.0](RELEASE_CANDIDATE_0.2.0.md) | Retain functionality, profile compatibility, and native file-safety checks through every shared-code change. |
| macOS Apple Silicon | Planned; no product support claim | Primary 0.4.0 target. In 0.3.0 I name supported macOS versions and filesystems against the pinned dependencies and native test access. |
| macOS Intel | Out of scope | No x64 implementation, CI job, or package is planned. A later release may revisit this with its own acceptance. |
| Linux x64 | Planned; no product support claim | Initial 0.5.0 scope: one named Ubuntu LTS release and GNOME. In 0.3.0 I pin the distribution version, package format, filesystem, native libraries, and X11/Wayland backend/session coverage. |
| Other Linux distributions, desktops, and ARM64 | Outside the initial stable scope | Expand only with maintenance capacity, native packages, and recorded acceptance. |

I distinguish four evidence levels: compilation, shared automated tests, experimental desktop/package testing, and supported stable release. A green CI job or a new runtime identifier does not establish the final level. I record exact OS/build, architecture, filesystem and case behavior, desktop/backend, package hashes, and test results for each supported combination.

[Avalonia's support matrix](https://docs.avaloniaui.net/docs/supported-platforms) varies by OS version and architecture, and its Linux backends have distinct requirements. I use that upstream matrix as an input, not as proof that Dowlume's file operations work there. I confirm the matrix for the package versions actually pinned in the repository before fixing minimum OS requirements.

## Findings from the 0.2.0 source review

| Area | Current implementation | Required foundation |
| --- | --- | --- |
| Composition and capabilities | `App.axaml.cs` constructs Windows startup integration; UI properties and messages refer to Windows. | Select native adapters centrally and expose truthful capabilities. Avoid adding OS checks throughout view models or domain rules. |
| File manager | [DesktopDialogs](../Dowlume/Dowlume.App/Services/DesktopDialogs.cs) throws outside Windows and invokes Explorer on Windows. | Native reveal/open behavior with safe argument handling and clear unavailable/permission-denied results. |
| Downloads and profile directories | [DownloadsFolder](../Dowlume/Dowlume.Infrastructure/DownloadsFolder.cs) uses the Windows known folder and a simple home-directory fallback elsewhere. | Native/configured folder discovery, platform profile locations, and explicit permissions; do not assume every user has a literal `~/Downloads` directory. |
| Path identity | [PathSafety](../Dowlume/Dowlume.Infrastructure/PathSafety.cs) chooses case comparison solely from `OperatingSystem.IsWindows()`. | Review volume-specific case behavior, Unicode names, protected directories, links, hard links, mount boundaries, and collision identity before enabling production file actions on a port. |
| File mutation | [SafeFileMover](../Dowlume/Dowlume.Infrastructure/SafeFileMover.cs) has a Windows handle-based path and a non-Windows `File.Move` fallback; [DirectoryLease](../Dowlume/Dowlume.Infrastructure/DirectoryLease.cs) acquires native directory handles only on Windows. | Define and demonstrate equivalent no-overwrite, identity/race, metadata, and interrupted-copy guarantees per native implementation. An open managed stream alone is not accepted as proof of equivalent protection on every OS. |
| Monitoring and readiness | Watcher hints, reconciliation, and readiness checks exist. | Native tests for missed/duplicate events, active writers, suspend/resume, unavailable volumes, and restart; establish platform limits explicitly. |
| Desktop lifecycle | Window, tray, profile lock, shutdown, and login startup have Windows evidence. | Native menu/dock/tray behavior, keyboard conventions, reopen/quit, optional autostart, and a usable no-tray path. Validate permissions without requiring elevated launch. |
| Test reporting | Several Windows-specific tests return early outside Windows. | Separate native coverage and report unsupported cases explicitly so a passing aggregate cannot imply unexecuted native checks passed. |
| Persistence | Profiles and history contain local paths. | Preserve 0.2.0 profiles on their original OS, validate schema upgrades and backups, and treat cross-OS profile import as a separate design. |
| Distribution | [Build-Release.ps1](../scripts/Build-Release.ps1) creates Windows x64 ZIP/installer packages. | Native package layouts, dependencies, signing, source/hash provenance, installation, upgrade, and uninstall acceptance. |

These are review findings and planned work, not completed fixes. I keep Windows supported while addressing them in small PRs. I use disposable fixtures for all native investigations, and leave historical evidence and checkpoints unchanged.

## Distribution decisions and dependencies

For macOS I target a proper application bundle and signed/notarized public distribution, with Finder integration and documented permissions. I need access to the appropriate signing credentials and native acceptance environments before I can close that release gate. [Apple's notarization guidance](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution) informs the distribution work. I do not treat cross-compilation or an unsigned local launch as equivalent to an accepted public installer.

For Linux I start with one primary package format for the declared distribution and document native dependencies even when .NET is included. I choose the format in 0.3.0 after checking permissions, updates, and integration; I do not promise AppImage, Flatpak, Snap, and native packages simultaneously. X11, XWayland, and native Wayland are separate acceptance combinations, and tray availability cannot be assumed across desktops.

## File recovery and release acceptance

Native recycling remains disabled. I do not make the first macOS/Linux ports depend on implementing native Trash or on repeating the retired Windows capacity tests. A recoverable quarantine requires a separate design and acceptance; 1.0 must explicitly include an accepted implementation or document its exclusion.

Before declaring a platform supported, I verify the documented feature set in both languages on the exact packages: preview/cancel, organization and monitoring, Keep/Snooze/Move, eligible Undo, history, interrupted-operation recovery, restart, native dialogs, file-manager reveal, startup, and desktop lifecycle. I cover permissions, low disk space, source replacement, collisions, case/Unicode behavior, metadata, mount boundaries, and cross-volume interruptions using disposable fixtures. Network shares, links, and cloud placeholders remain unsupported unless separately designed and accepted.

I record automated and manual results separately, repeat affected Windows regression checks, and publish the exact accepted artifacts. I add support claims only after these checks pass, not when a milestone is scheduled.
