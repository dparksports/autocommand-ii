# AutoCommand v2026.10.29.1 — DNS queries and UDP, visible at last

Two windows open that were dark since the beginning: **what this machine
resolves** and **everything that is not TCP**.

## 🌐 DNS Queries (new tab, WATCH group)

The app's Sysmon config has captured DNS query events since the first
install — this release is the first thing that actually reads them.

- **By domain**: who asks for each hostname, how often, what addresses came
  back (range-classified, blocked-list checked), and **beacon regularity** —
  a domain asked on a metronome gets flagged with its gap variation
  (beacons look like that; scheduled updaters do too).
- **By process**: everything a program resolves — a browser tab and a
  background talker are instantly distinguishable.
- **DoH honesty, built in**: queries to known encrypted resolvers
  (Cloudflare, Google, Quad9, NextDNS…) are flagged 🔒 — that's the moment
  a process switches to DNS no local observer can see. No false confidence.
- Resolved addresses name the Process Monitor rows they belong to.

## UDP joins the Process Monitor

Rows track **every transport seen** against their remote address — a new
**Proto** column shows TCP, UDP, or TCP+UDP — plus a transport lens
(All / TCP / UDP) beside the TX-only lens. A UDP talker can no longer hide
behind a TCP-first label, and pure-UDP endpoints now get rows of their own.
The packet counters already counted UDP datagrams (the raw-IP sniffer is
transport-agnostic by construction); now the labels tell the truth.

## For AI assistants

New read-only **`list_dns_queries`** tool: the same two facets, window and
contains filters, per-address classification and blocked status, gap_cv
regularity — with the DoH blindness note carried in the response.

## Verification

New **DnsFeedSelfTest** suite (20 checks: aggregation, facets, regularity
math, encrypted-resolver flags, result-address parsing, persistence
lifecycle, window/contains filters) added to the CI strict gate; all nine
offline suites green; app, AgentProxy and AgentHost build clean in Debug
and Release.

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
