# AutoCommand II v2026.10.27.9 — containment hardening: the review follow-ups

Small engineering-iteration release: three fixes from the code review of
the containment feature (v2026.10.27.7), nothing user-visible removed.

## What changed

- **`BlockContainmentReader.Take` fails fast instead of hanging.** The
  gatherer blocks (inventory fetch, full process scan, archive read), and
  a direct UI-thread call would deadlock silently. It now throws a clear
  message if called on a UI thread — UI callers use Task.Run (the Blocks
  tab already does).
- **The quiet rule is engine-owned.** "Which blocks are quiet" moved into
  the verifier (`QuietTargets`) so the AI bridge tool and any future
  consumer apply exactly one rule; the tool's inline copy is gone.
  Disabled (paused) rules are never listed as quiet — nothing to verify.
- **The known limits are documented where they live**: the attempt-side
  pid-reuse conflation (can only overcount attempts; can never fabricate
  a leak) is now stated in the engine header rather than living only in a
  review.

## Verification

Contain suite now at 48 offline checks, all passing (three new: quiet
excludes hit IPs, quiet excludes app-blocked sibling paths, paused rules
never listed). All seven offline suites pass against the shipped
assemblies; app, AgentProxy and AgentHost build clean in Debug and
Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
