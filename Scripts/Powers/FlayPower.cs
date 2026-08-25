using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class FlayPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("DamageReduction", 25m),
        new DynamicVar("ExtraWeakLoss", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_flay_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_flay_power.png";

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            DynamicVars["DamageReduction"].BaseValue = Math.Min(100m, Amount * 25m);
            DynamicVars["ExtraWeakLoss"].BaseValue = Amount;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        foreach (var enemy in participants.Where(e => e.IsAlive))
        {
            WeakPower weak = enemy.GetPower<WeakPower>();
            if (weak != null && weak.Amount > 0)
            {
                await PowerCmd.ModifyAmount(choiceContext, weak, -Amount, Owner, null);
            }
        }
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("剥皮", "虚弱的敌人造成的攻击伤害额外减少[blue]{DamageReduction}[/blue]%。\n敌人每回合额外失去[blue]{ExtraWeakLoss}[/blue]层虚弱。", "虚弱的敌人造成的攻击伤害额外减少[blue]{DamageReduction}[/blue]%。\n敌人每回合额外失去[blue]{ExtraWeakLoss}[/blue]层虚弱。"),
        _ => new PowerLoc("Flay", "Attacks from enemies with [gold]Weak[/gold] deal [blue]{DamageReduction}[/blue]% less damage.\nEnemies lose [blue]{ExtraWeakLoss}[/blue] additional [gold]Weak[/gold] each turn.", "Attacks from enemies with [gold]Weak[/gold] deal [blue]{DamageReduction}[/blue]% less damage.\nEnemies lose [blue]{ExtraWeakLoss}[/blue] additional [gold]Weak[/gold] each turn.")
    };
}
