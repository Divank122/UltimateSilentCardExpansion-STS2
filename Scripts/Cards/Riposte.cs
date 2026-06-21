using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Riposte : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (CombatState == null)
            {
                return false;
            }
            return CombatState.HittableEnemies.Any((Creature e) => e.Monster?.IntendsToAttack ?? false);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("针锋相对", "造成{Damage:diff()}点伤害。\n如果敌人的意图是攻击，额外造成2次伤害。"),
        _ => new CardLoc("Riposte", "Deal {Damage:diff()} damage.\nIf the enemy intends to attack, deal damage 2 more times.")
    };

    public Riposte() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int damage = (int)DynamicVars.Damage.BaseValue;
        int hitCount = cardPlay.Target.Monster?.IntendsToAttack == true ? 3 : 1;

        await DamageCmd.Attack(damage).WithHitCount(hitCount).FromCard(this).Targeting(cardPlay.Target)
            .WithHitVfxNode((Creature t) => NBigSlashVfx.Create(t))
            .WithHitVfxNode((Creature t) => NBigSlashImpactVfx.Create(t))
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}