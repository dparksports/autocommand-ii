# AutoCommand v2026.10.28 — the cutover: AutoCommand II is now AutoCommand

## The move, in plain words

On Oct 25 the app's development moved into a rewrite tree called **AutoCommand
II** (`dparksports/autocommand-ii`) while the original repo kept shipping.
As of today the original repo is frozen at v2026.10.25.4 and **this tree is
the one AutoCommand**: releases, issues and discussions happen here. Nothing
was lost in the move — every user-facing fix the original shipped (including
the RDAP registry-respect work in v2026.10.25.4) is in this build, verified
line-for-line.

## What carries over when you upgrade

Everything, with no action from you. Your blocks, the undo ledger (the
🚑 Restore Internet button), your preferences, traces and the DNS/RDAP caches
all live outside the program folder (`%LOCALAPPDATA%\AutoCommand` and
`C:\ProgramData\AutoCommand`) — unzipping a new folder anywhere and running
it picks them up. **New in this release:** the first run from a changed
folder says exactly that in one dialog, once, so you never have to wonder
whether an update reset anything.

## What's new since the original's last release (v2026.10.25.4)

- **Block containment proof.** Every row on the Blocks tab now shows whether
  a block is actually holding — leak / stale-flow / contained / quiet, with
  the evidence in its tooltip — and AI assistants can ask the same question
  (`verify_block_containment`) and get the same facts.
- **The Investigate tab.** One click (or one `investigate_process` call from
  an assistant) answers "why is this transmitting?" — who started it, where
  it connects, how regularly it beacons — as a plain-English verdict with
  the raw numbers one click away.
- **Be My Guest registrations self-heal.** Moved to a new install folder and
  an assistant's registration points at the old one? The Be My Guest tab
  detects it and offers a one-click refresh — nothing is rewritten without
  your click, same backup-and-verify rules as every registration before.
- **AgentHost, the headless face.** `AgentHost\AutoCommand.AgentHost.exe` in
  the zip runs an AI-access session from a terminal without booting the app.
  It needs the .NET 10 runtime (the main app doesn't — it ships
  self-contained), and it ships with a README that says so right next to the
  exe. It can never change anything: no confirmation dialog exists there, so
  mutating tools are permanently denied.

## For the curious: where the code went

The rewrite's structural work moved every engine out of the WPF exe into six
class libraries (NetWatch, Intercept, Contain, Forensics, Sitedoctor, Agent)
with zero WPF references — same namespaces, same behavior, one engine many
faces. Every offline self-test suite now runs the shipped assemblies rather
than source copies, which is why the verification line below is stronger
than it used to be.

## Verification

All eight offline self-test suites pass against the shipped assemblies
(NetWatch, BITS, PyTrace reassemble, SiteDoctor, TaskAttribution,
ProxyInstall — now including six migration-scan checks — OneSettingsCadence
at 52 checks, Contain at 48). App, AgentProxy and AgentHost build clean in
Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
