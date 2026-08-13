using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Inverted : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;

    public override TargetType TargetType => TargetType.Self;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(USCEKeywords.Drifting)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("倒立", "获得{Block:diff()}点[gold]格挡[/gold]。\n给[gold]手牌[/gold]中的一张牌添加[gold]游离[/gold]。"),
        _ => new CardLoc("Inverted", "Gain {Block:diff()} [gold]Block[/gold].\nAdd [gold]Drifting[/gold] to a card in your [gold]Hand[/gold].")
    };

    public Inverted() : base(energyCost, type, rarity, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        var selectedCard = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext,
            player: Owner,
            filter: (CardModel c) => !c.Keywords.Contains(USCEKeywords.Drifting),
            source: this
        )).FirstOrDefault();

        if (selectedCard != null)
        {
            CardCmd.ApplyKeyword(selectedCard, USCEKeywords.Drifting);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.GetPower<DriftingPower>() == null)
        {
            await PowerCmd.Apply<DriftingPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        }
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && Owner.Creature.GetPower<DriftingPower>() == null)
        {
            await PowerCmd.Apply<DriftingPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, this);
        }
    }
}
