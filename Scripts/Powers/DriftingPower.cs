using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace USCE.Scripts.Powers;

public class DriftingPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    private HashSet<ModelId> DriftingCardsBeforePlay
    {
        get => GetInternalData<TurnData>().DriftingCardIds;
        set => GetInternalData<TurnData>().DriftingCardIds = value;
    }

    protected override object? InitInternalData() => new TurnData();

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        var hand = PileType.Hand.GetPile(Owner.Player);
        if (hand == null)
        {
            return;
        }

        DriftingCardsBeforePlay = hand.Cards
            .Where(c => c.Keywords.Contains(USCEKeywords.Drifting))
            .Select(c => c.Id)
            .ToHashSet();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        var hand = PileType.Hand.GetPile(Owner.Player);
        if (hand == null)
        {
            return;
        }

        var driftingCards = hand.Cards
            .Where(c => DriftingCardsBeforePlay.Contains(c.Id))
            .ToList();
        if (driftingCards.Count == 0)
        {
            return;
        }

        Flash();
        foreach (var card in driftingCards)
        {
            await CardCmd.Discard(choiceContext, card);
            await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
        }
    }

    private class TurnData
    {
        public HashSet<ModelId> DriftingCardIds = new();
    }
}
