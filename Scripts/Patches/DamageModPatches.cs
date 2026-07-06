using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Patches;

/// <summary>
/// 跨版本兼容的伤害修改 Patch
/// 通过 Patch Hook.ModifyDamage 实现，避免直接重写虚方法
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public static class DamageModPatches
{
    /// <summary>
    /// 在伤害计算的乘法阶段应用巨刀伤害倍率
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void ApplyGreatBladeMultiplier(
        ICombatState? combatState,
        Creature? dealer,
        ref decimal damage,
        CardModel? cardSource,
        ModifyDamageHookType modifyDamageHookType,
        ValueProp props)
    {
        // 只在乘法阶段处理
        if (!modifyDamageHookType.HasFlag(ModifyDamageHookType.Multiplicative))
        {
            return;
        }

        // 检查是否有巨刀修饰 Power
        if (combatState == null || cardSource == null || dealer == null)
        {
            return;
        }

        // 获取玩家并检查是否有该 Power
        var player = combatState.Players.FirstOrDefault(p => p.Creature == dealer);
        if (player == null)
        {
            return;
        }

        var power = player.Creature.GetPower<GreatBladeModifierPower>();
        if (power == null)
        {
            return;
        }

        // 只处理有来源的攻击伤害（Powered Attack）
        if (!props.IsPoweredAttack())
        {
            return;
        }

        // 应用倍率
        decimal multiplier = power.GetGreatBladeMultiplier(dealer, cardSource);
        if (multiplier != 1m)
        {
            damage *= multiplier;
        }
    }

    /// <summary>
    /// 在伤害计算的加法阶段应用穷追不舍伤害加成
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void ApplyRelentlessPursuitBonus(
        ICombatState? combatState,
        Creature? dealer,
        ref decimal damage,
        CardModel? cardSource,
        ModifyDamageHookType modifyDamageHookType)
    {
        // 只在加法阶段处理
        if (!modifyDamageHookType.HasFlag(ModifyDamageHookType.Additive))
        {
            return;
        }

        // 检查是否有穷追不舍 Power
        if (combatState == null || cardSource == null || dealer == null)
        {
            return;
        }

        // 获取玩家并检查是否有该 Power
        var player = combatState.Players.FirstOrDefault(p => p.Creature == dealer);
        if (player == null)
        {
            return;
        }

        var power = player.Creature.GetPower<RelentlessPursuitPower>();
        if (power == null)
        {
            return;
        }

        // 应用加成
        decimal bonus = power.GetRelentlessPursuitBonus(dealer, cardSource);
        if (bonus != 0m)
        {
            damage += bonus;
        }
    }
}