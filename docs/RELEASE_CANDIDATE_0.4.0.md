# Dowlume 0.4.0 acceptance

[English](#) · [Español](es/RELEASE_CANDIDATE_0.4.0.md)

I implement 0.4.0 for macOS Apple Silicon and preserve Windows. I keep 0.3.0 as the latest stable release until I accept the new binaries. Signing is deferred; it is not a functional milestone gate. This record distinguishes implementation, automation and remaining exact-package checks.

## Scope

I target macOS 15+ ARM64 and ordinary local APFS folders on the same volume. I include preview, organization, monitoring, review, Keep, Snooze, Move, eligible Undo, recovery, history, settings, English/Spanish, Finder, native menus and optional login startup. I refuse cross-volume/cloud/network/link operations; Linux remains a restricted preview and recycling remains disabled. See [notes](RELEASE_NOTES_0.4.0.md) and [native guarantees](FILE_OPERATION_CONTRACT.md).

## Automated evidence

I run the full Release suite, formatting, analyzer/dependency checks, a native matrix on two newly created APFS images, and extracted-package workflow checks. Reports identify the source, dirty state, operating system and archive hashes. I do not use dirty development packages as release candidates.

Local development on macOS 26.6.2 ARM64, SDK 10.0.401, passed the supported workflow and both APFS case policies, including a full 256 MiB disposable volume. I verify native race/collision preservation, xattrs/mode/timestamps, hard-link refusal, native readiness, permissions, watcher/drain, reopened-journal recovery and cross-volume refusal before journaling. CI supplies separate macOS 15, Windows and Linux evidence. I retain the historical releases and retired Recycle Bin probes unchanged.

The macOS package script signs ad hoc, extracts the ZIP, verifies it again, and exercises its actual executable with a newly required disposable profile. The workflow report covers all tabs in both languages, persisted language, native lifecycle and Undo, reopened database and watcher shutdown. It explicitly leaves manual installation, actual login and clean-machine acceptance unclaimed.

## Exact-package checklist

Before stable publication I record the candidate archive hash and these results against that same archive:

- macOS: extract and install the full `.app`, review any Gatekeeper/permission prompt under the actual ad-hoc signing status, and verify ordinary-user access and denied access with disposable sample files.
- macOS: upgrade a closed 0.3.0 preview profile, preserving settings, language and history; verify restart, native folder selection/Finder, Command-comma, Hide/Dock reopen, single-instance refusal, confirmation cancellation and Quit while monitoring.
- macOS: enable login startup from the installed app, sign out/in, confirm launch with monitoring paused, then disable it and verify the owned registration is removed. Uninstall retains profile and managed files.
- Windows: repeat upgrade from 0.3.0, both languages after restart, organization/review/Undo, tray and closing on the new package. Hosted automation is recorded separately from manual confirmation.

I do not treat old 0.3.0 user confirmation as approval of 0.4.0 binaries. I publish a clearly labeled candidate while the exact-package checks remain pending, and promote only the accepted artifacts without rebuilding them.
