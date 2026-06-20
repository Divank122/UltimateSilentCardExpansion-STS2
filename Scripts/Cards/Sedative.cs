using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Hooks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Sedative : SilentCardModel, ILocalizationProvider
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [USCEKeywords.Sluggish];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("镇静剂", "抽牌直到抽满[gold]手牌[/gold]。\n这张牌在本场战斗中的耗能增加{energyPrefix:energyIcons(1)}。"),
        _ => new CardLoc("Sedative", "Draw cards until your [gold]Hand[/gold] is full.\nThis card's cost increases by {energyPrefix:energyIcons(1)} for the rest of combat.")
    };

    public Sedative() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int maxHandSize = MaxHandSizePatch.GetMaxHandSize(Owner);
        int currentHandSize = PileType.Hand.GetPile(Owner).Cards.Count();
        int cardsToDraw = maxHandSize - currentHandSize;

        if (cardsToDraw > 0)
        {
            await CardPileCmd.Draw(choiceContext, cardsToDraw, Owner);
        }

        EnergyCost.AddThisCombat(1);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
