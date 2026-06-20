using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

namespace USCE.Scripts.Cards;

[Pool(typeof(SilentCardPool))]
public class Amulet : SilentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            if (CombatState == null)
            {
                return false;
            }
            return CombatState.HittableEnemies.Any(e => e.GetPower<PoisonPower>()?.Amount > 0);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DynamicVar("PoisonLoss", 3m),
        new DynamicVar("BonusEnergy", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        EnergyHoverTip
    ];

    protected override IEnumerable<string> ExtraRunAssetPaths => NSmokePuffVfx.AssetPaths;

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new CardLoc("护符", "获得{IfUpgraded:show:{energyPrefix:energyIcons(2)}|{energyPrefix:energyIcons(1)}}。\n如果敌方拥有[gold]中毒[/gold]，使其失去{PoisonLoss:diff()}层[gold]中毒[/gold]，额外获得{energyPrefix:energyIcons(1)}。"),
        _ => new CardLoc("Amulet", "Gain {IfUpgraded:show:{energyPrefix:energyIcons(2)}|{energyPrefix:energyIcons(1)}}.\nIf the enemy has [gold]Poison[/gold], they lose {PoisonLoss:diff()} [gold]Poison[/gold] and you gain {energyPrefix:energyIcons(1)}.")
    };

    public Amulet() : base(energyCost, type, rarity, targetType)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int baseEnergy = IsUpgraded ? 2 : 1;
        int poisonLoss = DynamicVars["PoisonLoss"].IntValue;
        int bonusEnergy = DynamicVars["BonusEnergy"].IntValue;

        await PlayerCmd.GainEnergy(baseEnergy, Owner);

        if (cardPlay.Target != null)
        {
            NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
            if (nCreature != null)
            {
                NGaseousImpactVfx child = NGaseousImpactVfx.Create(nCreature.VfxSpawnPosition, new Color("83eb85"));
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
            }

            var poisonPower = cardPlay.Target.GetPower<PoisonPower>();
            if (poisonPower != null && poisonPower.Amount > 0)
            {
                int newAmount = poisonPower.Amount - poisonLoss;
                if (newAmount <= 0)
                {
                    await PowerCmd.Remove(poisonPower);
                }
                else
                {
                    poisonPower.SetAmount(newAmount);
                }

                await PlayerCmd.GainEnergy(bonusEnergy, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
