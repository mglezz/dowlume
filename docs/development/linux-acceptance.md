# Linux 0.5.0 acceptance

Status: **automated native and extracted-package checks passed; manual acceptance pending**.
The [0.5.0-rc.1 record](../releases/acceptance/0.5.0.md) identifies the exact source,
packages and CI evidence. No local Linux VM was used. Do not publish 0.5.0 as stable
until the matrix below has evidence for the exact final artifact hash.

Local development checks on macOS (2026-10-27): Release compilation passed; the
regression suite passed 114 tests with 25 platform/environment skips; four Python
packaging tests passed; formatting and repository/link checks passed. The first local
run used macOS's symlinked temporary directory and was discarded after native path
rejections; the successful run used a real `artifacts/test-tmp` directory, matching CI.
No Linux-native result is inferred from these checks.

## Prepared commands

On Ubuntu 24.04 LTS x64 with .NET SDK from `global.json`, Python 3.12+,
`e2fsprogs`, `util-linux`, `sudo`, `desktop-file-utils`, `xdg-utils`, `xvfb`, `xauth`
and the desktop libraries listed in `scripts/Build-Linux.py`:

```sh
dotnet build Dowlume.slnx -c Release
python3 scripts/Build-Linux.py --output artifacts/linux-candidate
python3 scripts/Test-Linux.py --output artifacts/linux-native --archive artifacts/linux-candidate/Dowlume-0.5.0-linux-x64.tar.gz
```

Run the test command as an ordinary user with sudo, never root. It creates a fresh
512 MiB image, formats only that new file, mounts it as ext4, and runs tests with a
disposable temporary directory there. A generated UUID alias is created only if udev
has not provided one; it is removed during cleanup. This synthetic mapping tests
native behavior, not UUID discovery on a clean real installation. It never formats a
physical disk. Reports and TRX files remain outside the fixture. If unmount fails,
inspect the reported temporary mount before removing it; do not force deletion.

The suite covers native identity/metadata, no-replace collisions, source and ancestor
replacement, content races, links/FIFOs, permissions, Unicode names, unsupported mounts,
full organizer lifecycle, monitoring, and reopened pending journals before/after rename.
The package smoke scenario covers the extracted executable, language persistence,
English/Spanish tabs, native lifecycle, watcher and graceful shutdown under Xvfb.
These are not actual-login, power-loss, installation, or real GNOME evidence.

## Final required matrix

Record distribution, kernel, desktop version, session type, actual rendering backend,
filesystem/mount options, UUID discovery, app version, commit, archive SHA-256,
result and diagnostics. Xvfb is only an automation aid. A Wayland session running
through XWayland must be labeled XWayland; it does not prove a native Wayland backend.

| Gate | Ubuntu GNOME X11 | Ubuntu GNOME Wayland |
| --- | --- | --- |
| Fresh `.deb` install without system .NET; dependencies resolve | Pending | Pending |
| Menu/icon, launch, folder picker, file-manager reveal, window close | Pending | Pending |
| Preview/confirmation/cancel, classify, monitor incomplete and completed downloads | Pending | Pending |
| Review, Keep, Snooze, Move, eligible Undo and language change/restart | Pending | Pending |
| XDG login opt-in, actual logout/login, disable, desktop override | Pending | Pending |
| Second instance, active-work close, permissions and unavailable mounts | Pending | Pending |
| Update while running is refused; closed update retains settings/history/files | Pending | Pending |
| `.deb` remove/reinstall, portable relocation/update/menu removal | Pending | Pending |
| Both languages: every tab/dialog/error, scaling and accented filenames | Pending | Pending |

Additional filesystem gates: real ext4 with normal udev UUID mapping; source/destination
on different ext4 mounts (including bind mounts); Btrfs/XFS/FUSE/NFS/overlay rejection;
casefold and unsupported `statx` fields; held advisory locks; no-space directory/rename
failures; forced process termination after Pending and after rename; restart and reboot
recovery; power-loss behavior on a disposable VM. Journal fixture tests do not replace
forced-termination/power-loss tests. Inspect retained files and hashes for every failure.
Never run destructive capacity/crash tests on personal storage.

No Linux 0.4.0 stable package exists. Preserve the old restricted preview profile and
verify that a new stable profile starts paused. For future updates, test an earlier
0.5.0 candidate and rollback with a backed-up profile. Do not invent a prior stable
Linux upgrade path. Keep Windows/macOS regression checks passing.

## Fedora and Arch

Use the same glibc x64 portable artifact, with native dependencies installed by the
distribution's package manager. There is no runtime distribution allowlist. This avoids
maintaining separate RPM/AUR installers initially. Test each on local ext4 first.
Btrfs (common on Fedora installations) is deliberately refused until a separate adapter
scope and evidence are accepted; merely running on Fedora does not enable Btrfs.
Maintaining distro-specific packages and validating more filesystems/desktops is the
additional cost, not a separate organizer implementation. Record these results separately
and widen official support only after they pass.
