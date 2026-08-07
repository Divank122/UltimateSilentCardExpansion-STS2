using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Hooks;
using MegaCrit.Sts2.Core.CardSelection;
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
using USCE.Scripts.Cards;

namespace USCE.Scripts.Powers;

public class BanePower : CustomPowerModel, IMaxHandSizeModifier
{
    private class Data
    {
        public int UpgradedCount;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_bane_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_bane_power.png";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BaseCount", 0m),
        new DynamicVar("UpgradedCount", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Transform)
    ];

    protected override object? InitInternalData()
    {
        return new Data();
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && cardSource is Bane bane)
        {
            Data data = GetInternalData<Data>();
            if (bane.IsUpgraded)
            {
                data.UpgradedCount++;
            }
            DynamicVars["UpgradedCount"].BaseValue = data.UpgradedCount;
            DynamicVars["BaseCount"].BaseValue = Amount - data.UpgradedCount;
        }
        return Task.CompletedTask;
    }

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        return currentMaxHandSize - (int)Amount;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        int upgradedCount = GetInternalData<Data>().UpgradedCount;
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, (int)Amount);
        List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this)).ToList();

        int upgradesToMake = Math.Min(selected.Count, upgradedCount);
        for (int i = 0; i < selected.Count; i++)
        {
            CardModel card = selected[i];
            CardModel replacement = card.CardScope.CreateCard<Bane>(card.Owner);
            if (i < upgradesToMake)
            {
                CardCmd.Upgrade(replacement);
            }
            await CardCmd.Transform(card, replacement);
        }
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("厄咒", "[gold]手牌[/gold]上限减少[blue]{Amount}[/blue]。\n每回合开始时，将[gold]手牌[/gold]中的{UpgradedCount:cond:>0?[blue]{UpgradedCount}[/blue]张牌变化为[gold]厄咒+[/gold]{BaseCount:cond:>0?，[blue]{BaseCount}[/blue]张牌变化为[gold]厄咒[/gold]|}|[blue]{BaseCount}[/blue]张牌变化为[gold]厄咒[/gold]}。", "[gold]手牌[/gold]上限减少[blue]{Amount}[/blue]。\n每回合开始时，将[gold]手牌[/gold]中的{UpgradedCount:cond:>0?[blue]{UpgradedCount}[/blue]张牌变化为[gold]厄咒+[/gold]{BaseCount:cond:>0?，[blue]{BaseCount}[/blue]张牌变化为[gold]厄咒[/gold]|}|[blue]{BaseCount}[/blue]张牌变化为[gold]厄咒[/gold]}。"),
        _ => new PowerLoc("Bane", "Reduce your max [gold]Hand[/gold] size by [blue]{Amount}[/blue].\nAt the start of each turn, transform {UpgradedCount:cond:>0?[blue]{UpgradedCount}[/blue] cards in your [gold]hand[/gold] into a [gold]Bane+[/gold]{BaseCount:cond:>0?, and [blue]{BaseCount}[/blue] cards into a [gold]Bane[/gold]|}|[blue]{BaseCount}[/blue] cards in your [gold]hand[/gold] into a [gold]Bane[/gold]}.", "Reduce your max [gold]Hand[/gold] size by [blue]{Amount}[/blue].\nAt the start of each turn, transform {UpgradedCount:cond:>0?[blue]{UpgradedCount}[/blue] cards in your [gold]hand[/gold] into a [gold]Bane+[/gold]{BaseCount:cond:>0?, and [blue]{BaseCount}[/blue] cards into a [gold]Bane[/gold]|}|[blue]{BaseCount}[/blue] cards in your [gold]hand[/gold] into a [gold]Bane[/gold]}.")
    };
}
