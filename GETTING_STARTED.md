# Getting started with Dowlume

[English](#) · [Español](docs/es/GETTING_STARTED.md)

## Requirements

- Windows x64 for the supported desktop application. Native recycling remains disabled.
- .NET SDK from the **10.0.4xx family**: [`global.json`](global.json) sets `10.0.400` with `rollForward: latestPatch`. CI uses `10.0.401`, a compatible patch in the same feature band.
- Inno Setup 6 only when building the installer.

I use C#, Avalonia MVVM, SQLite, System.Threading.Channels, and xUnit. I keep code identifiers, comments, branch names, and Conventional Commits in English. I maintain English and Spanish UI resources and a [Spanish documentation section](docs/es/README.md). The published 0.2.0 installer and portable ZIP include the language selector.

## Build and test

```powershell
dotnet restore Dowlume/Dowlume.slnx
dotnet build Dowlume/Dowlume.slnx -c Release --no-restore
dotnet test Dowlume/Dowlume.slnx -c Release --no-build
dotnet run --project Dowlume/Dowlume.App -c Release
```

All tests run without invoking native recycling. The previous native tests and capacity probes are retired; preserve their evidence and do not repeat them. Regression tests verify refusal and file preservation through all disabled paths.

Use a disposable folder for initial UI testing. The real Downloads directory is never used by automated tests. To isolate application state as well, pass `-- --data-directory C:\path\to\test-profile` to `dotnet run`.

## Experimental native development

I also build on macOS 15 Apple Silicon and Ubuntu 24.04 x64. These hosts expose folder preview and preferences; production file operations, recovery, tray hiding and login startup are unavailable. Intel Macs are excluded. See [platform packages and dependencies](docs/PLATFORM_SUPPORT.md).

On macOS I use a real temporary directory inside the repository so safety checks do not traverse the system temporary-directory symlink:

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume/Dowlume.slnx -c Release
```

Shared tests use a fixture mover outside Windows; explicit native skips do not count as native acceptance. I preserve the accepted Windows profile location and use separate preview profiles. I document [backup and file-operation limits](docs/FILE_OPERATION_CONTRACT.md).

## Next steps

Follow [First use](docs/USER_GUIDE.md#first-use) for Settings, Preview, consent, and review decisions. The [domain model](docs/requirements/DOMAIN_MODEL.md) explains the concepts and business rules. Read [Architecture](docs/ARCHITECTURE.md) before changing detection, file operations, or recovery; it defines the safety boundaries and practical filesystem limits.

## Git and release workflow

Use GitHub Flow: one permanent `main` branch, short-lived feature branches, Conventional Commits, and review plus passing checks before merge. See [Contributing](CONTRIBUTING.md) for validation and the [branch policy](docs/BRANCHES.md) for the workflow.

The implementation milestones are tracked in [ROADMAP.md](ROADMAP.md). Packaging and publication are separate: follow the [release checklist](docs/RELEASE_CHECKLIST.md), including clean-machine testing and hosted CI. Do not mark those complete solely because local tests pass.
