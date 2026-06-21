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
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Gash : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];



    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("割破", "造成{Damage:diff()}点伤害。\n你打出的下一张描述中有[gold]中毒[/gold]的牌耗能变为0{energyPrefix:energyIcons(1)}。"),
        _ => new CardLoc("Gash", "Deal {Damage:diff()} damage.\nThe next card you play with [gold]Poison[/gold] in its description costs 0 {energyPrefix:energyIcons(1)}.")
    };

    public Gash() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int damage = (int)DynamicVars.Damage.BaseValue;
        await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<FreePoisonPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}