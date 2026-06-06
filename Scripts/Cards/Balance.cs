using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Balance : SilentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [USCEKeywords.Drifting];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("制衡", "下个回合抽{Cards:diff()}张牌并获得{IfUpgraded:show:{energyPrefix:energyIcons(2)}|{energyPrefix:energyIcons(1)}}。\n你在本回合内只能再打出一张牌。"),
        _ => new CardLoc("Balance", "Next turn, draw {Cards:diff()} cards and gain {IfUpgraded:show:{energyPrefix:energyIcons(2)}|{energyPrefix:energyIcons(1)}}.\nYou can only play 1 more card this turn.")
    };

    public Balance() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int energyGain = IsUpgraded ? 2 : 1;
        int cardsToDraw = DynamicVars.Cards.IntValue;

        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, energyGain, Owner.Creature, this);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, cardsToDraw, Owner.Creature, this);
        
        if (Owner.Creature.GetPower<BalancePower>() == null)
        {
            await PowerCmd.Apply<BalancePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.GetPower<DriftingPower>() == null)
        {
            await PowerCmd.Apply<DriftingPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && Owner.Creature.GetPower<DriftingPower>() == null)
        {
            await PowerCmd.Apply<DriftingPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        }
    }
}
