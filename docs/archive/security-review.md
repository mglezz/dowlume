# Windows security validation

[English](#) · [Español](../es/archive/security-review.md)

I record the security boundaries, verified checks, and remaining limitations of my implementation here.

**v0.1.0 containment:** native recycling is disabled in the UI, organizer API, internal executor, and compatibility adapter. The distributed application contains no native shell deletion implementation. The clean guest's capacity probes remain failed: an earlier recycled fixture disappeared and the oversized source was not preserved. Preserve all evidence and do not rerun those probes. See the [investigation and proposed quarantine design](recycle-capacity-investigation.md). Automated final-artifact acceptance passed; on 2026-10-18 I confirmed all human tests and independent review, then explicitly authorized publication. [v0.1.0 is published](https://github.com/mglezz/dowlume/releases/tag/v0.1.0) with verified public downloads; these results do not change the historical failures or recover missing contents.

The 2026-10-17 review of two existing runs found no oversized source and no shell-visible bin item in the tester's final inventories. The harness has no file cleanup or alternate restore destination; its explicit incoming-item restore explains the near-capacity source's later reappearance. The complete raw bin inventory contained no payloads; all four surviving metadata records describe restored ordinary/near-capacity fixtures. Permanent deletion and capacity eviction remain the leading native explanations. A separate, hash-verified checkpoint copy was unlocked read-only using its existing BitLocker clear key, allowing first-run filesystem inspection without a recovery password or changes to the evidence VM. Access to the encrypted checkpoint does not establish recovery of the missing contents. Treat this as a failed recoverability invariant, regardless of the shell HRESULT. No architectural replacement is implemented yet.

Run `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Security.ps1` from the repository. The script creates a fresh ignored report directory and fails on incomplete dependency results, vulnerable direct/transitive packages, analyzer warnings, or detected secrets. Gitleaks 8.30.1 is downloaded from its official release and verified against a pinned SHA-256 before execution. Reports redact secret values; CI uploads only report files, not source snapshots or private application profiles.

The final clean candidate source `94ba177` passed fresh local and hosted security checks on 2026-10-18: no vulnerable direct/transitive packages, analyzer warnings, or detected secrets in the source snapshot and 35 local history commits. Exact artifact identities and remaining acceptance are in the [candidate record](../releases/acceptance/0.1.0.md); passing scans alone do not authorize publication.

All .NET security analyzers run during normal builds with warnings treated as errors. The dedicated security workflow additionally scans dependencies and all fetched Git history, plus a snapshot of tracked and unignored source files. Local helper projects are built with the same analyzers. NuGet data and scanner rules can change; rerun before publication. Passing scans do not prove that software is free of vulnerabilities.

## Reviewed boundaries

- SQL statements are selected from a private enum and fixed literals; record values and history filters remain parameters. No local file name or search text becomes executable SQL.
- Explorer uses its absolute Windows path and structured arguments. Native DLL imports explicitly search System32. Startup registration uses a quoted absolute application path in the current user's Run key and starts disabled.
- Windows moves use the verified source handle, reject existing destinations, and hold ancestor directory handles against renames. Cross-volume copies preserve alternate streams, verify the main-stream hash before source removal, and preserve ambiguous copies for recovery.
- Organizer actions are serialized. Regression coverage includes overlapping review decisions, changed previews, interrupted journals, concurrent source replacement, and late destination collisions.
- Recycling is refused before filesystem access or journaling, even if callers bypass the disabled UI command. The organizer no longer receives a recycle adapter. Original COM adapter/sink code is archived as text outside the projects; no configuration or runtime switch can enable it. Tests cover direct calls, locked/changed files, repeated requests, forced UI execution, and historical journal recovery.

## Acceptance and limits

Use the [Windows acceptance procedure](../development/windows-acceptance.md) for final-artifact clean installation, manual organization/undo, disabled-recycle UI, tray overflow, sign-in startup, upgrades, and uninstall. Use fictional fixtures in a separate environment. Do not run disabled/undersized/full native bin probes or modify the preserved evidence VM.

Hyper-V access was initially unavailable on 2026-10-16 and later resolved. The tester subsequently completed clean Windows 11 Pro build 26300 installation and reported passing standard-account package acceptance. Native guest probes then passed ordinary recycle/restore and disabled-bin refusal, but failed the oversized and near-capacity cases. Host and guest checks, the later containment acceptance, and their provenance are recorded in [Validation](validation.md).

Filesystem checks are not a security boundary against another hostile process with the same user's privileges. Native shell recycling and its non-atomic path handoff have been removed from the application execution paths. Hashes cover the main content stream. Process-interruption tests do not simulate power loss or damaged storage. These limits remain after acceptance; passing scans or human tests does not prove absence of every defect.
