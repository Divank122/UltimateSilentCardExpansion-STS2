using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class FlayPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_flay_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_flay_power.png";

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner || target == null || target.Side == Owner.Side)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }

        WeakPower weak = target.GetPower<WeakPower>();
        if (weak == null || weak.Amount <= 0)
        {
            return 0m;
        }

        return Amount * weak.Amount;
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("剥皮", "敌人身上每有一层[gold]虚弱[/gold]，对其攻击额外造成[blue]{Amount}[/blue]点伤害。", "敌人身上每有一层[gold]虚弱[/gold]，对其攻击额外造成[blue]{Amount}[/blue]点伤害。"),
        _ => new PowerLoc("Flay", "Deal [blue]{Amount}[/blue] additional damage to enemies for each [gold]Weak[/gold] on them.", "Deal [blue]{Amount}[/blue] additional damage to enemies for each [gold]Weak[/gold] on them.")
    };
}
