# Linux 0.5.0 acceptance

Status: **rc.1 manual testing confirmed by the maintainer on 2026-11-01; final package automation recorded separately**.
The [0.5.0 acceptance record](../releases/acceptance/0.5.0.md) identifies exact source revisions, hashes and CI evidence. The confirmation is global; separate desktop/backend, guest and per-step logs were not supplied. It does not establish a native Wayland rendering backend or additional clean-machine measurements.

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

## Manual checklist supplied for rc.1

Record distribution, kernel, desktop version, session type, actual rendering backend,
filesystem/mount options, UUID discovery, app version, commit, archive SHA-256,
result and diagnostics. Xvfb is only an automation aid. A Wayland session running
through XWayland must be labeled XWayland; it does not prove a native Wayland backend.

| Gate | Ubuntu GNOME X11 | Ubuntu GNOME Wayland |
| --- | --- | --- |
| Fresh `.deb` install without system .NET; dependencies resolve | Overall confirmation only | Overall confirmation only |
| Menu/icon, launch, folder picker, file-manager reveal, window close | Overall confirmation only | Overall confirmation only |
| Preview/confirmation/cancel, classify, monitor incomplete and completed downloads | Overall confirmation only | Overall confirmation only |
| Review, Keep, Snooze, Move, eligible Undo and language change/restart | Overall confirmation only | Overall confirmation only |
| XDG login opt-in, actual logout/login, disable, desktop override | Overall confirmation only | Overall confirmation only |
| Second instance, active-work close, permissions and unavailable mounts | Overall confirmation only | Overall confirmation only |
| Update while running is refused; closed update retains settings/history/files | Overall confirmation only | Overall confirmation only |
| `.deb` remove/reinstall, portable relocation/update/menu removal | Overall confirmation only | Overall confirmation only |
| Both languages: every tab/dialog/error, scaling and accented filenames | Overall confirmation only | Overall confirmation only |

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
