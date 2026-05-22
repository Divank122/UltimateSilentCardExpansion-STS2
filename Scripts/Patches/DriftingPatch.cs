using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using USCE.Scripts.Cards;

namespace USCE.Scripts.Patches;

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class DriftingAfterCardPlayedPatch
{
    static async void Postfix(CombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var playedCard = cardPlay.Card;
        if (playedCard == null)
        {
            return;
        }

        var owner = playedCard.Owner;
        if (owner == null)
        {
            return;
        }

        var hand = PileType.Hand.GetPile(owner);
        if (hand == null)
        {
            return;
        }

        var driftingCards = hand.Cards.OfType<ChaosStrike>().Where(c => c != playedCard).ToList();

        if (driftingCards.Count == 0)
        {
            return;
        }

        foreach (var card in driftingCards)
        {
            await CardCmd.Discard(choiceContext, card);
            await CardPileCmd.Draw(choiceContext, 1, owner);
        }
    }
}
