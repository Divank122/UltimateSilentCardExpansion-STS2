using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using USCE.Scripts.Cards;

namespace USCE.Scripts.Powers;

public class DriftingPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

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

        var driftingCards = hand.Cards.OfType<ChaosStrike>().ToList();
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
}
