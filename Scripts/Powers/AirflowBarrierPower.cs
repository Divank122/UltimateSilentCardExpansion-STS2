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

public class AirflowBarrierPower : CustomPowerModel
{
    private class Data
    {
        public int BlockGainCount;
        public int TriggerCount;
    }

    private const int BlockGainThreshold = 4;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => BlockGainThreshold - GetInternalData<Data>().BlockGainCount % BlockGainThreshold;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_airflow_barrier_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_airflow_barrier_power.png";

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("气流屏障", "每当你从卡牌中获得[gold]格挡[/gold]4次，获得1点[gold]敏捷[/gold]。", "每当你从卡牌中获得[gold]格挡[/gold]4次，获得[blue]{Amount}[/blue]点[gold]敏捷[/gold]。"),
        _ => new PowerLoc("Airflow Barrier", "Whenever you gain [gold]Block[/gold] from a card 4 times, gain 1 [gold]Dexterity[/gold].", "Whenever you gain [gold]Block[/gold] from a card 4 times, gain [blue]{Amount}[/blue] [gold]Dexterity[/gold].")
    };

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature != Owner || amount <= 0m || cardSource == null)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.BlockGainCount++;
        int triggers = data.BlockGainCount / BlockGainThreshold - data.TriggerCount;
        if (triggers > 0)
        {
            Flash();
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
            data.TriggerCount += triggers;
        }
        InvokeDisplayAmountChanged();
    }
}
