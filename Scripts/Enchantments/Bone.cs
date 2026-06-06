using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Enchantments;

public class Bone : CustomEnchantmentModel
{
    protected override string? CustomIconPath => "res://UltimateSilentCardExpansion/images/enchantments/usce_bone.png";

    public override bool HasExtraCardText => true;

    public override bool ShowAmount => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        return -1m;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 1, Card.Owner);
    }
}
