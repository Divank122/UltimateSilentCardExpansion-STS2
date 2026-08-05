using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using USCE.Scripts.Patches;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Synthesize : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("化合", "选择你[gold]手牌[/gold]中的一张牌，将其变为耗能X以打出X次。"),
        _ => new CardLoc("Synthesize", "Choose a card in your [gold]Hand[/gold]. It costs X and is played X times.")
    };

    public Synthesize() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(SelectionScreenPrompt, 1) with { PretendCardsCanBePlayed = true };
        
        var result = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, (CardModel c) => 
        {
            return !c.Keywords.Contains(CardKeyword.Unplayable);
        }, this);
        
        CardModel? card = result.FirstOrDefault();

        if (card != null)
        {
            card.EnergyCost.SetCostsX(true);
            CardModelPatch.MarkAsSynthesized(card);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
