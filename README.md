# Dowlume

[English](#) · [Español](docs/es/README.md)

[![Build and test](https://github.com/mglezz/dowlume/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/mglezz/dowlume/actions/workflows/build.yml?query=branch%3Amain)

**Your downloads, under control.**

I build Dowlume as a local-first desktop application for Windows and macOS that organizes completed downloads, moves aging files into a review area, and leaves the final decision with you.

**Status:** I published [Dowlume v0.4.0 for macOS Apple Silicon and Windows x64](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) on 2026-10-23. Download the [macOS app](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-osx-arm64.zip), [Windows installer](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-win-x64-setup.exe) or [Windows portable ZIP](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-win-x64.zip), and verify the [SHA-256 checksums](https://github.com/mglezz/dowlume/releases/download/v0.4.0/SHA256SUMS.all.txt). macOS requires version 15+ and local same-volume APFS; Intel is excluded. The macOS app is ad-hoc signed, without Developer ID/notarization; Windows packages are unsigned.

I prioritize completing macOS and Linux functionality while preserving Windows reliability. I defer publisher signing and macOS notarization to final cross-platform stabilization; see the [current priorities](docs/PLATFORM_SUPPORT.md#functional-priorities-and-deferred-signing). **Native recycling remains disabled**, including internal execution, after Recycle Bin capacity tests failed to preserve files. Keep, Snooze, Move, and eligible Undo remain available. The [investigation and future quarantine design](docs/RECYCLE_CAPACITY_INVESTIGATION.md) retain the failed evidence. I published 0.3.0 after automated checks passed and I confirmed that the final Windows candidate worked correctly on 2026-10-20. See the [release acceptance record](docs/RELEASE_CANDIDATE_0.3.0.md).

I confirmed rc.1 manually on macOS and Windows, then corrected a shutdown race and validated rc.2 through native regression and package checks. I publish the corrected rc.2 application packages without rebuilding them; see the [acceptance record](docs/RELEASE_CANDIDATE_0.4.0.md). Publisher signing remains deferred.

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

I implement the macOS Apple Silicon workflow in 0.4.0: native same-volume APFS moves, monitoring, review, Undo, recovery, desktop integration and migration from the preview profile. Linux remains a restricted preview for 0.5.0. Version 0.4.0 is the current stable release within the documented platform scope. See [0.4.0 notes](docs/RELEASE_NOTES_0.4.0.md), [candidate acceptance](docs/RELEASE_CANDIDATE_0.4.0.md) and the [platform matrix](docs/PLATFORM_SUPPORT.md).

## Languages

I keep English as the default and provide Spanish in v0.2.0. In **Settings → Language**, select **English** or **Español**; the interface changes immediately and remembers the choice for that profile. Pause monitoring first. Folder names, files, rules, and journal records are preserved. The **0.4.0 packages retain the selector introduced in 0.2.0**. I retain the original English-only 0.1.0 assets unchanged. See the [0.2.0 release notes](docs/RELEASE_NOTES_0.2.0.md).

I maintain the [Spanish documentation](docs/es/README.md) alongside these English pages. Screenshots and historical evidence identify the revision they document.

## Technologies

**C# · .NET 10 · Avalonia (MVVM) · SQLite · xUnit**

The desktop UI uses Avalonia; SQLite stores settings, tracked files, and the operation journal. xUnit covers domain rules, filesystem safety, recovery, monitoring, and headless desktop workflows.

## Engineering challenges

- **Knowing when a download is ready:** watcher events trigger checks, not immediate moves. Stable size/timestamps, a write-age margin, and native readiness checks work alongside periodic scans to handle unfinished downloads and missed events.
- **Protecting files during interruptions:** moves never overwrite a destination, content hashes detect unexpected changes, and ambiguous copies are left for inspection.
- **Keeping SQLite and disk consistent:** a Pending journal entry precedes each move; tracked state and completion are then committed together. Recovery checks the actual files before deciding what completed.

See [Architecture](docs/ARCHITECTURE.md#engineering-challenges) for the design, tradeoffs, and test coverage.

## Run from source

Install a **.NET SDK from the 10.0.4xx family** on Windows or macOS Apple Silicon (with Xcode command-line tools). [`global.json`](global.json) sets `10.0.400` with `rollForward: latestPatch`; CI currently uses `10.0.401` within that same feature band.

```powershell
dotnet restore Dowlume/Dowlume.slnx
dotnet run --project Dowlume/Dowlume.App -c Release
```

Choose a disposable sample folder in Settings first, then use Preview. See [developer setup](GETTING_STARTED.md) for build and test commands, or the [user guide](docs/USER_GUIDE.md) for the complete workflow and current limits.

## Packages

Download the macOS app or Windows installer/portable ZIP from [GitHub Releases](https://github.com/mglezz/dowlume/releases/tag/v0.4.0). These packages include the runtime; no separate .NET installation is required. To build from source or prepare future packages, see [packaging instructions](docs/RELEASE_CHECKLIST.md). Local packages in `artifacts/` remain distinct from the published, verified release assets.

## Explore the project

| Document | What you will find |
| --- | --- |
| [Domain model](docs/requirements/DOMAIN_MODEL.md) | Conceptual vocabulary, UML structure, file lifecycles, processes, and business rules |
| [Architecture](docs/ARCHITECTURE.md) | Component diagram, processing flow, recovery, and safety boundaries |
| [Roadmap](ROADMAP.md) | Completed MVP, remaining release gates, and future ideas |
| [macOS implementation](docs/MACOS_IMPLEMENTATION.md) | Native workflows, desktop integration and acceptance gates |
| [Validation](docs/VALIDATION.md) | Dated evidence and outstanding acceptance checks |
| [Contributing](CONTRIBUTING.md) | Contribution checks and [branch policy](docs/BRANCHES.md) |

## License

[MIT](LICENSE). Third-party dependencies retain their respective licenses.
