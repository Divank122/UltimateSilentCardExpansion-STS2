using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace USCE.Scripts.Patches;

public static class CardModelPatch
{
    private static readonly HashSet<CardModel> SynthesizedCards = new();

    public static int SynthesizedCardsCount => SynthesizedCards.Count;

    public static void MarkAsSynthesized(CardModel card)
    {
        SynthesizedCards.Add(card);
        GD.Print($"[Synthesize] Marked card as synthesized: {card?.GetType().Name}");
        GD.Print($"[Synthesize] SynthesizedCards count after marking: {SynthesizedCards.Count}");
    }

    public static bool IsSynthesized(CardModel card)
    {
        return SynthesizedCards.Contains(card);
    }

    public static void ApplyPatch(Harmony harmony)
    {
        // 查找所有GetDescriptionForPile重载并Patch
        var allMethods = typeof(CardModel).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == "GetDescriptionForPile")
            .ToList();

        GD.Print($"[Synthesize] Found {allMethods.Count} GetDescriptionForPile methods");
        foreach (var method in allMethods)
        {
            var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            GD.Print($"[Synthesize] Method: {method.Name}({parameters}) - Return: {method.ReturnType.Name}");
        }

        // Patch所有重载
        var postfixMethod = AccessTools.Method(typeof(CardModelPatch), "GetDescriptionForPilePostfix");
        foreach (var method in allMethods)
        {
            harmony.Patch(method, postfix: new HarmonyMethod(postfixMethod));
            var paramTypes = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
            GD.Print($"[Synthesize] Patched GetDescriptionForPile({paramTypes})");
        }
    }

    public static void GetDescriptionForPilePostfix(CardModel __instance, ref string __result)
    {
        GD.Print($"[Synthesize] GetDescriptionForPile called for card: {__instance?.GetType().Name}");
        GD.Print($"[Synthesize] IsSynthesized: {IsSynthesized(__instance)}");

        if (IsSynthesized(__instance))
        {
            GD.Print($"[Synthesize] Adding '重复X次：' prefix");
            __result = "重复X次：\n" + __result;
        }
    }
}

[HarmonyPatch(typeof(Hook))]
public static class HookPatch
{
    [HarmonyPatch("ModifyCardPlayCount")]
    [HarmonyPostfix]
    public static void ModifyCardPlayCountPostfix(CombatState combatState, CardModel card, int playCount, Creature? target, List<AbstractModel> modifyingModels, ref int __result)
    {
        if (CardModelPatch.IsSynthesized(card) && card.EnergyCost.CostsX)
        {
            int xValue = card.ResolveEnergyXValue();
            __result = playCount * xValue;
        }
    }
}

[HarmonyPatch(typeof(CardEnergyCost))]
public static class CardEnergyCostPatch
{
    private static readonly FieldInfo CostsXField = AccessTools.Field(typeof(CardEnergyCost), "<CostsX>k__BackingField");
    private static readonly FieldInfo BaseField = AccessTools.Field(typeof(CardEnergyCost), "_base");
    private static readonly MethodInfo InvokeEnergyCostChangedMethod = AccessTools.Method(typeof(CardModel), "InvokeEnergyCostChanged");
    private static readonly FieldInfo CardField = AccessTools.Field(typeof(CardEnergyCost), "_card");

    public static void SetCostsX(this CardEnergyCost cost, bool value)
    {
        var card = (CardModel?)CardField.GetValue(cost);
        if (card == null) return;
        
        card.AssertMutable();
        
        CostsXField.SetValue(cost, value);
        
        if (value)
        {
            BaseField.SetValue(cost, 0);
        }
        else
        {
            BaseField.SetValue(cost, cost.Canonical);
        }
        
        InvokeEnergyCostChangedMethod.Invoke(card, null);
    }
}
