using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace USCE.Scripts.Powers;

public class ContrivePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_contrive_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_contrive_power.png";

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("精打细算", "在你的回合结束时，每有一张[gold]手牌[/gold]，就对所有敌人造成2点伤害。", "在你的回合结束时，每有一张[gold]手牌[/gold]，就对所有敌人造成[blue]{Amount}[/blue]点伤害。"),
        _ => new PowerLoc("Contrive", "At the end of your turn, deal 2 damage to ALL enemies for each card in your hand.", "At the end of your turn, deal [blue]{Amount}[/blue] damage to ALL enemies for each card in your hand.")
    };

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        IReadOnlyList<CardModel> cards = PileType.Hand.GetPile(Owner.Player!).Cards;
        if (cards.Count == 0)
            return;

        int totalDamage = (int)(cards.Count * Amount);
        Flash();
        VfxCmd.PlayOnCreatureCenters(CombatState.HittableEnemies, "vfx/vfx_attack_slash");
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, totalDamage, ValueProp.Unpowered, Owner, null);
    }
}