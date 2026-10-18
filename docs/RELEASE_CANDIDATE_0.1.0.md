# Windows v0.1.0 candidate: 94ba177

This is an unpublished, unsigned candidate. Native recycling is disabled. No final tag or stable release has been created by this work. Human acceptance and explicit publication authorization remain mandatory.

## Exact artifacts

Source: `94ba177ea0da5e4809f87a59edfe5c7893b65537`, clean merged `main` after [PR #20](https://github.com/mglezz/dowlume/pull/20). The later acceptance-documentation commit records these artifacts; it does not change their source identity. `BUILD-INFO.json` records `DirtyWorkingTree: false`, self-contained `win-x64`, and unsigned packaging.

Local release directory: `artifacts/releases/v0.1.0-94ba177-20261018/`. Preserve these exact files for remaining acceptance; any rebuilt binaries need new hashes and affected checks repeated.

| File | SHA-256 |
| --- | --- |
| `Dowlume-0.1.0-win-x64-setup.exe` | `8B389F71177BE3EB5B8D83AD0618444D27B438AC4AFB239C63143FC1A88FDEAF` |
| `Dowlume-0.1.0-win-x64.zip` | `8806F9181E348AE1429921FE69476BFE6693072A419F9DA6CD22F3632B5148C3` |
| `Dowlume-0.1.0-windows-acceptance.zip` | `3E24F5E9EAD0F7BC0A7A01F5E4813B955A242A7C70A19BF0332AB0F2792A6E02` |
| `BUILD-INFO.json` | `2C3C714D4261759253D1E0AA248780AB88BA474D617F8F73BD706AC2C4795A8F` |
| `RELEASE_NOTES_0.1.0.md` | `DA0760C60B8E793A3D773146F18DE986C09E795F189358BA559A750DD54172CA` |

`SHA256SUMS.txt` contains those five entries; all were verified after packaging. The earlier upgrade fixture is the retained `cf42326` installer with SHA-256 `73C465CBFC4A47562D4859B9EF9AE0338EF66C9D836C5B07599E4CD37C93F507`.

## Completed checks

- [Hosted build, 69 tests, formatting and evidence utilities on merged main](https://github.com/mglezz/dowlume/actions/runs/39122455468): passed.
- [Hosted security on merged main](https://github.com/mglezz/dowlume/actions/runs/39122455435): passed.
- [Hosted packaging and package smoke test before merge](https://github.com/mglezz/dowlume/actions/runs/39122234994): passed. Packaging is a PR/manual workflow; this result is not claimed as a separate post-merge run.
- Local Release packaging reran all 69 tests: no failures or skips. Security analyzers, direct/transitive dependency audit, and redacted secret scans passed on the clean merged source, including 35 local Git-history commits. Reports: `artifacts/security/merged-94ba177-20261018/`.
- Exact-artifact host portable/install/upgrade/tray/uninstall acceptance passed. It verifies duplicate-instance refusal, running-app guards, startup-entry retention/removal, graceful exit, and preserved fictional files/profile. Report: `artifacts/validation-20261018-integration/package-94ba177-host.json`.
- Exact-artifact native desktop automation passed: settings, preview without changes, cancellation, organization with matching content hash, partial-file preservation, pause, visible disabled Recycle control, and graceful exit. Report: `artifacts/validation-20261018-integration/first-use-94ba177-host.json`. This does not replace a person's first-use test or selected-file Review acceptance.

## Separate clean guest

The clean pre-test checkpoint was exported and imported with a new identity into `E:\Dowlume.ContainmentAcceptance\clean-20261018-7c1adee0`. The imported system disk is standalone, with no parent disk in the evidence VM. Only the imported copy's saved state was removed for a fresh boot. The original VM and its three checkpoint identities remain unchanged; neither native probes nor recovery attempts were repeated.

New VM: `Dowlume-Windows11-Containment`, ID `98a117ea-3f5d-4391-abb7-cb23ccade398`. The copy is disconnected from the network, has automatic start/checkpoints disabled, and uses a dedicated disposable report-transfer disk. No .NET SDK/runtime was installed. The user signed in as the standard `DowlumeTester` account.

The host kit is `E:\Dowlume.ContainmentAcceptance\clean-20261018-7c1adee0\Kit-94ba177`; its per-file manifest covers packages and scripts. The guest kit is `C:\DowlumeAcceptance\containment-94ba177`. The runner verifies the account, interactive desktop, artifact hashes, clean-build identity and dedicated report marker before testing. Its first preflight refused because Windows did not expose the FAT volume label; the guard was corrected to identify the marker on the expected-size FAT disk. No application test or evidence-VM operation occurred during that refused preflight.

**Guest acceptance passed.** The raw package and first-use reports were retrieved from the dedicated report disk after normal shutdown, using a read-only filesystem reader. The report disk's SHA-256 was unchanged during extraction; provenance is recorded in `artifacts/validation-20261018-integration/guest-report-provenance.json`. Raw results are in `artifacts/validation-20261018-integration/guest-94ba177-raw/`, with a separate file-hash manifest. The VM remains off and the report disk is detached and retained.

`RESULT.TXT`, `SUMMARY.TXT`, `PACKAGE.TXT` and `FIRSTUSE.TXT` all report success for Windows 11 Pro build **26300**, standard interactive `DowlumeTester`, source `94ba177`, and the exact installer/portable hashes above. The package report enforced `RequiredNoSharedDotNet: true` and detected no shared runtimes or runtime directories. It passed portable startup, previous-candidate upgrade, native tray registration/restore, duplicate/running-app guards, startup registry retention/removal, exit, and file/profile preservation. First-use automation passed preview/cancel preservation, matching organized content, partial-file preservation, pause/exit, and the disabled Recycle control. The executable hash matches the host candidate; the retrieved build information and complete kit manifest match the host copies byte-for-byte.

The cloned guest clock was approximately one hour ahead of the host when these reports were generated. Raw timestamps are preserved unchanged; provenance and artifact hashes identify the run, rather than assuming clock agreement. These are automated results, not a sign-off by an independent human tester.

## Remaining steps in order

1. Start only `Dowlume-Windows11-Containment` and sign in as `DowlumeTester`. Run `C:\DowlumeAcceptance\containment-94ba177\Dowlume-0.1.0-win-x64-setup.exe` interactively. Complete [first use with a person](WINDOWS_ACCEPTANCE.md#first-use-with-a-person) on these exact artifacts: installer wizard/shortcuts, preview/cancel/organize/pause, tray overflow, selected-file disabled Recycle and safe Review/Undo, keyboard/scaling, actual Windows sign-in with startup enabled and disabled, and wizard upgrade/uninstall. Use a new fictional sample folder. Record tester, Windows build, date, hashes, observed outcomes and any issue. Automated checks do not certify these human checks. Do not rerun the completed automated kit or any native recycle probe.
2. Obtain independent review of the integrated containment change and all acceptance results; resolve any findings and repeat affected checks if binaries change.
3. Obtain the owner's explicit authorization to publish, as requested. The instruction to prepare and test the version is not treated as permission to skip unfinished gates.
4. Finalize the release notes' draft warning, refresh its checksum if edited, and reverify every distributable checksum. Tag only the accepted source commit as `v0.1.0`, then publish the installer, portable ZIP, acceptance ZIP, build information, notes and checksums. Do not attach private VM images, profiles or raw reports containing local paths.

The [release checklist](RELEASE_CHECKLIST.md) remains the publication gate. Recoverable quarantine, Linux and macOS remain deferred.
