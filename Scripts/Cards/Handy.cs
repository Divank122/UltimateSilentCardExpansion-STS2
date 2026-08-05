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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Handy : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("得心应手", "从你的[gold]抽牌堆[/gold]中选择{Cards:diff()}张牌，将其放入你的[gold]手牌[/gold]并丢弃。"),
        _ => new CardLoc("Handy", "Choose {Cards:diff()} cards from your [gold]Draw Pile[/gold], put them into your [gold]Hand[/gold] and discard.")
    };

    public Handy() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        var drawPile = PileType.Draw.GetPile(Owner).Cards.ToList();
        int cardCount = DynamicVars["Cards"].IntValue;
        
        if (drawPile.Count == 0)
        {
            return;
        }

        var cardsToSelect = drawPile.OrderBy(c => c.Rarity).ThenBy(c => c.Id).ToList();
        
        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cardsToSelect,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, cardCount)
        );

        var selectedList = selectedCards.ToList();
        
        if (selectedList.Count > 0)
        {
            await CardPileCmd.Add(selectedList, PileType.Hand);
            
            var cardsInHand = selectedList.Where(c => c.Pile?.Type == PileType.Hand).ToList();
            
            if (cardsInHand.Count > 0)
            {
                await CardCmd.Discard(choiceContext, cardsInHand);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
