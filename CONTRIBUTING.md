# Contributing to Dowlume

[Documentation](docs/README.md) · [Español](docs/es/development/contributing.md)

Start with the [development setup](docs/development/getting-started.md) and [repository guide](docs/development/repository.md). Read the [architecture and safety boundaries](docs/architecture/overview.md) before changing file operations.

## Working on a change

Use a short-lived branch from `main`, keep changes focused, and describe the problem and resulting behavior in the pull request. Follow the [branch policy](docs/development/branches.md); do not force-push shared history.

Use English for code, comments, branches, Conventional Commits, pull requests, release titles and bodies, and primary documentation. Spanish documentation lives under `docs/es/` and is reached through links in English pages. Update translated guidance when behavior changes. Shared screenshots, diagrams, and historical evidence have one original copy.

Prefer cohesive methods, immutable domain records, constructor injection, and explicit failure handling. Keep business decisions out of views. Comments should explain constraints or non-obvious choices. Technical reference material should describe the system directly; preserve attribution and the scope of historical evidence.

## Localization

Maintain matching resource keys and format placeholders in `src/Dowlume.App/Localization/Strings.resx` and `Strings.es.resx`. English is the default and fallback for older profiles or unsupported language codes. Translate display labels, not persisted enum values, file paths, configured folder names, or journal data.

## Validation

Run the checks appropriate to the change from the repository root:

```sh
dotnet restore Dowlume.slnx
dotnet build Dowlume.slnx -c Release --no-restore
dotnet test Dowlume.slnx -c Release --no-build
dotnet format Dowlume.slnx --verify-no-changes --no-restore
python3 scripts/check_repository.py
git diff --check
```

On macOS, configure the isolated test directory described in [Getting started](docs/development/getting-started.md#native-development) before running filesystem tests. Native checks require the corresponding operating system; report skipped or unperformed acceptance explicitly.

For dependency changes, also run `dotnet list Dowlume.slnx package --vulnerable --include-transitive`. The Windows security workflow checks dependencies, analyzers, and secrets. Meaningful safety tests are required for changed file-operation behavior; tests should exercise observable contracts rather than mirror implementation details.

Native recycling remains disabled. Do not run historical recycle/capacity probes or alter the preserved evidence VM. All filesystem tests use disposable fixtures. Never point tests at personal Downloads.

For documentation, inspect rendered images and links. Use the [screenshot renderer](docs/images/README.md) with fictional data. Raw test captures can contain personal temporary paths and must be inspected before sharing. Keep release checkboxes tied to observed evidence and follow the [release procedure](docs/development/releasing.md).
