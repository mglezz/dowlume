# Windows release acceptance

Use this procedure for the exact installer and ZIP proposed for `v0.1.0`. Record their SHA-256 values, source commit from `BUILD-INFO.json`, Windows build, tester, date, and observed results. An unchecked item is not a pass. Do not use personal Downloads or change Recycle Bin settings on a daily-use computer.

## Prepare a clean environment

Use a fresh Windows 11 x64 VM or spare machine, a standard user account, and no separately installed .NET runtime. Record how the image was created. A development PC or a GitHub runner with the SDK installed does not satisfy this check.

Use a separate clean environment for this candidate. Do not modify, resume for testing, restore, or delete the existing evidence VM, exports, or checkpoints. No Recycle Bin policy changes or native capacity probes are part of v0.1.0 acceptance.

Use the [Hyper-V preparation guide](HYPER_V_ACCEPTANCE.md) and `scripts/New-WindowsAcceptanceVm.ps1` to create the guest, verify official Windows media, transfer the candidate, and establish a clean checkpoint. Windows setup and the interactive acceptance checks still require the guest desktop.

Download the proposed installer, portable ZIP, `BUILD-INFO.json`, and `SHA256SUMS.txt`. Compare each file with `Get-FileHash -Algorithm SHA256`. The initial candidate is unsigned; checksums identify the reviewed artifacts but do not replace trusting their source. Do not publish a package built with `DirtyWorkingTree: true`.

## Automated package smoke test

The acceptance kit contains `Test-WindowsPackage.ps1`, which runs on Windows PowerShell without installing .NET. From the directory containing the downloaded artifacts:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Test-WindowsPackage.ps1 -Installer .\Dowlume-0.1.0-win-x64-setup.exe -Archive .\Dowlume-0.1.0-win-x64.zip -ReportPath .\package-result.json -RequireNoSharedDotNet
```

The kit includes `Test-WindowsTray.ps1`; keep it beside the package script. The script refuses an existing registered Dowlume installation or startup entry. It creates a unique temporary installation, application profile, and sample file; checks portable launch/exit, install/reinstall, duplicate-instance refusal, running-app upgrade/uninstall refusal, native tray registration/reopening, startup-entry retention/removal, and profile/file retention after uninstall. Tray reopening uses Windows UI Automation and a native tray-click message; it does not replace testing the overflow menu with a person. Reports and fixtures remain in the printed temporary directory for inspection. It records detected runtimes but does not independently prove that the OS image was clean.

To test an upgrade from the earlier candidate, add `-PreviousInstaller <path-to-previous-installer>`. Without that argument, the upgrade check is a reinstall of the same package. Preserve both installer hashes in the report. If a check fails, inspect its retained logs before rerunning; the script intentionally refuses to overwrite a registered installation left by a failed test.

## First use with a person

After the smoke test, install normally with the wizard and a standard account. Ask someone unfamiliar with the implementation to perform these steps without developer assistance:

1. Open the Start menu shortcut and the included quick-start guide.
2. Create a disposable folder with copies of a document, image, archive, and an unfinished `.crdownload` file. Save that folder in Settings.
3. Refresh Preview. Confirm that files remain at their original locations, only completed files appear, and proposed destinations are understandable.
4. Select Organize and monitor, then cancel. Confirm that nothing moved. Repeat and confirm; verify the category folders and file contents.
5. Add another completed sample, wait for organization, then Pause. Confirm that a subsequent sample is left in place.
6. Use Hide to tray, reopen from the tray icon, and exit from the tray menu. Confirm the process has exited. Relaunch and verify that monitoring remains paused.
   Also minimize before hiding and verify both clicking the icon and choosing Open Dowlume restore a normal window. Check the notification-area overflow if the icon is not pinned.
7. In the disposable Windows account, enable Start Dowlume when I sign in to Windows. Sign out and back in; verify the app opens with monitoring paused. Disable the option and repeat sign-in. Enable it again before upgrading; confirm the upgrade retains it and uninstall removes only the entry for the installed copy. Do not test sign-in by signing out of the host's working account.
8. Find the sample in Activity, inspect its paths, and undo an eligible operation. Check that content is preserved and an occupied original path is never overwritten.
9. Close the app, upgrade/reinstall through the wizard, and reopen it. Confirm the saved configuration and history remain. Close and uninstall; confirm the sample files and profile remain.
10. With a fictional file in Review, verify the recycling explanation is readable and Recycle remains disabled with mouse and keyboard. Verify Keep, Snooze, Move, Show in Explorer, and eligible Undo still work and preserve content. Check the minimum window size and Windows display scaling.

Record unclear wording, errors, unexpected windows, display scaling, and keyboard navigation problems. Attach only redacted screenshots; the journal and profile contain private paths.

## Native file-safety acceptance

- **Cross-volume move and interruption:** on two separate local volumes, use the harness below. Record the physical disk/volume arrangement. Verify a completed move and undo, then terminate only the isolated worker during a copy. Recovery must preserve the intact source and partial destination, mark NeedsAttention, and block another move until resolved. A source-only or fully completed copy is an inconclusive interruption test.
- **Disabled recycling:** run the automated regression suite on the candidate source. It checks direct adapter/organizer calls, forced desktop command execution, repeated requests, and old journals without invoking the Windows bin. The old capacity harness is retired and must not be run; its failed results remain in the [investigation](RECYCLE_CAPACITY_INVESTIGATION.md).

Developer cross-volume harness (requires the repository SDK; it is not part of clean-machine testing):

```powershell
dotnet run --project scripts/FileOperationAcceptance -c Release -- verify E:\Dowlume.Acceptance artifacts/acceptance/cross-volume.json
```

Choose a writable folder on a second fixed local volume. The harness writes only uniquely named fixture directories, moves a 64 MiB sample and undoes it, then interrupts a separate 512 MiB copy. It retains both fixture directories and reports their paths. It does not simulate power loss or modify Recycle Bin settings.

## Publication handoff

Update the release checklist with evidence tied to the artifact hashes. Resolve failures, obtain independent acceptance review, and repeat affected checks after changes. Obtain the owner's explicit publication authorization before creating the final tag or publishing stable `v0.1.0`. New features and Linux/macOS work remain deferred.
