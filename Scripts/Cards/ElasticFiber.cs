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
public class ElasticFiber : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ElasticFiberPower>("Plating", 1m),
        new IntVar("OnUpgradePlating", 0m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<PlatingPower>()
    };

    public ElasticFiber()
        : base(energyCost, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ElasticFiberPower>(choiceContext, Owner.Creature, DynamicVars["Plating"].BaseValue, Owner.Creature, this);
        if (DynamicVars["OnUpgradePlating"].BaseValue > 0)
        {
            await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["OnUpgradePlating"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OnUpgradePlating"].UpgradeValueBy(2m);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("弹性纤维", "每获得8点[gold]格挡[/gold]，获得{Plating:diff()}层[gold]覆甲[/gold]。{OnUpgradePlating:cond:>0?\n获得{OnUpgradePlating:diff()}层[gold]覆甲[/gold]。|}"),
        _ => new CardLoc("Elastic Fiber", "Whenever you gain 8 [gold]Block[/gold], gain {Plating:diff()} [gold]Plating[/gold].{OnUpgradePlating:cond:>0?\nGain {OnUpgradePlating:diff()} [gold]Plating[/gold].|}")
    };
}
