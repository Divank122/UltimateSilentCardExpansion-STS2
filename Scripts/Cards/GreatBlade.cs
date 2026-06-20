using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using USCE.Scripts.Powers;
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
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Cards;

[Pool(typeof(TokenCardPool))]
public sealed class GreatBlade : SilentCardModel
{
    public override TargetType TargetType => HasFanOfKnives ? TargetType.AllEnemies : TargetType.AnyEnemy;

    private bool HasFanOfKnives
    {
        get
        {
            if (IsMutable && Owner != null)
            {
                return Owner.Creature.HasPower<FanOfKnivesPower>();
            }
            return false;
        }
    }

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Shiv };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        new HashSet<CardKeyword> { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(USCEKeywords.GreatBlade),
        HoverTipFactory.FromCard<Shiv>()
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("巨刀", "造成{Damage:diff()}点伤害。\n你的[gold]手牌[/gold]中每有一张其他[gold]小刀[/gold]，这张牌的伤害减半一次。"),
        _ => new CardLoc("Great Blade", "Deal {Damage:diff()} damage.\nFor each other [gold]Shiv[/gold] in your [gold]Hand[/gold], this card's damage is halved.")
    };

    public GreatBlade() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attackCommand = DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this);
        if (HasFanOfKnives)
        {
            Creature lastEnemy = CombatState.HittableEnemies.LastOrDefault();
            attackCommand = attackCommand.TargetingAllOpponents(CombatState).WithHitVfxNode((Creature t) => NBigSlashVfx.Create(t)).WithHitVfxNode((Creature t) => NBigSlashImpactVfx.Create(t));
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            attackCommand = attackCommand.Targeting(cardPlay.Target).WithHitVfxNode((Creature t) => NBigSlashVfx.Create(t)).WithHitVfxNode((Creature t) => NBigSlashImpactVfx.Create(t));
        }
        await attackCommand.Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.GetPower<GreatBladeModifierPower>() == null)
        {
            await PowerCmd.Apply<GreatBladeModifierPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && Owner.Creature.GetPower<GreatBladeModifierPower>() == null)
        {
            await PowerCmd.Apply<GreatBladeModifierPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        }
    }

    public static async Task<CardModel?> CreateInHand(Player owner, ICombatState combatState)
    {
        return (await CreateInHand(owner, 1, combatState)).FirstOrDefault();
    }

    public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, ICombatState combatState)
    {
        if (count == 0)
        {
            return Array.Empty<CardModel>();
        }
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<CardModel>();
        }

        List<CardModel> blades = new List<CardModel>();
        for (int i = 0; i < count; i++)
        {
            blades.Add(combatState.CreateCard<GreatBlade>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(blades, PileType.Hand, owner);
        return blades;
    }
}
