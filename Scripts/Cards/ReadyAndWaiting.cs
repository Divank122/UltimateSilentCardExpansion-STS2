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
public class ReadyAndWaiting : SilentCardModel, ILocalizationProvider
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Sly];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(IsUpgraded)
    ];

    public ReadyAndWaiting()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (IsUpgraded)
        {
            await PowerCmd.Apply<ReadyAndWaitingPowerPlus>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<ReadyAndWaitingPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("有备而来", "每当你触发[gold]奇巧[/gold]时，添加1张[gold]{IfUpgraded:show:小刀+|小刀}[/gold]到你的[gold]手牌[/gold]。"),
        _ => new CardLoc("Ready And Waiting", "Whenever you trigger [gold]Sly[/gold], add 1 [gold]{IfUpgraded:show:Shiv+|Shiv}[/gold] into your [gold]Hand[/gold].")
    };
}
