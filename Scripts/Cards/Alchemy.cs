using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using USCE.Scripts.Powers;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Alchemy : SilentCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Gold", 30m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Sly, USCEKeywords.Thirsty];

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("炼金术", "获得{Gold:diff()}[gold]金币[/gold]。"),
        _ => new CardLoc("Alchemy", "Gain {Gold:diff()} [gold]Gold[/gold].")
    };

    public Alchemy() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override bool ShouldGlowRedInternal => ThirstyPower.IsThirsty(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gold = DynamicVars["Gold"].IntValue;
        await PlayerCmd.GainGold(gold, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gold"].UpgradeValueBy(10m);
    }
}
