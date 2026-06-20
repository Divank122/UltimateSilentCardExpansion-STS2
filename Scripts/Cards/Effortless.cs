using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Effortless : SilentCardModel, ILocalizationProvider
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(IsUpgraded)
    ];

    public Effortless()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (IsUpgraded)
        {
            await PowerCmd.Apply<EffortlessPowerPlus>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<EffortlessPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("游刃有余", "每当你打出一张不是[gold]小刀[/gold]的攻击牌时，将一张[gold]{IfUpgraded:show:小刀+|小刀}[/gold]添加到你的[gold]手牌[/gold]。"),
        _ => new CardLoc("Effortless", "Whenever you play a non-[gold]Shiv[/gold] Attack, add a [gold]{IfUpgraded:show:Shiv+|Shiv}[/gold] into your [gold]Hand[/gold].")
    };
}
