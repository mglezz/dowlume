# Dowlume 0.4.0

[English](#) · [Español](es/RELEASE_NOTES_0.4.0.md)

I implement the macOS Apple Silicon milestone. Version 0.3.0 remains the published stable release until I accept and publish the new packages. I record the exact candidate and outstanding manual checks in [0.4.0 acceptance](RELEASE_CANDIDATE_0.4.0.md).

## Changes

- I enable native organization, monitoring, aging/review, Keep, Snooze, Move, eligible Undo and journal recovery on macOS 15+ ARM64, for local APFS folders on the same volume.
- I use exclusive atomic rename, verified file/directory descriptors and persistent volume/file identity. I retain ambiguous files and show the observed location in Activity when available.
- I add native application/Dock menus, Command-comma for Settings, reopen handling and graceful Quit. Closing the window exits after draining work; there is no macOS tray mode.
- I add optional login startup for an installed `.app`, and copy a closed 0.3.0 preview profile into the new `Dowlume` profile without deleting the original.
- I package the native adapter, icon, English/Spanish resources and offline guides. I exercise the actual extracted macOS binary, including the full native workflow and both language tabs.
- I retain Windows functionality and test package upgrades from the published 0.3.0 installer. Linux stays a restricted preview for the 0.5.0 milestone.

## Boundaries

I refuse cross-volume moves, hard links, symbolic links, network/non-APFS storage and known cloud-managed locations. Detection cannot certify every third-party sync provider; I support only ordinary local folders outside synchronization services. Concurrent POSIX writers do not have to honor cooperative locks. I verify after rename and retain files on a detected race; I do not promise exclusion of arbitrary writers or automatic rollback. See the [contract](FILE_OPERATION_CONTRACT.md).

Native recycling remains disabled. Intel Macs remain excluded. I defer Developer ID, notarization and Windows publisher signing to final stabilization; macOS has an ad-hoc signature. Installation may require the OS's explicit approval for an unsigned publisher. I never remove quarantine or disable Gatekeeper.

For installation, migration, backup and uninstall, see the [user guide](USER_GUIDE.md#macos-040).
