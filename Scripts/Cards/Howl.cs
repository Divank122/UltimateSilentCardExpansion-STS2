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
public class Howl : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new CardsVar(1)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("呼啸", "对所有敌人造成{Damage:diff()}点伤害，抽{Cards:diff()}张牌。\n这张牌在本场战斗中额外抽1张牌。"),
        _ => new CardLoc("Howl", "Deal {Damage:diff()} damage to ALL enemies. Draw {Cards:diff()} cards.\nEach time this card is played this combat, draw 1 additional card.")
    };

    public Howl() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var damage = DynamicVars.Damage.IntValue;

        SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(Owner.Creature, new Godot.Color("#b1ccca"), goingRight: true))
            .BeforeDamage(async () =>
            {
                var enemies = CombatState.HittableEnemies;
                foreach (var enemy in enemies)
                {
                    var impact = NDaggerSprayImpactVfx.Create(enemy, new Godot.Color("#b1ccca"), goingRight: true);
                    MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(impact);
                }
                await Task.CompletedTask;
            })
            .Execute(choiceContext);

        // 抽牌
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        // 每次打出后，抽牌数+1（本场战斗生效）
        DynamicVars.Cards.BaseValue += 1;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
