using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Patches;

public static class FlayWeakMultiplierPatch
{
    private static MethodInfo? _weakModifyDamageMultiplicative;

    public static void ApplyPatches(Harmony harmony)
    {
        _weakModifyDamageMultiplicative = typeof(WeakPower).GetMethod("ModifyDamageMultiplicative",
            BindingFlags.Public | BindingFlags.Instance, null,
            [typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay)], null);

        if (_weakModifyDamageMultiplicative == null)
        {
            _weakModifyDamageMultiplicative = typeof(WeakPower).GetMethod("ModifyDamageMultiplicative",
                BindingFlags.Public | BindingFlags.Instance, null,
                [typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)], null);
        }

        if (_weakModifyDamageMultiplicative != null)
        {
            var postfix = new HarmonyMethod(typeof(FlayWeakMultiplierPatch).GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static));
            harmony.Patch(_weakModifyDamageMultiplicative, postfix: postfix);
            GD.Print("[FlayWeak] Patched WeakPower.ModifyDamageMultiplicative");
        }
        else
        {
            GD.PrintErr("[FlayWeak] Failed to find WeakPower.ModifyDamageMultiplicative");
        }
    }

    public static void Postfix(ref decimal __result, Creature? dealer)
    {
        if (dealer == null)
        {
            return;
        }
        WeakPower weak = dealer.GetPower<WeakPower>();
        if (weak == null || weak.Amount <= 0)
        {
            return;
        }
        var combatState = dealer.CombatState;
        if (combatState == null)
        {
            return;
        }
        decimal flayAmount = 0m;
        foreach (var ally in combatState.PlayerCreatures)
        {
            FlayPower flay = ally.GetPower<FlayPower>();
            if (flay != null)
            {
                flayAmount += flay.Amount;
            }
        }
        if (flayAmount > 0)
        {
            __result = Math.Max(0m, __result - 0.25m * flayAmount);
        }
    }
}
