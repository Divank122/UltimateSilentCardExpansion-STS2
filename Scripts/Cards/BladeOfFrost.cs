using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using USCE.Scripts.Enchantments;

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class BladeOfFrost : SilentCardModel, ILocalizationProvider
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> list = new List<IHoverTip>();
            list.Add(HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(IsUpgraded));
            list.AddRange(HoverTipFactory.FromEnchantment<Frosty>());
            return list;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar("Shivs", 2)
    ];

    public BladeOfFrost()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var shiv in await MegaCrit.Sts2.Core.Models.Cards.Shiv.CreateInHand(Owner, DynamicVars["Shivs"].IntValue, CombatState!))
        {
            CardCmd.Enchant<Frosty>(shiv, 1m);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(shiv);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("霜之刃", "添加{Shivs:diff()}张[purple]寒霜[/purple][gold]{Shivs:plural:{IfUpgraded:show:小刀+|小刀}|{IfUpgraded:show:小刀+|小刀}}[/gold]到你的[gold]手牌[/gold]。"),
        _ => new CardLoc("Blade of Frost", "Add {Shivs:diff()} [purple]Frosty[/purple] [gold]{Shivs:plural:{IfUpgraded:show:Shiv+|Shiv}|{IfUpgraded:show:Shivs+|Shivs}}[/gold] into your [gold]Hand[/gold].")
    };
}
