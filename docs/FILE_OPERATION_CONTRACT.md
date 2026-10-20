# File-operation contract for the ports

[English](#) · [Español](es/FILE_OPERATION_CONTRACT.md)

I use this contract to decide whether a native mover can advertise `CanMove`. In 0.3.0 only the existing Windows implementation can do so. The production macOS/Linux mover refuses all mutations; a portable `File.Move` call or passing shared orchestration tests is insufficient evidence.

| Boundary | Required behavior before enabling a port |
| --- | --- |
| Path and identity | Resolve containment using verified directory/file identities and volume behavior. Do not infer case sensitivity from the OS name or normalize Unicode blindly. Reject ambiguous aliases, links, unsupported mounts, cloud placeholders and unavailable volumes. |
| Destination | Never overwrite an existing file, including case/Unicode aliases. Reserve or create destinations atomically; a prior existence check does not protect against replacement. |
| Source | Verify expected content and identity. Protect against writers, path replacement and directory swaps throughout the operation. A managed share mode is not a portable exclusion guarantee. |
| Same-volume move | Preserve supported metadata and identity; verify the resulting object. Document filesystem-specific limitations. |
| Cross-volume move | Copy without overwrite, flush and verify content before deleting the verified original. Preserve required timestamps, permissions and extended metadata, or refuse unsupported transfers. |
| Journal and recovery | Persist Pending before mutation. Commit tracked state and completion together. On interruption inspect both paths and identities; preserve ambiguous copies and require attention rather than delete speculatively. |
| Capabilities | Reject unsupported requests before journal entries, directory creation or file mutation. UI disabling alone is insufficient. Preview on a restricted host must not replay recovery. |

I retain the existing Windows path policy and its accepted native implementation in 0.3.0. `PathSafety` still uses OS-based comparison; this is an explicit limitation, not a completed volume-aware implementation. I defer production volume-aware resolution and native movers to the respective port milestones, where I can test them together on APFS variants and ext4. Monitoring now retains case-distinct hints; it does not decide filesystem identity.

I require disposable native tests for competing writers, replacement races, links, mount changes, same/cross-volume collisions, Unicode/case aliases, permissions, full disks, interrupted copies, restart and recovery. Shared fixture movers exercise the journal and workflow only. The five explicit Windows-native tests and existing Windows acceptance remain separate evidence.

## Profiles and backups

I keep schema 1 and the Windows profile location. Fixed 0.2.0 fixtures verify both languages, customized rules, tracked files and operation history without generating the fixture from the new serializer. I verify that a closed database backup opens with the same retained data.

For a real backup I close Dowlume completely, copy the entire profile directory (including any remaining SQLite sidecar files), and separately back up the managed files. A database backup does not restore file contents or undo filesystem operations. I restore only with the application closed, first into a disposable same-OS profile using `--data-directory`; I inspect preview and history before enabling monitoring. I never copy a live SQLite file alone or promise cross-OS portability.

## Initial performance budgets

I define these as investigation thresholds for the future accepted native implementations, not measured performance claims: interactive startup within 3 seconds; preview of 1,000 completed 4 KiB local files within 5 seconds; first history page from 10,000 records within 1 second; idle working set below 300 MiB; no sustained idle polling beyond the configured reconciliation interval. I record OS, CPU, storage, build, cold/warm runs and the median of five runs before comparing results. File hashing scales with content size, so large downloads need a separate throughput measurement.

The 0.3.0 native archive smoke test has a 60-second failure timeout and checks window launch/clean exit. That timeout is not a performance benchmark. Stable-port acceptance must add measured baselines, workload regression checks and responsive cancellation before I claim these budgets are met.
