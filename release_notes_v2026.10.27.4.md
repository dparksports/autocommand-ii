# AutoCommand II v2026.10.27.4 — phase 4: the Forensics core

Same app, same behavior — one more engine family moved out of the UI
executable into its own tested library.

## What moved

Everything that answers "who launched this, what is this program, what has
it been doing": process provenance (the PID Inquiry engine), task
attribution for scheduled-task spawns, the image spawn watcher, the trace
metrics behind Phone Home and Traces, the BITS transfer watcher with its
reason catalog, and the Python instrumentation pair — 17 files, now in
`AutoCommand.Forensics.Core`, the top layer of the core stack
(Forensics → Contain → Intercept → NetWatch).

## Verification

- All six offline self-test suites build and pass against the exact
  libraries shipped in this zip. This phase retired the last three
  source-copying harnesses: every suite now tests the shipped assemblies
  themselves.
- The Python spawn row model lost its last two UI-framework properties
  (they became plain strings the UI converts) — one more guarantee the
  engines can never drag Windows UI code into a future headless host.
- App builds clean in Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway** (open
   source; talks to no one). If the zip will not open or the exe will not
   start, run the `unblock-autocommand.ps1` asset once.

Deleting the folder uninstalls it completely. Apache 2.0. The original
repository (dparksports/autocommand-windows) keeps shipping until this
one replaces it. Remaining rewrite work: the site-doctor core, then the
agent core + headless host (phase 6, the payoff).
