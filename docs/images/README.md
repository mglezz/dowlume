# Documentation screenshots

[English](#) · [Español](../es/images/README.md)

I retain these English screenshots as illustrations of the v0.1.0 interface. They predate the language selector; I do not present them as captures of the localized revision.

These PNGs show the real Avalonia `MainWindow` rendered headlessly on Windows, with staged fictional data. They illustrate separate Overview, Preview, Review, and Activity states; they are not evidence that file operations or native Windows dialogs passed acceptance testing.

| Image | Demonstrates |
| --- | --- |
| `overview.png` | Six managed files, category counts, and two items awaiting review |
| `classification.png` | Six sample downloads mapped to their category folders before organization |
| `review.png` | An archive and installer awaiting a decision, with recycling disabled and explained |
| `activity.png` | Search, action/status filters, pagination, and selected journal paths |

## Regenerate

The 0.6.0 application icon is a geometric D with an inset mint download arrow on a dark-blue rounded tile. Its editable master is [dowlume.svg](../../src/Dowlume.App/Assets/dowlume.svg). Run `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-AppIcon.ps1` on Windows to regenerate the 1024-pixel PNG, 512-pixel Linux theme icon, and Windows ICO with 16, 20, 24, 32, 48, 64, 128 and 256-pixel images. macOS packaging derives its icon set from the full-resolution PNG. No external artwork, fonts or rendering dependencies are required.

From the repository root on Windows with the [required SDK](../development/getting-started.md#requirements):

```powershell
dotnet run --project tools/DocumentationScreenshots -c Release
```

Inspect the four PNGs listed above in `artifacts/documentation-screenshots/`, then copy approved captures into `docs/images/`. The renderer also produces `activity-compact.png` and `review-compact.png` at the minimum window size for layout review; those additional captures stay under `artifacts/`. These curated images are versioned; raw test captures and release outputs stay ignored under `artifacts/`.

The renderer uses the application's views, view model, category rules, and an isolated temporary SQLite profile. Download names and the displayed `C:\Dowlume-Demo\Downloads` path are fictional. No download folder is created, watched, or modified; no organize, move, or recycle commands are invoked. The temporary profile is removed when rendering finishes. Re-run and inspect captures after UI changes, and record the date and application revision in [Validation](../archive/validation.md).
