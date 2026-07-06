using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class CorrosiveBurstPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_corrosive_burst_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_corrosive_burst_power.png";

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("猛蚀", "每当你给予敌人中毒，使其受到26点伤害。", "每当你给予敌人[gold]中毒[/gold]，使其受到[blue]{Amount}[/blue]点伤害。"),
        _ => new PowerLoc("Corrosive Burst", "Whenever you apply Poison to an enemy, they take 26 damage.", "Whenever you apply [gold]Poison[/gold] to an enemy, they take [blue]{Amount}[/blue] damage.")
    };

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (applier == Owner && amount > 0m && power is PoisonPower && power.Owner.Side != Owner.Side)
        {
            Flash();
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), power.Owner, (int)Amount, ValueProp.Unpowered, Owner);
        }
    }
}
