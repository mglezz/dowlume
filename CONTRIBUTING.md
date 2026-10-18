# Contributing

Use English for code, comments, documentation, UI, branches, and commit messages.

Keep changes focused. Use domain names, cohesive methods, immutable records, constructor injection, and explicit failure handling. Keep business decisions out of views, and use comments to explain constraints or non-obvious safety decisions. Read the [architecture and safety boundaries](docs/ARCHITECTURE.md) before changing file operations.

## Validation

For code changes, run the build and tests in [Getting started](GETTING_STARTED.md#build-and-test), then:

```powershell
dotnet format Dowlume/Dowlume.slnx --verify-no-changes --no-restore
dotnet list Dowlume/Dowlume.slnx package --vulnerable --include-transitive
```

Native recycling is disabled for v0.1.0. Run the full regression suite; do not run historical recycle/capacity probes or modify the preserved evidence VM. All filesystem tests use temporary directories; never point tests at personal Downloads. A future quarantine implementation requires separate design and acceptance review.

For documentation changes, check local links, image rendering, and `git diff --check`. Use the [screenshot renderer](docs/images/README.md) for publishable images with fictional data. `DOWLUME_TEST_SCREENSHOTS` captures all five screens during the desktop workflow test, but those captures can contain personal temporary paths and must be inspected before sharing.

## Git workflow

Create a short-lived topic branch from current `main`, make focused Conventional Commits, and open a PR. Merge after review and passing CI; never force-push shared history. The [branch policy](docs/BRANCHES.md) contains the commands and merge conventions.

Meaningful safety tests are required for changed file-operation behavior. Do not add tests that only mirror property getters or implementation details. Keep all release checkboxes tied to observed evidence.
