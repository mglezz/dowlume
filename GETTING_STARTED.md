# Getting started with Dowlume

[English](#) · [Español](docs/es/GETTING_STARTED.md)

## Requirements

- Windows x64 or macOS 15+ Apple Silicon (Xcode command-line tools). I keep Linux as a restricted preview. Native recycling remains disabled.
- .NET SDK from the **10.0.4xx family**: [`global.json`](global.json) sets `10.0.400` with `rollForward: latestPatch`. CI uses `10.0.401`, a compatible patch in the same feature band.
- Inno Setup 6 only when building the installer.

I use C#, Avalonia MVVM, SQLite, System.Threading.Channels, and xUnit. I keep code identifiers, comments, branch names, and Conventional Commits in English. I maintain English and Spanish UI resources and a [Spanish documentation section](docs/es/README.md). The published 0.3.0 installer and portable ZIP include the language selector introduced in 0.2.0.

## Build and test

```powershell
dotnet restore Dowlume/Dowlume.slnx
dotnet build Dowlume/Dowlume.slnx -c Release --no-restore
dotnet test Dowlume/Dowlume.slnx -c Release --no-build
dotnet run --project Dowlume/Dowlume.App -c Release
```

All tests run without invoking native recycling. The previous native tests and capacity probes are retired; preserve their evidence and do not repeat them. Regression tests verify refusal and file preservation through all disabled paths.

Use a disposable folder for initial UI testing. The real Downloads directory is never used by automated tests. To isolate application state as well, pass `-- --data-directory C:\path\to\test-profile` to `dotnet run`.

## Native development

I enable the macOS 0.4.0 workflow on local APFS within one volume. Native menus, Finder, migration and packaged login startup accompany the file adapter. Ubuntu 24.04 x64 remains a restricted preview. Intel Macs are excluded. See [platform packages and dependencies](docs/PLATFORM_SUPPORT.md).

On macOS I use a real temporary directory inside the repository so safety checks do not traverse the system temporary-directory symlink:

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume/Dowlume.slnx -c Release
```

Shared tests use a fixture mover outside Windows; explicit native skips do not count as native acceptance. I preserve the Windows profile location, migrate a closed macOS preview profile on default launch, and retain the separate Linux preview profile. I document [backup and file-operation limits](docs/FILE_OPERATION_CONTRACT.md).

After the Release build, I run the disposable APFS matrix with `python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-operations`. I build and exercise the extracted macOS archive with `python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package`. Output directories must be new. Custom profiles do not register login startup or migrate personal data.

## Next steps

Follow [First use](docs/USER_GUIDE.md#first-use) for Settings, Preview, consent, and review decisions. The [domain model](docs/requirements/DOMAIN_MODEL.md) explains the concepts and business rules. Read [Architecture](docs/ARCHITECTURE.md) before changing detection, file operations, or recovery; it defines the safety boundaries and practical filesystem limits.

## Git and release workflow

Use GitHub Flow: one permanent `main` branch, short-lived feature branches, Conventional Commits, and review plus passing checks before merge. See [Contributing](CONTRIBUTING.md) for validation and the [branch policy](docs/BRANCHES.md) for the workflow.

The implementation milestones are tracked in [ROADMAP.md](ROADMAP.md). Packaging and publication are separate: follow the [release checklist](docs/RELEASE_CHECKLIST.md), including clean-machine testing and hosted CI. Do not mark those complete solely because local tests pass.
