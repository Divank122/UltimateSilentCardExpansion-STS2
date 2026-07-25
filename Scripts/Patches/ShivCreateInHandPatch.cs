using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using USCE.Scripts.Cards;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Patches;

public static class ShivCreateInHandPatches
{
    private static bool HasBladeMountain(Creature creature)
    {
        return creature != null && (creature.GetPower<BladeMountainPower>() != null || creature.GetPower<BladeMountainPowerPlus>() != null);
    }

    private static bool HasBladeMountainPlus(Creature creature)
    {
        return creature != null && creature.GetPower<BladeMountainPowerPlus>() != null;
    }

    public static void ApplyPatches(Harmony harmony)
    {
        var flags = BindingFlags.Public | BindingFlags.Static;

        // v0.107 signature (without creator parameter)
        var createInHandSingle107 = typeof(Shiv).GetMethod("CreateInHand",
            flags, null, [typeof(Player), typeof(ICombatState)], null);

        if (createInHandSingle107 != null)
        {
            var prefixSingle107 = typeof(ShivCreateInHandPatches).GetMethod("PrefixSingleV107",
                BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(createInHandSingle107, new HarmonyMethod(prefixSingle107));
            GD.Print("[BladeMountain] Patched Shiv.CreateInHand (v107 single)");
        }

        var createInHandMultiple107 = typeof(Shiv).GetMethod("CreateInHand",
            flags, null, [typeof(Player), typeof(int), typeof(ICombatState)], null);

        if (createInHandMultiple107 != null)
        {
            var prefixMultiple107 = typeof(ShivCreateInHandPatches).GetMethod("PrefixMultipleV107",
                BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(createInHandMultiple107, new HarmonyMethod(prefixMultiple107));
            GD.Print("[BladeMountain] Patched Shiv.CreateInHand (v107 multiple)");
        }

        // v0.109 signature (with creator parameter)
        var createInHandSingle109 = typeof(Shiv).GetMethod("CreateInHand",
            flags, null, [typeof(Player), typeof(ICombatState), typeof(Player)], null);

        if (createInHandSingle109 != null)
        {
            var prefixSingle109 = typeof(ShivCreateInHandPatches).GetMethod("PrefixSingleV109",
                BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(createInHandSingle109, new HarmonyMethod(prefixSingle109));
            GD.Print("[BladeMountain] Patched Shiv.CreateInHand (v109 single)");
        }

        var createInHandMultiple109 = typeof(Shiv).GetMethod("CreateInHand",
            flags, null, [typeof(Player), typeof(int), typeof(ICombatState), typeof(Player)], null);

        if (createInHandMultiple109 != null)
        {
            var prefixMultiple109 = typeof(ShivCreateInHandPatches).GetMethod("PrefixMultipleV109",
                BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(createInHandMultiple109, new HarmonyMethod(prefixMultiple109));
            GD.Print("[BladeMountain] Patched Shiv.CreateInHand (v109 multiple)");
        }
    }

    // v0.107 signature (without creator parameter)
    public static bool PrefixSingleV107(Player owner, ICombatState combatState, ref Task<CardModel?> __result)
    {
        if (owner?.Creature == null || !HasBladeMountain(owner.Creature))
        {
            return true;
        }

        GD.Print("[BladeMountain] Replacing single Shiv with GreatBlade (v107)");
        __result = CreateGreatBladeSingle(owner, combatState);
        return false;
    }

    // v0.109 signature (with optional creator parameter)
    public static bool PrefixSingleV109(Player owner, ICombatState combatState, Player? creator, ref Task<CardModel?> __result)
    {
        if (owner?.Creature == null || !HasBladeMountain(owner.Creature))
        {
            return true;
        }

        GD.Print("[BladeMountain] Replacing single Shiv with GreatBlade (v109)");
        __result = CreateGreatBladeSingle(owner, combatState);
        return false;
    }

    // v0.107 signature (without creator parameter)
    public static bool PrefixMultipleV107(Player owner, int count, ICombatState combatState, ref Task<IEnumerable<CardModel>> __result)
    {
        if (owner?.Creature == null || !HasBladeMountain(owner.Creature))
        {
            return true;
        }

        GD.Print($"[BladeMountain] Replacing {count} Shivs with GreatBlades (v107)");
        __result = CreateGreatBlades(owner, count, combatState);
        return false;
    }

    // v0.109 signature (with optional creator parameter)
    public static bool PrefixMultipleV109(Player owner, int count, ICombatState combatState, Player? creator, ref Task<IEnumerable<CardModel>> __result)
    {
        if (owner?.Creature == null || !HasBladeMountain(owner.Creature))
        {
            return true;
        }

        GD.Print($"[BladeMountain] Replacing {count} Shivs with GreatBlades (v109)");
        __result = CreateGreatBlades(owner, count, combatState);
        return false;
    }

    private static async Task<CardModel?> CreateGreatBladeSingle(Player owner, ICombatState combatState)
    {
        var blades = await CreateGreatBlades(owner, 1, combatState);
        return blades.FirstOrDefault();
    }

    private static async Task<IEnumerable<CardModel>> CreateGreatBlades(Player owner, int count, ICombatState combatState)
    {
        if (count == 0)
        {
            return System.Array.Empty<CardModel>();
        }

        bool upgradeBlades = HasBladeMountainPlus(owner.Creature);

        List<CardModel> blades = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            var blade = combatState.CreateCard<GreatBlade>(owner);
            if (upgradeBlades)
            {
                blade.UpgradeInternal();
                blade.FinalizeUpgradeInternal();
            }
            blades.Add(blade);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(blades, PileType.Hand, owner);
        return blades;
    }
}