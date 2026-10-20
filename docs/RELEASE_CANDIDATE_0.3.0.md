# Dowlume 0.3.0 — acceptance and publication

[English](#) · [Español](es/RELEASE_CANDIDATE_0.3.0.md)

I published [v0.3.0](https://github.com/mglezz/dowlume/releases/tag/v0.3.0) as the latest stable Windows x64 release on 2026-10-20, at `2026-10-20T16:50:30Z`, using the same 15 verified draft assets without rebuilding or replacing them. Source and tag target: `6b8a33d7b43f8da5889d17dd2506b8c4c7a5e647`, integrated through [PR #29](https://github.com/mglezz/dowlume/pull/29), after the native CI foundation in [PR #28](https://github.com/mglezz/dowlume/pull/28). I retain 0.2.0 and its assets unchanged. macOS/Linux remain restricted experimental previews; Intel Macs remain excluded.

## Final-source evidence

| Check | Recorded result |
| --- | --- |
| [Native build/tests](https://github.com/mglezz/dowlume/actions/runs/39459672054) | Windows: 84 passed, one Linux-native skip. macOS: 79 passed, six native skips. Linux: 80 passed, five Windows-native skips. No failures. Formatting passed. |
| [Security](https://github.com/mglezz/dowlume/actions/runs/39459672727) | Passed. |
| [Windows package](https://github.com/mglezz/dowlume/actions/runs/39459676032) | Passed on `Microsoft Windows NT 10.0.26100.0`: verified 0.2.0 upgrade baseline, portable launch/exit, both resources/guides, language switching and restart persistence, duplicate-instance refusal, tray restore, running-app upgrade/uninstall guards, startup registration retention/removal, profile and sample preservation. |
| [Native archives](https://github.com/mglezz/dowlume/actions/runs/39459680677) | macOS 15.7.9 ARM64 and Ubuntu 24.04 x64: extracted archive launch and graceful exit passed. macOS ad-hoc signature verified before/after extraction; Linux launch used X11 under Xvfb. |
| Provenance | All three build-info files identify `6b8a33d7b43f8da5889d17dd2506b8c4c7a5e647` and `DirtyWorkingTree: false`. I verified archive/package checksums and uploaded asset digests. |

I reviewed the macOS Spanish window and normal shutdown during development and corrected clipped tab labels and preview-only instructions. This is development UI inspection, not final independent desktop acceptance. The native archive reports leave manual desktop and clean-machine acceptance false. Shared tests outside Windows use a test-only fixture mover; production mutations and recovery remain disabled on the ports. See the [platform scope](PLATFORM_SUPPORT.md) and [file-operation contract](FILE_OPERATION_CONTRACT.md).

## Manual acceptance and scope

On 2026-10-20, after receiving the exact candidate and requested checks for upgrade from 0.2.0, preserved settings/history/content, language persistence, organization, review, Undo and tray, I confirmed: “funciona perfectamente sigue adelante”. I recorded this as overall functional acceptance of the new Windows binaries and completed the authorized publication.

I did not record the human tester's Windows build, display scaling or a separate new clean-machine report. The automated package report identifies Windows build 26100 and shared .NET runtimes on its own runner; it does not describe the human environment. I retain that evidence limit and do not present historical clean-guest results as new acceptance. I did not alter the evidence VM/checkpoints or repeat retired recycling probes.

The packaged notes and acceptance kit retain their build-time wording. This publication record supersedes that pending status while preserving the exact accepted files. A future rebuild requires repeating affected checks.

I downloaded all 15 public release assets without authentication and verified their sizes and SHA-256 values against the accepted draft.

## Published files

I retain the Windows checksum list and separate native checksum lists. The published release contains these 15 files; no accepted older asset was replaced.

| File | SHA-256 |
| --- | --- |
| `BUILD-INFO.json` | `19c0366bd1e321858acd2c0532691833519a7d6d823a0e14dc5805ec0b8404f7` |
| `Dowlume-0.3.0-win-x64-setup.exe` | `1e72330b1ed1c662cdbab9fa93511626f736488067d261f07683443a08826cd3` |
| `Dowlume-0.3.0-win-x64.zip` | `bbd6e769a54a6961970237215a4a3d463fe1b8f300515575b8262a3ffeb74c89` |
| `Dowlume-0.3.0-windows-acceptance.zip` | `c418ab9e2fad2af10216ecfbaa0a7078d58a2ee78d65fe529e349ff3b8fa64ad` |
| `RELEASE_NOTES_0.3.0.es.md` | `ba0d35947925b7da53101a6f9048f4fb82d5a580030385332a386e69fb650e3b` |
| `RELEASE_NOTES_0.3.0.md` | `29784970c06eb73728aa821e9cf7b82b707956ec19042785585b6b8d2af10abe` |
| `SHA256SUMS.txt` | `ce80faca44e1a6d9544c441b29842036772dfb97b6164dd91cbef1913298de3b` |
| `ACCEPTANCE.osx-arm64.json` | `f510aba9d30e2eb20bb5c05e7e7e5b929192a825feccb517415cd73f6f3ad3d8` |
| `BUILD-INFO.osx-arm64.json` | `080e375e99e5ef9bb7dc7cf77f0d9bb5836a84c8cb3f2e5e92661466e6b9d14a` |
| `Dowlume-0.3.0-osx-arm64-preview.zip` | `97da16c0124c69ed6aaee7c569c765dc427e49115cf0422d9af87f94e6ba4376` |
| `SHA256SUMS.osx-arm64.txt` | `54365b1b2c6359f937e93fa9a9c86370afa90ed3552f9b63df6544abb0522b24` |
| `ACCEPTANCE.linux-x64.json` | `700b3c6fb493655ef71f801727f6bb5a0ddbd2ee4303eca7f10b12905a7499c0` |
| `BUILD-INFO.linux-x64.json` | `d3aae37537e75812c5f38c4f06b87dec8d9a4d8ab1507c7239555b730fb4128c` |
| `Dowlume-0.3.0-linux-x64-preview.tar.gz` | `36c1fd404d5521281ebb5bc169902c0dbe2cca7eda7ba43f060fe6ac7984e96b` |
| `SHA256SUMS.linux-x64.txt` | `8229cde91f0116220de253632c6f9aa6113de4bfd31d4041407b74c8ef08f380` |
