# File-operation contract for the ports

[English](#) · [Español](es/FILE_OPERATION_CONTRACT.md)

I use this contract to enable native operations. In 0.4.0 I retain Windows and enable macOS Apple Silicon on local same-volume APFS. Linux stays restricted. I distinguish Windows native exclusion from macOS POSIX race detection and preservation; these are different guarantees.

| Boundary | Required behavior before enabling a port |
| --- | --- |
| Path and identity | Resolve containment using verified directory/file identities and volume behavior. Do not infer case sensitivity from the OS name or normalize Unicode blindly. Reject ambiguous aliases, links, unsupported mounts, cloud placeholders and unavailable volumes. |
| Destination | Never overwrite an existing file, including case/Unicode aliases. Reserve or create destinations atomically; a prior existence check does not protect against replacement. |
| Source | Verify expected content and identity. State and test the native writer/replacement guarantees; detected ambiguity must retain files for inspection. A managed share mode is not a portable exclusion guarantee. |
| Same-volume move | Preserve supported metadata and identity; verify the resulting object. Document filesystem-specific limitations. |
| Cross-volume move | Copy without overwrite, flush and verify content before deleting the verified original. Preserve required timestamps, permissions and extended metadata, or refuse unsupported transfers. |
| Journal and recovery | Persist Pending before mutation. Commit tracked state and completion together. On interruption inspect both paths and identities; preserve ambiguous copies and require attention rather than delete speculatively. |
| Capabilities | Reject unsupported requests before journal entries, directory creation or file mutation. UI disabling alone is insufficient. Preview on a restricted host must not replay recovery. |

## macOS 0.4.0

I resolve existing paths through volume UUID/inode/birth identity and compare missing names conservatively using the opened volume's case policy. The kernel exclusive rename, rather than a string comparison, decides collisions. I retain source/parent descriptors, verify content before and after moving, and journal identity before mutation. I refuse cross-volume transfers before Pending or directory creation.

`flock` is cooperative. An arbitrary POSIX writer or same-user namespace replacement can race the final checks. A source replacement immediately before rename can move the replacement; I detect it afterward and retain both objects, without pathname rollback or deletion. I record the observed source-object path when available. I do not claim mandatory write exclusion, a frozen namespace or safety against hostile same-user modifications. Known cloud paths/attributes are rejected, but only ordinary folders outside all sync services are supported. Same-volume rename preserves the original object's metadata. Recovery requires recorded native identity and content, never hashes alone.

I verify reads/moves on disposable APFS and case-sensitive APFS images, including collisions, writers, directory/source swaps, permission denial, a full volume, cross-volume refusal, native monitoring and lifecycle/recovery across a reopened database. Details and package checks are in [macOS implementation](MACOS_IMPLEMENTATION.md). Shared fixture movers and historical Windows results remain separate evidence.

## Profiles and backups

I keep schema 1 and the Windows profile location. Fixed 0.2.0 fixtures verify both languages, customized rules, tracked files and operation history without generating the fixture from the new serializer. I verify that a closed database backup opens with the same retained data.

For a real backup I close Dowlume completely, copy the entire profile directory (including any remaining SQLite sidecar files), and separately back up the managed files. A database backup does not restore file contents or undo filesystem operations. I restore only with the application closed, first into a disposable same-OS profile using `--data-directory`; I inspect preview and history before enabling monitoring. I never copy a live SQLite file alone or promise cross-OS portability.

## Initial performance budgets

I define these as investigation thresholds for the future accepted native implementations, not measured performance claims: interactive startup within 3 seconds; preview of 1,000 completed 4 KiB local files within 5 seconds; first history page from 10,000 records within 1 second; idle working set below 300 MiB; no sustained idle polling beyond the configured reconciliation interval. I record OS, CPU, storage, build, cold/warm runs and the median of five runs before comparing results. File hashing scales with content size, so large downloads need a separate throughput measurement.

The 0.3.0 native archive smoke test has a 60-second failure timeout and checks window launch/clean exit. That timeout is not a performance benchmark. Stable-port acceptance must add measured baselines, workload regression checks and responsive cancellation before I claim these budgets are met.
