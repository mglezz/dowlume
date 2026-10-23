# macOS implementation for 0.4.0

[English](#) · [Español](es/MACOS_IMPLEMENTATION.md)

I target macOS 15+ on Apple Silicon, with local APFS folders on a single volume. I implement the complete supported workflow: preview, organization, monitoring, aging/review, Keep, Snooze, Move, eligible Undo, history, settings, both languages and recovery. I retain Windows and leave Linux mutations disabled. Intel and native recycling are excluded.

## Native file operations

I open paths with Darwin `O_NOFOLLOW_ANY`, `O_CLOEXEC` and `O_NONBLOCK`, inspect the ARM64 `stat` structure and reject links, nonregular files, dataless placeholders and hard-linked mutation targets. I check APFS/local mount flags, known cloud paths and provider attributes. These checks cannot recognize every sync product; supported folders must be outside cloud/sync services.

I prepare a move before writing Pending: open the source and both existing parent directories, require the same APFS volume, verify SHA-256 and identity, and acquire a cooperative `flock`. Persistent identity combines volume UUID, inode and birth timestamp. I create missing destination components relative to held directory descriptors, then call `renameatx_np` with `RENAME_EXCL`. Existing destinations, including APFS case/Unicode aliases, are never replaced. I flush directories and verify resulting identity, content and parent paths. Same-volume rename retains the original object and its metadata; there is no copy/delete fallback.

I use a small ARM64 C adapter for the variadic `fcntl(F_GETPATH)` call. I compile it with the macOS SDK and include it in the application bundle. I record the opened object's observed path on a failed operation when available, so Activity can help locate files after directory replacement.

Cooperative locks do not exclude arbitrary POSIX writers. A replacement immediately before rename can move the replacement into the destination; verification detects the mismatch, preserves the objects and records NeedsAttention. I never delete or roll back files by an uncertain pathname. The supported model excludes hostile same-user mutation and does not promise a frozen namespace. The [file-operation contract](FILE_OPERATION_CONTRACT.md) describes this boundary explicitly.

Recovery never repeats the move. I complete an interrupted journal entry only when the source is absent and the destination has the recorded identity and content. An intact source with no destination becomes Failed/retryable. Missing identity or ambiguous files require attention; legacy records are not completed from matching bytes alone on macOS. SQLite schema 1 remains compatible through optional JSON fields.

## Desktop and migration

I use native folder dialogs and Finder reveal. The app menu includes Open and Settings (Command-comma), alongside the native Hide/Services/Quit commands; Dock reopen restores the window. Closing the window or quitting drains work, closes confirmations and releases the profile lock. I do not offer tray hiding on macOS.

For an installed `.app`, optional login startup writes a per-user LaunchAgent with literal arguments and an ownership marker. I do not replace or remove an unrelated job. It takes effect at the next login; macOS settings can disable it. Development/custom-profile launches cannot change the user's login configuration.

I store the profile in `~/Library/Application Support/Dowlume`. Before its first creation I copy a closed `Dowlume.Preview` profile through a private staging directory, verify the copied database, then rename the complete directory into place. I retain the original, refuse a live source or links, and never merge into an existing destination. I keep monitoring paused after startup.

## Reproducible checks

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume/Dowlume.slnx -c Release
python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-operations
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package
```

Output directories must be new. The native filesystem script creates two private 256 MiB sparse images, APFS and case-sensitive APFS, enables ownership and runs as an ordinary user. It covers reads, identity, links/FIFOs, aliases, replacements, writers, destination collisions, permissions, same-volume lifecycle/restart/recovery, monitoring, cross-volume refusal and an actually full disposable volume. It detaches only its own images, never forces unmount, and preserves a fixture if detach fails. These tests do not revisit retired Recycle Bin capacity probes.

I retain TRX results and `ACCEPTANCE.mac-file-reads.json`, including revision, dirty-tree status, OS, architecture and SDK. CI runs the same tests on macOS 15. Local development evidence on macOS 26.6.2 is additional evidence, not a substitute for the baseline runner.

Packaging verifies the ad-hoc signature before and after ZIP extraction. The extracted self-contained application uses a newly required disposable profile, switches through all tabs in English and Spanish, verifies persisted language, exercises native organization/review/Snooze/Keep/Move/Undo and a reopened journal, starts a real watcher, and drains it before closing. The package report binds these checks to the archive hash. It does not claim human desktop, actual login or clean-machine acceptance; I track those in the [candidate record](RELEASE_CANDIDATE_0.4.0.md).

I defer Developer ID and notarization to final cross-platform stabilization. I document the ad-hoc signature and installation limits; scripts neither remove quarantine nor disable Gatekeeper. Historical releases, evidence and retired capacity investigations remain unchanged.
