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
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Dance : SilentCardModel, ILocalizationProvider
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Sly };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DancePower>("BlockAmount", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip,
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public Dance()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DancePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockAmount"].UpgradeValueBy(3m);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("起舞", "每回合第一次弃牌时，获得{energyPrefix:energyIcons(1)}和{BlockAmount:diff()}点[gold]格挡[/gold]。"),
        _ => new CardLoc("Dance", "Gain {energyPrefix:energyIcons(1)} and {BlockAmount:diff()} [gold]Block[/gold] the first time you discard each turn.")
    };
}
