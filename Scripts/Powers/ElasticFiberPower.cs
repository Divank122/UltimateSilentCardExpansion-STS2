using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class ElasticFiberPower : CustomPowerModel
{
    private class Data
    {
        public decimal BlockGained;
        public int TriggerCount;
    }

    private const decimal BlockThreshold = 8m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => (int)BlockThreshold - (int)GetInternalData<Data>().BlockGained % (int)BlockThreshold;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_elastic_fiber_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_elastic_fiber_power.png";

    protected override object? InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner || amount <= 0m)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.BlockGained += amount;
        int triggers = (int)(data.BlockGained / BlockThreshold) - data.TriggerCount;
        if (triggers > 0)
        {
            Flash();
            await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Owner, Amount * triggers, Owner, null);
            data.TriggerCount += triggers;
        }
        InvokeDisplayAmountChanged();
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("弹性纤维", "每获得8点[gold]格挡[/gold]，获得[blue]{Amount}[/blue]层[gold]覆甲[/gold]。", "每获得8点[gold]格挡[/gold]，获得[blue]{Amount}[/blue]层[gold]覆甲[/gold]。"),
        _ => new PowerLoc("Elastic Fiber", "Whenever you gain 8 [gold]Block[/gold], gain [blue]{Amount}[/blue] [gold]Plating[/gold].", "Whenever you gain 8 [gold]Block[/gold], gain [blue]{Amount}[/blue] [gold]Plating[/gold].")
    };
}
