using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using USCE.Scripts.Enchantments;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class DissolveForge : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DissolveForgePower>(1m),
        new DynamicVar("PoisonPower", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> list = new List<IHoverTip>();
            list.Add(HoverTipFactory.FromPower<PoisonPower>());
            list.Add(HoverTipFactory.FromCard<Shiv>());
            list.AddRange(HoverTipFactory.FromEnchantment<Bone>());
            return list;
        }
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("溶制", "给予所有敌人{PoisonPower:diff()}层[gold]中毒[/gold]。\n战斗结束时将一张[purple]骨制[/purple][gold]小刀[/gold]加入你的牌组。"),
        _ => new CardLoc("Dissolve Forge", "Apply {PoisonPower:diff()} [gold]Poison[/gold] to ALL enemies.\nAt the end of combat, add a [purple]Bone[/purple] [gold]Shiv[/gold] to your deck.")
    };

    public DissolveForge() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        foreach (Creature enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, DynamicVars["PoisonPower"].IntValue, Owner.Creature, this);
        }
        
        await PowerCmd.Apply<DissolveForgePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoisonPower"].UpgradeValueBy(2m);
    }
}
