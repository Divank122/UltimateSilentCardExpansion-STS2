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
public class Extract : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ExtractPower>("PoisonBonus", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public Extract() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ExtractPower>(choiceContext, Owner.Creature, DynamicVars["PoisonBonus"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoisonBonus"].UpgradeValueBy(1m);
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("萃取", "每当你给予敌人[gold]中毒[/gold]时，所给予的中毒层数增加{PoisonBonus:diff()}层。"),
        _ => new CardLoc("Extract", "Whenever you apply [gold]Poison[/gold] to an enemy, apply {PoisonBonus:diff()} additional [gold]Poison[/gold].")
    };
}
