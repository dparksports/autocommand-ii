# AutoCommand II v2026.10.27.7 — verify_block_containment: proof that blocks are silent

Blocking a process or IP has always shown you the attempts (the live
who-is-being-blocked feed). What it could not show was whether anything
was actually getting THROUGH. This release closes that loop.

## What is new in this build

- **`verify_block_containment`** (AI bridge tool): one call that proves a
  blocked process is silent, not just still trying. A block hit is a
  connect ATTEMPT — the kernel drops it and nothing logs the outcome — so
  no single source proves silence. This tool joins three: Sysmon-attributed
  attempts, the live kernel socket table, and the TLS-transmissions archive
  since the block was created. Per process instance it returns a verdict:
  - **contained** — attempts only; the block is dropping everything.
  - **stale-flow** — a socket whose owning process predates the block:
    firewall rules never sever flows that were already established, so it
    may still be alive. Sever or restart.
  - **leak** — a socket established after the block, or archived bytes to
    a blocked destination since the block. Escalate.
  Blocks nobody is hitting are listed as quiet. The honest limits are in
  the output: the archive only holds TLS flows over 10 KB, so
  sub-threshold/UDP/DNS traffic needs `trace_analyze`.
- The verdict logic is a pure engine (`BlockContainmentVerifier` in the
  Contain core) with the same identity rules the monitor enforces — a
  reused pid is flagged, never accused. 15 new offline self-test checks
  cover every verdict and guard (43 total in the Contain suite, all pass).

## Verification

All offline self-test suites build and pass against the exact libraries
shipped in this zip. App, AgentProxy and AgentHost build clean in
Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
