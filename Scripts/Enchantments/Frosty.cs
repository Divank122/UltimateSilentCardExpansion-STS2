using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Enchantments;

public class Frosty : CustomEnchantmentModel
{
    protected override string? CustomIconPath => "res://UltimateSilentCardExpansion/images/enchantments/usce_frosty.png";

    public override bool HasExtraCardText => true;

    public override bool ShowAmount => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (cardSource != Card)
            return;

        if (!props.IsPoweredAttack())
            return;

        if (result.TotalDamage <= 0)
            return;

        int blockToGain = result.TotalDamage / 2;
        if (blockToGain > 0)
        {
            var cardPlay = new CardPlay
            {
                Card = Card,
                Target = target,
                ResultPile = PileType.Discard,
                Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 },
                IsAutoPlay = true,
                PlayIndex = 0,
                PlayCount = 1
            };
            await CreatureCmd.GainBlock(Card.Owner.Creature, blockToGain, ValueProp.Move, cardPlay);
        }
    }
}
