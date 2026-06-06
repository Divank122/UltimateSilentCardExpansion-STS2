using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using USCE.Scripts.Cards;
using USCE.Scripts.Enchantments;

namespace USCE.Scripts.Powers;

public class DissolveForgePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_dissolve_forge_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_dissolve_forge_power.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Bone>();

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("溶制", "战斗结束时将一张[purple]骨制[/purple][gold]小刀[/gold]加入你的牌组。", "战斗结束时将[blue]{Amount}[/blue]张[purple]骨制[/purple][gold]小刀[/gold]加入你的牌组。"),
        _ => new PowerLoc("Dissolve Forge", "At the end of combat, add a [purple]Bone[/purple] [gold]Shiv[/gold] to your deck.", "At the end of combat, add [blue]{Amount}[/blue] [purple]Bone[/purple] [gold]Shiv[/gold]s to your deck.")
    };

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.IsAlive)
        {
            Flash();
            await Cmd.CustomScaledWait(0.1f, 1f);

            bool hasBladeMountain = Owner.GetPower<BladeMountainPower>() != null;
            bool hasBladeMountainPlus = Owner.GetPower<BladeMountainPowerPlus>() != null;

            for (int i = 0; i < Amount; i++)
            {
                CardModel card;

                if (hasBladeMountain || hasBladeMountainPlus)
                {
                    card = Owner.Player.RunState.CreateCard<GreatBlade>(Owner.Player);
                    if (hasBladeMountainPlus)
                    {
                        card.UpgradeInternal();
                        card.FinalizeUpgradeInternal();
                    }
                }
                else
                {
                    card = Owner.Player.RunState.CreateCard<Shiv>(Owner.Player);
                }

                CardCmd.Enchant<Bone>(card, 1);

                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
            }
        }
    }
}

public class DissolveForgePowerPlus : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_dissolve_forge_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_dissolve_forge_power.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Bone>();

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("溶制+", "战斗结束时将一张[purple]骨制[/purple][gold]小刀+[/gold]加入你的牌组。", "战斗结束时将[blue]{Amount}[/blue]张[purple]骨制[/purple][gold]小刀+[/gold]加入你的牌组。"),
        _ => new PowerLoc("Dissolve Forge+", "At the end of combat, add a [purple]Bone[/purple] [gold]Shiv+[/gold] to your deck.", "At the end of combat, add [blue]{Amount}[/blue] [purple]Bone[/purple] [gold]Shiv+[/gold]s to your deck.")
    };

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.IsAlive)
        {
            Flash();
            await Cmd.CustomScaledWait(0.1f, 1f);

            bool hasBladeMountain = Owner.GetPower<BladeMountainPower>() != null;
            bool hasBladeMountainPlus = Owner.GetPower<BladeMountainPowerPlus>() != null;

            for (int i = 0; i < Amount; i++)
            {
                CardModel card;

                if (hasBladeMountain || hasBladeMountainPlus)
                {
                    card = Owner.Player.RunState.CreateCard<GreatBlade>(Owner.Player);
                    if (hasBladeMountainPlus)
                    {
                        card.UpgradeInternal();
                        card.FinalizeUpgradeInternal();
                    }
                }
                else
                {
                    card = Owner.Player.RunState.CreateCard<Shiv>(Owner.Player);
                    card.UpgradeInternal();
                    card.FinalizeUpgradeInternal();
                }

                CardCmd.Enchant<Bone>(card, 1);

                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
            }
        }
    }
}
