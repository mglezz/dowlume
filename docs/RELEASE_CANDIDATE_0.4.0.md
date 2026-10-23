# Dowlume 0.4.0 acceptance

[English](#) · [Español](es/RELEASE_CANDIDATE_0.4.0.md)

I published [0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) for macOS Apple Silicon and Windows on 2026-10-23 after accepting the exact candidate on both platforms. Signing remains deferred. I distinguish automated evidence from final manual functional confirmation below.

## Scope

I target macOS 15+ ARM64 and ordinary local APFS folders on the same volume. I include preview, organization, monitoring, review, Keep, Snooze, Move, eligible Undo, recovery, history, settings, English/Spanish, Finder, native menus and optional login startup. I refuse cross-volume/cloud/network/link operations; Linux remains a restricted preview and recycling remains disabled. See [notes](RELEASE_NOTES_0.4.0.md) and [native guarantees](FILE_OPERATION_CONTRACT.md).

## Automated evidence

I run the full Release suite, formatting, analyzer/dependency checks, a native matrix on two newly created APFS images, and extracted-package workflow checks. Reports identify the source, dirty state, operating system and archive hashes. I do not use dirty development packages as release candidates.

Local development on macOS 26.6.2 ARM64, SDK 10.0.401, passed the supported workflow and both APFS case policies, including a full 256 MiB disposable volume. I verify native race/collision preservation, xattrs/mode/timestamps, hard-link refusal, native readiness, permissions, watcher/drain, reopened-journal recovery and cross-volume refusal before journaling. CI supplies separate macOS 15, Windows and Linux evidence. I retain the historical releases and retired Recycle Bin probes unchanged.

The macOS package script signs ad hoc, extracts the ZIP, verifies it again, and exercises its actual executable with a newly required disposable profile. The workflow report covers all tabs in both languages, persisted language, native lifecycle and Undo, reopened database and watcher shutdown. It explicitly leaves manual installation, actual login and clean-machine acceptance unclaimed.

## Candidate delivery — 2026-10-23

I first published [v0.4.0-rc.1](https://github.com/mglezz/dowlume/releases/tag/v0.4.0-rc.1) as a prerelease. I build all packages from clean source [`e51f59356612a77c87a5ada69300b523264415a4`](https://github.com/mglezz/dowlume/commit/e51f59356612a77c87a5ada69300b523264415a4), after integrating [macOS implementation #33](https://github.com/mglezz/dowlume/pull/33) and [desktop shutdown correction #34](https://github.com/mglezz/dowlume/pull/34). Later documentation commits do not change these binaries. I subsequently promoted the accepted application packages to 0.4.0 without rebuilding them.

| Final-source check | Result |
| --- | --- |
| [Build, tests and formatting](https://github.com/mglezz/dowlume/actions/runs/39914334936) | macOS: 107 passed / 8 skipped; Windows: 86 / 29; Linux: 82 / 33. No failures. Platform-specific skips are expected. |
| Native APFS matrix, same run | 25/25 passed on each case policy; macOS 15.7.9 ARM64. |
| [Security checks](https://github.com/mglezz/dowlume/actions/runs/39914334926) | Passed. |
| [Native packages](https://github.com/mglezz/dowlume/actions/runs/39953171400) | Extracted macOS workflow and restricted Linux desktop launch passed. |
| [Windows package](https://github.com/mglezz/dowlume/actions/runs/39953167507) | Upgrade from the accepted 0.3.0 installer, languages, portable launch, running-app guards and uninstall passed. |

I rejected the earlier packaging attempt from `032df79`: a desktop test removed its temporary profile before asynchronous shutdown released SQLite. I corrected teardown to await the actual close event and prevented a late initial refresh from restarting the timer during shutdown. The final-source package run above passes with that correction.

I attach build information, acceptance reports, `Dowlume-0.4.0-validation.zip` and `SHA256SUMS.all.txt` to the release. The local extracted-archive report is `ACCEPTANCE.osx-arm64-local.json`; I keep its automated result separate from manual installation and actual login.

### Package SHA-256

| Archivo / File | SHA-256 |
| --- | --- |
| `Dowlume-0.4.0-osx-arm64.zip` | `27cb13f73fdd034164c5aafa5b428e957912658ce9abfb1ef903febb7300de71` |
| `Dowlume-0.4.0-win-x64-setup.exe` | `24e13f5773b5e07010bf31afdc6a9041635c9f89e999f4fd978857d33ea47732` |
| `Dowlume-0.4.0-win-x64.zip` | `f31c9a3cab722c1e630b4688426ae32b0d1710780cca15e711af03dd7b2e86f0` |
| `Dowlume-0.4.0-linux-x64-preview.tar.gz` | `1e21bdb6795c2d27549c51a9cb1c504821d5334d050b97959d31ff980a7a1d21` |

## Exact-package checklist

I requested final checks against the hashed candidate using the following checklist. I record the overall confirmation below; I do not invent separate measurements or evidence for every item:

- macOS: extract and install the full `.app`, review any Gatekeeper/permission prompt under the actual ad-hoc signing status, and verify ordinary-user access and denied access with disposable sample files.
- macOS: upgrade a closed 0.3.0 preview profile, preserving settings, language and history; verify restart, native folder selection/Finder, Command-comma, Hide/Dock reopen, single-instance refusal, confirmation cancellation and Quit while monitoring.
- macOS: enable login startup from the installed app, sign out/in, confirm launch with monitoring paused, then disable it and verify the owned registration is removed. Uninstall retains profile and managed files.
- Windows: repeat upgrade from 0.3.0, both languages after restart, organization/review/Undo, tray and closing on the new package. Hosted automation is recorded separately from manual confirmation.

I do not reuse the old 0.3.0 confirmation. On 2026-10-23 I confirmed that the new candidate worked correctly on both macOS and Windows and proceeded with stable publication.

## Stable acceptance and publication — 2026-10-23

After receiving the exact-candidate checklist, I confirmed successful testing on Mac and Windows and approved continuing publication. This is an overall functional confirmation, not a separate recorded result for every checklist item. I did not supply additional OS build numbers, clean-machine evidence, screenshots or per-step logs; I do not infer those details. The automated JSON reports retain their original `ManualDesktopAcceptance: false` fields because they describe automation, not this later confirmation.

I publish [v0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) from the same `e51f593` source and preserve the installer, application archives, acceptance kit, build information and automated evidence byte-for-byte. Only standalone release notes and the affected checksum manifests are finalized after acceptance. Offline documentation inside the accepted packages describes the pre-publication state; this record and the standalone notes provide the final status. I retain the prerelease as historical evidence and make 0.4.0 the latest stable release.
