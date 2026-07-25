using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;
using USCE.Scripts.Cards;

namespace USCE.Scripts.Patches;

[HarmonyPatch(typeof(ArchaicTooth))]
public static class ArchaicToothPatch
{
    [HarmonyPatch("GetTranscendenceStarterCard")]
    [HarmonyPrefix]
    public static bool GetTranscendenceStarterCardPrefix(ref CardModel? __result, Player player)
    {
        var neutralizeId = ModelDb.Card<Neutralize>().Id;
        var survivorId = ModelDb.Card<Survivor>().Id;

        List<CardModel> candidates = new List<CardModel>();
        
        var neutralize = player.Deck.Cards.FirstOrDefault(c => c.Id == neutralizeId);
        if (neutralize != null)
        {
            candidates.Add(neutralize);
        }
        
        var survivor = player.Deck.Cards.FirstOrDefault(c => c.Id == survivorId);
        if (survivor != null)
        {
            candidates.Add(survivor);
        }

        if (candidates.Count == 1)
        {
            __result = candidates[0];
        }
        else if (candidates.Count > 1)
        {
            __result = player.RunState.Rng.UpFront.NextItem(candidates);
        }

        return false;
    }

    [HarmonyPatch("GetTranscendenceTransformedCard")]
    [HarmonyPrefix]
    public static bool GetTranscendenceTransformedCardPrefix(ref CardModel __result, CardModel starterCard)
    {
        var survivorId = ModelDb.Card<Survivor>().Id;

        if (starterCard.Id == survivorId)
        {
            var prospectorModel = ModelDb.Card<Prospector>();
            CardModel cardModel = starterCard.Owner.RunState.CreateCard(prospectorModel, starterCard.Owner);
            if (starterCard.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
            if (starterCard.Enchantment != null)
            {
                var enchantmentModel = (EnchantmentModel)starterCard.Enchantment.MutableClone();
                CardCmd.Enchant(enchantmentModel, cardModel, enchantmentModel.Amount);
            }
            __result = cardModel;
            return false;
        }
        return true;
    }
}
