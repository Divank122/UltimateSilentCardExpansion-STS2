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
using MegaCrit.Sts2.Core.Models.Powers;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Flay : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;

    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FlayPower>(1m),
        new DynamicVar("PoisonAmount", 4m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("剥皮", "每当你给予敌人[gold]中毒[/gold]，额外给予{FlayPower:diff()}层[gold]虚弱[/gold]。{IfUpgraded:show:\n给予随机敌人{PoisonAmount:diff()}层[gold]中毒[/gold]。}"),
        _ => new CardLoc("Flay", "Whenever you apply [gold]Poison[/gold] to an enemy, apply {FlayPower:diff()} [gold]Weak[/gold].{IfUpgraded:show:\nApply {PoisonAmount:diff()} [gold]Poison[/gold] to a random enemy.}")
    };

    public Flay() : base(energyCost, type, rarity, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<FlayPower>(Owner.Creature, DynamicVars["FlayPower"].IntValue, Owner.Creature, this);

        if (IsUpgraded)
        {
            var enemies = CombatState?.HittableEnemies.ToList();
            if (enemies != null && enemies.Count > 0)
            {
                var randomEnemy = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
                await PowerCmd.Apply<PoisonPower>(randomEnemy, DynamicVars["PoisonAmount"].IntValue, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
