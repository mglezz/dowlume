# Local validation record

[English](#) · [Español](es/VALIDATION.md)

I keep dated validation records here. A pending item in an older entry describes that revision and date; later entries record completion without changing the original evidence. Local artifact paths refer to retained private evidence and may be absent from a fresh clone.

## 2026-10-19: English/Spanish localization and documentation review

I added a per-profile English/Spanish selector to current source, retaining English defaults for older profiles and preserving folder names, file contents, rules, and stored history. I checked startup persistence, changes with unsaved settings, failed preference writes and retry, translated live views/tray/status/errors, and Spanish confirmation cancellation after changing language with an existing preview. Resource checks compare all English/Spanish keys, placeholders, and domain display labels. Desktop tests share one collection because Avalonia headless initialization is process-global. The first hosted Windows run exposed CRLF resource values failing to match LF confirmation templates; I normalized message lookup line endings, fixed resource checkout to LF, and added LF/CRLF regression coverage.

On macOS with .NET SDK 10.0.401, the Release build and all **74 tests passed**, with no skips. The default macOS temporary path crosses a symbolic link, so I ran the filesystem suite with `TMPDIR` pointing to a real ignored directory under `artifacts/test-tmp`; I did not relax path-safety checks. Formatting verification, `git diff --check`, and the direct/transitive NuGet vulnerability audit passed. I rendered and inspected all five Spanish tabs at the 880 × 680 minimum size, adjusted wrapping for settings labels, and retained private test captures under ignored `artifacts/localization/` rather than publishing local paths.

I added 20 Spanish documentation pages with cross-language navigation and checked all local links and heading targets across 40 Markdown pages. I retained English screenshots/UML and immutable historical sources as shared references, clarified their revision, updated the author voice, and corrected stale release/platform/localization status. I recovered the pending publication documentation through [PR #23](https://github.com/mglezz/dowlume/pull/23), which passed hosted checks before merging, and verified public v0.1.0 release metadata. I added a Spanish offline guide and a packaging guard for the Spanish resource assembly.

These local checks do not constitute manual Windows acceptance or a new release. The published v0.1.0 binaries and tag remain unchanged and English-only. Native folder-picker controls follow Windows; startup failures before reading preferences fall back to English, and unknown historical messages retain their original wording. Future localized binaries require the affected Windows acceptance checks.

## 2026-10-18: Authorized v0.1.0 publication

After all acceptance gates were completed, I explicitly authorized publishing v0.1.0 and creating its tag. [Release v0.1.0](https://github.com/mglezz/dowlume/releases/tag/v0.1.0) was published at `2026-10-18T17:00:48Z`. The tag resolves to tested source `94ba177ea0da5e4809f87a59edfe5c7893b65537`; it does not point to a later documentation-only commit. The finalization PR passed hosted build/test/format, security and packaging checks before merging.

All six release assets were downloaded without authentication and independently checked against local accepted files, GitHub's SHA-256 digests, sizes, and the downloaded checksum manifest. All matched. The installer and portable are the exact accepted binaries; standalone notes were finalized and their checksum updated before authorization. Publication evidence is retained in `artifacts/validation-20261018-integration/`, including `published-release.json`, `published-tag.json`, `publication-authorization.json`, `public-download-verification.json`, and the downloaded assets. The [candidate record](RELEASE_CANDIDATE_0.1.0.md#publication-completed) and [checklist](RELEASE_CHECKLIST.md) identify the finished gates. No VM, checkpoint, or failed native-probe evidence was modified.

## 2026-10-18: My confirmation of human acceptance and independent review

I recorded successful human acceptance for the exact `94ba177` candidate, with no apparent application issues, and confirmed that independent review of the change and results was included. This entry records my confirmation; it does not provide individual tester identities or additional manual logs. The [candidate record](RELEASE_CANDIDATE_0.1.0.md#human-acceptance-and-independent-review) records the scope and provenance.

The installer, portable ZIP, acceptance ZIP and build information are preserved byte-for-byte. Final publication staging is `artifacts/releases/v0.1.0-final-94ba177-20261018/`; only standalone release notes and checksums were updated after acceptance. The earlier draft's five assets and metadata were preserved with hashes matching GitHub's digests before replacement. No VM or checkpoint was changed and no VM acceptance or native recycle probes were repeated for this documentation-only finalization. Hosted CI still validates documentation PRs through the normal workflows. Explicit publication authorization remains pending; no final tag or stable release is claimed.

## 2026-10-18: Clean merged candidate and final host acceptance

[PR #20](https://github.com/mglezz/dowlume/pull/20) was merged as `94ba177ea0da5e4809f87a59edfe5c7893b65537` after build/test/format, security, and packaging checks passed. The realistic queue integration fixture also needed a bounded 60-second CI budget for durable SQLite writes; the test still requires all 40 completed journal entries and matching contents, and emits progress/error diagnostics on timeout. Both earlier hosted failures remain recorded. The changed fixture passed locally five consecutive times, then passed hosted CI. Post-merge build/test and security checks also passed.

The candidate was rebuilt from clean merged source with all **69 tests passing**, no failures/skips, and `DirtyWorkingTree: false`. Fresh dependency/security-analyzer and redacted Git/source secret scans passed; the history scan covered 35 local commits. Exact-artifact host package acceptance and native first-use automation passed. The [candidate record](RELEASE_CANDIDATE_0.1.0.md) identifies all five distributable hashes, CI links, report directories, separate-guest provenance, and remaining human/publication gates. Earlier dirty-tree packages are retained only as historical review artifacts.

The separate clean guest also passed both package and first-use automation on Windows 11 Pro build 26300 as standard `DowlumeTester`, with no detected shared .NET runtimes and `RequiredNoSharedDotNet: true`. Raw reports were independently retrieved after normal shutdown from its dedicated report disk without changing the disk hash. Their package/executable hashes, build information and kit manifest matched the host candidate. The original evidence VM and its three checkpoints were untouched. A report-volume-label preflight issue was corrected before application testing; it was a runner-identification issue, not a failed file-operation test. The imported guest's clock was about one hour ahead of the host; original report timestamps are retained and this limitation is documented. Human first use, actual sign-in startup, wizard checks, selected-file Review acceptance, independent review and publication authorization remain pending.

## 2026-10-18: Hosted integration and separate clean-guest preparation

Containment was committed as `715834e` and submitted in [PR #20](https://github.com/mglezz/dowlume/pull/20). The first hosted security scan passed. The first build/test run exposed a pre-existing test-fixture race in `FullQueueEventuallyDiscoversEveryFileAndIgnoresTemporaryFiles`: its unconditional `File.Move` callback treated a stale discovery hint as an existing file, and its completion counter advanced before the move. The fixture now exercises the real idempotent organizer, waits for monitor shutdown, and verifies all 40 completed journal entries and file contents. This changes the integration test, not application file-operation behavior. The failed hosted result is retained; it is not waived by local results.

The VM opened for follow-up acceptance was identified as the original evidence VM, with all three checkpoints still present. A separate export of the clean, signed-in checkpoint (before package/native probes) was started into new host storage. The original VM was not restored or used to run tests. Export/preparation alone does not establish guest acceptance; results must identify the new VM and final candidate hashes.

## 2026-10-18: v0.1.0 containment and Windows package validation

Work started on 2026-10-17 and completed locally on 2026-10-18. Base commit: `2dcc06967adb7cf815d9378fc8845c2c8045bb7e`, with uncommitted containment changes. This entry supersedes earlier current-state statements that application changes were on hold or native recycling remained enabled. All earlier dated evidence remains below unchanged. No VM, checkpoint, export, original probe report, or native bin policy was modified; no destructive recycle probe was repeated.

Native recycling is now disabled in the UI command, organizer API, internal decision/execution paths, and compatibility adapter. The organizer no longer has an injectable recycle dependency. COM deletion code and its progress sink are archived as exact text with SHA-256 hashes outside the projects and packages. Old probe entry points refuse before any action. Historical enum values and completed records remain compatible; recovery never replays recycling or infers success from missing content. Keep, Snooze, Move, and eligible Undo remain available. Quarantine is documented future work, not implemented.

| Check | Result |
| --- | --- |
| Release regression/integration suite | **69 passed, 0 failed, 0 skipped**, including disabled adapter calls with locked/changed sources, repeated organizer calls, internal execution guards, forced UI command execution, old-journal restart/recovery, safe review/undo, monitoring, collisions, and file races. Native recycling tests were replaced by refusal regressions; the lower total than the old 72-test suite is not a skipped-test result. |
| Evidence utilities | Five read-only inventory cases and nine in-memory metadata cases passed. No existing bin or guest data was changed. |
| Release/security builds | Application solution and both helper projects built with **0 warnings and 0 errors**. Direct/transitive NuGet audit reported no vulnerable packages. All .NET security analyzers passed. |
| Secret scans | Pinned, SHA-256-verified Gitleaks 8.30.1 reported no leaks in 32 local Git-history commits or the tracked/unignored working-source snapshot. This does not attest unseen remote history. |
| Formatting and source review | Solution and both helper-project format checks passed; `git diff --check` passed. Native shell recycle calls are absent from executable project sources. |
| Desktop/layout | Headless workflow passed, including the disabled visible Recycle button and forced execution refusal without confirmation or journal mutation. Review at 880 x 680 was inspected: safety explanation and safe controls remain readable. Updated only the affected published Review screenshot. Human Windows scaling/keyboard acceptance remains pending. |
| Technical documentation | Ten PlantUML diagrams validated/rendered; only three functionally affected activity/state SVGs changed. Those three were visually inspected. Local Markdown paths/anchors validated, including the historical archive; canonical repository links now use `mglezz/dowlume`. External URLs were not network-validated. |
| Windows packaging | Self-contained `win-x64` Release portable, per-user installer, acceptance kit, release notes, build information, and five SHA-256 entries generated in a fresh directory. No debug symbols or archived native source are distributed. |
| Package integration on development PC | **Passed**: portable launch/exit; upgrade from verified `cf42326` installer; duplicate-instance refusal; native tray registration and hidden/minimized restore; running-app upgrade/uninstall refusal; startup-entry retention/removal; graceful exit; profile and fictional-file preservation after uninstall. All five artifact checksums matched. This host has .NET installed and is not clean-machine acceptance. |

Local outputs: `artifacts/releases/v0.1.0-containment-20261018/`. Installer SHA-256: `CE14118D5BECECA71BE79B67D4004E88D59B5A7CB17C9143299E707036B4F173`. Portable SHA-256: `28F1ABABACEBC2D9704CD8FE43777A281EC63D2B6B5776A8A645EAD144C50B86`. `SHA256SUMS.txt` also covers release notes, acceptance ZIP, and `BUILD-INFO.json`. The build is **unsigned** and explicitly records **DirtyWorkingTree: true**; it is a local review candidate, not approved publication material.

Reports are retained under `artifacts/validation-20261017-containment/` (TRX, package result, link check, screenshots) and `artifacts/security/containment-20261018/` (dependency JSON, SARIF, redacted scans, summary). The package report identifies its unique retained temporary installation/profile/fixture directory and both installer hashes. The earlier candidate is launched only for package upgrade acceptance; monitoring/native recycle operations are never started. Historical archive integrity checks matched all four retained source hashes.

The prior physical NVMe/SATA move/interruption evidence is retained. The verified file-copy engine is unchanged; destructive/native capacity testing and evidence-VM use were not repeated. The missing oversized/sentinel contents remain unrecovered. No quarantine or Linux/macOS development was started.

**Still mandatory before publication:** review and integrate the changes, pass hosted CI for the final revision, rebuild from clean merged `main`, rerun current security/package checks and affected clean-machine acceptance against those exact hashes in a separate environment, complete nontechnical first use, manual sign-in startup, wizard upgrade/uninstall, disabled-Recycle and safe-review checks, obtain independent approval, and then obtain my explicit publication authorization. Final tag `v0.1.0` and release remain uncreated/unpublished by this work. See the [release checklist](RELEASE_CHECKLIST.md) and [Windows procedure](WINDOWS_ACCEPTANCE.md).

## 2026-10-17: Existing clear key permits checkpoint inspection

A separate forensic copy of the exported failure checkpoint was opened read-only with host-only Dissect tools. The Windows partition contains an existing BitLocker clear key; unlocking in memory succeeded and exposed a valid NTFS filesystem. A recovery password is **not required for this image**. This corrects the earlier assessment below, which reflected the NTFS-only reader's limitation rather than the available BitLocker unlocking mechanisms.

The exported VHDX and forensic copy matched SHA-256 `3AE5611E04F6031DF15EC3E4F6B78323C3B1D8E0B29650892461866751AC65C5`; the copy's hash remained identical after inspection. Private provenance, package installation hashes, scripts, and `CLEAR-KEY-RESULT.json` are retained with the evidence. No keys were printed/saved, no protectors were added, and the original VM, export, and checkpoints were not modified. The copy was not mounted or booted.

The first run's reports were read directly from the checkpoint. Its ordinary, disabled, and near-capacity fixtures match the expected hashes; oversized and sentinel files are absent from that directory. The second run's directory is absent, consistent with capture before that run. See [checkpoint access and limitations](RECYCLE_CAPACITY_INVESTIGATION.md#checkpoint-access-with-its-existing-clear-key), including why adding a protector now would not retroactively unlock an older checkpoint.

The subsequent read-only scan visited 185,178 valid MFT records, including deleted records, with no inspection errors among those records. It found no qualifying oversized/sentinel original-name or `$R*.bin` record at the expected fixture sizes. The image's SHA-256 remained unchanged after this scan. No missing contents could be hash-verified through that search. Reused/invalid records and raw-cluster carving are outside this bounded assessment; it does not prove physical irrecoverability. The four edited documentation files passed local link/anchor validation (27 targets) and `git diff --check`; no image assets changed.

This is forensic access validation, not a passed application preservation test. No native acceptance probe was repeated, no contents were restored, and no application behavior changed. Stable publication and independent recovery validation remain blocked.

## 2026-10-17: Raw metadata correlation

The tester reported a complete, error-free raw inventory: four `$I` metadata files plus `desktop.ini`, with no `$R` content files. Their reported sizes and SHA-256 values were retained privately on the host. The subsequent raw-byte output was independently hashed and decoded on the host: all four records describe the ordinary and near-capacity fixtures restored by the harness, one of each per run. Their report content hashes match the earlier retained-source inventory. None describes an oversized or sentinel fixture. Metadata alone cannot verify a missing content hash or restore file contents.

The new console-only metadata reader verifies the supplied manifest before decoding and correlates only recognized probe paths with existing reports. Nine in-memory parser checks passed, including Unicode paths, field decoding, unsupported versions, truncated/oversized length declarations, missing/embedded terminators, invalid encoding, and invalid FILETIME. No native acceptance test or recovery operation was executed.

Read-only host inspection confirmed all three checkpoints. The preserved failure checkpoint's creation time is `2026-10-16T18:49:38.5401180Z`, preceding the second oversized report; the earlier wording describing it as the completed failed state must not be interpreted as proof that it includes both completed runs. See [recovery assessment](RECYCLE_CAPACITY_INVESTIGATION.md#raw-bin-evidence-and-content-recovery). Native implementation changes and release publication remain on hold.

Export of that checkpoint completed into separate private host storage. Windows refused a read-only mount with error `0x80070522`; the exported disk remained unattached. A host-only image reader, built without warnings using DiscUtils 0.16.13 and opening the export with read-only file access, identified BitLocker on the Windows partition. It could not inspect the encrypted fixture paths. This was incomplete recovery coverage, not proof that the checkpoint lacks recoverable contents. No original checkpoint was restored/deleted, no guest recovery software was installed, and no file was recovered or restored. The later clear-key inspection above resolves this access limitation without a recovery password.

## 2026-10-17: Restore timing and two-run evidence review

The tester supplied oversized reports and final fixture inventories for two existing runs, recorded at `2026-10-16T18:46:21.5742316Z` and `2026-10-16T18:49:40.815674Z`. Both oversized reports show IOException with embedded shell result `00270008`, source absent, and matching candidate assembly hashes. Both final inventories lack the oversized and sentinel sources; the shell bin inventory is empty. The reported excerpts were retained separately on the host with their tester-supplied provenance. The second run's full summary has not been independently retrieved.

Source review confirms that the harness never deletes fixtures or chooses an alternative restore directory. It attempts shell restore after the immediate report, explaining the near-capacity source's later presence. Oversized restore outcomes were not recorded separately. No lasting oversized restoration is visible; permanent deletion is the leading explanation, with raw bin entries still unexamined. The optional read-only collector extension can inspect hidden/orphaned payloads without repeating any native operation. See the [timing analysis and isolated acceptance plan](RECYCLE_CAPACITY_INVESTIGATION.md#2026-10-17-two-run-evidence-and-restore-timing).

The host still has the running acceptance guest and all three checkpoints. No failed native test was requested or executed during this investigation. The release remains blocked and unpublished; the recovery-storage proposal remains unimplemented.

`scripts/Test-RecycleEvidenceInventory.ps1` passed five local cases using a new fictional directory: accessible empty inventory, hidden orphan payload with preserved hash/attributes, missing-path error, untraversed directory, and redirected-root refusal. The test loads only the read-only inventory function and never invokes the shell adapter; it is also included in the Windows build workflow. These checks validate the collector, not application recoverability.

## 2026-10-16: Native Recycle Bin capacity failures

The tester supplied the complete native summary recorded at `2026-10-16T18:46:22.3598918Z` from the same Windows 11 Pro build 26300 standard account. The self-contained worker used the exact candidate assemblies extracted from the verified portable archive. Ordinary recycling/Windows shell restore passed with a matching hash, and disabled-bin refusal preserved the source. A 2 MiB sample with a 1 MiB bin failed the source-preservation assertion. Adding a second 768 KiB sample to a 1 MiB bin recycled the incoming item but lost the earlier recycled sentinel. The script reported successful restoration of the original guest policy.

Acceptance is **failed**, not waived. A post-failure checkpoint preserves the guest before read-only evidence collection. The original clean and pre-probe checkpoints, worker, fixtures, and reports are retained. This summary originally described one run; the subsequent collector excerpts identify two existing runs, as recorded above. No rerun was requested by the investigation. See [capacity investigation](RECYCLE_CAPACITY_INVESTIGATION.md) for the implementation findings and proposed same-volume recoverable quarantine. No replacement implementation or release publication is claimed.

## 2026-10-16: Clean-guest package acceptance

The tester supplied the guest launcher's successful JSON summary, recorded at `2026-10-16T18:39:20.3258006Z`, after running the staged kit from the signed-in standard `DowlumeTester` desktop. The guest was freshly installed from the verified official Windows 11 x64 ISO; the tester confirmed no .NET SDK, shared runtime, or development tools were installed. The launcher enforced `-RequireNoSharedDotNet`, verified the standard account and package hashes, and completed successfully. Windows reported Microsoft Windows 11 Pro, version `10.0.26300`, build `26300`.

The tested source is `cf42326f4560515b1574ebe7794ef645a551bc21`; installer SHA-256 is `73C465CBFC4A47562D4859B9EF9AE0338EF66C9D836C5B07599E4CD37C93F507`. The tester explicitly confirmed native notification-area registration and restoration from hidden/minimized states. The completed script also covers portable launch/exit, previous-candidate upgrade, duplicate-instance and running-app guards, startup-entry retention/removal, uninstall, and profile/fictional-file preservation. The full `package-result.json` and transcript remain in the guest; the tester-supplied summary is retained separately on the host, with its provenance identified. This is a reported guest result, not an independently retrieved raw report.

The host retained clean and pre-Recycle-Bin checkpoints. Manual first use, actual sign-in/startup behavior, native disabled/full-bin checks, Windows restore, and independent review remain separate gates. The release has not been published.

## 2026-10-16: Hyper-V guest preparation

Hyper-V access now works from the development session; `Get-VM` initially returned no guests. A dedicated Generation 2 VM was created with two virtual CPUs, 4–6 GiB dynamic memory, a 96 GiB dynamic disk, Secure Boot, and virtual TPM. The firmware/security settings were queried after creation. The VM was initially left powered off pending official Windows 11 x64 media and interactive Windows setup. No guest acceptance test has run.

The exact clean `cf42326f4560515b1574ebe7794ef645a551bc21` candidate and the earlier `cdc5e35` installer were staged in dedicated test storage. All four release checksums and the earlier installer's SHA-256 matched. The preparation script and [Hyper-V handoff](HYPER_V_ACCEPTANCE.md) preserve the distinction between provisioning, automated interactive smoke, and manual acceptance. Microsoft's automated ISO link request was blocked; a browser download was requested. Personal files and host Recycle Bin policy were not changed, and the release remains unpublished.

I subsequently supplied the official English International x64 ISO. Its SHA-256 matched Microsoft's published value; [media details](HYPER_V_ACCEPTANCE.md#prepared-host-state--2026-10-16) are recorded with the VM handoff. The ISO is attached, the VM is running, and its console is open for interactive Windows setup. Preparation [PR #14](https://github.com/mglezz/dowlume/pull/14) passed build/test and security workflows before merging. Guest setup and acceptance remain pending.

## 2026-10-16: Background access, race review, and security scans

Tray/startup implementation: `3218f6a` (PR #12, merged after both hosted workflows passed). File-operation hardening: `eb01156`. All file-operation fixtures were generated for testing. No personal Downloads, host Recycle Bin policy, or VM was modified.

| Check | Result |
| --- | --- |
| Regression suite | 72 tests passed: 70 regular tests and two native WindowsShell tests; no failures or skips. Closing during a pending confirmation cancels it without organizing files or hanging shutdown. |
| Tray regression | Application-level attachment, reopening a hidden/minimized window from both commands, and disposal covered headlessly. Native Windows icon registration and UI-Automation Hide to tray followed by native click delivery also passed. |
| Windows startup | Default-off behavior, failure feedback, quoted paths, ownership-scoped removal, and unsupported hosts covered. Packaged upgrade retained the fixture startup entry; uninstall removed it. Actual sign-in remains a VM/manual check. |
| Race conditions | Verified source handles refused writes/renames/replacement; late destination collisions preserved both files; directory leases blocked ancestor renames. Overlapping review actions rejected the stale second decision. |
| Cross-volume files | A 64 MiB sample and an alternate data stream survived move and undo between the NVMe system volume and a separate SATA disk. Interrupting the 512 MiB worker preserved source/partial copy, marked NeedsAttention, and blocked another move. |
| Recycling | Ordinary native recycling and changed-content refusal passed. Callback tests cover permanent-delete refusal, changed content, missing recycled destination, disk-full HRESULT, and unconfirmed outcomes. These simulations do not certify disabled/full Windows bin policy. |
| Dependencies | No vulnerable direct/transitive packages reported by the configured NuGet sources. |
| Code scan | All .NET security analyzers passed for the solution and both helper projects with warnings treated as errors. SQL statements use a fixed private table selection and parameterized data; native library and Explorer lookup use system locations. |
| Secrets | Gitleaks 8.30.1 passed for all local Git refs and tracked/unignored source snapshots, with redacted reports. The scanner archive matched its pinned SHA-256. |
| Packaging | Local portable launch, previous-draft upgrade, running-app refusal, tray restoration, startup registration, graceful exit, uninstall, and fixture/profile preservation passed. |
| Hyper-V | The host service is running. Repeated host PowerShell `Get-VM` checks returned permission denied; no clean guest or manual VM workflow was validated. |

Reports are retained under `artifacts/security-20261016-final/` and `artifacts/acceptance/`, including `cross-volume-handle-streams.json` and `package-security-20261016.json`. The preliminary package was built with a dirty tree and is not an approved release artifact; rebuild the merged revision and repeat package checks before replacing draft assets. The dedicated security workflow repeats the scans and retains redacted reports.

Security/stability acceptance remains open for clean-guest installation, actual sign-in, manual organization/undo/upgrade/uninstall, disabled/full Recycle Bin policy, and independent first-use/release review. See [security review](SECURITY_REVIEW.md) for scope and residual race limits. Passing scanners does not authorize marking unavailable manual tests complete.

## 2026-10-16: Windows release hardening

Implementation revision: `d01ae7b`. Environment: Windows 11 Pro x64 build 26200, .NET SDK 10.0.401, Inno Setup 6.7.3. File-operation tests used unique disposable directories; personal Downloads and Recycle Bin settings were not modified. At that point, I confirmed that no clean Windows environment was available.

| Check | Result |
| --- | --- |
| Automated regression suite | 54 non-shell tests passed in Release configuration. |
| Native recycling | Separate WindowsShell test passed with the verified read handle; one uniquely named disposable file was recycled. The bin was not emptied. |
| Physical cross-volume move | A 64 MiB sample moved from the NVMe system volume to a separate SATA disk and was restored by Undo; hashes matched. |
| Interrupted physical copy | The isolated worker was terminated during a 512 MiB cross-volume copy. The original hash remained intact, the partial destination differed, recovery preserved both, marked NeedsAttention, and refused another move. |
| Portable and installer | ZIP extraction/launch/exit, per-user installation, upgrade from the earlier local candidate, and uninstall passed. The real SQLite profile and a sample user file were retained unchanged by install/uninstall. |
| Running application protection | Upgrade and uninstall both refused while Dowlume was running; executable content remained unchanged. Duplicate-profile startup was refused and normal shutdown completed. |
| Packaging checks | Existing output directory refused; runtime-free acceptance mode correctly refused the development PC. Offline guide and bundled runtime/native libraries present. |
| Formatting and dependencies | Solution and acceptance-harness formatting passed; PowerShell scripts parsed; no vulnerable packages reported by configured NuGet sources. |

The preliminary installer smoke used SHA-256 `379132929097280BF574461AFDD25FD70C79F32431169AEAA29BE85CCB6DC130`; the earlier candidate was `AE7AB83D591A82DE574B253141D705D4923E747F5F84E3772185649417D2CA36`. These preliminary packages were built before committing and are marked dirty in their build information. They establish the local checks above, not approval of final release assets. Final packages must be rebuilt from the clean merged revision and have their own smoke report and hashes.

Raw reports remain under `artifacts/acceptance/` because they contain local paths. Cross-volume fixtures were retained for inspection. These tests cover process interruption, not power loss or physical disk failure. Clean-machine installation without .NET, disabled/full Recycle Bin behavior, independent first-use testing, and final release review remain open. The [acceptance procedure](WINDOWS_ACCEPTANCE.md) and [release checklist](RELEASE_CHECKLIST.md) define the handoff; stable `v0.1.0` publication is not yet justified.

## 2026-10-16: Searchable activity history

Environment: Windows x64, .NET SDK 10.0.401. Application revision: `ce28fda`. This change extends Activity beyond the former 200-entry display limit; the tagged release candidate and existing local packages are unchanged.

| Check | Result |
| --- | --- |
| Release build and automated suite | `dotnet test Dowlume/Dowlume.slnx -c Release --no-restore --filter "Category!=WindowsShell"` passed all 54 tests, with no failures or skips. |
| History storage | All 251 fixture entries are reachable without duplication; paging survives reopening the database and new arrivals between reads. Updated statuses keep their journal position. |
| Search and counts | Unicode case comparison, literal SQL/wildcard characters, source/destination paths, null destinations, combined filters, empty results, page clamping, invalid inputs, and independent attention counts covered. |
| Desktop workflow | Browse older pages, retain selection during refresh/new arrivals, search an action older than 200 entries, undo it through the existing safety checks, clear filters, and render the new controls. |
| Activity screenshots | Fictional-data renderer completed successfully. Activity inspected at 1120 × 820 and 880 × 680; normal-size capture committed in [images](images/README.md). |
| Formatting and whitespace | `dotnet format --verify-no-changes --no-restore` passed for the solution and screenshot renderer; `git diff --check` passed. |
| Dependency audit | `dotnet list Dowlume/Dowlume.slnx package --vulnerable --include-transitive` reported no vulnerable packages from configured sources. |
| Documentation | Local Markdown file/image targets checked; user guide, architecture, roadmap, and [platform assessment](PLATFORM_SUPPORT.md) updated. |

Pagination uses schema version 1 without migrating or deleting history. Counts and search still scan journal JSON; organizer/recovery queries still load complete collections. This is a bounded history display, not a claim of million-file scalability.

Native recycle and installer code were not changed or revalidated in this increment. Clean-machine, physical cross-volume, and remaining release acceptance gates stay open. macOS and Linux ports are deferred until their native adapters and platform safety checks are implemented and validated.

## 2026-10-15: Domain-model documentation

Scope: the [domain model](requirements/DOMAIN_MODEL.md), its UML sources and SVGs, documentation links, and a standalone rendering script. Application source is unchanged from `a4b1c9a`. The model was checked against the current implementation and existing tests; no new application acceptance results are claimed.

| Check | Result |
| --- | --- |
| PlantUML syntax and generation | All ten sources passed syntax validation and SVG/PNG generation with PlantUML 1.2026.8 and Eclipse Temurin Java 21.0.12.1 on Windows x64. |
| SVG structure | Every generated file parsed as SVG XML; no embedded scripts or HTML were present. |
| Visual rendering | All ten final SVGs opened in headless Chrome with an isolated profile and were visually inspected. PlantUML PNG previews and direct SVG rasterization also completed. Labels, multiplicities, branches, and object-instance notation were checked. |
| Semantic consistency | Reviewed candidate selection, association multiplicities, transient versus tracked facts, the dated object scenario, both lifecycles, decision/undo outcomes, and recovery alternatives against the MVP code. This is an analysis review, not independent domain-expert approval. |
| Links and scope | Local Markdown links and anchors passed; all ten images and PlantUML sources are linked from the model. No actor, requirements, or use-case specification was added. |
| Tooling and whitespace | The rendering script executed successfully and passed PowerShell parsing; `git diff --check` passed. |

The [diagram README](requirements/diagrams/domain-model/README.md) records the pinned renderer, checksum, and reproduction command. Tool binaries and visual-review captures remain ignored under `artifacts/`. Runtime, shell, installer, and physical cross-volume tests were not repeated for this documentation-only change; the earlier evidence and remaining release gates are preserved below.

## 2026-10-15: Documentation and screenshots

Environment: Windows x64, .NET SDK 10.0.401. Application source is unchanged from `40b7537`; this update adds documentation and a standalone screenshot renderer.

| Check | Result |
| --- | --- |
| Release build and automated suite | `dotnet test Dowlume/Dowlume.slnx -c Release --no-restore --filter "Category!=WindowsShell"` built the solution and passed all 44 tests, with no failures or skips. |
| Documentation screenshots | `dotnet run --project scripts/DocumentationScreenshots -c Release` compiled and exited successfully. All three 1120 × 820 PNGs were visually inspected: Overview, classification Preview, and Review. |
| Screenshot privacy and provenance | Real Avalonia views rendered with fictional filenames and a demonstration path. The isolated SQLite profile is temporary; no download files are accessed. Curated captures and regeneration instructions are in [images](images/README.md). |
| Formatting and whitespace | `dotnet format --verify-no-changes --no-restore` passed for the application solution and the standalone renderer; `git diff --check` passed. |
| Documentation links | Relative file/image links and heading anchors checked against the repository. |
| Renderer dependency audit | `dotnet list scripts/DocumentationScreenshots/DocumentationScreenshots.csproj package --vulnerable --include-transitive` reported no vulnerable packages from the configured sources. |

Staged screenshots illustrate the interface, not completed filesystem operations. Native shell, installer, clean-machine, and physical cross-volume acceptance checks were not repeated for this documentation update. Earlier evidence and outstanding release gates remain below.

## 2026-10-14: Release-candidate application

Validation date: 2026-10-14. Environment: Windows 11 x64 (build 26200), .NET SDK 10.0.401. All file-operation fixtures used disposable temporary directories; personal Downloads was not used.

| Check | Result |
| --- | --- |
| Release solution build | Passed, zero warnings and errors. |
| Automated suite excluding native shell | 44 passed, zero failures or skips. |
| Native Windows Recycle Bin test | Passed separately, including the verified read handle used by the organizer. |
| Headless desktop workflow | Preview is non-mutating; declined consent preserves source; accepted consent starts monitoring; pause, review, and Keep work. |
| Headless rendering | All five tabs rendered; Overview and Settings captures inspected. |
| Filesystem safety | Collisions, changed content, stale previews, cancellation, locked files, invalid destinations, directory boundary checks, junction rejection, concurrent duplicates, and a 32 MiB file covered. |
| Recovery | Interruptions before move and after move/before state commit, ambiguous copies, and continued processing after one review failure covered. |
| Monitoring | Bounded queue saturation, 40-file bursts, incomplete extensions, rename, and restart discovery covered. |
| Dependency vulnerability audit | No vulnerable packages reported by configured NuGet sources, including transitive packages. |
| Formatting and whitespace | `dotnet format --verify-no-changes` and `git diff --check` passed. |
| Hosted CI | [Build, test, and formatting passed](https://github.com/marcosgonzalezhidalgo/dowlume/actions/runs/38627471880) for application revision `9322acc`, tagged `v0.1.0-rc.1`. |
| Windows package generation | Self-contained x64 publish, portable ZIP, Inno Setup installer, and SHA-256 checksums generated locally. |
| Packaged executable | Final binary started with an isolated profile, refused a second instance using that profile, and exited gracefully. |
| Installer/uninstaller smoke | Per-user installation into an isolated workspace directory and uninstall passed; the isolated profile was retained. |

The native shell test leaves only its own uniquely named sample text file in Windows Recycle Bin. It does not empty the bin. Raw test screenshots and generated packages are ignored under `artifacts/`; test reports are ignored under `TestResults/`. Curated documentation images are versioned separately under `docs/images/`.

On 2026-10-15, implementation PRs #1 through #5 were merged into `main` after their hosted checks passed. The integrated tree at `43992f3` was compared with the tested release branch at `16facb0` and is identical. The tagged source and locally packaged binaries still correspond to application revision `9322acc`; later documentation-only updates record validation and integration history.

The local installer smoke test is not a clean-machine acceptance test. Physical cross-volume interruption, disabled/full Recycle Bin, installer upgrade, release signing, independent review, and public publication remain separate gates in [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md). These are not claimed as completed by the local automated suite.
