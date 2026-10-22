# macOS implementation toward 0.4.0

[English](#) · [Español](es/MACOS_IMPLEMENTATION.md)

I target macOS 15 on Apple Silicon. Version 0.3.0 remains the published release; this work starts 0.4.0 development and does not enable native file mutations or constitute a new release. Intel remains outside the scope.

## Implemented: native reads and identity

I use `MacFileHandle` in the production preview hash and verified-read paths. I open an absolute path with Darwin `O_NOFOLLOW_ANY`, `O_CLOEXEC` and `O_NONBLOCK`, then inspect the descriptor with `fstat`. This rejects symbolic links in any path component during the open itself and rejects nonregular files without waiting for a FIFO writer. I reject the dataless flag when observed; this is not cloud-provider acceptance or a guarantee against hydration during open.

I capture the device/inode identity, hard-link count, mode, size, modification/change/birth timestamps and flags. I compare metadata after reading and reopen the path to check that it still describes the same object. I omit access time from the comparison because reading can update it. I query `_PC_CASE_SENSITIVE` on an opened descriptor for the native volume tests; I do not normalize Unicode names or infer their identity from strings.

I keep the adapter restricted to macOS 15+ ARM64. I use the Darwin ARM64 `stat` layout from the local SDK and test its meaningful fields against the system `stat` utility. I verify the open flags against Apple's [Darwin definitions](https://github.com/apple-oss-distributions/xnu/blob/main/bsd/sys/fcntl.h) and the layout against [sys/stat.h](https://github.com/apple-oss-distributions/xnu/blob/main/bsd/sys/stat.h).

## Reproducible native checks

I run ten `MacNativeRead` tests: production hashing/native metadata, final/ancestor symbolic links, directories/FIFOs, same-content replacement, ancestor replacement, concurrent writes with restored length/mtime, case/Unicode aliases, hard-link identity, permission denial, and invalid/missing paths. Other platforms explicitly skip these tests.

After a Release build, I run:

```sh
dotnet build Dowlume/Dowlume.slnx -c Release
python3 scripts/Test-MacFileReads.py --output artifacts/mac-file-reads
```

The output directory must be new. I run as an ordinary user on Apple Silicon with Xcode command-line tools and the repository SDK. The script creates two private 256 MiB sparse images, one APFS and one case-sensitive APFS, enables ownership and runs the native tests on each. It never takes a user-supplied volume or Downloads folder. It detaches only its own images and removes only its own temporary directories; a detach failure preserves the fixture and fails the run. It does not force-unmount devices.

I retain per-volume TRX results and `ACCEPTANCE.mac-file-reads.json`, including source commit, dirty-tree status, OS, architecture and SDK. Missing/skipped tests fail acceptance. The macOS CI job runs the same script and uploads its reports alongside the full regression results. Local development checks passed all ten tests on both APFS variants on macOS 26.6.2 ARM64, SDK 10.0.401; the report marks that run as a dirty development build. CI supplies separate macOS 15 evidence for the committed change. Local results do not extend the supported OS matrix.

## Remaining boundaries

These are read checks, not a transaction or a lock against POSIX writers. A writer can still modify content after verification. The checks do not pin ancestor directories, prove containment across mount changes, validate network/cloud storage, or establish a safe rename/delete operation. Hard links are observable aliases, not accepted mutation targets. `PathSafety` still has its existing OS-based comparison. I keep `CanMove` false on macOS and keep organization, review mutations, Undo and automatic recovery disabled.

My remaining implementation sequence is:

1. Define and validate the native mutation protocol: volume-aware containment, writer/replacement protection, atomic no-overwrite destinations, metadata, interrupted operations and journal recovery. Cross-volume operations need independent evidence or explicit refusal.
2. Complete desktop acceptance: permissions and denial, Finder/folder selection, Dock/menu/reopen/quit behavior, optional login startup, both languages and profile migration.
3. Measure performance, build the exact candidate, and accept installation/upgrade and the complete supported workflow before publishing 0.4.0.

I defer Developer ID signing and notarization to final cross-platform stabilization, together with Windows publisher signing. I first complete and validate the application workflows on macOS and Linux while preserving Windows. This distribution work does not block functional 0.4.0 development or release acceptance. I retain ad-hoc signing for macOS development packages and document their actual signing status and installation limitations. I revisit [Developer ID distribution](https://developer.apple.com/developer-id/) in the final distribution phase. I do not remove quarantine or disable Gatekeeper in build scripts or instructions.

I retain the existing Windows implementation and published artifacts. Native recycling remains disabled, and historical evidence and capacity investigations remain untouched.
