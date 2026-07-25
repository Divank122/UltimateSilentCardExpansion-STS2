using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace USCE.Scripts.Utils;

/// <summary>
/// Cross-version compatible helper for CardModel.CreateDupe method.
/// Handles differences between v0.107 (no parameter) and v0.109+ (with newOwner parameter).
/// </summary>
public static class CardModelDupeHelper
{
    private static readonly MethodInfo? CreateDupeWithOwnerMethod;
    private static readonly MethodInfo? CreateDupeNoParamMethod;

    static CardModelDupeHelper()
    {
        var flags = BindingFlags.Public | BindingFlags.Instance;
        var cardModelType = typeof(CardModel);

        // Try v0.109 signature first (with newOwner parameter)
        CreateDupeWithOwnerMethod = cardModelType.GetMethod("CreateDupe", flags, null,
            [typeof(Player)], null);

        // Try v0.107 signature (no parameter)
        CreateDupeNoParamMethod = cardModelType.GetMethod("CreateDupe", flags, null,
            System.Type.EmptyTypes, null);
    }

    /// <summary>
    /// Creates a duplicate of the card (cross-version compatible).
    /// </summary>
    public static CardModel CreateDupe(CardModel card, Player? newOwner = null)
    {
        // Try v0.109 signature first (with newOwner parameter)
        if (CreateDupeWithOwnerMethod != null)
        {
            return (CardModel)CreateDupeWithOwnerMethod.Invoke(card, new object?[] { newOwner })!;
        }

        // Fallback to v0.107 signature (no parameter)
        if (CreateDupeNoParamMethod != null)
        {
            return (CardModel)CreateDupeNoParamMethod.Invoke(card, null)!;
        }

        // Last resort: should not reach here if game version is correct
        throw new System.InvalidOperationException("CreateDupe method not found on CardModel");
    }
}