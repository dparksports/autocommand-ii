# AutoCommand II — rewrite plan

This repository is the AutoCommand rewrite. It began as a clean import of
`dparksports/autocommand-windows` @ `812fd51` (v2026.10.25.3 baseline,
fresh history) so the original repo stays untouched. The original keeps
shipping; everything here lands only when a phase is complete and green.

Two tracks, deliberately sequenced:

1. **UI plumbing fixes** (phase 0 — safe, behavior-preserving, started already)
2. **Core engine extraction** (phases 1+ — the structural rewrite: the ~70
   services leave the WPF exe for `*.Core` class libraries, following the
   in-repo pattern `SvchostAnalyzer.Core` and `OneSettingsCadence.Core`
   already established: one engine, many faces)

## Phase 0 — UI plumbing (in progress)

The measured problems this phase fixes (survey of the v2026.10.25.3 tree):

- All 14 tab views are constructed statically in `MainWindow.xaml`, so any
  timer started in a constructor ticks for the whole session.
- Views that poll with `Task.Run` + `Dispatcher.BeginInvoke` hand-roll the
  same ~40-line block (timer, reentrancy guard, thread hop, selection
  restore) per view.
- Full `ItemsSource = list` swaps regenerate every visual row per tick and
  force hand-coded selection restoration.

Delivered so far:

- **`Helpers/RefreshLoop.cs`** — one owner for timer + visibility gate +
  reentrancy guard + thread hop + single marshal. `RefreshLoop.Start(...)`
  for views built on the loop; `RefreshLoop.GateVisibility(view, () => timer)`
  as the minimal drop-in for a view that keeps its own tick logic.
- **`Helpers/ListSync.cs`** — `SyncByKey`: keyed in-place list sync;
  unchanged rows keep their containers and the user's selection survives
  reordering by construction.
- **PhoneHomeView** converted to both (the exemplar; `TraceCardVM` gained
  the value equality that makes "unchanged row" detectable).
- Visibility gates on the snapshot-poll timers that lacked them
  (TracesView 60 s, BlackHoleView 5 s, SvchostMonitorView block-state 60 s).
- BlackHoleView duplicate-subscription fix (Tick was re-subscribed on every
  tab re-entry, stacking one more RefreshAll per tick per visit).

### What may NOT be visibility-gated (learned the hard way — keep this list)

- `SvchostTraceView._poll` (1 s) — detects spawns by **diffing the live
  process set between ticks**; a paused tick loses the spawns that happened
  while hidden. Self-accumulating, not a snapshot read.
- `SvchostMonitorView._saveTimer` (1 h) — CSV persistence; must run while hidden.
- `SvchostMonitorView._lastSeenRefreshTimer` (10 s) — the row-enrichment pass
  (DNS enqueue, task attribution, svchost identity, archive reveals).
  Catch-up-safe in principle, but not worth the risk in the centerpiece view.
- `DefenderView._cpuDebounce` — a debounce delay, not a poll.
- One-shot timers (banner delays, toast auto-hides, status-text clearers).

Views that already self-gate via `Loaded`/`Unloaded` stop/start (leave them):
SpawnsView, BitsView, DefenderView, TaskHostSpawnView, BeMyGuestView,
TrafficPatternsView.

### Conversion playbook (per view)

1. Move the timer + guard + `Task.Run`/`BeginInvoke` block into
   `RefreshLoop.Start(this, interval, BuildRows, ApplyRows, onBegin: ...)`.
   `BuildRows` runs on the thread pool (no UI access, including
   `SelectedItem` — read selection state inside `ApplyRows`); `ApplyRows`
   runs on the UI thread. Button handlers force a pass with `_loop.Trigger()`.
2. For live tables: keep an `ObservableCollection<T>` field, bind
   `ItemsSource` once in the constructor, and refresh with
   `ListSync.SyncByKey(_rows, fresh, v => v.Key, RowList)`.
3. Give the row VM value equality (Equals/GetHashCode over rendered fields;
   exclude action handles like `TraceCardVM.What`) so unchanged rows are
   skipped entirely.

Next conversions by payoff: TransmissionsView (live feed, 1,068 lines),
SvchostMonitorView row pipeline, TracesView card refresh.

## Phases 1+ — core engine extraction (the structural rewrite)

### Phase 1 — delivered: `core/AutoCommand.NetWatch.Core`

Extracted (physically moved, namespaces deliberately unchanged —
`AutoCommand.Services` / `.Helpers` / `.Models` — so every consumer compiled
without edits):

- Services: TcpConnectionTable, IpClassifier, DnsResolutionService,
  RdapService, SysmonArchiveService, SysmonDiagnosticsService,
  SysmonInstallerService, SysmonProcessAuditService, SysmonWatcherService,
  ProcessGlossary
- Helpers: ProcessRunner
- Models: SvchostMonitorItem, IResolvableHost

Consumers now on the library: the app (ProjectReference),
NetWatchSelfTest (was: source-copying + a FrameworkReference hack — both
gone; ALL CHECKS PASSED against the shipped assembly, including the live
IPv6 loopback enumeration), BitsTraceSelfTest (IpClassifier/Rdap/Dns/
IResolvableHost now from the core; BITS-cluster sources still compiled
locally until their phase).

Deliberately NOT extracted yet (cross-cluster dependencies, move with
their clusters): SysmonConnectWatcherService (needs ImageSpawnWatcherService
— Forensics, and TcpStreamCaptureService — Intercept), BlockHitFeedService
(needs FirewallService — Contain).

**Lesson every future phase must repeat** (cost an afternoon here): a new
ProjectReference to a directory under the repo root is not enough — the
app's default Compile glob silently pulls the new core's `.cs` files into
the app assembly next to the ProjectReference (the repo already fought
this for svchost-watch/AgentProxy). Every extracted core needs its
matching `<Compile Remove="core\**" />` (already in AutoCommand.csproj for
the whole core\ tree — future cores under core\ inherit it for free).
Symptom when forgotten: the WPF _wpftmp markup pass fails with CS0579
duplicate-attribute errors that look like a versioning bug and are not.

Also new: `core/Directory.Build.props` (common settings for core\
projects only — deliberately not at the repo root so vendored engines and
self-test harnesses keep owning their settings).

### Remaining phases

Target layout (all plain `net10.0-windows` class libraries, zero WPF
references, same pattern as the existing vendored engines):

```
Sdk/AutoCommand.Sdk              contracts grow: IAgentInteraction, DTOs
core/AutoCommand.NetWatch.Core   ✔ done (phase 1, see above)
core/AutoCommand.Intercept.Core  sniffer, TLS, proxy/CA, transmissions pipeline
core/AutoCommand.Contain.Core    firewall, sinkhole, blocks, Defender
core/AutoCommand.Forensics.Core  provenance, attribution, PyTrace, Bits, traces
core/AutoCommand.Sitedoctor.Core fingerprint, probe, verdict, dependency blocks
core/AutoCommand.Agent.Core      bridge, tool registry, MCP registration
```

Rules of the road:

- Singleton/static service surfaces become composed instances; keep static
  forwarding shims during migration so views keep compiling, delete per cluster.
- UI coupling inverts through two interfaces in Sdk (`IAgentInteraction`,
  `INotifier`) implemented by the WPF shell.
- Each extraction switches its self-test harness from
  `<Compile Include="..\Services\X.cs" />` source-copying to a single
  `<ProjectReference>` — stronger than byte-identical: it runs the shipped
  assembly.
- Physical file moves (not linked sources) so the csproj `Compile Remove`
  tax does not grow.
- Order: NetWatch first (leaf services, has a self-test), then Forensics,
  Intercept, Contain, Sitedoctor, Agent last (it touches everything).
- A root `Directory.Build.props` centralizes TFM, `$(Version)`, and the
  `10.0.0-preview.4` package pins.
- No MVVM framework adoption, no public renames, no behavior changes: views
  keep their code-behind; the payoff is testability, headless hosting, and
  self-tests as real library consumers.
