using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Bane : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<IntangiblePower>("IntangibleAmount", 1m),
        new PowerVar<BanePower>("BanePower", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<IntangiblePower>()
    ];

    public Bane() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, DynamicVars["IntangibleAmount"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<BanePower>(choiceContext, Owner.Creature, DynamicVars["BanePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("厄咒", "获得[blue]{IntangibleAmount:diff()}[/blue]层[gold]无实体[/gold]。\n[gold]手牌[/gold]上限减少[blue]{BanePower}[/blue]。\n每回合开始时，将[gold]手牌[/gold]中的一张牌变化为{IfUpgraded:show:[gold]厄咒+[/gold]|[gold]厄咒[/gold]}。"),
        _ => new CardLoc("Bane", "Gain [blue]{IntangibleAmount:diff()}[/blue] [gold]Intangible[/gold].\nReduce your max [gold]Hand[/gold] size by [blue]{BanePower}[/blue].\nAt the start of each turn, transform a card in your [gold]hand[/gold] into a {IfUpgraded:show:[gold]Bane+[/gold]|[gold]Bane[/gold]}.")
    };
}
