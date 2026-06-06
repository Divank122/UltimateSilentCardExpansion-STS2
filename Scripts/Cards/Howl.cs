using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Howl : SilentCardModel, ILocalizationProvider, IKineticCard
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(2m, ValueProp.Move),
        new RepeatVar(2),
        new CardsVar(1)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("呼啸", "对所有敌人造成{Damage:diff()}点伤害{Repeat:diff()}次。抽{Cards:diff()}张牌。"),
        _ => new CardLoc("Howl", "Deal {Damage:diff()} damage to ALL enemies {Repeat:diff()} times. Draw {Cards:diff()} cards.")
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [USCEKeywords.Kinetic];

    public Howl() : base(energyCost, type, rarity, targetType)
    {
    }

    public IEnumerable<DynamicVar> GetKineticVars() =>
    [
        DynamicVars.Damage,
        DynamicVars.Repeat,
        DynamicVars.Cards
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage = DynamicVars.Damage.IntValue;
        var hits = DynamicVars.Repeat.IntValue;

        SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(hits)
            .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(Owner.Creature, new Godot.Color("#b1ccca"), goingRight: true))
            .BeforeDamage(async () =>
            {
                var enemies = CombatState.HittableEnemies;
                foreach (var enemy in enemies)
                {
                    var impact = NDaggerSprayImpactVfx.Create(enemy, new Godot.Color("#b1ccca"), goingRight: true);
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(impact);
                }
                await Task.CompletedTask;
            })
            .Execute(choiceContext);

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
