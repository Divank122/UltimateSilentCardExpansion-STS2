using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace USCE.Scripts.Patches;

[HarmonyPatch(typeof(Hook))]
public static class KineticPatch
{
    [HarmonyPatch(nameof(Hook.AfterCardPlayed))]
    [HarmonyPostfix]
    public static void AfterCardPlayed(CombatState combatState, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card != null && card.Keywords.Contains(USCEKeywords.Kinetic) && card is IKineticCard kineticCard)
        {
            foreach (var v in kineticCard.GetKineticVars())
            {
                v.BaseValue += 1;
            }
        }
    }
}
