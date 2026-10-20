# Dowlume

[English](#) · [Español](docs/es/README.md)

[![Build and test](https://github.com/mglezz/dowlume/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/mglezz/dowlume/actions/workflows/build.yml?query=branch%3Amain)

**Your downloads, under control.**

I build Dowlume as a local-first Windows desktop application that organizes completed downloads, moves aging files into a review area, and leaves the final decision with you.

**Status:** [Dowlume v0.2.0 for Windows x64 is published](https://github.com/mglezz/dowlume/releases/tag/v0.2.0). Download the [installer](https://github.com/mglezz/dowlume/releases/download/v0.2.0/Dowlume-0.2.0-win-x64-setup.exe) or [portable ZIP](https://github.com/mglezz/dowlume/releases/download/v0.2.0/Dowlume-0.2.0-win-x64.zip), and verify the [SHA-256 checksums](https://github.com/mglezz/dowlume/releases/download/v0.2.0/SHA256SUMS.txt). These packages are unsigned. See [validation](docs/VALIDATION.md) and the [release checklist](docs/RELEASE_CHECKLIST.md).

I continue to focus on Windows release reliability. **Native recycling remains disabled**, including internal execution, after Recycle Bin capacity tests failed to preserve files. Keep, Snooze, Move, and eligible Undo remain available. The [investigation and future quarantine design](docs/RECYCLE_CAPACITY_INVESTIGATION.md) retain the failed evidence. I published 0.2.0 after automated checks passed and I confirmed that all functionality worked correctly in the final Windows package on 2026-10-20. See the [release acceptance record](docs/RELEASE_CANDIDATE_0.2.0.md).

[Español](docs/es/README.md) · [User guide](docs/USER_GUIDE.md) · [Developer setup](GETTING_STARTED.md) · [Roadmap](ROADMAP.md) · [Contributing](CONTRIBUTING.md)

![Dowlume Overview showing six managed files, category counts, and two files awaiting review](docs/images/overview.png)

*The real Avalonia interface, rendered with fictional sample data. [Screenshot provenance and regeneration](docs/images/README.md).*

## Why Dowlume

Downloads folders accumulate useful documents, one-off installers, and files that are easy to forget. I built Dowlume to turn that clutter into a reviewable workflow: organize completed downloads, bring aging files back to your attention, and keep the final decision with you. I focus on making file automation recoverable when downloads, disk operations, or the application are interrupted.

## At a glance

| Preview | Organize | Review | Recover |
| --- | --- | --- | --- |
| Inspect proposed destinations before consenting. | Classify completed downloads into six configurable categories. | Keep, Snooze, or Move aging files in **Expiring Files**. | Inspect the activity journal, undo eligible moves, and resolve interrupted actions. |

![Dowlume Preview mapping sample PDFs, photos, videos, archives, and installers to category folders](docs/images/classification.png)

*Classification preview with sample data: inspect where each file will go before selecting **Organize and monitor**.*

There is no automatic permanent deletion, online account, cloud sync, or telemetry. Monitoring starts paused on launch. Existing subfolders are not imported automatically.

Activity now includes searchable history with action/status filters and pagination, so older operations remain accessible. See [finding an earlier action](docs/USER_GUIDE.md#find-an-earlier-action).

Windows x64 remains the supported target. The 0.3.0 source adds native CI, platform adapters and restricted previews for macOS 15 Apple Silicon and Ubuntu 24.04 x64. File mutations remain disabled on these previews. I plan accepted macOS support in 0.4.0, Linux in 0.5.0, and stabilization toward 1.0. These are gated milestones, not existing support or promised dates; see the [roadmap](ROADMAP.md#planned-releases-toward-10) and [platform scope](docs/PLATFORM_SUPPORT.md).

## Languages

I keep English as the default and provide Spanish in v0.2.0. In **Settings → Language**, select **English** or **Español**; the interface changes immediately and remembers the choice for that profile. Pause monitoring first. Folder names, files, rules, and journal records are preserved. The **0.2.0 installer and portable ZIP include the selector**. I retain the original English-only 0.1.0 assets unchanged. See the [0.2.0 release notes](docs/RELEASE_NOTES_0.2.0.md).

I maintain the [Spanish documentation](docs/es/README.md) alongside these English pages. Screenshots and historical evidence identify the revision they document.

## Technologies

**C# · .NET 10 · Avalonia (MVVM) · SQLite · xUnit**

The desktop UI uses Avalonia; SQLite stores settings, tracked files, and the operation journal. xUnit covers domain rules, filesystem safety, recovery, monitoring, and headless desktop workflows.

## Engineering challenges

- **Knowing when a download is ready:** watcher events trigger checks, not immediate moves. Stable size/timestamps, a write-age margin, and an exclusive-open probe work alongside periodic scans to handle unfinished downloads and missed events.
- **Protecting files during interruptions:** moves never overwrite a destination, content hashes detect unexpected changes, and ambiguous copies are left for inspection.
- **Keeping SQLite and disk consistent:** a Pending journal entry precedes each move; tracked state and completion are then committed together. Recovery checks the actual files before deciding what completed.

See [Architecture](docs/ARCHITECTURE.md#engineering-challenges) for the design, tradeoffs, and test coverage.

## Run from source

Install a **.NET SDK from the 10.0.4xx family** on Windows. [`global.json`](global.json) sets `10.0.400` with `rollForward: latestPatch`; CI currently uses `10.0.401` within that same feature band.

```powershell
dotnet restore Dowlume/Dowlume.slnx
dotnet run --project Dowlume/Dowlume.App -c Release
```

Choose a disposable sample folder in Settings first, then use Preview. See [developer setup](GETTING_STARTED.md) for build and test commands, or the [user guide](docs/USER_GUIDE.md) for the complete workflow and current limits.

## Packages

Download the installer or portable ZIP from [GitHub Releases](https://github.com/mglezz/dowlume/releases/tag/v0.2.0). Both include the runtime; no separate .NET installation is required. To build from source or prepare future packages, see [packaging instructions](docs/RELEASE_CHECKLIST.md). Local packages in `artifacts/` remain distinct from the published, verified release assets.

## Explore the project

| Document | What you will find |
| --- | --- |
| [Domain model](docs/requirements/DOMAIN_MODEL.md) | Conceptual vocabulary, UML structure, file lifecycles, processes, and business rules |
| [Architecture](docs/ARCHITECTURE.md) | Component diagram, processing flow, recovery, and safety boundaries |
| [Roadmap](ROADMAP.md) | Completed MVP, remaining release gates, and future ideas |
| [Validation](docs/VALIDATION.md) | Dated evidence and outstanding acceptance checks |
| [Contributing](CONTRIBUTING.md) | Contribution checks and [branch policy](docs/BRANCHES.md) |

## License

[MIT](LICENSE). Third-party dependencies retain their respective licenses.
