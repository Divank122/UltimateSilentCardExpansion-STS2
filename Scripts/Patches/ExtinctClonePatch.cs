using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using USCE.Scripts.Utils;
using MegaCrit.Sts2.Core.Runs;

namespace USCE.Scripts.Patches;

[HarmonyPatch(typeof(CardModel))]
public static class ExtinctClonePatch
{
    [HarmonyPatch(nameof(CardModel.CreateClone))]
    [HarmonyPrefix]
    public static bool CreateClonePrefix(CardModel __instance, ref CardModel? __result)
    {
        if (__instance.Keywords.Contains(USCEKeywords.Extinct))
        {
            __result = null;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(EnchantmentModel))]
public static class ExtinctCloneEnchantPatch
{
    [HarmonyPatch(nameof(EnchantmentModel.CanEnchant))]
    [HarmonyPostfix]
    public static void CanEnchantPostfix(EnchantmentModel __instance, CardModel card, ref bool __result)
    {
        if (__result && __instance is Clone && card.Keywords.Contains(USCEKeywords.Extinct))
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(CardPileCmd))]
public static class ExtinctCardPileCmdPatch
{
    [HarmonyPatch(nameof(CardPileCmd.AddGeneratedCardToCombat))]
    [HarmonyPrefix]
    public static bool AddGeneratedCardToCombatPrefix(CardModel card, ref Task<CardPileAddResult> __result)
    {
        if (card == null)
        {
            __result = Task.FromResult(new CardPileAddResult { success = false });
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(CardPileCmd.AddGeneratedCardsToCombat))]
    [HarmonyPrefix]
    public static void AddGeneratedCardsToCombatPrefix(ref IEnumerable<CardModel> cards)
    {
        cards = cards.Where(c => c != null);
    }

    [HarmonyPatch(nameof(CardPileCmd.Add), [typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)])]
    [HarmonyPrefix]
    public static bool AddPrefix(CardModel card, ref Task<CardPileAddResult> __result)
    {
        if (card == null)
        {
            __result = Task.FromResult(new CardPileAddResult { success = false });
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(CardPileCmd.Add), [typeof(CardModel), typeof(CardPile), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)])]
    [HarmonyPrefix]
    public static bool AddCardPilePrefix(CardModel card, ref Task<CardPileAddResult> __result)
    {
        if (card == null)
        {
            __result = Task.FromResult(new CardPileAddResult { success = false });
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(CardPileCmd.Add), [typeof(IEnumerable<CardModel>), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)])]
    [HarmonyPrefix]
    public static void AddEnumerablePrefix(ref IEnumerable<CardModel> cards)
    {
        cards = cards.Where(c => c != null);
    }


}

[HarmonyPatch(typeof(CardCmd))]
public static class ExtinctCardCmdPatch
{
    [HarmonyPatch(nameof(CardCmd.ApplyKeyword))]
    [HarmonyPrefix]
    public static bool ApplyKeywordPrefix(CardModel card)
    {
        return card != null;
    }
}

[HarmonyPatch(typeof(AdaptiveStrike))]
public static class ExtinctAdaptiveStrikePatch
{
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    public static bool OnPlayPrefix(AdaptiveStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = OnPlayInternal(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task OnPlayInternal(AdaptiveStrike instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        
        await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCardCompatibility(instance, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        
        var cardModel = instance.CreateClone();
        if (cardModel != null)
        {
            cardModel.EnergyCost.SetThisCombat(0);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Discard, instance.Owner), 1.5f);
        }
    }
}

[HarmonyPatch(typeof(HistoryCourse))]
public static class ExtinctHistoryCoursePatch
{
    [HarmonyPatch(nameof(HistoryCourse.AfterAutoPrePlayPhaseEntered))]
    [HarmonyPrefix]
    public static bool AfterAutoPrePlayPhaseEnteredPrefix(HistoryCourse __instance, PlayerChoiceContext choiceContext, Player player, ref Task? __result)
    {
        if (player != __instance.Owner)
        {
            return true;
        }

        var combatState = __instance.Owner?.Creature?.CombatState;
        if (combatState == null || combatState.RoundNumber == 1)
        {
            return true;
        }

        var allPlays = CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.CardPlay.Card.Owner == __instance.Owner && e.HappenedLastPlayerTurn(__instance.Owner))
            .ToList();

        CardModel? cardModel = allPlays
            .Where(e => (uint)(e.CardPlay.Card.Type - 1) <= 1u &&
                        !e.CardPlay.Card.IsDupe &&
                        !e.CardPlay.Card.Keywords.Contains(USCEKeywords.Extinct))
            .Select(e => e.CardPlay.Card)
            .LastOrDefault();

        if (cardModel != null)
        {
            __instance.Flash();
            var dupe = CardModelDupeHelper.CreateDupe(cardModel, __instance.Owner);
            __result = CardCmd.AutoPlay(choiceContext, dupe, null);
        }
        else
        {
            __result = Task.CompletedTask;
        }

        return false;
    }
}

[HarmonyPatch(typeof(NightmarePower))]
public static class ExtinctNightmarePatch
{
    private static readonly AccessTools.FieldRef<PowerModel, object?> InternalDataRef = AccessTools.FieldRefAccess<PowerModel, object?>("_internalData");

    [HarmonyPatch(nameof(NightmarePower.BeforeHandDraw))]
    [HarmonyPrefix]
    public static bool BeforeHandDrawPrefix(NightmarePower __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ref Task __result)
    {
        if (player != __instance.Owner.Player)
        {
            return true;
        }

        var data = InternalDataRef(__instance);
        if (data == null)
        {
            __result = PowerCmd.Remove(__instance);
            return false;
        }

        var selectedCardField = data.GetType().GetField("selectedCard", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (selectedCardField == null)
        {
            __result = PowerCmd.Remove(__instance);
            return false;
        }

        var selectedCard = selectedCardField.GetValue(data) as CardModel;
        if (selectedCard == null)
        {
            __result = PowerCmd.Remove(__instance);
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(DollysMirror))]
public static class ExtinctDollysMirrorPatch
{
    private static readonly System.Reflection.MethodInfo? SelectionScreenPromptGetter = typeof(RelicModel).GetProperty("SelectionScreenPrompt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetGetMethod(true);

    [HarmonyPatch(nameof(DollysMirror.AfterObtained))]
    [HarmonyPrefix]
    public static bool AfterObtainedPrefix(DollysMirror __instance, ref Task __result)
    {
        __result = AfterObtainedInternal(__instance);
        return false;
    }

    private static async Task AfterObtainedInternal(DollysMirror instance)
    {
        var prompt = SelectionScreenPromptGetter?.Invoke(instance, null) as LocString ?? new LocString("relics", "dollys_mirror.selectionScreenPrompt");
        var cardModel = (await CardSelectCmd.FromDeckGeneric(prefs: new CardSelectorPrefs(prompt, 1), player: instance.Owner, filter: Filter)).FirstOrDefault();
        if (cardModel != null)
        {
            var card = instance.Owner.RunState.CloneCard(cardModel);
            if (card != null)
            {
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
            }
        }
    }

    private static bool Filter(CardModel c)
    {
        return c.Type != CardType.Quest && !c.Keywords.Contains(USCEKeywords.Extinct);
    }
}

[HarmonyPatch(typeof(DualWield))]
public static class ExtinctDualWieldPatch
{
    private static readonly System.Reflection.MethodInfo? SelectionScreenPromptGetter = typeof(CardModel).GetProperty("SelectionScreenPrompt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetGetMethod(true);

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    public static bool OnPlayPrefix(DualWield __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = OnPlayInternal(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task OnPlayInternal(DualWield instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prompt = SelectionScreenPromptGetter?.Invoke(instance, null) as LocString ?? new LocString("cards", "dual_wield.selectionScreenPrompt");
        var selection = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(prompt, 1), context: choiceContext, player: instance.Owner, filter: Filter, source: instance)).FirstOrDefault();
        if (selection != null)
        {
            for (int i = 0; i < instance.DynamicVars.Cards.IntValue; i++)
            {
                var card = selection.CreateClone();
                if (card != null)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, instance.Owner);
                }
            }
        }
    }

    private static bool Filter(CardModel c)
    {
        CardType type = c.Type;
        return (type == CardType.Attack || type == CardType.Power) && !c.Keywords.Contains(USCEKeywords.Extinct);
    }
}
