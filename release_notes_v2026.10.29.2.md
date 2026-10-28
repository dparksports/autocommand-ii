# AutoCommand v2026.10.29.2 — paused blocks, manageable from the row

A small release with one job: a block you paused no longer strands you
in the Process Monitor.

## What changed

Right-clicking a row whose IP or process block is **paused** (⏸) used to
show a disabled menu item pointing elsewhere. Now the row menu does the
whole job in place:

- **✅ Unblock … — remove the paused block**: deletes the rules for good,
  same confirm dialog as an active block. Removal matches rule *names*,
  not their enabled state, so a paused block is exactly as removable as
  a live one — it just also stops pretending "traffic resumes" when it
  was already flowing (the confirm and the toast now say which case
  you're in).
- **▶ Re-enable block … — traffic stops again** (new item, appears only
  when a block is paused): flips the paused rules back on through the
  same enable path the Blocks tab uses, with the same bookkeeping
  (status + history event). The block action itself could not do this
  job — re-blocking an existing rule only refreshes its provenance and
  would silently not block anything — which is why the item exists
  instead of the old disabled hint.

Process blocks use the same identity recovery as blocking (row → live
module → Sysmon archive), and re-enabling from any recorded path resumes
the whole rule fleet for that exe name — sibling path rules included,
matching how pause and unblock already behave.

Everything else is untouched: active-block unblocking, the Blocked
popup, Manage all, and the 🚑 Restore Internet button work exactly as
before.

## Verification

App builds clean in Debug and Release (0 errors); the change is confined
to the Process Monitor view (XAML menu items + three handlers).

## Install — identical to AutoCommand

1. Unzip anywhere.
2. Right-click `AutoCommand.exe` -> **Run as administrator**.
3. Blue "protected your PC" screen: **More info -> Run anyway**. If the zip
   will not open or the exe will not start, run `unblock-autocommand.ps1`.

Deleting the folder uninstalls it completely. Apache 2.0.
