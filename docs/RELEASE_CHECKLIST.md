# Release validation

[English](#) · [Español](es/RELEASE_CHECKLIST.md)

For 0.3.0 I follow the current [Windows acceptance procedure](WINDOWS_ACCEPTANCE.md), [release notes](RELEASE_NOTES_0.3.0.md) and [experimental platform scope](PLATFORM_SUPPORT.md). Implementation and hosted automation do not replace final human acceptance.

For the current release, see the [0.2.0 acceptance and publication record](RELEASE_CANDIDATE_0.2.0.md). I retain the completed 0.1.0 checklist below unchanged.

I retain this checklist as the completed acceptance record for the published v0.1.0 artifacts. I assess changed binaries separately.

Target: Windows x64, version 0.1.0. Local validation is recorded in [Validation](VALIDATION.md). Native recycling is disabled at every execution boundary for this version. Earlier clean-guest package acceptance and failed native capacity results remain historical evidence; they do not approve changed binaries. Stable publication requires final-artifact acceptance, independent review, and my explicit authorization. A GitHub draft or locally built artifact is not a published stable release.

**Completed:** [v0.1.0 is published](https://github.com/mglezz/dowlume/releases/tag/v0.1.0) with my explicit authorization on 2026-10-18. The tag targets tested commit `94ba177`; all six public downloads were independently downloaded and hash-verified.

The accepted candidate is clean merged commit `94ba177`, staged for publication in `artifacts/releases/v0.1.0-final-94ba177-20261018/`. Automated host/clean-guest checks passed; I confirmed that all human tests and independent review passed on 2026-10-18. The tested binaries and build information are unchanged; standalone notes/checksums were finalized. It is unsigned and records `DirtyWorkingTree: false`. See the [candidate hashes and remaining steps](RELEASE_CANDIDATE_0.1.0.md). Earlier candidates remain historical evidence.

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

- [x] Verify hosted build, test, formatting, security, and package checks for the containment change: PR packaging passed before merge; merged `94ba177` build/test/format and security passed. Exact merged artifacts also passed local package acceptance; see the candidate record for precise run identities.
- [x] Rerun direct/transitive dependency, .NET security-analyzer, and redacted Git/source secret scans on clean `94ba177`; reports retained in `artifacts/security/merged-94ba177-20261018/`.
- [x] Integrate the five implementation PRs into `main` after their hosted checks pass, preserving their commits.
- [x] Obtain independent review of the integrated change and acceptance results: explicitly confirmed by me on 2026-10-18; see the candidate record for provenance.
- [x] Install on a clean supported Windows x64 machine without a preinstalled .NET runtime. The tester reported a passing standard-account guest run on Windows 11 Pro build 26300 for the exact `cf42326` candidate; see [evidence](VALIDATION.md#2026-10-16-clean-guest-package-acceptance).
- [x] Repeat clean-machine package/upgrade checks for exact `94ba177` artifacts in the separate Windows 11 Pro build 26300 guest as standard `DowlumeTester`, without shared .NET. Package and first-use automation passed; raw reports, hashes and read-only extraction provenance are retained. Evidence VM unchanged.
- [x] Confirm human first-run preview, cancellation, pause, tray reopen, and exit: I reported that all human acceptance tests passed on 2026-10-18.
- [x] Confirm Windows sign-in startup enabled/disabled behavior and manual wizard reinstall/uninstall: included in my positive human-acceptance report.
- [x] Test a real move and undo between physical NVMe/SATA volumes and interrupt a 512 MiB copy; verify original/partial-copy preservation, NeedsAttention recovery, and blocked retry.
- [x] Contain the [failed oversized/near-capacity Recycle Bin cases](RECYCLE_CAPACITY_INVESTIGATION.md) by disabling native recycling in the UI, organizer, internal executor, and adapter. Keep historical states and reports; do not relabel native tests as passed.
- [x] Preserve both existing runs, checkpoints, exports, and recovery findings without modifying the evidence VM. Missing content remains unrecovered; quarantine is future work, outside v0.1.0.
- [x] Verify disabled recycling and safe review operations on the final artifacts: included in my positive human-acceptance and independent-review confirmations. No native recycle/restore or capacity probe was repeated.
- [x] Test upgrade from the earlier local candidate and uninstall on the development PC; sample user files and the local database remain intact. Clean-machine acceptance remains separate.
- [x] Record the signing decision: initial candidate artifacts are unsigned, identified by SHA-256 and source commit. No signing certificate is configured; signing can be added before publication if one becomes available.
- [x] Rebuild the integrated containment change from clean merged `main` (`94ba177`); verify `DirtyWorkingTree: false`, checksums, final notes, and unsigned signing decision. Accepted binaries are preserved unchanged.
- [x] Obtain my explicit publication authorization after all mandatory checks pass: confirmed in the project conversation on 2026-10-18.
- [x] Publish release notes, checksums, installer, portable and acceptance ZIPs, and build information after authorization; tag approved source `94ba177` as `v0.1.0`. Verify all public downloads.

Do not mark unavailable hardware tests, clean-machine checks, remote CI, or signing complete based only on unit tests. Cross-volume failure simulations complement but do not replace real-volume testing.

## Published release

The [finalized release notes](RELEASE_NOTES_0.1.0.md) accompany the tested `94ba177` binaries. The former draft is now the public [v0.1.0 release](https://github.com/mglezz/dowlume/releases/tag/v0.1.0), containing exactly the accepted installer, portable ZIP, acceptance kit, build information, notes, and checksums. Earlier draft assets remain archived locally with verified hashes.

Acceptance, independent review, explicit publication authorization, tag creation, and public-download verification are complete. Repository visibility is unchanged. Collect future reports without private files or database uploads; do not move the accepted tag or replace tested assets with rebuilt binaries.
