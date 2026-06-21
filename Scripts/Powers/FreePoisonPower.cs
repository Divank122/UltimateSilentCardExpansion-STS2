using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace USCE.Scripts.Powers;

public class FreePoisonPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_free_poison_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_free_poison_power.png";

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("免费毒药", "你打出的下一张描述中有[gold]中毒[/gold]的牌耗能为0{energyPrefix:energyIcons(1)}。", "你打出的下[blue]{Amount}[/blue]张描述中有[gold]中毒[/gold]的牌耗能为0{energyPrefix:energyIcons(1)}。"),
        _ => new PowerLoc("Free Poison", "The next card you play with [gold]Poison[/gold] in its description costs 0 {energyPrefix:energyIcons(1)}.", "The next [blue]{Amount}[/blue] cards you play with [gold]Poison[/gold] in their description cost 0 {energyPrefix:energyIcons(1)}.")
    };

    private static bool IsPoisonRelatedCard(CardModel card)
    {
        return card switch
        {
            MegaCrit.Sts2.Core.Models.Cards.PoisonedStab => true,
            MegaCrit.Sts2.Core.Models.Cards.DeadlyPoison => true,
            MegaCrit.Sts2.Core.Models.Cards.Snakebite => true,
            MegaCrit.Sts2.Core.Models.Cards.BubbleBubble => true,
            MegaCrit.Sts2.Core.Models.Cards.Mirage => true,
            MegaCrit.Sts2.Core.Models.Cards.BouncingFlask => true,
            MegaCrit.Sts2.Core.Models.Cards.Haze => true,
            MegaCrit.Sts2.Core.Models.Cards.NoxiousFumes => true,
            MegaCrit.Sts2.Core.Models.Cards.Outbreak => true,
            MegaCrit.Sts2.Core.Models.Cards.CorrosiveWave => true,
            MegaCrit.Sts2.Core.Models.Cards.Accelerant => true,
            MegaCrit.Sts2.Core.Models.Cards.Envenom => true,
            Cards.AcuteCorrosion => true,
            Cards.Clot => true,
            Cards.Bane => true,
            Cards.ConfusingImpact => true,
            Cards.Flay => true,
            Cards.Amulet => true,
            Cards.Squirm => true,
            Cards.HeartPiercer => true,
            Cards.Meltforge => true,
            _ => false
        };
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner)
        {
            return false;
        }
        if (!IsPoisonRelatedCard(card))
        {
            return false;
        }
        bool flag;
        switch (card.Pile?.Type)
        {
        case PileType.Hand:
        case PileType.Play:
            flag = true;
            break;
        default:
            flag = false;
            break;
        }
        if (!flag)
        {
            return false;
        }
        modifiedCost = default(decimal);
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner && IsPoisonRelatedCard(cardPlay.Card))
        {
            bool flag;
            switch (cardPlay.Card.Pile?.Type)
            {
            case PileType.Hand:
            case PileType.Play:
                flag = true;
                break;
            default:
                flag = false;
                break;
            }
            if (flag)
            {
                await PowerCmd.Decrement(this);
            }
        }
    }
}
