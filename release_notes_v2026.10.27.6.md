# AutoCommand II v2026.10.27.6 — phase 6: the agent core, the contracts, and the headless host

The rewrite's structural work is complete. Same app, same behavior — but
the last engine family (the Be My Guest AI-access stack) now lives in its
own library, and the app's confirmation seam is a formal contract.

## What is new in this build

- **AgentHost**: a headless face for AI-access sessions. Scripts and
  unattended runs can now host an MCP session without the WPF application
  starting at all (zip: AgentHost\AutoCommand.AgentHost.exe; run
  `AutoCommand.AgentHost [seconds]` — it answers questions only, and every
  mutating tool is denied by design; it also refuses to run alongside the
  interactive app, by the same single-instance rule).
- The per-action confirmation guarantee ("nothing is touched without your
  yes") is now an explicit contract — Sdk/IAgentInteraction — implemented
  by the app's dialog. Behavior identical; the guarantee is now structural.

## Verification

All seven offline self-test suites build and pass against the exact
libraries shipped in this zip. The last source-copying harness is retired —
every suite tests the shipped assemblies. App builds clean in Debug and
Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
