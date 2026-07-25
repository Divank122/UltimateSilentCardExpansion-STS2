using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Cards;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Patches;

/// <summary>
/// 跨版本兼容的巨刀伤害修改Patch
/// 使用Harmony Prefix拦截ModifyDamageMultiplicative调用
/// </summary>
public static class GreatBladeDamagePatch
{
    private static MethodInfo? _modifyDamageMultiplicative;

    public static void ApplyPatches(Harmony harmony)
    {
        // 尝试获取v0.109版本（6参数）
        _modifyDamageMultiplicative = typeof(AbstractModel).GetMethod("ModifyDamageMultiplicative",
            BindingFlags.Public | BindingFlags.Instance, null,
            [typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay)], null);

        if (_modifyDamageMultiplicative != null)
        {
            GD.Print("[GreatBladeDamage] Found ModifyDamageMultiplicative (v109, 6 params)");
        }
        else
        {
            // 尝试获取v0.107版本（5参数）
            _modifyDamageMultiplicative = typeof(AbstractModel).GetMethod("ModifyDamageMultiplicative",
                BindingFlags.Public | BindingFlags.Instance, null,
                [typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)], null);

            if (_modifyDamageMultiplicative != null)
            {
                GD.Print("[GreatBladeDamage] Found ModifyDamageMultiplicative (v107, 5 params)");
            }
        }

        if (_modifyDamageMultiplicative != null)
        {
            var prefix = new HarmonyMethod(typeof(GreatBladeDamagePatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static));
            harmony.Patch(_modifyDamageMultiplicative, prefix: prefix);
            GD.Print("[GreatBladeDamage] Patched AbstractModel.ModifyDamageMultiplicative");
        }
        else
        {
            GD.PrintErr("[GreatBladeDamage] Failed to find ModifyDamageMultiplicative");
        }
    }

    /// <summary>
    /// Prefix: 拦截ModifyDamageMultiplicative调用
    /// </summary>
    public static bool Prefix(
        AbstractModel __instance,
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref decimal __result)
    {
        // 只处理GreatBladeModifierPower
        if (__instance is not GreatBladeModifierPower power)
        {
            return true; // 继续执行原始方法
        }

        // [DEBUG]
        GD.Print($"[GreatBladeModifier] ModifyDamageMultiplicative called: dealer={dealer?.Name ?? "null"}, cardSource={cardSource?.GetType().Name ?? "null"}, amount={amount}");

        // 只处理攻击者的伤害
        if (dealer != power.Owner)
        {
            return true;
        }

        // 只处理巨刀卡牌
        if (cardSource is not GreatBlade)
        {
            return true;
        }

        // 计算倍率并跳过原始方法
        var multiplier = power.GetGreatBladeMultiplier(dealer, cardSource);
        __result = multiplier;
        GD.Print($"[GreatBladeModifier] Applying multiplier: {multiplier} (amount {amount} -> {amount * multiplier})");
        
        return false; // 跳过原始方法
    }
}