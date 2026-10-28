# AutoCommand v2026.10.28.1 — unresolved IPs, explained and fixable

## Why some IPs never resolved, and what changed

The Host column's name/org lookups are rate-budgeted by design — one
ip-api.com answer per /24 block, throttled at the free tier, with backoff
after throttling — and that budget meant some rows sat blank with no
explanation. Three changes fix that, without loosening the throttling:

- **Every Host cell now explains itself.** Hover it: *resolving — queued
  (N waiting)*, *throttled — next attempt in ~2 min*, *no owner data —
  negative-cached, retries after 14:00*, *local address — no lookup
  exists*, or *resolved (when)*. The blank is gone.
- **You can act.** Right-click a row → **Resolve now** re-asks that one
  address urgently (it skips the queue spacing; the provider's cooldowns
  still apply). The new **⟳ Retry unresolved DNS** button forgets every
  cached "no answer" result and re-queues the view's unresolved rows at
  normal polite pacing.
- **More IPs resolve at all.** When ip-api has no answer — or is
  throttled — the lookup now falls back to the **RDAP registries** (the
  same authoritative-registry engine the Blocked tab uses: IANA-routed,
  own cache, own pacing). Org labels that come from RDAP are marked
  "· RDAP" in the row.

## The caches moved (so they stop resetting themselves)

The hostname, org-prefix and forward-DNS caches now live machine-wide in
`C:\ProgramData\AutoCommand` beside the RDAP cache, instead of beside the
exe — so unzipping a new build or moving the folder no longer starts every
lookup from cold. An existing beside-the-exe cache is copied over once on
first run.

## For AI assistants

New read-only **`lookup_ip`** tool: hostname, classification, org/geo
*with its cache state* ("negative-cached, retries after 14:00"), and
blocked-list status in one call — `force: true` is the user-initiated
urgent path.

## Verification

NetWatchSelfTest extended with seven resolve-state checks (all passing);
all eight offline suites green against the shipped assemblies; app,
AgentProxy and AgentHost build clean in Debug and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
