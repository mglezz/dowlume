# Release validation

Target: Windows x64, version 0.1.0. Local validation is recorded in [Validation](VALIDATION.md). Native recycling is disabled at every execution boundary for this version. Earlier clean-guest package acceptance and failed native capacity results remain historical evidence; they do not approve changed binaries. Stable publication requires final-artifact acceptance, independent review, and explicit owner authorization. A GitHub draft or locally built artifact is not a published stable release.

The 2026-10-18 local containment candidate passed 69 regression tests, security scans, and host package/upgrade/uninstall acceptance. Its artifacts are in `artifacts/releases/v0.1.0-containment-20261018/`; they are unsigned and marked `DirtyWorkingTree: true`. See the [dated evidence](VALIDATION.md#2026-10-18-v010-containment-and-windows-package-validation). Rebuild after integration before final acceptance/publication.

## Reproducible packaging

Install a .NET SDK from the **10.0.4xx family** ([SDK policy](../GETTING_STARTED.md#requirements)) and [Inno Setup 6](https://jrsoftware.org/isdl.php), then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Release.ps1 -RequireInstaller
```

If the compiler is in a custom location, pass `-InnoCompiler "C:\path\ISCC.exe"`. With `-RequireInstaller`, a missing compiler fails before packaging. Omit that switch only when intentionally building a portable-only candidate.

Each invocation creates a new directory under `artifacts/releases/`; `-OutputDirectory <new-path>` chooses another new directory. Existing output directories are refused so stale binaries or installers cannot enter a new build. Outputs include the self-contained publish directory, portable ZIP, installer, acceptance-kit ZIP, release notes, `BUILD-INFO.json`, and SHA-256 checksums for exactly those distributable files. The version is read from the application project. `-SkipTests` is intended only after the same source revision has already passed validation.

Debug symbols, including native PDB assets supplied by NuGet packages, are removed only from the fresh publish directory before archiving. The portable acceptance check refuses packages containing them; runtime libraries remain included.

`BUILD-INFO.json` identifies the source commit, runtime target, build time, signing status, and whether the working tree was dirty. Rebuild from clean, merged `main` before preparing release assets. Do not distribute a dirty-tree build as the approved release. Retain checksums in every acceptance report; changed binaries require affected acceptance checks to be repeated.

The installer runs without elevation, creates application and quick-start Start menu shortcuts, and retains application data on uninstall. It refuses upgrade/uninstall while a Dowlume process is running in the session. Exit the app first; Setup does not force it closed. The ZIP includes the runtime and offline `START-HERE.txt`. Windows sign-in startup is off by default and can be enabled in Settings; uninstall removes the entry only when it belongs to the installed copy. The icon is original code-drawn artwork; regenerate it with `scripts/New-AppIcon.ps1`.

The [package workflow](../.github/workflows/package.yml) builds candidates and exercises portable startup, installation, running-app guards, and uninstall on a hosted Windows runner. That runner has .NET installed and does not count as clean-machine acceptance. Follow [Windows acceptance](WINDOWS_ACCEPTANCE.md) for the repeatable smoke script, physical-volume harness, manual cases, and tester handoff.

## Before public release

- [ ] Verify hosted build, test, formatting, security, and package checks for the final merged containment revision. Earlier candidate CI remains historical evidence.
- [ ] Rerun direct/transitive dependency, .NET security-analyzer, and redacted Git/source secret scans on the final clean source; retain reports. See Validation for local containment-build results.
- [x] Integrate the five implementation PRs into `main` after their hosted checks pass, preserving their commits.
- [ ] Obtain independent release approval after checking the integrated branch and remaining acceptance results.
- [x] Install on a clean supported Windows x64 machine without a preinstalled .NET runtime. The tester reported a passing standard-account guest run on Windows 11 Pro build 26300 for the exact `cf42326` candidate; see [evidence](VALIDATION.md#2026-10-16-clean-guest-package-acceptance).
- [ ] Repeat clean-machine package/upgrade checks for the final containment artifacts in a separate environment, retaining hashes and raw reports. Do not change the evidence VM.
- [ ] Confirm first-run preview, cancellation, pause, tray reopen, and exit with a nontechnical tester.
- [ ] Confirm Windows sign-in startup enabled/disabled behavior and manual wizard reinstall/uninstall in the guest.
- [x] Test a real move and undo between physical NVMe/SATA volumes and interrupt a 512 MiB copy; verify original/partial-copy preservation, NeedsAttention recovery, and blocked retry.
- [x] Contain the [failed oversized/near-capacity Recycle Bin cases](RECYCLE_CAPACITY_INVESTIGATION.md) by disabling native recycling in the UI, organizer, internal executor, and adapter. Keep historical states and reports; do not relabel native tests as passed.
- [x] Preserve both existing runs, checkpoints, exports, and recovery findings without modifying the evidence VM. Missing content remains unrecovered; quarantine is future work, outside v0.1.0.
- [ ] Independently verify disabled recycling and safe review operations on the final artifacts. No native recycle/restore or capacity probe is required or permitted for this release scope.
- [x] Test upgrade from the earlier local candidate and uninstall on the development PC; sample user files and the local database remain intact. Clean-machine acceptance remains separate.
- [x] Record the signing decision: initial candidate artifacts are unsigned, identified by SHA-256 and source commit. No signing certificate is configured; signing can be added before publication if one becomes available.
- [ ] Rebuild from clean, reviewed, merged `main`; check `DirtyWorkingTree: false`, final checksums, notes, and signing decision. Repeat affected acceptance on those exact artifacts.
- [ ] Obtain the owner's explicit publication authorization after all mandatory checks pass.
- [ ] Only after authorization, publish release notes, checksums, installer, and ZIP; tag approved source as `v0.1.0`.

Do not mark unavailable hardware tests, clean-machine checks, remote CI, or signing complete based only on unit tests. Cross-volume failure simulations complement but do not replace real-volume testing.

## Release draft and publication

Use the [release notes draft](RELEASE_NOTES_0.1.0.md), targeting the tested merge commit. Attach the installer, portable ZIP, acceptance kit, build information, and checksums to an unpublished GitHub draft for `v0.1.0`. Keep the draft warning and do not create the stable Git tag while acceptance is incomplete.

After the remaining checks pass for those exact artifacts, record independent review and obtain the owner's explicit authorization. Only then replace the draft warning with the acceptance result and publish the release/tag. Repository visibility is unchanged; a release in a private repository is accessible only to authorized users. Invite testers through the project's normal access process and use the bug-report form to collect problems without private files or database uploads.
