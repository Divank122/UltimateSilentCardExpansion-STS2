using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class RelentlessPursuitPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_relentless_pursuit_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_relentless_pursuit_power.png";

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("穷追不舍", "打出攻击牌时，如果你的手牌中有与其名字相同的牌，额外造成4点伤害。", "打出攻击牌时，如果你的[gold]手牌[/gold]中有与其名字相同的牌，额外造成[blue]{Amount}[/blue]点伤害。"),
        _ => new PowerLoc("Relentless Pursuit", "When you play an Attack, if you have a card with the same name in your hand, deal 4 extra damage.", "When you play an Attack, if you have a card with the same name in your [gold]hand[/gold], deal [blue]{Amount}[/blue] extra damage.")
    };

    /// <summary>
    /// 计算额外伤害加成（跨版本兼容方法）
    /// </summary>
    public decimal GetRelentlessPursuitBonus(Creature? dealer, CardModel? cardSource)
    {
        if (cardSource == null || cardSource.Type != CardType.Attack)
        {
            return 0m;
        }
        if (dealer != Owner)
        {
            return 0m;
        }
        var hand = Owner.Player?.PlayerCombatState?.Hand?.Cards;
        if (hand == null)
        {
            return 0m;
        }
        bool hasSameName = hand.Any(c => c != cardSource && c.Id == cardSource.Id);
        if (!hasSameName)
        {
            return 0m;
        }
        return Amount;
    }
}
