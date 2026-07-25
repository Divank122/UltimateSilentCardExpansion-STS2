using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;

namespace USCE.Scripts.Utils;

/// <summary>
/// Cross-version compatible helper for Shiv.CreateInHand method.
/// Handles differences between v0.107 (no creator parameter) and v0.109+ (with optional creator parameter).
/// </summary>
public static class ShivHelper
{
    private static readonly MethodInfo? CreateInHandSingleMethod;
    private static readonly MethodInfo? CreateInHandMultipleMethod;

    static ShivHelper()
    {
        var flags = BindingFlags.Public | BindingFlags.Static;
        var shivType = typeof(MegaCrit.Sts2.Core.Models.Cards.Shiv);

        // Try v0.107 signature first (without creator parameter)
        CreateInHandSingleMethod = shivType.GetMethod("CreateInHand", flags, null,
            [typeof(Player), typeof(ICombatState)], null);

        CreateInHandMultipleMethod = shivType.GetMethod("CreateInHand", flags, null,
            [typeof(Player), typeof(int), typeof(ICombatState)], null);

        // If not found, try v0.109 signature (with optional creator parameter)
        if (CreateInHandSingleMethod == null)
        {
            CreateInHandSingleMethod = shivType.GetMethod("CreateInHand", flags, null,
                [typeof(Player), typeof(ICombatState), typeof(Player)], null);
        }

        if (CreateInHandMultipleMethod == null)
        {
            CreateInHandMultipleMethod = shivType.GetMethod("CreateInHand", flags, null,
                [typeof(Player), typeof(int), typeof(ICombatState), typeof(Player)], null);
        }
    }

    /// <summary>
    /// Creates a single Shiv card in hand (cross-version compatible).
    /// </summary>
    public static async Task<MegaCrit.Sts2.Core.Models.CardModel?> CreateInHand(Player owner, ICombatState combatState)
    {
        if (CreateInHandSingleMethod == null)
        {
            // Fallback: create Shiv directly
            var shiv = combatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(owner);
            await CardPileCmd.AddGeneratedCardsToCombat([shiv], PileType.Hand, owner);
            return shiv;
        }

        var parameters = CreateInHandSingleMethod.GetParameters().Length == 3
            ? new object?[] { owner, combatState, null }
            : new object?[] { owner, combatState };

        var result = CreateInHandSingleMethod.Invoke(null, parameters);
        if (result is Task<MegaCrit.Sts2.Core.Models.CardModel?> task)
        {
            return await task;
        }

        return null;
    }

    /// <summary>
    /// Creates multiple Shiv cards in hand (cross-version compatible).
    /// </summary>
    public static async Task<IEnumerable<MegaCrit.Sts2.Core.Models.CardModel>> CreateInHand(Player owner, int count, ICombatState combatState)
    {
        if (CreateInHandMultipleMethod == null)
        {
            // Fallback: create Shivs directly
            var shivs = new List<MegaCrit.Sts2.Core.Models.CardModel>();
            for (int i = 0; i < count; i++)
            {
                shivs.Add(combatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(owner));
            }
            await CardPileCmd.AddGeneratedCardsToCombat(shivs, PileType.Hand, owner);
            return shivs;
        }

        var parameters = CreateInHandMultipleMethod.GetParameters().Length == 4
            ? new object?[] { owner, count, combatState, null }
            : new object?[] { owner, count, combatState };

        var result = CreateInHandMultipleMethod.Invoke(null, parameters);
        if (result is Task<IEnumerable<MegaCrit.Sts2.Core.Models.CardModel>> task)
        {
            return await task;
        }

        return Array.Empty<MegaCrit.Sts2.Core.Models.CardModel>();
    }
}