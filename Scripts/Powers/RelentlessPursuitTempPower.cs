using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using USCE.Scripts.Cards;

namespace USCE.Scripts.Powers;

public class RelentlessPursuitTempPower : CustomTemporaryPowerModelWrapper<RelentlessPursuit, StrengthPower>
{
    public override string CustomPackedIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_relentless_pursuit_temp_power.png";
    public override string CustomBigIconPath => "res://UltimateSilentCardExpansion/images/powers/usce_relentless_pursuit_temp_power.png";
}
