# AutoCommand v2026.10.28.2 — the unresolved-IP backlog drains itself when you're idle

## What's new

Follows v2026.10.28.1 (every Host cell explains itself; Resolve now /
Retry unresolved; RDAP fallback; machine-wide caches). This release closes
the loop: the addresses that *stay* unresolved — because the machine was
too busy to spend the lookup budget on them — now get resolved
automatically, later, when it costs nothing:

- **Idle reconciliation.** Every placeholder answer is already persisted
  in the hostname cache, so "all unresolved IPs" is simply read back,
  oldest first. When the Sysmon feed has been quiet for **5 minutes**
  (traffic idle — checked every minute), batches of **8 addresses** are
  re-asked through the normal paced pipeline: normal spacing, normal
  backoff, never the urgent path. Two minutes cooldown between batches;
  after **5 attempts** an address rests until its cache entry ages out
  naturally (24 h) — permanently unanswerable addresses don't churn
  forever.
- The moment traffic resumes, the reconciler stands down: batches are
  only sent while the machine stays quiet.
- Nothing new to configure or see — a Host cell tooltip that said
  "throttled — next attempt in ~2 min" now eventually says "resolved",
  on its own, while you weren't using the network.

## Verification

NetWatchSelfTest extended with five idle-reconciliation checks (candidate
collection, busy-refusal, idle batch, queue state, batch size) — all
passing; all eight offline suites green against the shipped assemblies;
app, AgentProxy and AgentHost build clean in Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
