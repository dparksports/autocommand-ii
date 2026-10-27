# PORTING.md — upstream sync ledger

AutoCommand II is the rewrite staging repo; `dparksports/autocommand-windows`
(the original) keeps shipping to users until this replaces it. To keep the
two from diverging, after every II release window, diff the original's new
commits and port user-facing fixes/features into the cores here. Log every
port (and every deliberate skip) below.

## Window: 2026-10-27 (baseline + first five II releases)

Baseline: original @ `812fd51` (v2026.10.25.3). Checked through
original master @ that date.

| Upstream change | Status | Where it landed here |
|---|---|---|
| urgent-RDAP feature (uncommitted in original working tree, +425 lines: `RdapService` rewrite + `BlockedRulesWindow` caller) | **QUEUED — port when it lands upstream** | `core/AutoCommand.NetWatch.Core/RdapService.cs` + `Views/BlockedRulesWindow.xaml.cs`. Mirrored edits were mistakenly applied to this repo on 2026-10-27 and removed (commit 2b9c8ef note); the original's own copy was left untouched. Port after the original commits it. |

## How to port

1. `git -C ../autocommand1004 log --oneline <last-sync-HEAD>..HEAD` — list
   upstream changes.
2. For each user-facing fix: apply to the owning core here (paths moved —
   see REWRITE.md's layout), run the harness that covers it, note it above.
3. Docs/notes-only changes: skip with a note.
4. Anything that changes the WPF shell here (Views/, App.xaml.cs): apply
   directly; the shell is shared lineage, not a core.
