using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace USCE.Scripts.Powers;

public class RelentlessPursuitPower : CustomPowerModel
{
    private class Data
    {
        public ModelId? LastAttackId;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_relentless_pursuit_power.png";
    public override string? CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_relentless_pursuit_power.png";

    protected override object InitInternalData() => new Data();

    public override List<(string, string)>? Localization => LocManager.Instance.Language switch
    {
        "zhs" => new PowerLoc("穷追不舍", "每当你打出的攻击牌与打出的上一张攻击牌同名，抽[blue]{Amount}[/blue]张牌。", "每当你打出的攻击牌与打出的上一张攻击牌同名，抽[blue]{Amount}[/blue]张牌。"),
        _ => new PowerLoc("Relentless Pursuit", "Whenever you play an Attack with the same name as your previous Attack, draw [blue]{Amount}[/blue] cards.", "Whenever you play an Attack with the same name as your previous Attack, draw [blue]{Amount}[/blue] cards.")
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        var data = GetInternalData<Data>();

        // 只记录上一次打出的攻击牌，中间打出的其他牌不打断记录
        if (cardPlay.Card.Type == CardType.Attack)
        {
            if (data.LastAttackId != null && data.LastAttackId == cardPlay.Card.Id)
            {
                Flash();
                await CardPileCmd.Draw(choiceContext, (int)Amount, Owner.Player);
            }
            data.LastAttackId = cardPlay.Card.Id;
        }
    }
}
