# Windows release acceptance

[English](#) · [Español](../es/development/windows-acceptance.md)

Apply this procedure to each new Windows candidate. The [0.4.0 acceptance record](../releases/acceptance/0.4.0.md) records the results for that release; earlier evidence does not approve changed binaries. Record the package SHA-256 values, source commit from `BUILD-INFO.json`, Windows build, tester, date, and observed results. An unchecked item is not a pass. Use disposable samples in a separate acceptance environment.

The commands below use 0.4.0 as the candidate and the published 0.3.0 installer as the upgrade baseline. Update both deliberately for a future release.

## Prepare a clean environment

Use a fresh Windows 11 x64 VM or spare machine, a standard user account, and no separately installed .NET runtime. Record how the image was created. A development PC or a GitHub runner with the SDK installed does not satisfy this check.

Use a separate clean environment for this candidate. Do not modify, resume for testing, restore, or delete the existing evidence VM, exports, or checkpoints. I do not change Recycle Bin policies or run native capacity probes during this acceptance.

Use the [Hyper-V preparation guide](hyper-v-acceptance.md) and `scripts/New-WindowsAcceptanceVm.ps1` to create the guest, verify official Windows media, transfer the candidate, and establish a clean checkpoint. Windows setup and the interactive acceptance checks still require the guest desktop.

Download the proposed installer, portable ZIP, `BUILD-INFO.json`, and `SHA256SUMS.txt`. Compare each file with `Get-FileHash -Algorithm SHA256`. The initial candidate is unsigned; checksums identify the reviewed artifacts but do not replace trusting their source. Do not publish a package built with `DirtyWorkingTree: true`.

## Automated package smoke test

The acceptance kit contains `Test-WindowsPackage.ps1`, which runs on Windows PowerShell without installing .NET. From the directory containing the downloaded artifacts:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Test-WindowsPackage.ps1 -Installer .\Dowlume-0.4.0-win-x64-setup.exe -Archive .\Dowlume-0.4.0-win-x64.zip -PreviousInstaller .\Dowlume-0.3.0-win-x64-setup.exe -ReportPath .\package-result.json -RequireNoSharedDotNet -RequireLocalization
```

The kit includes `Test-WindowsTray.ps1` and `Test-WindowsLocalization.ps1`; keep both beside the package script. With `-RequireLocalization`, I verify both guides and Spanish resources, switch English → Español → English in the installed app, and restart after each switch to check persistence. The script refuses an existing registered Dowlume installation or startup entry. It creates a unique temporary installation, application profile, and sample file; checks portable launch/exit, install/reinstall, duplicate-instance refusal, running-app upgrade/uninstall refusal, native tray registration/reopening, startup-entry retention/removal, and profile/file retention after uninstall. Tray reopening uses Windows UI Automation and a native tray-click message; it does not replace testing the overflow menu with a person. Reports and fixtures remain in the printed temporary directory for inspection. It records detected runtimes but does not independently prove that the OS image was clean.

I use the published 0.3.0 installer as the upgrade baseline. To choose another baseline, use `-PreviousInstaller <path-to-previous-installer>`. Without that argument, the upgrade check is a reinstall of the same package. Preserve both installer hashes in the report. If a check fails, inspect its retained logs before rerunning; the script intentionally refuses to overwrite a registered installation left by a failed test.

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

## Language and upgrade acceptance for 0.3.0

I record the exact installer and ZIP hashes and the observed results for these checks before publication:

1. With 0.2.0 installed in a disposable Windows account, configure a sample folder, organize a fictional file, pause monitoring, and confirm its Activity entry. Close the application and install 0.3.0. Confirm that the folder, settings, history, and file content remain intact and monitoring starts paused.
2. In Settings > Language, choose Español. Check all five tabs, confirmation and cancellation dialogs, the disabled recycling explanation, and the tray menu. Confirm that no files move and existing category folder names remain unchanged when changing language.
3. Close and reopen; verify Spanish persists. Return to English, restart, and verify English persists. Check both languages at minimum window size and your Windows display scaling, including keyboard navigation.
4. Extract the portable ZIP into a new folder, keeping its `es` subfolder and both quick-start guides. Launch, change language, close, and reopen. Confirm the same behavior without installing .NET separately on the clean test machine.
5. Record Windows version, display scaling, date, and any observed problem. An automated runner checks package mechanics but does not replace this human review or prove a clean machine.

## Native file-safety acceptance

- **Cross-volume move and interruption:** on two separate local volumes, use the harness below. Record the physical disk/volume arrangement. Verify a completed move and undo, then terminate only the isolated worker during a copy. Recovery must preserve the intact source and partial destination, mark NeedsAttention, and block another move until resolved. A source-only or fully completed copy is an inconclusive interruption test.
- **Disabled recycling:** run the automated regression suite on the candidate source. It checks direct adapter/organizer calls, forced desktop command execution, repeated requests, and old journals without invoking the Windows bin. The old capacity harness is retired and must not be run; its failed results remain in the [investigation](../archive/recycle-capacity-investigation.md).

Developer cross-volume harness (requires the repository SDK; it is not part of clean-machine testing):

```powershell
dotnet run --project tools/FileOperationAcceptance -c Release -- verify E:\Dowlume.Acceptance artifacts/acceptance/cross-volume.json
```

Choose a writable folder on a second fixed local volume. The harness writes only uniquely named fixture directories, moves a 64 MiB sample and undoes it, then interrupts a separate 512 MiB copy. It retains both fixture directories and reports their paths. It does not simulate power loss or modify Recycle Bin settings.

## Publication handoff

I record evidence tied to the artifact hashes, resolve failures, obtain independent acceptance review, and repeat affected checks after changes. On 2026-10-20 I confirmed that all functionality worked correctly and published 0.2.0 using the same seven accepted draft assets. The packaged copy of this procedure retains its build-time pending status; the [acceptance record](../releases/acceptance/0.2.0.md) supersedes that status. For 0.3.0 I repeat the affected Windows acceptance on its exact hashes. macOS/Linux packages are restricted previews, with separate launch reports and no stable acceptance claim.
