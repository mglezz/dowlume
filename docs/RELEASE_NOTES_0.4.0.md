# Dowlume 0.4.0

[English](#) · [Español](es/RELEASE_NOTES_0.4.0.md)

I published the macOS Apple Silicon milestone as [v0.4.0](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) on 2026-10-23 after manual functional confirmation of rc.1 on macOS and Windows, followed by automated regression and package acceptance of the shutdown correction in rc.2. I promoted the corrected rc.2 application packages without rebuilding them. I record source, hashes, automated checks and manual confirmation in [0.4.0 acceptance](RELEASE_CANDIDATE_0.4.0.md).

## Changes

- I drain monitoring tasks before disposing their watcher, preventing an intermittent shutdown error.
- I enable native organization, monitoring, aging/review, Keep, Snooze, Move, eligible Undo and journal recovery on macOS 15+ ARM64, for local APFS folders on the same volume.
- I use exclusive atomic rename, verified file/directory descriptors and persistent volume/file identity. I retain ambiguous files and show the observed location in Activity when available.
- I add native application/Dock menus, Command-comma for Settings, reopen handling and graceful Quit. Closing the window exits after draining work; there is no macOS tray mode.
- I add optional login startup for an installed `.app`, and copy a closed 0.3.0 preview profile into the new `Dowlume` profile without deleting the original.
- I package the native adapter, icon, English/Spanish resources and offline guides. I exercise the actual extracted macOS binary, including the full native workflow and both language tabs.
- I retain Windows functionality and test package upgrades from the published 0.3.0 installer. Linux stays a restricted preview for the 0.5.0 milestone.

I classify `.dmg` and `.pkg` as Installers in new profiles. I preserve migrated category rules; these extensions can be added in Settings if they were absent. Application bundles are directories and are not imported as downloads.

## Boundaries

I refuse cross-volume moves, hard links, symbolic links, network/non-APFS storage and known cloud-managed locations. Detection cannot certify every third-party sync provider; I support only ordinary local folders outside synchronization services. Concurrent POSIX writers do not have to honor cooperative locks. I verify after rename and retain files on a detected race; I do not promise exclusion of arbitrary writers or automatic rollback. See the [contract](FILE_OPERATION_CONTRACT.md).

Native recycling remains disabled. Intel Macs remain excluded. I defer Developer ID, notarization and Windows publisher signing to final stabilization; macOS has an ad-hoc signature. Installation may require the OS's explicit approval for an unsigned publisher. I never remove quarantine or disable Gatekeeper.

For installation, migration, backup and uninstall, see the [user guide](USER_GUIDE.md#macos-040).
