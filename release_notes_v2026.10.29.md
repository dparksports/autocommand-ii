# AutoCommand v2026.10.29 — engineering-hygiene release

Nothing in this release changes what you see or click; it is the
post-cutover technical-debt register worked through to zero, verified by
the same gates as every release.

## What changed under the hood

- **Namespaces now match the architecture.** Each engine library owns its
  namespace (`AutoCommand.NetWatch.Core`, `.Intercept`, `.Contain`,
  `.Forensics`, `.Sitedoctor`, `.Agent`) instead of every engine sharing
  the old app-wide `AutoCommand.Services`. Also surfaced and fixed: two
  libraries had been compiling against a *transitive* NetWatch reference
  — they now carry the explicit reference the design always intended.
- **The DNS service reads in halves.** The geo/org engine (prefix cache,
  throttled ip-api client with backoff, RDAP fallback) moved to its own
  file; the hostname pipeline and the idle reconciler stay in the main
  one. Zero behavior change.
- **The last settable globals are gone.** A dead DNS/pytrace directory
  override with no users was removed, and the trace-projects store now
  follows the injectable-instance pattern the blocks ledger established —
  its "set the directory before first use" ordering trap no longer exists.
- **The offline tool list cannot drift.** The stub list an AI client sees
  when no session is running now carries all 22 tools, and a new
  self-test fails the build if it ever drifts from the tool registry.
- **Line endings normalized** (repository-wide LF policy declared), the
  delivered plan and the closed porting ledger moved to `docs/history/`,
  and a **CI workflow** now builds and runs the offline suites on every
  push — same house pattern as the release workflow.

## Verification

All eight offline suites green against the shipped assemblies; app,
AgentProxy and AgentHost build clean in Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
