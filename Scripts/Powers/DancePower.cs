using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Cards;

namespace USCE.Scripts.Powers;

public class DancePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_dance_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_dance_power.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar("BlockAmount", 0m, ValueProp.Unpowered)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.ForEnergy(this),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    private bool HasDiscardedThisTurn
    {
        get => GetInternalData<TurnData>().HasDiscarded;
        set => GetInternalData<TurnData>().HasDiscarded = value;
    }

    protected override object? InitInternalData()
    {
        return new TurnData();
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && cardSource is Dance dance)
        {
            DynamicVars["BlockAmount"].BaseValue += dance.DynamicVars["BlockAmount"].BaseValue;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player.Creature == Owner)
        {
            HasDiscardedThisTurn = false;
        }
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner.Creature != Owner || HasDiscardedThisTurn)
        {
            return;
        }

        HasDiscardedThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy((int)Amount, Owner.Player!);
        await CreatureCmd.GainBlock(Owner, DynamicVars["BlockAmount"].BaseValue, ValueProp.Unpowered, null, fast: true);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("起舞", "每回合第一次弃牌时，获得{Amount:energyIcons()}和[blue]{BlockAmount}[/blue]点[gold]格挡[/gold]。", "每回合第一次弃牌时，获得{Amount:energyIcons()}和[blue]{BlockAmount}[/blue]点[gold]格挡[/gold]。"),
        _ => new PowerLoc("Dance", "Gain {Amount:energyIcons()} and [blue]{BlockAmount}[/blue] [gold]Block[/gold] the first time you discard each turn.", "Gain {Amount:energyIcons()} and [blue]{BlockAmount}[/blue] [gold]Block[/gold] the first time you discard each turn.")
    };

    private class TurnData
    {
        public bool HasDiscarded;
    }
}
