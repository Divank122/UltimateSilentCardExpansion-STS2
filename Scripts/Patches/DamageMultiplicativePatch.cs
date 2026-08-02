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
/// 跨版本兼容的伤害修改Patch
/// 使用Harmony Prefix拦截ModifyDamageMultiplicative调用
/// 处理GreatBladeModifierPower和RelentlessPursuitPower
/// </summary>
public static class DamageMultiplicativePatch
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
            GD.Print("[DamageMultiplicative] Found ModifyDamageMultiplicative (v109, 6 params)");
        }
        else
        {
            // 尝试获取v0.107版本（5参数）
            _modifyDamageMultiplicative = typeof(AbstractModel).GetMethod("ModifyDamageMultiplicative",
                BindingFlags.Public | BindingFlags.Instance, null,
                [typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel)], null);

            if (_modifyDamageMultiplicative != null)
            {
                GD.Print("[DamageMultiplicative] Found ModifyDamageMultiplicative (v107, 5 params)");
            }
        }

        if (_modifyDamageMultiplicative != null)
        {
            var prefix = new HarmonyMethod(typeof(DamageMultiplicativePatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static));
            harmony.Patch(_modifyDamageMultiplicative, prefix: prefix);
            GD.Print("[DamageMultiplicative] Patched AbstractModel.ModifyDamageMultiplicative");
        }
        else
        {
            GD.PrintErr("[DamageMultiplicative] Failed to find ModifyDamageMultiplicative");
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
        // 处理GreatBladeModifierPower
        if (__instance is GreatBladeModifierPower greatBladePower)
        {
            return HandleGreatBlade(greatBladePower, dealer, cardSource, amount, ref __result);
        }

        // 处理RelentlessPursuitPower
        if (__instance is RelentlessPursuitPower relentlessPower)
        {
            return HandleRelentlessPursuit(relentlessPower, dealer, cardSource, amount, ref __result);
        }

        return true; // 继续执行原始方法
    }

    private static bool HandleGreatBlade(GreatBladeModifierPower power, Creature? dealer, CardModel? cardSource, decimal amount, ref decimal __result)
    {
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

    private static bool HandleRelentlessPursuit(RelentlessPursuitPower power, Creature? dealer, CardModel? cardSource, decimal amount, ref decimal __result)
    {
        // [DEBUG]
        GD.Print($"[RelentlessPursuit] ModifyDamageMultiplicative called: dealer={dealer?.Name ?? "null"}, cardSource={cardSource?.GetType().Name ?? "null"}, amount={amount}");

        // 只处理攻击者的伤害
        if (dealer != power.Owner)
        {
            return true;
        }

        // 只处理攻击牌
        if (cardSource?.Type != CardType.Attack)
        {
            return true;
        }

        // 计算额外伤害加成
        var bonus = power.GetRelentlessPursuitBonus(dealer, cardSource);
        if (bonus > 0)
        {
            // 额外伤害是加法，返回amount + bonus
            // 但ModifyDamageMultiplicative返回的是倍率
            // 所以需要返回 (amount + bonus) / amount = 1 + bonus/amount
            // 但如果amount为0，需要特殊处理
            
            // 实际上，根据游戏设计，额外伤害应该是直接加到amount上
            // 但由于这个方法返回的是"倍率"（multiplicative），我们需要转换
            
            // 简单的解决方案：返回 amount + bonus
            // 但这个方法名是"Multiplicative"，所以可能需要重新理解
            
            // 查看游戏的DoubleDamagePower等Power的实现：
            // DoubleDamagePower返回2.0m（倍率）
            // 所以额外伤害加成应该返回 (amount + bonus) / amount
            
            if (amount == 0)
            {
                __result = 1m;
            }
            else
            {
                __result = (amount + bonus) / amount;
            }
            
            GD.Print($"[RelentlessPursuit] Applying bonus: +{bonus} (amount {amount} -> {amount + bonus})");
            return false;
        }

        return true;
    }
}