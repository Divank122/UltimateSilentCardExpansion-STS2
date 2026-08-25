using BaseLib.Patches.Localization;
using Godot;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using USCE.Scripts.Patches;

namespace USCE.Scripts;

[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        var harmony = new Harmony("sts2.usce");
        harmony.PatchAll();
        ShivCreateInHandPatches.ApplyPatches(harmony);
        DamageMultiplicativePatch.ApplyPatches(harmony);
        FlayWeakMultiplierPatch.ApplyPatches(harmony);
        CardModelPatch.ApplyPatch(harmony);
        GD.Print("[USCE] Harmony patches applied");

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        SimpleLoc.EnableSimpleLoc("UltimateSilentCardExpansion");

        Log.Info("[USCE] Initialized!");
    }
}
