# Historical native recycling sources

These `.txt` files preserve the exact source bytes from the repository before v0.1.0 containment on 2026-10-17. They are reference material only, outside all build projects and release packages. Do not rename, compile, or execute them to repeat the failed probes. `SHA256SUMS.txt` records the archived bytes.

The source was copied from the clean working tree at base commit `2dcc06967adb7cf815d9378fc8845c2c8045bb7e`. Git attributes disable newline conversion for these historical source copies so their recorded hashes survive checkout.

- [WindowsRecycleBin.cs.txt](WindowsRecycleBin.cs.txt): original COM adapter.
- [RecycleOnlyProgressSink.cs.txt](RecycleOnlyProgressSink.cs.txt): original callbacks.
- [Test-WindowsRecycleAcceptance.ps1.txt](Test-WindowsRecycleAcceptance.ps1.txt): original capacity/policy harness.
- [RecycleProbe.cs.txt](RecycleProbe.cs.txt): original worker and immediate report timing.

The [investigation](../../RECYCLE_CAPACITY_INVESTIGATION.md) records the candidate commit, assembly hashes, both runs, VM checkpoints, forensic findings, and limitations. Original binaries, fixtures, reports, exports, and checkpoints remain separate evidence and have not been replaced by this archive. Current script/worker entry points refuse execution; the application adapter also always refuses.
