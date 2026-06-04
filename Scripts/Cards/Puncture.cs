using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Patches;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Puncture : SilentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2m, ValueProp.Move),
        new RepeatVar(6),
        new DamageVar("SlyDamage", 12m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Sly];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("扎透", "造成{Damage:diff()}点伤害{Repeat:diff()}次。\n通过[gold]奇巧[/gold]打出时，改为造成{SlyDamage:diff()}点伤害。"),
        _ => new CardLoc("Puncture", "Deal {Damage:diff()} damage {Repeat:diff()} times.\nWhen played via [gold]Sly[/gold], deal {SlyDamage:diff()} damage instead.")
    };

    public Puncture() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        bool isSlyPlay = SlyPlayTracker.IsSlyPlay(this);

        if (isSlyPlay)
        {
            int slyDamage = DynamicVars["SlyDamage"].IntValue;
            await DamageCmd.Attack(slyDamage).FromCard(this).Targeting(cardPlay.Target)
                .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(Owner.Creature, new Color("#b1ccca"), goingRight: true))
                .BeforeDamage(delegate
                {
                    var child = NDaggerSprayImpactVfx.Create(cardPlay.Target, new Color("#b1ccca"), goingRight: true);
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
                    return Task.CompletedTask;
                })
                .Execute(choiceContext);
        }
        else
        {
            int damage = DynamicVars.Damage.IntValue;
            int repeat = DynamicVars.Repeat.IntValue;

            await DamageCmd.Attack(damage).WithHitCount(repeat).FromCard(this).Targeting(cardPlay.Target)
                .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(Owner.Creature, new Color("#b1ccca"), goingRight: true))
                .BeforeDamage(delegate
                {
                    var child = NDaggerSprayImpactVfx.Create(cardPlay.Target, new Color("#b1ccca"), goingRight: true);
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
                    return Task.CompletedTask;
                })
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(2m);
        DynamicVars["SlyDamage"].UpgradeValueBy(4m);
    }
}
