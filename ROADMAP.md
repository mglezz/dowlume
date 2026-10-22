# Dowlume roadmap

[English](#) · [Español](docs/es/ROADMAP.md)

**Current state:** I published [v0.3.0 for Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.3.0) on 2026-10-20 after automated checks and my final functional confirmation. I include restricted macOS Apple Silicon/Linux previews as the porting foundation. See the [0.3.0 acceptance record](docs/RELEASE_CANDIDATE_0.3.0.md).

| Stage | Status | Details |
| --- | --- | --- |
| Windows MVP | Implemented | [Completed work](#completed-windows-mvp) |
| Public 0.1.0 release | Published 2026-10-18 | [Completed release gates](#completed-public-010-gates) |
| Public 0.2.0 release | Published 2026-10-20 | [Accepted artifacts](docs/RELEASE_CANDIDATE_0.2.0.md) |
| Public 0.3.0 foundation | Published 2026-10-20 | [Accepted files and preview limits](docs/RELEASE_CANDIDATE_0.3.0.md) |
| Cross-platform releases | Planned 0.4.0 → 0.5.0 → stabilization → 1.0.0 | [Release criteria](#planned-releases-toward-10) |
| Additional product ideas | Unscheduled | [Future work](#future-work) |

## Planned releases toward 1.0

I prioritize safe cross-platform operation after 0.2.0. These versions describe intended scope, not release dates or existing support. I fix defects in every milestone and publish patch releases when needed. I keep Windows working throughout the ports, and allow additional 0.x releases if the acceptance criteria are not met. I use release candidates before promoting new platform packages to stable.

| Target | Intended outcome | Exit criteria |
| --- | --- | --- |
| 0.3.0 | Common platform foundations; Windows remains the supported product | Native CI on Windows, macOS, and Linux; explicit platform capabilities and adapters; reviewed file-operation contracts and path policy; profile compatibility tests; reproducible experimental packages; recorded support matrix and unresolved native risks. Passing a build does not grant production support to a port. |
| 0.4.0 | Supported macOS release within a declared matrix | Native file-safety and recovery acceptance, Finder/folder discovery, permissions and permission denial, desktop lifecycle and optional login startup, English/Spanish UI, and accepted installation and upgrade packages with their signing status documented. I target Apple Silicon only. Intel is outside this roadmap and may be reconsidered in a later release. |
| 0.5.0 | Supported Linux release within a declared matrix | One named Ubuntu LTS release, x64, and GNOME as the initial target; explicit X11/Wayland backend coverage; configured Downloads discovery, permissions, file-manager integration, optional autostart, no-tray fallback, native file-safety acceptance, and an accepted package with documented system dependencies. Other distributions, desktops, and architectures require separate evidence. |
| 0.6.x–0.9.x, if needed | Stabilization and product completion | Resolve cross-platform gaps, migration issues, usability/accessibility defects, and performance regressions. Decide the recovery/quarantine scope explicitly. Address publisher signing and macOS notarization after functional completion across supported platforms. I do not add releases just to fill version numbers. |
| 1.0.0 | A dependable product within its published support contract | No known unresolved data-loss, overwrite, unsafe-recovery, or critical security defects; accepted platform matrix, profile upgrades, recovery/backup instructions, accessibility and both languages, measured performance budgets, supported dependency versions, distribution provenance, and final package acceptance on every supported target. |

“Supported” means the documented feature set works on the named OS versions, architectures, filesystems, and desktop sessions. It does not mean every Linux distribution, every macOS version, or identical native UI conventions. In 0.3.0 I pin the initial matrix against the actual .NET/Avalonia dependencies and available native test environments; adding a target requires acceptance, not only a new runtime identifier. See [platform scope and evidence](docs/PLATFORM_SUPPORT.md).

I integrated and published the 0.3.0 foundation after native automated checks and [final Windows functional confirmation](docs/RELEASE_CANDIDATE_0.3.0.md). Stable macOS/Linux acceptance remains work for 0.4.0/0.5.0.

### In progress: 0.4.0 for Apple Silicon

I have implemented native same-volume APFS operations and recovery, the supported lifecycle and watcher, native menus/reopen/quit, optional login startup, profile migration, both languages and self-contained packaging. I verify races, collisions, permissions and full-volume behavior on both APFS case policies. The extracted package runs a native workflow test. I track evidence and exact-package manual checks in [0.4.0 acceptance](docs/RELEASE_CANDIDATE_0.4.0.md).

My remaining release gate is acceptance of the exact candidate, including installation, real login and permission dialogs; I do not infer these from automated tests. I prioritize functional completion on macOS and Linux while preserving Windows behavior. I defer Windows publisher signing, macOS Developer ID signing and notarization to final cross-platform stabilization; they are not prerequisites for completing or releasing the functional 0.4.0/0.5.0 milestones. I document the actual signing status and installation limitations of each package. Native file safety and acceptance of the exact binaries remain required. Intel remains outside the initial scope.

### First implementation sequence: 0.3.0

I have implemented native CI, capability adapters, production refusal on experimental hosts, fixed 0.2.0 profile regressions and native preview packaging in the 0.3.0 source. I define initial performance investigation thresholds in the [contract](docs/FILE_OPERATION_CONTRACT.md); measured stable-port baselines remain pending. Final package acceptance is separate from implementation.


I deliver this work as focused PRs, with passing checks before each merge:

1. **Native CI and visible coverage:** run shared domain, persistence, monitoring, and headless tests on all three operating systems. Separate Windows-native tests from portable contracts, report unsupported tests explicitly, and prevent early-return tests from being mistaken for native validation. Establish disposable fixture locations compatible with each platform's path rules.
2. **Platform services and capability selection:** isolate folder discovery, reveal-in-file-manager, profile locations, login startup, and desktop/tray behavior behind narrow contracts. Select adapters in the composition root; keep OS checks out of domain rules. Hide or explain unavailable actions, and never hide the only window if no working way to reopen it exists.
3. **File-safety foundation:** define the volume-aware contract and block unaccepted native mutations. I retain the Windows path policy for 0.3.0 and implement native volume-aware resolution together with each port before enabling its file operations; see the [reviewed contract](docs/FILE_OPERATION_CONTRACT.md). Define identity, Unicode/case collision handling, no-overwrite moves, writer/replacement races, links and mount boundaries, metadata preservation, and interrupted cross-volume recovery. Demonstrate contracts on disposable fixtures; refuse unsupported operations rather than advertise the current non-Windows fallback as safe.
4. **Persistence and regression protection:** verify upgrades from 0.2.0 profiles, preserved English/Spanish settings and history, backup/restore procedures, single-instance behavior, and clean shutdown. Moving a profile to another OS is a separate migration problem because stored paths are platform-specific; I do not promise profile portability by copying SQLite files.
5. **Experimental packaging and review:** produce identifiable macOS/Linux test artifacts with source revision, hashes, and limitations; validate launch, native dependencies, and lifecycle. Record OS versions, architecture, filesystem, desktop/backend, test results, and remaining blockers before closing 0.3.0. Experimental packages remain for disposable fixtures until the native safety gates pass.

I keep this first release focused on the porting foundation. Duplicate detection, new classification features, automatic updating, and multi-folder monitoring do not enter it by default. I review the 0.3.0 evidence before opening the stable macOS acceptance gate.

### Quality gates across all milestones

I test preview and cancellation, organization/monitoring, Keep/Snooze/Move, eligible Undo, history, restart, and interrupted-operation recovery in both languages. I include locked/changing sources, destination collisions, low disk space, denied permissions, unavailable volumes, and interrupted copies. I preserve ambiguous copies and block actions whose safety cannot be established.

I establish representative workload sizes and measurable startup, preview, history, memory, and monitoring budgets during 0.3.0, then track regressions on every supported target. I repeat affected Windows checks for shared-code changes; I never treat an old release's acceptance as proof for new artifacts.

Native recycling stays disabled during the ports. Before 1.0 I either ship a separately designed and accepted recoverable quarantine, or explicitly exclude recycling/quarantine from the supported feature set and align the UI and documentation. I do not reactivate an unsafe feature to claim parity. Existing evidence, checkpoints, and failed capacity results remain untouched.

## Completed public 0.1.0 gates

- [x] Earlier candidate clean-machine automated package acceptance (Windows 11 build 26300); retained as historical evidence.
- [x] Integrate containment in `main`, pass hosted checks, and rebuild clean `94ba177` installer/portable with verified hashes; exact-artifact host package and automated first-use checks passed. See the [candidate record](docs/RELEASE_CANDIDATE_0.1.0.md).
- [x] Repeat package/upgrade and automated first-use checks on exact `94ba177` artifacts in a separate clean Windows guest without shared .NET; retain independently retrieved raw reports and hashes.
- [x] Complete human first use, actual sign-in startup, wizard checks, and selected-file Review acceptance: I reported that all human tests passed on 2026-10-18.
- [x] Physical cross-volume move, undo, and interrupted-copy recovery testing on separate NVMe/SATA disks.
- [x] Disable native recycling in v0.1.0 at the UI, organizer, and adapter boundaries after failed capacity probes; preserve evidence without rerunning them.
- [x] Independent review after integration and acceptance checks: explicitly confirmed by me on 2026-10-18.
- [x] Record unsigned initial-release decision; finalize notes and checksums while preserving tested binaries.
- [x] Obtain explicit publication authorization, publish the prepared release, and tag `v0.1.0` at tested commit `94ba177`; public downloads match all six accepted files.

The [release checklist](docs/RELEASE_CHECKLIST.md) is the detailed acceptance record. Local package generation and automated tests do not complete these gates.

I completed the v0.1.0 release gates and published English/Spanish localization in v0.2.0. Earlier clean-guest results do not approve changed binaries. The evidence VM and checkpoints must remain untouched; use a separate environment for final acceptance. The [acceptance kit and procedure](docs/WINDOWS_ACCEPTANCE.md) define acceptance for future packages; macOS/Linux work follows the planned release criteria above.

## Completed Windows MVP

### Milestone 0: Foundation

- [x] C# solution with Avalonia, Core, Infrastructure, Monitoring, and xUnit projects.
- [x] Constructor-based composition, formatting, structured diagnostics, and CI configuration.
- [x] Domain state machine and explicit safety requirements.
- [x] MIT license and contribution conventions.
- [x] Hosted GitHub Actions build/test/format checks verified for the release candidate.

### Milestone 1: Safe detection

- [x] Configurable local Downloads folder with Windows known-folder suggestion.
- [x] FileSystemWatcher creation/change/rename hints.
- [x] Bounded queue, duplicate-event handling, and a single consumer.
- [x] Conservative readiness checks and browser partial-file exclusions.
- [x] Startup/periodic reconciliation and nonrecursive discovery.
- [x] Automated burst, overflow, rename, locked-file, and restart checks.

### Milestone 2: Classification

- [x] Configurable categories and extension matching.
- [x] On-demand category folders, no-overwrite moves, numeric collision suffixes.
- [x] Dry-run preview and explicit consent before monitoring.
- [x] SQLite operation journal and disposable-filesystem integration tests.
- [x] Failure and cancellation behavior preserves existing content.

### Milestone 3: Lifecycle

- [x] Persist first-seen time, category, paths, status, and content hash.
- [x] Configurable category age thresholds.
- [x] Expiring Files holding area.
- [x] Persistent Keep, seven-day Snooze, and explicit Move. Native Recycle is disabled for v0.1.0; historical records remain readable.
- [x] Idempotent processing and protected undo semantics.

### Milestone 4: Desktop experience

- [x] Overview, preview, review, activity, and settings screens.
- [x] Folder pickers and confirmation dialogs.
- [x] Progress, errors, journal details, and recovery controls.
- [x] Tray hide/reopen/exit and orderly shutdown.
- [x] Headless end-to-end workflow and rendering checks.

### Milestone 5: Hardening and release preparation

- [x] Interrupted journal recovery and ambiguous-copy protection.
- [x] Content-change, collision, locked-source, and disabled-recycle regression tests. Earlier native capacity failures remain recorded, not waived.
- [x] Structured path-free diagnostic log with bounded rotation.
- [x] Self-contained Windows ZIP and per-user installer build scripts.
- [x] Usage, architecture, contribution, branch, and release documentation.
- [x] Merge the five implementation PRs into `main`, preserving commit and PR history.

## Development after the MVP

- [x] Search all recorded operations by filename or path, action, and status.
- [x] Browse older history in bounded pages while preserving undo checks and the global attention count.
- [x] Assess [macOS and Linux prerequisites](docs/PLATFORM_SUPPORT.md); implementation and native validation are planned in 0.3.0–0.5.0.

- [x] English/Spanish interface selection, saved per profile, and a parallel Spanish documentation section in the published v0.2.0 packages. I retain the original v0.1.0 binaries unchanged.

## Future work

These additional ideas have no assigned release or date; they do not expand the platform milestones by default.

- Duplicate detection and storage insights.
- Recoverable same-volume quarantine with explicit restore and no eviction, as [proposed in the investigation](docs/RECYCLE_CAPACITY_INVESTIGATION.md#proposed-recoverable-quarantine); not implemented in v0.1.0.
- Richer classification rules and virtual organization.
- Multi-folder simultaneous monitoring.
- Following external renames and explicitly accepting edited tracked content.
- Incremental organizer/recovery queries and indexed history searches for very large folders.
- Additional languages beyond English and Spanish. Accessibility is part of the release gates above.

I keep content-based classification, cloud synchronization, arbitrary scripts, and permanent automatic deletion outside this MVP.
