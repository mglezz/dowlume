# Linux file operations in 0.5.0

Implementation candidate; automated native/package checks passed and final manual acceptance is pending. The intended first supported
configuration is Ubuntu 24.04 LTS x64, GNOME and ordinary local ext4. No distribution
name is hardcoded in the runtime adapter. Fedora and Arch can use the portable build
when they satisfy the same native requirements; this is not an acceptance claim.

## Native boundary

`LinuxFileSystem` uses glibc `openat`, `statx`, `ioctl`, `flock`, `fsync` and
`renameat2`. Every pathname component is opened relative to a held directory with
`O_NOFOLLOW`; reads also use `O_NONBLOCK` and require a regular file. This avoids
following ancestor symlinks and blocking on FIFOs. The supported ABI is Linux x64.

Moves require the full requested `statx` basic/birth/mount mask. The descriptor's
mount ID is matched in `/proc/self/mountinfo` and must report ext4. Filesystem UUIDs
come from a unique `/dev/disk/by-uuid` block-device mapping matching `stx_dev_major`
and `stx_dev_minor`. Missing/ambiguous UUIDs are rejected, never replaced with a
volatile device number. UUID + inode + birth timestamp is journaled as persistent
identity; SHA-256 is checked separately. Source and destination must have the same
mount ID and UUID, including newly created destination parents. Bind mounts with
different mount IDs are intentionally refused. Casefold directories anywhere in either path,
multiple hard links and nonregular files are refused. Other filesystems are not enabled.

`LinuxFileMove` prepares open handles and verifies the source before the SQLite
Pending record is committed. Execution rechecks parent paths, identity and content;
creates directories using `mkdirat`; syncs directory entries and file content; then
uses `renameat2(RENAME_NOREPLACE)`. There is no copy/delete, overwrite, or weaker
rename fallback. Destination collisions (including dangling links) cannot replace an
existing object. It syncs both parents and verifies identity, source absence, content
and destination binding after the move. Failures retain objects; there is no rollback
by pathname. The descriptor path from `/proc/self/fd` is diagnostic only.

## Concurrency and recovery

Advisory `flock` does not exclude noncooperating writers. Linux has no general
rename-by-descriptor primitive that conditionally renames a given inode. There is
still a final check-to-rename race with an external pathname replacement; subsequent
verification detects it, but cannot promise that an unrelated object never moved.
This adapter preserves files on detected races; it is not an isolation boundary
against malicious processes with write permission. Use ordinary user-owned folders,
outside concurrent synchronization. Cloud synchronization on ext4 cannot be reliably
detected by filesystem type. A post-verification write is outside the operation.

Recovery never replays a move. A matching destination with absent source completes
the journal; an intact matching source with absent destination records failure and
allows an explicit retry. Both paths, changed bytes, missing identity, mismatched
identity or unverifiable storage require attention. An old Linux journal without
identity cannot be accepted by hash alone. UUID-based identity survives a reboot;
actual crash/power-loss acceptance is still required. A cloned filesystem UUID is
outside the supported ordinary-local-volume assumptions.

Linux uses a new stable `Dowlume` profile, preserving the restricted `Dowlume.Preview`
profile without migration. Existing schema and operation records need no migration.
Windows and macOS adapters retain their own native behavior.

## Desktop and distribution

The existing monitor and organizer handle discovery, classification, review, Keep,
Snooze, Move and Undo. `LinuxFileReadiness` uses descriptor snapshots as a discovery
heuristic, not a mutation lease. No tray is provided; close drains work and exits.
XDG user directories suggest Downloads; `xdg-open` reveals the containing folder.
Per-user XDG Autostart is opt-in, uses escaped literal executable paths, refuses
unmanaged entries/links and observes desktop disable flags. Custom data-directory
sessions do not register startup. Monitoring always starts paused.

`Build-Linux.py` builds self-contained `.deb` and `.tar.gz` candidates on Ubuntu 24.04.
The `.deb` installs to `/opt/dowlume`, with a desktop entry and the existing app icon;
it refuses an upgrade/removal while that executable runs. Portable registration is
optional and independent of login startup. No installer modifies user data or enables
monitoring. Build metadata explicitly leaves acceptance and publication pending.

See [Linux acceptance](../development/linux-acceptance.md) for native execution and
release gates. API references: [statx](https://man7.org/linux/man-pages/man2/statx.2.html),
[renameat2](https://man7.org/linux/man-pages/man2/rename.2.html),
[XDG Autostart](https://specifications.freedesktop.org/autostart/latest/),
and [desktop Exec escaping](https://specifications.freedesktop.org/desktop-entry/latest/exec-variables.html).
