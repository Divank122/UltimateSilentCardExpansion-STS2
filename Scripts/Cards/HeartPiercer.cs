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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using USCE.Scripts.Powers;
using USCE.Scripts.Utils;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class HeartPiercer : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<HeartPiercerPower>(1m),
        new DynamicVar("ShivCount", 4m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Shiv>(),
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("钻心", "[gold]小刀[/gold]额外给予{HeartPiercerPower:diff()}层[gold]中毒[/gold]。\n将{ShivCount:diff()}张[gold]小刀[/gold]添加到你的[gold]手牌[/gold]。"),
        _ => new CardLoc("Heart Piercer", "[gold]Shivs[/gold] apply additional {HeartPiercerPower:diff()} [gold]Poison[/gold].\nAdd {ShivCount:diff()} [gold]{ShivCount:plural:Shiv|Shivs}[/gold] into your [gold]Hand[/gold].")
    };

    public HeartPiercer() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<HeartPiercerPower>(choiceContext, Owner.Creature, DynamicVars["HeartPiercerPower"].IntValue, Owner.Creature, this);

        int shivCount = DynamicVars["ShivCount"].IntValue;
        for (int i = 0; i < shivCount; i++)
        {
            await ShivHelper.CreateInHand(Owner, CombatState!);
            await Cmd.Wait(0.1f);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HeartPiercerPower"].UpgradeValueBy(1m);
    }
}
