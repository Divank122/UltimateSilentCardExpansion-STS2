using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using USCE.Scripts.Cards;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Patches;

[HarmonyPatch(typeof(Shiv))]
public static class ShivCreateInHandPatches
{
    private static bool _shouldSkipUpgrade = false;

    public static bool ShouldSkipUpgrade => _shouldSkipUpgrade;

    public static void ClearAll()
    {
        _shouldSkipUpgrade = false;
    }

    private static bool IsFromUpgradedSource()
    {
        var stackTrace = new StackTrace();
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i)?.GetMethod();
            if (method != null)
            {
                var declaringType = method.DeclaringType;
                if (declaringType == typeof(ReadyAndWaitingPowerPlus))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool HasBladeMountain(Creature creature)
    {
        return creature.GetPower<BladeMountainPower>() != null || creature.GetPower<BladeMountainPowerPlus>() != null;
    }

    private static bool HasBladeMountainPlus(Creature creature)
    {
        return creature.GetPower<BladeMountainPowerPlus>() != null;
    }

    [HarmonyPatch(nameof(Shiv.CreateInHand), typeof(Player), typeof(CombatState))]
    [HarmonyPrefix]
    public static bool PrefixSingle(Player owner, CombatState combatState, ref Task<CardModel?> __result)
    {
        if (owner == null || combatState == null)
        {
            _shouldSkipUpgrade = false;
            return true;
        }

        if (!HasBladeMountain(owner.Creature))
        {
            _shouldSkipUpgrade = false;
            return true;
        }

        bool fromUpgradedSource = IsFromUpgradedSource();

        if (fromUpgradedSource)
        {
            _shouldSkipUpgrade = true;
        }
        else
        {
            _shouldSkipUpgrade = false;
        }

        __result = CreateGreatBlade(owner, combatState);
        return false;
    }

    [HarmonyPatch(nameof(Shiv.CreateInHand), typeof(Player), typeof(int), typeof(CombatState))]
    [HarmonyPrefix]
    public static bool PrefixMultiple(Player owner, int count, CombatState combatState, ref Task<IEnumerable<CardModel>> __result)
    {
        if (owner == null || combatState == null)
        {
            _shouldSkipUpgrade = false;
            return true;
        }

        if (!HasBladeMountain(owner.Creature))
        {
            _shouldSkipUpgrade = false;
            return true;
        }

        bool fromUpgradedSource = IsFromUpgradedSource();

        if (fromUpgradedSource)
        {
            _shouldSkipUpgrade = true;
        }
        else
        {
            _shouldSkipUpgrade = false;
        }

        __result = CreateGreatBlades(owner, count, combatState);
        return false;
    }

    private static async Task<CardModel?> CreateGreatBlade(Player owner, CombatState combatState)
    {
        if (HasBladeMountainPlus(owner.Creature))
        {
            var blade = await GreatBlade.CreateInHand(owner, combatState);
            if (blade != null)
            {
                blade.UpgradeInternal();
                blade.FinalizeUpgradeInternal();
            }
            return blade;
        }
        return await GreatBlade.CreateInHand(owner, combatState);
    }

    private static async Task<IEnumerable<CardModel>> CreateGreatBlades(Player owner, int count, CombatState combatState)
    {
        List<CardModel> result = new List<CardModel>();
        bool upgradeBlades = HasBladeMountainPlus(owner.Creature);

        for (int i = 0; i < count; i++)
        {
            var blade = await GreatBlade.CreateInHand(owner, combatState);
            if (blade != null)
            {
                if (upgradeBlades)
                {
                    blade.UpgradeInternal();
                    blade.FinalizeUpgradeInternal();
                }
                result.Add(blade);
            }
        }

        return result;
    }
}

[HarmonyPatch(typeof(CardCmd), "Upgrade", typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle))]
public static class CardCmdUpgradePatch
{
    [HarmonyPrefix]
    public static bool UpgradePrefix(IEnumerable<CardModel> cards)
    {
        if (ShivCreateInHandPatches.ShouldSkipUpgrade)
        {
            foreach (var card in cards)
            {
                if (card is GreatBlade)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(CombatManager), "SetUpCombat")]
public static class ShivCombatPatch
{
    public static async void Postfix(CombatState state)
    {
        ShivCreateInHandPatches.ClearAll();

        if (state.Players.Count > 0 && state.Players[0]?.Creature != null)
        {
            var playerCreature = state.Players[0].Creature;
            var existingPower = playerCreature.GetPower<GreatBladeModifierPower>();
            if (existingPower == null)
            {
                await PowerCmd.Apply<GreatBladeModifierPower>(playerCreature, 1m, null, null);
            }
        }
    }
}

