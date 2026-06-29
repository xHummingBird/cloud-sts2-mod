using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Cloud.CloudCode.Mechanics.ATB;

public static class ATBCostState
{
    private class Data
    {
        public bool FreeThisTurnOrUntilPlayed;
        public bool FreeThisCombat;
    }

    private static readonly ConditionalWeakTable<CardModel, Data> _data = new();

    private static Data GetData(CardModel card)
    {
        return _data.GetValue(card, _ => new Data());
    }

    public static void SetFreeThisTurnOrUntilPlayed(CardModel card)
    {
        if (card is not IATBCard)
            return;

        GetData(card).FreeThisTurnOrUntilPlayed = true;
        card.InvokeEnergyCostChanged();
    }

    public static void SetFreeThisCombat(CardModel card)
    {
        if (card is not IATBCard)
            return;

        GetData(card).FreeThisCombat = true;
        card.InvokeEnergyCostChanged();
    }

    public static void ClearThisTurnOrUntilPlayed(CardModel card)
    {
        if (card is not IATBCard)
            return;

        var data = GetData(card);

        if (!data.FreeThisTurnOrUntilPlayed)
            return;

        data.FreeThisTurnOrUntilPlayed = false;
        card.InvokeEnergyCostChanged();
    }

    public static void ClearAll(CardModel card)
    {
        if (card is not IATBCard)
            return;

        var data = GetData(card);

        data.FreeThisTurnOrUntilPlayed = false;
        data.FreeThisCombat = false;

        card.InvokeEnergyCostChanged();
    }

    public static bool IsATBFree(CardModel card)
    {
        if (card is not IATBCard)
            return false;

        var data = GetData(card);

        return data.FreeThisTurnOrUntilPlayed || data.FreeThisCombat;
    }

    
    public static int GetEffectiveATBCost(CardModel card)
    {
        if (card is not IATBCard atbCard)
            return 0;

        if (IsATBFree(card))
            return 0;

        var owner = card.Owner?.Creature;

        if (owner != null &&
            owner.HasPower<FreeAttackPower>() &&
            card.Type == CardType.Attack)
        {
            return 0;
        }

        return atbCard.ATBCost;
    }

}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisTurn))]
public static class CardModel_SetToFreeThisTurn_ATBPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        ATBCostState.SetFreeThisTurnOrUntilPlayed(__instance);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisCombat))]
public static class CardModel_SetToFreeThisCombat_ATBPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        ATBCostState.SetFreeThisCombat(__instance);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.EndOfTurnCleanup))]
public static class CardModel_EndOfTurnCleanup_ATBPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance)
    {
        ATBCostState.ClearThisTurnOrUntilPlayed(__instance);
    }
}
