# AutoCommand II v2026.10.27.8 — investigate_process: one answer to "why is this transmitting?"

Every investigation of a chatty process used to be ten tool calls in an
order only an expert knows: snapshot, Sysmon tail, transmissions check,
PID inquiry, 24-hour trace, classify each destination, rule out blocks.
This release collapses that playbook into one read-only call — on the
Investigate tab, or from any AI assistant over the bridge.

## What is new in this build

- **`ProcessInvestigationService`** (Forensics core) — joins facts the app
  already records rather than collecting anything new:
  - **PID provenance** (`ProcessProvenanceService`, incl. the PID-reuse
    guard: a recycled PID is flagged, never accused). Enabled when a PID
    is given; best-effort on a live instance when only a name is given.
  - **Traffic profile** (`TraceMetricsService`): frequency, destinations
    with classification, beacon regularity, recorded TLS volume, live
    sockets.
  - **Block hits** from the feed's in-memory cache — deliberately never
    the live Sysmon join, which is the slow path that could exceed an
    interactive call budget.
  - Verdicts describe the numbers without claiming a baseline, in
    priority order: **Actively blocked** → **Quiet** → **Internal-only**
    → **Outbound attempts, none completed** → **Mixed** — each with
    evidence lines. A one-way external profile (SYNs and ClientHellos
    with zero recorded TLS) reads as "attempts", not exfiltration.
- **🔬 Investigate tab** (SYSTEM group, opt-in via Settings → Advanced
  investigation): subject box (name or PID), 6/12/24 h lookback, colored
  verdict banner, monospace report, Copy and Save (markdown under
  `%LOCALAPPDATA%\AutoCommand\reports\`).
- **`investigate_process`** (AI bridge tool): the same service over MCP —
  structured JSON (`provenance`, `traffic`, `block_hits`, `verdict`,
  `evidence`) plus the identical `report_text`. Hours default 6, cap 24;
  the full-day archive scan is the slow path.
- `hours` now types as integer in tool input schemas (this also fixes
  `run_trace`'s schema, which previously declared it a string).

## Verification

Self-tested headless against live archives on the dev machine: the 24 h
`Reolink.exe` run reproduces the known camera-client profile — LAN
discovery broadcasts (UDP 2018/2000 at an 11 s cadence, CV 0.03),
unanswered cloud-relay attempts on port 9999, zero recorded TLS — in
~0.45 s, and a PID run on explorer.exe exercises the provenance half.
App, AgentProxy and AgentHost build clean in Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
