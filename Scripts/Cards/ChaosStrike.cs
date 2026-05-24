using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class ChaosStrike : SilentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    public override IEnumerable<CardKeyword> CanonicalKeywords 
    { 
        get
        {
            GD.Print($"[ChaosStrike] CanonicalKeywords called, Drifting value: {USCEKeywords.Drifting}");
            return [USCEKeywords.Drifting];
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("混乱打击", "造成{Damage:diff()}点伤害2次。\n丢弃2张牌。"),
        _ => new CardLoc("Chaos Strike", "Deal {Damage:diff()} damage twice. \nDiscard 2 cards.")
    };

    public ChaosStrike() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int damage = (int)DynamicVars.Damage.BaseValue;

        await DamageCmd.Attack(damage).WithHitCount(2).FromCard(this).Targeting(cardPlay.Target)
            .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(Owner.Creature, new Color("#b1ccca"), goingRight: true))
            .BeforeDamage(delegate
            {
                var child = NDaggerSprayImpactVfx.Create(cardPlay.Target, new Color("#b1ccca"), goingRight: true);
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
                return Task.CompletedTask;
            })
            .Execute(choiceContext);

        var cardToDiscard = await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 2), null, this);
        await CardCmd.Discard(choiceContext, cardToDiscard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.GetPower<DriftingPower>() == null)
        {
            await PowerCmd.Apply<DriftingPower>(Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && Owner.Creature.GetPower<DriftingPower>() == null)
        {
            await PowerCmd.Apply<DriftingPower>(Owner.Creature, 1m, Owner.Creature, this);
        }
    }
}
