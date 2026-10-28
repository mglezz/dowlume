<div align="center">

# Dowlume

**A little order for your Downloads folder.**

Find what you need. Revisit what you forgot. Decide what stays.

[Download Dowlume](https://github.com/mglezz/dowlume/releases/tag/v0.4.0) · [User guide](docs/user/guide.md) · [Documentation](docs/README.md) · [Español](docs/es/README.md)

[![Build and test](https://github.com/mglezz/dowlume/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/mglezz/dowlume/actions/workflows/build.yml?query=branch%3Amain)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

</div>

Downloads have a way of piling up: a receipt, a photo, an installer you needed once. Dowlume gives them a place to go and helps you come back to the ones you might have forgotten.

It organizes completed downloads into folders, brings older files to a review area, and lets you decide what to keep or move elsewhere. Everything runs on your computer, with no account, cloud sync, or telemetry.

![Dowlume showing organized downloads, category counts, and files ready for review](docs/images/overview.png)

*The Dowlume interface with fictional sample files. [About these screenshots](docs/images/README.md).*

## From a busy folder to a simple routine

| See what will happen | Let things settle into place | Decide when you're ready |
| --- | --- | --- |
| Preview where each file will go before starting. | Sort completed downloads into Documents, Images, Videos, Archives, Installers, and Other. Adjust the rules to suit you. | Older files appear in **Expiring Files**. Keep them, snooze the reminder, or move them somewhere else. |

You choose when monitoring starts. Dowlume leaves existing subfolders alone, keeps a history of its actions, and lets you undo eligible moves. There is no automatic deletion deadline. Recycling is currently disabled.

<details>
<summary><strong>Take a closer look at the preview</strong></summary>

![Dowlume preview showing the destination folder for each sample download](docs/images/classification.png)

You can check the proposed destinations before choosing **Organize and monitor**.

</details>

## Download

Download **Dowlume 0.4.0** for your computer. The runtime is included.

| Platform | Download |
| --- | --- |
| macOS 15+ · Apple Silicon | [Portable](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-osx-arm64-portable.zip) |
| Windows · x64 | [Installer (recommended)](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-win-x64-setup.exe) · [Portable](https://github.com/mglezz/dowlume/releases/download/v0.4.0/Dowlume-0.4.0-win-x64-portable.zip) |

Start with a few sample files in a local folder, open **Preview**, and see whether the setup feels right. The interface starts in English; Spanish is available in Settings.

The macOS app uses an ad-hoc signature and is not notarized; Windows packages are unsigned. Read the [installation guide](docs/user/guide.md#install-and-start) and check the [download checksums](docs/releases/0.4.0.md#download-checksums). macOS file moves require local APFS folders on the same volume. Try the [0.5.0-rc.1 prerelease](https://github.com/mglezz/dowlume/releases/tag/v0.5.0-rc.1) for the native Linux workflow, Ubuntu installer and portable packages. Automated native/package checks passed; [manual Linux acceptance](docs/development/linux-acceptance.md) remains pending before stable release. See [supported platforms](docs/user/platform-support.md) for the full scope.

## A closer look at Dowlume

- [Using Dowlume](docs/user/guide.md) — setup, everyday use, and recovery.
- [What's new](docs/releases/0.4.0.md) — changes in this release.
- [What's next](docs/development/roadmap.md) — the direction of the project.
- [Build and contribute](CONTRIBUTING.md) — work on the C# and Avalonia application.
- [Engineering documentation](docs/README.md#engineering) — architecture, file operations, and validation.

Dowlume is open source under the [MIT license](LICENSE).
