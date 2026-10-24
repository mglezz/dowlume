# Repository guide

[Documentation](../README.md) · [Contributing](../../CONTRIBUTING.md)

## Layout

```text
Dowlume.slnx                 Application, tests, and development tools
src/
  Dowlume.App/               Avalonia UI, localization, and desktop integration
  Dowlume.Core/              Domain records, rules, and contracts
  Dowlume.Infrastructure/    Persistence, native adapters, and file operations
  Dowlume.Monitoring/        Watcher, readiness checks, and reconciliation
tests/
  Dowlume.Tests/             Unit, integration, native, and headless UI tests
tools/
  DocumentationScreenshots/  Fictional-data screenshot renderer
  FileOperationAcceptance/   Disposable-file acceptance harness
scripts/                    Build, validation, and maintenance entry points
packaging/                  Installer definition and package quick starts
docs/
  user/                     Current installation and usage guides
  development/              Setup, workflow, roadmap, and release procedures
  architecture/             Implementation design and operation contracts
  requirements/             Domain model and its diagram sources
  releases/                 Version notes and candidate acceptance records
  archive/                  Historical investigations and validation
  images/                   Curated screenshots and their provenance
  es/                       Spanish translations with the same topic structure
artifacts/                  Ignored local build outputs and validation reports
```

Project directories retain their C# assembly names. Documentation uses lowercase, hyphen-separated filenames; `README.md` identifies an entry point. PowerShell scripts use `Verb-Noun.ps1`. Compiled development utilities belong in `tools/`; runnable maintenance scripts belong in `scripts/`. The root keeps the solution, build configuration, license, contribution entry point, and product README.

## Language and writing

Use English for code identifiers, comments, diagnostics, default UI resources, filenames, branch names, commit messages, pull requests, release titles and bodies, and primary documentation. Conventional Commit subjects describe the change in English.

Keep Spanish documentation under `docs/es/` and link to it from the corresponding English page. Package quick starts follow the same rule under `packaging/es/`. Spanish UI resources and localized OS permission descriptions remain separate from English resources. Never translate persisted enum values, user paths, configured folder names, or journal records.

The README explains the product in everyday language. Technical documentation describes responsibilities, contracts, failure modes, and observed evidence precisely. Avoid author narration in reference material. Historical acceptance records retain attribution, dates, hashes, and the distinction between automated and manual checks.

## Versioned content

Commit source, configuration, maintained documentation, diagram source and reviewed renderings, and curated screenshots with fictional data. Keep generated packages, test reports, databases, raw captures, downloaded tools, and personal editor settings out of Git. `artifacts/` is for local output, not source.

Historical evidence belongs in `docs/archive/` or `docs/releases/acceptance/`. Do not treat old test counts as current results or rerun retired destructive probes. The native recycling snapshots are inert text with recorded hashes; preserve their bytes. They are excluded from distributed packages.

## Changing the layout

Update project references, scripts, installer paths, workflow filters, documentation links, and package inputs together. Run the [development checks](getting-started.md#build-and-test), `python3 scripts/check_repository.py`, and `git diff --check`. Platform-specific acceptance still runs on its native OS.
