using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Dissolve : SilentCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("PoisonAmount", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    protected override IEnumerable<string> ExtraRunAssetPaths => NSmokePuffVfx.AssetPaths;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("消解", "给予{PoisonAmount:diff()}层[gold]中毒[/gold]。\n每当你弃牌时，将这张牌放入你的[gold]手牌[/gold]。"),
        _ => new CardLoc("Dissolve", "Apply {PoisonAmount:diff()} [gold]Poison[/gold].\nWhenever you discard, put this card into your [gold]Hand[/gold].")
    };

    public Dissolve() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
            if (nCreature != null)
            {
                NGaseousImpactVfx child = NGaseousImpactVfx.Create(nCreature.VfxSpawnPosition, new Color("83eb85"));
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
            }

            int poisonAmount = DynamicVars["PoisonAmount"].IntValue;
            await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, poisonAmount, Owner.Creature, this);
        }
    }

    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (Pile != null && Pile.Type != PileType.Hand)
        {
            await CardPileCmd.Add(this, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PoisonAmount"].UpgradeValueBy(1m);
    }
}
