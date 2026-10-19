# Dowlume roadmap

[English](#) · [Español](docs/es/ROADMAP.md)

**Current state:** [v0.1.0 for Windows x64 is published](https://github.com/mglezz/dowlume/releases/tag/v0.1.0), following automated acceptance, my confirmation of human tests and independent review, and explicit publication authorization on 2026-10-18. Evidence is recorded in [VALIDATION.md](docs/VALIDATION.md).

| Stage | Status | Details |
| --- | --- | --- |
| Windows MVP | Implemented | [Completed work](#completed-windows-mvp) |
| Public 0.1.0 release | Published 2026-10-18 | [Completed release gates](#completed-public-010-gates) |
| Post-MVP | Ideas, not delivery commitments | [Future work](#future-work) |

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

I have completed the v0.1.0 release gates and am adding English/Spanish localization to current source. Earlier clean-guest results do not approve changed binaries. The evidence VM and checkpoints must remain untouched; use a separate environment for final acceptance. The [acceptance kit and procedure](docs/WINDOWS_ACCEPTANCE.md) define acceptance for future packages; Linux/macOS work stays deferred.

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
- [x] Assess [macOS and Linux prerequisites](docs/PLATFORM_SUPPORT.md); native implementations and validation remain deferred.

- [x] English/Spanish interface selection, saved per profile, and a parallel Spanish documentation section in current source. These changes are not part of the published v0.1.0 binaries.

## Future work

These are possible directions after the MVP release, without scheduled versions or dates.

- Duplicate detection and storage insights.
- Recoverable same-volume quarantine with explicit restore and no eviction, as [proposed in the investigation](docs/RECYCLE_CAPACITY_INVESTIGATION.md#proposed-recoverable-quarantine); not implemented in v0.1.0.
- Richer classification rules and virtual organization.
- Multi-folder simultaneous monitoring.
- Following external renames and explicitly accepting edited tracked content.
- Incremental organizer/recovery queries and indexed history searches for very large folders.
- [Linux/macOS platform implementations](docs/PLATFORM_SUPPORT.md), additional languages, and broader accessibility review.

I keep content-based classification, cloud synchronization, arbitrary scripts, and permanent automatic deletion outside this MVP.
