# Code quality review — 2026-11-02

[Documentation](../README.md) · [Español](../es/development/code-quality-review.md)

The baseline is `f308eb9`, after the 0.5.0 release. This review examines runtime
layering, domain rules, orchestration, persistence, monitoring, desktop integration,
localization and package acceptance, with regression tests for the changed contracts.
It is a maintainability review, not a formal proof, a complete security audit or a
claim that every possible execution has been verified. Published 0.5.0 binaries
remain tied to their original accepted revision.

The review uses meaningful names, focused responsibilities, explicit error handling,
limited coupling and removal of duplicated knowledge as practical criteria. These
are drawn from Robert C. Martin's [Clean Code](https://www.informit.com/store/clean-code-a-handbook-of-agile-software-craftsmanship-9780132639408)
and his [discussion of names and small functions](https://www.informit.com/articles/article.aspx?p=3222335).
Line counts alone do not establish whether a class has one responsibility.

## Findings addressed

| Finding | Change and verification |
| --- | --- |
| The macOS and Linux package acceptance scenarios duplicated almost their entire implementation. Fixes or added checks could diverge. | One `NativePackageAcceptance` scenario selects native readiness and the report filename explicitly. Language checks, file workflow, monitoring and report writing have named stages. Native package CI exercises both implementations; a unit test refuses execution outside isolated smoke mode. |
| Preview consent compared serialized JSON rather than configuration values. UI decisions depended on serialization details and allocated two documents. | `AppSettings.HasSameValuesAs` and `CategoryRule.HasSameValuesAs` compare values explicitly. Ordinal string comparison and collection order are preserved. Tests cover independent equivalent settings and changed fields; a desktop test verifies that edited rules cannot start organization or open confirmation without a fresh preview. |
| `SqliteStateStore.Open` transferred connection ownership only on success but had no cleanup if opening/configuring failed. | Dispose the connection before rethrowing initialization failures. Successful callers retain their existing `using` ownership. Existing persistence, recovery and history suites exercise successful operations. Failure-path cleanup was reviewed statically; no injected PRAGMA failure is claimed. |
| macOS/Linux startup removal checked that an entry belonged to Dowlume but did not check which installed copy it referenced. | Match the registered executable before removing the entry, consistent with Windows. Native regression fixtures register an old copy, replace it with a new copy, and verify that disabling the old copy preserves the new registration byte-for-byte. |

The shared smoke scenario traverses the actual tab count instead of assuming five
tabs and requires nonempty navigation. Native platform selection happens before any
fixture or database writes. Reports retain their names and automation-only fields.

## Structure retained deliberately

- Core owns lifecycle/classification rules and contracts without a UI or SQLite
  dependency. Infrastructure implements persistence and native file operations;
  Monitoring depends on Core and receives work through callbacks. The application
  composition root constructs concrete services.
- Native adapters retain distinct descriptor, identity, mount and ABI checks. Similar
  text is not sufficient reason to merge Windows, APFS and ext4 contracts. Rechecking
  paths/content around a mutation is part of the race-preservation contract.
- Explicit semaphore acquisition and `finally` release in the organizer make the
  serialization boundary visible. A generic callback wrapper would obscure the
  ordering of cancellation, journal creation and mutation.
- Expected readiness failures return false. Logging avoids exception messages and
  stack traces that may expose paths. Broad catches at UI/background boundaries
  report failures; cleanup catches rethrow. These have different purposes.
- Persisted enum values, schema-1 records, English journal messages and their display
  translations are compatibility contracts. Renaming them for appearance alone
  would require migration and compatibility evidence.

## Remaining design tradeoffs

`MainViewModel` still coordinates settings, commands, refresh and lifetime through
concrete organizer/monitor services. Its size and coupling are the main candidates
for a future feature-oriented extraction; moving methods into partial files alone
would not reduce that coupling. Such an extraction should preserve consent and
shutdown contracts and have an independently justified scope.

Settings records contain mutable lists/arrays and therefore are not deeply immutable.
The new comparison is explicit value comparison, not a change to record equality or
collection ownership. New callers must not mutate settings shared with active
monitoring. An immutable settings model would require profile-serialization and UI
editing compatibility work.

Organizer/recovery still load complete record collections; activity paging bounds
materialized rows but not all JSON query work. These are documented personal-folder
scale limits, not evidence of acceptable performance for millions of entries.

## Validation

Use the [contribution checks](../../CONTRIBUTING.md#validation), shared/native test
matrix and both native package workflows before merging. Windows alone cannot
validate Linux/macOS startup registration or extracted-package behavior. No manual
desktop acceptance or new release is implied by this refactor.
