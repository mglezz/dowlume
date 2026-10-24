# Dowlume roadmap

[Documentation](../README.md) · [Español](../es/development/roadmap.md)

Version 0.4.0 provides the Windows x64 and macOS Apple Silicon workflows within the [published platform scope](../user/platform-support.md). Linux is a restricted preview. The next milestone is native Linux support, followed by cross-platform stabilization.

These milestones describe intended scope, not release dates. Additional 0.x releases may be needed before 1.0. Support expands only after native acceptance; a successful build alone is insufficient.

## Planned releases toward 1.0

| Milestone | Intended outcome | Acceptance criteria |
| --- | --- | --- |
| 0.5.0 · Linux | A supported Linux workflow | One named Ubuntu LTS release, x64, and GNOME initially; explicit X11/Wayland coverage; Downloads discovery, permissions, file-manager integration, optional autostart, no-tray fallback, native file-safety checks, and an accepted package with documented dependencies. |
| 0.6.x–0.9.x · Stabilization, if needed | Close gaps across supported platforms | Profile migration, usability and accessibility, performance regressions, recovery scope, Windows publisher signing, and macOS Developer ID signing and notarization. |
| 1.0.0 | A dependable product within its support contract | No known unresolved data-loss, overwrite, unsafe-recovery, or critical security defects; accepted platform matrix, upgrades, backup/recovery guidance, accessibility, both languages, measured performance budgets, supported dependencies, distribution provenance, and exact-package acceptance on every supported target. |

Windows reliability remains a requirement throughout Linux development. macOS targets Apple Silicon; Intel is outside the current plan. Other Linux distributions, desktops, and architectures require separate evidence.

Publisher signing and notarization remain deferred until functional completion across supported platforms. Every candidate must still document its actual signing status and installation limits.

## Quality gates

Each milestone covers preview and cancellation, organization and monitoring, Keep/Snooze/Move, eligible Undo, searchable history, restart, and interrupted-operation recovery in both languages. File-operation changes require disposable-fixture checks for locked or changing sources, destination collisions, denied permissions, unavailable volumes, low space, and interruption. Ambiguous copies must be preserved.

Performance work must measure startup, preview, history, memory, and monitoring against representative workloads. The [file-operation contract](../architecture/file-operations.md#initial-performance-budgets) defines investigation thresholds; measured native baselines remain pending.

Native recycling stays disabled. Before 1.0, either a separately designed and accepted recoverable quarantine must be implemented, or recycling and quarantine must be explicitly excluded from the supported feature set, with matching UI and documentation. Historical capacity failures remain evidence, not waived checks.

## Shipped milestones

| Version | Delivered scope | Record |
| --- | --- | --- |
| 0.1.0 | Windows organization, review, journaling, recovery, desktop UI, and packaging; native recycling disabled | [Release notes](../releases/0.1.0.md) · [Acceptance](../releases/acceptance/0.1.0.md) |
| 0.2.0 | English/Spanish interface selection and translated documentation | [Release notes](../releases/0.2.0.md) · [Acceptance](../releases/acceptance/0.2.0.md) |
| 0.3.0 | Platform services, native CI, compatibility checks, and restricted macOS/Linux previews | [Release notes](../releases/0.3.0.md) · [Acceptance](../releases/acceptance/0.3.0.md) |
| 0.4.0 | Native same-volume APFS workflow, macOS desktop integration, login startup, preview-profile migration, and self-contained app packaging | [Release notes](../releases/0.4.0.md) · [Acceptance](../releases/acceptance/0.4.0.md) |

The application also includes searchable, filtered, paginated operation history. Dated validation and earlier release gates remain in the [archive](../archive/README.md); they do not approve changed binaries.

## Future work

These ideas are unscheduled and do not expand the Linux milestone by default:

- Duplicate detection and storage insights.
- [Recoverable same-volume quarantine](../archive/recycle-capacity-investigation.md#proposed-recoverable-quarantine) with explicit restore and no eviction.
- Richer classification rules and virtual organization.
- Monitoring multiple folders simultaneously.
- Following external renames and explicitly accepting edited tracked content.
- Incremental organizer/recovery queries and indexed history searches for large profiles.
- Additional interface languages.

Content-based classification, cloud synchronization, arbitrary scripts, and permanent automatic deletion remain outside the current scope.
