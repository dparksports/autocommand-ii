// Contain self-test — the blocking/safety path, offline:
//   1. BlocksStore — round-trip persistence against a temp dir: add/upsert/
//      pause/remove, event log, provenance parsing, grouping keys.
//   2. TelemetryRulesService — synthetic catalog beats the built-in seed;
//      file detection + JSON state probe driven entirely by temp files.
//   3. FirewallService — the v2 rule-name builders (the strings the whole
//      block/undo/coverage machinery joins on).
// Nothing touches the real %LOCALAPPDATA% store, the hosts file, netsh, or
// Defender; those halves are exercised by the app itself.
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoCommand.Services;

int failures = 0;
void Check(string name, bool pass, string evidence = "")
{
    Console.WriteLine($"{(pass ? "PASS" : "FAIL")}  {name}{(pass || evidence.Length == 0 ? "" : "  — " + evidence)}");
    if (!pass) failures++;
}

// ─────────────────────────────────────────────────────────────────────
// 1. BlocksStore — the undo ledger every block surface writes through
// ─────────────────────────────────────────────────────────────────────
string storeDir = Path.Combine(Path.GetTempPath(), "contain-selftest-" + Guid.NewGuid().ToString("N"));
try
{
    BlocksStore.DataDirectory = storeDir;   // redirect BEFORE first use
    BlocksStore.Load();
    Check("empty store starts empty", BlocksStore.Records.Count == 0);

    BlocksStore.AddOrUpdate(new BlockRecord
    {
        Kind = "ip", Key = "1.2.3.4", ProcessName = "chrome.exe",
        ClassAtBlock = "Application", Origin = "blocks-tab",
    }, origin: "selftest");
    BlocksStore.AddOrUpdate(new BlockRecord
    {
        Kind = "ip", Key = "5.6.7.8", ProcessName = "svchost.exe",
        ServiceKey = "diagtrack", SvcDisplayName = "Connected User Experiences and Telemetry",
        ClassAtBlock = "Microsoft", Origin = "monitor",
    }, origin: "selftest");
    BlocksStore.AddOrUpdate(new BlockRecord
    {
        Kind = "proc", Key = @"c:\tools\upd.exe", ProcessName = "upd.exe",
        ClassAtBlock = "Application", Origin = "blocks-tab",
    }, origin: "selftest");
    Check("three adds → three records", BlocksStore.Records.Count == 3,
          $"count={BlocksStore.Records.Count}");

    // upsert: same kind+key refreshes in place, never duplicates
    BlocksStore.AddOrUpdate(new BlockRecord
    {
        Kind = "ip", Key = "1.2.3.4", ProcessName = "chrome.exe",
        ClassAtBlock = "Akamai (MS CDN)", Origin = "repair",
    }, origin: "selftest");
    Check("re-block same key upserts (still 3)", BlocksStore.Records.Count == 3);
    Check("upsert refreshed the classifier snapshot",
          BlocksStore.Records.First(r => r.Key == "1.2.3.4").ClassAtBlock == "Akamai (MS CDN)");

    // pause semantics: rules off, record kept
    BlocksStore.SetStatus("ip", "5.6.7.8", "off", origin: "selftest");
    Check("pause keeps the record with Status=off",
          BlocksStore.Records.First(r => r.Key == "5.6.7.8").Status == "off");

    // persistence round-trip: a fresh process state reloads from disk
    BlocksStore.Load();
    Check("reload from disk keeps 3 records", BlocksStore.Records.Count == 3,
          $"count={BlocksStore.Records.Count}");
    Check("reload keeps the pause", BlocksStore.Records.First(r => r.Key == "5.6.7.8").Status == "off");
    Check("store file exists", File.Exists(BlocksStore.StorePath), BlocksStore.StorePath);

    // the event ledger the Blocks tab shows
    Check("event log recorded adds", File.Exists(BlocksStore.EventLogPath) &&
          File.ReadAllLines(BlocksStore.EventLogPath).Count(l => l.Contains("1.2.3.4")) >= 2);

    // remove
    BlocksStore.Remove("proc", @"c:\tools\upd.exe", origin: "selftest");
    Check("remove drops the record", BlocksStore.Records.Count == 2 &&
          BlocksStore.Records.All(r => r.Kind != "proc"));

    // provenance parser: what block rules carry in their description
    Check("provenance: svchost label parses to service key",
          BlocksStore.TryParseServiceFromDescription(
              "[blocked 2026-10-25 10:00 UTC] svchost.exe [Connected User Experiences and Telemetry]",
              out string svcKey, out string svcDisplay) &&
          svcKey == "connected user experiences and telemetry",
          $"key='{svcKey}'");
    Check("provenance: '+2 more' collapses to the primary service",
          BlocksStore.TryParseServiceFromDescription(
              "[blocked x] svchost.exe [DiagTrack+2 more]", out string k2, out _) &&
          k2 == "diagtrack", $"key='{k2}'");
    Check("provenance: non-svchost process rejected",
          !BlocksStore.TryParseServiceFromDescription("[blocked x] chrome.exe [Whatever]", out _, out _));
    Check("provenance: 'no service' rejected",
          !BlocksStore.TryParseServiceFromDescription("[blocked x] svchost.exe [no service]", out _, out _));
    Check("provenance: garbage rejected",
          !BlocksStore.TryParseServiceFromDescription("plain description", out _, out _));

    // grouping: the Blocks tab groups rows by service identity
    Check("group key: ip rows group by service key",
          BlocksStore.ServiceGroupKey(BlocksStore.Records.First(r => r.Key == "5.6.7.8")) == "diagtrack");
}
finally
{
    try { Directory.Delete(storeDir, true); } catch { }
}

// ─────────────────────────────────────────────────────────────────────
// 2. TelemetryRulesService — catalog load + offline evaluation
// ─────────────────────────────────────────────────────────────────────
string assetsDir = Path.Combine(AppContext.BaseDirectory, "Assets");
string rulesPath = Path.Combine(assetsDir, "telemetry_rules.json");
try
{
    Directory.CreateDirectory(assetsDir);
    string detOn  = Path.Combine(Path.GetTempPath(), "contain-selftest-on.json");
    string detOff = Path.Combine(Path.GetTempPath(), "contain-selftest-off.json");
    string detGone = Path.Combine(Path.GetTempPath(), "contain-selftest-gone-" + Guid.NewGuid().ToString("N") + ".json");
    File.WriteAllText(detOn,  JsonSerializer.Serialize(new { sync = true }));
    File.WriteAllText(detOff, JsonSerializer.Serialize(new { sync = false }));

    File.WriteAllText(rulesPath, JsonSerializer.Serialize(new
    {
        rules = new object[]
        {
            new
            {
                id = "selftest-on", appName = "SelfTest ON", category = "test", mechanism = "x",
                scope = "User", riskNote = "none",
                detection = new { type = "file", path = detOn },
                stateProbe = new { type = "json", path = detOn, key = "sync", offValues = new[] { "false" } },
            },
            new
            {
                id = "selftest-off", appName = "SelfTest OFF", category = "test", mechanism = "x",
                scope = "User", riskNote = "none",
                detection = new { type = "file", path = detOff },
                stateProbe = new { type = "json", path = detOff, key = "sync", offValues = new[] { "false" } },
            },
            new
            {
                id = "selftest-undetected", appName = "SelfTest ABSENT", category = "test", mechanism = "x",
                scope = "User", riskNote = "none",
                detection = new { type = "file", path = detGone },
                stateProbe = new { type = "json", path = detGone, key = "sync", offValues = new[] { "false" } },
            },
        }
    }));

    var svc = new TelemetryRulesService();
    Check("catalog file beats the built-in seed", svc.Rules.Count == 3,
          $"count={svc.Rules.Count}");

    var statuses = await svc.EvaluateAllAsync();
    var on  = statuses.First(s => s.Rule.Id == "selftest-on");
    var off = statuses.First(s => s.Rule.Id == "selftest-off");
    var gone = statuses.First(s => s.Rule.Id == "selftest-undetected");
    Check("detected + still on → State=On", on.Detected && on.State == TelemetryState.On,
          $"detected={on.Detected} state={on.State} evidence='{on.Evidence}'");
    Check("detected + off-value → State=Off", off.Detected && off.State == TelemetryState.Off,
          $"detected={off.Detected} state={off.State} evidence='{off.Evidence}'");
    Check("not installed → Detected=false", !gone.Detected, gone.Evidence);

    // seed fallback: with no catalog file the built-in seed rules load
    File.Delete(rulesPath);
    var seedSvc = new TelemetryRulesService();
    Check("missing catalog falls back to the built-in seed",
          seedSvc.Rules.Count >= 2 && seedSvc.Rules.All(r => r.Id.StartsWith("ultralytics")),
          $"count={seedSvc.Rules.Count}");
}
finally
{
    try { File.Delete(rulesPath); Directory.Delete(assetsDir, true); } catch { }
}

// ─────────────────────────────────────────────────────────────────────
// 3. FirewallService — the v2 rule-name builders every surface joins on
// ─────────────────────────────────────────────────────────────────────
Check("ip rule prefix is the v2 shape", FirewallService.NewIpPrefix == "AC-BLOCK-IP ");
Check("app rule prefix is the v2 shape", FirewallService.NewAppPrefix == "AC-BLOCK-APP ");
Check("primary app rule name shape",
      FirewallService.PrimaryAppRuleName("upd.exe") == "AC-BLOCK-APP upd.exe (Out)",
      FirewallService.PrimaryAppRuleName("upd.exe"));

string s1 = FirewallService.SuffixedAppRuleName("upd.exe", @"C:\Tools\upd.exe");
string s2 = FirewallService.SuffixedAppRuleName("upd.exe", @"C:\Tools\upd.exe");
string s3 = FirewallService.SuffixedAppRuleName("upd.exe", @"C:\Tools\v2\upd.exe");
Check("sibling rule name extends the primary", s1.StartsWith("AC-BLOCK-APP upd.exe (Out) · ", StringComparison.Ordinal), s1);
Check("sibling suffix is 8 lowercase hex chars",
      System.Text.RegularExpressions.Regex.IsMatch(s1.Substring(s1.LastIndexOf('·') + 1).Trim(), "^[0-9a-f]{8}$"), s1);
Check("same path → same sibling name", s1 == s2);
Check("different path → different sibling name", s1 != s3);

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL CONTAIN SELF-TESTS PASSED" : $"{failures} CHECK(S) FAILED");
return failures == 0 ? 0 : 1;
