using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Runs;

namespace USCE.Scripts.Diagnostics;

/// <summary>Bounded, local-only observations in the game's normal log. Never changes localization.</summary>
internal static class LanguageDiagnostics
{
    private const int EventLimit = 64;
    private const int StackLimit = 12;
    private const int KeyLimit = 512;
    private static readonly object Gate = new();
    private static readonly string[] Tables = ["cards", "powers", "enchantments", "static_hover_tips", "card_keywords"];
    private static readonly Dictionary<string, string> LastText = [];
    private static readonly Dictionary<string, (bool Title, bool Description)> LastUi = [];
    private static readonly HashSet<string> MissingReported = [];
    private static readonly Dictionary<string, string[]> BaselineKeys = [];
    private static readonly FieldInfo? Translations = AccessTools.Field(typeof(LocTable), "_translations");
    private static int _events, _stacks;
    private static bool _limited, _modelsReady, _installed;
    [ThreadStatic] private static bool _emitting;

    internal static void Install()
    {
        if (_installed) return;
        _installed = true;
        var harmony = new Harmony("sts2.usce.language-diagnostics");
        // Manual installation keeps diagnostic compatibility failures out of gameplay PatchAll.
        Patch(harmony, typeof(LocManager), "SetLanguageInternal", nameof(BeforeLanguage), nameof(AfterLanguage));
        Patch(harmony, typeof(ModelDb), nameof(ModelDb.Init), null, nameof(AfterModels));
        Patch(harmony, typeof(LocTable), nameof(LocTable.GetRawText), null, nameof(AfterRead));
        Patch(harmony, typeof(LocTable), nameof(LocTable.MergeWith), nameof(BeforeMerge), null);
        Patch(harmony, typeof(NCard), nameof(NCard.UpdateVisuals), null, nameof(AfterUi));
    }

    private static void Patch(Harmony harmony, Type type, string name, string? prefix, string? postfix)
    {
        try
        {
            var target = AccessTools.Method(type, name) ?? throw new MissingMethodException(type.FullName, name);
            harmony.Patch(target,
                prefix: prefix == null ? null : new HarmonyMethod(typeof(LanguageDiagnostics), prefix) { priority = Priority.First },
                postfix: postfix == null ? null : new HarmonyMethod(typeof(LanguageDiagnostics), postfix) { priority = Priority.Last });
        }
        catch (Exception ex) { Emit("hookUnavailable", new { target = type.Name + "." + name, error = ex.GetType().Name }); }
    }

    private static string? Language() => LocManager.Instance?.Language;
    private static string Network()
    {
        try { return RunManager.Instance?.IsInProgress == true ? RunManager.Instance.NetService.Type.ToString() : "no-run"; }
        catch { return "unknown"; }
    }

    private static void Emit(string kind, object detail, bool stack = false)
    {
        if (_emitting) return;
        _emitting = true;
        try
        {
            lock (Gate)
            {
                if (_events >= EventLimit)
                {
                    if (!_limited)
                    {
                        _limited = true;
                        Log.Info("[USCE.Language] sessionLimitReached; further diagnostic records suppressed");
                    }
                    return;
                }
                _events++;
                // Method names only: no local source paths, platform IDs, or save data.
                var callStack = stack && _stacks++ < StackLimit
                    ? new StackTrace(2, false).GetFrames()?.Take(20).Select(f => {
                        var m = f.GetMethod();
                        return m?.DeclaringType?.FullName + "." + m?.Name;
                    }).ToArray() : null;
                Log.Info("[USCE.Language] " + JsonSerializer.Serialize(new {
                    schema = 1, sequence = _events, kind, language = Language(), network = Network(),
                    thread = System.Environment.CurrentManagedThreadId, detail, stack = callStack
                }));
            }
        }
        catch { /* Diagnostics must never interrupt game logic or rendering. */ }
        finally { _emitting = false; }
    }

    private static bool Active => !_limited;
    private static string Short(string text) => text.Length <= 180 ? text : text[..180] + "…";
    private static bool HasCjk(string text) => text.Any(c => c is >= '\u3400' and <= '\u9fff');
    private static string Fingerprint(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];

    private static void BeforeLanguage(LocManager __instance, string language)
    {
        try { if (Active) Emit("languageRequested", new { from = __instance.Language, to = language }, true); }
        catch { }
    }

    private static void AfterLanguage(LocManager __instance)
    {
        try { if (Active) Emit("languageApplied", new { value = __instance.Language, tables = _modelsReady ? Snapshot(false) : null }); }
        catch { }
    }

    private static void AfterModels()
    {
        try
        {
            if (!Active || _modelsReady) return;
            _modelsReady = true;
            Emit("initialized", new {
                game = ReleaseInfoManager.Instance.ReleaseInfo?.Version,
                build = typeof(LanguageDiagnostics).Assembly.ManifestModule.ModuleVersionId.ToString(),
                mods = ModManager.GetLoadedMods().Select(m => new { id = m.manifest?.id, version = m.manifest?.version }).ToArray(),
                tables = Snapshot(true)
            });
        }
        catch (Exception ex) { Emit("snapshotUnavailable", new { error = ex.GetType().Name }); }
    }

    private static object[] Snapshot(bool baseline)
    {
        var result = new List<object>();
        foreach (var name in Tables)
        {
            var table = LocManager.Instance.GetTable(name);
            if (Translations?.GetValue(table) is not Dictionary<string, string> values) continue;
            var keys = values.Keys.Where(k => k.StartsWith("USCE-", StringComparison.Ordinal)).OrderBy(k => k, StringComparer.Ordinal).Take(KeyLimit).ToArray();
            if (baseline)
            {
                BaselineKeys[name] = keys;
                foreach (var key in keys)
                    if (LastText.Count < KeyLimit) LastText[name + "/" + key] = values[key];
            }
            var missing = BaselineKeys.TryGetValue(name, out var original) ? original.Where(k => !values.ContainsKey(k)).ToArray() : [];
            result.Add(new { table = name, entries = keys.Length, missing = missing.Length, sampleMissing = missing.Take(8).ToArray(),
                fingerprint = Fingerprint(string.Join("\n", keys.Select(k => k + "=" + values[k]))) });
        }
        return result.ToArray();
    }

    private static void AfterRead(string key, string ____name, string __result)
    {
        try
        {
            if (!Active || !key.StartsWith("USCE-", StringComparison.Ordinal) || __result == null) return;
            lock (Gate)
            {
                var id = ____name + "/" + key;
                if (__result == ____name + "." + key && MissingReported.Count < KeyLimit && MissingReported.Add(id))
                    Emit("missingText", new { key = id }, true);
                if (LastText.TryGetValue(id, out var previous) && previous != __result)
                    Emit("rawTextChanged", new { key = id, previous = Short(previous), current = Short(__result),
                        previousHash = Fingerprint(previous), currentHash = Fingerprint(__result) }, true);
                if (LastText.ContainsKey(id) || LastText.Count < KeyLimit) LastText[id] = __result;
            }
        }
        catch { }
    }

    private static void BeforeMerge(Dictionary<string, string> otherTable, string ____name)
    {
        try
        {
            if (!Active || !_modelsReady) return;
            var keys = otherTable.Keys.Where(k => k.StartsWith("USCE-", StringComparison.Ordinal)).Take(8).ToArray();
            if (keys.Length != 0) Emit("tableMerge", new { table = ____name, sampleKeys = keys }, true);
        }
        catch { }
    }

    private static void AfterUi(NCard __instance)
    {
        try
        {
            if (!Active || __instance.Model is not { } card || !card.Id.Entry.StartsWith("USCE-", StringComparison.Ordinal) || !__instance.IsNodeReady()) return;
            var title = __instance.GetNode<Label>("%TitleLabel").Text;
            var description = __instance.GetNode<RichTextLabel>("%DescriptionLabel").Text;
            var script = (Title: HasCjk(title), Description: HasCjk(description));
            lock (Gate)
            {
                var id = card.Id.ToString();
                var seen = LastUi.TryGetValue(id, out var previous);
                if (seen && previous != script)
                    Emit("uiScriptChanged", new { card = id, previous = new { titleCjk = previous.Title, descriptionCjk = previous.Description }, current = new { titleCjk = script.Title, descriptionCjk = script.Description },
                        title = Short(title), description = Short(description) }, true);
                else if (!seen && ((Language() == "zhs" && (!script.Title || (description.Length > 0 && !script.Description)))
                    || (Language() == "eng" && (script.Title || script.Description))))
                    Emit("uiLanguageMismatch", new { card = id, title = Short(title), description = Short(description) }, true);
                if (seen || LastUi.Count < KeyLimit) LastUi[id] = script;
            }
        }
        catch { }
    }
}
