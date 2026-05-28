using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using USCE.Scripts.Afflictions;

namespace USCE.Scripts.Powers;

public class BalancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_balance_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_balance_power.png";

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("制衡", "本回合只能再打出{Amount}张牌。", "本回合只能再打出[blue]{Amount}[/blue]张牌。"),
        _ => new PowerLoc("Balance", "You can only play {Amount} more card(s) this turn.", "You can only play [blue]{Amount}[/blue] more card(s) this turn.")
    };

    private CardModel? SourceCard
    {
        get => GetInternalData<BalanceData>().SourceCard;
        set => GetInternalData<BalanceData>().SourceCard = value;
    }

    private int CardsPlayedBaseline
    {
        get => GetInternalData<BalanceData>().CardsPlayedBaseline;
        set => GetInternalData<BalanceData>().CardsPlayedBaseline = value;
    }

    protected override object? InitInternalData() => new BalanceData();

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SourceCard = cardSource;
        
        var history = CombatManager.Instance?.History;
        if (history != null)
        {
            CardsPlayedBaseline = history.CardPlaysStarted.Count(e => 
                e.HappenedThisTurn(CombatState) && 
                e.CardPlay.Card.Owner.Creature == Owner);
        }
        
        IEnumerable<CardModel> allCards = Owner.Player.PlayerCombatState.AllCards;
        foreach (CardModel card in allCards)
        {
            if (card.Affliction == null)
            {
                await CardCmd.Afflict<BalanceAffliction>(card, 1m);
            }
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner == Owner.Player && card.Affliction == null)
        {
            await CardCmd.Afflict<BalanceAffliction>(card, 1m);
        }
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        IEnumerable<CardModel> cards = oldOwner.Player?.PlayerCombatState?.AllCards ?? System.Array.Empty<CardModel>();
        foreach (CardModel card in cards)
        {
            if (card.Affliction is BalanceAffliction)
            {
                CardCmd.ClearAffliction(card);
            }
        }
        return Task.CompletedTask;
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner.Creature != Owner)
        {
            return true;
        }
        
        if (card.Affliction is not BalanceAffliction)
        {
            return true;
        }
        
        var history = CombatManager.Instance?.History;
        if (history == null) return true;
        
        bool cardAlreadyInPlay = history.CardPlaysStarted.Any(e => 
            e.HappenedThisTurn(CombatState) && 
            e.CardPlay.Card == card);
        
        if (cardAlreadyInPlay)
        {
            return true;
        }
        
        int cardsPlayedAfterBaseline = history.CardPlaysStarted.Count(e => 
            e.HappenedThisTurn(CombatState) && 
            e.CardPlay.Card.Owner.Creature == Owner) - CardsPlayedBaseline;
        
        return cardsPlayedAfterBaseline < Amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }

    private class BalanceData
    {
        public CardModel? SourceCard;
        public int CardsPlayedBaseline;
    }
}
