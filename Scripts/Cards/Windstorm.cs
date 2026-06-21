using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Windstorm : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 4;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(99m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, USCEKeywords.Extinct];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromRelic<OddlySmoothStone>()
        .Concat(new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Fatal) });

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("风暴", "造成{Damage:diff()}点伤害。\n[gold]斩杀[/gold]时，获得1个[gold]意外光滑的石头[/gold]。"),
        _ => new CardLoc("Windstorm", "Deal {Damage:diff()} damage.\nIf [gold]Fatal[/gold], gain 1 [gold]Oddly Smooth Stone[/gold].")
    };

    public Windstorm() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        
        bool shouldTriggerFatal = cardPlay.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
        
        NGrandFinaleVfx nGrandFinaleVfx = NGrandFinaleVfx.Create(Owner.Creature);
        if (nGrandFinaleVfx != null)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nGrandFinaleVfx);
            await Cmd.Wait(NGrandFinaleVfx.totalAnticipationDuration);
        }
        
        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitVfxNode(NGrandFinaleImpactVfx.Create)
            .WithHitFx(null, null, "blunt_attack.mp3")
            .Execute(choiceContext);
        
        if (shouldTriggerFatal && attackCommand.Results.SelectMany(r => r).Any(r => r.WasTargetKilled))
        {
            await RelicCmd.Obtain<OddlySmoothStone>(Owner);
        }
        
        if (DeckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(DeckVersion);
            DeckVersion = null;
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}