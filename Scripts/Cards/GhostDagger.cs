using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class GhostDagger : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new HashSet<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromCard<GhostDagger>(IsUpgraded)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("幽灵匕首", "造成{Damage:diff()}点伤害。\n如果敌人有[gold]虚弱[/gold]状态，在下个回合将1张[gold]{IfUpgraded:show:幽灵匕首+|幽灵匕首}[/gold]加入你的[gold]手牌[/gold]。"),
        _ => new CardLoc("Ghost Dagger", "Deal {Damage:diff()} damage.\nIf the enemy is [gold]Weak[/gold], next turn add 1 [gold]{IfUpgraded:show:Ghost Dagger+|Ghost Dagger}[/gold] into your [gold]Hand[/gold].")
    };

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (CombatState == null) return false;
            return CombatState.HittableEnemies.Any(e => e.GetPower<WeakPower>() != null);
        }
    }

    public GhostDagger() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        bool hasWeakBefore = cardPlay.Target.GetPower<WeakPower>() != null;

        int damage = (int)DynamicVars.Damage.BaseValue;
        await DamageCmd.Attack(damage).FromCardCompatibility(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        bool hasWeakAfter = !cardPlay.Target.IsDead && cardPlay.Target.GetPower<WeakPower>() != null;

        if (hasWeakBefore || hasWeakAfter)
        {
            if (IsUpgraded)
            {
                await PowerCmd.Apply<GhostDaggerPowerPlus>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
            else
            {
                await PowerCmd.Apply<GhostDaggerPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, ICombatState combatState)
    {
        if (count == 0) return Array.Empty<CardModel>();
        if (CombatManager.Instance.IsOverOrEnding) return Array.Empty<CardModel>();

        List<CardModel> daggers = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            daggers.Add(combatState.CreateCard<GhostDagger>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(daggers, PileType.Hand, owner);
        return daggers;
    }
}