using System.Collections.Generic;
using System.Linq;
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

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class AcuteCorrosion : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("PoisonPower", 18m),
        new DynamicVar("VulnerablePower", 2m),
        new DynamicVar("WeakPower", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("猛蚀", "给予{PoisonPower:diff()}层[gold]中毒[/gold]、{VulnerablePower:diff()}层[gold]易伤[/gold]和{WeakPower:diff()}层[gold]虚弱[/gold]。\n给予其他敌人一半效果。"),
        _ => new CardLoc("Acute Corrosion", "Apply {PoisonPower:diff()} [gold]Poison[/gold], {VulnerablePower:diff()} [gold]Vulnerable[/gold], and {WeakPower:diff()} [gold]Weak[/gold].\nApply half to other enemies.")
    };

    public AcuteCorrosion() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int poisonAmount = DynamicVars["PoisonPower"].IntValue;
        int vulnerableAmount = DynamicVars["VulnerablePower"].IntValue;
        int weakAmount = DynamicVars["WeakPower"].IntValue;

        // 主目标：完整效果
        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target!, poisonAmount, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target!, vulnerableAmount, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target!, weakAmount, Owner.Creature, this);

        // 其他敌人：一半效果
        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy != cardPlay.Target)
            {
                await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, poisonAmount / 2, Owner.Creature, this);
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, vulnerableAmount / 2, Owner.Creature, this);
                await PowerCmd.Apply<WeakPower>(choiceContext, enemy, weakAmount / 2, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoisonPower"].UpgradeValueBy(6m);
    }
}
