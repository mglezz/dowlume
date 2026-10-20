# Dowlume 0.3.0 — platform foundation

[English](#) · [Español](https://github.com/mglezz/dowlume/blob/main/docs/es/RELEASE_NOTES_0.3.0.md)

I prepare the shared foundation for macOS and Linux while retaining Windows x64 as the supported product. This version does not claim stable macOS/Linux support. I target Apple Silicon only; Intel Macs are outside this release.

- I run native CI on Windows x64, macOS ARM64, and Ubuntu x64. Windows-only tests report explicit skips elsewhere.
- I separate native file-manager integration, login-startup capabilities, and profile selection from the views. I preserve the Windows profile location and keep experimental profiles separate.
- I remove the unaccepted non-Windows file-move fallback. Experimental hosts can preview folders and change preferences, but cannot organize, monitor, review, undo, or replay recovery. The backend rejects these actions before journaling or touching files.
- I keep the window visible when there is no accepted tray integration. Experimental packages do not offer tray hiding or login startup.
- I read Linux configured Downloads locations as data, without executing a shell. Linux reveal opens the containing folder; macOS reveal uses Finder.
- I preserve case-distinct monitoring hints instead of merging them by case. Native path identity and file-operation acceptance remain release gates for the ports.

## Packages

The Windows installer and portable ZIP include .NET, English/Spanish resources, and both offline guides. Close Dowlume before upgrading from 0.2.0. The Windows packages remain unsigned. I retain settings, files, and history; uninstall retains the profile and user files.

Separate `osx-arm64-preview.zip` and `linux-x64-preview.tar.gz` artifacts are experimental. The macOS bundle is ad-hoc signed only, without Developer ID or notarization. Linux launch is tested with X11 under Xvfb and documented native libraries; desktop XWayland/native Wayland acceptance remains pending. Archive launch smoke tests do not replace manual desktop or clean-machine acceptance. Use disposable sample copies and keep the complete extracted package together.

I record exact source, hashes, and platform evidence alongside the artifacts. Native recycling remains disabled. I do not change historical evidence, enable cloud/network/link support, or promise cross-OS profile import. See the [platform scope](https://github.com/mglezz/dowlume/blob/main/docs/PLATFORM_SUPPORT.md) for the remaining 0.4.0 and 0.5.0 gates.
