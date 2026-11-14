# Settings safety review — 0.7.1

Scope: settings presentation, persistence, classification validation, startup consent and optional automation. This review does not establish that filesystem operations are risk-free.

| Setting | Behavior and retained protection | Decision |
| --- | --- | --- |
| Download root and classification | Absolute local root checks; portable unique destination names; duplicate extensions/patterns rejected; ambiguous name matches stop classification without moving that file. Changes invalidate consent until previewed and confirmed. | Retain behind paused-monitoring guard. |
| Custom rule removal | Removes configuration only. Existing files/history retain their recorded identity and fallback review information. | Retain; disclose editing scope. |
| Review periods | Bounded to 1–3650 days. Aging moves files into review; it does not delete them. Recycling remains disabled. | Retain. |
| Periodic organization | Opt-in; 1–1440 minutes; first pass waits; passes do not overlap and shutdown drains them. Future completed arrivals are included in the start confirmation. | Retain in Automation. |
| Startup resumption | Off by default; requires saved preview/start consent. Changed organization rules revoke consent. Explicit Off disables resumption; closing preserves it. | Retain separately from Windows login registration. |
| Whole-folder collection | Off by default; independent interval of 20–1440 minutes; two stable observations; bounded traversal; excludes protected destinations, links, hidden/system contents, partial/excluded downloads and inaccessible trees. Atomic exclusive move never merges or overwrites. Interrupted moves need manual inspection and are not replayed automatically. | Retain as advanced opt-in with the path-change warning in settings **and** start confirmation. |
| Theme and language | Immediate isolated profile writes preserve unsaved organization edits. Theme write failures restore the last saved selection. No file mutation. | Allow while monitoring; disable during an active command. |
| Login preferences | Per-user registration and independent saved presentation preference; background by default. | Retain in Application. |
| Updates | Explicit request; stable releases only; installer checksum verification and confirmation; monitors drain before exit. Portable/native package installation guidance remains platform-specific. | Retain in Application. |

The material residual risk is whole-folder collection: moving a working directory changes its path even if its contents remain intact. Metadata stability and sharing checks cannot guarantee inactivity, especially with POSIX processes that ignore sharing locks. No automatic folder undo is promised. Completed-download-only scope and this limitation are now visible before activation; working directories must stay outside the monitored root. Enabling a setting still does not bypass preview and start confirmation.

Popup regressions use actual open ComboBox controls, not only view-model labels. Navigation tests also verify that selecting a name rule persists that rule without enabling monitoring or folder collection. Existing tests exercise settings conflicts, changed-rule consent revocation, minimum folder delay, partial/busy/protected directories, collisions and interrupted folder recovery. Native filesystem guarantees remain limited to the [documented platforms](../user/platform-support.md).
