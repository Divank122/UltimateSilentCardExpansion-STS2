using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class SpiderWebPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_spider_web_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_spider_web_power.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("蛛网", "本回合每当你攻击敌人时，给予1层虚弱和1层易伤。", "本回合每当你攻击敌人时，给予[blue]{Amount}[/blue]层[gold]虚弱[/gold]和[blue]{Amount}[/blue]层[gold]易伤[/gold]。"),
        _ => new PowerLoc("Spider Web", "This turn, whenever you attack an enemy, apply 1 Weak and 1 Vulnerable.", "This turn, whenever you attack an enemy, apply [blue]{Amount}[/blue] [gold]Weak[/gold] and [blue]{Amount}[/blue] [gold]Vulnerable[/gold].")
    };

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner || command.TargetSide == Owner.Side)
        {
            return;
        }

        if (!command.DamageProps.IsPoweredAttack())
        {
            return;
        }

        foreach (List<DamageResult> resultList in command.Results)
        {
            foreach (DamageResult result in resultList)
            {
                if (result.TotalDamage > 0 && !result.Receiver.IsDead)
                {
                    await PowerCmd.Apply<WeakPower>(choiceContext, result.Receiver, Amount, Owner, null);
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, result.Receiver, Amount, Owner, null);
                }
            }
        }

        Flash();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}
