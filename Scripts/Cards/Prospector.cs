using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Prospector : SilentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
        new DynamicVar("PoisonPower", 3m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("探索者", "获得{Block:diff()}点[gold]格挡[/gold]。\n丢弃任意张牌，每丢弃一张牌，就随机给予敌人{PoisonPower:diff()}层[gold]中毒[/gold]一次。"),
        _ => new CardLoc("Prospector", "Gain {Block:diff()} [gold]Block[/gold].\nDiscard any number of cards. For each card discarded, apply {PoisonPower:diff()} [gold]Poison[/gold] to a random enemy.")
    };

    public Prospector() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        List<CardModel> discardedCards = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 0, 999999999), null, this)).ToList();

        if (discardedCards.Count > 0)
        {
            await CardCmd.Discard(choiceContext, discardedCards);

            int poisonAmount = DynamicVars["PoisonPower"].IntValue;
            var enemies = CombatState!.HittableEnemies;

            if (enemies.Count > 0)
            {
                for (int i = 0; i < discardedCards.Count; i++)
                {
                    Creature? randomEnemy = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
                    if (randomEnemy != null)
                    {
                        await PowerCmd.Apply<PoisonPower>(choiceContext, randomEnemy, poisonAmount, Owner.Creature, this);
                    }
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5m);
        DynamicVars["PoisonPower"].UpgradeValueBy(1m);
    }
}
