using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Cards;

namespace USCE.Scripts.Powers;

public class HeartPiercerPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_heart_piercer_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_heart_piercer_power.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<HeartPiercerPower>("ExtraLose", 2m)
    ];

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && cardSource is HeartPiercer heartPiercer)
        {
            DynamicVars["ExtraLose"].BaseValue = heartPiercer.DynamicVars["ExtraLose"].BaseValue;
        }
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("钻心", "每当你攻击敌人时，触发其拥有的[gold]中毒[/gold]，并使其额外失去[blue]{ExtraLose}[/blue]层[gold]中毒[/gold]。", "每当你攻击敌人时，触发其拥有的[gold]中毒[/gold]，并使其额外失去[blue]{ExtraLose}[/blue]层[gold]中毒[/gold]。"),
        _ => new PowerLoc("Heart Piercer", "Whenever you attack an enemy, trigger their [gold]Poison[/gold], and they lose [blue]{ExtraLose}[/blue] additional [gold]Poison[/gold].", "Whenever you attack an enemy, trigger their [gold]Poison[/gold], and they lose [blue]{ExtraLose}[/blue] additional [gold]Poison[/gold].")
    };

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner || command.TargetSide == Owner.Side)
        {
            return;
        }

        foreach (var resultList in command.Results)
        {
            foreach (var result in resultList)
            {
                if (result.TotalDamage > 0 && !result.Receiver.IsDead)
                {
                    var poison = result.Receiver.GetPower<PoisonPower>();
                    if (poison != null && poison.Amount > 0)
                    {
                        await CreatureCmd.Damage(choiceContext, new List<Creature> { result.Receiver }, poison.Amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
                        if (result.Receiver.IsAlive)
                        {
                            await PowerCmd.Decrement(poison);
                            if (DynamicVars["ExtraLose"].BaseValue > 0)
                            {
                                await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), poison, -DynamicVars["ExtraLose"].BaseValue, Owner, null);
                            }
                        }
                    }
                }
            }
        }

        Flash();
    }
}
