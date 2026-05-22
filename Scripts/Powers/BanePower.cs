using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class BanePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_bane_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_bane_power.png";

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("厄咒", "每当本回合你受到攻击时，失去2层中毒。", "每当本回合你受到攻击时，失去[blue]{Amount}[/blue]层[gold]中毒[/gold]。"),
        _ => new PowerLoc("Bane", "Lose 2 Poison each time you are attacked this turn.", "Lose [blue]{Amount}[/blue] [gold]Poison[/gold] each time you are attacked this turn.")
    };

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && props.IsPoweredAttack_())
        {
            var poisonPower = Owner!.GetPower<PoisonPower>();
            
            if (poisonPower != null && poisonPower.Amount > 0)
            {
                int poisonAmount = poisonPower.Amount;
                int newAmount = poisonAmount - Amount;
                if (newAmount <= 0)
                {
                    await PowerCmd.Remove(poisonPower);
                }
                else
                {
                    poisonPower.SetAmount(newAmount);
                }
            }
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Owner != null && side != Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}
