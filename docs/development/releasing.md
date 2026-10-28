# Release procedure

[Documentation](../README.md) · [Español](../es/development/releasing.md)

Release acceptance applies to exact source revisions and binaries. A successful build, draft release, or earlier acceptance record does not approve a new package. The [release history](../releases/README.md) links the recorded results; the [completed 0.1.0 checklist](../archive/release-checklist-0.1.0.md) is retained as historical evidence.

## Prepare the candidate

1. Set the version in `src/Dowlume.App/Dowlume.App.csproj` and add English notes under `docs/releases/<version>.md`. Put the Spanish translation under `docs/es/releases/<version>.md` and link it from the English notes.
2. Update package quick starts, the user guide, supported platform matrix, and upgrade baseline where behavior or requirements have changed. Keep historical evidence tied to its original revision.
3. Run the [development checks](getting-started.md#build-and-test), dependency audit, and applicable native checks. CI must pass on each supported target. Record skipped checks explicitly.
4. Build from a clean, committed revision. `BUILD-INFO.json` must identify that revision and report `DirtyWorkingTree: false`. Choose a new output directory for every build.

Release titles, bodies, commit messages, and default installation instructions are in English. Link Spanish notes as an optional translation; do not concatenate both languages into the release body.

## Reproducible packaging

### Windows x64

On Windows with the [required SDK](getting-started.md#requirements) and Inno Setup 6:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Release.ps1 -RequireInstaller
```

Use `-InnoCompiler "C:\path\ISCC.exe"` for a custom compiler location and `-OutputDirectory <new-path>` for a custom output directory. Omitting `-RequireInstaller` permits a portable-only build. `-SkipTests` is appropriate only when the same source has already passed the relevant checks; it does not count as validation.

Outputs include the self-contained portable ZIP, installer, acceptance kit, English and Spanish release notes, build metadata, and SHA-256 checksums. Standalone Markdown links point to the source revision. Package filenames retain the published `RELEASE_NOTES_<version>.md` convention; repository notes use `docs/releases/<version>.md`.

### macOS Apple Silicon and Linux candidates

Run on the named native OS and architecture with Python 3.13 and the required SDK:

```sh
python3 scripts/Build-PlatformPreview.py --runtime osx-arm64 --output artifacts/macos-candidate
# On Ubuntu x64:
python3 scripts/Build-Linux.py --output artifacts/linux-candidate
python3 scripts/Test-Linux.py --output artifacts/linux-native --archive artifacts/linux-candidate/Dowlume-0.5.0-linux-x64.tar.gz
```

The macOS build also needs Xcode command-line tools. See [platform dependencies](../user/platform-support.md#evidence-and-packages) for Linux. The script signs macOS bundles ad hoc, archives them, and tests the extracted application with a new disposable profile. It does not publish a release.

Packages include English quick starts, a separately linked Spanish translation, runtime resources, selected offline user guides, and build information. Links to engineering material resolve to the source revision online. Historical source archives and development tools are not distributed. Automated smoke checks do not replace installation, permissions, actual login, or clean-machine acceptance.

## Before public release

- Record the source commit, artifact names, SHA-256 checksums, OS, architecture, filesystem, and test scope in `docs/releases/acceptance/<version>.md`.
- Follow the [Windows acceptance procedure](windows-acceptance.md) for installer upgrades, portable launch, both interface languages, startup, and uninstall. Use a separate [clean guest](hyper-v-acceptance.md); preserve the investigation VM and its checkpoints.
- For macOS, run the [native APFS matrix](getting-started.md#native-development), extracted-package checks, and the desktop and installation checks in the [macOS implementation guide](../architecture/macos.md).
- Verify preview, organization, review decisions, eligible Undo, restart, and interrupted-operation recovery. Test file-preservation boundaries on disposable fixtures. Native recycling stays disabled; retired capacity probes must not run.
- Record manual acceptance separately from automation. Verify the actual signing status and installation limitations. Do not claim support beyond the [platform matrix](../user/platform-support.md).
- Inspect the final notes, included guides, and checksums. Obtain maintainer approval for publication of the exact candidate. Retain the accepted binaries; do not rebuild them during promotion.

## Published release

Tag the accepted source revision. For a stable release, attach only the supported application downloads:

- `Dowlume-<version>-win-x64-setup.exe` — Windows installer.
- `Dowlume-<version>-win-x64-portable.zip` — Windows portable application.
- `Dowlume-<version>-osx-arm64-portable.zip` — macOS portable application bundle.

Use **Installer** and **Portable** as the download labels. Keep build information, JSON reports, validation archives, acceptance kits, and standalone notes/checksum manifests in CI artifacts or the candidate evidence record. Publish application SHA-256 values in the linked release notes. Linux 0.5.0 candidates belong in prereleases until the [Linux acceptance matrix](linux-acceptance.md) is complete. Prereleases may attach the Ubuntu `.deb`, portable `.tar.gz`, checksums and technical evidence alongside the Windows/macOS candidates. Do not mark a prerelease as latest stable. Never rename a validation or acceptance archive to imply that it is a portable application.

Renaming a published application archive must preserve its bytes and accepted hash. Update current download links and document the filename change; historical acceptance tables retain their original names. GitHub also supplies its own source-code archive links.
 Use an English release body that explains the changes, platform scope, and installation requirements, with a link to the Spanish notes. Download the public assets and verify that their hashes match the accepted artifacts.

Update the README download links, release index, user guide, and roadmap after publication. Keep failed or superseded candidate evidence identified as such; it does not become a pass when a later candidate succeeds.
