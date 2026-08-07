using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace USCE.Scripts.Powers;

public class ExtractPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_extract_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_extract_power.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is PoisonPower && giver == Owner)
        {
            return Amount;
        }
        return 0m;
    }

    public override Task AfterModifyingPowerAmountGiven(PowerModel power)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("萃取", "每当你给予敌人[gold]中毒[/gold]时，所给予的中毒层数增加[blue]{Amount}[/blue]层。", "每当你给予敌人[gold]中毒[/gold]时，所给予的中毒层数增加[blue]{Amount}[/blue]层。"),
        _ => new PowerLoc("Extract", "Whenever you apply [gold]Poison[/gold] to an enemy, apply [blue]{Amount}[/blue] additional [gold]Poison[/gold].", "Whenever you apply [gold]Poison[/gold] to an enemy, apply [blue]{Amount}[/blue] additional [gold]Poison[/gold].")
    };
}
