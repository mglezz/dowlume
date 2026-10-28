# Getting started with Dowlume

[English](#) · [Español](../es/development/getting-started.md)

## Requirements

- Windows x64 or macOS 15+ Apple Silicon (Xcode command-line tools). Linux 0.5.0 is a native candidate targeting Ubuntu 24.04 LTS x64 and local ext4. Native recycling remains disabled.
- .NET SDK from the **10.0.4xx family**: [`global.json`](../../global.json) sets `10.0.400` with `rollForward: latestPatch`. CI uses `10.0.401`, a compatible patch in the same feature band.
- Inno Setup 6 only when building the installer.

The stack is C#, Avalonia with MVVM, SQLite, System.Threading.Channels, and xUnit. English is the default language for the interface and project communication. See the [repository conventions](repository.md) for layout and localization rules. Python 3.10+ is required for repository checks; native package scripts use Python 3.13 in CI.

## Build and test

```powershell
dotnet restore Dowlume.slnx
dotnet build Dowlume.slnx -c Release --no-restore
dotnet test Dowlume.slnx -c Release --no-build
dotnet format Dowlume.slnx --verify-no-changes --no-restore
python3 scripts/check_repository.py
python3 -m unittest discover -s tests/packaging
dotnet run --project src/Dowlume.App -c Release
```

The solution builds the application, test project, and both development tools. On macOS, set the temporary directory as described below before running filesystem tests.

All tests run without invoking native recycling. The previous native tests and capacity probes are retired; preserve their evidence and do not repeat them. Regression tests verify refusal and file preservation through all disabled paths.

Use a disposable folder for initial UI testing. The real Downloads directory is never used by automated tests. To isolate application state as well, pass `-- --data-directory C:\path\to\test-profile` to `dotnet run`.

## Native development

The macOS workflow operates on local APFS within one volume. Native menus, Finder, migration and packaged login startup accompany the file adapter. Ubuntu 24.04 x64 has the 0.5.0 native ext4 candidate workflow; manual desktop acceptance remains pending. Intel Macs are excluded. See [platform packages and dependencies](../user/platform-support.md).

On macOS, use a real temporary directory inside the repository so safety checks do not traverse the system temporary-directory symlink:

```sh
mkdir -p artifacts/test-tmp
TMPDIR="$PWD/artifacts/test-tmp" dotnet test Dowlume.slnx -c Release
```

Shared tests use a fixture mover outside Windows; explicit native skips do not count as native acceptance. The app preserves the Windows profile location, migrates a closed macOS preview profile on default launch, and starts a stable Linux profile while preserving the old preview profile without migration. See [backup and file-operation limits](../architecture/file-operations.md).

After the Release build, run the disposable APFS matrix with `python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-operations`. Build and exercise the extracted macOS archive with `python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-package`. Output directories must be new. Custom profiles do not register login startup or migrate personal data.

On Linux, build Release and run `python3 scripts/Test-Linux.py --output artifacts/linux-native` as an ordinary user with sudo. This creates a disposable ext4 image and runs native tests there. See the [Linux acceptance plan](linux-acceptance.md) for dependencies and packaged-workflow tests.

## Next steps

Follow [First use](../user/guide.md#first-use) for Settings, Preview, consent, and review decisions. The [domain model](../requirements/domain-model.md) explains the concepts and business rules. Read [Architecture](../architecture/overview.md) before changing detection, file operations, or recovery; it defines the safety boundaries and practical filesystem limits.

## Git and release workflow

Use GitHub Flow: one permanent `main` branch, short-lived feature branches, Conventional Commits, and review plus passing checks before merge. See [Contributing](../../CONTRIBUTING.md) for validation and the [branch policy](branches.md) for the workflow.

The implementation milestones are tracked in [Roadmap](roadmap.md). Packaging and publication are separate: follow the [release checklist](releasing.md), including clean-machine testing and hosted CI. Do not mark those complete solely because local tests pass.
